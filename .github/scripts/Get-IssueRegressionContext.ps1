#!/usr/bin/env pwsh

. "$PSScriptRoot/shared/Invoke-GhCommandWithRetry.ps1"

function Get-IssueRegressionRequest {
    param([Parameter(Mandatory)]$Event)

    if ($Event.action -cne 'created' -or $Event.repository.full_name -cne 'dotnet/maui' -or
        $null -eq $Event.issue -or $null -ne $Event.issue.pull_request -or
        $Event.comment.user.type -cne 'User' -or
        $Event.comment.body -cnotmatch '\A/issue +trace-regression[ \t\r\n]*\z') {
        return $null
    }

    $number = 0
    if (-not [int]::TryParse([string]$Event.issue.number, [ref]$number) -or $number -le 0 -or
        $Event.comment.user.login -cnotmatch '\A[A-Za-z0-9][A-Za-z0-9-]{0,38}\z') {
        return $null
    }

    return [pscustomobject]@{
        issueNumber = $number
        requester = $Event.comment.user.login
        commentNodeId = $Event.comment.node_id
    }
}

function Get-RegressionIssueFields {
    param([AllowEmptyString()][string]$Body)

    $fields = [ordered]@{}
    $matches = [regex]::Matches($Body, '(?ms)^### ([^\r\n]+)\r?\n(.*?)(?=^### |\z)')
    foreach ($match in $matches) {
        $heading = $match.Groups[1].Value.Trim()
        if (-not $fields.Contains($heading)) {
            $fields[$heading] = $match.Groups[2].Value.Trim()
        }
    }
    return $fields
}

