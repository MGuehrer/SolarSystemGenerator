using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.CelestialBodies;
using Assets.Scripts.LS.Element;
using Assets.Scripts.Managers;

namespace Assets.Scripts.LS.PlanetAccessories
{
    public class Atmosphere
    {
        public Atmosphere(Planet host)
        {
            Host = host;
            Elements = new Dictionary<Gas, float>();
        }

        public Planet Host { get; }

        public Dictionary<Gas, float> Elements { get; }

        public bool HasAtmosphere => Math.Abs(Density) > 0.001f;

        public float GreenhouseEffect
        {
            get
            {
                float sum = 0;
                foreach (var element in Elements)
                {
                    sum += (element.Value * 0.75f) * element.Key.GreenhouseEffect;
                }
                return 1.0f + sum;
            }
        }

        public float Density => Elements.Sum(e => e.Value);

        public void Init()
        {
            if (Host.IsGaseous)
            {
                CreateGaseousPlanetAtmosphere();
            }
            else
            {
                CreateRockyPlanetAtmosphere();
            }
        }

        private void CreateRockyPlanetAtmosphere()
        {
            var densityBoundary = new Boundary(0.4f, 4.8f);
            var massBoundary = new Boundary(8000, 14000);

            //if (!densityBoundary.Contains(Host.Density)) return;
            //if (!massBoundary.Contains(Host.Mass)) return;
            //if ((Host.Mass / Host.Radius) < 1) return;

            // TODO this needs a better density generator
            var atmosphericDensity = Rng.GetRandomNumber(0.01f, 3.0f, Host.Seed);
            
            var percentages = ResourceManager.Instance.GenerateElementPercentages(ElementType.Atmospherics, Host.Seed);
            foreach (var element in percentages)
            {
                if (ResourceManager.IsGasOutsideBoundary(ResourceManager.Instance.GetGasFromElementName(element.Key), Host)) continue;
                if (element.Value < 0.008f) continue;

                // Add the element
                Elements.Add(ResourceManager.Instance.GetGasFromElementName(element.Key), atmosphericDensity * element.Value);
            }
        }

        private void CreateGaseousPlanetAtmosphere()
        {
            // TODO this needs a better density generator
            var atmosphericDensity = Rng.GetRandomNumber(Host.Mass*0.02f, Host.Mass*0.04f, Host.Seed);

            var materials = Host.MaterialWealth.Elements;
            var totalAmount = materials.Sum(material => material.Value);
            foreach (var material in materials)
            {
                Elements.Add(ResourceManager.Instance.GetGasFromElementName(material.Key.Name), atmosphericDensity * (material.Value / totalAmount));
            }
        }
    }
}
