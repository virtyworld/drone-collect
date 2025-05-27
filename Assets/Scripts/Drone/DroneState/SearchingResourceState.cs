public class SearchingResourceState : DroneBaseState
{
    public SearchingResourceState(DroneAI drone, DroneStateMachine stateMachine) : base(drone, stateMachine) { }

    public override void EnterState()
    {
        // Логика начала поиска ресурса
    }

    public override void UpdateState()
    {
        // Искать ближайший свободный ресурс
        // Resource targetResource = drone.FindBestResource();
        // if (targetResource != null)
        // {
        //     drone.SetTargetResource(targetResource);
        //     stateMachine.ChangeState(drone.MovingToResourceState);
        // }
    }
}