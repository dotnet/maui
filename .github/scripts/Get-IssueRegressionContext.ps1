#!/usr/bin/env pwsh

. "$PSScriptRoot/shared/Invoke-GhCommandWithRetry.ps1"
. "$PSScriptRoot/shared/Get-RegressionSourceEvidence.ps1"
. "$PSScriptRoot/shared/Get-RegressionDiagnosticImages.ps1"

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
        issueNumber   = $number
        requester     = $Event.comment.user.login
        commentNodeId = $Event.comment.node_id
    }
}

function Get-RegressionUnfencedLine {
    param([AllowEmptyString()][string]$Body)

    $fenceCharacter = $null
    $fenceLength = 0
    foreach ($line in [regex]::Matches($Body, '(?m)^[^\r\n]*')) {
        $fence = [regex]::Match($line.Value, '^ {0,3}(?<fence>`{3,}|~{3,})(?<suffix>.*)$')
        if ($null -ne $fenceCharacter) {
            if ($fence.Success -and $fence.Groups['fence'].Value[0] -eq $fenceCharacter -and
                $fence.Groups['fence'].Value.Length -ge $fenceLength -and
                [string]::IsNullOrWhiteSpace($fence.Groups['suffix'].Value)) {
                $fenceCharacter = $null
                $fenceLength = 0
            }
            continue
        }
        if ($fence.Success -and
            ($fence.Groups['fence'].Value[0] -eq '~' -or -not $fence.Groups['suffix'].Value.Contains('`'))) {
            $fenceCharacter = $fence.Groups['fence'].Value[0]
            $fenceLength = $fence.Groups['fence'].Value.Length
            continue
        }
        $line
    }
}

function ConvertTo-RegressionFormVersion {
    param([AllowEmptyString()][string]$Value)

    if ($Value.Length -le 128 -and
        $Value -cmatch '\Av?(?<version>\d+\.\d+\.\d+(?:-[0-9A-Za-z][0-9A-Za-z.-]*)?)(?:[ \t]+(?:GA|SR\d+(?:\.\d+)?))?(?:[ \t]+\((?<annotation>[^()\r\n]*)\))?\z') {
        if ($Matches.annotation -match '\d+\.\d+\.\d+') { return $null }
        return $Matches.version
    }
    return $null
}

function Get-RegressionMetadataVersion {
    param([AllowEmptyString()][string]$Value)

    $version = ConvertTo-RegressionFormVersion -Value $Value
    if ($version -cmatch '\A\d+\.\d+\.\d+-(?:preview|rc)\.\d+\z') {
        return $version
    }
    return $null
}

function Get-RegressionIssueFields {
    param([AllowEmptyString()][string]$Body)

    $fields = [ordered]@{}
    $headings = @(Get-RegressionUnfencedLine -Body $Body |
            Where-Object { $_.Value -cmatch '\A### [^\r\n]+\z' })
    for ($index = 0; $index -lt $headings.Count; $index++) {
        $heading = $headings[$index].Value.Substring(4).Trim()
        $start = $headings[$index].Index + $headings[$index].Length
        $end = if ($index + 1 -lt $headings.Count) { $headings[$index + 1].Index } else { $Body.Length }
        $value = $Body.Substring($start, $end - $start).Trim()
        if ($fields.Contains($heading)) {
            $previousVersion = ConvertTo-RegressionFormVersion -Value ([string]$fields[$heading])
            $version = ConvertTo-RegressionFormVersion -Value $value
            if ($heading -notin @('Version with bug', 'Last version that worked well') -or
                $null -eq $previousVersion -or $previousVersion -cne $version) {
                $fields[$heading] = $null
            }
        }
        else {
            $fields[$heading] = $value
        }
    }
    return $fields
}

