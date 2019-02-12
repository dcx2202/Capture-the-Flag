using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Seek))]
    public class WanderTarget : MonoBehaviour
    {
        public float WanderRadius = 1.2f;

        public float WanderDistance = 2f;

        //maximum amount of random displacement a second
        public float WanderJitter = 40f;

        public Transform WanderTargetTransform;
        public bool IsWanderingTarget = true;
        private Vector3 _wanderTargetPosition;

        private SteeringBasics _steeringBasics;
        private Seek _seek;

        private void Start()
        {
            //stuff for the wander behavior
            //var theta = Random.value * 2 * Mathf.PI;

            //create a vector to a target position on the wander circle
            _wanderTargetPosition = WanderTargetTransform.position;

            _steeringBasics = GetComponent<SteeringBasics>();
            _seek = GetComponent<Seek>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsWanderingTarget == false) return;

            var accel = GetSteering();

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering()
        {
            _wanderTargetPosition = WanderTargetTransform.position;
            //get the jitter for this time frame
            var jitter = WanderJitter * Time.deltaTime;

            ////add a small random vector to the target's position
            _wanderTargetPosition += new Vector3(Random.Range(-1f, 1f) * jitter, 0, Random.Range(-1f, 1f) * jitter);

            //make the wanderTarget fit on the wander circle again
            //_wanderTargetPosition.Normalize();
            _wanderTargetPosition *= WanderRadius;

            //move the target in front of the character
            var targetPosition = transform.forward * WanderDistance + _wanderTargetPosition;

            //Debug.DrawLine(transform.position, targetPosition);
            //print(targetPosition);
            return _seek.GetSteering(targetPosition);
        }
    }
}