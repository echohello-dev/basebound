---
name: sbox-gamemode-dev
description: Beginner-friendly S&box gamemode development covering architecture patterns, Client/Pawn split, event-driven rules, component-based weapons, UI, and bots. Use when learning S&box from scratch, building multiplayer gamemodes, understanding event-driven architecture, or needing guidance on scalable FPS game structure.
metadata:
  author: basebound
  version: "2.0"
---

# S&box Gamemode Development for Beginners

This skill teaches you how to build scalable, maintainable gamemodes in S&box using proven architectural patterns. The approach emphasizes **data + composable behaviors** over sprawling inheritance hierarchies.

## When to Use This Skill

- Learning S&box game development from scratch
- Building a new gamemode (deathmatch, bomb defusal, etc.)
- Understanding multiplayer architecture patterns
- Implementing event-driven game rules
- Creating component-based weapons systems
- Building bot AI with behaviour trees
- Debugging networking and state issues

## Mental Model: How a Gamemode Works

A well-structured gamemode behaves like a layered control system:

| Layer | Responsibility | Key Pattern |
|-------|----------------|-------------|
| **GameLoop** | What phase the game is in | State machine |
| **Rules** | React to events, push state forward | Event-driven composition |
| **PawnSystem** | Player identity vs player body | Client/Pawn split |
| **Weapons** | Equipment behavior | Data + components |
| **HUD** | Visual feedback | Razor components |
| **Bots** | AI decision-making | Behaviour trees |

> **Key insight**: Feature growth doesn't mean complexity growth. New features become "more rules/components/nodes", not more special-case code inside a monolith.

## Learning Ladder (Recommended Order)

Build skills in this order for shortest feedback loops:

| Level | Focus | What You Learn |
|-------|-------|----------------|
| 1 | HUD & UI | Frame-to-frame updates, reading state |
| 2 | Game Events | Event-driven architecture, state notifications |
| 3 | Weapons | Data resources, component composition |
| 4 | World Interactions | Triggers, damage, physics |
| 5 | Game Rules | Scoring, state machine manipulation |
| 6 | Bot AI | Behaviour trees, input simulation |
| 7 | Full Mode | Orchestrating multiple systems |

## Recommended Learning Approach: Vertical Slices

Instead of building layers (infrastructure → rules → UI), build **complete vertical slices**:

### What is a Vertical Slice?

A vertical slice touches every layer for ONE feature:

```
┌─────────────────┐
│ Player takes    │  ← TRIGGER
│ fall damage     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ HealthComponent │  ← DATA LAYER
│ .Health -= 10   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ HudDataBridge   │  ← BRIDGE LAYER
│ reads health    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Vitals.razor    │  ← UI LAYER
│ displays bar    │
└─────────────────┘
```

### Why Vertical Slices Work Better

| Approach | Problem |
|----------|---------|
| **Top-down** (build infra first) | Lots of empty files before anything works |
| **Bottom-up** (wire UI ad-hoc) | May build wrong abstractions early |
| **Vertical slice** | Proves architecture end-to-end immediately |

### Suggested Slice Order

| Slice | Feature | What You Learn |
|-------|---------|----------------|
| 1 | **Health display** | Client/Viewer split, HUD data binding |
| 2 | **Ammo display** | Extending the bridge pattern |
| 3 | **Round timer** | Globals + Rules + Bridge |
| 4 | **Kill feed** | Event subscription in UI |
| 5 | **Scoreboard** | Reading multiple clients' data |

> **Key insight**: The first slice takes longest. Each subsequent slice reuses the pattern and goes faster.

## Core Architecture Patterns

### 1. GameMode as Orchestrator (Not God Object)

The GameMode should coordinate systems, not own every rule. Think "operating system kernel vs drivers":

```csharp
public sealed class GameMode : Component
{
    // GameMode is BORING on purpose - it just coordinates
    [Property] public StateMachine RoundState { get; set; }
    
    protected override void OnStart()
    {
        // Transition to first state, rules react via events
        RoundState.ChangeState("WarmUp");
    }
}
```

