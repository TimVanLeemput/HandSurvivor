using UnityEngine;

namespace HandSurvivor
{
    /// <summary>
    /// Defines which hand is the player's dominant/main hand
    /// </summary>
    public enum HandType
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Defines hand roles in gameplay
    /// </summary>
    public enum HandRole
    {
        /// <summary>
        /// Main hand - used for physical damage attacks
        /// </summary>
        MainHand = 0,

        /// <summary>
        /// Off hand - used for spirit abilities, collecting powerups, launching abilities
        /// </summary>
        OffHand = 1
    }

    /// <summary>
    /// Stores player's hand preference configuration
    /// </summary>
    [System.Serializable]
    public class HandPreference
    {
        [SerializeField]
        private HandType mainHand = HandType.Right;

        public HandType MainHand
        {
            get => mainHand;
            set => mainHand = value;
        }

        public HandType OffHand => mainHand == HandType.Left ? HandType.Right : HandType.Left;

        /// <summary>
        /// Gets the role of a specific hand based on current preference
        /// </summary>
        public HandRole GetHandRole(HandType hand)
        {
            return hand == mainHand ? HandRole.MainHand : HandRole.OffHand;
        }

        /// <summary>
        /// Gets which hand is assigned to a specific role
        /// </summary>
        public HandType GetHandForRole(HandRole role)
        {
            return role == HandRole.MainHand ? mainHand : OffHand;
        }

        /// <summary>
        /// Checks if the specified hand is the main hand
        /// </summary>
        public bool IsMainHand(HandType hand)
        {
            return hand == mainHand;
        }

        /// <summary>
        /// Checks if the specified hand is the off hand
        /// </summary>
        public bool IsOffHand(HandType hand)
        {
            return hand == OffHand;
        }

        /// <summary>
        /// Swaps main and off hand
        /// </summary>
        public void SwapHands()
        {
            mainHand = mainHand == HandType.Left ? HandType.Right : HandType.Left;
        }
    }
}
