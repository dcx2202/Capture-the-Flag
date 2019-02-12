using SteeringBehaviours.Scripts.Basics;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [RequireComponent(typeof(SteeringBasics))]
    [RequireComponent(typeof(WanderTarget))]
    [RequireComponent(typeof(Cohesion))]
    [RequireComponent(typeof(Separation))]
    [RequireComponent(typeof(VelocityMatch))]
    [RequireComponent(typeof(NearSensor))]
    public class Flocking : MonoBehaviour
    {
        public float CohesionWeight = 1.5f;
        public float SeparationWeight = 2f;
        public float VelocityMatchWeight = 1f;

        private SteeringBasics _steeringBasics;
        private WanderTarget _wander;
        private Cohesion _cohesion;
        private Separation _separation;
        private VelocityMatch _velocityMatch;

        private NearSensor _sensor;

        // Use this for initialization
        private void Start()
        {
            _steeringBasics = GetComponent<SteeringBasics>();
            _wander = GetComponent<WanderTarget>();
            _cohesion = GetComponent<Cohesion>();
            _separation = GetComponent<Separation>();
            _velocityMatch = GetComponent<VelocityMatch>();

            _sensor = GetComponent<NearSensor>();
        }

        // Update is called once per frame
        private void Update()
        {
            var accel = Vector3.zero;

            accel += _cohesion.GetSteering(_sensor.Targets) * CohesionWeight;
            accel += _separation.getSteering(_sensor.Targets) * SeparationWeight;
            accel += _velocityMatch.GetSteering(_sensor.Targets) * VelocityMatchWeight;

            if (accel.magnitude < 0.005f)
            {
                accel = _wander.GetSteering();
            }

            _steeringBasics.Steer(accel);
            _steeringBasics.LookWhereYoureGoing();
        }
    }
}