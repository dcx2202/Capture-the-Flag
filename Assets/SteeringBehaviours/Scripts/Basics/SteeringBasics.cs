using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviours.Scripts.Basics
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SteeringBasics : MonoBehaviour
    {
        //public float CharacterRadius = 0f;
        public float MaxVelocity = 3.5f;
        /* The maximum acceleration */
        public float MaxAcceleration = 10f;
        /* The radius from the target that means we are close enough and have arrived */
        public float TargetRadius = 0.005f;
        /* The radius from the target where we start to slow down  */
        public float SlowRadius = 1f;
        /* The time in which we want to achieve the targetSpeed */
        public float TimeToTarget = 0.1f;
        public float TurnSpeed = 20f;
        
        public bool Smoothing = true;
        public int NumSamplesForSmoothing = 5;

        private Queue<Vector3> _velocitySamples = new Queue<Vector3>();
        private Rigidbody _rb;

        // Use this for initialization
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }
        

        /// <summary>
        /// Updates the velocity of the current game object by the given linear acceleration
        /// </summary>
        /// <param name="linearAcceleration"></param>
        public void Steer(Vector3 linearAcceleration)
        {
            _rb.velocity += linearAcceleration * Time.deltaTime;

            if (_rb.velocity.sqrMagnitude > MaxVelocity*MaxVelocity)
            {
                _rb.velocity = _rb.velocity.normalized * MaxVelocity;
            }
        }


        /// <summary>
        /// Rotates "this" transform to the given direction
        /// </summary>
        /// <param name="direction"></param>
        public void Face(Vector3 direction)
        {
            direction.Normalize();
            // If we have a non-zero direction then look towards that direciton otherwise do nothing
            if (!(direction.sqrMagnitude > 0.001f)) return;

            var currentEuler = transform.rotation.eulerAngles;
            var toRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var rotation = Mathf.LerpAngle(currentEuler.y, toRotation, Time.deltaTime * TurnSpeed);

            transform.rotation = Quaternion.Euler(currentEuler.x, rotation, currentEuler.z);
        }

        /// <summary>
        /// Rotates "this" transform to the given direction
        /// </summary>
        /// <param name="toRotation"></param>
        public void Face(Quaternion toRotation)
        {
            Face(toRotation.eulerAngles.y);
        }

        /// <summary>
        /// Rotates "this" transform to the given direction
        /// </summary>
        /// <param name="toRotation"></param>
        public void Face(float toRotation)
        {
            var rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, toRotation, Time.deltaTime * TurnSpeed);

            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }


        /// <summary>
        /// Makes the current game object look where he is going
        /// </summary>
        public void LookWhereYoureGoing()
        {
            var direction = _rb.velocity;

            if (Smoothing)
            {
                if (_velocitySamples.Count == NumSamplesForSmoothing)
                {
                    _velocitySamples.Dequeue();
                }

                _velocitySamples.Enqueue(_rb.velocity);

                direction = Vector3.zero;

                foreach (var v in _velocitySamples)
                {
                    direction += v;
                }

                direction /= _velocitySamples.Count;
            }
            
            Face(direction);
        }

        

        ///// <summary>
        ///// Returns the middle point between two targets
        ///// </summary>
        ///// <param name="target1"></param>
        ///// <param name="target2"></param>
        ///// <returns></returns>
        //public Vector3 Interpose(Rigidbody target1, Rigidbody target2)
        //{
        //    var midPoint = (target1.position + target2.position) / 2;

        //    var timeToReachMidPoint = Vector3.Distance(midPoint, transform.position) / MaxVelocity;

        //    var futureTarget1Pos = target1.position + target1.velocity * timeToReachMidPoint;
        //    var futureTarget2Pos = target2.position + target2.velocity * timeToReachMidPoint;

        //    midPoint = (futureTarget1Pos + futureTarget2Pos) / 2;

        //    return Arrive(midPoint);
        //}

        /// <summary>
        /// Checks to see if the target is in front of the character
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsInFront(Vector3 target)
        {
            return IsFacing(target, 0);
        }

        public bool IsFacing(Vector3 target, float cosineValue)
        {
            var facing = transform.forward.normalized;

            var directionToTarget = (target - transform.position);
            directionToTarget.Normalize();

            return Vector3.Dot(facing, directionToTarget) >= cosineValue;
        }

        private void OnCollisionEnter(Collision col)
        {
            //if(col.transform.gameObject.CompareTag(Tags.Boundary.ToString()))
            //    _rb.velocity = Vector3.zero;
        }

        public void Stop()
        {
            //print("stopping");
            _rb.velocity = Vector3.zero;
        }
    }
}