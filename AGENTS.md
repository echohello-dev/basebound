# AGENTS.md

> AI agent instructions for the **Basebound** S&box game project.

## Project Overview

S&box game built with C# on Source 2 engine. Uses Unity/Godot-like scene system with GameObjects and Components.

- **Engine**: S&box (Facepunch) - C# with millisecond hot-reload
- **Target**: .NET 10, C# 14, root namespace `Sandbox`
- **Type**: Multiplayer (64 players, 50 tick rate)
- **Startup**: `Assets/scenes/minimal.scene`

## Documentation Structure

All detailed documentation is in the `Docs/` folder for easy reference and updates:

- **[Docs/setup.md](Docs/setup.md)** - Setup, installation, project structure, getting started
- **[Docs/architecture.md](Docs/architecture.md)** - Component pattern, lifecycle, editor extensions, project organization
- **[Docs/code-standards.md](Docs/code-standards.md)** - Naming conventions, coding patterns, best practices, error handling
- **[Docs/networking.md](Docs/networking.md)** - Multiplayer, RPC, proxy objects, network flow patterns
- **[Docs/gameplay.md](Docs/gameplay.md)** - Game systems (economy, raids, contracts, base building), tick cycle
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Pull request process, branch naming, development workflow

**Refer to these docs** when explaining features, not just inline code snippets.

### When to Update Documentation

Update docs whenever:
- Adding or modifying components and architecture → [Docs/architecture.md](Docs/architecture.md)
- Adding networking features → [Docs/networking.md](Docs/networking.md)
- Creating new game systems → [Docs/gameplay.md](Docs/gameplay.md)
- Changing code standards or conventions → [Docs/code-standards.md](Docs/code-standards.md)
- Updating setup or tooling → [Docs/setup.md](Docs/setup.md)
- Removing redundant information → consolidate into single doc, cross-reference
- Project overview changes → [README.md](README.md)

## Project Structure

```
Code/           → Runtime components (game logic)
Editor/         → Editor-only tools (separate assembly)
Assets/         → Scenes (.scene JSON), materials, models
ProjectSettings/ → Input.config, Collision.config
docs/           → Comprehensive documentation (keep updated!)
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

## Agent Skills

Agent skills are located in `.github/skills/` and follow the [agentskills.io specification](https://agentskills.io/specification).

### Using Skills

- **Creating skills**: Use the [agent-skill-authoring skill](.github/skills/agent-skill-authoring/)
- **Location**: All skills must be in `.github/skills/<skill-name>/SKILL.md`
- **Self-contained**: Skills should be standalone, no external doc updates required
- **Discovery**: Skills are loaded automatically by agents that support the agentskills.io spec

### Rules When Working on Skills

**DO NOT modify** these files when creating or updating skills:
- ❌ `Docs/` directory
- ❌ `README.md`
- ❌ `AGENTS.md` (except adding new skill to Available Skills list below)

Skills are self-contained and should not require project documentation updates.

### Available Skills

| Skill | Purpose | Use When |
|-------|---------|----------|
| [agent-skill-authoring](.github/skills/agent-skill-authoring/) | Meta-skill for creating effective agent skills | Creating new skills, understanding skill format |
| [component-attributes](.github/skills/component-attributes/) | S&box Component property attributes | Customizing inspector, organizing properties, adding constraints |
| [sbox-ui-razor](.github/skills/sbox-ui-razor/) | Razor UI development (HTML/CSS/C#) | Building HUDs, menus, healthbars, interactive UI |
| [sbox-triggers-collisions](.github/skills/sbox-triggers-collisions/) | Trigger & collision detection | Interactive zones, physics interactions, detecting enter/exit |
| [sbox-gamemode-dev](.github/skills/sbox-gamemode-dev/) | S&box gamemode development | Building game modes, player systems, scoring |

### Adding New Skills

When adding a new skill to the project:

1. Create skill directory: `.github/skills/<skill-name>/`
2. Add `SKILL.md` with proper frontmatter (see agent-skill-authoring)
3. **Optional**: Add entry to table above in AGENTS.md for visibility
4. **Do NOT** update `Docs/`, `README.md`, or other project files

## Documentation Standards

Keep documentation synchronized with code:

1. **Explain in docs/ first** - Use comprehensive guides for new features
2. **Cross-reference** - Link between related documentation files
3. **Remove redundancy** - Don't repeat info across multiple files; instead, cross-link
4. **Update on change** - Keep docs up to date with code changes
5. **Follow naming** - Use kebab-case for filenames: `code-standards.md`, `game-systems.md`

### Documentation Checklist

Before marking a feature complete:
- [ ] Code follows standards in [Docs/code-standards.md](Docs/code-standards.md)
- [ ] Architecture documented in [Docs/architecture.md](Docs/architecture.md) if applicable
- [ ] Networking documented in [Docs/networking.md](Docs/networking.md) if multiplayer
- [ ] Game system documented in [Docs/gameplay.md](Docs/gameplay.md) if new system
- [ ] Setup documented in [Docs/setup.md](Docs/setup.md) if tooling changed
- [ ] README.md updated if user-facing changes
- [ ] CONTRIBUTING.md updated if workflow changed
- [ ] No redundant info across docs (consolidate and cross-link)
