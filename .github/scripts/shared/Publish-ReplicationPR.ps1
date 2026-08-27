#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies a validated reproduction patch and opens a draft pull request.

.DESCRIPTION
    Run only from a clean trusted checkout after candidate and evidence
    validation. The GitHub token must be provided through GH_TOKEN.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ValidatedCandidatePath,

    [Parameter(Mandatory = $true)]
    [string]$PublishedEvidencePath,

    [Parameter(Mandatory = $true)]
    [string]$IssueContextPath,

    [Parameter(Mandatory = $true)]
    [string]$PatchPath,

    [string]$FixPatchPath,

    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$IssueOwner = 'dotnet',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$IssueRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$TargetOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$TargetRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$BaseBranch = 'main',

    [string]$BuildUrl = '',

    [string]$OutputPath = '',

    [switch]$DryRun,

    [switch]$SupersedeExisting
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')
. (Join-Path $PSScriptRoot 'Get-ReplicationUpstreamFix.ps1')

function ConvertTo-ReplicationSingleLine {
    param(
        [AllowEmptyString()][string]$Value,
        [int]$MaximumLength = 160
    )

    if ($null -eq $Value) {
        return ''
    }

    $safe = $Value -replace '[\x00-\x1F\x7F]', ' '
    $safe = $safe -replace '##vso\[[^\]]*\]', ''
    $safe = $safe -replace '##\[[^\]]*\]', ''
    $safe = $safe -replace '::(?:set-output|add-mask|error|warning|notice)[^\s]*', ''
    $safe = $safe -replace '\s+', ' '
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaximumLength) {
        $safe = $safe.Substring(0, $MaximumLength).TrimEnd()
    }
    return $safe
}

function ConvertTo-ReplicationInlineCode {
    param([AllowEmptyString()][string]$Value)
    return (ConvertTo-ReplicationSingleLine -Value $Value -MaximumLength 500).Replace('`', "'")
}

function Get-ReplicationPullRequestMarker {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    return "<!-- MAUI_COPILOT_REPLICATION issue=$IssueNumber platform=$($Platform.ToLowerInvariant()) -->"
}

function New-ReplicationBranchName {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform,
        [Parameter(Mandatory = $true)][string]$BuildId
    )

    $safePlatform = $Platform.ToLowerInvariant() -replace '[^a-z0-9-]', '-'
    $safeBuildId = $BuildId -replace '[^A-Za-z0-9._-]', '-'
    return "copilot/reproduce-$IssueNumber-$safePlatform-$safeBuildId"
}

function Get-ReplicationIndependentReviewBlock {
    <#
        .SYNOPSIS
            Renders the independent review of the winning fix, or says plainly
            that it was not measured.

        .DESCRIPTION
            Reports, never refuses. A blocking finding here does not stop
            publication: the arm's false-positive rate is unmeasurable, because
            every reviewed pull request in the corpus it was validated against
            carries a blocking finding, so there is no negative control. A wrong
            paragraph costs a reader a minute; a wrong refusal costs a certified
            fix, and every gate in this pipeline that could destroy work
            eventually did.

            An absent review renders "Not measured" rather than nothing. Silence
            is what hid the regression cross-reference for its entire life - a
            checker returning nothing is indistinguishable from a feature nobody
            wired up.
    #>
    param(
        [Parameter(Mandatory = $true)]$Candidate
    )

    $header = '**Independent review.**'
    $notMeasured = "$header Not measured - a second model did not return a usable report on this fix."

    $property = $Candidate.PSObject.Properties['fixIndependentReview']
    if (-not $property -or $null -eq $property.Value) { return $notMeasured }
    $review = $property.Value

    $summary = ConvertTo-ReplicationSingleLine -Value ([string]$review.summary) -MaximumLength 600
    if ([string]::IsNullOrWhiteSpace($summary)) { return $notMeasured }

    $model = ConvertTo-ReplicationSingleLine -Value ([string]$review.model) -MaximumLength 60
    $attribution = if ($model) { " A second model (``$model``) reviewed the winning diff without having written it." } else { '' }

    $lines = @("$header$attribution $summary")

    $findings = @()
    $findingsProperty = $review.PSObject.Properties['findings']
    if ($findingsProperty -and $null -ne $findingsProperty.Value) {
        foreach ($entry in @($findingsProperty.Value | Where-Object { $_ })) {
            $detail = ConvertTo-ReplicationSingleLine -Value ([string]$entry.detail) -MaximumLength 800
            if ([string]::IsNullOrWhiteSpace($detail)) { continue }
            $severity = ConvertTo-ReplicationSingleLine -Value ([string]$entry.severity) -MaximumLength 40
            if ([string]::IsNullOrWhiteSpace($severity)) { $severity = 'important' }
            $findings += ('- **' + $severity + '.** ' + $detail)
            if ($findings.Count -ge 6) { break }
        }
    }

    if ($findings.Count -gt 0) {
        $lines += ''
        $lines += $findings
        $lines += ''
        $lines += ('These findings did not block publication. They are reported so a maintainer sees them ' +
                   'in the body rather than having to rediscover them, and any of them may be wrong.')
    }

    return ($lines -join "`n")
}

function Get-ReplicationFixPanelBlock {
    <#
        .SYNOPSIS
            Renders the try-fix panel: every candidate that competed, what it
            proposed, and which one was selected.

        .DESCRIPTION
            The fix phase runs five cross-pollinated try-fix candidates and
            publishes one. Only the winner used to reach the body, so a reader
            could not distinguish a fix chosen from five competing approaches
            from the single candidate that happened to run. That difference is
            not hypothetical: before the tidiness fix, four of five candidates
            were routinely blocked for changing no file, and the published body
            read identically whether the panel had compared five approaches or
            none.

            Reports, never refuses, like every other disclosure in this body. An
            absent panel renders "Not measured" rather than nothing, because a
            silent block is indistinguishable from a feature nobody wired up.
    #>
    param(
        [Parameter(Mandatory = $true)]$Candidate
    )

    $header = '**Try-fix panel.**'
    $notMeasured = "$header Not measured - no per-candidate record was produced for this fix."

    $property = $Candidate.PSObject.Properties['fixPanel']
    if (-not $property -or $null -eq $property.Value) { return $notMeasured }

    $rows = @()
    foreach ($entry in @($property.Value | Where-Object { $_ })) {
        $model = ConvertTo-ReplicationSingleLine -Value ([string]$entry.model) -MaximumLength 60
        if ([string]::IsNullOrWhiteSpace($model)) { $model = 'unknown' }

        $result = ConvertTo-ReplicationSingleLine -Value ([string]$entry.result) -MaximumLength 40
        if ([string]::IsNullOrWhiteSpace($result)) { $result = 'unknown' }

        $detail = ConvertTo-ReplicationSingleLine -Value ([string]$entry.detail) -MaximumLength 300

        # A pipe in model-written prose ends the cell and silently shifts every
        # column after it, so the row would misreport which candidate produced
        # which result. Escaped rather than stripped: the character is often
        # load-bearing in the C# the candidate is describing.
        $model = $model -replace '\|', '\|'
        $result = $result -replace '\|', '\|'
        $detail = $detail -replace '\|', '\|'

        $marker = if ($entry.won) { ' **(selected)**' } else { '' }
        $rows += ('| ' + [int]$entry.attempt + ' | `' + $model + '` | ' + $result + $marker + ' | ' + $detail + ' |')
    }

    if ($rows.Count -eq 0) { return $notMeasured }

    $selected = @($property.Value | Where-Object { $_ -and $_.won }).Count
    $lead = ("$header " + $rows.Count + ' candidate(s) each ran the reviewer''s `try-fix` skill against this ' +
             'reproduction, sequentially and cross-pollinated, so each saw the approaches the earlier ones ' +
             'had already tried. ' +
             $(if ($selected -gt 0) { 'The selected row is the fix published below.' } else { 'No row is marked selected.' }))

    return (@(
        $lead,
        '',
        '| # | Model | Result | Approach or rejection |',
        '| --- | --- | --- | --- |'
    ) + $rows + @(
        '',
        ('Only the selected candidate''s diff was applied and put through the fix and restoration arms. ' +
         'The other rows are recorded so the comparison is visible rather than implied.')
    )) -join "`n"
}

