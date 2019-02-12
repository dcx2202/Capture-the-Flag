using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(Seek))]
    public class WallAvoidance : MonoBehaviour
    {
        /* How far ahead the ray should extend */
        public float MainWhiskerLen = 1.25f;

        /* The distance away from the collision that we wish go */
        public float WallAvoidDistance = 0.5f;

        public float SideWhiskerLen = 0.701f;

        public float SideWhiskerAngle = 45f;

        public float MaxAcceleration = 40f;


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
            var accel = GetSteering();

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }

        public Vector3 GetSteering()
        {
            return GetSteering(_rb.velocity);
        }

        public Vector3 GetSteering(Vector3 facingDir)
        {
            var acceleration = Vector3.zero;

            /* Creates the ray direction vector */
            var rayDirs = new Vector3[3];
            rayDirs[0] = facingDir.normalized;

            var orientation = Mathf.Atan2(_rb.velocity.x,_rb.velocity.z);

            rayDirs[1] = OrientationToVector(orientation + SideWhiskerAngle * Mathf.Deg2Rad);
            rayDirs[2] = OrientationToVector(orientation - SideWhiskerAngle * Mathf.Deg2Rad);

            RaycastHit hit;

            /* If no collision do nothing */
            if (!FindObstacle(rayDirs, out hit))
            {
                return acceleration;
            }

            /* Create a target away from the wall to seek */
            var targetPostition = hit.point + hit.normal * WallAvoidDistance;

            /* If velocity and the collision normal are parallel then move the target a bit to
             the left or right of the normal */
            var cross = Vector3.Cross(_rb.velocity, hit.normal);
            if (cross.magnitude < 0.005f)
            {
                targetPostition = targetPostition + new Vector3(-hit.normal.z, hit.normal.y, hit.normal.x);
            }

            return _seek.GetSteering(targetPostition, MaxAcceleration);
        }

        /* Returns the orientation as a unit vector */
        private Vector3 OrientationToVector(float orientation)
        {
            return new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
        }

        private bool FindObstacle(Vector3[] rayDirs, out RaycastHit firstHit)
        {
            firstHit = new RaycastHit();
            var foundObs = false;

            for (var i = 0; i < rayDirs.Length; i++)
            {
                var rayDist = i == 0 ? MainWhiskerLen : SideWhiskerLen;

                RaycastHit hit;

                if (Physics.Raycast(transform.position, rayDirs[i], out hit, rayDist))
                {
                    foundObs = true;
                    firstHit = hit;
                    break;
                }

                //Debug.DrawLine(transform.position, transform.position + rayDirs[i] * rayDist);
            }

            return foundObs;
        }

    }
}