using UnityEditor;
using UnityEngine;

public static class ExtractColliderTool
{
    [MenuItem("Tools/Transform/Move Collider to Child %#&k")] // CTRL + ALT + SHIFT + K
    private static void MoveColliderToChild()
    {
        GameObject selectedGO = Selection.activeGameObject;
        
        if (selectedGO == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        Collider collider = selectedGO.GetComponent<Collider>();

        if (collider == null)
        {
            Debug.LogWarning($"'{selectedGO.name}' has no Collider to move.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedGO, "Move Collider to Child");

        // Create the collider child
        GameObject colliderChild = new GameObject($"Collider-{selectedGO.name}");
        Undo.RegisterCreatedObjectUndo(colliderChild, "Move Collider to Child");
        colliderChild.transform.SetParent(selectedGO.transform);
        colliderChild.transform.localPosition = Vector3.zero;
        colliderChild.transform.localRotation = Quaternion.identity;
        colliderChild.transform.localScale = Vector3.one;

        // Move the collider component to the child
        UnityEditorInternal.ComponentUtility.CopyComponent(collider);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(colliderChild);
        Undo.DestroyObjectImmediate(collider);

        Debug.Log($"Moved collider from '{selectedGO.name}' to child 'Collider-{selectedGO.name}'");
    }
}