function Resolve-RegressionVersion {
    param([AllowEmptyString()][string]$Version)

    $result = [ordered]@{ reported = $Version; status = 'unresolved'; refs = @(); sha = $null }
    if ($Version -cnotmatch '\Av?(\d+\.\d+\.\d+(?:-[0-9A-Za-z][0-9A-Za-z.-]*)?)(?:[ \t]+(?:GA|SR\d+(?:\.\d+)?))?\z') {
        return [pscustomobject]$result
    }
    $versionNumber = $Matches[1]
    $refs = @(
        foreach ($tag in @($versionNumber, "v$versionNumber")) {
            $response = Invoke-GhCommandWithRetry -Arguments @(
                'api', "repos/dotnet/maui/git/ref/tags/$tag"
            ) -Description 'resolve release tag' -AllowNotFound -RequireOutput
            if ($null -eq $response) { continue }
            $reference = ($response | ConvertFrom-Json).object
            for ($depth = 0; $reference.type -eq 'tag' -and $depth -lt 5; $depth++) {
                if ($reference.sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid annotated tag SHA.' }
                $annotated = Invoke-GhCommandWithRetry -Arguments @(
                    'api', "repos/dotnet/maui/git/tags/$($reference.sha)"
                ) -Description 'peel release tag' -RequireOutput
                $reference = ($annotated | ConvertFrom-Json).object
            }
            if ($reference.type -ne 'commit' -or $reference.sha -cnotmatch '\A[0-9a-f]{40}\z') {
                throw 'Release tag did not resolve to a commit.'
            }
            [pscustomobject]@{ tag = $tag; sha = $reference.sha }
        }
    )
    $result.refs = $refs
    $shas = @($refs | ForEach-Object { $_.sha } | Select-Object -Unique)
    if ($shas.Count -eq 1) {
        $result.status = 'resolved'
        $result.sha = $shas[0]
    } elseif ($shas.Count -gt 1) {
        $result.status = 'ambiguous'
    }
    return [pscustomobject]$result
}

function Get-IssueRegressionContext {
    param([Parameter(Mandatory)]$Issue)

    if ($null -ne $Issue.pull_request -or [int]$Issue.number -le 0) {
        throw 'Regression tracing requires an issue, not a pull request.'
    }
    $fields = Get-RegressionIssueFields -Body ([string]$Issue.body)
    $context = [ordered]@{
        schemaVersion = 1
        repository = 'dotnet/maui'
        capturedAt = [DateTimeOffset]::UtcNow.ToString('o')
        issue = [ordered]@{
            number = $Issue.number
            url = $Issue.html_url
            author = $Issue.user.login
            title = $Issue.title
            body = $Issue.body
            updatedAt = $Issue.updated_at
            labels = @($Issue.labels | ForEach-Object { $_.name })
            fields = $fields
        }
        comments = @()
        commentsTruncated = [int]$Issue.comments -gt 100
        boundaries = [ordered]@{}
        comparison = $null
        gaps = [System.Collections.Generic.List[string]]::new()
    }

    try {
        $lastPage = [Math]::Max(1, [int][Math]::Ceiling([double]$Issue.comments / 100))
        $comments = @(
            for ($page = [Math]::Max(1, $lastPage - 1); $page -le $lastPage; $page++) {
                $response = Invoke-GhCommandWithRetry -Arguments @(
                    'api', "repos/dotnet/maui/issues/$($Issue.number)/comments?per_page=100&page=$page"
                ) -Description 'read issue comments' -RequireOutput
                $pageComments = $response | ConvertFrom-Json -NoEnumerate
                if ($pageComments -isnot [array] -or
                    ($pageComments.Count -eq 0 -and [int]$Issue.comments -gt 0)) {
                    throw 'Missing or invalid comment page; the issue history may have changed during collection.'
                }
                $pageComments | ForEach-Object {
                    [pscustomobject]@{
                        url = $_.html_url
                        author = $_.user.login
                        authorType = $_.user.type
                        association = $_.author_association
                        createdAt = $_.created_at
                        updatedAt = $_.updated_at
                        body = $_.body
                    }
                }
            }
        )
        $context.comments = @($comments | Select-Object -Last 100)
    } catch {
        $context.gaps.Add("Comments unavailable: $($_.Exception.Message)")
        Write-Warning 'Issue comments could not be collected; the context records this gap.'
    }

    foreach ($boundary in @(
        @{ key = 'reportedGood'; heading = 'Last version that worked well' },
        @{ key = 'reportedBad'; heading = 'Version with bug' }
    )) {
        try {
            $context.boundaries[$boundary.key] = Resolve-RegressionVersion -Version ([string]$fields[$boundary.heading])
            if ($context.boundaries[$boundary.key].status -ne 'resolved') {
                $status = $context.boundaries[$boundary.key].status
                $context.gaps.Add("$($boundary.key) is ${status}: the reported version does not identify one available exact release tag.")
            }
        } catch {
            $context.boundaries[$boundary.key] = [pscustomobject]@{
                reported = [string]$fields[$boundary.heading]; status = 'unavailable'; refs = @(); sha = $null
            }
            $context.gaps.Add("$($boundary.key) tag lookup failed: $($_.Exception.Message)")
            Write-Warning 'A release tag lookup failed; the context records this gap.'
        }
    }

    $good = $context.boundaries.reportedGood
    $bad = $context.boundaries.reportedBad
    if ($good.status -eq 'resolved' -and $bad.status -eq 'resolved') {
        try {
            $response = Invoke-GhCommandWithRetry -Arguments @(
                'api', "repos/dotnet/maui/compare/$($good.sha)...$($bad.sha)?per_page=100&page=1"
            ) -Description 'compare reported release boundaries' -RequireOutput
            $comparison = $response | ConvertFrom-Json
            $totalCommits = 0
            if ($null -eq $comparison -or
                $comparison.status -notin @('ahead', 'behind', 'identical', 'diverged') -or
                $comparison.merge_base_commit.sha -cnotmatch '\A[0-9a-f]{40}\z' -or
                -not [int]::TryParse([string]$comparison.total_commits, [ref]$totalCommits) -or
                $totalCommits -lt 0 -or
                $comparison.commits -isnot [array] -or $comparison.files -isnot [array]) {
                throw 'Missing or invalid release comparison evidence.'
            }
            $context.comparison = [ordered]@{
                url = $comparison.html_url
                status = $comparison.status
                mergeBaseSha = $comparison.merge_base_commit.sha
                aheadBy = $comparison.ahead_by
                behindBy = $comparison.behind_by
                isForwardRange = $comparison.status -eq 'ahead' -and $comparison.merge_base_commit.sha -eq $good.sha
                totalCommits = $totalCommits
                commitsTruncated = $totalCommits -gt @($comparison.commits).Count
                filesPossiblyTruncated = @($comparison.files).Count -ge 300
                commits = @($comparison.commits | ForEach-Object {
                    [pscustomobject]@{ sha = $_.sha; url = $_.html_url; subject = ($_.commit.message -split '\r?\n')[0] }
                })
                files = @($comparison.files | ForEach-Object {
                    [pscustomobject]@{ path = $_.filename; previousPath = $_.previous_filename; status = $_.status }
                })
            }
        } catch {
            $context.gaps.Add("Release comparison unavailable: $($_.Exception.Message)")
            Write-Warning 'The release comparison failed; the context records this gap.'
        }
    }
    return [pscustomobject]$context
}

function Invoke-IssueRegressionTrigger {
    param(
        [Parameter(Mandatory)]$Event,
        [Parameter(Mandatory)][string]$OutputPath
    )

    'should_run=false' >> $env:GITHUB_OUTPUT
    $request = Get-IssueRegressionRequest -Event $Event
    if ($null -eq $request) { return }

    $permissionJson = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/dotnet/maui/collaborators/$($request.requester)/permission"
    ) -Description 'check command author permission' -RequireOutput
    if (($permissionJson | ConvertFrom-Json).permission -notin @('admin', 'maintain', 'write')) {
        Write-Host 'The command author is not an authorized collaborator; leaving the comment visible.'
        return
    }

    $issueJson = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/dotnet/maui/issues/$($request.issueNumber)"
    ) -Description 'read target issue' -RequireOutput
    $issue = $issueJson | ConvertFrom-Json
    if ($null -ne $issue.pull_request -or $issue.number -ne $request.issueNumber) {
        throw 'The target did not resolve to the requested issue.'
    }

    $context = Get-IssueRegressionContext -Issue $issue
    New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
    $context | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    "issue_number=$($request.issueNumber)" >> $env:GITHUB_OUTPUT
    'should_run=true' >> $env:GITHUB_OUTPUT

    if (-not [string]::IsNullOrWhiteSpace($request.commentNodeId)) {
        $null = Invoke-GhCommandWithRetry -Arguments @(
            'api', 'graphql', '-f',
            'query=mutation($id: ID!) { minimizeComment(input: {subjectId: $id, classifier: RESOLVED}) { minimizedComment { isMinimized } } }',
            '-f', "id=$($request.commentNodeId)"
        ) -Description 'hide authorized trace-regression command' -AllowFailure
    }
}
