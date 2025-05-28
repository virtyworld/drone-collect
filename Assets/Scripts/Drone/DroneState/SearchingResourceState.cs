using UnityEngine;

public class SearchingResourceState : DroneBaseState
{
    private float searchRadius = 20f;
    private float searchInterval = 1f; // How often to search for resources
    private float lastSearchTime;
    private GameObject currentTargetResource = null;

    public SearchingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    public override void EnterState()
    {
        Debug.Log($"[SearchingResourceState] Entering state for drone {drone.gameObject.name}");
        // Enable the NavMeshAgent
        drone.StopMoving();
        lastSearchTime = Time.time;
        currentTargetResource = null;
    }

    public override void UpdateState()
    {
        // Search for resources periodically
        if (Time.time >= lastSearchTime + searchInterval)
        {
            lastSearchTime = Time.time;
            Debug.Log($"[SearchingResourceState] Searching for resources. Current target: {(currentTargetResource != null ? currentTargetResource.name : "none")}");

            // If we have a current target, check if we're still in queue
            if (currentTargetResource != null)
            {
                SpawnedResource spawnedResource = currentTargetResource.GetComponent<SpawnedResource>();
                if (spawnedResource != null)
                {
                    // If we're not in queue anymore, clear the target
                    if (!spawnedResource.IsInQueue(drone))
                    {
                        Debug.Log($"[SearchingResourceState] Lost queue position for resource {currentTargetResource.name}");
                        currentTargetResource = null;
                    }
                }
            }

            // If we don't have a target or lost our queue position, look for a new one
            if (currentTargetResource == null)
            {
                // Find the best resource within radius
                GameObject targetResource = drone.FindBestResource(searchRadius);

                if (targetResource != null)
                {
                    Debug.Log($"[SearchingResourceState] Found potential resource: {targetResource.name}");
                    SpawnedResource spawnedResource = targetResource.GetComponent<SpawnedResource>();
                    if (spawnedResource != null)
                    {
                        // Try to claim the resource or get in queue
                        if (spawnedResource.TryClaimResource(drone))
                        {
                            Debug.Log($"[SearchingResourceState] Successfully claimed resource {targetResource.name}");
                            drone.SetTargetResource(targetResource);
                            stateMachine.ChangeState(drone.MovingToResourceState);
                        }
                        else
                        {
                            // We're in queue, remember this resource
                            Debug.Log($"[SearchingResourceState] Resource {targetResource.name} is busy, joining queue");
                            currentTargetResource = targetResource;
                        }
                    }
                }
                else
                {
                    Debug.Log($"[SearchingResourceState] No resources found within {searchRadius} radius");
                }
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log($"[SearchingResourceState] Exiting state for drone {drone.gameObject.name}");
        // If we're leaving the state and have a target, remove ourselves from its queue
        if (currentTargetResource != null)
        {
            SpawnedResource spawnedResource = currentTargetResource.GetComponent<SpawnedResource>();
            if (spawnedResource != null)
            {
                Debug.Log($"[SearchingResourceState] Removing from queue for resource {currentTargetResource.name}");
                spawnedResource.RemoveFromQueue(drone);
            }
        }
        // Stop the drone when leaving this state
        drone.StopMoving();
    }
}