function Resolve-RegressionVersion {
    param([AllowEmptyString()][string]$Version)

    $result = [ordered]@{ reported = $Version; status = 'unresolved'; refs = @(); sha = $null }
    if ($Version.Length -gt 128 -or
        $Version -cnotmatch '\Av?(\d+\.\d+\.\d+(?:-[0-9A-Za-z][0-9A-Za-z.-]*)?)(?:[ \t]+(?:GA|SR\d+(?:\.\d+)?))?\z') {
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
    }
    elseif ($shas.Count -gt 1) {
        $result.status = 'ambiguous'
    }
    return [pscustomobject]$result
}

function Get-RegressionReleaseComparison {
    param(
        [Parameter(Mandatory)][string]$GoodSha,
        [Parameter(Mandatory)][string]$BadSha
    )

    if ($GoodSha -cnotmatch '\A[0-9a-f]{40}\z' -or $BadSha -cnotmatch '\A[0-9a-f]{40}\z') {
        throw 'Invalid release comparison revision.'
    }
    $response = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/dotnet/maui/compare/${GoodSha}...${BadSha}?per_page=100&page=1"
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
    return [ordered]@{
        url                    = $comparison.html_url
        status                 = $comparison.status
        mergeBaseSha           = $comparison.merge_base_commit.sha
        aheadBy                = $comparison.ahead_by
        behindBy               = $comparison.behind_by
        isForwardRange         = $comparison.status -eq 'ahead' -and $comparison.merge_base_commit.sha -eq $GoodSha
        totalCommits           = $totalCommits
        commitsTruncated       = $totalCommits -gt @($comparison.commits).Count
        filesPossiblyTruncated = @($comparison.files).Count -ge 300
        commits                = @($comparison.commits | ForEach-Object {
                if ($_.sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid comparison commit.' }
                [pscustomobject]@{ sha = $_.sha; url = $_.html_url; subject = ($_.commit.message -split '\r?\n')[0] }
            })
        files                  = @($comparison.files | ForEach-Object {
                [pscustomobject]@{ path = $_.filename; previousPath = $_.previous_filename; status = $_.status }
            })
    }
}

function Get-RegressionInvestigation {
    param([Parameter(Mandatory)]$Context)

    $investigation = [ordered]@{
        selection             = 'reported-form'
        good                  = $Context.boundaries.reportedGood
        bad                   = $Context.boundaries.reportedBad
        comparison            = $Context.comparison
        observations          = $Context.diagnostics.versionObservations
        observationsTruncated = $Context.diagnostics.versionObservationsTruncated
        limits                = @{ supplementalTagVersions = 4; comparisons = 2; releaseList = 30; releaseReads = 2 }
        releaseMetadata       = [ordered]@{ listRequested = $false; releasesRead = 0; listPossiblyTruncated = $false }
        gaps                  = [System.Collections.Generic.List[string]]::new()
    }
    if ($investigation.good.status -eq 'ambiguous' -or $investigation.bad.status -eq 'ambiguous') {
        return [pscustomobject]$investigation
    }

    $resolved = @{}
    foreach ($boundary in @($investigation.good, $investigation.bad)) {
        $version = ConvertTo-RegressionFormVersion -Value ([string]$boundary.reported)
        if ($null -ne $version) { $resolved[$version] = $boundary }
    }
    $lookups = 0
    $sourceOrder = @($investigation.observations | Select-Object -ExpandProperty mentionedAt -Unique)
    $groups = $investigation.observations | Group-Object mentionedAt -AsHashTable -AsString
    for ($index = $sourceOrder.Count - 1; $index -ge 0; $index--) {
        $observations = $groups[$sourceOrder[$index]]
        $goodVersions = @($observations | Where-Object role -EQ 'good' | Select-Object -ExpandProperty value -Unique)
        $badVersions = @($observations | Where-Object role -EQ 'bad' | Select-Object -ExpandProperty value -Unique)
        $formGood = $goodVersions.Count -eq 0 -and $badVersions.Count -eq 1 -and
        $Context.boundaries.reportedGood.status -eq 'resolved' -and
        @($observations | Where-Object { $_.role -eq 'bad' -and $_.firstObservedBad }).Count -gt 0
        if ($formGood) {
            $goodVersions = @(ConvertTo-RegressionFormVersion -Value ([string]$Context.boundaries.reportedGood.reported))
            $observations = @([pscustomobject]@{
                    value = $goodVersions[0]; role = 'good'; mentionedAt = $Context.issue.url
                    author = $Context.issue.author; updatedAt = $Context.issue.updatedAt
                    excerpt = "Last version that worked well: $($Context.boundaries.reportedGood.reported)"
                    kind = 'issue-form'; status = 'human-reported-unverified'
                }) + $observations
        }
        if ($goodVersions.Count -ne 1 -or $badVersions.Count -ne 1 -or
            $goodVersions[0] -ceq $badVersions[0]) { continue }
        $pair = @{}
        foreach ($role in @('good', 'bad')) {
            $observation = $observations | Where-Object role -EQ $role | Select-Object -First 1
            $version = [string]$observation.value
            if (-not $resolved.ContainsKey($version)) {
                if ($lookups -ge 4) {
                    $investigation.gaps.Add('Supplemental version tag lookups reached the four-version limit.')
                    break
                }
                $lookups++
                try {
                    $resolved[$version] = Resolve-RegressionVersion -Version $version
                }
                catch {
                    $resolved[$version] = [pscustomobject]@{ status = 'unavailable' }
                    $investigation.gaps.Add("Supplemental tag lookup failed for ${version}: $($_.Exception.Message)")
                    Write-Warning 'A supplemental release tag lookup failed.'
                    continue
                }
            }
            $boundary = $resolved[$version]
            if ($boundary.status -ne 'resolved') {
                $investigation.gaps.Add("Supplemental $role version $version is $($boundary.status); no exact source boundary was selected from it.")
                continue
            }
            $pair[$role] = [pscustomobject]@{
                reported = $version; status = $boundary.status; refs = $boundary.refs; sha = $boundary.sha
                origin = $observation; installedVersionVerified = $false
            }
        }
        if ($pair.Count -eq 2) {
            $investigation.selection = if ($formGood) { 'form-good-and-human-first-bad' } else { 'human-reported-pair' }
            $investigation.good = $pair.good
            $investigation.bad = $pair.bad
            break
        }
        if ($lookups -ge 4) { break }
    }

    $eligible = @('good', 'bad' | Where-Object {
            $boundary = $investigation[$_]
            $boundary.status -eq 'unresolved' -and
            $null -ne (Get-RegressionMetadataVersion -Value ([string]$boundary.reported))
        })
    if ($eligible.Count -gt 0) {
        try {
            $investigation.releaseMetadata.listRequested = $true
            $json = Invoke-GhCommandWithRetry -Arguments @(
                'api', 'repos/dotnet/maui/releases?per_page=30&page=1'
            ) -Description 'collect bounded Preview/RC release mappings' -RequireOutput
            $releases = $json | ConvertFrom-Json -NoEnumerate
            if ($releases -isnot [array] -or $releases.Count -gt 30) { throw 'Invalid release list.' }
            $investigation.releaseMetadata.listPossiblyTruncated = $releases.Count -eq 30
            $releaseReads = 0
            foreach ($role in $eligible) {
                $shorthand = Get-RegressionMetadataVersion -Value ([string]$investigation[$role].reported)
                $parts = [regex]::Match($shorthand, '\A(?<prefix>\d+\.\d+\.)\d+-(?<channel>(?:preview|rc)\.\d+)\z')
                $pattern = '\A' + [regex]::Escape($parts.Groups['prefix'].Value) +
                '\d+-' + [regex]::Escape($parts.Groups['channel'].Value) + '(?:\.\d+)*\z'
                $matchingReleases = @($releases | Where-Object { -not $_.draft -and $_.tag_name -cmatch $pattern })
                if ($matchingReleases.Count -ne 1) {
                    $investigation.gaps.Add("No unique published release mapping for $shorthand in the first 30 releases.")
                    continue
                }
                $id = 0L
                if (-not [long]::TryParse([string]$matchingReleases[0].id, [ref]$id) -or $id -le 0 -or $releaseReads -ge 2) {
                    throw 'Invalid or over-budget release mapping.'
                }
                $releaseReads++
                $investigation.releaseMetadata.releasesRead = $releaseReads
                $releaseJson = Invoke-GhCommandWithRetry -Arguments @(
                    'api', "repos/dotnet/maui/releases/$id"
                ) -Description 'read published MAUI package/source mapping' -RequireOutput
                $release = $releaseJson | ConvertFrom-Json
                if ($release.draft -or $release.id -ne $id -or $release.tag_name -cne $matchingReleases[0].tag_name -or
                    [string]::IsNullOrWhiteSpace([string]$release.published_at) -or
                    [Text.Encoding]::UTF8.GetByteCount([string]$release.body) -gt 128KB) {
                    throw 'Invalid published release mapping.'
                }
                $rowPattern = '(?m)^\| \*\*MAUI\*\* \| Microsoft\.NET\.Sdk\.Maui \| (?<version>' +
                [regex]::Escape($shorthand) + '(?:\.\d+)+) \| [^|\r\n]+ \| [^\r\n]*' +
                '\[Source\]\(https://github\.com/dotnet/maui/commit/(?<sha>[0-9a-f]{40})\) [^\r\n]*\|[ \t]*\r?$'
                $rows = [regex]::Matches([string]$release.body, $rowPattern)
                if ($rows.Count -ne 1) {
                    $investigation.gaps.Add("Published release $($release.tag_name) has no unique MAUI package/source row for $shorthand.")
                    continue
                }
                $investigation[$role] = [pscustomobject]@{
                    reported = $investigation[$role].reported; status = 'mapped-source'; refs = @()
                    sha = $rows[0].Groups['sha'].Value; packageVersion = $rows[0].Groups['version'].Value
                    workloadSetVersion = $release.tag_name; installedVersionVerified = $false
                    origin = [pscustomobject]@{
                        kind = 'published-release'; url = $release.html_url; updatedAt = $release.updated_at
                        excerpt = $rows[0].Value; releaseListPossiblyTruncated = $releases.Count -eq 30
                    }
                }
                $investigation.selection = 'published-release-source-lead'
            }
        }
        catch {
            $investigation.gaps.Add("Published release mapping unavailable: $($_.Exception.Message)")
            Write-Warning 'Bounded Preview/RC metadata could not be collected.'
        }
    }

    $good = $investigation.good
    $bad = $investigation.bad
    if ($good.status -in @('resolved', 'mapped-source') -and $bad.status -in @('resolved', 'mapped-source') -and
        ($good.sha -cne $Context.boundaries.reportedGood.sha -or
        $bad.sha -cne $Context.boundaries.reportedBad.sha)) {
        $investigation.comparison = $null
        try {
            $investigation.comparison = Get-RegressionReleaseComparison -GoodSha $good.sha -BadSha $bad.sha
        }
        catch {
            $investigation.gaps.Add("Supplemental source comparison unavailable: $($_.Exception.Message)")
            Write-Warning 'A supplemental source comparison could not be collected.'
        }
    }
    return [pscustomobject]$investigation
}

function Get-IssueRegressionContext {
    param(
        [Parameter(Mandatory)]$Issue,
        [string]$DiagnosticDirectory
    )

    if ($null -ne $Issue.pull_request -or [int]$Issue.number -le 0) {
        throw 'Regression tracing requires an issue, not a pull request.'
    }
    $fields = Get-RegressionIssueFields -Body ([string]$Issue.body)
    $context = [ordered]@{
        schemaVersion     = 1
        repository        = 'dotnet/maui'
        capturedAt        = [DateTimeOffset]::UtcNow.ToString('o')
        issue             = [ordered]@{
            number    = $Issue.number
            url       = $Issue.html_url
            author    = $Issue.user.login
            title     = $Issue.title
            body      = $Issue.body
            updatedAt = $Issue.updated_at
            labels    = @($Issue.labels | ForEach-Object { $_.name })
            fields    = $fields
        }
        comments          = @()
        commentsTruncated = [int]$Issue.comments -gt 100
        boundaries        = [ordered]@{}
        comparison        = $null
        investigation     = $null
        preflight         = $null
        diagnostics       = $null
        sourceEvidence    = $null
        gaps              = [System.Collections.Generic.List[string]]::new()
    }

    try {
        $lastPage = [Math]::Max(1, [int][Math]::Ceiling([double]$Issue.comments / 100))
        $comments = @(
            for ($page = [Math]::Max(1, $lastPage - 1); $page -le $lastPage; $page++) {
                $response = Invoke-GhCommandWithRetry -Arguments @(
                    'api', "repos/dotnet/maui/issues/$($Issue.number)/comments?per_page=100&page=$page"
                ) -Description 'read issue comments' -RequireOutput
                $pageComments = $response | ConvertFrom-Json -NoEnumerate
                $expectedCount = [Math]::Min(100, [int]$Issue.comments - (($page - 1) * 100))
                if ($pageComments -isnot [array] -or
                    $pageComments.Count -ne $expectedCount) {
                    throw 'Missing or invalid comment page; the issue history may have changed during collection.'
                }
                $pageComments | ForEach-Object {
                    [pscustomobject]@{
                        url           = $_.html_url
                        author        = $_.user.login
                        authorType    = $_.user.type
                        association   = $_.author_association
                        createdAt     = $_.created_at
                        updatedAt     = $_.updated_at
                        body          = $_.body
                        isPriorReport = Test-RegressionPriorReport -Body ([string]$_.body)
                    }
                }
            }
        )
        $issueJson = Invoke-GhCommandWithRetry -Arguments @(
            'api', "repos/dotnet/maui/issues/$($Issue.number)"
        ) -Description 'revalidate issue comment snapshot' -RequireOutput
        $currentIssue = $issueJson | ConvertFrom-Json
        if ($null -eq $currentIssue -or $currentIssue.number -ne $Issue.number -or
            [string]::IsNullOrWhiteSpace([string]$currentIssue.updated_at) -or
            [string]::IsNullOrWhiteSpace([string]$Issue.updated_at) -or
            $currentIssue.comments -ne $Issue.comments -or
            [DateTimeOffset]$currentIssue.updated_at -ne [DateTimeOffset]$Issue.updated_at) {
            throw 'The issue changed during comment collection or its snapshot could not be revalidated.'
        }
        $context.comments = @($comments | Select-Object -Last 100)
    }
    catch {
        $context.commentsTruncated = $true
        $context.gaps.Add("Comments unavailable: $($_.Exception.Message)")
        Write-Warning 'Issue comments could not be collected; the context records this gap.'
    }

    foreach ($boundary in @(
            @{ key = 'reportedGood'; heading = 'Last version that worked well' },
            @{ key = 'reportedBad'; heading = 'Version with bug' }
        )) {
        if ($fields.Contains($boundary.heading) -and $null -eq $fields[$boundary.heading]) {
            $context.boundaries[$boundary.key] = [pscustomobject]@{
                reported = $null; status = 'ambiguous'; refs = @(); sha = $null
            }
            $context.gaps.Add("$($boundary.key) is ambiguous: duplicate '$($boundary.heading)' headings were reported.")
            continue
        }
        try {
            $reported = [string]$fields[$boundary.heading]
            $version = ConvertTo-RegressionFormVersion -Value $reported
            if ($null -eq $version) { $version = $reported }
            $context.boundaries[$boundary.key] = Resolve-RegressionVersion -Version $version
            $context.boundaries[$boundary.key].reported = $reported
            if ($context.boundaries[$boundary.key].status -ne 'resolved') {
                $status = $context.boundaries[$boundary.key].status
                $context.gaps.Add("$($boundary.key) is ${status}: the reported version does not identify one available exact release tag.")
            }
        }
        catch {
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
            $context.comparison = Get-RegressionReleaseComparison -GoodSha $good.sha -BadSha $bad.sha
        }
        catch {
            $context.gaps.Add("Release comparison unavailable: $($_.Exception.Message)")
            Write-Warning 'The release comparison failed; the context records this gap.'
        }
    }
    $context.diagnostics = Get-RegressionDiagnosticInventory -Context $context
    $context.investigation = Get-RegressionInvestigation -Context $context
    $sourceGood = $context.investigation.good
    $sourceBad = $context.investigation.bad
    $sourceComparison = $context.investigation.comparison
    $ambiguous = $good.status -eq 'ambiguous' -or $bad.status -eq 'ambiguous'
    $metadataEligible = $null -ne (Get-RegressionMetadataVersion -Value ([string]$good.reported)) -or
    $null -ne (Get-RegressionMetadataVersion -Value ([string]$bad.reported))
    $context.preflight = [ordered]@{
        mode               = if ($ambiguous) {
            'boundary-only'
        }
        elseif ($metadataEligible -and ($good.status -ne 'resolved' -or $bad.status -ne 'resolved')) {
            'metadata-resolution'
        }
        elseif ($sourceGood.status -notin @('resolved', 'mapped-source') -and
            $sourceBad.status -notin @('resolved', 'mapped-source')) {
            'boundary-only'
        }
        else { 'source-leads' }
        usableForwardRange = $sourceGood.status -eq 'resolved' -and $sourceBad.status -eq 'resolved' -and
        $null -ne $sourceComparison -and $sourceComparison.isForwardRange
        reason             = if ($ambiguous) { 'Ambiguous reported boundary; do not select a replacement from prose.' }
        elseif ($metadataEligible -and ($good.status -ne 'resolved' -or $bad.status -ne 'resolved')) {
            'Preview/RC shorthand is eligible for bounded published release mapping, not proof of installed packages.'
        }
        elseif ($sourceGood.status -notin @('resolved', 'mapped-source') -and
            $sourceBad.status -notin @('resolved', 'mapped-source')) {
            'Neither reported boundary maps to an exact release tag; request exact installed MAUI versions.'
        }
        elseif ($null -eq $sourceComparison -or -not $sourceComparison.isForwardRange) {
            'No verified forward range; static source leads are not regression attribution.'
        }
        else { 'Exact release source range available; human observations do not verify equivalent runtime conditions.' }
    }
    if (-not [string]::IsNullOrWhiteSpace($DiagnosticDirectory)) {
        $images = Get-RegressionDiagnosticImages -Inventory $context.diagnostics -Directory $DiagnosticDirectory
        $context.diagnostics | Add-Member -NotePropertyName staticImages -NotePropertyValue $images
    }
    $context.sourceEvidence = Get-RegressionSourceEvidence -Context $context
    return [pscustomobject]$context
}

function Test-IssueRegressionPermission {
    param([Parameter(Mandatory)][string]$Requester)

    $permissionJson = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/dotnet/maui/collaborators/$Requester/permission"
    ) -Description 'check command author permission' -AllowNotFound -RequireOutput
    $permission = 'none'
    if ($null -ne $permissionJson) {
        $permission = ($permissionJson | ConvertFrom-Json).permission
    }
    if ($permission -notin @('admin', 'maintain', 'write')) {
        Write-Host 'The command author is not an authorized collaborator; leaving the comment visible.'
        return $false
    }
    return $true
}

