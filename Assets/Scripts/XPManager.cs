using UnityEngine;
using UnityEngine.Events;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance;
    public GameObject XPPrefab;
    public Transform XPParent;

    [Header("Level System")]
    public int Level = 1;
    public int XPIncreasePerLevel = 100;
    private int CurrentXPForLevel = 1000;
    private int CurrentXP = 0;

    [Header("Events")]
    public UnityEvent<int> OnLevelUp;

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
    }

    public void LevelUp()
    {
        Level++;
        OnLevelUp?.Invoke(Level);
        Debug.Log($"[XPManager] Level Up! Now level {Level}");
    }

    public void DropXP(int xpAmount, Vector3 position)
    {
        GameObject go = Instantiate(XPPrefab, XPParent);
        go.transform.position = position;
        go.GetComponent<XPDroplet>().XPAmount = xpAmount;
    }
}