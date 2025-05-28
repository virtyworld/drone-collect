using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class AStarPathfinding : MonoBehaviour
{
    [SerializeField] private LayerMask unwalkableMask; // Set this in the inspector to specify which layers block movement
    [SerializeField] private bool showDebugGrid = true; // Toggle grid visualization in the inspector   
    public bool HasReachedDestination { get; private set; } = false;

    private class Node
    {
        public Vector3 position;
        public float gCost; // Cost from start to this node
        public float hCost; // Heuristic cost from this node to end
        public float fCost { get { return gCost + hCost; } } // Total cost
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
    private float nodeRadius = 0.5f; // Size of each node in the grid
    private float nodeDiameter;
    private Vector3 gridWorldSize;
    private Node[,,] grid;
    private int gridSizeX, gridSizeY, gridSizeZ;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        // Увеличиваем размер сетки для лучшего покрытия сцены
        gridWorldSize = new Vector3(40f, 30f, 40f);
    }

    private void CreateGrid(Transform droneTransform)
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];

        // Center the grid at the origin
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2 - Vector3.forward * gridWorldSize.z / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft +
                        Vector3.right * (x * nodeDiameter + nodeRadius) +
                        Vector3.up * (y * nodeDiameter + nodeRadius) +
                        Vector3.forward * (z * nodeDiameter + nodeRadius);

                    // Проверяем проходимость с учетом размеров дрона
                    bool walkable = IsWalkable(droneTransform, worldPoint);
                    grid[x, y, z] = new Node(worldPoint, walkable);
                }
            }
        }
    }

    private bool IsWalkable(Transform droneTransform, Vector3 position)
    {
        if (droneTransform == null) return false;

        // Получаем размеры дрона из его scale
        float droneRadius = Mathf.Max(droneTransform.localScale.x, droneTransform.localScale.z) * 0.5f;
        float droneHeight = droneTransform.localScale.y;

        // Проверяем коллизии по горизонтали (радиус дрона)
        if (Physics.CheckSphere(position, droneRadius, unwalkableMask))
        {
            return false;
        }

        // Проверяем коллизии по вертикали (высота дрона)
        Vector3 topPoint = position + Vector3.up * droneHeight;
        if (Physics.CheckSphere(topPoint, droneRadius, unwalkableMask))
        {
            return false;
        }

        // Проверяем коллизии между нижней и верхней точками
        RaycastHit[] hits = Physics.SphereCastAll(position, droneRadius, Vector3.up, droneHeight, unwalkableMask);
        if (hits.Length > 0)
        {
            return false;
        }

        return true;
    }

    private Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // Calculate position relative to grid center
        Vector3 localPosition = worldPosition - transform.position;

        // Convert to grid coordinates
        float percentX = (localPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (localPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        float percentZ = (localPosition.z + gridWorldSize.z / 2) / gridWorldSize.z;

        // Clamp to grid bounds
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);

        // Ensure coordinates are within bounds
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);
        z = Mathf.Clamp(z, 0, gridSizeZ - 1);

        return grid[x, y, z];
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Find the node's grid coordinates
        int x = -1, y = -1, z = -1;
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                for (int k = 0; k < gridSizeZ; k++)
                {
                    if (grid[i, j, k] == node)
                    {
                        x = i;
                        y = j;
                        z = k;
                        break;
                    }
                }
                if (x != -1) break;
            }
            if (x != -1) break;
        }

        if (x == -1)
        {
            Debug.LogError($"Could not find node {node.position} in grid!");
            return neighbors;
        }

        // Check all 26 surrounding nodes (3x3x3 cube minus the center)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0) continue;

                    int checkX = x + i;
                    int checkY = y + j;
                    int checkZ = z + k;

                    if (checkX >= 0 && checkX < gridSizeX &&
                        checkY >= 0 && checkY < gridSizeY &&
                        checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        Node neighbor = grid[checkX, checkY, checkZ];
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

        if (!startNode.walkable || !targetNode.walkable)
        {
            Debug.LogWarning("Start or target node is not walkable!");
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.position, targetNode.position);

        int iterations = 0;
        const int maxIterations = 1000; // Prevent infinite loops

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                List<Vector3> path = RetracePath(startNode, targetNode);
                HasReachedDestination = true;
                return path;
            }

            List<Node> neighbors = GetNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCostToNeighbor = currentNode.gCost + Vector3.Distance(currentNode.position, neighbor.position);

                // If returning to base, add a penalty for moving away from the base
                if (isReturningToBase)
                {
                    float distanceToBase = Vector3.Distance(neighbor.position, targetPos);
                    float distanceFromStartToBase = Vector3.Distance(currentNode.position, targetPos);
                    if (distanceToBase > distanceFromStartToBase)
                    {
                        // Add penalty for moving away from base
                        newMovementCostToNeighbor += (distanceToBase - distanceFromStartToBase) * 2f;
                    }
                }

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.position, targetNode.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogWarning($"Pathfinding failed after {maxIterations} iterations - possible infinite loop");
        }
        else
        {
            Debug.LogWarning($"No path found after checking {iterations} nodes. Open set size: {openSet.Count}, Closed set size: {closedSet.Count}");
        }
        return null;
    }

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

        // Проверяем, что путь не содержит повторяющихся точек
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