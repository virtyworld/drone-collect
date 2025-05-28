/// <summary>
/// GameHandler manages the core game mechanics including drone initialization, resource management,
/// and game state control. It coordinates between different game components like drones, bases,
/// and UI elements.
/// </summary>
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

    /// <summary>
    /// Initializes all game components and sets up event listeners
    /// </summary>
    void Awake()
    {
        InitializeEvents();
        InitUI();
        InitBases();
        InitDrones();
        InitResourceSpawner();
    }

    /// <summary>
    /// Sets up all Unity events used for game communication
    /// </summary>
    private void InitializeEvents()
    {
        onDroneSpeedChanged = new UnityEvent<float>();
        onResourceSpawnIntervalChanged = new UnityEvent<float>();
        onPathVisibleChanged = new UnityEvent<bool>();
        onResourceUnloaded = new UnityEvent<int, int>();
        onDroneCountChanged = new UnityEvent<int>();
        onDroneCountChanged.AddListener(SetDroneCountPerFaction);
    }

    /// <summary>
    /// Initializes the UI system and connects it with game events
    /// </summary>
    private void InitUI()
    {
        uiHandler.Setup(onDroneSpeedChanged, onResourceSpawnIntervalChanged, onPathVisibleChanged, onDroneCountChanged, onResourceUnloaded);
    }

    /// <summary>
    /// Sets up both red and blue bases with their respective team IDs
    /// </summary>
    private void InitBases()
    {
        redBase.Setup(1, onResourceUnloaded);
        blueBase.Setup(2, onResourceUnloaded);
    }

    /// <summary>
    /// Initializes all drones for both teams with their movement and AI components
    /// </summary>
    private void InitDrones()
    {
        InitializeTeamDrones(redDrones, redDronesMovement, redDronesAI, redBase, 1);
        InitializeTeamDrones(blueDrones, blueDronesMovement, blueDronesAI, blueBase, 2);
    }

    /// <summary>
    /// Initializes drones for a specific team
    /// </summary>
    /// <param name="drones">List of drone GameObjects</param>
    /// <param name="movementList">List to store drone movement components</param>
    /// <param name="aiList">List to store drone AI components</param>
    /// <param name="teamBase">The team's base</param>
    /// <param name="teamId">The team's ID (1 for red, 2 for blue)</param>
    private void InitializeTeamDrones(List<GameObject> drones, List<DroneMovement> movementList,
        List<DroneAI> aiList, Base teamBase, int teamId)
    {
        foreach (var drone in drones)
        {
            SetupDroneComponents(drone, movementList, aiList, teamBase, teamId);
        }
    }

    /// <summary>
    /// Sets up individual drone components
    /// </summary>
    private void SetupDroneComponents(GameObject drone, List<DroneMovement> movementList,
        List<DroneAI> aiList, Base teamBase, int teamId)
    {
        DroneMovement droneMovement = drone.GetComponent<DroneMovement>();
        DroneAI droneAI = drone.GetComponent<DroneAI>();

        if (droneMovement != null)
        {
            SetupDroneMovement(droneMovement, movementList);
        }
        if (droneAI != null)
        {
            SetupDroneAI(droneAI, aiList, teamBase, teamId);
        }
    }

    /// <summary>
    /// Sets up drone movement component
    /// </summary>
    private void SetupDroneMovement(DroneMovement droneMovement, List<DroneMovement> movementList)
    {
        droneMovement.Setup(onDroneSpeedChanged, onPathVisibleChanged);
        movementList.Add(droneMovement);
    }

    /// <summary>
    /// Sets up drone AI component
    /// </summary>
    private void SetupDroneAI(DroneAI droneAI, List<DroneAI> aiList, Base teamBase, int teamId)
    {
        droneAI.Setup(teamBase, onResourceUnloaded, teamId);
        aiList.Add(droneAI);
    }

    /// <summary>
    /// Updates the number of active drones for each faction
    /// </summary>
    /// <param name="count">Number of drones to activate per team</param>
    public void SetDroneCountPerFaction(int count)
    {
        DeactivateAllDrones();
        ActivateDronesUpToCount(count);
    }

    /// <summary>
    /// Deactivates all drones for both teams
    /// </summary>
    private void DeactivateAllDrones()
    {
        foreach (var drone in redDrones)
        {
            drone.SetActive(false);
        }
        foreach (var drone in blueDrones)
        {
            drone.SetActive(false);
        }
    }

    /// <summary>
    /// Activates drones up to the specified count for both teams
    /// </summary>
    private void ActivateDronesUpToCount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!redDrones[i].activeSelf)
            {
                redDrones[i].SetActive(true);
                blueDrones[i].SetActive(true);
            }
        }
    }

    /// <summary>
    /// Updates the movement speed for all drones
    /// </summary>
    /// <param name="speed">New speed value</param>
    public void SetAllDronesSpeed(float speed)
    {
        onDroneSpeedChanged.Invoke(speed);
    }

    /// <summary>
    /// Toggles the visibility of drone paths
    /// </summary>
    /// <param name="isVisible">Whether paths should be visible</param>
    public void SetDronePathVisibility(bool isVisible)
    {
        onPathVisibleChanged.Invoke(isVisible);
    }

    /// <summary>
    /// Initializes the resource spawner with game events
    /// </summary>
    private void InitResourceSpawner()
    {
        resourceSpawner.Setup(onResourceSpawnIntervalChanged);
    }
}
