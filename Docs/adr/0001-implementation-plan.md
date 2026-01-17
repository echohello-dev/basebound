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
| 2 | Fix Client statics | `PlayerClient.cs` updates | No nulls during spawn/die |
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

## Complete Vertical Slice Roadmap

### Tier 1: Core HUD Foundation (Weeks 1-3)
*Must have for any playable build*

| # | Slice | Dependencies | Key Files | Est. Days |
|---|-------|--------------|-----------|-----------|
| 1 | **Health Display** | None | `HudDataBridge.cs`, `Vitals.razor` | 5 |
| 2 | **Ammo Display** | Slice 1, Weapons | `AmmoDisplay.razor` | 3 |
| 3 | **Round Timer** | Slice 1 | `RoundTimerGlobal.cs`, `RoundTimer.razor` | 4 |
| 4 | **Kill Events** | Slice 1 | `PlayerEvents.cs`, `KillFeed.razor` | 3 |
| 5 | **Spectate System** | Slice 1, 4 | `SpectateSystem.cs`, `Spectating.razor` | 3 |

---

### Tier 2: Game Loop & Economy (Weeks 4-6)
*Required for a competitive mode*

| # | Slice | Dependencies | Key Files | Est. Days |
|---|-------|--------------|-----------|-----------|
| 6 | **Round State Machine** | Slice 3 | `StateMachineComponent`, state prefabs | 4 |
| 7 | **Team Assignment** | Slice 6 | `TeamAssignerRule.cs`, `TeamSelectMenu.razor` | 3 |
| 8 | **Economy System** | Slice 6, 7 | `EconomyGlobal.cs`, `Balance.razor` | 4 |
| 9 | **Buy Menu** | Slice 8 | `BuyZone.cs`, `BuyMenu.razor` | 5 |
| 10 | **Scoreboard** | Slice 7 | `Scoreboard.razor`, `ScoreboardRow.razor` | 3 |

---

### Tier 3: Weapons & Combat (Weeks 7-9)
*Core shooting experience*

| # | Slice | Dependencies | Key Files | Est. Days |
|---|-------|--------------|-----------|-----------|
| 11 | **Equipment Resource** | None | `EquipmentResource.cs`, `.equip` files | 3 |
| 12 | **Shootable Component** | Slice 11 | `Shootable.cs`, hit detection | 4 |
| 13 | **Reloadable Component** | Slice 11, 12 | `Reloadable.cs` | 2 |
| 14 | **Aimable Component** | Slice 11 | `Aimable.cs`, ADS camera | 3 |
| 15 | **View Model** | Slice 11-14 | `ViewModel.cs`, animations | 4 |
| 16 | **Dropped Weapons** | Slice 11 | `DroppedWeapon.cs`, pickup | 3 |

---

### Tier 4: Player Feedback (Weeks 10-11)
*Juice and feel*

| # | Slice | Dependencies | Key Files | Est. Days |
|---|-------|--------------|-----------|-----------|
| 17 | **Crosshair** | Slice 12 | `Crosshair.razor`, dynamic spread | 2 |
| 18 | **Hit Markers** | Slice 4, 12 | `HitMarker.razor`, sounds | 2 |
| 19 | **Damage Indicators** | Slice 4 | `DamageIndicator.razor`, directional | 3 |
| 20 | **Death Cam** | Slice 5 | `Deathcam.razor`, killer focus | 3 |
| 21 | **Toast System** | Slice 4 | `Toast.razor`, notifications | 2 |

---

### Tier 5: Advanced Systems (Weeks 12-14)
*Polish and depth*

| # | Slice | Dependencies | Key Files | Est. Days |
|---|-------|--------------|-----------|-----------|
| 22 | **Minimap** | Slice 7 | `Minimap.razor`, player dots | 5 |
| 23 | **Voice Chat UI** | None | `Voices.razor`, speaking indicators | 2 |
| 24 | **Chat System** | None | `Chat.razor`, `ChatBox.cs` | 3 |
| 25 | **Bot Manager** | Slice 6, 12 | `BotManager.cs`, `BotController.cs` | 5 |
| 26 | **Ragdolls** | Slice 4 | `PlayerRagdollBehavior.cs` | 2 |

---

### Tier 6: Mode-Specific Features (Weeks 15+)
*Depends on your game mode*

| # | Slice | Mode | Key Files | Est. Days |
|---|-------|------|-----------|-----------|
| 27 | **Bomb Defusal** | Defuse | `BombSite.cs`, `C4.cs`, `Defuse.razor` | 7 |
| 28 | **Capture Points** | Control | `CaptureZone.cs`, `CaptureUI.razor` | 5 |
| 29 | **Cash Grab** | Cash | `CashPickup.cs`, `CashGrabRule.cs` | 4 |
| 30 | **Map Voting** | All | `MapVoteSystem.cs`, `MapVote.razor` | 4 |

---

## Slice Dependency Graph

