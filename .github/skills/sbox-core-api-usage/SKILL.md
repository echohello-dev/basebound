---
name: sbox-core-api-usage
description: S&box core API usage patterns and references. Use when building gameplay components, traces, effects, and networked interactions (excluding gamemode design).
metadata:
  author: basebound
  version: "1.0"
  source: "Facepunch sbox-hc1"
compatibility: S&box (Facepunch), .NET 10, C# 14
---

# S&box Core API Usage

Reference patterns for common S&box APIs drawn from `sbox-hc1`. Focused on component usage, scene queries, traces, spawning, and network patterns (no gamemode details).

## When to Use

- Implementing gameplay components or world interactions
- Spawning/despawning prefabs and effects
- Running traces and collision checks
- Playing audio or accessing resources
- Managing RPCs and host-only behavior

## Component Configuration

```csharp
[Property, Category("Config")] public float Duration { get; set; } = 45f;
[RequireComponent] public Spottable Spottable { get; private set; }
[Sync( SyncFlags.FromHost )] public TimeSince TimeSincePlanted { get; private set; }
```

## Scene Queries and Tags

```csharp
GameObject.Tags.Add( "zone" );
var zones = Scene.GetAllComponents<Zone>();
```

## Trace Patterns

```csharp
var trace = Scene.Trace.Ray( start, end )
    .IgnoreGameObjectHierarchy( GameObject.Root )
    .UseHitboxes()
    .WithoutTags( "trigger", "ragdoll", "movement" )
    .Run();

var hits = Scene.Trace.Ray( start, end )
    .Size( radius )
    .RunAll();
```

## Prefabs, Effects, and Destruction

```csharp
var dropped = Prefab.Clone( position, rotation );
dropped.NetworkSpawn();

if ( effectPrefab.IsValid() )
{
    effectPrefab.Clone( WorldPosition );
}

GameObject.Destroy();
```

## Razor UI Patterns

```razor
@inherits PanelComponent

@if (!IsHudEnabled)
    return;

<root>
    <DamageOverlay />
</root>

@code
{
    protected override int BuildHash()
    {
        return HashCode.Combine(IsHudEnabled, Client.Viewer.IsValid());
    }
}
```

## Scene and Prefab JSON

```json
{
  "Name": "Main Menu",
  "Components": [
    { "__type": "Sandbox.ScreenPanel" },
    { "__type": "Facepunch.UI.MainMenuPanel" }
  ]
}
```

```json
{
  "RootObject": {
    "Name": "hud",
    "Components": [
      { "__type": "Facepunch.UI.MainHUD" },
      { "__type": "Sandbox.ScreenPanel" }
    ]
  }
}
```

## Audio and Resources

```csharp
var resource = ResourceLibrary.Get<SoundEvent>( resourceId );
if ( resource != null )
{
    Sound.Play( resource, WorldPosition );
}
```

## Time Helpers

```csharp
if ( TimeSinceAction > Cooldown )
{
    TimeSinceAction = 0f;
}

var smooth = current.LerpTo( target, Time.Delta * 10f );
```

## Networking Patterns

```csharp
if ( !Networking.IsHost ) return;

[Rpc.Broadcast( NetFlags.HostOnly )]
public void StartDefusing( PlayerPawn player ) { }

[Rpc.Owner]
public void Deploy()
{
    if ( IsDeployed ) return;
    IsDeployed = true;
}

using ( Rpc.FilterInclude( Connection.Host ) )
{
    InflictDamage( target, damage, pos, dir, hitbox, flags );
}
```

## Gameplay Mechanics Patterns (Generic)

### Interaction and Use

```csharp
public UseResult CanUse( PlayerPawn player )
{
    return State is DoorState.Open or DoorState.Closed;
}

public void OnUse( PlayerPawn player )
{
    LastUse = 0f;
}
```

### Damage and Health

```csharp
[Property] public float Damage { get; set; } = 10f;
[Property] public float Interval { get; set; } = 0.5f;

void ITriggerListener.OnTriggerEnter( Collider other )
{
    if ( !Networking.IsHost ) return;
    var receiver = other.GameObject?.Root.GetComponentInChildren<IAreaDamageReceiver>();
    if ( !receiver.IsValid() ) return;
    receiver.ApplyAreaDamage( this );
}
```

### Audio and VFX

```csharp
var resource = ResourceLibrary.Get<SoundEvent>( resourceId );
if ( resource != null )
{
    Sound.Play( resource, WorldPosition );
}

if ( effectPrefab.IsValid() )
{
    effectPrefab.Clone( WorldPosition );
}
```

### Inventory and Equipment

```csharp
[Property] public EquipmentResource Resource { get; set; }

public UseResult CanUse( PlayerPawn player )
{
    if ( player.Inventory.CanTake( Resource ) == PlayerInventory.PickupResult.None )
        return "Can't pick this up";

    return true;
}
```

### Triggers and Zones

```csharp
GameObject.Tags.Add( "zone" );
var zones = Scene.GetAllComponents<Zone>();
```

## Common Pitfalls

### Forgetting Host Checks

**Problem**: Running authoritative logic on proxies.

**Fix**:

```csharp
if ( !Networking.IsHost ) return;
```

### Trace Ignoring Important Tags

**Problem**: Rays hitting trigger volumes or movement blockers.

**Fix**:

```csharp
Scene.Trace.Ray( start, end )
    .WithoutTags( "trigger", "playerclip" )
    .Run();
```

## Quick Reference

| Task | Example API |
| --- | --- |
| Spawn prefab | `Prefab.Clone(position, rotation)` |
| Network spawn | `GameObject.NetworkSpawn()` |
| Destroy | `GameObject.Destroy()` |
| Trace | `Scene.Trace.Ray(start, end)` |
| Play sound | `Sound.Play(soundEvent, WorldPosition)` |
| Load resource | `ResourceLibrary.Get<SoundEvent>(id)` |
| RPC broadcast | `[Rpc.Broadcast]` |
| Owner RPC | `[Rpc.Owner]` |
| Host-only guard | `if (!Networking.IsHost) return;` |
