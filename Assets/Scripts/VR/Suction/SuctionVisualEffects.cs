using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Handles visual effects for the VR suction mechanic:
    /// - Warp streak particles
    /// - Screen vignette/stretch
    /// - Final fade to black/white
    /// </summary>
    public class SuctionVisualEffects : MonoBehaviour
    {
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem warpParticlesPrefab;
        [SerializeField] private float particleSpawnDistance = 2f;
        [SerializeField] private AnimationCurve particleIntensityCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

        [Header("Post-Processing")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.6f);
        [SerializeField] private AnimationCurve lensDistortionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, -0.5f);

        [Header("Screen Fade")]
        [SerializeField] private bool fadeAtEnd = true;
        [SerializeField] [Range(0f, 1f)] private float fadeStartProgress = 0.8f;
        [SerializeField] private Color fadeColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioClip[] warpSounds;
        [SerializeField] private AnimationCurve volumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.8f;

        private VRSuctionController _suctionController;
        private ParticleSystem _activeParticles;
        private ParticleSystem.EmissionModule _emissionModule;
        private float _baseEmissionRate;
        private OVRCameraRig _cameraRig;
        private AudioSource _audioSource;

        // Post-processing components
        private Vignette _vignette;
        private LensDistortion _lensDistortion;
        private bool _hasPostProcessing;

        private void Awake()
        {
            _suctionController = GetComponent<VRSuctionController>();
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();

            SetupAudio();
            SetupPostProcessing();
        }

        private void OnEnable()
        {
            if (_suctionController != null)
            {
                _suctionController.OnSuctionStart.AddListener(OnSuctionStart);
                _suctionController.OnSuctionProgress.AddListener(OnSuctionProgress);
                _suctionController.OnSuctionComplete.AddListener(OnSuctionComplete);
            }
        }

        private void OnDisable()
        {
            if (_suctionController != null)
            {
                _suctionController.OnSuctionStart.RemoveListener(OnSuctionStart);
                _suctionController.OnSuctionProgress.RemoveListener(OnSuctionProgress);
                _suctionController.OnSuctionComplete.RemoveListener(OnSuctionComplete);
            }
        }

        private void SetupAudio()
        {
            if (warpSounds != null && warpSounds.Length > 0)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 0f; // 2D sound for immersion
                _audioSource.loop = true;
                _audioSource.volume = 0f;
                _audioSource.playOnAwake = false;
            }
        }

        private void SetupPostProcessing()
        {
            if (postProcessVolume == null)
            {
                // Try to find existing volume or create one
                postProcessVolume = FindFirstObjectByType<Volume>();
            }

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out _vignette);
                postProcessVolume.profile.TryGet(out _lensDistortion);
                _hasPostProcessing = _vignette != null || _lensDistortion != null;
            }
        }

        private void OnSuctionStart()
        {
            SpawnWarpParticles();
            StartAudio();
            ResetPostProcessing();
        }

        private void OnSuctionProgress(float progress)
        {
            UpdateParticleIntensity(progress);
            UpdatePostProcessing(progress);
            UpdateAudioVolume(progress);
            UpdateScreenFade(progress);
        }

        private void OnSuctionComplete()
        {
            CleanupParticles();
            StopAudio();
            // Keep post-processing at final state (caller decides when to reset)
        }

        private void SpawnWarpParticles()
        {
            if (warpParticlesPrefab == null || _cameraRig == null) return;

            // Spawn particles in front of the camera
            Transform hmd = _cameraRig.centerEyeAnchor;
            Vector3 spawnPos = hmd.position + hmd.forward * particleSpawnDistance;

            _activeParticles = Instantiate(warpParticlesPrefab, spawnPos, Quaternion.identity);
            _activeParticles.transform.SetParent(hmd); // Follow HMD
            _activeParticles.transform.localPosition = Vector3.forward * particleSpawnDistance;
            _activeParticles.transform.localRotation = Quaternion.identity;

            _emissionModule = _activeParticles.emission;
            _baseEmissionRate = _emissionModule.rateOverTime.constant;

            // Start with low emission
            ParticleSystem.MinMaxCurve rate = _emissionModule.rateOverTime;
            rate.constant = _baseEmissionRate * particleIntensityCurve.Evaluate(0f);
            _emissionModule.rateOverTime = rate;

            _activeParticles.Play();
        }

        private void UpdateParticleIntensity(float progress)
        {
            if (_activeParticles == null) return;

            float intensity = particleIntensityCurve.Evaluate(progress);
            ParticleSystem.MinMaxCurve rate = _emissionModule.rateOverTime;
            rate.constant = _baseEmissionRate * intensity;
            _emissionModule.rateOverTime = rate;

            // Also increase particle speed over time for acceleration feel
            ParticleSystem.MainModule main = _activeParticles.main;
            main.simulationSpeed = 1f + (progress * 2f); // 1x -> 3x speed
        }

        private void CleanupParticles()
        {
            if (_activeParticles != null)
            {
                _activeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(_activeParticles.gameObject, 2f); // Let existing particles fade
                _activeParticles = null;
            }
        }

        private void UpdatePostProcessing(float progress)
        {
            if (!_hasPostProcessing) return;

            if (_vignette != null)
            {
                _vignette.intensity.Override(vignetteCurve.Evaluate(progress));
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.Override(lensDistortionCurve.Evaluate(progress));
            }
        }

        private void ResetPostProcessing()
        {
            if (_vignette != null)
            {
                _vignette.intensity.Override(0f);
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.Override(0f);
            }
        }

        /// <summary>
        /// Call this to reset post-processing effects after suction is complete.
        /// </summary>
        public void ResetEffects()
        {
            ResetPostProcessing();

            if (OVRScreenFade.instance != null)
            {
                OVRScreenFade.instance.FadeIn();
            }
        }

        private void StartAudio()
        {
            if (_audioSource == null || warpSounds == null || warpSounds.Length == 0) return;

            AudioClip clip = warpSounds[Random.Range(0, warpSounds.Length)];
            _audioSource.clip = clip;
            _audioSource.volume = 0f;
            _audioSource.Play();
        }

        private void UpdateAudioVolume(float progress)
        {
            if (_audioSource == null) return;
            _audioSource.volume = volumeCurve.Evaluate(progress) * maxVolume;
        }

        private void StopAudio()
        {
            if (_audioSource != null)
            {
                StartCoroutine(FadeOutAudio(0.3f));
            }
        }

        private IEnumerator FadeOutAudio(float duration)
        {
            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _audioSource.Stop();
            _audioSource.volume = 0f;
        }

        private void UpdateScreenFade(float progress)
        {
            if (!fadeAtEnd) return;
            if (progress < fadeStartProgress) return;

            // Map progress from fadeStartProgress..1 to 0..1
            float fadeProgress = Mathf.InverseLerp(fadeStartProgress, 1f, progress);

            if (OVRScreenFade.instance != null)
            {
                OVRScreenFade.instance.fadeColor = fadeColor;
                // Manually set alpha since we're doing a progressive fade
                // OVRScreenFade doesn't have a direct alpha setter, so we trigger fade
                if (fadeProgress > 0.5f && !OVRScreenFade.instance.currentAlpha.Equals(1f))
                {
                    OVRScreenFade.instance.fadeTime = (1f - progress) * _suctionController.Duration;
                    OVRScreenFade.instance.FadeOut();
                }
            }
        }

        private void OnDestroy()
        {
            CleanupParticles();
            ResetPostProcessing();
        }
    }
}
