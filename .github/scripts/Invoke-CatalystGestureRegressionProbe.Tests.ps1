#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ProbePath = Join-Path $PSScriptRoot 'Invoke-CatalystGestureRegressionProbe.ps1'
    $script:ProductionPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
    $script:ProductionSource = Get-Content -LiteralPath $script:ProductionPath -Raw
    $script:PrewarmPath = Join-Path $PSScriptRoot (
        'shared/Replication-AppleCompanionPrewarm.ps1')
    $script:PipelinePath = Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml'
    $script:ProbeSource = Get-Content -LiteralPath $script:ProbePath -Raw
    $script:PrewarmSource = Get-Content -LiteralPath $script:PrewarmPath -Raw
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
    . (Join-Path $PSScriptRoot 'shared/Replication-AppleCompanionPrewarm.ps1')
    $productionTokens = $null
    $productionParseErrors = $null
    $productionAst = [Management.Automation.Language.Parser]::ParseFile(
        $script:ProductionPath,
        [ref]$productionTokens,
        [ref]$productionParseErrors)
    if ($productionParseErrors.Count -ne 0) {
        throw 'Replicate-Issue.ps1 did not parse while loading the production writer.'
    }
    $productionWriter = @($productionAst.FindAll({
                param($node)
                $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq 'Write-ReplicationRegressionEvidenceDocument'
            }, $true))
    if ($productionWriter.Count -ne 1) {
        throw 'The production regression evidence writer was not found exactly once.'
    }
    Invoke-Expression $productionWriter[0].Extent.Text
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
            [switch]$Incomplete,
            [string]$Platform = 'catalyst',
            [string]$Category = 'Gesture',
            [string]$Class = $script:CatalystProbeClass,
            [string[]]$Methods = $script:CatalystProbeMethods
        )

        New-Item -ItemType Directory -Path $Directory -Force | Out-Null
        $resultPath = Join-Path $Directory 'xunit-test-results.xml'
        $passed = if ($Outcome -ceq 'Pass') { 2 } else { 0 }
        $failed = if ($Outcome -ceq 'Fail') { 2 } else { 0 }
        $rows = foreach ($method in $Methods) {
            $name = "$Class.$method"
            if ($Outcome -ceq 'Pass') {
                "      <test name=`"$name`" type=`"$Class`" method=`"$method`" result=`"Pass`" time=`"0.01`" />"
            } else {
                @"
      <test name="$name" type="$Class" method="$method" result="Fail" time="0.01">
        <failure exception-type="Xunit.Sdk.SingleException">
          <message>Assert.Single() Failure: The collection was empty</message>
          <stack-trace>at $Class.&lt;&gt;c__DisplayClass0_0.&lt;$method&gt;b__0()
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
    <collection total="2" passed="$passed" failed="$failed" skipped="0" name="$Category">
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
            platform = $Platform
            testFilter = "Category=$Category"
            includeClass = $Class
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

    function Invoke-CatalystTestClosedIdentityFailure {
        param(
            [Parameter(Mandatory = $true)][string]$EnvironmentName,
            [Parameter(Mandatory = $true)][string]$InvalidValue
        )

        $root = Join-Path $script:ScratchRoot (
            "closed-identity-$EnvironmentName-$([guid]::NewGuid().ToString('N'))")
        $repository = Join-Path $root 'repository'
        $trusted = Join-Path $root 'trusted'
        $workspace = Join-Path $root 'workspace'
        $agentTemp = Join-Path $root 'agent-temp'
        $output = Join-Path $workspace 'CatalystGestureProbe'
        $attestationRoot = Join-Path $agentTemp 'catalyst-gesture-attestation'
        $attestation = Join-Path $attestationRoot 'trusted-tree.json'
        foreach ($directory in @(
                $repository, $trusted, $workspace, $agentTemp,
                $output, $attestationRoot)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        $source = 'a' * 40
        $bindings = [ordered]@{
            TF_BUILD = 'True'
            SYSTEM_DEFINITIONID = '27723'
            BUILD_REPOSITORY_NAME = 'dotnet/maui'
            BUILD_SOURCEBRANCH = 'refs/heads/copilot/replicate-issues-pipeline'
            BUILD_SOURCEVERSION = $source
            CATALYST_PROBE_MODE = 'catalyst-gesture-probe'
            BUILD_SOURCESDIRECTORY = $repository
            CATALYST_PROBE_TRUSTED_ROOT = $trusted
            CATALYST_PROBE_TRUSTED_ATTESTATION = $attestation
            CATALYST_PROBE_OUTPUT_ROOT = $output
            CATALYST_PROBE_RUNTIME_ROOT = (
                Join-Path $agentTemp 'catalyst-gesture-runtime')
            PIPELINE_WORKSPACE = $workspace
            AGENT_TEMPDIRECTORY = $agentTemp
        }
        $bindings[$EnvironmentName] = $InvalidValue
        $saved = @{}
        $patchCalls = 0
        $prepareCalls = 0
        $caught = $null
        try {
            foreach ($name in $bindings.Keys) {
                $saved[$name] = [Environment]::GetEnvironmentVariable($name)
                [Environment]::SetEnvironmentVariable($name, $bindings[$name])
            }
            try {
                Assert-CatalystProbeClosedAzureIdentity `
                    -ExpectedSourceVersion $source `
                    -RepositoryRoot $repository `
                    -TrustedRoot $trusted `
                    -TrustedTreeAttestation $attestation `
                    -OutputDirectory $output
                $patchCalls++
                $prepareCalls++
            } catch {
                $caught = $_
            }
        } finally {
            foreach ($name in $bindings.Keys) {
                [Environment]::SetEnvironmentVariable($name, $saved[$name])
            }
        }
        return [pscustomobject]@{
            Error = $caught
            PatchCalls = $patchCalls
            PrepareCalls = $prepareCalls
        }
    }

    function New-CatalystTestRealProductionV2Case {
        param([Parameter(Mandatory = $true)][string]$Name)

        $root = Join-Path $script:ScratchRoot $Name
        $productionRoot = Join-Path $root 'production-composite'
        $regressionRoot = Join-Path $productionRoot 'regression'
        foreach ($arm in @('baseline', 'fix')) {
            $null = New-CatalystTestEvidence `
                -Directory (Join-Path $regressionRoot $arm) `
                -Outcome Pass `
                -Platform ios `
                -Category Label `
                -Class $script:CatalystProbePrimaryClass `
                -Methods @('ExistingBehavior', 'SecondBehavior')
        }
        $null = New-CatalystTestEvidence `
            -Directory (Join-Path $regressionRoot 'catalyst-baseline') `
            -Outcome Pass
        $null = New-CatalystTestEvidence `
            -Directory (Join-Path $regressionRoot 'catalyst-fix') `
            -Outcome Fail

        $canonicalPatch = Join-Path $root 'known-negative-product.patch'
        # Unit checks run in shallow checkouts without the historical product commits.
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot (
                'fixtures/ReplicationGesturePlatformManagerKnownNegative.patch')) `
            -Destination $canonicalPatch
        (Get-FileHash -LiteralPath $canonicalPatch -Algorithm SHA256).
        Hash.ToLowerInvariant() | Should -BeExactly $script:CatalystProbePatchSha256

        $selection = [pscustomobject]@{
            BaselineSha = $script:CatalystProbeBaselineCommit
            Platform = 'ios'
            Project = 'Controls'
            ProjectPath = $script:CatalystProbeProjectPath
            Category = 'Label'
            TestClass = $script:CatalystProbePrimaryClass
            GeneratedTestPath = $script:CatalystProbePrimaryAnchorPath
        }
        $requirement = Get-ReplicationFixedCompanionRequirement `
            -Platform ios `
            -FixPaths @($script:CatalystProbeProductPath)
        return [pscustomobject]@{
            Root = $root
            ProductionRoot = $productionRoot
            CanonicalPatch = $canonicalPatch
            ExpectedPatch = Join-Path $productionRoot 'fix.patch'
            Selection = $selection
            Requirement = $requirement
            TrustedFixture = Join-Path $PSScriptRoot (
                'fixtures/ReplicationGesturePlatformManagerRegression.iOS.cs')
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

    function Invoke-CatalystTestGit {
        param(
            [Parameter(Mandatory = $true)][string]$Repository,
            [Parameter(Mandatory = $true)][string[]]$Arguments
        )

        $output = @(& git -C $Repository @Arguments 2>&1)
        if ($LASTEXITCODE -ne 0) {
            throw "Scratch Git command failed: git $($Arguments -join ' ')`n$($output -join "`n")"
        }
        return ($output -join "`n").TrimEnd("`r", "`n")
    }

    function New-CatalystTestGitCase {
        param([Parameter(Mandatory = $true)][string]$Name)

        $root = Join-Path $script:ScratchRoot $Name
        $repository = Join-Path $root 'repository'
        $trusted = Join-Path $root 'trusted'
        $workspace = Join-Path $root 'workspace'
        $agentTemp = Join-Path $root 'agent-temp'
        foreach ($directory in @($repository, $trusted, $workspace, $agentTemp)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $product = Join-Path $repository $script:CatalystProbeProductPath
        $trackedOutput = Join-Path $repository (
            'src/Core/src/Handlers/HybridWebView/HybridWebView.js')
        $fixture = Join-Path $trusted $script:CatalystProbeFixtureRelativePath
        $project = Join-Path $repository (
            'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj')
        $entitlements = Join-Path $trusted (
            'source-overrides/ReplicationMacCatalystControlsDeviceTests.entitlements')
        foreach ($parent in @(
                (Split-Path -Parent $product),
                (Split-Path -Parent $trackedOutput),
                (Split-Path -Parent $fixture),
                (Split-Path -Parent $project),
                (Split-Path -Parent $entitlements))) {
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
        }
        [IO.File]::WriteAllText($product, "baseline`n")
        [IO.File]::WriteAllText($trackedOutput, "generated baseline`n")
        [IO.File]::WriteAllText($fixture, "fixed fixture`n")
        [IO.File]::WriteAllText($project, "<Project />`n")
        [IO.File]::WriteAllText($entitlements, "<plist />`n")
        [IO.File]::WriteAllText(
            (Join-Path $repository '.gitignore'),
            "artifacts/`n")
        Invoke-CatalystTestGit -Repository $repository -Arguments @('init', '--quiet') |
            Out-Null
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'config', 'user.email', 'catalyst-probe-tests@example.invalid') | Out-Null
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'config', 'user.name', 'Catalyst Probe Tests') | Out-Null
        Invoke-CatalystTestGit -Repository $repository -Arguments @('add', '--all') |
            Out-Null
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'commit', '--quiet', '-m', 'baseline') | Out-Null
        $baselineCommit = Invoke-CatalystTestGit -Repository $repository `
            -Arguments @('rev-parse', 'HEAD')
        $baselineBlob = Invoke-CatalystTestGit -Repository $repository `
            -Arguments @('rev-parse', "HEAD:$($script:CatalystProbeProductPath)")
        [IO.File]::WriteAllText($product, "negative`n")
        $negativeFileSha256 =
        (Get-FileHash -LiteralPath $product -Algorithm SHA256).Hash.ToLowerInvariant()
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'add', '--', $script:CatalystProbeProductPath) | Out-Null
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'commit', '--quiet', '-m', 'negative') | Out-Null
        $negativeCommit = Invoke-CatalystTestGit -Repository $repository `
            -Arguments @('rev-parse', 'HEAD')
        $negativeBlob = Invoke-CatalystTestGit -Repository $repository `
            -Arguments @('rev-parse', "HEAD:$($script:CatalystProbeProductPath)")
        Invoke-CatalystTestGit -Repository $repository -Arguments @(
            'checkout', '--quiet', '--detach', $baselineCommit) | Out-Null

        $original = [ordered]@{
            BaselineCommit = $script:CatalystProbeBaselineCommit
            NegativeCommit = $script:CatalystProbeNegativeCommit
            BaselineBlob = $script:CatalystProbeBaselineBlob
            NegativeBlob = $script:CatalystProbeNegativeBlob
            BaselineFileSha256 = $script:CatalystProbeBaselineFileSha256
            NegativeFileSha256 = $script:CatalystProbeNegativeFileSha256
            FixtureSha256 = $script:CatalystProbeFixtureSha256
        }
        $script:CatalystProbeBaselineCommit = $baselineCommit
        $script:CatalystProbeNegativeCommit = $negativeCommit
        $script:CatalystProbeBaselineBlob = $baselineBlob
        $script:CatalystProbeNegativeBlob = $negativeBlob
        $script:CatalystProbeBaselineFileSha256 =
        (Get-FileHash -LiteralPath $product -Algorithm SHA256).Hash.ToLowerInvariant()
        $script:CatalystProbeNegativeFileSha256 = $negativeFileSha256
        $script:CatalystProbeFixtureSha256 =
        (Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash.ToLowerInvariant()

        return [pscustomobject]@{
            Root = $root
            Repository = $repository
            Trusted = $trusted
            Workspace = $workspace
            AgentTemp = $agentTemp
            Output = Join-Path $workspace 'proof'
            Runtime = Join-Path $agentTemp 'catalyst-gesture-runtime'
            Attestation = Join-Path $root 'trusted-tree.json'
            Product = $product
            TrackedOutput = $trackedOutput
            Fixture = $fixture
            FixtureTarget =
            Join-Path $repository $script:CatalystProbeFixtureTargetRelativePath
            Assets = Join-Path $repository (
                'artifacts/obj/Controls.DeviceTests/project.assets.json')
            OriginalConstants = $original
        }
    }

    function Restore-CatalystTestGitConstants {
        param([Parameter(Mandatory = $true)][pscustomobject]$Case)

        $script:CatalystProbeBaselineCommit = $Case.OriginalConstants.BaselineCommit
        $script:CatalystProbeNegativeCommit = $Case.OriginalConstants.NegativeCommit
        $script:CatalystProbeBaselineBlob = $Case.OriginalConstants.BaselineBlob
        $script:CatalystProbeNegativeBlob = $Case.OriginalConstants.NegativeBlob
        $script:CatalystProbeBaselineFileSha256 =
        $Case.OriginalConstants.BaselineFileSha256
        $script:CatalystProbeNegativeFileSha256 =
        $Case.OriginalConstants.NegativeFileSha256
        $script:CatalystProbeFixtureSha256 = $Case.OriginalConstants.FixtureSha256
    }

    function New-CatalystTestGitContext {
        param(
            [Parameter(Mandatory = $true)][pscustomobject]$Case,
            [ValidateSet('setup', 'baseline', 'negative')][string]$State = 'setup'
        )

        New-Item -ItemType Directory -Path $Case.Output -Force | Out-Null
        $logs = Join-Path $Case.Output 'logs'
        New-Item -ItemType Directory -Path $logs -Force | Out-Null
        $deadline = New-CatalystTestTaskDeadline
        return [pscustomobject]@{
            ExpectedSourceVersion = 'a' * 40
            RepositoryRoot = $Case.Repository
            TrustedRoot = $Case.Trusted
            TrustedTreeAttestation = $Case.Attestation
            OutputDirectory = $Case.Output
            LogDirectory = $logs
            RuntimeEnvironment =
            Get-CatalystProbeRuntimeEnvironment -RuntimeRoot $Case.Runtime
            TaskDeadline = $deadline
            ActiveDeadline = $deadline
            ActiveReserveSeconds = 0
            TrustedFixturePath = $Case.Fixture
            FixtureTargetPath = $Case.FixtureTarget
            ProductPath = $Case.Product
            RepositoryState = $State
            InitialRepositoryStatus = ''
        }
    }

    function Invoke-CatalystTestGitProcess {
        param(
            [Parameter(Mandatory = $true)][string[]]$ArgumentList,
            [Parameter(Mandatory = $true)][string]$WorkingDirectory
        )

        $stdout = Invoke-CatalystTestGit `
            -Repository $WorkingDirectory `
            -Arguments $ArgumentList
        return [pscustomobject]@{
            ExitCode = 0
            TimedOut = $false
            Stdout = $stdout
            Stderr = ''
            StartedUtc = 'start'
            CompletedUtc = 'end'
            LogSha256 = 'a' * 64
        }
    }

    function Write-CatalystTestAppleAssets {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [switch]$MissingCatalyst
        )

        $targets = [ordered]@{
            'net10.0-ios' = @{}
            'net10.0-ios/iossimulator-arm64' = @{}
            'net10.0-maccatalyst' = @{}
        }
        if (-not $MissingCatalyst) {
            $targets['net10.0-maccatalyst/maccatalyst-arm64'] = @{}
        }
        $document = [ordered]@{
            version = 3
            targets = $targets
            project = [ordered]@{
                restore = [ordered]@{
                    originalTargetFrameworks = @(
                        'net10.0-ios',
                        'net10.0-maccatalyst')
                }
                frameworks = [ordered]@{
                    'net10.0-ios' = @{ targetAlias = 'net10.0-ios' }
                    'net10.0-maccatalyst' = @{
                        targetAlias = 'net10.0-maccatalyst'
                    }
                }
            }
        }
        New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force |
            Out-Null
        [IO.File]::WriteAllText(
            $Path,
            (($document | ConvertTo-Json -Depth 8) + "`n"),
            [Text.UTF8Encoding]::new($false))
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
            SYSTEM_DEFINITIONID = '27723'
            BUILD_REPOSITORY_NAME = 'dotnet/maui'
            BUILD_SOURCEBRANCH = 'refs/heads/copilot/replicate-issues-pipeline'
            CATALYST_PROBE_MODE = 'catalyst-gesture-probe'
            BUILD_SOURCESDIRECTORY = $repository
            CATALYST_PROBE_TRUSTED_ROOT = $trusted
            CATALYST_PROBE_TRUSTED_ATTESTATION = $attestation
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

    It 'restores trusted setup tracked output before requiring the real initialization baseline' {
        $case = New-CatalystTestGitCase -Name 'tracked-output-initialization'
        Set-Content -LiteralPath $case.Attestation -Value '{}' -Encoding utf8NoBOM
        [IO.File]::WriteAllText($case.TrackedOutput, "regenerated by build tasks`n")
        $taskDeadline = New-CatalystTestTaskDeadline
        $containerResult = Join-Path $case.Root 'fresh-profile/TestResults.xUnit.xml'
        Mock Get-CatalystProbeHostFacts {
            [pscustomobject]@{ IsMacOS = $true; Architecture = 'arm64' }
        }
        Mock Get-CatalystProbeContainerResultPath { $containerResult }
        Mock Assert-CatalystProbeTrustedTree {}

        $saved = @{}
        $bindings = [ordered]@{
            TF_BUILD = 'true'
            SYSTEM_DEFINITIONID = '27723'
            BUILD_REPOSITORY_NAME = 'dotnet/maui'
            BUILD_SOURCEBRANCH = 'refs/heads/copilot/replicate-issues-pipeline'
            CATALYST_PROBE_MODE = 'catalyst-gesture-probe'
            BUILD_SOURCESDIRECTORY = $case.Repository
            CATALYST_PROBE_TRUSTED_ROOT = $case.Trusted
            CATALYST_PROBE_TRUSTED_ATTESTATION = $case.Attestation
            CATALYST_PROBE_OUTPUT_ROOT = $case.Output
            CATALYST_PROBE_RUNTIME_ROOT = $case.Runtime
            CATALYST_GESTURE_JOB_DEADLINE_UTC =
            [DateTimeOffset]::UtcNow.AddMinutes(60).ToString('O')
            CATALYST_GESTURE_ARTIFACT_TAIL_SECONDS = '300'
            BUILD_SOURCEVERSION = ('a' * 40)
            PIPELINE_WORKSPACE = $case.Workspace
            AGENT_TEMPDIRECTORY = $case.AgentTemp
        }
        try {
            foreach ($name in $bindings.Keys) {
                $saved[$name] = [Environment]::GetEnvironmentVariable($name)
                [Environment]::SetEnvironmentVariable($name, $bindings[$name])
            }
            $context = Initialize-CatalystGestureProbeContext `
                -ExpectedSourceVersion ('a' * 40) `
                -RepositoryRoot $case.Repository `
                -TrustedRoot $case.Trusted `
                -TrustedTreeAttestation $case.Attestation `
                -OutputDirectory $case.Output `
                -TaskDeadline $taskDeadline

            $context.RepositoryState | Should -BeExactly 'setup'
            (Invoke-CatalystTestGit -Repository $case.Repository `
                -Arguments @('status', '--porcelain=v1', '--untracked-files=all')) |
                Should -BeNullOrEmpty
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
        } finally {
            foreach ($name in $bindings.Keys) {
                [Environment]::SetEnvironmentVariable($name, $saved[$name])
            }
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'restores tracked output after the trusted Catalyst prewarm' {
        $case = New-CatalystTestGitCase -Name 'tracked-output-prewarm'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList `
                        -WorkingDirectory $WorkingDirectory
                }
                $FileName | Should -BeExactly 'dotnet'
                [IO.File]::WriteAllText(
                    $case.TrackedOutput,
                    "regenerated by trusted prewarm`n")
                [pscustomobject]@{
                    ExitCode = 0
                    TimedOut = $false
                    LogSha256 = 'a' * 64
                }
            }
            Mock Assert-ReplicationAppleCompanionAssets {
                [pscustomobject]@{
                    AssetsSha256 = 'b' * 64
                    TargetPairs = @(
                        'net10.0-ios/iossimulator-arm64',
                        'net10.0-maccatalyst/maccatalyst-arm64')
                    OriginalTargetFrameworks = @(
                        'net10.0-ios', 'net10.0-maccatalyst')
                    Frameworks = @(
                        'net10.0-ios', 'net10.0-maccatalyst')
                }
            }

            Invoke-CatalystProbeTrustedRestore -Context $context

            (Invoke-CatalystTestGit -Repository $case.Repository `
                -Arguments @('status', '--porcelain=v1', '--untracked-files=all')) |
                Should -BeNullOrEmpty
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'retains rejected pre-execution changes through the real outer finalizer' {
        $case = New-CatalystTestGitCase -Name 'rejected-input-outer-finalizer'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $context | Add-Member ResultPath (Join-Path $case.Output 'catalyst-gesture-probe.json')
            $context | Add-Member JobDeadlineUtc ([DateTimeOffset]::UtcNow.AddHours(1).ToString('O'))
            $context | Add-Member ArtifactTailSeconds 300
            $script:RejectedInputContext = $context
            Set-Content -LiteralPath $case.Attestation -Value '{}' -Encoding utf8NoBOM
            Mock Read-TrustedTreeAttestation { [pscustomobject]@{ treeHash = 'b' * 64 } }
            Mock Get-CatalystProbeFileSha256 {
                if ($Path -ceq $script:RejectedInputContext.ProductPath) {
                    $script:CatalystProbeBaselineFileSha256
                } elseif ($Path -ceq $script:RejectedInputContext.FixtureTargetPath) {
                    $script:CatalystProbeFixtureSha256
                } else {
                    'c' * 64
                }
            }
            Mock Assert-CatalystProbeClosedAzureIdentity {}
            Mock Initialize-CatalystGestureProbeContext { $script:RejectedInputContext }
            Mock New-CatalystProbeFixedPatch { Join-Path $case.Root 'unused.patch' }
            Mock Assert-CatalystProbeFixedPatchPolicy {}
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbePrepareIosSimulator {
                [pscustomobject]@{ udid = 'fixed'; installedOnly = $true }
            }
            Mock New-CatalystProbeProductionCompositeModule {
                New-Module -Name $script:CatalystProbeProductionModuleName `
                    -ScriptBlock {}
            }
            Mock Get-CatalystProbeProductionCompositeSelection {
                New-Item -ItemType Directory -Path (Split-Path $case.FixtureTarget) -Force |
                    Out-Null
                Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
                $Context.RepositoryState = 'baseline'
                [IO.File]::WriteAllText($case.TrackedOutput, "rejected before execution`n")
                Assert-CatalystProbeRepositoryState -Context $Context -State baseline
            }
            Mock Remove-CatalystProbeProductionCompositeModule {}
            Mock Stop-CatalystProbeOwnedApplication { 0 }
            Mock Invoke-CatalystProbeProductionPrewarm {
                throw 'native verifier must not launch for rejected input'
            }

            {
                Invoke-CatalystGestureRegressionProbeCore `
                    -ExpectedSourceVersion ('a' * 40) `
                    -RepositoryRoot $case.Repository `
                    -TrustedRoot $case.Trusted `
                    -TrustedTreeAttestation $case.Attestation `
                    -OutputDirectory $case.Output `
                    -TaskDeadline $context.TaskDeadline
            } | Should -Throw '*scope contains tracked verification output*'

            Should -Invoke Invoke-CatalystProbeProductionPrewarm -Times 0 -Exactly
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "rejected before execution`n"
            (Get-Content -LiteralPath $case.FixtureTarget -Raw) |
                Should -BeExactly "fixed fixture`n"
            $result = Get-Content -LiteralPath $context.ResultPath -Raw | ConvertFrom-Json
            $result.outcome | Should -BeExactly 'inconclusive'
            $result.cleanup.completed | Should -BeFalse
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects dirty input before trusted prewarm without restoring it' {
        $case = New-CatalystTestGitCase -Name 'rejected-prewarm-input'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            [IO.File]::WriteAllText($case.TrackedOutput, "rejected before prewarm`n")
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                throw 'dotnet must not launch for rejected input'
            } -ParameterFilter { $FileName -cne 'git' }

            { Invoke-CatalystProbeTrustedRestore -Context $context } |
                Should -Throw '*scope contains tracked verification output*'
            Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 0 -Exactly `
                -ParameterFilter { $FileName -cne 'git' }
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "rejected before prewarm`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'restores output from failed trusted prewarm within that phase' {
        $case = New-CatalystTestGitCase -Name 'failed-prewarm-output'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                [IO.File]::WriteAllText($case.TrackedOutput, "failed prewarm output`n")
                [pscustomobject]@{ ExitCode = 1; TimedOut = $false }
            }

            { Invoke-CatalystProbeTrustedRestore -Context $context } |
                Should -Throw '*trusted dual-Apple prewarm failed*'
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'charges preguard and plan work to the same prewarm deadline' {
        $case = New-CatalystTestGitCase -Name 'prewarm-entry-clock'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $parentDeadline = [long]$context.TaskDeadline.DeadlineTimestamp
            $script:PrewarmPreguardDeadline = $null
            Mock Assert-CatalystProbeRepositoryState {
                $script:PrewarmPreguardDeadline = $Context.ActiveDeadline
                $Context.ActiveDeadline.BudgetSeconds | Should -BeLessOrEqual 600
                $Context.ActiveDeadline.DeadlineTimestamp -= 60L * $Context.ActiveDeadline.Frequency
            }
            Mock Get-ReplicationAppleCompanionPrewarmPlan {
                (Get-CatalystProbeDeadlineRemainingSeconds -Deadline $script:PrewarmPreguardDeadline) |
                    Should -BeLessOrEqual 540
                throw 'planned prewarm admission rejection'
            }
            Mock Restore-CatalystProbeTrackedVerificationSideEffects {
                throw 'rejected prewarm input must not enter cleanup'
            }
            Mock Invoke-CatalystProbeBoundedProcess {
                throw 'rejected prewarm plan must not launch commands'
            }

            { Invoke-CatalystProbeTrustedRestore -Context $context } |
                Should -Throw '*planned prewarm admission rejection*'
            [long]$context.TaskDeadline.DeadlineTimestamp | Should -Be $parentDeadline
            Should -Invoke Restore-CatalystProbeTrackedVerificationSideEffects -Times 0 -Exactly
            Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 0 -Exactly
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'runs restore then both offline builds against one diminishing phase deadline' {
        $case = New-CatalystTestGitCase -Name 'dual-apple-prewarm-order'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $parentEntry = [long]$context.TaskDeadline.EntryTimestamp
            $parentDeadline = [long]$context.TaskDeadline.DeadlineTimestamp
            $script:PrewarmCommandNames = [Collections.Generic.List[string]]::new()
            $script:PrewarmDeadlineIds = [Collections.Generic.List[int]]::new()
            $script:PrewarmRemaining = [Collections.Generic.List[int]]::new()
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                $name = if ($ArgumentList[0] -ceq 'restore') {
                    Write-CatalystTestAppleAssets -Path $case.Assets
                    'restore'
                } else {
                    "$($ArgumentList[$ArgumentList.IndexOf('--framework') + 1])/" +
                    $ArgumentList[$ArgumentList.IndexOf('--runtime') + 1]
                }
                $script:PrewarmCommandNames.Add($name)
                $script:PrewarmDeadlineIds.Add(
                    [Runtime.CompilerServices.RuntimeHelpers]::GetHashCode($TaskDeadline))
                $script:PrewarmRemaining.Add(
                    (Get-CatalystProbeDeadlineRemainingSeconds -Deadline $TaskDeadline))
                $TaskDeadline.DeadlineTimestamp -=
                2L * [long]$TaskDeadline.Frequency
                [IO.File]::WriteAllText(
                    $case.TrackedOutput,
                    "prewarm generated output $name`n")
                [pscustomobject]@{
                    ExitCode = 0
                    TimedOut = $false
                    LogSha256 = ([string]([char](97 + $script:PrewarmCommandNames.Count))) * 64
                }
            }

            $summary = Invoke-CatalystProbeTrustedRestore -Context $context

            @($script:PrewarmCommandNames) | Should -Be @(
                'restore',
                'net10.0-ios/iossimulator-arm64',
                'net10.0-maccatalyst/maccatalyst-arm64')
            @($script:PrewarmDeadlineIds | Select-Object -Unique).Count | Should -Be 1
            $script:PrewarmRemaining[1] |
                Should -BeLessThan $script:PrewarmRemaining[0]
            $script:PrewarmRemaining[2] |
                Should -BeLessThan $script:PrewarmRemaining[1]
            $summary.ready | Should -BeTrue
            $summary.cleanupCompleted | Should -BeTrue
            $summary.phaseBudgetSeconds | Should -BeLessOrEqual 600
            $summary.assets.validationCount | Should -Be 3
            $summary.assets.unchangedThroughBuilds | Should -BeTrue
            @($summary.builds.targetFramework) | Should -Be @(
                'net10.0-ios', 'net10.0-maccatalyst')
            @($summary.builds.runtimeIdentifier) | Should -Be @(
                'iossimulator-arm64', 'maccatalyst-arm64')
            [long]$context.TaskDeadline.EntryTimestamp | Should -Be $parentEntry
            [long]$context.TaskDeadline.DeadlineTimestamp | Should -Be $parentDeadline
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects assets clobbered by the first build before the Catalyst build' {
        $case = New-CatalystTestGitCase -Name 'dual-apple-assets-clobber'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $script:PrewarmDotnetCalls = 0
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                $script:PrewarmDotnetCalls++
                if ($script:PrewarmDotnetCalls -eq 1) {
                    Write-CatalystTestAppleAssets -Path $case.Assets
                } elseif ($script:PrewarmDotnetCalls -eq 2) {
                    Write-CatalystTestAppleAssets `
                        -Path $case.Assets `
                        -MissingCatalyst
                }
                [IO.File]::WriteAllText(
                    $case.TrackedOutput,
                    "clobbering prewarm output`n")
                [pscustomobject]@{
                    ExitCode = 0
                    TimedOut = $false
                    LogSha256 = 'c' * 64
                }
            }

            { Invoke-CatalystProbeTrustedRestore -Context $context } |
                Should -Throw '*project.assets.json changed after the graph restore*'
            $script:PrewarmDotnetCalls | Should -Be 2
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
            Test-Path -LiteralPath $case.FixtureTarget | Should -BeFalse
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'restores tracked output when the later Catalyst build fails' {
        $case = New-CatalystTestGitCase -Name 'dual-apple-later-build-failure'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $script:PrewarmDotnetCalls = 0
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                $script:PrewarmDotnetCalls++
                if ($script:PrewarmDotnetCalls -eq 1) {
                    Write-CatalystTestAppleAssets -Path $case.Assets
                }
                [IO.File]::WriteAllText(
                    $case.TrackedOutput,
                    "later failed build output`n")
                [pscustomobject]@{
                    ExitCode = if ($script:PrewarmDotnetCalls -eq 3) { 1 } else { 0 }
                    TimedOut = $false
                    LogSha256 = 'd' * 64
                }
            }

            { Invoke-CatalystProbeTrustedRestore -Context $context } |
                Should -Throw '*trusted dual-Apple prewarm failed*'
            $script:PrewarmDotnetCalls | Should -Be 3
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'never returns readiness for a failed command' {
        $case = New-CatalystTestGitCase -Name 'dual-apple-nonzero'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $script:UnexpectedPrewarmResult = $null
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                [pscustomobject]@{
                    ExitCode = 1
                    TimedOut = $false
                    LogSha256 = 'e' * 64
                }
            }

            try {
                $script:UnexpectedPrewarmResult =
                Invoke-CatalystProbeTrustedRestore -Context $context
                throw 'Expected prewarm failure was not raised.'
            } catch {
                $_.Exception.Message | Should -BeLike '*trusted dual-Apple prewarm failed*'
            }
            $script:UnexpectedPrewarmResult | Should -BeNullOrEmpty
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'never returns readiness for a timed-out command' {
        $case = New-CatalystTestGitCase -Name 'dual-apple-timeout'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            $script:UnexpectedPrewarmResult = $null
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
                }
                [pscustomobject]@{
                    ExitCode = 124
                    TimedOut = $true
                    LogSha256 = 'e' * 64
                }
            }
            try {
                $script:UnexpectedPrewarmResult =
                Invoke-CatalystProbeTrustedRestore -Context $context
                throw 'Expected prewarm timeout was not raised.'
            } catch {
                $_.Exception.Message | Should -BeLike '*trusted dual-Apple prewarm failed*'
            }
            $script:UnexpectedPrewarmResult | Should -BeNullOrEmpty
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects dirty pre-execution scope instead of treating it as trusted cycle output' {
        $case = New-CatalystTestGitCase -Name 'dirty-cycle-input'
        try {
            $context = New-CatalystTestGitContext -Case $case -State baseline
            New-Item -ItemType Directory -Path (Split-Path -Parent $case.FixtureTarget) `
                -Force | Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            [IO.File]::WriteAllText($case.TrackedOutput, "dirty before verifier`n")
            Mock Get-ReplicationAppleIsolatedCommand {
                [pscustomobject]@{
                    FilePath = 'mock-native-verifier'
                    Arguments = @()
                    Environment = $context.RuntimeEnvironment
                }
            }
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                throw 'native verifier must not launch for dirty input'
            } -ParameterFilter { $FileName -cne 'git' }

            { Invoke-CatalystProbeCycle -Kind baseline -Context $context } |
                Should -Throw '*scope contains tracked verification output*'
            Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 0 -Exactly `
                -ParameterFilter { $FileName -cne 'git' }
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "dirty before verifier`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects unknown untracked paths without deleting or restoring anything' {
        $case = New-CatalystTestGitCase -Name 'unknown-untracked'
        try {
            $context = New-CatalystTestGitContext -Case $case -State baseline
            New-Item -ItemType Directory -Path (Split-Path -Parent $case.FixtureTarget) `
                -Force | Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            $unknown = Join-Path $case.Repository 'unexpected.txt'
            [IO.File]::WriteAllText($unknown, "foreign`n")
            [IO.File]::WriteAllText($case.TrackedOutput, "dirty output`n")
            Mock Assert-CatalystProbeTrustedTree {}

            {
                Restore-CatalystProbeTrackedVerificationSideEffects `
                    -Context $context `
                    -State baseline
            } | Should -Throw '*unexpected untracked repository path*'
            Test-Path -LiteralPath $unknown | Should -BeTrue
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "dirty output`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects staged tracked output without altering it' {
        $case = New-CatalystTestGitCase -Name 'staged-output'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            [IO.File]::WriteAllText($case.TrackedOutput, "staged output`n")
            Invoke-CatalystTestGit -Repository $case.Repository -Arguments @(
                'add', '--', 'src/Core/src/Handlers/HybridWebView/HybridWebView.js') |
                Out-Null
            Mock Assert-CatalystProbeTrustedTree {}

            {
                Restore-CatalystProbeTrackedVerificationSideEffects `
                    -Context $context `
                    -State setup
            } | Should -Throw '*refuses staged repository path*'
            (Invoke-CatalystTestGit -Repository $case.Repository `
                -Arguments @('status', '--porcelain=v1')) |
                Should -Match '^M  src/Core/src/Handlers/HybridWebView/HybridWebView\.js$'
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "staged output`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects protected product and fixture mutations without erasing them' {
        $productCase = New-CatalystTestGitCase -Name 'mutated-product'
        try {
            $context = New-CatalystTestGitContext -Case $productCase -State setup
            [IO.File]::WriteAllText($productCase.Product, "unexpected product`n")
            Mock Assert-CatalystProbeTrustedTree {}
            {
                Restore-CatalystProbeTrackedVerificationSideEffects `
                    -Context $context `
                    -State setup
            } | Should -Throw '*product bytes changed outside the fixed contract*'
            (Get-Content -LiteralPath $productCase.Product -Raw) |
                Should -BeExactly "unexpected product`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $productCase
        }

        $fixtureCase = New-CatalystTestGitCase -Name 'mutated-fixture'
        try {
            $context = New-CatalystTestGitContext -Case $fixtureCase -State baseline
            New-Item -ItemType Directory `
                -Path (Split-Path -Parent $fixtureCase.FixtureTarget) -Force |
                Out-Null
            [IO.File]::WriteAllText($fixtureCase.FixtureTarget, "unexpected fixture`n")
            Mock Assert-CatalystProbeTrustedTree {}
            {
                Restore-CatalystProbeTrackedVerificationSideEffects `
                    -Context $context `
                    -State baseline
            } | Should -Throw '*fixture bytes changed outside the fixed contract*'
            (Get-Content -LiteralPath $fixtureCase.FixtureTarget -Raw) |
                Should -BeExactly "unexpected fixture`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $fixtureCase
        }
    }

    It 'fails closed when tracked output restoration fails or times out' {
        foreach ($mode in @('failure', 'timeout')) {
            $case = New-CatalystTestGitCase -Name "restore-$mode"
            try {
                $context = New-CatalystTestGitContext -Case $case -State setup
                [IO.File]::WriteAllText($case.TrackedOutput, "$mode output`n")
                $script:RestorationFailureMode = $mode
                Mock Assert-CatalystProbeTrustedTree {}
                Mock Invoke-CatalystProbeBoundedProcess {
                    if ($FileName -ceq 'git' -and $ArgumentList[0] -ceq 'restore') {
                        return [pscustomobject]@{
                            ExitCode = if ($script:RestorationFailureMode -ceq 'failure') {
                                1
                            } else { 124 }
                            TimedOut = $script:RestorationFailureMode -ceq 'timeout'
                        }
                    }
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList `
                        -WorkingDirectory $WorkingDirectory
                }

                {
                    Restore-CatalystProbeTrackedVerificationSideEffects `
                        -Context $context `
                        -State setup
                } | Should -Throw '*tracked verification-output restoration*'
                (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                    Should -BeExactly "$mode output`n"
            } finally {
                Restore-CatalystTestGitConstants -Case $case
            }
        }
    }

    It 'fails closed when restoration reports success but leaves a dirty tracked path' {
        $case = New-CatalystTestGitCase -Name 'restore-still-dirty'
        try {
            $context = New-CatalystTestGitContext -Case $case -State setup
            [IO.File]::WriteAllText($case.TrackedOutput, "first dirty output`n")
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                $result = Invoke-CatalystTestGitProcess `
                    -ArgumentList $ArgumentList `
                    -WorkingDirectory $WorkingDirectory
                if ($FileName -ceq 'git' -and $ArgumentList[0] -ceq 'restore') {
                    [IO.File]::WriteAllText($case.TrackedOutput, "still dirty output`n")
                }
                return $result
            }

            {
                Restore-CatalystProbeTrackedVerificationSideEffects `
                    -Context $context `
                    -State setup
            } | Should -Throw '*scope contains tracked verification output*'
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "still dirty output`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'leaves protected and unknown paths untouched when final cleanup scope is invalid' {
        $case = New-CatalystTestGitCase -Name 'invalid-final-cleanup'
        try {
            $context = New-CatalystTestGitContext -Case $case -State negative
            New-Item -ItemType Directory -Path (Split-Path -Parent $case.FixtureTarget) `
                -Force | Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            [IO.File]::WriteAllText($case.Product, "negative`n")
            [IO.File]::WriteAllText($case.TrackedOutput, "cycle output`n")
            $unknown = Join-Path $case.Repository 'foreign-existing.xml'
            [IO.File]::WriteAllText($unknown, "<foreign />`n")
            Mock Stop-CatalystProbeOwnedApplication { 0 }
            Mock Assert-CatalystProbeTrustedTree {}

            { Restore-CatalystProbeRepository -Context $context } |
                Should -Throw '*unexpected untracked repository path*'
            (Get-Content -LiteralPath $case.Product -Raw) |
                Should -BeExactly "negative`n"
            (Get-Content -LiteralPath $case.FixtureTarget -Raw) |
                Should -BeExactly "fixed fixture`n"
            Test-Path -LiteralPath $unknown | Should -BeTrue
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "cycle output`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'rejects tracked output before applying the known-negative product image' {
        $case = New-CatalystTestGitCase -Name 'dirty-negative-input'
        try {
            $context = New-CatalystTestGitContext -Case $case -State baseline
            New-Item -ItemType Directory `
                -Path (Split-Path -Parent $case.FixtureTarget) -Force |
                Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            [IO.File]::WriteAllText($case.TrackedOutput, "dirty before apply`n")
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Invoke-CatalystProbeBoundedProcess {
                throw 'known-negative apply must not run for dirty input'
            } -ParameterFilter {
                $FileName -ceq 'git' -and $ArgumentList[0] -ceq 'apply'
            }

            {
                Enable-CatalystProbeKnownNegative `
                    -Context $context `
                    -PatchPath (Join-Path $case.Root 'unused.patch')
            } | Should -Throw '*scope contains tracked verification output*'
            Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 0 -Exactly `
                -ParameterFilter {
                $FileName -ceq 'git' -and $ArgumentList[0] -ceq 'apply'
            }
            (Get-Content -LiteralPath $case.Product -Raw) |
                Should -BeExactly "baseline`n"
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "dirty before apply`n"
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'finishes repository cleanup after tracked cycle output is restored' {
        $case = New-CatalystTestGitCase -Name 'final-output-cleanup'
        try {
            $context = New-CatalystTestGitContext -Case $case -State negative
            New-Item -ItemType Directory `
                -Path (Split-Path -Parent $case.FixtureTarget) -Force |
                Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            [IO.File]::WriteAllText($case.Product, "negative`n")
            [IO.File]::WriteAllText($case.TrackedOutput, "final cycle output`n")
            Mock Stop-CatalystProbeOwnedApplication { 0 }
            Mock Assert-CatalystProbeTrustedTree {}

            Restore-CatalystProbeTrackedVerificationSideEffects `
                -Context $context -State negative
            Restore-CatalystProbeRepository -Context $context

            Test-Path -LiteralPath $case.FixtureTarget | Should -BeFalse
            (Get-Content -LiteralPath $case.Product -Raw) |
                Should -BeExactly "baseline`n"
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
            (Invoke-CatalystTestGit -Repository $case.Repository `
                -Arguments @('status', '--porcelain=v1', '--untracked-files=all')) |
                Should -BeNullOrEmpty
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
    }

    It 'restores tracked output within both native cycle deadlines' {
        $case = New-CatalystTestGitCase -Name 'tracked-output-native-cycles'
        try {
            $context = New-CatalystTestGitContext -Case $case -State baseline
            New-Item -ItemType Directory `
                -Path (Split-Path -Parent $case.FixtureTarget) -Force |
                Out-Null
            Copy-Item -LiteralPath $case.Fixture -Destination $case.FixtureTarget
            $script:ActiveNativeCycleKind = $null
            $script:NativeCycleStarted = $false
            $script:OriginalNativeDeadline = $context.ActiveDeadline
            Mock Get-ReplicationAppleIsolatedCommand {
                [pscustomobject]@{
                    FilePath = 'mock-native-verifier'
                    Arguments = @()
                    Environment = $context.RuntimeEnvironment
                }
            }
            Mock Assert-CatalystProbeTrustedTree {}
            Mock Stop-CatalystProbeOwnedApplication { 0 }
            Mock Invoke-CatalystProbeBoundedProcess {
                if ($FileName -ceq 'git') {
                    if ($script:NativeCycleStarted) {
                        [object]::ReferenceEquals(
                            $TaskDeadline,
                            $script:OriginalNativeDeadline) | Should -BeFalse
                        $ReserveSeconds | Should -Be 10
                    }
                    return Invoke-CatalystTestGitProcess `
                        -ArgumentList $ArgumentList `
                        -WorkingDirectory $WorkingDirectory
                }
                $FileName | Should -BeExactly 'mock-native-verifier'
                $ReserveSeconds | Should -Be 30
                $script:NativeCycleStarted = $true
                [IO.File]::WriteAllText(
                    $case.TrackedOutput,
                    "regenerated by $script:ActiveNativeCycleKind native cycle`n")
                New-CatalystTestEvidence `
                    -Directory (Join-Path $case.Output $script:ActiveNativeCycleKind) `
                    -Outcome $(if ($script:ActiveNativeCycleKind -ceq 'baseline') {
                        'Pass'
                    } else {
                        'Fail'
                    }) | Out-Null
                [pscustomobject]@{
                    ExitCode = 0
                    TimedOut = $false
                    StartedUtc = 'start'
                    CompletedUtc = 'end'
                    LogSha256 = 'a' * 64
                }
            }

            $script:ActiveNativeCycleKind = 'baseline'
            $baseline = Invoke-CatalystProbeCycle -Kind baseline -Context $context
            $baseline.passed | Should -Be 2
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"

            [IO.File]::WriteAllText($case.Product, "negative`n")
            $context.RepositoryState = 'negative'
            $script:ActiveNativeCycleKind = 'negative'
            $script:NativeCycleStarted = $false
            $negative = Invoke-CatalystProbeCycle -Kind negative -Context $context
            $negative.failed | Should -Be 2
            (Get-Content -LiteralPath $case.TrackedOutput -Raw) |
                Should -BeExactly "generated baseline`n"
            (Invoke-CatalystTestGit -Repository $case.Repository `
                -Arguments @('status', '--porcelain=v1', '--untracked-files=all')) |
                Should -BeExactly (
                    " M $($script:CatalystProbeProductPath)`n" +
                    "?? $($script:CatalystProbeFixtureTargetRelativePath)")
        } finally {
            Restore-CatalystTestGitConstants -Case $case
        }
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
        $coreSource = [regex]::Match(
            $script:ProbeSource,
            '(?s)function Invoke-CatalystGestureRegressionProbeCore \{.*?' +
            '\n\}\n\nif \(\$MyInvocation\.InvocationName').Value
        $restoreIndex = $coreSource.IndexOf(
            'Invoke-CatalystProbeProductionPrewarm')
        $baselineIndex = $coreSource.IndexOf(
            'Invoke-CatalystProbeProductionComposite')
        $negativeIndex = $coreSource.IndexOf(
            'Enable-CatalystProbeKnownNegative')
        $restoreIndex | Should -BeLessThan $baselineIndex
        $baselineIndex | Should -BeLessThan $negativeIndex
        $script:ProbeSource | Should -Match (
            'fixed Label metadata anchor must remain absent')
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

    It 'fails closed when retained Catalyst raw XML is missing' {
        $strictPath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'missing-raw-xml') `
            -Outcome Pass
        Remove-Item -LiteralPath (Join-Path (
                Split-Path -Parent $strictPath) 'xunit-test-results.xml') -Force

        {
            Assert-CatalystProbeCycleEvidence `
                -Kind baseline `
                -StrictEvidencePath $strictPath
        } | Should -Throw
    }

    It 'fails closed when the Catalyst baseline contains a skipped fact' {
        $strictPath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'baseline-skip') `
            -Outcome Pass
        $root = Split-Path -Parent $strictPath
        $xmlPath = Join-Path $root 'xunit-test-results.xml'
        $xml = Get-Content -LiteralPath $xmlPath -Raw
        $firstMethod = $script:CatalystProbeMethods[0]
        $xml = $xml.Replace('total="2" passed="2" failed="0" skipped="0"',
            'total="2" passed="1" failed="0" skipped="1"')
        $xml = $xml.Replace(
            "method=`"$firstMethod`" result=`"Pass`"",
            "method=`"$firstMethod`" result=`"Skip`"")
        [IO.File]::WriteAllText(
            $xmlPath, $xml, [Text.UTF8Encoding]::new($false))
        $strict = Get-Content -LiteralPath $strictPath -Raw | ConvertFrom-Json
        $strict.passed = 1
        $strict.skipped = 1
        $strict.records[0].outcome = 'Skip'
        $strict.resultFiles[0].sha256 =
        (Get-FileHash -LiteralPath $xmlPath -Algorithm SHA256).
        Hash.ToLowerInvariant()
        $strict | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        {
            Assert-CatalystProbeCycleEvidence `
                -Kind baseline `
                -StrictEvidencePath $strictPath
        } | Should -Throw '*non-skipped facts*'
    }

    It 'fails closed when strict Catalyst evidence names a different method' {
        $strictPath = New-CatalystTestEvidence `
            -Directory (Join-Path $script:ScratchRoot 'wrong-method') `
            -Outcome Pass
        $root = Split-Path -Parent $strictPath
        $xmlPath = Join-Path $root 'xunit-test-results.xml'
        $oldMethod = $script:CatalystProbeMethods[0]
        $newMethod = 'DifferentGestureFact'
        $xml = (Get-Content -LiteralPath $xmlPath -Raw).Replace(
            $oldMethod, $newMethod)
        [IO.File]::WriteAllText(
            $xmlPath, $xml, [Text.UTF8Encoding]::new($false))
        $strict = Get-Content -LiteralPath $strictPath -Raw | ConvertFrom-Json
        $strict.records[0].method = $newMethod
        $strict.records[0].displayName =
        "$($script:CatalystProbeClass).$newMethod"
        $strict.resultFiles[0].sha256 =
        (Get-FileHash -LiteralPath $xmlPath -Algorithm SHA256).
        Hash.ToLowerInvariant()
        $strict | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        {
            Assert-CatalystProbeCycleEvidence `
                -Kind baseline `
                -StrictEvidencePath $strictPath
        } | Should -Throw '*wrong test identities*'
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
            RepositoryRoot = $caseRoot
            TrustedRoot = $caseRoot
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
        Mock Assert-CatalystProbeClosedAzureIdentity {}
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
        Mock Invoke-CatalystProbePrepareIosSimulator {
            [pscustomobject]@{ udid = 'fixed'; installedOnly = $true }
        }
        Mock New-CatalystProbeProductionCompositeModule {
            New-Module -Name $script:CatalystProbeProductionModuleName `
                -ScriptBlock {}
        }
        Mock Get-CatalystProbeProductionCompositeSelection {
            [pscustomobject]@{
                Selection = [pscustomobject]@{}
                Requirement = [pscustomobject]@{}
            }
        }
        Mock Invoke-CatalystProbeProductionPrewarm {}
        Mock Remove-CatalystProbeProductionCompositeModule {}
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

        Should -Invoke Invoke-CatalystProbeProductionPrewarm -Times 0 -Exactly
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
        Mock Assert-CatalystProbeClosedAzureIdentity {}
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
            RepositoryState = 'baseline'
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
        Mock Assert-CatalystProbeClosedAzureIdentity {}
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
        Mock Assert-CatalystProbeRepositoryState {}
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

        { Restore-CatalystProbeRepository -Context $script:SharedCleanupContext } |
            Should -Throw '*Catalyst probe cleanup failed*monotonic task deadline*'

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

    It 'retains timeout status and effective allocation for an empty child log' {
        $root = Join-Path $script:ScratchRoot 'empty-timeout-log'
        New-Item -ItemType Directory -Path $root | Out-Null
        $log = Join-Path $root 'process.log'
        $result = Invoke-CatalystProbeBoundedProcess `
            -FileName (Get-Command pwsh -CommandType Application |
                Select-Object -First 1).Source `
            -ArgumentList @('-NoProfile', '-NonInteractive', '-Command',
            '[Threading.Thread]::Sleep(30000)') `
            -WorkingDirectory $root `
            -Environment (Get-CatalystProbeRuntimeEnvironment `
                -RuntimeRoot (Join-Path $root 'runtime')) `
            -TimeoutSeconds 12 `
            -TaskDeadline (New-CatalystTestTaskDeadline -RemainingSeconds 30) `
            -LogPath $log

        $result.TimedOut | Should -BeTrue
        $result.ExitCode | Should -Be 124
        $result.EffectiveProcessSeconds | Should -Be 2
        $text = Get-Content -LiteralPath $log -Raw
        $text | Should -Match 'exitCode: 124'
        $text | Should -Match 'timedOut: True'
        $text | Should -Match 'effectiveProcessSeconds: 2'
        $text | Should -Match 'effectiveTimeoutSeconds: 12'
    }

    It 'retains an ordinary child failure without classifying it as a timeout' {
        $root = Join-Path $script:ScratchRoot 'ordinary-failure-log'
        New-Item -ItemType Directory -Path $root | Out-Null
        $log = Join-Path $root 'process.log'
        $result = Invoke-CatalystProbeBoundedProcess `
            -FileName (Get-Command pwsh -CommandType Application |
                Select-Object -First 1).Source `
            -ArgumentList @('-NoProfile', '-NonInteractive', '-Command', 'exit 7') `
            -WorkingDirectory $root `
            -Environment (Get-CatalystProbeRuntimeEnvironment `
                -RuntimeRoot (Join-Path $root 'runtime')) `
            -TimeoutSeconds 30 `
            -TaskDeadline (New-CatalystTestTaskDeadline -RemainingSeconds 60) `
            -LogPath $log

        $result.TimedOut | Should -BeFalse
        $result.ExitCode | Should -Be 7
        $text = Get-Content -LiteralPath $log -Raw
        $text | Should -Match 'exitCode: 7'
        $text | Should -Match 'timedOut: False'
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
            RepositoryState = 'baseline'
        }
        $script:CycleExecutionDeadline = $null
        $script:CycleCleanupDeadline = $null
        $script:CycleRestorationDeadline = $null
        Mock Get-ReplicationAppleIsolatedCommand {
            [pscustomobject]@{
                FilePath = 'pwsh'; Arguments = @(); Environment = $context.RuntimeEnvironment
            }
        }
        Mock Assert-CatalystProbeTrustedTree {}
        Mock Assert-CatalystProbeRepositoryState {}
        Mock Restore-CatalystProbeTrackedVerificationSideEffects {
            $script:CycleRestorationDeadline = $Context.ActiveDeadline
            $Context.ActiveReserveSeconds | Should -Be 10
        }
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
        [object]::ReferenceEquals(
            $script:CycleExecutionDeadline, $script:CycleRestorationDeadline) |
            Should -BeTrue
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
            'Run fixed report-only production composite' = 36
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
            "displayName: 'Run fixed report-only production composite'\r?\n" +
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
        $boundedCalls.Count | Should -Be 16
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

Describe 'Closed production composite diagnostic seam' {
    It 'materializes the canonical unit patch without repository history' {
        Mock git { throw 'Unit fixture setup must not query historical Git objects.' }
        $case = New-CatalystTestRealProductionV2Case -Name 'history-independent-patch'
        (Get-FileHash -LiteralPath $case.CanonicalPatch -Algorithm SHA256).
        Hash.ToLowerInvariant() | Should -BeExactly $script:CatalystProbePatchSha256
        Should -Invoke git -Times 0 -Exactly
    }

    BeforeEach {
        Mock Get-CatalystProbeHostFacts {
            [pscustomobject]@{ IsMacOS = $true; Architecture = 'arm64' }
        }
    }

    It 'rejects direct command-line access to the production library seam' {
        $output = @(& pwsh -NoLogo -NoProfile -NonInteractive `
                -File $script:ProductionPath `
                -IssueNumber 1 `
                -Platform ios `
                -BaseSha ('4' * 40) `
                -ContextPath 'unused' `
                -TrustedRoot 'unused' `
                -ArtifactRoot 'unused' `
                -TrustedTreeAttestationPath 'unused' `
                -TrustedSourceVersion ('a' * 40) `
                -StepTimeoutMinutes 0 `
                -FixedCatalystCompositeProbeLibraryOnly 2>&1)

        $LASTEXITCODE | Should -Not -Be 0
        $output -join "`n" | Should -Match (
            '(?s)available only while.*dot-sourcing the trusted orchestrator inside its fixed private module')
    }

    It 'rejects dot-sourcing the production seam from any other module' {
        {
            $null = New-Module -Name 'Untrusted.Catalyst.Module' -ScriptBlock {
                param($Path)
                . $Path `
                    -IssueNumber 1 `
                    -Platform ios `
                    -BaseSha ('4' * 40) `
                    -ContextPath 'unused' `
                    -TrustedRoot 'unused' `
                    -ArtifactRoot 'unused' `
                    -TrustedTreeAttestationPath 'unused' `
                    -TrustedSourceVersion ('a' * 40) `
                    -StepTimeoutMinutes 0 `
                    -FixedCatalystCompositeProbeLibraryOnly
            } -ArgumentList $script:ProductionPath
        } | Should -Throw (
            '*available only while dot-sourcing*fixed private module*')
    }

    It 'loads only the fixed production APIs instead of copying their implementations' {
        foreach ($name in @(
                'Invoke-ReplicationFixedCatalystCompositePrewarm',
                'Get-ReplicationRegressionLaneSelection',
                'Get-ReplicationFixedCompanionRequirement',
                'Invoke-ReplicationRegressionCompositeRun',
                'Write-ReplicationRegressionEvidenceDocument',
                'Assert-ReplicationRegressionEvidence')) {
            $script:ProbeSource | Should -Match ([regex]::Escape($name))
        }
        $script:ProbeSource | Should -Match (
            'FixedCatalystCompositeProbeLibraryOnly = \$true')
        $script:ProbeSource | Should -Not -Match (
            'function Invoke-ReplicationRegressionCompositeRun')
        $script:ProductionSource | Should -Match (
            'function Write-ReplicationRegressionEvidenceDocument')
        $script:ProductionSource | Should -Match (
            '(?s)Test-ReplicationFixRegression.*?' +
            'Write-ReplicationRegressionEvidenceDocument')
    }

    It 'requires pinned private tool restore and command-only XHarness proof before the dual-Apple graph' {
        $productionFunction = [regex]::Match(
            $script:ProductionSource,
            '(?ms)^function Invoke-ReplicationFixedCatalystCompositePrewarm\b.*?^}').
        Value
        $productionFunction | Should -Not -BeNullOrEmpty
        $toolRestore = $productionFunction.IndexOf(
            "Invoke-ReplicationTrustedRestore",
            [StringComparison]::Ordinal)
        $xharnessPreflight = $productionFunction.IndexOf(
            "Invoke-LoggedChildProcess",
            [StringComparison]::Ordinal)
        $graphPrewarm = $productionFunction.IndexOf(
            "Invoke-ReplicationAppleCompanionPrewarm",
            [StringComparison]::Ordinal)

        $toolRestore | Should -BeGreaterThan -1
        $xharnessPreflight | Should -BeGreaterThan $toolRestore
        $graphPrewarm | Should -BeGreaterThan $xharnessPreflight
        $productionFunction | Should -Match "-Verb 'tool-restore'"
        $productionFunction | Should -Match "'-PreflightXHarnessOnly'"
        $productionFunction | Should -Match '-DeadlineTimestamp \$DeadlineTimestamp'
        $script:ProbeSource | Should -Match (
            '(?s)Invoke-CatalystProbeProductionPrewarm.*?' +
            'Invoke-ReplicationFixedCatalystCompositePrewarm.*?' +
            '\$Deadline\.DeadlineTimestamp')
    }

    It 'passes the original prewarm deadline and prepared simulator into the production module' {
        $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 600
        $module = New-Module -Name 'Maui.CatalystProductionCompositeProbe.Test' `
            -ScriptBlock {
            $DeviceUdid = 'AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE'
            function Invoke-ReplicationFixedCatalystCompositePrewarm {
                param([long]$DeadlineTimestamp)
                [pscustomobject]@{
                    DeadlineTimestamp = $DeadlineTimestamp
                    EnvironmentUdid =
                    [Environment]::GetEnvironmentVariable(
                        'MAUI_REPLICATION_DEVICE_UDID')
                }
            }
        }
        $previous = [Environment]::GetEnvironmentVariable(
            'MAUI_REPLICATION_DEVICE_UDID')
        try {
            $result = Invoke-CatalystProbeProductionPrewarm `
                -Module $module `
                -Deadline $deadline
        } finally {
            Remove-Module -ModuleInfo $module -Force
        }

        $result.DeadlineTimestamp |
            Should -BeExactly $deadline.DeadlineTimestamp
        $result.EnvironmentUdid |
            Should -BeExactly 'AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE'
        $restored = [Environment]::GetEnvironmentVariable(
            'MAUI_REPLICATION_DEVICE_UDID')
        if ([string]::IsNullOrEmpty($previous)) {
            $restored | Should -BeNullOrEmpty
        } else {
            $restored | Should -BeExactly $previous
        }
    }

    It 'accepts only the exact expanded production prewarm log catalog' {
        $root = Join-Path $TestDrive 'expanded-prewarm-logs'
        $paths = @(
            'prewarm-build-ios-simulator-no-restore.log'
            'prewarm-build-maccatalyst-no-restore.log'
            'prewarm-restore-dual-apple-graph.log'
            'tool-restore.log'
            'xharness-command-probe/xharness-help-tail.log'
            'xharness-command-probe/xharness-help.log'
            'xharness-preflight-child.log'
            'xharness-preflight/xharness-help-tail.log'
            'xharness-preflight/xharness-help.log'
            'xharness-preflight/xharness-preflight.log'
        )
        foreach ($relativePath in $paths) {
            $path = Join-Path $root $relativePath
            New-Item -ItemType Directory -Path (Split-Path -Parent $path) `
                -Force | Out-Null
            'bounded' | Set-Content -LiteralPath $path -Encoding utf8NoBOM
        }

        $logs = @(Get-CatalystProbeProductionPrewarmLogs -PrewarmRoot $root)

        @($logs.name) | Should -Be $paths
        @($logs.sha256 | Where-Object {
                $_ -cnotmatch '^[0-9a-f]{64}$'
            }) | Should -BeNullOrEmpty
    }

    It 'rejects a missing private tool restore log from production prewarm evidence' {
        $root = Join-Path $TestDrive 'missing-tool-prewarm-log'
        $paths = @(
            'prewarm-build-ios-simulator-no-restore.log'
            'prewarm-build-maccatalyst-no-restore.log'
            'prewarm-restore-dual-apple-graph.log'
            'xharness-command-probe/xharness-help-tail.log'
            'xharness-command-probe/xharness-help.log'
            'xharness-preflight-child.log'
            'xharness-preflight/xharness-help-tail.log'
            'xharness-preflight/xharness-help.log'
            'xharness-preflight/xharness-preflight.log'
        )
        foreach ($relativePath in $paths) {
            $path = Join-Path $root $relativePath
            New-Item -ItemType Directory -Path (Split-Path -Parent $path) `
                -Force | Out-Null
            'bounded' | Set-Content -LiteralPath $path -Encoding utf8NoBOM
        }

        { Get-CatalystProbeProductionPrewarmLogs -PrewarmRoot $root } |
            Should -Throw '*exact bounded command logs*'
    }

    It 'keeps each production composite on one fixed 480 second deadline' {
        $task = New-CatalystTestTaskDeadline -RemainingSeconds 2040
        $baseline = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $task `
            -BudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -DownstreamReserveSeconds 780 `
            -Description 'baseline test'
        $negative = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $task `
            -BudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -DownstreamReserveSeconds 180 `
            -Description 'negative test'

        $baseline.BudgetSeconds | Should -Be 480
        $negative.BudgetSeconds | Should -Be 480
        $baseline.DeadlineTimestamp | Should -BeLessOrEqual (
            $task.DeadlineTimestamp -
            (780L * [Diagnostics.Stopwatch]::Frequency))
        $negative.DeadlineTimestamp | Should -BeLessOrEqual (
            $task.DeadlineTimestamp -
            (180L * [Diagnostics.Stopwatch]::Frequency))
        $script:ProbeSource | Should -Match (
            '(?s)Invoke-CatalystProbeProductionComposite.*?' +
            '-DeadlineTimestamp \$DeadlineTimestamp')
    }

    It 'passes the exact external cycle deadline into the production composite' {
        $module = New-Module -Name 'Catalyst.Composite.Wrapper.Test' -ScriptBlock {
            $script:CapturedComposite = $null
            $script:trustedScripts = 'trusted-scripts'
            $script:DeviceUdid = 'AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE'
            $script:RequirePreparedIosSimulator = $true
            function Invoke-ReplicationRegressionCompositeRun {
                param(
                    $Selection, $CompanionRequirement,
                    $PrimaryOutputDirectory, $CompanionOutputDirectory,
                    $TrustedScriptRoot, $TimeoutSeconds, $DeadlineTimestamp,
                    $DeviceUdid, [switch]$RequirePreparedIosSimulator)
                $script:CapturedComposite = [pscustomobject]@{
                    Selection = $Selection
                    Requirement = $CompanionRequirement
                    PrimaryOutputDirectory = $PrimaryOutputDirectory
                    CompanionOutputDirectory = $CompanionOutputDirectory
                    TrustedScriptRoot = $TrustedScriptRoot
                    TimeoutSeconds = $TimeoutSeconds
                    DeadlineTimestamp = $DeadlineTimestamp
                }
            }

        }
        try {
            $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 480
            $selection = [pscustomobject]@{ Category = 'Label' }
            $requirement = [pscustomobject]@{ Id = 'fixed' }

            Invoke-CatalystProbeProductionComposite `
                -Module $module `
                -Selection $selection `
                -Requirement $requirement `
                -PrimaryOutputDirectory 'primary' `
                -CompanionOutputDirectory 'catalyst' `
                -Deadline $deadline
            $captured = & $module { $script:CapturedComposite }

            $captured.Selection | Should -Be $selection
            $captured.Requirement | Should -Be $requirement
            $captured.PrimaryOutputDirectory | Should -BeExactly 'primary'
            $captured.CompanionOutputDirectory | Should -BeExactly 'catalyst'
            $captured.TrustedScriptRoot | Should -BeExactly 'trusted-scripts'
            $captured.TimeoutSeconds | Should -Be 480
            $captured.DeadlineTimestamp | Should -Be $deadline.DeadlineTimestamp
        } finally {
            Remove-Module -ModuleInfo $module -Force
        }
    }

    It 'forwards the module-bound prepared simulator into every production primary run' {
        $udid = 'AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE'
        $module = New-Module -Name 'Catalyst.Prepared.Composite.Test' `
            -ArgumentList $udid -ScriptBlock {
            param($PreparedUdid)
            $script:DeviceUdid = $PreparedUdid
            $script:RequirePreparedIosSimulator = $true
            $script:trustedScripts = 'trusted-scripts'
            $script:CapturedComposite = $null
            function Invoke-ReplicationRegressionCompositeRun {
                param(
                    $Selection, $CompanionRequirement,
                    $PrimaryOutputDirectory, $CompanionOutputDirectory,
                    $TrustedScriptRoot, $TimeoutSeconds, $DeadlineTimestamp,
                    $DeviceUdid, [switch]$RequirePreparedIosSimulator)
                $script:CapturedComposite = [pscustomobject]@{
                    DeviceUdid = $DeviceUdid
                    RequirePreparedIosSimulator =
                    [bool]$RequirePreparedIosSimulator
                }
            }
        }
        try {
            Invoke-CatalystProbeProductionComposite `
                -Module $module `
                -Selection ([pscustomobject]@{ Category = 'Label' }) `
                -Requirement ([pscustomobject]@{ Id = 'fixed' }) `
                -PrimaryOutputDirectory 'primary' `
                -CompanionOutputDirectory 'catalyst' `
                -Deadline (New-CatalystTestTaskDeadline -RemainingSeconds 480)
            $captured = & $module { $script:CapturedComposite }

            $captured.DeviceUdid | Should -BeExactly $udid
            $captured.RequirePreparedIosSimulator | Should -BeTrue
        } finally {
            Remove-Module -ModuleInfo $module -Force
        }
    }

    It 'binds private module creation to the installed prepared simulator identity' {
        $moduleFunction = [regex]::Match(
            $script:ProbeSource,
            '(?ms)^function New-CatalystProbeProductionCompositeModule\b.*?^}').Value
        $coreFunction = [regex]::Match(
            $script:ProbeSource,
            '(?ms)^function Invoke-CatalystGestureRegressionProbeCore\b.*?^}').Value

        $moduleFunction | Should -Match '\$PreparedSimulator'
        $moduleFunction | Should -Match (
            '\$preparedUdid\s*=\s*\[string\]\$PreparedSimulator\.udid')
        $moduleFunction | Should -Match 'DeviceUdid\s*=\s*\$preparedUdid'
        $moduleFunction | Should -Match (
            'RequirePreparedIosSimulator\s*=\s*\$true')
        $coreFunction | Should -Match (
            '(?s)New-CatalystProbeProductionCompositeModule.*?' +
            '-PreparedSimulator \$result\.simulator')
    }

    It 'rejects a mismatched prepared simulator before private module or child access' {
        Mock Get-CatalystProbeProcessTimeoutSeconds { 60 }
        Mock Assert-CatalystProbeTrustedTree {}
        Mock New-Module {
            throw 'Private module creation must remain unreachable.'
        }
        $context = [pscustomobject]@{
            OwnedIosSimulator = [pscustomobject]@{
                udid = 'AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE'
            }
        }
        $prepared = [pscustomobject]@{
            udid = '11111111-2222-3333-4444-555555555555'
            installedOnly = $true
            installedRuntimeIdentifiers = @(
                'com.apple.CoreSimulator.SimRuntime.iOS-26-0')
        }

        {
            New-CatalystProbeProductionCompositeModule `
                -Context $context `
                -CoordinationDeadline (
                New-CatalystTestTaskDeadline -RemainingSeconds 180) `
                -PreparedSimulator $prepared
        } | Should -Throw '*installed prepared iOS simulator identity*'

        Should -Invoke New-Module -Times 0 -Exactly
    }

    It 'invokes the shared production v2 writer before its trusted validator' {
        Mock Copy-CatalystProbeProductionValidationPatch {
            Join-Path $EvidenceArtifactRoot 'fix.patch'
        }
        $module = New-Module -Name 'Catalyst.V2.Wrapper.Test' -ScriptBlock {
            $script:V2Calls = [Collections.Generic.List[string]]::new()
            function Write-ReplicationRegressionEvidenceDocument {
                param(
                    $Selection, $CompanionRequirement,
                    $EvidenceArtifactRoot, $ProductPatchPath)
                $script:V2Calls.Add('writer')
                return (Join-Path $EvidenceArtifactRoot (
                        'regression/regression-evidence.json'))
            }
            function Assert-ReplicationRegressionEvidence {
                param(
                    $ArtifactRoot, $ExpectedBaselineSha, $ExpectedPlatform,
                    $ExpectedCategory, $ExpectedProject, $ExpectedProjectPath,
                    $ExpectedClass, $ExpectedGeneratedTestPath,
                    $ExpectedFixPaths, $TrustedFixturePath)
                $script:V2Calls.Add('validator')
                if ((@($ExpectedFixPaths) -join '') -cne (
                        'src/Controls/src/Core/Platform/GestureManager/' +
                        'GesturePlatformManager.iOS.cs')) {
                    throw 'wrong fixed path'
                }
            }

        }
        try {
            $selection = [pscustomobject]@{
                BaselineSha = $script:CatalystProbeBaselineCommit
                Platform = 'ios'
                Category = 'Label'
                Project = 'Controls'
                ProjectPath = 'device.csproj'
                TestClass = $script:CatalystProbePrimaryClass
                GeneratedTestPath = $script:CatalystProbePrimaryAnchorPath
            }
            $requirement = [pscustomobject]@{ Id = 'fixed' }
            $root = Join-Path $TestDrive 'v2-wrapper'
            $null = Invoke-CatalystProbeProductionV2Validation `
                -Module $module `
                -Selection $selection `
                -Requirement $requirement `
                -EvidenceArtifactRoot $root `
                -PatchPath (Join-Path $root 'negative.patch') `
                -TrustedFixturePath (Join-Path $root 'fixture.cs') `
                -Deadline (New-CatalystTestTaskDeadline -RemainingSeconds 30)

            @(& $module { @($script:V2Calls) }) |
                Should -Be @('writer', 'validator')
        } finally {
            Remove-Module -ModuleInfo $module -Force
        }
    }

    It 'reaches the expected Catalyst rejection with the real production writer and validator' {
        $case = New-CatalystTestRealProductionV2Case `
            -Name 'real-production-v2-rejection'
        $stagedPatch = Copy-CatalystProbeProductionValidationPatch `
            -PatchPath $case.CanonicalPatch `
            -EvidenceArtifactRoot $case.ProductionRoot

        $stagedPatch | Should -BeExactly $case.ExpectedPatch
        (Get-FileHash -LiteralPath $stagedPatch -Algorithm SHA256).
        Hash.ToLowerInvariant() | Should -BeExactly $script:CatalystProbePatchSha256
        $null = Write-ReplicationRegressionEvidenceDocument `
            -Selection $case.Selection `
            -CompanionRequirement $case.Requirement `
            -EvidenceArtifactRoot $case.ProductionRoot `
            -ProductPatchPath $case.CanonicalPatch

        {
            Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $case.ProductionRoot `
                -ExpectedBaselineSha $script:CatalystProbeBaselineCommit `
                -ExpectedPlatform ios `
                -ExpectedCategory Label `
                -ExpectedProject Controls `
                -ExpectedProjectPath $script:CatalystProbeProjectPath `
                -ExpectedClass $script:CatalystProbePrimaryClass `
                -ExpectedGeneratedTestPath $script:CatalystProbePrimaryAnchorPath `
                -ExpectedFixPaths @($script:CatalystProbeProductPath) `
                -TrustedFixturePath $case.TrustedFixture
        } | Should -Throw $script:CatalystProbeExpectedCompanionRejection
    }

    It 'rejects a tampered canonical patch before staging production validation bytes' {
        $case = New-CatalystTestRealProductionV2Case `
            -Name 'tampered-production-v2-patch'
        Add-Content -LiteralPath $case.CanonicalPatch -Value '# tampered'

        {
            Copy-CatalystProbeProductionValidationPatch `
                -PatchPath $case.CanonicalPatch `
                -EvidenceArtifactRoot $case.ProductionRoot
        } | Should -Throw '*immutable digest*'
        Test-Path -LiteralPath $case.ExpectedPatch | Should -BeFalse
    }

    It 'rejects real production validation after its staged fixed patch is removed' {
        $case = New-CatalystTestRealProductionV2Case `
            -Name 'missing-production-v2-patch'
        $null = Copy-CatalystProbeProductionValidationPatch `
            -PatchPath $case.CanonicalPatch `
            -EvidenceArtifactRoot $case.ProductionRoot
        $null = Write-ReplicationRegressionEvidenceDocument `
            -Selection $case.Selection `
            -CompanionRequirement $case.Requirement `
            -EvidenceArtifactRoot $case.ProductionRoot `
            -ProductPatchPath $case.CanonicalPatch
        Remove-Item -LiteralPath $case.ExpectedPatch -Force

        {
            Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $case.ProductionRoot `
                -ExpectedBaselineSha $script:CatalystProbeBaselineCommit `
                -ExpectedPlatform ios `
                -ExpectedCategory Label `
                -ExpectedProject Controls `
                -ExpectedProjectPath $script:CatalystProbeProjectPath `
                -ExpectedClass $script:CatalystProbePrimaryClass `
                -ExpectedGeneratedTestPath $script:CatalystProbePrimaryAnchorPath `
                -ExpectedFixPaths @($script:CatalystProbeProductPath) `
                -TrustedFixturePath $case.TrustedFixture
        } | Should -Throw '*fix.patch*'
    }

    It 'does not classify an unknown production failure as the expected negative' {
        $expected = [Management.Automation.ErrorRecord]::new(
            [InvalidOperationException]::new(
                $script:CatalystProbeExpectedCompanionRejection),
            'expected', 'InvalidData', $null)
        $unknown = [Management.Automation.ErrorRecord]::new(
            [InvalidOperationException]::new('missing raw XML'),
            'unknown', 'InvalidData', $null)

        Confirm-CatalystProbeExpectedProductionRejection `
            -ErrorRecord $expected | Should -BeExactly (
            $script:CatalystProbeExpectedCompanionRejection)
        {
            Confirm-CatalystProbeExpectedProductionRejection `
                -ErrorRecord $unknown
        } | Should -Throw '*missing raw XML*'
    }

    It 'binds the private seam to the fixed Azure identity and report-only inputs' {
        foreach ($literal in @(
                'Maui.CatalystProductionCompositeProbe',
                '27723',
                'refs/heads/copilot/replicate-issues-pipeline',
                'catalyst-gesture-probe',
                '40590267d8057fd5c044e5bfea77a9dd31fef29f',
                'catalyst-production-composite-private',
                'catalyst-production-composite-runtime')) {
            $script:ProductionSource | Should -Match ([regex]::Escape($literal))
        }
        $script:ProductionSource | Should -Match '\$IssueNumber -ne 1'
        $script:ProductionSource | Should -Match '\$Platform -cne ''ios'''
        $script:ProductionSource | Should -Match '\$StepTimeoutMinutes -ne 0'
        $script:Stage | Should -Match (
            'CATALYST_PROBE_MODE: catalyst-gesture-probe')
    }

    It 'uses an already-installed iOS runtime and the exact production boundaries' {
        $script:ProbeSource | Should -Match (
            "'simctl', 'list', 'runtimes', '--json'")
        $script:ProbeSource | Should -Match (
            'already-installed iOS 26\.0 runtime')
        $script:ProbeSource | Should -Not -Match (
            'Start-Emulator\.ps1')
        $script:ProbeSource | Should -Match (
            "'simctl', 'list', 'devicetypes', '--json'")
        $script:ProbeSource | Should -Match (
            "'simctl', 'list', 'devices', '--json'")
        $script:ProbeSource | Should -Match (
            "'simctl', 'create'")
        $script:ProbeSource | Should -Match (
            "'simctl', 'boot'")
        $script:ProbeSource | Should -Match (
            "Get-ReplicationRegressionLaneSelection[\s\S]*?-Platform 'ios'")
        $script:ProbeSource | Should -Match (
            "Get-ReplicationFixedCompanionRequirement[\s\S]*?-Platform 'ios'")
        $script:ProbeSource | Should -Match (
            "ios = 'ios-review-host-no-network-isolation'")
        $script:ProbeSource | Should -Match (
            "catalyst = 'signed-app-sandbox-live-outbound-deny'")
    }

    It 'rejects a wrong Azure definition before patch or simulator preparation' {
        $observed = Invoke-CatalystTestClosedIdentityFailure `
            -EnvironmentName SYSTEM_DEFINITIONID `
            -InvalidValue 1

        $observed.Error | Should -Not -BeNullOrEmpty
        $observed.Error.Exception.Message |
            Should -Match 'exact closed Azure pipeline identity'
        $observed.PatchCalls | Should -Be 0
        $observed.PrepareCalls | Should -Be 0
    }

    It 'rejects a wrong Azure branch before patch or simulator preparation' {
        $observed = Invoke-CatalystTestClosedIdentityFailure `
            -EnvironmentName BUILD_SOURCEBRANCH `
            -InvalidValue refs/heads/main

        $observed.Error | Should -Not -BeNullOrEmpty
        $observed.Error.Exception.Message |
            Should -Match 'exact closed Azure pipeline identity'
        $observed.PatchCalls | Should -Be 0
        $observed.PrepareCalls | Should -Be 0
    }

    It 'rejects a wrong probe mode before patch or simulator preparation' {
        $observed = Invoke-CatalystTestClosedIdentityFailure `
            -EnvironmentName CATALYST_PROBE_MODE `
            -InvalidValue production

        $observed.Error | Should -Not -BeNullOrEmpty
        $observed.Error.Exception.Message |
            Should -Match 'exact closed Azure pipeline identity'
        $observed.PatchCalls | Should -Be 0
        $observed.PrepareCalls | Should -Be 0
    }

    It 'rejects a wrong source version before patch or simulator preparation' {
        $observed = Invoke-CatalystTestClosedIdentityFailure `
            -EnvironmentName BUILD_SOURCEVERSION `
            -InvalidValue ('b' * 40)

        $observed.Error | Should -Not -BeNullOrEmpty
        $observed.Error.Exception.Message |
            Should -Match 'validated pipeline source'
        $observed.PatchCalls | Should -Be 0
        $observed.PrepareCalls | Should -Be 0
    }

    It 'orders the complete Azure identity guard before mutable production coordination' {
        $coreStart = $script:ProbeSource.IndexOf(
            'function Invoke-CatalystGestureRegressionProbeCore')
        $core = $script:ProbeSource.Substring($coreStart)
        $identity = $core.IndexOf('Assert-CatalystProbeClosedAzureIdentity')
        $patch = $core.IndexOf('New-CatalystProbeFixedPatch')
        $prepare = $core.IndexOf('Invoke-CatalystProbePrepareIosSimulator')

        $identity | Should -BeGreaterOrEqual 0
        $identity | Should -BeLessThan $patch
        $identity | Should -BeLessThan $prepare
    }

    It 'charges installed-runtime inspection and simulator preboot to one coordination deadline' {
        $root = Join-Path $TestDrive 'ios-coordination'
        $logs = Join-Path $root 'logs'
        New-Item -ItemType Directory -Path $logs -Force | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment =
            [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            LogDirectory = $logs
            OwnedIosSimulator = $null
        }
        $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 180
        $script:IosPreparationCalls = 0
        $script:IosPreparationDeadlines =
        [Collections.Generic.List[object]]::new()
        $script:IosPreparationArguments =
        [Collections.Generic.List[string]]::new()
        Mock Get-CatalystProbeXcodePath { '/Applications/Xcode_26.0.1.app' }
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:IosPreparationCalls++
            $script:IosPreparationDeadlines.Add($TaskDeadline)
            $script:IosPreparationArguments.Add(
                "$FileName $(@($ArgumentList) -join ' ')")
            if ($ArgumentList[0] -ceq 'simctl' -and
                $ArgumentList[1] -ceq 'list') {
                $TimeoutSeconds | Should -Be $script:CatalystProbeCoordinationBudgetSeconds
            }
            switch ($script:IosPreparationCalls) {
                1 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '/Applications/Xcode_26.0.1.app/Contents/Developer'
                    }
                }
                2 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"runtimes":[{"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-26-0","name":"iOS 26.0","version":"26.0","isAvailable":true}]}'
                    }
                }
                3 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"devicetypes":[{"name":"iPhone 11 Pro","identifier":"com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro"}]}'
                    }
                }
                4 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"devices":{"com.apple.CoreSimulator.SimRuntime.iOS-26-0":[]}}'
                    }
                }
                5 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '12345678-1234-1234-1234-123456789ABC'
                    }
                }
                default {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = 'ready'
                    }
                }
            }
        }

        $result = Invoke-CatalystProbePrepareIosSimulator `
            -Context $context `
            -CoordinationDeadline $deadline

        $result.udid | Should -BeExactly '12345678-1234-1234-1234-123456789ABC'
        $result.installedOnly | Should -BeTrue
        $result.createdByProbe | Should -BeTrue
        $result.bootedByProbe | Should -BeTrue
        $script:IosPreparationCalls | Should -Be 7
        foreach ($observed in $script:IosPreparationDeadlines) {
            [object]::ReferenceEquals($deadline, $observed) | Should -BeTrue
        }
        @($script:IosPreparationArguments) | Should -Be @(
            '/usr/bin/xcode-select -p',
            '/usr/bin/xcrun simctl list runtimes --json',
            '/usr/bin/xcrun simctl list devicetypes --json',
            '/usr/bin/xcrun simctl list devices --json',
            ('/usr/bin/xcrun simctl create ' +
            'Maui Catalyst Production Composite Probe ' +
            'com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro ' +
            'com.apple.CoreSimulator.SimRuntime.iOS-26-0'),
            '/usr/bin/xcrun simctl boot 12345678-1234-1234-1234-123456789ABC',
            '/usr/bin/xcrun simctl bootstatus 12345678-1234-1234-1234-123456789ABC -b')
        ($script:IosPreparationArguments -join "`n") | Should -Not -Match (
            '(?i)\b(?:downloadPlatform|runtime\s+add|xcode-select\s+-s|' +
            'simctl\s+runtime\s+add)\b')
    }

    It 'allows a slow installed-runtime query within the original coordination deadline' {
        $root = Join-Path $TestDrive 'slow-runtime-query'
        New-Item -ItemType Directory -Path $root | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment = Get-CatalystProbeRuntimeEnvironment `
                -RuntimeRoot (Join-Path $root 'runtime')
            LogDirectory = $root
            OwnedIosSimulator = $null
        }
        $script:RealRuntimeProcess = (Get-Command Invoke-CatalystProbeBoundedProcess).ScriptBlock
        $script:RuntimeQueryCalls = 0
        $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 180
        Mock Get-CatalystProbeXcodePath { '/Applications/Xcode_26.0.1.app' }
        Mock Invoke-CatalystProbeBoundedProcess {
            [object]::ReferenceEquals($TaskDeadline, $deadline) | Should -BeTrue
            if ($FileName -ceq '/usr/bin/xcode-select') {
                return [pscustomobject]@{
                    TimedOut = $false; ExitCode = 0
                    Stdout = '/Applications/Xcode_26.0.1.app/Contents/Developer'
                }
            }
            if ($ArgumentList[2] -ceq 'runtimes') {
                $script:RuntimeQueryCalls++
                return & $script:RealRuntimeProcess `
                    -FileName (Get-Command pwsh -CommandType Application |
                        Select-Object -First 1).Source `
                    -ArgumentList @('-NoProfile', '-NonInteractive', '-Command',
                    '[Threading.Thread]::Sleep(12000); ''{"runtimes":[{"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-26-0","isAvailable":true}]}''') `
                    -WorkingDirectory $WorkingDirectory `
                    -Environment $Environment `
                    -TimeoutSeconds $TimeoutSeconds `
                    -TaskDeadline $TaskDeadline `
                    -LogPath $LogPath
            }
            if ($ArgumentList[2] -ceq 'devicetypes') {
                return [pscustomobject]@{
                    TimedOut = $false; ExitCode = 0
                    Stdout = '{"devicetypes":[{"name":"iPhone 11 Pro","identifier":"com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro"}]}'
                }
            }
            if ($ArgumentList[2] -ceq 'devices') {
                return [pscustomobject]@{
                    TimedOut = $false; ExitCode = 0
                    Stdout = '{"devices":{"com.apple.CoreSimulator.SimRuntime.iOS-26-0":[{"udid":"12345678-1234-1234-1234-123456789ABC","deviceTypeIdentifier":"com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro","isAvailable":true,"state":"Booted"}]}}'
                }
            }
            ($ArgumentList -join ' ') | Should -BeExactly (
                'simctl bootstatus 12345678-1234-1234-1234-123456789ABC -b')
            [pscustomobject]@{ TimedOut = $false; ExitCode = 0; Stdout = '' }
        }

        $result = Invoke-CatalystProbePrepareIosSimulator `
            -Context $context -CoordinationDeadline $deadline
        $result.udid | Should -BeExactly '12345678-1234-1234-1234-123456789ABC'
        $result.createdByProbe | Should -BeFalse
        $script:RuntimeQueryCalls | Should -Be 1
    }

    It 'does not reset the coordination deadline for a late runtime query or retry it' {
        $root = Join-Path $TestDrive 'late-runtime-query'
        New-Item -ItemType Directory -Path $root | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment = Get-CatalystProbeRuntimeEnvironment `
                -RuntimeRoot (Join-Path $root 'runtime')
            LogDirectory = $root
            OwnedIosSimulator = $null
        }
        $script:RealRuntimeProcess = (Get-Command Invoke-CatalystProbeBoundedProcess).ScriptBlock
        $script:RuntimeQueryCalls = 0
        $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 180
        Mock Get-CatalystProbeXcodePath { '/Applications/Xcode_26.0.1.app' }
        Mock Invoke-CatalystProbeBoundedProcess {
            [object]::ReferenceEquals($TaskDeadline, $deadline) | Should -BeTrue
            if ($FileName -ceq '/usr/bin/xcode-select') {
                $TaskDeadline.DeadlineTimestamp =
                [Diagnostics.Stopwatch]::GetTimestamp() +
                (12 * [long]$TaskDeadline.Frequency)
                return [pscustomobject]@{
                    TimedOut = $false; ExitCode = 0
                    Stdout = '/Applications/Xcode_26.0.1.app/Contents/Developer'
                }
            }
            ($ArgumentList -join ' ') | Should -BeExactly 'simctl list runtimes --json'
            $script:RuntimeQueryCalls++
            & $script:RealRuntimeProcess `
                -FileName (Get-Command pwsh -CommandType Application |
                    Select-Object -First 1).Source `
                -ArgumentList @('-NoProfile', '-NonInteractive', '-Command',
                '[Threading.Thread]::Sleep(30000)') `
                -WorkingDirectory $WorkingDirectory `
                -Environment $Environment `
                -TimeoutSeconds $TimeoutSeconds `
                -TaskDeadline $TaskDeadline `
                -LogPath $LogPath
        }

        {
            Invoke-CatalystProbePrepareIosSimulator `
                -Context $context -CoordinationDeadline $deadline
        } | Should -Throw '*could not inspect installed runtimes (exitCode=124, timedOut=True)*'
        $context.OwnedIosSimulator | Should -BeNullOrEmpty
        $script:RuntimeQueryCalls | Should -Be 1
        $log = Get-Content -LiteralPath (
            Join-Path $root 'ios-installed-runtimes.log') -Raw
        $log | Should -Match 'requestedTimeoutSeconds: 180'
        $log | Should -Match 'effectiveTimeoutSeconds: 1[12]'
        $log | Should -Match 'effectiveProcessSeconds: [12]'
    }

    It 'retains owned simulator identity when preboot fails without provisioning fallback' {
        $root = Join-Path $TestDrive 'ios-preboot-failure'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment =
            [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            LogDirectory = $root
            OwnedIosSimulator = $null
        }
        $script:PrebootFailureCalls = 0
        Mock Get-CatalystProbeXcodePath { '/Applications/Xcode_26.0.1.app' }
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:PrebootFailureCalls++
            switch ($script:PrebootFailureCalls) {
                1 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '/Applications/Xcode_26.0.1.app/Contents/Developer'
                    }
                }
                2 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"runtimes":[{"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-26-0","isAvailable":true}]}'
                    }
                }
                3 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"devicetypes":[{"name":"iPhone 11 Pro","identifier":"com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro"}]}'
                    }
                }
                4 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '{"devices":{"com.apple.CoreSimulator.SimRuntime.iOS-26-0":[]}}'
                    }
                }
                5 {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 0
                        Stdout = '12345678-1234-1234-1234-123456789ABC'
                    }
                }
                default {
                    [pscustomobject]@{
                        TimedOut = $false
                        ExitCode = 1
                        Stdout = ''
                    }
                }
            }
        }

        {
            Invoke-CatalystProbePrepareIosSimulator `
                -Context $context `
                -CoordinationDeadline (
                New-CatalystTestTaskDeadline -RemainingSeconds 180)
        } | Should -Throw '*could not boot its owned simulator*'
        $context.OwnedIosSimulator.createdByProbe | Should -BeTrue
        $context.OwnedIosSimulator.bootAttemptedByProbe | Should -BeTrue
        $context.OwnedIosSimulator.bootedByProbe | Should -BeFalse
        $script:PrebootFailureCalls | Should -Be 6
    }

    It 'cleans only a simulator device created by the probe' {
        $root = Join-Path $TestDrive 'owned-ios-cleanup'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment =
            [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            LogDirectory = $root
        }
        $deadline = New-CatalystTestTaskDeadline -RemainingSeconds 120
        $script:OwnedCleanupArguments = [Collections.Generic.List[string]]::new()
        Mock Invoke-CatalystProbeBoundedProcess {
            $script:OwnedCleanupArguments.Add(@($ArgumentList) -join ' ')
            [pscustomobject]@{
                TimedOut = $false
                ExitCode = 0
                Stdout = ''
            }
        }

        Remove-CatalystProbeOwnedIosSimulator `
            -Context $context `
            -Simulator ([pscustomobject]@{
                udid = '12345678-1234-1234-1234-123456789ABC'
                createdByProbe = $true
                bootedByProbe = $true
            }) `
            -CleanupDeadline $deadline

        @($script:OwnedCleanupArguments) | Should -Be @(
            'simctl shutdown 12345678-1234-1234-1234-123456789ABC',
            'simctl delete 12345678-1234-1234-1234-123456789ABC')
    }

    It 'does not mutate a preexisting booted simulator during cleanup' {
        $root = Join-Path $TestDrive 'foreign-ios-cleanup'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $context = [pscustomobject]@{
            RepositoryRoot = $root
            RuntimeEnvironment =
            [Collections.Generic.Dictionary[string, string]]::new(
                [StringComparer]::Ordinal)
            LogDirectory = $root
        }
        Mock Invoke-CatalystProbeBoundedProcess {
            throw 'foreign simulator cleanup must not invoke simctl'
        }

        Remove-CatalystProbeOwnedIosSimulator `
            -Context $context `
            -Simulator ([pscustomobject]@{
                udid = '12345678-1234-1234-1234-123456789ABC'
                createdByProbe = $false
                bootedByProbe = $false
            }) `
            -CleanupDeadline (
            New-CatalystTestTaskDeadline -RemainingSeconds 120)

        Should -Invoke Invoke-CatalystProbeBoundedProcess -Times 0 -Exactly
    }

    It 'labels the retained result as a non-certifying known-bad replay' {
        $result = New-CatalystProbeResult `
            -ExpectedSourceVersion ('a' * 40) `
            -TrustedTreeHash ('b' * 64) `
            -TrustedTreeAttestationSha256 ('c' * 64)

        $result.schemaVersion | Should -Be 2
        $result.scope | Should -BeExactly 'known-bad-postimage-replay-only'
        $result.exclusions.certifiesIssue | Should -BeFalse
        $result.exclusions.certifiesProduct | Should -BeFalse
        $result.exclusions.publishesOutcome | Should -BeFalse
        $result.exclusions.mutatesPullRequest | Should -BeFalse
        $result.exclusions.invokesModel | Should -BeFalse
        $result.exclusions.generatedIssueTest | Should -BeFalse
        $result.selectors.primary.metadataAnchorIsGeneratedTest | Should -BeFalse
        $result.limitations.assetsFileRetained | Should -BeFalse
    }

    It 'accepts only the exact strict known-negative rejection in the production flow' {
        $root = Join-Path $TestDrive 'production-core'
        $output = Join-Path $root 'output'
        $trusted = Join-Path $root 'trusted'
        $logs = Join-Path $output 'logs'
        $attestation = Join-Path $root 'trusted-tree.json'
        $product = Join-Path $root 'product.cs'
        $fixture = Join-Path $root 'fixture.cs'
        New-Item -ItemType Directory -Path $output, $trusted, $logs -Force |
            Out-Null
        Set-Content -LiteralPath $attestation -Value '{}' -Encoding utf8NoBOM
        Set-Content -LiteralPath $product -Value 'baseline' -Encoding utf8NoBOM
        $task = New-CatalystTestTaskDeadline
        $script:ProductionCoreContext = [pscustomobject]@{
            ExpectedSourceVersion = 'a' * 40
            RepositoryRoot = $root
            TrustedRoot = $trusted
            TrustedTreeAttestation = $attestation
            OutputDirectory = $output
            ResultPath = Join-Path $output 'catalyst-gesture-probe.json'
            LogDirectory = $logs
            FixtureTargetPath = $fixture
            TrustedFixturePath = Join-Path $trusted (
                $script:CatalystProbeFixtureRelativePath)
            ProductPath = $product
            JobDeadlineUtc = [DateTimeOffset]::UtcNow.AddHours(1).ToString('O')
            ArtifactTailSeconds = 300
            TaskDeadline = $task
            ActiveDeadline = $task
            ActiveReserveSeconds = 0
            RepositoryState = 'setup'
        }
        $script:ProductionCompositeCalls = 0
        $script:ProductionCompositeDeadlines =
        [Collections.Generic.List[object]]::new()
        $selection = [pscustomobject]@{
            BaselineSha = $script:CatalystProbeBaselineCommit
            Platform = 'ios'
            Project = 'Controls'
            ProjectPath = $script:CatalystProbeProjectPath
            Category = 'Label'
            TestClass = $script:CatalystProbePrimaryClass
            GeneratedTestPath = $script:CatalystProbePrimaryAnchorPath
        }
        $requirement = [pscustomobject]@{
            Id = 'gesture-platform-manager-catalyst-v1'
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
        Mock Get-CatalystProbeFileSha256 {
            if ($Path -ceq $script:ProductionCoreContext.ProductPath) {
                $script:CatalystProbeBaselineFileSha256
            } else {
                'c' * 64
            }
        }
        Mock Assert-CatalystProbeClosedAzureIdentity {}
        Mock Initialize-CatalystGestureProbeContext {
            $script:ProductionCoreContext
        }
        Mock New-CatalystProbeFixedPatch {
            Join-Path $output 'known-negative-product.patch'
        }
        Mock Assert-CatalystProbeFixedPatchPolicy {}
        Mock Invoke-CatalystProbePrepareIosSimulator {
            [pscustomobject]@{ udid = 'SIMULATOR'; installedOnly = $true }
        }
        Mock New-CatalystProbeProductionCompositeModule {
            New-Module -Name $script:CatalystProbeProductionModuleName `
                -ScriptBlock {}
        }
        Mock Get-CatalystProbeProductionCompositeSelection {
            [pscustomobject]@{
                Selection = $selection
                Requirement = $requirement
            }
        }
        Mock Invoke-CatalystProbeProductionPrewarm {
            $prewarmRoot = Join-Path $output 'production-composite/prewarm'
            New-Item -ItemType Directory -Path $prewarmRoot -Force | Out-Null
            foreach ($name in @(
                    'prewarm-build-ios-simulator-no-restore.log',
                    'prewarm-build-maccatalyst-no-restore.log',
                    'prewarm-restore-dual-apple-graph.log',
                    'tool-restore.log',
                    'xharness-command-probe/xharness-help-tail.log',
                    'xharness-command-probe/xharness-help.log',
                    'xharness-preflight-child.log',
                    'xharness-preflight/xharness-help-tail.log',
                    'xharness-preflight/xharness-help.log',
                    'xharness-preflight/xharness-preflight.log')) {
                $path = Join-Path $prewarmRoot $name
                New-Item -ItemType Directory -Path (Split-Path -Parent $path) `
                    -Force | Out-Null
                Set-Content -LiteralPath $path `
                    -Value 'bounded' -Encoding utf8NoBOM
            }
            [pscustomobject]@{
                AssetsSha256 = 'd' * 64
                TargetPairs = @(
                    'net10.0-ios/iossimulator-arm64',
                    'net10.0-maccatalyst/maccatalyst-arm64')
            }
        }
        Mock Assert-CatalystProbeProductionAssetsRetained {
            [pscustomobject]@{
                AssetsSha256 = 'd' * 64
                TargetPairs = @(
                    'net10.0-ios/iossimulator-arm64',
                    'net10.0-maccatalyst/maccatalyst-arm64')
            }
        }
        Mock Assert-CatalystProbeRepositoryState {}
        Mock Invoke-CatalystProbeProductionComposite {
            $script:ProductionCompositeCalls++
            $script:ProductionCompositeDeadlines.Add($Deadline)
            if ($script:ProductionCompositeCalls -eq 2) {
                throw $script:CatalystProbeExpectedCompanionRejection
            }
        }
        Mock Assert-CatalystProbePrimaryRun {
            [pscustomobject]@{
                Digest = 'e' * 64
                Document = [pscustomobject]@{
                    total = 10
                    passed = 5
                    skipped = 5
                    failed = 0
                    resultFiles = @([pscustomobject]@{
                            name = 'label.xml'
                            sha256 = 'f' * 64
                        })
                }
            }
        }
        Mock Assert-CatalystProbeCycleEvidence {
            [pscustomobject]@{
                StrictEvidenceSha256 = '1' * 64
                ResultFiles = @([pscustomobject]@{
                        name = 'catalyst.xml'
                        sha256 = '2' * 64
                    })
                Identities = $identities
                Total = 2
                Passed = if ($Kind -ceq 'baseline') { 2 } else { 0 }
                Failed = if ($Kind -ceq 'negative') { 2 } else { 0 }
                Skipped = 0
                Errors = 0
            }
        }
        Mock Enable-CatalystProbeKnownNegative {
            $Context.RepositoryState = 'negative'
        }
        Mock Invoke-CatalystProbeProductionV2Validation {
            $path = Join-Path $EvidenceArtifactRoot (
                'regression/regression-evidence.json')
            New-Item -ItemType Directory -Path (Split-Path $path) -Force |
                Out-Null
            Set-Content -LiteralPath $path -Value '{}' -Encoding utf8NoBOM
            throw $script:CatalystProbeExpectedCompanionRejection
        }
        Mock Restore-CatalystProbeRepository {}
        Mock Remove-CatalystProbeProductionCompositeModule {}
        Mock Assert-CatalystProbeDeadlineAdmission {}

        $result = Invoke-CatalystGestureRegressionProbeCore `
            -ExpectedSourceVersion ('a' * 40) `
            -RepositoryRoot $root `
            -TrustedRoot $trusted `
            -TrustedTreeAttestation $attestation `
            -OutputDirectory $output `
            -TaskDeadline $task

        $result.outcome | Should -BeExactly (
            'production-composite-baseline-pass/known-negative-rejected')
        $result.expectedV2Rejection.rejected | Should -BeTrue
        $result.expectedV2Rejection.certifiable | Should -BeFalse
        $result.cleanup.completed | Should -BeTrue
        $result.cleanup.fixtureRemoved | Should -BeTrue
        $result.cleanup.productRestored | Should -BeTrue
        $result.trustedImplementation.'scripts/Replicate-Issue.ps1' |
            Should -BeExactly ('c' * 64)
        $result.baseline.primary.skipped | Should -Be 5
        $result.baseline.companion.passed | Should -Be 2
        $result.negative.companion.failed | Should -Be 2
        $result.prewarm.privateToolRestore | Should -BeTrue
        $result.prewarm.xharnessCommandPreflight | Should -BeTrue
        @($result.prewarm.commandLogs) | Should -HaveCount 10
        $result.baseline.primary.rawXml[0].name | Should -BeExactly 'label.xml'
        $result.negative.companion.rawXml[0].name |
            Should -BeExactly 'catalyst.xml'
        $script:ProductionCompositeCalls | Should -Be 2
        $script:ProductionCompositeDeadlines | Should -HaveCount 2
        foreach ($deadline in $script:ProductionCompositeDeadlines) {
            $deadline.BudgetSeconds | Should -Be 480
        }
    }
}

