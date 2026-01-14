---
name: sbox-gamemode-dev
description: Teaches S&box gamemode development with C# including component patterns, networking, input handling, UI, physics, and debugging. Use when learning S&box, building gamemodes, creating multiplayer games, or needing guidance on S&box best practices and common pitfalls.
metadata:
  author: basebound
  version: "1.0"
---

# S&box Gamemode Development Skill

This skill teaches you how to develop gamemodes in S&box (Facepunch's game engine on Source 2).

## When to Use This Skill

- Learning S&box game development from scratch
- Building a new gamemode or game feature
- Debugging multiplayer/networking issues
- Understanding component lifecycle and patterns
- Avoiding common pitfalls in S&box development

## Core Concepts

### Component Architecture

S&box uses a Unity/Godot-like component system. All game behavior lives in Components:

```csharp
public sealed class MyComponent : Component
{
    [Property] public float Speed { get; set; } = 100f;  // Exposed in editor
    [RequireComponent] Rigidbody Body { get; set; }      // Auto-linked
    
    protected override void OnStart() { }        // First enable
    protected override void OnUpdate() { }       // Every frame
    protected override void OnFixedUpdate() { }  // Physics tick (50/sec)
}
```

**Lifecycle order**: `OnLoad` → `OnAwake` → `OnStart` → `OnUpdate`/`OnFixedUpdate` → `OnDestroy`

### Input Handling

Define actions in `ProjectSettings/Input.config`, then query them:

```csharp
if (Input.Pressed("Jump"))     // Just pressed this frame
if (Input.Down("Attack1"))     // Held down
if (Input.Released("Reload"))  // Just released

Vector3 move = Input.AnalogMove;  // WASD or left stick
Angles look = Input.AnalogLook;   // Mouse or right stick
```

### Networking Fundamentals

**Player Spawning** (on host):
```csharp
public sealed class GameManager : Component, Component.INetworkListener
{
    [Property] public GameObject PlayerPrefab { get; set; }
    
    public void OnActive(Connection connection)
    {
        var player = PlayerPrefab.Clone(SpawnPosition);
        player.NetworkSpawn(connection);  // Assign ownership
    }
}
```

**Proxy Check** (critical for input):
```csharp
protected override void OnUpdate()
{
    if (IsProxy) return;  // Don't process input for other players!
    
    // Your input handling here
}
```

**RPC Broadcasting**:
```csharp
[Rpc.Broadcast]
public void OnDamaged(float amount)
{
    // Runs on ALL clients
    PlayHitSound();
}
```

### Physics & Raycasting

```csharp
var tr = Scene.Trace
    .Ray(startPos, endPos)
    .WithCollisionRules("bullet")
    .UseHitboxes(true)
    .Run();

if (tr.Hit)
{
    Log.Info($"Hit {tr.GameObject} at {tr.HitPosition}");
}
```

### UI with Razor

Create `.razor` files for UI panels:

```razor
@inherits PanelComponent

<root>
    <div class="hud">
        <label>Health: @Health</label>
    </div>
</root>

@code {
    [Property] public int Health { get; set; }
}
```

## Common Pitfalls & Blindspots

### 1. Forgetting `IsProxy` Check
**Problem**: Input code runs for all players, causing chaos.
**Solution**: Always check `if (IsProxy) return;` at the start of `OnUpdate`.

### 2. Not Calling `NetworkSpawn()`
**Problem**: Cloned objects don't appear on other clients.
**Solution**: Always call `go.NetworkSpawn()` after cloning networked prefabs.

### 3. Using `OnUpdate` for Physics
**Problem**: Frame-rate dependent physics behavior.
**Solution**: Use `OnFixedUpdate` for movement, forces, and physics logic.

### 4. Missing `[Property]` Attribute
**Problem**: Fields don't appear in the S&box editor.
**Solution**: Add `[Property]` to expose fields in the inspector.

### 5. Null Reference on Components
**Problem**: `GetComponent<T>()` returns null.
**Solution**: Use `[RequireComponent]` for guaranteed references, or check for null.

### 6. Scene References Breaking
**Problem**: References to scene objects become null after scene reload.
**Solution**: Use `[Property]` references or find objects dynamically.

### 7. RPC Not Calling
**Problem**: `[Rpc.Broadcast]` method doesn't execute on clients.
**Solution**: Ensure the method is on a networked GameObject and ownership is correct.

## Debugging Tips

```csharp
Log.Info("Debug message");           // Console output
Log.Warning("Something weird");      // Yellow warning
Log.Error("Something broke!");       // Red error

// Console commands
[ConCmd("debug_health")]
static void DebugHealth(int amount)
{
    Log.Info($"Setting health to {amount}");
}
```

**Log files**: `<sbox-install>/logs/Log.log`

## Project Structure Best Practices

```
Code/
├── Components/          # Reusable components
│   ├── Player/         # Player-related
│   └── Weapons/        # Weapon systems
├── GameModes/          # Gamemode-specific logic
├── UI/                 # Razor panels
└── Managers/           # Singleton managers

Assets/
├── prefabs/            # Prefab GameObjects
├── scenes/             # Scene files
└── ui/                 # UI assets
```

## Quick Reference

| Task | Code |
|------|------|
| Create GameObject | `var go = new GameObject();` |
| Clone prefab | `var go = Prefab.Clone(position);` |
| Get component | `var c = go.Components.Get<T>();` |
| Find in scene | `Scene.GetAllComponents<T>()` |
| Network spawn | `go.NetworkSpawn(connection);` |
| Check ownership | `if (IsProxy) return;` |
| Raycast | `Scene.Trace.Ray(start, end).Run()` |

## API References

- **Docs**: https://docs.facepunch.com/s/sbox-dev
- **API Reference**: https://sbox.game/api
- **GitHub Issues**: https://github.com/Facepunch/sbox-issues
