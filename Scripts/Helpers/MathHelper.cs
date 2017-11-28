using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.LS.CelestialBodies;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class MathHelper
    {
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        public static float GetAngleBetweenTwoVectors(Vector3 v1, Vector3 v2)
        {
            return Vector3.Angle(v1, v2);
        }

        public static Vector3 GetVectorFromAngle(Vector3 center, float distance, float angle)
        {
            //convert from degrees to radians
            var radians = angle * Mathf.Deg2Rad;

            var x = Mathf.Cos(radians);
            var z = Mathf.Sin(radians);
            return new Vector3(x, 0, z) * distance;
        }

        public static Point GetVectorFromAngle(Point center, float distance, float angle)
        {
            //convert from degrees to radians
            var radians = angle * Mathf.Deg2Rad;

            var x = Mathf.RoundToInt(Mathf.Cos(radians) * distance);
            var y = Mathf.RoundToInt(Mathf.Sin(radians) * distance);
            return new Point(x, y);
        }

        public static Vector3 GetWorldSpaceFromLocal(Vector3 parent, Vector3 local)
        {
            return new Vector3(parent.x - local.x, parent.y - local.y, parent.z - local.z);
        }

        public static List<int> SeedNumberList(int min, int max)
        {
            var returnList = new List<int>();
            var runningList = new List<int>();
            
            for (int i = min; i <= max; i++)
            {
                runningList.Add(i);
            }

            while (runningList.Count != 0)
            {
                var n = Rng.GetRandomNumber(0, runningList.Count-1);
                returnList.Add(runningList[n]);
                runningList.RemoveAt(n);
            }

            return returnList;
        }
    }
}
