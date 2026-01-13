# Contributing to Basebound

Thank you for your interest in contributing to Basebound! We welcome contributions from the community and appreciate your help in making this project better.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions. We're building a welcoming community for all contributors.

## Getting Started

### Prerequisites

- S&box SDK (latest version)
- .NET 10 SDK
- Visual Studio, VS Code, or Rider
- Git

### Setup Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/echohello-dev/basebound.git
   cd basebound
   ```

2. **Open in S&box**
   - Launch S&box and open `basebound.sbproj`
   - The project will load with hot-reload enabled

3. **Open in IDE**
   - For C# development: Open `basebound.sln`
   - Code will auto-compile with millisecond hot-reload

4. **Test your changes**
   - Run the default scene: `Assets/scenes/minimal.scene`
   - Changes compile and reload automatically

## Development Workflow

### Branch Naming

Create branches with descriptive names:
- `feature/base-building-ui` - New features
- `fix/economy-calculation` - Bug fixes
- `refactor/contract-system` - Code refactoring
- `docs/networking-guide` - Documentation

### Code Standards

#### Component Pattern

All game behavior must use the Component pattern. Follow this structure:

```csharp
public sealed class MyGameSystem : Component
{
    [Property] public string ConfigValue { get; set; } = "default";
    [RequireComponent] private OtherComponent Dependency { get; set; }

    protected override void OnAwake() { }       // Initialization
    protected override void OnStart() { }       // First frame setup
    protected override void OnUpdate() { }      // Per-frame logic
    protected override void OnFixedUpdate() { } // Physics tick (50/sec)
    protected override void OnDestroy() { }     // Cleanup
}
```

**Key Rules:**
- Use `sealed` classes unless inheritance is required
- Expose configuration with `[Property]` attributes
- Auto-link dependencies with `[RequireComponent]`
- Keep lifecycle methods focused on their purpose

#### Networking

Always consider multiplayer when writing code:

```csharp
// Bad: Only works for local player
if (Input.Down("attack")) Fire();

// Good: Check if controlled by this client
if (!IsProxy && Input.Down("attack"))
{
    Fire();
}

// Good: Broadcast to all clients
[Rpc.Broadcast]
public void OnPlayerJump() { }
```

#### Naming Conventions

- **Classes**: `PascalCase` - `PlayerController`, `EconomySystem`
- **Methods**: `PascalCase` - `OnPlayerSpawn()`, `CalculateReward()`
- **Properties**: `PascalCase` - `Health`, `IsAlive`
- **Fields**: `camelCase` (private) - `_localCache`, `_spawnPoints`
- **Constants**: `UPPER_SNAKE_CASE` - `MAX_PLAYERS`, `TICK_RATE`

#### Documentation

- Add XML comments to public classes and methods
- Keep comments concise but descriptive
- Update README.md if adding new systems

```csharp
/// <summary>
/// Calculates the reward for a completed contract based on difficulty.
/// </summary>
/// <param name="contract">The contract to evaluate</param>
/// <returns>Reward in currency</returns>
public int CalculateReward(Contract contract) { }
```

### Commit Messages

Write clear, descriptive commit messages:

```
feature: Add economy system tax calculation

- Implemented progressive tax system
- Added configurable tax rates per bracket
- Updated networking to broadcast tax changes
```

Format: `<type>: <subject>`

Types:
- `feature` - New functionality
- `fix` - Bug fix
- `refactor` - Code restructuring
- `docs` - Documentation
- `test` - Tests or testing infrastructure

### Pull Request Process

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow code standards above
   - Test thoroughly in S&box
   - Keep commits logical and atomic

3. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   - Title: Clear, descriptive title
   - Description: Explain what and why
   - Link related issues: `Closes #123`

4. **PR Template**
   ```markdown
   ## Description
   Brief explanation of changes

   ## Type of Change
   - [ ] New feature
   - [ ] Bug fix
   - [ ] Breaking change
   - [ ] Documentation

   ## Testing
   How to test these changes

   ## Checklist
   - [ ] Code follows style guidelines
   - [ ] Self-review completed
   - [ ] Comments added for complex logic
   - [ ] Documentation updated
   - [ ] Tested in S&box
   ```

5. **Review and merge**
   - Address feedback from reviewers
   - Keep discussion professional and constructive

## Areas to Contribute

### High Priority

- Base building system enhancements
- Economy balance improvements
- Raid system refinements
- Performance optimizations

### Good for Beginners

- Documentation improvements
- Bug fixes in existing systems
- UI/UX polish
- Configuration options

### Community Requested

Check GitHub Issues for community-requested features and report bugs.

## Testing

### Manual Testing

1. **Load minimal.scene** - Basic gameplay test
2. **Test multiplayer** - Use S&box play options
3. **Test hot-reload** - Verify changes compile and apply
4. **Test networking** - Verify RPC calls work correctly

### Reporting Bugs

Include:
- Reproduction steps
- Expected behavior
- Actual behavior
- S&box version and .NET version
- Relevant code or logs

## Questions?

- **Discord**: Join the S&box/Basebound community Discord
- **Issues**: Create a discussion issue on GitHub
- **Docs**: Check https://docs.facepunch.com/s/sbox-dev

## License

By contributing, you agree that your contributions will be licensed under the same MIT License that covers the project.

---

**Thank you for contributing to Basebound!** 🎮
