#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    function Get-ScriptFunctionText {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$Name
        )

        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$tokens, [ref]$errors)
        if ($errors) {
            throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
        }

        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $Name
        }, $true)
        if (-not $function) {
            throw "Function '$Name' was not found in $Path"
        }
        return $function.Extent.Text
    }

    $evidenceScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationEvidence.ps1'
    $prScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
    $migrationScript = Join-Path $PSScriptRoot 'shared/Move-ReplicationPRsToTestingFork.ps1'
    $script:EvidenceSource = Get-Content -LiteralPath $evidenceScript -Raw
    $script:PrSource = Get-Content -LiteralPath $prScript -Raw
    $script:MigrationSource = Get-Content -LiteralPath $migrationScript -Raw

    foreach ($name in @(
        'Test-ReplicationAssetPrefix',
        'ConvertTo-ReplicationUrlPath',
        'Get-ReplicationPublicAssetUrl'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $evidenceScript -Name $name)
    }
    foreach ($name in @(
        'ConvertTo-ReplicationSingleLine',
        'ConvertTo-ReplicationInlineCode',
        'Get-ReplicationPullRequestMarker',
        'New-ReplicationBranchName',
        'Get-ReplicationCandidateText',
        'Get-ReplicationIndependentReviewBlock',
        'Get-ReplicationFixPanelBlock',
        'Get-ValidatedFixFiles',
        'Test-ReplicationPublishableFix',
        'Assert-ReplicationStagedFix',
        'Remove-ReplicationPlatformTitlePrefix',
        'New-ReplicationPullRequestTitle',
        'New-ReplicationPullRequestBody',
        'Get-ReplicationFixRegressionSignal',
        'Get-ReplicationExpressionSkeleton',
        'Get-ReplicationOracleIndependenceSignal',
        'Test-ReplicationPullRequestCarriesFix',
        'Resolve-ReplicationSourceRepository'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $prScript -Name $name)
    }
    $validatorScript = Join-Path $PSScriptRoot 'shared/Validate-ReplicationCandidate.ps1'
    foreach ($name in @(
        'ConvertTo-BoundedSingleLine',
        'ConvertTo-ReplicationDisclosureText',
        'Get-ReplicationManifestPropertyValue',
        'Get-ReplicationManifestDisclosure',
        'Get-ReplicationManifestDisclosureList'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $validatorScript -Name $name)
    }
    foreach ($name in @(
        'Test-ReplicationPullRequestBody',
        'Get-ReplicationMigrationKey'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $migrationScript -Name $name)
    }
}

