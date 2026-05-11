using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Runtime GPU Forest Renderer.
/// Uses a single Graphics.DrawMeshInstancedIndirect call per camera with:
///   - GPU frustum culling via compute shader (colour rendering)
///   - null camera → draw submitted to ALL passes including URP shadow caster
///
/// Shadow casting now works because the TreeShader ShadowCaster pass compiles
/// correctly for PROCEDURAL_INSTANCING_ON (no more unity_LightShadowBias error).
/// </summary>
[ExecuteAlways]
public class GPUForestRenderer : MonoBehaviour
{
    [Header("Data & Rendering")]
    public ForestData    forestData;
    public Mesh          clusterMesh;
    public Material      treeMaterial;
    public ComputeShader cullingShader;

    [Header("Shadow Casting")]
    public bool castShadows    = true;
    public bool receiveShadows = true;

    // Each DrawInstance = float4x4 = 64 bytes
    private const int INSTANCE_DATA_STRIDE = 16 * sizeof(float);

    private ComputeBuffer _allInstancesBuffer;
    private ComputeBuffer _visibleInstancesBuffer;
    private ComputeBuffer _indirectArgsBuffer;

    // Per-draw property block — lets multiple renderers share the same TreeMat
    // without overwriting each other's _VisibleInstances buffer.
    private MaterialPropertyBlock _propertyBlock;

    private int  _kernelIndex;
    private bool _initialized = false;
    private int  _totalCount  = 0;

    private static readonly Plane[]   _frustumPlanes    = new Plane[6];
    private static readonly Vector4[] _frustumPlanesVec = new Vector4[6];
    private static readonly uint[]    _args             = new uint[5];

    // ── Lifecycle ────────────────────────────────────────────────────────────
    void OnEnable()
    {
        Initialize();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        Release();
    }

    void OnDestroy() => Release();

    // ── Per-Camera Hook ──────────────────────────────────────────────────────
    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam.cameraType == CameraType.Reflection ||
            cam.cameraType == CameraType.Preview) return;

        if (forestData == null || forestData.Count == 0)
        {
            if (_initialized) Release();
            return;
        }

        if (forestData.Count != _totalCount)
        {
            Release();
            Initialize();
            if (!_initialized) return;
        }

        if (!_initialized) return;

        RenderForCamera(cam);
    }

    private void RenderForCamera(Camera cam)
    {
        // ── Rebind compute buffers every frame ───────────────────────────────
        // REQUIRED when multiple GPUForestRenderer instances share the same
        // ComputeShader asset. The last Initialize() call overwrites the shader's
        // buffer bindings for everyone. Re-setting them right before each dispatch
        // ensures this renderer always uses its own correct buffers.
        cullingShader.SetBuffer(_kernelIndex, "_AllInstances",     _allInstancesBuffer);
        cullingShader.SetBuffer(_kernelIndex, "_VisibleInstances", _visibleInstancesBuffer);
        cullingShader.SetInt("_InstanceCount", _totalCount);

        // ── GPU frustum cull for this specific camera ────────────────────────
        GeometryUtility.CalculateFrustumPlanes(cam, _frustumPlanes);
        for (int i = 0; i < 6; i++)
        {
            _frustumPlanesVec[i] = new Vector4(
                _frustumPlanes[i].normal.x,
                _frustumPlanes[i].normal.y,
                _frustumPlanes[i].normal.z,
                _frustumPlanes[i].distance);
        }

        _visibleInstancesBuffer.SetCounterValue(0);
        _indirectArgsBuffer.SetData(new uint[] { _args[0], 0, _args[2], _args[3], 0 });

        cullingShader.SetVectorArray("_FrustumPlanes", _frustumPlanesVec);
        int threadGroups = Mathf.CeilToInt(_totalCount / 64f);
        cullingShader.Dispatch(_kernelIndex, threadGroups, 1, 1);

        ComputeBuffer.CopyCount(_visibleInstancesBuffer, _indirectArgsBuffer, sizeof(uint));

        // ── Single draw call, null camera = ALL render passes ────────────────
        // Passing null instead of 'cam' ensures URP includes this draw in the
        // shadow caster pass. With the TreeShader ShadowCaster now compiling
        // cleanly for PROCEDURAL_INSTANCING_ON, shadow geometry will be placed
        // at the correct world positions read from _VisibleInstances[instanceID].
        // Pass _indirectArgsBuffer instance count + _VisibleInstances via property block.
        // Using a property block (not material.SetBuffer) allows multiple renderers
        // to share the same TreeMat with their own independent GPU buffers.
        _propertyBlock.SetBuffer("_VisibleInstances", _visibleInstancesBuffer);

        Graphics.DrawMeshInstancedIndirect(
            clusterMesh, 0, treeMaterial,
            new Bounds(Vector3.zero, new Vector3(100000f, 100000f, 100000f)),
            _indirectArgsBuffer, 0, _propertyBlock,
            castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
            receiveShadows,
            gameObject.layer,
            null   // null = submit to ALL cameras AND shadow passes
        );
    }

    // ── Initialization ───────────────────────────────────────────────────────
    private void Initialize()
    {
        if (forestData == null || forestData.Count == 0)             return;
        if (clusterMesh == null || treeMaterial == null || cullingShader == null) return;

        Release();

        _totalCount = forestData.Count;

        _allInstancesBuffer     = new ComputeBuffer(_totalCount, INSTANCE_DATA_STRIDE);
        _allInstancesBuffer.SetData(forestData.matrixData);

        _visibleInstancesBuffer = new ComputeBuffer(_totalCount, INSTANCE_DATA_STRIDE, ComputeBufferType.Append);
        _indirectArgsBuffer     = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

        _args[0] = clusterMesh.GetIndexCount(0);
        _args[1] = 0;
        _args[2] = clusterMesh.GetIndexStart(0);
        _args[3] = clusterMesh.GetBaseVertex(0);
        _args[4] = 0;
        _indirectArgsBuffer.SetData(_args);

        _kernelIndex = cullingShader.FindKernel("CSFrustumCull");
        cullingShader.SetBuffer(_kernelIndex, "_AllInstances",     _allInstancesBuffer);
        cullingShader.SetBuffer(_kernelIndex, "_VisibleInstances", _visibleInstancesBuffer);
        cullingShader.SetInt("_InstanceCount", _totalCount);

        // Property block is per-renderer — safe to share across multiple instances
        _propertyBlock = new MaterialPropertyBlock();
        // _VisibleInstances is set each frame in RenderForCamera after CopyCount

        _initialized = true;
        Debug.Log($"[GPUForestRenderer] Initialized {_totalCount:N0} instances.");
    }

    // ── Cleanup ──────────────────────────────────────────────────────────────
    private void Release()
    {
        _allInstancesBuffer?.Release();
        _visibleInstancesBuffer?.Release();
        _indirectArgsBuffer?.Release();
        _allInstancesBuffer     = null;
        _visibleInstancesBuffer = null;
        _indirectArgsBuffer     = null;
        _initialized = false;
        _totalCount  = 0;
    }
}
