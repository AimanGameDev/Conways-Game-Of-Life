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
        public int defaultValue;
        public int minValue;
        public int maxValue;
        public string valueSuffix;
    }

    [Serializable]
    public struct FloatSettingConfiguration
    {
        public float defaultValue;
        public float minValue;
        public float maxValue;
        public string valueSuffix;
    }

    [NonSerialized]
    public ConwaySimulationConfigHolder configHolder;

    public Button collapseButton;
    public GameObject content;

    public Button restartSceneButton;

    [Header("Static Settings")]
    public IntSettingConfiguration seedSettingConfiguration;
    public IntSettingConfiguration spawnProbabilitySettingConfiguration;
    public IntSettingConfiguration widthSettingConfiguration;
    public IntSettingConfiguration heightSettingConfiguration;
    public IntSettingConfiguration depthSettingConfiguration;

    [Header("Dynamic Settings")]
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

        collapseButton.onClick.AddListener(() =>
        {
            content.SetActive(!content.activeSelf);
        });

        restartSceneButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        InitializeSetting(seedSetting.settingReferences, seedSettingConfiguration);
        InitializeSetting(spawnProbabilitySetting.settingReferences, spawnProbabilitySettingConfiguration);
        InitializeSetting(widthSetting.settingReferences, widthSettingConfiguration);
        InitializeSetting(heightSetting.settingReferences, heightSettingConfiguration);
        InitializeSetting(depthSetting.settingReferences, depthSettingConfiguration);
        InitializeSetting(cellSizeSetting.settingReferences, cellSizeSettingConfiguration);
        InitializeSetting(spacingSetting.settingReferences, spacingSettingConfiguration);
        InitializeSetting(simulationTickRateSetting.settingReferences, simulationTickRateSettingConfiguration);
        InitializeSetting(minPopulationCutoffSetting.settingReferences, minPopulationCutoffSettingConfiguration);
        InitializeSetting(maxPopulationCutoffSetting.settingReferences, maxPopulationCutoffSettingConfiguration);
        InitializeSetting(birthThresholdSetting.settingReferences, birthThresholdSettingConfiguration);

        seedSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.staticConfiguration.seed);
        seedSetting.settingReferences.value.SetText($"{configHolder.staticConfiguration.seed:N0} {seedSettingConfiguration.valueSuffix}");

        spawnProbabilitySetting.settingReferences.slider.SetValueWithoutNotify(configHolder.staticConfiguration.spawnProbability);
        spawnProbabilitySetting.settingReferences.value.SetText($"{configHolder.staticConfiguration.spawnProbability:N0} {spawnProbabilitySettingConfiguration.valueSuffix}");

        widthSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.staticConfiguration.width);
        widthSetting.settingReferences.value.SetText($"{configHolder.staticConfiguration.width:N0} {widthSettingConfiguration.valueSuffix}");

        heightSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.staticConfiguration.height);
        heightSetting.settingReferences.value.SetText($"{configHolder.staticConfiguration.height:N0} {heightSettingConfiguration.valueSuffix}");

        depthSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.staticConfiguration.depth);
        depthSetting.settingReferences.value.SetText($"{configHolder.staticConfiguration.depth:N0} {depthSettingConfiguration.valueSuffix}");

        cellSizeSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.cellSize);
        cellSizeSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.cellSize:N2} {cellSizeSettingConfiguration.valueSuffix}");

        spacingSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.spacing);
        spacingSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.spacing:N2} {spacingSettingConfiguration.valueSuffix}");

        simulationTickRateSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.simulationTickRate);
        simulationTickRateSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.simulationTickRate:N2} {simulationTickRateSettingConfiguration.valueSuffix}");

        minPopulationCutoffSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.minPopulationCutoff);
        minPopulationCutoffSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.minPopulationCutoff:N0} {minPopulationCutoffSettingConfiguration.valueSuffix}");

        maxPopulationCutoffSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.maxPopulationThreshold);
        maxPopulationCutoffSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.maxPopulationThreshold:N0} {maxPopulationCutoffSettingConfiguration.valueSuffix}");

        birthThresholdSetting.settingReferences.slider.SetValueWithoutNotify(configHolder.dynamicConfiguration.adjanceLiveCellCountForRevival);
        birthThresholdSetting.settingReferences.value.SetText($"{configHolder.dynamicConfiguration.adjanceLiveCellCountForRevival:N0} {birthThresholdSettingConfiguration.valueSuffix}");
    }

    private void ApplyConfigs()
    {
        configHolder.staticConfiguration.seed = (int)seedSetting.settingReferences.slider.value;
        configHolder.staticConfiguration.spawnProbability = (int)spawnProbabilitySetting.settingReferences.slider.value;
        configHolder.staticConfiguration.width = (int)widthSetting.settingReferences.slider.value;
        configHolder.staticConfiguration.height = (int)heightSetting.settingReferences.slider.value;
        configHolder.staticConfiguration.depth = (int)depthSetting.settingReferences.slider.value;

        configHolder.dynamicConfiguration.cellSize = cellSizeSetting.settingReferences.slider.value;
        configHolder.dynamicConfiguration.spacing = spacingSetting.settingReferences.slider.value;
        configHolder.dynamicConfiguration.simulationTickRate = simulationTickRateSetting.settingReferences.slider.value;
        configHolder.dynamicConfiguration.minPopulationCutoff = (int)minPopulationCutoffSetting.settingReferences.slider.value;
        configHolder.dynamicConfiguration.maxPopulationThreshold = (int)maxPopulationCutoffSetting.settingReferences.slider.value;
        configHolder.dynamicConfiguration.adjanceLiveCellCountForRevival = (int)birthThresholdSetting.settingReferences.slider.value;
    }

    private void InitializeSetting(SettingReferences setting, FloatSettingConfiguration configuration)
    {
        setting.slider.minValue = configuration.minValue;
        setting.slider.maxValue = configuration.maxValue;
        setting.slider.wholeNumbers = false;
        setting.slider.SetValueWithoutNotify(configuration.defaultValue);
        setting.value.SetText($"{configuration.defaultValue:N2} {configuration.valueSuffix}");
        setting.slider.onValueChanged.AddListener((value) =>
        {
            setting.value.SetText($"{value:N2} {configuration.valueSuffix}");
            ApplyConfigs();
        });
    }

    private void InitializeSetting(SettingReferences setting, IntSettingConfiguration configuration)
    {
        setting.slider.minValue = configuration.minValue;
        setting.slider.maxValue = configuration.maxValue;
        setting.slider.wholeNumbers = true;
        setting.slider.SetValueWithoutNotify(configuration.defaultValue);
        setting.value.SetText($"{configuration.defaultValue:N0} {configuration.valueSuffix}");
        setting.slider.onValueChanged.AddListener((value) =>
        {
            if (configuration.enforcePowerOfTwo)
            {
                value = Mathf.Pow(2, Mathf.Round(Mathf.Log(value) / Mathf.Log(2)));
                setting.slider.SetValueWithoutNotify(value);
            }

            setting.value.SetText($"{value:N0} {configuration.valueSuffix}");
            ApplyConfigs();
        });
    }
}
