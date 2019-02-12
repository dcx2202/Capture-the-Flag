using UnityEngine;

namespace SteeringBehaviours.Scripts.Basics
{
    [RequireComponent(typeof(SteeringBasics))]
    public class Seek : MonoBehaviour
    {
        public Transform Target;
        public bool IsSeekingTarget = true;

        private SteeringBasics _steeringBasics;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsSeekingTarget == false) return;

            var accel = GetSteering(Target.position);

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        /// <summary>
        /// A seek steering behavior. Will return the steering for the current game object to seek a given position
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="maxSeekAccel"></param>
        /// <returns></returns>
        public Vector3 GetSteering(Vector3 targetPosition, float maxSeekAccel)
        {
            //Get the direction
            Vector3 acceleration = targetPosition - transform.position;

            //Remove the z coordinate
            //acceleration.z = 0;

            acceleration.Normalize();

            //Accelerate to the target
            acceleration *= maxSeekAccel;

            return acceleration;
        }

        /// <summary>
        /// A seek steering behavior. Will return the steering for the current game object to seek a given position. Assumes default max acceleration.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public Vector3 GetSteering(Vector3 targetPosition)
        {
            return GetSteering(targetPosition, _steeringBasics.MaxAcceleration);
        }
    }
}
