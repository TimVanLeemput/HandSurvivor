using UnityEditor;
using UnityEngine;

namespace HandSurvivor.Core.Passive
{
    [CustomEditor(typeof(PassiveUpgradePath))]
    public class PassiveUpgradePathEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PassiveUpgradePath path = (PassiveUpgradePath)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Calculate Max Possible Level Behaviours"))
            {
                path.CalculateMaxLevelsEditor();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
