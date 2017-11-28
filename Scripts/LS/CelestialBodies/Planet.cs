using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.PlanetAccessories;
using Assets.Scripts.LS.TerrainGeneration;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.LS.CelestialBodies
{
    public class Planet : CelestialBody
    {
        public Planet(string name, string displayName, float mass, float diameter,
            float rotation)
            : base(name, displayName, mass, diameter, rotation)
        {
        }

        public Planet(string name, string displayName, CelestialBody parent,
            float distanceFromParent, float mass, float diameter, float rotation) 
            : base(name, displayName, parent, distanceFromParent, mass, diameter, rotation)
        {
            
        }
        
        public SolarSystem SystemManager { get; set; }
        
        public override bool IsSelectable => true;

        public override float GameDiameter => base.GameDiameter / 2;

        public Star HostStar { get; set; }

        public bool IsGaseous { get; private set; }

        /// <summary>
        /// The minerals and minable gases that are on the planet
        /// </summary>
        public MineralWealth MaterialWealth { get; private set; }

        /// <summary>
        /// The atmosphere of the planet. 
        /// This affects livability, weather and temperature
        /// </summary>
        public Atmosphere Atmosphere { get; private set; }
        

        /// <summary>
        /// A value between 0f and 100f, this is a ratio to how much
        /// water to land there is on the planet. higher the value, more water.
        /// </summary>
        public float Hydrosphere { get; set; }

        /// <summary>
        /// it is how much gaseous water there is in the atmosphere. (0-100)
        /// </summary>
        public float AeratedMoisture
        {
            get
            {
                var tempBound = new Boundary(-25f, 100f);
                var e = (double)tempBound.Ratio(Temperature);
                return (float)Math.Pow(e, 20);
            }
        }

        /// <summary>
        /// Gets the liquid water amount as a ratio to the hydrosphere (0-100)
        /// This is derived from the Hydrosphere and the aeratedMoisture
        /// </summary>
        public float LiquidWater
        {
            get
            {
                var availableWater = new Boundary(0, Hydrosphere-AeratedMoisture);
                var tempBound = new Boundary(-25f, 100f);
                if (!tempBound.Contains(Temperature)) return 0f;
                var r = tempBound.Ratio(Temperature);

                return availableWater.GetValueFromRatio(r);
            }
        }

        /// <summary>
        /// This is the remainder of the hydrosphere after the aeratedMoisture and LiquidWater is calculated.
        /// </summary>
        public float FrozenWater
        {
            get
            {
                if (Temperature > 100f) return 0;
                return Hydrosphere - (AeratedMoisture + LiquidWater);
            }
        }

        public float Temperature
        {
            get
            {
                var runningTemp = 0.0f;
                
                // This needs to be dependant on the star
                var maxDist = HostStar.MaximumDistanceForHeat; // t
                var lowestTemp = -273.15f;

                var tempBound = new Boundary(HostStar.Temperature, lowestTemp);
                var distBound = new Boundary(0.0f, maxDist);

                var ratio = distBound.Ratio(DistanceFromParent);

                var displacement = -0.1f + (0.4f * (DistanceFromParent / maxDist));

                ratio = Boundary.Clamp(0, 1, ratio + displacement);
                
                // Temp from the sun
                runningTemp += tempBound.GetValueFromRatio(ratio);

                // Temp from the planet
                runningTemp += (Mass / Diameter) * Density;

                if (Atmosphere.HasAtmosphere)
                {
                    var absRT = Mathf.Abs(runningTemp);
                    runningTemp += (Mathf.Abs(runningTemp) * Atmosphere.GreenhouseEffect) - absRT;
                }

                return runningTemp;
            }
        }

        public override bool CanHaveMoons()
        {
            // TODO arbitrarily picked these values. maybe make them make more sense.
            return Mass > 1000000 && Diameter > 10000000;
        }

        protected override int GetDefaultLaneSize()
        {
            return 270;
        }

        public override void Init()
        {
            base.Init();
            var material = Obj.GetComponent<Renderer>().material;
            var texture = material.mainTexture as Texture2D;

            if (texture == null)
            {
                Debug.LogError("The material does not have a main texture!", material);
                return;
            }
            if (texture.width != texture.height)
            {
                Debug.LogError("Texture is not square! will not generate terrain.", texture);
                return;
            }

            // Get the texture values (these need to be grabbed here because we can't get them
            // outside of the main thread)
            var tPixels = texture.GetPixels();
            var tWidth = texture.width;
            var tHeight = texture.height;

            // TODO: This is not working. Runs a dynamically generated texture to plug onto a sphere (planet)
            // Run this async
            //Task.Factory.StartNew(() =>
            //{
            //    // Run atmospheric initalise AFTER minerals
            //    // this is so we get the same atmos density and mineral density for gas planets
            //    MaterialWealth.Init();
            //    Atmosphere.Init();

            //    // This is a very slow process
            //    return TerrainGenerator.GenerateHeightmap(material, tPixels, tHeight, tWidth, Seed);
            //}).ContinueWith(FinaliseInit, TaskScheduler.FromCurrentSynchronizationContext());

            // --------------------------------------------------
            // This is shitty sync code
            MaterialWealth.Init();
            Atmosphere.Init();

            // This is a very slow process
            //var tex = TerrainGenerator.GenerateHeightmap(material, tPixels, tHeight, tWidth, Seed);

            //var finalTex = new Texture2D(tex.Width, tex.Height);
            //finalTex.SetPixels(tex.Image);
            //finalTex.Apply();

            //var finalMat = new Material(Obj.GetComponent<Renderer>().material) { mainTexture = finalTex };

            //Obj.GetComponent<Renderer>().material = finalMat;
            // ----------------------------------------------------
        }

        /// <summary>
        /// This should be run after the terrain has been generated.
        /// </summary>
        /// <param name="task"></param>
        private void FinaliseInit(Task<ThreadTexture> task)
        {
            var texture = task.Result;

            var finalTex = new Texture2D(texture.Width, texture.Height);
            finalTex.SetPixels(texture.Image);
            finalTex.Apply();

            var finalMat = new Material(Obj.GetComponent<Renderer>().material) {mainTexture = finalTex};

            Obj.GetComponent<Renderer>().material = finalMat;
            Debug.LogWarning($"Finished generating hightmap for {DisplayName}");
        }

        /// <summary>
        /// Factory function. This generates a new Plnaet object and returns the Planet class from it.
        /// </summary>
        /// <param name="systemManager">The solar system it exists in</param>
        /// <param name="parent">The parent star.</param>
        /// <param name="swimLane">The area in which the planet can be spawned</param>
        /// <returns></returns>
        public static Planet GeneratePlanet(SolarSystem systemManager, CelestialBody parent, Boundary swimLane)
        {
            // Generate the seed to be used for randoms
            var seed = Rng.GetRandomNumber(int.MinValue, int.MaxValue);

            // Find the planet object and instantiate it.
            var planetObj = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().PlanetObj;
            
            var obj = Instantiate(planetObj);
            var s = obj.AddComponent<Planet>();
            s.SystemManager = systemManager;
            s.Seed = seed;

            s.HostStar = s.SystemManager.Stars.FirstOrDefault();

            // Get a random position within the swimlane
            s.DistanceFromParent = Rng.GetRandomNumber(swimLane, seed);
            
            // These boundaries are used for the personal properties of the planet.
            var scalar = swimLane.Mean;
            var bound = new Boundary(0.0000001f*scalar, 0.00001f*scalar);
            
            s.name = $"{parent.name} Planet {parent.Children.Count() + 1}";
            s.DisplayName = $"{parent.DisplayName} {parent.Children.Count() + 1}";
            s.Diameter = Rng.GetRandomNumber(parent.Diameter * bound.Min, parent.Diameter * bound.Max, seed);

            // after getting the first value, reset the bounds to match the original random.'
            // this makes getting huge diameters and tiny mass a thing of the past.
            bound.Min = Boundary.Clamp(0, 1.0f, (s.Diameter / MaximumDiameter) - 0.1f);
            bound.Max = Boundary.Clamp(0, 1.0f, bound.Min + 0.6f);

            s.Mass = Rng.GetRandomNumber(parent.Mass * bound.Min, parent.Mass * bound.Max, seed);

            s.Rotation = Rng.GetRandomNumber(MaximumRotation/-1, MaximumRotation, seed);

            s.Obj = obj;
            s.Parent = parent;

            s.IsGaseous = s.Density < 0.7;

            s.MaterialWealth = new MineralWealth(s);
            s.Atmosphere = new Atmosphere(s);

            if (s.Temperature < 100) s.Hydrosphere = Rng.GetRandomNumber(0, 1.0f, seed);


            s.Pos = new Vector3(s.Parent.Pos.x, s.Parent.Pos.y, s.Parent.Pos.z + s.DistanceFromParent);
            s.Init();
            return s;
        }
    }
}
