using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ConwaySimulationConfigHolder : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var go = new GameObject("ConwaySimulationConfigHolder");
        DontDestroyOnLoad(go);
        var conwaySimulationConfigHolder = go.AddComponent<ConwaySimulationConfigHolder>();
        Instance = conwaySimulationConfigHolder;
        Instance.staticConfiguration = new ConwaySimulation.StaticConfiguration
        {
            seed = 0,
            spawnProbability = 8,
            width = 128,
            height = 128,
            depth = 1,
            deferredUpdate = true,
        };
        Instance.dynamicConfiguration = new ConwaySimulation.DynamicConfiguration
        {
            cellSize = 1.0f,
            spacing = 0.1f,
            minPopulationCutoff = 2,
            maxPopulationThreshold = 3,
            simulationTickRate = 0.015f,
            adjanceLiveCellCountForRevival = 3,
            canRender = true,
        };
    }

    public static ConwaySimulationConfigHolder Instance { get; private set; }
    public ConwaySimulation.DynamicConfiguration dynamicConfiguration;
    public ConwaySimulation.StaticConfiguration staticConfiguration;
}
