# .github Folder - AI Coding Assistant Instructions

This folder contains instructions and configurations for AI coding assistants working on the .NET MAUI repository.

## Available Agents

### PR Reviewer Agent
The PR reviewer agent conducts thorough, constructive code reviews of .NET MAUI pull requests with hands-on testing and validation.

### Issue Resolver Agent
The issue resolver agent investigates, reproduces, and fixes reported issues in the .NET MAUI repository with comprehensive testing and validation.

## How to Use

### Option 1: GitHub Copilot CLI (Local)

**PR Reviewer Agent:**
```bash
# Start GitHub Copilot CLI with agent support
copilot --allow-all-tools --allow-all-paths

# Invoke the pr-reviewer agent
/agent pr-reviewer

# Request a review
please review https://github.com/dotnet/maui/pull/XXXXX
```

**Issue Resolver Agent:**
```bash
# Start GitHub Copilot CLI with agent support
copilot --allow-all-tools --allow-all-paths

# Invoke the issue-resolver agent
/agent issue-resolver

# Request issue investigation
please investigate and fix https://github.com/dotnet/maui/issues/XXXXX
```

### Option 2: GitHub Copilot Agents (Web)

1. **Navigate to the agents tab** at https://github.com/copilot/agents

2. **Select your repository and branch** using the dropdown menus in the text box

3. **Choose your agent** from the dropdown:
   - `pr-reviewer` for PR reviews
   - `issue-resolver` for investigating and fixing issues

4. **Enter a task** in the text box:
   - For PR reviews: `Please review this PR: https://github.com/dotnet/maui/pull/XXXXX`
   - For issue resolution: `Please investigate and fix: https://github.com/dotnet/maui/issues/XXXXX`

5. **Click Start task** or press Return

6. **Follow the agent's progress** - The agent task will appear below the text box. Click into it to see live updates.

## What the Agents Do

### PR Reviewer Agent

Every PR review includes:

1. **Code Analysis** - Reviews code for correctness, style, and best practices
2. **Build & Deploy** - Builds the Sandbox app and deploys to simulator/emulator
3. **Real Testing** - Tests PR changes on actual devices with measurements
4. **Before/After Comparison** - Compares behavior with and without PR changes  
5. **Edge Case Testing** - Tests scenarios not mentioned by the PR author
6. **Documented Results** - Provides review with actual test data and evidence

### Issue Resolver Agent

Every issue resolution includes:

1. **Issue Investigation** - Analyzes the reported issue and gathers context
2. **Reproduction** - Creates minimal reproduction case in Sandbox app
3. **Root Cause Analysis** - Identifies the underlying problem in the codebase
4. **Fix Implementation** - Implements and tests the fix
5. **Validation** - Tests both with and without the fix to prove it works
6. **UI Test Creation** - Adds automated UI test to prevent regression
7. **Documentation** - Provides detailed explanation of the issue and fix

### When Agents Pause

Both agents will pause and ask for help if they encounter:
- Merge conflicts when applying changes
- Build errors that prevent testing
- Test results that are unexpected or confusing
- Any step that prevents thorough validation

## Agent Architecture

### Progressive Disclosure Pattern

Agents use a layered approach to prevent cognitive overload:

1. **Entry Point** - Minimal instructions with mandatory pre-work
2. **Essential Reading** - Quick-start guide with core workflow
3. **Quick Reference** - Command templates for common tasks
4. **Just-in-Time Learning** - Specialized guides referenced as needed
5. **Deep Dives** - Platform-specific and error-handling details

**Key principle**: STOP after Essential Reading. Don't read all 11 files upfront - reference specialized guides only when needed for specific tasks.

### Reading Order & Stopping Points

**START HERE (required)**:
1. Entry point (`agents/*.md`) - Understand role and mandatory pre-work
2. Quick-start guide - Learn workflow, checkpoints, and rules
3. **STOP** - Begin work, refer to other guides as needed

**Reference as needed**:
- Quick-ref guide - Command templates
- Platform guides - Platform-specific procedures
- Error-handling - When encountering specific issues
- Testing guidelines - For detailed testing procedures

### Time Philosophy

Agents work with **time budgets as estimates for planning**, not hard deadlines:
- Budgets help recognize when to checkpoint with humans
- Quality and thoroughness take precedence over speed
- Pause and ask for help rather than rushing incomplete work

