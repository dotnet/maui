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

    [string]$TrustedPublishBranches = "",

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
    $succeeded = $LASTEXITCODE -eq 0
    $global:LASTEXITCODE = 0
    return $succeeded
}

function Test-SafePublishSourceRef([string]$Value) {
    $ref = $Value.Trim()

    if ($ref -match "^[0-9a-fA-F]{40}$") {
        return $true
    }

    if ($ref.StartsWith("refs/") -and $ref -notmatch "^refs/(heads|tags)/") {
        return $false
    }

    if ($ref -notmatch "^(refs/(heads|tags)/)?[A-Za-z0-9][A-Za-z0-9._/-]*$") {
        return $false
    }

    return -not ($ref.Contains("..") -or $ref.Contains("//") -or $ref.Contains("@{") -or $ref.EndsWith("/") -or $ref.EndsWith("."))
}

function Get-TrustedPublishBranchNames(
    [string]$DefaultBranchName,
    [string]$ConfiguredBranches
) {
    $branches = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal
    )
    foreach ($candidate in @($DefaultBranchName) + @($ConfiguredBranches -split "[,`r`n]+")) {
        $branchName = ([string]$candidate).Trim()
        if ([string]::IsNullOrWhiteSpace($branchName)) {
            continue
        }

        $branchName = $branchName -replace "^refs/heads/", "" -replace "^origin/", ""
        if (-not (Test-GitSuccess -Arguments @("check-ref-format", "--branch", $branchName))) {
            throw "Trusted publish branch '$candidate' is not a valid exact branch name. Configure TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES with comma- or newline-separated branch names and no wildcards."
        }

        [void]$branches.Add($branchName)
    }

    return @($branches)
}

Push-Location $RepositoryPath
try {
    $sourceSha = (Invoke-Git -Arguments @("rev-parse", "HEAD")).Trim()
    $normalizedSourceRef = $SourceRef.Trim()

    if ($Publish -and -not (Test-SafePublishSourceRef $normalizedSourceRef)) {
        throw "Publishing source_ref '$SourceRef' contains characters or ref syntax that are not allowed for protected publishing. Use a trusted branch name, tag name, or full commit SHA."
    }

    $trustedBranchNames = @(
        Get-TrustedPublishBranchNames `
            -DefaultBranchName $DefaultBranch `
            -ConfiguredBranches $TrustedPublishBranches
    )
    $trustedBranches = @(
        foreach ($branchName in $trustedBranchNames) {
            $branchRef = "origin/$branchName"
            if (Test-GitSuccess -Arguments @("rev-parse", "--verify", $branchRef)) {
                $branchRef
            }
        }
    )

    $isTrusted = $false
    $trustedReason = ""

    $sourceBranchName = $normalizedSourceRef -replace "^refs/heads/", "" -replace "^origin/", ""
    if ($trustedBranchNames -ccontains $sourceBranchName) {
        $branchRef = "origin/$sourceBranchName"
        if (Test-GitSuccess -Arguments @("rev-parse", "--verify", $branchRef)) {
            $branchSha = (Invoke-Git -Arguments @("rev-parse", $branchRef)).Trim()
            if ($branchSha -eq $sourceSha) {
                $isTrusted = $true
                $trustedReason = "trusted branch '$sourceBranchName'"
            }
        }
    }

    $sourceTagName = $null
    if (-not $isTrusted -and ($normalizedSourceRef -match "^refs/tags/.+" -or (Test-GitSuccess -Arguments @("rev-parse", "--verify", "refs/tags/$normalizedSourceRef")))) {
        $tagName = $normalizedSourceRef -replace "^refs/tags/", ""
        $tagSha = (Invoke-Git -Arguments @("rev-list", "-n", "1", "refs/tags/$tagName")).Trim()
        if ($tagSha -eq $sourceSha) {
            $sourceTagName = $tagName
        }
    }

    if (-not $isTrusted) {
        foreach ($branch in $trustedBranches) {
            if (Test-GitSuccess -Arguments @("merge-base", "--is-ancestor", $sourceSha, $branch)) {
                $isTrusted = $true
                $trustedReason = if ($sourceTagName) { "tag '$sourceTagName' reachable from '$branch'" } else { "commit reachable from '$branch'" }
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
            throw "Publishing requires a trusted source_ref. '$SourceRef' resolved to '$sourceSha', which is not the default branch, an exact TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES entry, or a tag/commit reachable from one. Rerun with publish=false for a dry run."
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
