#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:validatorPath = Join-Path $PSScriptRoot 'Validate-ReplicationCandidate.ps1'
    . $script:validatorPath

    $script:scratchRoot = Join-Path $PSScriptRoot '.Validate-ReplicationCandidate.Tests.work'
    if (Test-Path -LiteralPath $script:scratchRoot) {
        Remove-Item -LiteralPath $script:scratchRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Path $script:scratchRoot | Out-Null

    $script:successfulProbe = {
        param($Path, $Kind)

        switch ($Kind) {
            'mp4' {
                [pscustomobject]@{
                    IsDecodable = $true
                    FormatName = 'mov,mp4,m4a,3gp,3g2,mj2'
                    DurationSeconds = 4.25
                    Width = 720
                    Height = 1280
                }
            }
            'gif' {
                [pscustomobject]@{
                    IsDecodable = $true
                    FormatName = 'gif'
                    Width = 360
                    Height = 640
                }
            }
            'png' {
                [pscustomobject]@{
                    IsDecodable = $true
                    FormatName = 'png_pipe'
                    Width = 360
                    Height = 640
                }
            }
        }
    }

    function Write-TestText {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$Value
        )

        $parent = [System.IO.Path]::GetDirectoryName($Path)
        if (-not (Test-Path -LiteralPath $parent)) {
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
        }
        [System.IO.File]::WriteAllText(
            $Path,
            $Value,
            [System.Text.UTF8Encoding]::new($false)
        )
    }

    function Write-TestJson {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][object]$Value
        )

        Write-TestText `
            -Path $Path `
            -Value ($Value | ConvertTo-Json -Depth 8)
    }

    function New-TestSource {
        param(
            [Parameter(Mandatory = $true)][string]$TestType,
            [long]$IssueNumber = 12345,
            [string]$TestName = 'Issue12345',
            [string]$FailurePattern = 'Expected control to remain visible',
            [ValidateSet('android', 'ios', 'catalyst', 'windows')]
            [string]$Platform = 'android'
        )

        $framework = if ($TestType -ceq 'UITest') { 'NUnit.Framework' } else { 'Xunit' }
        $attribute = if ($TestType -ceq 'UITest') { 'Test' } else { 'Fact' }
        $assertion = if ($TestType -ceq 'UITest') {
            "Assert.Fail(`"$FailurePattern`");"
        } else {
            "Assert.True(false, `"$FailurePattern`");"
        }

        $body = @"
using $framework;

$(if ($TestType -ceq 'DeviceTest') { "[Category(`"Issue$IssueNumber`")]`n" })public class $TestName
{
    [$attribute]
    public void ReproducesIssue()
    {
        $assertion
    }
}
"@

        # DeviceTests and TestCases.Shared.Tests link-compile into every platform
        # assembly, so a reproduction there has to name the platform it proved.
        if ($TestType -cin @('DeviceTest', 'UITest')) {
            $symbol = @{
                android  = 'ANDROID'
                ios      = 'IOS'
                catalyst = 'MACCATALYST'
                windows  = 'WINDOWS'
            }[$Platform]
            return "#if $symbol`n$body#endif`n"
        }

        return $body
    }

    function New-AddOnlyPatch {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$Content
        )

        $normalized = $Content.Replace("`r`n", "`n").TrimEnd("`r", "`n")
        $contentLines = $normalized.Split([char]"`n")
        $patchLines = [System.Collections.Generic.List[string]]::new()
        $patchLines.Add("diff --git a/$Path b/$Path")
        $patchLines.Add('new file mode 100644')
        $patchLines.Add('index 0000000..1111111')
        $patchLines.Add('--- /dev/null')
        $patchLines.Add("+++ b/$Path")
        $patchLines.Add("@@ -0,0 +1,$($contentLines.Count) @@")
        foreach ($line in $contentLines) {
            $patchLines.Add("+$line")
        }

        return ($patchLines -join "`n") + "`n"
    }

    function Get-DefaultCandidatePath {
        param([Parameter(Mandatory = $true)][string]$TestType)

        switch ($TestType) {
            'UnitTest' { return 'src/Core/tests/UnitTests/Issue12345Tests.cs' }
            'XamlUnitTest' { return 'src/Controls/tests/Xaml.UnitTests/Issues/Issue12345Tests.cs' }
            'DeviceTest' { return 'src/Core/tests/DeviceTests/Handlers/Issue12345Tests.cs' }
            'UITest' { return 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issue12345Tests.cs' }
        }
    }

    function Write-FixtureManifest {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [hashtable]$Overrides
        )

        $manifest = [ordered]@{
            schemaVersion = 1
            issueNumber = $Fixture.IssueNumber
            platform = $Fixture.Platform
            testType = $Fixture.TestType
            testName = $Fixture.TestName
            testFilter = $Fixture.TestFilter
            expectedFailurePattern = $Fixture.FailurePattern
            reproductionMarker = 'UNCONDITIONAL_REPRODUCTION_TEST'
            proposedFiles = @($Fixture.CandidatePath)
        }
        if ($Overrides) {
            foreach ($key in $Overrides.Keys) {
                $manifest[$key] = $Overrides[$key]
            }
        }
        Write-TestJson -Path $Fixture.ManifestPath -Value $manifest
    }

    function Write-FixturePatch {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [string]$Content = $Fixture.Source,
            [string]$Path = $Fixture.CandidatePath
        )

        $Fixture.Source = $Content
        Write-TestText `
            -Path $Fixture.PatchPath `
            -Value (New-AddOnlyPatch -Path $Path -Content $Content)
    }

    function Write-FixtureEvidence {
        param([Parameter(Mandatory = $true)][object]$Fixture)

        $metadata = [ordered]@{
            schemaVersion = 1
            issueNumber = $Fixture.IssueNumber
            platform = $Fixture.Platform
            testType = $Fixture.TestType
            testName = $Fixture.TestName
            testFilter = $Fixture.TestFilter
            expectedFailurePattern = $Fixture.FailurePattern
            verificationMode = 'failure-only'
            verificationStatus = 'VERIFICATION PASSED'
            video = 'repro.mp4'
            preview = 'preview.gif'
        }
        Write-TestJson `
            -Path (Join-Path $Fixture.EvidenceDir 'evidence.json') `
            -Value $metadata

        $report = @(
            '## Gate: Test Verification (Failure-Only Mode)'
            ''
            '**Result:** ✅ PASSED'
            ''
            '| Test | Type | Outcome |'
            '|------|------|---------|'
            "| $($Fixture.TestName) | $($Fixture.TestType) | FAIL ✅ (expected) |"
        ) -join "`n"
        Write-TestText `
            -Path (Join-Path $Fixture.EvidenceDir 'verification-report.md') `
            -Value $report

        $verificationLog = @(
            'Verify Tests Fail (Failure Only Mode)'
            "Platform: $($Fixture.Platform)"
            "TestFilter: $($Fixture.TestFilter)"
            "TestName: $($Fixture.TestName)"
            "[$($Fixture.TestType)] $($Fixture.TestName): Passed=False Failed=1"
            'VERIFICATION PASSED'
        ) -join "`n"
        Write-TestText `
            -Path (Join-Path $Fixture.EvidenceDir 'verification-log.txt') `
            -Value $verificationLog

        $failureLog = @(
            "Test Filter: $($Fixture.TestFilter)"
            "Failed Maui.Tests.$($Fixture.TestName).ReproducesIssue [12 ms]"
            "Xunit.Sdk.XunitException: $($Fixture.FailurePattern)"
            'Failed: 1'
            'Total tests: 1'
        ) -join "`n"
        Write-TestText `
            -Path (Join-Path $Fixture.EvidenceDir "test-failure-$($Fixture.TestName).log") `
            -Value $failureLog

        $mp4Bytes = [byte[]](
            0, 0, 0, 24,
            102, 116, 121, 112,
            105, 115, 111, 109,
            0, 0, 0, 0,
            105, 115, 111, 109,
            109, 112, 52, 50
        )
        [System.IO.File]::WriteAllBytes(
            (Join-Path $Fixture.EvidenceDir 'repro.mp4'),
            $mp4Bytes
        )
        $gifBytes = [byte[]](
            71, 73, 70, 56, 57, 97,
            1, 0, 1, 0,
            128, 0, 0,
            0, 0, 0,
            255, 255, 255,
            59
        )
        [System.IO.File]::WriteAllBytes(
            (Join-Path $Fixture.EvidenceDir 'preview.gif'),
            $gifBytes
        )
    }

    function New-ValidationFixture {
        param(
            [string]$TestType = 'UnitTest',
            [string]$CandidatePath,
            [long]$IssueNumber = 12345,
            [string]$Platform = 'android',
            [string]$TestName = 'Issue12345',
            [string]$TestFilter = 'Issue12345',
            [string]$TestClassName = 'Microsoft.Maui.DeviceTests.Issue12345',
            [string]$TestMethodName = 'ReproducesReportedFailure',
            [string]$FailurePattern = 'Expected control to remain visible'
        )

        if (-not $CandidatePath) {
            $CandidatePath = Get-DefaultCandidatePath -TestType $TestType
        }
        $root = Join-Path $script:scratchRoot ([guid]::NewGuid().ToString('N'))
        $repoRoot = Join-Path $root 'repo'
        $evidenceDir = Join-Path $root 'evidence'
        New-Item -ItemType Directory -Path $repoRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null

        $fixture = [pscustomobject]@{
            Root = $root
            RepoRoot = $repoRoot
            EvidenceDir = $evidenceDir
            ManifestPath = Join-Path $root 'candidate.json'
            PatchPath = Join-Path $root 'candidate.patch'
            OutputPath = Join-Path $root 'validated.json'
            IssueNumber = $IssueNumber
            Platform = $Platform
            TestType = $TestType
            TestName = $TestName
            TestFilter = $TestFilter
            TestClassName = $TestClassName
            TestMethodName = $TestMethodName
            FailurePattern = $FailurePattern
            CandidatePath = $CandidatePath
            Source = New-TestSource `
                -TestType $TestType `
                -IssueNumber $IssueNumber `
                -TestName $TestName `
                -FailurePattern $FailurePattern `
                -Platform $Platform
        }
        Write-FixtureManifest -Fixture $fixture
        Write-FixturePatch -Fixture $fixture
        Write-FixtureEvidence -Fixture $fixture
        return $fixture
    }

    function Invoke-FixtureValidation {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [scriptblock]$Probe = $script:successfulProbe,
            [string]$CandidateCommit,
            [string]$BaseCommit,
            [string]$FixPatchPath
        )

        $parameters = @{
            RepoRoot = $Fixture.RepoRoot
            CandidateManifestPath = $Fixture.ManifestPath
            EvidenceDir = $Fixture.EvidenceDir
            IssueNumber = $Fixture.IssueNumber
            Platform = $Fixture.Platform
            OutputPath = $Fixture.OutputPath
            MediaProbe = $Probe
        }
        if ($CandidateCommit -and $BaseCommit) {
            $parameters.CandidateCommit = $CandidateCommit
            $parameters.BaseCommit = $BaseCommit
        } else {
            $parameters.PatchPath = $Fixture.PatchPath
            if ($FixPatchPath) {
                $parameters.FixPatchPath = $FixPatchPath
            }
        }

        return Invoke-ReplicationCandidateValidation @parameters
    }

    function ConvertTo-ArtifactContractFixture {
        param([Parameter(Mandatory = $true)][object]$Fixture)

        & git -C $Fixture.RepoRoot init --quiet
        & git -C $Fixture.RepoRoot config user.name 'Replication Validator Tests'
        & git -C $Fixture.RepoRoot config user.email 'validator-tests@example.invalid'
        Write-TestText -Path (Join-Path $Fixture.RepoRoot 'README') -Value 'trusted base'
        & git -C $Fixture.RepoRoot add README
        & git -C $Fixture.RepoRoot commit --quiet --no-gpg-sign -m 'base'
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to create artifact-contract fixture git base.'
        }
        $baseSha = (& git -C $Fixture.RepoRoot rev-parse HEAD).Trim()

        $publishedType = switch ($Fixture.TestType) {
            'UnitTest' { 'unit' }
            'XamlUnitTest' { 'xaml' }
            'DeviceTest' { 'device' }
            'UITest' { 'ui' }
        }
        $deviceId = switch ($Fixture.Platform) {
            'catalyst' { 'mac-catalyst-host' }
            'windows' { 'windows-host' }
            default { 'device-1' }
        }
        $manifest = [ordered]@{
            schemaVersion = 1
            issueNumber = $Fixture.IssueNumber
            platform = $Fixture.Platform
            baseSha = $baseSha
            status = 'reproduced'
            blocked = $null
            selectedDevice = [ordered]@{
                id = $deviceId
                name = 'Test Device'
                osVersion = '1.0'
            }
            attempts = [ordered]@{
                sandbox = 1
                automatedTest = 1
            }
            reproductionSteps = @('Launch the local scenario', 'Trigger the failing behavior')
            expectedBehavior = 'The control remains visible'
            observedBehavior = 'The control disappears'
            testType = $publishedType
            testFilter = $Fixture.TestFilter
            testClassName = $Fixture.TestClassName
            testMethodName = $Fixture.TestMethodName
            expectedFailureSignature = $Fixture.FailurePattern
            files = @($Fixture.CandidatePath)
            sandboxFiles = [ordered]@{
                xaml = 'sandbox/MainPage.xaml'
                codeBehind = 'sandbox/MainPage.xaml.cs'
                appiumPlan = 'sandbox/appium-plan.json'
            }
            reproductionResult = 'reproduction-result.json'
            evidenceManifest = 'evidence/evidence.json'
            verificationResult = 'verification/verification-result.json'
            patch = 'test.patch'
        }
        Write-TestJson -Path $Fixture.ManifestPath -Value $manifest

        $artifactRoot = Join-Path $Fixture.Root 'artifact'
        $evidenceRoot = Join-Path $artifactRoot 'evidence'
        $verificationRoot = Join-Path $artifactRoot 'verification'
        New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $verificationRoot -Force | Out-Null
        Copy-Item `
            -LiteralPath (Join-Path $Fixture.EvidenceDir 'repro.mp4') `
            -Destination (Join-Path $evidenceRoot 'repro.mp4')
        Copy-Item `
            -LiteralPath (Join-Path $Fixture.EvidenceDir 'preview.gif') `
            -Destination (Join-Path $evidenceRoot 'preview.gif')
        [System.IO.File]::WriteAllBytes(
            (Join-Path $evidenceRoot 'thumbnail.png'),
            [byte[]](137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0)
        )

        $videoPath = Join-Path $evidenceRoot 'repro.mp4'
        $video = Get-Item -LiteralPath $videoPath
        $evidenceMetadata = [ordered]@{
            schemaVersion = 1
            platform = $Fixture.Platform
            device = $deviceId
            durationSeconds = 4.25
            dimensions = [ordered]@{
                width = 720
                height = 1280
            }
            sha256 = (Get-FileHash -LiteralPath $videoPath -Algorithm SHA256).Hash.ToLowerInvariant()
            videoBytes = [long]$video.Length
            files = [ordered]@{
                video = 'repro.mp4'
                thumbnail = 'thumbnail.png'
                preview = 'preview.gif'
            }
        }
        Write-TestJson `
            -Path (Join-Path $evidenceRoot 'evidence.json') `
            -Value $evidenceMetadata

        $console = @(
            'VERIFY FAILURE ONLY MODE'
            "Platform: $($Fixture.Platform)"
            "Filter: $($Fixture.TestFilter)"
            "[$($Fixture.TestType)] $($Fixture.TestName): FAILED ✅ (expected)"
            '📊 Parsed test results: Passed=0 Failed=1 Total=1 (from 1 result blocks)'
            'VERIFICATION PASSED ✅ All 1 test(s) FAILED as expected!'
            'REPLICATION TEST VERIFICATION PASSED'
        ) -join "`n"
        Write-TestText `
            -Path (Join-Path $verificationRoot 'verification-console.log') `
            -Value $console
        Write-TestText `
            -Path (Join-Path $verificationRoot 'verification-console-run-2.log') `
            -Value $console
        $verificationResult = [ordered]@{
            schemaVersion = 1
            issueNumber = $Fixture.IssueNumber
            platform = $Fixture.Platform
            testType = $Fixture.TestType
            testFilter = $Fixture.TestFilter
            expectedFailureSignature = $Fixture.FailurePattern
            actualFailureMessage = "Xunit.Sdk.XunitException: $($Fixture.FailurePattern)"
            verifierExitCode = 0
            verifierPassed = $true
            signatureMatched = $true
            signatureEquivalent = $true
            effectiveFailureSignature = $Fixture.FailurePattern
            infrastructureFailure = $false
            verificationPassed = $true
            requestedRunCount = 2
            completedRunCount = 2
            consistentRuns = $true
            stableFailureMessage = $true
            logFiles = @('verification-console.log', 'verification-console-run-2.log')
        }
        Write-TestJson `
            -Path (Join-Path $verificationRoot 'verification-result.json') `
            -Value $verificationResult
        Write-TestJson `
            -Path (Join-Path $artifactRoot 'reproduction-result.json') `
            -Value ([ordered]@{
                schemaVersion = 1
                issueNumber = $Fixture.IssueNumber
                platform = $Fixture.Platform
                baseSha = $baseSha
                attempt = 1
                succeeded = $true
                confirmedRuns = 2
                device = $deviceId
                evidenceManifest = 'evidence/evidence.json'
            })

        $Fixture.EvidenceDir = $artifactRoot
        $Fixture | Add-Member -NotePropertyName BaseSha -NotePropertyValue $baseSha -Force
        return $Fixture
    }

    function Get-RejectedOutput {
        param([Parameter(Mandatory = $true)][object]$Fixture)

        return Get-Content -Raw -LiteralPath $Fixture.OutputPath | ConvertFrom-Json
    }
}

