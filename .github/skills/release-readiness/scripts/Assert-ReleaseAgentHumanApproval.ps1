#!/usr/bin/env pwsh
#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$ReviewsJsonPath,
    [string]$ReviewerPermissionsJsonPath,
    [string]$PullRequestAuthor,
    [string]$PullRequestHeadSha,
    [ValidateRange(1, 20)]
    [int]$RequiredApprovals = 2
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-ReleaseAgentHumanApproval {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [object[]]$Reviews,
        [Parameter(Mandatory)]
        [System.Collections.IDictionary]$ReviewerPermissions,
        [Parameter(Mandatory)]
        [string]$PullRequestAuthor,
        [Parameter(Mandatory)]
        [string]$PullRequestHeadSha,
        [ValidateRange(1, 20)]
        [int]$RequiredApprovals = 2
    )

    $decisionStates = @('APPROVED', 'CHANGES_REQUESTED', 'DISMISSED')
    $trustedAssociations = @('OWNER', 'MEMBER', 'COLLABORATOR')
    $trustedPermissions = @('write', 'maintain', 'admin')
    $knownBotLogins = @(
        'github-actions',
        'app/github-actions',
        'dotnet-maestro[bot]',
        'azure-pipelines[bot]',
        'dotnet-policy-service[bot]',
        'dotnet-bot',
        'mauibot',
        'maui-bot',
        'maui-bot[bot]'
    )
    $latestByReviewer = @{}
    $normalizedPermissions = @{}

    foreach ($permissionEntry in $ReviewerPermissions.GetEnumerator()) {
        $normalizedPermissions[[string]$permissionEntry.Key.ToLowerInvariant()] =
            [string]$permissionEntry.Value.ToLowerInvariant()
    }

    foreach ($review in $Reviews) {
        $login = [string]$review.user.login
        $state = ([string]$review.state).ToUpperInvariant()
        $association = ([string]$review.author_association).ToUpperInvariant()
        $userType = [string]$review.user.type

        if ([string]::IsNullOrWhiteSpace($login) -or $state -notin $decisionStates) {
            continue
        }
        if ($userType -ne 'User' -or $association -notin $trustedAssociations) {
            continue
        }
        if ($login.Equals($PullRequestAuthor, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }
        $key = $login.ToLowerInvariant()
        if ($login.EndsWith('[bot]', [System.StringComparison]::OrdinalIgnoreCase) -or
            $knownBotLogins -contains $key) {
            continue
        }

        if (-not $normalizedPermissions.ContainsKey($key) -or
            $normalizedPermissions[$key] -notin $trustedPermissions) {
            continue
        }

        [DateTimeOffset]$submittedAt = [DateTimeOffset]::MinValue
        if (-not [DateTimeOffset]::TryParse(
            [string]$review.submitted_at,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [System.Globalization.DateTimeStyles]::AssumeUniversal,
            [ref]$submittedAt)) {
            throw "Review $($review.id) from '$login' has an invalid submitted_at value."
        }

        [long]$reviewId = 0
        if (-not [long]::TryParse([string]$review.id, [ref]$reviewId)) {
            throw "Review from '$login' has an invalid id."
        }

        $candidate = [pscustomobject]@{
            Id          = $reviewId
            Login       = $login
            State       = $state
            SubmittedAt = $submittedAt
            CommitId    = [string]$review.commit_id
        }

        if (-not $latestByReviewer.ContainsKey($key)) {
            $latestByReviewer[$key] = $candidate
            continue
        }

        $current = $latestByReviewer[$key]
        if ($submittedAt -gt $current.SubmittedAt -or
            ($submittedAt -eq $current.SubmittedAt -and $reviewId -gt $current.Id)) {
            $latestByReviewer[$key] = $candidate
        }
    }

    $approvers = @(
        $latestByReviewer.Values |
            Where-Object {
                $_.State -eq 'APPROVED' -and
                $_.CommitId.Equals($PullRequestHeadSha, [System.StringComparison]::OrdinalIgnoreCase)
            } |
            ForEach-Object Login |
            Sort-Object -Unique
    )

    [pscustomobject]@{
        Approved          = $approvers.Count -ge $RequiredApprovals
        Approvers         = $approvers
        ApprovalCount     = $approvers.Count
        RequiredApprovals = $RequiredApprovals
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    if ([string]::IsNullOrWhiteSpace($ReviewsJsonPath) -or
        [string]::IsNullOrWhiteSpace($ReviewerPermissionsJsonPath) -or
        [string]::IsNullOrWhiteSpace($PullRequestAuthor) -or
        [string]::IsNullOrWhiteSpace($PullRequestHeadSha)) {
        throw '-ReviewsJsonPath, -ReviewerPermissionsJsonPath, -PullRequestAuthor, and -PullRequestHeadSha are required.'
    }

    $rawReviews = Get-Content -LiteralPath $ReviewsJsonPath -Raw
    if (-not $rawReviews.TrimStart().StartsWith('[')) {
        throw "Reviews input must be a JSON array: $ReviewsJsonPath"
    }

    $reviews = @(ConvertFrom-Json -InputObject $rawReviews)
    $reviewerPermissions = ConvertFrom-Json `
        -InputObject (Get-Content -LiteralPath $ReviewerPermissionsJsonPath -Raw) `
        -AsHashtable
    $result = Get-ReleaseAgentHumanApproval `
        -Reviews $reviews `
        -ReviewerPermissions $reviewerPermissions `
        -PullRequestAuthor $PullRequestAuthor `
        -PullRequestHeadSha $PullRequestHeadSha `
        -RequiredApprovals $RequiredApprovals

    $result | ConvertTo-Json -Depth 4 -Compress
    if (-not $result.Approved) {
        Write-Error "Release-agent PR requires $RequiredApprovals approval(s) on the current head from distinct non-bot MAUI maintainers with write access other than the PR author; found $($result.ApprovalCount)."
        exit 1
    }
}
