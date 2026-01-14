# Copilot Review Agent Instructions

These instructions describe how the Copilot review agent should evaluate pull requests for the Basebound S&box project. Apply them in addition to the default GitHub review heuristics.

## 1. Scope Awareness

1. Confirm the change matches the linked issue or feature description. Flag unrelated edits.
2. Note the target branch (`feature/weaps` by default) and call out any incompatible changes with `main` if obvious.
3. Highlight large, multi-domain changes that may benefit from splitting or extra reviewers.

## 2. Documentation Expectations

1. Verify that modifications touching system architecture, networking, gameplay systems, or tooling reference the appropriate doc in `Docs/` per `AGENTS.md`.
2. Require updates to `Docs/architecture.md`, `Docs/networking.md`, `Docs/gameplay.md`, `Docs/setup.md`, or `Docs/code-standards.md` when code changes alter those topics.
3. Ensure README or CONTRIBUTING changes accompany user-facing workflow adjustments.

## 3. Code Quality Checklist

1. **Component Pattern** – New components should follow the lifecycle methods described in `Code/MyComponent.cs`, leverage `[Property]` for inspector configuration, and guard proxy logic for multiplayer scenarios.
2. **Networking** – RPC usage must specify the correct attribute (`[Rpc.Broadcast]`, `[Rpc.Multicast]`, etc.), avoid server/client authority violations, and prevent proxy-side input processing.
3. **Folder Placement** – Files must land in the folders outlined in `AGENTS.md` (GameLoop, PawnSystem, Weapons, UI, etc.). Flag assets or code placed outside those conventions.
4. **Naming** – Enforce PascalCase for C# files, PascalCase.razor for UI, PascalCase.razor.scss for styles, and snake_case for prefabs/resources/scenes.
5. **Null Safety & Hot Reload** – Watch for missing null checks when accessing scene components, and prefer hot-reload-safe patterns (no static mutable state unless intentional).

## 4. Gameplay & Weapons Standards

1. Gameplay rules inside `GameLoop/Rules` should remain composable; avoid hard-coded references between rule modules unless explicitly documented.
2. Weapon logic must route through `Weapons/Components` mix-ins when adding firing, aiming, reloading, or throwable behavior—call out duplicated logic.
3. Player changes in `PawnSystem/Player` should respect server authority, input prediction, and crosshair/camera synchronization.

## 5. Testing & Validation

1. Request evidence of local playtesting or automated checks for gameplay-affecting changes (movement, combat, networking).
2. For UI updates, expect screenshots or Razor preview notes when possible.
3. Ensure new features include minimal entry points (prefabs, scenes, config) so they are loadable in the editor.

## 6. Pull Request Hygiene

1. Require concise summaries referencing subsystems touched (GameLoop, PawnSystem, Weapons, UI, Docs).
2. Encourage checklist-style validation (tests run, docs updated, assets imported) in the PR description.
3. Flag generated artifacts (e.g., `*.scene_c`) unless they are intentionally versioned.

When in doubt, refer back to `AGENTS.md` and the documentation in `Docs/` for authoritative guidance. The review agent should leave actionable comments citing the relevant section (for example, “Per `Docs/code-standards.md`, weapon components should expose `[Property]` for tuning”).