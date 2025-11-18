using System.Collections.Generic;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    public class MeteorActiveSkill : ActiveSkillBase
    {
        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private TransformReference meteorSpawnTransformReference;

        private Transform meteorSpawnTransform;

        [Header("Charge System")]
        [SerializeField] private int currentCharges = 1;
        [SerializeField] private int maxCharges = 1;

        // Track active meteors
        private List<MeteorProjectile> activeMeteors = new List<MeteorProjectile>();

        // Cooldown tracking for charge refill
        private float nextChargeRefillTime = 0f;
        private bool isRefillCooldownActive = false;

        // Track pending charge refills from destroyed meteors
        private int pendingChargeRefills = 0;

        protected override void Start()
        {
            base.Start();

            // Initialize with full charges
            currentCharges = maxCharges;

            // Get spawn transform from reference
            if (meteorSpawnTransformReference != null)
            {
                meteorSpawnTransform = meteorSpawnTransformReference.Value;
            }
        }

        protected override void Update()
        {
            base.Update();

            // Update spawn transform from reference if needed
            if (meteorSpawnTransform == null && meteorSpawnTransformReference != null)
            {
                meteorSpawnTransform = meteorSpawnTransformReference.Value;
            }

            // Handle charge refill cooldown
            if (isRefillCooldownActive && Time.time >= nextChargeRefillTime)
            {
                RefillCharge();
            }

            // Clean up null references from destroyed meteors
            activeMeteors.RemoveAll(meteor => meteor == null);
        }

        public override bool CanActivate()
        {
            // Clean up null references first
            activeMeteors.RemoveAll(meteor => meteor == null);

            // Can spawn if we have available charges
            // Charges represent meteors we're allowed to have active
            return currentCharges > 0;
        }

        protected override void OnActivated()
        {
            if (meteorPrefab == null)
            {
                Debug.LogError("MeteorActiveSkill: meteorPrefab is not assigned!");
                return;
            }

            if (meteorSpawnTransformReference == null)
            {
                Debug.LogError("MeteorActiveSkill: meteorSpawnTransformReference is not assigned!");
                return;
            }

            if (meteorSpawnTransform == null)
            {
                Debug.LogError("MeteorActiveSkill: meteorSpawnTransform from TransformReference is null! Make sure the reference is set at runtime.");
                return;
            }

            // Consume one charge
            currentCharges--;

            // Spawn meteor at designated transform
            GameObject meteorInstance = Instantiate(
                meteorPrefab,
                meteorSpawnTransform.position,
                meteorSpawnTransform.rotation
            );

            // Apply size multiplier
            meteorInstance.transform.localScale = meteorPrefab.transform.localScale * sizeMultiplier;

            // Configure meteor projectile
            MeteorProjectile projectile = meteorInstance.GetComponent<MeteorProjectile>();
            if (projectile != null)
            {
                projectile.SetDamage((int)(data.damage * damageMultiplier));

                // Damage radius scales with size
                projectile.SetDamageRadius(sizeMultiplier);

                // Set callback to start cooldown when meteor is destroyed
                projectile.SetOnDestroyedCallback(OnMeteorDestroyed);

                activeMeteors.Add(projectile);
            }
            else
            {
                Debug.LogError("MeteorActiveSkill: meteorPrefab is missing MeteorProjectile component!");
            }

            Debug.Log($"Meteor spawned! Charges remaining: {currentCharges}/{maxCharges}");
        }

        protected override void OnDeactivated()
        {
            // MeteorActiveSkill doesn't need deactivation logic
            // Meteors persist until they explode or are destroyed
        }

        private void StartChargeRefillCooldown()
        {
            float cooldown = GetModifiedCooldown();
            nextChargeRefillTime = Time.time + cooldown;
            isRefillCooldownActive = true;
        }

        private void RefillCharge()
        {
            if (currentCharges < maxCharges)
            {
                currentCharges++;
                pendingChargeRefills--;
                Debug.Log($"Charge refilled! Charges: {currentCharges}/{maxCharges}, Pending refills: {pendingChargeRefills}");

                // If we have more pending refills, restart cooldown
                if (pendingChargeRefills > 0 && currentCharges < maxCharges)
                {
                    StartChargeRefillCooldown();
                }
                else
                {
                    isRefillCooldownActive = false;
                }
            }
            else
            {
                // Already at max, stop refilling
                isRefillCooldownActive = false;
                pendingChargeRefills = 0;
            }
        }

        private int GetModifiedMaxCharges()
        {
            return Mathf.Max(1, maxCharges);
        }

        private void OnMeteorDestroyed()
        {
            // Queue a charge refill for this destroyed meteor
            pendingChargeRefills++;
            Debug.Log($"Meteor destroyed! Pending charge refills: {pendingChargeRefills}");

            // Start cooldown if not already active
            if (!isRefillCooldownActive && currentCharges < maxCharges)
            {
                StartChargeRefillCooldown();
            }
        }

        public override void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
        {
            // Handle standard upgrades via base class
            base.ApplyPassiveUpgrade(upgrade);

            // Handle meteor-specific charges upgrade
            if (upgrade.type == PassiveType.ChargesIncrease)
            {
                // Clean up destroyed meteors first
                activeMeteors.RemoveAll(meteor => meteor == null);

                int previousMax = maxCharges;
                maxCharges += Mathf.RoundToInt(upgrade.value);

                // Calculate total meteors (active + charges)
                // This represents how many meteors the player "owns" right now
                int totalMeteors = activeMeteors.Count + currentCharges;

                // Set current charges based on new max and active meteors
                // If we have active meteors, they consume charge slots
                currentCharges = Mathf.Max(0, maxCharges - activeMeteors.Count);

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[{Data.displayName}] Max charges increased to {maxCharges}. Active meteors: {activeMeteors.Count}, Available charges: {currentCharges}");

                // Trigger max event if specified
                if (upgrade.triggersMaxEvent)
                {
                    TriggerMaxPassiveReached();
                }
            }
        }

        public int GetCurrentCharges()
        {
            return currentCharges;
        }

        public int GetMaxCharges()
        {
            return GetModifiedMaxCharges();
        }

        public float GetChargeRefillProgress()
        {
            if (!isRefillCooldownActive)
                return 1f;

            float cooldown = GetModifiedCooldown();
            float elapsed = Time.time - (nextChargeRefillTime - cooldown);
            return Mathf.Clamp01(elapsed / cooldown);
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (meteorSpawnTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(meteorSpawnTransform.position, 0.15f);
                Gizmos.DrawLine(meteorSpawnTransform.position, meteorSpawnTransform.position + meteorSpawnTransform.forward * 0.5f);
            }
        }
    }
}
