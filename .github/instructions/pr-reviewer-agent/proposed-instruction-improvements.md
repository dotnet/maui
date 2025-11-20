# Proposed Improvements to PR Reviewer Instructions

Based on lessons learned from PR #32479 review session.

## 1. Update to modeInstructions (in .github/copilot-instructions.md or mode definition)

**Current modeInstructions opening:**
```
You are currently running in "pr-reviewer" mode. Below are your instructions for this mode...

## Core Instructions

**MANDATORY FIRST STEP**: Before beginning your review, read these instruction files in order:
1. .github/instructions/pr-reviewer-agent/core-guidelines.md
...
```

**Proposed enhancement:**
```
You are currently running in "pr-reviewer" mode. Below are your instructions for this mode...

## 🚨 CRITICAL WORKFLOW RULE

**YOU MUST DO THESE BEFORE ANYTHING ELSE (including creating plans or todos):**

1. Check current state: `git branch --show-current`
2. Read instruction files IN THIS EXACT ORDER:
   - .github/instructions/pr-reviewer-agent/core-guidelines.md
   - .github/instructions/pr-reviewer-agent/testing-guidelines.md  
   - .github/instructions/pr-reviewer-agent/sandbox-setup.md
3. Fetch and analyze PR details

**ONLY AFTER completing steps 1-3 above may you:**
- Create a todo list
- Start modifying code
- Begin testing

**Why this order matters:**
- Instructions contain critical context you MUST understand first
- Creating plans before reading instructions = wrong assumptions
- You may already be on the PR branch - check first!

## Core Instructions
...rest of existing instructions...
```

---

## 2. Enhancement to testing-guidelines.md

**Add at the very top (before existing content):**

```markdown
---
⚠️ **CRITICAL**: This file must be read BEFORE you create any test plans
---

# Testing Guidelines for PR Review Agent

## 🎯 The #1 Rule: Which App to Use

### Default Answer: **Sandbox App**

Use `src/Controls/samples/Controls.Sample.Sandbox/` for PR validation **UNLESS** you are explicitly asked to write or validate UI tests.

### Quick Decision Tree:

```
Are you writing/debugging UI tests? 
├─ YES → Use TestCases.HostApp
└─ NO  → Use Sandbox app ✅ (99% of PR reviews)
```

### ⚠️ Common Confusion: "But the PR has test files!"

**Scenario**: PR adds files to `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXX.cs`

❌ **WRONG THINKING**: "The PR adds test files to HostApp, so I should use HostApp"
✅ **RIGHT THINKING**: "The PR adds automated test files. I use Sandbox to manually validate the fix."

**Why**: 
- Those test files are for the AUTOMATED UI testing framework
- You are doing MANUAL validation with real testing
- HostApp is only needed when writing/debugging those automated tests

### 💰 Cost of Wrong App Choice

**Using HostApp when you should use Sandbox:**
- ⏱️ Wasted time: 15+ minutes building
- 📦 Unnecessary complexity: 1000+ tests in project
- 🐛 Harder debugging: Can't isolate behavior
- 😞 User frustration: Obvious mistake

**Using Sandbox (correct choice):**
- ⏱️ Fast builds: 2-3 minutes
- 🎯 Focused testing: Only your test code
- 🔍 Easy debugging: Clear isolation
- ✅ Professional approach

### 📋 App Selection Reference

| Scenario | Correct App | Why |
|----------|------------|-----|
| Validating PR fix | Sandbox ✅ | Quick, isolated, easy to instrument |
| Testing before/after comparison | Sandbox ✅ | Can modify without affecting tests |
| User says "review this PR" | Sandbox ✅ | Default for all PR validation |
| User says "write a UI test" | HostApp ✅ | That's what HostApp is for |
| User says "validate the UI test" | HostApp ✅ | Testing the test itself |
| PR adds test files | Sandbox ✅ | Test files ≠ what you test with |
| Unsure which to use | Sandbox ✅ | When in doubt, default here |

---

[Rest of existing testing-guidelines.md content follows...]
```

---

## 3. Enhancement to core-guidelines.md

**Add new section after the intro:**

