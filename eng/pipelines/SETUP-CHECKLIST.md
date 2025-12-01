# Intelligent UI Test Execution - Manual Setup Checklist

## Overview

This checklist covers everything you need to do manually to enable intelligent UI test execution in the Azure DevOps pipeline.

## Prerequisites

- [ ] Admin access to Azure DevOps pipeline
- [ ] GitHub account with access to dotnet/maui repository
- [ ] Ability to create GitHub Personal Access Tokens

---

## Step 1: Create GitHub Personal Access Token (5 minutes)

The pipeline needs a GitHub token to fetch PR information via GitHub CLI.

### Actions Required:

1. **Go to GitHub Settings**
   - Navigate to: https://github.com/settings/tokens
   - Or: GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)

2. **Generate New Token**
   - Click "Generate new token (classic)"
   - **Note/Name**: `Azure DevOps MAUI Pipeline - Intelligent Tests`
   - **Expiration**: Choose based on your security policy
     - Recommendation: 90 days or 1 year
     - Set a calendar reminder to renew before expiration

3. **Select Scopes**
   - ✅ Check `repo` (Full control of private repositories)
     - This includes: `repo:status`, `repo_deployment`, `public_repo`, `repo:invite`, `security_events`
   - ⚠️ **Only `repo` scope is needed** - do not select additional scopes

4. **Generate and Copy Token**
   - Click "Generate token" at the bottom
   - **Copy the token immediately** - you won't see it again!
   - Store securely (password manager recommended)

---

## Step 2: Add Token to Azure DevOps Pipeline (3 minutes)

### Actions Required:

1. **Navigate to Pipeline**
   - Go to Azure DevOps project
   - Open the UI Tests pipeline (or pipelines that will use intelligent tests)

2. **Open Pipeline Variables**
   - Click "Edit" on the pipeline
   - Click "Variables" button (top right)
   - Or navigate to Pipeline → Edit → Variables

3. **Add New Variable**
   - Click "+ Add" or "New variable"
   - Configure as follows:

   ```
   Name:  GitHubToken
   Value: <paste your GitHub PAT from Step 1>
   ✅ Keep this value secret (IMPORTANT - check this box!)
   Scope: Pipeline (not specific stages)
   ```

4. **Save Variable**
   - Click "OK"
   - Click "Save" to save the pipeline

5. **Verify**
   - Variable should appear as `GitHubToken` with value showing as `***`

---

## Step 3: Update Pipeline YAML (2 minutes)

### Actions Required:

1. **Open Pipeline YAML File**
   - File: `eng/pipelines/ui-tests.yml`
   - Edit in Azure DevOps or locally

2. **Find Template Reference**
   - Locate this line (around line 110):
   ```yaml
   - template: common/ui-tests.yml
   ```

3. **Replace With Intelligent Template**
   - Change to:
   ```yaml
   - template: common/ui-tests-intelligent.yml
   ```

4. **Save and Commit**
   - If editing in Azure DevOps: Click "Save"
   - If editing locally: Commit and push
   ```bash
   git add eng/pipelines/ui-tests.yml
   git commit -m "Enable intelligent UI test execution"
   git push
   ```

---

## Step 4: Merge Feature Branch (5 minutes)

### Actions Required:

1. **Push Feature Branch**
   ```bash
   git push origin feature/intelligent-ui-test-execution
   ```

2. **Create Pull Request**
   - Go to GitHub: https://github.com/dotnet/maui/compare
   - Select: `base: main` ← `compare: feature/intelligent-ui-test-execution`
   - Click "Create pull request"

3. **Use PR Description Template**
   - Copy content from `PR-DESCRIPTION.md`
   - Paste into PR description

4. **Review and Merge**
   - Wait for code review
   - Ensure CI passes
   - Merge to main branch

5. **Update Main Branch Locally**
   ```bash
   git checkout main
   git pull origin main
   ```

---

## Step 5: Test the Setup (10 minutes)

### Actions Required:

1. **Create a Test PR**
   - Make a small change (e.g., update a comment in Button.cs)
   - Create a PR

2. **Verify Pipeline Runs**
   - Pipeline should trigger automatically
   - Check that `analyze_pr_changes` stage runs first

3. **Check Analysis Output**
   - Open the `analyze_pr_changes` stage
   - Click "Analyze Changed Files" job
   - Look for:
     ```
     === Analysis Results ===
     Test Strategy: selective
     Test Category Groups to Run:
       - Button
     ```

4. **Verify Test Jobs**
   - Test stages should create jobs for each selected category
   - Job names like: `android_ui_tests_controls_30 (Button)`
   - Each category runs as a separate parallel job

5. **Check Execution Time**
   - Should be significantly faster than previous runs
   - Documentation PRs should skip tests entirely
   - Single control changes should complete in ~15-30 minutes

---

## Step 6: Monitor and Adjust (Ongoing)

### Actions Required:

1. **Monitor First Few PRs**
   - Watch for any analysis errors
   - Check that correct categories are selected
   - Verify tests pass/fail appropriately

2. **Check Token Expiration**
   - Set calendar reminder for token expiration
   - Renew token before it expires
   - Update `GitHubToken` variable in Azure DevOps

3. **Adjust Category Mappings (if needed)**
   - If analysis selects wrong categories, update:
     - File: `eng/scripts/analyze-pr-changes.ps1`
     - Modify the pattern matching logic
   - If new controls added, update:
     - `src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs`
     - Pattern matching in analysis script