AfterAll {
    if (Test-Path -LiteralPath $script:scratchRoot) {
        Remove-Item -LiteralPath $script:scratchRoot -Recurse -Force
    }
}

Describe 'Validate-ReplicationCandidate happy paths' {
    It 'validates an add-only <TestType> candidate' -TestCases @(
        @{
            TestType = 'UnitTest'
            CandidatePath = 'src/Core/tests/UnitTests/Issue12345Tests.cs'
        },
        @{
            TestType = 'XamlUnitTest'
            CandidatePath = 'src/Controls/tests/Xaml.UnitTests/Issues/Issue12345Tests.cs'
        },
        @{
            TestType = 'DeviceTest'
            CandidatePath = 'src/Core/tests/DeviceTests/Handlers/Issue12345Tests.cs'
        },
        @{
            TestType = 'UITest'
            CandidatePath = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issue12345Tests.cs'
        }
    ) {
        param($TestType, $CandidatePath)

        $fixture = New-ValidationFixture `
            -TestType $TestType `
            -CandidatePath $CandidatePath
        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.status | Should -BeExactly 'validated'
        $expectedPublishedType = switch ($TestType) {
            'UnitTest' { 'unit' }
            'XamlUnitTest' { 'xaml' }
            'DeviceTest' { 'device' }
            'UITest' { 'ui' }
        }
        $result.testType | Should -BeExactly $expectedPublishedType
        $result.verificationTestType | Should -BeExactly $TestType
        $result.proposedFiles | Should -Be @($CandidatePath)
        $result.evidence.video | Should -BeExactly 'repro.mp4'

        $firstOutput = Get-Content -Raw -LiteralPath $fixture.OutputPath
        Invoke-FixtureValidation -Fixture $fixture | Out-Null
        (Get-Content -Raw -LiteralPath $fixture.OutputPath) | Should -BeExactly $firstOutput

        $trustedJson = $firstOutput | ConvertFrom-Json
        @($trustedJson.PSObject.Properties.Name) | Should -Not -Contain 'markdown'
        $firstOutput | Should -Not -Match 'Gate: Test Verification'
    }

    It 'validates a commit pair without reading the working tree candidate' {
        $fixture = New-ValidationFixture
        & git -C $fixture.RepoRoot init --quiet
        & git -C $fixture.RepoRoot config user.name 'Replication Validator Tests'
        & git -C $fixture.RepoRoot config user.email 'validator-tests@example.invalid'
        Write-TestText -Path (Join-Path $fixture.RepoRoot 'README') -Value 'base'
        & git -C $fixture.RepoRoot add README
        & git -C $fixture.RepoRoot commit --quiet --no-gpg-sign -m 'base'
        $LASTEXITCODE | Should -Be 0
        $baseCommit = (& git -C $fixture.RepoRoot rev-parse HEAD).Trim()

        $candidateFullPath = Join-Path $fixture.RepoRoot $fixture.CandidatePath
        Write-TestText -Path $candidateFullPath -Value $fixture.Source
        & git -C $fixture.RepoRoot add -- $fixture.CandidatePath
        & git -C $fixture.RepoRoot commit --quiet --no-gpg-sign -m 'candidate'
        $LASTEXITCODE | Should -Be 0
        $candidateCommit = (& git -C $fixture.RepoRoot rev-parse HEAD).Trim()

        $result = Invoke-FixtureValidation `
            -Fixture $fixture `
            -CandidateCommit $candidateCommit `
            -BaseCommit $baseCommit

        $result.status | Should -BeExactly 'validated'
        $result.candidateSource | Should -BeExactly 'commit'
        $result.baseSha | Should -BeExactly $baseCommit
    }

    It 'validates the fixed candidate, recorder, and verifier artifact contract' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.validationPassed | Should -BeTrue
        $result.baseSha | Should -BeExactly $fixture.BaseSha
        $result.testType | Should -BeExactly 'unit'
        $result.expectedFailureSignature | Should -BeExactly $fixture.FailurePattern
        $result.actualFailureMessage |
            Should -BeExactly "Xunit.Sdk.XunitException: $($fixture.FailurePattern)"
        $result.files | Should -Be @($fixture.CandidatePath)
        $result.reproductionSteps.Count | Should -Be 2
    }

    It 'validates the selected host identifier for <Platform> evidence' -TestCases @(
        @{ Platform = 'catalyst'; DeviceId = 'mac-catalyst-host' },
        @{ Platform = 'windows'; DeviceId = 'windows-host' }
    ) {
        param($Platform, $DeviceId)

        $fixture = New-ValidationFixture -Platform $Platform
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Not -Throw
        (Get-Content -LiteralPath (Join-Path $fixture.EvidenceDir 'evidence/evidence.json') -Raw |
            ConvertFrom-Json).device | Should -BeExactly $DeviceId
    }

    It 'rejects an artifact contract without a successful trusted execution result' {
        $fixture = ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)
        $resultPath = Join-Path $fixture.EvidenceDir 'reproduction-result.json'
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.succeeded = $false
        Write-TestJson -Path $resultPath -Value $result

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*successful trusted run*'
    }

    It 'accepts the orchestrator Sandbox and generated-test attempt limits' {
        $fixture = ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)
        $manifest = Get-Content -LiteralPath $fixture.ManifestPath -Raw |
            ConvertFrom-Json
        $manifest.attempts.sandbox = 5
        $manifest.attempts.automatedTest = 5
        Write-TestJson -Path $fixture.ManifestPath -Value $manifest

        $resultPath = Join-Path $fixture.EvidenceDir 'reproduction-result.json'
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.attempt = 5
        Write-TestJson -Path $resultPath -Value $result

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Not -Throw
    }

    It 'supports direct PatchPath script invocation' {
        $fixture = New-ValidationFixture

        $result = & $script:validatorPath `
            -RepoRoot $fixture.RepoRoot `
            -CandidateManifestPath $fixture.ManifestPath `
            -PatchPath $fixture.PatchPath `
            -EvidenceDir $fixture.EvidenceDir `
            -IssueNumber $fixture.IssueNumber `
            -Platform $fixture.Platform `
            -OutputPath $fixture.OutputPath `
            -MediaProbe $script:successfulProbe

        $result.validationPassed | Should -BeTrue
        (Get-Content -Raw -LiteralPath $fixture.OutputPath | ConvertFrom-Json).status |
            Should -BeExactly 'validated'
    }
}

Describe 'Validate-ReplicationCandidate manifest boundary' {
    It 'rejects a manifest for a different issue and leaves only rejected status' {
        $fixture = New-ValidationFixture
        Write-FixtureManifest -Fixture $fixture -Overrides @{ issueNumber = 99999 }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*issue number does not match*'

        $output = Get-RejectedOutput -Fixture $fixture
        $output.status | Should -BeExactly 'rejected'
        @($output.PSObject.Properties.Name) | Should -Be @('schemaVersion', 'status', 'validationPassed')
    }

    It 'rejects duplicate JSON properties' {
        $fixture = New-ValidationFixture
        $manifest = Get-Content -Raw -LiteralPath $fixture.ManifestPath
        $manifest = $manifest -replace '"issueNumber":\s*12345,', '"issueNumber": 12345, "issue_number": 12345,'
        Write-TestText -Path $fixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*more than one alias*'
    }

    It 'rejects free-form manifest properties' {
        $fixture = New-ValidationFixture
        $manifest = Get-Content -Raw -LiteralPath $fixture.ManifestPath | ConvertFrom-Json
        $manifest | Add-Member -NotePropertyName markdown -NotePropertyValue '# publish me'
        Write-TestJson -Path $fixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*unexpected property*'
    }

    It 'rejects attempt counts beyond the orchestrator limits' {
        $fixture = ConvertTo-ArtifactContractFixture -Fixture (
            New-ValidationFixture)
        $manifest = Get-Content -Raw -LiteralPath $fixture.ManifestPath |
            ConvertFrom-Json
        $manifest.attempts.sandbox = 6
        Write-TestJson -Path $fixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*Sandbox attempt count must be between 1 and 5*'

        $manifest.attempts.sandbox = 5
        $manifest.attempts.automatedTest = 6
        Write-TestJson -Path $fixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*automated test attempt count must be between 1 and 5*'
    }

    It 'rejects a manifest whose expected pattern is an infrastructure failure' {
        $fixture = New-ValidationFixture
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ expectedFailurePattern = 'Operation timed out waiting for emulator' }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*build, infrastructure, timeout, or missing-baseline*'
    }

    It 'rejects a conditional reproduction marker' {
        $fixture = New-ValidationFixture
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ reproductionMarker = 'MAUI_REPRODUCTION_ISSUE: 99999' }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*unconditional reproduction test*'
    }
}

Describe 'Validate-ReplicationCandidate patch boundary' {
    It 'rejects traversal in a proposed candidate path' {
        $fixture = New-ValidationFixture
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ proposedFiles = @('../src/Core/tests/UnitTests/Escape.cs') }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*traversal*'
    }

    It 'rejects traversal embedded directly in a patch header' {
        $fixture = New-ValidationFixture
        $patch = Get-Content -Raw -LiteralPath $fixture.PatchPath
        $escapedPath = "../$($fixture.CandidatePath)"
        $patch = $patch.Replace(
            "diff --git a/$($fixture.CandidatePath) b/$($fixture.CandidatePath)",
            "diff --git a/$escapedPath b/$escapedPath"
        ).Replace(
            "+++ b/$($fixture.CandidatePath)",
            "+++ b/$escapedPath"
        )
        Write-TestText -Path $fixture.PatchPath -Value $patch

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*traversal*'
    }

    It 'rejects product, project, workflow, script, and snapshot paths' -TestCases @(
        @{ Path = 'src/Controls/src/Core/ProductChange.cs' },
        @{ Path = 'src/Core/tests/UnitTests/Core.UnitTests.csproj' },
        @{ Path = '.github/workflows/publish.yml' },
        @{ Path = 'src/Core/tests/UnitTests/run.ps1' },
        @{ Path = 'src/Core/tests/UnitTests/Baselines/repro.png' }
    ) {
        param($Path)

        $fixture = New-ValidationFixture
        Write-FixtureManifest -Fixture $fixture -Overrides @{ proposedFiles = @($Path) }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw
    }

    It 'rejects a platform-specific test source for another platform' {
        $fixture = New-ValidationFixture `
            -TestType DeviceTest `
            -CandidatePath 'src/Core/tests/DeviceTests/Handlers/Issue12345.ios.cs' `
            -Platform android

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*different platform*'
    }

    It 'rejects edited, deleted, renamed, executable, symlink, and submodule patch forms' -TestCases @(
        @{
            Name = 'edit'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', 'old mode 100644') }
        },
        @{
            Name = 'delete'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', 'deleted file mode 100644') }
        },
        @{
            Name = 'rename'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', "rename from old.cs`nrename to new.cs") }
        },
        @{
            Name = 'executable'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', 'new file mode 100755') }
        },
        @{
            Name = 'symlink'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', 'new file mode 120000') }
        },
        @{
            Name = 'submodule'
            Mutate = { param($patch) $patch.Replace('new file mode 100644', 'new file mode 160000') }
        }
    ) {
        param($Name, $Mutate)

        $fixture = New-ValidationFixture
        $patch = Get-Content -Raw -LiteralPath $fixture.PatchPath
        Write-TestText -Path $fixture.PatchPath -Value (& $Mutate $patch)

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw
    }

    It 'rejects a binary git patch' {
        $fixture = New-ValidationFixture
        $binaryPatch = @"
diff --git a/$($fixture.CandidatePath) b/$($fixture.CandidatePath)
new file mode 100644
index 0000000..1111111
GIT binary patch
literal 4
LcmeAS@N?(olHy``u
"@
        Write-TestText -Path $fixture.PatchPath -Value $binaryPatch

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*binary patch*'
    }

    It 'rejects an oversized added source file' {
        $fixture = New-ValidationFixture
        $oversized = $fixture.Source + ("`n// padding" * 30000)
        Write-FixturePatch -Fixture $fixture -Content $oversized

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*oversized file*'
    }

    It 'rejects a manifest that omits a file from the patch' {
        $fixture = New-ValidationFixture
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ proposedFiles = @('src/Core/tests/UnitTests/OtherIssueTests.cs') }

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*exactly match*'
    }

    It 'rejects a fake new-file patch for an existing repository path' {
        $fixture = New-ValidationFixture
        Write-TestText `
            -Path (Join-Path $fixture.RepoRoot $fixture.CandidatePath) `
            -Value 'existing tracked-or-worktree content'

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*existing repository path*'
    }
}

