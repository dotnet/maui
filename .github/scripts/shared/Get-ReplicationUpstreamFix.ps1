#!/usr/bin/env pwsh
<#
    .SYNOPSIS
        Answers whether dotnet/maui has already fixed an issue on a branch
        this pipeline does not build.

    .DESCRIPTION
        dotnet/maui merges bug fixes to `inflight/current` and leaves the
        issue open until release, so issue state carries no information: all
        three cases this was validated against are OPEN and carry a merged
        fix. The tree this pipeline builds tracks `main`, so an upstream fix
        living only on `inflight/current` is invisible to every other signal -
        the issue is open, the defect genuinely reproduces, and our red test
        is honest.

        Measured over the complete keying population rather than a sample:
        756 `Issue<N>.cs` test cases on `inflight/current` against 720 on
        `main`, a delta of 37, of which 37 of 37 are genuinely fixed upstream.
        Eight of the seventy open fix pull requests at the time (11%) targeted
        an issue already fixed there, and two human reviewers found the class
        independently before any probe did.

        This file is the single authority for that question because it has two
        consumers that ask it at different moments: the pre-flight scope gate
        refuses before a device is provisioned, and the publisher discloses in
        the body of a run that already spent one. A second inline copy in
        either place would drift, which is how a recorder timeout, an element
        text failure and a guard refusal each went unnamed here.
#>
function Get-ReplicationUpstreamTestCasePresence {
    <#
        .SYNOPSIS
            Answers whether a HostApp test case for an issue exists on a ref.

        .DESCRIPTION
            Three outcomes, deliberately distinct: 'present', 'absent', and
            'unknown'. A 404 is a real measurement and must not be confused
            with a call that failed, because `gh api` exits non-zero for both.
            A zero produced by an exception is not a zero.

            The single-file contents endpoint is used rather than a directory
            listing, which caps at 1000 entries and gives no truncation signal -
            post-filtering that listing reported the known positive as absent.
    #>
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Ref,
        [Parameter(Mandatory = $true)][string]$Repo
    )

    # A ref carrying '/' has to be encoded or the API reads it as more path.
    $encodedRef = [uri]::EscapeDataString($Ref)
    $output = ''
    try {
        $output = (& gh api "repos/$Repo/contents/$Path`?ref=$encodedRef" --jq '.sha' 2>&1 |
            Out-String)
    } catch {
        return 'unknown'
    }

    if ($output -match '^[0-9a-f]{40}') { return 'present' }
    if ($output -match 'HTTP 404' -or $output -match '"status":\s*"404"') {
        # A missing repository, a missing branch and a missing file are all 404,
        # so 'absent' has to be earned by proving the ref itself resolves. A
        # measurement that could not be taken must never render as a clean
        # "nothing found" - that is how an absent measurement becomes a finding.
        $refOutput = ''
        try {
            $refOutput = (& gh api "repos/$Repo/commits/$encodedRef" --jq '.sha' 2>&1 | Out-String)
        } catch {
            return 'unknown'
        }

        if ($refOutput -match '^[0-9a-f]{40}') { return 'absent' }
        return 'unknown'
    }

    return 'unknown'
}

function Get-ReplicationUpstreamDuplicateVerdict {
    <#
        .SYNOPSIS
            Answers, before a device is provisioned, whether reproducing this
            issue would duplicate work dotnet/maui has already merged.

        .DESCRIPTION
            Three outcomes, and the third is load-bearing: 'duplicate',
            'clear', and 'unknown'. A gate that cannot take its measurement
            must say so and let the run proceed, because an absent measurement
            rendered as a finding is the single most expensive shape this
            pipeline has recorded.

            Presence is asked of the upstream branch over the API. Absence is
            read from the working tree this run will actually build, which is
            exact, free, and cannot disagree with itself about which ref is
            checked out - the publisher asks the same question of a base
            commit because that is what it has. Both consumers share the
            presence primitive above; only the "is it already in my tree" half
            differs, because the honest answer to it differs.

            Unlike the publisher's disclosure, which reports and never
            refuses, this refuses - and the difference is what a refusal
            costs. Here nothing has been spent: no SDK, no simulator, no
            reproduction, no video. A refusal saves roughly forty minutes. The
            publisher refusing would destroy all of it, which is why it does
            not.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$IssueNumber,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$RepositoryRoot,
        [Parameter(Mandatory = $false)][string]$Repo = 'dotnet/maui',
        [Parameter(Mandatory = $false)][string]$UpstreamRef = 'inflight/current'
    )

    if ($IssueNumber -notmatch '^\d+$') {
        return [pscustomobject]@{
            Verdict = 'unknown'
            Path    = ''
            Reason  = "The issue number '$IssueNumber' is not a number, so no upstream test case could be looked up."
        }
    }

    if ([string]::IsNullOrWhiteSpace($RepositoryRoot) -or
        -not (Test-Path -LiteralPath $RepositoryRoot -PathType Container)) {
        # The platform scope gate beside this one read a path nothing ever
        # wrote and silently approved every run it was added to stop. A gate
        # that cannot find its evidence has to say so rather than quietly pass
        # - and, just as important, rather than quietly refuse.
        return [pscustomobject]@{
            Verdict = 'unknown'
            Path    = ''
            Reason  = "The repository root '$RepositoryRoot' is not a directory, so the tree under test could not be read."
        }
    }

    $candidatePaths = @(
        "src/Controls/tests/TestCases.HostApp/Issues/Issue$IssueNumber.cs",
        "src/Controls/tests/TestCases.HostApp/Issues/Issue$IssueNumber.xaml"
    )

    $sawUnknown = $false
    $unknownReason = ''
    foreach ($path in $candidatePaths) {
        $localPath = Join-Path $RepositoryRoot $path
        if (Test-Path -LiteralPath $localPath -PathType Leaf) {
            # Already in the tree under test, so upstream having it is not news.
            continue
        }

        $upstream = 'unknown'
        try {
            $upstream = Get-ReplicationUpstreamTestCasePresence `
                -Path $path -Ref $UpstreamRef -Repo $Repo
        } catch {
            # A pre-flight gate that throws costs the run it was written to
            # save. The probe shells out to gh, so a transport fault, an auth
            # failure or a torn connection all surface here - and none of them
            # is a measurement. The yml call site catches too, but a contract
            # kept only by its caller is not a contract.
            $upstream = 'unknown'
        }

        if ($upstream -eq 'unknown') {
            $sawUnknown = $true
            $unknownReason = "Could not read '$path' on ``$Repo``'s ``$UpstreamRef`` branch."
            continue
        }

        if ($upstream -eq 'present') {
            return [pscustomobject]@{
                Verdict = 'duplicate'
                Path    = $path
                Reason  = ("dotnet/maui already carries a test case for this issue on " +
                           "``$Repo``'s ``$UpstreamRef`` branch ($path), which the tree under " +
                           'test does not have, so the issue is already fixed there and a ' +
                           'reproduction or fix for it would duplicate merged work.')
            }
        }
    }

    if ($sawUnknown) {
        return [pscustomobject]@{
            Verdict = 'unknown'
            Path    = ''
            Reason  = $unknownReason
        }
    }

    return [pscustomobject]@{
        Verdict = 'clear'
        Path    = ''
        Reason  = "No upstream test case for this issue exists on ``$Repo``'s ``$UpstreamRef`` branch."
    }
}
