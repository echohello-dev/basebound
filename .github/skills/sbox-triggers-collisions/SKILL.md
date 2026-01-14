---
name: sbox-triggers-collisions
description: Implement trigger and collision detection in S&box Scene System. Use when creating interactive zones (pickups, damage areas), physics interactions (bouncy pads, moving platforms), or detecting when objects enter/exit areas or collide with each other.
metadata:
  author: basebound
  version: "1.0"
  source: "S&box Triggers & Collisions 2024"
compatibility: S&box (Facepunch), Scene System 2024, .NET 10, C# 14
---

# S&box Triggers & Collisions

Implement trigger zones and collision detection in S&box using the component-based interface system.

## When to Use

- Creating interactive zones (heal pads, damage areas, checkpoints)
- Detecting when players enter/exit areas
- Implementing physics interactions (bouncy pads, jump pads)
- Building pickups and collectibles
- Creating moving platforms or obstacles
- Detecting solid object collisions

## Core Concepts

S&box uses **Component Interfaces** to handle physical interactions:

| Type | Purpose | Example |
|------|---------|---------|
| **Trigger** | Ethereal zones (no physics) | Pickups, damage zones, checkpoints |
| **Collision** | Solid interactions (physics) | Bouncy pads, walls, platforms |

### Trigger vs Collision

```
Trigger:
- Player passes through
- No physical bump
- Requires: Collider with "Is Trigger" = true

Collision:
- Player bumps into object
- Physical interaction
- Requires: Collider + Rigidbody on moving object
```

## Triggers

### Interface: `Component.ITriggerListener`

Used for zones that detect entry/exit without blocking movement.

### Requirements

1. GameObject must have a **Collider** (Box, Sphere, Capsule)
2. Collider must have **"Is Trigger"** enabled
3. Component must implement `Component.ITriggerListener`

### Methods

```csharp
void OnTriggerEnter(Collider other)  // Object enters zone
void OnTriggerExit(Collider other)   // Object leaves zone
```

### Basic Trigger Example

```csharp
using Sandbox;

public sealed class HealZone : Component, Component.ITriggerListener
{
    [Property] public float HealAmount { get; set; } = 10f;
    [Property] public float HealInterval { get; set; } = 1f;
    
    public void OnTriggerEnter(Collider other)
    {
        Log.Info($"{other.GameObject.Name} entered heal zone");
        
        var player = other.GameObject.Components.Get<PlayerController>();
        if (player != null)
        {
            player.Heal(HealAmount);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        Log.Info($"{other.GameObject.Name} left heal zone");
    }
}
```

### Scene Setup for Triggers

1. Create GameObject (Cube, Sphere, etc.)
2. Add **Box Collider** (or Sphere Collider)
3. Enable **"Is Trigger"** checkbox
4. Attach your trigger component

```
GameObject: HealPad
├── Box Collider (Is Trigger: ✓)
└── HealZone Component
```

## Collisions

### Interface: `Component.ICollisionListener`

Used for solid interactions where objects physically bump into each other.

### Requirements

1. GameObject must have a **Collider**
2. **Moving object** (e.g., Player) must have a **Rigidbody**
3. Component must implement `Component.ICollisionListener`

**Critical**: At least one object in the collision must have a Rigidbody component.

### Methods

```csharp
void OnCollisionStart(Collision collision)   // Collision begins
void OnCollisionUpdate(Collision collision)  // Collision ongoing
void OnCollisionStop(Collision collision)    // Collision ends
```

### Basic Collision Example

```csharp
using Sandbox;

public sealed class BouncyPad : Component, Component.ICollisionListener
{
    [Property] public float Bounciness { get; set; } = 500f;
    
    public void OnCollisionStart(Collision collision)
    {
        Log.Info($"Collision with {collision.Other.GameObject.Name}");
        
        var player = collision.Other.GameObject.Components.Get<PlayerController>();
        if (player != null)
        {
            // Launch player upward
            player.Velocity = player.Velocity.WithZ(Bounciness);
        }
    }
    
    public void OnCollisionUpdate(Collision collision)
    {
        // Called every frame while colliding
    }
    
    public void OnCollisionStop(Collision collision)
    {
        // Called when collision ends
    }
}
```

### Player Rigidbody Setup

For collisions to work with Character Controllers:

```csharp
// Player component setup
[RequireComponent] CharacterController Controller { get; set; }
[RequireComponent] Rigidbody Body { get; set; }

protected override void OnStart()
{
    // Make Rigidbody "inert" - doesn't interfere with movement
    Body.Gravity = false;
    Body.LockAxes = RigidbodyAngularLockMode.All;
}
```

### Scene Setup for Collisions

1. Create GameObject (platform, obstacle)
2. Add **Collider** (not trigger)
3. Ensure **Player has Rigidbody**
4. Attach your collision component