**Why this works**: The kernel is stable and predictable. Rules are loadable modules that can change frequently without destabilizing everything.

### 2. Client vs Pawn Split (Critical for Multiplayer)

This is the backbone of multiplayer correctness:

| Concept | Represents | Survives Death? | Examples |
|---------|------------|-----------------|----------|
| **Client** | Player identity | ✅ Yes | Team, score, connection, settings |
| **Pawn** | Controllable body | ❌ No | Health, position, inventory |

```csharp
// Client - persistent player record
public sealed class Client : Component
{
    [Sync] public int Team { get; set; }
    [Sync] public int Score { get; set; }
    public Pawn CurrentPawn { get; private set; }
    
    public void Possess(Pawn pawn)
    {
        CurrentPawn?.Depossess();
        CurrentPawn = pawn;
        pawn.Owner = this;
    }
}

// Pawn - disposable body
public sealed class Pawn : Component
{
    [Sync] public float Health { get; set; } = 100f;
    public Client Owner { get; set; }
    
    public void OnDeath()
    {
        // Client survives, pawn is destroyed
        Owner.CurrentPawn = null;
        GameObject.Destroy();
    }
}
```

**Practical implications**:
- Spectating = "client has no live pawn, attach to someone else's"
- Respawn = "spawn new pawn, possess it"
- Scoreboards remain stable while pawns churn

### 3. Event-Driven Rules System

Rules subscribe to events instead of being called directly. This makes "new mode" = "new prefab + rule set":

```csharp
// Define game events
public record RoundStartedEvent : IGameEvent;
public record PlayerKilledEvent(Client Killer, Client Victim) : IGameEvent;
public record BombPlantedEvent(Vector3 Position) : IGameEvent;

// Rules react independently
public sealed class FreezeTimeRule : Component, IGameEventHandler<RoundStartedEvent>
{
    [Property] public float FreezeDuration { get; set; } = 5f;
    
    public void OnGameEvent(RoundStartedEvent e)
    {
        // Freeze all pawns for N seconds
        foreach (var pawn in Scene.GetAllComponents<Pawn>())
        {
            pawn.IsFrozen = true;
        }
        // Schedule unfreeze...
    }
}

public sealed class KillScoreRule : Component, IGameEventHandler<PlayerKilledEvent>
{
    [Property] public int PointsPerKill { get; set; } = 1;
    
    public void OnGameEvent(PlayerKilledEvent e)
    {
        e.Killer.Score += PointsPerKill;
    }
}
```

**How to extend without breaking**: Add a new rule that subscribes to existing events. Avoid touching GameLoop unless you need a new phase.

### 4. Component-Based Weapons

Weapons are **data + mix-in components**, not inheritance trees:

```csharp
// Data defines stats (EquipmentResource.cs)
[GameResource("Equipment", "equip", "Equipment Definition")]
public class EquipmentResource : GameResource
{
    public string DisplayName { get; set; }
    public int Price { get; set; }
    public float Damage { get; set; }
    public float FireRate { get; set; }
    public int MagazineSize { get; set; }
    public GameObject ViewModelPrefab { get; set; }
    public GameObject WorldModelPrefab { get; set; }
}

// Base equipment loads data
public class Equipment : Component
{
    [Property] public EquipmentResource Resource { get; set; }
}

// Behaviors are composable components
public class Shootable : Component
{
    [RequireComponent] Equipment Equipment { get; set; }
    
    public void Shoot()
    {
        // Use Equipment.Resource.Damage, FireRate, etc.
    }
}

public class Reloadable : Component
{
    [Property] public int CurrentAmmo { get; set; }
    
    public void Reload() { /* ... */ }
}

public class Aimable : Component
{
    [Property] public float AimFOV { get; set; } = 60f;
    
    public void StartAiming() { /* ... */ }
}
```

**To create a new weapon**: Combine components + tweak data. No new classes needed.