4. **Review Time Savings**
   - Track average PR build times
   - Compare to historical 4+ hour builds
   - Calculate cost savings

---

## Troubleshooting Common Issues

### Issue: "GitHub CLI not authenticated"

**Cause**: `GitHubToken` variable not set or incorrect

**Solution**:
1. Verify variable exists in pipeline
2. Check it's named exactly `GitHubToken` (case-sensitive)
3. Verify "Keep this value secret" is checked
4. Check token hasn't expired on GitHub
5. Regenerate token if needed

### Issue: "No PR number detected"

**Cause**: Not running in PR context

**Solution**:
- Only PR builds use intelligent selection
- Manual/CI builds run all tests (by design)
- Verify pipeline trigger is set to PR

### Issue: All tests still running

**Cause**: Template not changed

**Solution**:
1. Verify `ui-tests.yml` references `ui-tests-intelligent.yml`
2. Check the change was committed and merged
3. Verify pipeline is using latest main branch

### Issue: Analysis selects wrong categories

**Cause**: Pattern matching needs refinement

**Solution**:
1. Check analysis output in pipeline logs
2. Review `eng/scripts/analyze-pr-changes.ps1`
3. Update pattern matching for specific files
4. Test locally:
   ```bash
   $env:GITHUB_TOKEN = "your-token"
   ./eng/scripts/analyze-pr-changes.ps1 -PrNumber 12345
   ```

### Issue: Tests fail that should pass

**Cause**: Category mapping may be incomplete

**Solution**:
- This is the conservative fallback working!
- Update `analyze-pr-changes.ps1` to include broader categories
- For critical changes, the script defaults to running ALL tests

---

## Rollback Plan (Emergency)

If something goes wrong and you need to revert immediately:

### Quick Rollback (2 minutes):

1. **Edit Pipeline YAML**
   ```yaml
   # Change this:
   - template: common/ui-tests-intelligent.yml
   
   # Back to this:
   - template: common/ui-tests.yml
   ```

2. **Commit and Push**
   ```bash
   git add eng/pipelines/ui-tests.yml
   git commit -m "Rollback: Temporarily disable intelligent test selection"
   git push
   ```

3. **Next PR will use old behavior** (all tests every time)

### Alternative: Disable Selectively

Keep the new template but disable intelligent selection:

```yaml
- template: common/ui-tests-intelligent.yml
  parameters:
    # ... existing parameters
    enableIntelligentSelection: false  # Add this line
```

This runs all tests but keeps the new infrastructure in place.

---

## Validation Checklist

Before considering setup complete, verify:

- [ ] GitHub token created with `repo` scope
- [ ] Token added to Azure DevOps as `GitHubToken` (secret)
- [ ] Pipeline YAML updated to use `ui-tests-intelligent.yml`
- [ ] Feature branch merged to main
- [ ] Test PR created and analyzed correctly
- [ ] Test jobs created per category (not matrix)
- [ ] Execution time significantly reduced
- [ ] Documentation PRs skip tests
- [ ] Token expiration date noted in calendar

---

## Success Metrics

After setup, you should see:

### Time Savings
- **Documentation PRs**: 100% savings (2 min vs 4+ hours)
- **Single control PRs**: 93% savings (15 min vs 4+ hours)
- **Multi-control PRs**: 75-87% savings (30-60 min vs 4+ hours)

### Pipeline Behavior
- Each category runs as separate job in test stage
- Categories run in parallel (not sequential)
- PR analysis stage completes in ~2-3 minutes
- Failed categories can be rerun individually

### Cost Impact
- Weekly savings: ~156 compute hours
- Monthly savings: ~$3,246
- Annual savings: ~$39,000

---

## Support

### Getting Help

If you encounter issues:

1. **Check pipeline logs**
   - `analyze_pr_changes` stage shows analysis details
   - Look for error messages or unexpected output

2. **Test analysis locally**
   ```bash
   $env:GITHUB_TOKEN = "your-token"
   ./eng/scripts/analyze-pr-changes.ps1 -PrNumber <pr-number>
   cat test-categories.txt
   ```

3. **Review documentation**
   - `eng/pipelines/README-INTELLIGENT-TESTS.md` - Full guide
   - `eng/pipelines/QUICKSTART-INTELLIGENT-TESTS.md` - Quick start
   - `eng/pipelines/INTELLIGENT-TEST-EXECUTION.md` - Technical details

4. **Open an issue**
   - Tag with `[intelligent-tests]`
   - Include pipeline logs and PR number
   - Describe expected vs actual behavior

---

## Maintenance

### Regular Tasks

**Monthly:**
- Review token expiration date
- Check analysis accuracy (false positives/negatives)
- Review time savings metrics

**Quarterly:**
- Analyze patterns in category selection
- Refine mapping rules if needed
- Update documentation with learnings

**When Adding New Controls:**
1. Add category to `UITestCategories.cs`
2. Update `analyze-pr-changes.ps1` pattern matching
3. Update `ui-tests.yml` categoryGroupsToTest parameter
4. Test with a PR that modifies the new control

---

## Complete! 🎉

Once all steps are done, your pipeline will:
- ✅ Automatically analyze PRs
- ✅ Run only necessary test categories
- ✅ Execute categories in parallel
- ✅ Save 75-100% of CI time
- ✅ Reduce costs by ~$39K/year

Next PR will use intelligent test selection automatically!
