#!/usr/bin/env pwsh
#Requires -Version 7.0

$Script:InternalOfficialBuildDefinitionId = 1095
$Script:InternalOfficialBuildOrg = 'dnceng'
$Script:InternalOfficialBuildProject = 'internal'

function Get-InternalBuildProperty {
    param(
        [AllowNull()]$InputObject,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $InputObject) { return $null }
    if ($InputObject -is [System.Collections.IDictionary]) {
        if ($InputObject.Contains($Name)) { return $InputObject[$Name] }
        return $null
    }
    if ($InputObject.PSObject -and $InputObject.PSObject.Properties[$Name]) {
        return $InputObject.$Name
    }
    return $null
}

function Test-IsGitHubActions {
    param([AllowNull()][string]$Value = $env:GITHUB_ACTIONS)
    return -not [string]::IsNullOrWhiteSpace($Value) -and $Value.Equals('true', [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-InternalOfficialBuildBranches {
    param(
        [Parameter(Mandatory = $true)][int]$MajorVersion,
        [Parameter(Mandatory = $true)][string]$ReleaseBranch,
        [Parameter(Mandatory = $true)][bool]$ReleaseBranchExists
    )

    $refs = [System.Collections.Generic.List[string]]::new()
    [void]$refs.Add("refs/heads/net$MajorVersion.0")
    $releaseRef = "refs/heads/$ReleaseBranch"
    if ($ReleaseBranchExists -and -not $refs.Contains($releaseRef)) {
        [void]$refs.Add($releaseRef)
    }
    return @($refs)
}

function Get-InternalOfficialBuildClassification {
    param(
        [AllowNull()]$Build,
        [Parameter(Mandatory = $true)][string]$ExpectedBranchRef,
        [AllowNull()][string]$BranchHeadSha
    )

    if ($null -eq $Build) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'no-build' }
    }

    $sourceBranch = [string](Get-InternalBuildProperty $Build 'sourceBranch')
    $sourceSha = [string](Get-InternalBuildProperty $Build 'sourceVersion')
    $status = [string](Get-InternalBuildProperty $Build 'status')
    $result = [string](Get-InternalBuildProperty $Build 'result')

    if ([string]::IsNullOrWhiteSpace($sourceBranch) -or
        -not $sourceBranch.Equals($ExpectedBranchRef, [System.StringComparison]::OrdinalIgnoreCase)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'branch-mismatch' }
    }
    if ([string]::IsNullOrWhiteSpace($sourceSha)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'missing-source-sha' }
    }
    if ([string]::IsNullOrWhiteSpace($BranchHeadSha)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'missing-branch-head' }
    }
    if (-not $sourceSha.Equals($BranchHeadSha, [System.StringComparison]::OrdinalIgnoreCase)) {
        return [PSCustomObject]@{ Classification = 'stale'; Reason = 'source-sha-trails-head' }
    }

    switch ($status.ToLowerInvariant()) {
        { $_ -in @('inprogress', 'notstarted', 'postponed', 'cancelling') } {
            return [PSCustomObject]@{ Classification = 'in-progress'; Reason = 'build-not-complete' }
        }
        'completed' {
            switch ($result.ToLowerInvariant()) {
                'succeeded' {
                    return [PSCustomObject]@{ Classification = 'green'; Reason = 'completed-succeeded' }
                }
                { $_ -in @('failed', 'partiallysucceeded', 'canceled', 'cancelled') } {
                    return [PSCustomObject]@{ Classification = 'red'; Reason = "completed-$($_)" }
                }
                default {
                    return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'completed-with-unknown-result' }
                }
            }
        }
        default {
            return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'unknown-build-status' }
        }
    }
}

function Get-InternalOfficialBuildOverallClassification {
    param([Parameter(Mandatory = $true)][array]$Branches)

    if ($Branches.Count -eq 0) { return 'skipped' }

    $rank = @{
        'skipped'     = 0
        'green'       = 1
        'in-progress' = 2
        'unknown'     = 3
        'stale'       = 4
        'red'         = 5
    }
    $worst = 'skipped'
    foreach ($branch in $Branches) {
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        if (-not $rank.ContainsKey($classification)) { $classification = 'unknown' }
        if ($rank[$classification] -gt $rank[$worst]) { $worst = $classification }
    }
    return $worst
}

