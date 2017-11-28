using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.TerrainGeneration.Shapes;
using csDelaunay;
using UnityEngine;

namespace Assets.Scripts.LS.TerrainGeneration
{
    public class Rectangle
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public int Width => Mathf.RoundToInt(Boundary.DistanceBetween(StartPoint.x, EndPoint.x));
        public int Height => Mathf.RoundToInt(Boundary.DistanceBetween(StartPoint.y, EndPoint.y));

        public Rectangle(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public Rectangle(int x, int y, int x2, int y2)
        {
            StartPoint = new Point(x, y);
            EndPoint = new Point(x2, y2);
        }
    }

    internal class Region
    {
        public Region(TexturePoint point, int id)
        {
            Points = new List<TexturePoint>{ point };
            Id = id;
        }

        public List<TexturePoint> Points { get; }
        public int Id { get; }
    }

    /// <summary>
    /// The whole image. This is the one to paint to and return. (includes dead space)
    /// </summary>
    public class ThreadTexture
    {
        internal ThreadTexture(Color[] image, int width, int height)
        {
            Image = image;
            Width = width;
            Height = height;
        }

        internal int Width { get; }
        internal int Height { get; }

        internal Color[] Image { get; }

        private int GetIndex(int x, int y)
        {
            var scalar = y * Height;
            return scalar + x;
        }

        internal Color GetPixel(int x, int y)
        {
            return Image[GetIndex(x, y)];
        }

        internal void SetPixel(int x, int y, Color color)
        {
            Image[GetIndex(x, y)] = color;
        }
    }

    /// <summary>
    /// The paintable image. This does not include dead space, only the perfect rectangle.
    /// Paint to your hearts content here.
    /// </summary>
    public class PaintTexture
    {
        /// <summary>
        /// Feed in the whole image, this will break it down into the smaller rect.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="threadTexture"></param>
        internal PaintTexture(ThreadTexture threadTexture, int seed)
        {
            Seed = seed;
            StartPoint = new Point(0, 0);
            Width = 1024;
            Height = 1024;

            EndPoint = new Point(StartPoint.x + Width, StartPoint.y + Height);
            Texture = threadTexture;
            Points = new TexturePoints(GetAllPixels(), new Rectangle(0,0, threadTexture.Width, threadTexture.Height));
            Regions = new List<Region>();
        }

        internal int Width { get; }
        internal int Height { get; }
        internal Point StartPoint { get; }
        internal Point EndPoint { get; }
        internal Rectangle Canvas => new Rectangle(StartPoint, EndPoint);
        internal TexturePoints Points { get; }
        internal ThreadTexture Texture { get; }
        internal int Seed { get; }
        internal List<Region> Regions { get; }

        private int GetIndex(int x, int y)
        {
            var scalar = y * Height;
            return scalar + x;
        }

        internal Color GetPixelColour(int x, int y)
        {
            var ex = x;
            var ey = y;

            if (x < 0) ex = x + Width;
            if (y < 0) ey = y + Height;
            if (x > EndPoint.x) ex = x - Width;
            if (y > EndPoint.y) ey = y - Height;

            return Texture.Image[GetIndex(ex, ey)];
        }

        private List<TexturePoint> GetAllPixels()
        {
            var points = new List<TexturePoint>();
            for (int x = StartPoint.x; x < EndPoint.x; x++)
            {
                for (int y = StartPoint.y; y < EndPoint.y; y++)
                {
                    points.Add(new TexturePoint(x, y, 0));
                }
            }
            return points;
        }

        internal TexturePoint GetPixel(Point point)
        {
            return Points.GetPoint(point);
        }

        internal IEnumerable<TexturePoint> GetPixels(Rectangle rect)
        {
            for (int y = rect.StartPoint.y; y < rect.EndPoint.y; y++)
            {
                for (int x = rect.StartPoint.x; x < rect.EndPoint.x; x++)
                {
                    var ex = x;
                    var ey = y;

                    if (ex > 1024) ex -= 1024;
                    if (ex < 0) ex += 1024;

                    yield return GetPixel(new Point(ex, ey));
                }
            }
        }

