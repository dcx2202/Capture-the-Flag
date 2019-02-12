using System.Collections;
using System.Collections.Generic;
using General_Scripts.AI.GOAP;
using General_Scripts.Labourers;
using MoreActionsTeam.GoalOrientedBehaviour.Scripts.GameData.Actions;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;
using System.Linq;
using General_Scripts;
using MoreActionsTeam.Pathfinding.Scripts.Pathfinding;
using Node = MoreActionsTeam.Pathfinding.Scripts.Pathfinding.Node;

namespace MoreActionsTeam
{
    public class TeamManager : MonoBehaviour
    {
        // Tweakable Variables ---------------------------------------------

        [Tooltip("Defense Position")]
        public static Vector3 defendingPosition = new Vector3(-22, 0, -22);

        [Tooltip("Middle Position")]
        public static Vector3 middlePosition = new Vector3(0, 0, 0);

        [Tooltip("We will check for targets to throw the flag to in the range (throwFlagCheckAngle, -throwFlagCheckAngle) with 0 being directly in front of us.")]
        public static int throwFlagCheckAngle = 60;

        [Tooltip("Radius used to determine how far we will check when looking for targets to throw the flag to.")]
        public static int throwFlagCheckSurroundingsRadius = 10;

        [Tooltip("Minimum distance a teammate should be from us to be eligible to get the flag thrown towards.")]
        public static float throwFlagCloseRangeDistance = 2f;

        [Tooltip("Angle used to check if a teammate is close to an enemy when looking for targets to throw the flag to.")]
        public static float throwFlagAngleToEnemyCheck = 10f;

        [Tooltip("Minimum distance from the base that we can be to throw the flag to the base.")]
        public int noThrowFlagDistanceToBase = 5;

        [Tooltip("Minimum distance we can be from the flag before leaving defense and tackling the flag carrier.")]
        public float defenseTackleRadius = 2f;

        [Tooltip("Minimum velocity we can have before trying to throw the flag.")]
        public static float minimumVelocityBeforeThrowingFlag = 4.5f;

        [Tooltip("Margin of error when guessing whether or not the point is guaranteed.")]
        public const float teamPointGuaranteedErrorMargin = 0.65f;

        //------------------------------------------------------------------
        
        // Team dependent variable - must be initialized after we know which team is ours

        // Vector to store our team's quadrant (corner)
        private static Vector3 teamQuadrant;

        // Vector to store the enemy's quadrant (corner)
        private static Vector3 enemyQuadrant;

        // Other Variables
        public bool WeHaveFlag;

        public List<GoapAgent> MyAgents;

        public List<Runner> MyRunners;

        public static Dictionary<Runner, List<Vector3>> MyRunnersWaypoints = new Dictionary<Runner, List<Vector3>>();

        private static Transform _myTeamBase;

        private static FlagComponent _flag;

        // List of our teammates
        private static List<Runner> teammates;


        public void SetTeamNewGoal(string goal)
        {
            foreach (var runner in MyRunners)
            {
                runner.Goals.Clear();
                runner.Goals.Add(goal);
            }
        }

        private void Awake()
        {
            foreach (Runner runner in MyRunners)
                MyRunnersWaypoints.Add(runner, new List<Vector3>());

            // Get the flag
            _flag = FindObjectOfType<FlagComponent>();

            // Get our team base
            var bases = GameObject.FindGameObjectsWithTag("TeamBase");
            _myTeamBase = bases.First(b => b.name.Contains(MyRunners[0].MyTeam.ToString())).transform;

            // Get a list of our teammates
            teammates = FindObjectsOfType<Runner>().Where(elem => "Team" + elem.MyTeam == _myTeamBase.name).ToList();

            // Get our quadrant
            if (_myTeamBase.name == "TeamA")
            {
                teamQuadrant.x = -1.5f; teamQuadrant.z = -1.5f;
                enemyQuadrant.x = 0.8f; enemyQuadrant.z = 1.6f;
            }
            else if (_myTeamBase.name == "TeamB")
            {
                teamQuadrant.x = 0.8f; teamQuadrant.z = 1.6f;
                enemyQuadrant.x = -1.5f; enemyQuadrant.z = -1.5f;
            }

            StartCoroutine(RequestNewPlan());
            StartCoroutine(AdaptActionsCosts());
        }

