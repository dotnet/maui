#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies the pr-finalize (Phase 4) recommended title/description to the PR.

.DESCRIPTION
    The /review pipeline's Phase 4 already evaluates the PR's existing title and
    description and — when they are stale, vague, or inaccurate — writes a
    copy-paste-ready replacement to:

      CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/pr-finalize/content.md

    Until now that recommendation was only *rendered* in the AI Summary comment, so a
    human had to copy it across by hand. This script closes that last mile: it parses
    the recommendation and applies it through the GitHub REST API, which makes it the
    squash-merge commit message without requiring GraphQL-only token scopes.

    Deliberately conservative — it does nothing unless Phase 4 explicitly recommended an
    update, the raw submitted PR won the candidate comparison, and it never discards author
    signal:

      * Keep-as-is verdict  -> no-op (Phase 4 said the current metadata is already good).
      * Candidate won       -> no-op (candidate-only behavior is not on the PR branch yet).
      * Unparseable content -> no-op (never guess at a replacement).
      * No net change       -> no-op (avoids edit churn / notification spam).
      * Triage prefixes     -> preserved ([WIP], [inflight regression], [net11.0], ...).
                              Un-WIP-ing a PR is the author's call, not the bot's.
      * Testing-note block  -> preserved. Phase 4 is told to omit the repo's required
                              "test the resulting artifacts" note from its recommendation,
                              so applying the body verbatim would silently delete it.

.PARAMETER PRNumber
    PR number to update (required).

.PARAMETER ContentFile
    Path to pr-finalize/content.md. Auto-discovered from PRNumber when omitted.

.PARAMETER WinnerFile
    Path to PRAgent/winner.json. Auto-discovered next to the pr-finalize directory when
    omitted. Automatic edits are allowed only when the manifest names the raw `pr`
    candidate as the winner.

.PARAMETER Repo
    Target repo in owner/name form. Defaults to dotnet/maui.

.PARAMETER DryRun
    Print what would be applied without updating the pull request.

.EXAMPLE
    ./apply-pr-finalize.ps1 -PRNumber 36769

.EXAMPLE
    ./apply-pr-finalize.ps1 -PRNumber 36769 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$ContentFile,

    [Parameter(Mandatory = $false)]
    [string]$WinnerFile,

    [Parameter(Mandatory = $false)]
    [string]$Repo = "dotnet/maui",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Platform tags are owned by the recommendation itself (the pr-finalize title formula is
# "[Platform] Component: What changed"), so they are never re-prepended from the old title.
# Everything else in a leading [bracket] run is author/triage signal and is preserved.
#
# Deliberately excludes "net": a "[net11.0]" tag marks a backport target branch, not a
# platform, and dropping it would erase real triage signal.
$script:PlatformPrefixes = @(
    'android', 'ios', 'maccatalyst', 'macos', 'mac', 'windows', 'winui', 'tizen'
)

# Sentence that uniquely identifies the repo's required "dogfood this PR" note.
$script:TestingNoteMarker = 'Are you waiting for the changes in this PR to be merged?'

function ConvertTo-AzdoSafeConsole {
    <#
    .SYNOPSIS
        Defangs AzDO logging commands in PR-controlled text before it is written to stdout.
    .DESCRIPTION
        Mirrors the canonical implementation in Review-PR.ps1. It is duplicated rather than
        imported because this script also runs standalone (and is dot-sourced by Pester), so
        it cannot depend on the orchestrator's scope. Apply-PRFinalize.Tests.ps1 pins the same
        behaviours that Review-PR.Tests.ps1 asserts for the original.

        Required by rule 6 of ci-copilot-pipeline-security.instructions.md: the Post phase runs
        with GH_COMMENT_TOKEN and sets GateFailed/CopilotFailed, so an unsanitized "##vso[...]"
        reaching stdout here could mask a gate failure.
    #>
    param([string]$Text)

    # Collapse CR/LF/FF/VT so PR-influenceable text can't fabricate a fresh column-0 line,
    # then defang the AzDO logging-command prefixes.
    return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
}

function Test-FinalizeIsNoOp {
    <#
    .SYNOPSIS
        True when Phase 4 concluded the existing title/description are already good.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Content
    )

    if ([string]::IsNullOrWhiteSpace($Content)) { return $true }

    $normalized = ($Content -replace "`r`n", "`n").Trim()

    # Mirrors Test-PhaseContentIsNoOp("pr-finalize") in post-ai-summary-comment.ps1 so the
    # comment and the apply step agree on what "no change recommended" looks like.
    return [bool]($normalized -match '✅\s*Current title and description accurately reflect the change\s*[—-]\s*recommend keeping as-is')
}

