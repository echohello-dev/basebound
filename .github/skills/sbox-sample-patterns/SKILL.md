---
name: sbox-sample-patterns
description: Patterns from Facepunch sample gamemodes (hc1, walker, jumpgame) covering prefab-driven state graphs, additive UI scenes, player prefabs, scene wiring, and standard S&box APIs. Use when building Basebound with reference to real shipped sample structures.
metadata:
  author: basebound
  version: "1.0"
  sources:
    - Facepunch sbox-hc1
    - Facepunch sbox-walker
    - Facepunch sbox-jumpgame
compatibility: S&box (Facepunch), .NET 10, C# 14
---

# S&box Sample Gamemode Patterns

Use this skill when you want to align Basebound’s structure with real Facepunch sample gamemodes (prefabs, scenes, UI, and spawn flow).

## When to Use

- Planning Basebound’s gamemode architecture and scene layout
- Organizing prefabs, UI, and scene wiring for S&box
- Reusing sample-proven spawn/lobby patterns
- Referencing how Facepunch composes rules with state graphs

## Key Patterns From Samples

### 1. Prefab-Driven Gamemode State Graphs (hc1)

- Root `GameMode` object owns a `StateMachineComponent` under a `States` child.
- Each state (`StateComponent`) hosts rule components like respawn, countdown, UI, scoring.
- Mode selection is driven by prefab hierarchy rather than hard-coded logic.

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-hc1\Assets\prefabs\game_modes\deathmatch.prefab`
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-hc1\Code\GameLoop\GameMode.cs`

### 2. Host Startup + Lobby + Spawn (walker, jumpgame)

- Host creates lobby in `ISceneStartup.OnHostInitialize`.
- `Component.INetworkListener.OnActive` spawns a player prefab for each connection.
- Player spawn uses `GameObject.Clone` + `NetworkSpawn(owner)`.

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-walker\code\GameManager.cs`
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-jumpgame\Code\GamePlay\GameManager.cs`

### 3. Additive Engine Scene for UI (walker)

- Host loads a lightweight UI scene additively.
- Engine scene wires `ScreenPanel` and Razor UI components (HUD, chat, scoreboard).

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-walker\Assets\Scenes\engine.scene`

### 4. Player Prefab Composition (walker, jumpgame)

Player prefabs bundle the full pawn stack:

- `PlayerController`, `Rigidbody`, movement modes (`MoveModeWalk`, `MoveModeSwim`, `MoveModeLadder`).
- Gameplay components (inventory, stats, use, custom input).
- Model + animation helpers.

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-walker\Assets\player.prefab`
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-jumpgame\Assets\Prefabs\Player\jumperplayer.prefab`

### 5. Scene-Wired Gameplay Components (jumpgame)

- Trigger volumes + components are placed directly in scenes.
- Gameplay FX are child objects controlled by component parameters.

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-jumpgame\Assets\scenes\jumper.scene`

### 6. Razor + SCSS UI Pairing

- Each UI component has a matching `.razor.scss`.
- UI composition via `PanelComponent` and `BuildHash`.

References:
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-walker\code\UI\Hud.razor`
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-walker\code\UI\Hud.razor.scss`
- `C:\Users\JohnnyHuynh\Projects\github.com\Facepunch\sbox-jumpgame\Code\UI\JumperMenu.razor`

## Practical Guidance for Basebound

1. **Start with prefab wiring**: define your gamemode flow in prefab state graphs first.
2. **Keep spawn logic thin**: let prefabs own pawn setup; `GameManager` just clones/spawns.
3. **Separate UI scene**: load an additive engine/UI scene to keep UI isolated.
4. **Scene-first gameplay**: place triggers and map logic in scenes with attached components.

## Quick Reference

| Pattern | Sample Source | Why it Helps |
| --- | --- | --- |
| State graph in prefab | `sbox-hc1` | Swap modes without code rewrites |
| Additive UI scene | `sbox-walker` | Isolates UI and simplifies startup |
| Player prefab stack | `walker`, `jumpgame` | Spawns are deterministic and data-driven |
| Scene-wired triggers | `jumpgame` | Designers can edit gameplay without code |

## Common Pitfalls

- Avoid putting game rules directly in `GameManager` when a prefab graph can own them.
- Avoid hardcoding UI into gameplay scenes; use a separate engine/UI scene.
- Avoid spawning incomplete pawns; keep all pawn dependencies inside the prefab.
