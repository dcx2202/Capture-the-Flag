using General_Scripts.AI.GOAP;
using HSM.Scripts;
using HSM.Scripts.Abstracts;

namespace General_Scripts.AI.HSM.Conditions
{
    /// <summary>
    /// <see cref="ICondition"/> used by the <see cref="HierarchicalStateMachine"/>. Returns true of the <see cref="GoapAgent"/> of this state machine needs a new plan
    /// </summary>
    public class NeedNewPlanCondition : ICondition
    {
        /// <summary>
        /// Returns true of the <see cref="GoapAgent"/> of this state machine needs a new plan
        /// </summary>
        /// <param name="watch"></param>
        /// <returns></returns>
        public bool Test(object watch)
        {
            return ((GoapAgent) watch).NeedNewPlan;
        }
    }
}
