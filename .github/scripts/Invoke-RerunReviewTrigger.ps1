#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates AI rerun-scanner decisions and emits a deterministic action list.

.DESCRIPTION
    This script does NOT talk to GitHub or Azure DevOps. It parses the agent's
    batched `decisions`, validates every decision against the deterministic
    candidate set (`candidates.json`), and writes a normalized actions array to
    the path in $env:RERUN_ACTIONS_PATH.

    A downstream `actions/github-script` step consumes that file and performs the
    side effects via the GitHub REST API (octokit):

      * trigger -> dispatch the `review-trigger.yml` workflow (the exact same
        workflow a maintainer `/review` comment runs), then react `rocket` (🚀)
        to the rerun comment to acknowledge the dispatch was queued.
        `review-trigger.yml` owns PR validation, the
        `s/agent-review-in-progress` lock, platform inference, OIDC, and the AzDO
        pipeline trigger.
      * skip    -> react `-1` to the rerun comment and remove the queue label.

    Splitting validation (here) from I/O (github-script) keeps this logic unit
    testable and routes every GitHub write through octokit, which works reliably
    in the gh-aw safe-output job context (the `gh` CLI does not — it returns a
    spurious HTTP 404 for `repos/.../pulls/N` there).
#>

param(
    [string]$DefaultPipelineRef = 'main'
)

$ErrorActionPreference = 'Stop'

function ConvertTo-TrimmedString {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return ([string]$Value).Trim()
}

function ConvertTo-SafeLogValue {
    param(
        [string]$Value,
        [int]$MaxLength = 180
    )

    $safe = ([string]$Value) -replace '[\r\n]+', ' '
    $safe = $safe -replace '::', ': :'
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaxLength) {
        $safe = $safe.Substring(0, $MaxLength - 3) + '...'
    }

    return $safe
}

function Expand-RerunDecisionItems {
    param([object[]]$Items)

    # A custom safe-output job is capped at one invocation per run, so the agent
    # batches every candidate's decision into a single item's `decisions` field
    # (a JSON array, or array of objects). Expand that into one object per
    # decision. Items that already carry scalar decision fields (legacy shape or
    # a single decision) are passed through unchanged for back-compatibility.
    $expanded = [System.Collections.Generic.List[object]]::new()
    foreach ($item in $Items) {
        $rawDecisions = $item.PSObject.Properties['decisions']
        if (-not $rawDecisions -or $null -eq $rawDecisions.Value) {
            if ($item.PSObject.Properties['pr_number']) {
                $expanded.Add($item)
            }
            continue
        }

        $value = $rawDecisions.Value
        $parsed = $null
        if ($value -is [string]) {
            if ([string]::IsNullOrWhiteSpace($value)) { continue }
            try {
                $parsed = $value | ConvertFrom-Json
            } catch {
                Write-Host "::warning::Skipping unparseable decisions payload: $($_.Exception.Message)"
                continue
            }
        } else {
            $parsed = $value
        }

        foreach ($decision in @($parsed)) {
            if ($null -ne $decision) {
                $expanded.Add($decision)
            }
        }
    }

    return $expanded.ToArray()
}

function Get-AgentItems {
    if (-not $env:GH_AW_AGENT_OUTPUT -or -not (Test-Path $env:GH_AW_AGENT_OUTPUT)) {
        throw "GH_AW_AGENT_OUTPUT is missing or does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $env:GH_AW_AGENT_OUTPUT | ConvertFrom-Json
    $triggerItems = @($payload.items | Where-Object { $_.type -eq 'trigger_rerun_review' })
    return Expand-RerunDecisionItems -Items $triggerItems
}

function Get-CandidateItems {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "RERUN_CANDIDATES_PATH is required."
    }
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Rerun candidates file '$Path' does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    return @($payload.candidates)
}

function Get-MatchingCandidate {
    param(
        [object[]]$Candidates,
        [Parameter(Mandatory = $true)][int]$PRNumber
    )

    return @($Candidates | Where-Object { [int]$_.prNumber -eq $PRNumber } | Select-Object -First 1)
}

function Get-DeterministicRerunDecision {
    param([Parameter(Mandatory = $true)]$Candidate)

    $isDraftProperty = $Candidate.PSObject.Properties['isDraft']
    if (-not $isDraftProperty -or $isDraftProperty.Value -isnot [bool]) {
        throw "Candidate is missing a boolean isDraft value."
    }

    $activityProperty = $Candidate.PSObject.Properties['activity']
    if (-not $activityProperty -or $null -eq $activityProperty.Value) {
        throw "Candidate is missing deterministic activity metadata."
    }
    $activity = $activityProperty.Value

    $headChangedProperty = $activity.PSObject.Properties['headChanged']
    if (-not $headChangedProperty -or $headChangedProperty.Value -isnot [bool]) {
        throw "Candidate activity is missing a boolean headChanged value."
    }

    $counts = @{}
    foreach ($field in @('newAuthorCommentCount', 'newCommitCount')) {
        $property = $activity.PSObject.Properties[$field]
        $raw = if ($property) { ConvertTo-TrimmedString $property.Value } else { '' }
        if ($raw -notmatch '^\d+$') {
            throw "Candidate activity is missing a non-negative integer $field value."
        }
        $counts[$field] = [Int64]$raw
    }

    $hasActivity = [bool]$headChangedProperty.Value -or
        $counts.newAuthorCommentCount -gt 0 -or
        $counts.newCommitCount -gt 0
    if ([bool]$isDraftProperty.Value -or -not $hasActivity) {
        return 'skip'
    }
    return 'trigger'
}

