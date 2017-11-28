using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Collections;
using Assets.Scripts.Helpers;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Objects
{
    public abstract class SSObject : MonoBehaviour
    {
        protected SSObject(string name, string displayName)
        {
            Obj.name = name;
            DisplayName = displayName;
            Identifier = SSManager.Instance.GetSafeIndentifier();

            SSManager.Instance.Children.Add(this);
        }


        private TimeManager _clock;
        /// <summary>
        /// External Clock manager. Use this to reference the gameclock.
        /// </summary>
        public TimeManager Clock
        {
            get
            {
                if (_clock == null)
                {
                    var gameManager = GameObject.FindGameObjectWithTag("GameManager");
                    _clock = gameManager.GetComponent<TimeManager>();
                }
                return _clock;
            }
        }

        /// <summary>
        /// This is how granular the movement steps are. increase for
        /// more precise movements. Decrease for speed.
        /// </summary>
        public int MovementGranularity => 80;


        private List<Vector3> _movementSteps;
        public List<Vector3> MovementSteps
        {
            get { return _movementSteps ?? (_movementSteps = new List<Vector3>()); }
            set { _movementSteps = value; }
        }

        /// <summary>
        /// This is a threadsafe variation of update to allow only
        /// theoretical changes to this object. This will not update
        /// GameObject changes.
        /// </summary>
        public abstract void UpdateThisObject(float deltaTime);

        /// <summary>
        /// This will update gameobject changes over the delta time
        /// This takes a bit of time to do and is not threadsafe.
        /// </summary>
        public virtual void VisualUpdate(float deltaTime)
        {
            DistanceToPos = Vector3.Distance(Obj.transform.position, Pos);

            if (MovementSteps.Count != 0)
            {
                var step = Mathf.RoundToInt(MovementGranularity * Clock.DayProgressPercent);

                Boundary.Clamp(0, MovementSteps.Count-1, ref step);
                Obj.transform.position = MovementSteps[step];
                Pos = MovementSteps[step];
            }
            //Obj.transform.position = Vector3.MoveTowards(Obj.transform.position, Pos, deltaTime*DistanceToPos);
            Obj.transform.rotation = Rot;

            foreach (var ssObject in Children.GetCollection())
            {
                ssObject.VisualUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Instantly update all visual changes on the object and it's children.
        /// Not threadsafe.
        /// </summary>
        public virtual void ForceVisualUpdate()
        {
            Obj.transform.position = Pos;
            Obj.transform.rotation = Rot;

            foreach (var ssObject in Children.GetCollection())
            {
                ssObject.ForceVisualUpdate();
            }
        }


        private float DistanceToPos { get; set; }

        private void OnMouseDown()
        {
            // TODO This is crazy dumb. this needs to be smarter
            if (IsSelectable && Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    IsSelected = true;
                }
            }
            else
            {
                IsSelected = false;
            }
        }

        /// <summary>
        /// The actual game object this is attached to.
        /// </summary>
        public GameObject Obj { get; set; }

        /// <summary>
        /// A simple identifier for later. this has questionable use.
        /// TODO Audit the usages of these and see if it's needed.
        /// </summary>
        public int Identifier { get; }

        /// <summary>
        /// This visible name of the object (This is displayed to the user)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A simple description of the object. (This is displayed to the user)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Allow / Disallow it to be selected.
        /// </summary>
        public abstract bool IsSelectable { get; }

        private SSCollection _children;
        /// <summary>
        /// A list of any SSObject children this SSObject has.
        /// </summary>
        public SSCollection Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new SSCollection();
                    _children.CollectionModified += ChildrenModified;
                }
                return _children;
            }
        }

        private void ChildrenModified(object sender, EventArgs eventArgs)
        {
            foreach (var child in Children.GetCollection())
            {
                child.Obj.transform.parent = this.Obj.transform;
            }
        }

        // Selection Event
        private bool _isSelected;

        /// <summary>
        /// If the object is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                SSManager.Instance.OnSelectionChange.Invoke(this, null);
            }
        }

        /// <summary>
        /// The "Theoretical" location of the object.
        /// This should remain threadsafe.
        /// </summary>
        public Vector3 Pos { get; set; }

        /// <summary>
        /// The "Theoretical" rotation of the object
        /// This should remain threadsafe.
        /// </summary>
        public Quaternion Rot { get; set; }

        /// <summary>
        /// Destroy this object.
        /// </summary>
        public virtual void Dispose()
        {
            Destroy(Obj);
        }
    }
}