function Get-FinalizeApplyDecision {
    <#
    .SYNOPSIS
        Decides whether the recommendation may be applied to the live PR.
    .DESCRIPTION
        Phase 4 runs after candidate comparison. A pr-plus-reviewer or try-fix winner may
        describe code that exists only in a temporary sandbox, so applying its metadata
        would make the PR claim unsubmitted behavior and tests. Fail closed unless the
        machine-readable manifest explicitly says the raw `pr` candidate won.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$WinnerFile
    )

    if ([string]::IsNullOrWhiteSpace($WinnerFile)) {
        return [pscustomobject]@{
            ShouldApply = $false
            Winner      = ''
            Reason      = 'No winner manifest path was provided.'
        }
    }

    if (-not (Test-Path -LiteralPath $WinnerFile)) {
        return [pscustomobject]@{
            ShouldApply = $false
            Winner      = ''
            Reason      = 'The winner manifest is missing.'
        }
    }

    try {
        $manifest = Get-Content -Raw -LiteralPath $WinnerFile -Encoding UTF8 -ErrorAction Stop |
            ConvertFrom-Json -ErrorAction Stop
    } catch {
        return [pscustomobject]@{
            ShouldApply = $false
            Winner      = ''
            Reason      = 'The winner manifest could not be parsed.'
        }
    }

    if ($null -eq $manifest) {
        return [pscustomobject]@{
            ShouldApply = $false
            Winner      = ''
            Reason      = 'The winner manifest is empty.'
        }
    }

    $winner = if ($manifest.PSObject.Properties['winner']) {
        ([string]$manifest.winner).Trim()
    } else {
        ''
    }

    if ($winner -eq 'pr' -and
        $manifest.PSObject.Properties['isPRFix'] -and
        $manifest.isPRFix -eq $true) {
        return [pscustomobject]@{
            ShouldApply = $true
            Winner      = $winner
            Reason      = 'The raw submitted PR won.'
        }
    }

    $reason = if ($winner -match '^(?:pr-plus-reviewer|try-fix-\d+)$') {
        "Candidate '$winner' won; its changes are not on the PR branch."
    } elseif ($winner -eq 'pr') {
        "The winner manifest is inconsistent for the raw PR candidate."
    } else {
        "The winner manifest contains an unsupported winner."
    }

    return [pscustomobject]@{
        ShouldApply = $false
        Winner      = $winner
        Reason      = $reason
    }
}

