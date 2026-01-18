---
name: sbox-sample-patterns
description: Patterns from common S&box sample gamemodes covering prefab-driven state graphs, additive UI scenes, player prefabs, scene wiring, and standard S&box APIs. Use when building Basebound with reference to portable sample structures.
metadata:
  author: basebound
  version: "1.0"
compatibility: S&box (Facepunch), .NET 10, C# 14
---

# S&box Sample Gamemode Patterns

Use this skill to align Basebound’s structure with portable, sample-proven patterns for prefabs, scenes, UI, and spawn flow.

## When to Use

- Planning Basebound’s gamemode architecture and scene layout
- Organizing prefabs, UI, and scene wiring for S&box
- Reusing sample-proven spawn/lobby patterns
- Referencing how to compose rules with prefab state graphs

## Key Patterns From Samples

### 1. Prefab-Driven Gamemode State Graphs

- Root `GameMode` object owns a `StateMachineComponent` under a `States` child.
- Each state (`StateComponent`) hosts rule components like respawn, countdown, UI, scoring.
- Mode selection is driven by prefab hierarchy rather than hard-coded logic.

### 2. Host Startup + Lobby + Spawn

- Host creates lobby in `ISceneStartup.OnHostInitialize`.
- `Component.INetworkListener.OnActive` spawns a player prefab for each connection.
- Player spawn uses `GameObject.Clone` + `NetworkSpawn(owner)`.

### 3. Additive Engine Scene for UI

- Host loads a lightweight UI scene additively.
- Engine scene wires `ScreenPanel` and Razor UI components (HUD, chat, scoreboard).

### 4. Player Prefab Composition

Player prefabs bundle the full pawn stack:

- `PlayerController`, `Rigidbody`, movement modes (`MoveModeWalk`, `MoveModeSwim`, `MoveModeLadder`).
- Gameplay components (inventory, stats, use, custom input).
- Model + animation helpers.

### 5. Scene-Wired Gameplay Components

- Trigger volumes + components are placed directly in scenes.
- Gameplay FX are child objects controlled by component parameters.

### 6. Razor + SCSS UI Pairing

- Each UI component has a matching `.razor.scss`.
- UI composition via `PanelComponent` and `BuildHash`.

## Practical Guidance for Basebound

1. **Start with prefab wiring**: define your gamemode flow in prefab state graphs first.
2. **Keep spawn logic thin**: let prefabs own pawn setup; `GameManager` just clones/spawns.
3. **Separate UI scene**: load an additive engine/UI scene to keep UI isolated.
4. **Scene-first gameplay**: place triggers and map logic in scenes with attached components.

## Quick Reference

| Pattern | Why it Helps |
| --- | --- |
| State graph in prefab | Swap modes without code rewrites |
| Additive UI scene | Isolates UI and simplifies startup |
| Player prefab stack | Spawns are deterministic and data-driven |
| Scene-wired triggers | Designers can edit gameplay without code |

## Common Pitfalls

- Avoid putting game rules directly in `GameManager` when a prefab graph can own them.
- Avoid hardcoding UI into gameplay scenes; use a separate engine/UI scene.
- Avoid spawning incomplete pawns; keep all pawn dependencies inside the prefab.
