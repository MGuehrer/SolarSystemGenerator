using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Collections;
using Assets.Scripts.Helpers;
using Assets.Scripts.Managers;
using Assets.Scripts.Objects;
using Vectrosity;
using UnityEngine;

namespace Assets.Scripts.LS.CelestialBodies
{
    public abstract class CelestialBody : SSObject
    {
        #region Constants
        // Constants
        public static float MaximumMass = 100000;
        public static float MaximumDiameter = 500000;
        public static float MaximumRotation = 4;

        #endregion

        #region Constructors
        private CelestialBody(string name, string displayName)
            : base(name, displayName)
        {
        }

        /// <summary>
        /// Create Body with no parent. (This can be a system center)
        /// </summary>
        /// <param name="name">The reference name of the object</param>
        /// <param name="displayName">The visible name of the object</param>
        /// <param name="localSpace">The position in local space</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="diameter">The diameter of the object</param>
        /// <param name="rotation">The rotation speed of the object</param>
        protected CelestialBody(string name, string displayName, float mass, float diameter, float rotation)
            : this(name, displayName)
        {
            Mass = mass;
            Diameter = diameter;
            Rotation = rotation;
        }

        /// <summary>
        /// Create body with a parent
        /// </summary>
        /// <param name="name">The reference name of the object</param>
        /// <param name="displayName">The visible name of the object</param>
        /// <param name="localSpace">The position in local space</param>
        /// <param name="systemId">The system Identifier</param>
        /// <param name="parent">The parent object that this orbits around</param>
        /// <param name="distanceFromParent">The distance from the parent</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="diameter">The diameter of the object</param>
        /// <param name="rotation">The rotation speed of the object</param>
        protected CelestialBody(string name, string displayName, CelestialBody parent,
            float distanceFromParent, float mass, float diameter, float rotation)
            : this (name, displayName, mass, diameter, rotation)
        {
            Parent = parent;
            DistanceFromParent = distanceFromParent;
        }

        // Use this for initialization
        public virtual void Init()
        {
            // Get the clock before starting anything else
            var c = Clock;

            // Randomly place the body in orbit around the parent.
            // (if applicable)
            this.OrbitAngle = Rng.GetRandomNumber(0, 359, Seed);

            // Update all the visible properties of the body
            // including swimlanes
            UpdateObjectParameters();
        }

        #endregion

        /// <summary>
        /// Is only true if it is a planet over a certain mass / diameter
        /// </summary>
        public abstract bool CanHaveMoons();

        // -----------------
        // Global Properties
        public int SystemId { get; set; }

        // -----------------
        // Parent Properties
        
        /// <summary>
        /// The gameobject this is orbiting 
        /// </summary>
        private CelestialBody _parent;
        public CelestialBody Parent {
            get { return _parent; }
            set
            {
                _parent = value;
                // If the parent is set to null, do no more.
                if (_parent == null) return;

                // If the Parent does not contain this object in it's children
                // add it to the list of children. This is bad coding practice. but whatever.
                if (!_parent.Children.Contains(this))
                {
                    _parent.Children.Add(this);
                }
            }
        }

        /// <summary>
        /// The distance of orbit from parent 
        /// </summary>
        public float DistanceFromParent { get; set; }

        private float _orbitSpeed;

        /// <summary>
        /// The rotation speed is derived from the DistanceFromParent and affects how quickly the object orbits the parent
        /// </summary>
        public float OrbitSpeed
        {
            get
            {
                if (Math.Abs(_orbitSpeed) < 0.000001f)
                {
                    _orbitSpeed = 10 / (DistanceFromParent / 2800);
                }
                return _orbitSpeed;
            }
        }

        public float OrbitAngle { get; set; }

        private VectorLine _swimLane;
        public VectorLine SwimLane
        {
            get
            {
                if (_swimLane != null) return _swimLane;

                _swimLane = new VectorLine("OrbitLine", new List<Vector3>(72), 0.5f, LineType.Continuous, Joins.None);
                _swimLane.material = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().OrbitLineMaterial;
                _swimLane.color = new Color(0, .25f, 0.05f);
                _swimLane.MakeCircle(Vector3.zero, Vector3.up, Vector3.Distance(Obj.transform.localPosition, Vector3.zero));
                _swimLane.drawTransform = Parent.Obj.transform;
                _swimLane.Draw3DAuto();

                VectorManager.useDraw3D = true;

                return _swimLane;
            }
        }

        // -------------------
        // Personal Properties

        /// <summary>
        /// Mass of the body (this affects gravity)
        /// Gamewise, the earth is 80 million units
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Diameter of the body (this affects the size of the object)
        /// Gamewise, the earth is 120 million units
        /// </summary>
        public float Diameter { get; set; }

        public float Radius => Diameter / 2;

        private float _volume;

        /// <summary>
        /// Get the volume in units^3
        /// </summary>
        public float Volume
        {
            get
            {
                if (Math.Abs(_volume) < 0.1)
                {
                    _volume = (4f / 3f) * Mathf.PI * Mathf.Pow(Radius, 3.0f);
                    _volume /= Mathf.Pow(10, 7);
                }
                return _volume;
            }
        }

