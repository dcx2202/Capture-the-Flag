using System.Collections.Generic;
using General_Scripts.Labourers;
using GUIManager;
using UnityEngine;

namespace General_Scripts
{
    /// <summary>
    /// Base class for work components.
    /// </summary>
    public class WorkComponent : MonoBehaviour
    {
        /// <summary>
        /// Current durability of this work component.
        /// </summary>
        [SerializeField]
        [Tooltip("Current durability of this work component.")]
        private int _durability = 5;
        /// <summary>
        /// The maximum number of workers that can work this at this component at any given point.
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum number of workers that can work this at this component at any given point.")]
        private int _maximumWorkers = 5;
        /// <summary>
        /// Number of current workers. Only used for visualization in the editor.
        /// </summary>
        [SerializeField]
        [Tooltip("Number of current workers. Only used for visualization in the editor.")]
        private int _currentWorkers = 0;

        /// <summary>
        /// Indicates if this work component has a maximum number of workers that can work on it.
        /// </summary>
        [Tooltip("Indicates if this work component has a maximum number of workers that can work on it.")]
        public bool IsLimited;

        /// <summary>
        /// Workers currently working this component
        /// </summary>
        private readonly HashSet<Labourer> _workers = new HashSet<Labourer>();

        //private ResourcesGuiManager _resourcesGuiManager;

        private List<int> _myNumbers;

        /// <summary>
        /// Current durability of this work component.
        /// </summary>
        public int Durability
        {
            get { return _durability; }
            set
            {
                _durability = value;
                //_resourcesGuiManager.UpdateResources();
            }
        }

        /// <summary>
        /// The maximum number of workers that can work this at this component at any given point.
        /// </summary>
        public int MaximumWorkers
        {
            get { return _maximumWorkers; }
            set { _maximumWorkers = value; }
        }

        /// <summary>
        /// Number of current workers working this component
        /// </summary>
        public int CurrentWorkers
        {
            get { return _workers.Count; }
        }

        /// <summary>
        /// True if there is room to fit another worker.
        /// </summary>
        public bool CanBeWorked
        {
            get { return CurrentWorkers < MaximumWorkers && CurrentWorkers < _durability; }
        }

        /// <summary>
        /// True if there are too many workers on it
        /// </summary>
        public bool IsOverloaded
        {
            get { return CurrentWorkers > MaximumWorkers || CurrentWorkers > _durability; }
        }

        private void Awake()
        {
            //_workers = new HashSet<Labourer>();
            //_resourcesGuiManager = GetComponent<ResourcesGuiManager>();
        }

        private void Update()
        {
            if (Durability > 0) return;

            // if we have no more durability, remove ourself from the spawns list in the resource manager and destroy this gameobject
            // for optimization, instead of destroy we can add this object to the respawn pool. try it.
            //ResourcesSpawner.Instance.RemoveWithKey(Parser.StringToEnum<Spawns>(name.Replace("(Clone)", "")), gameObject);
            Destroy(gameObject);
        }

        /// <summary>
        /// Add the recevied worker to the list of workers working this work component. Returns false if the work is already working this component or if the component cannot be worked.
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public bool StartWorking(Labourer worker)
        {
            if (IsLimited && CanBeWorked && _workers.Add(worker))
            {
                _currentWorkers = _workers.Count;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the received worker from the workers list. Returns false if the worker was not working this component.
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public bool StoptWorking(Labourer worker)
        {
            if (IsLimited && _workers.Remove(worker))
            {
                _currentWorkers = _workers.Count;
                return true;
            }

            return false;
        }

        public void RemoveAllWorkers()
        {
            _workers.Clear();
            _currentWorkers = 0;
        }
    }
}