```
                           ┌──────────────────┐
                           │  1. Health       │ ◄── START
                           └────────┬─────────┘
                                    │
              ┌─────────────────────┼─────────────────────┐
              ▼                     ▼                     ▼
     ┌────────────────┐    ┌────────────────┐    ┌────────────────┐
     │  2. Ammo       │    │  3. Timer      │    │  4. Events     │
     └────────────────┘    └───────┬────────┘    └───────┬────────┘
                                   │                     │
                                   ▼                     ▼
                          ┌────────────────┐    ┌────────────────┐
                          │  6. State      │    │  5. Spectate   │
                          │    Machine     │    └────────────────┘
                          └───────┬────────┘
                                  │
              ┌───────────────────┼───────────────────┐
              ▼                   ▼                   ▼
     ┌────────────────┐  ┌────────────────┐  ┌────────────────┐
     │  7. Teams      │  │  8. Economy    │  │  25. Bots      │
     └───────┬────────┘  └───────┬────────┘  └────────────────┘
             │                   │
             ▼                   ▼
    ┌────────────────┐  ┌────────────────┐
    │  10. Scoreboard│  │  9. Buy Menu   │
    └────────────────┘  └────────────────┘

         WEAPONS TRACK (parallel)
    ┌────────────────┐
    │  11. Resource  │
    └───────┬────────┘
            │
    ┌───────┴───────┬───────────────┐
    ▼               ▼               ▼
┌────────┐   ┌────────────┐   ┌──────────┐
│12.Shoot│   │ 13. Reload │   │ 14. Aim  │
└───┬────┘   └────────────┘   └──────────┘
    │
    ▼
┌────────────────┐
│ 17. Crosshair  │
│ 18. Hit Marker │
└────────────────┘
```

---

## Minimum Viable Gamemode

To have a **playable deathmatch**, complete these slices:

| Priority | Slices | Result |
|----------|--------|--------|
| **P0** | 1, 2, 3, 4, 5 | HUD works, can see health/ammo/timer/kills |
| **P1** | 6, 11, 12, 13 | Round loop, guns that shoot |
| **P2** | 7, 10, 17 | Teams, scoreboard, crosshair |
| **P3** | Everything else | Polish |

**P0 + P1 = ~25 days** to a playable prototype.

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
| 2 | Fix Client statics | `PlayerClient.Local`/`PlayerClient.Viewer` work through spawn/die | ☐ |
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
│   └── PlayerClient.cs                  # (existing)
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
- [ ] Verify `PlayerClient.Local` assigned on spawn
- [ ] Verify `PlayerClient.Viewer` swaps on spectate
- [ ] Fix any null issues in PlayerClient.cs
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

### Slice 5: Spectate System (Week 3)
- [ ] Create `SpectateSystem.cs` singleton
- [ ] Create `Spectating.razor` (shows who you're watching)
- [ ] Test: death → spectate teammate
- [ ] Test: spectate → respawn
- [ ] Test: freecam (no pawn)

### Slice 6: Round State Machine (Week 4)
- [ ] Add `StateMachineComponent` to game mode prefab
- [ ] Create Waiting/Preparing/Playing/End states
- [ ] Create `MatchPhaseGlobal.cs`
- [ ] Wire `RoundStateDisplay.razor`
- [ ] Test: state transitions work

### Slice 7: Team Assignment (Week 4)
- [ ] Create `TeamAssignerRule.cs`
- [ ] Create `TeamSelectMenu.razor`
- [ ] Add team color to HUD elements
- [ ] Test: join → assigned team → spawn on team side

### Slice 8: Economy System (Week 5)
- [ ] Create `EconomyGlobal.cs`
- [ ] Create `EconomyRule.cs` (money on kill, round win)
- [ ] Create `Balance.razor` (money display)
- [ ] Test: kill → earn money → see balance

### Slice 9: Buy Menu (Week 5)
- [ ] Create `BuyZone.cs` trigger
- [ ] Create `BuyMenu.razor`
- [ ] Create `BuyMenuItem.cs`
- [ ] Test: enter zone → open menu → buy weapon → money deducted

### Slice 10: Scoreboard (Week 5)
- [ ] Create `Scoreboard.razor`
- [ ] Create `ScoreboardRow.razor`
- [ ] Show all players, teams, K/D/A
- [ ] Test: Tab → scoreboard → updates live

### Slice 11-16: Weapons Track (Weeks 6-7)
- [ ] Create `EquipmentResource.cs` base
- [ ] Create first `.equip` file (pistol)
- [ ] Implement `Shootable.cs`
- [ ] Implement `Reloadable.cs`
- [ ] Implement `Aimable.cs`
- [ ] Create `ViewModel.cs`
- [ ] Create `DroppedWeapon.cs`
- [ ] Test: shoot → hit → damage → reload

### Slice 17-21: Player Feedback (Week 8)
- [ ] Create `Crosshair.razor` with dynamic spread
- [ ] Create `HitMarker.razor`
- [ ] Create `DamageIndicator.razor` (directional)
- [ ] Create `Deathcam.razor`
- [ ] Create `Toast.razor` notification system

### Slice 22-26: Advanced Systems (Weeks 9-10)
- [ ] Create `Minimap.razor` with player dots
- [ ] Create `Voices.razor` speaking indicators
- [ ] Create `Chat.razor` + `ChatBox.cs`
- [ ] Create `BotManager.cs` + basic bot AI
- [ ] Create `PlayerRagdollBehavior.cs`

### Slice 5: Spectate Hardening
- [ ] Test death → spectate teammate flow
- [ ] Test spectate → respawn flow
- [ ] Test freecam (no pawn) gracefully
- [ ] Document edge cases found

---

## References

- [ADR 0001](0001-control-plane-and-hud-bridge.md) — The decision document
- [HC1 GameMode.cs](../../sbox-hc1/Code/GameLoop/GameMode.cs) — Reference implementation
- [HC1 PlayerClient.cs](../../sbox-hc1/Code/PawnSystem/PlayerClient.cs) — Viewer pattern reference
- [HC1 Events.cs](../../sbox-hc1/Code/GameLoop/Rules/Events.cs) — Event definitions
