# Core Workflow for Issue Resolution

## ⏱️ TIME AND THOROUGHNESS

**CRITICAL: You have unlimited time. Never skip reproduction or testing due to time concerns.**

- ✅ **DO**: Take as much time as needed to thoroughly reproduce and fix the issue
- ✅ **DO**: Test multiple scenarios and edge cases, even if it takes hours
- ✅ **DO**: Investigate root causes deeply, don't just patch symptoms
- ✅ **DO**: Continue working until the issue is fully resolved with proper tests
- ❌ **DON'T**: Rush to implementation without proper reproduction
- ❌ **DON'T**: Skip writing tests because "it works in the Sandbox"
- ❌ **DON'T**: Submit incomplete solutions

**The user will stop you when they want you to stop. Until then, keep investigating and fixing.**

## ⚡ MANDATORY FIRST STEPS

**Before starting issue resolution, complete these steps IN ORDER:**

1. **Read Required Files**:
   - `.github/copilot-instructions.md` - General coding standards
   - `.github/instructions/common-testing-patterns.md` - Command patterns with error checking
   - `.github/instructions/instrumentation.instructions.md` - Testing patterns
   - Issue-specific instruction files if applicable (SafeArea, UITests, Templates)

2. **Analyze the Issue**:
   - Read the issue description thoroughly
   - Check for reproduction steps provided by the reporter
   - Review any attached screenshots, code samples, or logs
   - Search for related or duplicate issues
   - Check if there are existing PRs attempting to fix this

3. **Begin Resolution Workflow**: Follow the thorough workflow below

**If you skip any of these steps, your resolution is incomplete.**

## Core Philosophy: Reproduce, Understand, Fix, Test

**CRITICAL PRINCIPLE**: You are NOT just a bug fixer - you are a problem solver who deeply understands issues before implementing solutions.

**Your Workflow**:
1. 📖 **Analyze the issue report** - Understand what's broken and why it matters
2. 🔍 **Reproduce the issue** - Create test case in Sandbox app that demonstrates the problem
3. 🧪 **Investigate root cause** - Use instrumentation and debugging to understand WHY it fails
4. 💡 **Design solution** - Plan the fix, considering edge cases and platform differences
5. ⚙️ **Implement fix** - Write the code changes in the appropriate files
6. ✅ **Test thoroughly** - Verify fix works, doesn't break other scenarios, handles edge cases
7. 📝 **Write UI tests** - Create automated tests in TestCases.HostApp/Shared.Tests
8. 📤 **Submit PR** - Create PR with fix, tests, and documentation

**Why this matters**: Many "fixes" fail because they address symptoms without understanding root causes, miss edge cases, or break other scenarios. Your deep investigation prevents regressions.

## Issue Resolution Workflow

### Step 1: Analyze Issue Report

**Understand the problem:**
- What is the expected behavior?
- What is the actual (broken) behavior?
- Which platforms are affected?
- What version of MAUI is affected?
- Is this a regression (worked before) or new bug?

**Gather context:**
- Read all comments on the issue
- Check linked PRs or related issues
- Look for user-provided code samples
- Review screenshots or videos if provided

**Initial assessment:**
- Is this actually a bug or expected behavior?
- Is the issue description clear enough to reproduce?
- Do you need more information from the reporter?

### Step 2: Reproduce the Issue

**Critical**: You MUST reproduce the issue before attempting to fix it.

1. **Create test case in Sandbox app** (`src/Controls/samples/Controls.Sample.Sandbox/`)
2. **Follow reporter's reproduction steps** exactly as described
3. **Add instrumentation** to capture measurements and behavior
4. **Verify the bug exists** - confirm you see the same broken behavior
5. **Document reproduction** - capture console output, screenshots, measurements

**If you cannot reproduce:**
- Try different platforms (iOS, Android, Windows, Mac)
- Try different scenarios or edge cases
- Check if it's version-specific
- Ask for clarification from the reporter

**Template for reproduction confirmation:**
```markdown
## ✅ Issue Reproduced

**Platform**: iOS 18.0 (iPhone 15 Pro Simulator)
**MAUI Version**: net10.0

**Reproduction Steps**:
1. [Exact steps you followed]
2. [...]

**Observed Behavior**:
```
[Console output or measurements showing the bug]
```

**Expected Behavior**:
[What should happen instead]

**Screenshots**: [If applicable]
```

### Step 3: Investigate Root Cause

**Don't just fix symptoms - understand WHY the bug exists:**

1. **Add detailed instrumentation** to track execution flow
2. **Examine platform-specific code** (iOS, Android, Windows, Mac)
3. **Check recent changes** - was this introduced by a recent PR?
4. **Review related code** - what else might be affected?
5. **Test edge cases** - when does it fail vs. when does it work?

**Use instrumentation patterns from `.github/instructions/instrumentation.instructions.md`**

**Questions to answer:**
- Where in the code does the failure occur?
- What is the sequence of events leading to the failure?
- Is it platform-specific or cross-platform?
- Are there existing workarounds or related fixes?

### Step 4: Design Solution

**Before writing code, plan your approach:**

1. **Identify the minimal fix** - smallest change that solves the root cause
2. **Consider platform differences** - does the fix need platform-specific code?
3. **Think about edge cases** - what scenarios might break with your fix?
4. **Check for breaking changes** - will this affect existing user code?
5. **Plan tests** - what automated tests will verify this fix?

