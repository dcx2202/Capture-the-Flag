using General_Scripts.AI.GOAP;
using HSM.Scripts;
using HSM.Scripts.Abstracts;
using UnityEngine;

namespace General_Scripts.AI.HSM.Actions
{
    /// <summary>
    /// Active action of the Acting State of the <see cref="HierarchicalStateMachine"/>.
    /// </summary>
    public class PerformActionAction : IAction
    {
        /// <summary>
        /// The <see cref="GoapAgent"/> that will act
        /// </summary>
        private readonly GoapAgent _agent;
        /// <summary>
        /// The data provider of the <see cref="GoapAgent"/>
        /// </summary>
        private readonly IGoap _dataProvider;

        public PerformActionAction(GoapAgent agent, IGoap dataProvider)
        {
            _agent = agent;
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Implementation of <see cref="IAction"/> interface. Executes the <see cref="GoapAction"/> in the <see cref="GoapAgent"/> current plan.
        /// </summary>
        public void Execute()
        {
            if (_agent.NeedNewPlan) // there is no more actions in the current plan, needs a new plan
            {
                Debug.Log("<color=red>Done actions</color>");
                _dataProvider.ActionsFinished();
                return;
            }

            // check if the current action has finish its execution
            var currentActions = _agent.GetCurrentActions();
            var action = currentActions.Peek();
            if (action.IsDone())
            {
                // the action is done. Remove it so we can perform the next one
                var currAction = currentActions.Dequeue();

                GoapAction nextAction = null;
                if (currentActions.Count > 0)
                    nextAction = currentActions.Dequeue();

                _dataProvider.CurrentActionFinished(currAction, nextAction); // todo
            }

            if (_agent.NeedNewPlan == false) // in case the previous action was complete, we need to check again if we stil have a plan
            {
                // perform the next action
                action = currentActions.Peek();

                // check if we are in range of the next action or if we need to move
                var inRange = action.RequiresInRange() == false || action.InRange;
                if (inRange)
                {
                    // we are in range, so perform the action
                    var success = action.Perform(_agent.gameObject);

                    if (success) return;

                    // soemthing went wrong
                    // action failed, we need to plan again
                    currentActions.Clear(); // we need a new plan
                    _dataProvider.PlanAborted(action); // call plan aborted to perform clean up if required
                }
                else // we need to move
                    _dataProvider.MoveAgent(action);
            }
            else // _agent.NeedNewPlan == true
            {
                // all actions are completed. Perform clean up code of the current plan. No need to change state, since the NeedNewPlanCondition will be true every time the NeedNewPlan property is true
                _dataProvider.ActionsFinished();
            }
        }
    }
}
