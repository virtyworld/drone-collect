using UnityEngine;
using UnityEngine.UI; // Для стандартных UI элементов (Slider, Toggle)
using TMPro;          // Для TextMeshPro элементов (TMP_InputField, TMP_Text)

public class UIManager : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private Slider droneCountSlider;
    [SerializeField] private Slider droneSpeedSlider;
    [SerializeField] private TMP_InputField resourceSpawnRateInput;
    [SerializeField] private Toggle showDronePathToggle;
    [SerializeField] private TMP_Text redFactionScoreText;
    [SerializeField] private TMP_Text blueFactionScoreText;

    private GameHandler gameHandler;
    public void Setup(GameHandler gameHandler)
    {
        this.gameHandler = gameHandler;
    }
    void Start()
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
        gameHandler.SetDroneCountPerFaction(count);
    }



    private void OnDroneSpeedChanged(float value)
    {
        if (gameHandler != null)
            gameHandler.SetAllDronesSpeed(value);
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
                gameHandler.SetResourceSpawnInterval(interval);
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
        gameHandler.SetDronePathVisibility(isOn);
    }


    public void UpdateFactionScoreUI(int factionId, int score)
    {
        if (factionId == 1 && redFactionScoreText != null)
        {
            redFactionScoreText.text = $"{score}";
        }
        else if (factionId == 2 && blueFactionScoreText != null)
        {
            blueFactionScoreText.text = $"{score}";
        }
    }

    void OnDestroy()
    {
        if (droneCountSlider != null)
            droneCountSlider.onValueChanged.RemoveListener(OnDroneCountChanged);
        if (droneSpeedSlider != null)
            droneSpeedSlider.onValueChanged.RemoveListener(OnDroneSpeedChanged);
        if (resourceSpawnRateInput != null)
            resourceSpawnRateInput.onEndEdit.RemoveListener(OnResourceSpawnRateChanged);
        if (showDronePathToggle != null)
            showDronePathToggle.onValueChanged.RemoveListener(OnShowDronePathToggled);
    }
}