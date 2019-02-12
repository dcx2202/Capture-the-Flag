using UnityEngine;

namespace DefaultTeam.GoalOrientedBehaviour.Scripts.GameData
{
    /// <summary>
    /// Offers randomization
    /// </summary>
    public static class MyRandom
    {
        /// <summary>
        /// Gets a random Vector3 between a min and a max vectors. This implementation generates random points between the min and max vectors in a rectangle area.
        /// </summary>
        /// <param name="min">The minimal limit for the random vector.</param>
        /// <param name="max">The maximum limit for the random vector</param>
        /// <returns></returns>
        public static Vector3 GetRandomVector3(Vector3 min, Vector3 max)
        {
            return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        }
    }
}
