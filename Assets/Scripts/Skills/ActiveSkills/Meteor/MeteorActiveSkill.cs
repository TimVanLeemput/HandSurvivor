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

        // Passive upgrade multipliers
        private float chargesMultiplier = 1f;

        // Track active meteors
        private List<MeteorProjectile> activeMeteors = new List<MeteorProjectile>();

        // Cooldown tracking for charge refill
        private float nextChargeRefillTime = 0f;
        private bool isRefillCooldownActive = false;

        protected override void Start()
        {
            base.Start();

            // Initialize with full charges
            currentCharges = GetModifiedMaxCharges();

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
            // Can activate if we have at least one charge available
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
                projectile.SetDamageRadius(3f * sizeMultiplier);

                activeMeteors.Add(projectile);
            }
            else
            {
                Debug.LogError("MeteorActiveSkill: meteorPrefab is missing MeteorProjectile component!");
            }

            // Start charge refill cooldown if not at max charges
            if (currentCharges < GetModifiedMaxCharges())
            {
                StartChargeRefillCooldown();
            }

            Debug.Log($"Meteor spawned! Charges remaining: {currentCharges}/{GetModifiedMaxCharges()}");
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
            int maxCharges = GetModifiedMaxCharges();

            if (currentCharges < maxCharges)
            {
                currentCharges++;
                Debug.Log($"Charge refilled! Charges: {currentCharges}/{maxCharges}");
            }

            // If still not at max, restart cooldown for next charge
            if (currentCharges < maxCharges)
            {
                StartChargeRefillCooldown();
            }
            else
            {
                isRefillCooldownActive = false;
            }
        }

        private int GetModifiedMaxCharges()
        {
            return Mathf.Max(1, Mathf.RoundToInt(maxCharges * chargesMultiplier));
        }

        public override void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
        {
            // Handle standard upgrades via base class
            base.ApplyPassiveUpgrade(upgrade);

            // Handle meteor-specific charges upgrade
            if (upgrade.type == PassiveType.ChargesIncrease)
            {
                chargesMultiplier += upgrade.value / 100f;

                // Update current charges if max increased
                int newMaxCharges = GetModifiedMaxCharges();
                if (currentCharges > newMaxCharges)
                {
                    currentCharges = newMaxCharges;
                }

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[{Data.displayName}] Max charges updated to {newMaxCharges} (multiplier: {chargesMultiplier:F2})");

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