Describe 'Validate-ReplicationCandidate source boundary' {
    It 'allows trimming annotations from System.Diagnostics.CodeAnalysis' {
        $fixture = New-ValidationFixture
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + @'

using System.Diagnostics.CodeAnalysis;
[RequiresUnreferencedCode("Loads bounded inline XAML.")]
static void LoadInlineXaml()
{
}
'@)

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Not -Throw
    }

    It 'rejects unsafe <Kind> source content' -TestCases @(
        @{ Kind = 'HttpClient'; Snippet = 'var value = new HttpClient();' },
        @{ Kind = 'WebClient'; Snippet = 'var value = new WebClient();' },
        @{ Kind = 'socket'; Snippet = 'var value = new Socket(SocketType.Stream, ProtocolType.Tcp);' },
        @{ Kind = 'Process.Start'; Snippet = 'Process.Start("whoami");' },
        @{ Kind = 'aliased process'; Snippet = 'using P = System.Diagnostics.Process; P.Start("whoami");' },
        @{ Kind = 'PInvoke'; Snippet = '[DllImport("evil")] static extern void Run();' },
        @{ Kind = 'remote URL'; Snippet = 'var value = "https://example.invalid/payload";' },
        @{ Kind = 'environment enumeration'; Snippet = 'Environment.GetEnvironmentVariables();' },
        @{ Kind = 'other environment secret'; Snippet = 'Environment.GetEnvironmentVariable("SECRET_VALUE");' },
        @{ Kind = 'shell execution'; Snippet = 'var shell = "bash -c whoami";' },
        @{ Kind = 'package reference'; Snippet = '#r "nuget: Evil.Package, 1.0.0"' }
    ) {
        param($Kind, $Snippet)

        $fixture = New-ValidationFixture
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + "`n$Snippet")

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*prohibited*'
    }

    It 'rejects an opt-in reproduction environment variable' {
        $fixture = New-ValidationFixture
        $source = $fixture.Source.Replace(
            'Assert.True(false, "Expected control to remain visible");',
            @'
if (Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE") == "12345")
    Assert.True(false, "Expected control to remain visible");
'@
        )
        Write-FixturePatch -Fixture $fixture -Content $source

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*environment-secrets*'
    }

    It 'rejects a framework behavior switch that manufactures the failure' {
        $fixture = New-ValidationFixture
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + "`nVisualElement.SkipMeasureInvalidatedPropagation = true;")

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw "*framework-behavior-switch*"
    }

    It 'rejects Catalyst UIKit source in an unsafe MacCatalyst filename' {
        $fixture = New-ValidationFixture `
            -TestType DeviceTest `
            -Platform catalyst `
            -CandidatePath 'src/Core/tests/DeviceTests/Handlers/Issue12345.MacCatalyst.cs'
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + "`nusing UIKit;")

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*unsafe MacCatalyst filename*'
    }

    It 'rejects a test constructor that runs before the issue guard' {
        $fixture = New-ValidationFixture
        $source = $fixture.Source.Replace(
            "public class $($fixture.TestName)`n{",
            @"
public class $($fixture.TestName)
{
    public $($fixture.TestName)()
    {
        throw new Exception("Runs before the issue guard");
    }
"@
        )
        Write-FixturePatch -Fixture $fixture -Content $source

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*unguarded test-class constructor*'
    }

    It 'rejects pre-execution code in an auxiliary generated C# file' {
        $fixture = New-ValidationFixture
        $helperPath = 'src/Core/tests/UnitTests/Issue12345Bootstrap.cs'
        $helperSource = @'
using System.Runtime.CompilerServices;

public static class Issue12345Bootstrap
{
    [ModuleInitializer]
    public static void Initialize() => throw new Exception("Runs before the guarded test");
}
'@
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ proposedFiles = @($fixture.CandidatePath, $helperPath) }
        Write-TestText `
            -Path $fixture.PatchPath `
            -Value (
                (New-AddOnlyPatch `
                    -Path $fixture.CandidatePath `
                    -Content $fixture.Source) +
                (New-AddOnlyPatch `
                    -Path $helperPath `
                    -Content $helperSource)
            )

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*test lifecycle attribute*Move the setup inside the test method body.*'
    }

    It 'allows a UI HostApp companion constructor' {
        $fixture = New-ValidationFixture -TestType UITest
        $hostPath = 'src/Controls/tests/TestCases.HostApp/Issues/Issue12345Page.xaml.cs'
        $hostSource = @'
public partial class Issue12345Page : ContentPage
{
    public Issue12345Page()
    {
        InitializeComponent();
    }
}
'@
        Write-FixtureManifest `
            -Fixture $fixture `
            -Overrides @{ proposedFiles = @($fixture.CandidatePath, $hostPath) }
        Write-TestText `
            -Path $fixture.PatchPath `
            -Value (
                (New-AddOnlyPatch `
                    -Path $fixture.CandidatePath `
                    -Content $fixture.Source) +
                (New-AddOnlyPatch `
                    -Path $hostPath `
                    -Content $hostSource)
            )

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Not -Throw
    }

    It 'rejects a source-level verification spoof' {
        $fixture = New-ValidationFixture
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + "`n// VERIFICATION PASSED")

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*verification-spoof*'
    }

    It 'rejects a package reference even though project files are already denied' {
        $fixture = New-ValidationFixture
        Write-FixturePatch `
            -Fixture $fixture `
            -Content ($fixture.Source + "`n// <PackageReference Include=`"Bad`" />")

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*package-reference*'
    }
}

Describe 'Validate-ReplicationCandidate verification boundary' {
    It 'rejects spoofed conflicting verification output' {
        $fixture = New-ValidationFixture
        Add-Content `
            -LiteralPath (Join-Path $fixture.EvidenceDir 'verification-log.txt') `
            -Value "`nVERIFICATION FAILED"

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*conflicting, spoofed*'
    }

    It 'rejects a full-verification report presented as failure-only' {
        $fixture = New-ValidationFixture
        Add-Content `
            -LiteralPath (Join-Path $fixture.EvidenceDir 'verification-log.txt') `
            -Value "`nFULL VERIFICATION MODE`nPASS with fix"

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*not failure-only*'
    }

    It 'rejects verification metadata for a different filter' {
        $fixture = New-ValidationFixture
        $metadataPath = Join-Path $fixture.EvidenceDir 'evidence.json'
        $metadata = Get-Content -Raw -LiteralPath $metadataPath | ConvertFrom-Json
        $metadata.testFilter = 'DifferentIssue'
        Write-TestJson -Path $metadataPath -Value $metadata

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*does not exactly match*'
    }

    It 'rejects a pass report for the wrong named test' {
        $fixture = New-ValidationFixture
        $reportPath = Join-Path $fixture.EvidenceDir 'verification-report.md'
        $report = Get-Content -Raw -LiteralPath $reportPath
        Write-TestText `
            -Path $reportPath `
            -Value $report.Replace($fixture.TestName, 'SpoofedTest')

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*named test*'
    }

    It 'rejects an assertion signature that appears only in summary metadata' {
        $fixture = New-ValidationFixture
        $failurePath = Join-Path $fixture.EvidenceDir "test-failure-$($fixture.TestName).log"
        $failure = Get-Content -Raw -LiteralPath $failurePath
        Write-TestText `
            -Path $failurePath `
            -Value $failure.Replace($fixture.FailurePattern, 'Different assertion')

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*expected assertion signature*'
    }

    It 'rejects <Kind>-only failures even when VERIFICATION PASSED is spoofed' -TestCases @(
        @{ Kind = 'compile'; Marker = 'error CS1002: ; expected' },
        @{ Kind = 'build'; Marker = 'BUILD ERROR: project failed to build' },
        @{ Kind = 'infra'; Marker = 'ENV ERROR: Appium server failed' },
        @{ Kind = 'timeout'; Marker = 'Operation timed out waiting for test completion' },
        @{ Kind = 'missing baseline'; Marker = 'Baseline snapshot not yet created' }
    ) {
        param($Kind, $Marker)

        $fixture = New-ValidationFixture
        Add-Content `
            -LiteralPath (Join-Path $fixture.EvidenceDir "test-failure-$($fixture.TestName).log") `
            -Value "`n$Marker"

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*disqualified*'
    }

    It 'rejects an on-device reproduction that was never replayed' {
        # The orchestrator replays the plan before claiming a reproduction. If
        # that ever stops happening, a single lucky observation must still not
        # reach a pull request.
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'reproduction-result.json'
        $reproduction = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $reproduction.confirmedRuns = 1
        Write-TestJson -Path $resultPath -Value $reproduction

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*two confirmed reproduction runs*'
    }

    It 'rejects a reproduction proved by a single execution' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.requestedRunCount = 1
        $verificationResult.completedRunCount = 1
        $verificationResult.logFiles = @('verification-console.log')
        Write-TestJson -Path $resultPath -Value $verificationResult
        Remove-Item -LiteralPath (
            Join-Path $fixture.EvidenceDir 'verification/verification-console-run-2.log') -Force

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*independent runs*'
    }

    It 'rejects a candidate that abandoned a requested repeat run' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.requestedRunCount = 3
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*every requested run*'
    }

    It 'rejects repeated runs that were not consistent' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.consistentRuns = $false
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*consistentRuns*'
    }

    It 'accepts a result that omits the executed test count' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } | Should -Not -Throw
    }

    It 'rejects a filter that selected more than one test' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult |
            Add-Member -NotePropertyName 'executedTestCounts' -NotePropertyValue @(2)
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*instead of exactly one targeted test*'
    }

    It 'accepts a filter that selected exactly one test' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult |
            Add-Member -NotePropertyName 'executedTestCounts' -NotePropertyValue @(1)
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } | Should -Not -Throw
    }

    It 'rejects a verifier that reported an ambiguous selection' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult |
            Add-Member -NotePropertyName 'selectionAmbiguous' -NotePropertyValue $true
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*not attributable to the named test*'
    }

    It 'rejects a repeat run whose console log does not prove the failure' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        Write-TestText `
            -Path (Join-Path $fixture.EvidenceDir 'verification/verification-console-run-2.log') `
            -Value 'VERIFY FAILURE ONLY MODE
VERIFICATION FAILED'

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*conflicting, spoofed, or not failure-only*'
    }

    It 'rejects a candidate whose repeat console log is missing' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        Remove-Item -LiteralPath (
            Join-Path $fixture.EvidenceDir 'verification/verification-console-run-2.log') -Force

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*console log for every completed run*'
    }

    It 'rejects a machine-readable verifier result with a spoofed signature match' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.signatureEquivalent = $false
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*signatureEquivalent*'
    }

    It 'publishes the wording the test produced when the prediction differed' {
        # Build 14999429 discarded a reproduction that had failed at its intended
        # assertion because the predicted wording was not an exact substring.
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $observed = 'attributed title foreground stayed blue after the runtime change'
        $verificationResult.actualFailureMessage = "Xunit.Sdk.XunitException: $observed"
        $verificationResult.signatureMatched = $false
        $verificationResult.effectiveFailureSignature = $observed
        Write-TestJson -Path $resultPath -Value $verificationResult

        $document = Invoke-FixtureValidation -Fixture $fixture
        $document.observedFailureSignature | Should -BeExactly $observed
    }

    It 'refuses to publish a signature the test never produced' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.signatureMatched = $false
        $verificationResult.effectiveFailureSignature =
            'a failure the targeted test never reported'
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*does not contain the published failure signature*'
    }

    It 'refuses to rewrite a signature that already matched exactly' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.actualFailureMessage =
            "Xunit.Sdk.XunitException: $($verificationResult.expectedFailureSignature) tail"
        $verificationResult.effectiveFailureSignature = 'tail'
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*must be published unchanged*'
    }

    It 'rejects a signature found only in machine-result metadata, not the failure message' {
        $fixture = New-ValidationFixture `
            -TestName Issue12345 `
            -TestFilter Issue12345 `
            -FailurePattern Issue12345
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.actualFailureMessage =
            'Xunit.Sdk.EqualException: expected red but was blue'
        $verificationResult.signatureMatched = $true
        $verificationResult.effectiveFailureSignature = 'Issue12345'
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*targeted test failure message*'
    }
    It 'refuses to spend a credential on a reproduction whose red is the harness, not the defect' {
        # Defense in depth for the orchestrator-side oracle guard: even if a
        # proposal slipped through, the message the run actually produced is
        # what reviewers read, so a harness teardown assertion must not publish.
        $fixture = New-ValidationFixture `
            -FailurePattern 'The app was expected to be running still'
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*non-falsifiable oracle*'
    }

    It 'rejects a clean-looking signature quoted from inside a harness failure' {
        # The nominated fragment passes inspection on its own, but the message
        # the run produced is the teardown assertion. Publishing this would
        # advertise an attributable red that the harness, not the defect, caused.
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $verificationResult = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
        $verificationResult.actualFailureMessage =
            'The app was expected to be running still, investigate as possible crash. ' +
            $verificationResult.expectedFailureSignature
        Write-TestJson -Path $resultPath -Value $verificationResult

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*non-falsifiable oracle*'
    }
}

Describe 'Validate-ReplicationCandidate media and file safety boundary' {
    It 'rejects a missing required video' {
        $fixture = New-ValidationFixture
        Remove-Item -LiteralPath (Join-Path $fixture.EvidenceDir 'repro.mp4')

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*missing required artifact*'
    }

    It 'rejects a zero-length media file' {
        $fixture = New-ValidationFixture
        [System.IO.File]::WriteAllBytes(
            (Join-Path $fixture.EvidenceDir 'repro.mp4'),
            [byte[]]::new(0)
        )

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*empty or smaller*'
    }

    It 'rejects invalid media magic before trusting the probe' {
        $fixture = New-ValidationFixture
        [System.IO.File]::WriteAllBytes(
            (Join-Path $fixture.EvidenceDir 'repro.mp4'),
            [System.Text.Encoding]::ASCII.GetBytes('not-an-mp4-file')
        )

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*valid MP4 file signature*'
    }

    It 'rejects media that the injected probe cannot decode' {
        $fixture = New-ValidationFixture
        $failedProbe = {
            param($Path, $Kind)
            if ($Kind -ceq 'mp4') {
                return [pscustomobject]@{
                    IsDecodable = $false
                    FormatName = 'mov,mp4'
                    DurationSeconds = 1
                    Width = 100
                    Height = 100
                }
            }
            return [pscustomobject]@{
                IsDecodable = $true
                FormatName = 'gif'
                Width = 100
                Height = 100
            }
        }

        { Invoke-FixtureValidation -Fixture $fixture -Probe $failedProbe | Out-Null } |
            Should -Throw '*not decodable*'
    }

    It 'rejects a symbolic-link media artifact' {
        $fixture = New-ValidationFixture
        $realVideo = Join-Path $fixture.Root 'outside.mp4'
        Copy-Item `
            -LiteralPath (Join-Path $fixture.EvidenceDir 'repro.mp4') `
            -Destination $realVideo
        Remove-Item -LiteralPath (Join-Path $fixture.EvidenceDir 'repro.mp4')
        $null = [System.IO.File]::CreateSymbolicLink(
            (Join-Path $fixture.EvidenceDir 'repro.mp4'),
            $realVideo
        )

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*Symbolic links*'
    }

    It 'rejects an unexpected evidence file that could be published accidentally' {
        $fixture = New-ValidationFixture
        Write-TestText `
            -Path (Join-Path $fixture.EvidenceDir 'agent-notes.md') `
            -Value '# untrusted markdown'

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*unexpected artifact*'
    }

    It 'accepts thumbnail.png as the bounded preview alternative' {
        $fixture = New-ValidationFixture
        Remove-Item -LiteralPath (Join-Path $fixture.EvidenceDir 'preview.gif')
        $pngBytes = [byte[]](137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0)
        [System.IO.File]::WriteAllBytes(
            (Join-Path $fixture.EvidenceDir 'thumbnail.png'),
            $pngBytes
        )
        $metadataPath = Join-Path $fixture.EvidenceDir 'evidence.json'
        $metadata = Get-Content -Raw -LiteralPath $metadataPath | ConvertFrom-Json
        $metadata.preview = 'thumbnail.png'
        Write-TestJson -Path $metadataPath -Value $metadata

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.evidence.preview | Should -BeExactly 'thumbnail.png'
    }

    It 'rejects recorder metadata whose video hash was spoofed' {
        $fixture = New-ValidationFixture
        $fixture = ConvertTo-ArtifactContractFixture -Fixture $fixture
        $metadataPath = Join-Path $fixture.EvidenceDir 'evidence/evidence.json'
        $metadata = Get-Content -Raw -LiteralPath $metadataPath | ConvertFrom-Json
        $metadata.sha256 = '0' * 64
        Write-TestJson -Path $metadataPath -Value $metadata

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*SHA-256 does not match*'
    }
}

