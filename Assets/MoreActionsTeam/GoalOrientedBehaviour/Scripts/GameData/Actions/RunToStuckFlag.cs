using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using System.Collections.Generic;
using System.Linq;
using ThunderRunners.Pathfinding.Scripts.Pathfinding;
using UnityEngine;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    /// <summary>
	/// Goes to the middle
	/// </summary>
	public class RunToStuckFlag : GoapAction
    {
        /// <summary>
        /// The object used for the effect
        /// </summary>
        private bool _hasReachedStuckFlag;

        // Flag
        private FlagComponent _flag;

        // Where we want to go
        public GameObject _stuckFlagPosition;

        // Where our base is
        private GameObject _myTeamBase;

        // Our runner
        private Runner _runner;

        // Used in CheckProceduralPrecondition()
        private bool _canRunToStuckFlag;

        // Grid
        public Grid grid;

        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", false);
            AddEffect("hasFlag", true);
            ActionName = General_Scripts.Enums.Actions.PickupFlag;

            // cache the flag, the runner, this action's target and our base
            _flag = FindObjectOfType<FlagComponent>();
            _runner = GetComponent<Runner>();

            // Create a new game object in the middle position
            _stuckFlagPosition = new GameObject();
            _stuckFlagPosition.transform.position = new Vector3(0, 0, 0);

            // Set it as our target
            Target = _stuckFlagPosition;

            // Get our base
            var bases = GameObject.FindGameObjectsWithTag("TeamBase");
            _myTeamBase = bases.First(b => b.name.Contains(_runner.MyTeam.ToString()));
        }

        /// <summary>
        /// Resets the action to its default values, so it can be used again.
        /// </summary>
        public override void Reset()
        {
            _hasReachedStuckFlag = false;
            StartTime = 0;
        }

        /// <summary>
        /// Check if the action has been completed
        /// </summary>
        /// <returns></returns>
        public override bool IsDone()
        {
            return _hasReachedStuckFlag;
        }

        /// <summary>
        /// Checks if the agent need to be in range of the target to complete this action.
        /// </summary>
        /// <returns></returns>
        public override bool RequiresInRange()
        {
            return true;
        }

        /// <summary>
        /// Checks if there is a <see cref="ChoppingBlockComponent"/> close to the agent.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            // If the flag is stuck then we can run to the flag
            if (!TeamManager.isFlagPosWalkable())
            {
                _canRunToStuckFlag = true;
                _stuckFlagPosition.transform.position = getStuckFlagPosition();
            }
            else
                _canRunToStuckFlag = false;

            if (_canRunToStuckFlag)
                Target = _stuckFlagPosition;

            return _canRunToStuckFlag;
        }

        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public override bool Perform(GameObject agent)
        {
            if (StartTime == 0)
            {
                AnimManager.Work();
                StartTime = Time.time;
            }

            // still working
            if (StillWorking())
                return true;

            if (Target == null)
                return false;

            if (_canRunToStuckFlag == false) return false;

            
            _hasReachedStuckFlag = true;
            _runner.GetComponent<RunToStuckFlag>().Cost = float.PositiveInfinity;

            // Add this runner to the list of runners that have completed this action since the flag got stuck
            TeamManager.runnersThatRanToStuckFlag.Add(_runner);

            AnimManager.GoIdle();

            return true;
        }

        // Returns the closest walkable position to the flag
        private Vector3 getStuckFlagPosition()
        {
            // Get the flag's neighbour nodes
            Vector3 flagPos = _flag.transform.position;
            List<Pathfinding.Scripts.Pathfinding.Node> nodes = grid.GetNeighbours(grid.NodeFromWorldPoint(new Vector3(flagPos.x, 0.3f, flagPos.z)), TeamManager.stuckFlagPosCheckDepth).ToList();

            // Get the closest walkable node's position
            float minDistanceToFlag = float.PositiveInfinity;
            Vector3 stuckFlagPosition = flagPos;

            for (var i = 0; i < nodes.Count; i++)
            {
                float distance = Vector3.Distance(nodes[i].WorldPosition, flagPos);
                if (distance < minDistanceToFlag && nodes[i].Walkable)
                {
                    minDistanceToFlag = distance;
                    stuckFlagPosition = nodes[i].WorldPosition;
                }
            }

            return stuckFlagPosition;
        }
    }
}