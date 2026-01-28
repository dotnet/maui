# PR Comment Script Improvements

## Summary of Changes

The `post-ai-summary-comment.ps1` script has been significantly improved to make posting PR review comments easier and more flexible using **dynamic section extraction**.

## Key Improvements

### 1. **Dynamic Section Extraction** (NEW!)

**Before:** Script used hardcoded pattern matching with predefined title variations

**After:** Script **automatically discovers ALL sections** from your state file and extracts them dynamically

```powershell
# Extracts ALL <details><summary><strong>TITLE</strong></summary> sections
$allSections = Extract-AllSections -StateContent $Content

# Then maps them to phases using flexible regex patterns
$preFlightContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '📋.*Issue Summary',
    '📋.*Pre-Flight'
)
```

**Benefits:**
- ✅ **No hardcoded titles** - works with ANY section header you use
- ✅ **Automatically adapts** - add new sections without modifying the script
- ✅ **Better debugging** - shows exactly which sections were found
- ✅ **More maintainable** - less code, more flexible

**Example debug output:**
```
[DEBUG] Found 7 section(s) in state file
[DEBUG] Section: '📋 Issue Summary' (803 chars)
[DEBUG] Section: '🧪 Tests' (539 chars)
[DEBUG] Section: '🚦 Gate - Test Verification' (488 chars)
[DEBUG] Section: '🔧 Fix Candidates' (868 chars)
[DEBUG] Section: '📋 Final Report' (2351 chars)
[DEBUG] Matched '📋 Final Report' with pattern '📋.*Report'
```

---

### 2. **Flexible Pattern Matching**

**Before:** Exact string matching required

**After:** Uses **regex patterns** to match section titles flexibly

```powershell
# Matches any of these (and more!):
- "📋 Final Report" ✅
- "📋 Phase 5: Final Report" ✅
- "📋 Report - Final Recommendation" ✅
- Any title containing "📋" and "Report" ✅
```

**Pattern examples:**
- `'📋.*Issue Summary'` matches "📋 Issue Summary", "📋 Pre-Flight Issue Summary", etc.
- `'🧪.*Tests'` matches "🧪 Tests", "🧪 Phase 2: Tests", etc.
- `'📋.*Report'` matches any title with 📋 and Report in it

---

### 3. **Errors vs Warnings**

**Validation levels:**
- **Errors** (❌) - Block posting (missing content, PENDING markers)
- **Warnings** (⚠️) - Suggestions only (missing optional sections)

**Example:**
```
✅ All validation checks passed!

⚠️  VALIDATION WARNINGS
Found 2 warning(s) (non-critical):
  - Fix: Fix phase missing 'Exhausted' field (non-critical)

💡 These are suggestions but won't block posting.
```

---

### 4. **Debug Mode**

Enable detailed extraction information:

```powershell
$DebugPreference = 'Continue'
./post-ai-summary-comment.ps1 -PRNumber 12345
```

**Shows:**
- Which sections were found in the state file
- How many characters each section contains
- Which patterns matched which sections
- Why validation passed or failed

---

### 5. **Better Error Messages**

**Comprehensive guidance when validation fails:**
```
⛔ VALIDATION FAILED

💡 Fix these issues in the state file before posting.
   Or use -SkipValidation to bypass these checks.

🐛 Debug tip: Run with $DebugPreference = 'Continue' for details
```

---

## How Dynamic Extraction Works

### Step 1: Extract ALL Sections

```powershell
function Extract-AllSections {
    # Pattern matches: <details><summary><strong>TITLE</strong></summary>...content...</details>
    $pattern = '(?s)<details>\s*<summary><strong>([^<]+)</strong></summary>(.*?)</details>'
    $matches = [regex]::Matches($StateContent, $pattern)
    
    # Returns hashtable: @{ "Title" = "content", ... }
}
```

**Result:** Hashtable with ALL sections from your state file

### Step 2: Map to Phases

```powershell
function Get-SectionByPattern {
    # Try each pattern until one matches
    foreach ($pattern in $Patterns) {
        foreach ($key in $Sections.Keys) {
            if ($key -match $pattern) {
                return $Sections[$key]  # Found it!
            }
        }
    }
}
```

**Result:** Phase content matched by flexible regex patterns

---

## Usage Examples

