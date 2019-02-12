using System.Collections;
using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using UnityEngine;

namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    public class TacklePlayer : GoapAction
    {
        /// <summary>
        /// For the runner, this will have the same effect as scoring a point
        /// </summary>
        private bool _dropFlag;

        private Runner _runner;
        private FlagComponent _flag;

        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", false);
            AddEffect("dropFlag", true);

            _runner = GetComponent<Runner>();
            _flag = FindObjectOfType<FlagComponent>();
        }

        public override void Reset()
        {
            _dropFlag = false;
            StartTime = 0;
        }

        public override bool IsDone()
        {
            return _dropFlag;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_flag.Carrier == null)
                return false;

            Target = _flag.Carrier.gameObject;
            return _flag.Carrier.MyTeam != _runner.MyTeam && _onCooldown == false;
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

            if (Target == null || _onCooldown || _flag.Carrier == null)
                return false;

            //print("player tackled");

            _flag.Carrier.Backpack.Flag = null;
            _flag.Carrier.Backpack.HasFlag = false;

            _flag.ThrowFlag(new Vector3(Random.Range(0, 1f), 1, Random.Range(0,1f)));
            _dropFlag = true; // you have dropped the flag
            AnimManager.GoIdle();

            StartCoroutine(StartCooldown());

            return true;
        }

        private bool _onCooldown;

        private IEnumerator StartCooldown()
        {
            _onCooldown = true;
            yield return new WaitForSeconds(2f);
            _onCooldown = false;
        }

        public override bool RequiresInRange()
        {
            return true;
        }
    }
}