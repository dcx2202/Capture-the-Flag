using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(ObjectCollisionProps))]
    public class Separation : MonoBehaviour
    {

        /* The maximum acceleration for separation */
        public float SepMaxAcceleration = 25;

        /* This should be the maximum separation distance possible between a separation
         * target and the character.
         * So it should be: separation sensor radius + max target radius */
        public float MaxSepDist = 1f;

        private float _boundingRadius;

        // Use this for initialization
        private void Start()
        {
            _boundingRadius =GetComponent<ObjectCollisionProps>().BodyRadius;
        }

        public Vector3 getSteering(ICollection<Rigidbody> targets)
        {
            Vector3 acceleration = Vector3.zero;

            foreach (Rigidbody r in targets)
            {
                /* Get the direction and distance from the target */
                Vector3 direction = transform.position - r.position;
                float dist = direction.magnitude;

                if (dist < MaxSepDist)
                {
                    float targetRadius = r.GetComponent<ObjectCollisionProps>().BodyRadius;

                    /* Calculate the separation strength (can be changed to use inverse square law rather than linear) */
                    var strength = SepMaxAcceleration * (MaxSepDist - dist) / (MaxSepDist - _boundingRadius - targetRadius);

                    /* Added separation acceleration to the existing steering */
                    direction.Normalize();
                    acceleration += direction * strength;
                }
            }

            return acceleration;
        }
    }
}