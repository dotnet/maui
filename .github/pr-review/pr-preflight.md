# PR Pre-Flight — Context Gathering & Code Review

> **SCOPE:** Gather context, classify files, and perform deep code review. No code changes. No fix selection. No test execution.

---

## Part A: Context Gathering (Steps 1–6)

1. **Read the issue** — full body + ALL comments via GitHub MCP tools
2. **Find the PR** — read description, diff summary, review comments, inline feedback
3. **Fetch PR discussion** — detect prior agent reviews, import findings if found
4. **Classify files** — separate fix files from test files, identify test type (UI / Device / Unit)
5. **Document edge cases** — from comments mentioning "what about...", "does this work with..."
6. **Record PR's fix** in Fix Candidates table (pending validation)

```bash
# Fetch PR metadata
gh pr view XXXXX --json title,body,url,author,labels,files

# Find linked issue
gh pr view XXXXX --json body --jq '.body' | grep -oE "(Fixes|Closes|Resolves) #[0-9]+" | head -1
gh issue view ISSUE_NUMBER --json title,body,comments

# PR comments
gh pr view XXXXX --json comments --jq '.comments[] | "Author: \(.author.login)\n\(.body)\n---"'

# Inline review comments (CRITICAL — often contains key technical feedback)
gh api "repos/dotnet/maui/pulls/XXXXX/comments" --jq '.[] | "File: \(.path):\(.line // .original_line)\nAuthor: \(.user.login)\n\(.body)\n---"'

# Detect prior agent reviews
gh pr view XXXXX --json comments --jq '.comments[] | select(.body | contains("Final Recommendation") and contains("| Phase | Status |")) | .body'
```

**If prior agent review found:** Parse phase statuses, import findings, resume from incomplete phase.

---

## Part B: Code Review (Step 7)

> **Purpose:** Perform deep code analysis using the `code-review` skill to surface correctness issues, safety concerns, and MAUI convention violations BEFORE Try-Fix explores alternatives. These findings guide Try-Fix models toward higher-quality fixes.

> **🚨 Independence-first requirement:** Step 7 MUST be invoked as a **separate sub-agent** (via the `task` tool with `agent_type: "general-purpose"`) so the code-review skill can form its assessment from the code BEFORE reading any PR narrative. The sub-agent receives ONLY the PR number — not the context gathered in Part A. This prevents anchoring bias.
>
> **Validation constraint:** The Step 7 prompt MUST NOT contain issue titles, root-cause descriptions, bug summaries, or any Part A content — only `PR #XXXXX`. If you find yourself adding context "to help" the sub-agent, you are violating independence-first.

7. **Invoke the code-review skill as a sub-agent:**

   Use the `task` tool to launch a separate agent. The prompt MUST NOT contain issue titles, root-cause descriptions, or any Part A context — only the PR number.

   ```python
   task(
     name="code-review",
     description="Code review for PR",
     agent_type="general-purpose",
     mode="sync",
     prompt="""
       Run the code-review skill for PR #XXXXX.
       Follow the full 6-step workflow in .github/skills/code-review/SKILL.md.
       Output the review in the format specified by that skill.
     """
   )
   ```

   The sub-agent internally follows the code-review skill's 6-step workflow:
   1. Gather code context (independence-first — reads code BEFORE PR description)
   2. Load MAUI review rules from `.github/skills/code-review/references/review-rules.md`
   3. Form independent assessment
   4. Reconcile with PR narrative and prior reviews
   5. Check CI status
   6. Blast radius, failure-mode probing, and verdict

**If Step 7 fails, times out, or returns malformed output:**
- Write `pre-flight/code-review.md` with: `## Code Review: SKIPPED\n\nReason: {failure description}`
- Set verdict to `SKIPPED` in the Code Review Summary section of `content.md`
- Omit `hints` from Try-Fix prompts (the `hints` field becomes optional when code review is unavailable)
- Do NOT apply the code-review hard gate in Phase 3 (Report) — treat as if code review was not run

**Store the sub-agent's full output** in `pre-flight/code-review.md` — use the exact output format from the code-review skill (do NOT reformat or summarize into a different template).

**Extract key items for Try-Fix consumption** and add to `content.md`:
- All ❌ Error findings (with file:line references)
- All ⚠️ Warning findings (with file:line references)
- Failure-mode probes and their answers
- Blast radius assessment summary
- The overall verdict and confidence level

---

## Output Files

```bash
mkdir -p CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/pre-flight
```

Write `content.md`:
```markdown
**Issue:** #{IssueNumber} - {Title}
**PR:** #{PRNumber} - {Title}
**Platforms Affected:** {platforms}
**Files Changed:** {count} implementation, {count} test

### Key Findings
- {Finding 1}
- {Finding 2}

### Code Review Summary
**Verdict:** {LGTM / NEEDS_CHANGES / NEEDS_DISCUSSION / SKIPPED}
**Confidence:** {high / medium / low / N/A}
**Errors:** {count} | **Warnings:** {count} | **Suggestions:** {count}

Key code review findings:
- {❌/⚠️/💡} {Brief finding with file:line reference}
- ...
*(If SKIPPED: "Code review sub-agent failed or timed out. Reason: {details}")*

### Fix Candidates
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| PR | PR #XXXXX | {approach} | ⏳ PENDING (Gate) | `file.cs` | Original PR |
```

Write `code-review.md` — the exact output from the code-review sub-agent, in the format specified by `.github/skills/code-review/SKILL.md` (Review Output Format section). Do NOT reformat or create a custom template — preserve the skill's native output verbatim.

---

## Common Mistakes

- ❌ Skipping the code-review step — it provides critical findings for Try-Fix
- ❌ Reading the PR description before code in Step 7 — independence-first prevents anchoring bias
- ❌ Running tests — that's the Gate phase
- ❌ Proposing fixes — save fix ideas for Try-Fix phase
