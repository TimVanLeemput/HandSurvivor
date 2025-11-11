using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor
{
    /// <summary>
    /// Manages player's hand preference globally across the application.
    /// Persists hand selection using PlayerPrefs and provides events for hand changes.
    /// </summary>
    public class HandSelectionManager : MonoBehaviour
    {
        private const string HAND_PREFERENCE_KEY = "HandSurvivor_MainHand";

        [Header("Hand Preference")]
        [SerializeField]
        [Tooltip("Current hand preference configuration")]
        private HandPreference handPreference = new HandPreference();

        [Header("Events")]
        [Tooltip("Triggered when the main hand selection changes")]
        public UnityEvent<HandType> OnMainHandChanged;

        [Tooltip("Triggered when hand preference is loaded from storage")]
        public UnityEvent<HandType> OnPreferenceLoaded;

        // Singleton instance
        private static HandSelectionManager instance;
        public static HandSelectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<HandSelectionManager>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject("HandSelectionManager");
                        instance = go.AddComponent<HandSelectionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        // Public accessors for hand preference
        public HandType MainHand => handPreference.MainHand;
        public HandType OffHand => handPreference.OffHand;
        public HandPreference Preference => handPreference;

        private void Awake()
        {
            // Enforce singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preference
            LoadPreference();
        }

        /// <summary>
        /// Loads hand preference from PlayerPrefs
        /// </summary>
        private void LoadPreference()
        {
            if (PlayerPrefs.HasKey(HAND_PREFERENCE_KEY))
            {
                int savedHand = PlayerPrefs.GetInt(HAND_PREFERENCE_KEY, (int)HandType.Right);
                handPreference.MainHand = (HandType)savedHand;
                Debug.Log($"[HandSelection] Loaded preference: Main Hand = {handPreference.MainHand}");
            }
            else
            {
                // Default to right hand if no preference saved
                handPreference.MainHand = HandType.Right;
                SavePreference();
                Debug.Log($"[HandSelection] No saved preference - defaulting to Right hand");
            }

            OnPreferenceLoaded?.Invoke(handPreference.MainHand);
        }

        /// <summary>
        /// Saves current hand preference to PlayerPrefs
        /// </summary>
        private void SavePreference()
        {
            PlayerPrefs.SetInt(HAND_PREFERENCE_KEY, (int)handPreference.MainHand);
            PlayerPrefs.Save();
            Debug.Log($"[HandSelection] Saved preference: Main Hand = {handPreference.MainHand} - Int should be {(int)handPreference.MainHand}");
        }

        /// <summary>
        /// Sets the main hand preference
        /// </summary>
        public void SetMainHand(HandType hand)
        {
            if (handPreference.MainHand == hand)
            {
                Debug.Log($"[HandSelection] Main hand already set to {hand}");
                return;
            }

            handPreference.MainHand = hand;
            SavePreference();

            Debug.Log($"[HandSelection] Main hand changed to: {hand} (Off hand: {handPreference.OffHand})");
            OnMainHandChanged?.Invoke(hand);
        }

        /// <summary>
        /// Swaps main and off hand
        /// </summary>
        public void SwapHands()
        {
            handPreference.SwapHands();
            SavePreference();

            Debug.Log($"[HandSelection] Hands swapped - Main: {handPreference.MainHand}, Off: {handPreference.OffHand}");
            OnMainHandChanged?.Invoke(handPreference.MainHand);
        }

        /// <summary>
        /// Gets the role of a specific hand
        /// </summary>
        public HandRole GetHandRole(HandType hand)
        {
            return handPreference.GetHandRole(hand);
        }

        /// <summary>
        /// Gets which hand is assigned to a specific role
        /// </summary>
        public HandType GetHandForRole(HandRole role)
        {
            return handPreference.GetHandForRole(role);
        }

        /// <summary>
        /// Checks if the specified hand is the main hand (physical damage)
        /// </summary>
        public bool IsMainHand(HandType hand)
        {
            return handPreference.IsMainHand(hand);
        }

        /// <summary>
        /// Checks if the specified hand is the off hand (spirit abilities)
        /// </summary>
        public bool IsOffHand(HandType hand)
        {
            return handPreference.IsOffHand(hand);
        }

        /// <summary>
        /// Resets hand preference to default (Right hand)
        /// </summary>
        public void ResetToDefault()
        {
            SetMainHand(HandType.Right);
        }

        /// <summary>
        /// Static helper - Get main hand without needing Instance reference
        /// </summary>
        public static HandType GetMainHand()
        {
            return Instance.MainHand;
        }

        /// <summary>
        /// Static helper - Get off hand without needing Instance reference
        /// </summary>
        public static HandType GetOffHand()
        {
            return Instance.OffHand;
        }

        /// <summary>
        /// Static helper - Check if hand is main hand
        /// </summary>
        public static bool CheckIsMainHand(HandType hand)
        {
            return Instance.IsMainHand(hand);
        }

        /// <summary>
        /// Static helper - Check if hand is off hand
        /// </summary>
        public static bool CheckIsOffHand(HandType hand)
        {
            return Instance.IsOffHand(hand);
        }

        /// <summary>
        /// Static helper - Check if a GameObject is part of the main hand hierarchy
        /// </summary>
        public static bool IsMainHandObject(GameObject obj)
        {
            return Utilities.TargetHandFinder.IsMainHand(obj);
        }

        /// <summary>
        /// Static helper - Check if a GameObject is part of the off hand hierarchy
        /// </summary>
        public static bool IsOffHandObject(GameObject obj)
        {
            return Utilities.TargetHandFinder.IsOffHand(obj);
        }

        /// <summary>
        /// Static helper - Get the HandType (Left/Right) from a GameObject
        /// Returns null if the GameObject is not part of a hand
        /// </summary>
        public static HandType? GetHandTypeFromObject(GameObject obj)
        {
            return Utilities.TargetHandFinder.GetHandTypeFromObject(obj);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Log Current Preference")]
        private void DebugLogPreference()
        {
            Debug.Log($"[HandSelection] Current Preference:\n" +
                     $"  Main Hand: {handPreference.MainHand} (Physical Damage)\n" +
                     $"  Off Hand: {handPreference.OffHand} (Spirit Abilities)");
        }

        [ContextMenu("Debug: Clear Saved Preference")]
        private void DebugClearPreference()
        {
            PlayerPrefs.DeleteKey(HAND_PREFERENCE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[HandSelection] Cleared saved preference");
        }
#endif
    }
}
