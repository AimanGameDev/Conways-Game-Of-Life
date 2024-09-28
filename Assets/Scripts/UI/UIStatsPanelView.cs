using TMPro;
using UnityEngine;

public class UIStatsPanelView : MonoBehaviour
{
    public ConwaySimulation conwaySimulation;
    public FlyCamera flyCamera;

    public TextMeshProUGUI fpsLabel;
    public TextMeshProUGUI maxCountLabel;
    public TextMeshProUGUI generationLabel;
    public TextMeshProUGUI aliveCellsLabel;
    public TextMeshProUGUI cameraSpeedLabel;
    public TextMeshProUGUI cameraPosXLabel;
    public TextMeshProUGUI cameraPosYLabel;
    public TextMeshProUGUI cameraPosZLabel;

    private int m_fpsAccumulator;
    private float m_fpsNextCapturePeriod;
    private const float FPS_MEASURE_PERIOD = 0.5f;

    void Awake()
    {
        m_fpsAccumulator = 0;
        m_fpsNextCapturePeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
    }

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
        cameraPosXLabel.SetText($"X: {flyCamera.transform.position.x:N2}");
        cameraPosYLabel.SetText($"Y: {flyCamera.transform.position.y:N2}");
        cameraPosZLabel.SetText($"Z: {flyCamera.transform.position.z:N2}");

        m_fpsAccumulator++;
        if (Time.realtimeSinceStartup > m_fpsNextCapturePeriod)
        {
            var fps = (int)(m_fpsAccumulator / FPS_MEASURE_PERIOD);
            fpsLabel.SetText($"FPS: {fps}");
            m_fpsAccumulator = 0;
            m_fpsNextCapturePeriod += FPS_MEASURE_PERIOD;
        }

        if (Time.frameCount % 5 == 0)
        {
            var cameraSpeed = flyCamera.speed * 3.6f; // 3.6f to convert m/s to km/h
            var cameraSpeedText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Camera Speed: {0:N0} km/h", cameraSpeed);
            cameraSpeedLabel.SetText(cameraSpeedText);
        }
    }
}
