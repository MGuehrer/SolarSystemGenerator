using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class Point
    {
        internal Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        internal Point(float x, float y)
        {
            this.x = Mathf.RoundToInt(x);
            this.y = Mathf.RoundToInt(y);
        }

        public int x { get; set; }
        public int y { get; set; }

        public bool Equals(Point target)
        {
            return target.x.Equals(x) && target.y.Equals(y);
        }

        public override string ToString()
        {
            return $"{x}, {y}";
        }
    }
}