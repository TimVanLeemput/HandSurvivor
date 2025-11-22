using Oculus.Interaction;
using UnityEngine;

namespace VR.Suction
{
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