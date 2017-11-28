using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Assets.Scripts.Collections;
using Assets.Scripts.LS.CelestialBodies;
using Assets.Scripts.Managers;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.LS
{
    public class SolarSystem : SSObject
    {
        public SolarSystem(string name, string displayName)
            : base(name, displayName)
        {
        }

        private bool _updating;

        /// <summary>
        /// A low priority updater that modifies the position of the planets.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DayChangeUpdater(object sender, EventArgs e)
        {
            if (this.Obj.activeSelf && !_updating)
            {
                _updating = true;
                // Star updates
                Task.Run(() =>
                {
                    foreach (var star in Stars)
                    {
                        star.UpdateThisObject(Clock.DayDeltaTime);
                    }
                });

                // Planet updates
                Task.Run(() =>
                {
                    foreach (var planet in Planets)
                    {
                        planet.UpdateThisObject(Clock.DayDeltaTime);
                    }
                });

                // Things orbiting planets updates
                Task.Run(() =>
                {
                    foreach (var planet in Planets)
                    {
                        foreach (var child in planet.Children.GetCollection())
                        {
                            if (child is CelestialBody)
                            {
                                child.UpdateThisObject(Clock.DayDeltaTime);
                            }
                        }
                    }
                });
                _updating = false;
            }
        }

        private void VisualUpdater(object sender, EventArgs e)
        {
            foreach (var star in Stars)
            {
                star.VisualUpdate(Clock.DeltaTime);
            }
        }

        /// <summary>
        /// Generate a brand new solarsystem
        /// </summary>
        public void Init()
        {
            _updating = true;
            // Set up time event
            Clock.DayChange += DayChangeUpdater;
            Clock.Tick += VisualUpdater;

            // Destroy any children that might exist previously.
            DestroyChildren();
            
            // Create Host Star
            var host = HostStar;

            // Create Planetary Bodies
            foreach (var lane in host.GetPlanetLanes())
            {
                Children.Add(Planet.GeneratePlanet(this, host, lane));
            }

            // Create Moons
            foreach (var planet in Planets.Where(p=>p.CanHaveMoons()))
            {
                var lanes = planet.GetPlanetLanes();
                foreach (var lane in lanes)
                {
                    planet.Children.Add(Planet.GeneratePlanet(this, planet, lane));
                }
            }

            // Create Asteroids

            _updating = false;
        }

        private void ChildrenModified(object sender, EventArgs eventArgs)
        {
            foreach (var child in Children.GetCollection())
            {
                child.Obj.transform.parent = this.Obj.transform;
            }
        }

        // ----------
        // Properties

        private Star HostStar
        {
            get
            {
                // If this has children, the first one is the host.
                if (!Children.Any())
                {
                    var ssComponent = Obj.GetComponent<SolarSystem>();
                    Children.Add(Star.GenerateStar(ssComponent));
                }

                var star = Children.GetFirstObject() as Star;
                if (star != null) return star;
                Debug.LogWarning("Host star is not first object!");
                return null;
            }
        }

        /// <summary>
        /// Get a list of all star objects in this solar system
        /// </summary>
        public IEnumerable<Star> Stars
        {
            get { return Children.GetCollection().OfType<Star>().Select(child => child); }
        }

        /// <summary>
        /// Get a list of all planetoids in this solar system (Does not include asteroids)
        /// </summary>
        public IEnumerable<Planet> Planets
        {
            get { return Children.GetCollection().OfType<Planet>().Select(child => child); }
        }

        private SSCollection _children;
        /// <summary>
        /// A list of all GameObjects within this solarsystem.
        /// </summary>
        public SSCollection Children {
            get
            {
                if (_children != null) return _children;
                // Create new children collection
                _children = new SSCollection();

                // setup children event handler;
                _children.CollectionModified += ChildrenModified;
                return _children;
            }
            set { _children = value; } }

        

        // ---------
        // Functions
        private void DestroyChildren()
        {
            if (Children == null) return;
            foreach (var child in Children.GetCollection())
            {
                child?.Dispose();
            }
            Children.Clear();
            Children = null;
            GC.Collect();
        }

        public override void UpdateThisObject(float deltaTime)
        {
            // This does nothing right now.
        }

        public override bool IsSelectable => false;

        public static SolarSystem GenerateSolarSystem(string name, string displayName)
        {
            var ssm = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().SolarSystemManager;

            var obj = Instantiate(ssm);
            var s = obj.AddComponent<SolarSystem>();

            // Set values
            s.name = name;
            s.DisplayName = displayName;
            s.Obj = obj;

            return s;
        }
    }
}
