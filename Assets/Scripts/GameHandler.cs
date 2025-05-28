using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> redDrones = new List<GameObject>();
    [SerializeField] private List<GameObject> blueDrones = new List<GameObject>();
    [SerializeField] private UIManager uiHandler;
    [SerializeField] private ResourceSpawner resourceSpawner;

    private UnityAction<float> onDroneSpeedChanged;
    private UnityAction<float> onResourceSpawnIntervalChanged;
    private UnityAction<bool> onPathVisibleChanged;
    private List<DroneMovement> redDronesMovement;
    private List<DroneMovement> blueDronesMovement;

    void Awake()
    {
        redDronesMovement = new List<DroneMovement>();
        blueDronesMovement = new List<DroneMovement>();
        onDroneSpeedChanged = new UnityAction<float>(speed => { });
        onResourceSpawnIntervalChanged = new UnityAction<float>(interval => { });
        onPathVisibleChanged = new UnityAction<bool>(isVisible => { });
        InitUI();
        InitDrones();
    }


    private void InitUI()
    {
        uiHandler.Setup(this);
    }
    private void InitDrones()
    {
        foreach (var drone in redDrones)
        {
            DroneMovement droneMovement = drone.GetComponent<DroneMovement>();
            if (droneMovement != null)
            {
                onDroneSpeedChanged += droneMovement.SetDroneSpeed;
                onPathVisibleChanged += droneMovement.SetPathVisible;
                redDronesMovement.Add(droneMovement);
            }
        }
        foreach (var drone in blueDrones)
        {
            DroneMovement droneMovement = drone.GetComponent<DroneMovement>();
            if (droneMovement != null)
            {
                onPathVisibleChanged += droneMovement.SetPathVisible;
                onDroneSpeedChanged += droneMovement.SetDroneSpeed;
                blueDronesMovement.Add(droneMovement);
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

    public void SetResourceSpawnInterval(float interval)
    {
        resourceSpawner.UpdateSpawnInterval(interval);
    }

    public void SetDronePathVisibility(bool isVisible)
    {
        onPathVisibleChanged.Invoke(isVisible);
    }
}
