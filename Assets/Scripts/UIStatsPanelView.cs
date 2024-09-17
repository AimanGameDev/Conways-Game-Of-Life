using TMPro;
using UnityEngine;

public class UIStatsPanelView : MonoBehaviour
{
    public ConwaySimulation conwaySimulation;

    public TextMeshProUGUI fpsLabel;
    public TextMeshProUGUI maxCountLabel;
    public TextMeshProUGUI generationLabel;
    public TextMeshProUGUI aliveCellsLabel;

    private float m_frameTime;
    private float m_frameCaptureRate = 10;

    void Update()
    {
        var maxCount = conwaySimulation.maxCount;
        var generation = conwaySimulation.generationCount;
        var aliveCells = conwaySimulation.aliveCellsCount;

        var maxCountText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Max Count: {0:N0}", maxCount);
        var generationText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Generation: {0:N0}", generation);
        var aliveCellsText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Alive Cells: {0:N0}", aliveCells);

        maxCountLabel.SetText(maxCountText);
        generationLabel.SetText(generationText);
        aliveCellsLabel.SetText(aliveCellsText);

        m_frameTime += 1f / Time.deltaTime;
        if (Time.frameCount % m_frameCaptureRate == 0)
        {
            var fps = m_frameTime / m_frameCaptureRate;
            var fpsText = $"FPS: {(int)fps}";
            fpsLabel.SetText(fpsText);
            m_frameTime = 0;
        }
    }
}
