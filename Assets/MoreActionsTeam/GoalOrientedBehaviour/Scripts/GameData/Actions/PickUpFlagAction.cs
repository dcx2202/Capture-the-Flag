using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using UnityEngine;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    /// <summary>
    /// Picks up unattended flag
    /// </summary>
    public class PickUpFlagAction : GoapAction
    {
        /// <summary>
        /// The object used for the effect
        /// </summary>
        private bool _hasFlag;

        /// <summary>
        /// The target of this action
        /// </summary>
        private FlagComponent _flag;

        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", false); // we cannot have the flag to pick up the flag
            //AddPrecondition("dropFlag", false);
            AddEffect("hasFlag", true); // we will have the flag after we picked it up
            ActionName = General_Scripts.Enums.Actions.PickupFlag;

            // cache the flag
            _flag = FindObjectOfType<FlagComponent>();
            Target = _flag.gameObject;
        }

        /// <summary>
        /// Resets the action to its default values, so it can be used again.
        /// </summary>
        public override void Reset ()
        {
            //print("Reset action");

            _hasFlag = false;
            StartTime = 0;
        }

        /// <summary>
        /// Check if the action has been completed
        /// </summary>
        /// <returns></returns>
        public override bool IsDone ()
        {
            return _hasFlag;
        }

        /// <summary>
        /// Checks if the agent need to be in range of the target to complete this action.
        /// </summary>
        /// <returns></returns>
        public override bool RequiresInRange ()
        {
            return true; // yes we need to be near the flag to pick it up  
        }

        /// <summary>
        /// Checks if there is a <see cref="ChoppingBlockComponent"/> close to the agent.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public override bool CheckProceduralPrecondition (GameObject agent)
        {
            // the flag will "be worked" if there is some1 carrying it. Otherwise it is free to be picked up.
            if (_flag.CanBeWorked)
                Target = _flag.gameObject;

            return _flag.CanBeWorked;
        }
	
        /// <summary>
        /// Once the WorkDuration is compelted, adds 5 FireWood to the agent's backpack.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public override bool Perform (GameObject agent)
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

            if (_flag.CanBeWorked == false) return false;
            _flag.StartWorking(GetComponent<Labourer>());

            var backpack = agent.GetComponent<BackpackComponent>();

            _hasFlag = true;
            backpack.Flag = _flag; // picked up the flag
            backpack.HasFlag = true;
            _flag.PickUp(agent.GetComponent<Runner>());
            AnimManager.GoIdle();

            return true;
        }
    }
}

