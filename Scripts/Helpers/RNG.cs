
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Helpers
{
    internal static class Rng
    {
        //Function to get random number
        private static Random _getrandom = new Random();
        private static readonly object SyncLock = new object();
        public static int GetRandomNumber(int min, int max, int seed = 0)
        {
            lock (SyncLock)
            { // synchronize
                _getrandom = SeedRandom(seed);
                return _getrandom.Next(min, max);
            }
        }

        public static float GetRandomNumber(float min, float max, int seed = 0)
        {
            lock (SyncLock)
            { // synchronize
                _getrandom = SeedRandom(seed);
                return (float)_getrandom.NextDouble() * (max - min) + min;
            }
        }

        public static float GetRandomNumber(Boundary bounds, int seed = 0)
        {
            lock (SyncLock)
            {
                _getrandom = SeedRandom(seed);
                return GetRandomNumber(bounds.Min, bounds.Max);
            }
        }

        public static int GetRandomNumber(int min, int max, int count, int seed = 0)
        {
            lock (SyncLock)
            {
                // synchronize
                var scalar = seed < 0 ? count : count / -1;
                _getrandom = SeedRandom(seed+scalar);
                return _getrandom.Next(min, max);
            }
        }

        public static List<float> GetRandomNumbers(float min, float max, int count, int seed = 0)
        {
            lock (SyncLock)
            {
                var f = new List<float>();
                // synchronize
                for (int i = 0; i < count; i++)
                {
                    var scalar = (i+1) * 100.0f/count;
                    _getrandom = SeedRandom(Mathf.RoundToInt(seed*scalar));
                    f.Add((float)_getrandom.NextDouble() * (max - min) + min);
                }
                return f;
            }
        }

        private static Random SeedRandom(int seed)
        {
            if (seed == 0) seed = System.DateTime.Now.Millisecond;
            return new Random(seed);
        }
    }
}
