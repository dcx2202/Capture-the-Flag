using System.Collections.Generic;
using HSM.Scripts.Abstracts;

namespace HSM.Scripts
{
    /// <summary>
    /// General implementation of a singular state. 
    /// </summary>
    public class State : IState
    {
        /// <summary>
        /// Name of this sate
        /// </summary>
        public string Name { get; set; }
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
        public IEnumerable<IState> States
        {
            get
            {
                return new List<IState>{this};
            }
        }

        /// <summary>
        /// State that parents this state.
        /// </summary>
        public IState Parent { get; set; }

        /// <summary>
        /// <para>Returns ActiveActions if implemented in a State.</para>
        /// <para>Recursively updates the sub state machine and returns all of its relevant actions 
        /// (may be active, transition, entry or exit actions)</para>
        /// </summary>
        public UpdateResult Update()
        {
            var result = new UpdateResult();
            result.AddAction(ActiveActions);

            return result;
        }

        /// <summary>
        /// Recurses up the parent hierarchy, transitioning into each state in turn for hte given number of levels
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAction> UpdateDown(IState state, int level)
        {
            throw new System.NotImplementedException("This is not suposed to be called on a State concrete.");
        }

        public State(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}