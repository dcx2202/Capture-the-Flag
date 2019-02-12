using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Arrive))]
    [RequireComponent(typeof(Rigidbody))]
    public class FollowPath : MonoBehaviour
    {
        public float StopRadius = 0.005f;

        public float PathOffset = 0.71f;

        public float PathDirection = 1f;

        public bool PathLoop;

        public bool ReversePath;

        public LinePath Path;

        private SteeringBasics _steeringBasics;
        private Arrive _arrive;
        private Rigidbody _rb;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _rb = GetComponent<Rigidbody>();
            _arrive = GetComponent<Arrive>();

            Path.CalcDistances();
        }
        // Update is called once per frame
        private void Update()
        {
            Path.Draw();

            if (ReversePath && IsAtEndOfPath())
            {
                Path.ReversePath();
            }

            var accel = GetSteering(Path, PathLoop);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public bool IsAtEndOfPath()
        {
            var aux = new Vector3(transform.position.x, 0, transform.position.z);
            return Vector3.Distance(Path.EndNode, aux) < StopRadius;
        }

        public Vector3 GetSteering(LinePath path)
        {
            return GetSteering(path, false);
        }

        public Vector3 GetSteering(LinePath path, bool pathLoop)
        {
            Vector3 targetPosition;
            return GetSteering(path, pathLoop, out targetPosition);
        }

        public Vector3 GetSteering(LinePath path, bool pathLoop, out Vector3 targetPosition)
        {

            // If the path has only one node then just go to that position;
            if (path.Length == 1)
            {
                targetPosition = path[0];
            }
            // Else find the closest spot on the path to the character and go to that instead.
            else
            {
                if (!pathLoop)
                {
                    /* Find the final destination of the character on this path */
                    var finalDestination = PathDirection > 0 ? path[path.Length - 1] : path[0];

                    /* If we are close enough to the final destination then either stop moving or reverse if 
                     * the character is set to loop on paths */
                    if (Vector3.Distance(transform.position, finalDestination) < StopRadius)
                    {
                        targetPosition = finalDestination;

                        _rb.velocity = Vector3.zero;
                        return Vector3.zero;
                    }
                }

                /* Get the param for the closest position point on the path given the character's position */
                var param = path.GetParam(transform.position);

                /* Move down the path */
                param += PathDirection * PathOffset;

                /* Set the target position */
                targetPosition = path.GetPosition(param, pathLoop);
            }

            return _arrive.GetSteering(targetPosition);
        }
    }

}