function Select-LatestInternalOfficialBuild {
    param([AllowNull()][object[]]$Builds)

    return @($Builds) |
        Sort-Object -Property @(
            @{ Expression = {
                $queueTime = Get-InternalBuildProperty $_ 'queueTime'
                if ($queueTime) {
                    try { return [DateTimeOffset]::Parse([string]$queueTime) } catch { }
                }
                return [DateTimeOffset]::MinValue
            }; Descending = $true },
            @{ Expression = {
                $id = Get-InternalBuildProperty $_ 'id'
                $parsedId = 0L
                if ($null -ne $id -and [long]::TryParse([string]$id, [ref]$parsedId)) {
                    return $parsedId
                }
                return 0L
            }; Descending = $true }
        ) |
        Select-Object -First 1
}

function New-AzdoInternalOfficialBuildFetcher {
    param(
        [int]$DefinitionId = $Script:InternalOfficialBuildDefinitionId,
        [string]$Org = $Script:InternalOfficialBuildOrg,
        [string]$Project = $Script:InternalOfficialBuildProject,
        [AllowNull()][string]$ManualBuildId,
        [AllowNull()][string]$ManualBuildBranchRef
    )

    $definition = $DefinitionId
    $organization = $Org
    $projectName = $Project
    $manualId = $ManualBuildId
    $manualRef = $ManualBuildBranchRef

    return {
        param([string]$BranchRef)

        if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'access'
                Message = 'Azure CLI is unavailable.'
            }
        }

        $azArgs = if (-not [string]::IsNullOrWhiteSpace($manualId) -and $BranchRef -eq $manualRef) {
            @(
                'pipelines', 'build', 'show',
                '--id', $manualId,
                '--org', "https://dev.azure.com/$organization",
                '--project', $projectName,
                '-o', 'json'
            )
        } else {
            @(
                'pipelines', 'build', 'list',
                '--definition-ids', "$definition",
                '--branch', $BranchRef,
                '--top', '20',
                '--org', "https://dev.azure.com/$organization",
                '--project', $projectName,
                '-o', 'json'
            )
        }

        $global:LASTEXITCODE = 0
        $output = & az @azArgs 2>&1
        $exitCode = $LASTEXITCODE
        $text = $output -join "`n"
        if ($exitCode -ne 0) {
            $failureKind = if ($text -match '(?i)TF400813|VS30063|unauthori[sz]ed|forbidden|401|403|not have permission|az login|sign in|authentication') {
                'access'
            } else {
                'query'
            }
            return [PSCustomObject]@{
                Success = $false
                FailureKind = $failureKind
                Message = "Azure DevOps query failed (exit $exitCode)."
            }
        }

        try {
            $parsed = $text | ConvertFrom-Json -ErrorAction Stop
        } catch {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'malformed'
                Message = 'Azure DevOps returned malformed JSON.'
            }
        }

        $build = if (-not [string]::IsNullOrWhiteSpace($manualId) -and $BranchRef -eq $manualRef) {
            $parsed
        } else {
            Select-LatestInternalOfficialBuild -Builds @($parsed)
        }
        return [PSCustomObject]@{ Success = $true; Build = $build }
    }.GetNewClosure()
}

function New-GitHubBranchHeadFetcher {
    param([Parameter(Mandatory = $true)][string]$Repository)

    $repo = $Repository
    return {
        param([string]$BranchRef)

        if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { return $null }
        $branchName = $BranchRef -replace '^refs/heads/', ''
        $encodedBranch = [System.Uri]::EscapeDataString($branchName)
        $global:LASTEXITCODE = 0
        $output = & gh api "repos/$repo/commits/$encodedBranch" --jq '.sha' 2>$null
        if ($LASTEXITCODE -ne 0) { return $null }
        $sha = ($output -join "`n").Trim()
        if ([string]::IsNullOrWhiteSpace($sha)) { return $null }
        return $sha
    }.GetNewClosure()
}

