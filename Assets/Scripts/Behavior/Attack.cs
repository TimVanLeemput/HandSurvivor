using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Attack")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Attack", message: "Attack", category: "Events", id: "c00b28a9a0b4409271292bb7d0661f6b")]
public sealed partial class Attack : EventChannel { }

