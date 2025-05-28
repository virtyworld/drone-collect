using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> redDrones = new List<GameObject>();
    [SerializeField] private List<GameObject> blueDrones = new List<GameObject>();
    [SerializeField] private UIManager uiHandler;
    [SerializeField] private ResourceSpawner resourceSpawner;
    [SerializeField] private Base redBase;
    [SerializeField] private Base blueBase;


    private UnityEvent<float> onDroneSpeedChanged;
    private UnityEvent<float> onResourceSpawnIntervalChanged;
    private UnityEvent<bool> onPathVisibleChanged;
    public UnityEvent<int, int> onResourceUnloaded;
    public UnityEvent<int> onDroneCountChanged;
    private List<DroneMovement> redDronesMovement = new List<DroneMovement>();
    private List<DroneMovement> blueDronesMovement = new List<DroneMovement>();
    private List<DroneAI> redDronesAI = new List<DroneAI>();
    private List<DroneAI> blueDronesAI = new List<DroneAI>();

    void Awake()
    {
        onDroneSpeedChanged = new UnityEvent<float>();
        onResourceSpawnIntervalChanged = new UnityEvent<float>();
        onPathVisibleChanged = new UnityEvent<bool>();
        onResourceUnloaded = new UnityEvent<int, int>();
        onDroneCountChanged = new UnityEvent<int>();
        onDroneCountChanged.AddListener(SetDroneCountPerFaction);
        InitUI();
        InitBases();
        InitDrones();
        InitResourceSpawner();
    }


    private void InitUI()
    {
        uiHandler.Setup(onDroneSpeedChanged, onResourceSpawnIntervalChanged, onPathVisibleChanged, onDroneCountChanged, onResourceUnloaded);
    }
    private void InitBases()
    {
        redBase.Setup(1, onResourceUnloaded);
        blueBase.Setup(2, onResourceUnloaded);
    }
    private void InitDrones()
    {
        foreach (var drone in redDrones)
        {
            DroneMovement droneMovement = drone.GetComponent<DroneMovement>();
            DroneAI droneAI = drone.GetComponent<DroneAI>();
            if (droneMovement != null)
            {
                droneMovement.Setup(onDroneSpeedChanged, onPathVisibleChanged);
                redDronesMovement.Add(droneMovement);
            }
            if (droneAI != null)
            {
                droneAI.Setup(redBase, onResourceUnloaded, 1);
                redDronesAI.Add(droneAI);
            }
        }
        foreach (var drone in blueDrones)
        {
            DroneMovement droneMovement = drone.GetComponent<DroneMovement>();
            DroneAI droneAI = drone.GetComponent<DroneAI>();
            if (droneMovement != null)
            {
                droneMovement.Setup(onDroneSpeedChanged, onPathVisibleChanged);
                blueDronesMovement.Add(droneMovement);
            }
            if (droneAI != null)
            {
                droneAI.Setup(blueBase, onResourceUnloaded, 2);
                blueDronesAI.Add(droneAI);
            }
        }
    }



    public void SetDroneCountPerFaction(int count)
    {
        foreach (var drone in redDrones)
        {
            drone.SetActive(false);
        }
        foreach (var drone in blueDrones)
        {
            drone.SetActive(false);
        }
        for (int i = 0; i < count; i++)
        {
            if (!redDrones[i].activeSelf)
            {
                redDrones[i].SetActive(true);
                blueDrones[i].SetActive(true);
            }
        }

    }

    public void SetAllDronesSpeed(float speed)
    {
        onDroneSpeedChanged.Invoke(speed);
    }

    public void SetDronePathVisibility(bool isVisible)
    {
        onPathVisibleChanged.Invoke(isVisible);
    }
    private void InitResourceSpawner()
    {
        resourceSpawner.Setup(onResourceSpawnIntervalChanged);
    }
}
