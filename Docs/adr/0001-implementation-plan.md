# Implementation Plan: Control Plane & HUD Bridge

**Companion to**: [ADR 0001](0001-control-plane-and-hud-bridge.md)  
**Created**: 2026-01-16

---

## Dependency Graph

```
                    ┌─────────────────┐
                    │ 1. Viewer Audit │ ◄── START HERE (foundation)
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
    │ 2. Control  │  │ 3. Event    │  │ 5. Globals  │
    │    Plane    │  │   Contract  │  │   Skeleton  │
    └──────┬──────┘  └──────┬──────┘  └──────┬──────┘
           │                │                │
           └────────────────┼────────────────┘
                            ▼
                   ┌─────────────────┐
                   │ 6. HUD Bridge   │ ◄── FIRST MILESTONE
                   └────────┬────────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
    │ 4. Initial  │  │ 7. HUD      │  │ 8. Spectate │
    │   Rules     │  │   Refactor  │  │   Testing   │
    └─────────────┘  └─────────────┘  └─────────────┘
```

---

## Phased Approach

### Phase 0: Validate Foundation (1-2 days)

> **Goal**: Prove `Client.Local`/`Client.Viewer` works before building on it

| Task | Deliverable | Success Criteria |
|------|-------------|------------------|
| Debug HUD widget | `DebugViewerPanel.razor` | Shows Local, Viewer, Pawn validity in real-time |
| Manual test | Spawn → die → spectate | Values update correctly through state changes |
| Fix any gaps | Client.cs tweaks | No nulls during transitions |

**Why first**: Everything else assumes Viewer works. If it's broken, you'll build on sand.

---

### Phase 1: Skeleton Layer (3-4 days)

> **Goal**: Empty but connected infrastructure

| Task | Deliverable | Notes |
|------|-------------|-------|
| Event contract | `Code/GameLoop/Events/*.cs` | Just record definitions, no handlers yet |
| Globals stubs | `Code/GameLoop/Globals/*.cs` | Empty components with `[Sync]` properties |
| HUD Bridge shell | `HudDataBridge.cs` | Reads Viewer + one global, exposes to Razor |
| StateMachine placeholder | Single "Playing" state | Real states come later |

**Milestone check**: HUD reads health through bridge → bridge reads from pawn → pawn exists. No direct pawn access from Razor.

---

### Phase 2: First Vertical Slice (3-4 days)

> **Goal**: One complete loop working end-to-end

Pick **one feature** to prove the architecture:

| Option A: Health Display | Option B: Round Timer |
|--------------------------|----------------------|
| Player takes damage | Timer counts down |
| `PlayerDamagedEvent` dispatched | `RoundTimerGlobal` updates |
| `HudDataBridge.HealthPercent` updates | `HudDataBridge.TimerText` updates |
| `Vitals.razor` redraws | `RoundDisplay.razor` redraws |

**Recommendation**: Start with Health—it's simpler and you can test by jumping off things.

---

### Phase 3: Expand Horizontally (1 week)

> **Goal**: Add remaining globals/rules using proven pattern

| System | Global | Rule | HUD Widget |
|--------|--------|------|------------|
| Health/Armor | `PlayerVitalsGlobal` | — | `Vitals.razor` |
| Round phase | `MatchPhaseGlobal` | `RoundTimerRule` | `RoundDisplay.razor` |
| Team scores | `TeamScoreGlobal` | `EliminationScoringRule` | `Scoreboard.razor` |
| Economy | `EconomySnapshotGlobal` | `EconomyTickRule` | `BuyMenu.razor` |

Each follows the same pattern—should get faster as you go.

---

### Phase 4: Spectate Hardening (2-3 days)

> **Goal**: Viewer swapping becomes reliable

| Scenario | Test |
|----------|------|
| Death → spectate teammate | Viewer changes, HUD updates smoothly |
| Spectate → respawn | Viewer returns to Local, HUD resets |
| Spectate enemy (if allowed) | Team color, health still work |
| Freecam | HUD gracefully handles "no pawn" |

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Viewer nulls during transitions | Phase 0 debug widget catches this early |
| HUD developers bypass bridge | Code review checklist, maybe a Roslyn analyzer later |
| Event bus becomes spaghetti | Keep events coarse (round-level, not frame-level) |
| Globals proliferate | Rule: 1 global per domain, not per field |

