using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.CelestialBodies;
using Assets.Scripts.LS.Element;
using Assets.Scripts.Managers;
using ResourceManager = Assets.Scripts.Managers.ResourceManager;
using Element = Assets.Scripts.LS.Element;

namespace Assets.Scripts.LS.PlanetAccessories
{
    public class MineralWealth
    {
        public MineralWealth(Planet host)
        {
            Host = host;
            Elements = new Dictionary<Element.Element, float>();
        }

        public Planet Host { get; }

        public Dictionary<Element.Element, float> Elements { get; private set; }

        public void Init()
        {
            if (Host.IsGaseous)
            {
                CreateGaseousPlanetWealth();
            }
            else
            {
                CreateRockyPlanetWealth();
            }
        }

        private void CreateRockyPlanetWealth()
        {
            // There is a 10% chance that the planet has
            // no material wealth what so ever.
            if (Rng.GetRandomNumber(0.0f, 1.0f, Host.Seed) > 0.9)
            {
                return;
            }
            
            var amount = Host.Mass * (Host.Density * 1.5f);
            var elements = ResourceManager.Instance.GenerateElementPercentages(ElementType.Minerals, Host.Seed);
            
            foreach (var element in elements)
            {
                var materialAmount = element.Value / 10;
                if (materialAmount < 0.01f || Math.Abs(materialAmount) < 0.001f) continue;
                Elements.Add(ResourceManager.Instance.GetMineralFromElementName(element.Key), amount * materialAmount);
            }

            amount = Host.Density / 0.0005f;
            elements = ResourceManager.Instance.GenerateElementPercentages(ElementType.RockyGases, Host.Seed);

            foreach (var element in elements)
            {
                var elem = ResourceManager.Instance.GetGasFromElementName(element.Key);
                if (ResourceManager.IsGasOutsideBoundary(elem, Host)) continue;
                if (element.Value < 0.01f || Math.Abs(element.Value) < 0.001f) continue;

                Elements.Add(ResourceManager.Instance.GetGasFromElementName(element.Key), amount * element.Value/1000);
            }
        }

        private void CreateGaseousPlanetWealth()
        {
            // There is a 10% chance that the planet has
            // no material wealth what so ever.
            if (Rng.GetRandomNumber(0.0f, 1.0f, Host.Seed) > 0.9)
            {
                return;
            }

            var amount = Host.Volume * 0.4f;
            var elements = ResourceManager.Instance.GenerateElementPercentages(ElementType.GasGiantGases, Host.Seed);

            var cutoff = 0.0f;
            foreach (var element in elements)
            {
                if (Rng.GetRandomNumber(0.0f, 1.0f, Host.Seed) > cutoff)
                {
                    Elements.Add(ResourceManager.Instance.GetGasFromElementName(element.Key), amount * element.Value);
                }
                cutoff += 0.5f;
            }
        }
    }
}
