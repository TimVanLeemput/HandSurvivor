
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEditor;

public class MakeGrabbableEditor : EditorWindow
{
    [MenuItem("Tools/Make Selection Grabbable")]
    private static void MakeSelectionGrabbable()
    {
        GameObject selectedObject = Selection.activeGameObject;
        
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a GameObject in the scene.", "OK");
            return;
        }

        // Ajouter le Rigidbody
        Rigidbody rb = selectedObject.AddComponent<Rigidbody>();

        // Ajouter le script Grabbable
        Grabbable grabbable = selectedObject.AddComponent<Grabbable>();

        // Ajouter le script HandGrabInteractable 
        HandGrabInteractable handGrabInteractable = selectedObject.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rb);
        
        EditorUtility.SetDirty(selectedObject);
    }

    [MenuItem("Tools/Make Selection Grabbable", true)]
    private static bool ValidateMakeSelectionGrabbable()
    {
        // Le menu est actif seulement si un GameObject est sélectionné
        return Selection.activeGameObject != null;
    }
}
