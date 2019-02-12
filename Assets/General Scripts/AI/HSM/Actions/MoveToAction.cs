using General_Scripts.AI.GOAP;
using HSM.Scripts;
using HSM.Scripts.Abstracts;
using UnityEngine;

namespace General_Scripts.AI.HSM.Actions
{
    /// <summary>
    /// Active action of the Moving State of the <see cref="HierarchicalStateMachine"/>.
    /// </summary>
    public class MoveToAction : IAction
    {
        /// <summary>
        /// The <see cref="GoapAgent"/> that is moving
        /// </summary>
        private readonly GoapAgent _agent;
        /// <summary>
        /// The data provider of the <see cref="GoapAgent"/>
        /// </summary>
        private readonly IGoap _dataProvider;

        public MoveToAction(GoapAgent agent, IGoap dataProvider)
        {
            _agent = agent;
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Implementation of <see cref="IAction"/> interface. Calls the MoveAgent of the <see cref="IGoap"/> interface.
        /// </summary>
        public void Execute()
        {
            // get the action we need to move to
            var action = _agent.PeekNextAction();
            // get the agent to move itself
            _dataProvider.MoveAgent(action);
            if (action.RequiresInRange() == false || action.Target != null) return;

            // something went wrong. we need a new plan
            Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");

            _agent.GetCurrentActions().Clear(); // set state to PlanState
        }
    }
}
