using UnityEngine;
using System.Collections;

public class UnloadingResourceState : DroneBaseState
{
    private bool isUnloading = false;
    private float unloadingTime = 2f; // Time it takes to unload the resource
    private Base homeBase;

    public UnloadingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    public void Setup(Base homeBase)
    {
        this.homeBase = homeBase;
    }
    public override void EnterState()
    {
        // Stop the drone
        drone.agent.isStopped = true;
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
        if (homeBase != null)
        {
            homeBase.OnResourceUnloaded();
        }

        // Return to searching for resources
        stateMachine.ChangeState(drone.SearchingState);
    }
}