function Invoke-IssueRegressionTrigger {
    param(
        [Parameter(Mandatory)]$Event,
        [Parameter(Mandatory)][string]$OutputPath
    )

    'should_run=false' >> $env:GITHUB_OUTPUT
    $request = Get-IssueRegressionRequest -Event $Event
    if ($null -eq $request) { return }

    if (-not (Test-IssueRegressionPermission -Requester $request.requester)) {
        return
    }

    $issueJson = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/dotnet/maui/issues/$($request.issueNumber)"
    ) -Description 'read target issue' -RequireOutput
    $issue = $issueJson | ConvertFrom-Json
    if ($null -ne $issue.pull_request -or $issue.number -ne $request.issueNumber) {
        throw 'The target did not resolve to the requested issue.'
    }

    $context = Get-IssueRegressionContext -Issue $issue `
        -DiagnosticDirectory (Join-Path (Split-Path -Parent $OutputPath) 'diagnostics')
    if (-not (Test-IssueRegressionPermission -Requester $request.requester)) {
        return
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
    $context | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    "issue_number=$($request.issueNumber)" >> $env:GITHUB_OUTPUT
    'should_run=true' >> $env:GITHUB_OUTPUT
}

function Assert-IssueRegressionOutputTarget {
    param(
        [Parameter(Mandatory)]$Output,
        [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
        [ValidatePattern('\A[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+\z')][string]$Repository = 'dotnet/maui'
    )

    if ($null -eq $Output -or $Output.items -isnot [array]) {
        throw 'The report output must contain an items array.'
    }
    $reportCount = 0
    foreach ($item in $Output.items) {
        if ($item.type -cne 'add_comment') {
            if ($item.type -cnotin @('noop', 'missing_data', 'missing_tool', 'report_incomplete')) {
                throw 'The report output contains an unsupported operation.'
            }
            continue
        }
        if ($item.body -isnot [string] -or [string]::IsNullOrWhiteSpace($item.body)) {
            throw 'A regression report requires a nonempty text body.'
        }
        $reportCount++
        if ($reportCount -gt 1) {
            throw 'The report output contains multiple report intents.'
        }
        foreach ($field in @('item_number', 'issue_number', 'pr-number',
                'pull_request_number', 'pr_number', 'pr', 'pull_number', 'discussion_number')) {
            $property = $item.PSObject.Properties[$field]
            if ($null -eq $property -or $null -eq $property.Value) { continue }
            $number = 0
            if (-not [int]::TryParse([string]$property.Value, [ref]$number) -or $number -ne $IssueNumber) {
                throw "Report target '$field' does not match the triggering issue."
            }
        }
        if (-not [string]::IsNullOrWhiteSpace([string]$item.repo) -and
            ([string]$item.repo).Trim() -cne $Repository -and
            ([string]$item.repo).Trim() -cne ($Repository -split '/')[1]) {
            throw 'The report repository does not match the triggering repository.'
        }
        foreach ($field in @('comment_id', 'commentId', 'comment-id', 'target')) {
            if ($null -ne $item.PSObject.Properties[$field].Value) {
                throw 'A regression report must create a new comment, not edit an existing comment.'
            }
        }
    }
}

function Complete-IssueRegressionRequest {
    param(
        [Parameter(Mandatory)]$Event,
        [Parameter(Mandatory)][long]$PublishedCommentId,
        [ValidatePattern('\A[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+\z')][string]$Repository = 'dotnet/maui'
    )

    if ($PublishedCommentId -le 0) { throw 'A published report comment ID is required.' }
    $request = Get-IssueRegressionRequest -Event $Event
    if ($null -eq $request -or [string]::IsNullOrWhiteSpace($request.commentNodeId)) { return }
    $commentJson = Invoke-GhCommandWithRetry -Arguments @(
        'api', "repos/$Repository/issues/comments/$PublishedCommentId"
    ) -Description 'verify published regression report scope' -RequireOutput
    $comment = $commentJson | ConvertFrom-Json
    if ($null -eq $comment -or $comment.id -ne $PublishedCommentId -or
        [string]::IsNullOrWhiteSpace([string]$comment.issue_url)) {
        throw 'The published report comment could not be verified.'
    }
    if ($comment.issue_url -cne "https://api.github.com/repos/$Repository/issues/$($request.issueNumber)") {
        Write-Warning 'The published report is outside the triggering issue; leaving the command visible.'
        return
    }
    if (-not (Test-IssueRegressionPermission -Requester $request.requester)) { return }

    Write-Host "Report $PublishedCommentId was published; minimizing the authorized command."
    $null = Invoke-GhCommandWithRetry -Arguments @(
        'api', 'graphql', '-f',
        'query=mutation($id: ID!) { minimizeComment(input: {subjectId: $id, classifier: RESOLVED}) { minimizedComment { isMinimized } } }',
        '-f', "id=$($request.commentNodeId)"
    ) -Description 'hide authorized trace-regression command after publication' -AllowFailure
}
