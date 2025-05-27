public class DroneStateMachine
{
    public DroneBaseState CurrentState { get; private set; }

    public void Initialize(DroneBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();
    }

    public void ChangeState(DroneBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}