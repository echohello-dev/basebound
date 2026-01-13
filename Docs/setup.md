# Setup & Getting Started

## Prerequisites

- **S&box SDK** (latest version) - https://sbox.game
- **.NET 10 SDK** - https://dotnet.microsoft.com/download/dotnet/10.0
- **IDE**: Visual Studio 2022, VS Code, or Rider
- **Git**

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/echohello-dev/basebound.git
cd basebound
```

### 2. Open in S&box

1. Launch **S&box Launcher**
2. Click "Create Project" or "Open Project"
3. Select `basebound.sbproj` from the cloned directory
4. Wait for initial load and compilation
5. The project will load with **hot-reload** enabled

### 3. Open in IDE

**For C# Development:**
- Open `basebound.sln` in Visual Studio 2022 or Rider
- Code will auto-compile with millisecond hot-reload
- Use [Code/Properties/launchSettings.json](../Code/Properties/launchSettings.json) "Editor" profile for debugging

**For other editors:**
- VS Code with C# extension works well
- Open the `Code` folder

## Running the Game

### First Run

1. S&box will load the default scene: `Assets/scenes/minimal.scene`
2. Press **Play** in the S&box editor
3. You're now running the game

### Available Scenes

- `Assets/scenes/minimal.scene` - Minimal startup scene (default)
- `Assets/construct1.scene` - Construction/base building test map
- `Assets/flatgrass1.scene` - Flat grass terrain test map

## Development Workflow

### Hot Reload

One of S&box's key features is **millisecond hot-reload**:

1. Edit C# code in your IDE
2. Save the file
3. Code compiles automatically (~100-500ms)
4. Changes apply instantly in the running game
5. No restart needed

This makes iteration extremely fast during development.

### Testing Your Changes

```bash
# Terminal in VS Code or IDE
# Make changes to code
# Save file → automatically compiles
# See changes in running S&box game

# To rebuild from scratch
sbox-dev.exe -project "path/to/basebound.sbproj"
```

### Project Files

- **basebound.sbproj** - Main project file
- **basebound.sln** - Solution for IDE
- **Code/basebound.csproj** - Runtime code project
- **Editor/basebound.editor.csproj** - Editor tools project

## File Structure

```
basebound/
├── README.md                 → Project overview
├── CONTRIBUTING.md          → Contribution guidelines
├── LICENSE                  → MIT License
├── docs/                    → Documentation
│   ├── setup.md            → This file
│   ├── architecture.md      → Component pattern & architecture
│   ├── networking.md        → Multiplayer & networking
│   ├── code-standards.md    → Naming and code conventions
│   └── gameplay.md          → Game systems (economy, raids, etc)
├── Code/                    → Runtime game logic
│   ├── Assembly.cs
│   ├── MyComponent.cs       → Component template
│   └── Properties/
├── Editor/                  → Editor-only tools
│   ├── Assembly.cs
│   ├── MyEditorMenu.cs      → Menu template
│   └── Properties/
├── Assets/                  → Scenes, models, materials
│   ├── scenes/
│   │   └── minimal.scene
│   ├── construct1.scene
│   └── flatgrass1.scene
└── ProjectSettings/         → Engine configuration
    ├── Collision.config
    └── Input.config
```

## Troubleshooting

### Project Won't Load

- Ensure .NET 10 SDK is installed: `dotnet --version`
- Clear S&box cache: Delete `AppData/Local/sbox` and reopen
- Verify S&box SDK is up to date

### Compilation Errors

- Check Output panel in VS Code or IDE
- Ensure all NuGet packages are restored: `dotnet restore`
- Try rebuilding: Right-click solution → Rebuild

### Hot Reload Not Working

- Verify code is saved
- Check for syntax errors (red squiggles in IDE)
- Restart S&box and IDE if stuck
- Hot-reload works best with small, focused changes

### Game Crashes on Load

- Check log file in S&box output
- Verify all required components are present
- Ensure no circular component dependencies

## Next Steps

1. **Read the architecture**: See [architecture.md](architecture.md)
2. **Understand components**: Review [Code/MyComponent.cs](../Code/MyComponent.cs)
3. **Check code standards**: See [code-standards.md](code-standards.md)
4. **Start contributing**: Read [CONTRIBUTING.md](../CONTRIBUTING.md)

## Resources

- **S&box Documentation**: https://docs.facepunch.com/s/sbox-dev
- **S&box API Reference**: https://sbox.game/api
- **Facepunch Discord**: https://discord.gg/sbox
- **Project Issues**: https://github.com/echohello-dev/basebound/issues
