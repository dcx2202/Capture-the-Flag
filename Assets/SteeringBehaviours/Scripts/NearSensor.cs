using System.Collections.Generic;
using DefaultTeam.Pathfinding.Scripts.Enums;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    public class NearSensor : MonoBehaviour
    {
        public List<Rigidbody> Targets = new List<Rigidbody>();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tags.Ground.ToString()) || 
                other.CompareTag(Tags.Boundary.ToString()) ||
                other.CompareTag(Tags.Props.ToString()))
                return;
            var rb = other.GetComponent<Rigidbody>();
            if (rb == null || Targets.Contains(rb)) return;
            Targets.Add(rb);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Ground.ToString()) ||
                other.CompareTag(Tags.Boundary.ToString()) ||
                other.CompareTag(Tags.Props.ToString()))
                return;
            var rb = other.GetComponent<Rigidbody>();
            if (rb == null || Targets.Contains(rb) == false) return;
            Targets.Remove(rb);
        }
    }
}