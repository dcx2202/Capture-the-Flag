using UnityEngine;

namespace SteeringBehaviours.Scripts.Basics
{
    [RequireComponent(typeof(Rigidbody))]
    public class Flee : MonoBehaviour
    {
        public float PanicDist = 3.5f;

        public bool DecelerateOnStop = true;

        public float MaxAcceleration = 10f;

        public float TimeToTarget = 0.1f;
        public bool IsFleeingTarget = true;

        private Rigidbody _rb;

        public Transform Target;

        private SteeringBasics _steeringBasics;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsFleeingTarget == false) return;
            var accel = GetSteering(Target.position);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        

        public Vector3 GetSteering(Vector3 targetPosition)
        {
            //Get the direction
            var acceleration = transform.position - targetPosition;

            //If the target is far way then don't flee
            if (acceleration.magnitude > PanicDist)
            {
                //Slow down if we should decelerate on stop
                if (DecelerateOnStop && _rb.velocity.magnitude > 0.001f)
                {
                    //Decelerate to zero velocity in time to target amount of time
                    acceleration = -_rb.velocity / TimeToTarget;

                    if (acceleration.magnitude > MaxAcceleration)
                        acceleration = GiveMaxAccel(acceleration);

                    return acceleration;
                }

                _rb.velocity = Vector3.zero;
                return Vector3.zero;
            }

            return GiveMaxAccel(acceleration);
        }

        private Vector3 GiveMaxAccel(Vector3 velocity)
        {
            //Remove the z coordinate
            //velocity.z = 0;

            velocity.Normalize();

            //Accelerate to the target
            velocity *= MaxAcceleration;

            return velocity;
        }
    }
}