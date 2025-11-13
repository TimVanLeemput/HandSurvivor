using UnityEngine;
using HandSurvivor.ActiveSkills;

public class SlotTester : MonoBehaviour
{
    [SerializeField] private GameObject laserSkillPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddSkillToSlot();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowSlotStatus();
        }
    }

    void AddSkillToSlot()
    {
        if (laserSkillPrefab == null) return;

        GameObject skillObj = Instantiate(laserSkillPrefab);
        ActiveSkillBase skill =
            skillObj.GetComponent<ActiveSkillBase>();

        if (skill != null)
        {
            skill.Pickup();
            Debug.Log($"Added skill. Slots: {ActiveSkillSlotManager.Instance.GetFilledSlotCount()}/4");
        }
    }

    void ShowSlotStatus()
    {
        if (ActiveSkillSlotManager.Instance != null)
        {
            Debug.Log($"Filled slots: {ActiveSkillSlotManager.Instance.GetFilledSlotCount()}/{ActiveSkillSlotManager.Instance.MaxSlots}");
        }
    }
}

