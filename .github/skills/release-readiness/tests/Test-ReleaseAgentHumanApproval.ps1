#!/usr/bin/env pwsh
#Requires -Version 7.0

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot '../scripts/Assert-ReleaseAgentHumanApproval.ps1')

$script:passed = 0
$script:failed = 0

function Assert-Equal {
    param(
        [Parameter(Mandatory)]
        [string]$Label,
        [Parameter(Mandatory)]
        $Expected,
        [Parameter(Mandatory)]
        $Actual
    )

    if ($Expected -eq $Actual) {
        Write-Host "  PASS $Label" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  FAIL $Label" -ForegroundColor Red
        Write-Host "       expected: $Expected" -ForegroundColor DarkRed
        Write-Host "       actual:   $Actual" -ForegroundColor DarkRed
        $script:failed++
    }
}

function New-Review {
    param(
        [long]$Id,
        [string]$Login,
        [string]$State,
        [string]$SubmittedAt,
        [string]$CommitId = 'head-sha',
        [string]$Association = 'MEMBER',
        [string]$UserType = 'User'
    )

    [pscustomobject]@{
        id                 = $Id
        user               = [pscustomobject]@{ login = $Login; type = $UserType }
        state              = $State
        submitted_at       = $SubmittedAt
        commit_id          = $CommitId
        author_association = $Association
    }
}

function Get-TestResult {
    param(
        [object[]]$Reviews,
        [System.Collections.IDictionary]$Permissions = @{
            alice   = 'write'
            bob     = 'maintain'
            carol   = 'admin'
            author  = 'admin'
            mauibot = 'write'
        }
    )

    Get-ReleaseAgentHumanApproval `
        -Reviews $Reviews `
        -ReviewerPermissions $Permissions `
        -PullRequestAuthor 'author' `
        -PullRequestHeadSha 'head-sha' `
        -RequiredApprovals 2
}

$aliceApproval = New-Review 1 'alice' 'APPROVED' '2026-01-01T00:00:00Z'
$bobApproval = New-Review 2 'bob' 'APPROVED' '2026-01-01T00:01:00Z'

Write-Host '[Unit] Release-agent human approval' -ForegroundColor Cyan

$empty = Get-TestResult @()
Assert-Equal 'no reviews fail the gate' $false $empty.Approved

$valid = Get-TestResult @($aliceApproval, $bobApproval)
Assert-Equal 'two current human maintainer approvals pass' $true $valid.Approved
Assert-Equal 'two distinct approvers are counted' 2 $valid.ApprovalCount

$selfApproval = Get-TestResult @(
    (New-Review 3 'author' 'APPROVED' '2026-01-01T00:02:00Z'),
    $aliceApproval
)
Assert-Equal 'PR author cannot satisfy the gate' $false $selfApproval.Approved

$botApproval = Get-TestResult @(
    (New-Review 4 'MaUiBoT' 'APPROVED' '2026-01-01T00:03:00Z'),
    $aliceApproval
)
Assert-Equal 'PAT-based bot account cannot satisfy the gate' $false $botApproval.Approved

$appApproval = Get-TestResult @(
    (New-Review 5 'review-app[bot]' 'APPROVED' '2026-01-01T00:04:00Z' -UserType 'Bot'),
    $aliceApproval
)
Assert-Equal 'GitHub App bot cannot satisfy the gate' $false $appApproval.Approved

$staleApproval = Get-TestResult @(
    (New-Review 6 'alice' 'APPROVED' '2026-01-01T00:05:00Z' -CommitId 'old-sha'),
    $bobApproval
)
Assert-Equal 'approval for an old head SHA is rejected' $false $staleApproval.Approved

$changesRequestedLatest = Get-TestResult @(
    $aliceApproval,
    (New-Review 7 'alice' 'CHANGES_REQUESTED' '2026-01-01T00:06:00Z'),
    $bobApproval
)
Assert-Equal 'a later changes-request withdraws an earlier approval' $false $changesRequestedLatest.Approved

$approvalLatest = Get-TestResult @(
    (New-Review 8 'alice' 'CHANGES_REQUESTED' '2026-01-01T00:07:00Z'),
    (New-Review 9 'alice' 'APPROVED' '2026-01-01T00:08:00Z'),
    $bobApproval
)
Assert-Equal 'a later current-head approval supersedes changes-requested' $true $approvalLatest.Approved

$commentDoesNotClear = Get-TestResult @(
    $aliceApproval,
    (New-Review 10 'alice' 'COMMENTED' '2026-01-01T00:09:00Z'),
    $bobApproval
)
Assert-Equal 'a comment does not clear an approval' $true $commentDoesNotClear.Approved

$dismissedApproval = Get-TestResult @(
    $aliceApproval,
    (New-Review 11 'alice' 'DISMISSED' '2026-01-01T00:10:00Z'),
    $bobApproval
)
Assert-Equal 'a dismissed approval is rejected' $false $dismissedApproval.Approved

$readOnlyReviewer = Get-TestResult `
    -Reviews @($aliceApproval, (New-Review 12 'reader' 'APPROVED' '2026-01-01T00:11:00Z')) `
    -Permissions @{ alice = 'write'; reader = 'read' }
Assert-Equal 'a reviewer without write permission is rejected' $false $readOnlyReviewer.Approved

$singleReviewer = Get-TestResult @(
    $aliceApproval,
    (New-Review 13 'ALICE' 'APPROVED' '2026-01-01T00:12:00Z')
)
Assert-Equal 'repeated approvals from one reviewer count once' 1 $singleReviewer.ApprovalCount

$malformedFailedClosed = $false
try {
    Get-TestResult @(
        (New-Review 14 'alice' 'APPROVED' 'not-a-date'),
        $bobApproval
    ) | Out-Null
} catch {
    $malformedFailedClosed = $true
}
Assert-Equal 'malformed review data fails closed' $true $malformedFailedClosed

$workflowPath = Join-Path $PSScriptRoot '../../../workflows/release-agent-human-approval.yml'
$workflow = Get-Content -LiteralPath $workflowPath -Raw
Assert-Equal 'workflow never creates the repository tracking label' $false `
    $workflow.Contains('gh api --method POST "repos/$REPOSITORY/labels"')
Assert-Equal 'workflow fails closed when the tracking label is missing' $true `
    $workflow.Contains("::error::Required repository label '`$TRACKING_LABEL' is missing")

Write-Host "`nPassed: $script:passed   Failed: $script:failed" `
    -ForegroundColor $(if ($script:failed -eq 0) { 'Green' } else { 'Red' })
exit $(if ($script:failed -eq 0) { 0 } else { 1 })
