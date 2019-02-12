using System;

namespace DefaultTeam.Pathfinding.Scripts.Pathfinding
{
    /// <summary>
    /// Represents a result from the pathfinding
    /// </summary>
    public struct PathResult
    {
        /// <summary>
        /// The path
        /// </summary>
        private readonly Node[] _path;
        /// <summary>
        /// True if the pathfinding was successful
        /// </summary>
        private readonly bool _success;
        /// <summary>
        /// The callback
        /// </summary>
        private readonly Action<Node[], bool> _callback;
        
        public PathResult(Node[] path, bool success, Action<Node[], bool> callback) 
        {
            _path = path;
            _success = success;
            _callback = callback;
        }

        public void CallCalback()
        {
            if (_callback == null) return;

            _callback(_path, _success);
        }
    }
}
