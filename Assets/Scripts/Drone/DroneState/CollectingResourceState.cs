using System.Collections;
using UnityEngine;

public class CollectingResourceState : DroneBaseState
{
    private GameObject targetResource;
    private bool isCollecting = false;

    public CollectingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    public override void EnterState()
    {
        // Остановить движение дрона
        drone.StopMoving();
        isCollecting = false;
        targetResource = drone.GetTargetResource();
    }

    public override void UpdateState()
    {
        if (!isCollecting)
        {
            isCollecting = true;
            drone.StartCoroutine(CollectResource());
        }
    }

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
