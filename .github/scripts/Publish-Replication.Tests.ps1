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
        'New-ReplicationPullRequestBody',
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