function Get-InternalOfficialBuildHealth {
    param(
        [Parameter(Mandatory = $true)][int]$MajorVersion,
        [Parameter(Mandatory = $true)][string]$ReleaseBranch,
        [Parameter(Mandatory = $true)][bool]$ReleaseBranchExists,
        [Parameter(Mandatory = $true)][scriptblock]$BuildFetcher,
        [Parameter(Mandatory = $true)][scriptblock]$HeadFetcher,
        [bool]$GitHubActions = (Test-IsGitHubActions)
    )

    $branchRefs = @(Get-InternalOfficialBuildBranches `
        -MajorVersion $MajorVersion `
        -ReleaseBranch $ReleaseBranch `
        -ReleaseBranchExists $ReleaseBranchExists)

    if ($MajorVersion -ne 11) {
        return [PSCustomObject]@{
            overall = 'skipped'
            skipReason = 'unsupported-major'
            branches = @()
        }
    }
    if ($GitHubActions) {
        return [PSCustomObject]@{
            overall = 'skipped'
            skipReason = 'github-actions'
            branches = @()
        }
    }

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($branchRef in $branchRefs) {
        try {
            $fetchResult = & $BuildFetcher $branchRef
        } catch {
            $fetchResult = [PSCustomObject]@{
                Success = $false
                FailureKind = 'query'
                Message = 'Internal build fetcher threw an exception.'
            }
        }

        $success = [bool](Get-InternalBuildProperty $fetchResult 'Success')
        $failureKind = [string](Get-InternalBuildProperty $fetchResult 'FailureKind')
        if (-not $success -and $failureKind -eq 'access') {
            return [PSCustomObject]@{
                overall = 'skipped'
                skipReason = 'internal-auth-unavailable'
                branches = @()
            }
        }

        $branchName = $branchRef -replace '^refs/heads/', ''
        if (-not $success) {
            [void]$results.Add([PSCustomObject]@{
                branch = $branchName
                branchRef = $branchRef
                classification = 'unknown'
                reason = if ($failureKind) { $failureKind } else { 'query' }
                headSha = $null
                build = $null
            })
            continue
        }

        $build = Get-InternalBuildProperty $fetchResult 'Build'
        $headSha = try { [string](& $HeadFetcher $branchRef) } catch { $null }
        $classification = Get-InternalOfficialBuildClassification `
            -Build $build `
            -ExpectedBranchRef $branchRef `
            -BranchHeadSha $headSha

        $buildId = Get-InternalBuildProperty $build 'id'
        $buildUrl = if ($buildId) {
            "https://dev.azure.com/$($Script:InternalOfficialBuildOrg)/$($Script:InternalOfficialBuildProject)/_build/results?buildId=$buildId"
        } else {
            [string](Get-InternalBuildProperty $build 'url')
        }

        [void]$results.Add([PSCustomObject]@{
            branch = $branchName
            branchRef = $branchRef
            classification = $classification.Classification
            reason = $classification.Reason
            headSha = $headSha
            build = if ($null -eq $build) {
                $null
            } else {
                [PSCustomObject]@{
                    id = $buildId
                    buildNumber = Get-InternalBuildProperty $build 'buildNumber'
                    status = Get-InternalBuildProperty $build 'status'
                    result = Get-InternalBuildProperty $build 'result'
                    sourceSha = Get-InternalBuildProperty $build 'sourceVersion'
                    url = $buildUrl
                }
            }
        })
    }

    $branchResults = @($results)
    return [PSCustomObject]@{
        overall = Get-InternalOfficialBuildOverallClassification -Branches $branchResults
        skipReason = $null
        branches = $branchResults
    }
}

