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
        return @{ BuildId = [long]$Matches[1]; Result = $Matches[2] }
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
    param([hashtable]$Issue, [string]$Branch)

    $initial = Get-BranchCiObservation $Issue.body
    if ($null -eq $initial) { throw "Monitor issue #$($Issue.number) has no valid build marker." }
    $observations = @($initial)
    $comments = @(Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($Issue.number)/comments?per_page=100" -Paginate)
    foreach ($comment in ($comments | Sort-Object { $_.id })) {
        if ($null -eq $comment.user -or $comment.user.login -cne 'github-actions[bot]') { continue }
        if (([string]$comment.body -split '\r?\n') -cnotcontains (Get-BranchCiMarker $Branch)) { continue }
        $observation = Get-BranchCiObservation $comment.body
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
            description = 'maui-pr branch outage tracked by the six-hour branch monitor'
        }
    }
}

function Invoke-BranchCiMonitor {
    param([switch]$Apply)

    # Use the paginated issues API, not search: search indexing can lag issue creation.
    $issues = @(Invoke-BranchCiGitHub -Endpoint (
        "repos/$script:BranchCiRepo/issues?state=all&labels=$script:BranchCiLabel" +
        '&creator=github-actions%5Bbot%5D&per_page=100&sort=created&direction=desc'
    ) -Paginate)
    $rows = @('| Branch | Observation | Action |', '| --- | --- | --- |')
    $errors = @()
    $labelReady = $false

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
            $owned = @($issues | Where-Object {
                -not $_.ContainsKey('pull_request') -and
                $null -ne $_.user -and $_.user.login -ceq 'github-actions[bot]' -and
                $script:BranchCiLabel -cin @($_.labels | ForEach-Object { $_.name }) -and
                (([string]$_.body -split '\r?\n') -ccontains $marker)
            } | Sort-Object { $_.number } -Descending)
            $open = @($owned | Where-Object { $_.state -ceq 'open' })
            if ($open.Count -gt 1) { throw "Multiple open monitor issues for $branch; refusing ambiguous writes." }
            $issue = if ($open.Count -eq 1) { $open[0] } elseif ($owned.Count -gt 0) { $owned[0] } else { $null }
            $last = $null
            if ($null -ne $issue) {
                $history = @(Get-BranchCiHistory -Issue $issue -Branch $branch)
                $last = $history[-1]
                $maxBuildId = ($history | Measure-Object -Property BuildId -Maximum).Maximum
                if ($build.id -lt $maxBuildId) {
                    throw "Latest available build predates the history on #$($issue.number); CI health is unknown."
                }
            }
            $same = $null -ne $last -and $last.BuildId -eq $build.id -and $last.Result -ceq $build.result
            $body = Format-BranchCiObservation -Branch $branch -Build $build
            $action = 'No change'

            if ($build.result -ceq 'succeeded') {
                if ($open.Count -eq 1) {
                    $action = "Close #$($issue.number) after recovery"
                    if ($Apply) {
                        if (-not $same) {
                            $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($issue.number)/comments" -Method POST -Body @{ body = $body }
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
                        $null = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues/$($issue.number)/comments" -Method POST -Body @{ body = $body }
                    }
                } else {
                    $action = 'Create outage issue and notify @kubaflo'
                    if ($Apply) {
                        if (-not $labelReady) {
                            Initialize-BranchCiLabel
                            $labelReady = $true
                        }
                        $created = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/issues" -Method POST -Body @{
                            title = "[maui-pr] $branch is failing"
                            body = Format-BranchCiObservation -Branch $branch -Build $build -NewIssue
                            labels = @($script:BranchCiLabel)
                        }
                        $action = "Created #$($created.number); notified @kubaflo"
                    }
                }
            }
            if (-not $Apply) { $action = "Dry run: $action" }
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
