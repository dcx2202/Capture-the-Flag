using System.Collections.Generic;
using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    public class Grid : MonoBehaviour
    {
        /// <summary>
        /// Display the grid as gizmos
        /// </summary>
        [Tooltip("Display the grid as gizmos")]
        public bool DisplayGridGizmos;
        /// <summary>
        /// Walkable regions and their associated costs
        /// </summary>
        [Tooltip("Walkable regions and their associated costs")]
        public TerrainType[] WalkableRegions;
        /// <summary>
        /// Penalty cost for being close to an obstacle.
        /// </summary>
        [Tooltip("Penalty cost for being close to an obstacle.")]
        public int ObstacleProximityPenalty = 10;
        /// <summary>
        /// Walkable layer mask
        /// </summary>
        private LayerMask _walkableMask;
        /// <summary>
        /// Dictionary with all walkable areas
        /// </summary>
        private readonly Dictionary<int, int> _walkableRegionsDictionary = new Dictionary<int, int>();

        /// <summary>
        /// The Unwalkable LayberMask, used to define which are the layers that are not walkable
        /// </summary>
        [Tooltip("The Unwalkable LayberMask, used to define which are the layers that are not walkable")]
        public LayerMask UnwalkableMask;
        /// <summary>
        /// The area that this grid is going to cover in the world.
        /// </summary>
        [Tooltip("The area that this grid is going to cover in the world.")]
        public Vector2 GridWorldSize;
        /// <summary>
        /// The space each individual node covers
        /// </summary>
        [Tooltip("The space each individual node covers")]
        public float NodeRadius;
        
        /// <summary>
        /// The grid array
        /// </summary>
        private static Node[,] _grid;

        /// <summary>
        /// The diameter of a node
        /// </summary>
        private float _nodeDiameter;
        /// <summary>
        /// number of horizontal nodes in the grid
        /// </summary>
        private int _gridSizeX;
        /// <summary>
        /// number of vertical nodes in the grid
        /// </summary>
        private int _gridSizeY;
        /// <summary>
        /// The lowest movement penalty in the grid
        /// </summary>
        private int _penaltyMin = int.MaxValue;
        /// <summary>
        /// The highest movement penalty in the grid
        /// </summary>
        private int _penaltyMax = int.MinValue;

        /// <summary>
        /// The maximum size of the grid
        /// </summary>
        public int MaxSize {  get { return _gridSizeX * _gridSizeY; } }


        private void Awake()
        {
            _nodeDiameter = NodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter); // Round to the closes int
            _gridSizeY = Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter); // Round to the closes int

            foreach (var region in WalkableRegions)
            {
                _walkableMask.value |= region.TerrainMask.value;
                _walkableRegionsDictionary.Add((int)Mathf.Log(region.TerrainMask.value, 2), region.TerrainPenalty);
            }
            CreateGrid();
        }
        
        /// <summary>
        /// Creates a grid for this scene
        /// </summary>
        private void CreateGrid()
        {
            //instantiate the grid
            _grid = new Node[_gridSizeX, _gridSizeY];

            // the bottom left world position for the grid
            var worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.forward * GridWorldSize.y / 2;

            for (var x = 0; x < _gridSizeX; x++)
            {
                for (var y = 0; y < _gridSizeY; y++)
                {
                    // the position of this node
                    var worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) + Vector3.forward * (y * _nodeDiameter + NodeRadius);
                    // check if it is a walkable node or not. This is done by checking collisions in a sphere, centered in the worldPoint with radios equals to NodeRadius
                    var walkable = Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask) == false;

                    var movementPenalty = 0;

                    // ray cast to find the layer
                    var ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, _walkableMask))
                        _walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

                    if (walkable == false)
                        movementPenalty += ObstacleProximityPenalty;

                    //print("walkable: " + walkable);
                    // add the node to the grid
                    _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }
            }

            BlurPenaltyMap(2);
        }

        /// <summary>
        /// Blurs the map to add more realistic paths. check https://www.youtube.com/watch?v=Tb-rM3wGwv4 for more information
        /// </summary>
        /// <param name="blurSize"></param>
        private void BlurPenaltyMap(int blurSize)
        {
            var kernelSize = blurSize * 2 + 1;
            var kernelExtents = (kernelSize - 1) / 2;  // how many squares are there between the middle of the kernal and the extremeties of the kernal

            var penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            var penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            // horizontal pass
            for (var y = 0; y < _gridSizeY; y++)
            {
                for (var x = -kernelExtents; x <= kernelExtents; x++) // loops through the kernel extendes horizontally
                {
                    var sampleX = Mathf.Clamp(x, 0, kernelExtents); // clamps X to zero if it is negative
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty; // add the movement penalty of the current node to the horizontal pass matrix
                }

                for (var x = 1; x < _gridSizeX; x++)
                {
                    var removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX - 1); // the index of the node that is no longer ins ide the kernel
                    var addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1); // the index of the kerner that has entered the kernel

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -         // previous value
                                                    _grid[removeIndex, y].MovementPenalty +     // remove the value that there is no longer inside the kernel
                                                    _grid[addIndex, y].MovementPenalty;         // add the new value that is in the kernel
                }
            }

            // vertical pass
            for (var x = 0; x < _gridSizeX; x++)
            {
                for (var y = -kernelExtents; y <= kernelExtents; y++) // loops through the kernel extendes horizontally
                {
                    var sampleY = Mathf.Clamp(y, 0, kernelExtents); // clamps X to zero if it is negative
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY]; // add the movement penalty of the current node to the horizontal pass matrix
                }

                //set on the first row
                var blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                _grid[x, 0].MovementPenalty = blurredPenalty;

                // set for the other rows
                for (var y = 1; y < _gridSizeY; y++)
                {
                    var removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY); // the index of the node that is no longer ins ide the kernel
                    var addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1); // the index of the kerner that has entered the kernel

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] -         // previous value
                                                  penaltiesHorizontalPass[x, removeIndex] +     // remove the value that there is no longer inside the kernel
                                                  penaltiesHorizontalPass[x, addIndex];         // add the new value that is in the kernel
                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    _grid[x, y].MovementPenalty = blurredPenalty;

                    UpdatePenalties(blurredPenalty);
                }
            }
        }

        /// <summary>
        /// Updates the maxiumum and minimal penalties
        /// </summary>
        /// <param name="blurredPenalty"></param>
        private void UpdatePenalties(int blurredPenalty)
        {
            if (blurredPenalty > _penaltyMax)
                _penaltyMax = blurredPenalty;
            if (blurredPenalty < _penaltyMin)
                _penaltyMin = blurredPenalty;
        }

        /// <summary>
        /// Modifies the movement penalties arround the received node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cost">positive cost to add, negative cost to remove<para> final penalty = ((influenceSize + 1) * 10) - (dist * cost)</para></param>
        /// <param name="influenceSize">final penalty = ((influenceSize + 1) * 10) - (dist * cost)</param>
        public void UpdateSurroundingInfluence(Node node, float cost, int influenceSize)
        {
            var neighbours = GetNeighbours(node, influenceSize);
            var maximumPenalty = (influenceSize + 1 ) * 10 * cost;

            foreach (var neighbour in neighbours)
            {
                var penalty = Mathf.RoundToInt(maximumPenalty - AStar.GetDistance(node, neighbour) * cost);
                neighbour.MovementPenalty += cost > 0 && penalty < 0 || cost < 0 && penalty > 0 ? 0 : penalty;
                UpdatePenalties(penalty);
            }
            if(PathRequestManager.Instance != null)
                PathRequestManager.Instance.Notify();
        }

        /// <summary>
        /// Get all the neighbours of the received node
        /// </summary>
        public IEnumerable<Node> GetNeighbours(Node node, int depth=1)
        {
            var neighbours = new List<Node>();

            // searches for nodes in a 3x3 square
            for (var x = -depth; x <= depth; x++)
            {
                for (var y = -depth; y <= depth; y++)
                {
                    // center of the square is the received node. skip it
                    if (x == 0 && y == 0) 
                        continue;

                    var checkX = node.GridX + x;
                    var checkY = node.GridY + y;

                    // if check variables are inside the grid, add it ot the neighbours list
                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY) 
                        neighbours.Add(_grid[checkX, checkY]);
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Gets a node from the grid, from a Vector3 World Position
        /// </summary>
        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            var percentX = (worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x; //from 0 to 1
            var percentY = (worldPosition.z + GridWorldSize.y / 2) / GridWorldSize.y; //from 0 to 1
            percentX = Mathf.Clamp01(percentX); // clamp overflows
            percentY = Mathf.Clamp01(percentY); // clamp overflows

            var x = Mathf.RoundToInt((_gridSizeX - 1) * percentX); // get the x index
            var y = Mathf.RoundToInt((_gridSizeY - 1) * percentY); // get the y index
            return _grid[x, y];
        }

        /// <summary>
        /// Draw the gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            // this allow draws the while wire cube in the scene view, to allow a better resize and positioning of the grid position
            Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, 1, GridWorldSize.y));

           
            if (_grid != null && DisplayGridGizmos)
            {
                foreach (var node in _grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, node.MovementPenalty <= 0 ? 0 :
                        Mathf.InverseLerp(_penaltyMin, _penaltyMax, node.MovementPenalty));
                    Gizmos.color = (node.Walkable) ? Gizmos.color : Color.red;

                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter));
                }
            }
        }
    }
}