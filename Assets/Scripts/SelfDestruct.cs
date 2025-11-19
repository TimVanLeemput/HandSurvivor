using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float Min;
    public float Max;

    public FloatRange(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public bool Contains(float value)
    {
        return value >= Min && value <= Max;
    }
}

public class SelfDestruct : MonoBehaviour
{
    [Header("Check Settings")]
    [SerializeField] private float _checkInterval = 3f;
    [SerializeField] private float _delay = 5f;
    
    [Header("X Axis Boundaries")]
    [SerializeField] private bool _checkXAxis = false;
    [SerializeField] private FloatRange _xRange = new FloatRange(-100f, 100f);

    [Header("Y Axis Boundaries")]
    [SerializeField] private bool _checkYAxis = true;
    [SerializeField] private FloatRange _yRange = new FloatRange(-50f, 50f);

    [Header("Z Axis Boundaries")]
    [SerializeField] private bool _checkZAxis = false;
    [SerializeField] private FloatRange _zRange = new FloatRange(-100f, 100f);

    private void Start()
    {
        InvokeRepeating(nameof(CheckBounds), _checkInterval, _checkInterval);
    }

    private void CheckBounds()
    {
        Vector3 position = transform.position;

        if (_checkXAxis && !_xRange.Contains(position.x))
        {
            Destroy(gameObject);
            return;
        }

        if (_checkYAxis && !_yRange.Contains(position.y))
        {
            Destroy(gameObject);
            return;
        }

        if (_checkZAxis && !_zRange.Contains(position.z))
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public void DestroyWithDelay()
    {
        Invoke(nameof(Destroy), _delay);
    }
    
    public void DestroyWithDelay(float delay)
    {
        Invoke(nameof(Destroy), delay);
    }

    public void CancelSelfDestruct()
    {
        CancelInvoke(nameof(Destroy));
        CancelInvoke(nameof(CheckBounds));
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

}