Describe 'Replication candidate test-name matching' {
    BeforeAll {
        $script:uiTestPath =
            'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue33037LargeTitleNavigationPage.cs'
        $script:hostAppPath =
            'src/Controls/tests/TestCases.HostApp/Issues/Issue33037LargeTitleNavigationPage.cs'

        function script:New-NameFixture {
            param([string]$TestName, [string]$TypeName)

            return @{
                Manifest = [pscustomobject]@{
                    TestName = $TestName
                    TestType = 'UITest'
                    IssueNumber = 33037
                    Platform = 'ios'
                    ProposedFiles = @($script:hostAppPath, $script:uiTestPath)
                }
                CandidateFiles = @(
                    [pscustomobject]@{
                        Path = $script:hostAppPath
                        Mode = '100644'
                        Content = "public class $TypeName : ContentPage { }"
                    },
                    [pscustomobject]@{
                        Path = $script:uiTestPath
                        Mode = '100644'
                        Content = @"
#if IOS
public class $TypeName : _IssuesUITest
{
    [Test]
    public void LargeTitleCollapsesToVisibleStandardTitle() { }
}
#endif
"@
                    }
                )
            }
        }
    }

    It 'accepts the repository convention of an issue-prefixed descriptive type name' {
        $fixture = script:New-NameFixture `
            -TestName 'Issue33037' `
            -TypeName 'Issue33037LargeTitleNavigationPage'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Not -Throw
    }

    It 'accepts an exactly named test type' {
        $fixture = script:New-NameFixture -TestName 'Issue33037' -TypeName 'Issue33037'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Not -Throw
    }

    It 'rejects candidate files that only reference a different issue number' {
        $fixture = script:New-NameFixture `
            -TestName 'Issue33037' `
            -TypeName 'Issue330371ScrollBehavior'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Throw '*named test from the exact filter*'
    }

    It 'rejects a filter token that does not start an identifier' {
        $fixture = script:New-NameFixture `
            -TestName 'Issue33037' `
            -TypeName 'RegressionIssue33037Page'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Throw '*named test from the exact filter*'
    }
}

Describe 'The publishing gate checks fonts on the host page too' {
    BeforeAll {
        $script:fontRepo = Join-Path ([System.IO.Path]::GetTempPath()) ("fontgate-" + [guid]::NewGuid().ToString('n'))
        $programDir = Join-Path $script:fontRepo 'src/Controls/tests/TestCases.HostApp'
        New-Item -ItemType Directory -Path $programDir -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $programDir 'MauiProgram.cs') -Value @'
fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
'@

        function script:New-FontFixture {
            param([Parameter(Mandatory = $true)][string]$FontFamily)

            $hostPath = 'src/Controls/tests/TestCases.HostApp/Issues/Issue36505.cs'
            $testPath = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36505.cs'
            return @{
                Manifest = [pscustomobject]@{
                    TestName      = 'Issue36505'
                    TestType      = 'UITest'
                    IssueNumber   = 36505
                    Platform      = 'ios'
                    ProposedFiles = @($hostPath, $testPath)
                }
                CandidateFiles = @(
                    [pscustomobject]@{
                        Path    = $hostPath
                        Mode    = '100644'
                        Content = "public class Issue36505 : ContentPage { Label _label = new Label { FontFamily = `"$FontFamily`" }; }"
                    },
                    [pscustomobject]@{
                        Path    = $testPath
                        Mode    = '100644'
                        Content = @'
#if IOS
public class Issue36505 : _IssuesUITest
{
    [Test]
    public void WrappedTextKeepsItsMeasuredHeight() { }
}
#endif
'@
                    }
                )
            }
        }
    }

    AfterAll {
        if ($script:fontRepo -and (Test-Path -LiteralPath $script:fontRepo)) {
            Remove-Item -LiteralPath $script:fontRepo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'rejects a host page that asks for a font the repository never registers' {
        # PR 230: the font was named on the host page, which is exempt from the
        # test-shape guards, so the gate published a candidate that could only
        # go red for a missing font.
        $fixture = script:New-FontFixture -FontFamily 'Segoe UI Bold'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles `
                -RepositoryRoot $script:fontRepo
        } | Should -Throw '*Segoe UI Bold*does not register*'
    }

    It 'accepts a host page that asks for a registered font' {
        $fixture = script:New-FontFixture -FontFamily 'OpenSansRegular'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles `
                -RepositoryRoot $script:fontRepo
        } | Should -Not -Throw
    }

    It 'skips the font check when no repository is available to read' {
        $fixture = script:New-FontFixture -FontFamily 'Segoe UI Bold'

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Not -Throw
    }

    It 'passes the checked-out repository from the publishing entry point' {
        $source = Get-Content -LiteralPath $script:validatorPath -Raw
        $source | Should -Match 'Assert-ReplicationCandidateSources\s*`\s*\n\s*-Manifest \$manifest `\s*\n\s*-CandidateFiles \$candidateFiles `\s*\n\s*-RepositoryRoot \$repoPath'
    }
}

Describe 'The publishing gate runs the guards that were only ever run on device' {
    BeforeAll {
        function script:New-WiringFixture {
            param(
                [Parameter(Mandatory = $true)][string]$TestType,
                [Parameter(Mandatory = $true)][string]$Path,
                [Parameter(Mandatory = $true)][string]$Content
            )

            return @{
                Manifest = [pscustomobject]@{
                    TestName      = 'Issue12345'
                    TestType      = $TestType
                    IssueNumber   = 12345
                    Platform      = 'android'
                    ProposedFiles = @($Path)
                }
                CandidateFiles = @(
                    [pscustomobject]@{ Path = $Path; Mode = '100644'; Content = $Content }
                )
            }
        }
    }

    It 'rejects a device test the on-device runner could never select' {
        # PR 215: the stock Android runner honours only Category= filters, so a
        # device test without the issue category runs the whole suite.
        $fixture = script:New-WiringFixture `
            -TestType 'DeviceTest' `
            -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue12345.Android.cs' `
            -Content @'
#if ANDROID
public class Issue12345
{
    [Fact]
    public void ReproducesIssue() { }
}
#endif
'@

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Throw '*cannot be selected on device*Category("Issue12345")*'
    }

    It 'accepts a device test that carries the issue category' {
        $fixture = script:New-WiringFixture `
            -TestType 'DeviceTest' `
            -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue12345.Android.cs' `
            -Content @'
#if ANDROID
[Category("Issue12345")]
public class Issue12345
{
    [Fact]
    public void ReproducesIssue() { }
}
#endif
'@

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Not -Throw
    }

    It 'rejects a headless unit test that claims platform runtime evidence' {
        # PR 226: the test ran as a non-platform net10.0 unit test, so
        # IsMacCatalyst was false and every handler was null. The platform code
        # was not merely unexercised, it was absent from the tested closure.
        $repo = Join-Path ([System.IO.Path]::GetTempPath()) ("wiring-" + [guid]::NewGuid().ToString('n'))
        $projectDir = Join-Path $repo 'src/Controls/tests/Core.UnitTests'
        New-Item -ItemType Directory -Path $projectDir -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $projectDir 'Controls.Core.UnitTests.csproj') -Value @'
<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>
'@
        try {
            $fixture = script:New-WiringFixture `
                -TestType 'UnitTest' `
                -Path 'src/Controls/tests/Core.UnitTests/Issue12345Tests.cs' `
                -Content @'
public class Issue12345
{
    [Fact]
    public void ReproducesIssue() { }
}
'@

            {
                Assert-ReplicationCandidateSources `
                    -Manifest $fixture.Manifest `
                    -CandidateFiles $fixture.CandidateFiles `
                    -RepositoryRoot $repo
            } | Should -Throw '*single non-platform target framework*'
        }
        finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'rejects a test that asserts an operating system version floor' {
        # PR 213: the test asserted the iOS version floor, so every device below
        # it went red before the oracle ran - red for the lane, not the defect,
        # and still red after a complete product fix.
        $fixture = script:New-WiringFixture `
            -TestType 'UITest' `
            -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs' `
            -Content @'
#if ANDROID
public class Issue12345
{
    [Test]
    public void ReproducesIssue()
    {
        Assert.True(OperatingSystem.IsIOSVersionAtLeast(26));
        Assert.Fail("reported defect");
    }
}
#endif
'@

        {
            Assert-ReplicationCandidateSources `
                -Manifest $fixture.Manifest `
                -CandidateFiles $fixture.CandidateFiles
        } | Should -Throw '*asserts an environment precondition*'
    }
}

Describe 'Every guard that exists is enforced by the publishing gate' {
    It 'invokes every Assert-Replication guard that is defined' {
        # The font, platform-closure, environment-gate and device-selectability
        # guards were each written for a real reviewer rejection, wired into the
        # on-device script, and never wired into this gate - which is the
        # authoritative one, because it is what runs before a credential is
        # exposed. Four guards silently did nothing here. Fail loudly instead of
        # letting the fifth one slip through.
        $guardPath = Join-Path $PSScriptRoot 'Assert-ReplicationTestGuard.ps1'
        $guardSource = Get-Content -LiteralPath $guardPath -Raw
        $gateSource = Get-Content -LiteralPath $script:validatorPath -Raw

        $defined = @([regex]::Matches($guardSource, '(?m)^function\s+(?<name>Assert-Replication\w+)') |
            ForEach-Object { $_.Groups['name'].Value } | Sort-Object -Unique)
        $defined.Count | Should -BeGreaterThan 0

        $invoked = [System.Collections.Generic.HashSet[string]]::new(
            [string[]]@([regex]::Matches($gateSource, 'Assert-Replication\w+') |
                ForEach-Object { $_.Value }),
            [System.StringComparer]::Ordinal)

        $missing = @($defined | Where-Object { -not $invoked.Contains($_) })
        $missing -join ', ' | Should -BeExactly ''
    }
}

Describe 'The publisher accepts every field the verifier actually writes' {
    It 'allows each key of the verification result manifest' {
        # A live catalyst run reached the publisher and was rejected with
        # "Verification result contains unexpected property 'stableFailureMessage'"
        # because a field was added to the verifier without being registered
        # here. The allow-list is strict, so that omission silently blocks every
        # candidate. Compare the two sources directly rather than trusting a
        # fixture, which would have been updated alongside the writer and missed it.
        $scriptRoot = Split-Path -Parent $PSCommandPath

        $verifierPath = Join-Path $scriptRoot 'Invoke-ReplicationTestVerification.ps1'
        $verifierAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $verifierPath, [ref]$null, [ref]$null)
        $resultAssignment = $verifierAst.Find({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left.Extent.Text -eq '$result' -and
            $node.Right.Extent.Text.Contains('[ordered]@{')
        }, $true)
        $resultAssignment | Should -Not -BeNullOrEmpty
        $hashtable = $resultAssignment.Right.Find({
            param($node)
            $node -is [System.Management.Automation.Language.HashtableAst]
        }, $true)
        $writtenKeys = @($hashtable.KeyValuePairs | ForEach-Object { $_.Item1.Extent.Text.Trim("'", '"') })
        $writtenKeys.Count | Should -BeGreaterThan 10

        $validatorPath = Join-Path $scriptRoot 'Validate-ReplicationCandidate.ps1'
        $validatorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $validatorPath, [ref]$null, [ref]$null)
        $allowList = $validatorAst.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.ArrayLiteralAst] -and
            $node.Extent.Text -like "*'verifierExitCode'*" -and
            $node.Extent.Text -like "*'schemaVersion'*"
        }, $true) | Select-Object -First 1
        $allowList | Should -Not -BeNullOrEmpty
        $allowedNames = @($allowList.Elements | ForEach-Object { $_.Extent.Text.Trim("'", '"') })

        $unregistered = @($writtenKeys | Where-Object { $allowedNames -notcontains $_ })
        $unregistered -join ', ' | Should -BeExactly ''
    }
}

Describe 'The reproduction result the orchestrator writes matches what the publisher demands' {
    BeforeAll {
        $scriptRoot = Split-Path -Parent $PSCommandPath
        $orchestratorPath = Join-Path (Split-Path -Parent $scriptRoot) 'Replicate-Issue.ps1'
        $orchestratorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $orchestratorPath, [ref]$null, [ref]$null)

        # The orchestrator pipes the manifest straight into the reproduction
        # result file, so find the hashtable whose pipeline names that path.
        $writerPipeline = $orchestratorAst.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.PipelineAst] -and
            $node.Extent.Text.Contains('$reproductionResultPath') -and
            $node.Extent.Text.Contains('[ordered]@{')
        }, $true) | Select-Object -First 1
        $writerHashtable = $writerPipeline.Find({
            param($node)
            $node -is [System.Management.Automation.Language.HashtableAst]
        }, $true)
        $script:ReproductionWrittenKeys = @(
            $writerHashtable.KeyValuePairs | ForEach-Object { $_.Item1.Extent.Text.Trim("'", '"') })

        $validatorPath = Join-Path $scriptRoot 'Validate-ReplicationCandidate.ps1'
        $script:ValidatorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $validatorPath, [ref]$null, [ref]$null)
        # Anchor on the guard call for this manifest rather than on the shape
        # of its array, so the test keeps pointing at the right allow-list when
        # fields are added or removed.
        $allowListCall = $script:ValidatorAst.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Assert-KnownProperties' -and
            $node.Extent.Text -like "*'Replication execution result'*"
        }, $true) | Select-Object -First 1
        $allowListCall | Should -Not -BeNullOrEmpty
        $allowList = $allowListCall.Find({
            param($node)
            $node -is [System.Management.Automation.Language.ArrayLiteralAst]
        }, $true)
        $script:ReproductionAllowedNames = @(
            $allowList.Elements | ForEach-Object { $_.Extent.Text.Trim("'", '"') })
    }

    It 'allows every field the orchestrator writes' {
        $script:ReproductionWrittenKeys.Count | Should -BeGreaterThan 5
        $unregistered = @(
            $script:ReproductionWrittenKeys |
                Where-Object { $script:ReproductionAllowedNames -notcontains $_ })
        $unregistered -join ', ' | Should -BeExactly ''
    }

    It 'demands no field the orchestrator never writes' {
        # Live run 15006827 was rejected with "Replication execution result is
        # missing required property 'confirmedRuns'". Checking only that the
        # writer stays inside the allow-list misses this opposite direction: a
        # field the publisher requires but nothing produces rejects every
        # candidate just as completely.
        $requiredNames = @(
            $script:ValidatorAst.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.CommandAst] -and
                $node.GetCommandName() -eq 'Find-AliasedProperty' -and
                $node.Extent.Text -like "*'Replication execution result'*" -and
                $node.Extent.Text -like '*-Required*'
            }, $true) | ForEach-Object {
                # A single-element @('x') parses as an array expression, not an
                # array literal, so match the enclosing @() and read the string
                # constants inside it.
                $namesArgument = $_.Find({
                    param($node)
                    $node -is [System.Management.Automation.Language.ArrayExpressionAst]
                }, $true)
                if ($namesArgument) {
                    $namesArgument.FindAll({
                        param($node)
                        $node -is [System.Management.Automation.Language.StringConstantExpressionAst]
                    }, $true) | ForEach-Object { $_.Value }
                }
            } | Select-Object -Unique)

        $requiredNames.Count | Should -BeGreaterThan 3
        $unwritten = @(
            $requiredNames | Where-Object { $script:ReproductionWrittenKeys -notcontains $_ })
        $unwritten -join ', ' | Should -BeExactly ''
    }
}

