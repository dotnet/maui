# .github Folder - AI Coding Assistant Instructions

This folder contains instructions and configurations for AI coding assistants working on the .NET MAUI repository.

## Available Agents

### PR Agent
The PR agent is a unified 4-phase workflow for investigating issues and reviewing/working on PRs. It handles everything from context gathering through test verification, fix exploration, and creating PRs or review reports.

### Sandbox Agent
The sandbox agent is your general-purpose tool for working with the .NET MAUI Sandbox app. Use it for manual testing, PR validation, issue reproduction, and experimentation with MAUI features.

### UI Test Coding Agent
The UI test coding agent writes and runs automated UI tests following .NET MAUI conventions. Use it for creating new test coverage, running existing tests from PRs, and validating UI test correctness.

## How to Use

### Option 1: Multi-Agent Mode (Recommended)

**Let GitHub Copilot automatically delegate to the right agent:**

Instead of manually selecting an agent, you can prompt Copilot directly and it will automatically delegate to the appropriate specialized agent. This is often more efficient because Copilot can coordinate multiple agents if needed.

**How to use it:**

```bash
# Start GitHub Copilot CLI
copilot

# Prompt naturally without selecting an agent
Can you reproduce https://github.com/dotnet/maui/issues/32941 inside the sandbox? 
Please make sure to check if there's an agent you can use for each step of your plan.

# Or for other tasks
Please review PR #12345
Please write UI tests for issue #67890
Please investigate and fix this bug: https://github.com/dotnet/maui/issues/XXXXX
```

**💡 Pro tip - Ask for the plan first:**

Sometimes it's helpful to see the execution plan before work begins. Add this to your prompt:

```bash
Can you reproduce https://github.com/dotnet/maui/issues/32941 inside the sandbox?
Please tell me your plan before you start.
```

This gives you a chance to adjust the plan or provide additional context before execution.

**Setting yourself up for success:**

When using multi-agent mode, craft your prompts to guide proper delegation:
- ✅ "Can you reproduce <URL> inside the sandbox? Please check if there's an agent for each step."
- ✅ "Please test this PR and make sure to use agents where appropriate"
- ✅ "Write UI tests for this issue, using the uitest agent if available"

The key phrases like "check if there's an agent", "use agents", and "inside the sandbox" help Copilot understand you want specialized agent delegation.

### Option 2: GitHub Copilot CLI (Direct Agent Selection)

**Sandbox Agent:**
```bash
# Start GitHub Copilot CLI with agent support
copilot

# Invoke the sandbox-agent
/agent sandbox-agent

# Test a PR or reproduce an issue
please test PR #32479
please reproduce issue #12345
```

**Write Tests Agent:**
```bash
# Start GitHub Copilot CLI with agent support
copilot

# Invoke the write-tests-agent
/agent write-tests-agent

# Write UI tests (agent will invoke write-ui-tests skill)
please write UI tests for issue #12345
```

**PR Agent:**
```bash
# Start GitHub Copilot CLI with agent support
copilot

# Invoke the pr agent
/agent pr

# Fix an issue or review a PR
please fix issue #12345
please review https://github.com/dotnet/maui/pull/XXXXX
```

**For issues without a PR (remote Copilot):**
```bash
# Use /delegate to have remote Copilot create the fix
/delegate fix issue https://github.com/dotnet/maui/issues/XXXXX
```

### Option 3: GitHub Copilot Agents (Web)

1. **Navigate to the agents tab** at https://github.com/copilot/agents

2. **Select your repository and branch** using the dropdown menus in the text box

3. **Choose your agent** from the dropdown:
   - `sandbox-agent` for manual testing and experimentation
   - `write-tests-agent` for writing tests (invokes appropriate skill)
   - `pr` for reviewing and working on existing PRs

4. **Enter a task** in the text box:
   - For sandbox testing: `Please test PR #32479`
   - For UI tests: `Please write UI tests for issue #12345`
   - For PR review: `Please review PR #XXXXX`
   - For PR reviews: `Please review this PR: https://github.com/dotnet/maui/pull/XXXXX`

5. **Click Start task** or press Return

6. **Follow the agent's progress** - The agent task will appear below the text box. Click into it to see live updates.

## What the Agents Do

### Sandbox Agent

Your go-to agent for hands-on work with the Sandbox app:

1. **PR Testing** - Manually validates PRs by running them in the Sandbox app
2. **Issue Reproduction** - Creates minimal reproduction cases for reported issues
3. **Feature Experimentation** - Try out MAUI features in a clean environment
4. **Quick Validation** - Fast manual testing without writing automated tests
5. **Automated Logging** - Uses `BuildAndRunSandbox.ps1` to capture all logs to `CustomAgentLogsTmp/Sandbox/`

### UI Test Coding Agent

Automated testing specialist for the .NET MAUI test suite:

1. **Test Creation** - Writes new UI tests following .NET MAUI conventions
2. **Test Execution** - Runs existing UI tests from PRs or repository
3. **Test Validation** - Verifies tests are correct and follow best practices
4. **Cross-Platform** - Tests on iOS, Android, Windows, and MacCatalyst
5. **Automated Workflow** - Uses `BuildAndRunHostApp.ps1` to handle building, deployment, and logging to `CustomAgentLogsTmp/UITests/`

### PR Agent

Unified 4-phase workflow for issue investigation and PR work:

1. **Pre-Flight** - Context gathering from issues/PRs
2. **Gate** - Verify tests exist and catch the issue (mandatory checkpoint)
3. **Fix** - Explore fix alternatives using `try-fix` skill, compare approaches
4. **Report** - Create PR or write review report

