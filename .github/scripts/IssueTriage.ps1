#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Gather', 'Validate')][string]$Stage,
    [Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$IssueNumber,
    [ValidateSet('dotnet/maui')][string]$Repository = 'dotnet/maui',
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Actor,
    [ValidateRange(0, [long]::MaxValue)][long]$CommandCommentId = 0,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$ContextDirectory,
    [string]$AgentOutputPath,
    [ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedContextHash
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/shared/Invoke-GhCommandWithRetry.ps1"
$root = [IO.Path]::GetFullPath("$PSScriptRoot/../..")
$policyPath = "$root/.github/skills/issue-triage-labels/references/label-policy.json"
$policy = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json -AsHashtable
$policyHash = (Get-FileHash -LiteralPath $policyPath -Algorithm SHA256).Hash
$validatorPolicyPath = "$root/.github/policies/resourceManagement.yml"
$validatorPolicyHash = (Get-FileHash -LiteralPath $validatorPolicyPath -Algorithm SHA256).Hash
$validatorRules = @([regex]::Split((Get-Content -LiteralPath $validatorPolicyPath -Raw),
    '(?m)(?=^    - (?:if|description):)') | Where-Object {
        $_ -match '(?m)^          label: partner/syncfusion\r?$'
    })
if ($validatorRules.Count -ne 1) { throw 'Cannot uniquely resolve the trusted Syncfusion identity policy.' }
$validators = @([regex]::Matches($validatorRules[0], '(?m)^            user: ([A-Za-z0-9_-]+)\r?$') |
    ForEach-Object { $_.Groups[1].Value })
if ($validators.Count -eq 0) { throw 'The trusted validator identity policy is empty or its format changed.' }
$permissions = @{}
$frameworkVersionPattern = '(?<![\w.-])\d+\.\d+\.\d+(?:\.\d+)?(?:-(?:preview|rc)\.\d+(?:\.\d+)*)?(?![\w.-])'
$validationReferencePattern = '(?:https://github\.com/(?<repository>[A-Za-z0-9_.-]{1,39}/[A-Za-z0-9_.-]{1,100})/(?:issues|pull)/|(?<repository>[A-Za-z0-9_.-]{1,39}/[A-Za-z0-9_.-]{1,100})#|(?<![\w/])#)(?<number>[1-9][0-9]{0,8})(?![\w])'
$null = ConvertFrom-Markdown -InputObject ' '
$markdownBuilder = [Markdig.MarkdownPipelineBuilder]::new()
$markdownBuilder.PreciseSourceLocation = $true
$null = [Markdig.MarkdownExtensions]::UseEmphasisExtras(
    $markdownBuilder, [Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions]::Strikethrough)
$markdownPipeline = $markdownBuilder.Build()

function Get-Timestamp($Value) {
    return ([DateTimeOffset]$Value).ToUniversalTime().ToString('o')
}

function Get-SupersessionTime($Source) {
    if ($Source.kind -ceq 'comment') { return $Source.updatedAt }
    return $Source.createdAt
}

function Get-Hash($Value) {
    $bytes = [Text.Encoding]::UTF8.GetBytes(($Value | ConvertTo-Json -Depth 30 -Compress))
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
}

function Test-TriageResultComment([string]$Body, [string]$Author) {
    return $Author -ceq 'github-actions[bot]' -and
        $Body -cmatch '(?m)^<!-- issue-triage-(?:command:[1-9][0-9]*|run:[1-9][0-9]*|local:[A-F0-9]{64}) -->\r?$'
}

function Get-DecisionFingerprint($Decision, [string]$Action) {
    $evidence = @(if ($Action -cne 'withheld') {
        $Decision.evidence | Sort-Object source, quote -CaseSensitive | ForEach-Object {
            [ordered]@{ source = $_.source; quote = $_.quote }
        }
    })
    $request = if ($Decision.ContainsKey('request')) { $Decision.request } else { '' }
    return Get-Hash ([ordered]@{
        action = $Action; label = $Decision.label; reason = $Decision.reason
        evidence = $evidence; request = $request
    })
}

function Assert-RegularPath([string]$Path, [switch]$Directory) {
    $full = [IO.Path]::GetFullPath($Path)
    if ($full -eq $root -or $full.StartsWith("$root$([IO.Path]::DirectorySeparatorChar)", [StringComparison]::Ordinal)) {
        throw 'Triage evidence and agent output must be outside the trusted checkout.'
    }
    $item = Get-Item -LiteralPath $full -Force
    if ($item.PSIsContainer -ne [bool]$Directory) { throw "Unexpected file type: $full" }
    while ($null -ne $item) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Symlinks/reparse points are not permitted: $full"
        }
        $item = if ($item -is [IO.DirectoryInfo]) { $item.Parent } else { $item.Directory }
    }
    return $full
}

function Read-Json([string]$Path, [long]$MaxBytes = 1MB) {
    $full = Assert-RegularPath $Path
    $file = Get-Item -LiteralPath $full
    if ($file.Length -eq 0 -or $file.Length -gt $MaxBytes) { throw "JSON exceeds its bounds or is empty: $full" }
    return Get-Content -LiteralPath $full -Raw | ConvertFrom-Json -AsHashtable
}

function Write-Json([string]$Name, $Value) {
    $text = $Value | ConvertTo-Json -Depth 30
    if ([Text.Encoding]::UTF8.GetByteCount($text) -gt 1MB) { throw "Prepared $Name exceeds 1 MiB; refusing to truncate." }
    Set-Content -LiteralPath (Join-Path $OutputDirectory $Name) -Value $text -Encoding utf8 -NoNewline
}

