# AGENTS.md

> AI agent instructions for the **Basebound** S&box game project.

## Project Overview

S&box game built with C# on Source 2 engine. Uses Unity/Godot-like scene system with GameObjects and Components.

- **Engine**: S&box (Facepunch) - C# with millisecond hot-reload
- **Target**: .NET 10, C# 14, root namespace `Sandbox`
- **Type**: Multiplayer (64 players, 50 tick rate)
- **Startup**: `Assets/scenes/minimal.scene`

## Project Structure

```
Code/           → Runtime components (game logic)
Editor/         → Editor-only tools (separate assembly)
Assets/         → Scenes (.scene JSON), materials, models
ProjectSettings/ → Input.config, Collision.config
```

## Component Pattern

All behavior uses Components inheriting from `Component`. See [Code/MyComponent.cs](Code/MyComponent.cs):

```csharp
public sealed class MyComponent : Component
{
    [Property] public string Value { get; set; }  // Inspector-exposed
    [RequireComponent] ModelRenderer Body { get; set; }  // Auto-linked

    protected override void OnAwake() { }       // Created, after deserialization
    protected override void OnStart() { }       // First enable, before OnFixedUpdate
    protected override void OnUpdate() { }      // Every frame
    protected override void OnFixedUpdate() { } // Physics tick (50/sec)
    protected override void OnDestroy() { }     // Cleanup
}
```

**Key attributes:**
- `[Property]` - Exposes to inspector, drag-drop from scene
- `[RequireComponent]` - Auto-references component on same GameObject (creates if missing)

## Networking (Multiplayer)

```csharp
// Spawn networked object
var go = PlayerPrefab.Clone(SpawnPoint.Transform.World);
go.NetworkSpawn();

// RPC broadcast to all clients
[Rpc.Broadcast]
public void OnJump() => Log.Info($"{this} jumped!");

// Check if controlled by another client
if (IsProxy) return;  // Don't process input for proxies
```

## Editor Extensions

Editor code in `Editor/` with separate assembly. See [Editor/MyEditorMenu.cs](Editor/MyEditorMenu.cs):

```csharp
[Menu("Editor", "Basebound/My Menu Option")]
public static void OpenMyMenu() { }
```

## Build & Run

1. **Launch**: `sbox-dev.exe -project "path/to/basebound.sbproj"`
2. **VS Debug**: Use [Code/Properties/launchSettings.json](Code/Properties/launchSettings.json) "Editor" profile
3. **Hot Reload**: Code changes compile in milliseconds automatically

## API Reference

- **Docs**: https://docs.facepunch.com/s/sbox-dev
- **API**: https://sbox.game/api
- **Global usings**: `Sandbox`, `System.Collections.Generic`, `System.Linq` ([Code/Assembly.cs](Code/Assembly.cs))
