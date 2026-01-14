---
name: agent-skill-authoring
description: Meta-skill for writing effective Agent Skills following the agentskills.io specification. Use when creating new skills, improving existing skills, or understanding how to structure procedural knowledge for AI agents.
metadata:
  author: basebound
  version: "1.0"
---

# Agent Skill Authoring

This skill teaches you how to write effective Agent Skills following the open agentskills.io specification.

## When to Use This Skill

- Creating a new skill for AI agents
- Improving an existing skill's effectiveness
- Understanding the Agent Skills format
- Packaging domain expertise for reuse

## Skill Location

All skills must be created in `.github/skills/`:
- ✅ Create: `.github/skills/my-new-skill/SKILL.md`
- ❌ Never modify: `Docs/`, `README.md`, `AGENTS.md` when working on skills
- Skills are self-contained and don't require documentation updates

## Skill Structure

Every skill is a directory with a `SKILL.md` file in `.github/skills/`:

```
.github/skills/my-skill/
├── SKILL.md          # Required: instructions + metadata
├── scripts/          # Optional: executable code
├── references/       # Optional: detailed documentation
└── assets/           # Optional: templates, resources
```

**IMPORTANT**: Skills are self-contained in `.github/skills/`. Never update project documentation files (`Docs/`, `README.md`, `AGENTS.md`) when creating or modifying skills.

## SKILL.md Format

### Required Frontmatter

```yaml
---
name: my-skill-name
description: What this skill does and when to use it. Include keywords that help agents identify relevant tasks.
---
```

**Name rules**:
- 1-64 characters
- Lowercase alphanumeric and hyphens only (`a-z`, `0-9`, `-`)
- No leading/trailing/consecutive hyphens
- Must match the parent directory name

**Description rules**:
- 1-1024 characters
- Describe WHAT it does AND WHEN to use it
- Include specific keywords for task matching

### Optional Frontmatter

```yaml
---
name: pdf-processing
description: Extract text and tables from PDF files, fill forms, merge documents.
license: Apache-2.0
compatibility: Requires Python 3.10+, poppler
metadata:
  author: your-org
  version: "1.0"
allowed-tools: Bash(python:*) Read Write
---
```

### Body Content

The Markdown body contains instructions. No format restrictions—write whatever helps agents perform the task.

**Recommended sections**:
1. When to use this skill
2. Step-by-step instructions
3. Code examples with context
4. Common pitfalls and edge cases
5. Quick reference tables

## Progressive Disclosure

Skills use progressive disclosure to manage context efficiently:

| Layer | Token Budget | When Loaded |
|-------|--------------|-------------|
| Metadata | ~100 tokens | At startup (all skills) |
| Instructions | <5000 tokens | When skill activates |
| Resources | As needed | When referenced |

**Key principle**: Keep `SKILL.md` under 500 lines. Move detailed reference material to `references/` directory.

## Writing Effective Instructions

### Do

✅ **Be specific and actionable**
```markdown
## Creating a Component
1. Inherit from `Component`
2. Add `[Property]` to exposed fields
3. Override `OnUpdate()` for frame logic
```

✅ **Include real code examples**
```markdown
```csharp
public sealed class MyComponent : Component
{
    [Property] public float Speed { get; set; }
}
```
```

✅ **Document common pitfalls**
```markdown
## Common Pitfalls
### Forgetting `IsProxy` Check
**Problem**: Input runs for all players
**Solution**: Add `if (IsProxy) return;` at the start
```

✅ **Use tables for quick reference**
```markdown
| Task | Code |
|------|------|
| Create object | `new GameObject()` |
| Clone prefab | `Prefab.Clone(pos)` |
```

### Don't

❌ Generic advice ("write clean code", "handle errors")
❌ Aspirational practices not in the codebase
❌ Long prose without structure
❌ Missing "when to use" context
❌ Deeply nested file references

## File References

Reference files with relative paths from skill root:

```markdown
See [detailed API reference](references/API.md) for more.

Run the setup script:
scripts/setup.py
```

Keep references one level deep. Avoid chains like `references/a.md` → `references/b.md` → `references/c.md`.

## Example: Well-Structured Skill

```markdown
---
name: database-migrations
description: Create and run database migrations for PostgreSQL. Use when adding/modifying database tables, fixing schema issues, or setting up new environments.
---

# Database Migrations

## When to Use
- Adding new tables or columns
- Modifying existing schema
- Rolling back problematic changes

## Creating a Migration

1. Run `./scripts/create-migration.sh <name>`
2. Edit the generated file in `migrations/`
3. Run `./scripts/migrate.sh` to apply

## Migration File Format

```sql
-- Up
ALTER TABLE users ADD COLUMN email VARCHAR(255);

-- Down
ALTER TABLE users DROP COLUMN email;
```

## Common Pitfalls

### Data Loss on Rollback
**Problem**: `DROP COLUMN` destroys data
**Solution**: Back up data before destructive migrations

## Quick Reference

| Command | Purpose |
|---------|---------|
| `migrate.sh` | Apply pending |
| `rollback.sh` | Undo last |
| `status.sh` | Show state |
```

## Validation

Use the reference library to validate skills:

```bash
npx skills-ref validate ./.github/skills/my-skill
```

This checks frontmatter validity and naming conventions.

## Important Rules

When creating or modifying skills:

1. ✅ **Do**: Create skills in `.github/skills/<skill-name>/SKILL.md`
2. ✅ **Do**: Keep skills self-contained with all needed information
3. ✅ **Do**: Include code examples and quick reference tables
4. ❌ **Don't**: Update `Docs/` files when creating skills
5. ❌ **Don't**: Update `README.md` when creating skills
6. ❌ **Don't**: Update `AGENTS.md` when creating skills
7. ❌ **Don't**: Cross-reference skills in project documentation

Skills are discovered automatically and should stand alone.

## Resources

- **Specification**: https://agentskills.io/specification
- **Examples**: https://github.com/anthropics/skills
- **Reference Library**: https://github.com/agentskills/agentskills/tree/main/skills-ref
- **Project Skills**: `.github/skills/` (view existing skills for examples)