function Convert-InternalOfficialBuildHealthToChecks {
    param(
        [Parameter(Mandatory = $true)]$Health,
        [Parameter(Mandatory = $true)][bool]$PublicSafe
    )

    $overall = [string](Get-InternalBuildProperty $Health 'overall')
    if ($PublicSafe) {
        $status = switch ($overall) {
            'green' { 'READY' }
            { $_ -in @('red', 'stale') } { 'BLOCKED' }
            'in-progress' { 'WATCH' }
            default { 'UNKNOWN' }
        }
        $wasSkipped = $overall -eq 'skipped'
        return ,([PSCustomObject]@{
            Area = 'Internal release pipelines'
            Status = $status
            Details = if ($wasSkipped) {
                'Internal official-build status was not queried in this public-safe run.'
            } else {
                "Internal release pipeline status is $status."
            }
            NextAction = if ($wasSkipped) {
                'Run locally with authorized internal access for official-build evidence.'
            } elseif ($status -eq 'READY') {
                'No action needed.'
            } else {
                'Release owner should inspect the authorized internal release pipeline.'
            }
        })
    }

    if ($overall -eq 'skipped') { return @() }

    $checks = [System.Collections.Generic.List[object]]::new()
    foreach ($branch in @((Get-InternalBuildProperty $Health 'branches'))) {
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        $build = Get-InternalBuildProperty $branch 'build'
        $status = switch ($classification) {
            'green' { 'READY' }
            { $_ -in @('red', 'stale') } { 'BLOCKED' }
            'in-progress' { 'WATCH' }
            default { 'UNKNOWN' }
        }
        $branchName = [string](Get-InternalBuildProperty $branch 'branch')
        $buildId = Get-InternalBuildProperty $build 'id'
        $buildNumber = Get-InternalBuildProperty $build 'buildNumber'
        $sourceSha = [string](Get-InternalBuildProperty $build 'sourceSha')
        $url = [string](Get-InternalBuildProperty $build 'url')
        $identity = if ($buildId) { "build $buildId / $buildNumber" } else { 'no build found' }
        $source = if ($sourceSha) { ", source $sourceSha" } else { '' }
        $link = if ($url) { " ([open build]($url))" } else { '' }

        [void]$checks.Add([PSCustomObject]@{
            Area = "Internal official build ($branchName)"
            Status = $status
            Details = "$($classification.ToUpperInvariant()): $identity$source$link."
            NextAction = switch ($classification) {
                'green' { 'No action needed.' }
                'red' { 'Investigate and repair the failed official build before release.' }
                'stale' { 'Run the official pipeline at current branch HEAD before judging readiness.' }
                'in-progress' { 'Wait for the current official build to complete.' }
                default { 'Verify internal access and confirm the latest official build manually.' }
            }
        })
    }
    return @($checks)
}

function Format-InternalOfficialBuildTable {
    param(
        [Parameter(Mandatory = $true)]$Health,
        [Parameter(Mandatory = $true)][bool]$PublicSafe
    )

    if ($PublicSafe) { return '' }

    $overall = [string](Get-InternalBuildProperty $Health 'overall')
    if ($overall -eq 'skipped') {
        $reason = [string](Get-InternalBuildProperty $Health 'skipReason')
        return "_Internal official-build query skipped: $reason._"
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    [void]$lines.Add('| Branch | Health | Build ID | Build number | Pipeline status/result | Source SHA | Build |')
    [void]$lines.Add('|--------|--------|----------|--------------|------------------------|------------|-------|')
    foreach ($branch in @((Get-InternalBuildProperty $Health 'branches'))) {
        $build = Get-InternalBuildProperty $branch 'build'
        $branchName = [string](Get-InternalBuildProperty $branch 'branch')
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        $id = Get-InternalBuildProperty $build 'id'
        $number = Get-InternalBuildProperty $build 'buildNumber'
        $status = Get-InternalBuildProperty $build 'status'
        $result = Get-InternalBuildProperty $build 'result'
        $sha = Get-InternalBuildProperty $build 'sourceSha'
        $url = Get-InternalBuildProperty $build 'url'
        $statusResult = if ($status -or $result) { "$status/$result" } else { '—' }
        $buildLink = if ($url) { "[open]($url)" } else { '—' }
        $branchCell = '`' + $branchName + '`'
        $shaCell = if ($sha) { '`' + $sha + '`' } else { '—' }
        [void]$lines.Add("| $branchCell | **$classification** | $(if ($id) { $id } else { '—' }) | $(if ($number) { $number } else { '—' }) | $statusResult | $shaCell | $buildLink |")
    }
    return $lines -join "`n"
}