function Get-ReplicationCandidateText {
    <#
        .SYNOPSIS
        Reads an optional validated-candidate property without tripping StrictMode.
    #>
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Candidate.PSObject.Properties[$Name]
    if (-not $property -or $null -eq $property.Value) {
        return ''
    }

    return [string]$property.Value
}

function Test-ReplicationWeakerThanOpenPullRequest {
    <#
        .SYNOPSIS
            True when an incoming publication carries no fix while the open
            pull request it would supersede does carry one.

        .DESCRIPTION
            Superseding exists so a re-run can refresh the evidence for an
            issue after the pipeline changes. It is not licence for a weaker
            publication to destroy a stronger one, which is what happened when
            reproduction-only runs retired two pull requests that each carried
            a four-arm certified fix.
    #>
    param(
        [AllowNull()][AllowEmptyString()][string]$DuplicateBody,
        [bool]$IncomingCarriesFix
    )

    if ($IncomingCarriesFix) { return $false }
    if ([string]::IsNullOrWhiteSpace($DuplicateBody)) { return $false }

    # The fix section is the only part of the body that a reproduction-only
    # publication never writes, so it is the honest discriminator. Matching the
    # title would trust a string the publisher does not control.
    return [bool]($DuplicateBody -match '(?m)^## Proposed fix\s*$')
}

function Get-ReplicationFixRegressionSignal {
    <#
        .SYNOPSIS
            Reports whether the fix deletes a line a previous bug-fix PR added.

        .DESCRIPTION
            The fix arms prove the fix repairs its own oracle. They say nothing
            about what else it changes, so a fix that passes its own test by
            reintroducing a bug someone already fixed is published today as
            `certified-oracle`.

            `Find-RegressionRisks.ps1` answers the sharpest form of that
            question mechanically - no AI, no device - and `REVERT` is precisely
            the shape of that failure.

            It REPORTS. It never refuses. Every gate in this pipeline that could
            destroy work eventually did, and a regression signal in the PR body
            costs nothing when it is wrong, while withholding a sound fix on a
            mechanical string comparison costs a reproduction, a device and four
            certification arms. An unavailable or failing check therefore says
            it was not measured, which is the truth, rather than CLEAN, which is
            a claim.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$FixPatchPath,
        [Parameter(Mandatory = $false)][AllowEmptyString()][string]$ScriptRoot,
        [Parameter(Mandatory = $false)][string]$Repo = 'dotnet/maui'
    )

    if ([string]::IsNullOrWhiteSpace($FixPatchPath) -or
        -not (Test-Path -LiteralPath $FixPatchPath)) {
        return $null
    }

    # Resolved here rather than as a parameter default, because a default is
    # evaluated on every call - including the ones that return above - so an
    # empty $PSScriptRoot would make a check that exists to REPORT throw instead.
    #
    # Two layouts have to work. In the repository this script sits in
    # .github/scripts/shared/ and the checker one level up in .github/scripts/.
    # In production the publish job copies a fixed list of scripts into a flat
    # trusted directory, so the checker sits beside this file instead. Looking
    # only upwards is why the cross-reference silently never ran: pull request
    # 406 was published from the commit that added it and carries no signal.
    $searchRoots = @()
    if (-not [string]::IsNullOrWhiteSpace($ScriptRoot)) {
        $searchRoots += $ScriptRoot
    } elseif (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $searchRoots += $PSScriptRoot
        $searchRoots += (Split-Path -Parent $PSScriptRoot)
    }

    $checker = $null
    foreach ($root in $searchRoots) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $probe = Join-Path $root 'Find-RegressionRisks.ps1'
        if (Test-Path -LiteralPath $probe -PathType Leaf) { $checker = $probe; break }
    }

    $outputDir = Join-Path ([IO.Path]::GetTempPath()) ("regression-" + [guid]::NewGuid().ToString('N'))
    $verdict = $null
    # A missing checker falls through to 'not measured' rather than returning
    # nothing. Silence is what hid this: an absent line is indistinguishable
    # from a feature that was never wired up, while a stated non-measurement is
    # a claim someone can check.
    if ($checker) {
        try {
            & pwsh -NoProfile -File $checker -DiffPath $FixPatchPath -Repo $Repo `
                -OutputDir $outputDir 2>&1 | Out-Null
            $resultPath = Join-Path $outputDir 'result.txt'
            if (Test-Path -LiteralPath $resultPath) {
                $verdict = (Get-Content -LiteralPath $resultPath -Raw).Trim()
            }
        } catch {
            $verdict = $null
        } finally {
            Remove-Item -LiteralPath $outputDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    switch ($verdict) {
        'CLEAN' {
            '**Regression cross-reference.** No line this fix deletes was added by a recent bug-fix pull request.'
        }
        'OVERLAP' {
            ('**Regression cross-reference.** This fix touches a file a recent bug-fix pull request also ' +
             'changed, but deletes none of the lines that fix added.')
        }
        'REVERT' {
            ('**⚠️ Regression cross-reference.** This fix deletes one or more lines that a recent **bug-fix** ' +
             'pull request added to the same file. That is the shape of a fix that passes its own test by ' +
             'reintroducing a defect someone already repaired. Review the fix commit against that history ' +
             'before merging.')
        }
        default {
            '**Regression cross-reference.** Not measured for this fix.'
        }
    }
}

function Get-ReplicationUpstreamFixSignal {
    <#
        .SYNOPSIS
            Reports whether the project has already fixed this issue on a
            branch the reproduction was not built against.

        .DESCRIPTION
            dotnet/maui merges bug fixes to `inflight/current` and the issue
            stays open until release, so issue state carries no information:
            all three cases this was validated against are OPEN and carry a
            merged fix. The tree we build tracks `main`, so an upstream fix
            living only on `inflight/current` is invisible to every other
            signal - the issue is open, the defect genuinely reproduces, and
            our red test is honest.

            So the reproduction is sound and only its *redundancy to the
            project* is undisclosed. This REPORTS. Refusing would destroy
            sound reproductions on a mechanical string comparison, and every
            gate in this pipeline that could destroy work eventually did.

            Presence is asked of the upstream branch and absence of the exact
            base commit under test, because "a commit touched this path" is
            not "the file exists there" - the commits endpoint returns
            deletions too, and a reverted test case is the opposite signal:
            upstream tried a fix and backed it out.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$IssueNumber,
        [Parameter(Mandatory = $false)][AllowEmptyString()][string]$BaseSha,
        [Parameter(Mandatory = $false)][string]$Repo = 'dotnet/maui',
        [Parameter(Mandatory = $false)][string]$UpstreamRef = 'inflight/current'
    )

    if ($IssueNumber -notmatch '^\d+$') {
        return '**Upstream cross-reference.** Not measured for this reproduction.'
    }

    $candidatePaths = @(
        "src/Controls/tests/TestCases.HostApp/Issues/Issue$IssueNumber.cs",
        "src/Controls/tests/TestCases.HostApp/Issues/Issue$IssueNumber.xaml"
    )

    $sawUnknown = $false
    foreach ($path in $candidatePaths) {
        $upstream = Get-ReplicationUpstreamTestCasePresence `
            -Path $path -Ref $UpstreamRef -Repo $Repo
        if ($upstream -eq 'unknown') { $sawUnknown = $true; continue }
        if ($upstream -eq 'absent') { continue }

        # Present upstream. It is only news if the tree under test lacks it.
        if ($BaseSha -match '^[0-9a-fA-F]{40,64}$') {
            $local = Get-ReplicationUpstreamTestCasePresence `
                -Path $path -Ref $BaseSha -Repo $Repo
            if ($local -eq 'unknown') { $sawUnknown = $true; continue }
            if ($local -eq 'present') { continue }
        }

        # Oldest commit touching the path is the one that introduced it; the
        # newest may be a later edit that names an unrelated pull request.
        $subject = ''
        try {
            $subject = (& gh api `
                "repos/$Repo/commits?sha=$UpstreamRef&path=$path&per_page=100" `
                --jq '[.[]|.commit.message|split("\n")[0]]|last' 2>$null | Out-String).Trim()
        } catch {
            $subject = ''
        }

        $introduced = ''
        if ($subject) {
            # Upstream text is uncontrolled, so it is sanitized inline rather
            # than handed to a validator that throws. A presentation bound must
            # never be able to discard the work it describes.
            $clean = ($subject -replace '[^\x20-\x7E]', ' ') -replace '\s+', ' '
            $clean = $clean.Trim()
            if ($clean.Length -gt 160) { $clean = $clean.Substring(0, 157) + '...' }
            if ($clean) { $introduced = " It was introduced by *$clean*." }
        }
        return ('**⚠️ Upstream cross-reference.** A test case for this issue already exists on ' +
                "``$Repo``'s ``$UpstreamRef`` branch, which this reproduction was not built against." +
                $introduced +
                ' The defect is genuinely red on the tree under test, so the evidence above stands - ' +
                'but a fix may already be queued for release, in which case this pull request is ' +
                'redundant. Please check that branch before merging.')
    }

    if ($sawUnknown) {
        return '**Upstream cross-reference.** Not measured: the upstream branch could not be read.'
    }

    return ("**Upstream cross-reference.** No test case for this issue exists on ``$Repo``'s " +
            "``$UpstreamRef`` branch.")
}

function Get-ReplicationExpressionSkeleton {
    <#
        .SYNOPSIS
        Reduces C# source to the ordered member-access and +/- operator tokens
        of each statement, with single-assignment locals inlined.

        .DESCRIPTION
        Receivers and local names are dropped deliberately, so renaming a
        variable cannot hide the fact that two expressions compute the same
        thing. `var i = v.AdjustedContentInset; ... i.Bottom` and
        `v.AdjustedContentInset.Bottom` therefore reduce to the same tokens.
    #>
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Source)

    if ([string]::IsNullOrWhiteSpace($Source)) { return @() }

    $text = $Source -replace '\s+', ' '
    foreach ($local in [regex]::Matches($text, '\bvar\s+(\w+)\s*=\s*([A-Za-z_]\w*(?:\.[A-Z]\w*)+)\s*;')) {
        $name = $local.Groups[1].Value
        $expr = $local.Groups[2].Value
        $text = $text -replace ('\b' + [regex]::Escape($name) + '\.'), ($expr + '.')
    }

    $skeletons = @()
    foreach ($statement in ($text -split ';')) {
        $tokens = @()
        foreach ($token in [regex]::Matches($statement, '\.([A-Z]\w*)|([+\-])')) {
            if ($token.Groups[1].Success) { $tokens += $token.Groups[1].Value }
            else { $tokens += ('OP' + $token.Groups[2].Value) }
        }
        if ($tokens.Count -gt 0) { $skeletons += , $tokens }
    }
    # Returned with a leading comma, and consumed with foreach rather than a
    # pipeline: a one-statement source otherwise unrolls to its bare tokens,
    # every "skeleton" reads as length 1, and the detector goes silent for a
    # reason that has nothing to do with the source.
    return , $skeletons
}

function Get-ReplicationOracleIndependenceSignal {
    <#
        .SYNOPSIS
        Reports a test whose expected value is computed with the same
        arithmetic the product fix introduces.

        .DESCRIPTION
        PR 469 asserted `ContentSize.Height + AdjustedContentInset.Bottom -
        Bounds.Height` while the fix it validates computes exactly that. An
        oracle that restates its implementation cannot fail while the
        implementation is present, and cannot tell a correct formula from the
        one this fix happens to use -- so all four control arms pass and prove
        nothing about correctness. A reviewer found it; nothing in the pipeline
        could.

        Measured before shipping, over all 57 open fix pull requests: it fires
        on PR 469 and on nothing else, so the false-positive rate on real
        published work is 0 of 56. Prevalence that low is why this reports
        rather than refuses -- a legitimate test may need the same API to state
        a specification, and per section 120 a guard that refuses correct tests
        is worse than the defect it prevents.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$TestSource,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$FixSource
    )

    if ([string]::IsNullOrWhiteSpace($TestSource)) { return '' }
    if ([string]::IsNullOrWhiteSpace($FixSource)) { return '' }

    $fixSkeletons = @()
    foreach ($skeleton in (Get-ReplicationExpressionSkeleton -Source $FixSource)) {
        if ($skeleton.Count -ge 4) { $fixSkeletons += , $skeleton }
    }
    if ($fixSkeletons.Count -eq 0) { return '' }

    foreach ($candidate in (Get-ReplicationExpressionSkeleton -Source $TestSource)) {
        # Real arithmetic only: at least two members and one operator, or a
        # bare property comparison would match everything.
        if ($candidate.Count -lt 4) { continue }
        $members = @($candidate | Where-Object { -not $_.StartsWith('OP') })
        if ($members.Count -lt 2) { continue }
        if (-not ($candidate | Where-Object { $_.StartsWith('OP') })) { continue }

        foreach ($fixSkeleton in $fixSkeletons) {
            $limit = $fixSkeleton.Count - $candidate.Count
            for ($start = 0; $start -le $limit; $start++) {
                $matched = $true
                for ($offset = 0; $offset -lt $candidate.Count; $offset++) {
                    if ($fixSkeleton[$start + $offset] -ne $candidate[$offset]) {
                        $matched = $false
                        break
                    }
                }
                if ($matched) {
                    $formula = ($candidate | ForEach-Object {
                            if ($_.StartsWith('OP')) { $_.Substring(2) } else { $_ }
                        }) -join ' '
                    return ('**⚠️ Oracle independence.** The test computes its expected value with the ' +
                        'same arithmetic this fix introduces (`' +
                        (ConvertTo-ReplicationSingleLine -Value $formula -MaximumLength 200) +
                        '`), so it restates the implementation rather than the behaviour a user ' +
                        'observes. It cannot distinguish a correct formula from this one, and the ' +
                        'control arms cannot see the difference. Please check the assertion against ' +
                        'an independently observable symptom.')
                }
            }
        }
    }
    return ''
}

