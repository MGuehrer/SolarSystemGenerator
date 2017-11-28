using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.Managers;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.LS.CelestialBodies
{
    public class Star : CelestialBody
    {
        public Star(string name, string displayName, float mass, float diameter,
            float rotation)
            : base(name, displayName, mass, diameter, rotation)
        {
        }

        public Star(string name, string displayName, CelestialBody parent,
            float distanceFromParent, float mass, float diameter, float rotation) 
            : base(name, displayName, parent, distanceFromParent, mass, diameter, rotation)
        {
            
        }
        
        public SolarSystem SystemManager { get; set; }

        private float _temperature;
        public float Temperature
        {
            get
            {
                if (_temperature < 1)
                {
                    _temperature = Diameter / (Density*1000000);
                }
                return _temperature;
            }
        }

        public float MaximumDistanceForHeat => PlanetaryBoundaries.Max * 1.1f;

        private float _luminosity;

        public float Luminosity
        {
            get
            {
                if (_luminosity < 0.001f)
                {
                    _luminosity = (Mass / Diameter) * Density;
                }
                return _luminosity;
            }
        }

        public override bool IsSelectable => true;
        
        public override bool CanHaveMoons()
        {
            return false;
        }

        /// <summary>
        /// Factory function. This generates a new star object and returns the star class from it.
        /// </summary>
        /// <param name="systemManager"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Star GenerateStar(SolarSystem systemManager, Star parent = null)
        {
            // generate seed for randoms
            var seed = Rng.GetRandomNumber(Int32.MinValue, int.MaxValue);

            var starObj = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().StarObj;

            var bound = SizeBoundary;

            var obj = Instantiate(starObj);
            var s = obj.AddComponent<Star>();
            s.SystemManager = systemManager;

            // If the parent is not set, it's the host star
            if (parent == null)
            {
                s.name = systemManager.name + "_PrimaryStar";
                s.DisplayName = systemManager.name;
                s.Diameter = Rng.GetRandomNumber(MaximumDiameter * bound.Min, MaximumDiameter, seed);

                // after getting the first value, reset the bounds to match the original random.'
                // this makes getting huge diameters and tiny mass a thing of the past.
                bound.Min = (s.Diameter / MaximumDiameter) -0.1f;
                bound.Max = bound.Min + 0.2f;

                s.Mass = Rng.GetRandomNumber(MaximumMass * bound.Min, MaximumMass, seed);
                s.Rotation = Rng.GetRandomNumber(MaximumRotation * bound.Min, MaximumRotation, seed);
            }
            else
            {
                s.name = $"{parent.name} {systemManager.Stars.Count() + 1}";
                s.DisplayName = systemManager.name;
                s.Diameter = Rng.GetRandomNumber(MaximumDiameter * bound.Min, MaximumDiameter * bound.Max, seed);
                
                // after getting the first value, reset the bounds to match the original random.'
                // this makes getting huge diameters and tiny mass a thing of the past.
                bound.Min = (s.Diameter / MaximumDiameter) - 0.1f;
                bound.Max = bound.Min + 0.2f;

                s.Mass = Rng.GetRandomNumber(parent.Mass * bound.Min, parent.Mass * bound.Max, seed);
                s.Rotation = Rng.GetRandomNumber(MaximumRotation / -1, MaximumRotation, seed);
                s.Parent = parent;

                // TODO Generate a real number for DistanceFromParent
            }
            
            s.Obj = obj;
            
            s.Pos = new Vector3(0, 0, 80);

            s.Init();
            return s;
        }

        public static Boundary SizeBoundary => new Boundary(0.6f, 0.99f);
    }
}
