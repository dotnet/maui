#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0

function Initialize-ReplicationSourceBranch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$SourceOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$SourceRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BranchName,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9a-f]{40}$')]
        [string]$BaselineSha
    )

    $payloadPath = [IO.Path]::GetTempFileName()
    try {
        [ordered]@{
            ref = "refs/heads/$BranchName"
            sha = $BaselineSha
        } | ConvertTo-Json -Compress |
            Set-Content -LiteralPath $payloadPath -Encoding utf8NoBOM
        $createdJson = & gh api `
            -X POST `
            "repos/$SourceOwner/$SourceRepository/git/refs" `
            --input $payloadPath
        if ($LASTEXITCODE -ne 0) {
            throw 'Creating the baseline-bound source branch failed.'
        }
        $created = $createdJson | ConvertFrom-Json -Depth 10
        if ([string]$created.ref -cne "refs/heads/$BranchName" -or
            [string]$created.object.sha -cne $BaselineSha) {
            throw 'GitHub created the source branch at an unexpected ref or commit.'
        }
    }
    finally {
        Remove-Item -LiteralPath $payloadPath -Force -ErrorAction SilentlyContinue
    }
}

function Open-ReplicationDraftPullRequest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$TargetOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$TargetRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$SourceOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BranchName,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BaseBranch,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
        [string]$BodyPath
    )

    $prUrl = & gh pr create `
        --repo "$TargetOwner/$TargetRepository" `
        --head "$SourceOwner`:$BranchName" `
        --base $BaseBranch `
        --title $Title `
        --body-file $BodyPath `
        --draft
    if ($LASTEXITCODE -ne 0 -or
        [string]::IsNullOrWhiteSpace([string]$prUrl)) {
        throw 'Creating the draft fix pull request failed.'
    }

    $normalized = ([string]$prUrl).Trim()
    $expectedPrefix =
        "https://github.com/$TargetOwner/$TargetRepository/pull/"
    if (-not $normalized.StartsWith(
            $expectedPrefix,
            [StringComparison]::Ordinal) -or
        $normalized.Substring($expectedPrefix.Length) -notmatch '^\d+$') {
        throw 'GitHub returned an invalid draft pull request URL.'
    }

    return $normalized
}

function Assert-ReplicationDraftPullRequest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^https://github\.com/[A-Za-z0-9-]+/[A-Za-z0-9._-]+/pull/\d+$')]
        [string]$Url,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$TargetOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$TargetRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$SourceOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BranchName,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BaseBranch,

        [string]$ExpectedAuthor = 'MauiBot'
    )

    $pullRequestNumber =
        [int]([regex]::Match($Url, '/pull/(?<number>\d+)$').
            Groups['number'].Value)
    $verificationJson = & gh pr view $pullRequestNumber `
        --repo "$TargetOwner/$TargetRepository" `
        --json 'number,isDraft,state,author,baseRefName,headRefName,headRepositoryOwner,url'
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to verify the created draft pull request.'
    }
    $verification = $verificationJson | ConvertFrom-Json -Depth 10
    if ($verification.number -ne $pullRequestNumber -or
        $verification.isDraft -ne $true -or
        [string]$verification.state -cne 'OPEN' -or
        [string]$verification.author.login -cne $ExpectedAuthor -or
        [string]$verification.baseRefName -cne $BaseBranch -or
        [string]$verification.headRefName -cne $BranchName -or
        [string]$verification.headRepositoryOwner.login -cne $SourceOwner -or
        [string]$verification.url -cne $Url) {
        throw 'The created pull request did not match the expected MauiBot draft routing.'
    }

    return [pscustomobject]@{
        Number = $pullRequestNumber
        Url = $Url
    }
}

function Get-ReplicationDraftPullRequestByHead {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$TargetOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$TargetRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$SourceOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BranchName,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BaseBranch
    )

    $responseJson = & gh api `
        -X GET `
        "repos/$TargetOwner/$TargetRepository/pulls" `
        -f state=open `
        -f "head=$SourceOwner`:$BranchName" `
        -f "base=$BaseBranch" `
        -f per_page=10
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to locate the draft pull request by its exact head and base.'
    }
    $matches = @($responseJson | ConvertFrom-Json -Depth 10)
    if ($matches.Count -gt 1) {
        throw 'GitHub returned multiple open pull requests for one exact smoke branch.'
    }
    if ($matches.Count -eq 0) {
        return $null
    }
    return $matches[0]
}

function Remove-ReplicationDraftPullRequest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$TargetOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$TargetRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
        [string]$SourceOwner,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._-]+$')]
        [string]$SourceRepository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BranchName,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9._/-]+$')]
        [string]$BaseBranch,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$CloseComment
    )

    $errors = [Collections.Generic.List[string]]::new()
    $pullRequest = $null
    try {
        $pullRequest = Get-ReplicationDraftPullRequestByHead `
            -TargetOwner $TargetOwner `
            -TargetRepository $TargetRepository `
            -SourceOwner $SourceOwner `
            -BranchName $BranchName `
            -BaseBranch $BaseBranch
    }
    catch {
        $errors.Add(
            "Locating the draft pull request failed: $($_.Exception.Message)")
    }
    $closed = $false
    $number = $null
    $url = $null
    if ($null -ne $pullRequest) {
        $number = [int]$pullRequest.number
        $url = [string]$pullRequest.html_url
        try {
            & gh pr close $number `
                --repo "$TargetOwner/$TargetRepository" `
                --comment $CloseComment |
                Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw "gh pr close exited with code $LASTEXITCODE."
            }
            $state = & gh pr view $number `
                --repo "$TargetOwner/$TargetRepository" `
                --json state `
                --jq '.state'
            if ($LASTEXITCODE -ne 0 -or
                [string]$state -cne 'CLOSED') {
                throw 'GitHub did not report the pull request as closed.'
            }
            $closed = $true
        }
        catch {
            $errors.Add(
                "Closing draft pull request #$number failed: " +
                $_.Exception.Message)
        }
    }

    $sourceUrl =
        "https://github.com/$SourceOwner/$SourceRepository.git"
    $branchDeleted = $false
    try {
        & git push $sourceUrl --delete $BranchName
        $deleteExitCode = $LASTEXITCODE
        & git ls-remote --exit-code `
            $sourceUrl `
            "refs/heads/$BranchName" *> $null
        $verifyExitCode = $LASTEXITCODE
        $global:LASTEXITCODE = 0
        if ($verifyExitCode -eq 0) {
            throw 'The temporary publication branch still exists after cleanup.'
        }
        if ($verifyExitCode -ne 2) {
            throw (
                'The temporary publication branch deletion could not be verified ' +
                "(delete exit $deleteExitCode, verification exit $verifyExitCode).")
        }
        $branchDeleted = $true
    }
    catch {
        $errors.Add(
            "Deleting the temporary publication branch failed: " +
            $_.Exception.Message)
    }

    return [pscustomobject]@{
        Found = $null -ne $pullRequest
        Number = $number
        Url = $url
        Closed = $closed
        BranchDeleted = $branchDeleted
        Errors = @($errors)
    }
}
