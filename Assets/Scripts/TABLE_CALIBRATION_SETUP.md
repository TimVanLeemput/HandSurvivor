# Table/Desk AR Calibration Setup Guide

This guide explains how to set up table/desk AR calibration for HandSurvivor using Meta's MR Utility Kit.

## Overview

The table calibration system allows HandSurvivor to detect and calibrate to real-world table/desk surfaces in mixed reality mode. This enables enemies and gameplay elements to spawn on physical surfaces.

## Prerequisites

- Unity 6+ with HandSurvivor project open
- Meta XR SDK v81 installed (already configured in this project)
- Meta Quest headset with space setup completed
- Unity Editor connected to Quest via Link/Air Link for testing

## Scene Setup Instructions

### Step 1: Set Up VR Camera Rig

1. **Delete the Main Camera** in your scene (if not already using OVRCameraRig)
   - Select "Main Camera" in Hierarchy
   - Press Delete

2. **Add OVRCameraRig Prefab**
   - Open the Packages window (Window > Package Manager)
   - Find "Meta XR Core SDK"
   - In the Project window, navigate to: `Packages/Meta XR Core SDK/Prefabs/`
   - Drag **OVRCameraRig.prefab** into your scene Hierarchy
   - Position it at (0, 0, 0)

3. **Configure OVRCameraRig** (Inspector)
   - **Tracking Origin Type**: Floor Level
   - **Enable Hand Tracking**: True (for hand combat mechanics)
   - **Hand Tracking Support**: Controllers And Hands

### Step 2: Add MRUK for Scene Understanding

1. **Create MRUK GameObject**
   - Right-click in Hierarchy > Create Empty
   - Rename to "MRUK"
   - Position at (0, 0, 0)

2. **Add MRUK Component**
   - Select the MRUK GameObject
   - In Inspector, click "Add Component"
   - Search for "MRUK" and add the component

3. **Configure MRUK Settings**
   - **Scene Data Source**: Device (or DeviceWithPrefabFallback for editor testing)
   - **Room Filter**: Current Room Only
   - **Enable World Lock**: True
   - **Create Plane Meshes**: True (to visualize detected surfaces)

### Step 3: Add Table Calibration Manager

1. **Create TableCalibration GameObject**
   - Right-click in Hierarchy > Create Empty
   - Rename to "TableCalibrationManager"

2. **Add TableCalibrationManager Script**
   - Select the TableCalibrationManager GameObject
   - In Inspector, click "Add Component"
   - Search for "TableCalibrationManager" and add it

3. **Configure Calibration Settings**
   - **Min Table Height**: 0.5 (50cm - low coffee tables)
   - **Max Table Height**: 1.2 (120cm - high standing desks)
   - **Min Table Area**: 0.3 (0.3 square meters minimum)
   - **Auto Calibrate**: True (automatically find table on start)
   - **Allow Manual Selection**: True
   - **Show Debug Gizmos**: True (helpful during development)

4. **Optional: Create Table Surface Material**
   - Right-click in Project > Create > Material
   - Name it "TableSurfaceMaterial"
   - Configure shader/color (e.g., semi-transparent green to visualize play area)
   - Drag this material to the **Table Surface Material** field in TableCalibrationManager

### Step 4: Request Scene Permissions (Android Manifest)

The MR features require scene permissions on Quest. Meta XR SDK should handle this automatically, but verify:

1. **Check Android Manifest**
   - Navigate to: `Assets/Plugins/Android/AndroidManifest.xml`
   - Ensure it includes:
   ```xml
   <uses-permission android:name="com.oculus.permission.USE_SCENE" />
   ```

2. If the file doesn't exist, it will be auto-generated during build. The SDK's OVRProjectConfig should handle permissions.

### Step 5: Configure Build Settings for MR

1. **Open Build Settings** (File > Build Settings)
   - Platform: Android
   - Texture Compression: ASTC

2. **Player Settings** (click "Player Settings" button)
   - **XR Plug-in Management**:
     - Oculus: Enabled
   - **Other Settings**:
     - Color Space: Linear
     - Minimum API Level: Android 10.0 (API 29)
   - **Publishing Settings**:
     - Package Name: com.yourcompany.handsurvivor

### Step 6: Enable Scene Permissions in OVRManager

1. Find or create an OVRManager in your scene:
   - If you added OVRCameraRig, it includes OVRManager
   - Select the OVRCameraRig object

