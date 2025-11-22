using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Controls the UFO from inside the cockpit after being sucked in.
    /// Uses two joysticks: Left for height, Right for horizontal movement.
    /// </summary>
    public class UFOCockpitController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform ufoTransform;
        [SerializeField] private GameObject cockpitPrefab;
        [SerializeField] private Transform cockpitSpawnPoint;

        [Header("Joystick References")]
        [Tooltip("Left joystick controls height (Y axis)")]
        [SerializeField] private CockpitJoystick leftJoystick;
        [Tooltip("Right joystick controls horizontal movement (X/Z axes)")]
        [SerializeField] private CockpitJoystick rightJoystick;

        [Header("Movement Settings")]
        [SerializeField] private float horizontalSpeed = 5f;
        [SerializeField] private float verticalSpeed = 3f;
        [SerializeField] private float maxHeight = 10f;
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float movementSmoothing = 5f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector3 boundsCenter = Vector3.zero;
        [SerializeField] private Vector3 boundsSize = new Vector3(20f, 10f, 20f);

        [Header("Events")]
        public UnityEvent OnCockpitEntered;
        public UnityEvent OnCockpitExited;

        private OVRCameraRig _cameraRig;
        private GameObject _activeCockpit;
        private Vector3 _targetVelocity;
        private Vector3 _currentVelocity;
        private bool _isControlling;
        private Transform _originalParent;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;

        public bool IsControlling => _isControlling;

        private void Awake()
        {
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();
        }

        private void Update()
        {
            if (!_isControlling || ufoTransform == null) return;

            UpdateMovementFromJoysticks();
            ApplyMovement();
        }

        /// <summary>
        /// Enter the cockpit and start controlling the UFO.
        /// Call this after the suction sequence completes.
        /// </summary>
        public void EnterCockpit()
        {
            if (_isControlling) return;

            _isControlling = true;

            // Store original rig state
            if (_cameraRig != null)
            {
                _originalParent = _cameraRig.transform.parent;
                _originalLocalPosition = _cameraRig.transform.localPosition;
                _originalLocalRotation = _cameraRig.transform.localRotation;
            }

            // Spawn cockpit if we have a prefab
            if (cockpitPrefab != null && _activeCockpit == null)
            {
                Transform spawnPoint = cockpitSpawnPoint != null ? cockpitSpawnPoint : ufoTransform;
                _activeCockpit = Instantiate(cockpitPrefab, spawnPoint.position, spawnPoint.rotation);
                _activeCockpit.transform.SetParent(ufoTransform);

                // Find joysticks in spawned cockpit if not already assigned
                if (leftJoystick == null)
                    leftJoystick = _activeCockpit.transform.Find("LeftJoystick")?.GetComponent<CockpitJoystick>();
                if (rightJoystick == null)
                    rightJoystick = _activeCockpit.transform.Find("RightJoystick")?.GetComponent<CockpitJoystick>();
            }

            // Parent camera rig to UFO so player moves with it
            if (_cameraRig != null && ufoTransform != null)
            {
                _cameraRig.transform.SetParent(ufoTransform);
            }

            OnCockpitEntered?.Invoke();
        }

        /// <summary>
        /// Exit the cockpit and return control to normal.
        /// </summary>
        public void ExitCockpit()
        {
            if (!_isControlling) return;

            _isControlling = false;

            // Restore camera rig
            if (_cameraRig != null)
            {
                _cameraRig.transform.SetParent(_originalParent);
                // Don't reset position - let player be where they ended up
            }

            // Cleanup cockpit
            if (_activeCockpit != null)
            {
                Destroy(_activeCockpit);
                _activeCockpit = null;
            }

            leftJoystick = null;
            rightJoystick = null;

            OnCockpitExited?.Invoke();
        }

        private void UpdateMovementFromJoysticks()
        {
            _targetVelocity = Vector3.zero;

            // Right joystick: Horizontal movement (forward/back/left/right)
            if (rightJoystick != null)
            {
                Vector2 rightInput = rightJoystick.GetNormalizedInput();

                // Convert joystick input to world-space horizontal movement
                // Forward/back on joystick = Z axis, Left/right = X axis
                Vector3 horizontalMove = new Vector3(rightInput.x, 0f, rightInput.y) * horizontalSpeed;

                // Rotate by UFO's current Y rotation for intuitive controls
                horizontalMove = ufoTransform.rotation * horizontalMove;
                horizontalMove.y = 0f; // Keep it horizontal

                _targetVelocity += horizontalMove;
            }

            // Left joystick: Height control (up/down)
            if (leftJoystick != null)
            {
                Vector2 leftInput = leftJoystick.GetNormalizedInput();

                // Y axis of joystick controls height
                float verticalMove = leftInput.y * verticalSpeed;
                _targetVelocity.y += verticalMove;
            }
        }

        private void ApplyMovement()
        {
            // Smooth the velocity
            _currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, Time.deltaTime * movementSmoothing);

            // Calculate new position
            Vector3 newPosition = ufoTransform.position + _currentVelocity * Time.deltaTime;

            // Apply height constraints
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);

            // Apply bounds if enabled
            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, boundsCenter.x - boundsSize.x / 2f, boundsCenter.x + boundsSize.x / 2f);
                newPosition.z = Mathf.Clamp(newPosition.z, boundsCenter.z - boundsSize.z / 2f, boundsCenter.z + boundsSize.z / 2f);
            }

            ufoTransform.position = newPosition;
        }

        /// <summary>
        /// Set the UFO transform to control (call before EnterCockpit).
        /// </summary>
        public void SetUFO(Transform ufo)
        {
            ufoTransform = ufo;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawWireCube(boundsCenter, boundsSize);
            }
        }

        private void OnDestroy()
        {
            if (_isControlling)
            {
                ExitCockpit();
            }
        }
    }

    /// <summary>
    /// Individual joystick component for the cockpit.
    /// Attach to a grabbable object that pivots around its base.
    /// </summary>
    public class CockpitJoystick : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxAngle = 30f;
        [SerializeField] private float returnSpeed = 10f;
        [SerializeField] private bool returnToCenter = true;

        [Header("Grab Detection")]
        [SerializeField] private Grabbable grabbable;

        private Vector3 _neutralRotation;
        private bool _isGrabbed;

        public bool IsGrabbed => _isGrabbed;

        private void Awake()
        {
            _neutralRotation = transform.localEulerAngles;

            if (grabbable == null)
                grabbable = GetComponent<Grabbable>();
        }

        private void OnEnable()
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised += OnPointerEvent;
            }
        }

        private void OnDisable()
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= OnPointerEvent;
            }
        }

        private void OnPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Select:
                    _isGrabbed = true;
                    break;
                case PointerEventType.Unselect:
                    _isGrabbed = false;
                    break;
            }
        }

        private void Update()
        {
            // Return to center when not grabbed
            if (!_isGrabbed && returnToCenter)
            {
                Vector3 currentRotation = transform.localEulerAngles;
                Vector3 targetRotation = _neutralRotation;

                transform.localEulerAngles = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * returnSpeed);
            }

            // Clamp rotation while grabbed
            ClampRotation();
        }

        private void ClampRotation()
        {
            Vector3 localEuler = transform.localEulerAngles;

            // Normalize angles to -180 to 180
            float x = NormalizeAngle(localEuler.x - _neutralRotation.x);
            float z = NormalizeAngle(localEuler.z - _neutralRotation.z);

            // Clamp
            x = Mathf.Clamp(x, -maxAngle, maxAngle);
            z = Mathf.Clamp(z, -maxAngle, maxAngle);

            transform.localEulerAngles = new Vector3(
                _neutralRotation.x + x,
                _neutralRotation.y,
                _neutralRotation.z + z
            );
        }

        /// <summary>
        /// Get normalized joystick input (-1 to 1 for each axis).
        /// X = left/right tilt, Y = forward/back tilt.
        /// </summary>
        public Vector2 GetNormalizedInput()
        {
            Vector3 localEuler = transform.localEulerAngles;

            // Get rotation offset from neutral
            float xAngle = NormalizeAngle(localEuler.x - _neutralRotation.x);
            float zAngle = NormalizeAngle(localEuler.z - _neutralRotation.z);

            // Normalize to -1 to 1
            // X rotation (tilting forward/back) = Y output (forward/back movement)
            // Z rotation (tilting left/right) = X output (left/right movement)
            float normalizedY = -Mathf.Clamp(xAngle / maxAngle, -1f, 1f); // Negative because tilting forward is negative X rotation
            float normalizedX = Mathf.Clamp(zAngle / maxAngle, -1f, 1f);

            return new Vector2(normalizedX, normalizedY);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
