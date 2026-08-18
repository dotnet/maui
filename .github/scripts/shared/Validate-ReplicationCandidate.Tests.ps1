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

public class $TestName
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
            [string]$BaseCommit
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
            'VERIFICATION PASSED'
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

Describe 'The publisher refuses a test that asserts its own handler registration' {
    It 'rejects the self-fulfilling registration at publish time' {
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content @'
    handlers.AddHandler<Entry, EntryHandler2>();
    Assert.IsType<EntryHandler2>(entry.Handler);
'@ `
                -Path 'Issue37275.Android.cs'
        } | Should -Throw '*can only confirm the test setup*'
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
