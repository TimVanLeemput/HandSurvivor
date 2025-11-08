using UnityEditor;
using UnityEngine;

public static class TransformCopyPasteTool
{
    private static Matrix4x4 copiedMatrix;
    private static bool hasCopied;

    [MenuItem("Tools/Transform/Copy World Transform %#&c")] // CTRL + ALT + SHIFT + C
    private static void CopyWorldTransform()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("No object selected to copy transform from.");
            return;
        }

        copiedMatrix = Selection.activeTransform.localToWorldMatrix;
        hasCopied = true;

        Debug.Log($"Copied world transform from '{Selection.activeTransform.name}'.");
    }

    [MenuItem("Tools/Transform/Paste World Transform %#&v")] // CTRL + ALT + SHIFT + V
    private static void PasteWorldTransform()
    {
        if (!hasCopied)
        {
            Debug.LogWarning("No copied transform found. Copy a transform first.");
            return;
        }

        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("No object selected to paste transform to.");
            return;
        }

        Undo.RecordObject(Selection.activeTransform, "Paste World Transform");

        Transform t = Selection.activeTransform;

        // Extract position
        Vector3 position = copiedMatrix.GetColumn(3);

        // Extract scale (preserving negative scales)
        Vector3 xAxis = copiedMatrix.GetColumn(0);
        Vector3 yAxis = copiedMatrix.GetColumn(1);
        Vector3 zAxis = copiedMatrix.GetColumn(2);

        Vector3 scale = new Vector3(xAxis.magnitude, yAxis.magnitude, zAxis.magnitude);
        
        // Check for negative scales by examining determinant
        if (copiedMatrix.determinant < 0)
        {
            scale.x *= -1;
        }

        // Extract rotation (normalize axes for pure rotation)
        Quaternion rotation = Quaternion.LookRotation(
            zAxis / scale.z,
            yAxis / scale.y
        );

        // Apply position and rotation
        t.SetPositionAndRotation(position, rotation);

        // Apply scale
        if (t.parent != null)
        {
            Vector3 parentScale = t.parent.lossyScale;
            t.localScale = new Vector3(
                scale.x / parentScale.x,
                scale.y / parentScale.y,
                scale.z / parentScale.z
            );
        }
        else
        {
            t.localScale = scale;
        }

        Debug.Log($"Pasted world transform to '{t.name}'.");
    }
}