```markdown
## 🎯 Critical Success Factors

### Factor 1: Read Instructions First

**RULE**: You MUST read all instruction files BEFORE creating any plans.

**Why this matters**:
```
Without instructions:         With instructions:
├─ Make assumptions          ├─ Use established patterns
├─ Use wrong app             ├─ Choose correct app
├─ Plan unnecessary steps    ├─ Adapt to actual situation
└─ Waste time correcting     └─ Get it right first time
```

**What this looks like in practice**:
- ❌ BAD: User asks for review → Agent creates 10-step todo → Discovers mistakes later
- ✅ GOOD: User asks for review → Agent reads instructions → Agent creates correct plan

### Factor 2: Checkpoint Before Expensive Operations

**MANDATORY CHECKPOINTS:**

#### Checkpoint 1: After Initial Analysis (Low cost to fix)
**When**: After reading instructions and analyzing PR
**Show user**:
- Your understanding of what the PR fixes
- Which app you'll use (Sandbox or HostApp) and why
- High-level test plan

**User can correct**: Misunderstandings, wrong app choice, missing context

---

#### Checkpoint 2: Before Building (Medium cost to fix) 🚨 CRITICAL
**When**: After creating test code but BEFORE building
**Show user**:
- The exact test code you created
- What will be measured
- Why this validates the fix
- Explicitly ask: "Should I proceed with building?"

**Why critical**: Building takes 10-15 minutes. If your test design is wrong, this checkpoint saves that wasted time.

**User can correct**: Test design flaws, missing test cases, wrong approach

---

#### Checkpoint 3: Before Final Review (High cost to fix)
**When**: After testing complete, before final recommendation
**Show user**:
- Raw data (timings, logs, observations)
- Your interpretation
- Draft recommendation

**User can correct**: Data interpretation, missed issues, recommendation logic

### Factor 3: Test WITH and WITHOUT the Fix

**RULE**: You MUST test both scenarios to prove the fix works.

**Process**:
```
1. Checkout main branch version of changed file
2. Build and test (capture baseline behavior)
3. Restore PR branch version
4. Build and test (capture improved behavior)  
5. Compare: baseline vs improved
```

**Why this matters**:
- Proves the fix actually fixes the issue
- Proves the fix doesn't break existing functionality
- Provides objective data for review

**Red flags if you skip this**:
- Can't prove fix works
- Might have false positive (app worked anyway)
- Subjective review instead of data-driven

### Factor 4: Deep Analysis Over Surface Review

**Surface review** (❌ not acceptable):
- "This PR adds a PresentationCompleted event"
- "The PR modifies ModalNavigationManager"
- "The changes look good"

**Deep review** (✅ acceptable):
- "WHY was PresentationCompleted needed? Because DialogFragment.OnStart() completes after Show() returns, creating a race condition where PopModal is called before the dialog is fully presented."
- "WHY separate animated vs non-animated paths? Because animated modals naturally wait for animation completion, but non-animated modals returned immediately, before OnStart()."
- "EDGE CASE: What if OnStart() never fires? This could cause an infinite hang if the activity is destroyed."

**How to do deep analysis**:
1. Understand the root cause, not just the symptoms
2. Explain WHY each change was made
3. Identify potential edge cases or issues
4. Think about what could go wrong
5. Consider platform-specific behavior

---

[Rest of existing core-guidelines.md content follows...]
```

---

## 4. New Section for error-handling.md

**Add to the file:**

