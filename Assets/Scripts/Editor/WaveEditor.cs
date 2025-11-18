using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Wave))]
public class WaveEditor : Editor
{
    private SerializedProperty waveDurationProp;
    private SerializedProperty spawnFrequencyProp;
    private SerializedProperty bossProp;
    private SerializedProperty enemyEntriesProp;

    private void OnEnable()
    {
        waveDurationProp = serializedObject.FindProperty("WaveDuration");
        spawnFrequencyProp = serializedObject.FindProperty("SpawnFequency");
        bossProp = serializedObject.FindProperty("Boss");
        enemyEntriesProp = serializedObject.FindProperty("Enemies");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(waveDurationProp);
        EditorGUILayout.PropertyField(spawnFrequencyProp);
        EditorGUILayout.PropertyField(bossProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemies & Probabilities", EditorStyles.boldLabel);

        // Boutons Add / Clear
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Enemy"))
        {
            int index = enemyEntriesProp.arraySize;
            enemyEntriesProp.InsertArrayElementAtIndex(index);
            var newElement = enemyEntriesProp.GetArrayElementAtIndex(index);
            newElement.FindPropertyRelative("Enemy").objectReferenceValue = null;
            newElement.FindPropertyRelative("Probability").floatValue = 1f;
        }

        if (GUILayout.Button("Clear All"))
        {
            enemyEntriesProp.ClearArray();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        float totalProbability = 0f;

        // Liste des ennemis
        for (int i = 0; i < enemyEntriesProp.arraySize; i++)
        {
            SerializedProperty element = enemyEntriesProp.GetArrayElementAtIndex(i);
            SerializedProperty enemyProp = element.FindPropertyRelative("Enemy");
            SerializedProperty probProp = element.FindPropertyRelative("Probability");

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Enemy " + i, EditorStyles.boldLabel);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                enemyEntriesProp.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enemyProp, new GUIContent("Prefab"));
            probProp.floatValue = EditorGUILayout.Slider("Probability", probProp.floatValue, 0f, 1f);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            totalProbability += probProp.floatValue;
        }

        EditorGUILayout.Space();

        // Warning si la somme != 1
        const float epsilon = 0.001f;
        if (enemyEntriesProp.arraySize > 0 && Mathf.Abs(totalProbability - 1f) > epsilon)
        {
            EditorGUILayout.HelpBox(
                $"La somme des probabilités vaut {totalProbability:F3} (devrait être 1.000).",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Somme des probabilités : {totalProbability:F3}",
                MessageType.Info
            );
        }

        serializedObject.ApplyModifiedProperties();
    }
}