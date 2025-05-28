/// <summary>
/// Manages the user interface for a drone simulation game.
/// Handles UI elements including drone controls, resource management, and faction scoring.
/// Uses Unity's UI system and TextMeshPro for text rendering.
/// </summary>
using UnityEngine;
using UnityEngine.UI; // Для стандартных UI элементов (Slider, Toggle)
using TMPro;
using UnityEngine.Events;          // Для TextMeshPro элементов (TMP_InputField, TMP_Text)

public class UIManager : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private Slider droneCountSlider;
    [SerializeField] private Slider droneSpeedSlider;
    [SerializeField] private TMP_InputField resourceSpawnRateInput;
    [SerializeField] private Toggle showDronePathToggle;
    [SerializeField] private TMP_Text redFactionScoreText;
    [SerializeField] private TMP_Text blueFactionScoreText;

    private UnityEvent<float> onDroneSpeedChanged;
    private UnityEvent<float> onResourceSpawnIntervalChanged;
    private UnityEvent<bool> onPathVisibleChanged;
    private UnityEvent<int> onDroneCountChanged;
    public UnityEvent<int, int> onResourceUnloaded;
    private int currentRedScore = 0;
    private int currentBlueScore = 0;

    /// <summary>
    /// Initializes the UI manager with event handlers for various game parameters.
    /// Sets up listeners for drone speed, resource spawn interval, path visibility, drone count, and resource unloading.
    /// </summary>
    public void Setup(UnityEvent<float> onDroneSpeedChanged, UnityEvent<float> onResourceSpawnIntervalChanged,
    UnityEvent<bool> onPathVisibleChanged, UnityEvent<int> onDroneCountChanged, UnityEvent<int, int> onResourceUnloaded)
    {
        this.onDroneSpeedChanged = onDroneSpeedChanged;
        this.onResourceSpawnIntervalChanged = onResourceSpawnIntervalChanged;
        this.onPathVisibleChanged = onPathVisibleChanged;
        this.onDroneCountChanged = onDroneCountChanged;
        onResourceUnloaded.AddListener(UpdateFactionScoreUI);
        Initialize();
    }

    /// <summary>
    /// Sets up event listeners for all UI elements and initializes faction score displays.
    /// </summary>
    void Initialize()
    {
        droneCountSlider.onValueChanged.AddListener(OnDroneCountChanged);
        droneSpeedSlider.onValueChanged.AddListener(OnDroneSpeedChanged);
        resourceSpawnRateInput.onEndEdit.AddListener(OnResourceSpawnRateChanged);
        showDronePathToggle.onValueChanged.AddListener(OnShowDronePathToggled);

        // Инициализация текстов счета
        UpdateFactionScoreUI(1, 0); // Начальный счет для фракции 1
        UpdateFactionScoreUI(2, 0); // Начальный счет для фракции 2
    }

    /// <summary>
    /// Handles changes to the drone count slider.
    /// Converts the float value to an integer and invokes the drone count change event.
    /// </summary>
    private void OnDroneCountChanged(float value)
    {
        int count = Mathf.RoundToInt(value);
        onDroneCountChanged.Invoke(count);
    }

    /// <summary>
    /// Handles changes to the drone speed slider.
    /// Invokes the drone speed change event with the new value.
    /// </summary>
    private void OnDroneSpeedChanged(float value)
    {
        onDroneSpeedChanged.Invoke(value);
    }

    /// <summary>
    /// Processes changes to the resource spawn rate input field.
    /// Validates the input, converts it to a float, and invokes the spawn interval change event.
    /// Includes error handling for invalid inputs.
    /// </summary>
    private void OnResourceSpawnRateChanged(string valueString)
    {
        valueString = valueString.Replace(',', '.');
        if (float.TryParse(valueString, out float interval))
        {
            if (interval > 0)
            {
                Debug.Log("OnResourceSpawnRateChanged: " + interval);
                onResourceSpawnIntervalChanged.Invoke(interval);
            }
            else
            {
                Debug.LogWarning("Интервал спавна ресурсов должен быть положительным числом.");
            }
        }
        else
        {
            Debug.LogWarning("Некорректный ввод для интервала спавна ресурсов.");
        }
    }

    /// <summary>
    /// Handles the drone path visibility toggle.
    /// Invokes the path visibility change event with the new state.
    /// </summary>
    private void OnShowDronePathToggled(bool isOn)
    {
        onPathVisibleChanged.Invoke(isOn);
    }

    /// <summary>
    /// Updates the score display for either the red (faction 1) or blue (faction 2) team.
    /// Maintains running totals for each faction's score.
    /// </summary>
    public void UpdateFactionScoreUI(int factionId, int score)
    {
        if (factionId == 1 && redFactionScoreText != null)
        {
            currentRedScore += score;
            redFactionScoreText.text = $"{currentRedScore}";
        }
        else if (factionId == 2 && blueFactionScoreText != null)
        {
            currentBlueScore += score;
            blueFactionScoreText.text = $"{currentBlueScore}";
        }
    }
}