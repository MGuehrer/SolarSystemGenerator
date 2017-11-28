using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public class Points
    {
        public Points()
        {
            PointSet = new List<Point>();
        }

        public Points(List<Point> points)
        {
            PointSet = points;
        }

        public List<Point> PointSet { get; }

        public static bool ComparePointLocations(Point host, Point target)
        {
            return host.x == target.x && host.y == target.y;
        }
    }
}