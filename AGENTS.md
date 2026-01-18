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

### High-Level Layout

```
Code/           → Runtime components (game logic)
Editor/         → Editor-only tools (separate assembly)
Assets/         → Scenes (.scene JSON), materials, models
ProjectSettings/ → Input.config, Collision.config
Docs/           → Comprehensive documentation (keep updated!)
```

### Recommended Shooter-Friendly Breakdown

Describe the project as a layered collection of folders under `MyGame/`:

- Place a single `README.md` at the root with project overview, setup, and contribution pointers.
- Under `Assets/`, group every non-code artifact. Keep animation graphs in `animgraphs/`, fonts in `fonts/`, and level data inside `maps/`, where each map (for example `dm_arena/`) owns its own subfolder plus a shared `prefabs/` library for reusable level pieces such as doors. Split materials into `decals/`, `effects/`, and `world/`. Store models under `models/` with subfolders for `player`, `weapons`, `props`, and raw `animations`. Particle systems live in `particles/` (separate `weapons` vs `impacts`). Prefabs mirror gameplay concepts—`player/`, `equipment/`, `game_modes/`, `hud/`, `effects/`. Scenes sit in `scenes/`, with top-level menu scenes and a `maps/` child for playable environments. Round things out with dedicated folders for `shaders`, `sounds` (further divided into `weapons`, `player`, `ui`, `music`), `surfaces`, `ui` imagery (`icons`, `weapons`, `crosshairs`), and per-weapon data in `weapons/AK47`, `weapons/Pistol`, etc., each containing its `.equip`, view model prefab, and world model prefab.
- Under `Code/`, organize gameplay logic. Keep `Assembly.cs` at the root for shared usings. `GameLoop/` owns `GameMode.cs`, `GameNetworkManager.cs`, `GameUtils.cs`, plus subfolders: `Rules/` (Events, FreezeTime, RoundLimit, TeamAssigner, and nested `Scoring/`, `Spawning/`, `Defuse/` modules), `Bots/` (BotController, BaseBotBehavior, and behavior-tree `Nodes/`), and `Globals/` for match-wide state. `PawnSystem/` stores `Pawn.cs`, persistent `PlayerClient.cs`, and a `Player/` folder (character controller, inventory, camera, crosshair, and `Damage/` logic). `Weapons/` contains `Equipment.cs`, `EquipmentResource.cs`, `ViewModel.cs`, `WorldModel.cs`, a `Components/` suite (Shootable, Reloadable, Aimable, Throwable, Melee), and grenade-specific logic in `Grenades/`. `UI/` includes `HUD/` (MainHUD, KillFeed, Scoreboard, BuyMenu, plus reusable `Components/`), `MenuSystem/`, `Minimap/`, and shared `Styles/`. Keep world-interaction code in `World/` (Zone, Door, PlayArea), map helpers in `Maps/` (MapInformation), persistence in `Stats/`, configurable knobs inside `Settings/` (GameSettingsSystem), and generic helpers in `Utils/`.
- `Editor/` is reserved for editor-only tooling, typically grouped under `Tools/`.
- `Libraries/` holds third-party dependencies such as `facepunch.libevents/`.
- `ProjectSettings/` captures S&box configuration files like `Collision.config`, `Input.config`, and `Mixer.config`.

### Key Folder Responsibilities

- **GameLoop** – central authority for rules, modes, and shared game state.
- **Rules** – opt-in behaviors (economy, scoring, spawning) that compose a mode.
- **PawnSystem** – player and AI movement, inventory, and first-person feel.
- **Weapons** – all firearm, melee, and throwable logic plus supporting data.
- **Components** – weapon behavior mix-ins (shooting, reloading, aiming, etc.).
- **HUD** – runtime UI exposed through Razor files.
- **game_modes (Assets)** – prefabs defining rule sets playable by the engine.
- **weapons (Assets)** – asset bundles per weapon (resource, view model, world model).

### File Naming Conventions

- Code files: `PascalCase.cs` (for example `PlayerInventory.cs`).
- Razor components: `PascalCase.razor` (for example `KillFeed.razor`).
- Stylesheets: `PascalCase.razor.scss` (for example `KillFeed.razor.scss`).
- Prefabs: `snake_case.prefab` (for example `player_pawn.prefab`).
- Equipment resources: `snake_case.equip` (for example `ak47.equip`).
- Scenes: `snake_case.scene` (for example `dm_arena.scene`).

### Minimal Playable Starter

To hot-load a prototype, create eight essentials:

1. `Code/GameLoop/GameMode.cs` – primary game controller.
2. `Code/GameLoop/GameNetworkManager.cs` – networking bootstrap.
3. `Code/PawnSystem/Pawn.cs` – base pawn implementation.
4. `Code/PawnSystem/PlayerClient.cs` – persistent player state.
5. `Code/UI/HUD/MainHUD.razor` – simple in-game HUD container.
6. `Assets/prefabs/player/player.prefab` – networked player pawn asset.
7. `Assets/prefabs/game_modes/deathmatch.prefab` – default game-mode prefab.
8. `Assets/scenes/test.scene` (paired with `ProjectSettings/Input.config`) – launchable scene plus bindings.

These assets and scripts are sufficient for hot-reload driven iteration.

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
- **Base Addon**: Prefer referencing native base addon sources (Steam install `...\sbox\addons\base`) when matching engine/base behavior.
- **Global usings**: `Sandbox`, `System.Collections.Generic`, `System.Linq` ([Code/Assembly.cs](Code/Assembly.cs))

## MCP Servers

Use these MCP servers when relevant to the task:

- **Exa** – Web search and code context retrieval for current APIs and SDKs.
- **Ref** – Documentation lookup and deep reads for library/framework guidance.
- **Context7** – Up-to-date library docs and examples for specific packages.
- **S&box** – S&box-specific context and tooling (engine/API references).

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
| [sbox-gamemode-dev](.github/skills/sbox-gamemode-dev/) | Beginner-friendly gamemode architecture | Learning S&box, Client/Pawn split, event-driven rules, weapons, bots |
| [sbox-sample-patterns](.github/skills/sbox-sample-patterns/) | Patterns from Facepunch sample gamemodes (prefabs, scenes, UI, spawn flow) | Building Basebound with reference to sample game structures |
| [sbox-base-addon-references](.github/skills/sbox-base-addon-references/) | Locate native S&box base addon sources for code, prefabs, scenes, and UI | Referencing canonical base addon patterns or APIs |


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
