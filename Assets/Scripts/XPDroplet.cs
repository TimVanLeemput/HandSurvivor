using UnityEngine;

public class XPDroplet : MonoBehaviour
{
    public int XPAmount;
    
    public void OnDropLetCollected()
    {
        XPManager.Instance.AddXP(XPAmount);
    }
}