        private IEnumerator RequestNewPlan()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                foreach (var agent in MyAgents)
                {
                    if (agent.NeedNewPlan)
                        agent.GetComponent<SteeringBasics>().Stop();

                    agent.AbortPlan();
                }
            }
        }

        private IEnumerator AdaptActionsCosts()
        {
            while (true)
            {
                foreach (var runner in MyRunners)
                {
                    var steering = runner.GetComponent<SteeringBasics>();

                    // Get our teammates
                    List<Runner> teammembers = FindObjectsOfType<Runner>().Where(elem => "Team" + elem.MyTeam == _myTeamBase.name).ToList();

                    // Get our teammates ordered by their distance to the flag (ascending order)
                    List<Runner> teamByDistFlag = getTeammatesByDistanceToTarget(teammembers, _flag.transform.position);

                    // If the distance to our base is less than 5, we dont want to throw the flag anymore (avoid overshooting the base)
                    if (Vector3.Distance(runner.gameObject.transform.position, _myTeamBase.position) < noThrowFlagDistanceToBase)
                    {
                        runner.GetComponent<ThrowFlagAction>().Cost = float.PositiveInfinity;
                        runner.GetComponent<DropOffFlag>().Cost = 0;
                    }
                    else
                    {
                        // If the runner speed is less than the defined minimum velocity, we want to throw the flag
                        if (steering.MaxVelocity < minimumVelocityBeforeThrowingFlag)
                        {
                            runner.GetComponent<DropOffFlag>().Cost = float.PositiveInfinity;
                            runner.GetComponent<ThrowFlagAction>().Cost = 0;
                        }
                        else
                        {
                            runner.GetComponent<DropOffFlag>().Cost = 1 / runner.GetComponent<SteeringBasics>().MaxVelocity;
                            runner.GetComponent<ThrowFlagAction>().Cost = float.PositiveInfinity;
                        }
                    }

                    // If the flag is in the enemy quadrant
                    // runner closest to the flag - attack the flag
                    // From the remaining runners, the closest to the defense position - goes to defense
                    // the furthest from the defense position - goes to middle
                    if (isFlagInEnemyQuadrant())
                    {
                        teamByDistFlag[0].GetComponent<RunToMiddle>().Cost = float.PositiveInfinity;
                        teamByDistFlag[0].GetComponent<RunToDefense>().Cost = float.PositiveInfinity;

                        // Get the remaining runners by their distance to the defense position
                        List<Runner> remaining = new List<Runner>() { teamByDistFlag[1] , teamByDistFlag[2] };
                        List<Runner> runnersByDistDefense = getTeammatesByDistanceToTarget(remaining, defendingPosition);

                        // The closest to the defense position will defend
                        runnersByDistDefense[0].GetComponent<RunToMiddle>().Cost = float.PositiveInfinity;
                        runnersByDistDefense[0].GetComponent<RunToDefense>().Cost = 0;

                        // The other will go to middle
                        runnersByDistDefense[1].GetComponent<RunToMiddle>().Cost = 0;
                        runnersByDistDefense[1].GetComponent<RunToDefense>().Cost = float.PositiveInfinity;

						// If the flag is close to the runner defending, we want this runner to attack/tackle the flag
                        if (runner.GetComponent<RunToDefense>().Cost == 0 && Vector3.Distance(_flag.transform.position, runner.transform.position) < defenseTackleRadius)
                            runner.GetComponent<RunToDefense>().Cost = float.PositiveInfinity;

						// If this runner is going to middle but there is already another runner going there, we want to cancel this action
                        if (runner.GetComponent<RunToMiddle>().Cost == 0 && isAnyTeamMemberGoingMiddle(runner))
                            runner.GetComponent<RunToMiddle>().Cost = float.PositiveInfinity;

                    }

					// If the flag is in spawn position
                    else if (_flag.transform.position.x == 0 && _flag.transform.position.z == 0)
                    {
                        runner.GetComponent<RunToMiddle>().Cost = float.PositiveInfinity;
                        runner.GetComponent<RunToDefense>().Cost = float.PositiveInfinity;
                    }

                    // If the flag isn't in the enemy's quadrant and the point isn't guaranteed then they aren't running middle anyway and we just update the current costs of these actions
                    else if(!isTeamPointGuaranteed(runner))
                    {
                        runner.GetComponent<RunToMiddle>().Cost = float.PositiveInfinity;
                        runner.GetComponent<RunToDefense>().Cost = float.PositiveInfinity;
                    }

                    // If the point is guaranteed (and we aren't the flag carrier) and the flag is in our quadrant then run to middle
                    if(isTeamPointGuaranteed(runner) && isFlagInTeamQuadrant())
                        runner.GetComponent<RunToMiddle>().Cost = 0;

                    yield return null;
                }
            }
        }

        // Returns a list with our team's runners ordered by their distance to a given target
        private List<Runner> getTeammatesByDistanceToTarget(List<Runner> runners,Vector3 target)
        {
            List<Runner> result = new List<Runner>();

            // While there are runners to sort
            while (runners.Count > 0)
            {
                // Find which is closest to target
                float minDistance = float.PositiveInfinity;
                Runner closestRunner = null;
                foreach(Runner runner in runners)
                {
                    float distance = Vector3.Distance(target, runner.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestRunner = runner;
                    }
                }

                // Add to the sorted list and remove from the sorting list
                result.Add(closestRunner);
                runners.Remove(closestRunner);
            }
            return result;
        }

        // Returns true when a given runner is the closest to the flag among their teammates
        public static bool isClosestTeamMemberToFlag(Runner runner)
        {
            // Initialize the distance as positive infinity
            float minDistanceToFlag = float.PositiveInfinity;
            Runner closestTeammate = null;

            // Get the closest teammate
            foreach (Runner teammate in teammates)
            {
                // Calculate the distance from this teammate to the flag
                float distance = Vector3.Distance(_flag.transform.position, teammate.transform.position);

                // If it's the shortest yet
                if (distance < minDistanceToFlag)
                {
                    // Update the minimum distance and the closest teammate yet
                    minDistanceToFlag = distance;
                    closestTeammate = teammate;
                }
            }

            return runner == closestTeammate;
        }

        // Returns true if there is any team member going (or in) middle
        private bool isAnyTeamMemberGoingMiddle(Runner runner)
        {
            // For each teammate check if they are either going or in middle
            foreach (Runner teammate in teammates)
            {
                // If they are return true
                if(teammate.GetComponent<RunToMiddle>().Cost == 0 && teammate != runner)
                    return true;
            }
            return false;
        }

        public static bool isTeamPointGuaranteed(Runner runner)
        {
            // If the flag is being carried by the enemy then return false
            if (_flag.Carrier != null && ((_flag.transform.position.x == 0 && _flag.transform.position.z == 0) || "Team" + _flag.Carrier.MyTeam != _myTeamBase.name))
                return false;

            // Get the closest teammate to the flag
            float minDistanceToFlag = float.PositiveInfinity;
            Runner closestTeammate = null;
            foreach(Runner teammate in teammates)
            {
                float distance = Vector3.Distance(_flag.transform.position, teammate.transform.position);
                if (distance < minDistanceToFlag)
                {
                    minDistanceToFlag = distance;
                    closestTeammate = teammate;
                }
            }

            // If this teammate is our flag carrier then he is not eligible to go middle and return false
            if (runner == closestTeammate)
                return false;

            // Get the enemy teammembers
            List<Runner> enemies = FindObjectsOfType<Runner>().Where(elem => "Team" + elem.MyTeam != _myTeamBase.name).ToList();

            // Get the closest enemy to our base
            float minDistanceToBase = float.PositiveInfinity;
            Runner closestEnemy = null;
            foreach (Runner enemy in enemies)
            {
                float distance = Vector3.Distance(_myTeamBase.transform.position, enemy.transform.position);
                if (distance < minDistanceToBase)
                {
                    minDistanceToBase = distance;
                    closestEnemy = enemy;
                }
            }

            // Calculate our flag carrier teammate's distance to our base (the distance between them and the flag plus the distance between the flag and our base)
            float flagCarrierDistanceToBase = Vector3.Distance(_flag.transform.position, closestTeammate.transform.position) + Vector3.Distance(_myTeamBase.transform.position, _flag.transform.position);

            // Calculate the closest enemy's distance to our base
            float enemyDistanceToBase = Vector3.Distance(_myTeamBase.transform.position, closestEnemy.transform.position);

            // Shorten the closest enemy's distance to our base to give margin for error and return our guess on whether or not the point is guaranteed
            return (enemyDistanceToBase * teamPointGuaranteedErrorMargin) > flagCarrierDistanceToBase;
        }

        // Returns whether or not the flag is in the enemy's quadrant
        public static bool isFlagInEnemyQuadrant()
        {
            if(enemyQuadrant.x == -1.5f)
                return _flag.transform.position.x < enemyQuadrant.x && _flag.transform.position.z < enemyQuadrant.z;
            else
                return _flag.transform.position.x > enemyQuadrant.x && _flag.transform.position.z > enemyQuadrant.z;
        }

        // Returns whether or not the flag is in our team's quadrant
        public static bool isFlagInTeamQuadrant()
        {
            if (enemyQuadrant.x == -1.5f)
                return _flag.transform.position.x > teamQuadrant.x && _flag.transform.position.z > teamQuadrant.z;
            else
                return _flag.transform.position.x < teamQuadrant.x && _flag.transform.position.z < teamQuadrant.z;
        }

        // Returns whether or not a given runner is in the enemy's quadrant
        public static bool isRunnerInEnemyQuadrant(Runner runner)
        {
            if (enemyQuadrant.x == -1.5f)
                return runner.transform.position.x < enemyQuadrant.x && runner.transform.position.z < enemyQuadrant.z;
            else
                return runner.transform.position.x > enemyQuadrant.x && runner.transform.position.z > enemyQuadrant.z;
        }

        // Returns whether or not a position is walkable
        public static bool isPosWalkable(Vector3 position)
        {
            var x = (25 + position.x) / Grid.getNodeDiameter();
            var z = (25 + position.z) / Grid.getNodeDiameter();

            return Grid.isWalkable(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
        }
    }
}