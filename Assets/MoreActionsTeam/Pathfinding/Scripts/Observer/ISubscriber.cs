using System.Collections.Generic;

namespace ThunderRunners.Pathfinding.Scripts.Observer
{
    /// <summary>
    /// Simple observer abstraction used in the pathfinding
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// List of listeners subscribed to this subscriber
        /// </summary>
        HashSet<IListener> Listeners { get; set; }

        /// <summary>
        /// Add new listener
        /// </summary>
        /// <param name="listener"></param>
        void RegisterListener(IListener listener);
        /// <summary>
        /// Remove listener
        /// </summary>
        /// <param name="listener"></param>
        void RemoveListener(IListener listener);
        /// <summary>
        /// Notify all listeners of modifications
        /// </summary>
        void Notify();
    }
}
