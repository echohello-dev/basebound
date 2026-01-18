# Code Standards

All code must follow these standards to maintain consistency and quality across the Basebound codebase.

## Component Pattern

See [architecture.md](architecture.md) for detailed component architecture. All game behavior must inherit from `Component`.

## Naming Conventions

- **Classes**: `PascalCase`
  - `PlayerController`, `EconomySystem`, `RaidManager`
- **Methods**: `PascalCase`
  - `OnPlayerSpawn()`, `CalculateReward()`, `PlaceBlock()`
- **Properties**: `PascalCase`
  - `Health`, `IsAlive`, `CurrentBalance`
- **Fields**: `_camelCase` (private)
  - `_localCache`, `_spawnPoints`, `_playerData`
- **Constants**: `UPPER_SNAKE_CASE`
  - `MAX_PLAYERS`, `TICK_RATE`, `MIN_RAID_DURATION`
- **Parameters**: `camelCase`
  - `playerCount`, `blockType`, `rewardAmount`

## Code Style

### Class Declaration

```csharp
// Use sealed unless inheritance is required
public sealed class MyGameSystem : Component
{
    // Fields first (private)
    private int _internalValue;
    
    // Properties (public, exposed to inspector)
    [Property] public string ConfigValue { get; set; } = "default";
    [Property] public int MaxCount { get; set; } = 100;
    
    // Dependencies
    [RequireComponent] private OtherComponent Dependency { get; set; }
    
    // Lifecycle methods
    protected override void OnAwake() { }
    protected override void OnStart() { }
    protected override void OnUpdate() { }
    protected override void OnFixedUpdate() { }
    protected override void OnDestroy() { }
    
    // Public methods
    public void PublicMethod() { }
    
    // Private methods
    private void PrivateHelper() { }
}
```

### Properties and Fields

- Expose configuration to inspector with `[Property]`
- Use auto-properties where possible: `public int Value { get; set; }`
- Initialize properties with meaningful defaults
- Use `[RequireComponent]` for auto-linking dependencies

### Methods

```csharp
// Keep methods focused and single-responsibility
public void ProcessPlayerAction(PlayerAction action)
{
    if (!IsValidAction(action)) return;
    ApplyAction(action);
    BroadcastUpdate(action);
}

// Use descriptive names that explain intent
private bool IsValidAction(PlayerAction action) => action != null && action.IsEnabled;

// Document public methods
/// <summary>
/// Calculates the reward for a completed contract based on difficulty and skill.
/// </summary>
/// <param name="contract">The contract to evaluate</param>
/// <returns>Reward in currency</returns>
public int CalculateReward(Contract contract)
{
    return contract.BaseDifficulty * contract.SkillMultiplier;
}
```

## Engine API Patterns (S&box)

```csharp
// Trace helpers
var trace = Scene.Trace.Ray( start, end )
    .UseHitboxes()
    .WithoutTags( "trigger", "playerclip" )
    .Run();

// Spawn + destroy
var dropped = Prefab.Clone( position, rotation );
dropped.NetworkSpawn();
GameObject.Destroy();

// Time utilities
if ( TimeSinceShoot < FireRate ) return;
var smooth = current.LerpTo( target, Time.Delta * 10f );
```

## Mechanics Checklists

### Interaction + Use

- Implement `CanUse` to gate interaction and return a `UseResult` message when blocked.
- Keep `OnUse` lightweight and delegate longer work to helpers.
- Prefer `GrabAction` to reflect contextual hand animations.

### Damage + Health

- Guard damage application with `if ( !Networking.IsHost ) return;`.
- Keep damage interval logic in `OnFixedUpdate` or trigger listeners.
- Store falloff curves and damage flags as `[Property]` for tuning.

### Audio + VFX

- Use `ResourceLibrary.Get<T>` to resolve assets by id when needed.
- Prefer `Sound.Play` with positional audio at `WorldPosition`.
- Gate effect spawns with `IsValid()` checks.

### Inventory + Equipment

- Validate `CanTake` before pickup or equip.
- Use tags for state (`reloading`, `no_shooting`, `lowered`).
- Keep owner-only actions under `[Rpc.Owner]` when needed.

### Triggers + Zones

- Use `Tags` and `Trace` filters to avoid trigger hits.
- Favor `Scene.GetAllComponents<T>()` for broad queries.
- Track trigger membership in dictionaries for periodic cleanup.

## Networking Guidelines


### Always Consider Multiplayer

```csharp
// Bad: Only works for local player, ignores proxies
if (Input.Down("attack")) Fire();

// Good: Explicitly check if controlled by this client
if (!IsProxy && Input.Down("attack"))
{
    Fire();
}

// Good: Broadcast to all clients
[Rpc.Broadcast]
public void OnPlayerJump() => PlayJumpAnimation();
```

### RPC Patterns

```csharp
// Server-side validation before broadcasting
[Rpc.Broadcast]
public void PlaceBlock(Vector3 position, int blockType)
{
    if (!IsProxy) // Only server validates
    {
        if (!IsValidPlacement(position, blockType)) return;
        DeductCurrency(BLOCK_COST);
    }
    
    // All clients execute this to update their view
    SpawnBlockVisual(position, blockType);
}
```

## Documentation

### XML Comments

Add XML comments to all public classes and methods:

```csharp
/// <summary>
/// Manages player economy including currency and transactions.
/// </summary>
public sealed class EconomySystem : Component
{
    /// <summary>
    /// Gets the current balance for a player.
    /// </summary>
    /// <param name="playerId">The player's unique identifier</param>
    /// <returns>Current balance in currency units</returns>
    public int GetBalance(int playerId) { }
}
```

### Code Comments

- Keep comments concise and explain *why*, not *what*
- Good comment: `// Skip proxy objects to avoid duplicate processing`
- Avoid obvious comments: `// Increment counter` (this is clear from code)

### Update Documentation

When adding new systems or significant changes:
- Update [architecture.md](architecture.md) if changing component structure
- Update [networking.md](networking.md) if adding networked features
- Update [CONTRIBUTING.md](../CONTRIBUTING.md) for development workflow changes
- Keep README.md overview current

## Error Handling

```csharp
// Validate inputs early
public void SetPlayerLimit(int limit)
{
    if (limit < 1 || limit > MAX_PLAYERS)
    {
        Log.Error($"Invalid player limit: {limit}");
        return;
    }
    
    _playerLimit = limit;
}

// Use guard clauses for early returns
protected override void OnFixedUpdate()
{
    if (IsProxy) return;
    if (!IsEnabled) return;
    if (Players.Count == 0) return;
    
    // Main logic here
}
```

## Performance Considerations

```csharp
// Cache expensive lookups
private GameObject[] _cachedPlayers;

protected override void OnStart()
{
    _cachedPlayers = GetAllPlayers(); // Cache once
}

// Avoid LINQ in hot paths (OnFixedUpdate, OnUpdate)
// Bad in OnFixedUpdate:
foreach (var player in Players.Where(p => p.IsAlive))

// Good in OnFixedUpdate:
foreach (var player in _cachedPlayers)
{
    if (!player.IsAlive) continue;
}

// Use appropriate collections
// Dictionary for O(1) lookups: playerId -> PlayerData
// List for O(n) iteration: players to update each frame
```

## Version Control

See [CONTRIBUTING.md](../CONTRIBUTING.md#commit-messages) for commit message guidelines.

## Related Documentation

- [CONTRIBUTING.md](../CONTRIBUTING.md) - Full contribution guidelines
- [architecture.md](architecture.md) - Component pattern details
- [networking.md](networking.md) - Multiplayer implementation
