using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using csDelaunay;
using Circle.Delaunay;
using Circle.Shapes;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.LS.TerrainGeneration
{
    public class TerrainGenerator
    {
        /// <summary>
        /// This is a very slow process that takes a Sphere_Template file and
        /// processes a heightmap from the regions. Run this async so it doesn't
        /// kill your framerate.
        /// </summary>
        /// <param name="material">The renderer material</param>
        /// <param name="mainTexturePixels"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ThreadTexture GenerateHeightmap(Material material, Color[] mainTexturePixels, int height,
            int width, int seed)
        {
            var texture = new ThreadTexture(mainTexturePixels, width, height);
            var paintable = new PaintTexture(texture, seed);
            
            const int NUMBER_OF_POINTS = 1024;

            GenerateTerrain(NUMBER_OF_POINTS, paintable, seed);

            paintable.Paint();
            texture = paintable.Texture;
            return texture;
        }

        private static void GenerateTerrain(int numberOfPoints, PaintTexture texture, int seed)
        {
            var canvas = texture.Canvas;

            var sw = new Stopwatch();
            var sw2 = new Stopwatch();
            sw2.Start();

            sw.Start();
            var points = GetRandomPoints(numberOfPoints, canvas, seed);
            Debug.Log($"It took {sw.Elapsed} to Generate Points");

            sw.Restart();
            var graph = new Voronoi(points.ToList(),
                new Rectf(canvas.StartPoint.x, canvas.StartPoint.y, canvas.EndPoint.x, canvas.EndPoint.y), 1);
            Debug.Log($"It took {sw.Elapsed} to Generate the Graph");

            sw.Restart();
            SewBorder(graph);
            Debug.Log($"It took {sw.Elapsed} to SewBorders");

            sw.Restart();
            GenerateTectonics(graph, 64, seed);
            Debug.Log($"It took {sw.Elapsed} to Generate Tectonic Plates");

            sw.Restart();
            SmoothSites(graph, 2);
            Debug.Log($"It took {sw.Elapsed} to Smooth sites");

            sw.Restart();
            GenerateTectonicShifts(graph, 2);
            Debug.Log($"It took {sw.Elapsed} to Push the plates around");

            sw.Restart();
            graph.Plates.ForEach(p => p.EqualisePlateHeight(5));
            Debug.Log($"It took {sw.Elapsed} to Equalise Plates");

            sw.Restart();
            SmoothSites(graph, 4);
            Debug.Log($"It took {sw.Elapsed} to Smooth sites");

            sw.Restart();
            Paint(graph, texture);
            Debug.Log($"It took {sw.Elapsed} to Paint");

            Debug.Log($"It took {sw2.Elapsed} to Do everything\n\n");
        }

        private static void Paint(Voronoi graph, PaintTexture texture)
        {
            //Parallel.ForEach(graph.SitesIndexedByLocation, site =>
            //{
            foreach (var site in graph.SitesIndexedByLocation)
            {
                texture.PolygonFill(site.Value.OrderedColourPoints.Select(p => p.Position).ToList(),
                    site.Value.MapWeight);
            }
            
            //});
        }

        private static HashSet<Vector2f> GetRandomPoints(int count, Rectangle canvas, int seed)
        {
            var set = new HashSet<Vector2f>();

            var seedCounter = 0;
            for (int i = 0; i < count; i++)
            {
                var xr = Rng.GetRandomNumber(canvas.StartPoint.x, canvas.EndPoint.x, seedCounter++, seed);
                var yr = Rng.GetRandomNumber(canvas.StartPoint.y, canvas.EndPoint.y, seedCounter++, seed);

                var x = xr;
                var y = yr;

                if (set.Contains(new Vector2f(x, y)))
                {
                    i--;
                    continue;
                }

                set.Add(new Vector2f(x, y));
            }
            return set;
        }

        private static void SewBorder(Voronoi graph)
        {
            var farSide = (int)(graph.PlotBounds.width + graph.PlotBounds.x);

            var borderSites = graph.SitesIndexedByLocation.Values.Where(s => s.IsBorderedOnSeamSite);

            foreach (var s in borderSites)
            {
                var p = new Point(farSide, (int)s.y);

                var distances = new Dictionary<Site, int>();
                var sites = graph.SitesIndexedByLocation.Where(st => st.Key.x > farSide - 150);
                foreach (var site in sites)
                {
                    distances.Add(site.Value,
                        Mathf.RoundToInt(Boundary.DistanceBetween(new Point(p.x, s.Coord.y),
                            new Point(site.Key.x, site.Key.y))));
                }

                // Get the closest distance pair.
                var d = distances.OrderBy(kvp => kvp.Value).First();

                s.FalseNeighbours.Add(d.Key);
                d.Key.FalseNeighbours.Add(s);
            }
        }

        private static void GenerateTectonics(Voronoi graph, int numberOfPlates, int seed)
        {
            var sites = graph.SitesIndexedByLocation.Select(s => s.Value).ToList();
            graph.Plates = new List<TectonicPlate>();

            // Always generate tectonic plates for the north and south pole
            var northPole = new TectonicPlate(-1);
            var southPole = new TectonicPlate(-2);
            foreach (var site in sites.Where(s => s.IsNorthPoleSite))
            {
                northPole.AddSite(site);
            }
            foreach (var site in sites.Where(s => s.IsSouthPoleSite))
            {
                southPole.AddSite(site);
            }

            graph.Plates.Add(northPole);
            graph.Plates.Add(southPole);

            // Randomly assign a site a tectonic number.
            // If the randomly selected plate is already assigned, retry.
            var rnger = 0;
            for (var i = 1; i <= numberOfPlates - 2; i++)
            {
                var tp = new TectonicPlate(i);
                var siteIndex = Rng.GetRandomNumber(0, sites.Count, rnger++, seed);

                if (sites[siteIndex].TectonicPlate != null)
                {
                    i--;
                    continue;
                }

                tp.AddSite(sites[siteIndex]);
                graph.Plates.Add(tp);
            }

            // Now we've got our plates, it's time to expand them.
            // This may be a slow process, let's find out.
            Dictionary<TectonicPlate, bool> noNeighbours = new Dictionary<TectonicPlate, bool>();
            foreach (var p in graph.Plates)
            {
                noNeighbours.Add(p, false);
            }
            while (!noNeighbours.All(n => n.Value))
            {
                foreach (var plate in graph.Plates.Where(p => noNeighbours[p] == false))
                {
                    var neighbours = plate.NeighbourSites.Where(n => n.TectonicPlate == null).ToArray();
                    if (!neighbours.Any())
                    {
                        noNeighbours[plate] = true;
                        continue;
                    }

                    //var newSite = neighbours.OrderBy(n => n.Value).Last();

                    foreach (var site in neighbours)
                    {
                        plate.AddSite(site);
                    }
                }
            }

            foreach (var plate in graph.Plates)
            {
                plate.AssociatePlateToGlobe();
            }
        }

        private static void SmoothSites(Voronoi graph, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var site in graph.SitesIndexedByLocation)
                {
                    site.Value.SmoothHeightMap();
                }
            }
        }

        private static void GenerateTectonicShifts(Voronoi graph, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var plate in graph.Plates)
                {
                    foreach (var site in plate.Sites)
                    {
                        foreach (var s in site.GetNeighbouringTectonicPlateSites())
                        {
                            var p = plate.NeighbourPlates.FirstOrDefault(np => np.Key.Equals(s.TectonicPlate));
                            s.MapWeight += p.Value;
                            if (s.MapWeight > 255) s.MapWeight = 255;
                            if (s.MapWeight < 0) s.MapWeight = 0;
                        }
                    }
                }
            }

            // To gamify it a little
            foreach (var site in graph.SitesIndexedByLocation.Where(s => s.Value.IsNorthPoleSite || s.Value.IsSouthPoleSite))
            {
                site.Value.MapWeight = 255;
            }
        }
    }
}