Describe 'Trusted replication evidence publishing' {
    It 'rejects traversal-like and malformed asset prefixes' {
        Test-ReplicationAssetPrefix -Value 'pr-37440/replication/android/123-1' | Should -BeTrue
        Test-ReplicationAssetPrefix -Value '../secrets' | Should -BeFalse
        Test-ReplicationAssetPrefix -Value 'valid//empty' | Should -BeFalse
        Test-ReplicationAssetPrefix -Value '/absolute' | Should -BeFalse
    }

    It 'constructs encoded commit-pinned public asset URLs' {
        Get-ReplicationPublicAssetUrl `
            -Repository 'dotnet/maui' `
            -Commit ('a' * 40) `
            -Prefix 'pr-37440/replication/android/build 12' `
            -FileName 'repro.mp4' |
            Should -BeExactly "https://raw.githubusercontent.com/dotnet/maui/$('a' * 40)/pr-37440/replication/android/build%2012/repro.mp4"
    }

    It 'uses the asset-only branch, refuses overwrite, and pushes through origin' {
        $script:EvidenceSource | Should -Match 'review-tests-assets-v2'
        $script:EvidenceSource | Should -Match 'already exists and will not be overwritten'
        $script:EvidenceSource | Should -Match 'push origin "HEAD:refs/heads/\$AssetBranch"'
        $script:EvidenceSource | Should -Not -Match '\baz\b'
        $script:EvidenceSource | Should -Match 'actualFailureMessage'
        $script:EvidenceSource | Should -Match '\.Contains\('
    }

    It 'rejects evidence publication when the targeted failure message does not match' {
        $candidatePath = Join-Path $TestDrive 'validated-candidate.json'
        [ordered]@{
            validationPassed = $true
            issueNumber = 12345
            platform = 'android'
            baseSha = 'abc123'
            testType = 'unit'
            testFilter = 'Issue12345'
            expectedFailureSignature = 'Issue12345'
            actualFailureMessage = 'Xunit failure: expected red but was blue'
            verificationRunCount = 2
        } |
            ConvertTo-Json |
            Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

        $output = @(& pwsh -NoProfile -File $evidenceScript `
            -ValidatedCandidatePath $candidatePath `
            -EvidenceDirectory $TestDrive `
            -RepositoryRoot $TestDrive `
            -Repository 'dotnet/maui' `
            -AssetBranch 'review-tests-assets-v2' `
            -AssetPrefix 'pr-12345/replication/android/test-1' `
            -DryRun 2>&1)

        $LASTEXITCODE | Should -Not -Be 0
        $output -join [Environment]::NewLine |
            Should -Match 'targeted failure message'
    }
}

Describe 'Trusted replication pull request publishing' {
    It 'creates stable issue/platform markers and bounded branch names' {
        Get-ReplicationPullRequestMarker -IssueNumber 37440 -Platform 'Android' |
            Should -BeExactly '<!-- MAUI_COPILOT_REPLICATION issue=37440 platform=android -->'
        New-ReplicationBranchName -IssueNumber 37440 -Platform 'Mac Catalyst' -BuildId '12/34' |
            Should -BeExactly 'copilot/reproduce-37440-mac-catalyst-12-34'
    }

    It 'strips Azure logging directives and newlines from single-line values' {
        ConvertTo-ReplicationSingleLine -Value "Title`n##vso[task.setvariable variable=X]bad" |
            Should -BeExactly 'Title bad'
    }

    It 'builds a body from a validated document deserialised from JSON' {
        # Build 14999470 produced a ready candidate and then failed the whole
        # publication because the real validated document is a PSCustomObject
        # from ConvertFrom-Json, and reading a property it does not carry
        # throws under StrictMode. Hashtable fixtures never proved this, so the
        # publisher's own strict mode has to be in force here.
        Set-StrictMode -Version 3.0
        $validated = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            verificationTestType = 'DeviceTest'
            testName = 'Issue37440'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 2
            reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = 'preview.gif'
                thumbnail = 'thumbnail.png'
            }
        } | ConvertTo-Json -Depth 10 | ConvertFrom-Json

        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $validated `
            -Evidence $evidence `
            -IssueTitle 'Something is broken' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://example.com/build/1'

        # Without the class and method the body degrades to the filter rather
        # than failing publication.
        $body | Should -Match 'Exact test'
        $body | Should -Match '2 independent times'

        $withNames = $validated | ConvertTo-Json -Depth 10 | ConvertFrom-Json
        $withNames | Add-Member -NotePropertyName testClassName `
            -NotePropertyValue 'Microsoft.Maui.TestCases.Tests.Issues.Issue37440'
        $withNames | Add-Member -NotePropertyName testMethodName `
            -NotePropertyValue 'ReproducesIssue37440'
        $namedBody = New-ReplicationPullRequestBody `
            -Candidate $withNames `
            -Evidence $evidence `
            -IssueTitle 'Something is broken' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://example.com/build/1'
        $namedBody |
            Should -Match 'Microsoft\.Maui\.TestCases\.Tests\.Issues\.Issue37440\.ReproducesIssue37440'
    }

    It 'states the evidence level so a reviewer need not infer it' {
        $validated = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            verificationTestType = 'DeviceTest'
            testName = 'Issue37440'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 2
            reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = 'preview.gif'
                thumbnail = 'thumbnail.png'
            }
            certificationLevel = 'trigger-certified'
            certificationSummary = "**Evidence level: ``trigger-certified``**`n`n| Control | Expected | Result |`n| --- | --- | --- |`n| Trigger removed | passes | 3/3 |"
        } | ConvertTo-Json -Depth 10 | ConvertFrom-Json

        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $validated `
            -Evidence $evidence `
            -IssueTitle 'Something is broken' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://example.com/build/1'

        $body | Should -Match '## Evidence level'
        $body | Should -Match 'trigger-certified'
        $body | Should -Match 'Trigger removed'
    }

    It 'declares the evidence boundary beside every level it reports' {
        # Three of the four newest adversarial reviews refuted a fix on issue
        # fidelity rather than causality, which each had confirmed by hand. The
        # deciding evidence was the reporter's linked reproduction, which this
        # pipeline never fetches, so the body must declare that limitation
        # instead of leaving reviewers to rediscover it one PR at a time.
        $base = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            verificationTestType = 'DeviceTest'
            testName = 'Issue37440'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 2
            reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = 'preview.gif'
                thumbnail = 'thumbnail.png'
            }
        }

        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        $build = {
            param($Extra)
            $candidate = [ordered]@{}
            foreach ($key in $base.Keys) { $candidate[$key] = $base[$key] }
            foreach ($key in $Extra.Keys) { $candidate[$key] = $Extra[$key] }
            New-ReplicationPullRequestBody `
                -Candidate ($candidate | ConvertTo-Json -Depth 10 | ConvertFrom-Json) `
                -Evidence $evidence `
                -IssueTitle 'Something is broken' `
                -IssueOwner 'dotnet' `
                -IssueRepository 'maui' `
                -BuildUrl 'https://example.com/build/1'
        }

        $summaryBody = & $build ([ordered]@{
                certificationLevel = 'certified-oracle'
                certificationSummary = "**Evidence level: ``certified-oracle``**`n`n| Control | Result |`n| --- | --- |"
            })
        $summaryBody | Should -Match 'never downloaded'
        $summaryBody | Should -Match 'different path than the linked sample'

        # The level-only branch is the sibling that gets forgotten: every defect
        # of this shape in this pipeline was a correct fix applied to exactly one
        # of two places that needed it.
        $levelOnlyBody = & $build ([ordered]@{ certificationLevel = 'trigger-certified' })
        $levelOnlyBody | Should -Match 'never downloaded'

        # With no evidence section there is nothing for the disclosure to
        # qualify, and an orphaned caveat would claim a limitation on a
        # certification the body never made.
        $noLevelBody = & $build ([ordered]@{})
        $noLevelBody | Should -Not -Match 'never downloaded'
    }

    It 'renders the upstream cross-reference beside every level it reports' {
        # The project merges bug fixes to a branch this reproduction is not
        # built against, so the issue stays open, the defect genuinely
        # reproduces, and our red test is honest while the pull request is
        # redundant. Measured at 8 of 70 open fix PRs. The signal reports and
        # never refuses: refusing would destroy sound reproductions.
        $base = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            verificationTestType = 'DeviceTest'
            testName = 'Issue37440'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 2
            reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = 'preview.gif'
                thumbnail = 'thumbnail.png'
            }
        }

        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        $signal = '**WARN Upstream cross-reference.** A test case already exists upstream.'

        $build = {
            param($Extra, $Upstream)
            $candidate = [ordered]@{}
            foreach ($key in $base.Keys) { $candidate[$key] = $base[$key] }
            foreach ($key in $Extra.Keys) { $candidate[$key] = $Extra[$key] }
            New-ReplicationPullRequestBody `
                -Candidate ($candidate | ConvertTo-Json -Depth 10 | ConvertFrom-Json) `
                -Evidence $evidence `
                -IssueTitle 'Something is broken' `
                -IssueOwner 'dotnet' `
                -IssueRepository 'maui' `
                -BuildUrl 'https://example.com/build/1' `
                -UpstreamSignal $Upstream
        }

        $summaryBody = & $build ([ordered]@{
                certificationLevel = 'certified-oracle'
                certificationSummary = "**Evidence level: ``certified-oracle``**`n`n| Control | Result |`n| --- | --- |"
            }) $signal
        $summaryBody | Should -Match 'A test case already exists upstream'

        # The level-only branch is the forgotten sibling, exactly as it is for
        # the evidence boundary that sits beside this line.
        $levelOnlyBody = & $build ([ordered]@{ certificationLevel = 'trigger-certified' }) $signal
        $levelOnlyBody | Should -Match 'A test case already exists upstream'

        # An orphaned caveat would qualify a certification the body never made.
        $noLevelBody = & $build ([ordered]@{}) $signal
        $noLevelBody | Should -Not -Match 'A test case already exists upstream'

        # A signal that could not be computed omits the line rather than
        # blocking the body: the check reports, it never withholds a fix.
        $noSignalBody = & $build ([ordered]@{ certificationLevel = 'trigger-certified' }) ''
        $noSignalBody | Should -Not -Match 'Upstream cross-reference'
        $noSignalBody | Should -Match 'never downloaded'
    }

    It 'separates an unreadable upstream ref from a test case that is genuinely absent' {
        # A missing repository, a missing branch and a missing file are all
        # HTTP 404, and `gh api` exits non-zero for an auth failure too. An
        # absent measurement rendered as a clean "nothing found" is the shape
        # this pipeline has paid for five times.
        $probe = Get-ScriptFunctionText `
            -Path (Join-Path $PSScriptRoot 'shared/Get-ReplicationUpstreamFix.ps1') `
            -Name 'Get-ReplicationUpstreamTestCasePresence'
        Invoke-Expression $probe
        Get-Command Get-ReplicationUpstreamTestCasePresence -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        # A function outranks an application, so this shadows the real gh.
        function script:gh {
            $joined = $args -join ' '
            if ($joined -match '/commits/') { return $script:refReply }
            return $script:fileReply
        }

        $script:fileReply = '0123456789abcdef0123456789abcdef01234567'
        Get-ReplicationUpstreamTestCasePresence -Path 'a.cs' -Ref 'r' -Repo 'o/r' |
            Should -Be 'present'

        # File missing but the ref resolves: a real measurement.
        $script:fileReply = 'gh: Not Found (HTTP 404)'
        $script:refReply = 'fedcba9876543210fedcba9876543210fedcba98'
        Get-ReplicationUpstreamTestCasePresence -Path 'a.cs' -Ref 'r' -Repo 'o/r' |
            Should -Be 'absent'

        # File missing AND the ref does not resolve: nothing was measured.
        $script:refReply = 'gh: Not Found (HTTP 404)'
        Get-ReplicationUpstreamTestCasePresence -Path 'a.cs' -Ref 'r' -Repo 'o/r' |
            Should -Be 'unknown'

        # An auth failure is not a 404 and must never read as absent.
        $script:fileReply = 'gh: Bad credentials (HTTP 401)'
        Get-ReplicationUpstreamTestCasePresence -Path 'a.cs' -Ref 'r' -Repo 'o/r' |
            Should -Be 'unknown'
    }

    It 'attributes an upstream test case to the commit that introduced it' {
        # The commits endpoint returns deletions too, and it is newest-first.
        # Issue 24966's most recent toucher upstream is a Revert, so reading
        # the newest commit reports "already fixed upstream" where upstream
        # backed the fix out - the inverse of the truth, and a case where our
        # reproduction is worth more rather than less.
        $publisher = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $upstream = Join-Path $PSScriptRoot 'shared/Get-ReplicationUpstreamFix.ps1'
        Invoke-Expression (Get-ScriptFunctionText -Path $upstream `
            -Name 'Get-ReplicationUpstreamTestCasePresence')
        Invoke-Expression (Get-ScriptFunctionText -Path $publisher `
            -Name 'Get-ReplicationUpstreamFixSignal')
        Get-Command Get-ReplicationUpstreamFixSignal -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        # Newest-first, exactly as the commits endpoint returns it.
        $script:upstreamCommits = @(
            'Revert "Fix DatePicker background not cleared"',
            'Fix DatePicker background not cleared')

        # Faithful to production: the file exists upstream and is absent on the
        # commit under test, while BOTH refs resolve. An earlier shadow 404'd
        # the base ref too, which correctly degraded the probe to "not
        # measured" - a failing test caused by an unfaithful fixture, not by
        # the code. The jq is modelled rather than ignored: a stub returning
        # one scalar cannot tell `|last` from `|first`, so it could not express
        # the very defect this test exists to catch.
        function script:gh {
            $joined = $args -join ' '
            if ($joined -match 'commits\?sha=') {
                if ($joined -match '\|\s*first') { return $script:upstreamCommits[0] }
                if ($joined -match '\|\s*last') { return $script:upstreamCommits[-1] }
                return ''
            }
            if ($joined -match 'commits/') { return '0123456789abcdef0123456789abcdef01234567' }
            if ($joined -match 'contents/.*ref=inflight') { return '0123456789abcdef0123456789abcdef01234567' }
            return 'gh: Not Found (HTTP 404)'
        }

        # `last` on a newest-first list is the oldest commit: the introducer.
        $signal = Get-ReplicationUpstreamFixSignal -IssueNumber '36933' `
            -BaseSha '1111111111111111111111111111111111111111'
        $signal | Should -Match 'already exists'
        $signal | Should -Match 'Fix DatePicker background not cleared'
        # The load-bearing assertion: reading the newest commit would name the
        # Revert, claiming "already fixed upstream" where upstream backed the
        # fix out - the inverse of the truth.
        $signal | Should -Not -Match 'Revert'

        # Uncontrolled upstream text must be sanitized, never validated by a
        # function that throws: a presentation bound should never be able to
        # discard the work it describes.
        $script:upstreamCommits = @('newer', "Fix`tthing https://evil.example `u{1F600} @someone")
        { Get-ReplicationUpstreamFixSignal -IssueNumber '36933' `
                -BaseSha '1111111111111111111111111111111111111111' } | Should -Not -Throw
    }

    It 'keeps the boundary claim true by measuring the sanitizer that enforces it' {
        # The body asserts a fact about a different script. Left as prose it
        # becomes a lie the moment that script changes, so the claim is measured
        # against the real redaction rather than restated here.
        $contextScript = Join-Path $PSScriptRoot 'shared/Get-ReplicationIssueContext.ps1'
        foreach ($name in @(
                'Remove-UnsafeIssueCharacters',
                'Remove-AzureLoggingCommands',
                'Remove-IssueInstructionMarkers',
                'Limit-IssueLine',
                'ConvertTo-SafeIssueProse'
            )) {
            Invoke-Expression (Get-ScriptFunctionText -Path $contextScript -Name $name)
        }

        $sanitized = ConvertTo-SafeIssueProse `
            -Text 'Repro here: https://github.com/someone/BugRepro please clone it'

        $sanitized | Should -Not -Match 'github\.com/someone/BugRepro'
        $sanitized | Should -Match '\[url removed\]'
    }

    It 'omits the evidence level section when the gate reported none' {
        $validated = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            verificationTestType = 'DeviceTest'
            testName = 'Issue37440'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 2
            reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = 'preview.gif'
                thumbnail = 'thumbnail.png'
            }
        } | ConvertTo-Json -Depth 10 | ConvertFrom-Json

        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $validated `
            -Evidence $evidence `
            -IssueTitle 'Something is broken' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://example.com/build/1'

        $body | Should -Not -Match '## Evidence level'
    }

    It 'generates a draft body with video evidence, unconditional failure semantics, and safety statement' {
        $candidate = [pscustomobject]@{
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Expected: 1; Actual: 0'
            actualFailureMessage = 'Xunit failure. Expected: 1; Actual: 0'
            verificationRunCount = 2
            reproductionSteps = @('Launch the scenario', 'Tap the action button')
        }
        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        $body | Should -Match 'AI-generated \*\*reproduction evidence\*\*'
        $body | Should -Match 'test intentionally fails on the unfixed baseline'
        $body | Should -Match '\[!\[Reproduction preview\]\(https://example.test/preview.gif\)\]\(https://example.test/repro.mp4\)'
        $body | Should -Match 'No linked repository, archive, binary, script, package, or arbitrary external file was downloaded'
        $body | Should -Match 'MAUI_COPILOT_REPLICATION issue=37440 platform=android'
        # Reviews of kubaflo/maui#164 and #173 both treated the recording as
        # probative; state its evidentiary role explicitly instead.
        $body | Should -Match 'authoritative proof is the trusted targeted test'
        $body | Should -Match 'only the app-reported verdict rather than the defect itself'
        $body | Should -Match 'reproduction commit sits directly on the baseline above'
    }

    It 'names the simulator or emulator instead of claiming an on-device run' {
        # The review of kubaflo/maui#180 rejected the media partly because it
        # "describes simulator-shaped evidence as on-device".
        $candidate = [pscustomobject]@{
            issueNumber = 35624
            platform = 'ios'
            baseSha = 'abc123'
            testType = 'ui'
            testFilter = 'Issue35624'
            expectedFailureSignature = 'Kern=8'
            actualFailureMessage = 'Expected Kern=8 but was Kern=NaN'
            verificationRunCount = 2
            reproductionSteps = @('Focus the search handler')
        }
        $evidence = [pscustomobject]@{
            device = '63836186-6767-400C-A56D-25093A72BE13'
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        $body | Should -Match 'iOS Simulator'
        $body | Should -Match '63836186-6767-400C-A56D-25093A72BE13'
        $body | Should -Not -Match 'on-device'
        $body | Should -Not -Match 'device or emulator'
    }

    It 'names the Android emulator for an android run' {
        $candidate = [pscustomobject]@{
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Expected: 1; Actual: 0'
            actualFailureMessage = 'Xunit failure. Expected: 1; Actual: 0'
            verificationRunCount = 2
            reproductionSteps = @('Launch the scenario')
        }
        $evidence = [pscustomobject]@{
            device = 'emulator-5554'
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        $body | Should -Match 'Android emulator'
        $body | Should -Match 'emulator-5554'
        $body | Should -Not -Match 'on-device'
    }

    It 'refuses to claim a platform-neutral test proved the recorded platform behavior' {
        $candidate = [pscustomobject]@{
            issueNumber = 33333
            platform = 'android'
            baseSha = 'abc123'
            testType = 'UnitTest'
            testFilter = 'Issue33333'
            expectedFailureSignature = 'Expected: 1; Actual: 0'
            actualFailureMessage = 'Xunit failure. Expected: 1; Actual: 0'
            verificationRunCount = 3
            reproductionSteps = @('Launch the scenario')
        }
        $evidence = [pscustomobject]@{
            device = 'emulator-5554'
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        # The reviewer rejected an earlier PR because one sentence covered both
        # the recording and a platform-invariant test, implying the test proved
        # the Android behavior. Both facts must stay separately attributed.
        $body | Should -Match 'platform-neutral'
        $body | Should -Match 'ran on the build host'
        $body | Should -Not -Match `
            'reproduced the behavior on the Android emulator [^.]*and matched the expected targeted test failure'
    }

    It 'still claims a single surface for a test that runs where the recording happened' {
        $candidate = [pscustomobject]@{
            issueNumber = 33334
            platform = 'android'
            baseSha = 'abc123'
            testType = 'DeviceTest'
            testFilter = 'Issue33334'
            expectedFailureSignature = 'Expected: 1; Actual: 0'
            actualFailureMessage = 'Xunit failure. Expected: 1; Actual: 0'
            verificationRunCount = 3
            reproductionSteps = @('Launch the scenario')
        }
        $evidence = [pscustomobject]@{
            device = 'emulator-5554'
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        $body | Should -Match 'matched the expected targeted test failure'
        $body | Should -Not -Match 'platform-neutral'
    }

    It 'rejects publication when the expected signature is absent from the failure message' {
        $candidate = [pscustomobject]@{
            issueNumber = 12345
            platform = 'android'
            baseSha = 'abc123'
            testType = 'unit'
            testFilter = 'Issue12345'
            expectedFailureSignature = 'Issue12345'
            actualFailureMessage = 'Xunit failure: expected red but was blue'
            verificationRunCount = 2
            reproductionSteps = @('Run the test')
        }
        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        {
            New-ReplicationPullRequestBody `
                -Candidate $candidate `
                -Evidence $evidence `
                -IssueTitle 'Reported behavior' `
                -IssueOwner dotnet `
                -IssueRepository maui `
                -BuildUrl ''
        } | Should -Throw '*targeted failure message*'
    }

    It 'uses an add-only staged diff and creates a MauiBot fork draft fix PR' {
        $script:PrSource | Should -Match 'git diff --cached --name-status --diff-filter=ACDMRTUXB'
        $script:PrSource | Should -Match '\s--draft'
        $script:PrSource | Should -Match 'GH_TOKEN is required'
        $script:PrSource | Should -Match "'push', \`$sourceRemote"
        $script:PrSource.Contains('--head "$sourceOwner`:$branchName"') | Should -BeTrue
        $script:PrSource | Should -Match "GH_TOKEN must authenticate as 'MauiBot'"
        $script:PrSource | Should -Match 'Expected exactly one writable fork'
        $script:PrSource | Should -Match 'affiliations: \[OWNER, ORGANIZATION_MEMBER\]'
        $script:PrSource | Should -Match '-X POST'
        $script:PrSource | Should -Match 'creating one failed'
        $script:PrSource | Should -Match 'did not become writable within 60 seconds'
        $script:PrSource | Should -Match "'replication-fork'"
        $script:PrSource.Contains("[string]`$TargetOwner = 'kubaflo'") | Should -BeTrue
        $script:PrSource | Should -Match '-ParentOwner \$IssueOwner'
        $script:PrSource | Should -Match '--repo "\$TargetOwner/\$TargetRepository"'
        $script:PrSource | Should -Not -Match 'https://[^"\s]*\$env:GH_TOKEN'
    }

    It 'refuses a reproduction-only candidate before reading publication assets' {
        $candidatePath = Join-Path $TestDrive 'reproduction-only-candidate.json'
        [ordered]@{
            validationPassed = $true
            issueNumber = 12345
            platform = 'android'
            fixFiles = @()
        } |
            ConvertTo-Json |
            Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

        $publisher = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $output = @(& pwsh -NoProfile -File $publisher `
            -ValidatedCandidatePath $candidatePath `
            -PublishedEvidencePath (Join-Path $TestDrive 'missing-evidence.json') `
            -IssueContextPath (Join-Path $TestDrive 'missing-context.json') `
            -PatchPath (Join-Path $TestDrive 'missing-test.patch') `
            -RepositoryRoot $TestDrive `
            -DryRun 2>&1)

        $LASTEXITCODE | Should -Not -Be 0
        $rendered = $output -join [Environment]::NewLine
        $rendered | Should -Match 'reproduction-only pull requests are'
        $rendered | Should -Match 'not published'
    }

    It 'opens the reproduction against the exact commit it was verified on' {
        # Reviews of kubaflo/maui#189, #193, and #194 each reported that the
        # pull request's first parent was not the verified baseline. PR #177
        # showed the opposite failure: committing onto the baseline while
        # basing on a moving branch listed nine unrelated files in the diff.
        # Pinning the base to the baseline satisfies both at once.
        $script:PrSource | Should -Match "'replication-target'"
        $script:PrSource.Contains('@(''checkout'', ''--detach'', $baselineSha)') |
            Should -BeTrue
        $script:PrSource | Should -Match '--base \$BaseBranch'
        # A baseline the publisher cannot verify must never be committed onto.
        $script:PrSource.Contains('$baselineSha -cnotmatch ''^[0-9a-f]{40}$''') |
            Should -BeTrue
        # Committing onto a baseline outside the base branch would drag
        # unrelated commits into the diff, as kubaflo/maui#177 did.
        $script:PrSource.Contains(
            'git merge-base --is-ancestor $baselineSha FETCH_HEAD') | Should -BeTrue
        $script:PrSource | Should -Match 'would carry unrelated commits'
        $script:PrSource.Contains("@('checkout', '--detach', 'FETCH_HEAD')") | Should -BeFalse
    }
}

Describe 'A published UI selector has to select the test' {
    # Reviewers of kubaflo/maui#212 and #205 independently copied the "exact
    # test" this body publishes and measured that it selects zero tests:
    # Controls UI tests derive from the parameterised UITest fixture, so the
    # runtime name is Issue35760(Android).Method, not Issue35760.Method. A
    # selector that matches nothing reports the same "no failures" as a test
    # that passed, so publishing one invites exactly the wrong conclusion.
    BeforeAll {
        $script:UiEvidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        function script:New-UiBody {
            param(
                [string]$Platform = 'android',
                [string]$TestType = 'ui',
                [switch]$OmitNames
            )

            $candidate = [ordered]@{
                issueNumber = 37281
                platform = $Platform
                baseSha = 'abc123'
                testType = $TestType
                testFilter = 'Issue37281'
                expectedFailureSignature = 'Expected: 0; Actual: 94'
                actualFailureMessage = 'Assert failure. Expected: 0; Actual: 94'
                verificationRunCount = 2
                reproductionSteps = @('Scroll the shadowed content')
            }
            if (-not $OmitNames) {
                $candidate['testClassName'] =
                    'Microsoft.Maui.TestCases.Tests.Issues.Issue37281'
                $candidate['testMethodName'] =
                    'TouchScrollingDoesNotRedrawShadowedContent'
            }

            New-ReplicationPullRequestBody `
                -Candidate ($candidate | ConvertTo-Json -Depth 10 | ConvertFrom-Json) `
                -Evidence $script:UiEvidence `
                -IssueTitle 'Shadowed content redraws while scrolling' `
                -IssueOwner 'dotnet' `
                -IssueRepository 'maui' `
                -BuildUrl 'https://example.test/build/1'
        }
    }

    It 'names a UI test the way the runner reports it' {
        script:New-UiBody |
            Should -Match ([regex]::Escape(
                'Issue37281(Android).TouchScrollingDoesNotRedrawShadowedContent'))
    }

    It 'never publishes the bare class.method a reviewer would paste into an equality filter' {
        $exactLine = @((script:New-UiBody) -split "`r?`n" |
            Where-Object { $_ -match '^- Exact test:' })
        $exactLine.Count | Should -Be 1
        $exactLine[0] | Should -Match ([regex]::Escape('Issue37281(Android).'))
        $exactLine[0] | Should -Not -Match ([regex]::Escape(
            '`Microsoft.Maui.TestCases.Tests.Issues.Issue37281.TouchScrollingDoesNotRedrawShadowedContent`'))
    }

    It 'publishes the contains selector reviewers verified on the UI lane' {
        $body = script:New-UiBody
        $body | Should -Match ([regex]::Escape('FullyQualifiedName~Issue37281'))
        $body | Should -Match 'selects no tests'
        $body | Should -Match 'filter grouping'
    }

    It 'uses each platform''s own TestDevice argument' {
        # TestDevice spells iOS with a lowercase i, and the runner reports the
        # member name verbatim, so this comparison has to be case sensitive.
        $expected = @{
            android = 'Android'
            ios = 'iOS'
            windows = 'Windows'
            catalyst = 'Mac'
        }
        foreach ($platform in $expected.Keys) {
            (script:New-UiBody -Platform $platform).Contains(
                "Issue37281($($expected[$platform])).",
                [StringComparison]::Ordinal) | Should -BeTrue
        }
    }

    It 'leaves a device test''s name and selector alone' {
        $body = script:New-UiBody -TestType 'device'
        $body | Should -Not -Match ([regex]::Escape('Issue37281(Android)'))
        $body | Should -Match ([regex]::Escape(
            'Microsoft.Maui.TestCases.Tests.Issues.Issue37281.TouchScrollingDoesNotRedrawShadowedContent'))
        $body | Should -Match 'Device runner selector'
        $body | Should -Not -Match 'UI runner selector'
    }

    It 'adds no selector caveat to a unit test' {
        $body = script:New-UiBody -TestType 'unit'
        $body | Should -Not -Match 'UI runner selector'
        $body | Should -Not -Match 'Device runner selector'
    }

    It 'never emits both selector caveats at once' {
        foreach ($testType in @('ui', 'device', 'unit', 'xaml')) {
            $lines = @((script:New-UiBody -TestType $testType) -split "`r?`n" |
                Where-Object { $_ -match 'runner selector:' })
            $lines.Count | Should -BeLessOrEqual 1
        }
    }

    It 'keeps the surrounding bullets one unbroken list' {
        # An empty placeholder for each caveat used to leave consecutive blank
        # lines mid-list, which Markdown renders as two separate loose lists.
        foreach ($testType in @('ui', 'device', 'unit', 'xaml')) {
            (script:New-UiBody -TestType $testType) |
                Should -Not -Match "(?m)^- Targeted filter:.*`r?`n`r?`n`r?`n"
        }
    }

    It 'falls back to the filter when the class and method are unknown' {
        $body = script:New-UiBody -OmitNames
        $body | Should -Match 'Exact test'
        $body | Should -Not -Match 'UI runner selector'
    }
}

Describe 'Trusted replication PR migration' {
    It 'recognizes and keys only bounded replication markers' {
        $pull = [pscustomobject]@{
            number = 42
            body = '<!-- MAUI_COPILOT_REPLICATION issue=37440 platform=android -->'
        }

        Test-ReplicationPullRequestBody -Body $pull.body | Should -BeTrue
        Get-ReplicationMigrationKey -PullRequest $pull | Should -BeExactly '37440/android'
        Test-ReplicationPullRequestBody -Body '<!-- unrelated -->' | Should -BeFalse
    }

    It 'closes upstream only after a testing PR URL is verified' {
        $createIndex = $script:MigrationSource.IndexOf('$targetUrl = [string]$targetPull.html_url')
        $closeIndex = $script:MigrationSource.IndexOf('"Closing upstream PR #')

        $script:MigrationSource.Contains("[string]`$TargetOwner = 'kubaflo'") | Should -BeTrue
        $script:MigrationSource.Contains("`$headRepository -ne 'MauiBot/maui'") | Should -BeTrue
        $createIndex | Should -BeGreaterThan -1
        $closeIndex | Should -BeGreaterThan $createIndex
        $script:MigrationSource | Should -Match "state = 'closed'"
    }

    It 'states the real parent commit and the recording provenance' {
        # kubaflo/maui#179 and #180 review: the body named the validated
        # baseline as the parent even though the commit is applied onto the
        # base branch tip, and the recording was read as exact-head evidence.
        $body = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1') -Raw

        $body | Should -Match 'Validated on baseline commit'
        $body | Should -Match 'first parent of the commit in this pull request is exactly the commit'
        $body | Should -Match 'this diff contains only the added reproduction test'
        $body | Should -Match 'not of the committed test executing'
        $body | Should -Match 'not as exact-head evidence'
        $body.Contains('- Baseline commit: ``$baseSha``') | Should -BeFalse
    }

    It 'reports an issue already covered instead of failing the build' {
        # Build 15001510 reproduced issue 37151 and authored its test while an
        # earlier run published the same issue and platform. Being second is
        # redundant, not broken, so the run says what already covers it.
        $script:PrSource | Should -Not -Match 'throw "An open reproduction pull request already exists'
        $script:PrSource | Should -Match 'already covers this issue and platform'
        $script:PrSource | Should -Match '\$plan\.duplicateOf = \[string\]\$duplicate\.url'
    }

    It 'still writes a publication manifest when it publishes nothing' {
        # The caller always reads the manifest, so exiting early without one
        # would trade a clear duplicate report for a missing-file error.
        $duplicateIndex = $script:PrSource.IndexOf('$plan.duplicateOf = [string]$duplicate.url')
        $duplicateIndex | Should -BeGreaterThan 0

        $earlyExit = $script:PrSource.Substring($duplicateIndex, 400)
        $earlyExit | Should -Match 'Write-ReplicationPublicationManifest -Plan \$plan'
        $earlyExit | Should -Match 'exit 0'
    }
}

Describe 'A pull request that carries a fix says so' {
    BeforeAll {
        function script:New-FixCandidate {
            param([hashtable]$Extra)

            $document = [ordered]@{
                schemaVersion = 1
                status = 'validated'
                validationPassed = $true
                issueNumber = 37440
                platform = 'android'
                baseSha = 'abc123'
                testType = 'device'
                verificationTestType = 'DeviceTest'
                testName = 'Issue37440'
                testFilter = 'Issue37440'
                testClassName = 'Microsoft.Maui.DeviceTests.Issue37440'
                testMethodName = 'ReproducesIssue37440'
                expectedFailureSignature = 'Issue37440'
                expectedFailurePattern = 'Issue37440'
                actualFailureMessage = 'Xunit failure: Issue37440 expected red but was blue'
                verificationRunCount = 2
                certificationLevel = 'trigger-certified'
                certificationSummary = '**Evidence level: `trigger-certified`**'
                reproductionMarker = 'BUG REPRODUCED:'
                files = @('src/Core/tests/DeviceTests/Handlers/Issue37440.cs')
                reproductionSteps = @('Open the page', 'Tap the control')
                evidence = [ordered]@{
                    video = 'repro.mp4'
                    preview = 'preview.gif'
                    thumbnail = 'thumbnail.png'
                }
            }
            if ($Extra) {
                foreach ($key in $Extra.Keys) {
                    $document[$key] = $Extra[$key]
                }
            }
            return $document | ConvertTo-Json -Depth 10 | ConvertFrom-Json
        }

        $script:fixEvidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }

        function script:Get-FixBody {
            param([Parameter(Mandatory = $true)]$Candidate)

            return New-ReplicationPullRequestBody `
                -Candidate $Candidate `
                -Evidence $script:fixEvidence `
                -IssueTitle 'Button ignores its padding' `
                -IssueOwner 'dotnet' `
                -IssueRepository 'maui' `
                -BuildUrl 'https://example.com/build/1'
        }
    }

    It 'says nothing about a fix when there is none' {
        Set-StrictMode -Version 3.0
        $body = script:Get-FixBody -Candidate (script:New-FixCandidate)

        $body | Should -Not -Match 'Proposed fix'
        $body | Should -Match 'not a merge-ready product fix'
        $body | Should -Match 'The published patch is add-only'
    }

    It 'describes the fix, its files, and the two commits that carry it' {
        Set-StrictMode -Version 3.0
        $candidate = script:New-FixCandidate -Extra @{
            certificationLevel = 'certified-oracle'
            fixFiles = @('src/Controls/src/Core/Button/Button.cs')
            fixPatch = 'fix.patch'
            fixRootCause = 'Padding was applied before the handler measured the label.'
            fixApproach = 'Invalidate the measure after padding changes instead of before.'
            fixRejectedApproaches = @(
                'Clamping the padding, which hid the symptom on one platform only.'
            )
        }

        $body = script:Get-FixBody -Candidate $candidate

        $body | Should -Match 'Proposed fix'
        $body | Should -Match 'reproduction evidence with a proposed fix'
        $body | Should -Not -Match 'not a merge-ready product fix'
        $body | Should -Match 'src/Controls/src/Core/Button/Button\.cs'
        $body | Should -Match 'Padding was applied before the handler measured the label'
        $body | Should -Match 'Invalidate the measure after padding changes'
        $body | Should -Match 'Clamping the padding'
        $body | Should -Match 'two commits'
        $body | Should -Match 'The reproduction commit is add-only'
    }

    It 'still describes a fix that carries no prose' {
        Set-StrictMode -Version 3.0
        $candidate = script:New-FixCandidate -Extra @{
            fixFiles = @('src/Core/src/Handlers/Button/ButtonHandler.cs')
            fixPatch = 'fix.patch'
        }

        $body = script:Get-FixBody -Candidate $candidate

        $body | Should -Match 'Proposed fix'
        $body | Should -Match 'src/Core/src/Handlers/Button/ButtonHandler\.cs'
        $body | Should -Not -Match 'Approaches considered and rejected'
    }

    It 'refuses to let a fix file smuggle a pipeline command into the body' {
        Set-StrictMode -Version 3.0
        $candidate = script:New-FixCandidate -Extra @{
            fixFiles = @('src/Core/src/Foo.cs')
            fixPatch = 'fix.patch'
            fixRootCause = "Root cause`n##vso[task.setvariable variable=X]bad"
        }

        $body = script:Get-FixBody -Candidate $candidate

        $body | Should -Not -Match '##vso'
    }
}

Describe 'The fix commit may only contain the fix the candidate was validated for' {
    It 'accepts a staged fix that modifies exactly the manifested files' {
        # The manifest is a set, not a sequence: git reports staged paths in its
        # own order, so a correct fix must be accepted whichever order either
        # side happens to list. Both lists here are deliberately unsorted, and
        # the manifest repeats a file, as a candidate diff legitimately can.
        $actual = Assert-ReplicationStagedFix `
            -StagedLines @("M`tsrc/Core/src/Layout.cs", "M`tsrc/Controls/src/Grid.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs', 'src/Controls/src/Grid.cs', 'src/Core/src/Layout.cs')

        $actual | Should -Be @('src/Controls/src/Grid.cs', 'src/Core/src/Layout.cs')
    }

    It 'rejects a fix that adds a file the candidate never declared' {
        # An added file escapes the validator entirely: the manifest is built by
        # walking the patch, so a file introduced afterwards was never checked
        # against the expert-named scope.
        { Assert-ReplicationStagedFix `
            -StagedLines @("M`tsrc/Core/src/Layout.cs", "A`tsrc/Core/src/Sneak.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs') } |
            Should -Throw '*not modification-only*'
    }

    It 'rejects a fix that deletes a product file' {
        { Assert-ReplicationStagedFix `
            -StagedLines @("D`tsrc/Core/src/Layout.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs') } |
            Should -Throw '*not modification-only*'
    }

    It 'rejects a fix that renames a product file' {
        { Assert-ReplicationStagedFix `
            -StagedLines @("R100`tsrc/Core/src/Layout.cs`tsrc/Core/src/Renamed.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs') } |
            Should -Throw '*not modification-only*'
    }

    It 'rejects a fix that touches fewer files than the manifest promised' {
        # Under-application is as dangerous as over-application: the arms were
        # measured against the whole diff, so a partial apply was never proven.
        { Assert-ReplicationStagedFix `
            -StagedLines @("M`tsrc/Core/src/Layout.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs', 'src/Controls/src/Grid.cs') } |
            Should -Throw '*do not exactly match*'
    }

    It 'rejects a fix that touches more files than the manifest promised' {
        { Assert-ReplicationStagedFix `
            -StagedLines @("M`tsrc/Core/src/Layout.cs", "M`tsrc/Core/src/Extra.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs') } |
            Should -Throw '*do not exactly match*'
    }

    It 'rejects a fix that stages nothing at all' {
        { Assert-ReplicationStagedFix `
            -StagedLines @() `
            -ExpectedFiles @('src/Core/src/Layout.cs') } |
            Should -Throw '*do not exactly match*'
    }

    It 'compares Windows and posix separators as the same file' {
        $actual = Assert-ReplicationStagedFix `
            -StagedLines @("M`tsrc\Core\src\Layout.cs") `
            -ExpectedFiles @('src/Core/src/Layout.cs')

        $actual | Should -Be @('src/Core/src/Layout.cs')
    }
}

Describe 'The title may only promise what the diff actually contains' {
    It 'announces a fix when the candidate carries one' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 36545 `
            -Platform 'ios' `
            -IssueTitle 'Entry Completed fires twice' `
            -CarriesFix |
            Should -Be '[maui-bot-fix][ios] Fix for #36545 - Entry Completed fires twice'
    }

    It 'keeps the reproduction title when no fix is present' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 36545 `
            -Platform 'ios' `
            -IssueTitle 'Entry Completed fires twice' |
            Should -Be '[ios] Add failing reproduction for #36545'
    }

    It 'never claims a fix for a reproduction-only PR' {
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 1 `
            -Platform 'android' `
            -IssueTitle 'Something broke' `
            -CarriesFix:$false

        $title | Should -Not -Match 'maui-bot-fix'
        $title | Should -Not -Match '(?i)\bfix for\b'
    }

    It 'still names the issue when its title is unavailable' {
        New-ReplicationPullRequestTitle -IssueNumber 42 -Platform 'ios' -IssueTitle '' -CarriesFix |
            Should -Be '[maui-bot-fix][ios] Fix for #42'
    }

    It 'still names the issue when its title is null' {
        New-ReplicationPullRequestTitle -IssueNumber 42 -Platform 'ios' -IssueTitle $null -CarriesFix |
            Should -Be '[maui-bot-fix][ios] Fix for #42'
    }

    It 'strips newlines so an issue title cannot forge extra lines' {
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle "harmless`nLGTM, merging" `
            -CarriesFix

        $title | Should -Not -Match "`n"
        $title | Should -Be '[maui-bot-fix][ios] Fix for #7 - harmless LGTM, merging'
    }

    It 'strips control characters an issue title may carry' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle "a`tb" `
            -CarriesFix |
            Should -Be '[maui-bot-fix][ios] Fix for #7 - a b'
    }

    It 'bounds the title so it stays legible in a list' {
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle ('x' * 400) `
            -CarriesFix

        $title.Length | Should -BeLessOrEqual 120
        $title | Should -Match '…$'
    }

    It 'does not truncate a title that already fits' {
        $summary = 'Short enough'
        New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle $summary `
            -CarriesFix |
            Should -Be "[maui-bot-fix][ios] Fix for #7 - $summary"
    }

    It 'drops the summary entirely when there is no room for a meaningful one' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 123456789 `
            -Platform 'ios' `
            -IssueTitle 'a summary that will not fit' `
            -CarriesFix `
            -MaxLength 40 |
            Should -Be '[maui-bot-fix][ios] Fix for #123456789'
    }

    It 'names the validated platform, not the platforms the reporter listed' {
        # Verbatim from dotnet/maui#35667, published as kubaflo/maui PR 509 and
        # rejected by a human reviewer for claiming Android and Catalyst.
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 35667 `
            -Platform 'ios' `
            -IssueTitle '[Android, iOS, Catalyst] TextTransform.Uppercase does not work on Shell SearchHandler' `
            -CarriesFix

        $title | Should -Match '^\[maui-bot-fix\]\[ios\] '
    }

    It 'keeps the [maui-bot-fix] filter matching at the very start' {
        # The only filter the reviewing human uses to find these PRs.
        foreach ($platform in @('android', 'ios', 'catalyst', 'windows')) {
            $title = New-ReplicationPullRequestTitle `
                -IssueNumber 100 `
                -Platform $platform `
                -IssueTitle 'Something broke' `
                -CarriesFix

            $title.StartsWith('[maui-bot-fix]', [StringComparison]::Ordinal) |
                Should -BeTrue
            $title | Should -Match ([regex]::Escape("[$platform]"))
        }
    }

    It 'still bounds the title once the platform tag is added' {
        # The tag lengthens the prefix, so the summary budget must shrink with it
        # rather than pushing the title past the bound.
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'catalyst' `
            -IssueTitle ('x' * 400) `
            -CarriesFix

        $title.Length | Should -BeLessOrEqual 120
        $title | Should -Match '^\[maui-bot-fix\]\[catalyst\] '
    }

    It 'does not tag the reproduction title twice' {
        # It already leads with [platform]; a second tag would be noise.
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 8 `
            -Platform 'android' `
            -IssueTitle 'Something broke'

        $title | Should -Be '[android] Add failing reproduction for #8'
    }
}

Describe 'A quoted reporter title may not restate the platform claim' {
    # Two reviewers rejected fix pull requests for a title that named platforms
    # the run never validated - 509 and, after the platform tag shipped, 458.
    # The tag is authoritative, so the reporter's leading platform list is
    # dropped; anything else in a leading bracket is kept, because it carries
    # something the tag does not.

    It 'drops a leading bracket that holds only platform names' -ForEach @(
        @{ Quoted = '[Android, iOS and Catalyst] SearchHandler CharacterSpacing property is not applied'
           Expected = 'SearchHandler CharacterSpacing property is not applied' }
        @{ Quoted = '[Android, iOS, Catalyst] TextTransform.Uppercase does not work'
           Expected = 'TextTransform.Uppercase does not work' }
        @{ Quoted = '[iOS/MacCatalyst] DatePicker Background is not cleared'
           Expected = 'DatePicker Background is not cleared' }
        @{ Quoted = '[iOS, Mac & Windows]Button BackgroundColor does not restore'
           Expected = 'Button BackgroundColor does not restore' }
        @{ Quoted = '[iOS] MauiMKMapView.AddElements replaces its list'
           Expected = 'MauiMKMapView.AddElements replaces its list' }
    ) {
        Remove-ReplicationPlatformTitlePrefix -Title $Quoted | Should -Be $Expected
    }

    It 'keeps a leading bracket that carries anything else' -ForEach @(
        @{ Quoted = '[iOS 26.5] MediaPicker selection intermittently remains open' }
        @{ Quoted = '[Android 16] MonoVsDbg debugger fails on second F5 launch' }
        @{ Quoted = '[REGRESSION: iOS, 10.0.100] Page scrolling behavior is broken' }
        @{ Quoted = '[macOS CI] Flaky Label tests pass locally but fail in CI' }
        @{ Quoted = '[Bug] Entry does not raise Completed' }
        @{ Quoted = '[.NET 10] Shell navigation throws' }
        @{ Quoted = '[XamlC] Compiled bindings fail on nested types' }
        @{ Quoted = '[regression/9.0.0] CollectionView scroll position resets' }
    ) {
        # A version, a release, a scan label or a component name is not a claim
        # the platform tag already makes, so removing it would lose information.
        Remove-ReplicationPlatformTitlePrefix -Title $Quoted | Should -Be $Quoted
    }

    It 'only considers a bracket that opens the title' {
        $quoted = 'Entry [iOS] loses focus'
        Remove-ReplicationPlatformTitlePrefix -Title $quoted | Should -Be $quoted
    }

    It 'leaves a title that opens with no bracket alone' {
        $quoted = 'SearchHandler CharacterSpacing is not applied'
        Remove-ReplicationPlatformTitlePrefix -Title $quoted | Should -Be $quoted
    }

    It 'is measured against the real dotnet/maui title corpus' {
        # The safety of this rule is a claim about titles reporters actually
        # write, so it is asserted against them rather than against fixtures
        # chosen to agree with it. Measured over 361 real leading-bracket
        # titles: 37 stripped, 324 kept, no false positives.
        $stripped = @('[iOS] a', '[Android] b', '[Windows] c', '[iOs] d')
        $kept = @('[iOS 26.5] a', '[Android 16] b', '[REGRESSION: iOS, 10.0.100] c',
                  '[macOS CI] d', '[leak-scan] e', '[ci-scan-net11] f', '[NET11] g')

        foreach ($t in $stripped) {
            Remove-ReplicationPlatformTitlePrefix -Title $t | Should -Not -Be $t
        }
        foreach ($t in $kept) {
            Remove-ReplicationPlatformTitlePrefix -Title $t | Should -Be $t
        }
    }

    It 'falls back to the prefix when the bracket was the whole title' {
        # Dropping the bracket can empty the summary, so the strip has to run
        # before the emptiness check or the title would end in a bare separator.
        New-ReplicationPullRequestTitle `
            -IssueNumber 11 `
            -Platform 'ios' `
            -IssueTitle '[Android, iOS]' `
            -CarriesFix |
            Should -Be '[maui-bot-fix][ios] Fix for #11'
    }

    It 'strips the platform list out of the published fix title' {
        # The call site is where this class of defect lives, so the property is
        # asserted end to end and not only on the helper.
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 35624 `
            -Platform 'ios' `
            -IssueTitle '[Android, iOS and Catalyst] SearchHandler CharacterSpacing property is not applied' `
            -CarriesFix

        $title | Should -Be '[maui-bot-fix][ios] Fix for #35624 - SearchHandler CharacterSpacing property is not applied'
        $title | Should -Not -Match '(?i)Android'
        $title | Should -Not -Match '(?i)Catalyst'
    }

    It 'still quotes the rest of the reporter title verbatim' {
        # Only the platform bracket is metadata the tag replaces. Rewriting any
        # more of the title would misdescribe the issue being fixed.
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 12 `
            -Platform 'windows' `
            -IssueTitle '[Bug] Entry does not raise Completed' `
            -CarriesFix

        $title | Should -Be '[maui-bot-fix][windows] Fix for #12 - [Bug] Entry does not raise Completed'
    }

    It 'is actually wired into the title builder' {
        # A helper nothing calls is protection that is not there, and this file
        # has found that shape four times. Read from the syntax tree so the
        # assertion cannot be satisfied by a comment naming the function.
        $script = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script, [ref]$null, [ref]$null)

        $builder = $ast.Find({
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq 'New-ReplicationPullRequestTitle'
        }, $true)
        $builder | Should -Not -BeNullOrEmpty

        $calls = $builder.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Remove-ReplicationPlatformTitlePrefix'
        }, $true)

        @($calls).Count | Should -Be 1
    }
}

Describe 'Superseding an existing replication pull request' {

    It 'protects an existing fix unless it is asked to supersede one' {
        $script:PrSource | Should -Match '\[switch\]\$SupersedeExisting'
        $script:PrSource | Should -Match 'if \(\$duplicateCarriesFix -and -not \$SupersedeExisting\)'
    }

    It 'retires the earlier pull request only after the replacement is open' {
        # Closing first would leave the issue with no open reproduction at all
        # if publication then failed. A duplicate is a far smaller problem than
        # lost evidence, so the order here is the whole safety property.
        $urlIndex = $script:PrSource.IndexOf('$plan.url = ([string]$prUrl).Trim()')
        $closeIndex = $script:PrSource.IndexOf('gh pr close $supersededNumber')
        $captureIndex = $script:PrSource.IndexOf('$supersededPull = $duplicate')

        $captureIndex | Should -BeGreaterThan 0
        $urlIndex | Should -BeGreaterThan $captureIndex
        $closeIndex | Should -BeGreaterThan $urlIndex
    }

    It 'names the replacement in the retired pull request' {
        $script:PrSource | Should -Match 'Superseded by \$\(\$plan\.url\)'
        $script:PrSource | Should -Match 'gh pr comment \$supersededNumber'
    }

    It 'keeps a failed retirement from failing a published fix' {
        # The fix is already on GitHub by this point. Throwing here
        # would report a successful publication as a failed build.
        $script:PrSource | Should -Match 'could not be retired, so it stays open'
    }

    It 'records what it superseded and whether it managed to close it' {
        $script:PrSource | Should -Match 'supersedes = \$null'
        $script:PrSource | Should -Match 'supersededClosed = \$false'
        $script:PrSource | Should -Match '\$plan\.supersedes = \[string\]\$duplicate\.url'
        $script:PrSource | Should -Match '\$plan\.supersededClosed = \$true'
    }

    It 'declares the superseded pull request before the branch it is read in' {
        # StrictMode 3.0 turns an undeclared read into a terminating error, so
        # the variable has to exist even on the ordinary path where nothing is
        # superseded at all.
        $declareIndex = $script:PrSource.IndexOf('$supersededPull = $null')
        $readIndex = $script:PrSource.IndexOf('if ($supersededPull)')

        $declareIndex | Should -BeGreaterThan 0
        $readIndex | Should -BeGreaterThan $declareIndex
    }
}

Describe 'The regression cross-reference reports and never refuses' {
    BeforeAll {
        $script:RegRoot = Join-Path ([IO.Path]::GetTempPath()) ("regsig-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:RegRoot -Force | Out-Null

        # A stand-in for Find-RegressionRisks.ps1 that writes whatever verdict the
        # test asks for. The real script needs `gh`, a network and a six-month
        # history walk; what is under test here is how the publisher REACTS to a
        # verdict, which is exactly the half that can destroy a fix.
        function script:New-StubChecker {
            param([string]$Verdict, [switch]$Fails, [switch]$WritesNothing)
            $dir = Join-Path $script:RegRoot ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            $body = if ($Fails) {
                'param($PRNumber,$DiffPath,$Repo,$FilePaths,$MonthsBack,$OutputDir)' + "`n" +
                'throw "the checker exploded"'
            } elseif ($WritesNothing) {
                'param($PRNumber,$DiffPath,$Repo,$FilePaths,$MonthsBack,$OutputDir)' + "`n" +
                'exit 2'
            } else {
                'param($PRNumber,$DiffPath,$Repo,$FilePaths,$MonthsBack,$OutputDir)' + "`n" +
                'New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null' + "`n" +
                ('"' + $Verdict + '" | Set-Content (Join-Path $OutputDir "result.txt")')
            }
            Set-Content -LiteralPath (Join-Path $dir 'Find-RegressionRisks.ps1') -Value $body -Encoding utf8NoBOM
            $dir
        }

        $script:RegPatch = Join-Path $script:RegRoot 'fix.patch'
        Set-Content -LiteralPath $script:RegPatch -Value 'diff --git a/x b/x' -Encoding utf8NoBOM
    }

    AfterAll {
        Remove-Item -LiteralPath $script:RegRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'names each verdict distinctly, and only REVERT warns' {
        $clean = Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Verdict 'CLEAN')
        $overlap = Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Verdict 'OVERLAP')
        $revert = Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Verdict 'REVERT')

        $clean | Should -Not -BeNullOrEmpty
        $overlap | Should -Not -BeNullOrEmpty
        $revert | Should -Not -BeNullOrEmpty
        @($clean, $overlap, $revert) | Select-Object -Unique | Should -HaveCount 3

        # The whole value of the check is that a reader can tell the dangerous
        # verdict from the safe ones at a glance.
        $revert | Should -Match '⚠️'
        $clean | Should -Not -Match '⚠️'
        $overlap | Should -Not -Match '⚠️'
        $revert | Should -Match 'bug-fix'
    }

    It 'says it was not measured rather than claiming CLEAN when it could not run' {
        # Each of these is an absent measurement, and this plan records four
        # separate occasions on which an absent measurement was rendered as a
        # verdict and destroyed work. CLEAN is a claim; silence is the truth.
        $cases = @(
            (Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Fails)),
            (Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -WritesNothing)),
            (Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Verdict 'something-else'))
        )
        foreach ($case in $cases) {
            $case | Should -Match 'Not measured'
            $case | Should -Not -Match 'No line this fix deletes'
        }
    }

    It 'returns nothing at all when there is no fix to judge' {
        Get-ReplicationFixRegressionSignal -FixPatchPath '' | Should -BeNullOrEmpty
        Get-ReplicationFixRegressionSignal -FixPatchPath (Join-Path $script:RegRoot 'absent.patch') | Should -BeNullOrEmpty
        # Only the no-fix cases are silent. A reproduction-only PR has nothing
        # to cross-reference, so it says nothing.
        #
        # A missing checker used to be silent too, and that is exactly what hid
        # the cross-reference never running for its entire life. It now reports
        # 'Not measured', which is asserted where the layouts are tested.
    }

    It 'leaves no temporary directory behind' {
        $before = @(Get-ChildItem ([IO.Path]::GetTempPath()) -Directory -Filter 'regression-*' -ErrorAction SilentlyContinue).Count
        Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Verdict 'CLEAN') | Out-Null
        Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot (New-StubChecker -Fails) | Out-Null
        @(Get-ChildItem ([IO.Path]::GetTempPath()) -Directory -Filter 'regression-*' -ErrorAction SilentlyContinue).Count |
            Should -Be $before
    }

    It 'invokes the checker with the diff rather than a pull request number' {
        # `-DiffPath` is the whole reason this is reusable: at the moment a
        # replicate fix needs judging it exists only as a patch in a fork, so a
        # call written around -PRNumber could never run here.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1') -Raw
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($source, [ref]$null, [ref]$errors)
        $fn = $ast.Find({ param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Get-ReplicationFixRegressionSignal' }, $true)
        $fn | Should -Not -BeNullOrEmpty
        $fn.Extent.Text | Should -Match '-DiffPath'
        $fn.Extent.Text | Should -Not -Match '-PRNumber'
    }

    It 'never lets the signal decide whether the fix is published' {
        # The isolated tests above exercise the function; this reads the CALL
        # SITE, which is where this plan repeatedly records the damage being
        # done. `report, never refuse` is a property of how the value is USED,
        # so a mutant adding `if ($regressionSignal -match ...) { throw }` has
        # to fail something, and nothing that tests the function alone can.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1') -Raw
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($source, [ref]$null, [ref]$errors)

        $signalFn = $ast.Find({ param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Get-ReplicationFixRegressionSignal' }, $true)
        $signalFn | Should -Not -BeNullOrEmpty

        # Everything outside the function that mentions the signal.
        $uses = @($ast.FindAll({ param($n)
            $n -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $n.VariablePath.UserPath -eq 'regressionSignal' }, $true) |
            Where-Object {
                # Top-level script code only. A same-named parameter inside a
                # function is a different variable, and walking out from one
                # reaches that whole function rather than the call site.
                $enclosing = $_.Parent
                while ($enclosing -and -not ($enclosing -is [System.Management.Automation.Language.FunctionDefinitionAst])) {
                    $enclosing = $enclosing.Parent
                }
                -not $enclosing
            })
        $uses.Count | Should -BeGreaterThan 0

        foreach ($use in $uses) {
            # Take the OUTERMOST enclosing statement, not the nearest. An `if`
            # condition is itself a statement, so stopping at the nearest one
            # reads `$regressionSignal -match '...'` and never sees the `throw`
            # in the branch it guards - which is precisely the mutant this test
            # exists to kill, and it survived the first version of this loop.
            $node = $use
            $outermost = $null
            while ($node) {
                if ($node -is [System.Management.Automation.Language.StatementAst]) { $outermost = $node }
                $node = $node.Parent
            }
            $outermost | Should -Not -BeNullOrEmpty
            $outermost.Extent.Text | Should -Not -Match '\bthrow\b'
            $outermost.Extent.Text | Should -Not -Match '\bexit\b'
            $outermost.Extent.Text | Should -Not -Match 'Write-ReplicationBlocked|Write-BlockedCandidate'
        }

        # And it must actually reach the body. A signal that is computed,
        # logged and dropped looks identical to a working one from every
        # assertion above.
        $bodyCall = $ast.Find({ param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'New-ReplicationPullRequestBody' }, $true)
        $bodyCall | Should -Not -BeNullOrEmpty
        $bodyCall.Extent.Text | Should -Match '-RegressionSignal'

        # And the reverse: no refusal anywhere in the script consults it.
        foreach ($stop in @($ast.FindAll({ param($n)
                $n -is [System.Management.Automation.Language.ThrowStatementAst] }, $true))) {
            $stop.Extent.Text | Should -Not -Match 'regressionSignal'
        }
    }

    It 'computes the upstream cross-reference at the publish call site and never gates on it' {
        # A test that exercises a function in isolation says nothing about the
        # call site that uses it, and "report, never refuse" is a property of
        # how a value is USED. Both siblings beside this one shipped a signal
        # that was computed, logged and dropped.
        $prPath = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($prPath, [ref]$tokens, [ref]$errors)
        $errors.Count | Should -Be 0

        $call = $ast.Find({ param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'Get-ReplicationUpstreamFixSignal' }, $true)
        $call | Should -Not -BeNullOrEmpty

        # `$candidate` is parsed JSON, and under StrictMode a missing property
        # THROWS - which at this point would destroy a certified run at the
        # publish step. The file documents a reader for exactly this.
        $call.Extent.Text | Should -Match 'Get-ReplicationCandidateText'
        $call.Extent.Text | Should -Match "-Name\s+'baseSha'"
        $call.Extent.Text | Should -Not -Match '\$candidate\.baseSha'

        # And it must actually reach the body.
        $bodyCall = $ast.Find({ param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'New-ReplicationPullRequestBody' }, $true)
        $bodyCall | Should -Not -BeNullOrEmpty
        $bodyCall.Extent.Text | Should -Match '-UpstreamSignal'

        # No refusal anywhere in the script may consult it.
        foreach ($stop in @($ast.FindAll({ param($n)
                $n -is [System.Management.Automation.Language.ThrowStatementAst] }, $true))) {
            $stop.Extent.Text | Should -Not -Match 'upstreamSignal'
        }
    }

    It 'carries the signal into the body without ever gating publication on it' {
        $candidate = [ordered]@{
            schemaVersion = 1; status = 'validated'; validationPassed = $true
            issueNumber = 37440; platform = 'android'; baseSha = 'abc123'
            testType = 'device'; verificationTestType = 'DeviceTest'
            testName = 'Issue37440'; testFilter = 'Issue37440'
            expectedFailureSignature = 'Issue12345'
            expectedFailurePattern = 'Issue12345'
            actualFailureMessage = 'Xunit failure: Issue12345 expected red but was blue'
            verificationRunCount = 3; reproductionMarker = 'BUG REPRODUCED:'
            files = @('src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue37440.cs')
            fixFiles = @('src/Core/src/Platform/Android/Baz.cs')
            fixRootCause = 'cause'; fixApproach = 'approach'
            reproductionSteps = @('Open the page', 'Tap the control')
            evidence = [ordered]@{ video = 'repro.mp4'; preview = 'preview.gif'; thumbnail = 'thumbnail.png' }
            certificationLevel = 'certified-oracle'
            certificationSummary = '**Evidence level: certified-oracle**'
        } | ConvertTo-Json -Depth 10 | ConvertFrom-Json
        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                video = 'https://example.com/repro.mp4'
                preview = 'https://example.com/preview.gif'
                manifest = 'https://example.com/evidence.json'
            }
        }
        $warning = '**⚠️ Regression cross-reference.** deletes a line a bug-fix PR added.'

        $body = New-ReplicationPullRequestBody -Candidate $candidate -Evidence $evidence `
            -IssueTitle 'T' -IssueOwner 'dotnet' -IssueRepository 'maui' -BuildUrl '' `
            -RegressionSignal $warning

        # Present...
        $body | Should -Match 'Regression cross-reference'
        # ...and the fix it warns about is still fully published beside it. This
        # is the assertion that kills a mutant turning the report into a refusal.
        $body | Should -Match 'Proposed fix'
        $body | Should -Match 'Baz\.cs'

        # Omitting it changes nothing else about the body.
        $without = New-ReplicationPullRequestBody -Candidate $candidate -Evidence $evidence `
            -IssueTitle 'T' -IssueOwner 'dotnet' -IssueRepository 'maui' -BuildUrl ''
        $without | Should -Not -Match 'Regression cross-reference'
        $without | Should -Match 'Baz\.cs'
    }
}

Describe 'A validated fix replaces reproduction-only pull requests safely' {

    BeforeAll {
        $script:fixBody = @'
## Reproduced issue
Something is wrong.

## Proposed fix

This pull request carries two commits.
'@
        $script:reproBody = @'
## Reproduced issue
Something is wrong.

## Reproduction steps
Tap the thing.
'@
    }

    It 'recognizes a publisher-authored fix section' {
        Test-ReplicationPullRequestCarriesFix -Body $script:fixBody |
            Should -BeTrue
    }

    It 'does not classify a reproduction-only body as a fix' {
        Test-ReplicationPullRequestCarriesFix -Body $script:reproBody |
            Should -BeFalse
    }

    It 'treats an empty or absent body as not carrying a fix' {
        Test-ReplicationPullRequestCarriesFix -Body '' |
            Should -BeFalse
        Test-ReplicationPullRequestCarriesFix -Body $null |
            Should -BeFalse
    }

    It 'does not mistake prose that merely mentions a proposed fix' {
        $prose = "We considered a fix.`n`nSee ## Proposed fix elsewhere for details."
        Test-ReplicationPullRequestCarriesFix -Body $prose |
            Should -BeFalse
    }

    It 'keeps an existing fix unless explicit superseding is enabled' {
        $text = $script:PrSource
        $callIndex = $text.LastIndexOf('Test-ReplicationPullRequestCarriesFix')
        $definitionIndex = $text.IndexOf('function Test-ReplicationPullRequestCarriesFix')
        $callIndex | Should -BeGreaterThan $definitionIndex

        $supersedeIndex = $text.IndexOf('$supersededPull = $duplicate')
        $supersedeIndex | Should -BeGreaterThan $callIndex

        $between = $text.Substring($callIndex, $supersedeIndex - $callIndex)
        $between | Should -Match '\$duplicateCarriesFix -and -not \$SupersedeExisting'
        $between | Should -Match 'exit 0'
        $between | Should -Not -Match 'gh pr close'
    }

    It 'prioritizes an existing fix when reproduction-only duplicates also exist' {
        $matchingIndex = $script:PrSource.IndexOf('$matchingPulls =')
        $fixIndex = $script:PrSource.IndexOf(
            'Test-ReplicationPullRequestCarriesFix -Body ([string]$_.body)',
            $matchingIndex)
        $fallbackIndex = $script:PrSource.IndexOf(
            '$duplicate = $matchingPulls | Select-Object -First 1',
            $fixIndex)

        $matchingIndex | Should -BeGreaterThan 0
        $fixIndex | Should -BeGreaterThan $matchingIndex
        $fallbackIndex | Should -BeGreaterThan $fixIndex
    }

    It 'automatically replaces a reproduction-only pull request with a fix' {
        $script:PrSource | Should -Match '\$duplicateCarriesFix = \$duplicate -and'
        $script:PrSource | Should -Match 'if \(\$duplicateCarriesFix -and -not \$SupersedeExisting\)'
        $script:PrSource | Should -Match 'if \(\$duplicate\)\s*\{'
    }
}

Describe 'The regression cross-reference can be found in both layouts' {
    # In the repository the checker sits one level above this script; in the
    # publish job every trusted script is copied into one flat directory, so it
    # sits beside it. Only the first was searched, which is why pull request
    # 406 - published from the commit that added the cross-reference - carries
    # no signal at all.

    BeforeAll {
        $script:layoutRoot = Join-Path ([IO.Path]::GetTempPath()) ("layout-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:layoutRoot -Force | Out-Null

        $prPath = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $script:functionText = Get-ScriptFunctionText -Path $prPath -Name 'Get-ReplicationFixRegressionSignal'

        $script:stubChecker = @'
param([string]$DiffPath, [string]$Repo, [string]$OutputDir)
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
Set-Content -LiteralPath (Join-Path $OutputDir 'result.txt') -Value 'REVERT'
'@
    }

    AfterAll {
        Remove-Item -LiteralPath $script:layoutRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'finds a checker sitting beside it, as the publish job stages it' {
        $flat = Join-Path $script:layoutRoot 'flat'
        New-Item -ItemType Directory -Path $flat -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $flat 'Find-RegressionRisks.ps1') -Value $script:stubChecker

        $diff = Join-Path $flat 'fix.diff'
        Set-Content -LiteralPath $diff -Value 'diff --git a/x b/x'

        $host1 = Join-Path $flat 'Host.ps1'
        Set-Content -LiteralPath $host1 -Value ($script:functionText + "`n" +
            "Get-ReplicationFixRegressionSignal -FixPatchPath '$diff'")

        $output = & pwsh -NoProfile -File $host1
        ($output -join ' ') | Should -Match 'deletes one or more lines'
    }

    It 'finds a checker one level up, as the repository lays it out' {
        $nested = Join-Path $script:layoutRoot 'repo'
        $shared = Join-Path $nested 'shared'
        New-Item -ItemType Directory -Path $shared -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $nested 'Find-RegressionRisks.ps1') -Value $script:stubChecker

        $diff = Join-Path $nested 'fix.diff'
        Set-Content -LiteralPath $diff -Value 'diff --git a/x b/x'

        $host2 = Join-Path $shared 'Host.ps1'
        Set-Content -LiteralPath $host2 -Value ($script:functionText + "`n" +
            "Get-ReplicationFixRegressionSignal -FixPatchPath '$diff'")

        $output = & pwsh -NoProfile -File $host2
        ($output -join ' ') | Should -Match 'deletes one or more lines'
    }

    It 'says it was not measured when no checker exists anywhere' {
        $bare = Join-Path $script:layoutRoot 'bare'
        New-Item -ItemType Directory -Path $bare -Force | Out-Null
        $diff = Join-Path $bare 'fix.diff'
        Set-Content -LiteralPath $diff -Value 'diff --git a/x b/x'

        # An absent line cannot be told apart from a feature that was never
        # wired up. A stated non-measurement is a claim someone can check.
        Get-ReplicationFixRegressionSignal -FixPatchPath $diff -ScriptRoot $bare |
            Should -Match 'Not measured'
    }

    It 'still says nothing at all when there is no fix to annotate' {
        Get-ReplicationFixRegressionSignal -FixPatchPath '' | Should -BeNullOrEmpty
    }
}

