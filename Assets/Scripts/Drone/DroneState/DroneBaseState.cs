/// <summary>
/// Base abstract class for all drone states in the state machine pattern.
/// Provides common functionality and interface for different drone behaviors.
/// </summary>
public abstract class DroneBaseState
{
    protected DroneAI drone; // Ссылка на главный AI-компонент дрона
    protected DroneStateMachine stateMachine;

    public DroneBaseState(DroneAI drone, DroneStateMachine stateMachine)
    {
        this.drone = drone;
        this.stateMachine = stateMachine;
    }

    /// <summary>
    /// Called when entering this state. Override to implement state-specific initialization logic.
    /// </summary>
    public virtual void EnterState() { }

    /// <summary>
    /// Called every frame while in this state. Override to implement state-specific update logic.
    /// </summary>
    public virtual void UpdateState() { }

    /// <summary>
    /// Called when exiting this state. Override to implement state-specific cleanup logic.
    /// </summary>
    public virtual void ExitState() { }
}