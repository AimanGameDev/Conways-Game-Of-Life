using UnityEngine;

public class CubeRenderer : MonoBehaviour
{
    public ConwaySimulation conwaySimulation;
    public Material material;
    public Mesh cubeMesh;
    public Mesh quadMesh;

    private ComputeBuffer m_statesBuffer;
    private GraphicsBuffer m_commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private RenderParams m_renderParams;

    private const int COMMAND_COUNT = 1;
    private Mesh m_mesh;

    public struct MaterialPropertiesInfo
    {
        public const string OBJECT_TO_WORLD = "_ObjectToWorld";
        public const string WIDTH = "_Width";
        public const string HEIGHT = "_Height";
        public const string DEPTH = "_Depth";
        public const string SPACING = "_Spacing";
        public const string SIZE = "_Size";
        public const string STATES_LENGTH = "_StatesLength";
        public const string STATES = "_States";
    }

    private void Start()
    {
        m_mesh = conwaySimulation.useQuads ? quadMesh : cubeMesh;
        m_commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, COMMAND_COUNT, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[COMMAND_COUNT];
        m_statesBuffer = new ComputeBuffer(conwaySimulation.maxCount, sizeof(int));

        m_renderParams = new RenderParams(material);
        m_renderParams.worldBounds = new Bounds(conwaySimulation.center, conwaySimulation.boundsSize);
        m_renderParams.matProps = new MaterialPropertyBlock();
        m_renderParams.matProps.SetMatrix(MaterialPropertiesInfo.OBJECT_TO_WORLD, Matrix4x4.Translate(new Vector3(0f, 0f, 0f)));
        m_renderParams.matProps.SetInt(MaterialPropertiesInfo.WIDTH, conwaySimulation.width);
        m_renderParams.matProps.SetInt(MaterialPropertiesInfo.HEIGHT, conwaySimulation.height);
        m_renderParams.matProps.SetInt(MaterialPropertiesInfo.DEPTH, conwaySimulation.depth);
        m_renderParams.matProps.SetInt(MaterialPropertiesInfo.STATES_LENGTH, conwaySimulation.maxCount);
        commandData[0].indexCountPerInstance = m_mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)conwaySimulation.maxCount;
        m_commandBuf.SetData(commandData);
    }

    private void Update()
    {
        if (!conwaySimulation.canRender)
            return;

        if (conwaySimulation.markViewDirty)
        {
            m_statesBuffer.SetData(conwaySimulation.states);
            m_renderParams.matProps.SetBuffer(MaterialPropertiesInfo.STATES, m_statesBuffer);
        }

        m_renderParams.matProps.SetFloat(MaterialPropertiesInfo.SPACING, conwaySimulation.cellSize + conwaySimulation.spacing * 2f);
        m_renderParams.matProps.SetFloat(MaterialPropertiesInfo.SIZE, conwaySimulation.cellSize);

        Graphics.RenderMeshIndirect(m_renderParams, m_mesh, m_commandBuf, COMMAND_COUNT);
    }

    private void OnDestroy()
    {
        m_commandBuf?.Release();
        m_commandBuf = null;

        m_statesBuffer.Release();
        m_statesBuffer = null;
    }
}