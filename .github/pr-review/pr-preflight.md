# PR Pre-Flight — Context Gathering

> **SCOPE:** Document only. No code analysis. No fix opinions. No running tests.

---

## Steps

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

## Output File

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

### Fix Candidates
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| PR | PR #XXXXX | {approach} | ⏳ PENDING (Gate) | `file.cs` | Original PR |
```

---

## Common Mistakes

- ❌ Researching root cause — save for Try-Fix phase
- ❌ Looking at implementation code — just gather context
- ❌ Running tests — that's the Gate phase
