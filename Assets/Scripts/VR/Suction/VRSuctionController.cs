using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Reusable VR suction/pull mechanic that moves the player toward a target point.
    /// Used for GameOver sequences and UFO beam capture.
    /// </summary>
    public class VRSuctionController : MonoBehaviour
    {
        [Header("Suction Settings")]
        [SerializeField] private float suctionDuration = 3.5f;
        [SerializeField] private AnimationCurve suctionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Haptics")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private float hapticFrequency = 0.5f;
        [SerializeField] private AnimationCurve hapticIntensityCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);

        [Header("Events")]
        public UnityEvent OnSuctionStart;
        public UnityEvent OnSuctionComplete;
        public UnityEvent<float> OnSuctionProgress; // 0-1 progress value

        private OVRCameraRig _cameraRig;
        private Transform _targetPoint;
        private Vector3 _startPosition;
        private bool _isSuctioning;
        private Coroutine _suctionCoroutine;
        private Coroutine _hapticCoroutine;

        public bool IsSuctioning => _isSuctioning;
        public float Duration => suctionDuration;

        private void Awake()
        {
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();
        }

        /// <summary>
        /// Start the suction effect, pulling the player toward the target.
        /// </summary>
        /// <param name="target">The point to pull the player toward</param>
        public void StartSuction(Transform target)
        {
            if (_isSuctioning || target == null) return;

            if (_cameraRig == null)
            {
                _cameraRig = FindFirstObjectByType<OVRCameraRig>();
                if (_cameraRig == null)
                {
                    Debug.LogError("[VRSuctionController] No OVRCameraRig found!");
                    return;
                }
            }

            _targetPoint = target;
            _startPosition = _cameraRig.transform.position;
            _isSuctioning = true;

            OnSuctionStart?.Invoke();

            _suctionCoroutine = StartCoroutine(SuctionCoroutine());

            if (enableHaptics)
            {
                _hapticCoroutine = StartCoroutine(HapticFeedbackCoroutine());
            }
        }

        /// <summary>
        /// Start suction toward a world position (creates temporary target).
        /// </summary>
        public void StartSuction(Vector3 targetPosition)
        {
            GameObject tempTarget = new GameObject("SuctionTarget_Temp");
            tempTarget.transform.position = targetPosition;
            StartSuction(tempTarget.transform);
        }

        /// <summary>
        /// Force stop the suction effect.
        /// </summary>
        public void StopSuction()
        {
            if (!_isSuctioning) return;

            if (_suctionCoroutine != null)
            {
                StopCoroutine(_suctionCoroutine);
                _suctionCoroutine = null;
            }

            if (_hapticCoroutine != null)
            {
                StopCoroutine(_hapticCoroutine);
                _hapticCoroutine = null;
            }

            StopHaptics();
            _isSuctioning = false;
        }

        public void SetDuration(float duration)
        {
            suctionDuration = duration;
        }

        private IEnumerator SuctionCoroutine()
        {
            float elapsed = 0f;

            // Get the offset from rig to HMD (so we end up with HMD at target, not rig origin)
            Vector3 hmdOffset = _cameraRig.centerEyeAnchor.position - _cameraRig.transform.position;
            Vector3 adjustedTarget = _targetPoint.position - hmdOffset;

            while (elapsed < suctionDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / suctionDuration;

                // Apply easing curve for that "slow start -> accelerate -> snap" feel
                float curveValue = suctionCurve.Evaluate(normalizedTime);

                // Recalculate target in case it moves (for UFO scenario)
                hmdOffset = _cameraRig.centerEyeAnchor.position - _cameraRig.transform.position;
                adjustedTarget = _targetPoint.position - hmdOffset;

                // Lerp position
                _cameraRig.transform.position = Vector3.Lerp(_startPosition, adjustedTarget, curveValue);

                OnSuctionProgress?.Invoke(normalizedTime);

                yield return null;
            }

            // Snap to final position
            hmdOffset = _cameraRig.centerEyeAnchor.position - _cameraRig.transform.position;
            _cameraRig.transform.position = _targetPoint.position - hmdOffset;

            _isSuctioning = false;
            StopHaptics();

            // Clean up temp target if we created one
            if (_targetPoint != null && _targetPoint.name == "SuctionTarget_Temp")
            {
                Destroy(_targetPoint.gameObject);
            }

            OnSuctionComplete?.Invoke();
        }

        private IEnumerator HapticFeedbackCoroutine()
        {
            while (_isSuctioning)
            {
                float elapsed = 0f;
                float duration = suctionDuration;

                // Calculate current progress
                Vector3 currentPos = _cameraRig.transform.position;
                float distance = Vector3.Distance(_startPosition, _targetPoint.position);
                float traveled = Vector3.Distance(_startPosition, currentPos);
                float progress = distance > 0 ? Mathf.Clamp01(traveled / distance) : 0f;

                // Get intensity from curve
                float intensity = hapticIntensityCurve.Evaluate(progress);

                // Apply haptics to both controllers
                OVRInput.SetControllerVibration(hapticFrequency, intensity, OVRInput.Controller.LTouch);
                OVRInput.SetControllerVibration(hapticFrequency, intensity, OVRInput.Controller.RTouch);

                yield return new WaitForSeconds(0.05f); // Update haptics at 20Hz
            }
        }

        private void StopHaptics()
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }

        private void OnDisable()
        {
            StopSuction();
        }

        private void OnDestroy()
        {
            StopHaptics();
        }
    }
}
