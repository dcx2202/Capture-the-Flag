using System.Collections.Generic;
using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Evade))]
    [RequireComponent(typeof(Arrive))]
    public class Hide : MonoBehaviour
    {
        //public float DistanceFromBoundary = 0.6f;
        public Rigidbody Target;
        public List<Rigidbody> Objs;

        private SteeringBasics _steeringBasics;
        private Arrive _arrive;
        private Evade _evade;
        private WallAvoidance _wallAvoid;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _arrive = GetComponent<Arrive>();
            _evade = GetComponent<Evade>();
            _wallAvoid = GetComponent<WallAvoidance>();
        }
        // Update is called once per frame
        private void Update()
        {
            Vector3 hidePosition;
            var hideAccel = GetSteering(Target, Objs, out hidePosition);

            var accel = _wallAvoid.GetSteering(hidePosition - transform.position);

            if (accel.magnitude < 0.005f)
            {
                accel = hideAccel;
            }

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering(Rigidbody target, ICollection<Rigidbody> obstacles)
        {
            Vector3 bestHidingSpot;
            return GetSteering(target, obstacles, out bestHidingSpot);
        }

        public Vector3 GetSteering(Rigidbody target, ICollection<Rigidbody> obstacles, out Vector3 bestHidingSpot)
        {
            //Find the closest hiding spot
            var distToClostest = Mathf.Infinity;
            bestHidingSpot = Vector3.zero;

            foreach (var r in obstacles)
            {
                var hidingSpot = GetHidingPosition(r, target);

                var dist = Vector3.Distance(hidingSpot, transform.position);

                if (dist < distToClostest)
                {
                    distToClostest = dist;
                    bestHidingSpot = hidingSpot;
                }
            }

            //If no hiding spot is found then just evade the enemy
            if (float.IsPositiveInfinity(distToClostest))
            {
                return _evade.GetSteering(target);
            }

            //Debug.DrawLine(transform.position, bestHidingSpot);

            return _arrive.GetSteering(bestHidingSpot);
        }

        private Vector3 GetHidingPosition(Rigidbody obstacle, Rigidbody target)
        {
            var distAway = obstacle.GetComponent<ObjectCollisionProps>().BodyRadius;

            var dir = obstacle.position - target.position;
            dir.Normalize();

            return obstacle.position + dir * distAway;
        }
    }
}