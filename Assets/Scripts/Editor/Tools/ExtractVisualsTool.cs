using UnityEditor;
using UnityEngine;

public static class ExtractVisualsTool
{
    [MenuItem("Tools/Transform/Move Mesh to Child Visuals %#&g")] // CTRL + ALT + SHIFT + G
    private static void MoveMeshToChildVisuals()
    {
        GameObject selectedGO = Selection.activeGameObject;

        if (selectedGO == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        MeshFilter meshFilter = selectedGO.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = selectedGO.GetComponent<MeshRenderer>();

        if (meshFilter == null && meshRenderer == null)
        {
            Debug.LogWarning($"'{selectedGO.name}' has no MeshFilter or MeshRenderer to move.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedGO, "Move Mesh to Child Visuals");

        // Create "Visuals" parent child
        Transform visualsParent = selectedGO.transform.Find("Visuals");
        GameObject visualsParentGO;

        if (visualsParent == null)
        {
            visualsParentGO = new GameObject($"Visuals-{selectedGO.name}");
            Undo.RegisterCreatedObjectUndo(visualsParentGO, "Move Mesh to Child Visuals");
            visualsParentGO.transform.SetParent(selectedGO.transform);
            visualsParentGO.transform.localPosition = Vector3.zero;
            visualsParentGO.transform.localRotation = Quaternion.identity;
            visualsParentGO.transform.localScale = Vector3.one;
        }
        else
        {
            visualsParentGO = visualsParent.gameObject;
        }

        // Create the actual visual child
        GameObject visualChild = new GameObject($"{selectedGO.name}-Mesh");
        Undo.RegisterCreatedObjectUndo(visualChild, "Move Mesh to Child Visuals");
        visualChild.transform.SetParent(visualsParentGO.transform);
        visualChild.transform.localPosition = Vector3.zero;
        visualChild.transform.localRotation = Quaternion.identity;
        visualChild.transform.localScale = Vector3.one;

        // Move components to the child
        if (meshFilter != null)
        {
            MeshFilter newMeshFilter = visualChild.AddComponent<MeshFilter>();
            newMeshFilter.sharedMesh = meshFilter.sharedMesh;
            Undo.DestroyObjectImmediate(meshFilter);
        }

        if (meshRenderer != null)
        {
            MeshRenderer newMeshRenderer = visualChild.AddComponent<MeshRenderer>();
            newMeshRenderer.sharedMaterials = meshRenderer.sharedMaterials;
            Undo.DestroyObjectImmediate(meshRenderer);
        }

        Debug.Log(
            $"Moved mesh components from '{selectedGO.name}' to child hierarchy: Visuals/Visuals-{selectedGO.name}");
    }
}