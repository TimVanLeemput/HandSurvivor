using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(BoneIdMapper))]
public class BoneIdMapperEditor : Editor
{
    private string searchFilter = "";
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        BoneIdMapper mapper = (BoneIdMapper)target;
        SerializedProperty boneMappingsProp = serializedObject.FindProperty("boneMappings");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bone ID Mapper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Search field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            searchFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Auto-generate button
        if (GUILayout.Button("Auto-Generate Mappings"))
        {
            boneMappingsProp.ClearArray();

            OVRSkeleton.BoneId[] ovrBones = (OVRSkeleton.BoneId[])System.Enum.GetValues(typeof(OVRSkeleton.BoneId));

            // Create a mapping for each unique OVR bone
            HashSet<OVRSkeleton.BoneId> addedBones = new HashSet<OVRSkeleton.BoneId>();

            int index = 0;
            foreach (OVRSkeleton.BoneId ovrBone in ovrBones)
            {
                // Skip duplicates (enums with same integer value)
                if (addedBones.Contains(ovrBone))
                    continue;

                addedBones.Add(ovrBone);

                boneMappingsProp.InsertArrayElementAtIndex(index);
                SerializedProperty element = boneMappingsProp.GetArrayElementAtIndex(index);

                // Set OVR bone
                SerializedProperty ovrBoneProp = element.FindPropertyRelative("ovrBoneId");
                ovrBoneProp.enumValueIndex = System.Array.IndexOf(ovrBones, ovrBone);

                // Try to find matching HandJointId by name similarity
                SerializedProperty handJointProp = element.FindPropertyRelative("handJointId");
                string ovrBoneName = ovrBone.ToString();

                // Default to Invalid
                handJointProp.enumValueIndex = 0; // Assuming Invalid is first

                // Try to find matching name
                string[] handJointNames = handJointProp.enumNames;
                for (int j = 0; j < handJointNames.Length; j++)
                {
                    if (handJointNames[j].Replace("Hand", "").Replace("_", "") ==
                        ovrBoneName.Replace("Hand_", "").Replace("_", ""))
                    {
                        handJointProp.enumValueIndex = j;
                        break;
                    }
                }

                index++;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);

            Debug.Log($"Auto-generated {index} unique bone mappings");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total Mappings: {boneMappingsProp.arraySize}", EditorStyles.miniLabel);
        EditorGUILayout.Space();

        // Display filtered list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        serializedObject.Update();

        int displayedCount = 0;
        for (int i = 0; i < boneMappingsProp.arraySize; i++)
        {
            SerializedProperty element = boneMappingsProp.GetArrayElementAtIndex(i);
            SerializedProperty ovrBoneProp = element.FindPropertyRelative("ovrBoneId");
            SerializedProperty handJointProp = element.FindPropertyRelative("handJointId");

            string ovrBoneName = ovrBoneProp.enumNames[ovrBoneProp.enumValueIndex];
            string handJointName = handJointProp.enumNames[handJointProp.enumValueIndex];

            // Filter by search
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lowerSearch = searchFilter.ToLower();
                if (!ovrBoneName.ToLower().Contains(lowerSearch) &&
                    !handJointName.ToLower().Contains(lowerSearch))
                {
                    continue;
                }
            }

            displayedCount++;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(40));
            EditorGUILayout.PropertyField(ovrBoneProp, GUIContent.none);
            EditorGUILayout.LabelField("→", GUILayout.Width(20));
            EditorGUILayout.PropertyField(handJointProp, GUIContent.none);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                boneMappingsProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (!string.IsNullOrEmpty(searchFilter))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Showing {displayedCount} of {boneMappingsProp.arraySize}", EditorStyles.miniLabel);
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("Add New Mapping"))
        {
            boneMappingsProp.InsertArrayElementAtIndex(boneMappingsProp.arraySize);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
