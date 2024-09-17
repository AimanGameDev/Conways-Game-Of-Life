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
            depth = 128,
            sumRange = 256,
        };
        Instance.dynamicConfiguration = new ConwaySimulation.DynamicConfiguration
        {
            cellSize = 1.0f,
            spacing = 0.1f,
            minPopulationCutoff = 2,
            maxPopulationThreshold = 3,
            simulationTickRate = 0.015f,
            adjanceLiveCellCountForRevival = 3
        };
    }

    public static ConwaySimulationConfigHolder Instance { get; private set; }
    public ConwaySimulation.DynamicConfiguration dynamicConfiguration;
    public ConwaySimulation.StaticConfiguration staticConfiguration;

    private void OnGUI()
    {
        GUILayout.Label("Conway Job Configuration");

        // Seed
        GUILayout.BeginHorizontal();
        GUILayout.Label("Seed:");
        staticConfiguration.seed = int.Parse(GUILayout.TextField(staticConfiguration.seed.ToString()));
        GUILayout.EndHorizontal();

        // Size
        GUILayout.BeginHorizontal();
        GUILayout.Label("Size:");
        dynamicConfiguration.cellSize = float.Parse(GUILayout.TextField(dynamicConfiguration.cellSize.ToString()));
        GUILayout.EndHorizontal();

        // Width
        GUILayout.BeginHorizontal();
        GUILayout.Label("Width:");
        staticConfiguration.width = int.Parse(GUILayout.TextField(staticConfiguration.width.ToString()));
        GUILayout.EndHorizontal();

        // Height
        GUILayout.BeginHorizontal();
        GUILayout.Label("Height:");
        staticConfiguration.height = int.Parse(GUILayout.TextField(staticConfiguration.height.ToString()));
        GUILayout.EndHorizontal();

        // Depth
        GUILayout.BeginHorizontal();
        GUILayout.Label("Depth:");
        staticConfiguration.depth = int.Parse(GUILayout.TextField(staticConfiguration.depth.ToString()));
        GUILayout.EndHorizontal();


        // SumRange
        GUILayout.BeginHorizontal();
        GUILayout.Label("Sum Range:");
        staticConfiguration.sumRange = int.Parse(GUILayout.TextField(staticConfiguration.sumRange.ToString()));
        GUILayout.EndHorizontal();

        // Spacing
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spacing:");
        dynamicConfiguration.spacing = float.Parse(GUILayout.TextField(dynamicConfiguration.spacing.ToString()));
        GUILayout.EndHorizontal();

        // Min Alive Value
        GUILayout.BeginHorizontal();
        GUILayout.Label("Min Pop Cutoff:");
        dynamicConfiguration.minPopulationCutoff = int.Parse(GUILayout.TextField(dynamicConfiguration.minPopulationCutoff.ToString()));
        GUILayout.EndHorizontal();

        // Max Alive Value
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Pop Cutoff:");
        dynamicConfiguration.maxPopulationThreshold = int.Parse(GUILayout.TextField(dynamicConfiguration.maxPopulationThreshold.ToString()));
        GUILayout.EndHorizontal();

        // Reproduction State Count
        GUILayout.BeginHorizontal();
        GUILayout.Label("Repro Adj Count:");
        dynamicConfiguration.adjanceLiveCellCountForRevival = int.Parse(GUILayout.TextField(dynamicConfiguration.adjanceLiveCellCountForRevival.ToString()));
        GUILayout.EndHorizontal();

        // Spawn Probability
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spawn Probability:");
        staticConfiguration.spawnProbability = int.Parse(GUILayout.TextField(staticConfiguration.spawnProbability.ToString()));
        GUILayout.EndHorizontal();

        // Simulation Interval
        GUILayout.BeginHorizontal();
        GUILayout.Label("Simulation Tickrate:");
        dynamicConfiguration.simulationTickRate = float.Parse(GUILayout.TextField(dynamicConfiguration.simulationTickRate.ToString()));
        GUILayout.EndHorizontal();

        // Restart Button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart Scene"))
        {
            // Add your restart scene logic here
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        GUILayout.EndHorizontal();
    }
}