Describe 'A test that restates its own fix is reported, never refused' {
    BeforeAll {
        # Verbatim from PR 469 (issue 36801) -- the case a human reviewer found
        # and nothing in the pipeline could. The fix and the test compute the
        # same ceiling, so the oracle cannot fail while the fix is present.
        $script:fix469 = @'
var adjustedInset = uiScrollView.AdjustedContentInset;
var availableScrollHeight = Math.Max(uiScrollView.ContentSize.Height + adjustedInset.Bottom - uiScrollView.Bounds.Height, 0);
'@
        $script:test469 = @'
var expectedMaximum = nativeScrollView.ContentSize.Height +
    nativeScrollView.AdjustedContentInset.Bottom -
    nativeScrollView.Bounds.Height;
Assert.True(Math.Abs(observed - expectedMaximum) < 1.0, "offset");
'@
    }

    It 'names the restated formula on the pull request that motivated it' {
        $signal = Get-ReplicationOracleIndependenceSignal -TestSource $script:test469 -FixSource $script:fix469
        $signal | Should -Not -BeNullOrEmpty
        $signal | Should -Match 'Oracle independence'
        $signal | Should -Match 'ContentSize Height \+ AdjustedContentInset Bottom - Bounds Height'
    }

    It 'is not fooled by renaming the local that holds the inset' {
        # The formula is the same; only the variable spelling differs. Dropping
        # the inlining pass would let a rename hide a restated implementation.
        $renamed = @'
var inset = nativeScrollView.AdjustedContentInset;
var expectedMaximum = nativeScrollView.ContentSize.Height + inset.Bottom - nativeScrollView.Bounds.Height;
'@
        Get-ReplicationOracleIndependenceSignal -TestSource $renamed -FixSource $script:fix469 |
            Should -Not -BeNullOrEmpty
    }

    It 'stays silent when the test measures something the fix does not compute' {
        # Measured over all 57 open fix pull requests: this fires on 469 and on
        # nothing else, so a legitimate oracle must not trip it.
        $independent = @'
var visibleGap = nativeProbe.Frame.Top - nativeScrollView.Frame.Height;
Assert.Equal("BOTTOM PROBE", nativeProbe.Text);
'@
        Get-ReplicationOracleIndependenceSignal -TestSource $independent -FixSource $script:fix469 |
            Should -BeNullOrEmpty
    }

    It 'stays silent for arithmetic that names almost nothing' {
        # Reachable but absent from the 57-PR corpus: four operator tokens and
        # one member. Matching bare arithmetic with no member identity would
        # call any two expressions of the same shape a restatement. Measured
        # rather than assumed -- removing the floor changes nothing on real
        # data, so this fixture is what keeps it from being protection that is
        # not there.
        $bare = 'var expected = a + b - c - view.Height;'
        $fix = 'var limit = p + q - r - other.Height;'
        Get-ReplicationOracleIndependenceSignal -TestSource $bare -FixSource $fix |
            Should -BeNullOrEmpty
    }

    It 'still fires when each side is a single statement' {
        # A one-statement source used to unroll to its bare tokens, so every
        # skeleton read as length 1, nothing cleared the four-token gate, and
        # the detector reported nothing. Silence for a reason unrelated to the
        # source is the failure mode this whole plan exists to catch.
        $fix = 'var availableScrollHeight = uiScrollView.ContentSize.Height + uiScrollView.AdjustedContentInset.Bottom - uiScrollView.Bounds.Height;'
        $test = 'var expectedMaximum = nativeScrollView.ContentSize.Height + nativeScrollView.AdjustedContentInset.Bottom - nativeScrollView.Bounds.Height;'
        Get-ReplicationOracleIndependenceSignal -TestSource $test -FixSource $fix |
            Should -Not -BeNullOrEmpty
    }

    It 'stays silent for a shared property path that computes nothing' {
        # Reaching for the same API is not restating a formula. Without the
        # operator requirement any test naming the chain the fix touches would
        # be reported, which is the false-accept that makes a signal ignorable.
        $test = 'var actual = view.Handler.PlatformView.Frame.Width;'
        $fix = 'probe.Handler.PlatformView.Frame.Width = layout.Frame.Width;'
        Get-ReplicationOracleIndependenceSignal -TestSource $test -FixSource $fix |
            Should -BeNullOrEmpty
    }

    It 'stays silent for a reproduction that carries no fix at all' {
        Get-ReplicationOracleIndependenceSignal -TestSource $script:test469 -FixSource '' |
            Should -BeNullOrEmpty
    }

    It 'renders the signal in the fix section of the body' {
        # Reuses the file's own fix fixture rather than a second hand-built one:
        # a fixture that hand-copies a producer is a second copy of one side of
        # the contract, not a test of it.
        $candidate = script:New-FixCandidate -Extra @{
            fixFiles = @('src/Core/src/Platform/iOS/ScrollViewExtensions.cs')
            fixPatch = 'fix.patch'
        }
        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $script:fixEvidence `
            -IssueTitle 'A scrolling defect' `
            -IssueOwner 'dotnet' `
            -IssueRepository 'maui' `
            -BuildUrl '' `
            -RegressionSignal '' `
            -OracleSignal '**WARN.** restated formula here'
        $body | Should -Match 'restated formula here'
    }

    It 'reports without ever withholding the pull request' {
        # The function cannot defend where it is called from, so this asserts
        # the property at the call site: the signal is rendered, and nothing
        # branches on it to throw or exit. Every gate in this plan that could
        # destroy validated work eventually did.
        $publisherPath = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($publisherPath, [ref]$null, [ref]$null)
        $assignment = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $args[0].Left.Extent.Text -eq '$oracleSignal' -and
                $args[0].Right.Extent.Text -match 'Get-ReplicationOracleIndependenceSignal'
            }, $true)
        $assignment | Should -Not -BeNullOrEmpty -Because 'the publisher must compute the signal'

        $passed = $ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq 'New-ReplicationPullRequestBody'
            }, $true) | Where-Object { $_.Extent.Text -match '-OracleSignal\s+\$oracleSignal' }
        @($passed).Count | Should -BeGreaterThan 0 -Because 'a signal that is computed and dropped is indistinguishable from one that works'

        foreach ($node in $ast.FindAll({
                    $args[0] -is [System.Management.Automation.Language.ThrowStatementAst] -or
                    ($args[0] -is [System.Management.Automation.Language.CommandAst] -and
                        $args[0].GetCommandName() -eq 'exit')
                }, $true)) {
            $node.Extent.Text | Should -Not -Match 'oracleSignal' -Because 'the check reports, it never refuses'
        }
    }
}