function New-ReplicationPullRequestBody {
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)]$Evidence,
        [Parameter(Mandatory = $true)][string]$IssueTitle,
        [Parameter(Mandatory = $true)][string]$IssueOwner,
        [Parameter(Mandatory = $true)][string]$IssueRepository,
        [AllowEmptyString()][string]$BuildUrl,
        [AllowEmptyString()][string]$RegressionSignal,
        [AllowEmptyString()][string]$OracleSignal,
        [AllowEmptyString()][string]$UpstreamSignal
    )

    $issueNumber = [int]$Candidate.issueNumber
    $platform = ConvertTo-ReplicationSingleLine -Value ([string]$Candidate.platform) -MaximumLength 40
    $testType = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.testType)
    $testFilter = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.testFilter)
    # A validated document deserialised from JSON is a PSCustomObject, and
    # reading a property it does not carry throws under StrictMode. Build
    # 14999470 produced a ready candidate and then failed the whole publication
    # on exactly that, so these descriptive names are read defensively.
    $candidateTestClass = Get-ReplicationCandidateText -Candidate $Candidate -Name 'testClassName'
    $candidateTestMethod = Get-ReplicationCandidateText -Candidate $Candidate -Name 'testMethodName'

    # Controls UI tests derive from the parameterised UITest fixture
    # (src/Controls/tests/TestCases.Shared.Tests/UITest.cs), so NUnit inserts
    # the TestDevice argument between the class and the method at run time:
    # Issue37281(Android).TouchScrollingDoesNotRedrawShadowedContent. Two
    # reviewers independently copied the plain "Class.Method" this body used to
    # publish, and both selected zero tests -- which reads as a passing run
    # rather than a missing one. Publish the name the runner actually reports.
    $uiFixtureArgument = if ([string]$Candidate.testType -ceq 'ui') {
        switch ([string]$Candidate.platform) {
            'android' { 'Android' }
            'ios' { 'iOS' }
            'windows' { 'Windows' }
            'catalyst' { 'Mac' }
            default { '' }
        }
    } else {
        ''
    }

    $exactTestName = if ($candidateTestClass -and $candidateTestMethod) {
        $qualifiedClass = if ($uiFixtureArgument) {
            "{0}({1})" -f $candidateTestClass, $uiFixtureArgument
        } else {
            $candidateTestClass
        }
        ConvertTo-ReplicationInlineCode `
            -Value ("{0}.{1}" -f $qualifiedClass, $candidateTestMethod)
    } else {
        $testFilter
    }

    # Parentheses are grouping operators in the VSTest filter grammar, so the
    # fixture-qualified name above cannot be pasted into an equality filter
    # unescaped. The issue-keyed class token is the form reviewers verified
    # selects exactly this test on every UI lane.
    $uiSelectorLine = if ($uiFixtureArgument -and $candidateTestClass -and $candidateTestMethod) {
        '- UI runner selector: ``--filter "FullyQualifiedName~Issue{0}"`` — the exact name above carries the ``({1})`` fixture argument, so an equality filter on ``{2}.{3}`` selects no tests and an unescaped ``(`` is read as filter grouping; use this contains form' -f `
            $issueNumber, $uiFixtureArgument, $candidateTestClass, $candidateTestMethod
    } else {
        ''
    }

    # The stock device-test runner honours only "Category=X" and
    # "SkipCategories=X,Y" (DeviceTestSharedHelpers.GetExcludedTestCategories);
    # every other filter value returns no exclusions, so a bare class token
    # runs the whole suite. Reviewers measured exactly that on device --
    # "538 of 538 declarations WOULD RUN" -- so a device reproduction publishes
    # the selector the runner actually honours instead of one that looks exact.
    $deviceSelectorLine = if ([string]$Candidate.testType -ceq 'device') {
        '- Device runner selector: ``TestFilter=Category=Issue{0}`` — the stock device-test runner filters by category only, so use this form on device; the class token above selects nothing there' -f $issueNumber
    } else {
        ''
    }

    # Only one of these ever applies, and an empty placeholder left mid-list
    # would split the surrounding Markdown bullets into two loose lists.
    $selectorLines = (@($uiSelectorLine, $deviceSelectorLine) |
        Where-Object { $_ }) -join [Environment]::NewLine

    # Reviewers rejected evidence that called a simulator or emulator run
    # "on-device". Name the surface that actually ran the reproduction.
    $recordingSurface = switch ([string]$Candidate.platform) {
        'android' { 'Android emulator' }
        'ios' { 'iOS Simulator' }
        'windows' { 'Windows host' }
        'catalyst' { 'Mac Catalyst host' }
        default { "$platform host" }
    }
    $recordedDevice = if ($Evidence.PSObject.Properties['device']) {
        ConvertTo-ReplicationSingleLine -Value ([string]$Evidence.device) -MaximumLength 80
    } else {
        ''
    }
    if ($recordedDevice) {
        $recordingSurface = "$recordingSurface ``$recordedDevice``"
    }

    # Reviewers repeatedly read the platform above as a claim that the committed
    # test ran on that surface. Unit and XAML tests execute on the build host, so
    # the recording is evidence of the issue rather than of the test.
    $testHostDescription = switch ([string]$Candidate.testType) {
        'UnitTest' { "the **build host**, not the $recordingSurface. The recording below is evidence of the reported issue, not of this test executing." }
        'XamlUnitTest' { "the **build host**, not the $recordingSurface. The recording below is evidence of the reported issue, not of this test executing." }
        default { "the **$recordingSurface** used for the run above." }
    }
    # Publish what the test actually reported rather than what the agent
    # predicted; the validator already proved the two describe one defect.
    $rawFailureSignature = Get-ReplicationCandidateText `
        -Candidate $Candidate `
        -Name 'observedFailureSignature'
    if ([string]::IsNullOrWhiteSpace($rawFailureSignature)) {
        $rawFailureSignature = [string]$Candidate.expectedFailureSignature
    }
    $actualFailureMessage = [string]$Candidate.actualFailureMessage
    $normalizedActualMessage = ([regex]::Replace($actualFailureMessage, '\s+', ' ')).Trim()
    $normalizedSignature = ([regex]::Replace($rawFailureSignature, '\s+', ' ')).Trim()
    if ([string]::IsNullOrWhiteSpace($normalizedSignature) -or
        [string]::IsNullOrWhiteSpace($normalizedActualMessage) -or
        -not $normalizedActualMessage.Contains(
            $normalizedSignature,
            [StringComparison]::Ordinal)) {
        throw 'Validated candidate targeted failure message does not contain the expected failure signature.'
    }
    $verificationRunCount = 0
    $runCountProperty = $Candidate.PSObject.Properties['verificationRunCount']
    if ($runCountProperty) {
        $verificationRunCount = [int]$runCountProperty.Value
    }
    if ($verificationRunCount -lt 2) {
        throw 'Validated candidate does not prove the test failed in repeated independent runs.'
    }
    # Reviewers read a single sentence covering both the recording and the test
    # as a claim that the committed test ran on the recorded surface. A test in
    # a non-platform target framework would behave identically on a machine with
    # no platform SDK installed, so say which surface established which fact.
    $platformNeutralTestTypes = @('UnitTest', 'XamlUnitTest')
    $reproductionClaim = if ($platformNeutralTestTypes -contains [string]$Candidate.testType) {
        "- A trusted runner reproduced the behavior on the $recordingSurface. The committed test is platform-neutral: it ran on the build host and failed in $verificationRunCount consecutive executions, so it corroborates the same defect in cross-platform code rather than proving the $recordingSurface behavior itself."
    } else {
        "- A trusted runner reproduced the behavior on the $recordingSurface and matched the expected targeted test failure in $verificationRunCount consecutive executions."
    }
    $determinismLine = "- Determinism: the exact test above was executed **$verificationRunCount " +
        'independent times** on this baseline and failed at the same assertion every time'
    $failureSignature = ConvertTo-ReplicationInlineCode -Value $rawFailureSignature
    $baseSha = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.baseSha
    )
    $marker = Get-ReplicationPullRequestMarker -IssueNumber $issueNumber -Platform $platform
    $safeTitle = ConvertTo-ReplicationSingleLine -Value $IssueTitle -MaximumLength 180

    # Reviewers had to work out for themselves whether a red test had been shown
    # to depend on the reported trigger, so state it rather than leaving it to be
    # inferred from the run count.
    $certificationLevel = Get-ReplicationCandidateText -Candidate $Candidate -Name 'certificationLevel'
    $certificationSummary = Get-ReplicationCandidateText -Candidate $Candidate -Name 'certificationSummary'

    # Three of the four most recent adversarial reviews refuted a fix not on its
    # causality, which they each confirmed by hand, but on issue fidelity: the
    # reporter's linked sample drives the bug down a different path than the one
    # the test models. That evidence is unreachable from here by design - the
    # only network read is the GitHub issue API, and the sanitizer replaces every
    # URL in the body with [url removed] before the authoring agent sees it. A
    # detector was measured against two known cases and could not separate them
    # for exactly this reason, so the honest remedy is to declare the boundary
    # rather than to gate on evidence that was never fetched.
    $evidenceBoundary = 'This rests on the issue report as returned by the GitHub issues API. The ' +
    'reporter''s linked reproduction project is never downloaded, and links are stripped from the ' +
    'report before the test is authored, so a test faithful to the written report may still drive ' +
    'the defect down a different path than the linked sample. Please check that first.'

    # Joined once and used by both branches. The forgotten sibling is the
    # documented trap here: every defect of that shape in this pipeline was a
    # correct change applied to exactly one of two places that needed it, and
    # this block has two branches that both report an evidence level.
    $evidenceCaveats = if ($UpstreamSignal) {
        $evidenceBoundary + [Environment]::NewLine + [Environment]::NewLine + $UpstreamSignal
    } else {
        $evidenceBoundary
    }

    $certificationBlock = if ($certificationSummary) {
        "## Evidence level" + [Environment]::NewLine + [Environment]::NewLine + $certificationSummary +
        [Environment]::NewLine + [Environment]::NewLine + $evidenceCaveats
    } elseif ($certificationLevel) {
        "## Evidence level" + [Environment]::NewLine + [Environment]::NewLine +
        ('**Evidence level: `' + $certificationLevel + '`**') +
        [Environment]::NewLine + [Environment]::NewLine + $evidenceCaveats
    } else {
        ''
    }

    $steps = @()
    foreach ($step in @($Candidate.reproductionSteps)) {
        $safeStep = ConvertTo-ReplicationSingleLine -Value ([string]$step) -MaximumLength 300
        if ($safeStep) {
            $steps += "1. $safeStep"
        }
    }
    if ($steps.Count -eq 0) {
        $steps = @('1. Run the issue-specific scenario described in the linked issue.')
    }

    $buildLine = if ($BuildUrl) { "- Pipeline run: $BuildUrl" } else { '- Pipeline run: unavailable' }
    $issueUrl = "https://github.com/$IssueOwner/$IssueRepository/issues/$issueNumber"

    $fixFiles = @(Get-ValidatedFixFiles -Candidate $Candidate)
    $fixBlock = if ($fixFiles.Count -gt 0) {
        $fixRootCause = Get-ReplicationCandidateText -Candidate $Candidate -Name 'fixRootCause'
        $fixRegressionLane = Get-ReplicationCandidateText -Candidate $Candidate -Name 'fixRegressionLane'
        $fixApproach = Get-ReplicationCandidateText -Candidate $Candidate -Name 'fixApproach'
        $rejected = @()
        $rejectedProperty = $Candidate.PSObject.Properties['fixRejectedApproaches']
        if ($rejectedProperty -and $null -ne $rejectedProperty.Value) {
            foreach ($entry in @($rejectedProperty.Value)) {
                $safeEntry = ConvertTo-ReplicationSingleLine -Value ([string]$entry) -MaximumLength 300
                if ($safeEntry) {
                    $rejected += "- $safeEntry"
                }
            }
        }

        $fixLines = @(
            '## Proposed fix',
            '',
            ('This pull request carries two commits. The first adds the failing reproduction on its own, ' +
             'so its parent can be checked out and the test watched to fail. The second changes product ' +
             'code so the same test passes.'),
            ''
        )
        if ($fixRootCause) {
            $fixLines += ('**Root cause.** ' + (ConvertTo-ReplicationSingleLine -Value $fixRootCause -MaximumLength 600))
            $fixLines += ''
        }
        if ($fixRegressionLane) {
            $fixLines += ('**Regression lane.** The device tests beside this one declare `' +
                (ConvertTo-ReplicationSingleLine -Value $fixRegressionLane -MaximumLength 120) +
                '`. This test declares only its issue category, because a second category makes ' +
                'the advertised `Category=Issue<N>` selector match nothing on this runner.')
        } else {
            $fixLines += ('**Regression lane.** Not measured: the tests beside this one do not ' +
                'agree on a single conventional category.')
        }
        $fixLines += ''
        if ($fixApproach) {
            $fixLines += ('**Approach taken.** ' + (ConvertTo-ReplicationSingleLine -Value $fixApproach -MaximumLength 600))
            $fixLines += ''
        }
        if ($RegressionSignal) {
            $fixLines += $RegressionSignal
            $fixLines += ''
        }
        if ($OracleSignal) {
            $fixLines += $OracleSignal
            $fixLines += ''
        }
        $fixLines += (Get-ReplicationFixPanelBlock -Candidate $Candidate)
        $fixLines += ''
        $fixLines += (Get-ReplicationIndependentReviewBlock -Candidate $Candidate)
        $fixLines += ''
        $fixLines += '**Files changed by the fix commit:**'
        $fixLines += ''
        foreach ($file in ($fixFiles | Sort-Object -Unique)) {
            $fixLines += ('- `' + (ConvertTo-ReplicationSingleLine -Value $file -MaximumLength 240) + '`')
        }
        if ($rejected.Count -gt 0) {
            $fixLines += ''
            $fixLines += '**Approaches considered and rejected:**'
            $fixLines += ''
            $fixLines += $rejected
        }
        $fixLines -join [Environment]::NewLine
    } else {
        ''
    }

    $importantBanner = if ($fixFiles.Count -gt 0) {
        ('> This is AI-generated **reproduction evidence with a proposed fix**. The first commit adds a test ' +
         'that fails on the unfixed baseline; the second changes product code so it passes. Both the test and ' +
         'the fix need human review before merge.')
    } else {
        ('> This is AI-generated **reproduction evidence**, not a merge-ready product fix. The added test ' +
         'intentionally fails on the unfixed baseline. A product fix should make the test pass before this PR ' +
         'is considered for merge.')
    }
    $patchScopeLine = if ($fixFiles.Count -gt 0) {
        ('- The reproduction commit is add-only and restricted to approved MAUI test locations. The fix commit ' +
         'modifies only the product files listed above, and adds, deletes, renames, and mode changes are refused.')
    } else {
        '- The published patch is add-only and restricted to approved MAUI test locations.'
    }

    return @"
