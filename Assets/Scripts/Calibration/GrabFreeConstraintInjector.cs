using Oculus.Interaction;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using UnityEngine;

public class GrabFreeConstraintInjector : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         private CustomGrabFreeTransformer _grabFreeTransformer;
    private Grabbable _grabbable;

    private TransformerUtils.PositionConstraints _initialPositionConstraints =
        new TransformerUtils.PositionConstraints();

    private TransformerUtils.RotationConstraints _initialRotationConstraints =
        new TransformerUtils.RotationConstraints();

    private TransformerUtils.ScaleConstraints _initialScaleConstraints = new TransformerUtils.ScaleConstraints();

    [SerializeField]
    private TransformerUtils.PositionConstraints _newPositionConstraints = new TransformerUtils.PositionConstraints();

    [SerializeField]
    private TransformerUtils.RotationConstraints _newRotationConstraints = new TransformerUtils.RotationConstraints();

    [SerializeField]
    private TransformerUtils.ScaleConstraints _newScaleConstraints = new TransformerUtils.ScaleConstraints();

    private void Start()
    {
        if (_grabFreeTransformer == null)
        {
            _grabFreeTransformer = GetComponent<CustomGrabFreeTransformer>();
        }

        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogError("Grabbable missing for GrabFreeConstraintInjector", gameObject);
        }

        SetInitialConstraints();
    }

    private void SetInitialConstraints()
    {
        _initialPositionConstraints = _grabFreeTransformer.PositionConstraints;
        _initialRotationConstraints = _grabFreeTransformer.RotationConstraints;
        _initialScaleConstraints = _grabFreeTransformer.ScaleConstraints;
    }


    public void SetNewPositionConstraints(TransformerUtils.PositionConstraints positionConstraints)
    {
        _newPositionConstraints = positionConstraints;
    }
