---
name: component-attributes
description: Modify S&box Component properties using Property Attributes to customize inspector display. Use when creating components, enhancing inspector UX, organizing properties with headers/groups, adding constraints with Range, or conditionally showing fields.
metadata:
  author: basebound
  version: "1.0"
  source: "Carson Kompon - 20 S&box Attributes (https://www.youtube.com/watch?v=gY5PgW5pH90)"
compatibility: S&box (Facepunch), .NET 10, C# 14
---

# S&box Component Attributes

Enhance S&box Component properties with attributes to control inspector appearance and behavior.

## When to Use

- Creating new components with configurable properties
- Improving existing component inspector UX
- Organizing related properties into groups
- Adding value constraints (ranges, conditional display)
- Marking runtime state as read-only
- Creating debug/advanced feature tabs

## Core Concept

Properties with `[Property]` appear in the inspector. Additional attributes modify display:

```csharp
public sealed class MyComponent : Component
{
    [Property] 
    public string Basic { get; set; }
    
    [Property, Title("HP"), Range(0, 100), Icon("favorite")]
    public int Health { get; set; }
}
```

## Essential Attributes Quick Reference

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Header("Text")]` | Section label | `[Property, Header("Stats")]` |
| `[Group("Name")]` | Collapsible group | `[Property, Group("Rendering")]` |
| `[Range(min, max)]` | Slider control | `[Property, Range(0, 100)]` |
| `[Title("Text")]` | Custom label | `[Property, Title("HP")]` |
| `[Description("Text")]` | Help text | `[Property, Description("Max health")]` |
| `[Icon("name")]` | Visual marker | `[Property, Icon("favorite")]` |
| `[ReadOnly]` | Non-editable | `[Property, ReadOnly]` |
| `[ShowIf("prop", val)]` | Conditional show | `[Property, ShowIf("UseCustom", true)]` |
| `[HideIf("prop", val)]` | Conditional hide | `[Property, HideIf("AutoMode", true)]` |

## Organization Patterns

### Header Sections
Group related properties under labels:

```csharp
[Property, Header("Player Stats")]
public int MaxHealth { get; set; } = 100;
[Property]
public int CurrentHealth { get; set; } = 100;

[Property, Header("Economy")]
public long Currency { get; set; } = 1000;
```

### Collapsible Groups
Create foldable sections:

```csharp
[Property, Group("Rendering")]
public Color Color { get; set; }
[Property, Group("Rendering")]
public bool CastShadows { get; set; }
```

### Toggle Groups
Enable/disable groups with a checkbox:

```csharp
[Property]
public bool UseAdvancedSettings { get; set; } = false;

[Property, ToggleGroup("UseAdvancedSettings")]
public float AdvancedValue { get; set; }
```

### Feature Tabs
Separate advanced/debug features:

```csharp
[Property, Feature("Debug")]
public bool ShowDebugInfo { get; set; }

[Property] // Goes to "General" tab
public string Name { get; set; }
```

## Value Constraints

### Range Sliders
Constrain numeric values:

```csharp
[Property, Range(0, 100)]
public float Health { get; set; } = 100f;

// Allow manual input outside range
[Property, Range(0, 100, clamped: false)]
public int Value { get; set; }

// Number input instead of slider
[Property, Range(0, 100, slider: false)]
public int Precise { get; set; }
```

### Step Increment
Snap to specific increments:

```csharp
[Property, Step(10)]
public int RoundedValue { get; set; } = 50; // Increments by 10
```

## Conditional Display

### Show/Hide Based on Other Properties

```csharp
[Property]
public bool UseCustomSpeed { get; set; } = false;

[Property, ShowIf("UseCustomSpeed", true)]
public float CustomSpeed { get; set; } = 5f;

[Property, HideIf("AutoCalculate", true)]
public float ManualValue { get; set; }
```

## Display Enhancement

### Custom Labels and Icons

```csharp
[Property, Title("HP")]
public int Health { get; set; }

[Property, Icon("favorite"), Title("Health Points")]
public int MaxHealth { get; set; }
```

### Descriptions and Help Text

```csharp
[Property, Description("The maximum health this player can have")]
public int MaxHealth { get; set; } = 100;
```

### Read-Only Runtime State

```csharp
[Property, ReadOnly]
public int ContractsCompleted { get; set; } = 0;

[Property, ReadOnly, Icon("trending_up")]
public long TotalEarnings { get; set; } = 0;
```

## Practical Example: Health Component

```csharp
public sealed class HealthComponent : Component
{
    [Property, Header("Health Settings"), Range(1, 1000), Icon("favorite")]
    public int MaxHealth { get; set; } = 100;