### When Agents Pause

All agents will pause and ask for help if they encounter:
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
- **`agents/pr.md`** - PR workflow phases 1-2 (Pre-Flight, Gate)
- **`agents/pr/post-gate.md`** - PR workflow phases 3-4 (Fix, Report)
- **`agents/sandbox-agent.md`** - Sandbox agent for testing and experimentation
- **`agents/write-tests-agent.md`** - Test writing agent (dispatches to skills like write-ui-tests)

### Agent Files

Agent files in the `.github/agents/` directory:

- **`agents/pr.md`** - PR workflow phases 1-2 (Pre-Flight, Gate)
- **`agents/pr/post-gate.md`** - PR workflow phases 3-4 (Fix, Report)
- **`agents/sandbox-agent.md`** - Sandbox app testing and experimentation
- **`agents/write-tests-agent.md`** - Test writing (invokes skills like write-ui-tests)

### Shared Instruction Files

These provide specialized guidance for specific scenarios used by all agents:

- **`instructions/uitests.instructions.md`** - UI testing guidelines (when to use HostApp vs Sandbox)
- **`instructions/sandbox.instructions.md`** - Sandbox app testing patterns
- **`instructions/templates.instructions.md`** - Template modification rules
- **`instructions/xaml-unittests.instructions.md`** - XAML unit testing guidelines
- **`instructions/collectionview-handler-detection.instructions.md`** - CollectionView handler configuration
- **`instructions/agents.instructions.md`** - Custom agent authoring guidelines
- **`instructions/skills.instructions.md`** - Skill development standards
- **`instructions/helix-device-tests.instructions.md`** - Helix device test infrastructure

### Shared Scripts

Automated PowerShell scripts for testing workflows:

- **`scripts/BuildAndRunSandbox.ps1`** - Build, deploy, and test Sandbox app (Android/iOS)
- **`scripts/BuildAndRunHostApp.ps1`** - Build, deploy, and run UI tests (Android/iOS)
- **`scripts/templates/RunWithAppiumTest.template.cs`** - Template for Appium test scripts

### Log Directories

All agent logs are consolidated under `CustomAgentLogsTmp/`:

- **`CustomAgentLogsTmp/Sandbox/`** - Sandbox agent logs (appium.log, android-device.log, ios-device.log, RunWithAppiumTest.cs)
- **`CustomAgentLogsTmp/UITests/`** - UI test agent logs (appium.log, android-device.log, ios-device.log, test-output.log)

### Skills

Reusable skills in `.github/skills/` that agents can invoke:

- **`try-fix/`** - Proposes and tests independent fix approaches, records results, learns from failures
- **`verify-tests-fail-without-fix/`** - Verifies tests catch bugs; supports UI tests (Appium) and Device tests (xharness) with auto-detection
- **`write-ui-tests/`** - Creates UI tests for issues following MAUI conventions
- **`write-xaml-tests/`** - Creates XAML unit tests for parsing, XamlC, and source generation issues
- **`pr-build-status/`** - Retrieves Azure DevOps build status for PRs

### Recent Improvements (January 2026)

**PR Agent Consolidation:**
1. **Unified PR Agent** - Replaced separate `issue-resolver` and `pr-reviewer` agents with single 4-phase `pr` agent
2. **try-fix Skill** - New skill for exploring independent fix alternatives with empirical testing
3. **Skills Integration** - Added `verify-tests-fail-without-fix` and `write-ui-tests` skills for reusable test workflows
4. **Agent/Skills Guidelines** - New instruction files for authoring agents and skills

**Prior Infrastructure Consolidation (November 2025):**
1. **Unified Log Structure** - All logs now under `CustomAgentLogsTmp/` with subdirectories for Sandbox and UITests
2. **Shared Script Library** - Created reusable PowerShell scripts for device startup, build, and deployment
3. **Agent Simplification** - Consolidated test writing into `write-tests-agent` (dispatcher) + `write-ui-tests` skill
4. **Automated Testing Scripts** - All agents now use PowerShell scripts instead of manual commands

### General Guidelines
- **`copilot-instructions.md`** - General coding standards, build requirements, file conventions for the entire repository

### Prompts
- **`prompts/maui-release-notes.prompt.md`** - Prompt for generating release notes

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

**Agent Files**:
- 4 agent files (pr.md, pr/post-gate.md, sandbox-agent.md, write-tests-agent.md)
- 5 skills (try-fix, verify-tests-fail-without-fix, write-ui-tests, write-xaml-tests, pr-build-status)
- All validated and consistent with consolidated structure

**Automation**:
- 2 main PowerShell testing scripts (BuildAndRunSandbox.ps1, BuildAndRunHostApp.ps1)
- 3 shared utility scripts (Start-Emulator.ps1, Build-AndDeploy.ps1, shared-utils.ps1)
- 1 Appium test template (RunWithAppiumTest.template.cs)
- Automated log capture to `CustomAgentLogsTmp/` subdirectories

**Progressive disclosure effectiveness**:
- Entry point: Quick agent overview (< 2 minutes)
- Essential reading: Quick-start guides (< 10 minutes)
- Just-in-time references: Read only when specific need arises

---

**Last Updated**: 2026-01-07

**Note**: These instructions are actively being refined based on real-world usage. PR agent consolidation completed January 2026 (unified 4-phase workflow with try-fix skill). Feedback and improvements are welcome!
