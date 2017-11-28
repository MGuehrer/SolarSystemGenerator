using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.LS.Element
{
    class Mineral : Element
    {
        public Mineral(string name, float density)
        {
            Name = name;
            Density = density;
        }
    }
}