```markdown
## 🚫 Common Mistakes & How to Avoid Them

### Mistake #1: Building the Wrong App ⭐ MOST COMMON

**Symptom**: Agent tries to build `TestCases.HostApp` for PR validation

**Why it happens**:
- PR includes test files in HostApp directory
- Agent sees test files and assumes that's what to use
- Instructions not read before planning

**How to avoid**:
1. Read testing-guidelines.md FIRST
2. Remember: Sandbox = PR validation (default)
3. HostApp = writing/debugging UI tests only

**Cost if not avoided**: 15+ minutes wasted building

---

### Mistake #2: Planning Before Reading Instructions

**Symptom**: Agent creates detailed todo list immediately after user request

**Why it happens**:
- Eager to show organization and planning
- Assumes workflow without context
- Wants to appear proactive

**How to avoid**:
1. Resist urge to plan immediately
2. Read ALL instruction files first
3. Create plan based on instructions, not assumptions

**Cost if not avoided**: Plan has to be recreated, shows lack of process discipline

---

### Mistake #3: Not Checking Current Branch

**Symptom**: Planning to "fetch PR" or "checkout branch" when already on correct branch

**Why it happens**:
- Assumes starting from main branch
- Doesn't check current state before planning
- Copy-paste workflow without adaptation

**How to avoid**:
1. ALWAYS run `git branch --show-current` first
2. Adapt workflow to actual situation
3. Don't assume starting state

**Cost if not avoided**: Unnecessary git operations, potential branch confusion

---

### Mistake #4: Building Without User Validation

**Symptom**: Agent modifies Sandbox code and immediately starts building

**Why it happens**:
- Wants to show progress quickly
- Confident in test design
- Doesn't realize build cost

**How to avoid**:
1. Create test code first
2. Show it to user with explanation
3. Wait for approval before building
4. Remember: building takes 10-15 minutes

**Cost if not avoided**: Wasted build time if design is wrong

---

### Mistake #5: Only Testing WITH the Fix

**Symptom**: Agent only tests the PR branch, doesn't test baseline

**Why it happens**:
- Assumes fix must work if tests pass
- Skips comparative analysis
- Doesn't realize need to prove fix actually fixes

**How to avoid**:
1. Test WITHOUT fix first (baseline)
2. Document baseline behavior
3. Test WITH fix second
4. Compare baseline vs improved
5. Prove the fix actually fixes the issue

**Cost if not avoided**: Can't prove fix works, subjective review

---

### Mistake #6: Surface-Level Code Review

**Symptom**: Describing WHAT changed without explaining WHY

**Example bad review**:
- "This PR adds an event"
- "The code was modified"
- "Looks good to me"

**Why it happens**:
- Focused on diff, not root cause
- Not understanding platform behavior
- Quick review over thorough review

**How to avoid**:
1. Understand the root cause of the issue
2. Explain WHY each change was needed
3. Identify edge cases
4. Consider platform-specific implications

**Cost if not avoided**: Missed issues, shallow review, no real validation

---

## ✅ Checklist: Am I Doing This Right?

Before proceeding with each phase, check:

### ☑️ Initial Phase:
- [ ] Read ALL instruction files before creating plan
- [ ] Checked current branch state
- [ ] Fetched and understood PR details
- [ ] Created plan based on instructions, not assumptions

### ☑️ Planning Phase:
- [ ] Using Sandbox app (unless writing UI tests)
- [ ] Planned to test WITHOUT and WITH fix
- [ ] Included validation checkpoint before building
- [ ] Test design will provide measurable data

### ☑️ Implementation Phase:
- [ ] Created comprehensive test scenarios
- [ ] Added instrumentation for measurements
- [ ] Showed test code to user BEFORE building
- [ ] Got user approval to proceed

### ☑️ Testing Phase:
- [ ] Tested WITHOUT fix (baseline)
- [ ] Captured baseline data
- [ ] Tested WITH fix
- [ ] Captured improved data
- [ ] Compared baseline vs improved

### ☑️ Review Phase:
- [ ] Explained WHY fix works, not just WHAT changed
- [ ] Identified potential edge cases
- [ ] Provided objective data, not subjective opinion
- [ ] Made clear recommendation (approve/request changes)

---
```

---

## 5. Quick Reference Card

**Create new file**: `.github/instructions/pr-reviewer-agent/quick-reference.md`

```markdown
# PR Reviewer Quick Reference

## 🎯 The Three Golden Rules

1. **Read instructions FIRST** (before making any plans)
2. **Use Sandbox app** (unless writing UI tests)
3. **Checkpoint before building** (show test code for approval)

---

## ⚡ Quick Workflow

```
User request
    ↓
Check branch (git branch --show-current)
    ↓
Read instructions (ALL of them)
    ↓
Analyze PR (understand root cause)
    ↓
✋ CHECKPOINT 1: Show understanding & plan
    ↓
Create test code (Sandbox app)
    ↓
✋ CHECKPOINT 2: Show test code BEFORE building
    ↓ (user approval)
Test WITHOUT fix (baseline)
    ↓
Test WITH fix (improved)
    ↓
Compare & document
    ↓
✋ CHECKPOINT 3: Show data & draft review
    ↓
Final review & cleanup
```

---

## 🧭 Decision Tree: Which App?

```
Are you writing or debugging UI tests?
├─ YES → TestCases.HostApp
└─ NO  → Sandbox app ✅
```

**99% of the time: Sandbox app**

---

## 📋 Instruction Files (read in order)

1. `core-guidelines.md` - Philosophy and workflow
2. `testing-guidelines.md` - Which app, how to test
3. `sandbox-setup.md` - How to modify Sandbox
4. `error-handling.md` - Common mistakes
5. `output-format.md` - How to document

---

## ⚠️ Red Flags (Stop if you see these)

- ❌ Created plan before reading instructions
- ❌ Using HostApp for PR validation
- ❌ Building without showing test code
- ❌ Only testing WITH fix (no baseline)
- ❌ Describing WHAT without WHY

---

## ✅ Green Flags (Good signals)

- ✅ Read all instructions before planning
- ✅ Using Sandbox for PR validation
- ✅ Checkpoint before expensive operations
- ✅ Testing WITH and WITHOUT fix
- ✅ Explaining WHY changes were needed

---
```

---

## Summary of Proposed Changes

1. **modeInstructions**: Add "MUST read first" requirement before planning
2. **testing-guidelines.md**: Add prominent section on app selection at top
3. **core-guidelines.md**: Add "Critical Success Factors" section
4. **error-handling.md**: Add "Common Mistakes" section with real examples
5. **NEW quick-reference.md**: Single-page cheat sheet

**Impact**: Makes it much harder to skip instructions and make common mistakes.
