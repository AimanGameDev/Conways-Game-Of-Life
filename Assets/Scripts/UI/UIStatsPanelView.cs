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

    private float[] m_frameSamples;
    private int m_frameSampleIndex;
    private const int FRAME_SAMPLE_COUNT = 16;

    void Awake()
    {
        m_frameSamples = new float[FRAME_SAMPLE_COUNT];
        for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
        {
            m_frameSamples[i] = 0;
        }
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

        m_frameSamples[m_frameSampleIndex] = 1f / Time.deltaTime;
        m_frameSampleIndex = (m_frameSampleIndex + 1) % FRAME_SAMPLE_COUNT;

        var sum = 0f;
        for (var i = 0; i < FRAME_SAMPLE_COUNT; i++)
        {
            sum += m_frameSamples[i];
        }

        if (Time.frameCount % 5 == 0)
        {
            var fpsText = $"FPS: {(int)(sum / FRAME_SAMPLE_COUNT)}";
            fpsLabel.SetText(fpsText);

            var cameraSpeed = flyCamera.speed * 3.6f; // 3.6f to convert m/s to km/h
            var cameraSpeedText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Camera Speed: {0:N0} km/h", cameraSpeed);
            cameraSpeedLabel.SetText(cameraSpeedText);
        }
    }
}
