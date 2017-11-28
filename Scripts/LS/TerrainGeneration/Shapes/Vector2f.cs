using System;
using Assets.Scripts.Helpers;

// Recreation of the UnityEngine.Vector3, so it can be used in other thread
namespace Circle.Shapes
{
    public struct Vector2f {
	
        public float x;
        public float y;

        public static readonly Vector2f zero = new Vector2f(0,0);
        public static readonly Vector2f one = new Vector2f(1,1);

        public static readonly Vector2f right = new Vector2f(1,0);
        public static readonly Vector2f left = new Vector2f(-1,0);
	
        public static readonly Vector2f up = new Vector2f(0,1);
        public static readonly Vector2f down = new Vector2f(0,-1);
	
        public Vector2f(float x, float y) {
            this.x = x;
            this.y = y;
        }
        public Vector2f(double x, double y) {
            this.x = (float)x;
            this.y = (float)y;
        }
	
        public float magnitude {
            get{
                return (float)Math.Sqrt(x*x + y*y);
            }
        }

        public bool IsEmpty => Math.Abs(x - (-1)) < 0.01f && Math.Abs(y - (-1)) < 0.01f;

        public void Normalize() {
            float magnitude = this.magnitude;
            x /= magnitude;
            y /= magnitude;
        }

        public Vector2f MidPoint(Vector2f target)
        {
            var nx = (this.x + target.x) / 2;
            var ny = (this.y + target.y) / 2;

            return new Vector2f(nx,ny);
        }

        public Point ToPoint()
        {
            return new Point((int)x, (int)y);
        }

        public static Vector2f Normalize(Vector2f a) {
            float magnitude = a.magnitude;
            return new Vector2f(a.x/magnitude, a.y/magnitude);
        }
	
        public override bool Equals(object other) {
            if (!(other is Vector2f)) {
                return false;
            }
            Vector2f v = (Vector2f) other;
            return Math.Abs(x - v.x) < 0.1f &&
                   Math.Abs(y - v.y) < 0.1f;
        }
	
        public override string ToString () {
            return string.Format ("[Vector2f]"+x+","+y);
        }
	
        public override int GetHashCode () {
            return x.GetHashCode () ^ y.GetHashCode () << 2;
        }

        public float DistanceSquare(Vector2f v) {
            return Vector2f.DistanceSquare(this, v);
        }
        public static float DistanceSquare(Vector2f a, Vector2f b) {
            float cx = b.x - a.x;
            float cy = b.y - a.y;
            return cx*cx + cy*cy;
        }
	
        public static bool operator == (Vector2f a, Vector2f b) {
            return a.x == b.x && 
                   a.y == b.y;
        }
	
        public static bool operator != (Vector2f a, Vector2f b) {
            return a.x != b.x ||
                   a.y != b.y;
        }
	
        public static Vector2f operator - (Vector2f a, Vector2f b) {
            return new Vector2f( a.x-b.x, a.y-b.y);
        }
	
        public static Vector2f operator + (Vector2f a, Vector2f b) {
            return new Vector2f( a.x+b.x, a.y+b.y);
        }

        public static Vector2f operator * (Vector2f a, int i) {
            return new Vector2f(a.x*i, a.y*i);
        }
	
        public static Vector2f Min(Vector2f a, Vector2f b) {
            return new Vector2f(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }
        public static Vector2f Max(Vector2f a, Vector2f b) {
            return new Vector2f(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
    }
}