    [Property, ReadOnly, Icon("favorite_border")]
    public int CurrentHealth { get; set; } = 100;

    [Property, Space, Header("Regeneration")]
    public bool EnableRegeneration { get; set; } = true;

    [Property, ShowIf("EnableRegeneration", true), Range(0, 100)]
    public float RegenPerSecond { get; set; } = 5f;

    protected override void OnStart()
    {
        CurrentHealth = MaxHealth;
    }

    protected override void OnFixedUpdate()
    {
        if (EnableRegeneration && CurrentHealth < MaxHealth)
        {
            CurrentHealth = Math.Min(
                CurrentHealth + (int)(RegenPerSecond / 50f), 
                MaxHealth
            );
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(CurrentHealth - damage, 0);
    }
}
```

**Benefits**:
- Range constrains health values
- ReadOnly shows runtime state without allowing edits
- ShowIf conditionally displays regen rate
- Icons add visual categorization
- Headers organize logically

## Common Pitfalls

### Forgetting `[Property]`
```csharp
// ❌ Won't appear in inspector
[Range(0, 100)]
public int Value { get; set; }

// ✅ Correct
[Property, Range(0, 100)]
public int Value { get; set; }
```

### Using `[Property]` with `[RequireComponent]`
```csharp
// ❌ [RequireComponent] doesn't need [Property]
[Property, RequireComponent]
private ModelRenderer Body { get; set; }

// ✅ Correct
[RequireComponent]
private ModelRenderer Body { get; set; }
```

### Invalid ShowIf/HideIf References
```csharp
// ❌ Property doesn't exist
[Property, ShowIf("NonExistentProp", true)]
public int Value { get; set; }

// ✅ Valid property reference
[Property]
public bool Enable { get; set; }

[Property, ShowIf("Enable", true)]
public int Value { get; set; }
```

### Overusing Groups
```csharp
// ❌ Too many nested groups
[Property, Group("A"), ToggleGroup("B"), Feature("C")]
public int Value { get; set; }

// ✅ Pick one grouping strategy
[Property, Group("Settings")]
public int Value { get; set; }
```

## All Available Attributes

### Organization
- `[Header("Text")]` - Section label
- `[Space]` - Vertical spacing
- `[Group("Name")]` - Collapsible group box
- `[ToggleGroup("BoolProperty")]` - Checkbox-enabled group
- `[Feature("TabName")]` - Separate tab
- `[FeatureEnabled("TabName")]` - Toggle tab on/off (bool property)
- `[Order(priority)]` - Custom display order

### Display
- `[Title("Text")]` - Custom label
- `[Description("Text")]` - Help text below property
- `[Icon("name")]` - Icon next to label
- `[ReadOnly]` - Visible but not editable
- `[KeyProperty]` - Primary identifier in lists
- `[InlineEditor]` - Edit nested objects inline
- `[Hide]` - Completely hidden (still serialized)

### Constraints
- `[Range(min, max)]` - Slider control with optional clamping
- `[Step(increment)]` - Drag increment value
- `[ShowIf("property", value)]` - Show if condition met
- `[HideIf("property", value)]` - Hide if condition met

### Input Controls
- `[TextArea]` - Multi-line text input
- `[ImageAssetPath]` - Image file picker
- `[FontName]` - Font dropdown
- `[InputAction]` - Input action dropdown (from Input.config)
- `[Flags]` - Multi-select enum (requires `[Flags]` on enum)

### Component
- `[RequireComponent]` - Auto-link/create component on same GameObject

## Workflow for Enhancing Components

1. **Identify categories** - Group related properties
2. **Add headers/groups** - Use `[Header]` or `[Group]`
3. **Constrain values** - Add `[Range]` where applicable
4. **Mark runtime state** - Use `[ReadOnly]` for calculated values
5. **Add context** - Use `[Title]`, `[Description]`, `[Icon]`
6. **Implement conditionals** - Use `[ShowIf]`/`[HideIf]` for contextual properties
7. **Test in inspector** - Verify layout and behavior

## Resources

- **S&box Docs**: [Property Attributes](https://docs.facepunch.com/s/sbox-dev/doc/property-attributes-8MiHNEG7tu)
- **Video Tutorial**: [20 S&box Attributes](https://www.youtube.com/watch?v=gY5PgW5pH90) by Carson Kompon
- **Custom Editors**: [Creating Custom Editors](https://sbox.game/dev/doc/editor/custom-editors/)
- **Example**: See `Code/Player.cs` for real-world usage