function Get-Api([string]$Endpoint, [switch]$AllowNotFound) {
    $raw = Invoke-GhCommandWithRetry -Arguments @('api', '--method', 'GET', $Endpoint) `
        -Description $Endpoint -RequireOutput -AllowNotFound:$AllowNotFound
    if ($null -eq $raw) { return $null }
    if ([Text.Encoding]::UTF8.GetByteCount($raw) -gt 2MB) { throw "API response exceeds 2 MiB: $Endpoint" }
    return $raw | ConvertFrom-Json -AsHashtable
}

function Get-PublishedMauiReleases {
    $releases = @(Get-Api "repos/$Repository/releases?per_page=20")
    if ($releases.Count -gt 20) { throw 'The published release window exceeds its bound.' }
    return @($releases | Where-Object { -not $_.draft -and $_.published_at } | ForEach-Object {
        $release = $_
        $versionPattern = $frameworkVersionPattern
        $heading = [regex]::Match([string]$release.body,
            "(?im)^#\s+\.NET MAUI\s+(?<version>$versionPattern)")
        $linkedVersions = @([regex]::Matches([string]$release.body,
            "(?i)https://www\.nuget\.org/packages/Microsoft\.Maui\.Controls/(?<version>$versionPattern)") |
            ForEach-Object { $_.Groups['version'].Value })
        $version = if ($heading.Success) { $heading.Groups['version'].Value }
            elseif ($linkedVersions -icontains $release.tag_name) { [string]$release.tag_name }
            else { '' }
        $parsed = Get-FrameworkVersion $version
        $versions = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        if ($null -eq $parsed) {
            Write-Warning "Published release $($release.tag_name) has no recognized MAUI framework version; it cannot support a version request."
        } else {
            $null = $versions.Add($version)
            $null = $versions.Add(($version -replace '(?i)(-(?:preview|rc)\.\d+)(?:\.\d+)+$', '$1'))
            $packageVersions = $linkedVersions + @([string]$release.tag_name)
            foreach ($packageVersion in $packageVersions) {
                $package = Get-FrameworkVersion $packageVersion
                if ($null -ne $package -and (Compare-FrameworkVersion $package $parsed) -eq 0) {
                    $null = $versions.Add($packageVersion)
                }
            }
            if ($versions.Count -gt 20) { throw 'A published release has too many framework version aliases.' }
        }
        [ordered]@{
            id = $release.id; tag = $release.tag_name; publishedAt = Get-Timestamp $release.published_at
            versions = @($versions | Sort-Object -CaseSensitive); url = $release.html_url
        }
    })
}

function Get-Pages([string]$Endpoint, [int]$MaxItems) {
    $rows = [Collections.Generic.List[object]]::new()
    for ($page = 1; $page -le [Math]::Ceiling($MaxItems / 100) + 1; $page++) {
        $batch = @(Get-Api "${Endpoint}?per_page=100&page=$page")
        foreach ($row in $batch) { $rows.Add($row) }
        if ($rows.Count -gt $MaxItems) { throw "More than $MaxItems entries at $Endpoint; required evidence cannot be truncated." }
        if ($batch.Count -lt 100) { return $rows.ToArray() }
    }
    throw "Pagination did not complete: $Endpoint"
}

function Get-Permission([string]$Login) {
    if (-not $permissions.ContainsKey($Login)) {
        if ($permissions.Count -ge 60) { throw 'More than 60 evidence authors; refusing incomplete authority checks.' }
        if ($Login -notmatch '^[A-Za-z0-9_-]+$') { return 'none' }
        $result = Get-Api "repos/$Repository/collaborators/$Login/permission" -AllowNotFound
        if ($null -eq $result) {
            Write-Warning "No collaborator permission record for $Login; this author is not treated as an authorized maintainer."
            $permissions[$Login] = 'none'
        } else {
            $permissions[$Login] = [string]$result.permission
        }
    }
    return $permissions[$Login]
}

function Assert-Actor {
    if ((Get-Permission $Actor) -notin @('write', 'maintain', 'admin')) {
        throw 'The command requester does not currently have write/maintain/admin permission.'
    }
    if ($env:GITHUB_ACTIONS -eq 'true') {
        if ($env:GITHUB_RUN_ATTEMPT -cne '1') {
            throw 'Issue triage does not support job reruns. Inspect the retained report and request a fresh command or manual dispatch.'
        }
        if ($env:GITHUB_REPOSITORY -cne $Repository -or $env:GITHUB_ACTOR -cne $Actor) {
            throw 'The invocation does not match the repository/requester.'
        }
        $repo = Get-Api "repos/$Repository"
        if ($env:GITHUB_REF -cne "refs/heads/$($repo.default_branch)") {
            throw 'Issue-triage infrastructure must run from the default branch.'
        }
        if ($env:GITHUB_TRIGGERING_ACTOR -and
            (Get-Permission $env:GITHUB_TRIGGERING_ACTOR) -notin @('write', 'maintain', 'admin')) {
            throw 'The actor rerunning this workflow is not currently authorized.'
        }
        $event = Read-Json $env:GITHUB_EVENT_PATH 2MB
        if ($env:GITHUB_EVENT_NAME -eq 'issue_comment') {
            if ($event.action -cne 'created' -or $event.issue.ContainsKey('pull_request') -or
                $event.issue.number -ne $IssueNumber -or $event.comment.id -ne $CommandCommentId) {
                throw 'The invocation is not a new comment on the exact target issue.'
            }
        } elseif ($env:GITHUB_EVENT_NAME -eq 'workflow_dispatch') {
            if ($event.inputs.ContainsKey('aw_context') -and
                $null -ne $event.inputs.aw_context -and [string]$event.inputs.aw_context -cne '') {
                throw 'Issue triage does not accept caller workspace context.'
            }
            if ([string]$event.inputs.issue_number -cne [string]$IssueNumber -or $CommandCommentId -ne 0) {
                throw 'The invocation does not match the manual target.'
            }
        } else {
            throw 'Unsupported issue-triage event.'
        }
    }
}

function Test-Pattern([string]$Name, [string]$Pattern) {
    return $Name -cmatch ('^' + [regex]::Escape($Pattern).Replace('\*', '.*') + '$')
}

function Test-IssueReferenceMatch([Text.RegularExpressions.Match]$Match, [string]$Text) {
    # Color-shaped shorthand needs an explicit issue/PR context.
    return -not ($Match.Value.StartsWith('#') -and $Match.Groups['number'].Length -in @(3, 4, 6, 8) -and
        $Text.Substring(0, $Match.Index) -notmatch '(?i)\b(?:issue|PR|pull request|duplicate of|fix(?:es|ed)?|clos(?:e|es|ed)|resolv(?:e|es|ed)|see(?: also)?|refs?|references?|related to)\s+$')
}

function Get-Category([string]$Label) {
    foreach ($pattern in $policy.blocked) {
        if (Test-Pattern $Label $pattern) { return 'preserve' }
    }
    # Specific entries override broader families such as proposal/*.
    foreach ($category in @('content', 'evidence', 'maintainer')) {
        if ($policy.categories[$category].labels -ccontains $Label) { return $category }
    }
    foreach ($category in @('content', 'evidence', 'maintainer')) {
        foreach ($pattern in $policy.categories[$category].patterns) {
            if (Test-Pattern $Label $pattern) { return $category }
        }
    }
    if ($policy.removalOnly -ccontains $Label) { return 'removal-only' }
    return 'preserve'
}

function Get-Snapshot {
    $permissions.Clear()
    Assert-Actor
    $issue = Get-Api "repos/$Repository/issues/$IssueNumber"
    if ($issue.state -cne 'open' -or $issue.ContainsKey('pull_request')) {
        throw 'Only open issues, not pull requests, can be triaged.'
    }
    $comments = @(Get-Pages "repos/$Repository/issues/$IssueNumber/comments" 200)
    $events = @(Get-Pages "repos/$Repository/issues/$IssueNumber/events" 600)
    $catalog = @(Get-Pages "repos/$Repository/labels" 1000 | Sort-Object name -CaseSensitive)
    $publishedMauiReleases = @(Get-PublishedMauiReleases)
    if ($CommandCommentId -gt 0) {
        $command = Get-Api "repos/$Repository/issues/comments/$CommandCommentId"
        if ($command.user.login -cne $Actor -or
            $command.issue_url -cne "https://api.github.com/repos/$Repository/issues/$IssueNumber" -or
            $command.body -cnotmatch '\A/issue triage[ \t\r\n]*\z' -or
            @($comments | Where-Object { $_.id -eq $CommandCommentId }).Count -ne 1) {
            throw 'The source command was edited, deleted, retargeted, or is not exactly /issue triage.'
        }
        if ($env:GITHUB_ACTIONS -eq 'true') {
            $trigger = Read-Json $env:GITHUB_EVENT_PATH 2MB
            if ($command.body -cne $trigger.comment.body -or
                (Get-Timestamp $command.updated_at) -cne (Get-Timestamp $trigger.comment.updated_at)) {
                throw 'The command was edited after its creation event; post a fresh command.'
            }
        }
    }
    $sources = [Collections.Generic.List[object]]::new()
    $resultComments = [Collections.Generic.List[object]]::new()
    $sources.Add([ordered]@{
        id = "issue:$IssueNumber"; kind = 'issue'; author = $issue.user.login
        isMaintainer = $false; isValidator = $false; createdAt = Get-Timestamp $issue.created_at
        body = "$($issue.title)`n$($issue.body)"; url = "https://github.com/$Repository/issues/$IssueNumber"
    })
    foreach ($comment in @($comments | Sort-Object created_at, id)) {
        if ($comment.id -eq $CommandCommentId) { continue }
        if (Test-TriageResultComment $comment.body $comment.user.login) {
            $resultComments.Add([ordered]@{
                id = $comment.id; author = $comment.user.login
                createdAt = Get-Timestamp $comment.created_at
                updatedAt = Get-Timestamp $comment.updated_at; body = [string]$comment.body
            })
            continue
        }
        $humanAuthor = $comment.user.type -eq 'User' -and $policy.automationAuthors -notcontains $comment.user.login
        $maintainer = $humanAuthor -and
            (Get-Permission $comment.user.login) -in @('write', 'maintain', 'admin')
        $sources.Add([ordered]@{
            id = "comment:$($comment.id)"; kind = 'comment'; author = $comment.user.login
            isMaintainer = $maintainer
            isValidator = $humanAuthor -and ($maintainer -or $validators -contains $comment.user.login)
            createdAt = Get-Timestamp $comment.created_at
            updatedAt = Get-Timestamp $comment.updated_at; body = [string]$comment.body
            url = "https://github.com/$Repository/issues/$IssueNumber#issuecomment-$($comment.id)"
        })
    }
    foreach ($event in @($events | Sort-Object created_at, id)) {
        if ($event.event -notin @('labeled', 'unlabeled', 'milestoned', 'demilestoned')) { continue }
        $maintainer = $null -ne $event.actor -and $event.actor.type -eq 'User' -and
            $policy.automationAuthors -notcontains $event.actor.login -and
            (Get-Permission $event.actor.login) -in @('write', 'maintain', 'admin')
        $detail = if ($event.ContainsKey('label')) { $event.label.name } else { $event.milestone.title }
        $sources.Add([ordered]@{
            id = "event:$($event.id)"; kind = $event.event; author = if ($event.actor) { $event.actor.login } else { '' }
            isMaintainer = $maintainer; isValidator = $false; createdAt = Get-Timestamp $event.created_at
            body = "$($event.event): $detail"; url = "https://github.com/$Repository/issues/$IssueNumber"
        })
    }
    $references = [Collections.Generic.HashSet[int]]::new()
    foreach ($source in $sources) {
        $referenceText = Get-MarkdownText $source.body -IncludeQuotes
        foreach ($match in [regex]::Matches($referenceText,
            '(?:https://github\.com/dotnet/maui/(?:issues|pull)/|(?<![\w/])#)(?<number>[1-9][0-9]{0,8})(?![\w])')) {
            if (-not (Test-IssueReferenceMatch $match $referenceText)) { continue }
            $number = [int]$match.Groups['number'].Value
            if ($number -ne $IssueNumber) { $null = $references.Add($number) }
        }
    }
    if ($references.Count -gt 8) { throw 'More than eight related issue references; refusing to omit potentially contrary context.' }
    foreach ($number in @($references | Sort-Object)) {
        $related = Get-Api "repos/$Repository/issues/$number" -AllowNotFound
        if ($null -eq $related) {
            Write-Warning "Referenced issue/PR #$number was not found or is inaccessible; it cannot supply related evidence."
            continue
        }
        $sources.Add([ordered]@{
            id = "related:$number"; kind = 'related'; author = $related.user.login
            isMaintainer = $false; isValidator = $false; createdAt = Get-Timestamp $related.created_at
            updatedAt = Get-Timestamp $related.updated_at; state = $related.state
            body = "$($related.title)`n$($related.body)"; url = "https://github.com/$Repository/issues/$number"
        })
    }
    $after = Get-Api "repos/$Repository/issues/$IssueNumber"
    if ((Get-Hash $issue) -cne (Get-Hash $after)) { throw 'The issue changed while evidence was being collected; request a fresh run.' }
    $labels = @($issue.labels | ForEach-Object { $_.name } | Sort-Object -CaseSensitive)
    $labelDefinitions = @($catalog | ForEach-Object {
        [ordered]@{ id = $_.id; name = $_.name; description = $_.description; category = Get-Category $_.name }
    })
    $snapshot = [ordered]@{
        schemaVersion = 1; repository = $Repository; issueNumber = $IssueNumber
        actor = $Actor; commandCommentId = $CommandCommentId; policyHash = $policyHash
        validatorPolicyHash = $validatorPolicyHash
        labels = $labels; labelCatalog = $labelDefinitions
        eligibleLabels = @($labelDefinitions | Where-Object { $_.category -notin @('preserve', 'removal-only') } |
            ForEach-Object { $_.name })
        removableLabels = @($labelDefinitions | Where-Object {
            $_.category -cne 'preserve' -and $labels -ccontains $_.name
        } | ForEach-Object { $_.name })
        sources = $sources.ToArray()
        resultComments = $resultComments.ToArray()
        publishedMauiReleases = $publishedMauiReleases
    }
    $snapshot.contextHash = Get-Hash $snapshot
    return $snapshot
}

function Assert-Keys($Object, [string[]]$Required, [string[]]$Optional = @()) {
    if ($Object -isnot [Collections.IDictionary]) { throw 'Expected a JSON object.' }
    foreach ($key in $Required) {
        if (-not $Object.Contains($key)) { throw "Missing required field: $key" }
    }
    foreach ($key in $Object.Keys) {
        if ($key -cnotin ($Required + $Optional)) { throw "Unexpected field: $key" }
    }
}

function Assert-Text($Text, [int]$Min = 1, [int]$Max = 800) {
    if ($Text -isnot [string] -or $Text.Length -lt $Min -or $Text.Length -gt $Max -or
        $Text -match '[\x00-\x1f\x7f@<>]' -or $Text -match '(?i)\b(?:https?|ftp|file|javascript|data):|www\.') {
        throw 'Expected bounded, single-line plain text without mentions, HTML or URLs.'
    }
}

function Get-MarkdownText([string]$Body, [switch]$IncludeQuotes, [switch]$ExcludeLinkMetadata) {
    $document = [Markdig.Markdown]::Parse($Body, $markdownPipeline, $null)
    $ranges = [Collections.Generic.List[object]]::new()
    foreach ($node in [Markdig.Syntax.MarkdownObjectExtensions]::Descendants($document)) {
        if ($node -is [Markdig.Syntax.CodeBlock] -or
            $node -is [Markdig.Syntax.Inlines.CodeInline] -or
            $node -is [Markdig.Syntax.HtmlBlock] -or
            $node -is [Markdig.Syntax.Inlines.HtmlInline] -or
            ($node -is [Markdig.Syntax.Inlines.EmphasisInline] -and $node.DelimiterChar -ceq '~') -or
            (-not $IncludeQuotes -and $node -is [Markdig.Syntax.QuoteBlock]) -or
            ($ExcludeLinkMetadata -and (
                $node -is [Markdig.Syntax.LinkReferenceDefinition] -or
                $node -is [Markdig.Syntax.LinkReferenceDefinitionGroup]))) {
            $ranges.Add(@{ start = $node.Span.Start; end = $node.Span.End })
        } elseif ($ExcludeLinkMetadata -and $node -is [Markdig.Syntax.Inlines.LinkInline]) {
            if ($node.IsImage -or $null -eq $node.FirstChild -or $null -eq $node.LastChild) {
                $ranges.Add(@{ start = $node.Span.Start; end = $node.Span.End })
            } else {
                $ranges.Add(@{ start = $node.Span.Start; end = $node.FirstChild.Span.Start - 1 })
                $ranges.Add(@{ start = $node.LastChild.Span.End + 1; end = $node.Span.End })
            }
        }
    }
    foreach ($match in [regex]::Matches($Body,
        '(?is)<(blockquote|pre|code|script|style|textarea)\b[^>]*>.*?(?:</\1\s*>|\z)',
        [Text.RegularExpressions.RegexOptions]::None, [TimeSpan]::FromSeconds(2))) {
        $ranges.Add(@{ start = $match.Index; end = $match.Index + $match.Length - 1 })
    }
    $deletions = [Collections.Generic.Stack[object]]::new()
    foreach ($match in [regex]::Matches($Body, '(?is)<(?<closing>/)?(?<tag>del|s|strike)\b[^>]*>',
        [Text.RegularExpressions.RegexOptions]::None, [TimeSpan]::FromSeconds(2))) {
        if (-not $match.Groups['closing'].Success) {
            $deletions.Push(@{ tag = $match.Groups['tag'].Value; start = $match.Index })
        } elseif ($deletions.Count -gt 0 -and $deletions.Peek().tag -ieq $match.Groups['tag'].Value) {
            $opening = $deletions.Pop()
            $ranges.Add(@{ start = $opening.start; end = $match.Index + $match.Length - 1 })
        }
    }
    foreach ($opening in $deletions) {
        $ranges.Add(@{ start = $opening.start; end = $Body.Length - 1 })
    }
    $text = [Text.StringBuilder]::new($Body)
    $next = 0
    foreach ($range in @($ranges | Sort-Object start, end)) {
        $end = [Math]::Min($range.end, $Body.Length - 1)
        for ($index = [Math]::Max($range.start, $next); $index -le $end; $index++) {
            if ($Body[$index] -notin @("`r", "`n")) { $text[$index] = ' ' }
        }
        $next = [Math]::Max($next, $end + 1)
    }
    return $text.ToString()
}

