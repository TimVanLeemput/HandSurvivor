# Hand Selection System (HAN-11)

## Overview

The Hand Selection System allows players to configure which hand is their dominant hand throughout the game. This affects gameplay mechanics:

- **Main Hand** (Dominant): Used for physical damage attacks
- **Off Hand**: Used for spirit abilities, collecting powerups, and launching spirit projectiles

## Components

### 1. Core Data (`HandPreference.cs`)

```csharp
// Enums
public enum HandType { Left, Right }
public enum HandRole { MainHand, OffHand }

// Data class
public class HandPreference
{
    public HandType MainHand { get; set; }
    public HandType OffHand { get; }
    public HandRole GetHandRole(HandType hand)
    public HandType GetHandForRole(HandRole role)
    public bool IsMainHand(HandType hand)
    public bool IsOffHand(HandType hand)
    public void SwapHands()
}
```

### 2. Manager (`HandSelectionManager.cs`)

Singleton manager that persists across scenes and handles storage.

**Key Features:**
- Auto-loads from PlayerPrefs on startup
- Auto-saves on every change
- Provides events for hand preference changes
- Singleton pattern with static helpers

**Events:**
- `OnMainHandChanged` - Triggered when main hand selection changes
- `OnPreferenceLoaded` - Triggered when preference is loaded from storage

### 3. UI Component (`HandSelectionUI.cs`)

Ready-to-use UI component for hand selection interface.

**Features:**
- Visual feedback (button colors)
- Role descriptions
- Optional auto-close after selection
- Works with traditional UI and VR canvases

### 4. Example Usage (`HandSelectionExample.cs`)

Demonstrates how to integrate the system into gameplay code.

## Quick Start

### Setup

1. **Add HandSelectionManager to scene:**
   ```
   - Create empty GameObject named "HandSelectionManager"
   - Add HandSelectionManager component
   - It will auto-persist via DontDestroyOnLoad
   ```

2. **Create hand selection UI:**
   ```
   - Create Canvas (or use existing VR canvas)
   - Add panel for hand selection
   - Add HandSelectionUI component to panel
   - Assign buttons and text fields in inspector
   ```

3. **Configure UI:**
   - Left Hand Button
   - Right Hand Button
   - Selection Text (optional)
   - Hand Labels (optional)
   - Selected/Unselected colors

### Usage in Gameplay

#### Method 1: Static Helpers (Simplest)

```csharp
using HandSurvivor;

// Check which hand is main/off
HandType mainHand = HandSelectionManager.GetMainHand();
HandType offHand = HandSelectionManager.GetOffHand();

// Check if specific hand is main/off
if (HandSelectionManager.CheckIsMainHand(HandType.Right))
{
    PerformPhysicalAttack();
}
```

#### Method 2: Instance Access

```csharp
using HandSurvivor;

// Get role of specific hand
HandRole role = HandSelectionManager.Instance.GetHandRole(detectedHand);

if (role == HandRole.MainHand)
{
    // Physical damage logic
}
else
{
    // Spirit ability logic
}
```

#### Method 3: Event Subscription

```csharp
using HandSurvivor;

private void Start()
{
    // Subscribe to hand changes
    HandSelectionManager.Instance.OnMainHandChanged.AddListener(OnHandChanged);
}

private void OnHandChanged(HandType newMainHand)
{
    // Update weapon positions, UI, etc.
    UpdateGameplayElements();
}

private void OnDestroy()
{
    HandSelectionManager.Instance.OnMainHandChanged.RemoveListener(OnHandChanged);
}
```

## Integration Examples

### Example 1: Hand Tracking Input

```csharp
public class HandController : MonoBehaviour
{
    [SerializeField] private HandType thisHand;

    private void OnTriggerPressed()
    {
        if (HandSelectionManager.CheckIsMainHand(thisHand))
        {
            // This is main hand - physical damage
            PerformMeleeAttack();
        }
        else
        {
            // This is off hand - spirit ability
            LaunchSpiritProjectile();
        }
    }
}
```

### Example 2: Powerup Collection

```csharp
public class PowerupCollector : MonoBehaviour
{
    [SerializeField] private HandType thisHand;

    private void OnPowerupCollected(Powerup powerup)
    {
        // Bonus points if collected with off hand (spirit hand)
        if (HandSelectionManager.CheckIsOffHand(thisHand))
        {
            powerup.ApplyBonus();
            Debug.Log("Spirit hand bonus!");
        }

        powerup.Collect();
    }
}
```

### Example 3: Dynamic Weapon Attachment

