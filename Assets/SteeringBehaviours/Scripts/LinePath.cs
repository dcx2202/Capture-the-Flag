using System;
using UnityEngine;

namespace SteeringBehaviours.Scripts
{
    [Serializable]
    public class LinePath
    {
        public Vector3[] Nodes;
        private float[] _distances;
        [NonSerialized]
        public float MaxDist;

        // Indexer declaration.
        public Vector3 this[int i]
        {
            get { return Nodes[i]; }
            set { Nodes[i] = value;}
        }

        public int Length
        {
            get { return Nodes.Length; }
        }

        public Vector3 EndNode
        {
            get { return Nodes[Nodes.Length - 1]; }
        }

        /* This function creates a path of line segments */
        public LinePath(Vector3[] nodes)
        {
            Nodes = nodes;

            CalcDistances();
        }

        /* Loops through the path's nodes and determines how far each node in the path is 
         * from the starting node */
        public void CalcDistances()
        {
            _distances = new float[Nodes.Length];
            _distances[0] = 0;

            for (var i = 0; i < Nodes.Length - 1; i++)
            {
                _distances[i + 1] = _distances[i] + Vector3.Distance(Nodes[i], Nodes[i + 1]);
            }

            MaxDist = _distances[_distances.Length - 1];
        }

        /* Draws the path in the scene view */
        public void Draw()
        {
            for (var i = 0; i < Nodes.Length - 1; i++)
            {
                Debug.DrawLine(Nodes[i], Nodes[i + 1], Color.cyan, 0.0f, false);
            }
        }

        /* Gets the param for the closest point on the path given a position */
        public float GetParam(Vector3 position)
        {
            var closestSegment = GetClosestSegment(position);

            var param = _distances[closestSegment] + GetParamForSegment(position, Nodes[closestSegment], Nodes[closestSegment + 1]);

            return param;
        }

        public int GetClosestSegment(Vector3 position)
        {
            /* Find the first point in the closest line segment to the path */
            var closestDist = DistToSegment(position, Nodes[0], Nodes[1]);
            var closestSegment = 0;

            for (var i = 1; i < Nodes.Length - 1; i++)
            {
                var dist = DistToSegment(position, Nodes[i], Nodes[i + 1]);

                if (dist <= closestDist)
                {
                    closestDist = dist;
                    closestSegment = i;
                }
            }

            return closestSegment;
        }

        /* Given a param it gets the position on the path */
        public Vector3 GetPosition(float param, bool pathLoop = false)
        {
            /* Make sure the param is not past the beginning or end of the path */
            if (param < 0)
            {
                param = pathLoop ? param + MaxDist : 0;
            }
            else if (param > MaxDist)
            {
                param = pathLoop ? param - MaxDist : MaxDist;
            }

            /* Find the first node that is farther than given param */
            var i = 0;
            for (; i < _distances.Length; i++)
            {
                if (_distances[i] > param)
                {
                    break;
                }
            }

            /* Convert it to the first node of the line segment that the param is in */
            if (i > _distances.Length - 2)
            {
                i = _distances.Length - 2;
            }
            else
            {
                i -= 1;
            }

            /* Get how far along the line segment the param is */
            var t = (param - _distances[i]) / Vector3.Distance(Nodes[i], Nodes[i + 1]);

            /* Get the position of the param */
            return Vector3.Lerp(Nodes[i], Nodes[i + 1], t);
        }

        /* Gives the distance of a point to a line segment.
         * p is the point, v and w are the two points of the line segment */
        private static float DistToSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            var vw = w - v;

            var l2 = Vector3.Dot(vw, vw);

            if (l2 == 0)
            {
                return Vector3.Distance(p, v);
            }

            var t = Vector3.Dot(p - v, vw) / l2;

            if (t < 0)
            {
                return Vector3.Distance(p, v);
            }

            if (t > 1)
            {
                return Vector3.Distance(p, w);
            }

            var closestPoint = Vector3.Lerp(v, w, t);

            return Vector3.Distance(p, closestPoint);
        }

        /* Finds the param for the closest point on the segment vw given the point p */
        private static float GetParamForSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            var vw = w - v;

            var l2 = Vector3.Dot(vw, vw);

            if (l2 == 0)
            {
                return 0;
            }

            var t = Vector3.Dot(p - v, vw) / l2;

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            return t * Mathf.Sqrt(l2);
        }

        public void RemoveNode(int i)
        {
            var newNodes = new Vector3[Nodes.Length - 1];

            var newNodesIndex = 0;
            for (var j = 0; j < newNodes.Length; j++)
            {
                if (j != i)
                {
                    newNodes[newNodesIndex] = Nodes[j];
                    newNodesIndex++;
                }
            }

            Nodes = newNodes;

            CalcDistances();
        }

        public void ReversePath()
        {
            Array.Reverse(Nodes);

            CalcDistances();
        }
    }
}