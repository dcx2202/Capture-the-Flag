using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Flee))]
    public class Evade : MonoBehaviour
    {
        public Rigidbody Target;

        public float MaxPrediction = 1f;
        public bool IsEvadingTarget = true;
        private Flee _flee;
        private SteeringBasics _steeringBasics;

        // Use this for initialization
        private void Start()
        {
            _flee = GetComponent<Flee>();
            _steeringBasics = GetComponent<SteeringBasics>();
        }

        private void Update()
        {
            if (IsEvadingTarget == false) return;
            var accel = GetSteering(Target);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering(Rigidbody target)
        {
            /* Calculate the distance to the target */
            var displacement = target.position - transform.position;
            var distance = displacement.magnitude;

            /* Get the targets's speed */
            var speed = target.velocity.magnitude;

            /* Calculate the prediction time */
            float prediction;
            if (speed <= distance / MaxPrediction)
            {
                prediction = MaxPrediction;
            }
            else
            {
                prediction = distance / speed;
                //Place the predicted position a little before the target reaches the character
                prediction *= 0.9f;
            }

            /* Put the target together based on where we think the target will be */
            var explicitTarget = target.position + target.velocity * prediction;

            return _flee.GetSteering(explicitTarget);
        }
    }
}