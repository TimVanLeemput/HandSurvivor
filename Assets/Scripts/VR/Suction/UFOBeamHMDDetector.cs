using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Detects when the UFO beam collides with the player's HMD (head).
    /// Triggers the suction sequence to pull the player into the UFO cockpit.
    /// </summary>
    public class UFOBeamHMDDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 0.3f;
        [SerializeField] private LayerMask beamLayerMask = -1;
        [SerializeField] private string beamTag = "UFOBeam";

        [Header("References")]
        [SerializeField] private VRSuctionController suctionController;

        [Header("Events")]
        public UnityEvent OnBeamHitHMD;
        public UnityEvent OnCockpitEntered;

        private OVRCameraRig _cameraRig;
        private Transform _hmdTransform;
        private UFOAttractor _currentUFO;
        private UFOCockpitController _currentCockpit;
        private bool _isBeingCaptured;
        private SphereCollider _hmdCollider;

        public bool IsBeingCaptured => _isBeingCaptured;

        private void Start()
        {
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();

            if (_cameraRig != null)
            {
                _hmdTransform = _cameraRig.centerEyeAnchor;
                SetupHMDCollider();
            }

            if (suctionController == null)
                suctionController = FindFirstObjectByType<VRSuctionController>();
        }

        private void SetupHMDCollider()
        {
            // Add a trigger collider to the HMD for beam detection
            GameObject detectorObj = new GameObject("HMD_BeamDetector");
            detectorObj.transform.SetParent(_hmdTransform);
            detectorObj.transform.localPosition = Vector3.zero;
            detectorObj.transform.localRotation = Quaternion.identity;

            _hmdCollider = detectorObj.AddComponent<SphereCollider>();
            _hmdCollider.radius = detectionRadius;
            _hmdCollider.isTrigger = true;

            // Add the trigger handler
            HMDBeamTrigger trigger = detectorObj.AddComponent<HMDBeamTrigger>();
            trigger.Initialize(this, beamTag);
        }

        /// <summary>
        /// Called by HMDBeamTrigger when beam collision detected.
        /// </summary>
        public void OnBeamDetected(UFOAttractor ufo)
        {
            if (_isBeingCaptured || ufo == null) return;

            _isBeingCaptured = true;
            _currentUFO = ufo;

            // Immediately cancel self-destruct so UFO doesn't disappear during suction
            ufo.CancelSelfDestruct();

            OnBeamHitHMD?.Invoke();

            // Start suction toward UFO
            if (suctionController != null)
            {
                suctionController.OnSuctionComplete.AddListener(OnSuctionToUFOComplete);
                suctionController.StartSuction(ufo.transform);
            }
            else
            {
                // No suction controller, go directly to cockpit
                EnterCockpit();
            }
        }

        private void OnSuctionToUFOComplete()
        {
            suctionController.OnSuctionComplete.RemoveListener(OnSuctionToUFOComplete);
            EnterCockpit();
        }

        private void EnterCockpit()
        {
            if (_currentUFO == null) return;

            // Find the cockpit controller on the UFO (it's a child of the UFO prefab)
            _currentCockpit = _currentUFO.GetComponentInChildren<UFOCockpitController>(includeInactive: true);

            if (_currentCockpit != null)
            {
                _currentCockpit.EnterCockpit();
            }
            else
            {
                Debug.LogWarning("[UFOBeamHMDDetector] No UFOCockpitController found on UFO!");
            }

            OnCockpitEntered?.Invoke();
        }

        /// <summary>
        /// Exit the cockpit and release the player.
        /// </summary>
        public void ReleaseCapturedPlayer()
        {
            if (!_isBeingCaptured) return;

            _isBeingCaptured = false;

            if (_currentCockpit != null)
            {
                _currentCockpit.ExitCockpit();
                _currentCockpit = null;
            }

            _currentUFO = null;
        }

        private void OnDestroy()
        {
            if (_hmdCollider != null)
            {
                Destroy(_hmdCollider.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_hmdTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_hmdTransform.position, detectionRadius);
            }
        }
    }

    /// <summary>
    /// Helper component attached to HMD collider to detect beam triggers.
    /// </summary>
    public class HMDBeamTrigger : MonoBehaviour
    {
        private UFOBeamHMDDetector _detector;
        private string _beamTag;

        public void Initialize(UFOBeamHMDDetector detector, string beamTag)
        {
            _detector = detector;
            _beamTag = beamTag;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_detector == null) return;

            // Check if this is a UFO beam
            if (!string.IsNullOrEmpty(_beamTag) && !other.CompareTag(_beamTag))
            {
                // Also check parent hierarchy for UFOAttractor
                UFOAttractor ufo = other.GetComponentInParent<UFOAttractor>();
                if (ufo == null) return;

                _detector.OnBeamDetected(ufo);
            }
            else
            {
                // Tag matched, find the UFO
                UFOAttractor ufo = other.GetComponentInParent<UFOAttractor>();
                if (ufo != null)
                {
                    _detector.OnBeamDetected(ufo);
                }
            }
        }
    }
}
