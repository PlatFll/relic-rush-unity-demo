# Relic Rush

A small playable Unity prototype for an **endless hidden-object roguelite** designed around short mobile-friendly runs, escalating observation challenges, run upgrades, score chasing, and consumable-style powerups.

## Unity version

**Unity 6.3 LTS — 6000.3.21f1**

The project intentionally targets the LTS line rather than the newest Update release so the prototype starts on a stable production-oriented version.

## Open and play

1. Clone/download the repository.
2. In Unity Hub, install **Unity 6000.3.21f1** if you do not already have it.
3. Add this repository folder as a project.
4. Open `Assets/Scenes/Demo.unity`.
5. Press **Play**.

The entire game UI is assembled at runtime by `RelicRushBootstrap`, so the Demo scene is deliberately empty.

## Current demo loop

- Start a run from the main menu.
- Search one of two handcrafted pixel-art rooms.
- Find the randomly selected target objects before time expires.
- Fast consecutive finds build a score combo.
- Wrong taps break combo and subtract time.
- Each room also contains an unlisted **Bonus Relic** worth extra score and +2 seconds.
- Every two cleared rooms, choose one of three roguelite upgrades.
- As the run continues, rounds gain modifiers:
  - cryptic clue targets
  - darkness / spotlight searching
  - horizontally mirrored rooms
  - increasingly tight time limits and larger target lists
- Lose when the timer reaches zero.
- The local high score persists with `PlayerPrefs`.

## Prototype powerups

The bottom HUD contains two consumable-style powerups to demonstrate where mobile monetization could fit without changing the core game:

- **Hint** — flashes one remaining target.
- **Time Crystal** — adds 8 seconds.

They are free limited charges in this prototype. There is **no store, ad SDK, IAP, analytics SDK, or real-money purchase implementation**.

## Run upgrades

The current upgrade pool includes:

- Pocket Watch — more starting time every room.
- Detective Lens — additional/better hints.
- Gilded Combo Chain — stronger combo scoring.
- Mistake Ward — protects the first wrong tap each room.
- Crystal Satchel — more time-crystal uses.
- Hunter's Focus — slows target-list growth.

## Art

The playable rooms and UI palette were created specifically for this prototype as pixel-art assets. `Docs/Concept/relic_rush_ai_moodboard.png` is an additional generated high-detail visual direction reference for future production art.

The source art generator (`generate_art.py`) is kept in the repo so the current room artwork is reproducible and easy to iterate on.

## Controls

### Desktop / Editor
- Left click objects to search.
- Move the mouse to move the spotlight during darkness rounds.

### Mobile
- Tap objects.
- During darkness rounds, the spotlight follows the active touch.
- The project forces landscape orientation at runtime.

## Project structure

```text
Assets/
  Resources/Art/       Pixel-art room backgrounds
  Scenes/Demo.unity    Empty bootstrap scene
  Scripts/             Runtime game + bootstrap code
  Shaders/             Darkness spotlight UI shader
Docs/
  Concept/             Generated production-art moodboard
  hotspots.json        Source hotspot coordinates
generate_art.py        Reproducible pixel-art generator
```

## Prototype scope

This is intentionally a **vertical slice**, not a production game. The objective is to answer whether the basic combination of hidden-object searching + escalating endless runs + roguelite build choices feels promising before investing in many more rooms, progression systems, monetization infrastructure, or final art.
