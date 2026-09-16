#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Monitor branch builds of maui-pr and maintain one issue per branch outage.
.DESCRIPTION
    Uses public AzDO build metadata and GitHub issues, not an AI engine or CI logs.
    Read-only by default. Pass -Apply to publish observations and close recovered issues.
#>
[CmdletBinding()]
param([switch]$Apply)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:BranchCiRepo = 'dotnet/maui'
$script:BranchCiLabel = 'ci-branch-health'
$script:BranchCiBranches = @('inflight/current', 'inflight/candidate', 'main', 'net11.0', 'net12.0')

function Invoke-BranchCiGitHub {
    param(
        [Parameter(Mandatory)][string]$Endpoint,
        [ValidateSet('GET', 'POST', 'PATCH')][string]$Method = 'GET',
        [hashtable]$Body,
        [switch]$Paginate
    )

    $PSNativeCommandUseErrorActionPreference = $false
    $arguments = @('api', $Endpoint, '--method', $Method)
    if ($Paginate) { $arguments += @('--paginate', '--slurp') }
    if ($null -ne $Body) {
        $output = $Body | ConvertTo-Json -Depth 10 -Compress | & gh @arguments --input -
    } else {
        $output = & gh @arguments
    }
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub $Method $Endpoint failed (exit $LASTEXITCODE)."
    }
    $data = ($output -join "`n") | ConvertFrom-Json -AsHashtable -NoEnumerate
    if ($null -eq $data) { throw "Empty GitHub response from $Endpoint." }
    if ($Paginate) {
        if ($data -isnot [array]) { throw "Expected GitHub pagination from $Endpoint." }
        foreach ($page in $data) {
            if ($page -isnot [array]) { throw "Expected a paginated GitHub array from $Endpoint." }
            foreach ($item in $page) { $item }
        }
    } else {
        $data
    }
}

function Get-BranchCiLatestBuild {
    param([Parameter(Mandatory)][string]$Branch)

    $ref = [uri]::EscapeDataString("refs/heads/$Branch")
    # Queue order prevents an older, slow failure finishing after a newer green build
    # from reopening an outage. Canceled and in-progress builds are not observations.
    $uri = 'https://dev.azure.com/dnceng-public/public/_apis/build/builds' +
        "?definitions=302&repositoryId=dotnet%2Fmaui&repositoryType=GitHub&branchName=$ref" +
        '&statusFilter=completed&resultFilter=succeeded,failed,partiallySucceeded' +
        '&queryOrder=queueTimeDescending&$top=1&api-version=7.1'
    $response = Invoke-WebRequest -Uri $uri -TimeoutSec 60 -MaximumRetryCount 2 -RetryIntervalSec 2
    $data = $response.Content | ConvertFrom-Json -AsHashtable
    if ($data -isnot [System.Collections.IDictionary] -or $data.value -isnot [array] -or $data.value.Count -gt 1) {
        throw "Invalid AzDO build listing for $Branch."
    }
    if ($data.value.Count -eq 0) {
        throw "No completed, non-canceled maui-pr build found for $Branch; CI health is unknown."
    }

    $build = $data.value[0]
    if ($build.id -isnot [long] -or $build.id -le 0 -or
        $build.definition.id -ne 302 -or $build.definition.name -cne 'maui-pr' -or
        $build.repository.id -cne $script:BranchCiRepo -or $build.repository.type -cne 'GitHub' -or
        $build.sourceBranch -cne "refs/heads/$Branch" -or $build.reason -eq 'pullRequest' -or
        $build.status -cne 'completed' -or
        $build.result -cnotin @('succeeded', 'failed', 'partiallySucceeded') -or
        $build.sourceVersion -cnotmatch '^[0-9a-f]{40}$') {
        throw "AzDO returned an unexpected build identity or result for $Branch."
    }
    return $build
}

function Get-BranchCiMarker {
    param([string]$Branch)
    return "<!-- maui-pr-branch-monitor:$Branch -->"
}

