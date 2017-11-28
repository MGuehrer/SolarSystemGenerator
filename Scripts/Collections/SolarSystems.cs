using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.LS
{
    public class SolarSystems
    {
        public SolarSystems(List<SolarSystem> systems)
        {
            Systems = systems;
        }

        public List<SolarSystem> Systems { get; }

        public bool IsNameTaken(string name)
        {
            if (Systems == null) return true;
            //return Systems.Any(s => s..Equals(name));
            return true;
        }
    }
}
