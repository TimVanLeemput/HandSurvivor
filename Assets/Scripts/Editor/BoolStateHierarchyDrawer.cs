using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class BoolStateHierarchyDrawer
{
    private static readonly Color TrueColor = new Color(0.2f, 0.8f, 0.2f);
    private static readonly Color FalseColor = new Color(0.8f, 0.2f, 0.2f);
    private const float DotSize = 8f;
    private const float DotSpacing = 12f;
    private const float RightMargin = 16f;

    static BoolStateHierarchyDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
    }

    private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
        {
            return;
        }

        BoolState boolState = gameObject.GetComponent<BoolState>();
        if (boolState != null)
        {
            DrawSingleDot(selectionRect, boolState.State);
            return;
        }

        BoolStateChecker boolStateChecker = gameObject.GetComponent<BoolStateChecker>();
        if (boolStateChecker != null)
        {
            DrawMultipleDots(selectionRect, boolStateChecker);
        }
    }

    private static void DrawSingleDot(Rect rect, bool state)
    {
        Rect dotRect = new Rect(
            rect.xMax - RightMargin - DotSize,
            rect.y + (rect.height - DotSize) / 2f,
            DotSize,
            DotSize
        );

        Color color = state ? TrueColor : FalseColor;
        EditorGUI.DrawRect(dotRect, color);
    }

    private static void DrawMultipleDots(Rect rect, BoolStateChecker checker)
    {
        if (checker.BoolStates == null || checker.BoolStates.Count == 0)
        {
            return;
        }

        float startX = rect.xMax - RightMargin - DotSize;

        for (int i = checker.BoolStates.Count - 1; i >= 0; i--)
        {
            BoolState boolState = checker.BoolStates[i];
            if (boolState == null)
            {
                continue;
            }

            Rect dotRect = new Rect(
                startX - (checker.BoolStates.Count - 1 - i) * DotSpacing,
                rect.y + (rect.height - DotSize) / 2f,
                DotSize,
                DotSize
            );

            Color color = boolState.State ? TrueColor : FalseColor;
            EditorGUI.DrawRect(dotRect, color);
        }
    }
}
