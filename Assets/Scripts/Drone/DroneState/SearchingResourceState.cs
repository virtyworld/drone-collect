using UnityEngine;

/// <summary>
/// Manages the drone's behavior when searching for resources.
/// Handles resource detection, queue management, and state transitions.
/// </summary>
public class SearchingResourceState : DroneBaseState
{
    private float searchRadius = 20f;
    private float searchInterval = 0.5f; // How often to search for resources
    private float lastSearchTime;
    private GameObject currentTargetResource = null;

    public SearchingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    /// <summary>
    /// Initializes the state when entered, resetting search parameters and stopping movement.
    /// </summary>
    public override void EnterState()
    {
        drone.StopMoving();
        lastSearchTime = Time.time;
        currentTargetResource = null;
    }

    /// <summary>
    /// Main update loop that handles periodic resource searching and queue management.
    /// </summary>
    public override void UpdateState()
    {
        if (ShouldSearchForResources())
        {
            lastSearchTime = Time.time;
            HandleResourceSearch();
        }
    }

    /// <summary>
    /// Cleans up when leaving the state, removing from resource queues and stopping movement.
    /// </summary>
    public override void ExitState()
    {
        RemoveFromResourceQueue();
        drone.StopMoving();
    }

    /// <summary>
    /// Checks if it's time to perform a new resource search based on the search interval.
    /// </summary>
    private bool ShouldSearchForResources()
    {
        return Time.time >= lastSearchTime + searchInterval;
    }

    /// <summary>
    /// Handles the main resource search logic, including queue management and target selection.
    /// </summary>
    private void HandleResourceSearch()
    {
        if (currentTargetResource != null)
        {
            CheckCurrentTargetStatus();
        }

        if (currentTargetResource == null)
        {
            FindAndProcessNewResource();
        }
    }

    /// <summary>
    /// Checks if the current target resource is still valid and in queue.
    /// </summary>
    private void CheckCurrentTargetStatus()
    {
        SpawnedResource spawnedResource = currentTargetResource.GetComponent<SpawnedResource>();
        if (spawnedResource != null && spawnedResource.IsInQueue(drone))
        {
            currentTargetResource = null;
        }
    }

    /// <summary>
    /// Searches for and processes a new resource target.
    /// </summary>
    private void FindAndProcessNewResource()
    {
        GameObject targetResource = drone.FindBestResource(searchRadius);
        if (targetResource != null)
        {
            ProcessFoundResource(targetResource);
        }
        else
        {
            Debug.Log($"[SearchingResourceState] No resources found within {searchRadius} radius");
        }
    }

    /// <summary>
    /// Processes a found resource, attempting to claim it or join its queue.
    /// </summary>
    private void ProcessFoundResource(GameObject targetResource)
    {
        SpawnedResource spawnedResource = targetResource.GetComponent<SpawnedResource>();
        if (spawnedResource != null)
        {
            if (spawnedResource.TryClaimResource(drone))
            {
                drone.SetTargetResource(targetResource);
                stateMachine.ChangeState(drone.MovingToResourceState);
            }
            else
            {
                currentTargetResource = targetResource;
            }
        }
    }

    /// <summary>
    /// Removes the drone from the current resource's queue if applicable.
    /// </summary>
    private void RemoveFromResourceQueue()
    {
        if (currentTargetResource != null)
        {
            SpawnedResource spawnedResource = currentTargetResource.GetComponent<SpawnedResource>();
            if (spawnedResource != null)
            {
                Debug.Log($"[SearchingResourceState] Removing from queue for resource {currentTargetResource.name}");
                spawnedResource.RemoveFromQueue(drone);
            }
        }
    }
}