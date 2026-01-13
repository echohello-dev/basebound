# Contributing to Basebound

Thank you for your interest in contributing to Basebound! We welcome contributions from the community and appreciate your help in making this project better.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions. We're building a welcoming community for all contributors.

## Getting Started

👉 **New contributor?** Start with [Setup & Getting Started](docs/setup.md) first!

- Prerequisites and project setup
- Development workflow and hot-reload
- Troubleshooting common issues

## Development Workflow

### Branch Naming

Create branches with descriptive names:
- `feature/base-building-ui` - New features
- `fix/economy-calculation` - Bug fixes
- `refactor/contract-system` - Code refactoring
- `docs/networking-guide` - Documentation

### Code Standards

👉 **See [Code Standards](docs/code-standards.md)** for detailed guidelines on:
- Naming conventions (classes, methods, properties, fields)
- Component pattern and best practices
- Networking patterns and RPC usage
- Error handling and performance
- Documentation standards

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

## Documentation

When adding features or making significant changes, update relevant documentation:

- **New architecture or structure changes** → Update [docs/architecture.md](docs/architecture.md)
- **New networking features** → Update [docs/networking.md](docs/networking.md)
- **New game systems** → Update [docs/gameplay.md](docs/gameplay.md)
- **Setup or tooling changes** → Update [docs/setup.md](docs/setup.md)
- **Code style changes** → Update [docs/code-standards.md](docs/code-standards.md)
- **Project overview changes** → Update [README.md](README.md)

## Questions?

- **Discord**: Join the S&box/Basebound community Discord
- **Issues**: Create a discussion issue on GitHub
- **Docs**: Check https://docs.facepunch.com/s/sbox-dev

## License

By contributing, you agree that your contributions will be licensed under the same MIT License that covers the project.

---

**Thank you for contributing to Basebound!** 🎮