function Get-FinalizeRecommendation {
    <#
    .SYNOPSIS
        Extracts the recommended title and description from pr-finalize/content.md.
    .OUTPUTS
        Hashtable with Title and Body keys, or $null when the content is not a
        parseable "recommend updating" result.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Content
    )

    if ([string]::IsNullOrWhiteSpace($Content)) { return $null }
    if (Test-FinalizeIsNoOp -Content $Content) { return $null }

    $normalized = $Content -replace "`r`n", "`n"

    # Each recommendation is a "**Recommended <field>**" label followed by a fenced block.
    # The description can itself contain same-length fenced examples. Its outer closing
    # fence must therefore be the final non-whitespace content, rather than the first fence
    # matching the opening length.
    $titlePattern = '(?im)^\s*\*\*Recommended\s+title\*\*\s*\n+(?<fence>`{3,})[^\n]*\n(?<value>.*?)\n\k<fence>[ \t]*(?:\n|$)'
    $bodyPattern = '(?im)^\s*\*\*Recommended\s+description\*\*\s*\n+(?<fence>`{3,})[^\n]*\n(?<value>.*?)\n\k<fence>[ \t]*(?:\n[ \t]*)*\z'

    $titleMatch = [regex]::Match($normalized, $titlePattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $bodyMatch = [regex]::Match($normalized, $bodyPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

    if (-not $titleMatch.Success -or -not $bodyMatch.Success) { return $null }

    $title = $titleMatch.Groups['value'].Value.Trim()
    $body = $bodyMatch.Groups['value'].Value.Trim()

    # A multi-line "title" means the fences were mis-parsed; refuse rather than mangle the PR.
    # Matches any line break, not just "`n": $normalized only collapses CRLF pairs, so a lone
    # CR would otherwise survive and let the title open a new column-0 console line.
    if ([string]::IsNullOrWhiteSpace($title) -or $title -match '[\r\n]') { return $null }
    if ([string]::IsNullOrWhiteSpace($body)) { return $null }

    return @{ Title = $title; Body = $body }
}

function Merge-PreservedTitlePrefix {
    <#
    .SYNOPSIS
        Re-applies leading triage/status tags from the current title.
    .DESCRIPTION
        Phase 4 writes a clean "[Platform] Component: What changed" title, which can drop
        tags that carry real workflow meaning — [WIP], [inflight regression], [net11.0].
        Those are restored in their original order. Platform tags are skipped (the
        recommendation supplies its own), as is any tag already present.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$CurrentTitle,

        [Parameter(Mandatory = $true)]
        [string]$RecommendedTitle
    )

    if ([string]::IsNullOrWhiteSpace($CurrentTitle)) { return $RecommendedTitle }

    $leading = [regex]::Match($CurrentTitle.Trim(), '^(?:\s*\[[^\]]+\])+')
    if (-not $leading.Success) { return $RecommendedTitle }

    $preserved = @()
    foreach ($tagMatch in [regex]::Matches($leading.Value, '\[([^\]]+)\]')) {
        $tag = $tagMatch.Groups[1].Value.Trim()
        if ([string]::IsNullOrWhiteSpace($tag)) { continue }

        # Defence in depth: only carry over tags that look like real triage/status markers
        # ([WIP], [inflight regression], [net11.0], [release/10.0.1xx]). The title is
        # author-controlled, so this stops control characters or an "##vso[" fragment from
        # riding a preserved tag into the PR title and the Post phase's console stream.
        if ($tag -notmatch '^[A-Za-z0-9][A-Za-z0-9 ._/\-]*$') { continue }

        # Platform tag? The recommendation owns those. Strip any trailing version
        # (e.g. "iOS18" -> "ios") so versioned platform tags still match.
        $firstWord = ($tag -split '[\s/]')[0].TrimEnd('0123456789.')
        if ($script:PlatformPrefixes -contains $firstWord.ToLowerInvariant()) { continue }

        # Already carried over by the recommendation (in any position)?
        if ($RecommendedTitle -match ('(?i)\[\s*' + [regex]::Escape($tag) + '\s*\]')) { continue }

        $preserved += "[$tag]"
    }

    if ($preserved.Count -eq 0) { return $RecommendedTitle }

    return (($preserved -join '') + $RecommendedTitle)
}

