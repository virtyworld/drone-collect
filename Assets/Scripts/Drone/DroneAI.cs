/// <summary>
/// Controls the AI behavior of a drone, managing its states and interactions with resources and home base.
/// Handles resource collection, movement, and state transitions through a state machine pattern.
/// </summary>
using UnityEngine;
using UnityEngine.Events;

public class DroneAI : MonoBehaviour
{
    [SerializeField] private Base homeBase;
    private DroneMovement movementController;
    private DroneStateMachine stateMachine;
    public bool isCarryingResource { get; private set; }
    private GameObject targetResource;
    private UnityEvent<int, int> onResourceUnloaded;
    private int homeBaseFactionId;
    public SearchingResourceState SearchingState { get; private set; }
    public MovingToResourceState MovingToResourceState { get; private set; }
    public CollectingResourceState CollectingResourceState { get; private set; }
    public MovingToHomeState MovingToHomeState { get; private set; }
    public UnloadingResourceState UnloadingResourceState { get; private set; }

    /// <summary>
    /// Initializes the drone with its home base, resource unload callback, and faction ID.
    /// </summary>
    public void Setup(Base homeBase, UnityEvent<int, int> onResourceUnloaded, int homeBaseFactionId)
    {
        this.homeBase = homeBase;
        this.homeBaseFactionId = homeBaseFactionId;
        this.onResourceUnloaded = onResourceUnloaded;
        Initialize();
    }

    /// <summary>
    /// Sets up the drone's movement controller and initializes the state machine with all possible states.
    /// </summary>
    private void Initialize()
    {
        movementController = GetComponent<DroneMovement>();

        if (movementController == null)
        {
            movementController = gameObject.AddComponent<DroneMovement>();
        }

        stateMachine = new DroneStateMachine();

        SearchingState = new SearchingResourceState(this, stateMachine);
        MovingToResourceState = new MovingToResourceState(this, stateMachine);
        CollectingResourceState = new CollectingResourceState(this, stateMachine);
        MovingToHomeState = new MovingToHomeState(this, stateMachine);
        UnloadingResourceState = new UnloadingResourceState(this, stateMachine);
        UnloadingResourceState.Setup(onResourceUnloaded, homeBaseFactionId);
        stateMachine.Initialize(SearchingState);
    }

    /// <summary>
    /// Updates the current state of the drone every frame.
    /// </summary>
    void Update()
    {
        if (stateMachine != null)
        {
            stateMachine.CurrentState.UpdateState();
        }
    }

    /// <summary>
    /// Updates whether the drone is currently carrying a resource.
    /// </summary>
    public void SetCarryingResource(bool value)
    {
        isCarryingResource = value;
    }

    /// <summary>
    /// Returns the position of the drone's home base.
    /// </summary>
    public Vector3 GetHomeBasePosition()
    {
        return homeBase != null ? homeBase.transform.position : transform.position;
    }

    /// <summary>
    /// Finds the closest available resource within the specified search radius.
    /// </summary>
    public GameObject FindBestResource(float searchRadius = 15f)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius);
        GameObject bestResource = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            // Check if the object has SpawnedResource component or Resource tag
            if (collider.CompareTag("Resource") || collider.GetComponent<SpawnedResource>() != null)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestResource = collider.gameObject;
                }
            }
        }

        return bestResource;
    }

    /// <summary>
    /// Sets the current target resource for the drone to collect.
    /// </summary>
    public void SetTargetResource(GameObject resource)
    {
        targetResource = resource;
    }

    /// <summary>
    /// Returns the current target resource the drone is pursuing.
    /// </summary>
    public GameObject GetTargetResource()
    {
        return targetResource;
    }

    /// <summary>
    /// Commands the drone to move to a specified destination.
    /// </summary>
    public void MoveTo(Vector3 destination, bool isReturningToBase = false)
    {
        movementController.SetDestination(destination, isReturningToBase);
    }

    /// <summary>
    /// Checks if the drone has reached its current destination.
    /// </summary>
    public bool HasReachedDestination()
    {
        return movementController.HasReachedDestination();
    }

    /// <summary>
    /// Stops the drone's current movement.
    /// </summary>
    public void StopMoving()
    {
        movementController.StopMoving();
    }

    /// <summary>
    /// Checks if the drone is currently returning to its home base.
    /// </summary>
    public bool IsReturningToBase()
    {
        return stateMachine.CurrentState == MovingToHomeState;
    }

    /// <summary>
    /// Resumes the drone's movement based on its current state (either to resource or home base).
    /// </summary>
    public void ContinueMovement()
    {
        if (stateMachine.CurrentState == MovingToResourceState)
        {
            if (targetResource != null)
            {
                MoveTo(targetResource.transform.position);
            }
        }
        else if (stateMachine.CurrentState == MovingToHomeState)
        {
            MoveTo(GetHomeBasePosition(), true);
        }
    }
}