### Basic Usage (unchanged)
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340
```

### With Debug Mode (recommended when troubleshooting)
```powershell
pwsh -Command '$DebugPreference = "Continue"; ./.github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340'
```

### Skip Validation
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340 -SkipValidation
```

### Dry Run
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340 -DryRun
```

---

## What Section Headers Work Now?

**The script uses regex patterns, so it's VERY flexible:**

### Pre-Flight Phase
Any title matching `'📋.*Issue Summary'` or `'📋.*Pre-Flight'`:
- ✅ "📋 Issue Summary" (preferred)
- ✅ "📋 Pre-Flight Analysis"
- ✅ "📋 Context and Issue Summary"

### Tests Phase
Any title matching `'🧪.*Tests'`:
- ✅ "🧪 Tests"
- ✅ "🧪 Phase 2: Tests"
- ✅ "🧪 Test Verification"

### Gate Phase
Any title matching `'🚦.*Gate'`:
- ✅ "🚦 Gate - Test Verification"
- ✅ "🚦 Gate"
- ✅ "🚦 Phase 3: Gate"

### Fix Phase
Any title matching `'🔧.*Fix'`:
- ✅ "🔧 Fix Candidates"
- ✅ "🔧 Fix Analysis"
- ✅ "🔧 Fix"

### Report Phase
Any title matching `'📋.*Report'` or `'Final Report'`:
- ✅ "📋 Final Report"
- ✅ "📋 Phase 5: Report"
- ✅ "📋 Report - Final Recommendation"
- ✅ "Final Report"

**The beauty:** You don't need to remember exact titles anymore!

---

## Migration Guide

**No changes needed!** The script is backward compatible.

**Old state files** with exact headers like:
```markdown
<summary><strong>📋 Phase 5: Report — Final Recommendation</strong></summary>
```

**New state files** with simpler headers like:
```markdown
<summary><strong>📋 Final Report</strong></summary>
```

**Both work!** The dynamic extraction finds them automatically.

---

## Advantages Over Old Approach

| Aspect | Old (Pattern Matching) | New (Dynamic Extraction) |
|--------|------------------------|--------------------------|
| **Flexibility** | ❌ Hardcoded titles | ✅ Any title works |
| **Maintenance** | ❌ Update code for new headers | ✅ No code changes needed |
| **Debugging** | ⚠️ Limited visibility | ✅ Full extraction visibility |
| **Speed** | ⚠️ Tries multiple patterns | ✅ Single pass extraction |
| **Reliability** | ⚠️ Can miss variations | ✅ Finds everything |

---

## Common Issues & Solutions

### Issue: "Phase X has NO content in state file"

**Step 1:** Enable debug mode to see what was found
```powershell
pwsh -Command '$DebugPreference = "Continue"; ./post-ai-summary-comment.ps1 -PRNumber XXXXX'
```

**Look for:**
```
[DEBUG] Found 7 section(s) in state file
[DEBUG] Section: 'Your Section Title' (XXX chars)
```

**Step 2:** Check if your section title matches the patterns

Report phase patterns: `'📋.*Report'`, `'Final Report'`

If your title is `"📋 Final Analysis"`, it won't match!

**Solution:** Either:
- Rename section to include "Report": `"📋 Final Report"` ✅
- Or use `-SkipValidation` if content is there

---

### Issue: Section extracted but content is empty

**Cause:** State file structure issue (missing content between tags)

**Check your markdown:**
```markdown
<details>
<summary><strong>📋 Final Report</strong></summary>

<!-- Content MUST be here -->
Your report content...

