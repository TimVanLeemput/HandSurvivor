using HandSurvivor.ActiveSkills;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    public class UFOActiveSkill : ActiveSkillBase
    {
        [Header("UFO Settings")]
        [SerializeField] private GameObject ufoPrefab;
        [SerializeField] private TransformReference ufoSpawnTransformReference;

        private GameObject activeUFO;
        private UFOAttractor currentAttractor;

        protected override void Start()
        {
            base.Start();

            // Subscribe to OnExpire to auto-destroy UFO when duration ends
            OnExpire.AddListener(HandleExpire);
        }

        protected override void Activate()
        {
            isActive = true;
            activationTime = Time.time;

            // Don't start cooldown yet - wait for UFO destruction
            OnActivate?.Invoke();
            PlayActivationEffects();
            OnActivated();
        }

        protected override void OnActivated()
        {
            if (ufoPrefab == null)
            {
                Debug.LogError("[UFOActiveSkill] UFO prefab is not assigned!");
                return;
            }

            // Get spawn slot (randomization configured in SetTransformReference)
            Transform spawnTransform = null;
            if (ufoSpawnTransformReference != null)
            {
                spawnTransform = ufoSpawnTransformReference.GetNextSpawnSlot(maxSlots: 1);
            }

            if (spawnTransform == null)
            {
                Debug.LogWarning("[UFOActiveSkill] Spawn transform not assigned, spawning at skill position");
                spawnTransform = transform;
            }

            // Spawn UFO
            activeUFO = Instantiate(ufoPrefab, spawnTransform);
            activeUFO.transform.localPosition = Vector3.zero;
            activeUFO.transform.localRotation = Quaternion.identity;

            // Apply size multiplier from passive upgrades
            activeUFO.transform.localScale = ufoPrefab.transform.localScale * sizeMultiplier;

            // Configure UFOAttractor component
            currentAttractor = activeUFO.GetComponent<UFOAttractor>();
            if (currentAttractor != null)
            {
                // Size passive scales the collider radius
                currentAttractor.SetRadiusScale(sizeMultiplier);

                // Read base values from prefab and scale by damage multiplier
                float baseCollectDuration = currentAttractor.GetCollectDuration();
                float baseUFODuration = currentAttractor.GetUFODuration();

                currentAttractor.SetCollectDuration(baseCollectDuration * damageMultiplier);
                currentAttractor.SetUFODuration(baseUFODuration * damageMultiplier);

                // Subscribe to destruction event to start cooldown
                currentAttractor.OnUFODestroyed.AddListener(HandleUFODestroyed);
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[UFOActiveSkill] UFO spawned at {spawnTransform.position}");
        }

        protected override void OnDeactivated()
        {
            DestroyUFO();
        }

        private void HandleExpire()
        {
            DestroyUFO();
        }

        private void DestroyUFO()
        {
            if (activeUFO != null)
            {
                if (currentAttractor != null)
                {
                    currentAttractor.OnUFODestroyed.RemoveListener(HandleUFODestroyed);
                }

                Destroy(activeUFO);
                activeUFO = null;
                currentAttractor = null;

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[UFOActiveSkill] UFO destroyed");
            }
        }

        private void HandleUFODestroyed()
        {
            // Reset active state
            isActive = false;

            // Start cooldown when UFO is destroyed
            if (data.cooldown > 0f)
            {
                isOnCooldown = true;
                cooldownEndTime = Time.time + GetModifiedCooldown();

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[UFOActiveSkill] Cooldown started: {GetModifiedCooldown()}s");
            }
        }

        private void OnDestroy()
        {
            OnExpire.RemoveListener(HandleExpire);
            if (currentAttractor != null)
            {
                currentAttractor.OnUFODestroyed.RemoveListener(HandleUFODestroyed);
            }
        }
    }
}
