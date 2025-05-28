/// <summary>
/// Manages the state transitions for a drone, implementing the State pattern.
/// Handles initialization, state changes, and ensures proper state lifecycle.
/// </summary>
public class DroneStateMachine
{
    public DroneBaseState CurrentState { get; private set; }

    /// <summary>
    /// Initializes the state machine with a starting state and triggers its entry behavior.
    /// </summary>
    /// <param name="startingState">The initial state for the drone</param>
    public void Initialize(DroneBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();
    }

    /// <summary>
    /// Transitions the drone to a new state, handling exit behavior of current state
    /// and entry behavior of the new state.
    /// </summary>
    /// <param name="newState">The state to transition to</param>
    public void ChangeState(DroneBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}