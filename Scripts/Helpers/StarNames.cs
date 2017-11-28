using System.IO;
using Assets.Scripts.Managers;

namespace Assets.Scripts.Helpers
{
    public static class StarNames
    {
        public static string GetUniqueStarName(SSManager manager)
        {
            // TODO This is a dumb way to hardcode this. This should be fixed up later
            var fileName = @"E:\Unity\UITest\Assets\Scripts\SolarNames.txt";

            if (!File.Exists(fileName))
            {
                return "NO FILE WAS FOUND";
            }

            var starNames = File.ReadAllLines(fileName);

            var name = GetName(starNames);

            while (!manager.SolarSystems.IsNameTaken(name))
            {
                name = GetName(starNames);
            }

            return name;
        }

        private static string GetName(string[] names)
        {
            return names[Rng.GetRandomNumber(0, names.Length)];
        }
    }
}
