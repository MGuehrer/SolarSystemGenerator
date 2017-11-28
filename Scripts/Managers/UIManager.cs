using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.LS.CelestialBodies;
using Assets.Scripts.Objects;
using Assets.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas CanvasObj;
        // Top bar Objects
        [SerializeField] private GameObject TopBar;
        [SerializeField] private GameObject TopBarText;

        // Menu bar Objects
        [SerializeField] private GameObject MenuBar;

        // Map bar objects
        [SerializeField] private GameObject MapBar;
        [SerializeField] private GameObject MapBarText;
        [SerializeField] private GameObject ResetButton;

        // Time Bar objects
        [SerializeField] private GameObject TimeBar;
        [SerializeField] private GameObject TimeBarDateText;
        [SerializeField] private GameObject TimeBarSpeedUp;
        [SerializeField] private GameObject TimeBarSpeedDown;
        [SerializeField] private GameObject TimeBarSpeedText;


        #region Public UI Scripts

        public UIClickableElement btn_SpeedIncrease => TimeBarSpeedUp.GetComponent<UIClickableElement>();
        public UIClickableElement btn_SpeedDecrease => TimeBarSpeedDown.GetComponent<UIClickableElement>();
        public Text txt_GameSpeed => TimeBarSpeedText.GetComponent<Text>();
        public Text txt_GameDate => TimeBarDateText.GetComponent<Text>();

        #endregion

        private TimeManager _clock;

        void Awake()
        {
            SSManager.Instance.OnSelectionUpdate += OnSelectionUpdate;

            ResetButton.AddComponent<UIClickableElement>();
            ResetButton.GetComponent<UIClickableElement>().Subscribe((sender, args)=> GameObject.Find("GameManager").GetComponent<CreateGame>().NewGame());


            // Get the clock from it's game object.
            _clock = gameObject.GetComponent<TimeManager>();
        }

        private void OnSelectionUpdate(object sender, EventArgs eventArgs)
        {
            var obj = SSManager.Instance.SelectedObjects.FirstOrDefault();
            if (obj == null) return;

            var displayName = obj.DisplayName;
            TopBarText.GetComponent<Text>().text = displayName;

            var star = obj as Star;
            if (star != null)
            {
                MapBarText.GetComponent<Text>().text =
                    $"{displayName}\nMass: {star.Mass}" +
                    $"\nDiameter: {star.Diameter}" +
                    $"\nDensity: {star.Density}" +
                    $"\nVolume: {star.Volume}" +
                    $"\nLuminosity: {star.Luminosity}" +
                    $"\nMaximumDistanceForTemp: {star.MaximumDistanceForHeat}" +
                    $"\nTemperature: {star.Temperature}";
                return;
            }

            var planet = obj as Planet;
            if (planet != null)
            {
                var atmospherics = "Atmosphere: \n";
                foreach (var atmospheric in planet.Atmosphere.Elements)
                {
                    atmospherics += " * " + atmospheric.Key + $" ({atmospheric.Value})\n";
                }

                var mins = "Minerals: \n";
                foreach (var element in planet.MaterialWealth.Elements)
                {
                    mins += " * " + element.Key + $" ({element.Value})\n";
                }

                MapBarText.GetComponent<Text>().text =
                    $"{displayName}" +
                    $"\nParent: {planet.Parent}" +
                    $"\nMass: {planet.Mass} / {planet.Parent.Mass} ({planet.Mass / planet.Parent.Mass})" +
                    $"\nDiameter: {planet.Diameter} / {planet.Parent.Diameter}" +
                    $"\nOrbital Year: {planet.OrbitalYear}" +
                    $"\nDensity: {planet.Density}" +
                    $"\nVolume: {planet.Volume}" +
                    $"\nTemperature: {planet.Temperature}" +
                    $"\nIsGaseous: {planet.IsGaseous}" +
                    $"\n{atmospherics}" +
                    $"\n{mins}"
                    ;
                return;
            }
        }
    }
}
