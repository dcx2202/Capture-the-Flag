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


namespace MoreActionsTeam.GoalOrientedBehaviour.Scripts.GameData.Actions
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
            Vector3 throwPosition = getTeamMatePos(TeamManager.throwFlagCheckSurroundingsRadius);

            // If we are throwing it towards a team mate or our team base
            if (throwPosition != _runner.transform.position)
                backpack.Flag.ThrowFlag((throwPosition - agent.transform.position).normalized + agent.transform.up); // Throw the flag upwards at the angle we calculated

            // If we are simply throwing forwards
            else
                backpack.Flag.ThrowFlag(agent.transform.forward + agent.transform.up); // Throw the flag forwards and upwards

            backpack.Flag = null;
            backpack.HasFlag = false;
            _dropFlag = true; // you have dropped the flag
            AnimManager.GoIdle();

            return true;
        }

        // Returns the position of teammate we should throw the flag to
        private Vector3 getTeamMatePos(int checkRadius)
        {
            List<Runner> teamMembers = new List<Runner>();
            List<Runner> enemies = new List<Runner>();
            RaycastHit hit = new RaycastHit();

            // Get team members and enemies that are within the check radius
            foreach (var runner in _runners)
            {
                var distance = Vector3.Distance(runner.transform.position, _runner.transform.position);
                if (runner != _runner && distance <= TeamManager.throwFlagCheckSurroundingsRadius)
                {
                    if (runner.MyTeam == _runner.MyTeam)
                        teamMembers.Add(runner);
                    else
                        enemies.Add(runner);
                }
            }

            // Remove the team mates that are in the same direction as an enemy, that are too close to us or aren't in the angle range defined in TeamManager.cs
            for (var i = teamMembers.Count - 1; i >= 0 ; i--)
            {
                // Get the angle between us and the teammember we are evaluating
                var angleToTeamMember = AngleBetweenTwoPositions(teamMembers.ElementAt(i).transform.position, _runner.transform.position);

                // If the team member is not in front of us in the angle range defined in TeamManager.cs
                if (angleToTeamMember > TeamManager.throwFlagCheckAngle || angleToTeamMember < -TeamManager.throwFlagCheckAngle)
                {
                    // Then he is not a candidate anymore
                    teamMembers.Remove(teamMembers.ElementAt(i));
                    continue;
                }

                // If the team member is too close to us then don't consider him
                if ((teamMembers.ElementAt(i).transform.position - _runner.transform.position).magnitude < TeamManager.throwFlagCloseRangeDistance)
                {
                    teamMembers.Remove(teamMembers.ElementAt(i));
                    continue;
                }

                // Create a new raycast
                hit = new RaycastHit();

                // Get the raycast direction we will be using
                // It's the direction between this teammember we are evaluating and ourselves
                Vector3 raycastDirection = teamMembers.ElementAt(i).transform.position - _runner.transform.position;

                // Test for a hit
                if (Physics.Raycast(_runner.transform.position + new Vector3(0, 1, 0), raycastDirection.normalized, out hit, raycastDirection.magnitude))
                {
                    // If it didn't hit our teammate then it hit an obstacle and we don't want to throw it there
                    if (hit.transform.position != teamMembers.ElementAt(i).transform.position)
                    {
                        teamMembers.Remove(teamMembers.ElementAt(i));
                        continue;
                    }
                }

                // For each enemy check if this teammember is within a number of degrees defined in TeamManager.cs
                foreach (var enemy in enemies)
                {
                    // Get the angle between this enemy and the teammember we are evaluating
                    var angleToEnemy = AngleBetweenTwoPositions(enemy.transform.position, _runner.transform.position);

                    // If the angle between this team member and this enemy is lower than what's defined in TeamManager.cs then remove that team member
                    if((angleToTeamMember > 0f && angleToEnemy > 0f) || (angleToTeamMember < 0f && angleToEnemy < 0f))
                    {
                        if(Mathf.Abs((float) angleToTeamMember - angleToEnemy) < TeamManager.throwFlagAngleToEnemyCheck)
                        {
                            teamMembers.Remove(teamMembers.ElementAt(i));
                            break;
                        }
                    }
                    else
                    {
                        if (Mathf.Abs((float) angleToTeamMember) + Mathf.Abs((float) angleToEnemy) < TeamManager.throwFlagAngleToEnemyCheck)
                        {
                            teamMembers.Remove(teamMembers.ElementAt(i));
                            break;
                        }
                    }
                }
            }
            
            // Get the closest teammate to our base that we can throw the flag to
            float minDistanceToBase = float.PositiveInfinity;
            Runner closestTeammate = null;

            foreach (Runner teammate in teamMembers)
            {
                float distance = Vector3.Distance(_myTeamBase.transform.position, teammate.transform.position);

                if (distance < minDistanceToBase)
                {
                    minDistanceToBase = distance;
                    closestTeammate = teammate;
                }
            }

            // Reset the hit
            hit = new RaycastHit();

            // Any team member available to throw the flag to?
            if (closestTeammate != null)
                return closestTeammate.transform.position; // Return the team member to throw the flag to's position
            
            // If we are in the enemy's base quadrant then don't throw towards our base
            else if(TeamManager.isRunnerInEnemyQuadrant(_runner))
                return _runner.transform.position;

            // If we are running slowly and clear from obstacles (fences and mountains) then let's throw the flag in our base's direction
            else if (!Physics.Raycast(_runner.transform.position + new Vector3(0, 1, 0), (_myTeamBase.transform.position - _runner.transform.position).normalized, out hit, 5f))
                return _myTeamBase.transform.position;
            // Else if we did hit something
            else
            {
                // If we hit something but it isn't a prop then throw towards our base anyway
                if (hit.transform.tag != "Props")
                    return _myTeamBase.transform.position;
                // Else let's throw the flag in the direction we are facing
                return _runner.transform.position;
            }
        }

        // Returns the angle between two positions using the y axis as the rotation axis
        public static float AngleBetweenTwoPositions(Vector3 v1, Vector3 v2)
        {
            return Mathf.Atan2(v1.z - v2.z, v1.x - v2.x) * Mathf.Rad2Deg;
        }

        public override bool RequiresInRange()
        {
            return false; // as long as we have the flag, we can throw it
        }
    }
}
 