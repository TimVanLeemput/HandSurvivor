using UnityEditor;
using UnityEngine;

namespace HandSurvivor.Level.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
            SerializedProperty scenePathProperty = property.FindPropertyRelative("scenePath");

            EditorGUI.BeginChangeCheck();

            SceneAsset oldScene = sceneAssetProperty.objectReferenceValue as SceneAsset;
            SceneAsset newScene = EditorGUI.ObjectField(position, label, oldScene, typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                sceneAssetProperty.objectReferenceValue = newScene;

                if (newScene != null)
                {
                    scenePathProperty.stringValue = AssetDatabase.GetAssetPath(newScene);
                }
                else
                {
                    scenePathProperty.stringValue = string.Empty;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
