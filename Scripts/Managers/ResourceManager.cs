using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.CelestialBodies;
using Assets.Scripts.LS.Element;

namespace Assets.Scripts.Managers
{
    class ResourceManager
    {
        // TODO: This will one day have a random resource generator as well.


        // --------------
        // Singleton code
        private static readonly Lazy<ResourceManager> Manager = new Lazy<ResourceManager>(() => new ResourceManager());

        public static ResourceManager Instance => Manager.Value;

        private ResourceManager()
        {
            Iron = new Mineral("Iron", 7.87f);
            Silicon = new Mineral("Silicon", 2.53f);
            Magnesium = new Mineral("Magnesium", 1.74f);
            Aluminium = new Mineral("Aluminium", 2.7f);
            Copper = new Mineral("Copper", 8.95f);
            Lead = new Mineral("Lead", 11.34f);
            Sulfur = new Mineral("Sulfur", 2.01f);

            Hydrogen = new Gas("Hydrogen", 0.2f, 0.001f, 0.5f, new Boundary(-100, 8000));
            Oxygen = new Gas("Oxygen", 0.1f, 0.001f, 0.2f, new Boundary(-60, 60));
            Helium = new Gas("Helium", 0.01f, 0.01f, 0.1f, new Boundary(-300, 8000));
            Arsenic = new Gas("Arsenic", 1.2f, 0.01f, 0.002f, new Boundary(-100, 30));
            Methane = new Gas("Methane", 4.2f, 0.01f, 0.03f, new Boundary(-300, 4000));
            Radon = new Gas("Radon", 0.001f, 0.01f, 0.0001f, new Boundary(0, 4000));
            Solium = new Gas("Solium", 18.82f, 0.04f, 0.9f, new Boundary(800, 4000));
            CarbonDioxide = new Gas("Carbon Dioxide", 1.98f, 0.06f, 0.001f, new Boundary(-80, 80));
            Nitrogen = new Gas("Nitrogen", 0.09f, 0.005f, 0.001f, new Boundary(-140, 8000));
            Argon = new Gas("Argon", 1.38f, 0.05f, 0.001f, new Boundary(-140, 60));
        }

        // Minerals
        public Mineral Iron;
        public Mineral Silicon;
        public Mineral Magnesium;
        public Mineral Aluminium;
        public Mineral Copper;
        public Mineral Lead;
        public Mineral Sulfur;

        // Gases
        public Gas Hydrogen;
        public Gas Oxygen;
        public Gas Helium;
        public Gas Arsenic;
        public Gas Methane;
        public Gas Radon;
        public Gas Solium;
        public Gas CarbonDioxide;
        public Gas Nitrogen;
        public Gas Argon;

        public List<Mineral> Minerals => new List<Mineral>()
        {
            Iron,
            Silicon,
            Magnesium,
            Aluminium,
            Copper,
            Lead,
            Sulfur
        };

        public List<Gas> Gasses => new List<Gas>()
        {
            Hydrogen,
            Oxygen,
            Helium,
            Arsenic,
            Methane,
            Radon,
            Solium,
            CarbonDioxide,
            Nitrogen,
            Argon
        };

        public List<Gas> GasGiantGasses => new List<Gas>()
        {
            Hydrogen,
            Helium,
            Solium,
            Methane,
            Nitrogen
        };

        public List<Gas> Atmospherics => new List<Gas>()
        {
            Oxygen,
            Helium,
            Methane,
            CarbonDioxide,
            Nitrogen,
            Argon
        };

        public List<Gas> RockyGasses => new List<Gas>
        {
            Arsenic,
            Helium,
            Methane,
            Argon,
            Nitrogen
        };

        public Dictionary<string, float> GenerateElementPercentages(ElementType type, int rngSeed)
        {
            // index / percent
            var dict = new Dictionary<string, float>();

            var percentLeft = 1.0f;

            var minerals = type == ElementType.Minerals ? Minerals : null;
            List<Gas> gasses = null;
            if (type == ElementType.Gases) gasses = Gasses;
            if (type == ElementType.Atmospherics) gasses = Atmospherics;
            if (type == ElementType.GasGiantGases) gasses = GasGiantGasses;
            if (type == ElementType.RockyGases) gasses = RockyGasses;

            var count = minerals?.Count ?? gasses?.Count ?? -1;
            foreach (var index in MathHelper.SeedNumberList(0, count-1))
            {
                // there is a 10% chance that this element will no exist
                if (Rng.GetRandomNumber(0, 1.0f, rngSeed) > 0.9f) continue;

                var playingRoom = Boundary.Clamp(0f, 1f,
                    percentLeft - (minerals?[index].Commonality ?? gasses?[index].Commonality ?? -1f));
                var percent = Rng.GetRandomNumber(0, playingRoom, rngSeed);
                percentLeft -= playingRoom;
                dict.Add(minerals?[index].Name ?? gasses?[index].Name ?? "NO ELEMENT", percent);
            }

            return dict;
        }

        public Mineral GetMineralFromElementName(string name)
        {
            return Minerals.FirstOrDefault(m => m.Name.Equals(name));
        }

        public Gas GetGasFromElementName(string name)
        {
            return Gasses.FirstOrDefault(g => g.Name.Equals(name));
        }

        public static bool IsGasOutsideBoundary(Gas element, Planet host)
        {
            // These if statements will probably increase over time.
            if (!element.TemperatureAllowances.Contains(host.Temperature)) return true;

            return false;
        }
    }

    public enum ElementType
    {
        Minerals,
        Gases,
        Atmospherics,
        GasGiantGases,
        RockyGases
    }
}
