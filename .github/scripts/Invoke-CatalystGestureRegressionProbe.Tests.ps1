#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ProbePath = Join-Path $PSScriptRoot 'Invoke-CatalystGestureRegressionProbe.ps1'
    $script:PipelinePath = Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml'
    $script:ProbeSource = Get-Content -LiteralPath $script:ProbePath -Raw
    $script:VerificationSource = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot 'shared/Invoke-ReplicationTestVerification.ps1') -Raw
    $script:Pipeline = Get-Content -LiteralPath $script:PipelinePath -Raw
    $script:Stage = [regex]::Match(
        $script:Pipeline,
        '(?ms)^  - stage: ProbeCatalystGestureRegression\r?\n.*?(?=^  - stage:|\z)').Value

    . (Join-Path $PSScriptRoot 'shared/Assert-TrustedTreeAttestation.ps1')
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationExecutionEnvironment.ps1')
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationTestGuard.ps1')
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationCertificationBinding.ps1')
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationAppleAppSandbox.ps1')
    . $script:ProbePath `
        -ExpectedSourceVersion ('a' * 40) `
        -RepositoryRoot $PSScriptRoot `
        -TrustedRoot $PSScriptRoot `
        -TrustedTreeAttestation $script:ProbePath `
        -OutputDirectory $PSScriptRoot

    $script:ScratchRoot = Join-Path $PSScriptRoot (
        ".catalyst-gesture-probe-tests-$PID-$([guid]::NewGuid().ToString('N'))")
    New-Item -ItemType Directory -Path $script:ScratchRoot | Out-Null

    function New-CatalystTestEvidence {
        param(
            [Parameter(Mandatory = $true)][string]$Directory,
            [Parameter(Mandatory = $true)]
            [ValidateSet('Pass', 'Fail')][string]$Outcome,
            [switch]$Incomplete
        )

        New-Item -ItemType Directory -Path $Directory -Force | Out-Null
        $resultPath = Join-Path $Directory 'xunit-test-results.xml'
        $passed = if ($Outcome -ceq 'Pass') { 2 } else { 0 }
        $failed = if ($Outcome -ceq 'Fail') { 2 } else { 0 }
        $rows = foreach ($method in $script:CatalystProbeMethods) {
            $name = "$($script:CatalystProbeClass).$method"
            if ($Outcome -ceq 'Pass') {
                "      <test name=`"$name`" type=`"$($script:CatalystProbeClass)`" method=`"$method`" result=`"Pass`" time=`"0.01`" />"
            } else {
                @"
      <test name="$name" type="$($script:CatalystProbeClass)" method="$method" result="Fail" time="0.01">
        <failure exception-type="Xunit.Sdk.SingleException">
          <message>Assert.Single() Failure: The collection was empty</message>
          <stack-trace>at $($script:CatalystProbeClass).&lt;&gt;c__DisplayClass0_0.&lt;$method&gt;b__0()
at Microsoft.Maui.Controls.HandlerTestBase.InvokeOnMainThreadAsync()</stack-trace>
        </failure>
      </test>
"@
            }
        }
        $xml = @"
<?xml version="1.0" encoding="utf-8"?>
<assemblies>
  <assembly name="Controls.DeviceTests" total="2" passed="$passed" failed="$failed" skipped="0" errors="0" time="0.02">
    <collection total="2" passed="$passed" failed="$failed" skipped="0" name="Gesture">
$($rows -join "`n")
    </collection>
  </assembly>
