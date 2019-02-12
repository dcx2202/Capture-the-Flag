namespace HSM.Scripts.Abstracts
{
    /// <summary>
    /// Interface that represents an action for the <see cref="HierarchicalStateMachine"/>
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Execute the action!
        /// </summary>
        void Execute();
    }
}