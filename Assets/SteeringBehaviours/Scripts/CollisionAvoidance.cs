using System.Collections.Generic;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(ObjectCollisionProps))]
    [RequireComponent(typeof(NearSensor))]
    public class CollisionAvoidance : MonoBehaviour
    {
        public float MaxAcceleration = 15f;

        //public float agentRadius = 0.25f;

        private float _characterRadius = 0f;
        private ObjectCollisionProps _colProps;

        private NearSensor _colAvoidSensor;
        private SteeringBasics _steeringBasics;

        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _colProps = GetComponent<ObjectCollisionProps>();
            _characterRadius = _colProps.BodyRadius;
            _steeringBasics = GetComponent<SteeringBasics>();
            _colAvoidSensor = transform.GetComponent<NearSensor>();
        }

        private void Update()
        {
            var accel = GetSteering(_colAvoidSensor.Targets);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering(ICollection<Rigidbody> targets)
        {
            var acceleration = Vector3.zero;

            /* 1. Find the target that the character will collide with first */

            /* The first collision time */
            var shortestTime = float.PositiveInfinity;

            /* The first target that will collide and other data that we will need and can avoid recalculating */
            Rigidbody firstTarget = null;
            //float firstMinSeparation = 0, firstDistance = 0;
            var firstMinSeparation = 0f;
            var firstDistance = 0f;
            var firstRadius = 0f;

            var firstRelativePos = Vector3.zero;
            var firstRelativeVel = Vector3.zero;

            foreach (var targetRb in targets)
            {
                /* Calculate the time to collision */
                var relativePos = transform.position - targetRb.position;
                var relativeVel = _rb.velocity - targetRb.velocity;
                var distance = relativePos.magnitude;
                var relativeSpeed = relativeVel.magnitude;

                //if (Math.Abs(relativeSpeed) < 0.0001f)
                //{
                //    continue;
                //}

                var timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

                /* Check if they will collide at all */
                var separation = relativePos + relativeVel * timeToCollision;
                var minSeparation = separation.magnitude;

                var targetRadius = targetRb.GetComponent<ObjectCollisionProps>().BodyRadius;

                if (minSeparation > _characterRadius + targetRadius)
                    //if (minSeparation > 2 * agentRadius)
                {
                    continue;
                }

                /* Check if its the shortest */
                if (timeToCollision > 0 && timeToCollision < shortestTime)
                {
                    shortestTime = timeToCollision;
                    firstTarget = targetRb;
                    firstMinSeparation = minSeparation;
                    firstDistance = distance;
                    firstRelativePos = relativePos;
                    firstRelativeVel = relativeVel;
                    firstRadius = targetRadius;
                }
            }

            /* 2. Calculate the steering */

            /* If we have no target then exit */
            if (firstTarget == null)
            {
                return acceleration;
            }

            /* If we are going to collide with no separation or if we are already colliding then 
             * steer based on current position */
            if (firstMinSeparation <= 0 || firstDistance < _characterRadius + firstRadius)
                //if (firstMinSeparation <= 0 || firstDistance < 2 * agentRadius)
            {
                acceleration = transform.position - firstTarget.position;
            }
            /* Else calculate the future relative position */
            else
            {
                acceleration = firstRelativePos + firstRelativeVel * shortestTime;
            }

            /* Avoid the target */
            acceleration.Normalize();
            acceleration *= MaxAcceleration;

            return acceleration;
        }
    }
}