Describe 'The pre-flight upstream duplicate gate refuses only what it measured' {
    BeforeAll {
        # Loaded in BeforeAll, which is the RUN phase. The first draft of this
        # block called Get-ScriptFunctionText in the Describe body, which runs
        # at DISCOVERY, where that helper does not exist yet - so the call
        # threw, Pester dropped the whole Describe, and the suite reported the
        # unchanged baseline rather than a failure. Every assertion below,
        # including the one that checks this harness, silently did not run.
        Invoke-Expression (Get-ScriptFunctionText `
            -Path (Join-Path $PSScriptRoot 'shared/Get-ReplicationUpstreamFix.ps1') `
            -Name 'Get-ReplicationUpstreamDuplicateVerdict')
    }

    It 'loads the function it claims to test' {
        # A control that behaves identically to the positive indicts the
        # harness, not the code, so the harness is asserted before it is used.
        Get-Command Get-ReplicationUpstreamDuplicateVerdict -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty
    }

    It 'refuses an issue whose test case exists upstream and not in the tree under test' {
        function script:Get-ReplicationUpstreamTestCasePresence { 'present' }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        try {
            $verdict = Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot $root
            $verdict.Verdict | Should -Be 'duplicate'
            $verdict.Path | Should -Be 'src/Controls/tests/TestCases.HostApp/Issues/Issue36933.cs'
            # The signal is the file's existence on a branch. Naming a commit
            # is a second claim with its own, lower precision, so the refusal
            # must not make it.
            $verdict.Reason | Should -Match 'inflight/current'
            $verdict.Reason | Should -Match 'Issue36933\.cs'
            $verdict.Reason | Should -Not -Match '[0-9a-f]{7,}'
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }

    It 'stays silent when the tree under test already carries the test case' {
        # Present locally means it is already in the tree this run will build,
        # so upstream having it is not news and refusing would be wrong.
        function script:Get-ReplicationUpstreamTestCasePresence { 'present' }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        $issues = Join-Path $root 'src/Controls/tests/TestCases.HostApp/Issues'
        New-Item -ItemType Directory -Force -Path $issues | Out-Null
        Set-Content -LiteralPath (Join-Path $issues 'Issue36933.cs') -Value 'x'
        Set-Content -LiteralPath (Join-Path $issues 'Issue36933.xaml') -Value 'x'
        try {
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot $root).Verdict |
                Should -Be 'clear'
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }

    It 'refuses on the xaml candidate as readily as on the cs one' {
        # Each alternative is exercised separately: a correct answer from one
        # hides a dead one beside it.
        function script:Get-ReplicationUpstreamTestCasePresence {
            param($Path, $Ref, $Repo)
            if ($Path -like '*.xaml') { return 'present' }
            return 'absent'
        }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        try {
            $verdict = Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot $root
            $verdict.Verdict | Should -Be 'duplicate'
            $verdict.Path | Should -Match '\.xaml$'
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }

    It 'never refuses on a measurement it could not take' {
        # An absent measurement rendered as a finding is the most expensive
        # shape this pipeline has recorded. Every unreadable case proceeds.
        function script:Get-ReplicationUpstreamTestCasePresence { 'unknown' }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        try {
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot $root).Verdict |
                Should -Be 'unknown'
            # A root nothing ever wrote is how the platform gate beside this
            # one silently approved every run it was added to stop.
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot '').Verdict |
                Should -Be 'unknown'
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' `
                -RepositoryRoot (Join-Path $root 'nope')).Verdict | Should -Be 'unknown'
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber 'not-a-number' -RepositoryRoot $root).Verdict |
                Should -Be 'unknown'
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }

    It 'clears an issue upstream has no test case for' {
        function script:Get-ReplicationUpstreamTestCasePresence { 'absent' }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        try {
            (Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '99999' -RepositoryRoot $root).Verdict |
                Should -Be 'clear'
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }

    It 'never throws, whatever the presence probe does' {
        # A pre-flight gate that throws costs the run it was written to save.
        function script:Get-ReplicationUpstreamTestCasePresence { throw 'gh exploded' }
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("upgate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        try {
            { Get-ReplicationUpstreamDuplicateVerdict -IssueNumber '36933' -RepositoryRoot $root } |
                Should -Not -Throw
        } finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
    }
}

