using UnityEngine;
using UnityEngine.Events;
using VR.Suction;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Controls the UFO from inside the cockpit after being sucked in.
    /// Uses two joysticks: Left for height, Right for horizontal movement.
    /// Place this component at the root of the UFOCockpit prefab (child of UFO).
    /// </summary>
    public class UFOCockpitController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The UFO transform this cockpit controls. Set at runtime via SetUFO() or assign in inspector if cockpit is already child of UFO.")]
        [SerializeField] private Transform ufoTransform;
        [Tooltip("Where the player's head should be positioned. If null, uses this transform's position.")]
        [SerializeField] private Transform seatPosition;

        [Header("Joystick References (children of this prefab)")]
        [Tooltip("Left joystick controls height (Y axis)")]
        [SerializeField] private CockpitJoystick leftJoystick;
        [Tooltip("Right joystick controls horizontal movement (X/Z axes)")]
        [SerializeField] private CockpitJoystick rightJoystick;

        [Header("Movement Settings")]
        [SerializeField] private float horizontalSpeed = 1f;
        [SerializeField] private float verticalSpeed = 0.5f;
        [SerializeField] private float maxHeight = 10f;
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float movementSmoothing = 3f;
        [SerializeField] [Range(0f, 0.5f)] private float joystickDeadzone = 0.15f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector3 boundsCenter = Vector3.zero;
        [SerializeField] private Vector3 boundsSize = new Vector3(20f, 10f, 20f);

        [Header("Events")]
        public UnityEvent OnCockpitEntered;
        public UnityEvent OnCockpitExited;

        private OVRCameraRig _cameraRig;
        private Vector3 _targetVelocity;
        private Vector3 _currentVelocity;
        private bool _isControlling;
        private Transform _originalParent;
        private Vector3 _rigOffset;

        public bool IsControlling => _isControlling;

        private void Awake()
        {
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();

            // Auto-find UFO if not assigned (assumes cockpit is child of UFO)
            if (ufoTransform == null)
            {
                ufoTransform = transform.parent;
            }
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
        /// The cockpit prefab should already be a child of the UFO.
        /// </summary>
        public void EnterCockpit()
        {
            if (_isControlling) return;

            _isControlling = true;

            // Cancel UFO self-destruct so it doesn't disappear while player is inside
            UFOAttractor ufoAttractor = ufoTransform != null ? ufoTransform.GetComponent<UFOAttractor>() : null;
            if (ufoAttractor != null)
            {
                ufoAttractor.CancelSelfDestruct();
            }

            // Store original rig parent
            if (_cameraRig != null)
            {
                _originalParent = _cameraRig.transform.parent;

                // Determine target position (seat or cockpit center)
                Transform targetPos = seatPosition != null ? seatPosition : transform;

                // Calculate where rig needs to be so HMD ends up at target
                // HMD offset from rig in world space
                Vector3 hmdWorldOffset = _cameraRig.centerEyeAnchor.position - _cameraRig.transform.position;

                // Position rig so HMD lands at target
                _cameraRig.transform.position = targetPos.position - hmdWorldOffset;

                // Now parent to cockpit (keeps world position)
                _cameraRig.transform.SetParent(transform, worldPositionStays: true);

                _rigOffset = Vector3.zero;
            }

            // Enable cockpit visuals (in case they were disabled)
            gameObject.SetActive(true);

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
            }

            // Disable cockpit
            gameObject.SetActive(false);

            OnCockpitExited?.Invoke();
        }

        private void UpdateMovementFromJoysticks()
        {
            _targetVelocity = Vector3.zero;

            // Right joystick: Horizontal movement (forward/back/left/right)
            if (rightJoystick != null)
            {
                Vector2 rightInput = ApplyDeadzone(rightJoystick.GetNormalizedInput());

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
                Vector2 leftInput = ApplyDeadzone(leftJoystick.GetNormalizedInput());

                // Y axis of joystick controls height
                float verticalMove = leftInput.y * verticalSpeed;
                _targetVelocity.y += verticalMove;
            }
        }

        private Vector2 ApplyDeadzone(Vector2 input)
        {
            if (input.magnitude < joystickDeadzone)
                return Vector2.zero;

            // Remap from deadzone..1 to 0..1
            Vector2 normalized = input.normalized;
            float magnitude = Mathf.InverseLerp(joystickDeadzone, 1f, input.magnitude);
            return normalized * magnitude;
        }

        private void ApplyMovement()
        {
            // Smooth the velocity
            _currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, Time.deltaTime * movementSmoothing);

            // Store old position to calculate delta
            Vector3 oldPosition = ufoTransform.position;

            // Calculate new position
            Vector3 newPosition = oldPosition + _currentVelocity * Time.deltaTime;

            // Apply height constraints
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);

            // Apply bounds if enabled
            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, boundsCenter.x - boundsSize.x / 2f, boundsCenter.x + boundsSize.x / 2f);
                newPosition.z = Mathf.Clamp(newPosition.z, boundsCenter.z - boundsSize.z / 2f, boundsCenter.z + boundsSize.z / 2f);
            }

            // Move UFO
            ufoTransform.position = newPosition;

            // Also move camera rig by the same delta (OVR tracking can fight parenting)
            if (_cameraRig != null)
            {
                Vector3 delta = newPosition - oldPosition;
                _cameraRig.transform.position += delta;
            }
        }


        /// <summary>
        /// Set the UFO transform to control.
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
}