2. In the OVRManager component:
   - **Quest Features**:
     - **General** > **Scene Support**: Required
     - **Passthrough Support**: Required

## Usage in Code

### Accessing the Calibrated Table

```csharp
using HandSurvivor;

public class GameplayManager : MonoBehaviour
{
    private TableCalibrationManager tableCalibration;

    void Start()
    {
        tableCalibration = FindObjectOfType<TableCalibrationManager>();

        if (tableCalibration != null)
        {
            // Subscribe to calibration events
            tableCalibration.OnTableCalibrated.AddListener(OnTableReady);
            tableCalibration.OnCalibrationFailed.AddListener(OnCalibrationFailed);
        }
    }

    private void OnTableReady(Meta.XR.MRUtilityKit.MRUKAnchor table)
    {
        Debug.Log("Table calibrated! Ready to spawn enemies.");

        // Get table center for spawning
        Vector3 tableCenter = tableCalibration.GetTableCenter();

        // Spawn enemy on table
        SpawnEnemyOnTable(tableCenter);
    }

    private void OnCalibrationFailed()
    {
        Debug.LogWarning("Table calibration failed. Please ensure space setup is complete.");
    }

    void SpawnEnemyOnTable(Vector3 tableCenter)
    {
        // Spawn enemies at table center with slight random offset
        Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
        Vector3 spawnPos = tableCalibration.GetTablePosition(randomOffset);

        // Instantiate your enemy prefab here
        // Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
```

### Getting Table Bounds for Wave Spawning

```csharp
// Get the playable area bounds
Bounds tableBounds = tableCalibration.GetTableBounds();

// Spawn enemies within bounds
Vector3 randomPosOnTable = new Vector3(
    Random.Range(tableBounds.min.x, tableBounds.max.x),
    tableBounds.center.y + 0.05f, // Slightly above table
    Random.Range(tableBounds.min.z, tableBounds.max.z)
);
```

## Testing in Unity Editor

### With Meta Quest Link

1. **Connect Quest via Link/Air Link**
   - Enable Quest Link on your headset
   - Connect to PC via USB or WiFi

2. **Complete Space Setup on Quest**
   - Open the Quest settings
   - Navigate to Space Setup
   - Scan your room and define surfaces (especially tables)

3. **Run in Unity Editor**
   - Press Play in Unity
   - The TableCalibrationManager will attempt to detect tables
   - Check Console for detection logs
   - Look in Scene view to see Gizmos for detected surfaces (green = calibrated, yellow = candidates)

### Without Quest Link (Prefab Fallback)

For testing without a headset:

1. **Change MRUK Data Source**
   - Select MRUK GameObject
   - Set **Scene Data Source**: Prefab or DeviceWithPrefabFallback

2. **Add Test Room Prefab**
   - Navigate to `Packages/Meta XR MR Utility Kit/Core/Rooms/Prefabs/`
   - Find a room prefab (e.g., "Bedroom00.prefab")
   - Drag it into the scene as a child of MRUK
   - The calibration system will use this synthetic room data

## Troubleshooting

### "No valid table surfaces found"

- **Cause**: No surfaces detected that match height/size criteria
- **Fix**:
  - Ensure Space Setup is complete on Quest
  - Adjust min/max height in TableCalibrationManager
  - Reduce minTableArea if testing on small surfaces

### "MRUK instance not found"

- **Cause**: MRUK component missing from scene
- **Fix**: Follow Step 2 to add MRUK GameObject

### Scene permissions denied

- **Cause**: User denied scene access permission
- **Fix**:
  - Go to Quest Settings > Apps > HandSurvivor > Permissions
  - Enable "Spatial Data" permission

### Table detection works but enemies don't spawn

- **Cause**: Game code not using TableCalibrationManager API
- **Fix**: Use `GetTablePosition()` or `GetTableCenter()` to get spawn coordinates (see Usage section above)

## Next Steps

After table calibration is working:

1. **Integrate with Wave Manager** (HAN-23): Use table bounds to determine enemy spawn positions
2. **Enemy Navigation** (HAN-6): Ensure nav mesh is generated on table surface
3. **Visual Feedback**: Consider adding AR grid overlay on table surface during gameplay

## Additional Resources

- [Meta MR Utility Kit Documentation](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-overview)
- [Meta Scene Understanding Guide](https://developers.meta.com/horizon/documentation/unity/unity-scene-overview)
- [OVRCameraRig Setup](https://developers.meta.com/horizon/documentation/unity/unity-camera-rig)