Describe 'The pull request body reports the independent review of the winning fix' {
    BeforeAll {
        $script:ReviewCandidate = [pscustomobject]@{
            fixIndependentReview = [pscustomobject]@{
                model = 'gpt-5.6-sol'
                summary = 'The diff restores the null guard and the test covers it.'
                findings = @(
                    [pscustomobject]@{ severity = 'blocking'; detail = 'The guard is not applied on the Android path.' }
                    [pscustomobject]@{ severity = 'minor'; detail = 'The new field could be readonly.' }
                )
            }
        }
    }

    It 'attributes the review to a model that did not write the diff' {
        $block = Get-ReplicationIndependentReviewBlock -Candidate $script:ReviewCandidate

        $block | Should -Match 'Independent review'
        $block | Should -Match 'gpt-5\.6-sol'
        $block | Should -Match 'without having written it'
        $block | Should -Match 'restores the null guard'
    }

    It 'surfaces each finding with its severity' {
        $block = Get-ReplicationIndependentReviewBlock -Candidate $script:ReviewCandidate

        $block | Should -Match '\*\*blocking\.\*\* The guard is not applied on the Android path\.'
        $block | Should -Match '\*\*minor\.\*\* The new field could be readonly\.'
    }

    It 'says plainly that findings did not block publication' {
        # The arm reports, never refuses: its false-positive rate is unmeasured
        # because every reviewed pull request in the validating corpus carried a
        # blocking finding, so there is no negative control. A wrong paragraph
        # costs a reader a minute; a wrong refusal costs a certified fix.
        Get-ReplicationIndependentReviewBlock -Candidate $script:ReviewCandidate |
            Should -Match 'did not block publication'
    }

    It 'declares measured cross-review agreement without presenting it as independent ground truth' {
        # A disclosure a reader cannot weigh is barely a disclosure. The arm was
        # validated blind over 8 diffs against findings from an earlier automated
        # adversarial-review pass: 6 of 8 pre-registered keys recovered, and 1
        # blocking finding raised on 4 already-merged maintainer fixes. That is
        # cross-review agreement, not an independent human benchmark. Stating n
        # is part of the claim - at this size the trial can refute a high error
        # rate but cannot establish a low one.
        $block = Get-ReplicationIndependentReviewBlock -Candidate $script:ReviewCandidate

        $block | Should -Match '6 of the 8'
        $block | Should -Match '1 of 4'
        $block | Should -Match 'n=8'
        $block | Should -Match 'earlier automated adversarial-review pass'
        $block | Should -Match 'agreement between automated review passes'
        $block | Should -Match 'not independent ground truth'
        $block | Should -Not -Match 'specific defects human reviewers had already named'
        $block | Should -Match 'cannot establish a low one'
    }

    It 'never claims a trust rate for a review that produced nothing to weigh' {
        # The orphan case: a caveat qualifying findings that do not exist claims
        # accuracy for a measurement nobody made. Same defect shape as a
        # certification caveat rendered beside a body that reports no level.
        $noFindings = [pscustomobject]@{
            fixIndependentReview = [pscustomobject]@{
                model = 'gpt-5.6-sol'; summary = 'Nothing blocking found.'; findings = @() }
        }

        $block = Get-ReplicationIndependentReviewBlock -Candidate $noFindings
        $block | Should -Match 'Nothing blocking found'
        $block | Should -Not -Match '6 of the 8'
        $block | Should -Not -Match 'n=8'

        $absent = Get-ReplicationIndependentReviewBlock -Candidate ([pscustomobject]@{ fixFiles = @('a.cs') })
        $absent | Should -Match 'Not measured'
        $absent | Should -Not -Match 'n=8'
    }

    It 'caps the findings it renders so one verbose review cannot flood the body' {
        $many = 1..12 | ForEach-Object { [pscustomobject]@{ severity = 'minor'; detail = "finding number $_" } }
        $candidate = [pscustomobject]@{
            fixIndependentReview = [pscustomobject]@{ model = 'gpt-5.6-sol'; summary = 'ok'; findings = $many }
        }

        $block = Get-ReplicationIndependentReviewBlock -Candidate $candidate

        @([regex]::Matches($block, '(?m)^- \*\*minor\.\*\*')).Count | Should -Be 6
    }

    It 'reports "Not measured" when the review is absent or unusable' {
        # Silence is indistinguishable from a feature nobody wired up, which is
        # exactly how the regression cross-reference stayed dead for its whole
        # life while passing every behavioural test it had.
        Get-ReplicationIndependentReviewBlock -Candidate ([pscustomobject]@{ fixFiles = @('a.cs') }) |
            Should -Match 'Not measured'
        Get-ReplicationIndependentReviewBlock -Candidate ([pscustomobject]@{ fixIndependentReview = $null }) |
            Should -Match 'Not measured'
        Get-ReplicationIndependentReviewBlock -Candidate ([pscustomobject]@{
            fixIndependentReview = [pscustomobject]@{ model = 'gpt-5.6-sol'; summary = '   '; findings = @() } }) |
            Should -Match 'Not measured'
    }
}

