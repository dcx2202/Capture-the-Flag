using System;
using General_Scripts.AI;
using General_Scripts.Labourers;
using ThunderRunners.Pathfinding.Scripts.Observer;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    /// <summary>
    /// Represents a unit in this pathfinding system. Listens to modifications done to the grid
    /// </summary>
    public class Unit : MonoBehaviour, IListener, IUnit
    {
        /// <summary>
        /// The minimum distance the target has to move before we recalculate a new path to it
        /// </summary>
        [Tooltip("minimum distance the target has to move before we recalculate a new path to it")]
        private const float PathUpdateMoveThreashold = .5f;
        /// <summary>
        /// The minimum time we wait before recalculating the path.
        /// </summary>
        [Tooltip("The minimum time we wait before recalculating the path.")]
        private const float MinPathUpdateTime = .2f;
        
        [Tooltip("Our movement speed")]
        public float MovementSpeed = 20;
        [Tooltip("The distance from the nodes where we start turning. The bigger the distance, the more time we will have to turn.")]
        public float TurnDist = 5;
        [Tooltip("Our turning speed")]
        public float TurnSpeed = 3;
        [Tooltip("Distance from the final node where we start to slowdown")]
        public float StoppingDist = 10;

        /// <summary>
        /// The current path we are following
        /// </summary>
        private Path _path;
        /// <summary>
        /// The previous target position
        /// </summary>
        private Vector3 _previousTargetPosition = Vector3.zero;
        /// <summary>
        /// How much the target must move before we request an update
        /// </summary>
        private float _sqrMoveThreashold;
        
        /// <summary>
        /// My current path request
        /// </summary>
        private PathRequest _myPathRequest;
        /// <summary>
        /// current path index
        /// </summary>
        private int _pathIndex;

        /// <summary>
        /// Reference to the Arrive Steering Behaviour
        /// </summary>
        private Arrive _arrive;
        /// <summary>
        /// Reference tothe basic steering behaviours
        /// </summary>
        private SteeringBasics _steering;

        [Tooltip("The target we are following")]
        [SerializeField]
        private Transform _target;

        /// <summary>
        /// Cache of this unit transform for optimization
        /// </summary>
        private Transform _myTransform;
        //public bool MustRefreshPath { get; set; }

        private void Awake()
        {
            _myTransform = transform;
            _sqrMoveThreashold = PathUpdateMoveThreashold * PathUpdateMoveThreashold;
            _arrive = GetComponent<Arrive>();
            _steering = GetComponent<SteeringBasics>();
        }

        private void Start()
        {
            // register with the PathRequestManager. This will make each unit aware of modifications to the grid
            PathRequestManager.Instance.RegisterListener(this);
        }

        private void OnDestroy()
        {
            PathRequestManager.Instance.RemoveListener(this);
        }
        
        /// <summary>
        /// Callback for the path found. If it was successfull, update that path. If not and we have a target, request a new one.
        /// </summary>
        /// <param name="waypoints"></param>
        /// <param name="pathSuccessful"></param>
        private void OnPathFound(Node[] waypoints, bool pathSuccessful)
        {
            if (pathSuccessful == false)
            {
                if(GetTarget() != null)
                    RequestPath(GetTarget().transform, true);
                StopMoving();
                return;
            }
            _pathIndex = 0;
            _path = new Path(waypoints, _myTransform.position, TurnDist, StoppingDist);
            //TeamManager.updateRunnerPaths(_myTransform.position, waypoints);
        }

        /// <summary>
        /// Do validations to see if we need to request a new path.
        /// </summary>
        /// <param name="target"> The target</param>
        /// <param name="forceRequest"> Forces the request of a new path</param>
        public void RequestPath(Transform target, bool forceRequest = false)
        {
            // if the target has moved more than PathUpdateMoveThreashold, then request a new Path
            if (target != null && ((target.position - _previousTargetPosition).sqrMagnitude > _sqrMoveThreashold || forceRequest || _path != null && _path.FinishLineIndex == -1))
            {
                //print("requesting path ");
                if (_myPathRequest == null)
                    _myPathRequest = new PathRequest(_myTransform.position, target.position, OnPathFound);
                else
                    _myPathRequest.UpdatePathRequest(_myTransform.position, target.position, OnPathFound);

                PathRequestManager.Instance.RequestPath(_myPathRequest);
                // update the previous position
                _previousTargetPosition = target.position;
            }
        }
       
        /// <summary>
        /// returns true while still following a path. Returns false when it arrives
        /// </summary>
        /// <returns></returns>
        public bool DoFollowPathStep()
        {
            RequestPath(GetTarget().transform);
            var followingPath = true;
            var pos2D = Path.Vector3ToVector2(_myTransform.position);

            if (_path == null) // we are still waiting for the path to be calculated
            {
                StopMoving();
                return true; // we are tecnically still following the path (we have not arrived)
            }

            if (_pathIndex > _path.TurnBoundaries.Length || _path.FinishLineIndex == -1)
            {
                StopMoving();
                _pathIndex = 0;
                return true;
            }

            // loops through the turn boundaries till it founds the last boundary it has crossed. 
            // This solves the pathing problem if we move through several turn boundaries for each turn.
            while (_path.TurnBoundaries[_pathIndex].HasCrossedLine(pos2D))
            {
                // we arrived at the end of the path
                if (_pathIndex >= _path.FinishLineIndex)
                {
                    break;
                }

                _pathIndex++;
            }

            if (Vector3.Distance(transform.position, GetTarget().position) < 1f)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                followingPath = false;
            }

            // rotate and move the unit to the next point
            if (followingPath)
            {
                var acc = _arrive.GetSteering(_path.LookPoints[_pathIndex]);
                if (Math.Abs(acc.x) < 0.01f && Math.Abs(acc.z) < 0.01f) // something went wrong with the pathfinding and we are stuck. Do a new plan.
                {
                    if(GetTarget() != null)
                        RequestPath(GetTarget().transform, true);
                }
                _steering.Steer(acc);
                _steering.LookWhereYoureGoing();
            }
            return followingPath;
        }

        /// <summary>
        /// Vizualize with gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            //if (_path != null)
            //{
            //    _path.DrawWithGizmos();
            //}
        }

        /// <summary>
        /// Notify the listners if the grid has changed
        /// </summary>
        public void Notify()
        {
            if(_path != null)
               if(_path.PathValuesHasChanged(_pathIndex, 5) && GetTarget() != null)
                    RequestPath(GetTarget().transform, true);
        }
        
        /// <summary>
        /// Stop moving
        /// </summary>
        public void StopMoving()
        {
            _steering.Stop();
        }

        /// <summary>
        /// Gets the target
        /// </summary>
        /// <returns></returns>
        public Transform GetTarget()
        {
            return _target;
        }

        /// <summary>
        /// Sets the target
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Transform target)
        {
            if (_target != target)
                RequestPath(target.transform, true);
            _target = target;
        }

    }
}
