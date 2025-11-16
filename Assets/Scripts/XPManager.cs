using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class XPManager : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public static XPManager Instance;
    public GameObject XPPrefab;
    public Transform XPParent;

    [Header("Level System")]
    public int Level = 1;
    public int XPIncreasePerLevel = 100;
    [SerializeField,ReadOnly] private int CurrentXPForLevel = 1000;
    [SerializeField,ReadOnly] private int CurrentXP = 0;

    [Header("Events")]
    public UnityEvent<int> OnLevelUp;
    public UnityEvent<int> OnXPAdded;

    private void Awake()
    {
        Instance = this;
    }

    public void AddXP(int amount)
    {
        CurrentXP += amount;
        if (CurrentXP >= CurrentXPForLevel)
        {
            int over = CurrentXP - CurrentXPForLevel;
            CurrentXPForLevel += XPIncreasePerLevel;
            CurrentXP = over;
            LevelUp();
        }
        OnXPAdded?.Invoke(amount);
    }

    public void LevelUp()
    {
        Level++;
        OnLevelUp?.Invoke(Level);
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[XPManager] Level Up! Now level {Level}");
    }

    public void DropXP(int xpAmount, Vector3 position)
    {
        GameObject go = Instantiate(XPPrefab, XPParent);
        go.transform.position = position;
        go.GetComponent<XPDroplet>().XPAmount = xpAmount;
    }
    
    public void DropXP(Transform transform)
    {
        GameObject go = Instantiate(XPPrefab, XPParent);
        go.transform.position = transform.position;
        go.GetComponent<XPDroplet>().XPAmount = CurrentXPForLevel;
    }
}