using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using Circle.Shapes;

namespace Assets.Scripts.LS.TerrainGeneration.Shapes
{
    class PolygonTriangle
    {
        public Point A;
        public Point B;
        public Point C;

        public PolygonTriangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Rectangle Canvas
        {
            get
            {
                var list = new List<Point>(){A,B,C};

                var minX = list.Min(p => p.x);
                var minY = list.Min(p => p.y);
                var maxX = list.Max(p => p.x);
                var maxY = list.Max(p => p.x);

                return new Rectangle(minX, minY, maxX, maxY);
            }
        }
    }
}
