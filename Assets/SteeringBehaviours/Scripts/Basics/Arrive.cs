using UnityEngine;

namespace SteeringBehaviours.Scripts.Basics
{
    [RequireComponent(typeof(SteeringBasics))]
    public class Arrive : MonoBehaviour {

        public Transform Target;
        public bool ArriveToTarget = true;

        private SteeringBasics _steeringBasics;
        private Rigidbody _rb;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (ArriveToTarget == false) return;

            var accel = GetSteering(Target.position);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }
        /// <summary>
        /// Returns the steering for a character so it arrives at the target
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public Vector3 GetSteering(Vector3 targetPosition)
        {
            /* Get the right direction for the linear acceleration */
            var targetVelocity = targetPosition - transform.position;

            // Remove the z coordinate
            //targetVelocity.z = 0;

            /* Get the distance to the target */
            var dist = targetVelocity.magnitude;

            /* If we are within the stopping radius then stop */
            if (dist < _steeringBasics.TargetRadius)
            {
                _rb.velocity = Vector3.zero;
                return Vector3.zero;
            }

            /* Calculate the target speed, full speed at slowRadius distance and 0 speed at 0 distance */
            float targetSpeed;
            if (dist > _steeringBasics.SlowRadius)
            {
                targetSpeed = _steeringBasics.MaxVelocity;
            }
            else
            {
                targetSpeed = _steeringBasics.MaxVelocity * (dist / _steeringBasics.SlowRadius);
            }

            /* Give targetVelocity the correct speed */
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;

            /* Calculate the linear acceleration we want */
            var acceleration = targetVelocity - _rb.velocity;

            /*
         Rather than accelerate the character to the correct speed in 1 second, 
         accelerate so we reach the desired speed in timeToTarget seconds 
         (if we were to actually accelerate for the full timeToTarget seconds).
        */
            acceleration *= 1 / _steeringBasics.TimeToTarget;

            /* Make sure we are accelerating at max acceleration */
            if (acceleration.magnitude > _steeringBasics.MaxAcceleration)
            {
                acceleration.Normalize();
                acceleration *= _steeringBasics.MaxAcceleration;
            }

            return acceleration;
        }
    }
}