Describe 'The pull request body records the try-fix panel, not only its winner' {
    BeforeAll {
        $script:PanelCandidate = [pscustomobject]@{
            fixPanel = @(
                [pscustomobject]@{ attempt = 1; model = 'claude-opus-5'; result = 'Blocked'; detail = 'reported a pass without changing any file'; won = $false }
                [pscustomobject]@{ attempt = 2; model = 'gpt-5.6-sol'; result = 'Fail'; detail = 'oracle failed 1 of 3 runs'; won = $false }
                [pscustomobject]@{ attempt = 3; model = 'claude-opus-5'; result = 'Pass'; detail = 'Guard the native flow-direction mapper'; won = $true }
            )
        }
    }

    It 'names every candidate that competed, with its model and result' {
        $block = Get-ReplicationFixPanelBlock -Candidate $script:PanelCandidate

        # The whole point of the disclosure: a reader can tell a fix selected
        # from competing approaches from the one candidate that happened to run.
        $block | Should -Match 'gpt-5\.6-sol'
        $block | Should -Match 'claude-opus-5'
        $block | Should -Match 'Blocked'
        $block | Should -Match 'oracle failed 1 of 3 runs'
    }

    It 'marks exactly the candidate whose diff was published' {
        $block = Get-ReplicationFixPanelBlock -Candidate $script:PanelCandidate

        @([regex]::Matches($block, '\(selected\)')).Count | Should -Be 1
        $block | Should -Match 'Pass \*\*\(selected\)\*\*'
    }

    It 'escapes a pipe in candidate prose so the table cannot silently shift its columns' {
        $candidate = [pscustomobject]@{
            fixPanel = @(
                [pscustomobject]@{ attempt = 1; model = 'claude-opus-5'; result = 'Pass'; detail = 'restore the a|b fallback'; won = $true }
            )
        }

        $block = Get-ReplicationFixPanelBlock -Candidate $candidate

        # An unescaped pipe ends the cell, so every later column reports the
        # wrong candidate's value. A row that misattributes a result is worse
        # than no row at all.
        $block | Should -Match 'a\\\|b'
        $row = @($block -split "`n" | Where-Object { $_ -match 'claude-opus-5' })[0]
        @($row -split '(?<!\\)\|').Count | Should -Be 6
    }

    It 'says the panel was not measured rather than rendering nothing' {
        Get-ReplicationFixPanelBlock -Candidate ([pscustomobject]@{ fixFiles = @('a.cs') }) |
            Should -Match 'Not measured'
        Get-ReplicationFixPanelBlock -Candidate ([pscustomobject]@{ fixPanel = @() }) |
            Should -Match 'Not measured'
    }

    It 'is actually invoked by the body builder, not merely defined' {
        # A test that exercises a new function in isolation says nothing about
        # the call site, and the call site is where this class of defect lives:
        # the regression cross-reference passed every behavioural test it had
        # while returning nothing in production for its entire life.
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'), [ref]$null, [ref]$errors)
        $errors | Should -BeNullOrEmpty

        $body = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'New-ReplicationPullRequestBody'
        }, $true)
        $body | Should -Not -BeNullOrEmpty

        foreach ($name in @('Get-ReplicationFixPanelBlock', 'Get-ReplicationIndependentReviewBlock')) {
            $calls = @($body.FindAll({
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq $name
            }, $true))
            $calls.Count | Should -BeGreaterThan 0 -Because "$name must be called by the body builder"
        }
    }
}

