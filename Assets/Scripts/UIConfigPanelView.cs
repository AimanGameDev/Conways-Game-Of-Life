using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfigPanelView : MonoBehaviour
{
    [Serializable]
    public struct SettingReferences
    {
        public TextMeshProUGUI label;
        public Slider slider;
        public TextMeshProUGUI value;
    }

    [Serializable]
    public struct IntSettingConfiguration
    {
        public bool enforcePowerOfTwo;
        public int minValue;
        public int maxValue;
        public string valueSuffix;
    }

    [Serializable]
    public struct FloatSettingConfiguration
    {
        public float minValue;
        public float maxValue;
        public string valueSuffix;
    }

    [NonSerialized]
    public ConwaySimulationConfigHolder configHolder;

    public Button collapseButton;
    public TextMeshProUGUI collapseButtonLabel;
    public GameObject content;

    public Button restartSceneButton;

    [Header("Static Settings")]
    public Toggle deferredTicksToggle;
    public Toggle useQuadsToggle;
    public IntSettingConfiguration seedSettingConfiguration;
    public IntSettingConfiguration spawnProbabilitySettingConfiguration;
    public IntSettingConfiguration widthSettingConfiguration;
    public IntSettingConfiguration heightSettingConfiguration;
    public IntSettingConfiguration depthSettingConfiguration;

    [Header("Dynamic Settings")]
    public Toggle renderToggle;
    public FloatSettingConfiguration cellSizeSettingConfiguration;
    public FloatSettingConfiguration spacingSettingConfiguration;
    public FloatSettingConfiguration simulationTickRateSettingConfiguration;
    public IntSettingConfiguration minPopulationCutoffSettingConfiguration;
    public IntSettingConfiguration maxPopulationCutoffSettingConfiguration;
    public IntSettingConfiguration birthThresholdSettingConfiguration;

    [Header("References")]
    public UISettingReferenceHolder seedSetting;
    public UISettingReferenceHolder spawnProbabilitySetting;
    public UISettingReferenceHolder widthSetting;
    public UISettingReferenceHolder heightSetting;
    public UISettingReferenceHolder depthSetting;
    public UISettingReferenceHolder cellSizeSetting;
    public UISettingReferenceHolder spacingSetting;
    public UISettingReferenceHolder simulationTickRateSetting;
    public UISettingReferenceHolder minPopulationCutoffSetting;
    public UISettingReferenceHolder maxPopulationCutoffSetting;
    public UISettingReferenceHolder birthThresholdSetting;

    private void Start()
    {
        configHolder = FindObjectOfType<ConwaySimulationConfigHolder>();

        collapseButtonLabel.SetText(content.activeSelf ? "▼" : "▲");
        collapseButton.onClick.AddListener(() =>
        {
            var result = !content.activeSelf;
            collapseButtonLabel.SetText(result ? "▼" : "▲");
            content.SetActive(result);
        });

        restartSceneButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        deferredTicksToggle.isOn = configHolder.staticConfiguration.deferredUpdate;
        deferredTicksToggle.onValueChanged.AddListener((value) =>
        {
            configHolder.staticConfiguration.deferredUpdate = value;
        });

        useQuadsToggle.isOn = configHolder.staticConfiguration.useQuads;
        useQuadsToggle.onValueChanged.AddListener((value) =>
        {
            configHolder.staticConfiguration.useQuads = value;
        });

        renderToggle.isOn = configHolder.dynamicConfiguration.canRender;
        renderToggle.onValueChanged.AddListener((value) =>
        {
            configHolder.dynamicConfiguration.canRender = value;
        });

        InitializeSetting(seedSetting.settingReferences, seedSettingConfiguration, configHolder.staticConfiguration.seed);
        InitializeSetting(spawnProbabilitySetting.settingReferences, spawnProbabilitySettingConfiguration, configHolder.staticConfiguration.spawnProbability);
        InitializeSetting(widthSetting.settingReferences, widthSettingConfiguration, configHolder.staticConfiguration.width);
        InitializeSetting(heightSetting.settingReferences, heightSettingConfiguration, configHolder.staticConfiguration.height);
        InitializeSetting(depthSetting.settingReferences, depthSettingConfiguration, configHolder.staticConfiguration.depth);
        InitializeSetting(cellSizeSetting.settingReferences, cellSizeSettingConfiguration, configHolder.dynamicConfiguration.cellSize);
        InitializeSetting(spacingSetting.settingReferences, spacingSettingConfiguration, configHolder.dynamicConfiguration.spacing);
        InitializeSetting(simulationTickRateSetting.settingReferences, simulationTickRateSettingConfiguration, configHolder.dynamicConfiguration.simulationTickRate);
        InitializeSetting(minPopulationCutoffSetting.settingReferences, minPopulationCutoffSettingConfiguration, configHolder.dynamicConfiguration.minPopulationCutoff);
        InitializeSetting(maxPopulationCutoffSetting.settingReferences, maxPopulationCutoffSettingConfiguration, configHolder.dynamicConfiguration.maxPopulationThreshold);
        InitializeSetting(birthThresholdSetting.settingReferences, birthThresholdSettingConfiguration, configHolder.dynamicConfiguration.adjanceLiveCellCountForRevival);
    }

    private void ApplyConfigs()
    {
        var seed = (int)seedSetting.settingReferences.slider.value;
        var spawnProbability = (int)spawnProbabilitySetting.settingReferences.slider.value;
        var width = (int)widthSetting.settingReferences.slider.value;
        var height = (int)heightSetting.settingReferences.slider.value;
        var depth = (int)depthSetting.settingReferences.slider.value;

        var cellSize = cellSizeSetting.settingReferences.slider.value;
        var spacing = spacingSetting.settingReferences.slider.value;
        var simulationTickRate = simulationTickRateSetting.settingReferences.slider.value;
        var minPopulationCutoff = (int)minPopulationCutoffSetting.settingReferences.slider.value;
        var maxPopulationThreshold = (int)maxPopulationCutoffSetting.settingReferences.slider.value;
        var adjanceLiveCellCountForRevival = (int)birthThresholdSetting.settingReferences.slider.value;

        if (seedSettingConfiguration.enforcePowerOfTwo)
            seed = (int)Mathf.Pow(2, seed);

        if (spawnProbabilitySettingConfiguration.enforcePowerOfTwo)
            spawnProbability = (int)Mathf.Pow(2, spawnProbability);

        if (widthSettingConfiguration.enforcePowerOfTwo)
            width = (int)Mathf.Pow(2, width);

        if (heightSettingConfiguration.enforcePowerOfTwo)
            height = (int)Mathf.Pow(2, height);

        if (depthSettingConfiguration.enforcePowerOfTwo)
            depth = (int)Mathf.Pow(2, depth);

        if (minPopulationCutoffSettingConfiguration.enforcePowerOfTwo)
            minPopulationCutoff = (int)Mathf.Pow(2, minPopulationCutoff);

        if (maxPopulationCutoffSettingConfiguration.enforcePowerOfTwo)
            maxPopulationThreshold = (int)Mathf.Pow(2, maxPopulationThreshold);

        if (birthThresholdSettingConfiguration.enforcePowerOfTwo)
            adjanceLiveCellCountForRevival = (int)Mathf.Pow(2, adjanceLiveCellCountForRevival);

        configHolder.staticConfiguration.seed = seed;
        configHolder.staticConfiguration.spawnProbability = spawnProbability;
        configHolder.staticConfiguration.width = width;
        configHolder.staticConfiguration.height = height;
        configHolder.staticConfiguration.depth = depth;

        configHolder.dynamicConfiguration.cellSize = cellSize;
        configHolder.dynamicConfiguration.spacing = spacing;
        configHolder.dynamicConfiguration.simulationTickRate = simulationTickRate;
        configHolder.dynamicConfiguration.minPopulationCutoff = minPopulationCutoff;
        configHolder.dynamicConfiguration.maxPopulationThreshold = maxPopulationThreshold;
        configHolder.dynamicConfiguration.adjanceLiveCellCountForRevival = adjanceLiveCellCountForRevival;
    }

    private void InitializeSetting(SettingReferences setting, FloatSettingConfiguration configuration, float defaultValue)
    {
        setting.slider.minValue = configuration.minValue;
        setting.slider.maxValue = configuration.maxValue;
        setting.slider.wholeNumbers = false;
        setting.slider.SetValueWithoutNotify(defaultValue);
        setting.value.SetText($"{defaultValue:N3} {configuration.valueSuffix}");
        setting.slider.onValueChanged.AddListener((value) =>
        {
            setting.value.SetText($"{value:N3} {configuration.valueSuffix}");
            ApplyConfigs();
        });
    }

    private void InitializeSetting(SettingReferences setting, IntSettingConfiguration configuration, int defaultValue)
    {
        if (configuration.enforcePowerOfTwo)
        {
            configuration.minValue = (int)Mathf.Log(configuration.minValue, 2);
            configuration.maxValue = (int)Mathf.Log(configuration.maxValue, 2);
            defaultValue = (int)Mathf.Log(defaultValue, 2);
        }

        setting.slider.minValue = configuration.minValue;
        setting.slider.maxValue = configuration.maxValue;
        setting.slider.wholeNumbers = true;
        setting.slider.SetValueWithoutNotify(defaultValue);
        var valueText = configuration.enforcePowerOfTwo ? Mathf.Pow(2, defaultValue) : defaultValue;
        setting.value.SetText($"{valueText:N0} {configuration.valueSuffix}");
        setting.slider.onValueChanged.AddListener((value) =>
        {
            var valueText = configuration.enforcePowerOfTwo ? Mathf.Pow(2, value) : value;
            setting.value.SetText($"{valueText:N0} {configuration.valueSuffix}");
            ApplyConfigs();
        });
    }
}
