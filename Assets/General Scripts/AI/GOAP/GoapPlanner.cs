using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace General_Scripts.AI.GOAP
{
    /// <summary>
    /// Plans what actions can be completed in order to fulfill a goal state.
    /// </summary>
    public class GoapPlanner
    {
        /// <summary>
        /// Plan what sequence of actions can fulfill the goal. Returns null if a plan could not be found, or a list of the actions that must be performed, in order, to fulfill the goal.
        /// </summary>
        /// <param name="agent"> The agents the planner is planning. </param>
        /// <param name="availableActions"> The agent's current available actions</param>
        /// <param name="worldState"> The current world state relative to the agent. </param>
        /// <param name="goals"> This agent goals </param>
        /// <returns> Null if a plan could not be found, or a list of the actions that must be performed, in order, to fulfill the goal</returns>
        public Queue<GoapAction> Plan(GameObject agent, HashSet<GoapAction> availableActions, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goals)
        {
            // reset the actions so we can start fresh with them
            foreach (var action in availableActions)
                action.DoReset();

            // check what actions can run using their checkProceduralPrecondition
            var usableActions = new HashSet<GoapAction>();
            foreach (var action in availableActions)
            {
                if (action.CheckProceduralPrecondition(agent))
                    usableActions.Add(action);
            }

            // we now have all actions that can run, stored in usableActions
            // build up the tree and record the leaf nodes that provide a solution to the goal.
            var leaves = new Heap<Node>(100);

            // build graph
            var start = new Node(null, 0, worldState, null);
            var success = BuildGraph(start, leaves, usableActions, goals);

            if (success == false)
            {
                // oh no, we didn't get a plan
                //Debug.Log("NO PLAN");
                return null;
            }

            // get the cheapest leaf. It is the head of the heap.
            var cheapest = leaves.RemoveFirst();

            // get the last node and work back through the parents
            var result = new List<GoapAction>();
            var n = cheapest;
            while (n != null)
            {
                if (n.Action != null)
                {
                    result.Insert(0, n.Action); // insert the action in the front
                }
                n = n.Parent;
            }
            
            // we now have this action list in correct order
            var queue = new Queue<GoapAction>();
            foreach (var a in result)
            {
                queue.Enqueue(a);
            }

            // hooray we have a plan!
            return queue;
        }

        /// <summary>
        /// Returns true if at least one solution was found.
        /// The possible paths are stored in the leaves heap.Each leaf has a
        /// 'runningCost' value where the lowest cost will be the best action
        /// sequence.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="leaves"></param>
        /// <param name="usableActions"></param>
        /// <param name="goal"></param>
        /// <returns> True if at least one solution was found</returns>
        private bool BuildGraph(Node parent, Heap<Node> leaves, HashSet<GoapAction> usableActions, HashSet<KeyValuePair<string, object>> goal)
        {
            var foundOne = false;

            // go through each action available at this node and see if we can use it here
            foreach (var action in usableActions)
            {
                // if the parent state don't have the conditions for this action's preconditions, we cannot use it here
                if (InState(action.Preconditions, parent.State) == false) continue;

                // apply the action's effects to the parent state
                var currentState = PopulateState(parent.State, action.Effects);
                
                var node = new Node(parent, parent.RunningCost + action.Cost, currentState, action);

                if (InState(goal, currentState))
                {
                    // we found a solution!
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    // not at a solution yet, so test all the remaining actions and branch out the tree
                    var subset = ActionSubset(usableActions, action);
                    var found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                        foundOne = true;
                }
            }

            return foundOne;
        }

        /// <summary>
        /// Create a subset of the actions excluding the removeMe one. Creates a new set.
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="removeMe"></param>
        /// <returns></returns>
        private HashSet<GoapAction> ActionSubset(IEnumerable<GoapAction> actions, GoapAction removeMe)
        {
            var subset = new HashSet<GoapAction>(actions);
            subset.Remove(removeMe);

            return subset;
        }

        /// <summary>
        /// Check that all items in 'test' exist in 'state'. If just one does not match or is not there then this returns false.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool InState(IEnumerable<KeyValuePair<string, object>> test, IEnumerable<KeyValuePair<string, object>> state)
        {
            return test.All(state.Contains);
        }

        /// <summary>
        /// Apply the stateChange to the currentState
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="stateChange"></param>
        /// <returns></returns>
        private HashSet<KeyValuePair<string, object>> PopulateState(IEnumerable<KeyValuePair<string, object>> currentState, IEnumerable<KeyValuePair<string, object>> stateChange)
        {
            // copy the KVPs over as new objects
            var state = new HashSet<KeyValuePair<string, object>>(currentState);

            foreach (var change in stateChange)
            {
                // if the key exists in the current state, update the Value
                if (state.Contains(change))
                {
                    var change1 = change;
                    state.RemoveWhere(kvp => kvp.Key.Equals(change1.Key));
                    var updated = new KeyValuePair<string, object>(change.Key, change.Value);
                    state.Add(updated);
                }
                // if it does not exist in the current state, add it
                else
                    state.Add(new KeyValuePair<string, object>(change.Key, change.Value));
            }
            return state;
        }
    }
}
