using System.Collections.Generic;

namespace HSM.Scripts.Abstracts
{
    public interface IState
    {
        /// <summary>
        /// List of Actions to be execute while the state is active
        /// </summary>
        IEnumerable<IAction> ActiveActions { get; }
        /// <summary>
        /// List of Actions to be Executed when this state is entered
        /// </summary>
        IEnumerable<IAction> EntryActions { get; }

        /// <summary>
        /// List of Actions to be Executed when this state is exited
        /// </summary>
        IEnumerable<IAction> ExitActions { get; }

        /// <summary>
        /// List of all outgoing transitions of this state
        /// </summary>
        IEnumerable<ITransition> Transitions { get;  }

        /// <summary>
        /// States that belongs to this state. Used to build a Hierarchical State Machine. If any state of a statemachine has states, then it is an multi level state machine.
        /// </summary>
        IEnumerable<IState> States { get; }
        /// <summary>
        /// Name of this sate
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// State that parents this state.
        /// </summary>
        IState Parent { get; }

        /// <summary>
        /// <para>Returns ActiveActions if implemented in a State.</para>
        /// <para>Recursively updates the sub state machine and returns all of its relevant actions 
        /// (may be active, transition, entry or exit actions)</para>
        /// </summary>
        UpdateResult Update();

        /// <summary>
        /// Recurses up the parent hierarchy, transitioning into each state in turn for hte given number of levels
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAction> UpdateDown(IState state, int level);


        string ToString();
    }
}