| Weapon Type | Components |
|-------------|------------|
| Rifle | Shootable, Reloadable, Aimable |
| Shotgun | Shootable, Reloadable |
| Knife | Melee |
| Grenade | Throwable |
| Pistol | Shootable, Reloadable, Aimable |

## Component Fundamentals

### Lifecycle Methods

```csharp
public sealed class MyComponent : Component
{
    [Property] public float Speed { get; set; } = 100f;
    [RequireComponent] Rigidbody Body { get; set; }
    
    protected override void OnAwake() { }       // Created, after deserialization
    protected override void OnStart() { }       // First enable, before first update
    protected override void OnUpdate() { }      // Every frame (client-side feel)
    protected override void OnFixedUpdate() { } // Physics tick, 50/sec (authoritative)
    protected override void OnDestroy() { }     // Cleanup
}
```

**Order**: `OnAwake` → `OnStart` → `OnUpdate`/`OnFixedUpdate` → `OnDestroy`

### Input Handling

```csharp
protected override void OnUpdate()
{
    if (IsProxy) return;  // CRITICAL: Don't process input for other players!
    
    if (Input.Pressed("Jump")) Jump();
    if (Input.Down("Attack1")) Shoot();
    if (Input.Released("Reload")) Reload();
    
    Vector3 move = Input.AnalogMove;   // WASD or left stick
    Angles look = Input.AnalogLook;    // Mouse or right stick
}
```

## Networking Essentials

### Player Spawning

```csharp
public sealed class GameNetworkManager : Component, Component.INetworkListener
{
    [Property] public GameObject PlayerPrefab { get; set; }
    [Property] public GameObject PawnPrefab { get; set; }
    
    public void OnActive(Connection connection)
    {
        // Create persistent client
        var clientGo = new GameObject();
        var client = clientGo.Components.Create<Client>();
        clientGo.NetworkSpawn(connection);
        
        // Create disposable pawn
        var pawnGo = PawnPrefab.Clone(GetSpawnPoint());
        pawnGo.NetworkSpawn(connection);
        
        // Link them
        client.Possess(pawnGo.Components.Get<Pawn>());
    }
}
```

### RPC Patterns

```csharp
// Broadcast to all clients
[Rpc.Broadcast]
public void OnDamaged(float amount)
{
    PlayHitSound();
    ShowDamageEffect();
}

// Server-authoritative action
[Rpc.Host]
public void RequestBuyWeapon(string weaponId)
{
    if (CanAfford(weaponId))
    {
        DeductMoney(weaponId);
        GiveWeapon(weaponId);
    }
}
```

### Authority Rules

| What | Authority | Why |
|------|-----------|-----|
| Damage, hit validation | Server | Prevents cheating |
| Round state, scoring | Server | Single source of truth |
| Viewmodel animations | Client | Immediate feel |
| Crosshair feedback | Client | Responsiveness |

## UI with Razor Components

### The HUD Data Bridge Pattern

Don't let Razor components reach deep into game objects. Instead, use a **bridge component** that samples state and exposes HUD-friendly properties:

```csharp
// HudDataBridge.cs - Lives on HUD prefab
public sealed class HudDataBridge : Component
{
    // Viewer's pawn (who we're watching)
    private Pawn ViewerPawn => PlayerClient.Viewer?.Pawn as Pawn;
    private HealthComponent Health => ViewerPawn?.HealthComponent;
    
    // HUD-friendly projections (no nulls, formatted strings)
    public float HealthPercent => Health?.Health ?? 0f;
    public bool HasPawn => ViewerPawn.IsValid();
    public string ViewerName => PlayerClient.Viewer?.DisplayName ?? "Unknown";
    
    // Ammo (when you add weapons)
    public int CurrentAmmo => 0; // TODO: read from equipped weapon
    public int MaxAmmo => 0;
    
    // Timer (when you add round timer global)
    public string TimerText => "0:00"; // TODO: read from RoundTimerGlobal
}
```

### Razor Reads from Bridge

