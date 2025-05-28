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

    private void OnDroneCountChanged(float value)
    {
        int count = Mathf.RoundToInt(value);
        onDroneCountChanged.Invoke(count);
    }



    private void OnDroneSpeedChanged(float value)
    {
        onDroneSpeedChanged.Invoke(value);
    }

    private void OnResourceSpawnRateChanged(string valueString)
    {
        Debug.Log("OnResourceSpawnRateChanged: " + valueString);
        // Replace comma with dot for consistent parsing
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

    private void OnShowDronePathToggled(bool isOn)
    {
        onPathVisibleChanged.Invoke(isOn);
    }


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