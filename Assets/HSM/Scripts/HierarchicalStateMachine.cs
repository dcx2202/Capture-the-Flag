using System.Collections.Generic;
using System.Linq;
using HSM.Scripts.Abstracts;

namespace HSM.Scripts
{
    /// <summary>
    /// General implementation of the <see cref="IState"/> interface to build an Hierarchical State Machine.
    /// </summary>
    public class HierarchicalStateMachine : IState
    {
        /// <summary>
        /// List of Actions to be execute while the state is active
        /// </summary>
        public IEnumerable<IAction> ActiveActions { get; set; }
        /// <summary>
        /// List of Actions to be Executed when this state is entered
        /// </summary>
        public IEnumerable<IAction> EntryActions { get; set; }
        /// <summary>
        /// List of Actions to be Executed when this state is exited
        /// </summary>
        public IEnumerable<IAction> ExitActions { get; set; }
        /// <summary>
        /// List of all outgoing transitions of this state
        /// </summary>
        public IEnumerable<ITransition> Transitions { get; set; }
        /// <summary>
        /// States that belongs to this state. Used to build a Hierarchical State Machine. If any state of a statemachine has states, then it is an multi level state machine.
        /// </summary>
        public IEnumerable<IState> States { get; set; }
        /// <summary>
        /// Name of this sate
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// State that parents this state.
        /// </summary>
        public IState Parent { get; set; }
        /// <summary>
        /// Starting point of the machine
        /// </summary>
        public IState InitState { get; set; }
        /// <summary>
        /// Current state of the machine
        /// </summary>
        public IState CurState { get; set; }

        public HierarchicalStateMachine(IState init, string name)
        {
            InitState = init;
            Name = name;
            
        }

        /// <summary>
        /// <para>Returns ActiveActions if implemented in a State.</para>
        /// <para>Recursively updates the sub state machine and returns all of its relevant actions 
        /// (may be active, transition, entry or exit actions)</para>
        /// </summary>
        public UpdateResult Update()
        {
            var result = new UpdateResult();
            // if we are in no state, use the initial state
            if (CurState == null)
            {
                CurState = InitState;
                result = new UpdateResult(CurState.EntryActions);
                return result;
            }

            // if we have a state, then we need to check if there is a valid transition
            var triggerTransition = CurState.Transitions.FirstOrDefault(curStateTransition => curStateTransition.IsTriggered); // can be null
            
            // if there is a valid transition, create a UpdateResult for it
            if (triggerTransition != null)
            {
                result.Transition = triggerTransition;
                result.Level = triggerTransition.Level;
            }
            // otherwise, recurse down the state machine hierarchy for a result
            else
            {
                result = CurState.Update();
            }

            // Check if result contains a transition
            if (result.Transition != null)
            {
                // The transition occurs on this level
                if (result.Level == 0)
                {
                    // get the target state
                    var targetState = result.Transition.TargetState;

                    // add all actions for this transition
                    result.AddAction(CurState.ExitActions);
                    result.AddAction(result.Transition.Actions);
                    result.AddAction(targetState.EntryActions);

                    // update the current state
                    CurState = targetState;

                    // add our own actions to the result
                    result.AddAction(ActiveActions);

                    // clear the transition so it doesn't trigger again
                    result.Transition = null;
                }
                else if (result.Level > 0) // if the transition must be done at a higher level
                {
                    // get exit actions and exit current state
                    result.AddAction(CurState.ExitActions);
                    CurState = null;

                    // decrement the number of levels to go
                    result.Level--;
                }
                else // the transition must be done at a lower level
                {
                    var targetState = result.Transition.TargetState;
                    var targetMachine = targetState.Parent;
                    result.AddAction(result.Transition.Actions);
                    result.AddAction(targetMachine.UpdateDown(targetState, -result.Level));

                    // clear the transition so it doesn't trigger again
                    result.Transition = null;
                }
            }
            // if there is no transition
            else
            {
                result.AddAction(ActiveActions);
            }

            return result;
        }

        /// <summary>
        /// Recurses up the parent hierarchy, transitioning into each state in turn for hte given number of levels
        /// </summary>
        public IEnumerable<IAction> UpdateDown(IState state, int level)
        {
            var actions = new List<IAction>();
            // if we are not at top level, continue recursing
            if (level > 0)
                actions = Parent.UpdateDown(this, --level).ToList();
            
            if (CurState != null)
                actions.AddRange(CurState.ExitActions);

            CurState = state;
            actions.AddRange(state.EntryActions);

            return actions;
        }

        /// <summary>
        /// Get the current state stack
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IState> GetStates()
        {
            // if there is a current state, then returns that state (it can be a sub state machine)
            if (CurState != null)
                return CurState.States;

            // if there is no current state, then there is no stack
            return new List<IState>();
        }
    }
}