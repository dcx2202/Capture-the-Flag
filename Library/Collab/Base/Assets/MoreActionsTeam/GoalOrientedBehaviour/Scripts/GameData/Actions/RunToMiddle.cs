using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using System.Linq;
using UnityEngine;

namespace MoreActionsTeam.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    /// <summary>
	/// Picks up unattended flag
	/// </summary>
	public class RunToMiddle : GoapAction
    {
        /// <summary>
        /// The object used for the effect
        /// </summary>
        private bool _hasReachedMiddle;

        /// <summary>
        /// The target of this action
        /// </summary>
        private FlagComponent _flag;

        public GameObject _middlePosition;

        private GameObject _myTeamBase;

        private Runner _runner;

        private bool _canRunToMid;

        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", false); // we cannot have the flag to pick up the flag
            AddEffect("hasFlag", true);
            ActionName = General_Scripts.Enums.Actions.PickupFlag;

            // cache the flag spawn
            _flag = FindObjectOfType<FlagComponent>();
            _runner = GetComponent<Runner>();
            _middlePosition = new GameObject();
            _middlePosition.transform.position = new Vector3(0, 0, 0);
            Target = _middlePosition;
            //_myTeamBase = GameObject.FindGameObjectsWithTag("TeamBase")[0];

            var bases = GameObject.FindGameObjectsWithTag("TeamBase");
            _myTeamBase = bases.First(b => b.name.Contains(_runner.MyTeam.ToString()));
        }

        /// <summary>
        /// Resets the action to its default values, so it can be used again.
        /// </summary>
        public override void Reset()
        {
            //print("Reset action");

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
            return true; // yes we need to be near the flag to pick it up  
        }

        /// <summary>
        /// Checks if there is a <see cref="ChoppingBlockComponent"/> close to the agent.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (TeamManager.isTeamPointGuaranteed(_runner) || ((_flag.transform.position.x < -1.5f && _flag.transform.position.z < -1.5f) && !TeamManager.isClosestTeamMemberToFlag(_runner)))
                _canRunToMid = true;
            else if ((TeamManager.getTeammatesMiddle().Count > 0 && !TeamManager.amIMiddle(_runner)) || (TeamManager.getTeammatesMiddle().Count >= 2 && TeamManager.amIMiddle(_runner)))
            {
                _canRunToMid = false;
            }
            else
                _canRunToMid = false;

            if (_canRunToMid)
                Target = _middlePosition;

            return _canRunToMid;
        }

        /// <summary>
        /// Once the WorkDuration is compelted, adds 5 FireWood to the agent's backpack.
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
            //TeamManager.isAnyoneGoingMiddle = false;

            AnimManager.GoIdle();

            return true;
        }
    }
}