```razor
@inherits PanelComponent

@{
    var bridge = Scene.GetAllComponents<HudDataBridge>().FirstOrDefault();
    if (bridge == null) return;
}

<root>
    <div class="hud">
        @if (bridge.HasPawn)
        {
            <div class="health-bar">
                <div class="fill" style="width: @(bridge.HealthPercent)%"></div>
            </div>
            <label class="ammo">@bridge.CurrentAmmo / @bridge.MaxAmmo</label>
        }
        else
        {
            <div class="spectating">Spectating @bridge.ViewerName</div>
        }
    </div>
</root>

@code {
    protected override int BuildHash()
    {
        var bridge = Scene.GetAllComponents<HudDataBridge>().FirstOrDefault();
        return HashCode.Combine(bridge?.HealthPercent, bridge?.HasPawn, bridge?.CurrentAmmo);
    }
}
```

### Why This Pattern?

| Without Bridge | With Bridge |
|----------------|-------------|
| UI reaches into `PlayerClient.Viewer.Pawn.HealthComponent.Health` | UI reads `bridge.HealthPercent` |
| Null checks everywhere in Razor | Null handling in one place |
| Hard to debug (which null broke?) | Log bridge values, verify before UI |
| Tight coupling | Bridge is the contract |

### Legacy Pattern (Still Works, Less Clean)

```razor
@inherits PanelComponent

<root>
    <div class="hud">
        <div class="health-bar">
            <div class="fill" style="width: @(HealthPercent)%"></div>
        </div>
        <label class="ammo">@CurrentAmmo / @MaxAmmo</label>
    </div>
</root>

@code {
    private Pawn LocalPawn => PlayerClient.Local?.CurrentPawn;
    
    public float HealthPercent => LocalPawn?.Health ?? 0;
    public int CurrentAmmo => LocalPawn?.ActiveWeapon?.CurrentAmmo ?? 0;
    public int MaxAmmo => LocalPawn?.ActiveWeapon?.Resource.MagazineSize ?? 0;
}
```

**Key practice**: UI should be a "read model" that subscribes to state, not procedural "push updates".

## Bot AI with Behaviour Trees

Bots use behaviour trees with classic results (Success, Running, Failure):

```csharp
// Node interface
public interface IBehaviourNode
{
    NodeResult Execute(BotContext ctx);
}

public enum NodeResult { Success, Running, Failure }

// Example: Shoot if has target
public class ShootNode : IBehaviourNode
{
    public NodeResult Execute(BotContext ctx)
    {
        if (ctx.Target == null) return NodeResult.Failure;
        
        ctx.Pawn.LookAt(ctx.Target.Position);
        ctx.Pawn.ActiveWeapon?.Shoot();
        
        return NodeResult.Success;
    }
}

// Selector runs children until one succeeds
public class SelectorNode : IBehaviourNode
{
    public List<IBehaviourNode> Children { get; set; }
    
    public NodeResult Execute(BotContext ctx)
    {
        foreach (var child in Children)
        {
            var result = child.Execute(ctx);
            if (result != NodeResult.Failure)
                return result;
        }
        return NodeResult.Failure;
    }
}
```

**Why this works**: When a bot fails, you can pinpoint which node returned failure. Debug decisions as a trace, not a mystery.

## Common Pitfalls & Solutions

| Pitfall | Problem | Solution |
|---------|---------|----------|
| Missing `IsProxy` check | Input runs for all players | Add `if (IsProxy) return;` in `OnUpdate` |
| Forgetting `NetworkSpawn()` | Objects don't appear on other clients | Call `go.NetworkSpawn()` after cloning |
| Physics in `OnUpdate` | Frame-rate dependent behavior | Use `OnFixedUpdate` for physics |
| State on Pawn | Values "survive death" accidentally | Put persistent state on Client |
| God GameMode | Every rule is hardcoded | Extract rules as event handlers |
| Deep inheritance | Small changes break everything | Use component composition |
| UI querying deep objects | Tight coupling, hard to debug | UI subscribes to aggregated state |