function Get-Prose([string]$Body) {
    return Get-MarkdownText $Body -ExcludeLinkMetadata
}

function Test-Interrogative([string]$Prose) {
    return $Prose.Contains('?') -or
        $Prose -match '(?i)(?:^|[.!;:,]|\r?\n[ \t]*\r?\n)\s*(?:please\s+)?(can|could|should|would|will|may|might|is|are|was|were|does|do|did|has|have)\s+(we|i|you|they|he|she|this|it|that|LABEL|the (?:issue|report|behavior))\b'
}

function Test-ConditionalEvidence([string]$Paragraph, [switch]$Decision) {
    if ($Paragraph -match '(?i)\b(?:if|unless|provided that|providing that|assuming|supposing|as long as|on condition that|in case|in the event that|subject to|contingent (?:on|upon))\b') {
        return $true
    }
    if ($Decision) {
        return $Paragraph -match '(?i)\b(?:when|once|until|before|after|as soon as|pending)\b'
    }
    $condition = '\b(?:when|once|until|before|after|as soon as)\b'
    $clause = '(?:(?![,;!?\r\n]|\.(?:\s|$)).){0,80}'
    $outcome = '(?:is|are|has|have|can)\s+(?:(?:be|been|successfully|reliably|consistently)\s+){0,3}(?:reproduce[ds]?|reproducible|confirm(?:ed)?|verify|verified|validate[ds]?|correctly detect(?:s|ed)?)\b'
    return $Paragraph -match "(?i)$condition$clause\b$outcome"
}

function Test-TentativeEvidence([string]$Paragraph) {
    $candidate = [regex]::Replace($Paragraph, '(?i)\bas\s+expected\b', '')
    return $candidate -match '(?i)\b(?:should|could|may|might|would|will|maybe|perhaps|possibl(?:e|y)|probabl(?:e|y)|likely|potential(?:ly)?|apparently|suspect(?:ed|s)?|assum(?:e|ed|ing)|expect(?:ed)?|seems?|appears?|look(?:s|ed)?\s+like|suggest(?:s|ed)?|think|thinks|thought|believe(?:d|s)?|unsure|uncertain(?:ty)?|tentative(?:ly)?)\b'
}

function Get-LabelPattern([string]$Label) {
    return '(?<![\p{L}\p{N}\p{M}\p{S}_/.:-])' + [regex]::Escape($Label) +
        '(?![\p{L}\p{N}\p{M}\p{S}_/:-]|\.(?=[\p{L}\p{N}\p{M}\p{S}_/.:-]))'
}

