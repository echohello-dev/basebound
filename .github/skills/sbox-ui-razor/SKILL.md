---
name: sbox-ui-razor
description: Create UI in S&box using Razor (HTML/CSS/C# hybrid system). Use when building HUDs, menus, healthbars, inventory systems, or any in-game UI. Covers ScreenPanel vs WorldPanel, data binding, state management, BuildHash optimization, and interactive elements.
metadata:
  author: basebound
  version: "1.0"
  source: "S&box UI Tutorial 2024"
compatibility: S&box (Facepunch), .NET 10, C# 14, Razor
---

# S&box UI with Razor

Build interactive UI in S&box using Razor, a hybrid system that blends HTML/CSS structure with C# logic.

## When to Use

- Creating HUDs (health bars, ammo counters, minimaps)
- Building menus (pause, settings, inventory)
- Implementing interactive UI elements
- Designing in-world screens (terminals, billboards)
- Adding dynamic text/icons that update with game state

## Core Concepts

### Razor System
Razor files (`.razor`) combine:
- **HTML** for structure
- **SCSS/CSS** for styling (`.scss` file)
- **C#** for logic (using `@` symbol)

### Panel Types
- **Screen Panel** - Overlay HUD (2D, always faces camera)
- **World Panel** - In-world interactive screen (3D space)

## Project Setup

### Creating a Razor Component

1. Create a **Razor Panel Component** (not a regular component)
2. This generates two files:
   - `MyUI.razor` - HTML structure + C# logic
   - `MyUI.razor.scss` - Styling

### Scene Integration

Add a panel to your scene:
- **For HUD**: Create **Screen Panel** in Scene hierarchy
- **For 3D UI**: Create **World Panel** and position in world

```
Scene
└── Screen Panel
    └── My UI Component (attach your .razor component here)
```

## Data Binding Basics

Use `@` to execute C# code in HTML:

```razor
<label>@player.Health</label>
<div>Coins: @coins</div>
<label>@(Health / MaxHealth * 100)%</label>
```

## State Management & Optimization

### BuildHash - Critical for Performance

Override `BuildHash` to tell S&box when to redraw UI. If hash unchanged, UI doesn't update (saves performance).

```csharp
protected override int BuildHash()
{
    return System.HashCode.Combine(player.Health, player.Coins, timer);
}
```

**Rule**: Include ALL dynamic variables displayed in UI.

### Update Methods

| Context | Method | Use Case |
|---------|--------|----------|
| `PanelComponent` | `OnUpdate()` | UI components attached to GameObjects |
| Standalone `Panel` | `Tick()` | Custom panel classes |

```csharp
// In a PanelComponent
protected override void OnUpdate()
{
    // Update UI state every frame
}

// In a standalone Panel class
public override void Tick()
{
    // Update UI state every frame
}
```

## UI Elements

### Vitals (Coins, Timer, Score)

```razor
<div class="vitals">
    <label class="coins">@player.Coins</label>
    <label class="timer">@FormatTime(timer)</label>
</div>
```

```scss
.vitals {
    display: flex;
    flex-direction: column;
    gap: 8px;
    
    label {
        font-size: 24px;
        text-stroke-size: 2px;
        text-stroke-color: black;
    }
}
```

### Dynamic Health Bar

**Method 1: Inline Style (Razor)**

```razor
<div class="healthbar">
    <div class="fill" style="width: @(HealthPercent)%;"></div>
</div>
```

```csharp
float HealthPercent => (player.Health / player.MaxHealth) * 100f;

protected override int BuildHash()
{
    return System.HashCode.Combine(player.Health);
}
```

**Method 2: C# Reference (More Control)**

```razor
<div class="healthbar">
    <div @ref="HealthFill" class="fill"></div>
</div>
```

```csharp
Panel HealthFill { get; set; }

protected override void OnUpdate()
{
    var healthPercent = (player.Health / player.MaxHealth) * 100f;
    HealthFill.Style.Width = Length.Percent(healthPercent);
}
```

**Smooth Transitions:**

```scss
.fill {
    transition: all 0.2s ease; // Smooth drain animation
    background-color: #ff0000;
}
```

### Inventory / Hotbar

Use loops to generate slots dynamically:

```razor
<div class="hotbar">
    @foreach (var item in inventory.Items)
    {
        <div class="slot @(item.IsActive ? "active" : "")">
            <img src="@item.IconPath" />
            <label>@item.Count</label>
        </div>
    }
</div>
```

```scss
.hotbar {
    display: flex;
    gap: 4px;
    
    .slot {
        width: 64px;
        height: 64px;
        background-color: rgba(0, 0, 0, 0.5);
        
        &.active {
            border: 2px solid yellow;
        }
    }
}
```

### Pause Menu

**Toggle Visibility:**

```razor
<div class="pause-menu @(IsPaused ? "" : "hide")">
    <h1>Paused</h1>
    <button onclick="@Resume">Resume</button>
    <button onclick="@Quit">Quit</button>
</div>
```

```csharp
bool IsPaused { get; set; } = false;

protected override void OnUpdate()
{
    if (Input.Pressed("Menu"))
    {
        IsPaused = !IsPaused;
        Scene.TimeScale = IsPaused ? 0 : 1; // Freeze game time
    }
}

void Resume()
{
    IsPaused = false;
    Scene.TimeScale = 1;
}

void Quit()
{
    Game.Close();
}
```

```scss
.pause-menu {
    opacity: 1;
    transition: opacity 0.2s ease;
    
    &.hide {
        opacity: 0;
        pointer-events: none; // Disable clicks when hidden
    }
}

button {
    &:hover {
        sound-in: "ui.button.over"; // Play sound on hover
    }
    
    &:active {
        sound-in: "ui.button.click";
    }
}
```

## Interactive Elements

### Button Click Events

```razor
<button onclick="@MyFunction">Click Me</button>
<button onclick="@(() => DoSomething(5))">With Parameter</button>
```

```csharp
void MyFunction()
{
    Log.Info("Button clicked!");
}

void DoSomething(int value)
{
    Log.Info($"Value: {value}");
}
```

### CSS Audio

Play sounds directly from CSS:

```scss
button {
    &:hover {
        sound-in: "ui.button.over";
    }
    
    &:active {
        sound-in: "ui.button.click";
    }
}
```

## Modular Components

Break UI into reusable components:

**HealthBars.razor:**

```razor
@using Sandbox;
@inherits PanelComponent

<div class="healthbars">
    <div class="health">
        <div class="fill" style="width: @HealthPercent%;"></div>
    </div>
    <div class="armor">
        <div class="fill" style="width: @ArmorPercent%;"></div>
    </div>
</div>

@code {
    [Property] public Player Player { get; set; }
    
    float HealthPercent => (Player.Health / Player.MaxHealth) * 100f;
    float ArmorPercent => (Player.Armor / Player.MaxArmor) * 100f;
    
    protected override int BuildHash()
    {
        return System.HashCode.Combine(Player.Health, Player.Armor);
    }
}
```

**MainHUD.razor:**

```razor
@using Sandbox;
@inherits PanelComponent

<div class="hud">
    <HealthBars Player="@player" />
    <Vitals Player="@player" />
</div>

@code {
    Player player => Scene.GetAllComponents<Player>().FirstOrDefault();
}
```

## Practical Example: Complete HUD

```razor
@using Sandbox;
@inherits PanelComponent

<div class="hud">
    <div class="vitals">
        <label class="coins">💰 @player.Coins</label>
        <label class="timer">⏱️ @FormatTime(timer)</label>
    </div>
    
    <div class="healthbar">
        <div class="fill" style="width: @HealthPercent%;"></div>
    </div>
    
    <div class="hotbar">
        @for (int i = 0; i < inventory.SlotCount; i++)
        {
            var item = inventory.GetSlot(i);
            <div class="slot @(i == inventory.ActiveSlot ? "active" : "")">
                @if (item != null)
                {
                    <img src="@item.Icon" />
                    <label>@item.Count</label>
                }
            </div>
        }
    </div>
</div>

@code {
    Player player => Scene.GetAllComponents<Player>().FirstOrDefault();
    Inventory inventory => player?.Inventory;
    float timer = 0f;
    
    float HealthPercent => (player.Health / player.MaxHealth) * 100f;
    
    protected override void OnUpdate()
    {
        timer += Time.Delta;
    }
    
    protected override int BuildHash()
    {
        return System.HashCode.Combine(
            player.Health,
            player.Coins,
            inventory.ActiveSlot,
            (int)timer
        );
    }
    
    string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }
}
```

**Styling (HUD.razor.scss):**

```scss
.hud {
    position: absolute;
    width: 100%;
    height: 100%;
    pointer-events: none;
    
    .vitals {
        position: absolute;
        top: 20px;
        right: 20px;
        display: flex;
        flex-direction: column;
        gap: 8px;
        
        label {
            font-size: 24px;
            font-weight: bold;
            text-stroke-size: 2px;
            text-stroke-color: black;
        }
    }
    
    .healthbar {
        position: absolute;
        bottom: 40px;
        left: 20px;
        width: 300px;
        height: 30px;
        background-color: rgba(0, 0, 0, 0.5);
        
        .fill {
            height: 100%;
            background-color: #ff0000;
            transition: width 0.2s ease;
        }
    }
    
    .hotbar {
        position: absolute;
        bottom: 80px;
        left: 50%;
        transform: translateX(-50%);
        display: flex;
        gap: 4px;
        
        .slot {
            width: 64px;
            height: 64px;
            background-color: rgba(0, 0, 0, 0.7);
            border: 2px solid rgba(255, 255, 255, 0.2);
            pointer-events: all;
            
            &.active {
                border-color: yellow;
                box-shadow: 0 0 10px yellow;
            }
            
            img {
                width: 100%;
                height: 100%;
            }
            
            label {
                position: absolute;
                bottom: 2px;
                right: 2px;
                font-size: 14px;
                text-stroke-size: 1px;
                text-stroke-color: black;
            }
        }
    }
}
```

## Common Pitfalls

### Missing BuildHash Variables
```csharp
// ❌ UI won't update when health changes
protected override int BuildHash()
{
    return 0; // Static hash!
}

// ✅ UI updates when health changes
protected override int BuildHash()
{
    return System.HashCode.Combine(player.Health);
}
```

### Wrong Update Method
```csharp
// ❌ In PanelComponent
public override void Tick() { } // Won't be called!

// ✅ In PanelComponent
protected override void OnUpdate() { }
```

### Pointer Events
```scss
// ❌ Can't click buttons, UI blocks game interaction
.hud {
    pointer-events: all; // Everything blocks clicks
}

// ✅ Only buttons are clickable
.hud {
    pointer-events: none;
    
    button {
        pointer-events: all; // Only this is clickable
    }
}
```

### Null Reference
```razor
<!-- ❌ Crashes if player is null -->
<label>@player.Health</label>

<!-- ✅ Safe null check -->
<label>@(player?.Health ?? 0)</label>
```

## CSS Properties Reference

### Common Properties

| Property | Example | Use |
|----------|---------|-----|
| `position` | `absolute`, `relative` | Layout control |
| `display` | `flex`, `block`, `none` | Display mode |
| `flex-direction` | `row`, `column` | Flex layout |
| `gap` | `8px` | Spacing between items |
| `transition` | `all 0.2s ease` | Smooth animations |
| `opacity` | `0` to `1` | Transparency |
| `pointer-events` | `none`, `all` | Click handling |
| `text-stroke-size` | `2px` | Text outline width |
| `text-stroke-color` | `black` | Text outline color |
| `sound-in` | `"ui.click"` | Play sound on enter |
| `sound-out` | `"ui.hover"` | Play sound on exit |

### Positioning

```scss
.element {
    position: absolute;
    top: 20px;    // Distance from top
    left: 20px;   // Distance from left
    right: 20px;  // Distance from right
    bottom: 20px; // Distance from bottom
    
    // Center horizontally
    left: 50%;
    transform: translateX(-50%);
}
```

## Workflow

1. **Create Razor Component** - Generates `.razor` and `.scss` files
2. **Add to Scene** - Create Screen Panel or World Panel
3. **Structure HTML** - Use `<div>`, `<label>`, `<button>`, etc.
4. **Add Data Binding** - Use `@` for C# expressions
5. **Implement BuildHash** - Include all dynamic variables
6. **Style with SCSS** - Use flexbox, positioning, transitions
7. **Add Interactivity** - Use `onclick` for buttons
8. **Test Performance** - Verify BuildHash prevents unnecessary redraws

## Resources

- **S&box Docs**: [Razor UI Tutorial](https://docs.facepunch.com/s/sbox/v/devel/tutorials/ui/razor)
- **Style Properties**: [Supported CSS Properties](https://docs.facepunch.com/s/sbox-dev/doc/ui-style-properties)
- **Examples**: Check existing `.razor` files in S&box projects
- **Video Tutorial**: S&box UI Development 2024 (comprehensive walkthrough)

## Quick Reference

```razor
@using Sandbox;
@inherits PanelComponent

<!-- HTML Structure -->
<div class="my-ui">
    <label>@myValue</label>
    <button onclick="@MyMethod">Click</button>
</div>

@code {
    // C# Logic
    [Property] public int MyValue { get; set; }
    
    protected override void OnUpdate()
    {
        // Update logic
    }
    
    protected override int BuildHash()
    {
        return System.HashCode.Combine(MyValue);
    }
    
    void MyMethod()
    {
        Log.Info("Clicked!");
    }
}
```

```scss
// SCSS Styling
.my-ui {
    position: absolute;
    display: flex;
    flex-direction: column;
    
    label {
        font-size: 24px;
    }
    
    button {
        &:hover {
            sound-in: "ui.hover";
        }
    }
}
```
