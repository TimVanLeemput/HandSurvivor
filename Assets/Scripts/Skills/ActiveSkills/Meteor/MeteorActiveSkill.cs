using System.Collections.Generic;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;
using HandSurvivor.Interfaces;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    public class MeteorActiveSkill : ActiveSkillBase, IChargeableSkill
    {
        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private TransformReference meteorSpawnTransformReference;

        private Transform meteorSpawnTransform;

        [Header("Charge System")]
        [SerializeField] private int currentCharges = 1;
        [SerializeField] private int maxCharges = 1;
        [SerializeField] private int absoluteMaxCharges = 3; // Defines the number of slots that can be filled even if more charge upgrades are picked.

        // Slot tracking system
        private Dictionary<int, MeteorProjectile> activeSlotMeteors = new Dictionary<int, MeteorProjectile>();
        private Queue<int> freeSlots = new Queue<int>();

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
        }

        public override bool CanActivate()
        {
            // Can spawn if we have available charges AND available slots
            return currentCharges > 0 && activeSlotMeteors.Count < absoluteMaxCharges;
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

            // Get next available slot
            int slotIndex = GetNextAvailableSlot();
            Transform spawnTransform = GetSlotTransform(slotIndex);

            if (spawnTransform == null)
            {
                Debug.LogError($"MeteorActiveSkill: Could not get spawn transform for slot {slotIndex}!");
                return;
            }

            // Consume one charge
            currentCharges--;

            // Spawn meteor at slot
            GameObject meteorInstance = Instantiate(
                meteorPrefab,
                spawnTransform.position,
                spawnTransform.rotation
            );

            // Apply size multiplier
            meteorInstance.transform.localScale = meteorPrefab.transform.localScale * sizeMultiplier;

            // Configure meteor projectile
            MeteorProjectile projectile = meteorInstance.GetComponent<MeteorProjectile>();
            if (projectile != null)
            {
                projectile.SetDamage((int)(data.damage * damageMultiplier));
                projectile.SetDamageRadius(sizeMultiplier);

                // Set slot index and callback
                projectile.SetSlotIndex(slotIndex);
                projectile.SetOnDestroyedCallback(() => OnMeteorDestroyed(slotIndex));

                // Sync PassiveUpgradePath level from ActiveSkill to spawned projectile
                Debug.Log($"[MeteorActiveSkill] upgradePath is {(upgradePath != null ? "NOT NULL, Level=" + upgradePath.CurrentLevel : "NULL")}");

                if (upgradePath != null)
                {
                    PassiveUpgradePath projectileUpgradePath = meteorInstance.GetComponent<PassiveUpgradePath>();
                    Debug.Log($"[MeteorActiveSkill] Projectile upgradePath is {(projectileUpgradePath != null ? "NOT NULL" : "NULL")}");

                    if (projectileUpgradePath != null)
                    {
                        Debug.Log($"[MeteorActiveSkill] About to sync level {upgradePath.CurrentLevel} to projectile");
                        projectileUpgradePath.SetLevel(upgradePath.CurrentLevel);
                        Debug.Log($"[MeteorActiveSkill] Successfully synced level");
                    }
                }

                // Track meteor in slot
                activeSlotMeteors[slotIndex] = projectile;

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[MeteorActiveSkill] Spawned meteor at slot {slotIndex}. Charges remaining: {currentCharges}/{maxCharges}");
            }
            else
            {
                Debug.LogError("MeteorActiveSkill: meteorPrefab is missing MeteorProjectile component!");
            }
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

        private void OnMeteorDestroyed(int slotIndex)
        {
            // Free the slot
            if (activeSlotMeteors.ContainsKey(slotIndex))
            {
                activeSlotMeteors.Remove(slotIndex);
                freeSlots.Enqueue(slotIndex);

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[MeteorActiveSkill] Slot {slotIndex} freed. Free slots: {freeSlots.Count}");
            }

            // Queue a charge refill for this destroyed meteor
            pendingChargeRefills++;

            // Start cooldown if not already active
            if (!isRefillCooldownActive && currentCharges < maxCharges)
            {
                StartChargeRefillCooldown();
            }
        }

        /// <summary>
        /// Gets the next available slot index (prioritizes freed slots)
        /// </summary>
        private int GetNextAvailableSlot()
        {
            // Use freed slot if available
            if (freeSlots.Count > 0)
            {
                return freeSlots.Dequeue();
            }

            // Otherwise use next sequential slot
            return activeSlotMeteors.Count;
        }

        /// <summary>
        /// Gets the Transform for a specific slot index
        /// </summary>
        private Transform GetSlotTransform(int slotIndex)
        {
            if (meteorSpawnTransformReference == null)
                return null;

            return meteorSpawnTransformReference.GetSpawnSlot(slotIndex, absoluteMaxCharges);
        }

        public override void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
        {
            // Handle standard upgrades via base class
            base.ApplyPassiveUpgrade(upgrade);

            // Handle meteor-specific charges upgrade
            if (upgrade.type == PassiveType.ChargesIncrease)
            {
                // Only apply if we haven't reached the absolute max
                if (maxCharges < absoluteMaxCharges)
                {
                    int previousMax = maxCharges;
                    maxCharges = Mathf.Min(maxCharges + Mathf.RoundToInt(upgrade.value), absoluteMaxCharges);

                    // Set current charges based on new max and active slot meteors
                    currentCharges = Mathf.Max(0, maxCharges - activeSlotMeteors.Count);

                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[{Data.displayName}] Max charges increased to {maxCharges}. Active meteors: {activeSlotMeteors.Count}, Available charges: {currentCharges}");

                    // Trigger max event if specified
                    if (upgrade.triggersMaxEvent)
                    {
                        TriggerMaxPassiveReached();
                    }
                }
                else
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[{Data.displayName}] Charge upgrade ignored - already at absolute max ({absoluteMaxCharges})");
                }
            }
        }

        public int GetCurrentCharges()
        {
            return currentCharges;
        }

        public int GetMaxCharges()
        {
            int result = maxCharges;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[MeteorActiveSkill] GetMaxCharges() called - returning {result} (maxCharges field = {maxCharges})");
            return Mathf.Max(1, result);
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
            if (meteorSpawnTransformReference != null)
            {
                int totalSlots = meteorSpawnTransformReference.GetTotalSlotCount();
                for (int i = 0; i < Mathf.Min(totalSlots, absoluteMaxCharges); i++)
                {
                    Transform spawn = meteorSpawnTransformReference.GetSpawnSlot(i, absoluteMaxCharges);
                    if (spawn != null)
                    {
                        // Green if occupied, yellow if free
                        Gizmos.color = activeSlotMeteors.ContainsKey(i) ? Color.green : Color.yellow;
                        Gizmos.DrawWireSphere(spawn.position, 0.15f);
                        Gizmos.DrawLine(spawn.position, spawn.position + spawn.forward * 0.5f);
                    }
                }
            }
        }
    }
}
