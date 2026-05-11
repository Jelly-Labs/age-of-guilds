using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Transform))]
public class CameraOrbitController : MonoBehaviour
{
    // ── REFERENCES ────────────────────────────────────────────────────────────
    [Header("── REFERENCES ─────────────────────────────────────────")]
    [Tooltip("The Main Camera child. Auto-found if left empty.")]
    [SerializeField] Camera targetCamera;

    // ── ZOOM ──────────────────────────────────────────────────────────────────
    [Header("── ZOOM ─────────────────────────────────────────────────")]
    [Tooltip("Minimum zoom (closest to surface)")]
    [SerializeField] float minZoom = 5.5f;

    [Tooltip("Maximum zoom (farthest)")]
    [SerializeField] float maxZoom = 40f;

    [Tooltip("Starting zoom distance")]
    [SerializeField] float startZoom = 15f;

    [Tooltip("Scroll wheel sensitivity")]
    [SerializeField] float zoomSpeed = 2.5f;

    [Tooltip("Zoom smoothing in seconds")]
    [SerializeField] float zoomSmooth = 0.22f;

    // ── ROTATION & ELEVATION ──────────────────────────────────────────────────
    [Header("── ROTATION & ELEVATION ───────────────────────────────")]
    [Tooltip("Static camera pitch (used when Zoom Tilt is disabled).")]
    [SerializeField] float flatElevation = 55f;

    [Tooltip("Starting yaw angle.")]
    [SerializeField] float startAzimuth = 0f;

    [Tooltip("Right-drag heading rotation speed")]
    [SerializeField] float flatRotSpeed = 0.28f;

    // ── ZOOM TILT ─────────────────────────────────────────────────────────────
    [Header("── ZOOM TILT ─────────────────────────────────────────")]
    [Tooltip("When enabled, pitch adjusts automatically with zoom.\nZoomed in = perspective angle. Zoomed out = top-down.")]
    [SerializeField] bool zoomTiltEnabled = true;

    [Tooltip("Camera pitch when fully zoomed IN (perspective feel). E.g. 35-45.")]
    [SerializeField] float tiltAtMinZoom = 40f;

    [Tooltip("Camera pitch when fully zoomed OUT (top-down feel). E.g. 70-85.")]
    [SerializeField] float tiltAtMaxZoom = 75f;

    [Tooltip("How smoothly the tilt tracks zoom changes (seconds).")]
    [SerializeField] float tiltSmooth = 0.35f;

    // ── PANNING ───────────────────────────────────────────────────────────────
    [Header("── PANNING ──────────────────────────────────────────────")]
    [Tooltip("Keyboard pan speed")]
    [SerializeField] float keyboardPanSpeed = 25f;

    [Tooltip("Edge-scroll pan speed")]
    [SerializeField] float edgePanSpeed = 15f;

    [Tooltip("Inertia decay per 1/60 sec. 0.95 = floaty coast.  0.75 = snappy stop.")]
    [SerializeField] float inertiaDecay = 0.92f;

    [Tooltip("Pixels from screen edge that trigger auto-pan")]
    [SerializeField] float edgeScrollThreshold = 18f;

    [Tooltip("Uncheck to disable edge scrolling")]
    [SerializeField] bool edgeScrollEnabled = true;

    [Tooltip("Middle-mouse drag pan sensitivity (pixels to world units).")]
    [SerializeField] float dragPanSensitivity = 0.035f;

    [Tooltip("Follow-through decay after releasing drag. 0.95 = floaty coast, 0.75 = quick stop.")]
    [SerializeField][Range(0.5f, 0.99f)] float dragFollowThrough = 0.88f;

    [Tooltip("Keyboard acceleration ramp time (seconds). Lower = snappier start.")]
    [SerializeField] float panAccelTime = 0.1f;

    // ── PAN TILT FEEDBACK ───────────────────────────────────────────────────
    [Header("── PAN TILT FEEDBACK ───────────────────────────────")]
    [Tooltip("Camera tilts OPPOSITE to pan direction — simulates gimbal weight/inertia.\nZero to disable.")]
    [SerializeField] float panTiltPitch  = 4.0f;   // forward/back pan → pitch tilt
    [SerializeField] float panTiltRoll   = 3.0f;   // left/right pan   → roll tilt
    [Tooltip("Smoothing delay for the tilt. Lower = more lag = heavier feel.")]
    [SerializeField] float panTiltSmooth = 0.18f;

