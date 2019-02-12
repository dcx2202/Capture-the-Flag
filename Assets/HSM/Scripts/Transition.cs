using System.Collections.Generic;
using HSM.Scripts.Abstracts;

namespace HSM.Scripts
{
    /// <summary>
    /// General implementation of a transition
    /// </summary>
    public class Transition : ITransition
    {
        /// <summary>
        /// Condition that must be fufilled for this transition occur.
        /// </summary>
        public ICondition Condition { get; private set; }
        /// <summary>
        /// Return the differene in levels of the hierarchy from the source to the target of the transition
        /// </summary>
        /// <returns></returns>
        public int Level { get; private set; }
        /// <summary>
        /// List of Actions to be Executed when this transition is triggered
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAction> Actions { get; private set; }
        /// <summary>
        /// Gets the target State of this Transition
        /// </summary>
        /// <returns></returns>
        public IState TargetState { get; private set; }
        /// <summary>
        /// The object that will be used in the test condition
        /// </summary>
        public object Watch { get; private set; }
        /// <summary>
        /// Returns true when this transition is triggered
        /// </summary>
        public bool IsTriggered { get { return Condition.Test(Watch); } }
        /// <summary>
        /// Transition name
        /// </summary>
        public string Name { get; set; }


        public Transition(int level, IEnumerable<IAction> actions, IState targetState, string name, ICondition condition, object watch)
        {
            Level = level;
            Actions = actions;
            TargetState = targetState;
            Name = name;
            Condition = condition;
            Watch = watch;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}