# ADR 0001 – Control Plane, Rules, and HUD Projection

**Status**: Proposed  
**Date**: 2026-01-16  
**Owners**: Basebound Gameplay, UI, and Networking teams

## Context

Basebound currently inherits HC1-era code without the architectural discipline that keeps UI, gameplay authority, and networking in sync. HUD code often "hunts" through the scene for data, which breaks during respawn and spectating, and core lifecycle concerns blur across GameMode, pawns, and UI. We need an explicit control-plane pattern so gameplay logic remains modular, UI trust flows from a stable viewer, and spectating/killcams become first-class scenarios instead of bolt-ons.

## Decision

Adopt HC1's four-layer stack, reimagined for Basebound:

1. **GameLoop Control Plane** (`Code/GameLoop/`)
   - `GameMode`, `GameNetworkManager`, and a `StateMachineComponent` own lifecycle, scene bootstrap, join/leave, and authoritative event dispatch.
   - GameMode stays thin—no scoring, inventory, or HUD logic.
2. **Rules Engine** (`Code/GameLoop/Rules/…`)
   - Rules are sealed components registered by GameMode. Each handles a single concern (round timer, elimination scoring, economy ticks) and reacts to strongly typed events (`IGameEvent`).
3. **Global State Surface** (`Code/GameLoop/Globals/…`)
   - Replicated components that hold queryable facts (phase, round timer, team scores, money snapshots). They form the HUD/API boundary; only globals may be read by UI/bridges.
4. **HUD Data Bridge** (`Code/UI/HUD/HudDataBridge.cs` + Razor)
   - A dedicated component samples `PlayerClient.Viewer`, `ViewerPawn`, and globals, exposing HUD-friendly projections (health %, phase text, timer strings). Razor panels (`MainHUD.razor`, `DeveloperMenu.razor`) consume the bridge rather than world objects.

This explicitly reinforces the Client/Pawn split: `PlayerClient.Local` answers "who owns input," `PlayerClient.Viewer` answers "whose perspective is rendered." HUD begins at the viewer, so spectating, death cams, and freecam share the same path.

## Rationale

- **Authority clarity**: Rules mutate globals; HUD only reads projections, preventing "UI authority leaks" and inconsistent displays during replication delay.
- **Spectate-first UI**: By treating `PlayerClient.Viewer` as the HUD root, spectating is the default, reducing bespoke HUD patches when the viewer changes.
- **Modularity**: Each rule is testable and replaceable. GameMode just wires rules and state transitions, keeping it from becoming a God object.
- **Diagnostics**: A narrow set of globals makes debugging simpler (log the globals + viewer, verify lens before diving into UI widgets).

## Implementation Plan

1. **Viewer Correctness Audit**
   - Ensure `PlayerClient.Local`/`PlayerClient.Viewer` swap cleanly across spawn, death, and spectate. Add a debug HUD widget that prints Local, Viewer, ViewerPawn validity, and a couple of global values.
2. **Control Plane Skeleton**
   - Add `GameLoop/GameMode.cs`, `GameNetworkManager.cs`, and `StateMachineComponent.cs` (Warmup → Playing → End). GameMode spawns the state machine plus default rules listed in prefab metadata.
3. **Event Contract**
   - Create `Code/GameLoop/Events/IGameEvent.cs` along with records such as `PlayerSpawnedEvent`, `PlayerEliminatedEvent`, `RoundStateChangedEvent`. Provide a lightweight event bus for rules to subscribe/emit.
4. **Initial Rules**
   - Implement `RoundTimerRule` and `EliminationScoringRule` in `GameLoop/Rules/`. Each listens to events and updates exactly one global component via dependency injection or `[RequireComponent]` references.
5. **Global Truth Holders**
   - Add `MatchPhaseGlobal`, `RoundTimerGlobal`, `TeamScoreGlobal`, `EconomySnapshotGlobal` in `GameLoop/Globals/`. These are server-authoritative, replicated, and contain zero HUD-specific formatting.
6. **HUD Data Bridge**
   - Introduce `HudDataBridge` (likely attached to the `HeadsUpDisplay` prefab) that samples globals plus `PlayerClient.Viewer`. Expose sanitized properties (floats, strings, formatted timers) consumed by Razor.
7. **HUD Refactor**
   - Update `MainHUD.razor`, `DeveloperMenu.razor`, and future panels to call into `HudDataBridge` instead of pawns/rules directly. Document the "HUD-approved" fields in `Docs/architecture.md`.
8. **Spectate Regression + Tooling**
   - Treat spectating as the default QA scenario. Add optional logging in the event bus and keep the debug HUD panel available during development.
9. **Documentation + Prefabs**
   - Reference this ADR from `Docs/architecture.md` and `Docs/gameplay.md` when specific systems (rules, globals) are added. Update prefabs (`Assets/prefabs/game_modes/…`, `heads_up_display.prefab`) to include the new components.

## Consequences

- **Positive**: Unified authority model, simpler HUD flows, clear extension points for new rules, and faster debugging because globals become the narrow query surface.
- **Trade-offs**: Slight upfront cost to build the state machine, event bus, and globals before gameplay features. Requires strict discipline so HUD developers resist "quick" scene lookups.
- **Risks**: If Event Bus and Globals are under-documented, future contributors might bypass them. Mitigate by cross-linking this ADR in onboarding docs and enforcing lint checks or reviewer guidelines.

## Open Questions / Follow-ups

1. Do we need multiple state machines (match vs round) or can a single component drive both with substates?
2. What metrics/logging will we collect from globals for live debugging (eg. developer console commands or UI overlays)?
3. Should `HudDataBridge` live on the HUD prefab or as a singleton service referenced by multiple panels? Prototype and decide before expanding panels.
4. When should we introduce replay/killcam viewers, and how does that interact with prediction? Document this once Viewer swapping is battle-tested.