    // ── CONSTRAINTS ───────────────────────────────────────────────────────────
    [Header("── CONSTRAINTS ──────────────────────────────────────────")]
    [Tooltip("Bounds of the playable map area.")]
    [SerializeField] Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] Vector2 maxBounds = new Vector2(50f, 50f);

    // ─────────────────────────────────────────────────────────────────────────
    //  RUNTIME STATE
    // ─────────────────────────────────────────────────────────────────────────
    Vector3 focusPoint = Vector3.zero;
    float azimuth;
    float currentZoom, targetZoom, zoomVelocity;
    float currentElevation, elevationVelocity;  // zoom-driven tilt

    bool isRotating;
    Vector2 prevMousePos;

    Vector2 edgeVelocity;
    Vector2 keyboardVelocity;
    bool   isDragPanning;
    Vector2 prevDragScreenPos;
    Vector2 dragPanVelocity;   // world-space throw velocity after releasing drag
    Vector2 panTiltTarget;     // desired tilt angles from current pan velocity
    Vector2 currentPanTilt;    // smoothed tilt (x = roll, y = pitch offset)
    Vector2 panTiltVelocity;   // SmoothDamp internal velocity

    public Vector3 FocusPoint => focusPoint;
    public float CurrentZoom => currentZoom > 0.001f ? currentZoom : Mathf.Clamp(startZoom, minZoom, maxZoom);
    public float NormalizedZoom => Mathf.InverseLerp(minZoom, maxZoom, CurrentZoom);
    public float CurrentElevation
    {
        get
        {
            if (currentElevation > 0.001f)
            {
                return currentElevation;
            }

            return zoomTiltEnabled
                ? Mathf.Lerp(tiltAtMinZoom, tiltAtMaxZoom, NormalizedZoom)
                : flatElevation;
        }
    }

    void Awake()
    {
        if (targetCamera == null) targetCamera = GetComponentInChildren<Camera>();
        if (targetCamera == null) targetCamera = Camera.main;

        currentZoom = startZoom;
        targetZoom  = startZoom;
        azimuth     = startAzimuth;

        // Initialise elevation to the zoomed-in angle (or flat if tilt disabled)
        currentElevation = zoomTiltEnabled ? tiltAtMinZoom : flatElevation;
    }

    void Update()
    {
        Vector3 prevFocus = focusPoint;   // capture before movement for tilt calc

        HandleMouseInput();
        HandleKeyboardInput();
        HandleEdgeScroll();
        ApplyMomentum();

        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmooth);

        // ── Zoom-driven tilt ──────────────────────────────────────────────────
        float zoomT           = Mathf.InverseLerp(minZoom, maxZoom, currentZoom);
        float targetElevation = zoomTiltEnabled
            ? Mathf.Lerp(tiltAtMinZoom, tiltAtMaxZoom, zoomT)
            : flatElevation;
        currentElevation = Mathf.SmoothDamp(
            currentElevation, targetElevation, ref elevationVelocity, tiltSmooth);

        // ── Pan tilt feedback ─────────────────────────────────────────────────
        UpdatePanTilt(prevFocus);

        ConstrainFocusPoint();
        PlaceCamera();
    }

    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseScreenPos = mouse.position.ReadValue();

        // Scroll = zoom
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            targetZoom = Mathf.Clamp(targetZoom - (Mathf.Sign(scroll) * zoomSpeed), minZoom, maxZoom);

        // Right drag = heading rotation
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isRotating = true;
            prevMousePos = mouseScreenPos;
        }
        if (mouse.rightButton.wasReleasedThisFrame) isRotating = false;

        if (isRotating)
        {
            Vector2 delta = mouseScreenPos - prevMousePos;
            azimuth += delta.x * flatRotSpeed;
        }

        prevMousePos = mouseScreenPos;

        // ── Middle drag = pan with follow-through ─────────────────────────────
        if (mouse.middleButton.wasPressedThisFrame)
        {
            isDragPanning = true;
            prevDragScreenPos = mouseScreenPos;
            dragPanVelocity   = Vector2.zero;
            // Grab cancels any active keyboard/edge momentum
            keyboardVelocity = Vector2.zero;
            edgeVelocity     = Vector2.zero;
        }
        if (mouse.middleButton.wasReleasedThisFrame)
            isDragPanning = false;

        if (isDragPanning)
        {
            Vector2 screenDelta = mouseScreenPos - prevDragScreenPos;
            float   worldScale  = currentZoom / Screen.height * dragPanSensitivity * 100f;
            Vector2 worldDelta  = -screenDelta * worldScale;
            MoveFocusPoint(worldDelta);

            // Track smoothed velocity so release gives a natural throw
            if (Time.deltaTime > 0f)
                dragPanVelocity = Vector2.Lerp(dragPanVelocity,
                    worldDelta / Time.deltaTime, 0.4f);

            prevDragScreenPos = mouseScreenPos;
        }
    }

    void HandleKeyboardInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 inputDir = Vector2.zero;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) inputDir.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) inputDir.y -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) inputDir.x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) inputDir.x -= 1f;

        if (inputDir.sqrMagnitude > 0.1f)
        {
            Vector2 target = inputDir.normalized * keyboardPanSpeed;
            // Smooth ramp-up: avoids jarring instant-snap to full speed
            keyboardVelocity = Vector2.Lerp(keyboardVelocity, target,
                Time.deltaTime / Mathf.Max(panAccelTime, 0.001f));
        }
    }

    void HandleEdgeScroll()
    {
        if (!edgeScrollEnabled) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();

        if (pos.x < 0 || pos.y < 0 || pos.x > Screen.width || pos.y > Screen.height)
            return;

        Vector2 dir = Vector2.zero;
        float thr = edgeScrollThreshold;

        if (pos.x < thr)                      dir.x = -1f;
        else if (pos.x > Screen.width - thr)  dir.x =  1f;

        if (pos.y < thr)                      dir.y = -1f;
        else if (pos.y > Screen.height - thr) dir.y =  1f;

        if (dir.sqrMagnitude > 0.1f)
        {
            edgeVelocity = dir.normalized * edgePanSpeed;
        }
    }

    void ApplyMomentum()
    {
        float speedMultiplier = (currentZoom / startZoom);

        // ── Drag throw follow-through ──────────────────────────────────────
        // After releasing middle-mouse drag, camera coasts with decaying velocity.
        if (!isDragPanning && dragPanVelocity.sqrMagnitude > 0.001f)
        {
            MoveFocusPoint(dragPanVelocity * Time.deltaTime);
            dragPanVelocity *= Mathf.Pow(dragFollowThrough, Time.deltaTime * 60f);
        }

        // ── Keyboard Momentum
        if (keyboardVelocity.sqrMagnitude > 0.001f)
        {
            MoveFocusPoint(keyboardVelocity * speedMultiplier * Time.deltaTime);
            var kbDir = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed || keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed || keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed || keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    kbDir = Vector2.one;
            }
            if (kbDir == Vector2.zero) 
                keyboardVelocity *= Mathf.Pow(inertiaDecay, Time.deltaTime * 60f);
        }

        // Edge Scroll Momentum
        if (edgeVelocity.sqrMagnitude > 0.001f)
        {
            MoveFocusPoint(edgeVelocity * speedMultiplier * Time.deltaTime);
            
            var mouse = Mouse.current;
            bool hoveringEdge = false;
            if (mouse != null && edgeScrollEnabled) {
                Vector2 pos = mouse.position.ReadValue();
                if (pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height)
                {
                    if (pos.x < edgeScrollThreshold || pos.x > Screen.width - edgeScrollThreshold ||
                        pos.y < edgeScrollThreshold || pos.y > Screen.height - edgeScrollThreshold) {
                            hoveringEdge = true;
                    }
                }
            }
            if (!hoveringEdge)
                edgeVelocity *= Mathf.Pow(inertiaDecay, Time.deltaTime * 60f);
        }
    }

    void MoveFocusPoint(Vector2 panDelta)
    {
        // Pan relative to current camera heading
        Quaternion rot = Quaternion.Euler(0, azimuth, 0);
        Vector3 localDir = new Vector3(panDelta.x, 0, panDelta.y);
        Vector3 worldDir = rot * localDir;

        focusPoint += worldDir;
    }

    void ConstrainFocusPoint()
    {
        focusPoint.x = Mathf.Clamp(focusPoint.x, minBounds.x, maxBounds.x);
        focusPoint.z = Mathf.Clamp(focusPoint.z, minBounds.y, maxBounds.y);
        focusPoint.y = 0f;
    }

    void UpdatePanTilt(Vector3 prevFocus)
    {
        if (Time.deltaTime <= 0f) return;

        // How far did the focus point move this frame?
        Vector3 delta = focusPoint - prevFocus;

        // Project onto camera-relative axes (yaw only, ignore pitch for axis correctness)
        Quaternion camYaw  = Quaternion.Euler(0, azimuth, 0);
        float lateralSpeed = Vector3.Dot(delta, camYaw * Vector3.right)   / Time.deltaTime;
        float forwardSpeed = Vector3.Dot(delta, camYaw * Vector3.forward)  / Time.deltaTime;

        // Normalise by keyboard pan speed so value is roughly -1..1
        float norm = Mathf.Max(keyboardPanSpeed, 0.1f);
        float normLat = lateralSpeed / norm;
        float normFwd = forwardSpeed  / norm;

        // Tilt OPPOSITE to movement: pan right → lean left (negative roll)
        //                            pan forward → lean back (positive pitch)
        panTiltTarget = new Vector2(
            Mathf.Clamp(-normLat * panTiltRoll,  -panTiltRoll,  panTiltRoll),
            Mathf.Clamp( normFwd * panTiltPitch, -panTiltPitch, panTiltPitch)
        );

        currentPanTilt = Vector2.SmoothDamp(
            currentPanTilt, panTiltTarget, ref panTiltVelocity, panTiltSmooth);
    }

    void PlaceCamera()
    {
        if (targetCamera == null) return;

        Quaternion baseRot = Quaternion.Euler(currentElevation, azimuth, 0);
        Vector3 offset = baseRot * new Vector3(0, 0, -currentZoom);
        targetCamera.transform.position = focusPoint + offset;

        // Build look direction then apply pan inertia tilt as a local offset.
        // roll (x) = leans opposite to left/right pan.
        // pitch (y) = tilts opposite to forward/back pan.
        Quaternion lookRot    = Quaternion.LookRotation(
            focusPoint - targetCamera.transform.position, Vector3.up);
        Quaternion tiltOffset = Quaternion.Euler(currentPanTilt.y, 0, currentPanTilt.x);
        targetCamera.transform.rotation = lookRot * tiltOffset;
    }
}