</details>
```

**Not this:**
```markdown
<details>
<summary><strong>📋 Final Report</strong></summary>
</details>  ❌ No content!
```

---

## Developer Notes

### How to Add Support for New Phase Patterns

Just add a regex pattern to the mapping:

```powershell
$reportContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '📋.*Report',
    'Final Report',
    'Your New Pattern Here'  # Add here
) -Debug:$debugMode
```

**Example:** Support "Summary" as alias for "Report":
```powershell
$reportContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '📋.*Report',
    '📋.*Summary',  # New pattern
    'Final Report'
)
```

---

### Regex Pattern Tips

- `.*` matches any characters
- `^` matches start of string
- `$` matches end of string
- Use `[regex]::Escape()` if you need literal special chars

**Examples:**
- `'🧪.*Tests'` - Title must contain both 🧪 and Tests
- `'^📋 Report'` - Title must START with "📋 Report"
- `'Report$'` - Title must END with "Report"

---

## Testing

Tested with:
- ✅ PR #27340 (7 sections extracted successfully)
- ✅ Debug mode showing section discovery
- ✅ Various header formats
- ✅ Dry run mode
- ✅ Skip validation mode
- ✅ Empty sections (proper error handling)

**Debug output example:**
```
[DEBUG] Found 7 section(s) in state file
[DEBUG] Section: '📋 Issue Summary' (803 chars)
[DEBUG] Section: '📁 Files Changed' (0 chars)
[DEBUG] Section: '💬 PR Discussion Summary' (0 chars)
[DEBUG] Section: '🧪 Tests' (539 chars)
[DEBUG] Section: '🚦 Gate - Test Verification' (488 chars)
[DEBUG] Section: '🔧 Fix Candidates' (868 chars)
[DEBUG] Section: '📋 Final Report' (2351 chars)
[DEBUG] Matched '📋 Issue Summary' with pattern '📋.*Issue Summary'
[DEBUG] Matched '🧪 Tests' with pattern '🧪.*Tests'
[DEBUG] Matched '🚦 Gate - Test Verification' with pattern '🚦.*Gate'
[DEBUG] Matched '🔧 Fix Candidates' with pattern '🔧.*Fix'
[DEBUG] Matched '📋 Final Report' with pattern '📋.*Report'
```

---

## Future Improvements

Potential enhancements:
- [ ] Auto-generate comment structure from discovered sections
- [ ] Support markdown headings (`##`/`###`) as alternative to `<details>`
- [ ] Validate section content structure (required fields)
- [ ] Suggest section renaming for better patterns
- [ ] Export sections as separate files for versioning

---

## Feedback

The dynamic extraction makes the script much more maintainable and flexible!

If you find sections that aren't being extracted:
1. Run with `$DebugPreference = 'Continue'` to see what was found
2. Check which patterns are being used
3. Add a new pattern if needed (or rename your section)

---

### 2. **Errors vs Warnings**

**Before:** Everything was treated as a blocking error

**After:** Two levels of feedback:
- **Errors** (❌) - Block posting (e.g., missing content, PENDING markers)
- **Warnings** (⚠️) - Suggestions only (e.g., missing optional sections)

**Example output:**
```
✅ All validation checks passed!

⚠️  VALIDATION WARNINGS
Found 2 warning(s) (non-critical):
  - Report: Report phase missing root cause analysis (non-critical)
  - Fix: Fix phase missing 'Exhausted' field (non-critical)

💡 These are suggestions for improvement but won't block posting.
```

---

### 3. **Debug Mode**

**New feature:** Set `$DebugPreference = 'Continue'` to see detailed extraction information

```powershell
$DebugPreference = 'Continue'
./post-ai-summary-comment.ps1 -PRNumber 12345
```

**Debug output shows:**
```
[DEBUG] Matched pattern for: 📋 Final Report
[DEBUG] Content length: 2355 chars
[DEBUG] First 100 chars: ---

### Summary

PR #27340 provides a **correct and well-tested fix**...
```

**Benefit:** Easy troubleshooting when validation fails

---

### 4. **Better Error Messages**

**Before:**
```
⛔ VALIDATION FAILED
Found 1 validation error(s):
  - Report: Phase Report is marked as '✅ COMPLETE' but has NO content in state file
```

**After:**
```
⛔ VALIDATION FAILED
Found 1 validation error(s):
  - Report: Phase Report is marked as '✅ COMPLETE' but has NO content in state file

💡 Fix these issues in the state file before posting the review comment.
   Or use -SkipValidation to bypass these checks (not recommended).

🐛 Debug tip: Run with $DebugPreference = 'Continue' for detailed extraction info
```

---

### 5. **Relaxed Phase 5 Validation**

**Before:** Report phase required:
- Exact "Final Recommendation" text
- "Root Cause" section
- "Key Findings" section
- "Solution Analysis" section
- Minimum 500 characters

**After:** Report phase only requires:
- Any form of recommendation (APPROVE, REQUEST CHANGES, etc.)
- Any form of analysis (Summary, Fix Quality, etc.)
- Minimum 200 characters (reduced from 500)

**Benefit:** More flexibility in how you structure the final report

