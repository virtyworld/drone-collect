using UnityEngine;
using System.Collections.Generic;

public class DroneMovement : MonoBehaviour
{
    [SerializeField] private AStarPathfinding pathfinding;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private float moveSpeed = 5f;
    private float pathUpdateInterval = 5f;
    private float lastPathUpdateTime;
    private Vector3 targetPosition;
    private bool isMoving;
    private float waypointReachedDistance = 0.5f;
    private float minDistanceToTarget = 0.2f;
    private bool hasReachedDestination = false;

    // Collision avoidance parameters
    private float avoidanceRadius = 2f;
    private float avoidanceForce = 2f;
    private float raycastDistance = 3f;
    private LayerMask droneLayer;
    private Vector3 currentVelocity;
    private float smoothTime = 0.1f;

    private void Awake()
    {
        // Set up the drone layer mask
        droneLayer = LayerMask.GetMask("Drone");
    }

    public void SetDestination(Vector3 target)
    {
        if (isMoving)
        {
            return;
        }

        targetPosition = target;
        isMoving = true;
        hasReachedDestination = false;
        UpdatePath();
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
    }

    private Vector3 CalculateAvoidanceVelocity()
    {
        Vector3 avoidanceVelocity = Vector3.zero;

        // Check for drones in front using raycast
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, avoidanceRadius, transform.forward, raycastDistance, droneLayer);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject) // Ignore self
            {
                // Calculate avoidance force
                Vector3 awayFromDrone = transform.position - hit.point;
                float distance = awayFromDrone.magnitude;

                if (distance < avoidanceRadius)
                {
                    // Stronger force when closer
                    float force = avoidanceForce * (1f - (distance / avoidanceRadius));
                    avoidanceVelocity += awayFromDrone.normalized * force;
                }
            }
        }

        return avoidanceVelocity;
    }

    private void UpdatePath()
    {
        if (pathfinding != null)
        {
            Debug.Log($"Updating path from {transform.position} to {targetPosition}");
            currentPath = pathfinding.FindPath(transform, targetPosition);
            if (currentPath != null && currentPath.Count > 0)
            {
                Debug.Log($"New path calculated with {currentPath.Count} waypoints");
                currentPathIndex = 0;
                if (currentPath.Count > 1 && Vector3.Distance(transform.position, currentPath[0]) < waypointReachedDistance)
                {
                    currentPathIndex = 1;
                }
                lastPathUpdateTime = Time.time;
            }
            else
            {
                Debug.LogWarning("Failed to calculate path");
                isMoving = false;
            }
        }
        else
        {
            Debug.LogError("Pathfinding component is null!");
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
    }
}