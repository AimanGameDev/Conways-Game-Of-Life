using TMPro;
using UnityEngine;

public class UIStatsPanelView : MonoBehaviour
{
    public ConwaySimulation conwaySimulation;

    public TextMeshProUGUI fpsLabel;
    public TextMeshProUGUI maxCountLabel;
    public TextMeshProUGUI generationLabel;
    public TextMeshProUGUI aliveCellsLabel;

    void Update()
    {
        var fps = 1 / Time.deltaTime;
        var maxCount = conwaySimulation.maxCount;
        var generation = conwaySimulation.generationCount;
        var aliveCells = conwaySimulation.aliveCellsCount;

        var fpsText = $"FPS: {(int)fps}";
        var maxCountText = $"Max Count: {maxCount:N0}";
        var generationText = $"Generation: {generation:N0}";
        var aliveCellsText = $"Alive Cells: {aliveCells:N0}";

        fpsLabel.SetText(fpsText);
        maxCountLabel.SetText(maxCountText);
        generationLabel.SetText(generationText);
        aliveCellsLabel.SetText(aliveCellsText);
    }
}