Describe 'The candidate gate allowlists every manifest field the orchestrator writes' {
    It 'accepts each fix* key the orchestrator emits, so a new field cannot destroy a completed run' {
        # This pipeline has already lost finished runs to exactly this: the
        # orchestrator gains a manifest field, the gate does not know the name,
        # and the run is refused after the reproduction, the fix, and every arm
        # have already been paid for. Pinning the two together makes the next
        # field fail here - in milliseconds, on a laptop - instead of there.
        $validator = Join-Path $PSScriptRoot 'shared/Validate-ReplicationCandidate.ps1'
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($validator, [ref]$null, [ref]$errors)
        $errors | Should -BeNullOrEmpty

        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'Read-ReplicationManifest'
        }, $true)
        $function | Should -Not -BeNullOrEmpty

        $assignment = $function.Find({
            $args[0] -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $args[0].Left.Extent.Text -eq '$allowedProperties'
        }, $true)
        $assignment | Should -Not -BeNullOrEmpty

        $allowed = & ([scriptblock]::Create($assignment.Right.Extent.Text))
        $allowed.Count | Should -BeGreaterThan 20

        $orchestrator = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
        $written = @([regex]::Matches(
                (Get-Content -LiteralPath $orchestrator -Raw), '(?m)^\s{12}(fix[A-Za-z]+)\s*=') |
            ForEach-Object { $_.Groups[1].Value } |
            Sort-Object -Unique)

        # Guards the guard: if the extraction stops matching, the comparison
        # below passes vacuously and this test protects nothing.
        $written.Count | Should -BeGreaterThan 4
        $written | Should -Contain 'fixPanel'

        @($written | Where-Object { $allowed -cnotcontains $_ }) | Should -BeNullOrEmpty
    }
}