        internal IEnumerable<TexturePoint> GetPixels()
        {
            return GetPixels(Canvas);
        }

        internal void SetPixel(Point point, float value)
        {
            var p = GetPixel(point);
            p.Value = value;
        }

        internal void PolygonFill(List<Point> points, int colour)
        {
            var centerX = points.Sum(p => p.x) / points.Count;
            var centerY = points.Sum(p => p.y) / points.Count;
            var centerPoint = new Point(centerX, centerY);
            var triangles = new List<PolygonTriangle>();

            // Split polygon into triangles
            for (var i = 0; i < points.Count; i++)
            {
                var nextPoint = i + 1 < points.Count ? i+1 : 0;
                triangles.Add(new PolygonTriangle(points[i], points[nextPoint], centerPoint));
            }

            // Fill each triangle
            //Parallel.ForEach(triangles, triangle =>
            //{
            foreach (var triangle in triangles)
            {
                Painter.FillTriangleSimple(this, triangle.Canvas, triangle.A.x, triangle.A.y, triangle.B.x,
                    triangle.B.y, triangle.C.x, triangle.C.y, colour);
            }
                
            //});
        }

        internal void DrawLine(Point p1, Point p2, Color colour)
        {
            Point pMin;
            Point pMax;
            if (p1.x < p2.x)
            {
                pMin = p1;
                pMax = p2;
            }
            else
            {
                pMin = p2;
                pMax = p1;
            }

            var dx = pMax.x - pMin.x;
            var dy = pMax.y - pMin.y;
            for (var x = pMin.x; x < pMax.x; x++)
            {
                var y = pMin.y + dy * (x - pMin.x) / dx;
                SetPixel(new Point(x, y), colour.r);
            }
        }

        internal void Paint()
        {
            foreach (var p in Points.PointsList)
            {
                Texture.SetPixel(p.x, p.y, p.Heightmap);
            }
        }
    }

    internal class TexturePoints
    {
        public List<TexturePoint> PointsList { get; set; }
        public TexturePoint[,] PointsArray { get; set; }

        public TexturePoints(List<TexturePoint> points, Rectangle size)
        {
            PointsList = points;
            PointsArray = new TexturePoint[size.Width, size.Height];
            foreach (var point in points)
            {
                PointsArray[point.x, point.y] = point;
            }
        }

        public TexturePoint GetPoint(Point point)
        {
            TexturePoint p;
            p = PointsArray[point.x, point.y];
            if (p == null) GetPointSlow(point);
            if (p == null) return new TexturePoint(point.x, point.y, 0);

            return p;
        }

        public TexturePoint GetPoint(int x, int y)
        {
            TexturePoint p;
            try
            {
                p = PointsArray[x, y];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            if (p == null) GetPointSlow(new Point(x,y));
            if (p == null) return new TexturePoint(x, y);

            return p;
        }

        public TexturePoint GetPointSlow(Point point)
        {
            return PointsList.FirstOrDefault(p => p.Position == point);
        }

        /// <summary>
        /// Will return true if point locations are identical.
        /// </summary>
        /// <returns></returns>
        public bool ComparePointLocation(Point host, Point target)
        {
            return host.x == target.x && host.y == target.y;
        }

        public bool ComparePoint(TexturePoint target)
        {
            return PointsArray[target.x, target.y].CompareProperties(target);
        }
    }

    public class TexturePoint
    {
        internal TexturePoint(int x, int y)
        {
            this.x = x;
            this.y = y;
            RegionId = -1;
        }

        internal TexturePoint(int x, int y, float value)
        {
            this.x = x;
            this.y = y;
            this.Value = value;
            RegionId = -1;
        }

        internal int x { get; set; }
        internal int y { get; set; }
        internal float Value { get; set; }
        internal int RegionId { get; set; }

        internal Color Heightmap => new Color(Value, Value, Value, 1);

        internal Point Position => new Point(x, y);

        public override string ToString()
        {
            return $"{x}, {y}";
        }

        internal void CopyProperties(TexturePoint point)
        {
            this.Value = point.Value;
        }

        internal bool CompareProperties(TexturePoint point)
        {
            return Math.Abs(point.Value - Value) < 0.001f;
        }
    }
}
