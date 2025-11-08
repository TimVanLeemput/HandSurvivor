
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

        // Ajouter le Rigidbody s'il n'existe pas déjà
        Rigidbody rb = selectedObject.AddComponent<Rigidbody>();

        // Ajouter le script Grabbable s'il n'existe pas déjà
        Grabbable grabbable = selectedObject.AddComponent<Grabbable>();

        // Créer un objet enfant pour HandGrabInteractable
        GameObject childObject = new GameObject("HandGrabInteractable");
        childObject.transform.SetParent(selectedObject.transform);

        // Ajouter le script HandGrabInteractable à l'enfant
        HandGrabInteractable handGrabInteractable = childObject.AddComponent<HandGrabInteractable>();
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
