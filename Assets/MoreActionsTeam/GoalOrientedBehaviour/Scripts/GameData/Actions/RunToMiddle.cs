using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using System.Linq;
using UnityEngine;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    /// <summary>
	/// Goes to the middle
	/// </summary>
	public class RunToMiddle : GoapAction
    {
        /// <summary>
        /// The object used for the effect
        /// </summary>
        private bool _hasReachedMiddle;

        // Flag
        private FlagComponent _flag;

        // Where we want to go
        public GameObject _middlePosition;

        // Where our base is
        private GameObject _myTeamBase;

        // Our runner
        private Runner _runner;

        // Used in CheckProceduralPrecondition()
        private bool _canRunToMid;

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
            _middlePosition = new GameObject();
            _middlePosition.transform.position = TeamManager.middlePosition;

            // Set it as our target
            Target = _middlePosition;

            // Get our base
            var bases = GameObject.FindGameObjectsWithTag("TeamBase");
            _myTeamBase = bases.First(b => b.name.Contains(_runner.MyTeam.ToString()));
        }

        /// <summary>
        /// Resets the action to its default values, so it can be used again.
        /// </summary>
        public override void Reset()
        {
            _hasReachedMiddle = false;
            StartTime = 0;
        }

        /// <summary>
        /// Check if the action has been completed
        /// </summary>
        /// <returns></returns>
        public override bool IsDone()
        {
            return _hasReachedMiddle;
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
            // If we will score a point or if the flag is in the enemy's quadrant and we aren't the closest team member then we can run to middle
            if (TeamManager.isTeamPointGuaranteed(_runner) || (TeamManager.isFlagInEnemyQuadrant() && !TeamManager.isClosestTeamMemberToFlag(_runner)) || !TeamManager.isFlagPosWalkable())
                _canRunToMid = true;
            else
                _canRunToMid = false;

            if (_canRunToMid)
                Target = _middlePosition;

            return _canRunToMid;
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

            if (_canRunToMid == false) return false;

            _hasReachedMiddle = true;

            AnimManager.GoIdle();

            return true;
        }
    }
}