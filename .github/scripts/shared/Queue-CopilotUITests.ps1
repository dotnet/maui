<#
.SYNOPSIS
  Queue the ci-copilot-uitests child pipeline with the categories
  detected by detect-ui-test-categories.ps1.

.DESCRIPTION
  Used by Review-PR.ps1 STEP 3 (CI mode) to spawn a separate AzDO pipeline
  run that mirrors CI pipeline 313's Build UITests Sample App + per-platform
  matrix-based test stages, instead of running tests in-process on the
  Copilot agent.

  The matrix in common/ui-tests.yml is a COMPILE-TIME expansion over the
  categoryGroupsToTest parameter, so we have to pass the detected list at
  queue time. We do that by serialising it as a JSON array and feeding it
  via `--parameters categoryGroupsToTest=<json>`.

  Returns the queued build's ID (integer). Caller is responsible for
  polling completion (see Wait-CopilotUITests.ps1).

.PARAMETER PRNumber
  The pull request being reviewed. Stamped into the child pipeline's
  build name and used to scope artifact downloads later.

.PARAMETER PRBranch
  The merge-commit SHA (or branch ref) that Copilot reviewed. Passed to
  the child build's `--commit-id` so all matrix jobs see the exact tree
  the parent reviewed.

.PARAMETER Platform
  android | ios | windows | catalyst — passed straight through to the
  child pipeline's targetPlatform parameter so only the matching stages
  expand.

.PARAMETER Categories
  Comma-separated list of detected categories from
  detect-ui-test-categories.ps1's UITestCategoryList output. Empty or
  'NONE' makes this function return $null without queueing.

.PARAMETER PipelineId
  AzDO numeric pipeline ID for ci-copilot-uitests. Defaults to the
  COPILOT_UITESTS_PIPELINE_ID env var, then a hard-coded fallback set
  during pipeline registration.

.PARAMETER Org / Project / SourceBranch
  Match the parent ci-copilot.yml pipeline configuration (DevDiv).

.OUTPUTS
  [int] queued build ID, or $null when nothing was queued.
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$PRNumber,

    [Parameter(Mandatory=$true)]
    [string]$PRBranch,

    [Parameter(Mandatory=$true)]
    [ValidateSet('android','ios','windows','catalyst')]
    [string]$Platform,

    [Parameter(Mandatory=$true)]
    [AllowEmptyString()]
    [string]$Categories,

    [int]$PipelineId = 0,

    [string]$Org = "https://devdiv.visualstudio.com",
    [string]$Project = "DevDiv",
    [string]$SourceBranch = "feature/regression-check"
)

$ErrorActionPreference = 'Stop'

# --------- Validate inputs ----------
if ([string]::IsNullOrWhiteSpace($Categories) -or $Categories -eq 'NONE') {
    Write-Host "Queue-CopilotUITests: nothing to queue (Categories='$Categories')" -ForegroundColor DarkGray
    return $null
}

if ($PipelineId -le 0) {
    if ($env:COPILOT_UITESTS_PIPELINE_ID) {
        $PipelineId = [int]$env:COPILOT_UITESTS_PIPELINE_ID
    }
}
if ($PipelineId -le 0) {
    Write-Host "Queue-CopilotUITests: COPILOT_UITESTS_PIPELINE_ID env var not set and no -PipelineId provided; cannot queue child pipeline" -ForegroundColor Yellow
    return $null
}

# --------- Build the categories JSON array ----------
# AzDO `--parameters` for an `object` parameter wants a JSON literal as the
# parameter value. ConvertTo-Json on a single-element array drops the [],
# so force depth+array semantics.
$catList = @($Categories -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
if ($catList.Count -eq 0) {
    Write-Host "Queue-CopilotUITests: Categories parsed to empty list, nothing to queue" -ForegroundColor Yellow
    return $null
}
$catJson = ConvertTo-Json -InputObject $catList -Compress -Depth 5
# Single-element list normalisation
if ($catList.Count -eq 1 -and -not $catJson.StartsWith('[')) {
    $catJson = "[$catJson]"
}

Write-Host "Queue-CopilotUITests: queueing pipeline #$PipelineId for PR #$PRNumber on '$Platform'" -ForegroundColor Cyan
Write-Host "  Categories ($($catList.Count)): $($catList -join ', ')" -ForegroundColor Gray
Write-Host "  PRBranch:    $PRBranch" -ForegroundColor Gray
Write-Host "  Source:      $SourceBranch" -ForegroundColor Gray

# --------- Queue via az pipelines run ----------
# We pass parameters as KEY=VALUE. Object/typed parameters (like
# categoryGroupsToTest) accept JSON via this same KEY=<json> syntax.
$paramArgs = @(
    "PRNumber=$PRNumber",
    "PRBranch=$PRBranch",
    "targetPlatform=$Platform",
    "categoryGroupsToTest=$catJson"
)

# Retry up to 3 times — AzDO API has rare transient 502s.
$queuedBuildId = $null
$attempts = 0
$lastErr = $null
while ($attempts -lt 3 -and -not $queuedBuildId) {
    $attempts++
    try {
        # `az pipelines run` returns the queued build object as JSON.
        $azOut = az pipelines run `
            --org $Org `
            --project $Project `
            --id $PipelineId `
            --branch $SourceBranch `
            --parameters @paramArgs `
            -o json 2>&1
        if ($LASTEXITCODE -ne 0) {
            $lastErr = "az exit $LASTEXITCODE — $($azOut -join "`n")"
            Write-Host "  Attempt $attempts failed: $lastErr" -ForegroundColor Yellow
            Start-Sleep -Seconds (5 * $attempts)
            continue
        }
        $build = $azOut | ConvertFrom-Json
        if ($build.id) {
            $queuedBuildId = [int]$build.id
            Write-Host "  ✅ Queued build #$queuedBuildId — $($build.url)" -ForegroundColor Green
        } else {
            $lastErr = "az returned no build id: $($azOut -join "`n")"
            Write-Host "  Attempt ${attempts}: $lastErr" -ForegroundColor Yellow
        }
    } catch {
        $lastErr = "exception: $_"
        Write-Host "  Attempt $attempts threw: $_" -ForegroundColor Yellow
        Start-Sleep -Seconds (5 * $attempts)
    }
}

if (-not $queuedBuildId) {
    Write-Host "Queue-CopilotUITests: failed to queue after $attempts attempts. Last error: $lastErr" -ForegroundColor Red
    return $null
}

return $queuedBuildId
