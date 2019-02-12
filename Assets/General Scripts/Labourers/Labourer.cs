using System.Collections.Generic;
using General_Scripts.AI;
using General_Scripts.AI.GOAP;
using GUIManager;
using UnityEngine;

namespace General_Scripts.Labourers
{
    /// <summary>
    /// A general labourer class.
    /// You should subclass this for specific Labourer classes and implement
    /// the CreateGoalState() method that will populate the goal for the GOAP
    /// planner.
    /// </summary>
    //[RequireComponent(typeof(BackpackComponent))]
    //[RequireComponent(typeof(Unit))]
    public abstract class Labourer : MonoBehaviour, IGoap
    {
        /// <summary>
        /// Reference to this laboured backpack
        /// </summary>
        public BackpackComponent Backpack;
        //public float MoveSpeed = 1;

        /// <summary>
        /// reference to the pathfinding <see cref="Unit"/> of this Labourer. Used to move the Labourer
        /// </summary>
        protected IUnit PathfindingUnit;

        /// <summary>
        /// Reference to the <see cref="WorkComponent"/> where the labour is working.
        /// </summary>
        private WorkComponent _myWorkStation;

        private GoapAgent _agent;

        private AnimationManager _animManager;

        protected virtual void Awake()
        {
            if (Backpack == null)
                Backpack = GetComponent<BackpackComponent>();
            
            PathfindingUnit = GetComponent<IUnit>();
            _agent = GetComponent<GoapAgent>();
            _animManager = GetComponent<AnimationManager>();
        }

        
        /// <summary>
        /// Key-Value data that will feed the GOAP actions and system while planning.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<KeyValuePair<string, object>> GetWorldState()
        {
            var worldData = new HashSet<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("hasFlag", Backpack.HasFlag)
                //new KeyValuePair<string, object>("hasFirewood", Backpack.NumFirewood > 0),

                //new KeyValuePair<string, object>("hasLogs", Backpack.NumLogs > 0),
                //new KeyValuePair<string, object>("hasTool", Backpack.Tool != null && Backpack.Tool.Destroyed() == false)
            };

            return worldData;
        }

        /// <summary>
        /// Implement in subclasses. 
        /// </summary>
        /// <returns></returns>
        public abstract HashSet<KeyValuePair<string, object>> CreateGoalState();

        /// <summary>
        /// Plan failed. Add cleanup code if necessary
        /// </summary>
        /// <param name="failedGoal"></param>
        public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {
            // Not handling this here since we are making sure our goals will always succeed.
            // But normally you want to make sure the world state has changed before running
            // the same goal again, or else it will just fail.
        }

        /// <summary>
        /// Plan found. Set the workstation
        /// </summary>
        /// <param name="goal"></param>
        /// <param name="actions"></param>
        public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            // Yay we found a plan for our goal
            //Debug.Log("<color=green>Plan found</color> " + GoapAgent.PrettyPrint(actions));
            //_pathfindingUnit.RequestPath(actions.Peek().Target.transform);
            //if (_myWorkStation != null)
            //    _myWorkStation.StartWorking(this);

        }

        /// <summary>
        /// Everything is done, we completed our actions for this gool. Hooray! Add code if necessary
        /// </summary>
        public void ActionsFinished()
        {
            // 
            //Debug.Log("<color=blue>Actions completed</color>");
        }
         /// <summary>
         /// Current action is finished. We are no longer using the workstation.
         /// </summary>
         /// <param name="currentAction"></param>
        public void CurrentActionFinished(GoapAction currentAction)
        {
            if (_myWorkStation == null) return;

            _myWorkStation.StoptWorking(this);
            _myWorkStation = null; // reset work station
        }

        /// <summary>
        /// Clean up for the current action and setup for the next action.
        /// </summary>
        /// <param name="currentAction"></param>
        /// <param name="nextAction"></param>
        public void CurrentActionFinished(GoapAction currentAction, GoapAction nextAction)
        {
            CurrentActionFinished(currentAction);
            // if we have more actions, verify that it is still possible to perform the plan. Abort if there is no longer an available target.
            if (nextAction != null && nextAction.CheckProceduralPrecondition(gameObject) == false)
                PlanAborted(nextAction);
        }

        /// <summary>
        /// An action bailed out of the plan. State has been reset to plan again.
        /// Take note of what happened and make sure if you run the same goal again
        /// that it can succeed.
        /// </summary>
        /// <param name="aborter"></param>
        public void PlanAborted(GoapAction aborter)
        {
            //Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.PrettyPrint(aborter));
            _agent.AbortPlan();
            if (_myWorkStation == null) return;

            _myWorkStation.StoptWorking(this);
            _myWorkStation = null; // reset work station
        }

        /// <summary>
        /// Moves the agent to the target of the next action. This implementation uses AStart pathfinding and calls the <see cref="Unit.DoFollowPathStep"/> method.
        /// </summary>
        /// <param name="nextAction"></param>
        /// <returns>Returns true if we are in range of the target</returns>
        public bool MoveAgent(GoapAction nextAction)
        {
            _animManager.Move();
            if (nextAction.Target == null)
                return false; // we are not there yet
            
            PathfindingUnit.SetTarget(nextAction.Target.transform);
            if (PathfindingUnit.DoFollowPathStep() == false) // if we are not following the path anymore
            {
                // we are at the target location, we are done
                nextAction.InRange = true;

                _animManager.GoIdle();

                return true; // we have arrived
            }

            return false; // we are not there yet
        }
        
        public void UpdateWorkStation(WorkComponent station)
        {
            if (station != null)
            {
                _myWorkStation = station;
            }
        }
    }
}