## Debugging Tips

```csharp
// Console logging
Log.Info("Debug message");
Log.Warning("Something weird");
Log.Error("Something broke!");

// Make invisible systems visible - add debug HUD
protected override void OnUpdate()
{
    if (DebugMode)
    {
        DebugOverlay.Text($"RoundState: {GameMode.CurrentState}");
        DebugOverlay.Text($"Score: {PlayerClient.Local?.Score}");
    }
}

// Console commands
[ConCmd("debug_kill")]
static void DebugKill()
{
    PlayerClient.Local?.CurrentPawn?.TakeDamage(9999);
}
```

## Project Structure

```
Code/
├── GameLoop/
│   ├── GameMode.cs              # Orchestrator
│   ├── GameNetworkManager.cs    # Connection handling
│   └── Rules/                   # Composable rules
│       ├── FreezeTimeRule.cs
│       ├── TeamAssignerRule.cs
│       └── RoundLimitRule.cs
├── PawnSystem/
│   ├── PlayerClient.cs                # Persistent player state
│   ├── Pawn.cs                  # Disposable body
│   └── Player/
│       ├── PlayerController.cs
│       ├── PlayerInventory.cs
│       └── PlayerCamera.cs
├── Weapons/
│   ├── Equipment.cs
│   ├── EquipmentResource.cs
│   └── Components/
│       ├── Shootable.cs
│       ├── Reloadable.cs
│       └── Aimable.cs
├── UI/
│   └── HUD/
│       ├── MainHUD.razor
│       └── Scoreboard.razor
└── Bots/
    ├── BotController.cs
    └── Nodes/

Assets/
├── prefabs/
│   ├── player/
│   ├── weapons/
│   └── game_modes/
│       ├── deathmatch.prefab
│       └── bomb_defusal.prefab
└── scenes/
```

## Quick Reference

| Task | Code |
|------|------|
| Create GameObject | `var go = new GameObject();` |
| Clone prefab | `var go = Prefab.Clone(position);` |
| Get component | `go.Components.Get<T>()` |
| Find all in scene | `Scene.GetAllComponents<T>()` |
| Network spawn | `go.NetworkSpawn(connection);` |
| Check ownership | `if (IsProxy) return;` |
| Raycast | `Scene.Trace.Ray(start, end).Run()` |
| Emit event | `Scene.Dispatch(new MyEvent());` |
| Subscribe to event | Implement `IGameEventHandler<T>` |

## Highest Leverage Moves for Beginners

1. **Build one tiny HUD component** (timer) and make it update correctly every frame
2. **Implement one event handler** and log/visualize the event on-screen  
3. **Change one weapon stat** via a resource file and verify in-game
4. **Add one interaction** that touches health/damage (heal zone or explosive)

> **Make invisible systems visible**: When learning events, rules, or AI, add temporary debug text. Most "I'm stuck" moments come from not knowing whether code is firing at all.

## References

### Official Documentation
- **Docs**: https://docs.facepunch.com/s/sbox-dev
- **API Reference**: https://sbox.game/api
- **GitHub Issues**: https://github.com/Facepunch/sbox-issues

### Official Facepunch Gamemode Repositories

Study these repos for production-quality patterns:

| Repository | What to Learn |
|------------|---------------|
| [Facepunch/sbox-hc1](https://github.com/Facepunch/sbox-hc1) | Counter-Strike style FPS - state machines, event-driven rules, Client/Pawn split, component weapons, bot AI |
| [Facepunch/sbox-scenestaging](https://github.com/Facepunch/sbox-scenestaging) | Core engine examples - component patterns, networking basics, scene setup |
| [Facepunch/sbox-sdftest](https://github.com/Facepunch/sbox-sdftest) | Technical demos - SDF rendering, procedural geometry |

> **Tip**: Clone `sbox-hc1` and search for patterns like `IGameEventHandler`, `Client`, `Pawn`, and `Equipment` to see the architecture in action.