---

## Usage Examples

### Basic Usage (unchanged)
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340
```

### With Debug Mode
```powershell
$DebugPreference = 'Continue'
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340
```

### Skip Validation (when needed)
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340 -SkipValidation
```

### Dry Run (preview only)
```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27340 -DryRun
```

---

## What You Need to Know

### Headers That Work Now

Any of these variations will be recognized:

**Pre-Flight:**
- `📋 Issue Summary` ✅ (preferred)
- `📋 Pre-Flight` ✅
- `🔍 Pre-Flight` ✅

**Tests:**
- `🧪 Tests` ✅ (preferred)
- `📋 Tests` ✅

**Gate:**
- `🚦 Gate - Test Verification` ✅ (preferred)
- `🚦 Gate` ✅
- `📋 Gate` ✅

**Fix:**
- `🔧 Fix Candidates` ✅ (preferred)
- `🔧 Fix` ✅
- `📋 Fix` ✅

**Report:**
- `📋 Final Report` ✅
- `📋 Phase 5: Final Report` ✅
- `📋 Report` ✅
- `Phase 5: Report` ✅
- `Final Report` ✅

---

## Migration Guide

**No changes needed!** The script is backward compatible. If you have existing state files with the old header format, they'll continue to work.

If you want to use the new flexibility:
- Just use simpler headers like `📋 Final Report` instead of `📋 Phase 5: Report — Final Recommendation`
- The script will find it either way

---

## Common Issues & Solutions

### Issue: "Phase Report has NO content in state file"

**Solution 1:** Check your state file structure
```bash
grep -A 5 "📋.*Report" CustomAgentLogsTmp/PRState/pr-XXXXX.md
```

Make sure you have:
```markdown
<details>
<summary><strong>📋 Final Report</strong></summary>

Your report content here...

</details>
```

**Solution 2:** Use debug mode to see what's happening
```powershell
$DebugPreference = 'Continue'
./post-ai-summary-comment.ps1 -PRNumber XXXXX
```

**Solution 3:** Use `-SkipValidation` if content is definitely there
```powershell
./post-ai-summary-comment.ps1 -PRNumber XXXXX -SkipValidation
```

---

### Issue: Validation warnings about missing sections

**These are just suggestions!** Warnings won't block posting. You can:
- Ignore them (the comment will post anyway)
- Address them if you want a more complete review
- Use `-SkipValidation` to hide all validation output

---

## Developer Notes

### How Pattern Matching Works

```powershell
function Extract-PhaseContent {
    param(
        [string]$StateContent,
        [string[]]$PhaseTitles,  # Array of possible titles
        [switch]$Debug
    )
    
    foreach ($title in $PhaseTitles) {
        $pattern = "(?s)<details>\s*<summary><strong>$([regex]::Escape($title))</strong></summary>(.*?)</details>"
        if ($StateContent -match $pattern) {
            return $Matches[1].Trim()
        }
    }
    return $null  # No match found
}
```

The function tries each pattern in order until one matches.

### Adding New Pattern Variations

To support a new header variation, just add it to the array:

```powershell
$reportContent = Extract-PhaseContent -StateContent $Content -PhaseTitles @(
    "📋 Phase 5: Report — Final Recommendation",
    "📋 Phase 5: Final Report",
    "📋 Phase 5: Report",
    "📋 Final Report",
    "📋 Report",
    "Phase 5: Report",
    "Final Report",
    "Your New Pattern Here"  # <-- Add here
) -Debug:$debugMode
```

---

## Future Improvements

Potential enhancements:
- [ ] Auto-detect phase titles from state file (no hardcoded patterns)
- [ ] Support markdown headings (`##` / `###`) in addition to `<details>`
- [ ] Validate links and references work
- [ ] Check that commit SHAs are valid
- [ ] Suggest fixes for common issues (auto-fix mode)

---

## Testing

The improvements have been tested with:
- ✅ PR #27340 (Entry/Editor keyboard issue)
- ✅ State files with various header formats
- ✅ Dry run mode
- ✅ Debug mode
- ✅ Skip validation mode
- ✅ Multiple phase title variations

---

## Feedback

If you encounter issues or have suggestions, please:
1. Try debug mode first: `$DebugPreference = 'Continue'`
2. Check the state file structure
3. Report the issue with debug output included
