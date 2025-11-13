# Technical Design Document - HandSurvivor

## Architecture Overview

### Core Systems
1. **Wave Manager**: Spawning, difficulty scaling, wave progression
2. **XP System**: Drop, collection, leveling
3. **Skill System**: Active skills (cooldown-based auto-cast), passive upgrades
4. **Enemy System**: Types, behaviors, health, death
5. **Hand Interaction**: Gesture detection, collision, physics
6. **UI System**: Level-up selection, HUD

## Hand Interaction System

### Main Hand (OVRHand - Right)
- **Gesture Recognition**
  - Sweep: Velocity threshold + palm direction
  - Grab: Pinch strength + proximity to enemy
  - Squish: Thumb-index pinch on magician enemy

- **Physics Interactions**
  - Rigidbody-based collisions
  - Throw velocity from hand tracking
  - Force application on sweep

### Off Hand (OVRHand - Left)
- **Active Skill Casting Point**
  - Skills spawn/activate from left hand position
  - Hand pose can trigger visual effects
  - No manual casting - auto-cast on cooldown

## Skill System Architecture

### Skill Base Class
```
SkillBase
├── SkillType (Active/Passive)
├── Cooldown (float)
├── Duration (float)
├── Level (int)
├── Cast() - virtual
└── Upgrade(PassiveType) - virtual
```

### Active Skills
- **ElectricHandSkill**: Damage over time, hand particle effects
- **GiantHandSkill**: Scale hand transform, increase collision damage
- **LaserFingerSkill**: Raycast from index, line renderer
- **SpaceshipSkill**: Spawn ship prefab, grab mechanics, attractor beam

### Passive Upgrades
- Modify active skill properties:
  - `cooldownMultiplier`
  - `damageMultiplier`
  - `scaleMultiplier`

### Skill Manager
- Tracks 4 active skill slots
- Handles auto-cast timers
- Provides level-up options (3 random skills)

## Enemy System

### Enemy Base Class
```
EnemyBase
├── EnemyType (Small/Medium/Magician)
├── Health
├── MoveSpeed
├── XPValue
├── TakeDamage()
├── Die() - spawn XP
└── MoveTowardsPlayer()
```

### Enemy Types
- **SmallEnemy**: Low HP, swarm AI, sweep-killable
- **MediumEnemy**: Grabbable rigidbody, throwable
- **MagicianEnemy**: Squish-only kill condition

## Wave System

### WaveManager
- Wave definition: `List<EnemySpawnData>`
- Spawn patterns: Circle around player
- Difficulty scaling formula:
  - `enemyCount = baseCount * (1 + wave * 0.2)`
  - `enemyHealth = baseHealth * (1 + wave * 0.15)`

### Wave Progression
- Timer-based (60s) OR clear-based
- Wave complete → brief pause → next wave
- Wave 10 complete → Victory screen

## XP System

### XP Drop
- Enemies drop XP orb on death
- Orb physics: spawn upward, fall to ground
- Orb lifetime: 30s before despawn

### XP Collection
- Manual collection: Hand collider触碰 XP orb
- No auto-vacuum (POC simplicity)

### Level Up
- XP threshold: `100 * level`
- Pause game on level up
- Show 3 random skill options
- Resume after selection

## Performance Considerations

### VR Optimization
- Object pooling for enemies and XP orbs
- LOD for distant enemies
- Particle system limits
- Avoid GC allocations in Update loops

### Target Frame Rate
- Quest 3: 90Hz sustained
- Quest 2: 72Hz minimum

## Meta SDK Integration

### Hand Tracking
- `OVRHand` for gesture data
- `OVRSkeleton` for bone transforms
- `OVRHandPrefab` for visual hands

### Interaction SDK
- `PokeInteractable` for UI buttons
- `HandRef` for hand-specific logic
- Custom collision filters (PokeInteractableHandFilter)

## Project Structure
```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs
│   └── WaveManager.cs
├── Skills/
│   ├── SkillBase.cs
│   ├── ActiveSkills/
│   └── SkillManager.cs
├── Enemies/
│   ├── EnemyBase.cs
│   └── Types/
├── XP/
│   ├── XPOrb.cs
│   └── XPManager.cs
├── Hand/
│   ├── HandGestureDetector.cs
│   └── HandCombat.cs
└── UI/
    ├── LevelUpUI.cs
    └── HUD.cs
```