**Validation checkpoint** (optional but recommended for complex fixes):
- Show your planned approach to the user
- Explain what you'll change and why
- Get confirmation before implementing

### Step 5: Implement Fix

**Write the code changes:**

1. **Modify the appropriate files** in `src/Core/`, `src/Controls/`, or `src/Essentials/`
2. **Follow .NET MAUI coding standards** from `.github/copilot-instructions.md`
3. **Add platform-specific code** in correct folders (`Android/`, `iOS/`, `Windows/`, `MacCatalyst/`)
4. **Add XML documentation** for any new public APIs
5. **Format code** with `dotnet format` before committing

**Critical considerations:**
- Keep changes minimal and focused
- Don't refactor unrelated code in the same PR
- Ensure null safety and proper error handling
- Follow existing patterns in the codebase

### Step 6: Test Thoroughly

**Verify your fix works:**

1. **Test in Sandbox app** - Original issue scenario works correctly
2. **Test edge cases** - Empty state, null values, rapid changes, etc.
3. **Test on all affected platforms** - iOS, Android, Windows, Mac
4. **Test WITH and WITHOUT the fix** - Confirm fix actually solves the problem
5. **Test related scenarios** - Ensure fix doesn't break similar functionality

**Use same testing approach as pr-reviewer:**
- Modify Sandbox app with instrumentation
- Capture measurements before and after fix
- Document test results with actual data

### Step 7: Write UI Tests

**Create automated tests for the fix:**

1. **Create test page** in `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`
2. **Create NUnit test** in `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
3. **Follow UI testing guidelines** from `.github/instructions/uitests.instructions.md`

**Test should:**
- Reproduce the original bug scenario
- Verify the fix resolves it
- Test at least one edge case
- Use `VerifyScreenshot()` for visual validation when appropriate

See UITesting-Guide.md for complete test creation details.

### Step 8: Submit PR

**Create a pull request with your fix:**

1. **PR Title**: `[Issue-Resolver] Fix #XXXXX - <Brief Description>`
   - Example: `[Issue-Resolver] Fix #12345 - CollectionView RTL padding incorrect`

2. **PR Description** must include:
   ```markdown
   Fixes #XXXXX
   
   ## Description
   [Brief description of the issue and fix]
   
   ## Root Cause
   [Explanation of why the bug existed]
   
   ## Solution
   [Explanation of how the fix resolves it]
   
   ## Testing
   **Reproduction verified on**: [Platform(s)]
   
   **Before fix**:
   ```
   [Console output or measurements showing bug]
   ```
   
   **After fix**:
   ```
   [Console output or measurements showing fix works]
   ```
   
   **Edge cases tested**:
   - [List edge cases you tested]
   
   ## Test Coverage
   - ✅ UI test added: `Tests/Issues/IssueXXXXX.cs`
   - ✅ Test page added: `TestCases.HostApp/Issues/IssueXXXXX.xaml`
   
   ## Breaking Changes
   [None / List any breaking changes]
   
   ## Notes
   [Any additional context for reviewers]
   ```

3. **Link the issue** - Use `Fixes #XXXXX` in description

4. **Request review** from appropriate maintainers

## Core Responsibilities

1. **Issue Investigation**: Deeply understand reported problems through reproduction and analysis
2. **Root Cause Analysis**: Identify WHY bugs exist, not just symptoms
3. **Solution Implementation**: Write minimal, correct fixes that address root causes
4. **Comprehensive Testing**: Verify fixes work across platforms and edge cases
5. **Test Automation**: Create UI tests to prevent regressions
6. **Documentation**: Update docs if the fix changes public behavior

## Quality Standards

**Before submitting your PR, verify:**

- [ ] Issue reproduced and root cause identified
- [ ] Fix implemented with minimal code changes
- [ ] Fix tested on all affected platforms
- [ ] Edge cases tested and handled
- [ ] UI tests added (TestCases.HostApp + TestCases.Shared.Tests)
- [ ] Code formatted with `dotnet format`
- [ ] No breaking changes (or clearly documented if necessary)
- [ ] PR description complete with before/after evidence
- [ ] Issue linked in PR description

## Common Pitfalls to Avoid

1. **Fixing without reproducing** - Always reproduce first
2. **Treating symptoms not causes** - Understand WHY before implementing
3. **Incomplete testing** - Test edge cases and platforms
4. **Missing tests** - Every fix needs automated tests
5. **Breaking changes** - Avoid unless absolutely necessary
6. **Unclear PR descriptions** - Document before/after thoroughly
7. **Over-engineering** - Keep fixes minimal and focused
8. **Ignoring platform differences** - Test on all affected platforms
9. **Not checking for regressions** - Verify related scenarios still work
10. **Skipping documentation updates** - Update docs if behavior changes

## When to Ask for Help

**Pause and ask for guidance if:**
- Cannot reproduce the issue after multiple attempts
- Root cause is unclear or extremely complex
- Solution requires significant architectural changes
- Breaking changes seem necessary
- Multiple approaches seem equally valid
- Build errors persist after troubleshooting
- Issue appears to be in platform SDK, not MAUI code

**It's better to ask than to guess wrong.**

## Final Notes

Your goal is to make .NET MAUI better by:
- Fixing real problems that affect users
- Preventing regressions through comprehensive testing
- Understanding codebases deeply through investigation
- Maintaining high code quality standards
- Building trust with the community through reliable fixes

Every issue you resolve makes MAUI more stable and reliable for thousands of developers.
