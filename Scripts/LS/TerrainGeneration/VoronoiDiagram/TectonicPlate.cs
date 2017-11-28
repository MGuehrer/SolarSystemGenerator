using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using csDelaunay;

namespace Circle.Delaunay
{
    public class TectonicPlate
    {
        public TectonicPlate(int identifier)
        {
            var time = DateTime.Now.Millisecond;
            var r = (int)Math.Round(new Random(time).NextDouble() * int.MaxValue);
            var rng = new Random(r);

            Direction = (int)(rng.NextDouble() * 359);
            Identifier = identifier;
            Sites = new List<Site>();
        }

        public TectonicPlate(int identifier, Site centerSite)
            : this(identifier)
        {
            Sites.Add(centerSite);
        }

        public void AddSite(Site site)
        {
            Sites.Add(site);
            site.TectonicPlate = this;
        }

        public int Identifier { get; }

        public List<Site> Sites { get; private set; }

        public int Direction { get; set; }

        public Point CenterOfPlate => Sites[0].Coord.ToPoint();

        public Dictionary<TectonicPlate, int> NeighbourPlates { get; private set; }

        public void AssociatePlateToGlobe()
        {
            NeighbourPlates = new Dictionary<TectonicPlate, int>();

            foreach (var site in NeighbourSites)
            {
                var ntp = site.TectonicPlate;
                if (ntp == this) continue;

                if (NeighbourPlates.ContainsKey(ntp)) continue;

                var angle = (int)Site.GetAngleBetweenTwoPoints(this.CenterOfPlate, ntp.CenterOfPlate);
                angle += 180;

                var dMin = (Direction - 45);
                var dMax = (Direction + 45);
                if (IsBetween(dMin, dMax, angle))
                {
                    NeighbourPlates.Add(ntp, 16);
                    continue;
                }

                var aMin = (Direction + 90) - 45;
                var aMax = (Direction + 90) + 45;
                if (IsBetween(aMin, aMax, (angle)))
                {
                    NeighbourPlates.Add(ntp, -2);
                    continue;
                }

                var bMin = (Direction - 90) - 45;
                var bMax = (Direction - 90) + 45;
                if (IsBetween(bMin, bMax, (angle)))
                {
                    NeighbourPlates.Add(ntp, -2);
                    continue;
                }

                var cMin = (Direction - 180) - 45;
                var cMax = (Direction - 180) + 45;
                if (IsBetween(cMin, cMax, (angle)))
                {
                    NeighbourPlates.Add(ntp, -16);
                    continue;
                }
            }
        }

        public void EqualisePlateHeight(int iteration)
        {
            for (int i = 0; i < iteration; i++)
            {
                var middleValue = Sites.Sum(s => s.MapWeight) / Sites.Count;

                foreach (var site in Sites)
                {
                    if (site.MapWeight > middleValue) site.MapWeight -= 4;
                    if (site.MapWeight < middleValue) site.MapWeight += 4;
                }
            }
        }

        public bool IsBetween(float min, float max, float targetAngle)
        {
            var normalisedMin = min > 0 ? min : 2 * Math.PI + min;
            var normalisedMax = max > 0 ? max : 2 * Math.PI + max;
            var normalisedTarget = targetAngle > 0 ? targetAngle : 2 * Math.PI + targetAngle;

            return normalisedMin <= normalisedTarget && normalisedTarget <= normalisedMax;
        }

        // TODO this is a boundary static function
        public static int Loop(int min, int max, int value)
        {
            var r = value;
            if (value < min) r += max;
            if (value > max) r -= max;
            return r;
        }

        // TODO this is a boundary static function
        public static bool Contains(float min, float max, float value)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Get the unassigned neighbours with a cost association.
        /// </summary>
        public Dictionary<Site, int> TectoniclessNeighbourSites
        {
            get
            {
                var dict = new Dictionary<Site, int>();
                foreach (var site in NeighbourSites)
                {
                    if (site.TectonicPlate != null || dict.ContainsKey(site)) continue;

                    var cost = Math.Abs(site.MapWeight - Sites[0].MapWeight);

                    dict.Add(site, cost);
                }
                return dict;
            }
        }

        public IEnumerable<Site> NeighbourSites
        {
            get
            {
                foreach (var site in Sites)
                {
                    foreach (var nSite in site.GetAllNeighbors())
                    {
                        yield return nSite;
                    }
                }
            }
        }
    }
}