```csharp
public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private GameObject physicalWeapon;
    [SerializeField] private GameObject spiritCollector;

    private void Start()
    {
        HandSelectionManager.Instance.OnMainHandChanged.AddListener(UpdateWeaponPositions);
        UpdateWeaponPositions(HandSelectionManager.GetMainHand());
    }

    private void UpdateWeaponPositions(HandType mainHand)
    {
        Transform mainHandAnchor = mainHand == HandType.Left ? leftHandAnchor : rightHandAnchor;
        Transform offHandAnchor = mainHand == HandType.Left ? rightHandAnchor : leftHandAnchor;

        // Attach physical weapon to main hand
        physicalWeapon.transform.SetParent(mainHandAnchor, false);

        // Attach spirit collector to off hand
        spiritCollector.transform.SetParent(offHandAnchor, false);
    }
}
```

## Persistence

Hand preference is automatically saved to PlayerPrefs with key: `"HandSurvivor_MainHand"`

**Default:** Right hand if no preference saved

**Manual Controls:**
- `HandSelectionManager.Instance.SetMainHand(HandType hand)` - Set preference
- `HandSelectionManager.Instance.SwapHands()` - Toggle between hands
- `HandSelectionManager.Instance.ResetToDefault()` - Reset to right hand

## UI Setup Guide

### Required UI Elements

1. **Left Hand Button**
   - Button component
   - Optional: Image for visual feedback

2. **Right Hand Button**
   - Button component
   - Optional: Image for visual feedback

3. **Selection Text** (Optional)
   - TextMeshProUGUI component
   - Displays current selection and descriptions

4. **Hand Labels** (Optional)
   - TextMeshProUGUI for each button
   - Shows hand name and current role

### Inspector Configuration

```
HandSelectionUI Component:
├── UI References
│   ├── Left Hand Button
│   ├── Right Hand Button
│   ├── Selection Text (optional)
│   ├── Left Hand Label (optional)
│   └── Right Hand Label (optional)
├── Visual Feedback
│   ├── Selected Color (default: green)
│   ├── Unselected Color (default: gray)
│   └── Show Role Descriptions (checkbox)
└── Auto-Close
    ├── Auto Close After Selection (checkbox)
    └── Auto Close Delay (seconds)
```

## Debugging

### Context Menu Commands

**HandSelectionManager:**
- Right-click component → "Debug: Log Current Preference"
- Right-click component → "Debug: Clear Saved Preference"

**HandSelectionUI:**
- Right-click component → "Simulate Left Hand Selection"
- Right-click component → "Simulate Right Hand Selection"

**HandSelectionExample:**
- Right-click component → "Example: Simulate Left Hand Action"
- Right-click component → "Example: Simulate Right Hand Action"
- Right-click component → "Example: Log Current Setup"

### Console Logging

All hand selection operations log to console with `[HandSelection]` prefix:
- Preference loading/saving
- Hand changes
- UI interactions

## API Reference

### HandSelectionManager Static Methods

```csharp
HandType HandSelectionManager.GetMainHand()
HandType HandSelectionManager.GetOffHand()
bool HandSelectionManager.CheckIsMainHand(HandType hand)
bool HandSelectionManager.CheckIsOffHand(HandType hand)
```

### HandSelectionManager Instance Methods

```csharp
void SetMainHand(HandType hand)
void SwapHands()
void ResetToDefault()
HandRole GetHandRole(HandType hand)
HandType GetHandForRole(HandRole role)
bool IsMainHand(HandType hand)
bool IsOffHand(HandType hand)
```

### HandPreference Methods

```csharp
HandRole GetHandRole(HandType hand)
HandType GetHandForRole(HandRole role)
bool IsMainHand(HandType hand)
bool IsOffHand(HandType hand)
void SwapHands()
```

## Best Practices

1. **Always use static helpers for simple queries:**
   ```csharp
   if (HandSelectionManager.CheckIsMainHand(hand))
   ```

2. **Subscribe to events for dynamic updates:**
   ```csharp
   HandSelectionManager.Instance.OnMainHandChanged.AddListener(UpdateUI);
   ```

3. **Don't cache hand preferences - always query:**
   ```csharp
   // GOOD
   HandType mainHand = HandSelectionManager.GetMainHand();

   // BAD - Don't cache, player can change preference
   private HandType cachedMainHand; // Don't do this!
   ```

4. **Let UI handle preference changes, don't force in code:**
   ```csharp
   // Let player choose via UI
   // Only set programmatically for special cases (tutorials, etc.)
   ```

## Testing Checklist

- [ ] Hand preference persists between play sessions
- [ ] UI updates when preference changes
- [ ] Main hand triggers physical damage
- [ ] Off hand triggers spirit abilities
- [ ] Swapping hands updates all gameplay elements
- [ ] Default preference is Right hand
- [ ] Events fire correctly on change
- [ ] VR hand tracking respects preference

---

**Created for HAN-11**
*Hand Selection Interface Integration*
