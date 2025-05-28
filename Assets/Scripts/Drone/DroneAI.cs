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

    public void Setup(Base homeBase, UnityEvent<int, int> onResourceUnloaded, int homeBaseFactionId)
    {
        this.homeBase = homeBase;
        this.homeBaseFactionId = homeBaseFactionId;
        this.onResourceUnloaded = onResourceUnloaded;
        Initialize();
    }

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


    void Update()
    {
        if (stateMachine != null)
        {
            stateMachine.CurrentState.UpdateState();
        }
    }


    public void SetCarryingResource(bool value)
    {
        isCarryingResource = value;
    }

    public Vector3 GetHomeBasePosition()
    {
        return homeBase != null ? homeBase.transform.position : transform.position;
    }

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

    public void SetTargetResource(GameObject resource)
    {
        targetResource = resource;
    }

    public GameObject GetTargetResource()
    {
        return targetResource;
    }

    public void MoveTo(Vector3 destination, bool isReturningToBase = false)
    {
        movementController.SetDestination(destination, isReturningToBase);
    }

    public bool HasReachedDestination()
    {
        return movementController.HasReachedDestination();
    }

    public void StopMoving()
    {
        movementController.StopMoving();
    }

    public bool IsReturningToBase()
    {
        return stateMachine.CurrentState == MovingToHomeState;
    }

    public void ContinueMovement()
    {
        if (stateMachine.CurrentState == MovingToResourceState)
        {
            // Если мы двигались к ресурсу, продолжаем движение к нему
            if (targetResource != null)
            {
                MoveTo(targetResource.transform.position);
            }
        }
        else if (stateMachine.CurrentState == MovingToHomeState)
        {
            // Если мы возвращались на базу, продолжаем движение к ней
            MoveTo(GetHomeBasePosition(), true);
        }
    }
}