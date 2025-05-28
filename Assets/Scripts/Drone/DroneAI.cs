using UnityEngine;

public class DroneAI : MonoBehaviour
{
    [SerializeField] private Base homeBase;
    private DroneMovement movementController;
    private DroneStateMachine stateMachine;
    public bool isCarryingResource { get; private set; }
    private GameObject targetResource;

    public SearchingResourceState SearchingState { get; private set; }
    public MovingToResourceState MovingToResourceState { get; private set; }
    public CollectingResourceState CollectingResourceState { get; private set; }
    public MovingToHomeState MovingToHomeState { get; private set; }
    public UnloadingResourceState UnloadingResourceState { get; private set; }

    public void Setup(Base homeBase)
    {
        this.homeBase = homeBase;
    }

    void Awake()
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
    }

    void Start()
    {
        stateMachine.Initialize(SearchingState);
    }

    void Update()
    {
        stateMachine.CurrentState.UpdateState();
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
}