using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    public NavMeshAgent agent;
    private DroneStateMachine stateMachine;
    public bool isCarryingResource { get; private set; }
    private Base homeBase;

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
        agent = GetComponent<NavMeshAgent>();
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
}