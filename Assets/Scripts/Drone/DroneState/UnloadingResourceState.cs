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

    public void Setup(UnityEvent<int, int> onResourceUnloaded, int homeBaseFactionId)
    {
        this.homeBaseFactionId = homeBaseFactionId;
        this.onResourceUnloaded = onResourceUnloaded;
    }
    public override void EnterState()
    {
        // Stop the drone
        drone.StopMoving();
        isUnloading = false;
    }

    public override void UpdateState()
    {
        if (!isUnloading)
        {
            isUnloading = true;
            drone.StartCoroutine(UnloadResource());
        }
    }

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