function Get-PlatformFromLabels {
    param([string[]]$Labels, [string]$Fallback)

    $validPlatforms = @('android', 'ios', 'catalyst', 'windows')
    if ($Fallback -and $validPlatforms -contains $Fallback.ToLowerInvariant()) {
        return $Fallback.ToLowerInvariant()
    }

    $lower = @($Labels | ForEach-Object { $_.ToLowerInvariant() })
    if ($lower -contains 'platform/ios') { return 'ios' }
    if ($lower -contains 'platform/macos' -or $lower -contains 'platform/maccatalyst') { return 'catalyst' }
    if ($lower -contains 'platform/android') { return 'android' }
    if ($lower -contains 'platform/windows') { return 'windows' }
    return 'android'
}

function Normalize-PipelineRef {
    param([string]$Value, [string]$Fallback = 'main')

    $pipelineRef = if ([string]::IsNullOrWhiteSpace($Value)) { $Fallback } else { ([string]$Value).Trim() }
    $pipelineRef = $pipelineRef -replace '^refs/heads/', ''
    $pipelineRef = $pipelineRef -replace '[^a-zA-Z0-9/_.\-]', ''
    if ([string]::IsNullOrWhiteSpace($pipelineRef)) {
        return $Fallback
    }
    if ($pipelineRef -match '\.\.' -or $pipelineRef -match '//' -or $pipelineRef.EndsWith('/') -or $pipelineRef.StartsWith('/')) {
        return $Fallback
    }
    return $pipelineRef
}

