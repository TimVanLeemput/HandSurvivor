# Game Design Document - HandSurvivor

## High Concept
VR survivor game using hands as weapons. Core loop: kill enemies, collect XP, level up, gain powers. Think Soulstone Survivors meets hand tracking.

## POC Scope (10-Minute Loop)
- 10 waves total (1 per minute OR until wave cleared)
- Increasing difficulty per wave
- No meta progression
- Focus: addictive, repeatable dopamine loop

## Core Gameplay Loop
1. Enemy wave spawns
2. Player uses hands to kill enemies
3. Enemies drop XP orbs
4. Player collects XP manually
5. Level up → Choose from 3 active skills
6. Repeat until wave 10 or death

## Hand Combat System

### Main Hand (Physical Attacks)
- **Sweep**: Small enemies - swipe motion
- **Grab & Toss**: Medium enemies - grab, throw
- **Squish**: Magician enemies - pinch with index/thumb

### Off Hand (Active Skills)
Auto-cast on cooldown. 4 active skill slots available.

#### Active Skills
1. **Electric Hand**
   - Hand becomes lightning for X seconds
   - DPS to enemies in contact

2. **Giant Hand**
   - Hand scales up for X seconds
   - Smash large enemies + groups
   - "Fly swatter" gameplay

3. **Laser Finger**
   - Index finger shoots laser for X seconds
   - High damage, precise targeting

4. **Spaceship Spawn**
   - Alien ship spawns above player
   - Player grabs ship
   - Ship's attractor beam follows player aim
   - Ship struggles to escape (X seconds)
   - Released enemies ejected from beam area

## Progression System

### Active Skills Phase
- First 4 level-ups: Choose from 3 active skills
- Fill all 4 active skill slots

### Passive Skills Phase
After 4 actives filled:
- **Cooldown Reduction**: -X% cooldown for specific skill
- **Damage Increase**: +X% damage for specific skill
- **Size/Scale Increase**: Larger AOE (Giant Hand, etc.)

## Enemy Types
- **Small**: Swarm type, weak, swept away
- **Medium**: Grabbable, throwable
- **Magician**: Squishable with pinch gesture

## Wave System
- Wave 1-10
- Difficulty scaling:
  - More enemies per wave
  - Faster/tankier enemies
  - Mixed enemy compositions
