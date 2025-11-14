using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandSurvivor.Level
{
    /// <summary>
    /// Serializable scene reference for drag-and-drop in inspector
    /// </summary>
    [System.Serializable]
    public class SceneReference
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
#endif
        [SerializeField] private string scenePath;

        public string ScenePath => scenePath;

        public bool IsValid => !string.IsNullOrEmpty(scenePath);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneAsset != null)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            }
            else
            {
                scenePath = string.Empty;
            }
        }
#endif

        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference.ScenePath;
        }
    }
}
