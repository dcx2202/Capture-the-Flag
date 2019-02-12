using System.Collections.Generic;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Arrive))]
    public class Cohesion : MonoBehaviour
    {

        public float FacingCosine = 120f;

        private float _facingCosineVal;

        private SteeringBasics _steeringBasics;

        private Arrive _arrive;

        // Use this for initialization
        private void Start()
        {
            _facingCosineVal = Mathf.Cos(FacingCosine * Mathf.Deg2Rad);
            _steeringBasics = GetComponent<SteeringBasics>();
            _arrive = GetComponent<Arrive>();
        }

        public Vector3 GetSteering(ICollection<Rigidbody> targets)
        {
            var centerOfMass = Vector3.zero;
            var count = 0;

            /* Sums up everyone's position who is close enough and in front of the character */
            foreach (var r in targets)
            {
                if (_steeringBasics.IsFacing(r.position, _facingCosineVal))
                {
                    centerOfMass += r.position;
                    count++;
                }
            }

            if (count == 0)
            {
                return Vector3.zero;
            }
            else
            {
                centerOfMass = centerOfMass / count;

                return _arrive.GetSteering(centerOfMass);
            }
        }
    }
}