## File Structure

### Agent Definitions
- **`agents/pr-reviewer.md`** - PR reviewer agent entry point (72 lines)
- **`agents/issue-resolver.md`** - Issue resolver agent entry point (77 lines)

### Agent Instruction Packages

Each agent has a progressive disclosure structure for optimal learning:

**PR Reviewer Agent** (11 files, 3,463 lines):
- `pr-reviewer-agent/README.md` - Quick lookup by scenario (7 task-oriented scenarios)
- `pr-reviewer-agent/quick-start.md` - Essential reading (workflow, checkpoints, rules)
- `pr-reviewer-agent/quick-ref.md` - Command templates and patterns
- `pr-reviewer-agent/core-guidelines.md` - Philosophy and principles
- `pr-reviewer-agent/testing-guidelines.md` - Testing procedures
- `pr-reviewer-agent/error-handling.md` - Common mistakes (9 critical patterns)
- `pr-reviewer-agent/platforms/` - Platform-specific guides (Android, iOS, Windows, MacCatalyst)
- `pr-reviewer-agent/output-format.md` - Review output formatting

**Issue Resolver Agent** (8 files, 3,479 lines):
- `issue-resolver-agent/README.md` - Quick lookup by scenario
- `issue-resolver-agent/quick-start.md` - Essential reading
- `issue-resolver-agent/quick-ref.md` - Command templates
- `issue-resolver-agent/core-guidelines.md` - Philosophy
- `issue-resolver-agent/testing-guidelines.md` - Testing procedures
- `issue-resolver-agent/error-handling.md` - Common mistakes
- `issue-resolver-agent/platforms/` - Platform-specific guides
- `issue-resolver-agent/output-format.md` - PR description formatting

### Shared Instruction Files (4 files, 2,224 lines)

These provide specialized guidance for specific scenarios used by both agents:

- **`instructions/common-testing-patterns.md`** - Command sequences (UDID extraction, builds, deploys, error checking)
- **`instructions/uitests.instructions.md`** - UI testing guidelines (when to use HostApp vs Sandbox)
- **`instructions/safearea-testing.md`** - SafeArea testing patterns (measure children, not parents)
- **`instructions/instrumentation.md`** - Code instrumentation for debugging and testing
- **`instructions/appium-control.instructions.md`** - Standalone Appium scripts for manual debugging
- **`instructions/templates.instructions.md`** - Template modification rules

### Recent Improvements (Phase 1 - November 2025)

**PR Reviewer Agent enhancements:**
1. **Mandatory pre-work moved to top** - Critical requirements now at line 6 instead of line 43
2. **Reading order & stopping points** - Explicit "STOP after Essential Reading" to prevent reading loop
3. **Most critical mistake elevated** - "Don't Skip Testing" moved from Mistake #6 to Mistake #1 with complete Android emulator startup sequence
4. **Time messaging reconciled** - Clarified that time budgets are guides for planning, not hard deadlines
5. **Appium version updated** - All references updated to Appium.WebDriver 8.0.1 (latest stable)

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
- Work with the agent to resolve the issue before continuing
- This ensures work is based on actual working code

**Agent reads too many files upfront:**
- Should follow "STOP after Essential Reading" guidance
- Specialized guides should be referenced just-in-time, not upfront
- If agent reads all 11 files before starting, this needs refinement

## Support

For issues or questions about the AI agent instructions:
1. Check this README and referenced instruction files
2. Review recent PR reviews to see examples
3. Ask in the repository discussions or issues
4. Propose changes via PR to improve the instructions

## Metrics

**Total instruction content**:
- PR Reviewer: 11 files, 3,463 lines
- Issue Resolver: 8 files, 3,479 lines  
- Shared instructions: 4 files, 2,224 lines
- Total: 23 unique files, ~9,166 lines

**Progressive disclosure effectiveness**:
- Entry point: 72-77 lines (< 2 minutes)
- Essential reading: ~500 lines (< 10 minutes)
- Just-in-time references: Read only when specific need arises

---

**Last Updated**: 2025-11-23

**Note**: These instructions are actively being refined based on real-world usage. Phase 1 improvements completed November 2025. Feedback and improvements are welcome!
