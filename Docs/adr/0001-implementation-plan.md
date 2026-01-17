# Implementation Plan: Control Plane & HUD Bridge

**Companion to**: [ADR 0001](0001-control-plane-and-hud-bridge.md)  
**Created**: 2026-01-16  
**Delivery Approach**: Vertical slice (Option C) — build one feature end-to-end, then replicate.

---

## Purpose

- Establish a repeatable control plane and HUD bridge pattern that keeps UI in sync with authoritative gameplay state.
- Produce a working reference slice that demonstrates the architecture before scaling to additional telemetry.

## Guiding Principles

- Prove the architecture with vertical slices that ship observable player value.
- HUD panels read from bridge projections only; no direct scene hunting.
- Keep each slice small enough for rapid validation and hot reload feedback.

## Scope

**In scope**

- Control plane primitives (events, globals, rules) required to feed the HUD.
- HUD bridge component and Razor panels for health, ammo, timer, and combat feedback.
- Developer tooling that exposes viewer state and bridge outputs for debugging.

**Out of scope**

- Viewer-swapping flows (deferred to future UX work).
- Economy balancing, weapon feel, and other systemic tuning beyond telemetry needs.
- Editor tooling beyond what is required for the HUD bridge slices.

## Success Criteria

- [ ] Health telemetry updates live when the player takes damage.
- [ ] HUD recovers gracefully after death and respawn.
- [ ] Ammo counts and round timers surface via the bridge without scene lookups.
- [ ] Combat events drive UI feedback without direct references to gameplay objects.

## Stakeholders & Communication

- **Gameplay Systems** – rules, globals, damage flows.
- **UI / HUD** – bridge consumer panels and visual polish.
- **Networking** – validation that synced data stays authoritative.
- Weekly sync during implementation; async updates through the shared checklist.

## Workstreams & Checklists

### Workstream 1 – HUD Foundation (Slice 1)

**Goal**: Deliver the first vertical slice (health display) to validate the bridge pattern.  
**Dependencies**: None  
**Status**: In progress

**Checklist**

- [x] Add `DebugViewerPanel.razor` for viewer diagnostics (`Code/UI/DebugViewerPanel.razor`).
- [x] Stabilize `PlayerClient.Local` and `PlayerClient.Viewer` assignment across spawn/die (`Code/Player/PlayerClient.cs`).
- [x] Implement `HudDataBridge` with health projections (`Code/UI/HudDataBridge.cs`).
- [x] Attach the bridge to `heads_up_display.prefab` (`Assets/prefabs/heads_up_display.prefab`).
- [x] Build `Vitals.razor` health bar widget (`Code/UI/Vitals.razor`).
- [x] QA: Apply fall or damage events and confirm HUD health updates.
- [x] QA: Die and respawn to ensure health resets to 100%.

**Exit criteria**

- Bridge exposes accurate health metrics for the current viewer.
- HUD displays the health bar without null reference issues during gameplay loops.

### Workstream 2 – Ammo Telemetry (Slice 2)

**Goal**: Extend the bridge and HUD to expose ammunition data.  
**Dependencies**: Workstream 1  
**Status**: Not started

**Checklist**

- [x] Surface current, max, and reserve ammo through `HudDataBridge`.
- [x] Expose ammo data from equipped weapon components (temp source: `WeaponComponent` on pawn).
- [x] Create `AmmoDisplay.razor` panel.
- [x] Document shoot/reload keybinds for QA (default: `Attack1` = Mouse1, `Reload` = R; see `ProjectSettings/Input.config`).
- [ ] QA: Fire and reload to validate ammo updates.

**QA inputs (defaults)**

- `Attack1` → Mouse1 (primary fire)
- `Reload` → R
- Full list: `ProjectSettings/Input.config`

**Exit criteria**

- HUD reflects ammunition changes immediately after shots and reloads.

### Workstream 3 – Combat Events (Slice 3)

**Goal**: Deliver event-driven HUD feedback for kills and damage.  
**Dependencies**: Workstreams 1–2  
**Status**: Not started

**Checklist**

- [ ] Define `PlayerDamagedEvent` and `PlayerKilledEvent` records.
- [ ] Raise events from `PlayerState` damage and death paths.
- [ ] Subscribe `KillFeed.razor` to kill events.
- [ ] Add HUD feedback for damage (flash or indicator).

**Exit criteria**

- Combat events appear in HUD without polling gameplay state.

### Workstream 4 – Control Plane Hardening

**Goal**: Confirm control plane data remains stable during lifecycle transitions.  
**Dependencies**: Workstreams 1–3  
**Status**: Not started

**Checklist**

- [ ] Exercise death-to-respawn flow to ensure viewer references stay valid.
- [ ] Handle pre-spawn/no-pawn cases gracefully in the bridge.
- [ ] Document edge cases and mitigations in `Docs/architecture.md`.
- [ ] Keep the debug widget wired for regression testing.

**Exit criteria**

- Control plane surfaces consistent data before, during, and after player lifecycle changes.

## Milestones

- **M1 – HUD Foundation Verified**: Workstream 1 complete.
- **M2 – Combat Telemetry Online**: Workstreams 1–2 complete.
- **M3 – Event-Driven HUD Feedback**: Workstreams 1–3 complete.
- **M4 – Control Plane Stable**: Workstreams 1–4 complete.

## Dependency Overview

- Workstream 2 depends on health slice patterns for binding to the bridge.
- Workstream 3 leverages events emitted from `PlayerState`.
- Workstream 4 consolidates lifecycle validation after telemetry slices are proven.

## Risks & Mitigations

- **Viewer nulls during transitions** – Mitigate with debug widget and lifecycle QA (Workstream 5).
- **HUD developers bypass bridge** – Enforce code review checklist; consider analyzer in follow-up.
- **Event bus complexity** – Keep events coarse; evaluate third-party bus once slices demand it.
- **Global proliferation** – Limit to one global per gameplay domain and document consumers.

## Parallel Work Opportunities

- **Gameplay**: Implement rules, globals, and damage plumbing.
- **UI**: Iterate on bridge, panels, and debug instrumentation.
- Coordinate through the shared checklists to avoid conflicts.

## Minimum Viable Gamemode Backlog

- **P0** – Complete Workstreams 1–3 for a functional HUD.
- **P1** – Add game state progression, team assignment, and scoreboard integration.
- **P2** – Layer weapon components and feedback (crosshair, hit markers).
- **P3** – Extend into advanced systems (minimap, chat, bots) once the HUD foundation proves stable.

## References

- [ADR 0001](0001-control-plane-and-hud-bridge.md)
- [HC1 GameMode.cs](../../sbox-hc1/Code/GameLoop/GameMode.cs)
- [HC1 PlayerClient.cs](../../sbox-hc1/Code/PawnSystem/PlayerClient.cs)
- [HC1 Events.cs](../../sbox-hc1/Code/GameLoop/Rules/Events.cs)
