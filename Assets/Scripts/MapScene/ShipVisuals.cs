using UnityEngine;

[ExecuteAlways]
public class ShipVisuals : MonoBehaviour
{
    [Header("Base FBX Offset")]
    public Vector3 baseEulerOffset = new Vector3(-90, 0, 0);

    [Header("Bobbing & Rotation")]
    public float bobbingAmount = 0.08f;
    public float bobbingSpeed = 2f;
    
    [Header("Pitch (X-Axis Tilt)")]
    public float pitchAmount = 1.5f;
    public float pitchSpeed = 1.2f;
    
    [Header("Yaw (Y-Axis Turn)")]
    public float yawAmount = 2.0f;
    public float yawSpeed = 0.8f;

    [Header("Roll (Z-Axis Tilt)")]
    public float rollAmount = 3f;
    public float rollSpeed = 1.5f;
    
    public Transform visualModel;

    [Header("Wake")]
    public MeshRenderer wakeRenderer;
    public float wakeFadeSpeed = 8f;
    public float movementThreshold = 0.001f;

    private float initialVisualLocalY;
    private Vector3 lastPosition;
    private float currentWakeIntensity = 0f;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        if (visualModel != null) {
            initialVisualLocalY = visualModel.localPosition.y;
        }
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Idle Bobbing & Multi-Axis Rotation
        if (visualModel != null)
        {
            float time = Time.realtimeSinceStartup;
            float newY = initialVisualLocalY + Mathf.Sin(time * bobbingSpeed) * bobbingAmount;
            
            float newX = Mathf.Sin(time * pitchSpeed) * pitchAmount;
            float newYaw = Mathf.Sin(time * yawSpeed) * yawAmount;
            float newZ = Mathf.Sin(time * rollSpeed) * rollAmount;
            
            visualModel.localPosition = new Vector3(visualModel.localPosition.x, newY, visualModel.localPosition.z);
            
            // Absolutely locks the rotation to the exact hardcoded default without accumulating drift from Editor frames
            visualModel.localRotation = Quaternion.Euler(newX, newYaw, newZ) * Quaternion.Euler(baseEulerOffset);
        }

        // 2. Movement Tracking & Wake Fade
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        bool isMoving = distanceMoved > movementThreshold;
        lastPosition = transform.position;

        if (isMoving) {
            currentWakeIntensity = 1f; 
        } else {
            float dt = Application.isPlaying ? Time.deltaTime : 0.016f;
            currentWakeIntensity = Mathf.Lerp(currentWakeIntensity, 0f, wakeFadeSpeed * dt);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying && currentWakeIntensity > 0.01f) {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#endif

        if (wakeRenderer != null)
        {
            if (propBlock == null) propBlock = new MaterialPropertyBlock();
            wakeRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_Intensity", currentWakeIntensity);
            wakeRenderer.SetPropertyBlock(propBlock);
        }
    }
}
