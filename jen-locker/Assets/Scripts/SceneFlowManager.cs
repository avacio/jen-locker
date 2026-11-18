using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneFlowManager : MonoBehaviour
{
    [Header("Clickable Mesh Renderers")]
    [Tooltip("All MeshRenderers the user can click/tap on. Colliders will be added automatically at runtime.")]
    [SerializeField] private MeshRenderer[] clickableMeshes;

    [Header("Rotation Target")]
    [Tooltip("The Transform that will rotate when a mesh is clicked.")]
    [SerializeField] private Transform rotationTarget;

    [FormerlySerializedAs("targetEulerAngles")]
    [Tooltip("The rotation (Euler angles) to lerp TO when clicked.")]
    [SerializeField] private Vector3 targetLocalEulerAngles = new Vector3(0f, 90f, 0f);

    [FormerlySerializedAs("defaultEulerAngles")]
    [Tooltip("The rotation (Euler angles) to lerp BACK TO when clicking again or resetting.")]
    [SerializeField] private Vector3 startLocalEulerAngles = Vector3.zero;

    [Tooltip("How fast the rotation lerps.")]
    [SerializeField] private float rotationLerpSpeed = 5f;

    [Header("Camera Position")]
    [Tooltip("The camera to move. Defaults to Camera.main if left empty.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Where the camera sits by default (zoomed out).")]
    [SerializeField] private Transform cameraDefaultPosition;

    [Tooltip("Where the camera moves to when zoomed in.")]
    [SerializeField] private Transform cameraZoomedPosition;

    [Tooltip("How fast the camera position lerps.")]
    [SerializeField] private float cameraLerpSpeed = 5f;

    // ── Runtime state ──────────────────────────────────────────────
    private Quaternion _targetLockerLocalRotation;
    private Quaternion _defaultRotation;
    private Vector3 _targetCameraPos;
    private Quaternion _targetCameraRot;
    private bool _isZoomedIn = false;

    // ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Fall back to Camera.main if nothing was assigned
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Cache quaternions from the inspector euler values
        _defaultRotation = Quaternion.Euler(startLocalEulerAngles);
        _targetLockerLocalRotation  = _defaultRotation; // start at default

        // Initialise camera targets to the default position
        if (cameraDefaultPosition != null)
        {
            _targetCameraPos = cameraDefaultPosition.position;
            _targetCameraRot = cameraDefaultPosition.rotation;

            // Snap the camera to the default position immediately
            targetCamera.transform.SetPositionAndRotation(
                cameraDefaultPosition.position,
                cameraDefaultPosition.rotation
            );
        }

        // Auto-add MeshColliders to every listed renderer so raycasts work
        foreach (var meshRenderer in clickableMeshes)
        {
            if (meshRenderer == null) continue;

            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null) continue;

            // Only add a collider if one doesn't already exist
            if (meshRenderer.GetComponent<Collider>() == null)
            {
                var col = meshRenderer.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = meshFilter.sharedMesh;
            }
        }
    }

    private void Update()
    {
        HandleInput();
        ApplyRotationLerp();
        ApplyCameraLerp();
    }

    // ── Input ──────────────────────────────────────────────────────

    private void HandleInput()
    {
        // Support both mouse click and touch
        bool inputThisFrame = false;
        Vector2 screenPos   = Vector2.zero;

        if (Input.GetMouseButtonDown(0))
        {
            inputThisFrame = true;
            screenPos = Input.mousePosition;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputThisFrame = true;
            screenPos = Input.GetTouch(0).position;
        }

        if (!inputThisFrame) return;

        // Raycast from camera through the screen point
        Ray ray = targetCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (IsClickableMesh(hit.collider))
            {
                ToggleState();
            }
        }
    }

    // ── State toggle ───────────────────────────────────────────────

    private void ToggleState()
    {
        _isZoomedIn = !_isZoomedIn;

        _targetLockerLocalRotation = _isZoomedIn
            ? Quaternion.Euler(targetLocalEulerAngles)
            : _defaultRotation;

        // Pick the appropriate camera Transform to move toward
        Transform camTarget = _isZoomedIn ? cameraZoomedPosition : cameraDefaultPosition;
        if (camTarget != null)
        {
            _targetCameraPos = camTarget.position;
            _targetCameraRot = camTarget.rotation;
        }
    }

    // ── Lerp application ───────────────────────────────────────────

    private void ApplyRotationLerp()
    {
        if (rotationTarget == null) return;

        rotationTarget.localRotation = Quaternion.Lerp(
            rotationTarget.localRotation, 
            _targetLockerLocalRotation,
            Time.deltaTime * rotationLerpSpeed
        );
    }

    private void ApplyCameraLerp()
    {
        if (targetCamera == null) return;

        targetCamera.transform.SetPositionAndRotation(
            Vector3.Lerp(targetCamera.transform.position, _targetCameraPos, Time.deltaTime * cameraLerpSpeed),
            Quaternion.Lerp(targetCamera.transform.rotation, _targetCameraRot, Time.deltaTime * cameraLerpSpeed)
        );
    }

    // ── Helpers ────────────────────────────────────────────────────

    private bool IsClickableMesh(Collider col)
    {
        foreach (var meshRenderer in clickableMeshes)
        {
            if (meshRenderer != null && meshRenderer.gameObject == col.gameObject)
                return true;
        }
        return false;
    }

    // ── Editor-only snap helpers ───────────────────────────────────

    /// <summary>
    /// Resolves the camera reference the same way Awake() does,
    /// safe to call in Edit Mode where Camera.main may not exist.
    /// </summary>
    private Camera ResolveCamera()
    {
        if (targetCamera != null) return targetCamera;
#if UNITY_EDITOR
        // In Edit Mode Camera.main is often null; search the scene instead.
        return FindFirstObjectByType<Camera>();
#else
        return Camera.main;
#endif
    }

    /// <summary>Snaps transforms immediately without lerping. Safe to call in Edit Mode.</summary>
    private void SnapToState(bool zoomedIn)
    {
        Transform camTarget = zoomedIn ? cameraZoomedPosition : cameraDefaultPosition;
        Quaternion targetRot = zoomedIn
            ? Quaternion.Euler(targetLocalEulerAngles)
            : Quaternion.Euler(startLocalEulerAngles);

        if (rotationTarget != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(rotationTarget, zoomedIn ? "Trigger Zoom In" : "Reset To Default");
#endif
            rotationTarget.localRotation = targetRot;
        }

        Camera cam = ResolveCamera();
        if (cam != null && camTarget != null)
        {
#if UNITY_EDITOR
            Undo.RecordObject(cam.transform, zoomedIn ? "Trigger Zoom In" : "Reset To Default");
#endif
            cam.transform.SetPositionAndRotation(camTarget.position, camTarget.rotation);
        }
    }

    // ── Public API (optional use from other scripts / UI buttons) ──

    /// <summary>Programmatically reset to the default state.</summary>
    [ContextMenu("Reset To Default")]
    public void ResetToDefault()
    {
        _isZoomedIn                = false;
        _defaultRotation           = Quaternion.Euler(startLocalEulerAngles);
        _targetLockerLocalRotation = _defaultRotation;

        if (cameraDefaultPosition != null)
        {
            _targetCameraPos = cameraDefaultPosition.position;
            _targetCameraRot = cameraDefaultPosition.rotation;
        }

        // Snap immediately when called outside of Play Mode
        if (!Application.isPlaying)
            SnapToState(false);
    }

    /// <summary>Programmatically trigger the zoomed-in state.</summary>
    [ContextMenu("Trigger Zoom In")]
    public void TriggerZoomIn()
    {
        _isZoomedIn                = true;
        _targetLockerLocalRotation = Quaternion.Euler(targetLocalEulerAngles);

        if (cameraZoomedPosition != null)
        {
            _targetCameraPos = cameraZoomedPosition.position;
            _targetCameraRot = cameraZoomedPosition.rotation;
        }

        // Snap immediately when called outside of Play Mode
        if (!Application.isPlaying)
            SnapToState(true);
    }
}