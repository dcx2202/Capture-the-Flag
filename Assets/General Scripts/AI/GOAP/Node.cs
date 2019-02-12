using System.Collections.Generic;

namespace General_Scripts.AI.GOAP
{
    /// <summary>
    /// Used for building up the graph and holding the running costs of actions. This represents a node in the decision tree of the agent and it is never used for pathfinding!
    /// </summary>
    public class Node : IHeapItem<Node>
    {
        /// <summary>
        /// The node that precides this node in the decision tree.
        /// </summary>
        public readonly Node Parent;
        /// <summary>
        /// The total running cost from the start till this node.
        /// </summary>
        public readonly float RunningCost;
        /// <summary>
        /// The state this node represents
        /// </summary>
        public readonly HashSet<KeyValuePair<string, object>> State;
        /// <summary>
        /// The action associated with this node
        /// </summary>
        public readonly GoapAction Action;
        public int HeapIndex { get; set; }

        public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GoapAction action)
        {
            Parent = parent;
            RunningCost = runningCost;
            State = state;
            Action = action;
        }

        /// <summary>
        ///  1 - higher priority
        ///  0 - same priority
        /// -1 - lower priority
        /// </summary>
        public int CompareTo(Node other)
        {
            return RunningCost.CompareTo(other.RunningCost) * -1;
        }

    }
}
