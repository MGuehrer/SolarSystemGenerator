using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;

namespace Assets.Scripts.LS.Element
{
    public abstract class Element
    {
        /// <summary>
        /// The name of the element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// How dense the element is
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// The description of the element
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// How common the element is.
        /// </summary>
        public float Commonality { get; set; }

        /// <summary>
        /// At what temperature the element is most likely to spawn
        /// </summary>
        public Boundary TemperatureAllowances { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
