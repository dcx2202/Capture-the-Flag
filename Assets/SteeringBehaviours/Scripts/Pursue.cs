using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Seek))]
    public class Pursue : MonoBehaviour
    {
        /* Maximum prediction time the pursue will predict in the future */
        public float MaxPrediction = 1f;
        public Rigidbody Target;

        private Rigidbody _rb;
        private SteeringBasics _steeringBasics;
        private Seek _seek;

        // Use this for initialization
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _steeringBasics = GetComponent<SteeringBasics>();
            _seek = GetComponent<Seek>();
        }

        // Update is called once per frame
        private void Update()
        {
            var accel = GetSteering(Target);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering(Rigidbody target)
        {
            /* Calculate the distance to the target */
            var displacement = target.position - transform.position;
            var distance = displacement.magnitude;

            /* Get the character's speed */
            var speed = _rb.velocity.magnitude;

            /* Calculate the prediction time */
            float prediction;
            if (speed <= distance / MaxPrediction)
                prediction = MaxPrediction;
            else
                prediction = distance / speed;

            /* Put the target together based on where we think the target will be */
            var explicitTarget = target.position + target.velocity * prediction;

            return _seek.GetSteering(explicitTarget);
        }
    }
}