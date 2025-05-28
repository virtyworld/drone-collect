/// <summary>
/// Represents the state where a drone is actively collecting a resource.
/// This state handles the resource collection process, including waiting time,
/// resource destruction, and transitioning to the next appropriate state.
/// </summary>
using System.Collections;
using UnityEngine;

public class CollectingResourceState : DroneBaseState
{
    private GameObject targetResource;
    private bool isCollecting = false;

    public CollectingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    /// <summary>
    /// Initializes the collecting state by stopping the drone and getting the target resource.
    /// </summary>
    public override void EnterState()
    {
        // Остановить движение дрона
        drone.StopMoving();
        isCollecting = false;
        targetResource = drone.GetTargetResource();
    }

    /// <summary>
    /// Updates the state by initiating the resource collection process if not already collecting.
    /// </summary>
    public override void UpdateState()
    {
        if (!isCollecting)
        {
            isCollecting = true;
            drone.StartCoroutine(CollectResource());
        }
    }

    /// <summary>
    /// Coroutine that handles the actual resource collection process:
    /// - Waits for collection time
    /// - Releases and destroys the resource
    /// - Updates drone state
    /// - Transitions to appropriate next state
    /// </summary>
    private IEnumerator CollectResource()
    {
        // Ждем 2 секунды
        yield return new WaitForSeconds(2f);

        if (targetResource != null)
        {
            SpawnedResource spawnedResource = targetResource.GetComponent<SpawnedResource>();
            if (spawnedResource != null)
            {
                // Release the resource before destroying it
                spawnedResource.ReleaseResource();
            }

            Debug.Log("Collecting resource");
            // Уничтожаем ресурс
            Object.Destroy(targetResource);
            // Устанавливаем флаг, что дрон несет ресурс
            drone.SetCarryingResource(true);
            // Переходим к следующему состоянию
            stateMachine.ChangeState(drone.MovingToHomeState);
        }
        else
        {
            // If resource is gone, go back to searching
            stateMachine.ChangeState(drone.SearchingState);
        }
    }
}
