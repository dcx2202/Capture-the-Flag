using System.Collections;
using UnityEngine;

namespace General_Scripts
{
    public class ObstacleSpawner : MonoBehaviour
    {
        public Collider RedTeamCollider;
        public Collider YellowTeamCollider;

        public GameObject Obstacle;


        private void Awake()
        {
            SpawnObject(RedTeamCollider);
            SpawnObject(RedTeamCollider);
            SpawnObject(RedTeamCollider);
            SpawnObject(YellowTeamCollider);
            SpawnObject(YellowTeamCollider);
            SpawnObject(YellowTeamCollider);

        }

        private void SpawnObject(Collider teamCollider)
        {
            var obstacle = Instantiate(Obstacle);
            obstacle.transform.position = teamCollider.bounds.GetRandomPoint();
            //obstacle.GetComponent<Wander>().Bounds = teamCollider.bounds;
        }

    }

    public static class BoundsExtensions
    {
        public static Vector3 GetRandomPoint(this Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
    }
}