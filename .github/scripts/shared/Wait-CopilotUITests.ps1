<#
.SYNOPSIS
  Poll an AzDO build until it reaches a terminal state, then return its
  final status.

.DESCRIPTION
  Used by Review-PR.ps1 STEP 8 to block on the child UI-tests pipeline
  queued by STEP 3 (via Queue-CopilotUITests.ps1) before posting the
  final AI summary comment.

  Polls every PollSeconds (default 60). Logs progress with stage-level
  pass/fail counts so the parent log shows useful breadcrumbs while we
  wait. Stops when:
    * Build reaches state "completed" (returns the build object)
    * MaxWaitMinutes has elapsed (returns $null + warning)

  The caller is responsible for downloading artifacts and parsing TRX
  files (see Aggregate-UITestArtifacts.ps1).

.PARAMETER BuildId
  AzDO build ID returned by Queue-CopilotUITests.ps1.

.PARAMETER MaxWaitMinutes
  Hard cap. Defaults to 240 — matches the per-job timeout in
  common/ui-tests.yml so we never block longer than the child build itself
  could take.

.PARAMETER PollSeconds
  Interval between polls. 60 is a safe default — AzDO's build API is
  rate-limited and most UI-test jobs take at least 10 minutes.

.OUTPUTS
  PSCustomObject with .id .status .result .url, OR $null on timeout.
#>
param(
    [Parameter(Mandatory=$true)]
    [int]$BuildId,

    [int]$MaxWaitMinutes = 240,

    [int]$PollSeconds = 60,

    [string]$Org = "https://devdiv.visualstudio.com",
    [string]$Project = "DevDiv"
)

$ErrorActionPreference = 'Stop'

if ($BuildId -le 0) {
    Write-Host "Wait-CopilotUITests: invalid BuildId '$BuildId'" -ForegroundColor Yellow
    return $null
}
if ($PollSeconds -lt 5) { $PollSeconds = 5 }

$start = Get-Date
$deadline = $start.AddMinutes($MaxWaitMinutes)
$lastSummary = ""
$consecutiveErrors = 0

Write-Host "Wait-CopilotUITests: polling build #$BuildId every ${PollSeconds}s (cap=${MaxWaitMinutes}m)" -ForegroundColor Cyan

while ((Get-Date) -lt $deadline) {
    $build = $null
    try {
        $azOut = az pipelines runs show --org $Org --project $Project --id $BuildId -o json 2>&1
        if ($LASTEXITCODE -ne 0) { throw "az exit $LASTEXITCODE — $($azOut -join "`n")" }
        $build = $azOut | ConvertFrom-Json
        $consecutiveErrors = 0
    } catch {
        $consecutiveErrors++
        Write-Host "  ⚠️  poll failed (attempt $consecutiveErrors): $_" -ForegroundColor Yellow
        if ($consecutiveErrors -ge 5) {
            Write-Host "Wait-CopilotUITests: 5 consecutive poll failures, giving up" -ForegroundColor Red
            return $null
        }
        Start-Sleep -Seconds $PollSeconds
        continue
    }

    $status = $build.status
    $result = $build.result
    $elapsedMin = [math]::Round(((Get-Date) - $start).TotalMinutes, 1)

    # Lightweight stage breakdown via the timeline API — cheap to call once
    # per minute and gives the caller something useful in the log.
    $stageSummary = ""
    try {
        $tl = az devops invoke `
            --org $Org `
            --route-parameters project=$Project buildId=$BuildId `
            --area build --resource Timeline --api-version 7.1 `
            -o json 2>$null | ConvertFrom-Json
        if ($tl -and $tl.records) {
            $stages = $tl.records | Where-Object { $_.type -eq 'Stage' }
            $sCompleted = @($stages | Where-Object { $_.state -eq 'completed' }).Count
            $sRunning   = @($stages | Where-Object { $_.state -eq 'inProgress' }).Count
            $sPending   = @($stages | Where-Object { $_.state -eq 'pending' }).Count
            $sTotal     = $stages.Count
            $stageSummary = "stages: $sCompleted/$sTotal completed (running=$sRunning, pending=$sPending)"
        }
    } catch {
        # non-fatal; just log the headline status
    }

    $line = "  [+${elapsedMin}m] status=$status result=$result $stageSummary"
    if ($line -ne $lastSummary) {
        Write-Host $line -ForegroundColor Gray
        $lastSummary = $line
    }

    if ($status -eq 'completed') {
        Write-Host "Wait-CopilotUITests: build #$BuildId reached terminal state '$result' after ${elapsedMin}m" -ForegroundColor Green
        return $build
    }
    if ($status -eq 'cancelling') {
        # AzDO sometimes parks here for a few minutes — keep polling
    }

    Start-Sleep -Seconds $PollSeconds
}

Write-Host "Wait-CopilotUITests: cap of ${MaxWaitMinutes}m hit — build #$BuildId still status='$($build.status)' result='$($build.result)'" -ForegroundColor Yellow
return $null
