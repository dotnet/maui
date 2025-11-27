# Intelligent UI Test Execution

## Overview

This system implements intelligent UI test execution for Azure DevOps pipelines by analyzing PR changes and determining which test categories need to run. This significantly reduces CI time and resource usage while maintaining test coverage.

## How It Works

### 1. PR Analysis Stage

When a PR is created or updated, the pipeline:

1. **Checks out the repository** with full history
2. **Installs GitHub CLI** (`gh`) if not present
3. **Authenticates** using `GITHUB_TOKEN` environment variable
4. **Fetches changed files** using `gh pr view --json files`
5. **Analyzes the changes** using rule-based logic to map files to test categories
6. **Outputs test categories** to run based on the analysis

### 2. Change Analysis Logic

The analysis script (`eng/scripts/analyze-pr-changes.ps1`) uses intelligent mapping:

#### Documentation Only → No Tests
- Files: `*.md`, `docs/*`, `LICENSE*`, `README.md`, etc.
- Action: Skip all UI tests

#### Core Framework Changes → All Tests
- Files: `src/Core/*`, `src/Controls/src/Core/Layout/*`, `VisualElement*`, `Element*`
- Action: Run ALL test categories (core affects everything)
- Reasoning: Core changes can have wide-reaching effects

#### Control-Specific Changes → Targeted Tests
- Files: `Button.cs`, `Label/`, `Entry.Android.cs`
- Action: Run tests for that specific control
- Example: `Button.cs` → Run `Button` category

#### Platform-Specific Changes → Platform Tests
- Files: `*.Android.cs`, `*.iOS.cs`, `*.Windows.cs`, `*/MacCatalyst/*`
- Action: Include broader coverage for affected platform
- Includes: `ViewBaseTests` and related controls

#### Handler Changes → Control-Specific Tests
- Files: `*Handler.cs`, `*Handler.Android.cs`
- Action: Map to the specific control or all tests if uncertain

#### Test Infrastructure → Selective Tests
- Files: `*.Tests/*`, `TestCases.HostApp/Issues/*`
- Action: Run affected test categories based on test file path

### 3. Dynamic Test Execution

The pipeline uses the analysis results to:

1. **Skip build stages** if no tests needed (docs-only changes)
2. **Run only necessary test categories** for selective execution
3. **Run all test categories** for core framework changes
4. **Set Azure DevOps variables** for downstream stages to use

### 4. Test Category Mapping

Based on `src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs`:

| File Pattern | Test Categories |
|-------------|----------------|
| `Button` | Button |
| `Label` | Label |
| `Entry` | Entry |
| `Editor` | Editor |
| `CollectionView` | CollectionView |
| `CarouselView` | CarouselView |
| `Shell` | Shell, Navigation |
| `Layout` | Layout, ViewBaseTests |
| `SafeArea` | SafeAreaEdges |
| `*Handler` | Related control or ALL |
| `src/Core/*` | ALL categories |

## Configuration

### Required Secrets/Variables

1. **GITHUB_TOKEN** or **GH_TOKEN**: Personal Access Token for GitHub CLI
   - Scope: `repo` (to read PR data)
   - Configure in Azure DevOps pipeline variables (mark as secret)

2. **GitHubToken**: Variable name used in pipeline
   - Should be mapped to GITHUB_TOKEN
   - Used by `analyze-pr-changes.ps1` script

### Pipeline Parameters

```yaml
parameters:
  # Enable/disable intelligent test selection
  enableIntelligentSelection: true
  
  # Category groups to test (populated dynamically from PR analysis)
  categoryGroupsToTest: []
```

## Files Modified/Created

### New Files

1. **`.github/agents/pipeline-optimizer-agent.yml`**
   - Custom agent definition for future AI-powered analysis
   - Provides guidance for intelligent category mapping
   - Can be integrated with GitHub Copilot CLI for enhanced analysis

2. **`eng/scripts/analyze-pr-changes.ps1`**
   - PowerShell script that performs the PR analysis
   - Installs GitHub CLI if needed
   - Fetches changed files from PR
   - Maps changes to test categories
   - Outputs results for pipeline consumption

3. **`eng/pipelines/common/ui-tests-intelligent.yml`**
   - Modified pipeline template with intelligent test execution
   - Adds `analyze_pr_changes` stage
   - Uses dynamic test category matrix
   - Includes conditional stage execution

4. **`eng/pipelines/INTELLIGENT-TEST-EXECUTION.md`**
   - This documentation file

### Modified Files

None yet - this is a new feature. To enable:

1. Update `eng/pipelines/ui-tests.yml` to use `ui-tests-intelligent.yml` template
2. Add `GitHubToken` variable to Azure DevOps pipeline

## Usage

### Enabling Intelligent Test Execution

1. **Add GitHub Token** to Azure DevOps pipeline variables:
   ```
   Name: GitHubToken
   Value: <your-github-pat>
   Secret: Yes
   ```

