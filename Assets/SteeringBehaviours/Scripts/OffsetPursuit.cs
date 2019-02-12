using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Arrive))]
    [RequireComponent(typeof(Separation))]
    [RequireComponent(typeof(NearSensor))]
    public class OffsetPursuit : MonoBehaviour
    {
        /* Maximum prediction time the pursue will predict in the future */
        public float MaxPrediction = 1f;
        public Vector3 Offset;
        public float GroupLookDist = 1.5f;
        public Rigidbody Target;

        //private Rigidbody _rb;
        private SteeringBasics _steeringBasics;
        private Arrive _arrive;
        private Separation _separation;
        private NearSensor _sensor;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _arrive = GetComponent<Arrive>();
            _separation = GetComponent<Separation>();
            _sensor = GetComponent<NearSensor>();
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            Vector3 targetPos;
            var offsetAccel = GetSteering(Target, Offset, out targetPos);
            var sepAccel = _separation.getSteering(_sensor.Targets);

            _steeringBasics.Steer(offsetAccel + sepAccel);

            /* If we are still arriving then look where we are going, else look the same direction as our formation target */
            if (Vector3.Distance(transform.position, targetPos) > GroupLookDist)
            {
                _steeringBasics.LookWhereYoureGoing();
            }
            else
            {
                _steeringBasics.Face(Target.rotation);
            }
        }

        public Vector3 GetSteering(Rigidbody target, Vector3 offset)
        {
            Vector3 targetPos;
            return GetSteering(target, offset, out targetPos);
        }

        public Vector3 GetSteering(Rigidbody target, Vector3 offset, out Vector3 targetPos)
        {
            var worldOffsetPos = target.position + target.transform.TransformDirection(offset);

            //Debug.DrawLine(transform.position, worldOffsetPos);

            /* Calculate the distance to the offset point */
            var displacement = worldOffsetPos - transform.position;
            var distance = displacement.magnitude;

            /* Get the character's speed */
            var speed = Target.velocity.magnitude;

            /* Calculate the prediction time */
            float prediction;
            if (speed <= distance / MaxPrediction)
            {
                prediction = MaxPrediction;
            }
            else
            {
                prediction = distance / speed;
            }

            /* Put the target together based on where we think the target will be */
            targetPos = worldOffsetPos + target.velocity * prediction;

            return _arrive.GetSteering(targetPos);
        }
    }
}