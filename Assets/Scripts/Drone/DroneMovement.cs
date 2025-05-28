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

    private void Awake()
    {
        droneLayer = LayerMask.GetMask("Drone");
        lastPosition = transform.position;
        lastDebugTime = Time.time;

        // Initialize LineRenderer if not assigned
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

    public void SetDestination(Vector3 target, bool isReturningToBase = false)
    {
        if (isMoving)
        {
            return;
        }

        targetPosition = target;
        isMoving = true;
        hasReachedDestination = false;
        UpdatePath(isReturningToBase);
    }

    private void Update()
    {

        if (!isMoving || currentPath == null || currentPath.Count == 0)
        {
            return;
        }
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= minDistanceToTarget)
        {
            isMoving = false;
            hasReachedDestination = true;
            currentPath = null;
            pathLineRenderer.positionCount = 0;

            return;
        }

        // Calculate desired velocity
        Vector3 desiredVelocity;
        if (currentPathIndex >= currentPath.Count - 1)
        {
            desiredVelocity = (targetPosition - transform.position).normalized * moveSpeed;
        }
        else
        {
            Vector3 currentWaypoint = currentPath[currentPathIndex];
            desiredVelocity = (currentWaypoint - transform.position).normalized * moveSpeed;
        }

        // Apply collision avoidance
        Vector3 avoidanceVelocity = CalculateAvoidanceVelocity();
        desiredVelocity += avoidanceVelocity;

        // Smoothly change velocity
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime / smoothTime);

        // Move the drone
        transform.position += currentVelocity * Time.deltaTime;

        // Check if we've reached the current waypoint
        if (currentPathIndex < currentPath.Count - 1)
        {
            Vector3 currentWaypoint = currentPath[currentPathIndex];
            if (Vector3.Distance(transform.position, currentWaypoint) < waypointReachedDistance)
            {
                currentPathIndex++;
            }
        }

        // Update path if needed
        if (Time.time >= lastPathUpdateTime + pathUpdateInterval && !hasReachedDestination)
        {
            float distanceToNextWaypoint = Vector3.Distance(transform.position, currentPath[currentPathIndex]);
            if (distanceToNextWaypoint > waypointReachedDistance * 2)
            {
                UpdatePath();
            }
        }

        // Debug logging
        if (debugMode && Time.time >= lastDebugTime + DEBUG_INTERVAL)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            lastPosition = transform.position;
            lastDebugTime = Time.time;
        }
    }

    private Vector3 CalculateAvoidanceVelocity()
    {
        Vector3 avoidanceVelocity = Vector3.zero;

        // Check for drones in all directions using multiple raycasts
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, avoidanceRadius, direction, raycastDistance, droneLayer);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != gameObject)
                {
                    Vector3 awayFromDrone = transform.position - hit.point;
                    float distance = awayFromDrone.magnitude;

                    if (distance < avoidanceRadius)
                    {
                        float force = avoidanceForce * (1f - (distance / avoidanceRadius));
                        avoidanceVelocity += awayFromDrone.normalized * force;

                        if (debugMode)
                        {
                            Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f);
                        }
                    }
                }
            }
        }

        return avoidanceVelocity;
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

    public void SetDroneSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void SetPathVisible(bool visible)
    {
        pathLineRenderer.enabled = visible;
    }
}