Describe 'The publisher refuses a drag the platform would never recognise' {
    It 'rejects an element-rect-scaled drag at publish time' {
        {
            Assert-ReplicationGestureTravel `
                -Content @'
    var segment = Math.Max(1, (int)Math.Round(itemBounds.Height * 0.15));
    sequence.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
'@ `
                -Path 'Issue35770.cs'
        } | Should -Throw '*element rect*'
    }

    It 'accepts a window-scaled drag at publish time' {
        {
            Assert-ReplicationGestureTravel `
                -Content @'
    var windowSize = app.Driver.Manage().Window.Size;
    var segment = (int)Math.Round(windowSize.Height * 0.15);
    sequence.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
'@ `
                -Path 'Issue35770.cs'
        } | Should -Not -Throw
    }
}

Describe 'Reading which handlers the product gates behind a runtime feature' {
    # These use a fabricated hosting file rather than the real one, so the
    # brace-walking stays pinned even as src/Controls churns.
    BeforeAll {
        $script:HostingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("hosting-" + [guid]::NewGuid())
        $script:HostingFile = Join-Path $script:HostingRoot 'src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs'
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $script:HostingFile) | Out-Null
    }

    AfterAll {
        Remove-Item -Recurse -Force -LiteralPath $script:HostingRoot -ErrorAction SilentlyContinue
    }

    It 'collects only the handlers inside the feature-switched block' {
        Set-Content -LiteralPath $script:HostingFile -Encoding utf8 -Value @'
	internal static IMauiHandlersCollection AddControlsHandlers(this IMauiHandlersCollection handlers)
	{
		handlers.AddHandler<CollectionView, CollectionViewHandler2>();
		if (RuntimeFeature.IsMaterial3Enabled)
		{
			handlers.AddHandler<Entry, EntryHandler2>();
		}
		else
		{
			handlers.AddHandler<Entry, EntryHandler>();
		}
		handlers.AddHandler<Button, ButtonHandler>();
	}
'@

        $switched = @(Get-ReplicationFeatureSwitchedHandlers -RepositoryRoot $script:HostingRoot)

        $switched | Should -Contain 'EntryHandler2'
        $switched | Should -Not -Contain 'EntryHandler'
        $switched | Should -Not -Contain 'CollectionViewHandler2'
        $switched | Should -Not -Contain 'ButtonHandler'
    }

    It 'keeps reading past a nested block instead of stopping at its brace' {
        Set-Content -LiteralPath $script:HostingFile -Encoding utf8 -Value @'
		if (RuntimeFeature.IsMaterial3Enabled)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(31))
			{
				handlers.AddHandler<Label, LabelHandler2>();
			}
			handlers.AddHandler<Entry, EntryHandler2>();
		}
		handlers.AddHandler<Button, ButtonHandler>();
'@

        $switched = @(Get-ReplicationFeatureSwitchedHandlers -RepositoryRoot $script:HostingRoot)

        $switched | Should -Contain 'LabelHandler2'
        $switched | Should -Contain 'EntryHandler2'
        $switched | Should -Not -Contain 'ButtonHandler'
    }

    It 'reads nothing rather than guessing when the hosting file is gone' {
        $empty = Join-Path ([System.IO.Path]::GetTempPath()) ("hosting-none-" + [guid]::NewGuid())

        @(Get-ReplicationFeatureSwitchedHandlers -RepositoryRoot $empty) | Should -BeNullOrEmpty
        @(Get-ReplicationFeatureSwitchedHandlers -RepositoryRoot '') | Should -BeNullOrEmpty
    }

    It 'reads the real product source the guard is aimed at' {
        $switched = @(Get-ReplicationFeatureSwitchedHandlers `
            -RepositoryRoot (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path)

        # Android registers EntryHandler2 only under RuntimeFeature.IsMaterial3Enabled...
        $switched | Should -Contain 'EntryHandler2'
        # ...while iOS registers CollectionViewHandler2 unconditionally.
        $switched | Should -Not -Contain 'CollectionViewHandler2'
        $switched | Should -Not -Contain 'EntryHandler'
    }
}

Describe 'The publisher refuses a test that asserts its own handler registration' {
    It 'rejects the self-fulfilling registration at publish time' {
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content @'
    handlers.AddHandler<Entry, EntryHandler2>();
    Assert.IsType<EntryHandler2>(entry.Handler);
'@ `
                -Path 'Issue37275.Android.cs' `
                -RepositoryRoot (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        } | Should -Throw '*can only confirm the test setup*'
    }

    It 'leaves the default handler every device test registers alone' {
        # Reviewers accepted kubaflo/maui#195, #200, #202 and #208, each of
        # which registers the handler its scenario needs and then reaches
        # through Assert.IsType to get at PlatformView. Only handlers the
        # product itself gates behind a runtime feature switch are suspect.
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content @'
    handlers.AddHandler<Entry, EntryHandler>();
    var entryHandler = Assert.IsType<EntryHandler>(entry.Handler);
    Assert.NotNull(entryHandler.PlatformView);
'@ `
                -Path 'Issue37151Tests.Android.cs' `
                -RepositoryRoot (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        } | Should -Not -Throw
    }
}

Describe 'A platform-suffixed file is already scoped to its platform' {
    # Reviewers accepted kubaflo/maui#195, #200, #202 and #208, whose tests
    # carry no #if at all: src/MultiTargeting.targets and
    # Controls.DeviceTests.csproj remove *.Android.cs from every target
    # framework but Android, so demanding one asked for dead code.
    BeforeAll {
        $script:ScopeRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:UnscopedTest = @'
namespace Microsoft.Maui.DeviceTests
{
    public class Issue37151Tests
    {
        [Fact]
        public async Task EntryReportsItsContentDescription()
        {
            await Task.CompletedTask;
        }
    }
}
'@
    }

    It 'accepts an Android device test named for Android and carrying no #if' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue37151Tests.Android.cs' `
                -Platform 'android'
        } | Should -Not -Throw
    }

    It 'still refuses an unscoped test in a file with no platform in its name' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue37151Tests.cs' `
                -Platform 'android'
        } | Should -Throw '*also run on*'
    }

    It 'refuses a file named for a platform that did not produce the evidence' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue37151Tests.Windows.cs' `
                -Platform 'android'
        } | Should -Throw '*named for a platform it cannot serve*'
    }

    It 'treats an iOS-named file as serving Mac Catalyst too' {
        # Controls.DeviceTests.csproj removes *.iOS.cs only when the target
        # framework is neither -ios nor -maccatalyst.
        foreach ($platform in @('ios', 'catalyst')) {
            {
                Assert-ReplicationTestPlatformScope `
                    -Content $script:UnscopedTest `
                    -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue1.iOS.cs' `
                    -Platform $platform
            } | Should -Not -Throw
        }
    }

    It 'does not read MaciOS as an iOS suffix, nor Mac as either' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue1.MaciOS.cs' `
                -Platform 'catalyst'
        } | Should -Not -Throw
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue1.Mac.cs' `
                -Platform 'ios'
        } | Should -Throw '*the non-platform one*'
    }

    It 'reads a platform folder the same way as a platform suffix' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:UnscopedTest `
                -Path 'src/Essentials/test/DeviceTests/Tests/Android/Geolocation_Tests.cs' `
                -Platform 'ios'
        } | Should -Throw '*named for a platform it cannot serve*'
    }
}

Describe 'A font the operating system already provides needs no registration' {
    BeforeAll {
        $script:FontRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    }

    It 'knows the font families each platform ships' {
        # Named individually so shrinking a list is a test failure rather than
        # a silent widening of what the guard will accept.
        $expected = @{
            windows  = @('Segoe UI', 'Segoe UI Bold', 'Segoe MDL2 Assets', 'Calibri', 'Consolas', 'Arial')
            android  = @('Roboto', 'sans-serif', 'monospace', 'serif')
            ios      = @('Helvetica', 'Helvetica Neue', 'Arial')
            catalyst = @('Helvetica', 'Helvetica Neue', 'Arial')
        }

        foreach ($platform in $expected.Keys) {
            $fonts = @(Get-ReplicationSystemFonts -Platform $platform)
            foreach ($font in $expected[$platform]) {
                $fonts | Should -Contain $font -Because "$platform ships $font"
            }
        }

        @(Get-ReplicationSystemFonts -Platform 'ios') | Should -Not -Contain 'Segoe UI'
        @(Get-ReplicationSystemFonts -Platform 'android') | Should -Not -Contain 'Segoe UI'
        @(Get-ReplicationSystemFonts -Platform 'windows') | Should -Not -Contain 'Roboto'
    }

    It 'accepts Arial on Windows, as reviewers did for kubaflo/maui#232' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'searchHandler.FontFamily = "Arial";' `
                -Path 'src/Controls/tests/DeviceTests/Elements/Shell/Issue36629.Windows.cs' `
                -RepositoryRoot $script:FontRepoRoot `
                -Platform 'windows'
        } | Should -Not -Throw
    }

    It 'still refuses Segoe UI Bold on iOS, as reviewers did for kubaflo/maui#230' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'FontFamily = "Segoe UI Bold",' `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue36505.cs' `
                -RepositoryRoot $script:FontRepoRoot `
                -Platform 'ios'
        } | Should -Throw '*does not provide it as a system font*'
    }

    It 'does not hand one platform another platform''s fonts' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'FontFamily = "Roboto",' `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' `
                -RepositoryRoot $script:FontRepoRoot `
                -Platform 'windows'
        } | Should -Throw '*windows does not provide it*'
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'FontFamily = "Roboto",' `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' `
                -RepositoryRoot $script:FontRepoRoot `
                -Platform 'android'
        } | Should -Not -Throw
    }
}

Describe 'A test may not read back a verdict it announced itself' {
    # kubaflo/maui#221 set statusLabel.Text = "BUG REPRODUCED:" from an
    # animation completion callback and then asserted that same string. The
    # reviewer: "that label is assigned unconditionally when the animation
    # completes" -- so the assertion proves the animation ran, not that a
    # single pixel was wrong.
    It 'rejects the self-announced verdict a reviewer proved vacuous' {
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    pulseAnimation.Commit(page, AnimationName, 16, 3600, Easing.Linear, (_, cancelled) =>
    {
        if (!cancelled)
            statusLabel.Text = "BUG REPRODUCED:";
    });
    Assert.Equal("BUG REPRODUCED:", statusLabel.Text);
'@ `
                -Path 'src/Controls/tests/DeviceTests/Elements/Path/Issue36761Tests.iOS.cs'
        } | Should -Throw '*reading back a verdict it announced itself*'
    }

    It 'leaves the value-precedence idiom the product actually decides alone' {
        # src/Controls/tests/Core.UnitTests/StyleTests.cs sets a value and
        # asserts it, to prove a local set outranks a Style. The product
        # decides that answer, and "bar" announces nothing.
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    label.Style = style;
    Assert.Equal("foo", label.Text);
    label.Text = "bar";
    Assert.Equal("bar", label.Text);
'@ `
                -Path 'src/Controls/tests/Core.UnitTests/Issue1Tests.cs'
        } | Should -Not -Throw
    }

    It 'allows a verdict string the test never assigned' {
        # Reading a marker the *app* produced is evidence; only reading back
        # the test's own announcement is not.
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content 'Assert.Equal("Reproduced", App.FindElement("status").GetText());' `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs'
        } | Should -Not -Throw
    }

    It 'allows the test to assign a verdict string it never asserts' {
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    statusLabel.Text = "BUG REPRODUCED:";
    Assert.Equal(0, observation.OutsideRedPixels);
'@ `
                -Path 'src/Controls/tests/DeviceTests/Elements/Path/Issue1.iOS.cs'
        } | Should -Not -Throw
    }

    It 'catches the self-announced verdict in the NUnit spelling too' {
        # The guard only ever matched the xUnit argument order. Across the ten
        # published reproductions Assert.That outnumbered Assert.Equal 57 to 9,
        # so the form the repository's UI tests actually use was unguarded, and
        # PR 265 asserted Is.EqualTo("ALIGNED") against a label it set itself.
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    statusLabel.Text = "BUG REPRODUCED";
    Assert.That(statusLabel.Text, Is.EqualTo("BUG REPRODUCED"));
'@ `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs'
        } | Should -Throw -ExpectedMessage '*reading back a verdict it announced*'
    }

    It 'still allows an NUnit assertion against content the app displays' {
        # Reading a content label the app rendered is evidence. Only reading
        # back an announcement of the outcome is not.
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    bandText.Text = "EDGE-TO-EDGE CONTENT START";
    Assert.That(bandText.Text, Is.EqualTo("EDGE-TO-EDGE CONTENT START"));
'@ `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs'
        } | Should -Not -Throw
    }

    It 'keeps every assertion pattern separate from its neighbour' {
        # PowerShell binds the comma tighter than the plus, so an array of
        # concatenated patterns written as @(a + b, c + d) collapses into the
        # single element 'ab cd'. That parses cleanly, matches nothing, and
        # silently disables this guard; it is only visible as both spellings
        # being accepted at once. Pin the element count.
        $source = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-ReplicationTestGuard.ps1') -Raw
        $ast = [System.Management.Automation.Language.Parser]::ParseInput(
            $source, [ref]$null, [ref]$null)
        $forms = $ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $node.Left.Extent.Text -eq '$assertionForms'
            }, $true)

        $forms.Count | Should -Be 1
        $values = $forms[0].Right.Expression.SubExpression.Statements[0].PipelineElements[0].Expression.Elements
        $values.Count | Should -Be 2
    }

    It 'catches the verdict announcement whatever member carries it' {
        foreach ($member in @('Text', 'AutomationId', 'ClassId')) {
            {
                Assert-ReplicationVerdictIsNotSelfAnnounced `
                    -Content "marker.$member = `"TEST PASSED`";`nAssert.Equal(`"TEST PASSED`", marker.$member);" `
                    -Path 'src/Controls/tests/DeviceTests/Issue1.iOS.cs'
            } | Should -Throw '*verdict it announced itself*'
        }
    }

    It 'does not fire across two different objects' {
        # Setting the virtual view and asserting the platform view is how
        # every propagation test in the repository is written.
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    entry.Text = "FAILED";
    Assert.Equal("FAILED", platformEntry.Text);
'@ `
                -Path 'src/Controls/tests/DeviceTests/Elements/Entry/Issue1.Android.cs'
        } | Should -Not -Throw
    }

    It 'ignores a verdict that only appears in a comment' {
        {
            Assert-ReplicationVerdictIsNotSelfAnnounced `
                -Content @'
    // statusLabel.Text = "BUG REPRODUCED:";
    Assert.Equal("BUG REPRODUCED:", statusLabel.Text);
'@ `
                -Path 'src/Controls/tests/DeviceTests/Issue1.iOS.cs'
        } | Should -Not -Throw
    }
}

Describe 'The publisher refuses a test that throws away its own verdict' {
    It 'rejects a native identity verdict at publish time' {
        $source = @'
    var before = entry.Handler.PlatformView;
    Trigger();
    Assert.Same(before, entry.Handler.PlatformView);
'@

        {
            Assert-ReplicationPlatformViewIdentity -Content $source -Path 'Issue36298.cs'
        } | Should -Throw '*platform view of*'
    }

    It 'rejects a swallowed crash at publish time' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'void Wire() { AndroidEnvironment.UnhandledExceptionRaiser += OnCrash; }' `
                -Path 'Issue36298.cs'
        } | Should -Throw '*global-exception-suppression*'
    }

    It 'rejects a discarded short-circuit at publish time' {
        {
            Assert-ReplicationWaitResultIsUsed `
                -Content '    _ = Wait("a", 5000) || Wait("b", 5000);' `
                -Path 'Issue36298.cs'
        } | Should -Throw '*thrown away*'
    }
}

Describe 'The publisher refuses a reproduction that claims unproven platforms' {
    It 'rejects an unscoped shared UI test at publish time' {
        $source = @'
using NUnit.Framework;

public class Issue36298 : _IssuesUITest
{
    [Test]
    public void Reproduces()
    {
        App.WaitForElement("target");
    }
}
'@
        {
            Assert-ReplicationTestPlatformScope `
                -Content $source `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36298.cs' `
                -Platform 'windows'
        } | Should -Throw '*android, ios, catalyst*'
    }
}