$marker

> [!IMPORTANT]
$importantBanner

$certificationBlock

## Reproduced issue

- Issue: [$IssueOwner/$IssueRepository#$issueNumber — $safeTitle]($issueUrl)
- Platform: **$platform**
- Validated on baseline commit: ``$baseSha`` — the trusted device reproduction and the failing-test verification both ran against this commit
- Base branch: the reproduction commit sits directly on the baseline above, so the first parent of the commit in this pull request is exactly the commit the device reproduction and the failing-test verification ran against, and this diff contains only the added reproduction test.
- Test type: **$testType**
- Test execution host: $testHostDescription
- Exact test: ``$exactTestName``
- Targeted filter: ``$testFilter`` — an issue-keyed class token; use the exact test above when a runner needs a precise selector
$selectorLines
- Expected failing assertion: ``$failureSignature``
$determinismLine
$buildLine

## Recorded evidence ($recordingSurface)

[![Reproduction preview]($($Evidence.blobs.preview))]($($Evidence.blobs.video))

[Open the full MP4 recording]($($Evidence.blobs.video)) · [Evidence manifest]($($Evidence.blobs.manifest))

The authoritative proof is the trusted targeted test failing with the expected assertion above. The recording corroborates that; for defects with no visible symptom it may show only the app-reported verdict rather than the defect itself.

This recording is of the trusted Sandbox reproduction app that established the behavior on the $recordingSurface, not of the committed test executing. Its on-screen text therefore comes from that Sandbox app and will not match the assertion payload emitted by the committed test. Treat it as corroboration of the symptom, not as exact-head evidence for the commit in this pull request.

## Reproduction steps

$($steps -join [Environment]::NewLine)

## Safety and validation

- The pipeline reconstructed the scenario from issue text, inline snippets, and allowed raster screenshots.
- No linked repository, archive, binary, script, package, or arbitrary external file was downloaded.
$reproductionClaim
$patchScopeLine

$fixBlock
"@
}

