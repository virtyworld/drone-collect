/// <summary>
/// A* Pathfinding implementation for 3D space navigation.
/// This class handles pathfinding for drones, creating a 3D grid of nodes and finding optimal paths
/// while avoiding obstacles. It includes collision detection and supports both resource searching
/// and base return behaviors.
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class AStarPathfinding : MonoBehaviour
{
    [SerializeField] private LayerMask unwalkableMask;
    [SerializeField] private bool showDebugGrid = true;
    public bool HasReachedDestination { get; private set; } = false;
    public bool IsSearchingForResource { get; private set; } = false;

    private class Node
    {
        public Vector3 position;
        public float gCost;
        public float hCost;
        public float fCost { get { return gCost + hCost; } }
        public Node parent;
        public bool walkable;

        public Node(Vector3 pos, bool walkable = true)
        {
            position = pos;
            this.walkable = walkable;
            gCost = float.MaxValue;
            hCost = 0;
            parent = null;
        }
    }

    private bool isInitialized = false;
    private float nodeRadius = 0.5f;
    private float nodeDiameter;
    private Vector3 gridWorldSize;
    private Node[,,] grid;
    private int gridSizeX, gridSizeY, gridSizeZ;

    /// <summary>
    /// Initializes the grid dimensions and node size
    /// </summary>
    private void Awake()
    {
        InitializeGridParameters();
    }

    /// <summary>
    /// Sets up the initial grid parameters
    /// </summary>
    private void InitializeGridParameters()
    {
        nodeDiameter = nodeRadius * 2;
        gridWorldSize = new Vector3(40f, 30f, 40f);
    }

    /// <summary>
    /// Creates a 3D grid of nodes around the drone's position
    /// </summary>
    private void CreateGrid(Transform droneTransform)
    {
        CalculateGridDimensions();
        InitializeGridArray();
        PopulateGrid(droneTransform);
    }

    /// <summary>
    /// Calculates the dimensions of the grid based on world size
    /// </summary>
    private void CalculateGridDimensions()
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
    }

    /// <summary>
    /// Initializes the 3D grid array
    /// </summary>
    private void InitializeGridArray()
    {
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
    }

    /// <summary>
    /// Populates the grid with nodes and checks their walkability
    /// </summary>
    private void PopulateGrid(Transform droneTransform)
    {
        Vector3 worldBottomLeft = CalculateGridOrigin();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = CalculateNodePosition(worldBottomLeft, x, y, z);
                    bool walkable = IsWalkable(droneTransform, worldPoint);
                    grid[x, y, z] = new Node(worldPoint, walkable);
                }
            }
        }
    }

    /// <summary>
    /// Calculates the bottom-left corner position of the grid
    /// </summary>
    private Vector3 CalculateGridOrigin()
    {
        return transform.position - Vector3.right * gridWorldSize.x / 2
                                 - Vector3.up * gridWorldSize.y / 2
                                 - Vector3.forward * gridWorldSize.z / 2;
    }

    /// <summary>
    /// Calculates the world position for a node at given grid coordinates
    /// </summary>
    private Vector3 CalculateNodePosition(Vector3 worldBottomLeft, int x, int y, int z)
    {
        return worldBottomLeft +
               Vector3.right * (x * nodeDiameter + nodeRadius) +
               Vector3.up * (y * nodeDiameter + nodeRadius) +
               Vector3.forward * (z * nodeDiameter + nodeRadius);
    }

    /// <summary>
    /// Checks if a position is walkable for the drone
    /// </summary>
    private bool IsWalkable(Transform droneTransform, Vector3 position)
    {
        if (droneTransform == null) return false;

        Vector2 droneDimensions = GetDroneDimensions(droneTransform);
        return CheckHorizontalCollisions(position, droneDimensions.x) &&
               CheckVerticalCollisions(position, droneDimensions);
    }

    /// <summary>
    /// Gets the dimensions of the drone
    /// </summary>
    private Vector2 GetDroneDimensions(Transform droneTransform)
    {
        float droneRadius = Mathf.Max(droneTransform.localScale.x, droneTransform.localScale.z) * 0.5f;
        float droneHeight = droneTransform.localScale.y;
        return new Vector2(droneRadius, droneHeight);
    }

    /// <summary>
    /// Checks for horizontal collisions at a position
    /// </summary>
    private bool CheckHorizontalCollisions(Vector3 position, float droneRadius)
    {
        return !Physics.CheckSphere(position, droneRadius, unwalkableMask);
    }

    /// <summary>
    /// Checks for vertical collisions at a position
    /// </summary>
    private bool CheckVerticalCollisions(Vector3 position, Vector2 droneDimensions)
    {
        Vector3 topPoint = position + Vector3.up * droneDimensions.y;
        if (Physics.CheckSphere(topPoint, droneDimensions.x, unwalkableMask))
        {
            return false;
        }

        return CheckVerticalSpace(position, droneDimensions);
    }

    /// <summary>
    /// Checks if there's enough vertical space for the drone
    /// </summary>
    private bool CheckVerticalSpace(Vector3 position, Vector2 droneDimensions)
    {
        RaycastHit[] hits = Physics.SphereCastAll(position, droneDimensions.x, Vector3.up, droneDimensions.y, unwalkableMask);
        return hits.Length == 0;
    }

    /// <summary>
    /// Converts a world position to grid coordinates
    /// </summary>
    private Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;
        Vector3 percentPosition = CalculatePercentPosition(localPosition);
        Vector3Int gridPosition = CalculateGridPosition(percentPosition);
        return GetNodeAtGridPosition(gridPosition);
    }

    /// <summary>
    /// Calculates the percentage position within the grid
    /// </summary>
    private Vector3 CalculatePercentPosition(Vector3 localPosition)
    {
        return new Vector3(
            (localPosition.x + gridWorldSize.x / 2) / gridWorldSize.x,
            (localPosition.y + gridWorldSize.y / 2) / gridWorldSize.y,
            (localPosition.z + gridWorldSize.z / 2) / gridWorldSize.z
        );
    }

    /// <summary>
    /// Calculates the grid position from percentage position
    /// </summary>
    private Vector3Int CalculateGridPosition(Vector3 percentPosition)
    {
        return new Vector3Int(
            Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentPosition.x), 0, gridSizeX - 1),
            Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentPosition.y), 0, gridSizeY - 1),
            Mathf.Clamp(Mathf.RoundToInt((gridSizeZ - 1) * percentPosition.z), 0, gridSizeZ - 1)
        );
    }

    /// <summary>
    /// Gets the node at the specified grid position
    /// </summary>
    private Node GetNodeAtGridPosition(Vector3Int gridPosition)
    {
        return grid[gridPosition.x, gridPosition.y, gridPosition.z];
    }

    /// <summary>
    /// Gets all walkable neighboring nodes for a given node
    /// </summary>
    private List<Node> GetNeighbors(Node node)
    {
        Vector3Int nodePosition = FindNodePosition(node);
        if (nodePosition.x == -1) return new List<Node>();

        return GetWalkableNeighbors(nodePosition);
    }

    /// <summary>
    /// Finds the grid position of a node
    /// </summary>
    private Vector3Int FindNodePosition(Node node)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    if (grid[x, y, z] == node)
                    {
                        return new Vector3Int(x, y, z);
                    }
                }
            }
        }
        return new Vector3Int(-1, -1, -1);
    }

    /// <summary>
    /// Gets all walkable neighbors for a node at the given position
    /// </summary>
    private List<Node> GetWalkableNeighbors(Vector3Int nodePosition)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;

                    Vector3Int checkPosition = nodePosition + new Vector3Int(x, y, z);
                    if (IsValidGridPosition(checkPosition))
                    {
                        Node neighbor = grid[checkPosition.x, checkPosition.y, checkPosition.z];
                        if (neighbor.walkable)
                        {
                            neighbors.Add(neighbor);
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Checks if a grid position is valid
    /// </summary>
    private bool IsValidGridPosition(Vector3Int position)
    {
        return position.x >= 0 && position.x < gridSizeX &&
               position.y >= 0 && position.y < gridSizeY &&
               position.z >= 0 && position.z < gridSizeZ;
    }

    /// <summary>
    /// Finds a path from the drone's current position to the target position
    /// </summary>
    public List<Vector3> FindPath(Transform droneTransform, Vector3 targetPos, bool isReturningToBase = false)
    {
        if (!isInitialized)
        {
            CreateGrid(droneTransform);
            isInitialized = true;
        }

        HasReachedDestination = false;
        Node startNode = NodeFromWorldPoint(droneTransform.position);
        Node targetNode = NodeFromWorldPoint(targetPos);

        if (!ValidateNodes(startNode, targetNode))
        {
            return null;
        }

        return CalculatePath(startNode, targetNode, isReturningToBase);
    }

    /// <summary>
    /// Validates the start and target nodes
    /// </summary>
    private bool ValidateNodes(Node startNode, Node targetNode)
    {
        if (!startNode.walkable || !targetNode.walkable)
        {
            Debug.LogWarning("Start or target node is not walkable!");
            IsSearchingForResource = true;
            return false;
        }
        IsSearchingForResource = false;
        return true;
    }

    /// <summary>
    /// Calculates the path using A* algorithm
    /// </summary>
    private List<Vector3> CalculatePath(Node startNode, Node targetNode, bool isReturningToBase)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.position, targetNode.position);

        int iterations = 0;
        const int maxIterations = 1000;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Node currentNode = GetBestNode(openSet);
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                HasReachedDestination = true;
                return RetracePath(startNode, targetNode);
            }

            ProcessNeighbors(currentNode, targetNode, openSet, closedSet, isReturningToBase);
        }

        LogPathfindingFailure(iterations, openSet.Count, closedSet.Count);
        return null;
    }

    /// <summary>
    /// Gets the best node from the open set
    /// </summary>
    private Node GetBestNode(List<Node> openSet)
    {
        Node bestNode = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < bestNode.fCost ||
                (openSet[i].fCost == bestNode.fCost && openSet[i].hCost < bestNode.hCost))
            {
                bestNode = openSet[i];
            }
        }
        return bestNode;
    }

    /// <summary>
    /// Processes neighboring nodes for pathfinding
    /// </summary>
    private void ProcessNeighbors(Node currentNode, Node targetNode, List<Node> openSet, HashSet<Node> closedSet, bool isReturningToBase)
    {
        foreach (Node neighbor in GetNeighbors(currentNode))
        {
            if (!neighbor.walkable || closedSet.Contains(neighbor))
            {
                continue;
            }

            float newCost = CalculateNewCost(currentNode, neighbor, targetNode, isReturningToBase);
            if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
            {
                UpdateNode(neighbor, currentNode, targetNode, newCost);
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Calculates the new cost to reach a neighbor node
    /// </summary>
    private float CalculateNewCost(Node currentNode, Node neighbor, Node targetNode, bool isReturningToBase)
    {
        float newCost = currentNode.gCost + Vector3.Distance(currentNode.position, neighbor.position);

        if (isReturningToBase)
        {
            newCost += CalculateBaseReturnPenalty(neighbor.position, currentNode.position, targetNode.position);
        }

        return newCost;
    }

    /// <summary>
    /// Calculates penalty for moving away from base when returning
    /// </summary>
    private float CalculateBaseReturnPenalty(Vector3 neighborPos, Vector3 currentPos, Vector3 targetPos)
    {
        float distanceToBase = Vector3.Distance(neighborPos, targetPos);
        float distanceFromStartToBase = Vector3.Distance(currentPos, targetPos);
        return distanceToBase > distanceFromStartToBase ? (distanceToBase - distanceFromStartToBase) * 2f : 0f;
    }

    /// <summary>
    /// Updates a node's pathfinding values
    /// </summary>
    private void UpdateNode(Node node, Node parent, Node target, float newCost)
    {
        node.gCost = newCost;
        node.hCost = Vector3.Distance(node.position, target.position);
        node.parent = parent;
    }

    /// <summary>
    /// Logs pathfinding failure information
    /// </summary>
    private void LogPathfindingFailure(int iterations, int openSetSize, int closedSetSize)
    {
        if (iterations >= 1000)
        {
            Debug.LogWarning($"Pathfinding failed after {iterations} iterations - possible infinite loop");
        }
        else
        {
            Debug.LogWarning($"No path found after checking {iterations} nodes. Open set size: {openSetSize}, Closed set size: {closedSetSize}");
        }
    }

    /// <summary>
    /// Retraces the path from end node to start node
    /// </summary>
    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;

            if (currentNode == null)
            {
                Debug.LogError("Path retracing failed - parent is null!");
                return null;
            }
        }
        path.Add(startNode.position);
        path.Reverse();

        return OptimizePath(path);
    }

    /// <summary>
    /// Optimizes the path by removing redundant points
    /// </summary>
    private List<Vector3> OptimizePath(List<Vector3> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            if (Vector3.Distance(path[i], path[i - 1]) < 0.1f)
            {
                path.RemoveAt(i);
                i--;
            }
        }
        return path;
    }

    /// <summary>
    /// Visualizes the grid in the Unity editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGrid || grid == null) return;

        foreach (Node node in grid)
        {
            if (node != null)
            {
                Gizmos.color = node.walkable ? new Color(1, 1, 1, 0.3f) : new Color(1, 0, 0, 0.3f);
                Gizmos.DrawCube(node.position, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}