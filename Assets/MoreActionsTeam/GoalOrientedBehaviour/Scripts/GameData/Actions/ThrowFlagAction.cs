using General_Scripts;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using SteeringBehaviours.Scripts.Basics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using System.Linq;
using UnityEngine.Assertions.Must;
using System.Collections;


namespace ThunderRunners.GoalOrientedBehaviour.Scripts.GameData.Actions
{
    public class ThrowFlagAction : GoapAction
    {
        /// <summary>
        /// For the runner, this will have the same effect as scoring a point
        /// </summary>
        private bool _dropFlag;

        /// <summary>
        /// The runner that will perform this action
        /// </summary>
        private Runner _runner;

        /// <summary>
        /// Cache for the steering basics of the runner
        /// </summary>
        private SteeringBasics _steering;

        // Our base
        private GameObject _myTeamBase;

        // Our runners
        private Runner[] _runners;


        protected override void Awake()
        {
            base.Awake();
            AddPrecondition("hasFlag", true);
            AddEffect("hasFlag", false);
            AddEffect("dropFlag", true);

            _runner = GetComponent<Runner>();
            _runners = FindObjectsOfType<Runner>();
            _steering = GetComponent<SteeringBasics>();

            // Get our base
            var bases = GameObject.FindGameObjectsWithTag("TeamBase");
            _myTeamBase = bases.First(b => b.name.Contains(_runner.MyTeam.ToString()));
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
            return _steering.MaxVelocity < TeamManager.minimumVelocityBeforeThrowingFlag; // we decided we want to throw the flag when our velocity is lower than 4.5
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

            var backpack = agent.GetComponent<BackpackComponent>();

            // Get the position to throw the flag to
            Vector3 throwPosition = getTeamMatePos();

            if(throwPosition == new Vector3(0, 0, 0))
                return false;

            // If we are throwing it towards our team base
            if (throwPosition != _runner.transform.position)
                backpack.Flag.ThrowFlag((throwPosition - agent.transform.position).normalized + agent.transform.up); // Throw the flag upwards at the angle we calculated

            // If we are simply throwing forwards
            else
                backpack.Flag.ThrowFlag(agent.transform.forward + agent.transform.up); // Throw the flag forwards and upwards

            backpack.Flag = null;
            backpack.HasFlag = false;
            _dropFlag = true; // you have dropped the flag
            AnimManager.GoIdle();

            // Reset the variable
            TeamManager.minimumVelocityBeforeThrowingFlag = TeamManager.minimumVelocityBeforeThrowingFlag_backup;
            return true;
        }

        // Returns where we should throw the flag to
        private Vector3 getTeamMatePos()
        {
            List<Runner> teamMembers = new List<Runner>();
            List<Runner> enemies = new List<Runner>();
            RaycastHit hit = new RaycastHit();

            // If we are in the enemy's base quadrant then don't throw towards our base
            if (TeamManager.isRunnerInEnemyQuadrant(_runner))
            {
                if (Physics.Raycast(_runner.transform.position + new Vector3(0, 1, 0), _runner.transform.forward, out hit, TeamManager.throwFlagCheckSurroundingsRadius))
                {
                    if (hit.transform.tag != "Props")
                        return _runner.transform.position;

                    if (TeamManager.minimumVelocityBeforeThrowingFlag > 3f)
                    {
                        TeamManager.minimumVelocityBeforeThrowingFlag -= 0.5f;
                        return new Vector3(0, 0, 0);
                    }
                }
                return _runner.transform.position;
            }

            // If we are running slowly and clear from obstacles (fences and mountains) then let's throw the flag in our base's direction
            else if (!Physics.Raycast(_runner.transform.position + new Vector3(0, 1, 0), (_myTeamBase.transform.position - _runner.transform.position).normalized, out hit, TeamManager.throwFlagCheckSurroundingsRadius))
                return _myTeamBase.transform.position;
            // Else if we did hit something
            else
            {
                // If we hit something but it isn't a prop then throw towards our base anyway
                if (hit.transform.tag != "Props")
                    return _myTeamBase.transform.position;

                // Else let's try throwing the flag in the direction we are facing
                hit = new RaycastHit();

                // If we hit something
                if (Physics.Raycast(_runner.transform.position + new Vector3(0, 1, 0), _runner.transform.forward, out hit, TeamManager.throwFlagCheckSurroundingsRadius))
                {
                    // And it isn't a prop then we can throw the flag forwards
                    if (hit.transform.tag != "Props")
                        return _runner.transform.position;

                    // Else if our velocity is greater than 1
                    if (TeamManager.minimumVelocityBeforeThrowingFlag > 3f)
                    {
                        // Allow it to decrease a little past our set minimum velocity before throwing flag so that we align correctly with our path (happens automatically when following the path)
                        TeamManager.minimumVelocityBeforeThrowingFlag -= 0.5f;

                        // Return a distinct vector so we can identify this situation
                        return new Vector3(0, 0, 0);
                    }
                }

                // Else we can throw the flag in the direction we are facing
                return _runner.transform.position;
            }
        }

        public override bool RequiresInRange()
        {
            return false; // as long as we have the flag, we can throw it
        }
    }
}
