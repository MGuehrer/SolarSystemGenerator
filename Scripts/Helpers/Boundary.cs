using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class Boundary
    {
        public Boundary(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; set; }
        public float Max { get; set; }

        /// <summary>
        /// How much is between the min and max value
        /// </summary>
        public float Distance => Max - Min;

        /// <summary>
        /// The middle value between min and max
        /// </summary>
        public float Mean => (Min + Max) / 2;

        public bool Contains(float value)
        {
            return value > Min && value < Max;
        }

        /// <summary>
        /// Get at what percentage the value is between
        /// the min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public float Ratio(float value)
        {
            var a = Max - Min;
            var b = value - Min;
            return b / a;
        }

        /// <summary>
        /// Gets the value from the percentage between the min / max
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public float GetValueFromRatio(float ratio)
        {
            return (Distance * ratio) + Min;
        }

        /// <summary>
        /// If value is outside of the min / max, then return the closest value.
        /// </summary>
        public float Clamp(float value)
        {
            if (value < Min) return Min;
            if (value > Max) return Max;
            return value;
        }

        // ------------------------------------------------------------------------
        // --------- Static Functions ---------------------------------------------
        // ------------------------------------------------------------------------

        /// <summary>
        /// If value is outside of the min / max, then return the closest value.
        /// </summary>
        /// <returns></returns>
        public static float Clamp(float min, float max, float value)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(float min, float max, int value)
        {
            return Mathf.RoundToInt(Clamp(min,max,(float)value));
        }

        /// <summary>
        /// If value is outside of the min / max, then return the closest value.
        /// </summary>
        public static void Clamp(float min, float max, ref int value)
        {
            value = Mathf.RoundToInt(Clamp(min, max, value));
        }

        public static bool Contains(int min, int max, float value)
        {
            return value > min && value < max;
        }

        public static bool Contains(float min, float max, float value)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Get at what percentage the value is between
        /// the min and max.
        /// </summary>
        /// <returns></returns>
        public static float Ratio(float min, float max, float value)
        {
            var a = max - min;
            var b = value - min;
            return b / a;
        }

        /// <summary>
        /// Returns the distance between the two given values
        /// If min is greater than max, it will return a negative.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float DistanceBetween(float min, float max)
        {
            return max - min;
        }

        public static float DistanceBetween(Point p1, Point p2)
        {
            var x1 = p1.x;
            var y1 = p1.y;
            var x2 = p2.x;
            var y2 = p2.y;

            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }

        public static int Loop(int min, int max, int value)
        {
            var r = value;
            if (value < min) r += max;
            if (value > max) r -= max;
            return r;
        }
    }
}
