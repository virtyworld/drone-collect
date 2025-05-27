public abstract class DroneBaseState
{
    protected DroneAI drone; // Ссылка на главный AI-компонент дрона
    protected DroneStateMachine stateMachine;

    public DroneBaseState(DroneAI drone, DroneStateMachine stateMachine)
    {
        this.drone = drone;
        this.stateMachine = stateMachine;
    }

    public virtual void EnterState() { } // Выполняется при входе в состояние
    public virtual void UpdateState() { } // Выполняется каждый кадр
    public virtual void ExitState() { } // Выполняется при выходе из состояния
}