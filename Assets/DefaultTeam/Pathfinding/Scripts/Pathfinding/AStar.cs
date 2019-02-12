using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultTeam.Pathfinding.Scripts.Pathfinding
{
    public class AStar : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Grid Object
        /// </summary>
        public Grid Grid;
        
        /// <summary>
        /// Find the closest path in the Grid, from the startnode to the endnote
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        public void FindPath(PathRequest request, Action<PathResult> callback)
        {
            //print("start: " + request.PathStart);
            //print("end: " + request.PathEnd);

            var waypoints = new Node[0];
            
            // Get the start and end node
            if (request == null)
            {
                callback(new PathResult(null, false, null));
                return;
            }

            var startNode = Grid.NodeFromWorldPoint(request.PathStart);
            var targetNode = Grid.NodeFromWorldPoint(request.PathEnd);
            startNode.Parent = startNode;

            //print("start: " + startNode.Walkable);
            //print("end: " + targetNode.Walkable);

            var pathSuccess = PathSuccess(startNode, targetNode);

            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
                //Debug.Log("path found");
            }
            //else
            //    Debug.Log("failed to find path");

            callback(new PathResult(waypoints, pathSuccess, request.Callback));
        }

        /// <summary>
        /// Runs the path finding algorithm. Returns true if a path as found and false if not. The resulst if stored in the target node and can be retraced through the parent reference. Returns true if a path was found and false if not.
        /// </summary>
        /// <param name="startNode"> The start node for the path finding.</param>
        /// <param name="targetNode"> The end node for the path findign. If a path is found, you can retrace the path from the parent references.</param>
        /// <returns> True if a path is found.</returns>
        private bool PathSuccess(Node startNode, Node targetNode)
        {
            if (startNode.Walkable == false || targetNode.Walkable == false) return false;

            var pathSuccess = false;
            var frontier = new General_Scripts.Heap<Node>(Grid.MaxSize); // open set
            var visited = new HashSet<Node>(); // closed set
            frontier.Add(startNode);

            while (frontier.Count > 0) // While there are nodes in the frontier
            {
                var currentNode = frontier.RemoveFirst(); // get the cheapest node

                // remove the current set from the frontier and add it to the visited set
                visited.Add(currentNode);

                // if the current node is the target node, then we arrived 
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    //sw.Stop();
                    //print("path found: " + sw.ElapsedMilliseconds + "ms");
                    break;
                }

                foreach (var neighbour in Grid.GetNeighbours(currentNode))
                {
                    if (neighbour.Walkable == false || visited.Contains(neighbour))
                        continue;

                    // calculate the G cost for this neighbour
                    var newCostToNeighbour =
                        currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
                    // if the new cost is lower than the previous GCost or if the neighbour is not in the frontier (first time visited)
                    if (newCostToNeighbour < neighbour.GCost || frontier.Contains(neighbour) == false)
                    {
                        neighbour.GCost = newCostToNeighbour;
                        neighbour.HCost =
                            GetDistance(neighbour,
                                targetNode); // streight distance from neighbour node to the end (heuristic)
                        neighbour.Parent = currentNode; // store from where this node was entered

                        // if not in the frontier, add it to the frontier
                        if (frontier.Contains(neighbour) == false)
                            frontier.Add(neighbour);
                        else
                            frontier.UpdateItem(neighbour);
                    }
                }
            }
            return pathSuccess;
        }

        /// <summary>
        /// Retrace the path. Returns all nodes, begining at the start and finishing at the end.
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        private Node[] RetracePath(Node startNode, Node endNode)
        {
            var path = new List<Node>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            //var waypoints = SimplifyPath(path);
            var waypoints = path.ToArray();
            //invert the list so it will be from the start
            Array.Reverse(waypoints);

            return waypoints;
            //path.Reverse();
            //return path.Select(n => n.WorldPosition).ToArray();
        }

        /// <summary>
        /// Removes redundant nodes from the path. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Vector3[] SimplifyPath(List<Node> path)
        {
            var waypoints = new List<Vector3>();
            var directionOld = Vector2.zero;

            for (var i = 1; i < path.Count; i++)
            {
                var directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].WorldPosition);
                    directionOld = directionNew;
                }

            }

            return waypoints.ToArray();
        }

        /// <summary>
        /// Returns a int with the distance of two notes. Horizontal or Vertical moves costs 10, diagonal moves costs 14 (~sqrt(2)*10)
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        public static int GetDistance(Node nodeA, Node nodeB)
        {
            var dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX); // absolute value for the X's difference
            var dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY); // absolute value for the Y's difference

            if (dstX > dstY)
                return 14*dstY + 10 * (dstX-dstY);

            return 14*dstX + 10 * (dstY-dstX);
        }

        /// <summary>
        /// Get the cost from start to end points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public float GetCost(Vector3 start, Vector3 end)
        {
            var startNode = Grid.NodeFromWorldPoint(start);
            var targetNode = Grid.NodeFromWorldPoint(end);
            startNode.Parent = startNode;

            // return the cost of the path
            if (PathSuccess(startNode, targetNode))
            {
                print("cost: " + targetNode.FCost);
                return targetNode.FCost;
            }

            // if we cannot find a path, then the cost is infinity
            print("cost: " + Mathf.Infinity);
            return Mathf.Infinity;
        }
    }
}
