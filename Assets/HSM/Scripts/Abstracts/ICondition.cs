namespace HSM.Scripts.Abstracts
{
    /// <summary>
    /// The condition interface offsets the problem of whether
    /// transitions should fire by having a separate set of condition
    /// instances that can be combined together with boolean operators.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Performs the test for this condition.
        /// </summary>
        /// <returns></returns>
        bool Test(object watch);

        string ToString();
    }
}