```
GameObject: BouncyPlatform
├── Box Collider (Is Trigger: ✗)
└── BouncyPad Component

GameObject: Player
├── Character Controller
├── Rigidbody (Gravity: Off, Lock Axes: All)
└── PlayerController Component
```

## Accessing Collision Data

### From Collider (Triggers)

```csharp
public void OnTriggerEnter(Collider other)
{
    // Get GameObject
    var go = other.GameObject;
    
    // Get specific component
    var player = other.GameObject.Components.Get<PlayerController>();
    
    // Check if component exists
    if (other.GameObject.Components.TryGet<Health>(out var health))
    {
        health.TakeDamage(10);
    }
}
```

### From Collision

```csharp
public void OnCollisionStart(Collision collision)
{
    // Get the other GameObject
    var other = collision.Other.GameObject;
    
    // Get collision point
    var contact = collision.Contact;
    Vector3 position = contact.Point;
    Vector3 normal = contact.Normal;
    
    // Get component from other object
    var player = other.Components.Get<PlayerController>();
}
```

## Practical Examples

### Checkpoint System

```csharp
public sealed class Checkpoint : Component, Component.ITriggerListener
{
    [Property] public int CheckpointID { get; set; }
    [Property] public bool IsActivated { get; set; } = false;
    
    public void OnTriggerEnter(Collider other)
    {
        var player = other.GameObject.Components.Get<Player>();
        if (player != null && !IsActivated)
        {
            IsActivated = true;
            player.SetRespawnPoint(GameObject.Transform.Position);
            Log.Info($"Checkpoint {CheckpointID} activated!");
        }
    }
    
    public void OnTriggerExit(Collider other) { }
}
```

### Damage Zone

```csharp
public sealed class DamageZone : Component, Component.ITriggerListener
{
    [Property] public float Damage { get; set; } = 10f;
    [Property] public float DamageInterval { get; set; } = 1f;
    
    private Dictionary<GameObject, TimeSince> _damageTimes = new();
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.GameObject.Components.TryGet<Health>(out var health))
        {
            _damageTimes[other.GameObject] = 0;
        }
    }
    
    protected override void OnUpdate()
    {
        foreach (var kvp in _damageTimes.ToList())
        {
            if (kvp.Value > DamageInterval)
            {
                if (kvp.Key.Components.TryGet<Health>(out var health))
                {
                    health.TakeDamage(Damage);
                    _damageTimes[kvp.Key] = 0;
                }
            }
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        _damageTimes.Remove(other.GameObject);
    }
}
```

### Jump Pad

```csharp
public sealed class JumpPad : Component, Component.ICollisionListener
{
    [Property] public Vector3 LaunchVelocity { get; set; } = new Vector3(0, 0, 800);
    [Property] public string SoundEffect { get; set; } = "jumppad";
    
    public void OnCollisionStart(Collision collision)
    {
        var controller = collision.Other.GameObject.Components.Get<CharacterController>();
        if (controller != null)
        {
            // Launch player
            controller.Velocity = LaunchVelocity;
            
            // Play sound
            Sound.Play(SoundEffect, GameObject.Transform.Position);
        }
    }
    
    public void OnCollisionUpdate(Collision collision) { }
    public void OnCollisionStop(Collision collision) { }
}
```

### Collectible Pickup

```csharp
public sealed class Coin : Component, Component.ITriggerListener
{
    [Property] public int Value { get; set; } = 1;
    [Property] public string PickupSound { get; set; } = "coin";
    
    public void OnTriggerEnter(Collider other)
    {
        var player = other.GameObject.Components.Get<Player>();
        if (player != null)
        {
            // Give player coins
            player.AddCoins(Value);
            
            // Play sound
            Sound.Play(PickupSound, GameObject.Transform.Position);
            
            // Destroy this coin
            GameObject.Destroy();
        }
    }
    
    public void OnTriggerExit(Collider other) { }
}
```

### Moving Platform (Collision)

```csharp
public sealed class MovingPlatform : Component, Component.ICollisionListener
{
    [Property] public Vector3 MoveDistance { get; set; } = new Vector3(0, 0, 100);
    [Property] public float MoveSpeed { get; set; } = 50f;
    
    private Vector3 _startPosition;
    private HashSet<GameObject> _ridingObjects = new();
    
    protected override void OnStart()
    {
        _startPosition = GameObject.Transform.Position;
    }
    
    protected override void OnFixedUpdate()
    {
        // Oscillate position
        float t = (float)Math.Sin(Time.Now * MoveSpeed * 0.01f);
        GameObject.Transform.Position = _startPosition + MoveDistance * t;
        
        // Move riding objects with platform
        foreach (var obj in _ridingObjects)
        {
            if (obj.IsValid())
            {
                var controller = obj.Components.Get<CharacterController>();
                if (controller != null)
                {
                    controller.Velocity += MoveDistance * MoveSpeed * Time.Delta;
                }
            }
        }
    }
    
    public void OnCollisionStart(Collision collision)
    {
        _ridingObjects.Add(collision.Other.GameObject);
    }
    
    public void OnCollisionUpdate(Collision collision) { }
    
    public void OnCollisionStop(Collision collision)
    {
        _ridingObjects.Remove(collision.Other.GameObject);
    }
}
```