//TODO Should be more flexible to allow us to pick to which Gameobjects to align which axis. 
    public void SetNewPositionConstraints(GameObject gameObject)
    {
        _newPositionConstraints.XAxis.AxisRange.Min = transform.position.x;
        _newPositionConstraints.XAxis.AxisRange.Max = transform.position.x;
        _newPositionConstraints.YAxis.AxisRange.Min = gameObject.transform.position.y - transform.position.y;
        _newPositionConstraints.YAxis.AxisRange.Max = gameObject.transform.position.y - transform.position.y;
        _newPositionConstraints.ZAxis.AxisRange.Min = transform.position.z;
        _newPositionConstraints.ZAxis.AxisRange.Max = transform.position.z;
    }

    public void SetShouldConstrainXPosition(bool shouldConstrain)
    {
        _newPositionConstraints.XAxis.ConstrainAxis = shouldConstrain;
 
    }
    
    public void SetShouldConstrainYPosition(bool shouldConstrain)
    {
        _newPositionConstraints.YAxis.ConstrainAxis = shouldConstrain;
 
    }
    
    public void SetShouldConstrainZPosition(bool shouldConstrain)
    {
        _newPositionConstraints.ZAxis.ConstrainAxis = shouldConstrain;
    }
   
    // need to implement rotation and scale bool injection

    public void SetNewConstraints(
        Vector3 newXMinPositionVector, Vector3 newXMaxPositionVector,
        Vector3 newMinYPositionVector, Vector3 newYMaxPositionVector,
        Vector3 newMinZPositionVector, Vector3 newZMaxPositionVector,
        Vector3 newXMinRotationVector, Vector3 newXMaxRotationVector,
        Vector3 newMinYRotationVector, Vector3 newYMaxRotationVector,
        Vector3 newMinZRotationVector, Vector3 newZMaxRotationVector,
        Vector3 newXMinScaleVector, Vector3 newXMaxScaleVector,
        Vector3 newMinYScaleVector, Vector3 newYMaxScaleVector,
        Vector3 newMinZScaleVector, Vector3 newZMaxScaleVector)
    {
        // Position constraints
        TransformerUtils.PositionConstraints newPositionConstraints = new TransformerUtils.PositionConstraints();
        newPositionConstraints.XAxis.AxisRange.Min = newXMinPositionVector.x;
        newPositionConstraints.XAxis.AxisRange.Max = newXMaxPositionVector.x;
        newPositionConstraints.YAxis.AxisRange.Min = newMinYPositionVector.y;
        newPositionConstraints.YAxis.AxisRange.Max = newYMaxPositionVector.y;
        newPositionConstraints.ZAxis.AxisRange.Min = newMinZPositionVector.z;
        newPositionConstraints.ZAxis.AxisRange.Max = newZMaxPositionVector.z;

        // Rotation constraints
        TransformerUtils.RotationConstraints newRotationConstraints = new TransformerUtils.RotationConstraints();
        newRotationConstraints.XAxis.AxisRange.Min = newXMinRotationVector.x;
        newRotationConstraints.XAxis.AxisRange.Max = newXMaxRotationVector.x;
        newRotationConstraints.YAxis.AxisRange.Min = newMinYRotationVector.y;
        newRotationConstraints.YAxis.AxisRange.Max = newYMaxRotationVector.y;
        newRotationConstraints.ZAxis.AxisRange.Min = newMinZRotationVector.z;
        newRotationConstraints.ZAxis.AxisRange.Max = newZMaxRotationVector.z;

        // Scale constraints
        TransformerUtils.ScaleConstraints newScaleConstraints = new TransformerUtils.ScaleConstraints();
        newScaleConstraints.XAxis.AxisRange.Min = newXMinScaleVector.x;
        newScaleConstraints.XAxis.AxisRange.Max = newXMaxScaleVector.x;
        newScaleConstraints.YAxis.AxisRange.Min = newMinYScaleVector.y;
        newScaleConstraints.YAxis.AxisRange.Max = newYMaxScaleVector.y;
        newScaleConstraints.ZAxis.AxisRange.Min = newMinZScaleVector.z;
        newScaleConstraints.ZAxis.AxisRange.Max = newZMaxScaleVector.z;

        // Apply the new constraints
        _newPositionConstraints = newPositionConstraints;
        _newRotationConstraints = newRotationConstraints;
        _newScaleConstraints = newScaleConstraints;
    }


    public void ApplyNewConstraints()
    {
        _grabFreeTransformer.InjectOptionalPositionConstraints(_newPositionConstraints);
        _grabFreeTransformer.InjectOptionalRotationConstraints(_newRotationConstraints);
        _grabFreeTransformer.InjectOptionalScaleConstraints(_newScaleConstraints);
        _grabFreeTransformer.Initialize(_grabbable);
    }

    public void ApplyNewPositionConstraints()
    {
        _grabFreeTransformer.ScaleConstraints.ConstraintsAreRelative = _newPositionConstraints.ConstraintsAreRelative;
        _grabFreeTransformer.InjectOptionalPositionConstraints(_newPositionConstraints);
        _grabFreeTransformer.Initialize(_grabbable);
    }

    public void ApplyNewRotationConstraints()
    {
        _grabFreeTransformer.InjectOptionalRotationConstraints(_newRotationConstraints);
        _grabFreeTransformer.Initialize(_grabbable);
    }

    public void ApplyNewScaleConstraints()
    {
        _grabFreeTransformer.InjectOptionalScaleConstraints(_newScaleConstraints);
        _grabFreeTransformer.Initialize(_grabbable);
    }

    public void ApplyInitialConstraints()
    {
        _grabFreeTransformer.InjectOptionalPositionConstraints(_initialPositionConstraints);
        _grabFreeTransformer.InjectOptionalRotationConstraints(_initialRotationConstraints);
        _grabFreeTransformer.InjectOptionalScaleConstraints(_initialScaleConstraints);
        _grabFreeTransformer.Initialize(_grabbable);
    }
}