function Merge-PreservedBodyPreamble {
    <#
    .SYNOPSIS
        Re-attaches the repo's required testing-note preamble to a recommended body.
    .DESCRIPTION
        Phase 4 is instructed to omit the "test the resulting artifacts" boilerplate, so
        applying its body verbatim would delete a note the repo requires on every PR.
        The current body's preamble (leading HTML comment + note blockquote) is preserved.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$CurrentBody,

        [Parameter(Mandatory = $true)]
        [string]$RecommendedBody
    )

    if ([string]::IsNullOrWhiteSpace($CurrentBody)) { return $RecommendedBody }

    $normalizedCurrent = $CurrentBody -replace "`r`n", "`n"
    if ($normalizedCurrent -notmatch [regex]::Escape($script:TestingNoteMarker)) { return $RecommendedBody }

    # Already present in the recommendation — don't duplicate it.
    if (($RecommendedBody -replace "`r`n", "`n") -match [regex]::Escape($script:TestingNoteMarker)) { return $RecommendedBody }

    # Preamble = everything up to the end of the blockquote containing the marker.
    $lines = $normalizedCurrent -split "`n"
    $markerIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match [regex]::Escape($script:TestingNoteMarker)) { $markerIndex = $i; break }
    }
    if ($markerIndex -lt 0) { return $RecommendedBody }

    $endIndex = $markerIndex
    for ($i = $markerIndex + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i].TrimStart().StartsWith('>')) { $endIndex = $i } else { break }
    }

    $preamble = ($lines[0..$endIndex] -join "`n").TrimEnd()
    if ([string]::IsNullOrWhiteSpace($preamble)) { return $RecommendedBody }

    return "$preamble`n`n$RecommendedBody"
}

function New-PullRequestUpdatePayload {
    <#
    .SYNOPSIS
        Builds the minimal REST payload for the changed PR metadata fields.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [bool]$TitleChanged,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [bool]$BodyChanged,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Body
    )

    $payload = [ordered]@{}
    if ($TitleChanged) { $payload['title'] = $Title }
    if ($BodyChanged) { $payload['body'] = $Body }
    return $payload
}

function New-ExclusiveTempFile {
    <#
    .SYNOPSIS
        Creates a temp file at a fresh, unpredictable path, never writing to one that exists.
    .DESCRIPTION
        `Set-Content` follows a pre-existing symlink and writes through to its target, so a
        predictable temp path (pr-finalize-body-<PR>.md) is a write-through primitive if
        anything can pre-create it. Reaching that requires arbitrary filesystem write as the
        agent user, which already grants strictly more capability — but the fix is cheap, so
        close it anyway rather than relying on that argument holding.

        Prefers the AzDO agent temp directory over the shared system temp when available.
        Creating with New-Item (no -Force) refuses any path that already exists, including a
        pre-planted or dangling symlink, so the write cannot be redirected. An occupied path
        is skipped for a fresh random name; after $MaxAttempts the helper throws rather than
        falling back to a predictable path, so exhaustion can never reopen the vector.
    .PARAMETER NameGenerator
        Test seam only. Lets a test force known candidate names so it can pre-plant a symlink
        at the exact path the helper will try. Production uses random names.
    .OUTPUTS
        Full path to the newly created file.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prefix,

        [scriptblock]$NameGenerator = { [System.IO.Path]::GetRandomFileName() },

        [int]$MaxAttempts = 5
    )

    # -PathType Container so a stale AGENT_TEMPDIRECTORY pointing at a *file* falls back
    # cleanly instead of failing later inside New-Item.
    $baseDir = if ($env:AGENT_TEMPDIRECTORY -and (Test-Path -LiteralPath $env:AGENT_TEMPDIRECTORY -PathType Container)) {
        $env:AGENT_TEMPDIRECTORY
    } else {
        [System.IO.Path]::GetTempPath()
    }

    for ($attempt = 0; $attempt -lt $MaxAttempts; $attempt++) {
        $candidate = Join-Path $baseDir "$Prefix-$(& $NameGenerator).md"
        try {
            # -Path, not -LiteralPath: New-Item has no -LiteralPath parameter (binding it
            # throws ParameterBindingException). For this invocation, a complete leaf path
            # without -Name is treated literally, so it cannot resolve onto an existing file.
            # New-Item can expand wildcards when -Path is combined with -Name; do not infer
            # a general no-globbing guarantee from this call. Reviewers have suggested
            # -LiteralPath here twice, but it is not applicable.
            $file = New-Item -ItemType File -Path $candidate -ErrorAction Stop
            return $file.FullName
        } catch [System.IO.DirectoryNotFoundException] {
            # Derives from IOException, so it must be caught ahead of the collision case —
            # a missing base directory will never resolve by picking another name.
            throw
        } catch [System.IO.IOException] {
            # The path is occupied (regular file, or a pre-planted/dangling symlink). Skip it
            # rather than write through, and try a different name.
            continue
        }
        # Anything else (access denied, invalid path) is a real fault: let it surface
        # unwrapped instead of being retried into a generic "after N attempts" message.
    }

    throw "Could not create a temp file under '$baseDir' after $MaxAttempts attempts."
}

