using UnityEngine;

public class DebugGUI : MonoBehaviour
{
    public ConwaySimulation conwaySimulation;

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 150, 10, 150, 20), $"FPS: {(int)(1 / Time.deltaTime)}");
        GUI.Label(new Rect(Screen.width - 150, 30, 150, 20), $"Max Count: {conwaySimulation.width * conwaySimulation.height * conwaySimulation.depth}");
        GUI.Label(new Rect(Screen.width - 150, 50, 150, 20), $"Generation: {conwaySimulation.generationCount}");
        GUI.Label(new Rect(Screen.width - 150, 70, 150, 20), $"Alive Cells: {conwaySimulation.aliveCellsCount}");
    }
}
