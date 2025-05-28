/// <summary>
/// State that handles drone movement back to the home base after collecting resources.
/// Manages navigation and arrival detection at the base location.
/// </summary>
using UnityEngine;

public class MovingToHomeState : DroneBaseState
{
    private float arrivalDistance = 2f; // Distance at which we consider the drone has arrived at the base

    public MovingToHomeState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    /// <summary>
    /// Initializes the state by setting the drone's destination to the home base position
    /// </summary>
    public override void EnterState()
    {
        // Set destination to home base
        Vector3 homePosition = drone.GetHomeBasePosition();
        // Use the isReturningToBase parameter to ensure the drone moves towards the base
        drone.MoveTo(homePosition, true);
    }

    /// <summary>
    /// Checks if the drone has reached the home base and transitions to unloading state when arrived
    /// </summary>
    public override void UpdateState()
    {
        // Check if we've reached the base
        if (Vector3.Distance(drone.transform.position, drone.GetHomeBasePosition()) <= arrivalDistance)
        {
            // We've reached the base, transition to unloading state
            stateMachine.ChangeState(drone.UnloadingResourceState);
        }
    }

    /// <summary>
    /// Stops the drone's movement when exiting this state
    /// </summary>
    public override void ExitState()
    {
        // Stop the drone when leaving this state
        drone.StopMoving();
    }
}
