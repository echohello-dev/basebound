# Implementation Plan: Control Plane & HUD Bridge

**Companion to**: [ADR 0001](0001-control-plane-and-hud-bridge.md)  
**Created**: 2026-01-16  
**Approach**: **Vertical Slice** (Option C) — Build one complete feature stack, then replicate

---

## Strategy: Vertical Slice

Instead of building infrastructure first (top-down) or wiring HUD ad-hoc (bottom-up), we build **ONE complete feature end-to-end** to prove the architecture, then replicate the pattern.

### Why This Approach

1. **Immediate proof** — See if the architecture works before committing
2. **Reference implementation** — First slice becomes template to copy
3. **Visible progress** — Health bar on screen by day 3
4. **Catch issues early** — Viewer bugs surface during first slice, not later

### The First Slice: Health Display

```
┌─────────────────┐
│ Player takes    │
│ fall damage     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ HealthComponent │  ← S&box built-in or custom
│ .Health -= 10   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ HudDataBridge   │  ← YOU BUILD THIS
│ reads Viewer's  │
│ pawn health     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Vitals.razor    │  ← YOU BUILD THIS
│ displays bar    │
└─────────────────┘
```

**Day 3 deliverable**: Health bar updates when you jump off a ledge.

---

## Phased Approach (Slice-First)

### Slice 1: Health Display (Days 1-5)

> **Goal**: One working vertical slice proving the entire data flow

| Day | Focus | Deliverable | Success Test |
|-----|-------|-------------|--------------|
| 1 | Debug widget | `DebugViewerPanel.razor` | Shows Local/Viewer/Pawn validity |
| 2 | Fix Client statics | `Client.cs` updates | No nulls during spawn/die |
| 3 | HUD Bridge | `HudDataBridge.cs` | Exposes `HealthPercent` property |
| 4 | Vitals widget | `Vitals.razor` | Health bar renders |
| 5 | Death/respawn QA | Manual testing | Bar resets on respawn, follows spectate |

**Milestone**: Jump off ledge → health bar shrinks → die → spectate shows teammate's health.

---

### Slice 2: Ammo Display (Days 6-8)

> **Goal**: Prove the pattern replicates

| Day | Focus | Deliverable |
|-----|-------|-------------|
| 6 | Extend bridge | Add `CurrentAmmo`, `MaxAmmo` to bridge |
| 7 | Ammo widget | `AmmoDisplay.razor` |
| 8 | Integration | Shoot → ammo decrements → reload → refills |

**Pattern validated**: If this goes faster than Slice 1, the architecture works.

---

### Slice 3: Round Timer (Days 9-12)

> **Goal**: Add first global + rule

| Day | Focus | Deliverable |
|-----|-------|-------------|
| 9 | Timer global | `RoundTimerGlobal.cs` with `[Sync]` |
| 10 | Timer rule | `RoundTimerRule.cs` ticks the global |
| 11 | Bridge extension | Add `TimerText` to bridge |
| 12 | Timer widget | `RoundTimer.razor` displays countdown |

**New pattern introduced**: Global → Rule → Bridge → Widget

---

### Slice 4: Game Events (Days 13-15)

> **Goal**: Add event system for reactive UI

| Day | Focus | Deliverable |
|-----|-------|-------------|
| 13 | Event records | `PlayerDamagedEvent`, `PlayerKilledEvent` |
| 14 | Kill feed | `KillFeed.razor` subscribes to kills |
| 15 | Damage flash | HUD reacts to `PlayerDamagedEvent` |

---

### Slice 5: Spectate Hardening (Days 16-18)

> **Goal**: Viewer swapping is bulletproof

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
| Implementation approach | Top-down / Bottom-up / Vertical Slice | **Vertical Slice** — prove pattern with health, then replicate |

---

## Week 1 Sprint (Health Slice)

| Day | Focus | Deliverable | Done? |
|-----|-------|-------------|-------|
| 1 | Debug widget | `DebugViewerPanel.razor` showing Local/Viewer/Pawn | ☐ |
| 2 | Fix Client statics | `Client.Local`/`Client.Viewer` work through spawn/die | ☐ |
| 3 | HUD Bridge v1 | `HudDataBridge.cs` exposes `HealthPercent` | ☐ |
| 4 | Vitals widget | `Vitals.razor` shows health bar | ☐ |
| 5 | QA + spectate | Health updates on damage, follows viewer on death | ☐ |

**Week 1 Success Criteria**: 
- Jump off ledge → health bar shrinks
- Die → spectate teammate → see THEIR health
- Respawn → health resets to 100%

---

## Week 2 Sprint (Ammo + Timer)

| Day | Focus | Deliverable | Done? |
|-----|-------|-------------|-------|
| 6 | Ammo bridge | Add `CurrentAmmo`, `MaxAmmo` to bridge | ☐ |
| 7 | Ammo widget | `AmmoDisplay.razor` | ☐ |
| 8 | Timer global | `RoundTimerGlobal.cs` | ☐ |
| 9 | Timer rule | `RoundTimerRule.cs` | ☐ |
| 10 | Timer widget | `RoundTimer.razor` | ☐ |

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

### Slice 1: Health Display (Week 1)
- [ ] Create `DebugViewerPanel.razor`
- [ ] Verify `Client.Local` assigned on spawn
- [ ] Verify `Client.Viewer` swaps on spectate
- [ ] Fix any null issues in Client.cs
- [ ] Create `HudDataBridge.cs` with `HealthPercent`
- [ ] Wire bridge to `heads_up_display.prefab`
- [ ] Create `Vitals.razor` health bar
- [ ] Test: damage → HUD updates
- [ ] Test: death → spectate → HUD follows viewer
- [ ] Test: respawn → health resets

### Slice 2: Ammo Display (Week 2, Days 1-3)
- [ ] Add `CurrentAmmo`, `MaxAmmo` to `HudDataBridge`
- [ ] Create `AmmoDisplay.razor`
- [ ] Test: shoot → ammo decrements
- [ ] Test: reload → ammo refills

### Slice 3: Round Timer (Week 2, Days 4-5)
- [ ] Create `Code/GameLoop/Globals/RoundTimerGlobal.cs`
- [ ] Create `Code/GameLoop/Rules/RoundTimerRule.cs`
- [ ] Add `TimerText` to `HudDataBridge`
- [ ] Create `RoundTimer.razor`
- [ ] Test: timer counts down

### Slice 4: Game Events (Week 3)
- [ ] Create `Code/GameLoop/Events/PlayerEvents.cs`
- [ ] Implement `PlayerDamagedEvent`, `PlayerKilledEvent`
- [ ] Create `KillFeed.razor` subscribing to kills
- [ ] Add damage flash effect to HUD

### Slice 5: Spectate Hardening
- [ ] Test death → spectate teammate flow
- [ ] Test spectate → respawn flow
- [ ] Test freecam (no pawn) gracefully
- [ ] Document edge cases found

---

## References

- [ADR 0001](0001-control-plane-and-hud-bridge.md) — The decision document
- [HC1 GameMode.cs](../../sbox-hc1/Code/GameLoop/GameMode.cs) — Reference implementation
- [HC1 Client.cs](../../sbox-hc1/Code/PawnSystem/Client.cs) — Viewer pattern reference
- [HC1 Events.cs](../../sbox-hc1/Code/GameLoop/Rules/Events.cs) — Event definitions
