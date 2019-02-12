using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    public class Node : General_Scripts.IHeapItem<Node>
    {
        /// <summary>
        /// Defines if this Node is walkable or not
        /// </summary>
        public bool Walkable;
        /// <summary>
        /// represents this node in Unity world position
        /// </summary>
        public Vector3 WorldPosition;
        /// <summary>
        /// X coordinate of this node on the Grid
        /// </summary>
        public int GridX;
        /// <summary>
        /// Y coordinate of this node on the Grid
        /// </summary>
        public int GridY;

        /// <summary>
        /// Real cost to get here
        /// </summary>
        public int GCost;
        /// <summary>
        /// Heuristic cost to get to the end
        /// </summary>
        public int HCost;
        /// <summary>
        /// Points to the Node that was used to enter this node
        /// </summary>
        public Node Parent;

        /// <summary>
        /// Movement penaty assossiated with this node
        /// </summary>
        public int MovementPenalty;

        /// <summary>
        /// GCost + HCost
        /// </summary>
        public int FCost { get { return GCost + HCost; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="walkable">Defines if this node is walkable</param>
        /// <param name="worldPos">Defines this node world position</param>
        /// <param name="gridX">X coordinate of this node on the Grid</param>
        /// <param name="gridY">Y coordinate of this node on the Grid</param>
        /// <param name="movementPenalty"></param>
        public Node(bool walkable, Vector3 worldPos, int gridX, int gridY, int movementPenalty) {
            Walkable = walkable;
            WorldPosition = worldPos;
            GridX = gridX;
            GridY = gridY;
            MovementPenalty = movementPenalty;
        }

        #region IHeapItem interface

        /// <summary>
        ///  1 - higher priority
        ///  0 - same priority
        /// -1 - lower priority
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public int CompareTo(Node other)
        {
            var compare = FCost.CompareTo(other.FCost);

            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);

            // The int compareTo returns inversed of what we want for our heap, so we must multiply it by -1 in the end.
            return compare * -1;
        }

        /// <summary>
        /// This item current index in the heap.
        /// </summary>
        public int HeapIndex { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("({0}, {1})", GridX, GridY);
        }
    }
}
