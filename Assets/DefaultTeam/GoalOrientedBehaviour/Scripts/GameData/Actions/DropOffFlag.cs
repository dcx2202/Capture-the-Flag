using System.Linq;
using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using UnityEngine;

namespace DefaultTeam.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    /// <summary>
    /// Drop off the flag at team base
    /// </summary>
    public class DropOffFlag : GoapAction
    {

        /// <summary>
        /// The object used for the effect
        /// </summary>
        private bool _droppedFlag;

        /// <summary>
        /// Target of this action
        /// </summary>
        private Transform _myTeamBase;

        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", true); // we must have the flag to drop it at the base
            AddEffect("dropFlag", true); // we will have dropped the flag once we finish
            AddEffect("hasFlag", false); // we will no longer have the flag after we drop it
            
            // cache my team base location
            var runner = GetComponent<Runner>();

            var bases = GameObject.FindGameObjectsWithTag("TeamBase");

            _myTeamBase = bases.First(b => b.name.Contains(runner.MyTeam.ToString())).transform;
            Target = _myTeamBase.gameObject;
            ActionName = General_Scripts.Enums.Actions.DropFlag;
        }

        public override void Reset()
        {
            //print("Reset action");
            _droppedFlag = false;
            StartTime = 0;
        }

        public override bool IsDone()
        {
            return _droppedFlag;
        }

        public override bool RequiresInRange()
        {
            return true; // you must be in range to drop the flag
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            Target = _myTeamBase.gameObject;
            return true; // we can always drop the flag
        }

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

            var backpack = agent.GetComponent<BackpackComponent>();

            if (backpack.Flag == null)
                return false; // some1 tackled me before I was able to drop the flag

            backpack.Flag.Drop();
            backpack.Flag = null;
            backpack.HasFlag = false;
            _droppedFlag = true; // you have dropped the flag
            AnimManager.GoIdle();


            return true;
        }

    }
}