        private float _density;
        /// <summary>
        /// Get the Density in perUnit^3
        /// </summary>
        public float Density
        {
            get
            {
                if (Math.Abs(_density) < 0.1)
                {
                    _density = (Mass / Volume) / Radius;
                    _density *= Mathf.Pow(10, 7);
                }
                return _density;
            }
        }
        /// <summary>
        /// Personal rotation of the object. (This affects visible rotation)
        /// </summary>
        public float Rotation { get; set; }

        private float _orbitalYear;
        public float OrbitalYear
        {
            get
            {
                if (Math.Abs(_orbitalYear) < 0.01)
                {
                    var anglePerDay = OrbitSpeed * 1;
                    var anglePerYear = anglePerDay * Clock.Date.GetYearLengthInDays();
                    var numberOfYearsPerOrbit = 360 / anglePerYear; // number of degrees in a circle
                    _orbitalYear = numberOfYearsPerOrbit * Clock.Date.GetYearLengthInDays();
                }
                return _orbitalYear;
            }
        }

        // -------------------
        // Gamewise Properties

        /// <summary>
        /// The visible size of the planet. Gamewise, Earth is 1200 game units
        /// </summary>
        public virtual float GameDiameter => Diameter / 120;

        /// <summary>
        /// The boundaries of where orbiting bodies can spawn
        /// </summary>
        public Boundary PlanetaryBoundaries
        {
            get
            {
                var min = GameDiameter * 1.1f;
                var max = (Mass) * 0.6f;
                return new Boundary(min, max);
            }
        }

        protected virtual int GetDefaultLaneSize()
        {
            return 8000;
        }

        public int Seed { get; set; }
        // ---------
        // Functions

        public List<Boundary> GetPlanetLanes()
        {
            var defaultLaneSize = GetDefaultLaneSize();
            var laneBounds = PlanetaryBoundaries;
            var primeDistanceFromHost = laneBounds.Max * 0.65f;

            var distance = laneBounds.Distance;
            var numberOfLanes = (int) Mathf.Ceil(distance / defaultLaneSize);
            numberOfLanes = Mathf.Min(numberOfLanes, 12);

            var boundaries = new List<Boundary>();
            for (int i = 1; i < numberOfLanes; i++)
            {
                // Has a 60% chance of creating a lane.
                if (Rng.GetRandomNumber(0.0f, 1.0f, Seed) > 0.6f)
                {
                    continue;
                }

                var posScalar = i * defaultLaneSize;

                // Size lane based on distance from star
                var low = Mathf.Min(primeDistanceFromHost, posScalar);
                var high = Mathf.Max(primeDistanceFromHost, posScalar);

                var laneSize = defaultLaneSize * (low / high);

                // Tighten / expand distance between lanes
                var differenceBetweenSizes = laneSize - defaultLaneSize;

                // Create default boundary in position
                var startPos = (laneBounds.Min + posScalar) - differenceBetweenSizes / 2;
                var boundary = new Boundary(startPos, startPos + laneSize);

                boundaries.Add(boundary);
            }
            return boundaries;
        }

        /// <summary>
        /// This should be called when updating the object's personal properties
        /// </summary>
        public virtual void UpdateObjectParameters()
        {
            var local = GameDiameter;
            if (Parent != null && Parent.Obj != null)
            {
                local = (GameDiameter / Parent.Obj.transform.localScale.x) * 1.5f;
            }
            this.Obj.transform.localScale = new Vector3(local, local, local);

            // This is a dumb way to enforce that the object is in the right location
            UpdateOrbitLocation(0f);
            ForceVisualUpdate();

            if (Parent != null)
            {
                _swimLane = null;
                var sl = SwimLane;
            }
        }

        /// <summary>
        /// Update the orbit of the object (This will only work if it has a parent)
        /// -- THREADSAFE
        /// </summary>
        public virtual void UpdateOrbitLocation(float deltaTime)
        {
            // If the object doesn't have a parent, don't try setting an orbit.
            // It's happy where it is as it's the biggest boy on campus.
            if (Parent == null){ return; }

            var timeProgressionInAngle = OrbitSpeed * (deltaTime / MovementGranularity);
            
            MovementSteps.Clear();
            for (var step = 0; step < MovementGranularity; step++)
            {
                OrbitAngle += timeProgressionInAngle;
                
                var parentPos = Parent.MovementSteps.Count == 0 ? Parent.Pos : Parent.MovementSteps[step];
                var stepPos = parentPos + MathHelper.GetVectorFromAngle(parentPos, DistanceFromParent, OrbitAngle);
                MovementSteps.Add(stepPos);
            }
        }

        /// <summary>
        /// Update the rotation and orbit of the object's theory position.
        /// -- THREADSAFE
        /// </summary>
        public override void UpdateThisObject(float deltaTime)
        {
            //Obj.GetComponent<Renderer>().material.shader =
            //    Shader.Find(IsSelected ? "Self-Illumin/Outlined Diffuse" : "Diffuse");

            Rot = new Quaternion(90, Rotation, 90, 0);

            UpdateOrbitLocation(deltaTime);
        }

        public override void Dispose()
        {
            VectorLine.Destroy(ref _swimLane);
            base.Dispose();
        }
    }
}
