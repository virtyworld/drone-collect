/// <summary>
/// Controls the movement and pathfinding behavior of a drone in a 3D environment.
/// Handles path following, collision avoidance, and smooth movement transitions.
/// Uses A* pathfinding for navigation and includes debug visualization capabilities.
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class DroneMovement : MonoBehaviour
{
    [SerializeField] private AStarPathfinding pathfinding;
    [SerializeField] private LineRenderer pathLineRenderer;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float pathUpdateInterval = 2f; // Reduced from 5f to 2f
    [SerializeField] private float waypointReachedDistance = 0.5f;
    [SerializeField] private float minDistanceToTarget = 0.2f;
    [SerializeField] private float avoidanceRadius = 2f;
    [SerializeField] private float avoidanceForce = 2f;
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private bool debugMode = true;

    private bool isInitialized = false;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private float lastPathUpdateTime;
    private Vector3 targetPosition;
    private bool isMoving;
    private bool hasReachedDestination = false;
    private LayerMask droneLayer;
    private Vector3 currentVelocity;

    // Debug variables
    private Vector3 lastPosition;
    private float lastDebugTime;
    private const float DEBUG_INTERVAL = 1f;

    private UnityEvent<float> onDroneSpeedChanged;
    private UnityEvent<bool> onPathVisibleChanged;
    public void Setup(UnityEvent<float> onDroneSpeedChanged, UnityEvent<bool> onPathVisibleChanged)
    {
        this.onDroneSpeedChanged = onDroneSpeedChanged;
        this.onPathVisibleChanged = onPathVisibleChanged;

        Initialize();
    }
    /// <summary>
    /// Initializes the drone movement system with required components and settings
    /// </summary>
    private void Initialize()
    {
        isInitialized = true;
        droneLayer = LayerMask.GetMask("Drone");
        lastPosition = transform.position;
        lastDebugTime = Time.time;

        InitializeLineRenderer();
        SetupEventListeners();
    }

    /// <summary>
    /// Sets up the LineRenderer component for path visualization
    /// </summary>
    private void InitializeLineRenderer()
    {
        if (pathLineRenderer == null)
        {
            pathLineRenderer = gameObject.AddComponent<LineRenderer>();
            pathLineRenderer.startWidth = 0.1f;
            pathLineRenderer.endWidth = 0.1f;
            pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            pathLineRenderer.startColor = Color.yellow;
            pathLineRenderer.endColor = Color.yellow;
        }
    }

    /// <summary>
    /// Sets up event listeners for drone speed and path visibility changes
    /// </summary>
    private void SetupEventListeners()
    {
        onDroneSpeedChanged.AddListener(SetDroneSpeed);
        onPathVisibleChanged.AddListener(SetPathVisible);
    }

    /// <summary>
    /// Updates the visual representation of the current path
    /// </summary>
    private void UpdatePathVisualization()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            pathLineRenderer.positionCount = currentPath.Count;
            for (int i = 0; i < currentPath.Count; i++)
            {
                pathLineRenderer.SetPosition(i, currentPath[i]);
            }
        }
        else
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Sets a new destination for the drone to move towards
    /// </summary>
    public void SetDestination(Vector3 target, bool isReturningToBase = false)
    {
        if (isMoving) return;

        targetPosition = target;
        isMoving = true;
        hasReachedDestination = false;
        UpdatePath(isReturningToBase);
    }

    /// <summary>
    /// Main update loop handling drone movement and path following
    /// </summary>
    private void Update()
    {
        if (!ShouldUpdate()) return;

        if (HasReachedTarget())
        {
            HandleDestinationReached();
            return;
        }

        MoveDrone();
        UpdateWaypointProgress();
        CheckPathUpdateNeeded();
        UpdateDebugInfo();
    }

    /// <summary>
    /// Checks if the update loop should continue
    /// </summary>
    private bool ShouldUpdate()
    {
        return isInitialized && isMoving && currentPath != null && currentPath.Count > 0;
    }

    /// <summary>
    /// Checks if the drone has reached its target position
    /// </summary>
    private bool HasReachedTarget()
    {
        return Vector3.Distance(transform.position, targetPosition) <= minDistanceToTarget;
    }

    /// <summary>
    /// Handles cleanup when destination is reached
    /// </summary>
    private void HandleDestinationReached()
    {
        isMoving = false;
        hasReachedDestination = true;
        currentPath = null;
        pathLineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Handles the actual movement of the drone
    /// </summary>
    private void MoveDrone()
    {
        Vector3 desiredVelocity = CalculateDesiredVelocity();
        Vector3 avoidanceVelocity = CalculateAvoidanceVelocity();
        Vector3 finalVelocity = desiredVelocity + avoidanceVelocity;

        currentVelocity = Vector3.Lerp(currentVelocity, finalVelocity, Time.deltaTime / smoothTime);
        transform.position += currentVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Calculates the desired velocity based on current waypoint or target
    /// </summary>
    private Vector3 CalculateDesiredVelocity()
    {
        if (currentPathIndex >= currentPath.Count - 1)
        {
            return (targetPosition - transform.position).normalized * moveSpeed;
        }
        return (currentPath[currentPathIndex] - transform.position).normalized * moveSpeed;
    }

    /// <summary>
    /// Updates the current waypoint progress
    /// </summary>
    private void UpdateWaypointProgress()
    {
        if (currentPathIndex < currentPath.Count - 1)
        {
            Vector3 currentWaypoint = currentPath[currentPathIndex];
            if (Vector3.Distance(transform.position, currentWaypoint) < waypointReachedDistance)
            {
                currentPathIndex++;
            }
        }
    }

    /// <summary>
    /// Checks if the path needs to be updated
    /// </summary>
    private void CheckPathUpdateNeeded()
    {
        if (Time.time >= lastPathUpdateTime + pathUpdateInterval && !hasReachedDestination)
        {
            float distanceToNextWaypoint = Vector3.Distance(transform.position, currentPath[currentPathIndex]);
            if (distanceToNextWaypoint > waypointReachedDistance * 2)
            {
                UpdatePath();
            }
        }
    }

    /// <summary>
    /// Updates debug information if debug mode is enabled
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (debugMode && Time.time >= lastDebugTime + DEBUG_INTERVAL)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            lastDebugTime = Time.time;
        }
    }

    /// <summary>
    /// Calculates avoidance velocity to prevent collisions with other drones
    /// </summary>
    private Vector3 CalculateAvoidanceVelocity()
    {
        Vector3 avoidanceVelocity = Vector3.zero;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            avoidanceVelocity += CalculateAvoidanceForDirection(direction);
        }
        return avoidanceVelocity;
    }

    /// <summary>
    /// Calculates avoidance force for a specific direction
    /// </summary>
    private Vector3 CalculateAvoidanceForDirection(Vector3 direction)
    {
        Vector3 avoidanceVelocity = Vector3.zero;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, avoidanceRadius, direction, raycastDistance, droneLayer);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                avoidanceVelocity += CalculateAvoidanceForce(hit.point);
            }
        }
        return avoidanceVelocity;
    }

    /// <summary>
    /// Calculates the avoidance force for a specific collision point
    /// </summary>
    private Vector3 CalculateAvoidanceForce(Vector3 hitPoint)
    {
        Vector3 awayFromDrone = transform.position - hitPoint;
        float distance = awayFromDrone.magnitude;

        if (distance < avoidanceRadius)
        {
            float force = avoidanceForce * (1f - (distance / avoidanceRadius));
            if (debugMode)
            {
                Debug.DrawLine(transform.position, hitPoint, Color.red, 0.1f);
            }
            return awayFromDrone.normalized * force;
        }
        return Vector3.zero;
    }

    private void UpdatePath(bool isReturningToBase = false)
    {
        if (pathfinding != null)
        {
            currentPath = pathfinding.FindPath(transform, targetPosition, isReturningToBase);
            if (currentPath != null && currentPath.Count > 0)
            {
                currentPathIndex = 0;
                if (currentPath.Count > 1 && Vector3.Distance(transform.position, currentPath[0]) < waypointReachedDistance)
                {
                    currentPathIndex = 1;
                }
                lastPathUpdateTime = Time.time;
                UpdatePathVisualization();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Failed to calculate path to {targetPosition}");
                isMoving = false;
                pathLineRenderer.positionCount = 0;
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Pathfinding component is null!");
        }
    }

    public bool HasReachedDestination()
    {
        return hasReachedDestination;
    }

    public void StopMoving()
    {
        isMoving = false;
        currentPath = null;
        currentVelocity = Vector3.zero;
        pathLineRenderer.positionCount = 0;
    }
    public void StopAndWait()
    {
        isMoving = false;
    }
    public void SetDroneSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void SetPathVisible(bool visible)
    {
        pathLineRenderer.enabled = visible;
    }

    public void AddAvoidanceForce(Vector3 force)
    {
        currentVelocity += force;
    }

    public Vector3 GetVelocity()
    {
        return currentVelocity;
    }
}