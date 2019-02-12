using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using SteeringBehaviours.Scripts.Basics;
using System.Linq;
using UnityEngine;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
	/// <summary>
	/// Picks up unattended flag
	/// </summary>
	public class RunToDefense : GoapAction
	{
		/// <summary>
		/// The object used for the effect
		/// </summary>
		private bool _hasReachedDefense;

        // Flag
		private FlagComponent _flag;

        // Where we want to go
		public GameObject _defensePosition;

        // Our team base
		private GameObject _myTeamBase;

        // Our runner
		private Runner _runner;

        // Used in CheckProceduralPrecondition()
        private bool _canRunToDefense;

		protected override void Awake()
		{
			base.Awake();
			AddPrecondition("hasFlag", false); 
			AddEffect("hasFlag", true);
			ActionName = General_Scripts.Enums.Actions.PickupFlag;

			// cache the flag, the runner, this action's target and our base
			_flag = FindObjectOfType<FlagComponent>();
			_runner = GetComponent<Runner>();

            // Create a new game object in the defense position
			_defensePosition = new GameObject();
			_defensePosition.transform.position = TeamManager.defendingPosition;

            // Set it as our target
			Target = _defensePosition;

            // Get our base
			var bases = GameObject.FindGameObjectsWithTag("TeamBase");
			_myTeamBase = bases.First(b => b.name.Contains(_runner.MyTeam.ToString()));
		}

		/// <summary>
		/// Resets the action to its default values, so it can be used again.
		/// </summary>
		public override void Reset()
		{
			//print("Reset action");

			_hasReachedDefense = false;
			StartTime = 0;
		}

		/// <summary>
		/// Check if the action has been completed
		/// </summary>
		/// <returns></returns>
		public override bool IsDone()
		{
			return _hasReachedDefense;
		}

		/// <summary>
		/// Checks if the agent need to be in range of the target to complete this action.
		/// </summary>
		/// <returns></returns>
		public override bool RequiresInRange()
		{
			return true;
		}

		/// </summary>
		/// <param name="agent"></param>
		/// <returns></returns>
		public override bool CheckProceduralPrecondition(GameObject agent)
		{
            // If the flag is in the enemy's quadrant and we aren't going middle and we aren't the closest team member to the flag then we can run to defense
            if (TeamManager.isFlagInEnemyQuadrant() && _runner.GetComponent<RunToMiddle>().Cost == float.PositiveInfinity && !TeamManager.isClosestTeamMemberToFlag(_runner))
                _canRunToDefense = true;
            else
                _canRunToDefense = false;

            // If we can run to defense then update our target
			if (_canRunToDefense)
				Target = _defensePosition;

			return _canRunToDefense;
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

			if (_canRunToDefense == false) return false;

			_hasReachedDefense = true;
            
            // If the runner has reached the defending position then face towards our base
            _runner.GetComponent<SteeringBasics>().Face((_myTeamBase.transform.position - _runner.transform.position).normalized);

            AnimManager.GoIdle();

			return true;
		}
	}
}