# ─── Main ───────────────────────────────────────────────────────────────────────
# Dot-sourced by the Pester suite to test the helpers above without executing the flow.
if ($MyInvocation.InvocationName -eq '.') { return }

if ([string]::IsNullOrWhiteSpace($ContentFile)) {
    $ContentFile = Join-Path (Get-Location) "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/content.md"
}

Write-Host "  📝 PR finalize — applying recommended title/description..." -ForegroundColor Cyan

if (-not (Test-Path -LiteralPath $ContentFile)) {
    Write-Host "     ℹ️  No pr-finalize content at '$(ConvertTo-AzdoSafeConsole $ContentFile)' — nothing to apply." -ForegroundColor Gray
    exit 0
}

if ([string]::IsNullOrWhiteSpace($WinnerFile)) {
    $finalizeDir = Split-Path -Parent $ContentFile
    $prAgentDir = Split-Path -Parent $finalizeDir
    $WinnerFile = Join-Path $prAgentDir 'winner.json'
}

$applyDecision = Get-FinalizeApplyDecision -WinnerFile $WinnerFile
if (-not $applyDecision.ShouldApply) {
    Write-Host "     ⏭️  Leaving PR metadata unchanged: $(ConvertTo-AzdoSafeConsole $applyDecision.Reason)" -ForegroundColor Gray
    exit 0
}

$content = Get-Content -Raw -LiteralPath $ContentFile -Encoding UTF8

if (Test-FinalizeIsNoOp -Content $content) {
    Write-Host "     ✅ Phase 4 recommends keeping the current title/description — no change." -ForegroundColor Green
    exit 0
}

$recommendation = Get-FinalizeRecommendation -Content $content
if (-not $recommendation) {
    Write-Host "     ⚠️  Could not parse a recommended title/description — leaving the PR untouched." -ForegroundColor Yellow
    exit 0
}

# Current metadata, so we can preserve author signal and skip no-op edits.
# A read failure must abort: with an empty $currentBody the preamble merge has nothing to
# re-attach, so we would silently strip the repo's required testing note.
$prJson = $null
try {
    $prOutput = @(& gh api "repos/$Repo/pulls/$PRNumber" 2>$null)
    $readExitCode = $LASTEXITCODE
    if ($readExitCode -ne 0) {
        throw "gh api exited with code $readExitCode."
    }
    $prJson = ($prOutput -join "`n") | ConvertFrom-Json
} catch {
    Write-Host "     ⚠️  Could not read the current PR metadata ($(ConvertTo-AzdoSafeConsole "$_")) — skipping to avoid clobbering it." -ForegroundColor Yellow
    exit 0
}

if (-not $prJson) {
    Write-Host "     ⚠️  GitHub REST API returned no metadata for #$PRNumber — skipping to avoid clobbering it." -ForegroundColor Yellow
    exit 0
}

