using UnityEngine;
using Oculus.Interaction.Input;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that maps OVRSkeleton.BoneId to HandJointId
/// Provides 1:1 mapping between OVR bone system and OpenXR/Interaction SDK bone system
/// </summary>
[CreateAssetMenu(fileName = "BoneIdMapper", menuName = "HandSurvivor/Bone ID Mapper", order = 100)]
public class BoneIdMapper : ScriptableObject
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [System.Serializable]
    public struct BoneMapping
    {
        public OVRSkeleton.BoneId ovrBoneId;
        public HandJointId handJointId;
    }

    [SerializeField]
    private List<BoneMapping> boneMappings = new List<BoneMapping>();

    private Dictionary<OVRSkeleton.BoneId, HandJointId> ovrToHandJointCache;
    private Dictionary<HandJointId, OVRSkeleton.BoneId> handJointToOvrCache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        ovrToHandJointCache = new Dictionary<OVRSkeleton.BoneId, HandJointId>();
        handJointToOvrCache = new Dictionary<HandJointId, OVRSkeleton.BoneId>();

        foreach (BoneMapping mapping in boneMappings)
        {
            ovrToHandJointCache[mapping.ovrBoneId] = mapping.handJointId;
            handJointToOvrCache[mapping.handJointId] = mapping.ovrBoneId;
        }
    }

    public HandJointId GetHandJointId(OVRSkeleton.BoneId ovrBoneId)
    {
        if (ovrToHandJointCache == null)
        {
            BuildCache();
        }

        if (ovrToHandJointCache.TryGetValue(ovrBoneId, out HandJointId result))
        {
            return result;
        }

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


            Debug.LogWarning($"[BoneIdMapper] No mapping found for OVRBoneId: {ovrBoneId}");
        return HandJointId.Invalid;
    }

    public OVRSkeleton.BoneId GetOVRBoneId(HandJointId handJointId)
    {
        if (handJointToOvrCache == null)
        {
            BuildCache();
        }

        if (handJointToOvrCache.TryGetValue(handJointId, out OVRSkeleton.BoneId result))
        {
            return result;
        }

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


            Debug.LogWarning($"[BoneIdMapper] No mapping found for HandJointId: {handJointId}");
        return OVRSkeleton.BoneId.Invalid;
    }

    public List<BoneMapping> GetAllMappings()
    {
        return new List<BoneMapping>(boneMappings);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Generate Mappings")]
    private void AutoGenerateMappings()
    {
        boneMappings.Clear();

        System.Array ovrBones = System.Enum.GetValues(typeof(OVRSkeleton.BoneId));
        System.Array handJoints = System.Enum.GetValues(typeof(HandJointId));

        int maxCount = Mathf.Min(ovrBones.Length, handJoints.Length);

        for (int i = 0; i < maxCount; i++)
        {
            boneMappings.Add(new BoneMapping
            {
                ovrBoneId = (OVRSkeleton.BoneId)ovrBones.GetValue(i),
                handJointId = (HandJointId)handJoints.GetValue(i)
            });
        }

        UnityEditor.EditorUtility.SetDirty(this);
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[BoneIdMapper] Auto-generated {boneMappings.Count} bone mappings");
    }
#endif
}
