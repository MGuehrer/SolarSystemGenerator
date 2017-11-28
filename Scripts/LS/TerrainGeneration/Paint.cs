using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.LS.TerrainGeneration
{
    public static class Painter
    {
        private static void Swap(ref int n1, ref int n2)
        {
            var holder = n2;
            n2 = n1;
            n1 = holder;
        }

        public static void FillTriangleSimple(PaintTexture bitmap, Rectangle canvas, int x0, int y0, int x1, int y1, int x2, int y2, int color)
        {
            var c = (float)(color / 255f);
            TexturePoint[] pixels = bitmap.GetPixels(canvas).ToArray();
            int width = canvas.Width;
            int height = canvas.Height;
            // sort the points vertically
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            if (y0 > y1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            double dx_far = Convert.ToDouble(x2 - x0) / (y2 - y0 + 1);
            double dx_upper = Convert.ToDouble(x1 - x0) / (y1 - y0 + 1);
            double dx_low = Convert.ToDouble(x2 - x1) / (y2 - y1 + 1);
            double xf = x0;
            double xt = x0 + dx_upper; // if y0 == y1, special case
            for (int y = y0; y <= (y2 > height - 1 ? height - 1 : y2); y++)
            {
                if (y >= 0)
                {
                    for (int x = (xf > 0 ? Convert.ToInt32(xf) : 0); x <= (xt < width ? xt : width - 1); x++)
                        pixels[Convert.ToInt32(x + y * width)].Value = c;
                    for (int x = (xf < width ? Convert.ToInt32(xf) : width - 1); x >= (xt > 0 ? xt : 0); x--)
                        pixels[Convert.ToInt32(x + y * width)].Value = c;
                }
                xf += dx_far;
                if (y < y1)
                    xt += dx_upper;
                else
                    xt += dx_low;
            }
        }
    }
}
