using System;
using Oculus.Interaction.Input;
using UnityEngine;

public class CustomVRManager : MonoBehaviour
{
    private OVRCameraRigRef _oVRCameraRigRef = null;
    private HandPokeInteractorController _handPokeInteractorController = null;


    private void Start()
    {
        _oVRCameraRigRef = FindFirstObjectByType<OVRCameraRigRef>();
        _handPokeInteractorController = FindFirstObjectByType<HandPokeInteractorController>();
        // Debug.Log($"Found _oVRCameraRigRef => {_oVRCameraRigRef.gameObject.gameObject}", _oVRCameraRigRef.gameObject);
    }
}