Describe 'Catalyst gesture pipeline isolation' {
    It 'selects the fixed baseline Xcode instead of the review override' {
        $script:Stage | Should -Not -Match 'REQUIRED_XCODE'
        Get-CatalystProbeXcodePath | Should -BeExactly '/Applications/Xcode_26.0.1.app'
        $script:Stage | Should -Match '\$xcode = Get-CatalystProbeXcodePath'
        $script:Stage | Should -Match 'Assert-CatalystProbeXcodeVersion -VersionOutput \$xcodeVersion'
        $script:Stage.IndexOf('Assert-CatalystProbeXcodeVersion') |
            Should -BeLessThan $script:Stage.IndexOf("--target=dotnet ")
    }

    It 'rejects a selected Xcode version that differs from the immutable baseline' {
        { Assert-CatalystProbeXcodeVersion -VersionOutput @(
                'Xcode 26.0.1', 'Build version 17A400') } | Should -Not -Throw
        { Assert-CatalystProbeXcodeVersion -VersionOutput @(
                'Xcode 26.5', 'Build version 17F42') } |
            Should -Throw '*requires the fixed baseline Xcode 26.0.1*'
        { Assert-CatalystProbeXcodeVersion -VersionOutput @(
                'Xcode 26.0.1', 'Build version 17A400', 'unexpected output') } |
            Should -Throw '*requires the fixed baseline Xcode 26.0.1*'
        { Assert-CatalystProbeXcodeVersion -VersionOutput @() } |
            Should -Throw '*requires the fixed baseline Xcode 26.0.1*'
    }

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
            "displayName: 'Run fixed report-only production composite'")
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

    It 'prewarms the fixed dual-Apple graph and keeps both cycles bounded' {
        foreach ($value in @(
                'IncludeMacCatalystTargetFrameworks=true',
                'IncludeIosTargetFrameworks=true',
                'IncludeAndroidTargetFrameworks=false',
                'IncludeWindowsTargetFrameworks=false',
                'IncludeMacOSTargetFrameworks=false',
                '--force-evaluate',
                '--no-restore',
                'iossimulator-arm64',
                'maccatalyst-arm64')) {
            $script:PrewarmSource | Should -Match ([regex]::Escape($value))
        }
        $script:PrewarmSource | Should -Not -Match '-p:RuntimeIdentifiers='
        $script:ProbeSource | Should -Match 'New-CatalystProbePrewarmDeadline'
        $script:ProbeSource | Should -Match (
            "'Replication-AppleCompanionPrewarm\.ps1'")
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