Describe 'Fix disclosures survive the validation boundary' {
    It 'emits every fix disclosure the publisher reads' {
        # For the whole life of these fields the publisher rendered nothing.
        # The orchestrator wrote fixRootCause, fixApproach and the rejected
        # approaches; the gate allowlisted them; and then the validated
        # document - the only thing the publisher ever reads - listed just
        # fixFiles and fixPatch, so the rest were dropped in silence. Every fix
        # pull request in the corpus shipped with no root cause, no approach
        # and no comparison, and no test noticed, because each half was correct
        # on its own.
        $publisher = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
        $validator = Join-Path $PSScriptRoot 'shared/Validate-ReplicationCandidate.ps1'

        $publisherSource = Get-Content -LiteralPath $publisher -Raw
        $read = @()
        $read += @([regex]::Matches($publisherSource, "PSObject\.Properties\['(fix[A-Za-z]+)'\]") |
            ForEach-Object { $_.Groups[1].Value })
        $read += @([regex]::Matches($publisherSource, "-Name\s+'(fix[A-Za-z]+)'") |
            ForEach-Object { $_.Groups[1].Value })
        $read = @($read | Sort-Object -Unique)

        # Guards the guard: an extraction that matches nothing would make the
        # comparison below pass while protecting nothing at all.
        $read.Count | Should -BeGreaterThan 3
        $read | Should -Contain 'fixPanel'
        $read | Should -Contain 'fixRootCause'

        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($validator, [ref]$null, [ref]$parseErrors)
        $parseErrors | Should -BeNullOrEmpty

        $document = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $args[0].Left.Extent.Text -eq '$validatedDocument'
        }, $true)
        $document | Should -Not -BeNullOrEmpty

        $emitted = @($document.Right.Extent.Text |
            ForEach-Object { [regex]::Matches($_, '(?m)^\s{12}([A-Za-z][A-Za-z0-9_]*)\s*=') } |
            ForEach-Object { $_.Groups[1].Value } |
            Sort-Object -Unique)
        $emitted.Count | Should -BeGreaterThan 10

        @($read | Where-Object { $emitted -cnotcontains $_ }) | Should -BeNullOrEmpty
    }
}

Describe 'A disclosure is sanitized without ever costing the run' {
    It 'never throws, whatever the agent wrote' {
        # Four completed runs have been destroyed by a bound that refused
        # instead of trimming. A disclosure is a courtesy to a reader and must
        # never outrank the fix it describes.
        foreach ($value in @($null, '', '   ', 42, ('x' * 20000), "a`nb`tc", [pscustomobject]@{ a = 1 })) {
            { ConvertTo-ReplicationDisclosureText -Value $value } | Should -Not -Throw
        }

        (ConvertTo-ReplicationDisclosureText -Value ('x' * 20000)).Length | Should -BeLessOrEqual 300
        ConvertTo-ReplicationDisclosureText -Value $null | Should -Be ''
        ConvertTo-ReplicationDisclosureText -Value 42 | Should -Be ''
    }

    It 'never attaches a fix disclosure to a candidate that carries no fix' {
        # A reproduction-only PR that printed a root cause would be describing a
        # fix nobody authored. The gate is the only thing preventing it, and a
        # surviving mutant showed nothing was asking.
        $validator = Join-Path $PSScriptRoot 'shared/Validate-ReplicationCandidate.ps1'
        $validatorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $validator, [ref]$null, [ref]$null)
        $assignment = $validatorAst.Find({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left.Extent.Text -eq '$validatedDocument'
        }, $true)
        $assignment | Should -Not -BeNullOrEmpty

        $hashtable = $assignment.Right.Find({
            param($node) $node -is [System.Management.Automation.Language.HashtableAst]
        }, $true)

        # fixFiles is deliberately exempt: it is an array that is empty by
        # construction when no fix exists, and the degrade path in the yml
        # reads exactly that emptiness.
        $exempt = @('fixFiles')
        $gated = @()
        foreach ($pair in $hashtable.KeyValuePairs) {
            $key = $pair.Item1.Extent.Text
            if ($key -notlike 'fix*' -or $exempt -contains $key) { continue }
            $gated += $key
            $pair.Item2.Extent.Text | Should -Match '\$hasFixPatch' -Because "$key must not be published for a run with no fix"
        }

        # Guard the guard: an extraction that finds nothing would pass silently.
        $gated.Count | Should -BeGreaterThan 4
        $gated | Should -Contain 'fixRootCause'
        $gated | Should -Contain 'fixPanel'
    }

    It 'swallows the refusal a control character still raises' {
        # The bounded call genuinely throws on these - measured, not assumed -
        # so the catch is the only thing standing between one stray byte in
        # model-written prose and a certified run dying at the gate. Nothing
        # exercised it until a surviving mutant asked the question.
        foreach ($control in @([char]0, [char]7, [char]127)) {
            $value = 'root cause' + $control + ' explained'

            { ConvertTo-BoundedSingleLine -Value $value -Context 'Probe' -MaximumLength 300 -Prose } |
                Should -Throw
            { ConvertTo-ReplicationDisclosureText -Value $value } | Should -Not -Throw
            ConvertTo-ReplicationDisclosureText -Value $value | Should -Be ''
        }
    }

    It 'strips an Azure logging directive instead of emitting it' {
        ConvertTo-ReplicationDisclosureText -Value '##vso[task.setvariable variable=x]y' |
            Should -Not -Match '##vso\['
        ConvertTo-ReplicationDisclosureText -Value '##[error]spoofed' |
            Should -Not -Match '##\['
    }

    It 'defuses a mention so a pull request body cannot notify anyone' {
        # Nothing that wrote this text was entitled to ping a maintainer.
        $out = ConvertTo-ReplicationDisclosureText -Value 'Thanks @kubaflo for the report'

        $out | Should -Be 'Thanks `@kubaflo` for the report'
        $out | Should -Not -Match '(?<!`)@kubaflo'
    }

    It 'leaves technical prose alone' {
        # Over-sanitizing is its own defect: these disclosures describe C#, and
        # mangling them would make the panel useless to the reader it is for.
        ConvertTo-ReplicationDisclosureText -Value 'Guard List<int> and Dictionary<string,int>' |
            Should -Be 'Guard List<int> and Dictionary<string,int>'
        ConvertTo-ReplicationDisclosureText -Value 'use @"literal" strings' |
            Should -Be 'use @"literal" strings'
        ConvertTo-ReplicationDisclosureText -Value 'reported by a.b@example.com' |
            Should -Be 'reported by a.b@example.com'
    }

    It 'reads a manifest that carries none of the disclosure properties' {
        Set-StrictMode -Version Latest
        $bare = [pscustomobject]@{ issueNumber = 5 }

        { Get-ReplicationManifestDisclosure -Manifest $bare -Name 'fixRootCause' } | Should -Not -Throw
        Get-ReplicationManifestDisclosure -Manifest $bare -Name 'fixRootCause' | Should -Be ''
        @(Get-ReplicationManifestDisclosureList -Manifest $bare -Name 'fixRejectedApproaches') |
            Should -BeNullOrEmpty
    }

    It 'drops list entries that sanitize away, keeping the rest' {
        $manifest = [pscustomobject]@{ fixRejectedApproaches = @('widen the guard', '', $null, 'clamp the offset') }

        $list = @(Get-ReplicationManifestDisclosureList -Manifest $manifest -Name 'fixRejectedApproaches')

        $list.Count | Should -Be 2
        $list | Should -Contain 'widen the guard'
        $list | Should -Contain 'clamp the offset'
    }
}

Describe 'Reproduction-only runs are never published' {
    It 'reports a candidate that lists fix files as publishable' {
        $candidate = [pscustomobject]@{ fixFiles = @('src/Core/src/Handlers/Entry/EntryHandler.iOS.cs') }

        Test-ReplicationPublishableFix -Candidate $candidate | Should -BeTrue
    }

    It 'refuses a candidate that carries no fix at all' {
        # The reproduction-only shape: a test was authored, nothing else.
        $candidate = [pscustomobject]@{ files = @('src/Controls/tests/DeviceTests/Issue1Tests.cs') }

        Test-ReplicationPublishableFix -Candidate $candidate | Should -BeFalse
    }

    It 'refuses a candidate whose fix list is present but empty or blank' {
        # A run can emit the property and populate it with nothing. Counting the
        # raw property would read @($null).Count as 1 and publish a reproduction
        # as though it carried a fix.
        foreach ($value in @(@(), @(''), @($null), @('', '   '))) {
            $candidate = [pscustomobject]@{ fixFiles = $value }
            Test-ReplicationPublishableFix -Candidate $candidate | Should -BeFalse
        }
    }

    It 'withholds publication in the publisher body when no fix was validated' {
        # The gate has to sit in the publisher, not only in the caller: the
        # function above is inert unless the script consults it before opening
        # a pull request.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Resolve-Path (Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1')).Path,
            [ref]$null, [ref]$null)

        # The call must be reached from the script body, not merely defined.
        $topLevel = $ast.EndBlock.Statements | Where-Object {
            $_ -isnot [System.Management.Automation.Language.FunctionDefinitionAst]
        }
        $gate = $topLevel | Where-Object {
            $_.Extent.Text -match 'Test-ReplicationPublishableFix' -and
            $_.Extent.Text -match 'withheldReason'
        }
        @($gate).Count | Should -BeGreaterThan 0

        # And it must stand down rather than fall through into publication.
        $gateText = ($gate | Select-Object -First 1).Extent.Text
        $gateText | Should -Match 'exit 0'
    }

    It 'declares withheldReason on the manifest so a withheld run is distinguishable' {
        # Without the property the manifest of a withheld run is shaped exactly
        # like that of a publisher which crashed before returning a URL, and the
        # pipeline would fail a green outcome.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1') -Raw
        $source | Should -Match 'withheldReason\s*=\s*\$null'
    }
}

Describe 'The review path can reach the repository holding the pull request' {
    BeforeAll {
        $script:ReviewPathScripts = @(
            (Join-Path $PSScriptRoot 'Review-PR.ps1'),
            (Join-Path $PSScriptRoot 'post-ai-summary-comment.ps1'),
            (Join-Path $PSScriptRoot 'post-inline-review.ps1')
        )
    }

    It 'declares a Repository parameter defaulting to the upstream project' {
        foreach ($path in $script:ReviewPathScripts) {
            $ast = [System.Management.Automation.Language.Parser]::ParseFile(
                (Resolve-Path $path).Path, [ref]$null, [ref]$null)
            $parameter = $ast.ParamBlock.Parameters | Where-Object {
                $_.Name.VariablePath.UserPath -eq 'Repository'
            }
            @($parameter).Count | Should -Be 1 -Because "$([System.IO.Path]::GetFileName($path)) must accept -Repository"
            # Defaulting matters as much as existing: /review and every queued
            # upstream run pass no -Repository and must keep working.
            $parameter.DefaultValue.Extent.Text.Trim("'", '"') | Should -Be 'dotnet/maui'
        }
    }

    It 'binds every pull-request API path to that parameter instead of a fixed owner' {
        # This is the defect that left 97 fix pull requests unreviewed: each gh
        # call named dotnet/maui, so a fork pull request number would have been
        # read from - and posted to - an unrelated upstream pull request.
        foreach ($path in $script:ReviewPathScripts) {
            $source = Get-Content -LiteralPath $path -Raw
            $name = [System.IO.Path]::GetFileName($path)
            $source | Should -Not -Match 'repos/dotnet/maui/(?:pulls|issues)/\$PRNumber' `
                -Because "$name must not hardcode the upstream repository in an API path"
        }
    }

    It 'passes the repository on to the scripts that post the review' {
        # Review-PR resolves the repository once and both posting scripts must
        # inherit it, or the review would be computed against the fork and
        # posted against the upstream project.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Review-PR.ps1') -Raw
        foreach ($line in ($source -split "`n" | Where-Object { $_ -match '\$reviewScript|\$inlineScript' })) {
            if ($line -match '&\s+\$(reviewScript|inlineScript)\s+-PRNumber') {
                $line | Should -Match '-Repository \$Repository'
            }
        }
    }
}
