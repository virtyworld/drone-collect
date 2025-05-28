/// <summary>
/// Represents the state where a drone unloads collected resources at its home base.
/// Handles the unloading animation and notifies the base about resource delivery.
/// </summary>
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class UnloadingResourceState : DroneBaseState
{
    private bool isUnloading = false;
    private float unloadingTime = 2f; // Time it takes to unload the resource
    private int homeBaseFactionId;
    private UnityEvent<int, int> onResourceUnloaded;
    public UnloadingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    /// <summary>
    /// Initializes the unloading state with necessary event handlers and faction information
    /// </summary>
    public void Setup(UnityEvent<int, int> onResourceUnloaded, int homeBaseFactionId)
    {
        this.homeBaseFactionId = homeBaseFactionId;
        this.onResourceUnloaded = onResourceUnloaded;
    }

    /// <summary>
    /// Called when entering the unloading state. Stops the drone's movement and resets unloading flag
    /// </summary>
    public override void EnterState()
    {
        // Stop the drone
        drone.StopMoving();
        isUnloading = false;
    }

    /// <summary>
    /// Updates the unloading state. Initiates the unloading process if not already started
    /// </summary>
    public override void UpdateState()
    {
        if (!isUnloading)
        {
            isUnloading = true;
            drone.StartCoroutine(UnloadResource());
        }
    }

    /// <summary>
    /// Coroutine that handles the resource unloading process, including timing and state transitions
    /// </summary>
    private IEnumerator UnloadResource()
    {
        // Wait for the unloading time
        yield return new WaitForSeconds(unloadingTime);

        // Unload the resource
        drone.SetCarryingResource(false);

        // Notify the base about resource unloading
        onResourceUnloaded.Invoke(homeBaseFactionId, 1);

        // Return to searching for resources
        stateMachine.ChangeState(drone.SearchingState);
    }
}
