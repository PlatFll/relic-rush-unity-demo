# Relic Rush

**Relic Rush** is a playable Unity prototype for an endless hidden-object roguelite designed around short mobile-friendly runs, escalating observation challenges, and score chasing.

## Unity version

**Unity 6.3 LTS — 6000.3.21f1**

## Open and play

1. Clone this repository.
2. In Unity Hub, install Unity **6000.3.21f1** if needed.
3. Add the cloned folder as a Unity project.
4. Open `Assets/Scenes/Demo.unity`.
5. Press **Play**.

The demo scene itself is intentionally minimal. `RelicRushBootstrap` creates the playable game at runtime.

## Current playable demo

- Two procedurally generated pixel-art environment variants:
  - Alchemist Workshop
  - Smuggler's Tavern
- Sixteen searchable hidden objects.
- Random target lists each room.
- Endless room progression until the timer reaches zero.
- Target count and clock pressure increase as the run continues.
- Fast-find combo scoring.
- Wrong-tap time penalties.
- Cryptic clue rounds.
- Darkness rounds.
- Limited-use Hint powerup.
- Persistent local high score using `PlayerPrefs`.
- Unity UI input suitable for mouse or touch.

## Art approach

The GitHub version is self-contained and does not depend on external texture files. The demo generates its pixel-art rooms and props at runtime, so a clone does not have missing art dependencies.

## Monetization note

The Hint system demonstrates one possible consumable-style powerup. This prototype contains **no store, ads, analytics SDK, IAP SDK, or real-money purchase code**.

## Main project files

```text
Assets/
  Scenes/Demo.unity
  Scripts/RelicRushBootstrap.cs
  Scripts/RelicRushGameCompact.cs
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
```

## Scope

This is a vertical-slice prototype intended to test whether hidden-object searching plus endless escalation makes a fun repeatable mobile loop before investing in production art, more modifiers, upgrade systems, daily challenges, leaderboards, meta progression, or monetization infrastructure.