---

## Parallel Work Opportunities

If you have multiple people:

```
Person A (Gameplay):          Person B (UI):
├─ StateMachine               ├─ Debug widget
├─ Events + Rules             ├─ HUD Bridge
├─ Globals                    ├─ Razor refactor
└─ Spectate logic             └─ Style polish
```

---

## Key Decisions

| Question | Options | Decision |
|----------|---------|----------|
| HUD Bridge location | Singleton vs per-prefab | **Per-prefab** (on `heads_up_display.prefab`) — simpler lifecycle |
| Event bus | Facepunch `libevents` vs custom | **Use libevents** (HC1 uses it, less to build) |
| Single vs nested state machines | One StateMachine with substates vs separate Match/Round | **Start single**, split if it gets complex |
| Global granularity | One big `GameStateGlobal` vs many small | **Many small** — easier to sync/test independently |

---

## First Sprint (1 week)

| Day | Focus | Output |
|-----|-------|--------|
| 1 | Phase 0 | Debug widget working, Viewer validated |
| 2 | Events + Globals stubs | Files created, compiles, does nothing |
| 3 | HUD Bridge v1 | Reads Viewer health, exposes to Razor |
| 4 | Wire MainHUD | Health displays through bridge |
| 5 | Manual QA + fixes | Spawn/die/spectate all work |

---

## File Structure (Target)

```
Code/
├── GameLoop/
│   ├── GameMode.cs
│   ├── GameNetworkManager.cs
│   ├── GameUtils.cs
│   ├── Events/
│   │   ├── IGameEvent.cs
│   │   ├── PlayerEvents.cs        # PlayerSpawnedEvent, PlayerEliminatedEvent
│   │   └── RoundEvents.cs         # RoundStateChangedEvent
│   ├── Globals/
│   │   ├── MatchPhaseGlobal.cs
│   │   ├── RoundTimerGlobal.cs
│   │   ├── TeamScoreGlobal.cs
│   │   └── EconomySnapshotGlobal.cs
│   └── Rules/
│       ├── RoundTimerRule.cs
│       └── EliminationScoringRule.cs
├── PawnSystem/
│   └── Client.cs                  # (existing)
├── UI/
│   ├── HUD/
│   │   ├── HudDataBridge.cs       # NEW: The bridge component
│   │   ├── DebugViewerPanel.razor # NEW: Phase 0 debug widget
│   │   ├── Vitals.razor
│   │   └── ...
│   └── MainHUD.razor
```

---

## Progress Checklist

### Phase 0
- [ ] Create `DebugViewerPanel.razor`
- [ ] Verify `Client.Local` assigned on spawn
- [ ] Verify `Client.Viewer` swaps on spectate
- [ ] Fix any null issues in Client.cs

### Phase 1
- [ ] Create `Code/GameLoop/Events/` folder
- [ ] Define `PlayerSpawnedEvent`, `PlayerEliminatedEvent`
- [ ] Create `Code/GameLoop/Globals/` folder
- [ ] Stub `MatchPhaseGlobal.cs`
- [ ] Create `HudDataBridge.cs` shell
- [ ] Wire bridge to `heads_up_display.prefab`

### Phase 2
- [ ] Implement health reading through bridge
- [ ] Update `MainHUD.razor` to use bridge
- [ ] Test damage → HUD update flow
- [ ] Remove direct pawn access from Razor

### Phase 3
- [ ] Add `RoundTimerGlobal` + `RoundTimerRule`
- [ ] Add `TeamScoreGlobal` + `EliminationScoringRule`
- [ ] Add `EconomySnapshotGlobal`
- [ ] Wire remaining HUD widgets

### Phase 4
- [ ] Test death → spectate flow
- [ ] Test spectate → respawn flow
- [ ] Test freecam (no pawn) gracefully
- [ ] Document edge cases found

---

## References

- [ADR 0001](0001-control-plane-and-hud-bridge.md) — The decision document
- [HC1 GameMode.cs](../../sbox-hc1/Code/GameLoop/GameMode.cs) — Reference implementation
- [HC1 Client.cs](../../sbox-hc1/Code/PawnSystem/Client.cs) — Viewer pattern reference
- [HC1 Events.cs](../../sbox-hc1/Code/GameLoop/Rules/Events.cs) — Event definitions
