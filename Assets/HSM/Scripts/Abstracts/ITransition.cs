using System.Collections.Generic;

namespace HSM.Scripts.Abstracts
{
    public interface ITransition
    {
        /// <summary>
        /// Condition that must be fufilled for this transition occur.
        /// </summary>
        ICondition Condition { get; }

        /// <summary>
        /// Return the differene in levels of the hierarchy from the source to the target of the transition
        /// </summary>
        /// <returns></returns>
        int Level { get; }

        /// <summary>
        /// List of Actions to be Executed when this transition is triggered
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAction> Actions { get; }

        /// <summary>
        /// Gets the target State of this Transition
        /// </summary>
        /// <returns></returns>
        IState TargetState { get; }

        /// <summary>
        /// Returns true when this transition is triggered
        /// </summary>
        bool IsTriggered { get; }

        /// <summary>
        /// Used as an identifier
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The object that will be used in the test condition
        /// </summary>
        object Watch { get; }

        string ToString();
    }
}