function Get-RerunActions {
    param(
        [object[]]$Items,
        [object[]]$Candidates,
        [string]$DefaultPipelineRef = 'main'
    )

    # Returns @{ Actions = <object[]>; HadFailure = <bool> }. Each action is a
    # normalized, validated decision ready for the github-script I/O step.
    $actions = [System.Collections.Generic.List[object]]::new()
    $hadFailure = $false
    $decisionNumberCounts = @{}
    $decisionPRNumberSet = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($item in $Items) {
        $raw = ConvertTo-TrimmedString $item.pr_number
        $parsed = 0
        if ($raw -match '^[1-9]\d*$' -and [int]::TryParse($raw, [ref]$parsed)) {
            [void]$decisionPRNumberSet.Add($parsed)
            if ($decisionNumberCounts.ContainsKey($parsed)) {
                $decisionNumberCounts[$parsed]++
            } else {
                $decisionNumberCounts[$parsed] = 1
            }
        }
    }
    $duplicatePRNumbers = @($decisionNumberCounts.GetEnumerator() |
        Where-Object { $_.Value -gt 1 } |
        ForEach-Object { [int]$_.Key } |
        Sort-Object)
    if ($duplicatePRNumbers.Count -gt 0) {
        foreach ($duplicatePRNumber in $duplicatePRNumbers) {
            Write-Host "::error::Duplicate agent decisions were emitted for PR #$duplicatePRNumber."
        }
        return @{
            Actions    = @()
            HadFailure = $true
        }
    }

    $candidatePRNumberSet = [System.Collections.Generic.HashSet[int]]::new()
    $hasInvalidCandidateNumber = $false
    foreach ($candidate in $Candidates) {
        $raw = ConvertTo-TrimmedString $candidate.prNumber
        $parsed = 0
        if ($raw -notmatch '^[1-9]\d*$' -or -not [int]::TryParse($raw, [ref]$parsed)) {
            Write-Host "::error::A deterministic rerun candidate has an invalid prNumber."
            $hasInvalidCandidateNumber = $true
            continue
        }
        [void]$candidatePRNumberSet.Add($parsed)
    }
    if ($hasInvalidCandidateNumber) {
        return @{
            Actions    = @()
            HadFailure = $true
        }
    }

    $missingPRNumbers = @($candidatePRNumberSet |
        Where-Object { -not $decisionPRNumberSet.Contains([int]$_) } |
        Sort-Object)
    $unexpectedPRNumbers = @($decisionPRNumberSet |
        Where-Object { -not $candidatePRNumberSet.Contains([int]$_) } |
        Sort-Object)
    if ($missingPRNumbers.Count -gt 0 -or $unexpectedPRNumbers.Count -gt 0) {
        foreach ($missingPRNumber in $missingPRNumbers) {
            Write-Host "::error::Missing agent decision for deterministic candidate PR #$missingPRNumber."
        }
        foreach ($unexpectedPRNumber in $unexpectedPRNumbers) {
            Write-Host "::error::Agent decision for unexpected PR #$unexpectedPRNumber is outside the deterministic candidate set."
        }
        return @{
            Actions    = @()
            HadFailure = $true
        }
    }

    foreach ($item in $Items) {
        $prNumber = 0
        try {
            $prNumberRaw = (ConvertTo-TrimmedString $item.pr_number)
            if ($prNumberRaw -notmatch '^[1-9]\d*$') {
                throw "Invalid pr_number; expected positive integer string."
            }
            $prNumber = [int]$prNumberRaw
            $decision = (ConvertTo-TrimmedString $item.decision).ToLowerInvariant()
            $expectedHeadSha = ConvertTo-TrimmedString $item.expected_head_sha

            if ($decision -notin @('trigger', 'skip')) {
                throw "Invalid decision; expected 'trigger' or 'skip'."
            }
            if ([string]::IsNullOrWhiteSpace($expectedHeadSha)) {
                throw "Missing expected head SHA for $decision decision on PR #$prNumber."
            }

            $candidate = Get-MatchingCandidate -Candidates $Candidates -PRNumber $prNumber
            if (-not $candidate) {
                throw "PR #$prNumber was not in the deterministic rerun candidate set."
            }
            if ([string]::IsNullOrWhiteSpace([string]$candidate.headSha)) {
                throw "Candidate for PR #$prNumber has no recorded head SHA."
            }
            if ([string]$candidate.headSha -ne $expectedHeadSha) {
                throw "PR #$prNumber decision head SHA does not match candidate head SHA."
            }
            $expectedDecision = Get-DeterministicRerunDecision -Candidate $candidate
            if ($decision -ne $expectedDecision) {
                throw "PR #$prNumber decision '$decision' does not match deterministic decision '$expectedDecision'."
            }
            $activityCheckpointRaw = ConvertTo-TrimmedString $candidate.activityCheckpoint
            if ($activityCheckpointRaw -notmatch '^[1-9]\d*$') {
                throw "Candidate for PR #$prNumber has no valid activity checkpoint."
            }
            $activityCheckpoint = [Int64]$activityCheckpointRaw
            $activityKey = (ConvertTo-TrimmedString $candidate.activityKey).ToLowerInvariant()
            if ($activityKey -notmatch '^[0-9a-f]{64}$') {
                throw "Candidate for PR #$prNumber has no valid activity key."
            }

            # Operational values come from the deterministic candidate set, not the
            # agent's emission (its platform / pipeline_ref / rerun_comment_id are advisory).
            $rerunCommentId = if ($candidate.rerunCommentId) { [Int64]$candidate.rerunCommentId } else { [Int64]0 }
            $platform = Get-PlatformFromLabels -Labels @() -Fallback ([string]$candidate.platform)
            $pipelineRef = Normalize-PipelineRef -Value ([string]$candidate.pipelineRef) -Fallback $DefaultPipelineRef

            # The queue label does not prove which command created the current cycle,
            # so source-ambiguous candidates intentionally carry no reaction target.
            # review-trigger.yml does not need a comment to dispatch.
            if ($decision -eq 'trigger' -and $rerunCommentId -le 0) {
                Write-Host "PR #$prNumber trigger has no proven current-cycle rerun comment; dispatching without a reaction target."
            }

            $actions.Add([pscustomobject]@{
                prNumber       = $prNumber
                decision       = $decision
                platform       = $platform
                pipelineRef    = $pipelineRef
                rerunCommentId = $rerunCommentId
                headSha        = [string]$candidate.headSha
                activityCheckpoint = $activityCheckpoint
                activityKey    = $activityKey
            })
            Write-Host "Validated PR #$prNumber decision=$decision platform=$platform pipelineRef=$pipelineRef rerunCommentId=$rerunCommentId"
        } catch {
            $target = if ($prNumber -gt 0) { "PR #$prNumber" } else { "agent decision" }
            Write-Host "::error::Failed to validate $target`: $(ConvertTo-SafeLogValue ([string]$_))"
            $hadFailure = $true
        }
    }

    return @{
        Actions    = $actions.ToArray()
        HadFailure = $hadFailure
    }
}

# Allow dot-sourcing for tests without executing the body.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

$items = Get-AgentItems
$candidates = @(Get-CandidateItems -Path $env:RERUN_CANDIDATES_PATH)

$result = Get-RerunActions -Items $items -Candidates $candidates -DefaultPipelineRef $DefaultPipelineRef
$actions = @($result.Actions)

if (-not $env:RERUN_ACTIONS_PATH) {
    throw "RERUN_ACTIONS_PATH is required."
}

# Emit a JSON array (always an array, even for a single action) for the
# github-script I/O step to consume.
$json = ConvertTo-Json -InputObject @($actions) -Depth 10
Set-Content -LiteralPath $env:RERUN_ACTIONS_PATH -Value $json -Encoding utf8
Write-Host "Wrote $($actions.Count) action(s) to $env:RERUN_ACTIONS_PATH"

if ($result.HadFailure) {
    exit 1
}

exit 0