</assemblies>
"@
        [IO.File]::WriteAllText(
            $resultPath,
            $xml,
            [Text.UTF8Encoding]::new($false))

        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [Xml.XmlReader]::Create($resultPath, $settings)
        try {
            $document = [Xml.XmlDocument]::new()
            $document.XmlResolver = $null
            $document.Load($reader)
        } finally {
            $reader.Dispose()
        }
        $records = foreach ($test in @($document.SelectNodes('/assemblies/assembly//test'))) {
            [ordered]@{
                type = [string]$test.GetAttribute('type')
                method = [string]$test.GetAttribute('method')
                displayName = [string]$test.GetAttribute('name')
                outcome = [string]$test.GetAttribute('result')
                failureSignature = if ($Outcome -ceq 'Fail') {
                    Get-ReplicationDeviceTestFailureSignature -Test $test
                } else { '' }
            }
        }
        $started = [DateTimeOffset]::Parse('2000-01-01T00:00:00Z')
        $strict = [ordered]@{
            schemaVersion = 1
            completed = -not $Incomplete
            runStartedUtc = $started.ToString('O')
            completedUtc = [DateTimeOffset]::UtcNow.ToString('O')
            project = 'Controls'
            platform = 'catalyst'
            testFilter = 'Category=Gesture'
            includeClass = $script:CatalystProbeClass
            records = @($records)
            resultFiles = @(
                [ordered]@{
                    name = [IO.Path]::GetFileName($resultPath)
                    sha256 = (Get-FileHash -LiteralPath $resultPath -Algorithm SHA256).
                        Hash.ToLowerInvariant()
                }
            )
            total = 2
            passed = $passed
            failed = $failed
            skipped = 0
            errors = 0
        }
        $strictPath = Join-Path $Directory 'strict-test-evidence.json'
        [IO.File]::WriteAllText(
            $strictPath,
            (($strict | ConvertTo-Json -Depth 8) + "`n"),
            [Text.UTF8Encoding]::new($false))
        return $strictPath
    }

    function New-CatalystTestTaskDeadline {
        param([int]$RemainingSeconds = 2040)

        $now = [Diagnostics.Stopwatch]::GetTimestamp()
        $frequency = [Diagnostics.Stopwatch]::Frequency
        return [pscustomobject]@{
            EntryTimestamp = $now
            DeadlineTimestamp =
                $now + ([long]$RemainingSeconds * [long]$frequency)
            Frequency = $frequency
            BudgetSeconds = $RemainingSeconds
        }
    }

    function New-CatalystTestCleanupContext {
        param([Parameter(Mandatory = $true)][string]$Name)

        $root = Join-Path $script:ScratchRoot $Name
        $app = Join-Path $root (
            'artifacts/bin/Controls.DeviceTests/Debug/' +
            'net10.0-maccatalyst/maccatalyst-arm64/Controls Tests.app')
        $executable = Join-Path $app 'Contents/MacOS/Microsoft.Maui.Controls.DeviceTests'
        $logs = Join-Path $root 'logs'
        New-Item -ItemType Directory -Path (Split-Path $executable), $logs -Force |
            Out-Null
        Set-Content -LiteralPath (Join-Path $app 'Contents/Info.plist') -Value 'plist'
        Set-Content -LiteralPath $executable -Value 'executable'
        return [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment = [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            LogDirectory = $logs
            ActiveDeadline = New-CatalystTestTaskDeadline -RemainingSeconds 30
            ActiveReserveSeconds = 0
        }
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Fixed report-only Catalyst gesture probe' {
    It 'initializes the real Azure boundary before binding its container result path' {
        $caseRoot = Join-Path $script:ScratchRoot 'real-initialization'
        $repository = Join-Path $caseRoot 'repository'
        $trusted = Join-Path $caseRoot 'trusted'
        $workspace = Join-Path $caseRoot 'workspace'
        $agentTemp = Join-Path $caseRoot 'agent-temp'
        $output = Join-Path $workspace 'proof'
        $runtime = Join-Path $agentTemp 'catalyst-gesture-runtime'
        foreach ($directory in @($repository, $trusted, $workspace, $agentTemp)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $attestation = Join-Path $caseRoot 'trusted-tree.json'
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        $containerResult = Join-Path $caseRoot 'fresh-profile/TestResults.xUnit.xml'
        $taskDeadline = New-CatalystTestTaskDeadline

        Mock Get-CatalystProbeHostFacts {
            [pscustomobject]@{ IsMacOS = $true; Architecture = 'arm64' }
        }
        Mock Get-CatalystProbeContainerResultPath { $containerResult }
        Mock Get-CatalystProbeRuntimeEnvironment {
            [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
        }
        Mock Assert-CatalystProbeTrustedTree {}
        Mock Get-CatalystProbeFileSha256 {
            if ($Path -like '*src/Core/Platform/GestureManager/*') {
                $script:CatalystProbeBaselineFileSha256
            } else {
                $script:CatalystProbeFixtureSha256
            }
        }
        Mock Invoke-CatalystProbeGitText {
            $command = $ArgumentList -join ' '
            if ($command -ceq 'rev-parse HEAD') {
                return $script:CatalystProbeBaselineCommit
            }
            if ($command -like 'cat-file -t *^{commit}') {
                return 'commit'
            }
            if ($command -like 'cat-file -t *') {
                return 'blob'
            }
            if ($command -like "rev-parse $($script:CatalystProbeBaselineCommit):*") {
                return $script:CatalystProbeBaselineBlob
            }
            if ($command -like "rev-parse $($script:CatalystProbeNegativeCommit):*") {
                return $script:CatalystProbeNegativeBlob
            }
            if ($command -like 'diff --name-status *') {
                return "M`t$($script:CatalystProbeProductPath)"
            }
            if ($command -like 'hash-object *') {
                return $script:CatalystProbeBaselineBlob
            }
            if ($command -like 'status --porcelain=v1 *') {
                return ''
            }
            throw "Unexpected initialization Git command: $command"
        }

        $saved = @{}
        $bindings = [ordered]@{
            TF_BUILD = 'true'
            BUILD_SOURCESDIRECTORY = $repository
            CATALYST_PROBE_TRUSTED_ROOT = $trusted
            CATALYST_PROBE_OUTPUT_ROOT = $output
            CATALYST_PROBE_RUNTIME_ROOT = $runtime
            CATALYST_GESTURE_JOB_DEADLINE_UTC =
                [DateTimeOffset]::UtcNow.AddMinutes(60).ToString('O')
            CATALYST_GESTURE_ARTIFACT_TAIL_SECONDS = '300'
            BUILD_SOURCEVERSION = ('a' * 40)
            PIPELINE_WORKSPACE = $workspace
            AGENT_TEMPDIRECTORY = $agentTemp
        }
        try {
            foreach ($name in $bindings.Keys) {
                $saved[$name] = [Environment]::GetEnvironmentVariable($name)
                [Environment]::SetEnvironmentVariable($name, $bindings[$name])
            }
            $context = Initialize-CatalystGestureProbeContext `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot $repository `
                -TrustedRoot $trusted `
                -TrustedTreeAttestation $attestation `
                -OutputDirectory $output `
                -TaskDeadline $taskDeadline
        } finally {
            foreach ($name in $bindings.Keys) {
                [Environment]::SetEnvironmentVariable($name, $saved[$name])
            }
        }

        $context.ContainerResultPath | Should -Be $containerResult
        $context.RuntimeRoot | Should -Be $runtime
        Test-CatalystProbePathOverlap `
            -First $context.RuntimeRoot `
            -Second $context.OutputDirectory | Should -BeFalse
        Should -Invoke Get-CatalystProbeContainerResultPath -Times 1 -Exactly
        Should -Invoke Invoke-CatalystProbeGitText -Times 10 -Exactly
        Should -Invoke Get-CatalystProbeHostFacts -Times 1 -Exactly
    }

    It 'rejects a non-macOS host at the real initialization boundary' {
        $caseRoot = Join-Path $script:ScratchRoot 'non-macos-initialization'
        foreach ($directory in @('repository', 'trusted', 'output')) {
            New-Item -ItemType Directory -Path (Join-Path $caseRoot $directory) `
                -Force | Out-Null
        }
        $attestation = Join-Path $caseRoot 'trusted-tree.json'
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        Mock Get-CatalystProbeHostFacts {
            [pscustomobject]@{ IsMacOS = $false; Architecture = 'x64' }
        }

        {
            Initialize-CatalystGestureProbeContext `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot (Join-Path $caseRoot 'repository') `
                -TrustedRoot (Join-Path $caseRoot 'trusted') `
                -TrustedTreeAttestation $attestation `
                -OutputDirectory (Join-Path $caseRoot 'output') `
                -TaskDeadline (New-CatalystTestTaskDeadline)
        } | Should -Throw '*fresh arm64 macOS Azure job*'
        Should -Invoke Get-CatalystProbeHostFacts -Times 1 -Exactly
        ([regex]::Matches(
                $script:ProbeSource,
                '\[OperatingSystem\]::IsMacOS\(\)')).Count | Should -Be 1
        ([regex]::Matches(
                $script:ProbeSource,
                'RuntimeInformation\]::\s*\r?\n?\s*OSArchitecture')).Count |
            Should -Be 1
    }

    It 'uses only immutable public product and fixture inputs' {
        foreach ($value in @(
                '40590267d8057fd5c044e5bfea77a9dd31fef29f',
                'e456312886ee33fc0e69307e030c3028597ee32e',
                '68393011a77138e20c05194e268e0751b5bb3198',
                '3e79d48f36a378f7618694d1472812288cbd82e0',
                '07ae3f4da0a04ebcf22c2d395994b4dd68fb66c9ae38889c0dc5d782e2655775',
                '9df820c9d684243f88dd4cc39c8090071299cba5e1731e8857e686aec66a0794',
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs')) {
            $script:ProbeSource | Should -Match ([regex]::Escape($value))
        }
        $script:ProbeSource | Should -Match (
            [regex]::Escape("'diff', '--binary', '--full-index', '--no-color'"))
        $script:ProbeSource | Should -Match (
            [regex]::Escape("-Paths @(`$script:CatalystProbeProductPath)"))
        $script:ProbeSource | Should -Match (
            [regex]::Escape("'hash-object'"))
        $script:ProbeSource | Should -Match 'Assert-ReplicationFixSources'
        $script:ProbeSource | Should -Not -Match 'cherry-pick|refs/pull|Issue38291Tests'
        $script:ProbeSource | Should -Match (
            'requires an initially absent container result')
        $script:ProbeSource | Should -Match 'deletesPreexistingContainerResult = \$false'
        $script:ProbeSource | Should -Match "GetEnvironmentVariable\('TF_BUILD'\)"
        $restoreIndex = $script:ProbeSource.LastIndexOf(
            'Invoke-CatalystProbeTrustedRestore -Context $context')
        $fixtureIndex = $script:ProbeSource.LastIndexOf(
            'Copy-CatalystProbeFixture -Context $context')
        $baselineIndex = $script:ProbeSource.LastIndexOf(
            '$result.baseline = Invoke-CatalystProbeCycle')
        $negativeIndex = $script:ProbeSource.LastIndexOf(
            'Enable-CatalystProbeKnownNegative')
        $restoreIndex | Should -BeLessThan $fixtureIndex
        $fixtureIndex | Should -BeLessThan $baselineIndex
        $baselineIndex | Should -BeLessThan $negativeIndex
    }

    It 'runs the same exact two facts through the strict Catalyst boundary' {
        foreach ($value in @(
                'SecondaryToPrimaryCreatesNativeTap',
                'SecondaryToBothCreatesNativeTap',
                'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression',
                'Category=Gesture',
                'Assert.Single')) {
            $script:ProbeSource | Should -Match ([regex]::Escape($value))
        }
        $script:ProbeSource | Should -Match 'Get-ReplicationAppleIsolatedCommand'
        $script:ProbeSource | Should -Match 'Invoke-ReplicationTestVerification\.ps1'
        $script:ProbeSource | Should -Match "'-RegressionEvidence'"
        $script:VerificationSource | Should -Match "'-RequireMacCatalystAppSandbox'"
        $script:VerificationSource | Should -Match (
            'if \(\$Platform -ceq ''catalyst''\) \{ ''maccatalyst'' \}')
        $script:ProbeSource | Should -Not -Match 'Run-DeviceTests\.ps1'
        $script:ProbeSource | Should -Not -Match 'BuildOnly|SkipXcodeVersionCheck'
    }

    It 'accepts a passing baseline and the expected native assertion negative' {
        $baselinePath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'baseline') `
            -Outcome Pass
        $negativePath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'negative') `
            -Outcome Fail

        $baseline = Assert-CatalystProbeCycleEvidence `
            -Kind baseline `
            -StrictEvidencePath $baselinePath
        $negative = Assert-CatalystProbeCycleEvidence `
            -Kind negative `
            -StrictEvidencePath $negativePath

        $baseline.Passed | Should -Be 2
        $negative.Failed | Should -Be 2
        @($baseline.Identities | ForEach-Object { $_.method }) -join "`n" |
            Should -Be (@($negative.Identities | ForEach-Object { $_.method }) -join "`n")
    }

    It 'fails closed when the known-negative unexpectedly passes' {
        $negativePath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'negative-pass') `
            -Outcome Pass
        {
            Assert-CatalystProbeCycleEvidence `
                -Kind negative `
                -StrictEvidencePath $negativePath
        } | Should -Throw '*known-negative did not fail both*'
    }

    It 'fails closed on incomplete strict XML evidence' {
        $incompletePath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'incomplete') `
            -Outcome Fail `
            -Incomplete
        {
            Assert-CatalystProbeCycleEvidence `
                -Kind negative `
                -StrictEvidencePath $incompletePath
        } | Should -Throw '*completed schema-v1 run*'
    }

    It 'rejects slow initialization and patch preparation before native prewarm' {
        $caseRoot = Join-Path $script:ScratchRoot 'slow-patch-preparation'
        New-Item -ItemType Directory -Path $caseRoot -Force | Out-Null
        $attestation = Join-Path $caseRoot 'trusted-tree.json'
        $resultPath = Join-Path $caseRoot 'catalyst-gesture-probe.json'
        $product = Join-Path $caseRoot 'product.cs'
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        Set-Content -LiteralPath $product -Value 'product' -Encoding utf8NoBOM
        $taskDeadline = New-CatalystTestTaskDeadline
        $script:SlowPatchContext = [pscustomobject]@{
            OutputDirectory = $caseRoot
            ResultPath = $resultPath
            FixtureTargetPath = Join-Path $caseRoot 'fixture.cs'
            ProductPath = $product
            JobDeadlineUtc = [DateTimeOffset]::UtcNow.AddHours(1).ToString('O')
            ArtifactTailSeconds = 300
            TaskDeadline = $taskDeadline
            ActiveDeadline = $taskDeadline
            ActiveReserveSeconds = 180
        }

        Mock Read-TrustedTreeAttestation {
            [pscustomobject]@{ treeHash = 'b' * 64 }
        }
        Mock Get-CatalystProbeFileSha256 {
            if ($Path -ceq $product) {
                $script:CatalystProbeBaselineFileSha256
            } else {
                'c' * 64
            }
        }
        Mock Initialize-CatalystGestureProbeContext { $script:SlowPatchContext }
        Mock Assert-CatalystProbeDeadlineAdmission {}
        Mock New-CatalystProbeFixedPatch {
            $minimumSeconds =
                $script:CatalystProbePrewarmBudgetSeconds +
                (2 * $script:CatalystProbeCycleBudgetSeconds) +
                $script:CatalystProbePatchApplyBudgetSeconds +
                $script:CatalystProbeCleanupBudgetSeconds +
                $script:CatalystProbeSummaryBudgetSeconds
            $taskDeadline.DeadlineTimestamp =
                [Diagnostics.Stopwatch]::GetTimestamp() +
                ([long]($minimumSeconds - 1) * [long]$taskDeadline.Frequency)
            Join-Path $caseRoot 'negative.patch'
        }
        Mock Assert-CatalystProbeFixedPatchPolicy {}
        Mock Invoke-CatalystProbeTrustedRestore {}
        Mock Restore-CatalystProbeRepository {}

        {
            Invoke-CatalystGestureRegressionProbeCore `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot $caseRoot `
                -TrustedRoot $caseRoot `
                -TrustedTreeAttestation $attestation `
                -OutputDirectory $caseRoot `
                -TaskDeadline $taskDeadline
        } | Should -Throw '*insufficient monotonic task time for prewarm*'

        Should -Invoke Invoke-CatalystProbeTrustedRestore -Times 0 -Exactly
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.outcome | Should -Be 'inconclusive'
        $result.reason | Should -Be 'probe-failed-closed'
        $result.budget.taskRemainingAtSummarySeconds | Should -BeGreaterThan 0
        $result.budget.azureTerminationReserveSeconds | Should -Be 120
    }

    It 'overrides a demonstrated A/B result when cleanup fails' {
        $caseRoot = Join-Path $script:ScratchRoot 'cleanup-failure'
        New-Item -ItemType Directory -Path $caseRoot -Force | Out-Null
        $attestation = Join-Path $caseRoot 'trusted-tree.json'
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        $resultPath = Join-Path $caseRoot 'catalyst-gesture-probe.json'
        $fixtureTarget = Join-Path $caseRoot 'fixture.cs'
        $product = Join-Path $caseRoot 'product.cs'
        Set-Content -LiteralPath $product -Value 'product' -Encoding utf8NoBOM
        $taskDeadline = New-CatalystTestTaskDeadline
        $script:CoreTestContext = [pscustomobject]@{
            OutputDirectory = $caseRoot
            ResultPath = $resultPath
            FixtureTargetPath = $fixtureTarget
            ProductPath = $product
            JobDeadlineUtc = [DateTimeOffset]::UtcNow.AddHours(1).ToString('O')
            ArtifactTailSeconds = 300
            TaskDeadline = $taskDeadline
            ActiveDeadline = $taskDeadline
            ActiveReserveSeconds = 180
        }
        $identities = @($script:CatalystProbeMethods | ForEach-Object {
            [ordered]@{
                type = $script:CatalystProbeClass
                method = $_
                displayName = "$($script:CatalystProbeClass).$_"
                outcome = 'Pass'
                failureSignature = ''
            }
        })

        Mock Read-TrustedTreeAttestation {
            [pscustomobject]@{ treeHash = 'b' * 64 }
        }
        Mock Initialize-CatalystGestureProbeContext { $script:CoreTestContext }
        Mock New-CatalystProbeFixedPatch { Join-Path $caseRoot 'negative.patch' }
        Mock Assert-CatalystProbeFixedPatchPolicy {}
        Mock Copy-CatalystProbeFixture {}
        Mock Invoke-CatalystProbeTrustedRestore {}
        Mock Enable-CatalystProbeKnownNegative {}
        Mock Assert-CatalystProbeOwnedContainerResult { 'c' * 64 }
        Mock Assert-CatalystProbeDeadlineAdmission {}
        Mock Invoke-CatalystProbeCycle {
            [pscustomobject]@{
                identities = $identities
                passed = if ($Kind -ceq 'baseline') { 2 } else { 0 }
                failed = if ($Kind -ceq 'negative') { 2 } else { 0 }
            }
        }
        Mock Restore-CatalystProbeRepository {
            throw 'simulated cleanup refusal'
        }

        {
            Invoke-CatalystGestureRegressionProbeCore `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot $caseRoot `
                -TrustedRoot $caseRoot `
                -TrustedTreeAttestation $attestation `
                -OutputDirectory $caseRoot `
                -TaskDeadline $taskDeadline
        } | Should -Throw '*simulated cleanup refusal*'

        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.outcome | Should -Be 'inconclusive'
        $result.reason | Should -Be 'cleanup-failed-closed'
        $result.cleanup.completed | Should -BeFalse
    }

    It 'shares one cleanup deadline across three Git operations and retains a failure summary' {
        $caseRoot = Join-Path $script:ScratchRoot 'shared-cleanup-deadline'
        $logs = Join-Path $caseRoot 'logs'
        New-Item -ItemType Directory -Path $logs -Force | Out-Null
        $attestation = Join-Path $caseRoot 'trusted-tree.json'
        $resultPath = Join-Path $caseRoot 'catalyst-gesture-probe.json'
        $fixtureTarget = Join-Path $caseRoot 'fixture.cs'
        $product = Join-Path $caseRoot 'product.cs'
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        Set-Content -LiteralPath $fixtureTarget -Value 'fixture' -Encoding utf8NoBOM
        Set-Content -LiteralPath $product -Value 'product' -Encoding utf8NoBOM
        $taskDeadline = New-CatalystTestTaskDeadline
        $environment = [Collections.Generic.Dictionary[string, string]]::new(
            [StringComparer]::Ordinal)
        $script:SharedCleanupContext = [pscustomobject]@{
            ExpectedSourceVersion = 'a' * 40
            RepositoryRoot = $caseRoot
            TrustedRoot = $caseRoot
            TrustedTreeAttestation = $attestation
            OutputDirectory = $caseRoot
            ResultPath = $resultPath
            LogDirectory = $logs
            FixtureTargetPath = $fixtureTarget
            ProductPath = $product
            InitialRepositoryStatus = ''
            RuntimeEnvironment = $environment
            JobDeadlineUtc = [DateTimeOffset]::UtcNow.AddHours(1).ToString('O')
            ArtifactTailSeconds = 300
            TaskDeadline = $taskDeadline
            ActiveDeadline = $taskDeadline
            ActiveReserveSeconds = 180
        }
        $identities = @($script:CatalystProbeMethods | ForEach-Object {
            [ordered]@{
                type = $script:CatalystProbeClass
                method = $_
                displayName = "$($script:CatalystProbeClass).$_"
            }
        })
        $script:CleanupGitCalls = 0
        $script:CleanupDeadlineReferences =
            [Collections.Generic.List[object]]::new()
        $script:CleanupEffectiveTimeouts =
            [Collections.Generic.List[int]]::new()

        Mock Read-TrustedTreeAttestation {
            [pscustomobject]@{ treeHash = 'b' * 64 }
        }
        Mock Get-CatalystProbeFileSha256 {
            if ($Path -ceq $product) {
                $script:CatalystProbeBaselineFileSha256
            } else {
                'c' * 64
            }
        }
        Mock Initialize-CatalystGestureProbeContext {
            $script:SharedCleanupContext
        }
        Mock New-CatalystProbeFixedPatch { Join-Path $caseRoot 'negative.patch' }
        Mock Assert-CatalystProbeFixedPatchPolicy {}
        Mock Invoke-CatalystProbeTrustedRestore {}
        Mock Copy-CatalystProbeFixture {}
        Mock Enable-CatalystProbeKnownNegative {}
        Mock Assert-CatalystProbeOwnedContainerResult { 'd' * 64 }
        Mock Assert-CatalystProbeDeadlineAdmission {}
        Mock Invoke-CatalystProbeCycle {
            [pscustomobject]@{
                identities = $identities
                passed = if ($Kind -ceq 'baseline') { 2 } else { 0 }
                failed = if ($Kind -ceq 'negative') { 2 } else { 0 }
            }
        }
        Mock Stop-CatalystProbeOwnedApplication { 0 }
        Mock Assert-CatalystProbeTrustedTree {}
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:CleanupGitCalls++
            $script:CleanupDeadlineReferences.Add($TaskDeadline)
            $effective = Get-CatalystProbeProcessTimeoutSeconds `
                -Deadline $TaskDeadline `
                -RequestedSeconds $TimeoutSeconds `
                -ReserveSeconds $ReserveSeconds `
                -Description 'mock cleanup Git operation'
            $script:CleanupEffectiveTimeouts.Add($effective)
            if ($script:CleanupGitCalls -eq 1) {
                $TaskDeadline.DeadlineTimestamp =
                    [Diagnostics.Stopwatch]::GetTimestamp() +
                    (2 * [long]$TaskDeadline.Frequency)
            } elseif ($script:CleanupGitCalls -eq 2) {
                $TaskDeadline.DeadlineTimestamp =
                    [Diagnostics.Stopwatch]::GetTimestamp()
            }
            [pscustomobject]@{
                TimedOut = $false
                ExitCode = 0
                Stdout = if ($ArgumentList[0] -ceq 'hash-object') {
                    $script:CatalystProbeBaselineBlob
                } else {
                    ''
                }
            }
        }

        {
            Invoke-CatalystGestureRegressionProbeCore `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot $caseRoot `
                -TrustedRoot $caseRoot `
                -TrustedTreeAttestation $attestation `
                -OutputDirectory $caseRoot `
                -TaskDeadline $taskDeadline
        } | Should -Throw '*Catalyst probe cleanup failed*monotonic task deadline*'

        $script:CleanupGitCalls | Should -Be 3
        $script:CleanupDeadlineReferences.Count | Should -Be 3
        foreach ($deadlineReference in $script:CleanupDeadlineReferences) {
            [object]::ReferenceEquals(
                $script:CleanupDeadlineReferences[0],
                $deadlineReference) | Should -BeTrue
        }
        $script:CleanupEffectiveTimeouts.Count | Should -Be 2
        $script:CleanupEffectiveTimeouts[0] | Should -BeLessOrEqual 120
        $script:CleanupEffectiveTimeouts[1] | Should -BeLessOrEqual 2
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $result.outcome | Should -Be 'inconclusive'
        $result.reason | Should -Be 'cleanup-failed-closed'
        $result.budget.taskRemainingAtSummarySeconds | Should -BeGreaterThan 0
        $result.budget.azureTaskSeconds | Should -Be 2160
        $result.budget.azureTerminationReserveSeconds | Should -Be 120
    }

    It 'strips credentials and Azure commands from every bounded child log' {
        $script:ProbeSource | Should -Match 'startInfo\.Environment\.Clear\(\)'
        $script:ProbeSource | Should -Match 'Assert-ReplicationExecutionEnvironment'
        $script:ProbeSource | Should -Match 'Invoke-WithoutReplicationSecrets'
        foreach ($name in @(
                'GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN',
                'SYSTEM_ACCESSTOKEN', 'AZURE_DEVOPS_EXT_PAT')) {
            $script:ProbeSource | Should -Match ([regex]::Escape("'$name'"))
        }
        (ConvertTo-CatalystProbeLogData `
            -Text '##vso[task.setvariable variable=x]secret ##[error]message') |
            Should -Be 'secret message'

        $savedToken = $env:GH_TOKEN
        try {
            $env:GH_TOKEN = 'unit-test-token-that-must-not-be-inherited'
            $environment = Get-CatalystProbeRuntimeEnvironment `
                -RuntimeRoot (Join-Path $script:ScratchRoot 'private-runtime')
            $environment.ContainsKey('GH_TOKEN') | Should -BeFalse
            $environment['NUGET_PACKAGES'] | Should -Match (
                [regex]::Escape((Join-Path $script:ScratchRoot 'private-runtime')))
            {
                Assert-ReplicationExecutionEnvironment -Environment $environment
            } | Should -Not -Throw
        } finally {
            $env:GH_TOKEN = $savedToken
        }
    }

    It 'bounds redirected pipe draining when an exited parent leaves a live descendant' {
        $root = Join-Path $script:ScratchRoot 'inherited-pipe'
        New-Item -ItemType Directory -Path $root | Out-Null
        $parentPath = Join-Path $root 'parent.ps1'
        $recordPath = Join-Path $root 'owned-child.txt'
        $pwshPath = (Get-Command pwsh -CommandType Application |
            Select-Object -First 1).Source
        $escapedPwsh = $pwshPath.Replace("'", "''")
        $escapedRecord = $recordPath.Replace("'", "''")
        $parentSource = @"
`$info = [Diagnostics.ProcessStartInfo]::new()
`$info.FileName = '$escapedPwsh'
`$info.UseShellExecute = `$false
`$info.ArgumentList.Add('-NoProfile')
`$info.ArgumentList.Add('-NonInteractive')
`$info.ArgumentList.Add('-Command')
`$info.ArgumentList.Add('[Threading.Thread]::Sleep(20000)')
`$child = [Diagnostics.Process]::Start(`$info)
[IO.File]::WriteAllText('$escapedRecord', "`$(`$child.Id)|`$(`$child.StartTime.ToUniversalTime().Ticks)")
[Environment]::Exit(0)
"@
        [IO.File]::WriteAllText($parentPath, $parentSource)
        $environment = Get-CatalystProbeRuntimeEnvironment `
            -RuntimeRoot (Join-Path $root 'runtime')
        $savedTermination = $script:CatalystProbeProcessTerminationSeconds
        $script:CatalystProbeProcessTerminationSeconds = 1
        $elapsed = [Diagnostics.Stopwatch]::StartNew()
        try {
            {
                Invoke-CatalystProbeBoundedProcess `
                    -FileName $pwshPath `
                    -ArgumentList @('-NoProfile', '-NonInteractive', '-File', $parentPath) `
                    -WorkingDirectory $root `
                    -Environment $environment `
                    -TimeoutSeconds 5 `
                    -TaskDeadline (New-CatalystTestTaskDeadline -RemainingSeconds 30) `
                    -LogPath (Join-Path $root 'parent.log')
            } | Should -Throw '*redirected streams exceeded the command deadline*'
            $elapsed.Elapsed.TotalSeconds | Should -BeLessThan 10
            Test-Path -LiteralPath $recordPath | Should -BeTrue
        } finally {
            $script:CatalystProbeProcessTerminationSeconds = $savedTermination
            if (Test-Path -LiteralPath $recordPath) {
                $record = (Get-Content -LiteralPath $recordPath -Raw) -split '\|'
                $held = $null
                try {
                    $held = [Diagnostics.Process]::GetProcessById([int]$record[0])
                    if (-not $held.HasExited -and
                        $held.StartTime.ToUniversalTime().Ticks -eq [long]$record[1]) {
                        $held.Kill($true)
                        $held.WaitForExit(2000) | Should -BeTrue
                    }
                } catch [ArgumentException] {
                    # The owned child may already have completed before cleanup.
                } finally {
                    if ($held) { $held.Dispose() }
                }
            }
        }
    }

    It 'fails bounded owned-app cleanup when signature validation stalls' {
        $context = New-CatalystTestCleanupContext -Name 'stalled-signature'
        Mock Invoke-CatalystProbeBoundedProcess {
            [object]::ReferenceEquals($TaskDeadline, $context.ActiveDeadline) |
                Should -BeTrue
            $ReserveSeconds | Should -Be 0
            $FileName | Should -BeExactly '/usr/bin/codesign'
            [pscustomobject]@{
                ExitCode = 124
                TimedOut = $true
                Stdout = ''
                Stderr = ''
            }
        }
        Mock Get-Process { throw 'No process may be killed without validated identity.' }

        { Stop-CatalystProbeOwnedApplication -Context $context } |
            Should -Throw '*cleanup command failed: cleanup-signature*'
        Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 1 -Exactly
        Should -Invoke Get-Process -Times 0 -Exactly
    }

    It 'keeps cycle execution cleanup and evidence inside the same reserved deadline' {
        $root = Join-Path $script:ScratchRoot 'cycle-deadline'
        New-Item -ItemType Directory -Path $root | Out-Null
        $originalDeadline = New-CatalystTestTaskDeadline
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            TrustedRoot = $root
            OutputDirectory = $root
            LogDirectory = $root
            RuntimeEnvironment = [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            ActiveDeadline = $originalDeadline
            ActiveReserveSeconds = 180
        }
        $script:CycleExecutionDeadline = $null
        $script:CycleCleanupDeadline = $null
        Mock Get-ReplicationAppleIsolatedCommand {
            [pscustomobject]@{
                FilePath = 'pwsh'; Arguments = @(); Environment = $context.RuntimeEnvironment
            }
        }
        Mock Assert-CatalystProbeTrustedTree {}
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:CycleExecutionDeadline = $TaskDeadline
            $ReserveSeconds | Should -Be 30
            New-CatalystTestEvidence -Directory (Join-Path $root 'baseline') -Outcome Pass |
                Out-Null
            [pscustomobject]@{
                ExitCode = 0; TimedOut = $false; StartedUtc = 'start'
                CompletedUtc = 'end'; LogSha256 = 'a' * 64
            }
        }
        Mock Stop-CatalystProbeOwnedApplication {
            $script:CycleCleanupDeadline = $Context.ActiveDeadline
            $Context.ActiveReserveSeconds | Should -Be 10
            return 0
        }

        $result = Invoke-CatalystProbeCycle -Kind baseline -Context $context

        $result.passed | Should -Be 2
        [object]::ReferenceEquals(
            $script:CycleExecutionDeadline, $script:CycleCleanupDeadline) | Should -BeTrue
        $script:CycleExecutionDeadline.BudgetSeconds | Should -BeLessOrEqual 480
        [object]::ReferenceEquals($context.ActiveDeadline, $originalDeadline) | Should -BeTrue
        $context.ActiveReserveSeconds | Should -Be 180
    }

    It 'shares the cleanup deadline across signed identity commands and owned process waits' {
        $context = New-CatalystTestCleanupContext -Name 'owned-process-wait'
        $script:CleanupCommandDeadlines = [Collections.Generic.List[object]]::new()
        $script:OwnedCleanupWait = 0
        $script:OwnedCleanupKills = 0
        $script:ForeignCleanupKills = 0
        $ownedPath = Join-Path $context.RepositoryRoot (
            'artifacts/bin/Controls.DeviceTests/Debug/net10.0-maccatalyst/' +
            'maccatalyst-arm64/Controls Tests.app/Contents/MacOS/Microsoft.Maui.Controls.DeviceTests')
        $foreignPath = Join-Path $context.RepositoryRoot 'foreign-executable'
        Set-Content -LiteralPath $foreignPath -Value 'unowned'
        $owned = [pscustomobject]@{
            HasExited = $false; Path = $ownedPath; StartTime = [DateTime]::UtcNow
        }
        $foreign = [pscustomobject]@{
            HasExited = $false; Path = $foreignPath; StartTime = [DateTime]::UtcNow
        }
        $owned | Add-Member ScriptMethod Kill {
            param($Tree)
            $script:OwnedCleanupKills++
        }
        $owned | Add-Member ScriptMethod WaitForExit {
            param($Milliseconds)
            $script:OwnedCleanupWait = $Milliseconds
            return $true
        }
        $owned | Add-Member ScriptMethod Dispose {}
        $foreign | Add-Member ScriptMethod Kill { $script:ForeignCleanupKills++ }
        $foreign | Add-Member ScriptMethod Dispose {}
        Mock Get-Process { @($foreign, $owned) }
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:CleanupCommandDeadlines.Add($TaskDeadline)
            $text = if ($ArgumentList -contains 'Print :CFBundleExecutable') {
                'Microsoft.Maui.Controls.DeviceTests'
            } elseif ($ArgumentList -contains 'Print :CFBundleIdentifier') {
                $TaskDeadline.DeadlineTimestamp =
                    [Diagnostics.Stopwatch]::GetTimestamp() +
                    (3 * [long]$TaskDeadline.Frequency)
                'com.microsoft.maui.controls.devicetests'
            } elseif ($ArgumentList -contains '--entitlements') {
                '<plist><dict><key>com.apple.security.app-sandbox</key><true/></dict></plist>'
            } else { '' }
            [pscustomobject]@{ ExitCode = 0; TimedOut = $false; Stdout = $text; Stderr = '' }
        }

        Stop-CatalystProbeOwnedApplication -Context $context | Should -Be 1
        $script:CleanupCommandDeadlines | Should -HaveCount 4
        foreach ($deadline in $script:CleanupCommandDeadlines) {
            [object]::ReferenceEquals($deadline, $context.ActiveDeadline) |
                Should -BeTrue
        }
        $script:OwnedCleanupKills | Should -Be 1
        $script:ForeignCleanupKills | Should -Be 0
        $script:OwnedCleanupWait | Should -BeGreaterThan 0
        $script:OwnedCleanupWait | Should -BeLessOrEqual 3000
    }

    It 'keeps private SDK and package caches outside every published proof path' {
            $caseRoot = Join-Path $script:ScratchRoot 'private-cache-separation'
            $agentTemp = Join-Path $caseRoot 'agent-temp'
            $output = Join-Path $caseRoot 'pipeline-workspace/proof'
            $repository = Join-Path $caseRoot 'pipeline-workspace/repository'
            $trusted = Join-Path $caseRoot 'artifact-staging/trusted'
            foreach ($directory in @($agentTemp, $output, $repository, $trusted)) {
                New-Item -ItemType Directory -Path $directory -Force | Out-Null
            }
            $runtime = Join-Path $agentTemp 'catalyst-gesture-runtime'
            $resolved = Resolve-CatalystProbePrivateRuntimeRoot `
                -RuntimeRoot $runtime `
                -AgentTempDirectory $agentTemp `
                -OutputDirectory $output `
                -RepositoryRoot $repository `
                -TrustedRoot $trusted
            $environment = Get-CatalystProbeRuntimeEnvironment -RuntimeRoot $resolved

            Test-CatalystProbePathOverlap -First $resolved -Second $output |
                Should -BeFalse
            Test-CatalystProbePathOverlap `
                -First $environment['NUGET_PACKAGES'] `
                -Second $output | Should -BeFalse
            Test-CatalystProbePathOverlap `
                -First $environment['DOTNET_CLI_HOME'] `
                -Second $output | Should -BeFalse
            $script:Stage | Should -Match (
                'CatalystGestureRuntimeRoot: \$\(Agent\.TempDirectory\)/catalyst-gesture-runtime')
            $script:Stage | Should -Match "targetPath: '\$\(CatalystGestureProbeRoot\)'"
            $script:Stage | Should -Not -Match (
                "targetPath: '\$\(CatalystGestureRuntimeRoot\)'")
            $resultJson = New-CatalystProbeResult `
                -ExpectedSourceVersion ('a' * 40) `
                -TrustedTreeHash ('b' * 64) `
                -TrustedTreeAttestationSha256 ('c' * 64) |
                ConvertTo-Json -Depth 12
            $resultJson | Should -Not -Match (
                'nuget-packages|dotnet-home|unit-test-token|agent-temp')

            {
                Resolve-CatalystProbePrivateRuntimeRoot `
                    -RuntimeRoot (Join-Path $output 'runtime') `
                    -AgentTempDirectory $agentTemp `
                    -OutputDirectory $output `
                    -RepositoryRoot $repository `
                    -TrustedRoot $trusted
            } | Should -Throw '*fixed Agent.TempDirectory root*'
    }

    It 'rejects the former retry envelope and reserves a runnable evidence tail' {
            $started = [DateTimeOffset]::Parse('2026-09-12T00:00:00Z')
            $fixed = Get-CatalystProbeFixedPipelineBudget -StartedUtc $started
            $fixed.requiredSeconds | Should -Be 4920
            $fixed.slackSeconds | Should -Be 480
            $fixed.configuredTaskEnvelopeSeconds | Should -Be 5220
            $fixed.configuredTaskSlackSeconds | Should -Be 180
            @($fixed.phases | Where-Object { $_.retryCount -ne 0 }).Count |
                Should -Be 0
            ($fixed.checkoutTimeoutSeconds +
                $fixed.validationTimeoutSeconds +
                $fixed.availableAfterValidationSeconds) | Should -BeLessOrEqual 6000

            $taskTimeouts = [ordered]@{
                'Validate closed Catalyst probe inputs before setup' = 2
                'Capture fixed Catalyst trusted tree' = 2
                'Attest fixed Catalyst trusted tree' = 3
                'Fetch fixed public product objects and detach baseline' = 4
                'Select fixed preinstalled Catalyst Xcode' = 3
                'Restore pinned baseline tools' = 5
                'Provision baseline Catalyst SDK and workloads' = 17
                'Build baseline MSBuild tasks only' = 12
                'Run fixed report-only Catalyst gesture A/B' = 36
                'Preserve report-only Catalyst probe evidence' = 5
            }
            $script:Stage | Should -Match (
                '(?ms)- checkout: self.*?persistCredentials: false\r?\n' +
                '\s+timeoutInMinutes: 5')
            $configuredMinutes = 5 # checkout
            foreach ($entry in $taskTimeouts.GetEnumerator()) {
                $match = [regex]::Match(
                    $script:Stage,
                    "(?ms)displayName: '$([regex]::Escape($entry.Key))'\r?\n" +
                    "\s+(?:condition: .*\r?\n\s+)?timeoutInMinutes: (\d+)")
                $match.Success | Should -BeTrue -Because (
                    "$($entry.Key) must retain a bounded timeout")
                [int]$match.Groups[1].Value | Should -Be $entry.Value
                $configuredMinutes += [int]$match.Groups[1].Value
            }
            $configuredMinutes | Should -Be 94
            $configuredMinutes | Should -BeLessThan 100

            $formerEnvelope = @(
                [ordered]@{
                    name = 'workload-bootstrap'
                    maximumSeconds = 1800
                    retryCount = 2
                }
                [ordered]@{
                    name = 'build-tasks'
                    maximumSeconds = 1200
                    retryCount = 0
                }
                [ordered]@{
                    name = 'report-only-probe'
                    maximumSeconds = 2400
                    retryCount = 0
                }
            )
            {
                Assert-CatalystProbeBudgetSchedule `
                    -Phases $formerEnvelope `
                    -AvailableSeconds 5400 `
                    -ArtifactTailSeconds 300
            } | Should -Throw '*worst-case phase and retry envelope*'

            $deadline = $started.AddSeconds(1000).ToString('O')
            {
                Assert-CatalystProbeDeadlineAdmission `
                    -DeadlineUtc $deadline `
                    -Phase 'bounded test' `
                    -PhaseBudgetSeconds 600 `
                    -RemainingPhaseBudgetSeconds 100 `
                    -ArtifactTailSeconds 300 `
                    -NowUtc $started
            } | Should -Not -Throw
            {
                Assert-CatalystProbeDeadlineAdmission `
                    -DeadlineUtc $deadline `
                    -Phase 'late bounded test' `
                    -PhaseBudgetSeconds 600 `
                    -RemainingPhaseBudgetSeconds 100 `
                    -ArtifactTailSeconds 300 `
                    -NowUtc $started.AddSeconds(1)
            } | Should -Throw '*evidence-publication tail*'
    }

    It 'derives one fixed monotonic deadline from the final task entry' {
        $names = @(
            'CATALYST_PROBE_TASK_ENTRY_TIMESTAMP',
            'CATALYST_PROBE_TASK_STOPWATCH_FREQUENCY',
            'CATALYST_PROBE_TASK_BUDGET_SECONDS')
        $saved = @{}
        try {
            foreach ($name in $names) {
               $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            }
            [Environment]::SetEnvironmentVariable(
               $names[0],
               [string][Diagnostics.Stopwatch]::GetTimestamp())
            [Environment]::SetEnvironmentVariable(
               $names[1],
               [string][Diagnostics.Stopwatch]::Frequency)
            [Environment]::SetEnvironmentVariable($names[2], '2040')

            $deadline = New-CatalystProbeTaskDeadline
            $deadline.BudgetSeconds | Should -Be 2040
            $script:CatalystProbePrewarmBudgetSeconds | Should -Be 600
            $script:CatalystProbeCycleBudgetSeconds | Should -Be 480
            $script:CatalystProbeVerifierBudgetSeconds | Should -Be 450
            $script:CatalystProbePatchApplyBudgetSeconds | Should -Be 120
            $script:CatalystProbeCleanupBudgetSeconds | Should -Be 120
            $script:CatalystProbeSummaryBudgetSeconds | Should -Be 60
            $script:CatalystProbeCoordinationBudgetSeconds | Should -Be 180
            $script:CatalystProbeTaskTimeoutSeconds | Should -Be 2160
            ($script:CatalystProbeCoordinationBudgetSeconds +
                $script:CatalystProbePrewarmBudgetSeconds +
                (2 * $script:CatalystProbeCycleBudgetSeconds) +
                $script:CatalystProbePatchApplyBudgetSeconds +
                $script:CatalystProbeCleanupBudgetSeconds +
                $script:CatalystProbeSummaryBudgetSeconds) | Should -Be 2040
            (Get-CatalystProbeDeadlineRemainingSeconds -Deadline $deadline) |
               Should -BeGreaterThan 2037

            [Environment]::SetEnvironmentVariable($names[2], '2041')
            { New-CatalystProbeTaskDeadline } |
               Should -Throw '*fixed monotonic task-entry deadline*'
        } finally {
            foreach ($name in $names) {
               [Environment]::SetEnvironmentVariable($name, $saved[$name])
            }
        }

        ([regex]::Matches(
               $script:Stage,
               'CATALYST_PROBE_TASK_ENTRY_TIMESTAMP')).Count | Should -Be 1
        $script:Stage | Should -Match (
            "(?ms)- pwsh: \|\r?\n" +
            "\s+\`$env:CATALYST_PROBE_TASK_ENTRY_TIMESTAMP = " +
            "\[string\]\[Diagnostics\.Stopwatch\]::GetTimestamp\(\).*?" +
            "displayName: 'Run fixed report-only Catalyst gesture A/B'\r?\n" +
            "\s+timeoutInMinutes: 36")

        $tokens = $null
        $parseErrors = $null
        $ast = [Management.Automation.Language.Parser]::ParseFile(
            $script:ProbePath,
            [ref]$tokens,
            [ref]$parseErrors)
        $parseErrors.Count | Should -Be 0
        $boundedCalls = @($ast.FindAll({
            param($node)
            $node -is [Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -ceq 'Invoke-CatalystProbeBoundedProcess'
        }, $true))
        $boundedCalls.Count | Should -Be 6
        foreach ($call in $boundedCalls) {
            $call.Extent.Text | Should -Match '-TaskDeadline'
        }
        $script:ProbeSource | Should -Match (
            'WaitForExit\(\[int\]\$processWaitMilliseconds\)')
        $script:ProbeSource | Should -Match (
            'Get-CatalystProbeDeadlineRemainingMilliseconds -Deadline \$operationDeadline')
        $script:ProbeSource | Should -Not -Match 'WaitForExit\(30000\)'
    }
}

