using UnityEngine;

public class MovingToHomeState : DroneBaseState
{
    private float arrivalDistance = 0.5f; // Distance at which we consider the drone has arrived at the base

    public MovingToHomeState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    public override void EnterState()
    {
        // Set destination to home base
        Vector3 homePosition = drone.GetHomeBasePosition();
        drone.MoveTo(homePosition);
    }

    public override void UpdateState()
    {
        // Check if we've reached the base

        if (Vector3.Distance(drone.transform.position, drone.GetHomeBasePosition()) <= arrivalDistance)
        {
            // We've reached the base, transition to unloading state
            stateMachine.ChangeState(drone.UnloadingResourceState);
        }
    }

    public override void ExitState()
    {
        // Stop the drone when leaving this state
        drone.StopMoving();
    }
}