## Common Pitfalls

### Trigger Not Firing
```csharp
// ❌ Forgot to enable "Is Trigger"
// Solution: In scene, select GameObject → Box Collider → Is Trigger: ✓

// ❌ Didn't implement interface
public class MyTrigger : Component { } // Missing : Component.ITriggerListener

// ✅ Correct
public class MyTrigger : Component, Component.ITriggerListener { }
```

### Collision Not Working
```csharp
// ❌ Player missing Rigidbody
// Solution: Add Rigidbody to player GameObject

// ❌ Rigidbody interfering with movement
// Solution: Disable gravity and lock axes
protected override void OnStart()
{
    Body.Gravity = false;
    Body.LockAxes = RigidbodyAngularLockMode.All;
}

// ❌ Collider set to "Is Trigger"
// Solution: Uncheck "Is Trigger" for solid collisions
```

### Null Reference
```csharp
// ❌ Crashes if component doesn't exist
var player = other.GameObject.Components.Get<Player>();
player.Health += 10; // Null reference!

// ✅ Safe check
var player = other.GameObject.Components.Get<Player>();
if (player != null)
{
    player.Health += 10;
}

// ✅ TryGet pattern
if (other.GameObject.Components.TryGet<Player>(out var player))
{
    player.Health += 10;
}
```

## Advanced Patterns

### Tag-Based Filtering

```csharp
public sealed class PlayerOnlyTrigger : Component, Component.ITriggerListener
{
    [Property] public string RequiredTag { get; set; } = "Player";
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.GameObject.Tags.Has(RequiredTag))
        {
            // Only trigger for objects with "Player" tag
            HandlePlayerEnter(other.GameObject);
        }
    }
    
    public void OnTriggerExit(Collider other) { }
}
```

### Layered Collision Response

```csharp
public sealed class MultiResponsePlatform : Component, Component.ICollisionListener
{
    public void OnCollisionStart(Collision collision)
    {
        var go = collision.Other.GameObject;
        
        // Different responses for different components
        if (go.Components.TryGet<Player>(out var player))
        {
            player.Velocity = player.Velocity.WithZ(500);
        }
        else if (go.Components.TryGet<Enemy>(out var enemy))
        {
            enemy.TakeDamage(100);
        }
        else if (go.Components.TryGet<Projectile>(out var projectile))
        {
            projectile.Destroy();
        }
    }
    
    public void OnCollisionUpdate(Collision collision) { }
    public void OnCollisionStop(Collision collision) { }
}
```

### One-Time Trigger

```csharp
public sealed class OneTimeTrigger : Component, Component.ITriggerListener
{
    [Property] public bool HasTriggered { get; set; } = false;
    
    public void OnTriggerEnter(Collider other)
    {
        if (HasTriggered) return;
        
        var player = other.GameObject.Components.Get<Player>();
        if (player != null)
        {
            HasTriggered = true;
            // Do one-time action
            player.UnlockAbility("DoubleJump");
        }
    }
    
    public void OnTriggerExit(Collider other) { }
}
```

## Exploring More Interfaces

S&box provides many component interfaces. Discover them by typing `Component.I` in your IDE:

- `Component.ITriggerListener` - Trigger zones
- `Component.ICollisionListener` - Collision detection
- `Component.ITintable` - Color tinting
- `Component.INetworkSpawnable` - Network spawning
- And more...

## Quick Reference

### Trigger Template

```csharp
using Sandbox;

public sealed class MyTrigger : Component, Component.ITriggerListener
{
    public void OnTriggerEnter(Collider other)
    {
        // Object entered
    }
    
    public void OnTriggerExit(Collider other)
    {
        // Object exited
    }
}
```

**Scene Setup:**
- Add Collider
- Enable "Is Trigger"
- Attach component

### Collision Template

```csharp
using Sandbox;

public sealed class MyCollision : Component, Component.ICollisionListener
{
    public void OnCollisionStart(Collision collision)
    {
        // Collision started
    }
    
    public void OnCollisionUpdate(Collision collision)
    {
        // Collision ongoing
    }
    
    public void OnCollisionStop(Collision collision)
    {
        // Collision ended
    }
}
```

**Scene Setup:**
- Add Collider (not trigger)
- Ensure player has Rigidbody
- Attach component

## Resources

- **S&box Wiki**: [Scene System Documentation](https://wiki.facepunch.com/sbox/)
- **API Reference**: [Component Interfaces](https://docs.facepunch.com/s/sbox-dev)
- **Video Tutorial**: S&box Triggers & Collisions 2024
- **Component Discovery**: Type `Component.I` in IDE to see all interfaces