Describe 'Catalyst gesture pipeline isolation' {
    It 'routes one standalone fresh-Azure report-only mode' {
        $script:Pipeline | Should -Match '(?m)^\s+- catalyst-gesture-probe\s*$'
        $script:Stage | Should -Not -BeNullOrEmpty
        $script:Stage | Should -Match (
            "eq\('\$\{\{ parameters\.Mode \}\}', 'catalyst-gesture-probe'\)")
        $script:Stage | Should -Match 'pool: \$\{\{ parameters\.macPool \}\}'
        $script:Stage | Should -Match 'workspace:\s+clean: all'
        $script:Stage | Should -Match 'persistCredentials: false'
        $script:Stage | Should -Match 'restricted to dotnet/maui Azure definition 27723'
        $script:Stage | Should -Match 'refs/heads/copilot/replicate-issues-pipeline'
        $script:Stage | Should -Match 'artifact: ''CatalystGestureRegressionProbe'''
    }

    It 'rejects every open input combination before setup or fetch' {
        foreach ($value in @(
                'PARAM_MODE', 'PARAM_PLATFORM', 'PARAM_PR_NUMBER',
                'PARAM_ISSUE_NUMBER', 'PARAM_REVIEW_REPOSITORY',
                'PARAM_PUBLISH_OUTCOME', 'PARAM_SUPERSEDE',
                'PARAM_ANDROID_PREFLIGHT', 'PARAM_ANDROID_NATIVE_PROBE')) {
            $script:Stage | Should -Match ([regex]::Escape($value))
        }
        $validate = $script:Stage.IndexOf(
            "displayName: 'Validate closed Catalyst probe inputs before setup'")
        $capture = $script:Stage.IndexOf(
            "displayName: 'Capture fixed Catalyst trusted tree'")
        $fetch = $script:Stage.IndexOf(
            "displayName: 'Fetch fixed public product objects and detach baseline'")
        $run = $script:Stage.IndexOf(
            "displayName: 'Run fixed report-only Catalyst gesture A/B'")
        $validate | Should -BeGreaterThan -1
        $validate | Should -BeLessThan $capture
        $capture | Should -BeLessThan $fetch
        $fetch | Should -BeLessThan $run
    }

    It 'does not inherit credentials or enter review replication publication or model paths' {
        $script:Stage | Should -Not -Match (
            'persistCredentials: true|GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|' +
            'SYSTEM_ACCESSTOKEN|Replicate-Issue\.ps1|Publish-ReplicationPR\.ps1|' +
            'PublishTestResults|Invoke-Copilot|COPILOT_CLI|agentic|' +
            'SkipXcodeVersionCheck')
        $script:Stage | Should -Match 'invokesModel = \$false'
        $script:Stage | Should -Not -Match 'template: common/provision\.yml'
        $script:Stage | Should -Not -Match 'retryCountOnTaskFailure'
        $script:Stage | Should -Match 'Select fixed preinstalled Catalyst Xcode'
        $script:Stage | Should -Match 'https://github\.com/dotnet/maui\.git'
        $script:Stage | Should -Match 'env -i PATH="\$PATH" HOME="\$HOME"'
    }

    It 'prewarms only the Catalyst graph and keeps both cycles bounded' {
        foreach ($value in @(
                'IncludeMacCatalystTargetFrameworks=true',
                'IncludeIosTargetFrameworks=false',
                'IncludeAndroidTargetFrameworks=false',
                'IncludeWindowsTargetFrameworks=false',
                'IncludeMacOSTargetFrameworks=false',
                '--no-dependencies',
                'maccatalyst-arm64')) {
            $script:ProbeSource | Should -Match ([regex]::Escape($value))
        }
        $script:Stage | Should -Match '--target=dotnet-buildtasks'
        $script:Stage | Should -Match 'BuildTaskOnlyBuild=true'
        $script:ProbeSource | Should -Match 'CatalystProbeCycleBudgetSeconds = 480'
        $script:ProbeSource | Should -Match 'cycleCountMaximum = 2'
        $script:ProbeSource | Should -Match 'retries = 0'
        $script:Stage | Should -Match 'timeoutInMinutes: 36'
        $script:Stage | Should -Match 'timeoutInMinutes: 5'
        $script:Stage | Should -Match 'CATALYST_GESTURE_JOB_DEADLINE_UTC'
        $script:Stage | Should -Match 'CATALYST_GESTURE_ARTIFACT_TAIL_SECONDS'
        $script:Stage | Should -Match (
            "(?s)Preserve report-only Catalyst probe evidence.*?condition: always\(\)")
    }
}
