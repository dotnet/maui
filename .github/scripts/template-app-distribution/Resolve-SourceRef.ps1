#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$RepositoryPath,

    [Parameter(Mandatory)]
    [string]$SourceRef,

    [Parameter(Mandatory)]
    [string]$WorkflowRef,

    [Parameter(Mandatory)]
    [string]$DefaultBranch,

    [Parameter(Mandatory)]
    [bool]$Publish
)

$ErrorActionPreference = "Stop"

function Invoke-Git([string[]]$Arguments, [switch]$IgnoreExitCode) {
    $output = & git @Arguments 2>&1
    if (-not $IgnoreExitCode -and $LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed: $output"
    }

    return $output
}

function Test-GitSuccess([string[]]$Arguments) {
    & git @Arguments *> $null
    return $LASTEXITCODE -eq 0
}

Push-Location $RepositoryPath
try {
    $sourceSha = (Invoke-Git -Arguments @("rev-parse", "HEAD")).Trim()
    $normalizedSourceRef = $SourceRef.Trim()
    $trustedBranches = @(
        Invoke-Git -Arguments @("for-each-ref", "--format=%(refname:short)", "refs/remotes/origin") |
            Where-Object { $_ -eq "origin/$DefaultBranch" -or $_ -match "^origin/net\d+\.0$" }
    )

    $isTrusted = $false
    $trustedReason = ""

    $sourceBranchName = $normalizedSourceRef -replace "^refs/heads/", "" -replace "^origin/", ""
    if ($sourceBranchName -eq $DefaultBranch -or $sourceBranchName -match "^net\d+\.0$") {
        $isTrusted = $true
        $trustedReason = "trusted branch '$sourceBranchName'"
    }

    if (-not $isTrusted -and ($normalizedSourceRef -match "^refs/tags/.+" -or (Test-GitSuccess -Arguments @("rev-parse", "--verify", "refs/tags/$normalizedSourceRef")))) {
        $tagName = $normalizedSourceRef -replace "^refs/tags/", ""
        $tagSha = (Invoke-Git -Arguments @("rev-list", "-n", "1", "refs/tags/$tagName")).Trim()
        if ($tagSha -eq $sourceSha) {
            $isTrusted = $true
            $trustedReason = "tag '$tagName'"
        }
    }

    if (-not $isTrusted) {
        foreach ($branch in $trustedBranches) {
            if (Test-GitSuccess -Arguments @("merge-base", "--is-ancestor", $sourceSha, $branch)) {
                $isTrusted = $true
                $trustedReason = "commit reachable from '$branch'"
                break
            }
        }
    }

    if ($Publish) {
        $expectedWorkflowRef = "refs/heads/$DefaultBranch"
        if ($WorkflowRef -ne $expectedWorkflowRef) {
            throw "Publishing must be run from workflow ref '$expectedWorkflowRef'. Current workflow ref is '$WorkflowRef'."
        }

        if (-not $isTrusted) {
            throw "Publishing requires a trusted source_ref. '$SourceRef' resolved to '$sourceSha', which is not a trusted branch/tag or reachable from a trusted branch. Rerun with publish=false for a dry run."
        }
    }

    Write-Host "Source ref '$SourceRef' resolved to $sourceSha"
    Write-Host "Trusted for publishing: $isTrusted $trustedReason"

    if ($env:GITHUB_OUTPUT) {
        "source_sha=$sourceSha" >> $env:GITHUB_OUTPUT
        "trusted=$($isTrusted.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
        "trusted_reason=$trustedReason" >> $env:GITHUB_OUTPUT
    }
}
finally {
    Pop-Location
}
