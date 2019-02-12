using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    /// <summary>
    /// Represents a patrol in the pathfinding grid. Modifies the costs arround it
    /// </summary>
    public class Patrol : MonoBehaviour
    {
        /// <summary>
        /// This object last position
        /// </summary>
        private Vector3 _previousPosition;

        /// <summary>
        /// How much this object must move before updating the grid
        /// </summary>
        [Tooltip("How much this object must move before updating the grid")]
        public float UpdateThreashold;
        /// <summary>
        /// How large is the influence radius of this patrol
        /// </summary>
        [Tooltip("How large is the influence radius of this patrol")]
        public int InfluenceSize;
        /// <summary>
        /// How big is the penalty it adds
        /// </summary>
        [Tooltip("How big is the penalty it adds")]
        public float Cost;

        /// <summary>
        /// Reference to the grid in this scene
        /// </summary>
        private Grid _grid;

        private void Start()
        {
            _previousPosition = transform.position;
            _grid = FindObjectOfType<Grid>();
            if (_grid == null) return;

            _grid.UpdateSurroundingInfluence(_grid.NodeFromWorldPoint(_previousPosition), Cost, InfluenceSize);
        }

        private void Update()
        {
            if (_grid == null)
            {
                Debug.LogError("You need a grid in you scene to use this script!");
                return;
            }

            // if we have moved enough, update the grid
            if ((_previousPosition - transform.position).sqrMagnitude > UpdateThreashold * UpdateThreashold)
            {
                _grid.UpdateSurroundingInfluence(_grid.NodeFromWorldPoint(_previousPosition), -Cost, InfluenceSize);
                _grid.UpdateSurroundingInfluence(_grid.NodeFromWorldPoint(transform.position), Cost, InfluenceSize);
                _previousPosition = transform.position;
            }
        }

        private void OnDestroy()
        {
            if(_grid != null)
                _grid.UpdateSurroundingInfluence(_grid.NodeFromWorldPoint(_previousPosition), -Cost, InfluenceSize);
        }
    }
}
