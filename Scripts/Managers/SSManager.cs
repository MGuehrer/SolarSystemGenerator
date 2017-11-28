using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Collections;
using Assets.Scripts.LS;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public sealed class SSManager
    {
        // --------------
        // Singleton code
        private static readonly Lazy<SSManager> Manager = new Lazy<SSManager>(() => new SSManager());

        public static SSManager Instance => Manager.Value;

        private SSManager()
        {
            OnSelectionChange += AddSelectedObject;
        }

        // ------
        // Events
        public EventHandler OnSelectionChange;
        public EventHandler OnSelectionUpdate;

        /// <summary>
        /// Every SSObject entity in the game.
        /// </summary>
        public List<SSObject> Children { get; } = new List<SSObject>();

        public SolarSystems SolarSystems
        {
            get
            {
                lock (Children)
                {
                    return new SolarSystems(Children.Where(c => c is SolarSystem) as List<SolarSystem>);
                }
            }
        }

        public SelectedObjects SelectedObjects { get; } = new SelectedObjects();

        private void AddSelectedObject(object sender, EventArgs e)
        {
            var obj = sender as SSObject;
            if (obj != null && SelectedObjects.Contains(obj)) return;

            // If the user isn't looking to add more to the selection,
            // then clear out any previously selected object.
            if (!Input.GetButton("Control")) SelectedObjects.Clear();

            SelectedObjects.Add(obj);
            OnSelectionUpdate?.Invoke(this, null);
        }

        /// <summary>
        /// Get a free identifier number
        /// </summary>
        /// <returns></returns>
        public int GetSafeIndentifier()
        {
            // TODO This is dumb and slow. Fix this up later
            lock (Children)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    if (Children[i] == null) return i;
                }
                return Children.Count + 1;
            }
        }

        public void RunUpdateOnAll()
        {
            Task.Run(() =>
            {
                foreach (var ssObject in Children)
                {
                    ssObject.UpdateThisObject(ssObject.Clock.DayDeltaTime);
                }
            });
        }
    }
}
