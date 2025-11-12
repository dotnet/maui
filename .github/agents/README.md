# MAUI Agent Configuration

This directory contains agent configuration files for AI coding assistants working with the .NET MAUI repository.

## Available Agents

### 1. C# Expert (`my-agent.agent.md`)

General-purpose agent for .NET development. Provides guidance on:
- General C# conventions and best practices
- Async programming patterns
- Error handling and edge cases
- Testing with xUnit, NUnit, and MSTest
- SOLID principles and design patterns

**Use when:** Working on general .NET code, utilities, or when MAUI-specific patterns don't apply.

### 2. MAUI C# Expert (`maui-csharp-expert.agent.md`) ⭐ RECOMMENDED

Specialized agent for .NET MAUI development. Extends C# Expert with MAUI-specific guidance:
- Platform-specific code patterns (file extensions, conditional compilation)
- Handler pattern and architecture
- Interface requirements for controls and handlers
- Two-project UI testing pattern (HostApp + Test)
- Mobile performance considerations
- Platform threading and async requirements

**Use when:** Working on MAUI controls, handlers, UI tests, or platform-specific code.

### 3. PR Reviewer (`pr-reviewer.md`)

Agent for conducting pull request reviews.

**Use when:** Reviewing pull requests submitted to the repository.

## Analysis Documents

### `csharp-expert-maui-analysis.md`

Comprehensive analysis of how generic C# Expert guidance applies to MAUI:
- Identifies areas requiring modification (testing frameworks, platform patterns)
- Documents MAUI-specific patterns not covered by generic guidance
- Provides code examples and recommendations
- Serves as the foundation for the MAUI C# Expert agent

## Choosing the Right Agent

```
┌─────────────────────────────────────────┐
│ What are you working on?                │
└─────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
   MAUI-specific?          General .NET?
        │                       │
        ↓                       ↓
┌──────────────────┐    ┌──────────────┐
│ MAUI C# Expert   │    │  C# Expert   │
│                  │    │              │
│ • Controls       │    │ • Utilities  │
│ • Handlers       │    │ • Helpers    │
│ • UI Tests       │    │ • Generic    │
│ • Platform Code  │    │   libraries  │
└──────────────────┘    └──────────────┘
```

## Integration with Repository Instructions

Agents work alongside existing instruction files:

- `.github/copilot-instructions.md` - GitHub Copilot-specific guidance
- `.github/instructions/*.instructions.md` - Path-specific instructions
- `AGENTS.md` - Universal AI assistant guidance (repository root)

**Precedence order:**
1. Path-specific instructions (most specific)
2. Agent files (task/domain-specific)
3. Copilot instructions (tool-specific)
4. AGENTS.md (universal baseline)

## Contributing

When updating agents:

1. **Maintain synchronization** with related instruction files
2. **Test changes** by using the agent for actual development tasks
3. **Document examples** of when the agent applies vs doesn't
4. **Keep agents focused** - don't duplicate general guidance unnecessarily

## Version History

- **2025-11-12a**: Added MAUI C# Expert agent based on codebase analysis
- **2025-10-27a**: Added C# Expert agent (generic .NET guidance)

## Questions?

See `.github/copilot-instructions.md` for development environment setup and workflow guidance.
