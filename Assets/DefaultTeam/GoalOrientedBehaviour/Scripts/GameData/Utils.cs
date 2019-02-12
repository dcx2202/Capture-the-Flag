using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultTeam.GoalOrientedBehaviour.Scripts.GameData
{
    public static class Utils
    {
        /// <summary>
        /// Get the closest T from agent. Returns true if there is any in the objects list and false if there is none. if there is any, put it on the out variable
        /// </summary>
        /// <param name="objects">The list of T objects where we are going to search.</param>
        /// <param name="agent">The agent we are going to be using for comparing distances</param>
        /// <param name="closest">The out variable where we going to store the result. Can be null</param>
        /// <returns>Returns true fi we find a object.</returns>
        public static bool GetClosest<T>(IEnumerable<T> objects, Transform agent, out T closest) where T : MonoBehaviour
        {
            closest = objects
                .OrderBy(go => Vector3.Distance(go.transform.position, agent.position))
                .FirstOrDefault();

            return closest != default(T);
        }
    }
}