function Get-BranchCiObservation {
    param([AllowNull()][string]$Body)

    if ($Body -match '(?m)^<!-- maui-pr-branch-build:([1-9][0-9]*);result:(failed|partiallySucceeded|succeeded) -->\r?$') {
        return @{ Sequence = [long]$Matches[1]; Key = $Matches[1]; Result = $Matches[2] }
    }
    return $null
}

function Format-BranchCiObservation {
    param([string]$Branch, [hashtable]$Build, [switch]$NewIssue)

    $buildUrl = "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$($Build.id)"
    $lines = @(
        (Get-BranchCiMarker $Branch)
        "<!-- maui-pr-branch-build:$($Build.id);result:$($Build.result) -->"
        ''
        "The latest completed, non-canceled ``maui-pr`` build on ``$Branch`` **$($Build.result)**."
        ''
        "- Build: [maui-pr #$($Build.id)]($buildUrl)"
        "- Commit: [$($Build.sourceVersion)](https://github.com/$script:BranchCiRepo/commit/$($Build.sourceVersion))"
        ''
    )
    if ($NewIssue) {
        $lines += @(
            '@kubaflo please investigate the failing build and restore branch CI.'
            ''
            'This issue tracks branch health, not a diagnosed root cause. Inspect the linked build logs for details.'
            'The monitor checks every six hours, reports new failed builds here, and closes this issue after recovery.'
            'Existing ci-scan issues may track individual failures from the same build.'
        )
    }
    return $lines -join "`n"
}

function Get-BranchCiHistory {
    param([hashtable]$Issue, [string]$Marker, [scriptblock]$Parser)

    $initial = & $Parser $Issue.body
    if ($null -eq $initial) { throw "Monitor issue #$($Issue.number) has no valid build marker." }
    $observations = @($initial)
    $comments = @(Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($Issue.number)/comments?per_page=100" -Paginate)
    foreach ($comment in ($comments | Sort-Object { $_.id })) {
        if ($null -eq $comment.user -or $comment.user.login -cne 'github-actions[bot]') { continue }
        if (([string]$comment.body -split '\r?\n') -cnotcontains $Marker) { continue }
        $observation = & $Parser $comment.body
        if ($null -eq $observation) { throw "Monitor comment on #$($Issue.number) has no valid build marker." }
        $observations += $observation
    }
    return $observations
}

function Initialize-BranchCiLabel {
    $labels = @(Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/labels?per_page=100" -Paginate)
    if ($script:BranchCiLabel -cnotin @($labels | ForEach-Object { $_.name })) {
        $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/labels" -Method POST -Body @{
            name = $script:BranchCiLabel
            color = 'B60205'
            description = 'Branch build and nightly delivery outages tracked every six hours'
        }
    }
}

function Get-BranchCiIssues {
    # Use the paginated issues API, not search: search indexing can lag issue creation.
    Invoke-BranchCiGitHub -Endpoint (
        "repos/$script:BranchCiRepo/issues?state=all&labels=$script:BranchCiLabel" +
        '&creator=github-actions%5Bbot%5D&per_page=100&sort=created&direction=desc'
    ) -Paginate
}

function Sync-BranchCiIssue {
    param(
        [array]$Issues, [string]$Marker, [hashtable]$Observation,
        [scriptblock]$Parser, [bool]$Healthy, [string]$Title,
        [string]$Body, [string]$NewIssueBody, [switch]$Apply
    )

    $owned = @($Issues | Where-Object {
        -not $_.ContainsKey('pull_request') -and
        $null -ne $_.user -and $_.user.login -ceq 'github-actions[bot]' -and
        $script:BranchCiLabel -cin @($_.labels | ForEach-Object { $_.name }) -and
        (([string]$_.body -split '\r?\n') -ccontains $Marker)
    } | Sort-Object { $_.number } -Descending)
    $open = @($owned | Where-Object { $_.state -ceq 'open' })
    if ($open.Count -gt 1) { throw 'Multiple open monitor issues; refusing ambiguous writes.' }
    $issue = if ($open.Count -eq 1) { $open[0] } elseif ($owned.Count -gt 0) { $owned[0] } else { $null }
    $last = $null
    if ($null -ne $issue) {
        $history = @(Get-BranchCiHistory -Issue $issue -Marker $Marker -Parser $Parser)
        $last = $history[-1]
        $sequences = @($history | Where-Object { $null -ne $_.Sequence })
        if ($null -ne $Observation.Sequence -and $sequences.Count -gt 0 -and
            $Observation.Sequence -lt ($sequences | Measure-Object -Property Sequence -Maximum).Maximum) {
            throw "Latest available observation predates the history on #$($issue.number); health is unknown."
        }
    }
    $same = $null -ne $last -and $last.Key -ceq $Observation.Key -and $last.Result -ceq $Observation.Result
    $action = 'No change'
    if ($Healthy) {
        if ($open.Count -eq 1) {
            $action = "Close #$($issue.number) after recovery"
            if ($Apply) {
                if (-not $same) {
                    $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($issue.number)/comments" -Method POST -Body @{ body = $Body }
                }
                $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($issue.number)" -Method PATCH -Body @{
                    state = 'closed'; state_reason = 'completed'
                }
            }
        }
    } elseif (-not $same) {
        if ($open.Count -eq 1) {
            $action = "Report new failure on #$($issue.number)"
            if ($Apply) {
                $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($issue.number)/comments" -Method POST -Body @{ body = $Body }
            }
        } else {
            $action = 'Create outage issue and notify @kubaflo'
            if ($Apply) {
                Initialize-BranchCiLabel
                $created = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues" -Method POST -Body @{
                    title = $Title; body = $NewIssueBody; labels = @($script:BranchCiLabel)
                }
                $action = "Created #$($created.number); notified @kubaflo"
            }
        }
    }
    if (-not $Apply) { $action = "Dry run: $action" }
    return $action
}

