using System;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction.PoseDetection;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Manages hand shape detection and triggers events when specific poses are detected
    /// Set up in the Unity Inspector with ShapeRecognizerActiveState component
    /// </summary>
    public class HandShapeManager : MonoBehaviour
    {
        public static HandShapeManager Instance;
        [Header("Shape Recognizers")]
        [SerializeField] private ShapeRecognizerActiveState fingerGunRecognizer;

        [Header("Events")]
        public UnityEvent OnFingerGun;

        private bool wasFingerGunActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (fingerGunRecognizer != null)
            {
                bool isFingerGunActive = fingerGunRecognizer.Active;

                // Trigger event on pose detected (rising edge)
                if (isFingerGunActive && !wasFingerGunActive)
                {
                    OnFingerGun?.Invoke();
                }

                wasFingerGunActive = isFingerGunActive;
            }
        }
    }
}
