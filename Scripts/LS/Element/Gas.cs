using Assets.Scripts.Helpers;

namespace Assets.Scripts.LS.Element
{
    public class Gas : Element
    {
        public Gas(string name, float density, float greenhouseEffect, float commonality, Boundary safeTempZone)
        {
            Name = name;
            Density = density;
            GreenhouseEffect = greenhouseEffect;
            Commonality = commonality;
            TemperatureAllowances = safeTempZone;
        }
        public float GreenhouseEffect { get; }
    }
}