2. **Update pipeline** to use intelligent template:
   ```yaml
   # In eng/pipelines/ui-tests.yml
   stages:
     - template: common/ui-tests-intelligent.yml
       parameters:
         # ... existing parameters
         enableIntelligentSelection: true
   ```

3. **Commit and push** - pipeline will automatically use intelligent selection for PRs

### Testing Locally

You can test the analysis script locally:

```powershell
# Set your GitHub token
$env:GITHUB_TOKEN = "your-token-here"

# Run analysis for a PR
./eng/scripts/analyze-pr-changes.ps1 -PrNumber 12345

# View results
cat test-categories.txt
```

### Disabling for Specific PRs

To run all tests for a specific PR (override intelligent selection):

1. Add `[run-all-tests]` to PR title or description
2. Pipeline will detect and run full test suite

## Expected Benefits

### Time Savings

- **Documentation PRs**: ~100% time savings (skip all UI tests)
- **Focused control changes**: ~80-90% time savings (run 1-3 categories vs 19)
- **Platform-specific changes**: ~50-70% time savings (platform subset)
- **Core framework changes**: 0% savings (run all tests for safety)

### Example Scenarios

| Change Type | Files Changed | Categories Selected | Time Saved |
|------------|---------------|-------------------|-----------|
| Button fix | `Button.cs`, `Button.Android.cs` | Button only | ~95% |
| Label + Entry | `Label.cs`, `Entry.iOS.cs` | Label, Entry | ~90% |
| Layout engine | `src/Core/Layouts/*` | ALL categories | 0% (safety) |
| Documentation | `README.md`, `docs/button.md` | None | 100% |
| SafeArea fix | `SafeAreaInsets.cs` | SafeAreaEdges | ~95% |

### Cost Savings

With ~50 PR builds per week:
- Average PR build time: 4 hours (all platforms, all categories)
- With intelligent selection: ~1-2 hours average
- Weekly savings: ~100-150 build hours
- Monthly savings: ~400-600 build hours

## Monitoring and Debugging

### View Analysis Results

In Azure DevOps pipeline:

1. Navigate to the PR pipeline run
2. Open the `analyze_pr_changes` stage
3. View `Analyze PR Changes` job
4. Check `Display Analysis Results` task for details

### Debug Analysis Script

Add debugging to the script:

```powershell
# In analyze-pr-changes.ps1, add verbose output
$VerbosePreference = "Continue"
Write-Verbose "File: $file, Categories: $($categories.Keys -join ',')"
```

### Override Analysis

If analysis produces incorrect results, you can:

1. **Manual override**: Edit `test-categories.txt` artifact before test stages run
2. **Disable for PR**: Add `[run-all-tests]` to PR
3. **Fix mapping**: Update analysis logic in `analyze-pr-changes.ps1`

## Future Enhancements

### 1. AI-Powered Analysis

Integrate with GitHub Copilot CLI custom agent:
- More sophisticated change impact analysis
- Learning from historical test failures
- Predictive modeling for test category selection

### 2. Historical Data

- Track which categories fail most often for certain changes
- Use failure patterns to improve category selection
- Build confidence scores for selective execution

### 3. Parallel Test Execution

- Run selected categories in parallel instead of matrix
- Further reduce overall execution time
- Requires pipeline restructuring

### 4. Granular Test Selection

- Select individual test methods instead of categories
- Requires deeper test impact analysis
- Maximum time savings potential

### 5. Cross-PR Learning

- Learn from test results across multiple PRs
- Identify patterns in change → failure mappings
- Continuously improve selection accuracy

## Troubleshooting

### GitHub CLI Not Found

**Error**: `GitHub CLI (gh) not found`

**Solution**: Ensure the agent has internet access to download `gh`. If not, pre-install on agent image.

### Authentication Failed

**Error**: `GitHub CLI is not authenticated`

**Solution**: 
1. Verify `GitHubToken` variable is set in pipeline
2. Check token has `repo` scope
3. Ensure token is not expired

### No Tests Selected

**Issue**: Analysis selects no tests when it should

**Solution**:
1. Check analysis output in pipeline logs
2. Verify file paths match expected patterns
3. Update mapping logic in `analyze-pr-changes.ps1`

### Too Many Tests Selected

**Issue**: Analysis selects all tests unnecessarily

**Solution**:
1. Review file change patterns
2. Refine mapping rules to be more specific
3. Consider if core changes actually warrant full suite

### Pipeline Variable Not Set

**Error**: `TestCategoryGroups` variable not available in test stages

**Solution**:
1. Ensure `analyze_pr_changes` stage completed successfully
2. Check `AnalyzePR` task output variables
3. Verify stage dependencies are correct

## Contributing

When updating test categories or pipeline structure:

1. **Update UITestCategories.cs** first
2. **Update analysis script** mapping logic
3. **Update pipeline templates** with new category groups
4. **Update this documentation** with changes
5. **Test locally** before submitting PR

## References

- [GitHub CLI Documentation](https://cli.github.com/)
- [Azure DevOps Output Variables](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/variables#set-variables-in-scripts)
- [MAUI UI Testing Guide](.github/instructions/uitests.instructions.md)
- [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs)
