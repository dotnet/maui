# .github Folder - AI Coding Assistant Instructions

This folder contains instructions and configurations for AI coding assistants working on the .NET MAUI repository.

## Quick Start: Using the PR Reviewer Agent

The PR reviewer agent conducts thorough, constructive code reviews of .NET MAUI pull requests with hands-on testing and validation.

### How to Use

#### Option 1: GitHub Copilot CLI (Local)

```bash
# Start GitHub Copilot CLI with agent support
copilot --allow-all-tools --allow-all-paths

# Invoke the pr-reviewer agent
/agent pr-reviewer

# Request a review
please review <link to PR>
```

#### Example

```bash
copilot --allow-all-tools --allow-all-paths
/agent pr-reviewer
please review https://github.com/dotnet/maui/pull/32372
```

#### Option 2: GitHub Copilot Agents (Web)

1. **Navigate to the agents tab** at https://github.com/copilot/agents

2. **Select your repository and branch** using the dropdown menus in the text box

3. **Choose your agent** from the dropdown (pr-reviewer)

4. **Enter a task** in the text box:
   - For PR reviews: `Please review this PR: https://github.com/dotnet/maui/pull/XXXXX`

5. **Click Start task** or press Return

6. **Follow the agent's progress** - The agent task will appear below the text box. Click into it to see live updates.

## What the Agent Does

Every PR review includes:

1. **Code Analysis** - Reviews code for correctness, style, and best practices
2. **Build & Deploy** - Builds the Sandbox app and deploys to simulator/emulator
3. **Real Testing** - Tests PR changes on actual devices with measurements
4. **Before/After Comparison** - Compares behavior with and without PR changes  
5. **Edge Case Testing** - Tests scenarios not mentioned by the PR author
6. **Documented Results** - Provides review with actual test data and evidence

The agent will pause and ask for help if it encounters:
- Merge conflicts when applying PR changes
- Build errors that prevent testing
- Test results that are unexpected or confusing
- Any step that prevents thorough validation

## Important Notes

⚠️ **We're still refining the agent instructions** to ensure it consistently follows the testing workflow.

**The agent will pause and ask for help if:**
- Merge conflicts occur when applying PR changes
- Build errors prevent testing
- Test results are unexpected or confusing
- Any step fails that prevents thorough validation

This is by design - it's better to pause and get guidance than provide incomplete or misleading reviews.

## File Structure

### Agent Definitions
- **`agents/pr-reviewer.md`** - Main PR reviewer agent instructions and workflows

### Instruction Files
These files provide specialized guidance for specific scenarios:

- **`instructions/common-testing-patterns.md`** - Common testing patterns for command sequences (UDID extraction, builds, deploys, error checking)
- **`instructions/uitests.instructions.md`** - UI testing guidelines (when to use HostApp vs Sandbox)
- **`instructions/safearea-testing.instructions.md`** - SafeArea testing patterns (measure children, not parents)
- **`instructions/instrumentation.instructions.md`** - Code instrumentation patterns for debugging and testing
- **`instructions/appium-control.instructions.md`** - Standalone Appium scripts for manual debugging and exploration
- **`instructions/templates.instructions.md`** - Template modification rules and conventions

### General Guidelines
- **`copilot-instructions.md`** - General coding standards, build requirements, file conventions for the entire repository

### Prompts
- **`prompts/pr-reviewer.prompt.md`** - Ready-to-use prompt templates for PR reviews

## For GitHub Copilot (General Development)

When working on code in this repository, GitHub Copilot automatically uses:
- `.github/copilot-instructions.md` - General coding standards
- `.github/instructions/*.instructions.md` - Specialized instructions based on file patterns

See `.github/copilot-instructions.md` for comprehensive development guidelines including:
- Repository structure and setup
- Build and test workflows
- Platform-specific development
- Contribution guidelines

## Instruction Precedence

When multiple instruction files exist, follow this priority order:

1. **Highest Priority**: Agent-specific instructions (`.github/agents/*.md`)
2. **Secondary**: Specialized instructions (`.github/instructions/*.instructions.md`)
3. **General Guidance**: `.github/copilot-instructions.md`

If instructions conflict, higher priority files win.

## Contributing to Instructions

When updating agent instructions or guidelines:

1. **Keep them synchronized**: Changes to agent instructions may need corresponding updates to general instructions
2. **Test the changes**: Verify the agent/assistant behaves as expected
3. **Document your changes**: Update this README if you add new instruction files
4. **Be specific**: Clear, actionable instructions work better than vague guidance

## Related Documentation

- **Repository root**: `AGENTS.md` - Universal guidance for all AI coding assistants
- **Development**: `DEVELOPMENT.md` - Development environment setup
- **Contributing**: `CONTRIBUTING.md` - Contribution guidelines
- **Wiki**: [.NET MAUI GitHub Wiki](https://github.com/dotnet/maui/wiki) - Additional resources

## Troubleshooting

### GitHub Copilot CLI Issues

**If you have errors with the CLI working or authentication issues:**

Follow these steps to reset your authentication:

```bash
# Start Copilot CLI
copilot

# Logout from current session
/logout

# Exit Copilot CLI
exit

# Re-authenticate with GitHub
gh auth login

# Start Copilot CLI again
copilot
```

This will refresh your GitHub authentication and resolve most CLI-related issues.

### Agent Issues

**Agent doesn't complete testing:**
- Check the agent's output for error messages
- The agent should pause and ask for help if it encounters issues
- If it silently stops, let us know so we can improve the instructions

**Build or test failures:**
- The agent is designed to pause and ask for help rather than proceeding
- Work with the agent to resolve the issue before continuing the review
- This ensures reviews are based on actual working code

## Support

For issues or questions about the AI agent instructions:
1. Check this README and referenced instruction files
2. Review recent PR reviews to see examples
3. Ask in the repository discussions or issues
4. Propose changes via PR to improve the instructions

---

**Last Updated**: 2025-11-06

**Note**: These instructions are actively being refined. Feedback and improvements are welcome!