function Get-ValidatedCandidateFiles {
    param([Parameter(Mandatory = $true)]$Candidate)

    $property = $Candidate.PSObject.Properties['files']
    if (-not $property) {
        $property = $Candidate.PSObject.Properties['addedFiles']
    }
    if (-not $property) {
        throw 'Validated candidate does not contain a files or addedFiles property.'
    }

    $files = @($property.Value | ForEach-Object { ([string]$_).Replace('\', '/') })
    if ($files.Count -eq 0) {
        throw 'Validated candidate does not list any added files.'
    }
    return $files
}

function Get-ValidatedFixFiles {
    param([Parameter(Mandatory = $true)]$Candidate)

    $property = $Candidate.PSObject.Properties['fixFiles']
    if (-not $property -or $null -eq $property.Value) {
        return @()
    }
    return @($property.Value | ForEach-Object { ([string]$_).Replace('\', '/') } | Where-Object { $_ })
}

function Remove-ReplicationPlatformTitlePrefix {
    <#
    .SYNOPSIS
        Drops a leading bracket from an issue title when it holds nothing but
        platform names.

    .DESCRIPTION
        Reporters routinely open a title with every platform they saw, as
        dotnet/maui#35624 does with "[Android, iOS and Catalyst]". A run validates
        exactly one, and the pull request already names that one in its own tag, so
        quoting the reporter's list after it restates a claim the evidence does not
        support in the one field every reader sees before opening anything. Two
        independent human reviewers rejected fix pull requests on exactly that
        ground - 509 ("claims Android and Catalyst coverage that this
        implementation/test pair does not establish") and 458 ("the PR title claims
        [Android, iOS and Catalyst], but the product diff changes only the iOS
        tracker").

        Measured before it was written, over the 68 open fix pull requests: 22
        carry a leading pure-platform bracket and 8 of those name a platform the
        run did not validate, including both PRs a reviewer objected to.

        The rule is deliberately narrow, because a leading bracket usually is not a
        platform list and removing it really would misdescribe the issue. A bracket
        is dropped only when every token in it, split on the separators reporters
        actually use, is a bare platform name. Measured against 361 real
        dotnet/maui issue titles that open with a bracket, this drops 37 and keeps
        324, with no false positives: "[iOS 26.5]", "[Android 16]",
        "[REGRESSION: iOS, 10.0.100]", "[.NET 10]" and "[leak-scan]" are all kept,
        because each carries something the platform tag does not.

        Only the first bracket is considered, and only when it opens the title.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [AllowEmptyString()]
        [string]$Title
    )

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $Title
    }

    $match = [regex]::Match($Title, '^\s*\[([^\]]+)\]\s*')
    if (-not $match.Success) {
        return $Title
    }

    # The separators reporters use between platform names. A token that is not a
    # bare platform name - a version, a release, a scan label - keeps the bracket.
    $tokens = @($match.Groups[1].Value -split '(?:,|/|&|\+|\band\b)' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($tokens.Count -eq 0) {
        return $Title
    }

    foreach ($token in $tokens) {
        if ($token -notmatch '^(?i:android|ios|windows|catalyst|maccatalyst|mac|winui|uwp|tizen)$') {
            return $Title
        }
    }

    return $Title.Substring($match.Length)
}

function New-ReplicationPullRequestTitle {
    <#
    .SYNOPSIS
        Builds the pull request title.

    .DESCRIPTION
        A PR that carries a product fix is titled
        `[maui-bot-fix][<platform>] Fix for #N - <issue title>` so the bot's fixes
        are filterable at a glance and name the platform the evidence covers.

        A PR that carries only a reproduction keeps the platform-tagged
        reproduction title. Claiming a fix that is not in the diff would overstate
        the evidence in the one field every reader sees before opening anything,
        which is exactly the failure mode the certification levels exist to avoid.

        The platform tag exists for the same reason. Issue titles routinely name
        every platform a reporter saw, as dotnet/maui#35667 does with
        "[Android, iOS, Catalyst]", while a run validates exactly one. Quoting that
        title after "Fix for" reads as a claim to have fixed all of them, and a
        human reviewer of PR 509 rejected it on precisely that ground: "the current
        title also claims Android and Catalyst coverage that this
        implementation/test pair does not establish". The body has always been
        accurate - it states the validated platform and the four control arms - but
        the body is not what a reader sees in a PR list. Naming the validated
        platform next to the inherited tag makes the narrower claim the visible
        one.

        Tagging alone did not settle it. A reviewer of PR 458 objected again with
        the tag in place, because the reporter's own list was still quoted after
        it. So a leading bracket holding nothing but platform names is now dropped
        by Remove-ReplicationPlatformTitlePrefix, whose narrowness is measured
        there. The rest of the reporter's title is still quoted verbatim, because
        rewriting that would misdescribe the issue being fixed.

        The issue title is treated as untrusted: control characters are stripped so
        it cannot forge additional lines, and the whole title is bounded so it
        stays legible in a PR list.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [int]$IssueNumber,

        [Parameter(Mandatory = $true)]
        [string]$Platform,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [AllowEmptyString()]
        [string]$IssueTitle,

        [Parameter(Mandatory = $false)]
        [switch]$CarriesFix,

        [Parameter(Mandatory = $false)]
        [ValidateRange(40, 256)]
        [int]$MaxLength = 120
    )

    if (-not $CarriesFix) {
        return "[$Platform] Add failing reproduction for #$IssueNumber"
    }

    # Kept first so the [maui-bot-fix] filter every reader already uses still
    # matches, with the validated platform immediately after it.
    $prefix = "[maui-bot-fix][$Platform] Fix for #$IssueNumber"

    $summary = if ($null -eq $IssueTitle) { '' } else { $IssueTitle }
    $summary = ($summary -replace '[\p{C}]', ' ').Trim()
    $summary = $summary -replace '\s{2,}', ' '
    # The tag above already names the one validated platform, so a reporter's
    # leading platform list would restate it less accurately. Runs before the
    # whitespace check, because dropping the bracket can empty the summary.
    $summary = (Remove-ReplicationPlatformTitlePrefix -Title $summary).Trim()
    if ([string]::IsNullOrWhiteSpace($summary)) {
        return $prefix
    }

    $separator = ' - '
    $available = $MaxLength - $prefix.Length - $separator.Length
    if ($available -lt 12) {
        # No room for a summary that would still mean anything.
        return $prefix
    }

    if ($summary.Length -gt $available) {
        $summary = $summary.Substring(0, $available - 1).TrimEnd() + '…'
    }

    return "$prefix$separator$summary"
}

function Assert-ReplicationStagedFix {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowNull()][string[]]$StagedLines,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowNull()][string[]]$ExpectedFiles
    )

    $actual = @()
    foreach ($line in @($StagedLines)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        if ($line -notmatch '^M\s+(.+)$') {
            throw "The staged fix is not modification-only: $line"
        }
        $actual += $Matches[1].Trim().Replace('\', '/')
    }

    $expectedSorted = @(@($ExpectedFiles) | Where-Object { $_ } | Sort-Object -Unique)
    $actualSorted = @($actual | Sort-Object -Unique)
    if (($expectedSorted -join "`n") -ne ($actualSorted -join "`n")) {
        throw 'The staged fix files do not exactly match the validated candidate manifest.'
    }

    return $actualSorted
}

function Invoke-ReplicationExternalCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

function Resolve-ReplicationSourceRepository {
    param(
        [Parameter(Mandatory = $true)][string]$ParentOwner,
        [Parameter(Mandatory = $true)][string]$ParentRepository
    )

    $query = @'
query {
  viewer {
    login
    repositories(
      first: 100
      affiliations: [OWNER, ORGANIZATION_MEMBER]
    ) {
      nodes {
        nameWithOwner
        isFork
        viewerPermission
        parent {
          nameWithOwner
        }
      }
    }
  }
}
'@
    $responseJson = & gh api graphql -f "query=$query"
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to inspect repositories available to the publication token.'
    }
    $response = $responseJson | ConvertFrom-Json -Depth 20
    $expectedParent = "$ParentOwner/$ParentRepository"
    $matches = @($response.data.viewer.repositories.nodes) |
        Where-Object {
            $_.isFork -eq $true -and
            [string]$_.parent.nameWithOwner -eq $expectedParent -and
            [string]$_.viewerPermission -in @('WRITE', 'MAINTAIN', 'ADMIN')
        }
    if ($matches.Count -eq 0) {
        $createdForkJson = & gh api `
            -X POST `
            "repos/$ParentOwner/$ParentRepository/forks"
        if ($LASTEXITCODE -ne 0) {
            throw "MauiBot has no writable fork of $expectedParent and creating one failed."
        }
        $createdFork = $createdForkJson | ConvertFrom-Json -Depth 20
        $createdFullName = [string]$createdFork.full_name
        if ($createdFullName -notmatch '^[A-Za-z0-9-]+/[A-Za-z0-9._-]+$') {
            throw 'GitHub returned an invalid name for the newly created MauiBot fork.'
        }

        for ($attempt = 1; $attempt -le 12; $attempt++) {
            Start-Sleep -Seconds 5
            $forkJson = & gh api "repos/$createdFullName" 2>$null
            if ($LASTEXITCODE -eq 0) {
                $fork = $forkJson | ConvertFrom-Json -Depth 20
                if ($fork.fork -eq $true -and
                    [string]$fork.parent.full_name -eq $expectedParent -and
                    $fork.permissions.push -eq $true) {
                    return [pscustomobject]@{
                        Owner = ($createdFullName -split '/', 2)[0]
                        Repository = ($createdFullName -split '/', 2)[1]
                    }
                }
            }
        }
        throw 'The newly created MauiBot fork did not become writable within 60 seconds.'
    }
    if ($matches.Count -ne 1) {
        throw "Expected exactly one writable fork of $expectedParent; found $($matches.Count)."
    }

    $parts = ([string]$matches[0].nameWithOwner) -split '/', 2
    if ($parts.Count -ne 2 -or
        $parts[0] -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$' -or
        $parts[1] -notmatch '^[A-Za-z0-9._-]+$') {
        throw 'Resolved reproduction fork has an invalid repository name.'
    }
    return [pscustomobject]@{
        Owner = $parts[0]
        Repository = $parts[1]
    }
}

$candidate = Get-Content -LiteralPath $ValidatedCandidatePath -Raw | ConvertFrom-Json -Depth 50
if ($candidate.validationPassed -ne $true) {
    throw 'Candidate validation did not pass; a pull request will not be created.'
}

$evidence = Get-Content -LiteralPath $PublishedEvidencePath -Raw | ConvertFrom-Json -Depth 20
$context = Get-Content -LiteralPath $IssueContextPath -Raw | ConvertFrom-Json -Depth 20
$issueNumber = [int]$candidate.issueNumber
$platform = ConvertTo-ReplicationSingleLine -Value ([string]$candidate.platform) -MaximumLength 40
if ([int]$evidence.issueNumber -ne $issueNumber -or [string]$evidence.platform -ne [string]$candidate.platform) {
    throw 'Published evidence does not match the validated issue and platform.'
}

$issueTitle = if ($context.PSObject.Properties['title']) { [string]$context.title } else { "Issue #$issueNumber" }
$buildId = if ($env:BUILD_BUILDID) {
    if ($env:SYSTEM_JOBATTEMPT -match '^[1-9]\d*$') {
        "$($env:BUILD_BUILDID)-$($env:SYSTEM_JOBATTEMPT)"
    } else {
        $env:BUILD_BUILDID
    }
} else {
    [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
}
$branchName = New-ReplicationBranchName -IssueNumber $issueNumber -Platform $platform -BuildId $buildId
$marker = Get-ReplicationPullRequestMarker -IssueNumber $issueNumber -Platform $platform
$prTitle = New-ReplicationPullRequestTitle `
    -IssueNumber $issueNumber `
    -Platform $platform `
    -IssueTitle $issueTitle `
    -CarriesFix:(@(Get-ValidatedFixFiles -Candidate $candidate).Count -gt 0)
# Computed here rather than inside the body builder, which stays free of child
# processes and network so it can be tested directly. A null signal simply omits
# the line: the check reports, it never withholds a fix.
$regressionSignal = Get-ReplicationFixRegressionSignal -FixPatchPath $FixPatchPath
if ($regressionSignal) { Write-Host "Regression cross-reference: $regressionSignal" }
# Both patches are already on disk and validated by this point, so the oracle
# comparison needs no child process and no network. Read defensively: a missing
# patch means there is nothing to compare, never a refusal.
$oracleSignal = ''
if ($FixPatchPath -and (Test-Path -LiteralPath $FixPatchPath) -and
    $PatchPath -and (Test-Path -LiteralPath $PatchPath)) {
    $oracleSignal = Get-ReplicationOracleIndependenceSignal `
        -TestSource (Get-Content -LiteralPath $PatchPath -Raw) `
        -FixSource (Get-Content -LiteralPath $FixPatchPath -Raw)
}
if ($oracleSignal) { Write-Host "Oracle independence: $oracleSignal" }

# The project merges bug fixes to a branch this reproduction is not built
# against, where they are invisible to every other signal: the issue stays
# open and the defect still reproduces. Measured at 8 of 70 open fix pull
# requests, so this reports a real population rather than a hypothetical one.
$upstreamSignal = Get-ReplicationUpstreamFixSignal `
    -IssueNumber $issueNumber `
    -BaseSha (Get-ReplicationCandidateText -Candidate $candidate -Name 'baseSha') `
    -Repo "$IssueOwner/$IssueRepository"
if ($upstreamSignal) { Write-Host "Upstream cross-reference: $upstreamSignal" }

$prBody = New-ReplicationPullRequestBody `
    -RegressionSignal $regressionSignal `
    -OracleSignal $oracleSignal `
    -UpstreamSignal $upstreamSignal `
    -Candidate $candidate `
    -Evidence $evidence `
    -IssueTitle $issueTitle `
    -IssueOwner $IssueOwner `
    -IssueRepository $IssueRepository `
    -BuildUrl $BuildUrl

function Write-ReplicationPublicationManifest {
    param([Parameter(Mandatory)]$Plan)

    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $script:OutputPath = Join-Path (Split-Path -Parent $PublishedEvidencePath) 'published-pr.json'
    }
    $directory = Split-Path -Parent $OutputPath
    if ($directory) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $Plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
    Write-Host "Replication pull request publication manifest: $OutputPath"
}

$plan = [ordered]@{
    issueNumber = $issueNumber
    platform = $platform
    branch = $branchName
    title = $prTitle
    body = $prBody
    marker = $marker
    files = @(Get-ValidatedCandidateFiles -Candidate $candidate)
    fixFiles = @(Get-ValidatedFixFiles -Candidate $candidate)
    url = $null
    duplicateOf = $null
    supersedes = $null
    supersededClosed = $false
}

if (-not $DryRun) {
    if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
        throw 'GH_TOKEN is required to publish the reproduction pull request.'
    }

    $authenticatedLogin = Get-ReplicationGitHubLogin
    if (-not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
        throw "GH_TOKEN must authenticate as 'MauiBot'."
    }
    $source = Resolve-ReplicationSourceRepository `
        -ParentOwner $IssueOwner `
        -ParentRepository $IssueRepository
    $sourceOwner = [string]$source.Owner
    $sourceRepository = [string]$source.Repository

    Push-Location $RepositoryRoot
    try {
        $status = & git status --porcelain
        if ($LASTEXITCODE -ne 0 -or $status) {
            throw 'The trusted publishing checkout must be clean before applying the reproduction patch.'
        }

        $supersededPull = $null
        $openPullsJson = & gh pr list `
            --repo "$TargetOwner/$TargetRepository" `
            --state open `
            --limit 200 `
            --json 'number,body,url'
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to query existing reproduction pull requests.'
        }

        $duplicate = @($openPullsJson | ConvertFrom-Json) | Where-Object {
            [string]$_.body -like "*$marker*"
        } | Select-Object -First 1

        # A reproduction is not a replacement for a fix. Builds 15075238 and
        # 15076851 reproduced issues 36412 and 35624, found no fix, and retired
        # pull requests 393 and 396 - which each carried a four-arm certified
        # fix - replacing them with reproduction-only pull requests 398 and 403.
        # Superseding exists so a re-run can refresh evidence, not so a weaker
        # publication can destroy a stronger one, so an incoming run without a
        # fix stands aside for an open pull request that has one.
        $incomingCarriesFix = -not [string]::IsNullOrWhiteSpace($FixPatchPath) -and
            (Test-Path -LiteralPath $FixPatchPath)
        $standsAside = $duplicate -and (Test-ReplicationWeakerThanOpenPullRequest `
            -DuplicateBody ([string]$duplicate.body) -IncomingCarriesFix $incomingCarriesFix)
        if ($standsAside) {
            $plan.duplicateOf = [string]$duplicate.url
            Write-Host ("An open pull request already carries a fix for this issue and platform, " +
                "so this reproduction-only run stands aside: $($duplicate.url)")
            Write-ReplicationPublicationManifest -Plan $plan
            Pop-Location
            exit 0
        }

        if ($duplicate -and -not $SupersedeExisting) {
            # Build 15001510 reproduced issue 37151 and authored its test while
            # an earlier run was publishing the same issue and platform. The
            # second run is redundant, not broken, so it reports what already
            # covers the issue instead of failing the build.
            $plan.duplicateOf = [string]$duplicate.url
            Write-Host ("An open reproduction pull request already covers this issue and platform: " +
                "$($duplicate.url)")
            Write-ReplicationPublicationManifest -Plan $plan
            Pop-Location
            exit 0
        }

        if ($duplicate) {
            # Superseding is what lets an already-covered issue be re-run after
            # the pipeline itself changes. The earlier pull request is retired
            # only once its replacement exists, further down: a run that never
            # gets that far leaves the existing evidence exactly where it was.
            $supersededPull = $duplicate
            $plan.supersedes = [string]$duplicate.url
            Write-Host ("This run supersedes an open reproduction pull request: " +
                "$($duplicate.url)")
        }

        # Reviewers verify the reproduction against the pull request's first
        # parent. Opening against a moving branch made that parent a different
        # commit from the one the device run and the failing test were verified
        # on, and three independent reviews reported it as a provenance defect.
        # Commit onto the verified baseline itself so the first parent is
        # exactly the commit the evidence describes. That keeps the diff equal
        # to the add-only patch only while the baseline is an ancestor of the
        # base branch, so prove that rather than assume it.
        $baselineSha = [string]$Candidate.baseSha
        if ($baselineSha -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Validated candidate baseline commit is not a full lowercase SHA.'
        }
        & git cat-file -e "$baselineSha^{commit}" 2>$null
        if ($LASTEXITCODE -ne 0) {
            throw 'The validated baseline commit is missing from the publisher checkout.'
        }

        $targetRemote = 'replication-target'
        & git remote remove $targetRemote 2>$null
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('remote', 'add', $targetRemote, "https://github.com/$TargetOwner/$TargetRepository.git") `
            -Description 'Configuring reproduction target'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('fetch', '--no-tags', $targetRemote, $BaseBranch) `
            -Description 'Fetching the pull request base branch'
        & git merge-base --is-ancestor $baselineSha FETCH_HEAD
        if ($LASTEXITCODE -ne 0) {
            throw ('The validated baseline is not contained in the pull request ' +
                'base branch, so the reproduction diff would carry unrelated commits.')
        }

        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('checkout', '--detach', $baselineSha) -Description 'Checking out the verified baseline'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('switch', '-c', $branchName) -Description 'Creating reproduction branch'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('apply', '--index', '--whitespace=nowarn', $PatchPath) -Description 'Applying validated reproduction patch'

        $staged = @(& git diff --cached --name-status --diff-filter=ACDMRTUXB)
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to inspect the staged reproduction patch.'
        }
        $actualFiles = @()
        foreach ($line in $staged) {
            if ($line -notmatch '^A\s+(.+)$') {
                throw "The staged patch is not add-only: $line"
            }
            $actualFiles += $Matches[1].Replace('\', '/')
        }

        $expectedFiles = @($plan.files | Sort-Object -Unique)
        $actualFiles = @($actualFiles | Sort-Object -Unique)
        if (($expectedFiles -join "`n") -ne ($actualFiles -join "`n")) {
            throw 'The staged files do not exactly match the validated candidate manifest.'
        }

        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('config', 'user.name', 'maui-copilot-replication') -Description 'Configuring git author'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('config', 'user.email', '223556219+Copilot@users.noreply.github.com') -Description 'Configuring git email'
        Invoke-ReplicationExternalCommand -FilePath 'gh' -Arguments @('auth', 'setup-git') -Description 'Configuring bot Git authentication'

        $sourceRemote = 'replication-fork'
        & git remote remove $sourceRemote 2>$null
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('remote', 'add', $sourceRemote, "https://github.com/$sourceOwner/$sourceRepository.git") `
            -Description 'Configuring reproduction fork'

        $commitMessage = @"
Add failing reproduction for #$issueNumber on $platform

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
Copilot-Session: 735ac9a2-7bec-4baa-ad19-c298e5bc795a
"@
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('commit', '-m', $commitMessage) -Description 'Committing reproduction test'

        # The fix lands as a second commit so the red-to-green transition is
        # legible: the first commit is the failing reproduction on its own, and
        # anyone can check out its parent and watch the test fail.
        if (@($plan.fixFiles).Count -gt 0) {
            if ([string]::IsNullOrWhiteSpace($FixPatchPath)) {
                throw 'The validated candidate names fix files but no fix patch was supplied.'
            }
            Invoke-ReplicationExternalCommand `
                -FilePath 'git' `
                -Arguments @('apply', '--index', '--whitespace=nowarn', $FixPatchPath) `
                -Description 'Applying validated product fix'

            $stagedFix = @(& git diff --cached --name-status --diff-filter=ACDMRTUXB)
            if ($LASTEXITCODE -ne 0) {
                throw 'Unable to inspect the staged product fix.'
            }

            Assert-ReplicationStagedFix -StagedLines $stagedFix -ExpectedFiles @($plan.fixFiles) | Out-Null

            $fixCommitMessage = @"
Fix #$issueNumber so the reproduction passes

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
Copilot-Session: 735ac9a2-7bec-4baa-ad19-c298e5bc795a
"@
            Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('commit', '-m', $fixCommitMessage) -Description 'Committing product fix'
        } elseif (-not [string]::IsNullOrWhiteSpace($FixPatchPath)) {
            throw 'A fix patch was supplied but the validated candidate names no fix files.'
        }

        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('push', $sourceRemote, "HEAD:refs/heads/$branchName") `
            -Description 'Pushing reproduction branch'

        $bodyPath = Join-Path ([IO.Path]::GetTempPath()) "maui-replication-pr-$issueNumber-$buildId.md"
        try {
            $prBody | Set-Content -LiteralPath $bodyPath -Encoding utf8NoBOM
            $prUrl = & gh pr create `
                --repo "$TargetOwner/$TargetRepository" `
                --head "$sourceOwner`:$branchName" `
                --base $BaseBranch `
                --title $prTitle `
                --body-file $bodyPath `
                --draft
            if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace([string]$prUrl)) {
                throw 'Creating the draft reproduction pull request failed.'
            }
            $plan.url = ([string]$prUrl).Trim()

            if ($supersededPull) {
                # Only now, with the replacement open, is retiring the earlier
                # pull request safe. Closing it first would leave the issue with
                # no open reproduction at all if anything above had failed, and
                # a duplicate is a far smaller problem than lost evidence.
                try {
                    $supersededNumber = [string]$supersededPull.number
                    $supersedeNote = "Superseded by $($plan.url), published by build $buildId."
                    & gh pr comment $supersededNumber --repo "$TargetOwner/$TargetRepository" --body $supersedeNote | Out-Null
                    & gh pr close $supersededNumber --repo "$TargetOwner/$TargetRepository" | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        $plan.supersededClosed = $true
                    }
                } catch {
                    Write-Host ("The superseded pull request could not be retired, so it stays open: " +
                        "$($_.Exception.Message)")
                }
            }
        }
        finally {
            Remove-Item -LiteralPath $bodyPath -Force -ErrorAction SilentlyContinue
        }
    }
    finally {
        Pop-Location
    }
}

Write-ReplicationPublicationManifest -Plan $plan
