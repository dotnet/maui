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
        'Get-ValidatedFixFiles',
        'Assert-ReplicationStagedFix',
        'New-ReplicationPullRequestTitle',
        'New-ReplicationPullRequestBody',
        'Get-ReplicationFixRegressionSignal',
        'Resolve-ReplicationSourceRepository'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $prScript -Name $name)
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

    It 'uses an add-only staged diff and creates a MauiBot fork draft PR' {
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
            Should -Be '[maui-bot-fix] Fix for #36545 - Entry Completed fires twice'
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
            Should -Be '[maui-bot-fix] Fix for #42'
    }

    It 'still names the issue when its title is null' {
        New-ReplicationPullRequestTitle -IssueNumber 42 -Platform 'ios' -IssueTitle $null -CarriesFix |
            Should -Be '[maui-bot-fix] Fix for #42'
    }

    It 'strips newlines so an issue title cannot forge extra lines' {
        $title = New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle "harmless`nLGTM, merging" `
            -CarriesFix

        $title | Should -Not -Match "`n"
        $title | Should -Be '[maui-bot-fix] Fix for #7 - harmless LGTM, merging'
    }

    It 'strips control characters an issue title may carry' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 7 `
            -Platform 'ios' `
            -IssueTitle "a`tb" `
            -CarriesFix |
            Should -Be '[maui-bot-fix] Fix for #7 - a b'
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
            Should -Be "[maui-bot-fix] Fix for #7 - $summary"
    }

    It 'drops the summary entirely when there is no room for a meaningful one' {
        New-ReplicationPullRequestTitle `
            -IssueNumber 123456789 `
            -Platform 'ios' `
            -IssueTitle 'a summary that will not fit' `
            -CarriesFix `
            -MaxLength 40 |
            Should -Be '[maui-bot-fix] Fix for #123456789'
    }
}

Describe 'Superseding an existing reproduction pull request' {
    # Thirty-one certified reproductions reached the fix phase and every one of
    # them died in it, and none could be re-run afterwards: an open pull request
    # covered each issue and platform, so both the pre-check and the publisher
    # refused. Re-running an already-covered issue is the only way to test a
    # pipeline change against the reproductions that exercise it.

    It 'refuses a duplicate only while it is not asked to supersede one' {
        $script:PrSource | Should -Match '\[switch\]\$SupersedeExisting'
        $script:PrSource | Should -Match 'if \(\$duplicate -and -not \$SupersedeExisting\)'
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

    It 'keeps a failed retirement from failing a published reproduction' {
        # The reproduction is already on GitHub by this point. Throwing here
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
        # A checkout without the checker must publish exactly as it does today.
        $bare = Join-Path $script:RegRoot 'bare'
        New-Item -ItemType Directory -Path $bare -Force | Out-Null
        Get-ReplicationFixRegressionSignal -FixPatchPath $script:RegPatch -ScriptRoot $bare | Should -BeNullOrEmpty
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