Describe 'Validate-ReplicationCandidate certification' {
    BeforeAll {
        $script:ControlBaselineSource = @'
[Test]
public void Repro()
{
    App.NavigateTo("ShadowedButtonGallery");
    App.Tap("TriggerButton");
    Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Updated"));
}
'@
        $script:ControlVariantSource = @'
[Test]
public void Repro_Control()
{
    App.NavigateTo("PlainButtonGallery");
    App.Tap("TriggerButton");
    Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Updated"));
}
'@

    function Add-NegativeControl {
        param(
            $Fixture,
            [int]$RunCount = 2,
            [int]$PassCount = 2,
            [switch]$OmitSources,
            [string]$VariantSource
        )

        # The control runs after verification-result.json is written, so
        # production never puts the control inside it. Fixtures that did hid a
        # defect that graded every real reproduction as uncontrolled, so write
        # the artifact the verifier actually produces.
        $verificationRoot = Join-Path $Fixture.EvidenceDir 'verification'
        Write-TestJson `
            -Path (Join-Path $verificationRoot 'negative-control-result.json') `
            -Value ([ordered]@{
                schemaVersion = 1
                runCount      = $RunCount
                passCount     = $PassCount
            })

        if (-not $OmitSources) {
            Write-TestText `
                -Path (Join-Path $verificationRoot 'negative-control-baseline.cs') `
                -Value $script:ControlBaselineSource
            Write-TestText `
                -Path (Join-Path $verificationRoot 'negative-control-variant.cs') `
                -Value $(if ($VariantSource) { $VariantSource } else { $script:ControlVariantSource })
        }

        return $Fixture
    }
    }

    It 'grades a reproduction without a control as observed rather than certified' {
        $fixture = ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'observed-reproduction'
    }

    It 'certifies a control that edits the scene while the oracle lives elsewhere' {
        # A UI test's control edits the HostApp page, which has no assertions of
        # its own. Reading the oracle from that page, the gate found none and
        # refused a control that had passed on the device.
        $scene = @'
public class Issue1 : ContentPage
{
    public Issue1()
    {
        var grid = new Grid();
        grid.GestureRecognizers.Add(new TapGestureRecognizer());
        Content = grid;
    }
}
'@
        $sceneControl = $scene.Replace('grid.GestureRecognizers.Add(new TapGestureRecognizer());', '')
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -VariantSource $sceneControl
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        Write-TestText -Path (Join-Path $verificationRoot 'negative-control-baseline.cs') -Value $scene
        Write-TestText -Path (Join-Path $verificationRoot 'negative-control-oracle.cs') `
            -Value $script:ControlBaselineSource

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
    }

    It 'still refuses a scene control whose oracle snapshot has no assertion' {
        $scene = 'public class Issue1 : ContentPage { public Issue1() { var x = 1; } }'
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -VariantSource 'public class Issue1 : ContentPage { public Issue1() { } }'
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        Write-TestText -Path (Join-Path $verificationRoot 'negative-control-baseline.cs') -Value $scene
        Write-TestText -Path (Join-Path $verificationRoot 'negative-control-oracle.cs') `
            -Value 'public class T { [Test] public void M() { App.Tap("x"); } }'

        { Invoke-FixtureValidation -Fixture $fixture } | Should -Throw '*no assertion*'
    }

    It 'refuses a single-file control that drops the assertions it must preserve' {
        # A device test is one file, so its oracle is the file the control
        # edits and no oracle snapshot is written. The gate then compares
        # baseline against control, which is the only way this case can catch a
        # control that passes because it stopped measuring. Writing a snapshot
        # here would have the gate compare it against itself and let this
        # through.
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -VariantSource 'public class T { [Test] public void M() { var x = 1; } }'
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        Test-Path -LiteralPath (Join-Path $verificationRoot 'negative-control-oracle.cs') |
            Should -BeFalse

        { Invoke-FixtureValidation -Fixture $fixture } | Should -Throw '*asserts 0 times*'
    }

    It 'certifies a UI run whose console carries only the verifier summary' {
        # A UI test run prints no parsed runner counts at all. Requiring them
        # would have refused every UI reproduction, and six of the ten
        # published reproductions are UI tests.
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        foreach ($name in @('verification-console.log', 'verification-console-run-2.log')) {
            $path = Join-Path $verificationRoot $name
            Write-TestText -Path $path -Value (((Get-Content -LiteralPath $path) |
                    Where-Object { $_ -notmatch 'Parsed test results' }) -join "`n")
        }

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
    }

    It 'refuses to certify when the console does not prove one test ran' {
        # The claim used to be hard-coded, so a run that dragged in a
        # neighbouring test published a pull request saying it had not.
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        foreach ($name in @('verification-console.log', 'verification-console-run-2.log')) {
            $path = Join-Path $verificationRoot $name
            Write-TestText -Path $path -Value ((Get-Content -LiteralPath $path -Raw) -replace 'Total=1', 'Total=2')
        }

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw '*exactly one test was selected and executed*'
    }

    It 'refuses to certify when the console omits the verifier summary' {
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        foreach ($name in @('verification-console.log', 'verification-console-run-2.log')) {
            $path = Join-Path $verificationRoot $name
            Write-TestText -Path $path -Value (((Get-Content -LiteralPath $path) |
                    Where-Object { $_ -notmatch 'FAILED as expected' -and $_ -notmatch 'Parsed test results' }) -join "`n")
        }

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw '*exactly one test was selected and executed*'
    }

    It 'refuses to certify when the verifier summary counts more than one test' {
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        $verificationRoot = Join-Path $fixture.EvidenceDir 'verification'
        foreach ($name in @('verification-console.log', 'verification-console-run-2.log')) {
            $path = Join-Path $verificationRoot $name
            Write-TestText -Path $path -Value ((Get-Content -LiteralPath $path -Raw) -replace 'All 1 test', 'All 2 test')
        }

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw '*exactly one test was selected and executed*'
    }

    It 'refuses to certify when the verifier reports an unstable failure message' {
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        $resultPath = Join-Path $fixture.EvidenceDir 'verification/verification-result.json'
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.stableFailureMessage = $false
        Write-TestJson -Path $resultPath -Value $result

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw '*failure message was not identical across runs*'
    }

    It 'certifies a reproduction whose control passes without the trigger' {
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
    }

    It 'reports the certification matrix for the pull request body' {
        $fixture = Add-NegativeControl -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationSummary | Should -Match 'Trigger removed'
    }

    It 'refuses to certify when the test stays red without the trigger' {
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -PassCount 0

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'observed-reproduction'
    }

    It 'rejects a control reporting more passes than runs' {
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -RunCount 2 -PassCount 5

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*more passes than runs*'
    }

    It 'rejects a control that cannot be checked because its sources are missing' {
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -OmitSources

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*source snapshots*'
    }

    It 'rejects a control made green by weakening the oracle' {
        $weakened = $script:ControlVariantSource -replace 'Is\.EqualTo\("Updated"\)', 'Is.Not.Null'
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -VariantSource $weakened

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*changes the oracle*'
    }

    It 'rejects a control identical to the reproduction' {
        $fixture = Add-NegativeControl `
            -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)) `
            -VariantSource $script:ControlBaselineSource

        { Invoke-FixtureValidation -Fixture $fixture | Out-Null } |
            Should -Throw '*removes nothing*'
    }
}

Describe 'The pre-publish gate refuses a verdict the host page computed' {
    It 'runs the cross-file verdict guard before any credential is exposed' {
        # The publisher is the last gate before a draft PR appears, so a guard
        # the runner applies is worth nothing here unless this gate applies it
        # too.
        $source = Get-Content -LiteralPath $script:validatorPath -Raw
        $source | Should -Match (
            'Assert-ReplicationOracleIsNotInitialState -Files \$candidateContents\s*\r?\n\s*' +
            'Assert-ReplicationVerdictIsNotComputedByTheApp -Files \$candidateContents')
    }

    It 'rejects a candidate whose page decides the word the test asserts' {
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' =
                    'if (element == border) { edge = "ALIGNED"; }'
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' =
                    'Assert.That(status, Is.EqualTo("ALIGNED"));'
            }
        } | Should -Throw '*selects with the branch*'
    }
}