function Test-DecisionParagraph([string]$Paragraph, [string]$Label, [string]$Action, $Evidence, [string]$ReferenceContext = '', [switch]$Superseding) {
    $labelText = Get-LabelPattern $Label
    $addVerbs = 'add|apply|set|assign|approve|approved|accept|accepted|mark|prioritize|prioritized'
    $removeVerbs = 'remove|drop|clear|withdraw|revoke|reject|decline'
    $gap = '(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|instead|rather than)\b).){0,80}'
    $candidate = $Paragraph
    $prohibited = $false
    if ($Superseding) {
        $negatedVerbs = if ($Action -eq 'remove') { $addVerbs } else { $removeVerbs }
        $prohibition = "(?i)\b(?:do\s+not|don['\u2019]t|never)\s+(?:$negatedVerbs)\b(?=$gap$labelText)"
        $prohibited = [regex]::IsMatch($candidate, $prohibition)
        $candidate = [regex]::Replace($candidate, $prohibition, $Action)
    }
    $withoutLabel = [regex]::Replace($candidate, $labelText, 'LABEL', [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($Action -eq 'add' -and $Label -eq 's/not-a-bug') {
        $withoutLabel = [regex]::Replace($withoutLabel, '(?i)\b(?:expected behavior|not a bug)\b', 'DISPOSITION')
    }
    if ((Test-Interrogative $withoutLabel) -or
        (((Test-ConditionalEvidence $withoutLabel -Decision) -or
            (Test-TentativeEvidence $withoutLabel)) -and -not $prohibited)) { return $false }
    $allowedReferencePattern = ''
    if (-not $prohibited -and $Action -eq 'add' -and $Label -eq 's/duplicate 2️⃣') {
        $canonical = @([regex]::Matches($candidate,
            '(?i)\bduplicate of\s+(?:#|<?https://github\.com/dotnet/maui/(?:issues|pull)/)([1-9][0-9]{0,8})(?![\w])') |
            ForEach-Object { "related:$($_.Groups[1].Value)" } | Sort-Object -Unique)
        if ($canonical.Count -ne 1 -or -not $sourceMap.ContainsKey($canonical[0]) -or
            (-not $Superseding -and @($Evidence | Where-Object { $_.source -ceq $canonical[0] }).Count -eq 0)) { return $false }
        $canonicalNumber = $canonical[0].Substring('related:'.Length)
        $allowedReferencePattern = "(?i)\bduplicate of\s+(?:#|<?https://github\.com/dotnet/maui/(?:issues|pull)/)$canonicalNumber(?![\w/-]|\.(?=\w))"
    }
    if (-not (Test-CurrentIssueScope $Paragraph $ReferenceContext $allowedReferencePattern)) { return $false }
    if ($prohibited) { return $true }
    if ($withoutLabel -match "(?i)\b(not|no|never|cannot|can['\u2019]t|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t|do not|can|should|could|may|might|would|will|maybe|perhaps|possibly|probably|likely|potentially|suspect|assume|expect|consider|candidate|asked|suggested|requested|evaluation|experiment|simulation)\b") { return $false }
    if ($allowedReferencePattern) { return $true }
    if ($Action -eq 'add' -and $Label -eq 's/not-a-bug' -and
        $candidate -match '(?i)\b(expected behavior|by design|working as intended|not a bug)\b' -and
        $candidate -notmatch "(?i)\b(not|isn.t)\s+(expected|by design|working as intended|not a bug)\b") { return $true }
    $verbs = if ($Action -eq 'add') { $addVerbs } else { $removeVerbs }
    return $candidate -match "(?i)\b(?:$verbs)\b$gap$labelText"
}

function Test-Decision($Evidence, [string]$Label, [string]$Action) {
    foreach ($reference in $Evidence) {
        $source = $sourceMap[$reference.source]
        if (-not $source.isMaintainer -or $source.kind -cne 'comment') { continue }
        $prose = Get-Prose $source.body
        $referenceContext = Get-MarkdownText $source.body
        $sourceParagraphs = @($prose -split '\r?\n[ \t]*\r?\n')
        $paragraphs = @($sourceParagraphs |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        if ($paragraphs.Count -ne 1 -or
            -not (Test-DecisionParagraph $paragraphs[0] $Label $Action $Evidence -ReferenceContext $referenceContext)) { continue }
        $oppositeAction = if ($Action -eq 'add') { 'remove' } else { 'add' }
        $oppositeEvent = if ($Action -eq 'add') { 'unlabeled' } else { 'labeled' }
        if (@($sourceParagraphs | Where-Object {
            Test-DecisionParagraph $_ $Label $oppositeAction @() -ReferenceContext $referenceContext -Superseding
        }).Count -gt 0) { continue }
        $superseded = @($sourceMap.Values | Where-Object {
            $candidateSource = $_
            $_.id -cne $source.id -and $_.isMaintainer -and
            (Get-SupersessionTime $_) -ge $source.createdAt -and (
                ($_.kind -ceq $oppositeEvent -and $_.body -ceq "${oppositeEvent}: $Label") -or
                ($_.kind -ceq 'comment' -and @((Get-Prose $_.body) -split '\r?\n[ \t]*\r?\n' |
                    Where-Object { Test-DecisionParagraph $_ $Label $oppositeAction @() -ReferenceContext (Get-MarkdownText $candidateSource.body) -Superseding }).Count -gt 0)
            )
        })
        if ($superseded.Count -eq 0) { return $true }
    }
    return $false
}

function Get-SubjectNegativeValidationPattern([string]$ReferenceContext) {
    $subject = '(?:no(?:\s+|-)one|nobody|none|neither)(?:\s+of\s+(?:(?:the|these|those|our|their)\s+)?(?:us|them|tests?|testers?|validators?|developers?|maintainers?))?'
    $auxiliary = '(?:(?:has|have|had|is|are|was|were|can|could)\s+){0,2}'
    $modifiers = '(?:(?:be|been|being|able to|yet|ever|still|currently|successfully|reliably|consistently|actually|fully|independently|personally|locally)\s+){0,4}'
    $verbs = '(?:reproduce[ds]?|reproducing|reproducible|confirm(?:s|ed|ing)?|verify|verified|verifying|validate[ds]?|validating|correctly detect(?:s|ing)?)'
    $gap = '(?:(?![,;!?\r\n]|\.(?:\s|$)|\b(?:but|however|although|except|yet|and|or)\b).){0,50}?'
    $outcome = Get-ValidationOutcomePattern $ReferenceContext
    return "(?i)\b$subject\s+$auxiliary$modifiers$verbs\b$gap(?<![\w])$outcome(?![\w])"
}

function Test-NegativeValidation([string]$Prose, [string]$ReferenceContext = '') {
    if (-not $ReferenceContext) { $ReferenceContext = $Prose }
    $negative = "(?:not|never|no longer|cannot|can['\u2019]t|unable to|failed to|couldn['\u2019]t|could not|did not|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t)"
    $modifiers = '(?:(?:be|been|being|able to|possible to|yet|ever|still|currently|at all|successfully|reliably|consistently|actually|fully|independently|definitively|personally|locally|readily|easily|immediately)\s+){0,4}'
    $verbs = '(?:reproduce[ds]?|reproducing|reproducible|confirm(?:s|ed|ing)?|verify|verified|verifying|validate[ds]?|validating|correctly detect(?:s|ing)?)'
    $pattern = "(?i)\b$negative\s+$modifiers$verbs\b"
    $outcome = '(?:(?:the|this|that|reported|same|actual|original)\s+){1,4}(?:behavior|issue|bug|regression|problem)'
    $postposed = "(?i)\b$verbs\b(?:(?![;!?\r\n]|\.(?:\s|$)).){0,100}\b$negative\s+$outcome\b"
    $subjectNegative = Get-SubjectNegativeValidationPattern $ReferenceContext
    $paragraphs = @($Prose -split '\r?\n[ \t]*\r?\n')
    $position = 0
    # Repeated paragraphs retain their own reference-metadata positions.
    foreach ($paragraph in [regex]::Split((Get-Prose $ReferenceContext), '(\r?\n[ \t]*\r?\n)')) {
        if ($paragraph -cin $paragraphs -and
            ($paragraph -match $pattern -or $paragraph -match $postposed -or $paragraph -match $subjectNegative) -and
            (Test-CurrentIssueScope $paragraph $ReferenceContext -ReferenceStart $position)) { return $true }
        $position += $paragraph.Length
    }
    return $false
}

function Test-ForeignOutcome([string]$Paragraph) {
    return $Paragraph -match '(?i)\b(?:(?:completely|entirely|totally)\s+)?(?:another|different|unrelated|separate|other)\s+(?:(?:reported|actual|original)\s+){0,2}(?:behavior|issue|bug|regression|problem|report)\b'
}

function Test-ForeignIssueReference([string]$Text) {
    $references = [regex]::Matches($Text, $validationReferencePattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    return @($references | Where-Object {
        (Test-IssueReferenceMatch $_ $Text) -and (
            [int]$_.Groups['number'].Value -ne $IssueNumber -or
            ($_.Groups['repository'].Success -and $_.Groups['repository'].Value -ine $Repository))
    }).Count -gt 0
}

function Get-ValidationOutcomePattern([string]$ReferenceContext) {
    if (-not (Test-ForeignIssueReference $ReferenceContext) -and
        -not (Test-ForeignOutcome $ReferenceContext)) {
        return '(?:(?:the|this|that|reported|same|actual|original)\s+){1,4}(?:behavior|issue|bug|regression|problem)'
    }
    $current = '(?:(?:the|this)\s+)?current\s+(?:behavior|issue|bug|regression|problem|report)'
    $reference = "(?<![\w/])(?:#|dotnet/maui#|https://github\.com/dotnet/maui/(?:issues|pull)/)${IssueNumber}(?![\w/-]|\.(?=\w))"
    return "(?:$current|$reference)"
}

function Test-CurrentIssueScope([string]$Paragraph, [string]$ReferenceContext = '',
    [string]$AllowedReferencePattern = '', [int]$ReferenceStart = -1) {
    if (-not $ReferenceContext) { $ReferenceContext = $Paragraph }
    $referenceProse = Get-Prose $ReferenceContext
    $start = if ($ReferenceStart -ge 0) { $ReferenceStart }
        else { $referenceProse.IndexOf($Paragraph, [StringComparison]::Ordinal) }
    if ($start -lt 0 -or $start + $Paragraph.Length -gt $referenceProse.Length -or
        $referenceProse.Substring($start, $Paragraph.Length) -cne $Paragraph -or
        ($ReferenceStart -lt 0 -and
            $referenceProse.IndexOf($Paragraph, $start + $Paragraph.Length, [StringComparison]::Ordinal) -ge 0)) {
        return $false
    }
    $referenceParagraph = $ReferenceContext.Substring($start, $Paragraph.Length)
    if ($AllowedReferencePattern) {
        $Paragraph = [regex]::Replace($Paragraph, $AllowedReferencePattern, 'CANONICAL')
        $referenceParagraph = [regex]::Replace($referenceParagraph, $AllowedReferencePattern, 'CANONICAL')
        $ReferenceContext = [regex]::Replace($ReferenceContext, $AllowedReferencePattern, 'CANONICAL')
    }
    if ((Test-ForeignIssueReference $referenceParagraph) -or (Test-ForeignOutcome $Paragraph)) { return $false }
    if (-not (Test-ForeignIssueReference $ReferenceContext) -and
        -not (Test-ForeignOutcome $ReferenceContext)) { return $true }
    $currentTarget = Get-ValidationOutcomePattern $ReferenceContext
    return $Paragraph -match "(?i)(?<![\w])$currentTarget(?![\w])"
}

function Test-PositiveValidation([string]$Paragraph, [string]$ReferenceContext = '') {
    if ((Test-ConditionalEvidence $Paragraph) -or (Test-ForeignOutcome $Paragraph)) { return $false }
    if (-not $ReferenceContext) { $ReferenceContext = $Paragraph }
    if (-not (Test-CurrentIssueScope $Paragraph $ReferenceContext)) { return $false }
    $candidate = [regex]::Replace($Paragraph, $validationReferencePattern, {
        param($match)
        if (Test-IssueReferenceMatch $match $Paragraph) { return $match.Value }
        return ' ' * $match.Length
    }, [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    # Mask only subject-negative outcomes, retaining separate positive outcomes.
    $candidate = [regex]::Replace($candidate, (Get-SubjectNegativeValidationPattern $ReferenceContext), {
        param($match)
        return [regex]::Replace($match.Value, '\S', ' ')
    })
    $outcome = Get-ValidationOutcomePattern $ReferenceContext
    $target = "(?<![\w])$outcome(?![\w])"
    $reproduction = '(?:reproduced|reproducible|can reproduce)'
    $clause = "(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|although|except|yet|not|never|cannot)\b|$validationReferencePattern).){0,50}"
    if ($candidate -match "(?i)$target$clause\b$reproduction\b|\b$reproduction\b$clause$target|\b(?:confirmed|verified)\b$clause$target|\btest\b$clause\bcorrectly detect(?:s|ing)\b$clause$target") {
        return $true
    }
    if ((Test-ForeignIssueReference $ReferenceContext) -or (Test-ForeignOutcome $ReferenceContext)) { return $false }
    return $candidate -match "(?i)^\s*$outcome\b" -and
        $candidate -match "(?i)\bit\s+(?:can be|is|was|has been)\s+(?:(?:successfully|reliably|consistently)\s+)?$reproduction\b"
}

function Get-FrameworkVersion([string]$Text) {
    $match = [regex]::Match($Text, '(?i)^(\d+(?:\.\d+){0,3})(?:[- .]*(preview|rc)[- .]*(\d+)(?:\.\d+)*)?$')
    if (-not $match.Success) { return $null }
    $parts = @($match.Groups[1].Value -split '\.')
    while ($parts.Count -lt 4) { $parts += '0' }
    $core = $null
    if (-not [Version]::TryParse(($parts -join '.'), [ref]$core)) { return $null }
    $number = 0
    if ($match.Groups[3].Success -and -not [int]::TryParse($match.Groups[3].Value, [ref]$number)) { return $null }
    $phase = switch ($match.Groups[2].Value.ToLowerInvariant()) { 'preview' { 0 }; 'rc' { 1 }; default { 2 } }
    return @{ core = $core; phase = $phase; number = $number }
}

function Compare-FrameworkVersion($Left, $Right) {
    foreach ($field in @('core', 'phase', 'number')) {
        $comparison = $Left[$field].CompareTo($Right[$field])
        if ($comparison -ne 0) { return $comparison }
    }
    return 0
}

function Get-VersionScope([string]$Text, [switch]$Heading) {
    $name = ([regex]::Replace($Text, '[*_]', '')).Trim()
    if ($name -match '(?i)\bMAUI\b|^(?:Version with bug|Last version that worked well)$') { return 'maui' }
    if ($name -match '(?i)\b(?:SDK|runtime|Visual Studio|VS|Xcode|iOS|Android|Windows|macOS|MacCatalyst|Mac Catalyst|OS|operating system|platforms?)\b') {
        if (-not $Heading -or $name -match '(?i)\b(?:versions?|SDK|runtime|Visual Studio|VS|Xcode|operating system|affected platforms?)\b') {
            return 'other'
        }
    }
    return 'unknown'
}

function Get-AuthorVersionCandidates([string]$Prose) {
    $versionPattern = $frameworkVersionPattern
    $named = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($match in [regex]::Matches($Prose,
        "(?i)\bMAUI(?:\s+versions?)?\s*(?:[:=]\s*)?(?<versions>$versionPattern(?:\s*(?:,|and|or|vs\.?|versus|to)\s*$versionPattern)*)")) {
        foreach ($version in [regex]::Matches($match.Groups['versions'].Value, $versionPattern)) {
            $null = $named.Add($version.Value)
        }
    }
    $scoped = [Collections.Generic.List[string]]::new()
    $unscoped = [Collections.Generic.List[string]]::new()
    $sectionScope = 'unknown'
    $tableScopes = @()
    $lines = @($Prose -split '\r?\n')
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex++) {
        $line = $lines[$lineIndex]
        $heading = [regex]::Match($line, '^#{1,6}[ \t]+(?<name>.*)')
        if ($heading.Success) { $sectionScope = Get-VersionScope $heading.Groups['name'].Value -Heading }
        $units = [Collections.Generic.List[object]]::new()
        if ($line.Contains('|')) {
            $cells = @($line.Trim().Trim('|').Split('|'))
            $separator = @(if ($lineIndex + 1 -lt $lines.Count) {
                $lines[$lineIndex + 1].Trim().Trim('|').Split('|')
            })
            if ($tableScopes.Count -eq 0 -and $separator.Count -eq $cells.Count -and
                @($separator | Where-Object { $_ -notmatch '^\s*:?-+:?\s*$' }).Count -eq 0) {
                $tableScopes = @($cells | ForEach-Object { Get-VersionScope $_ })
            }
            if ($tableScopes.Count -gt 0) {
                $rowScope = Get-VersionScope $cells[0]
                for ($index = 0; $index -lt $cells.Count; $index++) {
                    $scope = if ($tableScopes.Count -eq $cells.Count) { $tableScopes[$index] } else { 'unknown' }
                    if ($scope -eq 'unknown') {
                        $scope = if ($rowScope -ne 'unknown') { $rowScope } else { $sectionScope }
                    }
                    $units.Add(@{ text = $cells[$index]; scope = $scope })
                }
            }
        } else {
            $tableScopes = @()
        }
        if ($units.Count -eq 0) { $units.Add(@{ text = $line; scope = $sectionScope }) }
        foreach ($unit in $units) {
            $other = @([regex]::Matches($unit.text,
                "(?i)\b(?:SDK|runtime|Visual Studio(?:\s+Insiders)?|VS|Xcode|iOS|Android|Windows|macOS|MacCatalyst|Mac Catalyst|OS)(?:\s+versions?)?\s*[:=]?\s*(?<versions>$versionPattern(?:\s*(?:,|and|or|vs\.?|versus|to)\s*$versionPattern)*)") |
                ForEach-Object { [regex]::Matches($_.Groups['versions'].Value, $versionPattern) } |
                ForEach-Object { $_.Value })
            foreach ($match in [regex]::Matches($unit.text, $versionPattern)) {
                if ($named.Contains($match.Value)) {
                    $scoped.Add($match.Value)
                } elseif ($unit.scope -eq 'other' -or $other -icontains $match.Value) {
                    continue
                } elseif ($unit.scope -eq 'maui') {
                    $scoped.Add($match.Value)
                } else {
                    $unscoped.Add($match.Value)
                }
            }
        }
    }
    return @{ scoped = $scoped.ToArray(); unscoped = $unscoped.ToArray() }
}

function Test-NewerPublishedVersionRequest([string]$Paragraph, [string]$Quote, [string]$ReferenceContext = '') {
    if (-not (Test-CurrentIssueScope $Paragraph $ReferenceContext)) { return $false }
    $versionPattern = $frameworkVersionPattern
    $gap = '(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|MAUI)\b|\b\d+\.\d+).){0,80}'
    $requested = @([regex]::Matches($Paragraph,
        "(?i)\b(?:try|retest|test|verify|update|upgrade)\b$gap\bMAUI\s+(?:versions?\s+)?(?<versions>$versionPattern(?:\s*(?:,|and|or|vs\.?|versus|to|as well as)\s*(?:(?:\.NET\s+)?MAUI\s+(?:versions?\s+)?|versions?\s+)?$versionPattern)*)") |
        ForEach-Object { [regex]::Matches($_.Groups['versions'].Value, $versionPattern) } |
        ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($requested.Count -ne 1 -or
        @([regex]::Matches($Quote, $versionPattern) | Where-Object { $_.Value -ieq $requested[0] }).Count -eq 0 -or
        $Paragraph -match "(?i)\b(not|cannot|can't|do not|don't)\b.{0,30}\b(try|retest|test|verify|update|upgrade)\b") { return $false }
    if (@($current.publishedMauiReleases | Where-Object {
        $_.versions -icontains $requested[0]
    }).Count -eq 0) { return $false }
    $target = Get-FrameworkVersion $requested[0]
    if ($null -eq $target) { return $false }
    $issue = $sourceMap["issue:$IssueNumber"]
    $sections = @([regex]::Matches((Get-Prose $issue.body),
        '(?ims)^###[ \t]+Version with bug[ \t]*\r?\n(?<value>.*?)(?=^#{1,6}[ \t]|\z)'))
    if ($sections.Count -ne 1) { return $false }
    $reported = @([regex]::Matches($sections[0].Groups['value'].Value, $versionPattern) |
        ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($reported.Count -ne 1) { return $false }
    $baseline = Get-FrameworkVersion $reported[0]
    if ($null -eq $baseline) { return $false }
    $unscopedVersions = [Collections.Generic.List[object]]::new()
    foreach ($source in $sourceMap.Values) {
        if ($source.kind -notin @('issue', 'comment') -or $source.author -cne $issue.author) { continue }
        $candidates = Get-AuthorVersionCandidates (Get-Prose $source.body)
        foreach ($version in $candidates.scoped) {
            $reportedVersion = Get-FrameworkVersion $version
            if ($null -eq $reportedVersion) { return $false }
            if ((Compare-FrameworkVersion $reportedVersion $baseline) -gt 0) {
                $baseline = $reportedVersion
            }
        }
        foreach ($version in $candidates.unscoped) {
            $reportedVersion = Get-FrameworkVersion $version
            if ($null -eq $reportedVersion) { return $false }
            $unscopedVersions.Add($reportedVersion)
        }
    }
    if (@($unscopedVersions | Where-Object { (Compare-FrameworkVersion $_ $baseline) -gt 0 }).Count -gt 0) { return $false }
    return (Compare-FrameworkVersion $target $baseline) -gt 0
}

function Test-RegressionValidation([string]$Paragraph, [string]$FirstBadVersion = '') {
    if ((Test-ConditionalEvidence $Paragraph) -or (Test-TentativeEvidence $Paragraph)) { return $false }
    $framework = '(?:\.NET(?:\s+MAUI)?|MAUI)'
    $version = '\d+(?:\.\d+){0,3}(?:[- .]*(?:preview|rc)[- .]*\d+(?:\.\d+)*)?(?![\w-]|\.(?=\w))'
    $gap = '(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|not|never|cannot|didn.t|isn.t|wasn.t|regressed\s+from|MAUI)\b|(?<![\w])\.NET\b|\b\d).){0,80}'
    $target = '(?:(?:the|this|that|reported|same|actual|original)\s+){1,4}'
    $working = "\b$target(?:behavior|scenario|feature)\s+(?:(?:was|is|has(?: been)?)\s+)?(?:worked|working|works|passed)\b"
    $namedVersion = "(?<![\w])$framework\s+(?:versions?\s+)?(?<version>$version)"
    $workingVersions = @([regex]::Matches($Paragraph,
        "(?i)$working$gap$namedVersion|$namedVersion$gap$working") |
        ForEach-Object { Get-FrameworkVersion $_.Groups['version'].Value } |
        Where-Object { $null -ne $_ })
    if ($workingVersions.Count -eq 0) { return $false }
    $outcome = "$target(?:behavior|issue|bug|regression|problem)"
    $reproduction = '(?:reproduced|reproducible|can reproduce)'
    $failing = "(?:\b$outcome\b$gap\b(?:$reproduction|fails|failed)\b|\b$reproduction\b$gap\b$outcome\b|\bit\s+(?:can be|is|was|has been)\s+(?:(?:successfully|reliably|consistently)\s+)?$reproduction\b)"
    $boundary = "\b(?:since|introduced(?: in| with)?|starting(?: in| with)?|first bad(?: version)?(?: is|:)?|regressed (?:in|since))\s+(?:$framework\s+(?:version\s+)?)?(?<version>$version)"
    $failingVersions = @([regex]::Matches($Paragraph,
        "(?i)$failing$gap(?:$namedVersion|$boundary)|$namedVersion$gap$failing") |
        ForEach-Object { Get-FrameworkVersion $_.Groups['version'].Value } |
        Where-Object { $null -ne $_ })
    if ($failingVersions.Count -eq 0) { return $false }
    foreach ($workingVersion in $workingVersions) {
        if (@($failingVersions | Where-Object {
            (Compare-FrameworkVersion $_ $workingVersion) -eq 0
        }).Count -gt 0) { return $false }
    }
    if (-not $FirstBadVersion) {
        foreach ($workingVersion in $workingVersions) {
            if (@($failingVersions | Where-Object {
                (Compare-FrameworkVersion $_ $workingVersion) -le 0
            }).Count -eq 0) { return $true }
        }
        return $false
    }
    $requested = Get-FrameworkVersion $FirstBadVersion
    if ($null -eq $requested) { return $false }
    foreach ($boundaryMatch in [regex]::Matches($Paragraph,
        "(?i)$boundary")) {
        $firstBad = Get-FrameworkVersion $boundaryMatch.Groups['version'].Value
        if ($null -eq $firstBad -or (Compare-FrameworkVersion $firstBad $requested) -ne 0) { continue }
        if (@($failingVersions | Where-Object {
            (Compare-FrameworkVersion $_ $firstBad) -eq 0
        }).Count -eq 0 -or @($failingVersions | Where-Object {
            (Compare-FrameworkVersion $_ $firstBad) -lt 0
        }).Count -gt 0) { continue }
        if (@($workingVersions | Where-Object {
            (Compare-FrameworkVersion $_ $firstBad) -lt 0
        }).Count -gt 0) { return $true }
    }
    return $false
}

function Test-ConfirmationParagraph([string]$Paragraph, [string]$ReferenceContext = '') {
    return -not (Test-Interrogative $Paragraph) -and
        (Test-PositiveValidation $Paragraph $ReferenceContext) -and
        -not (Test-TentativeEvidence $Paragraph) -and
        -not (Test-NegativeValidation $Paragraph $ReferenceContext)
}

function Test-SimulatorReproduction([string]$Paragraph, [string]$ReferenceContext = '') {
    if (-not $ReferenceContext) { $ReferenceContext = $Paragraph }
    if (-not (Test-ConfirmationParagraph $Paragraph $ReferenceContext)) { return $false }
    $separator = '(?i)[;!?\r\n]|\.(?:\s|$)|\b(?:but|however|although|except|yet)\b'
    $gap = '(?:(?!\b(?:not|never|cannot)\b).){0,80}'
    return @($Paragraph -split $separator | Where-Object {
        (Test-PositiveValidation $_ $ReferenceContext) -and
        $_ -match "(?i)\b(?:reproduced|reproducible|can reproduce)\b$gap\bsimulator\b"
    }).Count -gt 0
}

function Get-Confirmation($Evidence, [switch]$RequireRegression, [string]$FirstBadVersion) {
    return @($Evidence | Where-Object {
        $reference = $_
        $source = $sourceMap[$reference.source]
        $prose = Get-Prose $source.body
        $paragraphs = @($prose -split '\r?\n[ \t]*\r?\n' |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        $source.isValidator -and $source.kind -ceq 'comment' -and
        $prose.Contains($reference.quote, [StringComparison]::Ordinal) -and
        $prose -match '(?i)\b(Android|iOS|Windows|MacCatalyst|MacOS|Tizen|Linux|MAUI)\b|\.NET\s*\d+|\b\d+\.\d+' -and
        $paragraphs.Count -eq 1 -and
        (Test-ConfirmationParagraph $paragraphs[0] (Get-MarkdownText $source.body)) -and
        (-not $RequireRegression -or (Test-RegressionValidation $paragraphs[0] $FirstBadVersion)) -and
        -not (Test-NegativeValidation $prose (Get-MarkdownText $source.body))
    })
}

function Test-CurrentConfirmation($Confirmations, $Evidence, [string]$Label, $Snapshot) {
    if ($Confirmations.Count -eq 0) { return $false }
    $latest = @($Confirmations | Sort-Object { $sourceMap[$_.source].createdAt })[-1]
    if (@($Snapshot.sources | Where-Object {
        $_.kind -ceq 'comment' -and $_.isValidator -and
        (Get-SupersessionTime $_) -ge $sourceMap[$latest.source].createdAt -and
        (Test-NegativeValidation (Get-Prose $_.body) (Get-MarkdownText $_.body))
    }).Count -gt 0) { return $false }
    $laterRemovals = @($Snapshot.sources | Where-Object {
        $_.isMaintainer -and $_.kind -ceq 'unlabeled' -and $_.body -ceq "unlabeled: $Label" -and
        $_.createdAt -ge $sourceMap[$latest.source].createdAt
    } | Sort-Object createdAt)
    if ($laterRemovals.Count -gt 0) {
        $readdEvidence = @($Evidence | Where-Object {
            $sourceMap[$_.source].createdAt -gt $laterRemovals[-1].createdAt
        })
        if (-not (Test-Decision $readdEvidence $Label 'add')) { return $false }
    }
    return $true
}

function Test-AreaCause([string]$Paragraph, [string]$Quote, [string]$ReplacementLabel) {
    $labelText = Get-LabelPattern $ReplacementLabel
    $candidate = [regex]::Replace($Paragraph, $labelText, 'LABEL', [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ((Test-Interrogative $candidate) -or (Test-ConditionalEvidence $candidate -Decision) -or
        (Test-TentativeEvidence $candidate) -or
        $candidate -match "(?i)\b(?:not|never|no longer|cannot|can['\u2019]t|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t|another|different|unrelated|separate|other)\b") {
        return $false
    }
    $outcome = '(?:(?:the|this|that|reported|same|actual|original)\s+){1,4}(?:behavior|issue|bug|regression|problem)'
    $cause = "(?:\broot cause of\s+$outcome\s+(?:is|was)\s+(?:(?:in|within)\s+)?$labelText|" +
        "\b$outcome(?:['\u2019]s)?\s+root cause\s+(?:is|was)\s+(?:(?:in|within)\s+)?$labelText|" +
        "\b$outcome\s+(?:is|was)\s+(?:caused by|due to)\s+$labelText)"
    return $Paragraph -match "(?i)$cause" -and $Quote -match "(?i)$cause"
}

function Test-BlockedValidation([string]$Prose) {
    $resource = '(?:sample|repro(?:duction)?|repo(?:sitory)?|link|attachment|archive)'
    $blocked = '(?:inaccessible|unavailable|expired|missing|broken|not found|404|403|unauthorized|forbidden)'
    return $Prose -match "(?i)\b$resource\b.{0,60}\b$blocked\b|\b$blocked\b.{0,60}\b$resource\b" -or
        $Prose -match '(?i)\b(?:timed out|timeout|permission denied|authentication failed|authorization failed|network failure|infrastructure failure|build failed|failed to (?:download|clone|build|compile|install)|cannot (?:download|clone|build|compile|install))\b'
}

function Test-TriageRetraction([string]$Paragraph) {
    $negative = "(?:not|never|no longer|cannot|can['\u2019]t|unable to|failed to|couldn['\u2019]t|could not|did not|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t)"
    $modifiers = '(?:(?:be|been|being|able to|yet|still|currently|successfully|actually|fully)\s+){0,4}'
    return $Paragraph -match "(?i)\b$negative\s+$modifiers(?:review(?:ed)?|triage(?:d)?|investigate(?:d)?|complete(?:d)?|finish(?:ed)?)\b" -or
        $Paragraph -match '(?i)\b(triage|review|investigation)\s+(?:is|was|remains)\s+(incomplete|unfinished)\b'
}

function Get-NoReproductionOutcomePattern([string]$ReferenceContext) {
    $outcome = Get-ValidationOutcomePattern $ReferenceContext
    $modifiers = '(?:(?:successfully|reliably|consistently|actually|locally|independently)\s+){0,3}'
    $negative = "(?:cannot|can['\u2019]t|could not|couldn['\u2019]t|(?:was|were|have been)\s+unable to|unable to|did not|didn['\u2019]t)"
    return "(?:\b$negative\s+$modifiers(?:reproduce|replicate)\s+$outcome\b|\b$outcome\s+(?:(?:is|was|has been)\s+not|cannot|can['\u2019]t|could not|couldn['\u2019]t)\s+(?:be\s+)?$modifiers(?:reproduced|replicated)\b)"
}

function Test-UnobservedReproduction([string]$Prose, [string]$ReferenceContext = '') {
    if (-not $ReferenceContext) { $ReferenceContext = $Prose }
    $outcome = Get-ValidationOutcomePattern $ReferenceContext
    $negative = "(?:did not|didn['\u2019]t|have not|haven['\u2019]t|has not|hasn['\u2019]t|had not|hadn['\u2019]t|never)"
    $modifiers = '(?:(?:actually|yet|ever|even|personally|locally)\s+){0,3}'
    $object = "(?:it|$outcome|(?:(?:the|this|your|provided)\s+)?(?:sample|repro(?:duction)?|project))"
    $nonAttempt = "(?i)\b$negative\s+$modifiers(?:try|tried|test|tested|attempt(?:ed)?|run|ran|execute[ds]?)\b(?:\s+$object\b|\s*(?=[.;!?\r\n]|$))"
    $untested = "(?i)(?<![\w])$object\s+(?:is|was|remains)\s+(?:untested|not\s+(?:yet\s+)?tested|never\s+tested)\b"
    $gap = '(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|although|except|yet)\b).){0,60}'
    $pendingOutcome = "(?i)(?:$(Get-NoReproductionOutcomePattern $ReferenceContext))$gap\b(?:until|pending|while\s+(?:awaiting|waiting\s+(?:for|on)))\b"
    $pendingResource = '(?i)\b(?:pending|awaiting|waiting\s+(?:for|on))\s+(?:(?:a|an|the|your|their|requested|usable|working|minimal)\s+){0,3}(?:sample|repro(?:duction)?|project|access|permission|build|download)\b'
    $paragraphs = @($Prose -split '\r?\n[ \t]*\r?\n')
    $position = 0
    foreach ($paragraph in [regex]::Split((Get-Prose $ReferenceContext), '(\r?\n[ \t]*\r?\n)')) {
        if ($paragraph -cin $paragraphs -and
            ($paragraph -match $nonAttempt -or $paragraph -match $untested -or
                $paragraph -match $pendingOutcome -or $paragraph -match $pendingResource) -and
            (Test-CurrentIssueScope $paragraph $ReferenceContext -ReferenceStart $position)) { return $true }
        $position += $paragraph.Length
    }
    return $false
}

function Test-NoReproductionParagraph([string]$Paragraph, [string]$ReferenceContext = '') {
    if (-not $ReferenceContext) { $ReferenceContext = $Paragraph }
    $modifiers = '(?:(?:successfully|reliably|consistently|actually|locally|independently)\s+){0,3}'
    $pastNegative = "\b(?:could\s+not|couldn['\u2019]t)\s+(?=(?:be\s+)?$modifiers(?:reproduce|replicate)d?\b)"
    $candidate = [regex]::Replace($Paragraph, "(?i)$pastNegative", 'not ')
    if ((Test-Interrogative $Paragraph) -or (Test-ConditionalEvidence $Paragraph) -or
        (Test-TentativeEvidence $candidate) -or (Test-PositiveValidation $Paragraph $ReferenceContext) -or
        (Test-UnobservedReproduction $Paragraph $ReferenceContext) -or
        $Paragraph -match "(?i)\b(?:please|do not|don['\u2019]t|never|avoid|should not|must not|asked|instruct(?:ed|ion)?|recommend(?:ed|ation)?)\b") {
        return $false
    }
    return $Paragraph -match "(?i)$(Get-NoReproductionOutcomePattern $ReferenceContext)"
}

function Test-NonRegressionValidation([string]$Paragraph, [string]$Prose, [string]$ReferenceContext = '') {
    if ((Test-Interrogative $Paragraph) -or (Test-ConditionalEvidence $Paragraph -Decision) -or
        (Test-TentativeEvidence $Paragraph) -or (Test-ForeignOutcome $Prose) -or
        $Paragraph -match "(?i)\b(not|never|no longer|isn.t|wasn.t|doesn.t)\s+(?:(?:the|exactly|quite|really|actually|necessarily|at all|even)\s+){0,3}same (?:behavior|issue|problem)\b|\b(not|isn.t|wasn.t)\s+not (?:a )?regression\b") {
        return $false
    }
    if (-not $ReferenceContext) { $ReferenceContext = $Prose }
    $outcome = Get-ValidationOutcomePattern $ReferenceContext
    $subject = $outcome
    $foreignReference = Test-ForeignIssueReference $ReferenceContext
    if (-not $foreignReference) { $subject = "(?:$outcome|this|it)" }
    $clause = "(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|although|except|yet|not|never|cannot|isn['\u2019]t|wasn['\u2019]t|doesn['\u2019]t)\b).){0,80}"
    $sameOutcome = '(?:(?:the|this|that|reported|actual|original)\s+){1,3}same (?:behavior|issue|problem)'
    $comparison = "(?<![\w])$subject(?![\w])$clause\bsame (?:behavior|issue|problem)\b"
    if (-not $foreignReference) { $comparison = "(?:$comparison|\b$sameOutcome\b)" }
    $framework = '(?:\.NET(?:\s+MAUI)?|MAUI)'
    $version = '\d+(?:\.\d+){0,3}(?:[- .]*(?:preview|rc)[- .]*\d+(?:\.\d+)*)?(?![\w-]|\.(?=\w))'
    $earlierVersion = "\b(?:older|previous|earlier)\s+(?:(?:versions?|releases?)\s+of\s+)?$framework\s+(?:(?:versions?|releases?)\s+)?$version"
    return $Paragraph -match "(?i)(?<![\w])$subject\s+(?:is|was)\s+not\s+(?:a\s+)?regression\b" -or
        $Paragraph -match "(?i)$comparison$clause$earlierVersion"
}

function Test-AssessmentSuperseded($Source, [string]$Label) {
    $contraryLabels = @(switch ($Label) {
        's/no-repro' { 's/verified'; 'i/regression'; 'blazor-webview2-regression'; 'regressed-in-*' }
        'not-regression' { 'i/regression'; 'potential-regression'; 'blazor-webview2-regression'; 'regressed-in-*' }
        's/try-latest-version' { 's/verified'; 's/no-repro' }
    })
    $completedResponse = '(?i)\b(tested|retested|updated|upgraded|verified)\b(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|please|try|retest|test|verify|update|upgrade)\b).){0,80}\b(latest|requested|recommended)\b'
    foreach ($candidate in $sourceMap.Values) {
        if ($candidate.kind -ceq 'unlabeled' -and $candidate.body -ceq "unlabeled: $Label" -and
            $candidate.createdAt -ge $Source.createdAt) { return $true }
        if ($candidate.kind -ceq 'labeled' -and $candidate.createdAt -ge $Source.createdAt -and
            @($contraryLabels | Where-Object { Test-Pattern $candidate.body "labeled: $_" }).Count -gt 0) { return $true }
        if ($candidate.kind -cne 'comment') { continue }
        $sameSource = $candidate.id -ceq $Source.id
        if (-not $sameSource -and (Get-SupersessionTime $candidate) -lt $Source.createdAt) { continue }
        if ($Label -ceq 's/try-latest-version' -and -not $sameSource -and
            $candidate.author -ceq $sourceMap["issue:$IssueNumber"].author -and -not $candidate.isMaintainer -and
            $candidate.author -match '^[A-Za-z0-9_-]+$' -and
            $policy.automationAuthors -notcontains $candidate.author) { return $true }
        if (-not $candidate.isValidator) { continue }
        $prose = Get-Prose $candidate.body
        $referenceContext = Get-MarkdownText $candidate.body
        foreach ($paragraph in ($prose -split '\r?\n[ \t]*\r?\n')) {
            if ($candidate.isMaintainer -and
                (Test-DecisionParagraph $paragraph $Label 'remove' @() -ReferenceContext $referenceContext -Superseding)) { return $true }
            if (Test-Interrogative $paragraph) { continue }
            if ($Label -notin @('s/no-repro', 'not-regression') -and
                $paragraph -match '(?i)\b(should|could|may|might|would|will|maybe|perhaps|possibly|probably|likely|potentially|suspect|assume|expect)\b') { continue }
            switch ($Label) {
                's/no-repro' {
                    if (Test-PositiveValidation $paragraph $referenceContext) { return $true }
                }
                'not-regression' {
                    if ((Test-ConfirmationParagraph $paragraph $referenceContext) -and
                        (Test-RegressionValidation $paragraph)) { return $true }
                }
                'blazor-webview2-regression' {
                    if (Test-NegativeValidation $paragraph $referenceContext) { return $true }
                }
                's/triaged' {
                    if ((Test-CurrentIssueScope $paragraph $referenceContext) -and
                        (Test-TriageRetraction $paragraph)) { return $true }
                }
                's/try-latest-version' {
                    if (-not $sameSource -and
                        (Test-CurrentIssueScope $paragraph $referenceContext) -and
                        $paragraph -match $completedResponse -and
                        -not (Test-NegativeValidation $paragraph $referenceContext) -and
                        -not (Test-BlockedValidation $prose)) { return $true }
                }
            }
        }
    }
    return $false
}

function Get-TechnicalAssessment($Evidence, [string]$Label) {
    return @($Evidence | Where-Object {
        $reference = $_
        $source = $sourceMap[$reference.source]
        if (-not $source.isValidator -or $source.kind -cne 'comment') { return $false }
        $prose = Get-Prose $source.body
        $referenceContext = Get-MarkdownText $source.body
        $paragraphs = @($prose -split '\r?\n[ \t]*\r?\n' |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        if ($paragraphs.Count -ne 1) { return $false }
        $paragraph = $paragraphs[0]
        if ((Test-Interrogative $paragraph) -or
            -not (Test-CurrentIssueScope $paragraph $referenceContext)) { return $false }
        $assessed = switch ($Label) {
            's/triaged' {
                -not (Test-ConditionalEvidence $paragraph -Decision) -and
                -not (Test-TentativeEvidence $paragraph) -and
                $paragraph -match '(?i)\b(reviewed|triaged|investigated)\b.{0,50}\b(issue|report|reproduction|sample|behavior)\b|\b(completed|finished)\b.{0,30}\b(triage|review|investigation)\b' -and
                -not (Test-TriageRetraction $paragraph)
            }
            's/try-latest-version' {
                Test-NewerPublishedVersionRequest $paragraph $reference.quote $referenceContext
            }
            's/no-repro' {
                (Test-NoReproductionParagraph $paragraph $referenceContext) -and
                -not (Test-BlockedValidation $prose) -and
                -not (Test-UnobservedReproduction $prose $referenceContext)
            }
            'not-regression' {
                Test-NonRegressionValidation $paragraph $prose $referenceContext
            }
            'blazor-webview2-regression' {
                $paragraph -match '(?i)\bWebView2\b' -and
                $paragraph -match '(?i)\b(regression|regressed)\b' -and
                @(Get-Confirmation @($reference)).Count -gt 0
            }
            default { throw "Unsupported technical assessment: $Label" }
        }
        return $assessed -and -not (Test-AssessmentSuperseded $source $Label)
    })
}

function Test-WorkaroundFailure([string]$Paragraph, [string]$ReferenceContext = '') {
    if (-not (Test-CurrentIssueScope $Paragraph $ReferenceContext)) { return $false }
    $candidate = [regex]::Replace($Paragraph, '(?i)\bsuggested\s+(?=workarounds?\b)', '')
    if ((Test-Interrogative $Paragraph) -or (Test-ConditionalEvidence $Paragraph -Decision) -or
        (Test-TentativeEvidence $candidate) -or (Test-ForeignOutcome $Paragraph) -or
        $Paragraph -match '(?i)\b(?:do|does|did|has|have|is|are|was|were)\s+(?:(?:the|this|that|suggested|my|your|our|their|a|any)\s+){0,3}workarounds?\b') {
        return $false
    }
    $negative = "(?:not|never|cannot|can['\u2019]t|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t)"
    $gap = "(?:(?![;!?\r\n]|\.(?:\s|$)|\b(?:but|however|instead|can|$negative)\b).){0,80}"
    if ($Paragraph -match "(?i)\bworkarounds?\b$gap\b(?:works?|fixed|resolves?|helps?|succeeds?)\b") { return $false }
    return $Paragraph -match "(?i)\bworkarounds?\b$gap\b(?:(?:does(?:n.t| not)|do(?:n.t| not))\s+(?:work|fix|resolve|help)|fail(?:s|ed)?)\b"
}

function Assert-Evidence($Decision) {
    if ($Decision.evidence -isnot [array] -or $Decision.evidence.Count -lt 1 -or
        $Decision.evidence.Count -gt $policy.maxSources) { throw 'Each change needs one to four evidence sources.' }
    foreach ($reference in $Decision.evidence) {
        Assert-Keys $reference @('source', 'quote')
        if ($reference.source -isnot [string] -or -not $sourceMap.ContainsKey($reference.source) -or
            $reference.quote -isnot [string] -or $reference.quote.Length -lt 12 -or $reference.quote.Length -gt 1500 -or
            -not $sourceMap[$reference.source].body.Contains($reference.quote, [StringComparison]::Ordinal) -or
            -not (Get-Prose $sourceMap[$reference.source].body).Contains($reference.quote, [StringComparison]::Ordinal)) {
            throw "Invalid, fabricated or non-prose evidence for $($Decision.label)."
        }
    }
    if (@($Decision.evidence | Where-Object {
        $sourceMap[$_.source].kind -in @('issue', 'comment')
    }).Count -eq 0) {
        throw 'A label event or related report alone is not evidence about the target issue.'
    }
}

function Assert-Change($Decision, [string]$Action, $Snapshot, [string[]]$EffectiveLabels) {
    Assert-Keys $Decision @('label', 'reason', 'evidence') @('request')
    Assert-Text $Decision.label 1 100
    Assert-Text $Decision.reason 12
    Assert-Evidence $Decision
    $label = $Decision.label
    $category = Get-Category $label
    if ($category -eq 'preserve' -or ($category -eq 'removal-only' -and $Action -eq 'add') -or
        @($Snapshot.labelCatalog | Where-Object { $_.name -ceq $label }).Count -ne 1) {
        throw "Unsupported or nonexistent label: $label"
    }
    $present = $Snapshot.labels -ccontains $label
    if (($Action -eq 'add' -and $present) -or ($Action -eq 'remove' -and -not $present)) {
        throw "The proposed $Action is not a delta: $label"
    }
    if ($Decision.ContainsKey('request') -and $Decision.request) { Assert-Text $Decision.request 16 800 }
    if ($Action -eq 'add' -and $policy.requests -ccontains $label -and
        (-not $Decision.ContainsKey('request') -or -not $Decision.request)) {
        throw "An actionable request is required for $label."
    }
    $maintainerEvidence = @($Decision.evidence | Where-Object {
        $sourceMap[$_.source].isMaintainer -and $sourceMap[$_.source].kind -ceq 'comment'
    })
    $validatorEvidence = @($Decision.evidence | Where-Object {
        $sourceMap[$_.source].isValidator -and $sourceMap[$_.source].kind -ceq 'comment'
    })
    if ($category -eq 'maintainer' -and -not (Test-Decision $Decision.evidence $label $Action)) {
        throw "No explicit current maintainer decision supports $Action $label."
    }
    if ($Action -eq 'add') {
        if ($label -in $policy.confirmation -or $label.StartsWith('regressed-in-')) {
            $requireRegression = $label -ceq 'i/regression' -or $label.StartsWith('regressed-in-')
            $confirmations = @(Get-Confirmation $Decision.evidence -RequireRegression:$requireRegression)
            if ($confirmations.Count -eq 0) {
                if ($requireRegression) {
                    throw "No authorized target-specific validation with explicit earlier working and later failing framework outcomes supports $label."
                }
                throw "No positive authorized target-specific validation supports $label."
            }
            if (-not (Test-CurrentConfirmation $confirmations $Decision.evidence $label $Snapshot)) {
                throw "Later contrary validation or maintainer removal supersedes confirmation for $label; cite current evidence."
            }
        }
        if ($label.StartsWith('regressed-in-')) {
            $version = $label.Substring('regressed-in-'.Length)
            if ($version -notmatch '^\d' -or
                @(Get-Confirmation $Decision.evidence -RequireRegression -FirstBadVersion $version).Count -eq 0) {
                throw "No full-paragraph first-bad-version evidence supports $label."
            }
        }
        if ($policy.technicalAssessment -ccontains $label -and
            @(Get-TechnicalAssessment $Decision.evidence $label).Count -eq 0) {
            if ($label -ceq 's/try-latest-version') {
                throw 'A current authorized request for one exactly cited, published MAUI version strictly newer than the unambiguous reported baseline is required for s/try-latest-version.'
            }
            throw "A current affirmative, label-specific authorized technical assessment is required for $label."
        }
        if ($label -eq 's/duplicate 2️⃣' -and
            @($Decision.evidence | Where-Object { $sourceMap[$_.source].kind -eq 'related' }).Count -eq 0) {
            throw 'A duplicate decision must cite the fetched canonical related issue.'
        }
    } elseif ($category -ne 'maintainer' -and -not (Test-Decision $Decision.evidence $label 'remove')) {
        $transition = $policy.removalTransitions[$label]
        $replacement = @($EffectiveLabels | Where-Object {
            $name = $_
            @($transition | Where-Object { Test-Pattern $name $_ }).Count -gt 0
        })
        $transitionEvidence = @()
        foreach ($replacementLabel in $replacement) {
            if ($policy.confirmation -ccontains $replacementLabel) {
                $confirmations = @(Get-Confirmation $Decision.evidence -RequireRegression:($replacementLabel -ceq 'i/regression'))
                if (Test-CurrentConfirmation $confirmations $Decision.evidence $replacementLabel $Snapshot) {
                    $transitionEvidence += $confirmations
                }
            } elseif ($policy.technicalAssessment -ccontains $replacementLabel) {
                $transitionEvidence += @(Get-TechnicalAssessment $Decision.evidence $replacementLabel)
            } elseif ((Get-Category $replacementLabel) -ceq 'content') {
                $transitionEvidence += @($proposal.additions | Where-Object { $_.label -ceq $replacementLabel } |
                    ForEach-Object { $_.evidence } |
                    Where-Object { $sourceMap[$_.source].kind -in @('issue', 'comment') })
            } else {
                $transitionEvidence += $validatorEvidence
            }
        }
        $confirmedTransition = $transitionEvidence.Count -gt 0
        $lastRequest = @($Snapshot.sources | Where-Object {
            $_.kind -ceq 'labeled' -and $_.body -ceq "labeled: $label"
        } | Sort-Object createdAt | Select-Object -Last 1)
        if ($confirmedTransition -and $label -cne 'needs-area-label' -and $lastRequest.Count -gt 0 -and
            @($transitionEvidence | Where-Object {
                $source = $sourceMap[$_.source]
                $source.createdAt -gt $lastRequest[0].createdAt
            }).Count -eq 0) {
            $confirmedTransition = $false
        }
        $areaCorrection = $label.StartsWith('area-') -and
            @($Snapshot.labels | Where-Object { $_.StartsWith('area-') }).Count -eq 1 -and
            @($proposal.additions | Where-Object {
                $addition = $_
                $addition.label.StartsWith('area-') -and @($maintainerEvidence | Where-Object {
                    $reference = $_
                    $source = $sourceMap[$reference.source]
                    $paragraphs = @((Get-Prose $source.body) -split '\r?\n[ \t]*\r?\n' |
                        Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
                    $paragraphs.Count -eq 1 -and
                    (Test-AreaCause $paragraphs[0] $reference.quote $addition.label) -and
                    @($addition.evidence | Where-Object {
                        $_.source -ceq $reference.source -and $_.quote -ceq $reference.quote
                    }).Count -gt 0 -and
                    ($lastRequest.Count -eq 0 -or $source.createdAt -gt $lastRequest[0].createdAt)
                }).Count -gt 0
            }).Count -gt 0
        $contradictedFacet = $label -in @('has-workaround', 'repro:device-only') -and
            @($Decision.evidence | Where-Object {
                $source = $sourceMap[$_.source]
                $recent = $source.kind -in @('issue', 'comment') -and
                    ($lastRequest.Count -eq 0 -or
                        ($source.kind -eq 'comment' -and (Get-SupersessionTime $source) -ge $lastRequest[0].createdAt))
                $prose = Get-Prose $source.body
                $referenceContext = Get-MarkdownText $source.body
                $quote = $_.quote
                $paragraphs = @($prose -split '\r?\n[ \t]*\r?\n' |
                    Where-Object { $_.Contains($quote, [StringComparison]::Ordinal) })
                $recent -and (
                    ($label -eq 'has-workaround' -and $paragraphs.Count -eq 1 -and
                        (Test-WorkaroundFailure $quote $referenceContext) -and
                        (Test-WorkaroundFailure $paragraphs[0] $referenceContext)) -or
                    ($label -eq 'repro:device-only' -and $paragraphs.Count -eq 1 -and
                        (Test-SimulatorReproduction $quote $referenceContext) -and
                        (Test-SimulatorReproduction $paragraphs[0] $referenceContext) -and
                        -not (Test-NegativeValidation $prose $referenceContext))
                )
            }).Count -gt 0
        if (-not ($confirmedTransition -or $areaCorrection -or $contradictedFacet)) {
            throw "Removal lacks an authorized decision or supported narrow transition: $label"
        }
    }
}

$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$null = New-Item -ItemType Directory -Path $OutputDirectory -Force
$null = Assert-RegularPath $OutputDirectory -Directory

if ($Stage -eq 'Gather') {
    $snapshot = Get-Snapshot
    Write-Json 'context.json' $snapshot
    if ($env:GITHUB_OUTPUT) {
        Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "context_hash=$($snapshot.contextHash)"
        Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value 'ready=true'
    }
    Write-Host "Prepared complete bounded evidence for issue ${IssueNumber}: $($snapshot.sources.Count) sources."
    return
}

if (-not $ContextDirectory -or -not $AgentOutputPath) { throw 'Validate requires ContextDirectory and AgentOutputPath.' }
$original = Read-Json (Join-Path $ContextDirectory 'context.json')
$payload = Read-Json $AgentOutputPath 256KB
if (($env:GITHUB_ACTIONS -eq 'true' -and -not $ExpectedContextHash) -or
    ($ExpectedContextHash -and $original.contextHash -cne $ExpectedContextHash)) {
    throw 'The context artifact does not match the independently prepared job output.'
}
$current = Get-Snapshot
if ($original.contextHash -cne $current.contextHash -or $original.policyHash -cne $policyHash) {
    throw 'The issue, comments, labels, references, authority or catalog changed. Refusing stale publication; request a fresh run.'
}
$sourceMap = @{}
foreach ($source in $current.sources) { $sourceMap[$source.id] = $source }
Assert-Keys $payload @('items') @('errors', 'warnings')
if ($payload.ContainsKey('errors') -and @($payload.errors).Count -gt 0) { throw 'The agent output includes errors; refusing partial proposals.' }
if ($payload.items -isnot [array] -or $payload.items.Count -eq 0 -or $payload.items.Count -gt 3) {
    throw 'Expected one structured comment and at most two label intents, or one no-op/incomplete result.'
}
$items = @($payload.items)
if ($items.Count -eq 1 -and $items[0].type -in @('noop', 'report_incomplete', 'missing_tool')) {
    Write-Json 'validation.json' @{ status = $items[0].type; contextHash = $current.contextHash; writes = 0 }
    if ($items[0].type -ne 'noop') { throw 'Triage is incomplete; inspect the agent result, resolve the missing evidence, and rerun.' }
    Write-Host 'No label changes requested.'
    return
}
foreach ($item in $items) {
    if ($item.type -cnotin @('add_comment', 'add_labels', 'remove_labels')) { throw "Unexpected safe-output type: $($item.type)" }
    $keys = if ($item.type -eq 'add_comment') { @('type', 'item_number', 'body', 'data') }
        else { @('type', 'item_number', 'labels') }
    $optionalKeys = @('secrecy', 'integrity')
    if ($item.type -ceq 'add_comment') { $optionalKeys += 'temporary_id' }
    Assert-Keys $item $keys $optionalKeys
    if ($item.ContainsKey('temporary_id') -and
        ($item.temporary_id -isnot [string] -or $item.temporary_id -cnotmatch '^aw_[A-Za-z0-9]{8}$')) {
        throw 'Unexpected generated comment temporary ID.'
    }
    foreach ($annotation in @('secrecy', 'integrity')) {
        if ($item.ContainsKey($annotation)) { Assert-Text $item[$annotation] 1 40 }
    }
    if ($item.item_number -isnot [long] -and $item.item_number -isnot [int]) { throw 'Target must be an integer.' }
    if ($item.item_number -ne $IssueNumber) { throw 'Safe output targets a different issue.' }
}
$commentItems = @($items | Where-Object { $_.type -ceq 'add_comment' })
if ($commentItems.Count -ne 1) { throw 'Exactly one structured triage comment is required.' }
$comment = $commentItems[0]
Assert-Keys $comment.data @('triage')
$proposal = $comment.data.triage
Assert-Keys $proposal @('schemaVersion', 'issueNumber', 'contextHash', 'additions', 'removals', 'withheld')
if ($proposal.schemaVersion -ne 1 -or $proposal.issueNumber -ne $IssueNumber -or
    $proposal.contextHash -cne $current.contextHash) { throw 'Decision record does not match the trusted context.' }
foreach ($field in @('additions', 'removals', 'withheld')) {
    if ($proposal[$field] -isnot [array]) { throw "$field must be an array." }
}
if ($proposal.additions.Count + $proposal.removals.Count -gt $policy.maxChanges -or
    $proposal.removals.Count -gt $policy.maxRemovals -or $proposal.withheld.Count -gt 20) {
    throw 'The proposed triage exceeds its change/report bounds.'
}
if ($proposal.additions.Count + $proposal.removals.Count + $proposal.withheld.Count -eq 0) {
    throw 'An empty result must use noop, not an empty triage comment.'
}
$allDecisions = @($proposal.additions) + @($proposal.removals)
foreach ($decision in $allDecisions) { Assert-Keys $decision @('label', 'reason', 'evidence') @('request') }
$names = @($allDecisions | ForEach-Object { $_.label })
if (@($names | Sort-Object -Unique).Count -ne $names.Count) { throw 'Duplicate or opposing label decisions are not permitted.' }
$removedNames = @($proposal.removals | ForEach-Object { $_.label })
$effectiveLabels = @($current.labels | Where-Object { $removedNames -cnotcontains $_ }) +
    @($proposal.additions | ForEach-Object { $_.label })
if (@($proposal.removals | Where-Object { $_.label.StartsWith('area-') }).Count -gt 1 -or
    @($proposal.additions | Where-Object { $_.label.StartsWith('area-') }).Count -gt 2) {
    throw 'Only one dominant area correction and at most two justified area additions are permitted.'
}
if (@($allDecisions | Where-Object { $_.label.StartsWith('p/') }).Count -gt 0 -and
    @($effectiveLabels | Where-Object { $_.StartsWith('p/') }).Count -gt 1) {
    throw 'A priority change must not leave conflicting priority commitments.'
}
if (@($names | Where-Object { Test-Pattern $_ 'regressed-in-*' }).Count -gt 0 -and
    @($effectiveLabels | Where-Object { Test-Pattern $_ 'regressed-in-*' }).Count -gt 1) {
    throw 'A regression boundary change must not leave multiple regressed-in-* labels active; propose supported removals or withhold the change.'
}
$incompatibleStates = @(
    @{ label = 's/no-repro'; contrary = @('s/verified', 'i/regression', 'blazor-webview2-regression', 'regressed-in-*') }
    @{ label = 'not-regression'; contrary = @('potential-regression', 'i/regression', 'blazor-webview2-regression', 'regressed-in-*') }
    @{ label = 'potential-regression'; contrary = @('i/regression', 'blazor-webview2-regression', 'regressed-in-*') }
)
foreach ($state in $incompatibleStates) {
    $family = @($state.label) + $state.contrary
    $changed = @($names | Where-Object {
        $name = $_
        @($family | Where-Object { Test-Pattern $name $_ }).Count -gt 0
    })
    $contrary = @($effectiveLabels | Where-Object {
        $name = $_
        @($state.contrary | Where-Object { Test-Pattern $name $_ }).Count -gt 0
    })
    if ($changed.Count -gt 0 -and $effectiveLabels -ccontains $state.label -and $contrary.Count -gt 0) {
        throw "A status change must not leave $($state.label) active with $($contrary -join ', '); propose supported removals or withhold the change."
    }
}
foreach ($action in @('add', 'remove')) {
    $decisions = @(if ($action -eq 'add') { $proposal.additions } else { $proposal.removals })
    foreach ($decision in $decisions) { Assert-Change $decision $action $current $effectiveLabels }
    $intents = @($items | Where-Object { $_.type -ceq "${action}_labels" })
    if (($decisions.Count -eq 0 -and $intents.Count -ne 0) -or ($decisions.Count -gt 0 -and $intents.Count -ne 1)) {
        throw "$action label intent count does not match the decisions."
    }
    if ($intents.Count -eq 1) {
        if ($intents[0].labels -isnot [array] -or
            @($intents[0].labels | Where-Object { $_ -isnot [string] }).Count -gt 0 -or
            (Get-Hash @($intents[0].labels | Sort-Object -CaseSensitive)) -cne
            (Get-Hash @($decisions | ForEach-Object { $_.label } | Sort-Object -CaseSensitive))) {
            throw "$action label intent differs from the validated plain-string delta."
        }
    }
}
$withheldNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($decision in $proposal.withheld) {
    Assert-Keys $decision @('label', 'reason')
    Assert-Text $decision.label 1 100
    Assert-Text $decision.reason 12
    if (@($current.labelCatalog | Where-Object { $_.name -ceq $decision.label }).Count -ne 1) {
        throw "A withheld label must name an exact live label: $($decision.label)"
    }
    if (-not $withheldNames.Add($decision.label) -or $names -ccontains $decision.label) {
        throw 'Withheld labels must be unique and cannot overlap additions or removals.'
    }
}
$permissions.Clear()
Assert-Actor

$marker = if ($CommandCommentId -gt 0) { "issue-triage-command:$CommandCommentId" }
    elseif ($env:GITHUB_RUN_ID) { "issue-triage-run:$($env:GITHUB_RUN_ID)" }
    else { "issue-triage-local:$($current.contextHash)" }
$existing = @($current.resultComments | Where-Object {
    $_.body.Contains("<!-- $marker -->")
})
if ($existing.Count -gt 1) { throw 'Multiple triage reports exist for this invocation; use a fresh command or manual dispatch.' }
$report = [Collections.Generic.List[string]]::new()
$decisionMarkers = [Collections.Generic.List[string]]::new()
$report.Add("<!-- $marker -->")
$report.Add('**Issue triage: validated label proposal**')
$report.Add('')
$report.Add('The label handlers apply the delta below. Their final outcome is recorded in the workflow run; publication is not an atomic transaction.')
foreach ($action in @('add', 'remove', 'withheld')) {
    $decisions = @(switch ($action) { 'add' { $proposal.additions }; 'remove' { $proposal.removals }; 'withheld' { $proposal.withheld } })
    foreach ($decision in $decisions) {
        $decisionMarker = "<!-- issue-triage-decision:${action}:$(Get-DecisionFingerprint $decision $action) -->"
        $decisionMarkers.Add($decisionMarker)
        $report.Add($decisionMarker)
        $reason = [Net.WebUtility]::HtmlEncode($decision.reason) -replace '([\\`*_{}\[\]|])', '\$1'
        $report.Add("- **${action}:** ``$($decision.label)`` - $reason")
        if ($action -ne 'withheld') {
            foreach ($reference in $decision.evidence) {
                $source = $sourceMap[$reference.source]
                $report.Add("  Evidence: [$($reference.source)]($($source.url)).")
            }
            if ($decision.ContainsKey('request') -and $decision.request) {
                $request = [Net.WebUtility]::HtmlEncode($decision.request) -replace '([\\`*_{}\[\]|])', '\$1'
                $report.Add("  Request: $request")
            }
        }
    }
}
if (@($proposal.additions | Where-Object { $policy.requests -ccontains $_.label }).Count -gt 0) {
    $report.Add('')
    $report.Add('Feedback labels activate repository Policy Service replies, staleness reminders and potential automatic closure.')
}
if ($env:GITHUB_RUN_ID) {
    $report.Add('')
    $report.Add("[Workflow result](https://github.com/$Repository/actions/runs/$($env:GITHUB_RUN_ID)).")
}
$body = $report -join "`n"
if ($body.Length -gt 50000) { throw 'The rendered comment exceeds its limit.' }
if ($existing.Count -eq 1 -and @($decisionMarkers | Where-Object {
    -not $existing[0].body.Contains($_, [StringComparison]::Ordinal)
}).Count -gt 0) {
    throw 'The prior triage report does not cover this proposal; use a fresh command or manual dispatch before applying changed decisions.'
}
Set-Content -LiteralPath (Join-Path $OutputDirectory 'report.md') -Value $body -Encoding utf8
Write-Json 'decision.json' $proposal
Write-Json 'validation.json' @{ status = 'validated'; contextHash = $current.contextHash; existingComment = $existing.Count -gt 0 }
$comment.body = $body
$comment.Remove('data')
$comment.Remove('temporary_id')
# A retry can complete unchanged decisions from the original report without another comment.
if ($existing.Count -gt 0) { $payload.items = @($items | Where-Object { $_.type -cne 'add_comment' }) }
$payload | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $AgentOutputPath -Encoding utf8
Write-Host "Validated $($proposal.additions.Count) additions and $($proposal.removals.Count) removals for issue $IssueNumber."
