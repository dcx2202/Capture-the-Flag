using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviours.Scripts.Basics
{
    [RequireComponent(typeof(SteeringBasics))]
    public class VelocityMatch : MonoBehaviour
    {

        public float FacingCosine = 90;
        public float TimeToTarget = 0.1f;
        public float MaxAcceleration = 4f;

        private float _facingCosineVal;

        private Rigidbody _rb;
        private SteeringBasics _steeringBasics;

        // Use this for initialization
        private void Start()
        {
            _facingCosineVal = Mathf.Cos(FacingCosine * Mathf.Deg2Rad);

            _rb = GetComponent<Rigidbody>();
            _steeringBasics = GetComponent<SteeringBasics>();
        }

        public Vector3 GetSteering(ICollection<Rigidbody> targets)
        {
            var accel = Vector3.zero;
            var count = 0;

            foreach (var r in targets)
            {
                if (_steeringBasics.IsFacing(r.position, _facingCosineVal))
                {
                    /* Calculate the acceleration we want to match this target */
                    var a = r.velocity - _rb.velocity;
                    /*
                     Rather than accelerate the character to the correct speed in 1 second, 
                     accelerate so we reach the desired speed in timeToTarget seconds 
                     (if we were to actually accelerate for the full timeToTarget seconds).
                    */
                    a = a / TimeToTarget;

                    accel += a;

                    count++;
                }
            }

            if (count > 0)
            {
                accel = accel / count;

                /* Make sure we are accelerating at max acceleration */
                if (accel.magnitude > MaxAcceleration)
                {
                    accel = accel.normalized * MaxAcceleration;
                }
            }

            return accel;
        }
    }
}