Describe 'The evidence allowlist knows every field the recorder writes' {
    # Adding decodedFrames to the recorder without adding it to the publisher's
    # strict allowlist made the publisher throw "unexpected property
    # 'decodedFrames'" and killed build 15051402 at the final gate, after the
    # emulator, the recording and the whole verification had already been paid
    # for. Comparing the two lists by hand is exactly the check that was missed,
    # so derive both from the source instead.
    BeforeAll {
        function Get-HashtableKeys {
            param([string]$Path, [string]$MarkerKey)

            $ast = [System.Management.Automation.Language.Parser]::ParseFile(
                $Path, [ref]$null, [ref]$null)
            $tables = $ast.FindAll({
                    param($node)
                    $node -is [System.Management.Automation.Language.HashtableAst] -and
                    # -contains is case-insensitive, so a marker of 'width'
                    # also matches the internal media-info object's 'Width'
                    # and the test then compares the wrong hashtable against
                    # the allowlist. Case matters for JSON keys anyway.
                    @($node.KeyValuePairs | ForEach-Object { $_.Item1.Extent.Text.Trim("'`"") }) -ccontains $MarkerKey
                }, $true)
            if ($tables.Count -lt 1) { throw "No hashtable containing '$MarkerKey' in $Path." }
            return @($tables[0].KeyValuePairs | ForEach-Object { $_.Item1.Extent.Text.Trim("'`"") })
        }

        function Get-EvidenceAllowlist {
            param([string]$Path, [string]$Context = 'Evidence metadata')

            $ast = [System.Management.Automation.Language.Parser]::ParseFile(
                $Path, [ref]$null, [ref]$null)
            $calls = $ast.FindAll({
                    param($node)
                    $node -is [System.Management.Automation.Language.CommandAst] -and
                    $node.GetCommandName() -eq 'Assert-KnownProperties' -and
                    $node.Extent.Text -match ("Context\s+'" + [regex]::Escape($Context) + "'")
                }, $true)
            # 'Evidence metadata' is asserted twice, once with a populated
            # allowlist and once without, so take the call that actually
            # carries names rather than whichever parses first.
            $calls = @($calls | Where-Object { $_.Extent.Text -match "AllowedNames" })
            if ($calls.Count -lt 1) { throw "No allowlist found for '$Context'." }
            # Reading the call's raw text would also read its comments, and
            # the comment above this allowlist quotes 'decodedFrames' to
            # explain why it is there. That made the test pass while the
            # allowlist itself was empty of it, so bind the parameter and read
            # the array literal, which contains no comments by construction.
            $binder = [System.Management.Automation.Language.StaticParameterBinder]::BindCommand(
                $calls[0])
            $bound = $binder.BoundParameters['AllowedNames']
            if (-not $bound) { throw 'The evidence allowlist has no AllowedNames argument.' }
            $arrayAst = $bound.Value
            while ($arrayAst -is [System.Management.Automation.Language.UnaryExpressionAst] -or
                $arrayAst -is [System.Management.Automation.Language.ParenExpressionAst]) {
                $arrayAst = $arrayAst.Child ?? $arrayAst.Pipeline
            }
            $elements = $arrayAst.FindAll({
                    param($node)
                    $node -is [System.Management.Automation.Language.StringConstantExpressionAst]
                }, $true)
            return @($elements | ForEach-Object { $_.Value })
        }

        $script:RecorderPath = Join-Path $PSScriptRoot 'Record-Reproduction.ps1'
        $script:ValidatorPath = Join-Path $PSScriptRoot 'Validate-ReplicationCandidate.ps1'
    }

    # Every strict allowlist has the same drift risk as the evidence manifest
    # did, and the two largest are written in files the validator never
    # references. Pair each one with the hashtable that produces it.
    $script:Couplings = @(
        @{ Context = 'Evidence metadata'; Writer = 'Record-Reproduction.ps1';            Marker = 'schemaVersion' }
        @{ Context = 'Evidence dimensions'; Writer = 'Record-Reproduction.ps1';          Marker = 'width' }
        @{ Context = 'Evidence files'; Writer = 'Record-Reproduction.ps1';               Marker = 'thumbnail' }
        @{ Context = 'Verification result'; Writer = 'Invoke-ReplicationTestVerification.ps1'; Marker = 'verificationPassed' }
    )

    It 'allows every key <Writer> writes for "<Context>"' -ForEach $script:Couplings {
        $writerPath = Join-Path $PSScriptRoot $Writer
        $written = Get-HashtableKeys -Path $writerPath -MarkerKey $Marker
        $allowed = Get-EvidenceAllowlist -Path $script:ValidatorPath -Context $Context

        $written | Should -Not -BeNullOrEmpty
        $allowed | Should -Not -BeNullOrEmpty
        foreach ($key in $written) {
            $allowed | Should -Contain $key -Because "$Writer writes '$key' and the publisher rejects anything unlisted"
        }
    }

    It 'still lists the frame count that build 15051402 was rejected for' {
        (Get-EvidenceAllowlist -Path $script:ValidatorPath) | Should -Contain 'decodedFrames'
    }
}

Describe 'The fix patch is the inverse of the test patch' {
    BeforeAll {
        $script:fixScratch = Join-Path $script:scratchRoot 'fix-patch'
        New-Item -ItemType Directory -Path $script:fixScratch -Force | Out-Null

        $script:fixTarget = 'src/Controls/src/Core/Button/Button.cs'

        function script:New-FixPatchFile {
            param(
                [Parameter(Mandatory = $true)][string]$Name,
                [Parameter(Mandatory = $true)][string]$Value
            )

            $path = Join-Path $script:fixScratch "$Name.patch"
            [System.IO.File]::WriteAllText(
                $path,
                $Value,
                [System.Text.UTF8Encoding]::new($false)
            )
            return $path
        }

        function script:New-FixPatchText {
            param(
                [string]$Target = 'src/Controls/src/Core/Button/Button.cs',
                [string]$Hunk = @'
@@ -10,7 +10,7 @@ namespace Microsoft.Maui.Controls
 	public partial class Button
 	{
 		void Update()
-			=> Handler?.UpdateValue(nameof(Text));
+			=> Handler?.UpdateValue(nameof(Text), force: true);
 	}
 }
 
'@
            )

            return @"
diff --git a/$Target b/$Target
index cc87be1f60..4791badc38 100644
--- a/$Target
+++ b/$Target
$Hunk
"@
        }

        # The most honest fixture is one git itself produced, so the parser is
        # measured against real output rather than against our idea of it.
        $script:gitFixRepo = Join-Path $script:fixScratch 'repo'
        New-Item -ItemType Directory -Path (Join-Path $script:gitFixRepo (Split-Path $script:fixTarget -Parent)) -Force | Out-Null
        Push-Location $script:gitFixRepo
        try {
            git init --quiet 2>&1 | Out-Null
            git config user.email 'test@example.com' 2>&1 | Out-Null
            git config user.name 'Test' 2>&1 | Out-Null
            $original = (1..40 | ForEach-Object { "line $_" }) -join "`n"
            [System.IO.File]::WriteAllText(
                (Join-Path $script:gitFixRepo $script:fixTarget),
                "$original`n",
                [System.Text.UTF8Encoding]::new($false)
            )
            git add -A 2>&1 | Out-Null
            git commit --quiet -m 'base' 2>&1 | Out-Null

            $edited = @($original -split "`n")
            $edited[4] = 'line 5 changed'
            $edited[30] = 'line 31 changed'
            [System.IO.File]::WriteAllText(
                (Join-Path $script:gitFixRepo $script:fixTarget),
                (($edited -join "`n") + "`n"),
                [System.Text.UTF8Encoding]::new($false)
            )
            $script:realFixPatch = Join-Path $script:fixScratch 'real.patch'
            git diff --binary -- $script:fixTarget |
                Set-Content -LiteralPath $script:realFixPatch -Encoding utf8NoBOM
        } finally {
            Pop-Location
        }
    }

    It 'accepts a modification patch git actually produced' {
        $files = @(Get-ReplicationFixFilesFromPatch `
            -Path $script:realFixPatch `
            -AllowedPaths @($script:fixTarget))

        $files | Should -HaveCount 1
        $files[0].Path | Should -BeExactly $script:fixTarget
        $files[0].AddedLines | Should -Be 2
        $files[0].RemovedLines | Should -Be 2
        $files[0].HunkCount | Should -Be 2
    }

    It 'refuses a file the reviewed scope never named' {
        { Get-ReplicationFixFilesFromPatch `
            -Path $script:realFixPatch `
            -AllowedPaths @('src/Core/src/Handlers/Button/ButtonHandler.cs') } |
            Should -Throw '*outside the reviewed fix scope*'
    }

    It 'refuses a scope that differs only by case' {
        { Get-ReplicationFixFilesFromPatch `
            -Path $script:realFixPatch `
            -AllowedPaths @($script:fixTarget.ToUpperInvariant()) } |
            Should -Throw '*outside the reviewed fix scope*'
    }

    It 'refuses an empty scope outright' {
        { Get-ReplicationFixFilesFromPatch `
            -Path $script:realFixPatch `
            -AllowedPaths @() } |
            Should -Throw '*outside the reviewed fix scope*'
    }

    It 'accepts a hand-written patch of the same shape' {
        $path = script:New-FixPatchFile -Name 'baseline' -Value (script:New-FixPatchText)
        $files = @(Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget))

        $files | Should -HaveCount 1
        $files[0].AddedLines | Should -Be 1
        $files[0].RemovedLines | Should -Be 1
        $files[0].HunkCount | Should -Be 1
    }

    It 'refuses a patch that adds a file' {
        $text = @"
diff --git a/$($script:fixTarget) b/$($script:fixTarget)
new file mode 100644
index 0000000000..4791badc38
--- /dev/null
+++ b/$($script:fixTarget)
@@ -0,0 +1 @@
+namespace X;
"@
        $path = script:New-FixPatchFile -Name 'add' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*add, delete, rename, copy, mode change, or submodule*'
    }

    It 'refuses a patch that deletes a file' {
        $text = @"
diff --git a/$($script:fixTarget) b/$($script:fixTarget)
deleted file mode 100644
index cc87be1f60..0000000000
--- a/$($script:fixTarget)
+++ /dev/null
@@ -1 +0,0 @@
-namespace X;
"@
        $path = script:New-FixPatchFile -Name 'delete' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*add, delete, rename, copy, mode change, or submodule*'
    }

    It 'refuses a patch that changes a file mode' {
        $text = (script:New-FixPatchText) -replace 'index cc87be1f60', "old mode 100644`nnew mode 100755`nindex cc87be1f60"
        $path = script:New-FixPatchFile -Name 'mode' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*add, delete, rename, copy, mode change, or submodule*'
    }

    It 'refuses a rename even when both paths are in scope' {
        $other = 'src/Controls/src/Core/Button/Button2.cs'
        $text = @"
diff --git a/$($script:fixTarget) b/$other
similarity index 98%
rename from $($script:fixTarget)
rename to $other
"@
        $path = script:New-FixPatchFile -Name 'rename' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget, $other) } |
            Should -Throw '*rename or path mismatch*'
    }

    It 'refuses a binary patch' {
        $text = (script:New-FixPatchText) -replace '@@ -10,7 \+10,7 @@.*', 'GIT binary patch'
        $path = script:New-FixPatchFile -Name 'binary' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*binary patch*'
    }

    It 'refuses stray carriage returns' {
        $text = (script:New-FixPatchText).Replace("`n", "`n") + "`rtrailing"
        $path = script:New-FixPatchFile -Name 'cr' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*non-normalized line endings*'
    }

    It 'refuses an empty patch' {
        $path = script:New-FixPatchFile -Name 'empty' -Value "   `n"
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*empty*'
    }

    It 'refuses anything before the first diff header' {
        $text = "Here is my fix!`n" + (script:New-FixPatchText)
        $path = script:New-FixPatchFile -Name 'preamble' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*unexpected preamble or malformed diff header*'
    }

    It 'refuses a hunk header that claims more lines than it carries' {
        $text = (script:New-FixPatchText) -replace '@@ -10,7 \+10,7 @@', '@@ -10,9 +10,9 @@'
        $path = script:New-FixPatchFile -Name 'overclaim' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*hunk line counts do not match*'
    }

    It 'refuses a hunk header that claims fewer lines than it carries' {
        $text = (script:New-FixPatchText) -replace '@@ -10,7 \+10,7 @@', '@@ -10,5 +10,5 @@'
        $path = script:New-FixPatchFile -Name 'underclaim' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*unsupported metadata or a malformed hunk*'
    }

    It 'refuses a hunk body line with an unrecognised marker' {
        $text = (script:New-FixPatchText) -replace '(?m)^ (\tpublic partial class Button)$', '?$1'
        $text | Should -Match '(?m)^\?' -Because 'the mutation must actually land or the test proves nothing'
        $path = script:New-FixPatchFile -Name 'marker' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*malformed hunk body line*'
    }

    It 'refuses a patch that carries no hunk at all' {
        $text = @"
diff --git a/$($script:fixTarget) b/$($script:fixTarget)
index cc87be1f60..4791badc38 100644
--- a/$($script:fixTarget)
+++ b/$($script:fixTarget)
"@
        $path = script:New-FixPatchFile -Name 'nohunk' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*complete modification to a tracked file*'
    }

    It 'refuses a patch that changes no lines' {
        $hunk = @'
@@ -10,3 +10,3 @@ namespace Microsoft.Maui.Controls
 	public partial class Button
 	{
 	}
'@
        $path = script:New-FixPatchFile -Name 'nochange' -Value (script:New-FixPatchText -Hunk $hunk)
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*changes no lines*'
    }

    It 'refuses a hunk that precedes its own file headers' {
        $text = @"
diff --git a/$($script:fixTarget) b/$($script:fixTarget)
index cc87be1f60..4791badc38 100644
@@ -10,1 +10,1 @@
-a
+b
--- a/$($script:fixTarget)
+++ b/$($script:fixTarget)
"@
        $path = script:New-FixPatchFile -Name 'earlyhunk' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*hunk before its file headers*'
    }

    It 'refuses a target header that names a different file' {
        $text = (script:New-FixPatchText) -replace [regex]::Escape("+++ b/$($script:fixTarget)"), '+++ b/src/Controls/src/Core/Other.cs'
        $path = script:New-FixPatchFile -Name 'mismatch' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*unsupported metadata or a malformed hunk*'
    }

    It 'refuses the same file twice' {
        $text = (script:New-FixPatchText) + "`n" + (script:New-FixPatchText)
        $path = script:New-FixPatchFile -Name 'duplicate' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*duplicate path*'
    }

    It 'refuses a diff that touches more files than the ceiling allows' {
        $targets = 1..($script:FixFileMaxCount + 1) | ForEach-Object {
            "src/Controls/src/Core/Button/Probe$_.cs"
        }
        $text = ($targets | ForEach-Object { script:New-FixPatchText -Target $_ }) -join "`n"
        $path = script:New-FixPatchFile -Name 'toomanyfiles' -Value $text
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths $targets } |
            Should -Throw '*modifies too many files*'
    }

    It 'refuses a diff that rewrites more lines than the ceiling allows' {
        $bodyLines = 1..($script:FixChangedLineMaxCount + 2) | ForEach-Object { "+line $_" }
        $hunk = "@@ -10,0 +10,$($bodyLines.Count) @@`n" + ($bodyLines -join "`n")
        $path = script:New-FixPatchFile -Name 'toomanylines' -Value (script:New-FixPatchText -Hunk $hunk)
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*changes too many lines*'
    }

    It 'accepts an empty context line that lost its leading space in transit' {
        $hunk = "@@ -10,4 +10,4 @@ namespace Microsoft.Maui.Controls`n 	public partial class Button`n`n-	void A() { }`n+	void A() { return; }`n 	}"
        $path = script:New-FixPatchFile -Name 'looseblank' -Value (script:New-FixPatchText -Hunk $hunk)
        $files = @(Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget))

        $files | Should -HaveCount 1
        $files[0].AddedLines | Should -Be 1
    }

    It 'refuses a patch larger than the fix ceiling' {
        $filler = ('x' * 1024)
        $text = (script:New-FixPatchText) + "`n" + (($filler | ForEach-Object { $_ }) * 1)
        $big = [System.Text.StringBuilder]::new()
        [void]$big.Append((script:New-FixPatchText))
        while ($big.Length -le $script:FixPatchMaxBytes) {
            [void]$big.AppendLine($filler)
        }
        $path = script:New-FixPatchFile -Name 'huge' -Value $big.ToString()
        { Get-ReplicationFixFilesFromPatch -Path $path -AllowedPaths @($script:fixTarget) } |
            Should -Throw '*Fix patch*'
    }
}

Describe 'Which product files a fix is allowed to touch' {
    It 'accepts every established product source root' {
        $accepted = @(
            'src/Controls/src/Core/Button/Button.cs',
            'src/Core/src/Handlers/Button/ButtonHandler.Android.cs',
            'src/Essentials/src/Types/Shared/Battery.cs',
            'src/Graphics/src/Graphics/Canvas.cs',
            'src/BlazorWebView/src/Maui/BlazorWebView.cs',
            'src/Compatibility/Core/src/Foo.cs',
            'src/SingleProject/Resizetizer/src/Foo.cs',
            'src/Controls/src/Core/Templates/Bar.xaml'
        )

        foreach ($path in $accepted) {
            Assert-ReplicationFixPath -Path $path -AllowedPaths @($path) |
                Should -BeExactly $path
        }
    }

    It 'refuses test code, tooling, and infrastructure' {
        $rejected = @(
            'src/Controls/tests/DeviceTests/Elements/ButtonTests.cs',
            'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs',
            'src/Core/tests/UnitTests/Foo.cs',
            'src/Templates/src/Foo.cs',
            'src/Provisioning/Foo.cs',
            'eng/scripts/Foo.cs',
            '.github/workflows/ci.yml',
            'src/Controls/src/Core/Button/Button.csproj',
            'src/Controls/src/Core/Button/Button.g.cs',
            'src/Controls/src/Core/Button/Button.designer.cs',
            'src/Controls/src/Core/GlobalUsings.cs',
            'src/Controls/src/Core/AssemblyInfo.cs',
            'src/Controls/src/Core/obj/Generated.cs',
            'src/Controls/src/Core/snapshots/Button.cs'
        )

        foreach ($path in $rejected) {
            { Assert-ReplicationFixPath -Path $path -AllowedPaths @($path) } |
                Should -Throw -Because "$path must never be editable by a fix candidate"
        }
    }

    It 'refuses absolute paths, traversal, and windows separators' {
        foreach ($path in @(
            '/etc/passwd',
            'C:/Windows/System32/foo.cs',
            'src/Controls/src/Core/../../../../etc/passwd.cs',
            'src\Controls\src\Core\Button.cs',
            'src/Controls/src/Core/%2e%2e/Button.cs'
        )) {
            { Assert-ReplicationFixPath -Path $path -AllowedPaths @($path) } |
                Should -Throw -Because "$path is not a repository-relative product path"
        }
    }
}

