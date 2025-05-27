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
        drone.agent.isStopped = true;
        isCollecting = false;
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

        // Найти ближайший ресурс
        Collider[] colliders = Physics.OverlapSphere(drone.transform.position, 1f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {
                // Уничтожаем ресурс
                Object.Destroy(collider.gameObject);
                // Устанавливаем флаг, что дрон несет ресурс
                drone.SetCarryingResource(true);
                // Переходим к следующему состоянию
                stateMachine.ChangeState(drone.MovingToHomeState);
                break;
            }
        }
    }
}
