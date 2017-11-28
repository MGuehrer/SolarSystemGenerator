using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public sealed class TimeManager : MonoBehaviour
    {
        #region ManagerObjects

        private UIManager UI;

        #endregion

        #region Events


        /// <summary>
        /// This will run on update events
        /// (Unaffected by gamespeed)
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// This will run on ALL update events. (Unaffected by gamespeed)
        /// </summary>
        public event EventHandler GUITick;
        
        /// <summary>
        /// This will only run on day change events. This is affected
        /// by gamespeed and FPS.
        /// </summary>
        public event EventHandler DayChange;

        /// <summary>
        /// This will only run on year change events. This is affected
        /// by gamespeed and FPS.
        /// </summary>
        public event EventHandler YearChange;


        #endregion
        private TimeManager()
        {
        }

        void Start()
        {
            UI = gameObject.GetComponent<UIManager>();
            
            // Subscribe to time related UI elements
            UI.btn_SpeedIncrease.Subscribe((sender, args) => UnsafeSpeed += 1);
            UI.btn_SpeedDecrease.Subscribe((sender, args) => UnsafeSpeed -= 1);

            // The default speed
            UnsafeSpeed = 1;

            Date = new GameDateTime();
            Date.DayChange += DayChangeEvent;
            Date.YearChange += YearChangeEvent;
        }

        void Update()
        {
            DeltaTime = Time.deltaTime*Speed;

            // This runs no matter what
            GUITick?.Invoke(this, null);

            Tick?.Invoke(this, null);

            if (!IsPaused)
            {
                // This will run if the game is unpaused on update events.
                ProgressDay(DeltaTime * Speed);
            }
        }

        private void DayChangeEvent(object sender, EventArgs e)
        {
            DayChange?.Invoke(sender, e);
        }

        private void YearChangeEvent(object sender, EventArgs e)
        {
            YearChange?.Invoke(sender, e);
        }

        private void ProgressDay(float amount)
        {
            _runningDayDeltaTime += DeltaTime * Speed;
            DayProgress += amount;
            
            if (DayProgress > DayLength)
            {
                DayDeltaTime = _runningDayDeltaTime;
                _runningDayDeltaTime = 0;
                Date.IncrementDay();
                DayProgress = 0;

                UI.txt_GameDate.text = $"Date: {Date}";
            }
        }

        /// <summary>
        /// How long the day lasts in seconds
        /// </summary>
        public float DayLength => 2.0f;

        public float DayProgress { get; private set; }

        public float DayProgressPercent => DayProgress / DayLength;

        /// <summary>
        /// The game date of the game. This will change every 1ish second (depending on gametime)
        /// This will send day and year change events
        /// </summary>
        public GameDateTime Date { get; set; }

        private int _unsafeSpeed;
        /// <summary>
        /// Multiplier speed that has a global effect.
        /// </summary>
        public int UnsafeSpeed {
            get { return _unsafeSpeed; }
            set
            {
                DayProgress -= Mathf.Floor(DayProgress);
                SSManager.Instance.RunUpdateOnAll();
                _unsafeSpeed = Mathf.RoundToInt(Boundary.Clamp(0, 6, value));
                UI.txt_GameSpeed.text = $"{Speed}x";
            }
        }

        public int Speed
        {
            get
            {
                switch (_unsafeSpeed)
                {
                    case 0:
                        return 0;
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 5;
                    case 4:
                        return 10;
                    case 5:
                        return 25;
                    case 6:
                        return 50;
                }
                return 0;
            }
        }

        /// <summary>
        /// If is paused, nothing will update.
        /// </summary>
        public bool IsPaused
        {
            get { return UnsafeSpeed == 0; }
            set { UnsafeSpeed = value ? 0 : UnsafeSpeed == 0 ? 1 : UnsafeSpeed; }
        }

        /// <summary>
        /// Get the deltaTime
        /// </summary>
        public float DeltaTime { get; private set; }

        private float _runningDayDeltaTime;
        public float DayDeltaTime { get; private set; }
    }

    /// <summary>
    /// The GameDateTime runs on a day/year schedule where there are 365 days per year
    /// </summary>
    public class GameDateTime
    {
        public EventHandler DayChange;
        public EventHandler MonthChange;
        public EventHandler YearChange;

        public GameDateTime()
        {
            Day = 0;
            Year = 0;
        }

        private int _day;
        /// <summary>
        /// The day of the calendar. There are 360 days to a year in the game (30 days per month)
        /// if the value is greater than 30, then a month will be added to the date
        /// </summary>
        public int Day {
            get { return _day; }
            set
            {
                _day = value;
                if (_day > 30)
                {
                    _day = _day - 30;
                    Month++;
                }
                DayChange?.Invoke(this, null);
            }
        }

        private int _month;
        /// <summary>
        /// The day of the calendar. There are 12 months a year in the game
        /// if the value is greater than12, then a year will be added to the date
        /// and this will revert to the start of the calendar.
        /// </summary>
        public int Month
        {
            get { return _month; }
            set
            {
                _month = value;
                if (_month > 12)
                {
                    _month = _month - 12;
                    Year++;
                }
                MonthChange?.Invoke(this, null);
            }
        }

        private int year;
        /// <summary>
        /// 360 days to a year, no end date.
        /// </summary>
        public int Year
        {
            get { return year; }
            set
            {
                year = value;
                YearChange?.Invoke(this, null);
            }
        }

        public int GetYearLengthInDays()
        {
            return 360;
        }

        public void IncrementDay()
        {
            Day += 1;
        }

        public override string ToString()
        {
            return $"{Day} / {Month} / {Year}";
        }
    }
}
