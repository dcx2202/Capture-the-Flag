using System.Linq;
using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    public class Path
    {
        /// <summary>
        /// Another name for Waypoints. These are the 3d world coordinates for the waypoints
        /// </summary>
        public readonly Vector3[] LookPoints;

        public readonly float[] PointsValues;
        /// <summary>
        /// All the turn boundaries of this path
        /// </summary>
        public readonly Line[] TurnBoundaries;
        /// <summary>
        /// The index of the last point
        /// </summary>
        public readonly int FinishLineIndex;
        public readonly int SlowdownIndex;



        public Path(Node[] waypoints, Vector3 startPos, float turnDist, float stoppingDist)
        {
            LookPoints = waypoints.Select(wp => wp.WorldPosition).ToArray();
            // there are as many TurnBoundaries as points on the path
            TurnBoundaries = new Line[LookPoints.Length];
            PointsValues = new float[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            // initialize the previous point as the starting position
            var previousPoint = Vector3ToVector2(startPos);

            for (var i = 0; i < LookPoints.Length; i++)
            {
                // get the current point 
                var currentPoint = Vector3ToVector2(LookPoints[i]);
                PointsValues[i] = waypoints[i].MovementPenalty;
                // get the direction to the current point (normalized)
                var dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                // get the turn point boundary. This point will be used to determine the Line
                // We "substract" the turn distance, so the turn boundary will be before the point itself.
                // the last boundary should be on the point and not before it.
                var turnBoudaryPoint = i == FinishLineIndex ? currentPoint : currentPoint - dirToCurrentPoint * turnDist;

                // substract dirToCurrentPoint * turnDist to make sure the previous point is in the correct side of the line
                TurnBoundaries[i] = new Line(turnBoudaryPoint, previousPoint - dirToCurrentPoint * turnDist); 
                // update the previous point.
                previousPoint = turnBoudaryPoint;
            }

            // start at the end of the path and sum all the distance from the points
            // until we have summed a istance greater or equal to the stopping distance
            var distFromEndPoint = 0f;
            for (var i = LookPoints.Length - 1; i > 0 ; i--)
            {
                // sum the distance between this point and the previous
                distFromEndPoint += Vector3.Distance(LookPoints[i], LookPoints[i - 1]);

                // continue summing untill we are far enough
                if (!(distFromEndPoint > stoppingDist)) continue;

                // cache the slowdown index
                SlowdownIndex = i;

                // stop the loop
                break;
            }
        }

        /// <summary>
        /// Converts a Vector3 to a Vector2. 
        /// <para>(x, y, z) to (x, z).</para>
        /// </summary>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static Vector2 Vector3ToVector2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        /// <summary>
        /// Vizualize the path on the scene with gizmos
        /// </summary>
        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;

            foreach (var p in LookPoints)
                Gizmos.DrawCube(p + Vector3.up, Vector3.one/5);


            Gizmos.color = Color.white;
            foreach (var l in TurnBoundaries)
            {
                l.DrawWithGizmos(10);
            }
        }

        /// <summary>
        /// Check if the path values has changed compared with the initial values in the range received
        /// </summary>
        public bool PathValuesHasChanged(int currentIndex, int range)
        {
            for (var i = 0; i + currentIndex < LookPoints.Length && i < range; i++)
            {
                if (PathRequestManager.Instance.Grid.NodeFromWorldPoint(LookPoints[i + currentIndex]).MovementPenalty != PointsValues[i + currentIndex])
                    return true;
            }

            return false;
        }
    }
}
