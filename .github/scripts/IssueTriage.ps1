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
$null = ConvertFrom-Markdown -InputObject ' '
$markdownBuilder = [Markdig.MarkdownPipelineBuilder]::new()
$markdownBuilder.PreciseSourceLocation = $true
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
    $sources.Add([ordered]@{
        id = "issue:$IssueNumber"; kind = 'issue'; author = $issue.user.login
        isMaintainer = $false; isValidator = $false; createdAt = Get-Timestamp $issue.created_at
        body = "$($issue.title)`n$($issue.body)"; url = "https://github.com/$Repository/issues/$IssueNumber"
    })
    foreach ($comment in @($comments | Sort-Object created_at, id)) {
        if ($comment.id -eq $CommandCommentId) { continue }
        $maintainer = $comment.user.type -eq 'User' -and $policy.automationAuthors -notcontains $comment.user.login -and
            (Get-Permission $comment.user.login) -in @('write', 'maintain', 'admin')
        $sources.Add([ordered]@{
            id = "comment:$($comment.id)"; kind = 'comment'; author = $comment.user.login
            isMaintainer = $maintainer
            isValidator = $maintainer -or ($comment.user.type -eq 'User' -and $validators -contains $comment.user.login)
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
            '(?:https://github\.com/dotnet/maui/(?:issues|pull)/|(?<![\w/])#)([1-9][0-9]{0,8})(?![\w])')) {
            # Ambiguous RGB/RGBA-shaped shorthand needs an explicit issue/PR context.
            if ($match.Value.StartsWith('#') -and $match.Groups[1].Length -in @(3, 6, 8) -and
                $referenceText.Substring(0, $match.Index) -notmatch '(?i)\b(?:issue|PR|pull request|duplicate of|fix(?:es|ed)?|clos(?:e|es|ed)|resolv(?:e|es|ed)|see(?: also)?|refs?|references?|related to)\s+$') {
                continue
            }
            $number = [int]$match.Groups[1].Value
            if ($number -ne $IssueNumber) { $null = $references.Add($number) }
        }
    }
    if ($references.Count -gt 8) { throw 'More than eight related issue references; refusing to omit potentially contrary context.' }
    foreach ($number in @($references | Sort-Object)) {
        $related = Get-Api "repos/$Repository/issues/$number"
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

function Get-MarkdownText([string]$Body, [switch]$IncludeQuotes) {
    $document = [Markdig.Markdown]::Parse($Body, $markdownPipeline, $null)
    $ranges = [Collections.Generic.List[object]]::new()
    foreach ($node in [Markdig.Syntax.MarkdownObjectExtensions]::Descendants($document)) {
        if ($node -is [Markdig.Syntax.CodeBlock] -or
            $node -is [Markdig.Syntax.Inlines.CodeInline] -or
            $node -is [Markdig.Syntax.HtmlBlock] -or
            $node -is [Markdig.Syntax.Inlines.HtmlInline] -or
            (-not $IncludeQuotes -and $node -is [Markdig.Syntax.QuoteBlock])) {
            $ranges.Add(@{ start = $node.Span.Start; end = $node.Span.End })
        }
    }
    foreach ($match in [regex]::Matches($Body,
        '(?is)<(blockquote|pre|code|script|style|textarea)\b[^>]*>.*?(?:</\1\s*>|\z)',
        [Text.RegularExpressions.RegexOptions]::None, [TimeSpan]::FromSeconds(2))) {
        $ranges.Add(@{ start = $match.Index; end = $match.Index + $match.Length - 1 })
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
    return Get-MarkdownText $Body
}

function Test-Decision($Evidence, [string]$Label, [string]$Action) {
    foreach ($reference in $Evidence) {
        $source = $sourceMap[$reference.source]
        if (-not $source.isMaintainer -or $source.kind -cne 'comment' -or
            -not (Get-Prose $source.body).Contains($reference.quote, [StringComparison]::Ordinal)) { continue }
        $paragraphs = @((Get-Prose $source.body) -split '\r?\n[ \t]*\r?\n' |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        if ($paragraphs.Count -ne 1) { continue }
        $quote = $paragraphs[0]
        $labelText = '(?<![\p{L}\p{N}\p{M}\p{S}_/.:-])' + [regex]::Escape($Label) +
            '(?![\p{L}\p{N}\p{M}\p{S}_/:-]|\.(?=[\p{L}\p{N}\p{M}\p{S}_/.:-]))'
        $oppositeEvent = if ($Action -eq 'add') { 'unlabeled' } else { 'labeled' }
        $oppositeWords = if ($Action -eq 'add') {
            "remove|drop|withdraw|revoke|reject|decline|do not apply|don't apply|do not add|don't add"
        } else { "add|apply|set|approve|accept|do not remove|don't remove" }
        $superseded = @($sourceMap.Values | Where-Object {
            $_.isMaintainer -and (Get-SupersessionTime $_) -gt $source.createdAt -and (
                ($_.kind -ceq $oppositeEvent -and $_.body -ceq "${oppositeEvent}: $Label") -or
                ($_.kind -ceq 'comment' -and (Get-Prose $_.body) -match "(?i)\b(?:$oppositeWords)\b.{0,80}$labelText")
            )
        })
        if ($superseded.Count -gt 0) { continue }
        $withoutLabel = [regex]::Replace($quote, $labelText, 'LABEL', [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($Action -eq 'add' -and $Label -eq 's/not-a-bug' -and
            $quote -match '(?i)\b(expected behavior|by design|working as intended|not a bug)\b' -and
            $quote -notmatch "(?i)\b(not|isn.t)\s+(expected|by design|working as intended|not a bug)\b") { return $true }
        if ($withoutLabel -match "(?i)\b(not|no|don't|do not|should|could|maybe|consider|candidate|asked|suggested|requested|evaluation|experiment|simulation)\b") { continue }
        $verbs = if ($Action -eq 'add') { 'add|apply|set|assign|approve|approved|accept|accepted|mark|prioritize|prioritized' }
            else { 'remove|drop|clear|withdraw|revoke' }
        if ($quote -match "(?i)\b(?:$verbs)\b.{0,80}$labelText") { return $true }
        if ($Action -eq 'add' -and $Label -eq 's/duplicate 2️⃣' -and
            $quote -match '(?i)\bduplicate of\s+#([1-9][0-9]*)' -and
            $sourceMap.ContainsKey("related:$($Matches[1])")) { return $true }
    }
    return $false
}

function Test-NegativeValidation([string]$Prose) {
    $pattern = "(?is)\b(not|cannot|can['\u2019]t|unable to|failed to|couldn['\u2019]t|could not|did not|(?:do|does|did|is|was|were|has|have|are)n['\u2019]t)\b.{0,30}\b(reproduce[ds]?|reproducible|confirm(?:ed|ing)?|verify|verified|validate[ds]?|validating|correctly detect(?:s|ing)?)\b"
    foreach ($paragraph in ($Prose -split '\r?\n[ \t]*\r?\n')) {
        if ($paragraph -match $pattern) { return $true }
    }
    return $false
}

function Get-Confirmation($Evidence) {
    return @($Evidence | Where-Object {
        $reference = $_
        $source = $sourceMap[$reference.source]
        $paragraphs = @((Get-Prose $source.body) -split '\r?\n[ \t]*\r?\n' |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        $source.isValidator -and $source.kind -ceq 'comment' -and
        (Get-Prose $source.body).Contains($reference.quote, [StringComparison]::Ordinal) -and
        (Get-Prose $source.body) -match '(?i)\b(Android|iOS|Windows|MacCatalyst|MacOS|Tizen|Linux|MAUI)\b|\.NET\s*\d+|\b\d+\.\d+' -and
        $paragraphs.Count -eq 1 -and
        $paragraphs[0] -match '(?i)\b(reproduced|reproducible)\b|\b(confirmed|verified)\b.{0,50}\b(reported behavior|same behavior|bug on|issue on|regression on)\b|\btest\b.{0,50}\bcorrectly detect(s|ing)\b' -and
        $paragraphs[0] -notmatch '(?is)\b(should|could|may|might|would|will|maybe|perhaps|possibly|probably|likely|potentially|suspect|assume|expect)\b.{0,50}\b(reproduce[ds]?|reproducible|confirm(?:ed|ing)?|verify|verified|validate[ds]?|validating|correctly detect(?:s|ing)?)\b' -and
        -not (Test-NegativeValidation $paragraphs[0])
    })
}

function Get-TechnicalAssessment($Evidence, [string]$Label) {
    return @($Evidence | Where-Object {
        $reference = $_
        $source = $sourceMap[$reference.source]
        if (-not $source.isValidator -or $source.kind -cne 'comment') { return $false }
        $prose = Get-Prose $source.body
        $paragraphs = @($prose -split '\r?\n[ \t]*\r?\n' |
            Where-Object { $_.Contains($reference.quote, [StringComparison]::Ordinal) })
        if ($paragraphs.Count -ne 1) { return $false }
        $paragraph = $paragraphs[0]
        switch ($Label) {
            's/triaged' {
                $paragraph -match '(?i)\b(reviewed|triaged|investigated)\b.{0,50}\b(issue|report|reproduction|sample|behavior)\b|\b(completed|finished)\b.{0,30}\b(triage|review|investigation)\b' -and
                $paragraph -notmatch "(?i)\b(not|cannot|can't|unable to|could not|do not|don't)\b.{0,30}\b(review|reviewed|triage|triaged|investigate|investigated|complete|completed|finish|finished)\b"
            }
            's/try-latest-version' {
                $paragraph -match '(?i)\bMAUI\b.{0,60}\b\d+\.\d+(?:\.\d+)?\b|\b\d+\.\d+(?:\.\d+)?\b.{0,60}\bMAUI\b' -and
                $paragraph -match '(?i)\b(try|retest|test|verify|update|upgrade)\b.{0,80}\b(latest|newer|version|\d+\.\d+)\b' -and
                $paragraph -notmatch "(?i)\b(not|cannot|can't|do not|don't)\b.{0,30}\b(try|retest|test|verify|update|upgrade)\b"
            }
            's/no-repro' {
                $paragraph -match "(?i)\b(cannot|can't|unable to|could not|not)\s+(?:be\s+)?reproduce(?:d)?\b"
            }
            'not-regression' {
                $paragraph -match '(?i)\bnot (?:a )?regression\b|\bsame (?:behavior|issue|problem)\b.{0,80}\b(?:older|previous|earlier)\b'
            }
            'blazor-webview2-regression' {
                $paragraph -match '(?i)\bWebView2\b' -and
                $paragraph -match '(?i)\b(regression|regressed)\b' -and
                @(Get-Confirmation @($reference)).Count -gt 0
            }
            default { throw "Unsupported technical assessment: $Label" }
        }
    })
}

function Assert-Evidence($Decision) {
    if ($Decision.evidence -isnot [array] -or $Decision.evidence.Count -lt 1 -or
        $Decision.evidence.Count -gt $policy.maxSources) { throw 'Each change needs one to four evidence sources.' }
    foreach ($reference in $Decision.evidence) {
        Assert-Keys $reference @('source', 'quote')
        if ($reference.source -isnot [string] -or -not $sourceMap.ContainsKey($reference.source) -or
            $reference.quote -isnot [string] -or $reference.quote.Length -lt 12 -or $reference.quote.Length -gt 1500 -or
            -not $sourceMap[$reference.source].body.Contains($reference.quote, [StringComparison]::Ordinal)) {
            throw "Invalid or fabricated evidence for $($Decision.label)."
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
            $confirmations = @(Get-Confirmation $Decision.evidence)
            if ($confirmations.Count -eq 0) { throw "No positive authorized validation supports $label." }
            $latest = @($confirmations | Sort-Object { $sourceMap[$_.source].createdAt })[-1]
            $contrary = @($Snapshot.sources | Where-Object {
                $_.kind -eq 'comment' -and $_.isValidator -and
                (Get-SupersessionTime $_) -gt $sourceMap[$latest.source].createdAt -and
                (Test-NegativeValidation (Get-Prose $_.body))
            })
            if ($contrary.Count -gt 0) { throw "Later contrary validation exists for $label; withhold the confirmation." }
            $laterRemovals = @($Snapshot.sources | Where-Object {
                $_.isMaintainer -and $_.kind -ceq 'unlabeled' -and $_.body -ceq "unlabeled: $label" -and
                $_.createdAt -ge $sourceMap[$latest.source].createdAt
            } | Sort-Object createdAt)
            if ($laterRemovals.Count -gt 0) {
                $readdEvidence = @($Decision.evidence | Where-Object {
                    $sourceMap[$_.source].createdAt -gt $laterRemovals[-1].createdAt
                })
                if (-not (Test-Decision $readdEvidence $label 'add')) {
                    throw "A later maintainer removal supersedes confirmation for $label; cite a newer confirmation or explicit re-add."
                }
            }
        }
        if ($label.StartsWith('regressed-in-')) {
            $version = $label.Substring('regressed-in-'.Length)
            $versionPattern = [regex]::Escape($version)
            if ($version -match '^(\d+)-(preview|rc)(\d+)$') {
                $versionPattern = "$($Matches[1])(?:\.0(?:\.0)?)?[- .]*$($Matches[2])[- .]*$($Matches[3])"
            }
            if ($version -notmatch '^\d' -or @($Decision.evidence | Where-Object {
                $_.quote -match "(?i)\b(first bad|from|since|introduced|starting|regressed).{0,80}\b$versionPattern\b"
            }).Count -eq 0) { throw "No first-bad-version evidence supports $label." }
        }
        if ($policy.technicalAssessment -ccontains $label -and
            @(Get-TechnicalAssessment $Decision.evidence $label).Count -eq 0) {
            throw "An affirmative, label-specific authorized technical assessment is required for $label."
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
                $transitionEvidence += @(Get-Confirmation $Decision.evidence)
            } elseif ($policy.technicalAssessment -ccontains $replacementLabel) {
                $transitionEvidence += @(Get-TechnicalAssessment $Decision.evidence $replacementLabel)
            } else {
                $transitionEvidence += $validatorEvidence
            }
        }
        $confirmedTransition = $transitionEvidence.Count -gt 0
        $lastRequest = @($Snapshot.sources | Where-Object {
            $_.kind -ceq 'labeled' -and $_.body -ceq "labeled: $label"
        } | Sort-Object createdAt | Select-Object -Last 1)
        if ($confirmedTransition -and $lastRequest.Count -gt 0 -and
            @($transitionEvidence | Where-Object {
                $source = $sourceMap[$_.source]
                (Get-SupersessionTime $source) -ge $lastRequest[0].createdAt
            }).Count -eq 0) {
            $confirmedTransition = $false
        }
        $areaCorrection = $label.StartsWith('area-') -and
            @($proposal.additions | Where-Object { $_.label.StartsWith('area-') }).Count -gt 0 -and
            @($maintainerEvidence | Where-Object { $_.quote -match '(?i)\b(root cause|caused|due to|instead|rather than|actually)\b' }).Count -gt 0
        $contradictedFacet = $label -in @('has-workaround', 'repro:device-only') -and
            @($Decision.evidence | Where-Object {
                $source = $sourceMap[$_.source]
                $recent = $lastRequest.Count -eq 0 -or
                    ($source.kind -eq 'comment' -and (Get-SupersessionTime $source) -ge $lastRequest[0].createdAt)
                $recent -and (
                    ($label -eq 'has-workaround' -and $_.quote -match "(?i)\bworkarounds?\b.{0,80}\b(?:(?:does(?:n.t| not)|do(?:n.t| not))\s+(?:work|fix|resolve|help)|fail(?:s|ed)?)\b") -or
                    ($label -eq 'repro:device-only' -and $_.quote -match '(?i)\b(reproduced|reproducible)\b.{0,80}\bsimulator\b')
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
    Assert-Keys $item $keys @('secrecy', 'integrity')
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
$existing = @($current.sources | Where-Object {
    $_.kind -eq 'comment' -and $_.author -ceq 'github-actions[bot]' -and $_.body.Contains("<!-- $marker -->")
})
$report = [Collections.Generic.List[string]]::new()
$report.Add("<!-- $marker -->")
$report.Add('**Issue triage: validated label proposal**')
$report.Add('')
$report.Add('The label handlers apply the delta below. Their final outcome is recorded in the workflow run; publication is not an atomic transaction.')
foreach ($action in @('add', 'remove', 'withheld')) {
    $decisions = @(switch ($action) { 'add' { $proposal.additions }; 'remove' { $proposal.removals }; 'withheld' { $proposal.withheld } })
    foreach ($decision in $decisions) {
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
Set-Content -LiteralPath (Join-Path $OutputDirectory 'report.md') -Value $body -Encoding utf8
Write-Json 'decision.json' $proposal
Write-Json 'validation.json' @{ status = 'validated'; contextHash = $current.contextHash; existingComment = $existing.Count -gt 0 }
$comment.body = $body
$comment.Remove('data')
# A retry reconciles the label delta without creating another result comment.
if ($existing.Count -gt 0) { $payload.items = @($items | Where-Object { $_.type -cne 'add_comment' }) }
$payload | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $AgentOutputPath -Encoding utf8
Write-Host "Validated $($proposal.additions.Count) additions and $($proposal.removals.Count) removals for issue $IssueNumber."
