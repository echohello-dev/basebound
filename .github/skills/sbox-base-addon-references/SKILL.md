---
name: sbox-base-addon-references
description: Locate and reference native S&box base addon sources (code, prefabs, scenes, Razor, SCSS, CS) in the Steam install. Use when matching engine/base behavior or discovering canonical patterns.
metadata:
  author: basebound
  version: "1.0"
---

# S&box Base Addon References

## When to Use
- You need canonical examples of S&box gameplay, UI, or networking behavior.
- You want to mirror how the base addon structures code, prefabs, scenes, Razor, or SCSS.
- You need to confirm naming, file layout, or component usage from the native base implementation.

## What This Skill Does
Guides you to the native **base addon** files that ship with S&box so you can reference them (read-only) for implementation details and patterns.

## Key Locations (Read-Only)
- **Windows (default Steam path)**
  - `C:\Program Files (x86)\Steam\steamapps\common\sbox\addons\base\`
- **Alternate Steam libraries**
  - `...\SteamLibrary\steamapps\common\sbox\addons\base\`

### Common Subfolders
- Code: `addons\base\Code\` (C# `.cs`)
- UI: `addons\base\Code\UI\` (Razor `.razor`, styles `.razor.scss`)
- Assets: `addons\base\Assets\` (prefabs, scenes, materials)

## Step-by-Step
1. **Locate the base addon root** under your Steam library.
2. **Pick the artifact type** you need:
   - Code: `.cs`
   - UI: `.razor`, `.razor.scss`
   - Prefabs: `.prefab`
   - Scenes: `.scene`
3. **Search for a matching name or subsystem** (e.g., “Network”, “HUD”, “Player”).
4. **Compare patterns** (properties, components, lifecycle usage, RPC usage, UI binding).
5. **Apply the pattern locally** in Basebound, adapting to project conventions.

## Example Targets (Verified)
- Networking helper: `Code\Components\Networking\NetworkHelper.cs`
- Animation helper: `Code\Components\Citizen\CitizenAnimationHelper.cs`
- UI layout: `Code\UI\MenuPanel.razor`
- UI styling: `Code\UI\MenuPanel.razor.scss`
- UI controls: `Code\UI\Controls\SliderControl.razor`
- UI control styling: `Code\UI\Controls\SliderControl.razor.scss`
- Prefab effect: `Assets\prefabs\engine\explosion_med.prefab`
- Prefab effect: `Assets\prefabs\engine\ignite.prefab`
- Prefab surface FX: `Assets\prefabs\surface\metal_bullet.prefab`
- Prefab template: `Assets\templates\gameobject\Player Controller.prefab`
- UI screen template: `Assets\templates\gameobject\UI - Screen.prefab`
- Scene files: no `.scene` files found in the base addon `Assets` folder in the default install; check other addons if you need scene references.

## Quick Reference
| Need | Look Under |
|------|------------|
| C# component example | `Code\` |
| UI layout | `Code\UI\*.razor` |
| UI styling | `Code\UI\*.razor.scss` |
| Prefab structure | `Assets\prefabs\` |
| Scene setup | `Assets\scenes\` |

## Common Pitfalls
- **Editing base addon files**: treat them as read-only references.
- **Hard-copying without adapting**: base addon patterns may assume different project context.
- **Missing Steam library path**: check non-default SteamLibrary locations.

## Notes
- Use base addon sources as a **reference** for patterns and APIs, not as a dependency.
- Prefer native base addon behavior when matching engine defaults.