$currentTitle = if ($prJson.PSObject.Properties['title'] -and $prJson.title) { $prJson.title } else { '' }
$currentBody = if ($prJson.PSObject.Properties['body'] -and $prJson.body) { $prJson.body } else { '' }

$newTitle = Merge-PreservedTitlePrefix -CurrentTitle $currentTitle -RecommendedTitle $recommendation.Title
$newBody = Merge-PreservedBodyPreamble -CurrentBody $currentBody -RecommendedBody $recommendation.Body

$titleChanged = $newTitle.Trim() -ne $currentTitle.Trim()
$bodyChanged = ($newBody -replace "`r`n", "`n").Trim() -ne ($currentBody -replace "`r`n", "`n").Trim()

if (-not $titleChanged -and -not $bodyChanged) {
    Write-Host "     ✅ Recommendation matches the current title/description — no edit needed." -ForegroundColor Green
    exit 0
}

# Safety invariant: never let an edit drop the repo's required testing note. If the current
# body carries it, the replacement must too — otherwise abandon the body edit entirely.
if ($bodyChanged -and
    $currentBody.Contains($script:TestingNoteMarker) -and
    -not $newBody.Contains($script:TestingNoteMarker)) {
    Write-Host "     ⚠️  Replacement body would drop the required testing note — skipping the body edit." -ForegroundColor Yellow
    $bodyChanged = $false
    if (-not $titleChanged) { exit 0 }
}

# Every value below is PR/agent-derived, so it goes through the sanitizer before reaching
# stdout (rule 6). The Post phase sets GateFailed/CopilotFailed, so an unsanitized
# "##vso[task.setvariable ...]" echoed here could mask a gate failure.
Write-Host "     Title: $(ConvertTo-AzdoSafeConsole $newTitle)" -ForegroundColor Gray
Write-Host "     Changed: title=$titleChanged body=$bodyChanged" -ForegroundColor Gray

if ($DryRun) {
    if ($bodyChanged) {
        Write-Host "     --- body preview (first 15 lines) ---" -ForegroundColor DarkGray
        foreach ($line in (($newBody -split "`r?`n") | Select-Object -First 15)) {
            Write-Host "     | $(ConvertTo-AzdoSafeConsole $line)" -ForegroundColor DarkGray
        }
        Write-Host "     --- end preview ---" -ForegroundColor DarkGray
    }
    Write-Host "     🔍 DryRun — would apply the above (no pull request update issued)." -ForegroundColor Yellow
    exit 0
}

$payloadFile = $null
try {
    $payload = New-PullRequestUpdatePayload `
        -TitleChanged $titleChanged `
        -Title $newTitle `
        -BodyChanged $bodyChanged `
        -Body $newBody
    $payloadFile = New-ExclusiveTempFile -Prefix "pr-finalize-payload-$PRNumber"
    $payload | ConvertTo-Json -Compress | Set-Content -LiteralPath $payloadFile -Encoding UTF8 -NoNewline

    $ghArgs = @('api', "repos/$Repo/pulls/$PRNumber", '--method', 'PATCH', '--input', $payloadFile, '--silent')
    $output = & gh @ghArgs 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "     ✅ Applied the recommended PR title/description." -ForegroundColor Green
    } else {
        # Non-fatal: the review comment still carries the recommendation for a human.
        # The API error may quote title/body validation details, so sanitize it too.
        Write-Host "     ⚠️  GitHub REST update failed (non-fatal): $(ConvertTo-AzdoSafeConsole ($output -join ' '))" -ForegroundColor Yellow
    }
} catch {
    Write-Host "     ⚠️  Failed to apply the PR finalize recommendation (non-fatal): $(ConvertTo-AzdoSafeConsole "$_")" -ForegroundColor Yellow
} finally {
    if ($payloadFile) { Remove-Item -LiteralPath $payloadFile -Force -ErrorAction SilentlyContinue }
}

exit 0