Describe 'What it takes to publish a fix alongside the reproduction' {
    BeforeAll {
        $script:oracleFixTarget = 'src/Controls/src/Core/Button/Button.cs'

        $script:oracleControlBaseline = @'
[Test]
public void Issue12345()
{
    App.NavigateTo("PlainButtonGallery");
    App.Tap("TriggerButton");
    Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Updated"));
}
'@
        $script:oracleControlVariant = @'
[Test]
public void Issue12345()
{
    App.NavigateTo("PlainButtonGallery");
    Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Updated"));
}
'@

        function script:Add-OracleControl {
            param([Parameter(Mandatory = $true)][object]$Fixture)

            $verificationRoot = Join-Path $Fixture.EvidenceDir 'verification'
            Write-TestJson `
                -Path (Join-Path $verificationRoot 'negative-control-result.json') `
                -Value ([ordered]@{ schemaVersion = 1; runCount = 2; passCount = 2 })
            Write-TestText `
                -Path (Join-Path $verificationRoot 'negative-control-baseline.cs') `
                -Value $script:oracleControlBaseline
            Write-TestText `
                -Path (Join-Path $verificationRoot 'negative-control-variant.cs') `
                -Value $script:oracleControlVariant
            return $Fixture
        }

        function script:Add-OracleArms {
            param(
                [Parameter(Mandatory = $true)][object]$Fixture,
                [int]$FixRuns = 2,
                [int]$FixPasses = 2,
                [int]$RestorationRuns = 2,
                [int]$RestorationFailures = 2,
                [switch]$OmitFix,
                [switch]$OmitRestoration
            )

            $verificationRoot = Join-Path $Fixture.EvidenceDir 'verification'
            if (-not $OmitFix) {
                Write-TestJson `
                    -Path (Join-Path $verificationRoot 'fix-control-result.json') `
                    -Value ([ordered]@{ schemaVersion = 1; runCount = $FixRuns; passCount = $FixPasses })
            }
            if (-not $OmitRestoration) {
                Write-TestJson `
                    -Path (Join-Path $verificationRoot 'restoration-result.json') `
                    -Value ([ordered]@{
                        schemaVersion = 1
                        runCount = $RestorationRuns
                        failureCount = $RestorationFailures
                    })
            }
            return $Fixture
        }

        function script:Add-OracleFixPatch {
            param(
                [Parameter(Mandatory = $true)][object]$Fixture,
                [string]$Target = 'src/Controls/src/Core/Button/Button.cs',
                [string[]]$DeclaredFiles,
                [switch]$SkipProductFile
            )

            if (-not $SkipProductFile) {
                Write-TestText `
                    -Path (Join-Path $Fixture.RepoRoot $Target) `
                    -Value "namespace Microsoft.Maui.Controls;`n"
            }

            $fixPatchPath = Join-Path $Fixture.Root 'fix.patch'
            Write-TestText -Path $fixPatchPath -Value @"
diff --git a/$Target b/$Target
index cc87be1f60..4791badc38 100644
--- a/$Target
+++ b/$Target
@@ -10,7 +10,7 @@ namespace Microsoft.Maui.Controls
 	public partial class Button
 	{
 		void Update()
-			=> Handler?.UpdateValue(nameof(Text));
+			=> Handler?.UpdateValue(nameof(Text), force: true);
 	}
 }
 
"@

            $declared = if ($PSBoundParameters.ContainsKey('DeclaredFiles')) { @($DeclaredFiles) } else { @($Target) }
            $manifest = Get-Content -LiteralPath $Fixture.ManifestPath -Raw | ConvertFrom-Json
            Add-Member -InputObject $manifest -NotePropertyName 'fixFiles' -NotePropertyValue $declared -Force
            if (@($declared).Count -gt 0) {
                Add-Member -InputObject $manifest -NotePropertyName 'fixPatch' -NotePropertyValue 'fix.patch' -Force
            }
            Write-TestJson -Path $Fixture.ManifestPath -Value $manifest

            return $fixPatchPath
        }

        function script:New-OracleFixture {
            return script:Add-OracleControl `
                -Fixture (ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture))
        }
    }

    It 'certifies a reproduction whose fix turns it green and whose removal turns it red' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        $result = Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath

        $result.certificationLevel | Should -BeExactly 'certified-oracle'
        $result.fixFiles | Should -Be @($script:oracleFixTarget)
        $result.fixPatch | Should -BeExactly 'fix.patch'
    }

    It 'ignores fix arm results when no fix patch is published' {
        # Without this the run could claim the fix made the test green while
        # shipping no fix at all, which is the one claim a reviewer cannot check.
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
        $result.fixFiles | Should -Be @()
        $result.fixPatch | Should -BeNullOrEmpty
    }

    It 'refuses a fix arm reported without its restoration arm' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -OmitRestoration
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*without its restoration arm*'
    }

    It 'refuses a restoration arm reported without its fix arm' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -OmitFix
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*without its restoration arm*'
    }

    It 'still publishes a trigger-certified reproduction when the fix arm did not pass every run' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -FixPasses 1
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        $result = Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
        $result.fixFiles | Should -Be @() -Because 'a fix that did not work must be discarded, not published'
        $result.fixPatch | Should -BeNullOrEmpty
    }

    It 'still publishes a trigger-certified reproduction when removing the fix left it green' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -RestorationFailures 1
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        $result = Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
        $result.fixFiles | Should -Be @() -Because 'an unattributable fix must be discarded, not published'
        $result.fixPatch | Should -BeNullOrEmpty
    }

    It 'still publishes a trigger-certified reproduction when no fix was attempted at all' {
        $fixture = script:New-OracleFixture

        $result = Invoke-FixtureValidation -Fixture $fixture

        $result.certificationLevel | Should -BeExactly 'trigger-certified'
    }

    It 'refuses a fix arm that reports more passes than runs' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -FixRuns 1 -FixPasses 2
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*more passes than runs*'
    }

    It 'refuses a restoration arm that reports more failures than runs' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture -RestorationRuns 1 -RestorationFailures 2
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*more failures than runs*'
    }

    It 'refuses a manifest that names fix files without shipping a fix patch' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture
        $null = script:Add-OracleFixPatch -Fixture $fixture

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw '*names fix files but no fix patch*'
    }

    It 'refuses a fix patch the manifest never declared' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture
        $fixPatchPath = script:Add-OracleFixPatch `
            -Fixture $fixture `
            -DeclaredFiles @('src/Core/src/Handlers/Button/ButtonHandler.cs')

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*outside the reviewed fix scope*'
    }

    It 'refuses a manifest that declares a file the fix patch never touches' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture
        Write-TestText `
            -Path (Join-Path $fixture.RepoRoot 'src/Core/src/Handlers/Button/ButtonHandler.cs') `
            -Value "namespace Microsoft.Maui.Handlers;`n"
        $fixPatchPath = script:Add-OracleFixPatch `
            -Fixture $fixture `
            -DeclaredFiles @($script:oracleFixTarget, 'src/Core/src/Handlers/Button/ButtonHandler.cs')

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*names a fix file the fix patch never modifies*'
    }

    It 'refuses a fix that modifies a file the trusted checkout does not have' {
        $fixture = script:New-OracleFixture
        $null = script:Add-OracleArms -Fixture $fixture
        $fixPatchPath = script:Add-OracleFixPatch -Fixture $fixture -SkipProductFile

        { Invoke-FixtureValidation -Fixture $fixture -FixPatchPath $fixPatchPath } |
            Should -Throw '*does not exist in the trusted checkout*'
    }

    It 'refuses a manifest whose fix files overlap the test it is meant to turn green' {
        $fixture = script:New-OracleFixture
        $manifest = Get-Content -LiteralPath $fixture.ManifestPath -Raw | ConvertFrom-Json
        Add-Member -InputObject $manifest -NotePropertyName 'fixFiles' `
            -NotePropertyValue @($fixture.CandidatePath) -Force
        Write-TestJson -Path $fixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $fixture } |
            Should -Throw
    }
}

Describe 'A manifest cannot rename the fix patch out from under the gate' {
    BeforeEach {
        $script:renameFixture = ConvertTo-ArtifactContractFixture -Fixture (New-ValidationFixture)
    }

    It 'refuses a manifest that names a fix patch by some other name' {
        # The gate is handed the patch by path, so this field is documentation.
        # A manifest that documents a different artifact is describing a run
        # that did not happen, and the PR would cite evidence nobody validated.
        $manifest = Get-Content -LiteralPath $script:renameFixture.ManifestPath -Raw | ConvertFrom-Json
        Add-Member -InputObject $manifest -NotePropertyName 'fixFiles' `
            -NotePropertyValue @('src/Controls/src/Core/Button/Button.cs') -Force
        Add-Member -InputObject $manifest -NotePropertyName 'fixPatch' `
            -NotePropertyValue 'test.patch' -Force
        Write-TestJson -Path $script:renameFixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $script:renameFixture } |
            Should -Throw '*does not match the fixed artifact contract*'
    }

    It 'refuses a manifest that names a fix patch while claiming no fix files' {
        $manifest = Get-Content -LiteralPath $script:renameFixture.ManifestPath -Raw | ConvertFrom-Json
        Add-Member -InputObject $manifest -NotePropertyName 'fixPatch' `
            -NotePropertyValue 'fix.patch' -Force
        Write-TestJson -Path $script:renameFixture.ManifestPath -Value $manifest

        { Invoke-FixtureValidation -Fixture $script:renameFixture } |
            Should -Throw '*names a fix patch but no fix files*'
    }

    It 'accepts the reproduction-only manifest every run before the fix phase produced' {
        { Invoke-FixtureValidation -Fixture $script:renameFixture } | Should -Not -Throw
    }
}

Describe 'Reading the run instead of a summary of the run' {
    BeforeEach {
        $script:resultRoot = Join-Path $TestDrive ([Guid]::NewGuid().ToString('n'))
        New-Item -ItemType Directory -Path $script:resultRoot -Force | Out-Null

        # Shaped after the document build 14988245 actually produced. Writing a
        # document we invented would test our idea of xUnit rather than xUnit.
        $script:xunitOne = @'
<?xml version="1.0" encoding="utf-8"?>
<assemblies>
  <assembly name="Microsoft.Maui.Controls.DeviceTests.dll" total="1" passed="0" failed="1" skipped="0">
    <errors />
    <collection total="1" passed="0" failed="1" name="Serialize test" time="0.377">
      <test name="HtmlInsAndDelRenderWithTextDecorations" type="Microsoft.Maui.DeviceTests.Issue19519" method="HtmlInsAndDelRenderWithTextDecorations" time="0.37" result="Fail">
        <traits><trait name="Category" value="Issue19519" /></traits>
        <failure exception-type="Xunit.Sdk.TrueException">
          <message><![CDATA[HTML ins text should render as an underline.]]></message>
        </failure>
      </test>
    </collection>
  </assembly>
</assemblies>
'@

        function script:Write-ResultDocument {
            param([string]$Content, [string]$Name = 'verification-test-result.xml')
            $path = Join-Path $script:resultRoot $Name
            Set-Content -LiteralPath $path -Value $Content -Encoding utf8NoBOM
            return $path
        }

        function script:Assert-Result {
            param(
                [string]$Class = 'Microsoft.Maui.DeviceTests.Issue19519',
                [string]$Method = 'HtmlInsAndDelRenderWithTextDecorations'
            )
            return Assert-ReplicationAuthoritativeResult `
                -VerificationRoot $script:resultRoot `
                -TestClass $Class `
                -TestMethod $Method
        }
    }

    It 'accepts a run that executed exactly the named test and saw it fail' {
        $null = script:Write-ResultDocument -Content $script:xunitOne
        $result = script:Assert-Result

        $result.Name | Should -Be 'HtmlInsAndDelRenderWithTextDecorations'
        $result.Outcome | Should -Be 'Fail'
    }

    It 'refuses a run whose filter selected nothing at all' {
        # This is the defect the whole check exists for. An XHarness method
        # filter cannot express a display name containing a comma, so a theory
        # selected no test, no count was recorded, and the reproduction was
        # published as evidence of a failure that never ran.
        $null = script:Write-ResultDocument -Content @'
<?xml version="1.0" encoding="utf-8"?>
<assemblies>
  <assembly name="Microsoft.Maui.Controls.DeviceTests.dll" total="0" passed="0" failed="0" skipped="0">
    <errors />
  </assembly>
</assemblies>
'@

        { script:Assert-Result } | Should -Throw '*records no executed test*'
    }

    It 'refuses a run that executed more than the named test' {
        $null = script:Write-ResultDocument -Content ($script:xunitOne -replace
            '(?s)(<test name="HtmlIns.*?</test>)', '$1
      <test name="SomethingElse" type="Microsoft.Maui.DeviceTests.Issue19519" method="SomethingElse" result="Fail" />')

        { script:Assert-Result } | Should -Throw '*records 2 executed tests*'
    }

    It 'refuses a run whose named test passed' {
        $null = script:Write-ResultDocument -Content ($script:xunitOne -replace 'result="Fail"', 'result="Pass"')

        { script:Assert-Result } | Should -Throw '*published as a failing test*'
    }

    It 'refuses a run that executed a different method than the manifest claims' {
        $null = script:Write-ResultDocument -Content $script:xunitOne

        { script:Assert-Result -Method 'SomeOtherMethod' } |
            Should -Throw '*not the method the manifest claims*'
    }

    It 'refuses a run that executed the right method on a different class' {
        $null = script:Write-ResultDocument -Content $script:xunitOne

        { script:Assert-Result -Class 'Microsoft.Maui.DeviceTests.Issue99999' } |
            Should -Throw '*not the class the manifest claims*'
    }

    It 'refuses evidence with no authoritative document at all' {
        { script:Assert-Result } | Should -Throw '*no authoritative test result document*'
    }

    It 'refuses evidence carrying two authoritative documents' {
        $null = script:Write-ResultDocument -Content $script:xunitOne
        $null = script:Write-ResultDocument -Content $script:xunitOne -Name 'verification-test-result.trx'

        { script:Assert-Result } | Should -Throw '*more than one authoritative test result document*'
    }

    It 'reads a namespaced TRX, which a namespace-sensitive query would read as empty' {
        $null = script:Write-ResultDocument -Name 'verification-test-result.trx' -Content @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Microsoft.Maui.DeviceTests.Issue19519.HtmlInsAndDelRenderWithTextDecorations" outcome="Failed" />
  </Results>
</TestRun>
'@

        (script:Assert-Result).Outcome | Should -Be 'Failed'
    }

    It 'reads an NUnit document, which the UI test lanes emit' {
        $null = script:Write-ResultDocument -Content @'
<?xml version="1.0" encoding="utf-8"?>
<test-run>
  <test-suite type="TestFixture">
    <test-case id="1000" name="HtmlInsAndDelRenderWithTextDecorations" fullname="Microsoft.Maui.DeviceTests.Issue19519.HtmlInsAndDelRenderWithTextDecorations" classname="Microsoft.Maui.DeviceTests.Issue19519" result="Failed" />
  </test-suite>
</test-run>
'@

        (script:Assert-Result).Outcome | Should -Be 'Failed'
    }

    It 'refuses a document that tries to read this machine through a DTD' {
        $null = script:Write-ResultDocument -Content @'
<?xml version="1.0"?>
<!DOCTYPE assemblies [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
<assemblies><assembly><collection><test name="&xxe;" type="X" result="Fail" /></collection></assembly></assemblies>
'@

        { script:Assert-Result } | Should -Throw '*could not be read as XML*'
    }

    It 'refuses a document too large to be a single test result' {
        $path = script:Write-ResultDocument -Content $script:xunitOne
        $stream = [IO.File]::OpenWrite($path)
        try {
            $stream.Seek(5MB, [IO.SeekOrigin]::Begin) | Out-Null
            $stream.WriteByte(32)
        } finally { $stream.Dispose() }

        { script:Assert-Result } | Should -Throw '*exceeds the trusted size limit*'
    }
}

Describe 'A device reproduction must carry the run that proves it' {
    BeforeEach {
        $script:deviceFixture = ConvertTo-ArtifactContractFixture -Fixture (
            New-ValidationFixture -TestType 'DeviceTest')

        function script:Add-AuthoritativeDocument {
            param(
                [object]$Fixture,
                [string]$Method = 'ReproducesReportedFailure',
                [string]$Type = 'Microsoft.Maui.DeviceTests.Issue12345',
                [string]$Result = 'Fail',
                [switch]$Twice
            )

            $body = if ($Twice) {
                @"
      <test name="$Method" type="$Type" method="$Method" result="$Result" />
      <test name="AnotherOne" type="$Type" method="AnotherOne" result="$Result" />
"@
            } else {
                "      <test name=`"$Method`" type=`"$Type`" method=`"$Method`" result=`"$Result`" />"
            }

            $document = @"
<?xml version="1.0" encoding="utf-8"?>
<assemblies>
  <assembly name="Microsoft.Maui.Controls.DeviceTests.dll">
    <errors />
    <collection name="Serialize test">
$body
    </collection>
  </assembly>
</assemblies>
"@
            $target = Join-Path (Join-Path $Fixture.EvidenceDir 'verification') 'verification-test-result.xml'
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Set-Content -LiteralPath $target -Value $document -Encoding utf8NoBOM
        }
    }

    It 'publishes a device reproduction whose run document shows the one test failing' {
        script:Add-AuthoritativeDocument -Fixture $script:deviceFixture

        $result = Invoke-FixtureValidation -Fixture $script:deviceFixture
        $result.validationPassed | Should -BeTrue
    }

    It 'refuses a device reproduction that ships no run document' {
        # Without this the gate believes the summary, and a summary is written
        # by the same run that is being judged.
        { Invoke-FixtureValidation -Fixture $script:deviceFixture } |
            Should -Throw '*no authoritative test result document*'
    }

    It 'refuses a device reproduction whose run selected no test at all' {
        # The exact shape the comma-bearing display name produced: a document
        # exists, the run reported success in reaching the runner, and nothing
        # executed. Without this the gate reads an absent count as a clean run.
        $target = Join-Path (Join-Path $script:deviceFixture.EvidenceDir 'verification') 'verification-test-result.xml'
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        Set-Content -LiteralPath $target -Encoding utf8NoBOM -Value @'
<?xml version="1.0" encoding="utf-8"?>
<assemblies>
  <assembly name="Microsoft.Maui.Controls.DeviceTests.dll" total="0" passed="0" failed="0" skipped="0">
    <errors />
  </assembly>
</assemblies>
'@

        { Invoke-FixtureValidation -Fixture $script:deviceFixture } |
            Should -Throw '*records no executed test*'
    }

    It 'refuses a device reproduction whose run selected two tests' {
        script:Add-AuthoritativeDocument -Fixture $script:deviceFixture -Twice

        { Invoke-FixtureValidation -Fixture $script:deviceFixture } |
            Should -Throw '*records 2 executed tests*'
    }

    It 'refuses a device reproduction whose run shows the test passing' {
        script:Add-AuthoritativeDocument -Fixture $script:deviceFixture -Result 'Pass'

        { Invoke-FixtureValidation -Fixture $script:deviceFixture } |
            Should -Throw '*published as a failing test*'
    }

    It 'refuses a device reproduction whose run executed a different test' {
        script:Add-AuthoritativeDocument -Fixture $script:deviceFixture -Method 'SomethingElseEntirely'

        { Invoke-FixtureValidation -Fixture $script:deviceFixture } |
            Should -Throw '*not the method the manifest claims*'
    }
}