function Invoke-BranchCiMonitor {
    param([switch]$Apply)

    $issues = @(Get-BranchCiIssues)
    $rows = @('| Branch | Observation | Action |', '| --- | --- | --- |')
    $errors = @()

    foreach ($branch in $script:BranchCiBranches) {
        try {
            $refs = @(Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/git/matching-refs/heads/$branch")
            if ("refs/heads/$branch" -cnotin @($refs | ForEach-Object { $_.ref })) {
                Write-Warning "Branch $branch does not exist; skipping until it is created."
                $rows += "| $branch | Branch does not exist | Skipped |"
                continue
            }

            $build = Get-BranchCiLatestBuild $branch
            $marker = Get-BranchCiMarker $branch
            $body = Format-BranchCiObservation -Branch $branch -Build $build
            $action = Sync-BranchCiIssue -Issues $issues -Marker $marker `
                -Observation (Get-BranchCiObservation $body) -Parser { param($text) Get-BranchCiObservation $text } `
                -Healthy ($build.result -ceq 'succeeded') -Title "[maui-pr] $branch is failing" `
                -Body $body -NewIssueBody (Format-BranchCiObservation -Branch $branch -Build $build -NewIssue) -Apply:$Apply
            $rows += "| $branch | Build $($build.id): $($build.result) | $action |"
        } catch {
            $errors += "${branch}: $($_.Exception.Message)"
            Write-Warning $errors[-1]
            $rows += "| $branch | Unknown / operation failed | See workflow errors; not considered green |"
        }
    }

    $summary = "## maui-pr branch health`n`n" + ($rows -join "`n") + "`n"
    Write-Host $summary
    if ($env:GITHUB_STEP_SUMMARY) {
        Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $summary
    }
    if ($errors.Count -gt 0) {
        throw "Branch monitor encountered $($errors.Count) error(s):`n$($errors -join "`n")"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-BranchCiMonitor -Apply:$Apply
}
