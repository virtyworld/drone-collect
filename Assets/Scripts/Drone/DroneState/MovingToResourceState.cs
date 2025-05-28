/// <summary>
/// State that handles drone movement towards a target resource.
/// Manages navigation, arrival detection, and state transitions when moving to collect resources.
/// </summary>
using UnityEngine;

public class MovingToResourceState : DroneBaseState
{
    private float arrivalDistance = 0.7f; // Distance at which we consider the drone has arrived at the resource

    public MovingToResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    /// <summary>
    /// Initializes the state by setting the drone's destination to the target resource.
    /// If no target resource exists, transitions back to searching state.
    /// </summary>
    public override void EnterState()
    {
        // Set destination to target resource
        GameObject targetResource = drone.GetTargetResource();

        if (targetResource != null)
        {
            drone.MoveTo(targetResource.transform.position);
        }
        else
        {
            // If no target resource, go back to searching
            stateMachine.ChangeState(drone.SearchingState);
        }
    }

    /// <summary>
    /// Updates the drone's movement and checks for state transitions.
    /// Verifies resource validity, updates destination, and checks for arrival.
    /// Transitions to collecting state when resource is reached.
    /// </summary>
    public override void UpdateState()
    {
        GameObject targetResource = drone.GetTargetResource();

        if (targetResource == null)
        {
            // If target was destroyed, go back to searching
            stateMachine.ChangeState(drone.SearchingState);
            return;
        }

        SpawnedResource spawnedResource = targetResource.GetComponent<SpawnedResource>();
        if (spawnedResource == null)
        {
            stateMachine.ChangeState(drone.SearchingState);
            return;
        }

        // Check if we're still the current drone for this resource
        if (!spawnedResource.IsCurrentDrone(drone))
        {
            stateMachine.ChangeState(drone.SearchingState);
            return;
        }

        // Update destination in case resource moved
        drone.MoveTo(targetResource.transform.position);

        // Check if we've reached the resource
        if (Vector3.Distance(drone.transform.position, targetResource.transform.position) <= arrivalDistance)
        {
            // We've reached the resource, transition to collecting state
            stateMachine.ChangeState(drone.CollectingResourceState);
        }
    }

    /// <summary>
    /// Stops the drone's movement when exiting this state.
    /// </summary>
    public override void ExitState()
    {
        // Stop the drone when leaving this state
        drone.StopMoving();
    }
}
