using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Seek))]
    public class Wander : MonoBehaviour
    {
        /* The forward offset of the wander square */
        public float WanderOffset = 1.5f;

        /* The radius of the wander square */
        public float WanderRadius = 4;

        /* The rate at which the wander orientation can change */
        public float WanderRate = 0.4f;

        private float _wanderOrientation;

        private SteeringBasics _steeringBasics;
        private Seek _seek;

        //private GameObject debugRing;

        private void Start()
        {
            //		DebugDraw debugDraw = gameObject.GetComponent<DebugDraw> ();
            //		debugRing = debugDraw.createRing (Vector3.zero, wanderRadius);

            _steeringBasics = GetComponent<SteeringBasics>();
            _seek = GetComponent<Seek>();
        }

        // Update is called once per frame
        private void Update()
        {
            var accel = GetSteering();

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering()
        {
            var characterOrientation = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            /* Update the wander orientation */
            _wanderOrientation += RandomBinomial() * WanderRate;

            /* Calculate the combined target orientation */
            var targetOrientation = _wanderOrientation + characterOrientation;

            /* Calculate the center of the wander circle */
            var targetPosition = transform.position + OrientationToVector(characterOrientation) * WanderOffset;

            //debugRing.transform.position = targetPosition;

            /* Calculate the target position */
            targetPosition = targetPosition + OrientationToVector(targetOrientation) * WanderRadius;

            //Debug.DrawLine (transform.position, targetPosition);

            return _seek.GetSteering(targetPosition);
        }

        /* Returns a random number between -1 and 1. Values around zero are more likely. */
        private float RandomBinomial()
        {
            return Random.value - Random.value;
        }

        /* Returns the orientation as a unit vector */
        private Vector3 OrientationToVector(float orientation)
        {
            return new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        }
    }
}