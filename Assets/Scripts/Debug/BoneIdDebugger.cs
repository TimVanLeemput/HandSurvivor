using UnityEngine;
using System;
using System.Collections.Generic;

public class BoneIdDebugger : MonoBehaviour
{
    [Header("Bone ID Testing")]
     [SerializeField]
    private List<OVRSkeleton.BoneId> allBoneIds = new List<OVRSkeleton.BoneId>
    (
        // Initialize with all BoneIds from 0 to 84
        new OVRSkeleton.BoneId[]
        {
            (OVRSkeleton.BoneId)0,
            (OVRSkeleton.BoneId)1,
            (OVRSkeleton.BoneId)2,
            (OVRSkeleton.BoneId)3,
            (OVRSkeleton.BoneId)4,
            (OVRSkeleton.BoneId)5,
            (OVRSkeleton.BoneId)6,
            (OVRSkeleton.BoneId)7,
            (OVRSkeleton.BoneId)8,
            (OVRSkeleton.BoneId)9,
            (OVRSkeleton.BoneId)10,
            (OVRSkeleton.BoneId)11,
            (OVRSkeleton.BoneId)12,
            (OVRSkeleton.BoneId)13,
            (OVRSkeleton.BoneId)14,
            (OVRSkeleton.BoneId)15,
            (OVRSkeleton.BoneId)16,
            (OVRSkeleton.BoneId)17,
            (OVRSkeleton.BoneId)18,
            (OVRSkeleton.BoneId)19,
            (OVRSkeleton.BoneId)20,
            (OVRSkeleton.BoneId)21,
            (OVRSkeleton.BoneId)22,
            (OVRSkeleton.BoneId)23,
            (OVRSkeleton.BoneId)24,
            (OVRSkeleton.BoneId)25,
            (OVRSkeleton.BoneId)26,
            (OVRSkeleton.BoneId)27,
            (OVRSkeleton.BoneId)28,
            (OVRSkeleton.BoneId)29,
            (OVRSkeleton.BoneId)30,
            (OVRSkeleton.BoneId)31,
            (OVRSkeleton.BoneId)32,
            (OVRSkeleton.BoneId)33,
            (OVRSkeleton.BoneId)34,
            (OVRSkeleton.BoneId)35,
            (OVRSkeleton.BoneId)36,
            (OVRSkeleton.BoneId)37,
            (OVRSkeleton.BoneId)38,
            (OVRSkeleton.BoneId)39,
            (OVRSkeleton.BoneId)40,
            (OVRSkeleton.BoneId)41,
            (OVRSkeleton.BoneId)42,
            (OVRSkeleton.BoneId)43,
            (OVRSkeleton.BoneId)44,
            (OVRSkeleton.BoneId)45,
            (OVRSkeleton.BoneId)46,
            (OVRSkeleton.BoneId)47,
            (OVRSkeleton.BoneId)48,
            (OVRSkeleton.BoneId)49,
            (OVRSkeleton.BoneId)50,
            (OVRSkeleton.BoneId)51,
            (OVRSkeleton.BoneId)52,
            (OVRSkeleton.BoneId)53,
            (OVRSkeleton.BoneId)54,
            (OVRSkeleton.BoneId)55,
            (OVRSkeleton.BoneId)56,
            (OVRSkeleton.BoneId)57,
            (OVRSkeleton.BoneId)58,
            (OVRSkeleton.BoneId)59,
            (OVRSkeleton.BoneId)60,
            (OVRSkeleton.BoneId)61,
            (OVRSkeleton.BoneId)62,
            (OVRSkeleton.BoneId)63,
            (OVRSkeleton.BoneId)64,
            (OVRSkeleton.BoneId)65,
            (OVRSkeleton.BoneId)66,
            (OVRSkeleton.BoneId)67,
            (OVRSkeleton.BoneId)68,
            (OVRSkeleton.BoneId)69,
            (OVRSkeleton.BoneId)70,
            (OVRSkeleton.BoneId)71,
            (OVRSkeleton.BoneId)72,
            (OVRSkeleton.BoneId)73,
            (OVRSkeleton.BoneId)74,
            (OVRSkeleton.BoneId)75,
            (OVRSkeleton.BoneId)76,
            (OVRSkeleton.BoneId)77,
            (OVRSkeleton.BoneId)78,
            (OVRSkeleton.BoneId)79,
            (OVRSkeleton.BoneId)80,
            (OVRSkeleton.BoneId)81,
            (OVRSkeleton.BoneId)82,
            (OVRSkeleton.BoneId)83,
            (OVRSkeleton.BoneId)84
        }
    );
    
    private void Start()
    {
        Debug.Log("=== BONE ID DEBUGGER START ===", gameObject);
        foreach (OVRSkeleton.BoneId boneId in allBoneIds)
        {
            DebugBone(allBoneIds.IndexOf(boneId), boneId);
        }
        Debug.Log("=== BONE ID DEBUGGER END ===", gameObject);

        Debug.Log("=== ALL ENUM VALUES ===", gameObject);
        Array enumValues = Enum.GetValues(typeof(OVRSkeleton.BoneId));
        foreach (OVRSkeleton.BoneId boneId in enumValues)
        {
            int enumIntValue = (int)boneId;
            Debug.Log($"  Enum[{enumIntValue}] = {boneId}", gameObject);
        }
        Debug.Log($"=== Total: {enumValues.Length} bone IDs ===", gameObject);
    }

    private void DebugBone(int index, OVRSkeleton.BoneId boneId)
    {
        int enumValue = (int)boneId;
        Debug.Log($"  requiredBone{index}: {boneId} (enum int value: {enumValue})", gameObject);
    }
}
