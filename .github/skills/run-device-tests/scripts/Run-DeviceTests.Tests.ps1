#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Run-DeviceTests.ps1'
    $script:WindowsDeviceNoResultsMarker = 'WINDOWS_DEVICE_TEST_NO_RESULTS:'
    $script:WindowsDeviceTargetTimeoutMarker = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:'
    $script:WindowsDeviceCleanupFailureMarker = 'WINDOWS_DEVICE_TEST_CLEANUP_FAILED:'

    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $bindingPath = Join-Path $PSScriptRoot '../../../scripts/shared/Assert-ReplicationCertificationBinding.ps1'
    $bindingTokens = $null
    $bindingErrors = $null
    $bindingAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $bindingPath,
        [ref]$bindingTokens,
        [ref]$bindingErrors)
    if ($bindingErrors) {
        throw ($bindingErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }
    foreach ($functionName in @(
        'Get-ReplicationBindingFileDigest',
        'Get-ReplicationDeviceTestFailureSignature',
        'Read-ReplicationDeviceTestResultXmlStrict'
    )) {
        $function = $bindingAst.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)
        Invoke-Expression $function.Extent.Text
    }

    foreach ($functionName in @(
        'ConvertTo-AzdoSafeConsole',
        'Test-DeviceTestStrictRegressionSelector',
        'Invoke-BoundedWindowsDeviceBuild',
        'Get-CategoryFiltersFromTestFilter',
        'ConvertTo-DeviceTestClassFilterValue',
        'New-AndroidDeviceTestClassFilterInjection',
        'Get-XHarnessTestResultSnapshot',
        'Get-FreshXHarnessTestResultFiles',
        'New-XHarnessRunOutputDirectory',
        'ConvertTo-BoundedXHarnessDiagnosticText',
        'Write-XHarnessDiagnosticText',
        'ConvertTo-XHarnessDiagnosticLine',
        'Invoke-StreamingXHarnessCommand',
        'Get-XHarnessDiagnosticInventory',
        'New-XHarnessNoResultDiagnostic',
        'Test-XHarnessHelpExitCode',
        'Invoke-XHarnessPreflight',
        'Select-WindowsDeviceTestCategories',
        'Test-WindowsDeviceTestCategoryDiscovery',
        'Start-WindowsDeviceTestProcess',
        'Wait-ForPath',
        'ConvertTo-DeviceTestCount',
        'Get-DeviceTestResultSummary',
        'Write-DeviceTestStrictEvidence',
        'Complete-DeviceTestStrictEvidence',
        'Copy-DeviceTestStrictResultsToDurableDirectory',
        'Invoke-WindowsDeviceTestApp'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Build isolation options' {
    It 'embeds Android assemblies in the APK for XHarness in every configuration' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match (
            '(?s)"android"\s*\{\s*' +
            '\$buildArgs \+= "/p:AndroidPackageFormat=apk"\s*' +
            '(?:#[^\r\n]*\s*)?' +
            '\$buildArgs \+= "/p:EmbedAssembliesIntoApk=true"\s*\}')
    }

    It 'supports rebuilding the full project graph for A/B Gate runs' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\[switch\]\$Rebuild'
        $content | Should -Match '(?s)if \(\$Rebuild\)\s*\{\s*\$buildArgs \+= "-t:Rebuild"\s*\}'
    }

    It 'keeps Windows category results scoped to the requested class and methods' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\$summaryClassFilter\s*=\s*\$IncludeClasses'
        $content | Should -Match '\$summaryMethodFilter\s*=\s*\$IncludeMethods'
        $content | Should -Match (
            '(?s)-RequireClassIsolation:\(\s*\(\s*-not \$RequireAppContainer -or\s*' +
            '-not \[string\]::IsNullOrWhiteSpace\(\$StrictTestEvidencePath\)\)\s*-and\s*' +
            '-not \[string\]::IsNullOrWhiteSpace\(\$IncludeClasses\)\)')
        $content | Should -Not -Match '\$summaryClassFilter\s*=\s*if\s*\(-not\s+\$useCategoryFiltering\)'
    }

    It 'normalizes Mac Catalyst strict evidence to the replication platform name' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match ([regex]::Escape(
            "-Platform `$(if (`$Platform -ceq 'maccatalyst') { 'catalyst' } else { `$Platform })"))
    }

    It 'admits only the immutable class-shaped strict selector used by desktop regression runs' {
        $output = Join-Path $TestDrive 'regression'
        $valid = @{
            Project = 'Controls'
            TestFilter = 'Category=Label'
            IncludeClasses = 'Microsoft.Maui.DeviceTests.LabelTests'
            IncludeMethods = ''
            OutputDirectory = $output
            StrictTestEvidencePath = (Join-Path $output 'strict-test-evidence.json')
        }

        Test-DeviceTestStrictRegressionSelector @valid | Should -BeTrue
        foreach ($mutation in @(
            @{ Project = 'Core' },
            @{ IncludeMethods = 'HtmlTextInitializesCorrectly' },
            @{ IncludeClasses = 'Microsoft.Maui.DeviceTests.Issue29282Tests' },
            @{ IncludeClasses = (
                'Microsoft.Maui.DeviceTests.LabelTests,' +
                'Microsoft.Maui.DeviceTests.ButtonTests') },
            @{ TestFilter = 'Category=Issue29282' },
            @{ StrictTestEvidencePath = (Join-Path $TestDrive 'outside.json') }
        )) {
            $candidate = @{} + $valid
            foreach ($name in $mutation.Keys) { $candidate[$name] = $mutation[$name] }
            Test-DeviceTestStrictRegressionSelector @candidate | Should -BeFalse
        }
    }

    It 'loads the strict XML parser only from the trusted runner tree' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match (
            '(?s)\$trustedScriptRoot = Split-Path -Parent \(\s*' +
            'Split-Path -Parent \(Split-Path -Parent \$PSScriptRoot\)\)')
        $content | Should -Match (
            "scripts/shared/Assert-ReplicationCertificationBinding\.ps1")
        $content | Should -Match (
            '(?s)\$strictEvidenceHelper.+?\[IO\.FileAttributes\]::ReparsePoint')
        $content | Should -Not -Match (
            'Join-Path \$SharedScriptsDir "Assert-ReplicationCertificationBinding\.ps1"')
    }

    It 'packages replication tests into the audited Controls AppContainer only' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\[switch\]\$RequireWindowsAppContainer'
        $content | Should -Match (
            "Windows replication requires either one exact issue test or one strict sibling regression class")
        $content | Should -Match 'ReplicationWindowsControlsDeviceTestsManifest\.xml'
        $content | Should -Match '/p:WindowsPackageType=MSIX'
        $content | Should -Match '/p:PublishReadyToRun=false'
        $content | Should -Match '\$windowsGraphBuildArgs = @\('
        $content | Should -Match '/p:WindowsPackageType=None'
        $content | Should -Match '/p:BuildProjectReferences=true'
        $content | Should -Match '/p:BuildProjectReferences=false'
        $content | Should -Match (
            '/p:WindowsAppSdkBootstrapInitialize=false')
        $content | Should -Match (
            '/p:WindowsAppSdkDeploymentManagerInitialize=false')
        $graphBuildArguments = [regex]::Match(
            $content,
            '(?ms)\$windowsGraphBuildArgs = @\(.*?^\s*\)').Value
        $graphBuildArguments | Should -Match '/p:WindowsPackageType=None'
        $graphBuildArguments | Should -Match '/p:BuildProjectReferences=true'
        $graphBuildArguments | Should -Not -Match (
            'PackageManifest|GenerateAppxPackageOnBuild|' +
            'PackageCertificateThumbprint|AppxPackageDir')
        $graphBuild = $content.IndexOf(
            '-Arguments $windowsGraphBuildArgs',
            [StringComparison]::Ordinal)
        $packageBuild = $content.IndexOf(
            '-Arguments $buildArgs',
            $graphBuild,
            [StringComparison]::Ordinal)
        $graphBuild | Should -BeGreaterOrEqual 0
        $packageBuild | Should -BeGreaterThan $graphBuild
        $deploymentInitializer = $content.IndexOf(
            '$buildArgs += "/p:WindowsAppSdkDeploymentManagerInitialize=false"',
            $graphBuild,
            [StringComparison]::Ordinal)
        $deploymentInitializer | Should -BeGreaterThan $graphBuild
        $deploymentInitializer | Should -BeLessThan $packageBuild
        $content | Should -Match '/p:GenerateAppxPackageOnBuild=true'
        $content | Should -Match (
            '/p:CustomAfterMicrosoftCommonTargets=' +
            '\$windowsManifestOverrideTargets')
        $content | Should -Match (
            '/p:MauiReplicationAppContainerManifest=' +
            '\$windowsManifestPath')
        $content | Should -Match (
            '/p:_MauiManifestStampFile=\$windowsManifestStampPath')
        $content | Should -Match (
            '\[IO\.Path\]::GetFullPath\(\s*' +
            '\(Join-Path \$RepoRoot \$projectPath\)\)')
        $content | Should -Not -Match '_MauiReplicationWindowsManifest'
        $content | Should -Match 'Install-ReplicationWindowsAppContainerPackage'
        $content | Should -Match (
            '-ManifestObservationRoot \$windowsManifestObservationRoot')
        $content | Should -Match (
            '\$windowsManifestObservationDirectory')
        $content | Should -Match 'Start-ReplicationWindowsAppContainerProcess'
        $content | Should -Match 'PackageLocalStatePath'
        $content | Should -Match 'Remove-ReplicationWindowsAppContainerPackage'
        $content | Should -Match 'WINDOWS_DEVICE_TEST_CLEANUP_FAILED:'
        $content | Should -Match 'windows-unpackaged-graph-build\.log'
        $content | Should -Match 'windows-top-level-package-build\.log'
    }

    It 'retains bounded redacted Windows compiler diagnostics' {
        $root = Join-Path $TestDrive 'windows-build-diagnostics'
        New-Item -ItemType Directory -Path $root | Out-Null
        $logPath = Join-Path $root 'graph-build.log'
        $secret = 'ghp_abcdefghijklmnopqrstuvwxyz0123456789'
        $message = ''
        try {
            Invoke-BoundedWindowsDeviceBuild `
                -Arguments @('build', 'Controls.DeviceTests.csproj') `
                -LogPath $logPath `
                -AllowedRoot $root `
                -Description 'Unpackaged Windows graph build' `
                -CommandInvoker {
                    [pscustomobject]@{
                        ExitCode = 1
                        Output = @(
                            "GH_TOKEN=$secret",
                            "Authorization: Bearer $secret",
                            ('Issue37540.Windows.cs(42,17): error CS1061: ' +
                                "'Label' does not contain a definition for 'Missing'"),
                            'Build FAILED.'
                        )
                    }
                }
        } catch {
            $message = $_.Exception.Message
        }

        $message | Should -Match 'error CS1061'
        $message | Should -Match 'Retained diagnostics:'
        $message | Should -Not -Match ([regex]::Escape($secret))
        $item = Get-Item -LiteralPath $logPath -Force
        $item.PSIsContainer | Should -BeFalse
        ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) |
            Should -BeFalse
        $item.Length | Should -BeGreaterThan 0
        $item.Length | Should -BeLessOrEqual 1MB
        $content = Get-Content -LiteralPath $logPath -Raw
        $content | Should -Match 'error CS1061'
        $content | Should -Match '<redacted'
        $content | Should -Not -Match ([regex]::Escape($secret))

        {
            Invoke-BoundedWindowsDeviceBuild `
                -Arguments @('build') `
                -LogPath (Join-Path $TestDrive 'outside.log') `
                -AllowedRoot $root `
                -Description 'outside probe' `
                -CommandInvoker {
                    [pscustomobject]@{ ExitCode = 0; Output = @('ok') }
                }
        } | Should -Throw '*must stay inside its trusted root*'
    }

    It 'signs Mac Catalyst replication tests into the no-network App Sandbox' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\[switch\]\$RequireMacCatalystAppSandbox'
        $content | Should -Match (
            'ReplicationMacCatalystControlsDeviceTests\.entitlements')
        $content | Should -Match '/p:CodesignEntitlements=\$catalystEntitlementsPath'
        $content | Should -Match '/p:MtouchDebug=false'
        $content | Should -Match '/p:UseSystemResourceKeys=false'
        $content | Should -Match (
            'maccatalyst-\$\(\[Runtime\.InteropServices\.RuntimeInformation\]' +
            '::OSArchitecture\.ToString\(\)\.ToLowerInvariant\(\)\)')
        $content | Should -Not -Match (
            'RuntimeIdentifier\s*=\s*"maccatalyst-arm64"')
        $content | Should -Match 'Assert-ReplicationSignedMacCatalystAppSandbox'
        $content | Should -Match 'Start-ReplicationMacCatalystAppSandbox'
        $content | Should -Match 'Invoke-ReplicationMacCatalystDeviceTests'
        $content | Should -Match (
            '\$timeoutSeconds = \[Math\]::Min\(\$timeoutSeconds, 600\)')
        $content | Should -Match (
            '(?s)if \(\$RequireMacCatalystAppSandbox\).*?' +
            'Get-DeviceTestResultSummary.*?-RequireClassIsolation')
        $content | Should -Match (
            '(?s)\$platformConfig\.UsesXHarness -and\s*' +
            '-not \$RequireMacCatalystAppSandbox')
        $content | Should -Match (
            'Mac Catalyst App Sandbox denied outbound networking at runtime')
        $content | Should -Match 'App Sandbox'
        $content | Should -Match 'file channel'
        $content | Should -Match (
            'Expected exactly one isolated Mac Catalyst app')
    }
}

Describe 'Cross-platform device test class filtering' {
    It 'normalizes comma/semicolon-separated class names for the XHarness include variable' {
        ConvertTo-DeviceTestClassFilterValue `
            -Value ' Microsoft.Maui.DeviceTests.NewTests;Microsoft.Maui.DeviceTests.ExistingTests, Microsoft.Maui.DeviceTests.NewTests ' |
            Should -Be 'Microsoft.Maui.DeviceTests.NewTests,Microsoft.Maui.DeviceTests.ExistingTests'
    }

    It 'preserves normal execution when the host class filter is empty' {
        ConvertTo-DeviceTestClassFilterValue -Value '  ' | Should -BeNullOrEmpty
    }

    It 'rejects control characters before using a PR-derived class filter' {
        { ConvertTo-DeviceTestClassFilterValue -Value "Microsoft.Maui.Tests.Valid`nInjected" } |
            Should -Throw -ExpectedMessage '*control character*'
    }

    It 'encodes Android class names instead of embedding PR-derived text as C# source' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "android-class-filter-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $filter = 'Microsoft.Maui.Tests.Safe"; throw new System.Exception(); //'
            $injection = New-AndroidDeviceTestClassFilterInjection -IncludeClasses $filter -TempRoot $tempRoot
            $source = Get-Content $injection.SourcePath -Raw
            $targets = Get-Content $injection.TargetsPath -Raw

            $source | Should -Not -Match ([regex]::Escape($filter))
            $source | Should -Match 'FromBase64String'
            $source | Should -Match 'NUNIT_SKIPPED_CLASSES'
            $targets | Should -Match ([regex]::Escape("'`$(MSBuildProjectName)' == '`$(MauiCopilotClassFilterTargetProject)'"))
            $injection.TargetProject | Should -Be 'TestUtils.DeviceTests.Runners'
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'injects the class filter into the referenced shared runner before XHarness reads options' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "android-class-filter-build-$([guid]::NewGuid())"
        $runnerDir = Join-Path $tempRoot 'Runner'
        $appDir = Join-Path $tempRoot 'App'
        New-Item -ItemType Directory -Path $runnerDir, $appDir -Force | Out-Null

        try {
            @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
'@ | Set-Content (Join-Path $runnerDir 'TestUtils.DeviceTests.Runners.csproj') -Encoding UTF8
            'namespace Runner; public sealed class Marker { }' |
                Set-Content (Join-Path $runnerDir 'Marker.cs') -Encoding UTF8

            @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../Runner/TestUtils.DeviceTests.Runners.csproj" />
  </ItemGroup>
</Project>
'@ | Set-Content (Join-Path $appDir 'App.csproj') -Encoding UTF8
            @'
_ = new Runner.Marker();
System.Console.WriteLine(System.Environment.GetEnvironmentVariable("NUNIT_SKIPPED_CLASSES"));
'@ | Set-Content (Join-Path $appDir 'Program.cs') -Encoding UTF8

            $classFilter = 'Microsoft.Maui.Tests.One,Microsoft.Maui.Tests.Two'
            $injection = New-AndroidDeviceTestClassFilterInjection -IncludeClasses $classFilter -TempRoot $tempRoot
            $buildOutput = & dotnet build (Join-Path $appDir 'App.csproj') --nologo --verbosity quiet `
                "/p:CustomAfterMicrosoftCSharpTargets=$($injection.TargetsPath)" `
                "/p:MauiCopilotClassFilterSourcePath=$($injection.SourcePath)" `
                "/p:MauiCopilotClassFilterTargetProject=$($injection.TargetProject)" 2>&1

            $LASTEXITCODE | Should -Be 0 -Because ($buildOutput -join [Environment]::NewLine)

            $runOutput = & dotnet (Join-Path $appDir 'bin/Debug/net8.0/App.dll') 2>&1
            $LASTEXITCODE | Should -Be 0 -Because ($runOutput -join [Environment]::NewLine)
            ($runOutput -join [Environment]::NewLine) |
                Should -Match '\[Maui Copilot Gate\] XHarness class filter: Microsoft\.Maui\.Tests\.One,Microsoft\.Maui\.Tests\.Two'
            @($runOutput)[-1] | Should -Be $classFilter
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'applies the trusted packaged Windows class selector before execution and rejects unsafe selectors' {
        $appDir = Join-Path $TestDrive 'windows-packaged-class-filter'
        New-Item -ItemType Directory -Path $appDir -Force | Out-Null
        $sourcePath = Join-Path $PSScriptRoot (
            '../../../scripts/shared/ReplicationWindowsDeviceTestClassFilter.cs')
        $targetsPath = Join-Path $PSScriptRoot (
            '../../../scripts/shared/ReplicationWindowsDeviceTestClassFilter.targets')
        @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
'@ | Set-Content (
            Join-Path $appDir 'TestUtils.DeviceTests.Runners.csproj'
        ) -Encoding utf8NoBOM
        @'
var available = new[]
{
    "Microsoft.Maui.DeviceTests.LabelTests",
    "Microsoft.Maui.DeviceTests.FormattedStringTests"
};
var selected = Environment.GetEnvironmentVariable("NUNIT_SKIPPED_CLASSES");
Console.WriteLine("selected=" + string.Join(",", available.Where(value => value == selected)));
'@ | Set-Content (Join-Path $appDir 'Program.cs') -Encoding utf8NoBOM

        $buildOutput = & dotnet build (
            Join-Path $appDir 'TestUtils.DeviceTests.Runners.csproj'
        ) --nologo --verbosity quiet `
            "/p:CustomAfterMicrosoftCSharpTargets=$targetsPath" `
            "/p:MauiReplicationWindowsClassFilterSource=$sourcePath" 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($buildOutput -join [Environment]::NewLine)
        $app = Join-Path $appDir (
            'bin/Debug/net8.0/TestUtils.DeviceTests.Runners.dll')

        $safeOutput = @(& dotnet $app (
                '--maui-replication-include-class=' +
                'Microsoft.Maui.DeviceTests.LabelTests') 2>&1)
        $LASTEXITCODE | Should -Be 0 -Because ($safeOutput -join [Environment]::NewLine)
        $safeOutput | Should -Contain (
            'selected=Microsoft.Maui.DeviceTests.LabelTests')
        ($safeOutput -join [Environment]::NewLine) |
            Should -Not -Match 'selected=.*FormattedStringTests'

        $unsafeOutput = @(& dotnet $app (
                '--maui-replication-include-class=' +
                'Microsoft.Maui.DeviceTests.LabelTests;Microsoft.Maui.DeviceTests.FormattedStringTests') 2>&1)
        $LASTEXITCODE | Should -Not -Be 0
        ($unsafeOutput -join [Environment]::NewLine) |
            Should -Match 'packaged device-test class selector is invalid'
    }

    It 'injects the trusted Windows class-filter override into the baseline graph build' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match (
            "scripts/shared/ReplicationWindowsDeviceTestClassFilter\.cs")
        $content | Should -Match (
            "scripts/shared/ReplicationWindowsDeviceTestClassFilter\.targets")
        $content | Should -Match (
            '(?s)\$windowsGraphBuildArgs = @\(.*?' +
            'if \(\$strictRegressionSelector\)\s*\{\s*' +
            '\$windowsGraphBuildArgs \+= \$windowsClassFilterBuildProperties\s*\}.*?' +
            'Invoke-BoundedWindowsDeviceBuild')
        $content | Should -Match (
            '(?s)Start-WindowsDeviceTestProcess.*?' +
            '-PackagedIncludeClass \$packagedIncludeClass')
    }

    It 'passes only the validated regression class to the packaged Windows process' {
        $script:capturedPackagedArguments = $null
        function ConvertTo-ReplicationWindowsAppArguments {
            param([string[]]$Arguments)
            $script:capturedPackagedArguments = @($Arguments)
            return 'encoded arguments'
        }
        function Start-ReplicationWindowsAppContainerProcess {
            param(
                [string]$PackageName,
                [string]$AppArguments,
                [switch]$RequireWindow
            )
            return [pscustomobject]@{
                Process = [pscustomobject]@{ Id = 42 }
            }
        }

        $className = 'Microsoft.Maui.DeviceTests.LabelTests'
        $null = Start-WindowsDeviceTestProcess `
            -ArgumentList @('result.xml', '7') `
            -IncludeClasses $className `
            -PackagedIncludeClass $className `
            -RequireAppContainer `
            -PackageName 'Microsoft.Maui.Controls.DeviceTests'

        $script:capturedPackagedArguments | Should -Be @(
            'result.xml',
            '7',
            "--maui-replication-include-class=$className"
        )
        {
            Start-WindowsDeviceTestProcess `
                -ArgumentList @('result.xml', '7') `
                -PackagedIncludeClass 'Microsoft.Maui.DeviceTests.Issue29282Tests' `
                -RequireAppContainer `
                -PackageName 'Microsoft.Maui.Controls.DeviceTests'
        } | Should -Throw -ExpectedMessage (
            '*packaged device-test class selector is invalid*')
    }

    It 'uses the built-in XHarness class include variable for Apple runs' {
        Get-Content $scriptPath -Raw |
            Should -Match '--set-env=NUNIT_SKIPPED_CLASSES=\$IncludeClasses'
    }

    It 'does not reuse a stale XHarness result file when the current run produces none' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xharness-results-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $resultFile = Join-Path $tempRoot 'testResults.xml'
            '<assemblies />' | Set-Content $resultFile -Encoding UTF8
            $snapshot = Get-XHarnessTestResultSnapshot -OutputDirectory $tempRoot

            @(Get-FreshXHarnessTestResultFiles -OutputDirectory $tempRoot -BeforeSnapshot $snapshot).Count |
                Should -Be 0

            '<assemblies><assembly /></assemblies>' | Set-Content $resultFile -Encoding UTF8
            @(Get-FreshXHarnessTestResultFiles -OutputDirectory $tempRoot -BeforeSnapshot $snapshot) |
                Should -Be @($resultFile)
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'discovers fresh Apple XHarness xUnit result files' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xharness-apple-results-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $pattern = 'xunit-test-*.xml'
            $snapshot = Get-XHarnessTestResultSnapshot `
                -OutputDirectory $tempRoot `
                -ResultFileName $pattern
            $resultFile = Join-Path $tempRoot 'xunit-test-ios-simulator-64_26.5.xml'
            '<assemblies><assembly total="1" failed="1" /></assemblies>' |
                Set-Content $resultFile -Encoding UTF8

            @(Get-FreshXHarnessTestResultFiles `
                -OutputDirectory $tempRoot `
                -BeforeSnapshot $snapshot `
                -ResultFileName $pattern) |
                Should -Be @($resultFile)
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'isolates repeated class-filtered XHarness invocations under the diagnostics root' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xharness-run-root-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            '<assemblies />' | Set-Content (Join-Path $tempRoot 'testResults.xml') -Encoding UTF8

            $first = New-XHarnessRunOutputDirectory -OutputDirectory $tempRoot
            $second = New-XHarnessRunOutputDirectory -OutputDirectory $tempRoot

            $first | Should -Not -Be $second
            Test-Path -LiteralPath $first -PathType Container | Should -BeTrue
            Test-Path -LiteralPath $second -PathType Container | Should -BeTrue
            @(Get-ChildItem -LiteralPath $first -Force).Count | Should -Be 0
            @(Get-ChildItem -LiteralPath $second -Force).Count | Should -Be 0
            Test-Path -LiteralPath (Join-Path $tempRoot 'testResults.xml') | Should -BeTrue
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'requires the trusted per-run XHarness result filename' {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xharness-result-name-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $expectedName = "testResults-$([guid]::NewGuid().ToString('N')).xml"
            '<assemblies />' | Set-Content (Join-Path $tempRoot 'testResults.xml') -Encoding UTF8
            $snapshot = Get-XHarnessTestResultSnapshot `
                -OutputDirectory $tempRoot `
                -ResultFileName $expectedName

            @(Get-FreshXHarnessTestResultFiles `
                -OutputDirectory $tempRoot `
                -BeforeSnapshot $snapshot `
                -ResultFileName $expectedName).Count | Should -Be 0

            $expectedFile = Join-Path $tempRoot $expectedName
            '<assemblies><assembly /></assemblies>' | Set-Content $expectedFile -Encoding UTF8
            @(Get-FreshXHarnessTestResultFiles `
                -OutputDirectory $tempRoot `
                -BeforeSnapshot $snapshot `
                -ResultFileName $expectedName) | Should -Be @($expectedFile)
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'uses the isolated XHarness directory for execution and fresh-result discovery' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match 'New-XHarnessRunOutputDirectory -OutputDirectory \$OutputDirectory'
        $content | Should -Match '"-o", \$testOutputDirectory'
        $content | Should -Match 'results-file-name=\$xharnessResultFileName'
        $content | Should -Match '"xunit-test-\*\.xml"'
        $content | Should -Match '(?s)Get-XHarnessTestResultSnapshot\s+`\s*-OutputDirectory \$testOutputDirectory\s+`\s*-ResultFileName \$xharnessResultFileName'
        $content | Should -Match '(?s)Get-FreshXHarnessTestResultFiles\s+`\s*-OutputDirectory \$testOutputDirectory\s+`\s*-BeforeSnapshot \$xharnessResultSnapshot\s+`\s*-ResultFileName \$xharnessResultFileName'
    }

    It 'retains bounded XHarness diagnostics when Android produces no fresh result XML' {
        $runRoot = Join-Path $TestDrive 'xharness-no-result'
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        'install failed' | Set-Content (Join-Path $runRoot 'install.log') -Encoding UTF8

        $diagnostic = New-XHarnessNoResultDiagnostic `
            -OutputDirectory $runRoot `
            -ExpectedResultFileName 'testResults-abc.xml' `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.ButtonTests' `
            -RawExitCode 78 `
            -ConsoleLogPath (Join-Path $runRoot 'xharness-console.log') `
            -ConsoleText "raw xharness console`n##vso[task.setvariable variable=X]blocked"

        $diagnostic | Should -Match "XHarness did not produce the expected fresh result 'testResults-abc.xml'"
        $diagnostic | Should -Match 'no authoritative target-test result was produced'
        $diagnostic | Should -Match 'Raw XHarness exit code: 78'
        $diagnostic | Should -Match 'install\.log'
        $diagnostic | Should -Match 'raw xharness console'
        $diagnostic | Should -Not -Match '##vso\[task\.setvariable'

        $content = Get-Content $scriptPath -Raw
        $content | Should -Match 'xharness-console\.log'
        $content | Should -Match 'xharness-no-result-diagnostics\.txt'
        $content | Should -Match 'throw \$noResultDiagnostic'
    }

    It 'streams bounded XHarness output to retained logs while preserving exit code and tail' {
        $runRoot = Join-Path $TestDrive 'xharness-stream'
        $logPath = Join-Path $runRoot 'xharness-console.log'
        $pwsh = Join-Path $PSHOME $(if ($IsWindows) { 'pwsh.exe' } else { 'pwsh' })
        $command = @'
[Console]::Out.WriteLine("first stdout line")
[Console]::Error.WriteLine("##vso[task.setvariable variable=LEAK]blocked")
[Console]::Out.WriteLine("z" * 20000)
for ($i = 0; $i -lt 40; $i++) {
    [Console]::Out.WriteLine("line-$i " + ("x" * 80))
}
exit 17
'@

        $result = Invoke-StreamingXHarnessCommand `
            -FilePath $pwsh `
            -Arguments ([string[]]@('-NoLogo', '-NoProfile', '-NonInteractive', '-Command', $command)) `
            -LogPath $logPath `
            -MaximumLogLength 900 `
            -MaximumTailLines 4

        $result.ExitCode | Should -Be 17
        Test-Path -LiteralPath $logPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $result.TailLogPath -PathType Leaf | Should -BeTrue
        $log = Get-Content -LiteralPath $logPath -Raw
        $tail = Get-Content -LiteralPath $result.TailLogPath -Raw
        $log.Length | Should -BeLessOrEqual 900
        $log | Should -Match 'first stdout line'
        $log | Should -Match 'xharness console log truncated'
        $log | Should -Not -Match '##vso\[task\.setvariable'
        $tail | Should -Match 'line-39'
        $result.TailText | Should -Match 'line-39'
    }

    It 'offers Android XHarness preflight without building or running product tests' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\[switch\]\$PreflightXHarnessOnly'
        $content | Should -Match '(?s)if \(\$PreflightXHarnessOnly\).*?Invoke-XHarnessPreflight.*?exit 0.*?# ═+'
        $content | Should -Match 'Invoke-StreamingXHarnessCommand'
        $content | Should -Match "'android', 'test', '--help'"
        $content | Should -Match 'adb -s \$DeviceUdid get-state'
        $content | Should -Match 'adb -s \$DeviceUdid shell getprop sys\.boot_completed'
    }

    It 'accepts HELP_SHOWN only for explicit XHarness help probes' {
        foreach ($exitCode in @(0, 2)) {
            Test-XHarnessHelpExitCode -ExitCode $exitCode | Should -BeTrue
        }
        foreach ($exitCode in @($null, '0', '2', $false, -1, 1, 3, 70, 71, 78, 91, 94)) {
            Test-XHarnessHelpExitCode -ExitCode $exitCode | Should -BeFalse
        }
        $content = Get-Content $scriptPath -Raw
        [regex]::Matches($content, 'Test-XHarnessHelpExitCode -ExitCode').Count | Should -Be 3
        $buildPhase = $content.IndexOf('# BUILD PHASE')
        $buildPhase | Should -BeGreaterThan 0
        $content.Substring($buildPhase) | Should -Not -Match 'Test-XHarnessHelpExitCode'
        $content | Should -Match 'throw \$noResultDiagnostic'
    }

    It 'retains HELP_SHOWN readback and rejects actual Android help failures' {
        $script:helpProbeExitCode = 2
        function Invoke-StreamingXHarnessCommand {
            param($FilePath, $Arguments, $LogPath, $MaximumLogLength, $MaximumTailLines)
            $FilePath | Should -Be 'dotnet'
            ($Arguments -join '|') | Should -Be 'xharness|android|test|--help'
            return [pscustomobject]@{
                ExitCode = $script:helpProbeExitCode
                LogPath = $LogPath
                TailLogPath = "$LogPath.tail"
                TailText = "XHarness exit code: $script:helpProbeExitCode"
            }
        }
        $output = Join-Path $TestDrive 'help-shown'
        Invoke-XHarnessPreflight -UseLocalXHarness $true -Platform android -OutputDirectory $output
        $log = Join-Path $output 'xharness-preflight/xharness-preflight.log'
        Get-Content -LiteralPath $log -Raw | Should -Match 'android test --help exit code: 2'

        $script:helpProbeExitCode = 3
        { Invoke-XHarnessPreflight -UseLocalXHarness $true -Platform android -OutputDirectory $output } |
            Should -Throw '*could not invoke*exit 3*'
        Get-Content -LiteralPath $log -Raw | Should -Match 'android test --help exit code: 3'
    }
}

Describe 'Windows device test category filtering' {
    It 'extracts Category filters from VSTest-style expressions' {
        Get-CategoryFiltersFromTestFilter -Filter 'Category=Window|Category=Button' |
            Should -Be @('Window', 'Button')
    }

    It 'selects matching discovered categories case-insensitively' {
        Select-WindowsDeviceTestCategories `
            -AllCategories @('Button', 'Window', 'Shell') `
            -Filter 'Category=window' |
            Should -Be @('Window')
    }

    It 'returns all categories when no category filter is supplied' {
        Select-WindowsDeviceTestCategories `
            -AllCategories @('Button', 'Window') `
            -Filter '' |
            Should -Be @('Button', 'Window')
    }

    It 'requires the exact issue category in the AppContainer lane' {
        @(Select-WindowsDeviceTestCategories `
            -AllCategories @('safe_Issue37540', 'Button') `
            -Filter 'Issue37540' `
            -RequireExact) | Should -BeNullOrEmpty

        Select-WindowsDeviceTestCategories `
            -AllCategories @('Issue37540', 'safe_Issue37540') `
            -Filter 'Issue37540' `
            -RequireExact |
            Should -Be @('Issue37540')
    }

    It 'validates discovered category values before host path construction' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\^\[A-Za-z0-9_\.\+ -\]\{1,128\}\$'
        $content | Should -Match '-RequireExact:\$RequireAppContainer'
        $content | Should -Match 'result path escapes the trusted output root'
        $content | Should -Match 'result root contains a reparse point'
        $content | Should -Match 'result path is a reparse point'
    }

    It 'always requires category discovery for Controls' {
        Test-WindowsDeviceTestCategoryDiscovery `
            -Project 'Controls' `
            -TestFilter '' `
            -IncludeClasses 'Microsoft.Maui.Controls.DeviceTests.ButtonTests' |
            Should -BeTrue
    }

    It 'attempts category discovery for a filtered non-Controls project without class metadata' {
        Test-WindowsDeviceTestCategoryDiscovery `
            -Project 'Core' `
            -TestFilter 'Category=Window' `
            -IncludeClasses '' |
            Should -BeTrue
    }

    It 'uses the class-filtered normal runner for a non-Controls Gate test' {
        Test-WindowsDeviceTestCategoryDiscovery `
            -Project 'Core' `
            -TestFilter 'Category=Window' `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.WindowHandlerTests' |
            Should -BeFalse
    }

    It 'uses the full-suite runner for an unfiltered non-Controls project' {
        Test-WindowsDeviceTestCategoryDiscovery `
            -Project 'Core' `
            -TestFilter '' `
            -IncludeClasses '' |
            Should -BeFalse
    }

    It 'passes the exact class filter and app working directory to the Windows child process' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-process-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $script = Join-Path $tempRoot 'capture.sh'
            $output = Join-Path $tempRoot 'process-output.txt'
            @'
#!/bin/sh
printf '%s\n%s\n%s\n' "$PWD" "$NUNIT_SKIPPED_CLASSES" "$2" > "$1"
'@ | Set-Content -LiteralPath $script -Encoding utf8 -NoNewline
            & chmod +x $script

            $classFilter = 'Microsoft.Maui.DeviceTests.WindowHandlerTests'
            $process = Start-WindowsDeviceTestProcess `
                -AppPath $script `
                -ArgumentList @($output, 'argument with spaces') `
                -IncludeClasses $classFilter
            $process.WaitForExit()

            $process.ExitCode | Should -Be 0
            $lines = @(Get-Content -LiteralPath $output)
            [System.IO.Path]::GetFileName($lines[0]) |
                Should -Be ([System.IO.Path]::GetFileName($tempRoot))
            $lines[1] | Should -Be $classFilter
            $lines[2] | Should -Be 'argument with spaces'
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'resolves a relative result directory before launching from the app directory' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-results-$([guid]::NewGuid())"
        $appDirectory = Join-Path $tempRoot 'app'
        $invocationDirectory = Join-Path $tempRoot 'invocation'
        New-Item -ItemType Directory -Path $appDirectory, $invocationDirectory -Force | Out-Null

        try {
            $app = Join-Path $appDirectory 'device-tests.sh'
            @'
#!/bin/sh
cat > "$1" <<'EOF'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test type="Microsoft.Maui.DeviceTests.WindowHandlerTests" method="Runs" name="Microsoft.Maui.DeviceTests.WindowHandlerTests.Runs" result="Pass" />
    </collection>
  </assembly>
</assemblies>
EOF
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Core = 'com.microsoft.maui.core.devicetests'
            }
            Push-Location $invocationDirectory
            try {
                $exitCode = Invoke-WindowsDeviceTestApp `
                    -AppPath $app `
                    -Project 'Core' `
                    -AppName 'Core.DeviceTests' `
                    -OutputDirectory 'relative-results' `
                    -TestFilter 'Category=Window' `
                    -IncludeClasses 'Microsoft.Maui.DeviceTests.WindowHandlerTests' `
                    -IncludeMethods 'Runs' `
                    -Timeout '00:00:10'
            } finally {
                Pop-Location
            }

            $expectedResult = Join-Path $invocationDirectory 'relative-results/TestResults-com_microsoft_maui_core_devicetests.xml'
            Test-Path -LiteralPath $expectedResult | Should -BeTrue
            $exitCode | Should -Be 0
            $script:WindowsDeviceTestSummary.Total | Should -Be 1
            $script:WindowsDeviceTestSummary.Passed | Should -Be 1
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'bounds an exact-class run and identifies the requested target in the timeout' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-timeout-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $app = Join-Path $tempRoot 'device-tests.sh'
            @'
#!/bin/sh
exec sleep 30
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Core = 'com.microsoft.maui.core.devicetests'
            }

            {
                Invoke-WindowsDeviceTestApp `
                    -AppPath $app `
                    -Project 'Core' `
                    -AppName 'Core.DeviceTests' `
                    -OutputDirectory (Join-Path $tempRoot 'results') `
                    -TestFilter 'Category=Window' `
                    -IncludeClasses 'Microsoft.Maui.DeviceTests.WindowHandlerTests' `
                    -IncludeMethods 'TargetMethod' `
                    -Timeout '00:00:01'
            } | Should -Throw -ExpectedMessage '*WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:*within 1s*WindowHandlerTests*TargetMethod*'
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'bounds a scoped Controls category run and emits the trusted target-timeout marker' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-controls-timeout-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $app = Join-Path $tempRoot 'device-tests.sh'
            @'
#!/bin/sh
if [ "$2" = "-1" ]; then
    printf '%s\n' 'Window' > "$(dirname "$1")/devicetestcategories.txt"
    exit 0
fi
exec sleep 30
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Controls = 'com.microsoft.maui.controls.devicetests'
            }

            {
                Invoke-WindowsDeviceTestApp `
                    -AppPath $app `
                    -Project 'Controls' `
                    -AppName 'Controls.DeviceTests' `
                    -OutputDirectory (Join-Path $tempRoot 'results') `
                    -TestFilter 'Category=Window' `
                    -IncludeClasses 'Microsoft.Maui.Controls.DeviceTests.ButtonTests' `
                    -IncludeMethods 'TargetMethod' `
                    -Timeout '00:00:01'
            } | Should -Throw -ExpectedMessage '*WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:*within 1s*ButtonTests*TargetMethod*'
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'waits for a Controls category process to finish writing its result XML' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-controls-result-flush-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $app = Join-Path $tempRoot 'device-tests.sh'
            @'
#!/bin/sh
if [ "$2" = "-1" ]; then
    printf '%s\n' 'Window' > "$(dirname "$1")/devicetestcategories.txt"
    exit 0
fi
result="${1%.xml}_Window.xml"
: > "$result"
sleep 4
cat > "$result" <<'EOF'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test type="Microsoft.Maui.Controls.DeviceTests.ButtonTests" method="TargetMethod" name="TargetMethod" result="Pass" />
    </collection>
  </assembly>
</assemblies>
EOF
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Controls = 'com.microsoft.maui.controls.devicetests'
            }

            $exitCode = Invoke-WindowsDeviceTestApp `
                -AppPath $app `
                -Project 'Controls' `
                -AppName 'Controls.DeviceTests' `
                -OutputDirectory (Join-Path $tempRoot 'results') `
                -TestFilter 'Category=Window' `
                -IncludeClasses 'Microsoft.Maui.Controls.DeviceTests.ButtonTests' `
                -IncludeMethods 'TargetMethod' `
                -Timeout '00:00:10'

            $exitCode | Should -Be 0
            $script:WindowsDeviceTestSummary.Total | Should -Be 1
            $script:WindowsDeviceTestSummary.Passed | Should -Be 1
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'trusts complete scoped category XML when only Windows process teardown times out' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-controls-teardown-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $app = Join-Path $tempRoot 'device-tests.sh'
            @'
#!/bin/sh
if [ "$2" = "-1" ]; then
    printf '%s\n' 'Window' > "$(dirname "$1")/devicetestcategories.txt"
    exit 0
fi
result="${1%.xml}_Window.xml"
cat > "$result" <<'EOF'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test type="Microsoft.Maui.Controls.DeviceTests.ButtonTests" method="TargetMethod" name="TargetMethod" result="Pass" />
    </collection>
  </assembly>
</assemblies>
EOF
exec sleep 30
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Controls = 'com.microsoft.maui.controls.devicetests'
            }

            $exitCode = Invoke-WindowsDeviceTestApp `
                -AppPath $app `
                -Project 'Controls' `
                -AppName 'Controls.DeviceTests' `
                -OutputDirectory (Join-Path $tempRoot 'results') `
                -TestFilter 'Category=Window' `
                -IncludeClasses 'Microsoft.Maui.Controls.DeviceTests.ButtonTests' `
                -IncludeMethods 'TargetMethod' `
                -Timeout '00:00:01'

            $exitCode | Should -Be 0
            $script:WindowsDeviceTestSummary.Passed | Should -Be 1
            $script:WindowsDeviceTestSummary.Failed | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'trusts complete scoped XML when only Windows process teardown times out' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-teardown-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

        try {
            $app = Join-Path $tempRoot 'device-tests.sh'
            @'
#!/bin/sh
cat > "$1" <<'EOF'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test type="Microsoft.Maui.DeviceTests.WindowHandlerTests" method="TargetMethod" name="TargetMethod" result="Pass" />
    </collection>
  </assembly>
</assemblies>
EOF
exec sleep 30
'@ | Set-Content -LiteralPath $app -Encoding utf8 -NoNewline
            & chmod +x $app

            $script:WindowsDeviceTestPackageIds = @{
                Core = 'com.microsoft.maui.core.devicetests'
            }

            $exitCode = Invoke-WindowsDeviceTestApp `
                -AppPath $app `
                -Project 'Core' `
                -AppName 'Core.DeviceTests' `
                -OutputDirectory (Join-Path $tempRoot 'results') `
                -TestFilter 'Category=Window' `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.WindowHandlerTests' `
                -IncludeMethods 'TargetMethod' `
                -Timeout '00:00:01'

            $exitCode | Should -Be 0
            $script:WindowsDeviceTestSummary.Passed | Should -Be 1
            $script:WindowsDeviceTestSummary.Failed | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Get-DeviceTestResultSummary' {
    It 'clamps negative result counts to zero' {
        ConvertTo-DeviceTestCount -Value '-1' | Should -Be 0
    }

    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-results-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'aggregates xUnit assembly counters from Windows device-test XML files' {
        $file1 = Join-Path $script:testDir 'TestResults-One.xml'
        $file2 = Join-Path $script:testDir 'TestResults-Two.xml'

        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0" />
</assemblies>
'@ | Set-Content $file1 -Encoding UTF8

        @'
<assemblies>
  <assembly total="2" passed="1" failed="0" skipped="1" errors="0" />
</assemblies>
'@ | Set-Content $file2 -Encoding UTF8

        $summary = Get-DeviceTestResultSummary -ResultFiles @($file1, $file2)

        $summary.Total | Should -Be 5
        $summary.Passed | Should -Be 3
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
        $summary.Errors | Should -Be 0
    }

    It 'throws a descriptive error (not a null-ref) when a result file is empty' {
        $emptyFile = Join-Path $script:testDir 'TestResults-Empty.xml'
        New-Item -ItemType File -Path $emptyFile -Force | Out-Null

        { Get-DeviceTestResultSummary -ResultFiles @($emptyFile) } |
            Should -Throw -ExpectedMessage 'WINDOWS_DEVICE_TEST_NO_RESULTS:*empty or not valid XML*'
    }

    It 'throws a descriptive error (not a null-ref) when a result file is malformed' {
        $badFile = Join-Path $script:testDir 'TestResults-Bad.xml'
        '<assemblies><assembly total="1"' | Set-Content $badFile -Encoding UTF8

        { Get-DeviceTestResultSummary -ResultFiles @($badFile) } |
            Should -Throw -ExpectedMessage 'WINDOWS_DEVICE_TEST_NO_RESULTS:*empty or not valid XML*'
    }

    It 'counts only tests of the requested class when -IncludeClasses is set (matches on the xUnit type attribute)' {
        $file = Join-Path $script:testDir 'TestResults-Suite.xml'

        # Real xUnit v2 shape: the fully-qualified class is in `type`; `name` is the
        # (often theory/DisplayName) label, NOT the FQN.
        @'
<assemblies>
  <assembly total="5" passed="3" failed="1" skipped="1" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="OneB" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneB" result="Fail" />
      <test name="OneC" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneC" result="Skip" />
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
      <test name="TwoB" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoB" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
    }

    It 'matches the class even when the test name is a theory/DisplayName string (regression: false INCONCLUSIVE #36577)' {
        $file = Join-Path $script:testDir 'TestResults-Theory.xml'

        # These `name` values never start with the FQN — the original name-based matcher
        # counted 0 here and forced a false INCONCLUSIVE even though the tests ran.
        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0">
    <collection>
      <test name="PlatformView Transforms are not empty(size: 1)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Transforms" result="Pass" />
      <test name="CompletedFiresOnRealEnterKeyPress" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Updating Font Does Not Affect Alignment(initialSize: 10, newSize: 20)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Font" result="Fail" />
      <test name="Unrelated" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="Unrelated" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 1
    }

    It 'does not treat a class name as a prefix substring of another class' {
        $file = Join-Path $script:testDir 'TestResults-Prefix.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="OneB" type="Microsoft.Maui.DeviceTests.EntryHandlerTestsExtra" method="OneB" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 0
    }

    It 'falls back to the fully-qualified name when a runner omits the type attribute' {
        $file = Join-Path $script:testDir 'TestResults-NoType.xml'

        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoA" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
    }

    It 'supports multiple comma/semicolon-separated classes in -IncludeClasses' {
        $file = Join-Path $script:testDir 'TestResults-Multi.xml'

        @'
<assemblies>
  <assembly total="3" passed="3" failed="0" skipped="0" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
      <test name="ThreeA" type="Microsoft.Maui.DeviceTests.ButtonHandlerTests" method="ThreeA" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests;Microsoft.Maui.DeviceTests.LabelHandlerTests'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 2
    }

    It 'rejects a broad XHarness suite when class isolation was required' {
        $file = Join-Path $script:testDir 'TestResults-Unfiltered.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Target" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Target" result="Pass" />
      <test name="Unrelated" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="Unrelated" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -RequireClassIsolation } |
            Should -Throw -ExpectedMessage '*class filter was not enforced*1 test(s) outside*LabelHandlerTests*'
    }

    It 'requires an exact xUnit type match when validating class isolation' {
        $file = Join-Path $script:testDir 'TestResults-ClassPrefix.xml'

        @'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Nested" type="Microsoft.Maui.DeviceTests.EntryHandlerTests.Nested" method="Nested" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -RequireClassIsolation } |
            Should -Throw -ExpectedMessage '*class filter was not enforced*'
    }

    It 'accepts an XHarness result containing only the requested classes' {
        $file = Join-Path $script:testDir 'TestResults-Isolated.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="One" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="One" result="Pass" />
      <test name="Two" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Two" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -RequireClassIsolation

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 2
    }

    It 'does not accept an all-skipped class-filtered run as verification evidence' {
        $file = Join-Path $script:testDir 'TestResults-Skipped.xml'

        @'
<assemblies>
  <assembly total="1" passed="0" failed="0" skipped="1" errors="0">
    <collection>
      <test name="Target" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Target" result="Skip" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -RequireClassIsolation } |
            Should -Throw -ExpectedMessage '*only skipped tests*did not execute*'
    }

    It 'throws (not a false pass) when the requested class produced no tests, with diagnostics naming the classes present' {
        $file = Join-Path $script:testDir 'TestResults-Missing.xml'

        @'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        # The throw must distinguish "target class absent" from "no results at all": it
        # reports the total tests found and a sample of the CLASSES present for diagnosis.
        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' } |
            Should -Throw -ExpectedMessage '*did not run*Total tests found in result file(s): 1*Sample classes present*LabelHandlerTests*'
    }

    It 'reports a zero total when the result file has no <test> nodes at all' {
        $file = Join-Path $script:testDir 'TestResults-NoTests.xml'

        @'
<assemblies>
  <assembly total="0" passed="0" failed="0" skipped="0" errors="0">
    <collection />
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' } |
            Should -Throw -ExpectedMessage '*Total tests found in result file(s): 0*'
    }

    # ─────────────────────────────────────────────────────────────────────────────
    # Method-level scoping: when the gate knows the PR's specific methods, the tally
    # counts ONLY those methods within the class — so a pre-existing/flaky failure in
    # an unrelated sibling method of the same class cannot falsely redden the verdict.
    # ─────────────────────────────────────────────────────────────────────────────

    It 'counts only the requested methods within the class when -IncludeMethods is set' {
        $file = Join-Path $script:testDir 'TestResults-Methods.xml'

        @'
<assemblies>
  <assembly total="4" passed="3" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Completed does not fire on IME candidate confirmation Enter" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedDoesNotFireOnIMECandidateEnter" result="Pass" />
      <test name="Unrelated sibling A" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SomeOtherEntryTest" result="Pass" />
      <test name="Unrelated sibling B" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="AnotherEntryTest" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress;CompletedDoesNotFireOnIMECandidateEnter'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 0
    }

    It 'excludes an unrelated sibling failure in the same class (regression: no false FAILED from method-scoping)' {
        $file = Join-Path $script:testDir 'TestResults-Sibling.xml'

        # The target method passes; a DIFFERENT method in the same class fails. Class-only
        # scoping would report Failed=1 -> false FAILED. Method-scoping must report PASSED.
        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Pre-existing flaky Windows test" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UnrelatedFlakyTest" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        # Sanity: class-only scoping DOES see the sibling failure (the false FAILED we fix).
        $classOnly = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'
        $classOnly.Failed | Should -Be 1

        # Method-scoping ignores the unrelated sibling -> clean PASSED.
        $scoped = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress'
        $scoped.Total | Should -Be 1
        $scoped.Passed | Should -Be 1
        $scoped.Failed | Should -Be 0
    }

    It 'preserves a GENUINE target-method failure under method-scoping (does not mask fix-incomplete)' {
        $file = Join-Path $script:testDir 'TestResults-Genuine.xml'

        # Mirrors build 14695686 (#36577): the PR added two methods; with the fix applied
        # one target method still fails. Method-scoping must STILL report that failure.
        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Completed does not fire on IME candidate confirmation Enter" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedDoesNotFireOnIMECandidateEnter" result="Fail" />
      <test name="Unrelated sibling" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SomeOtherEntryTest" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress;CompletedDoesNotFireOnIMECandidateEnter'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        # The failing test must be named (type.method) so the verdict is auditable.
        ($summary.FailedTests -join ';') | Should -BeLike '*EntryHandlerTests.CompletedDoesNotFireOnIMECandidateEnter*'
    }

    It 'defangs XML-derived failed-test identities before they reach the pipeline log' {
        $file = Join-Path $script:testDir 'TestResults-LoggingCommand.xml'

        @'
<assemblies>
  <assembly total="1" passed="0" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.Target&#xA;##vso[task.setvariable variable=GateFailed]false" method="Target" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Failed | Should -Be 1
        $summary.FailedTests | Should -HaveCount 1
        $summary.FailedTests[0] |
            Should -Be 'Microsoft.Maui.DeviceTests.EntryHandlerTests.Target ## vso[task.setvariable variable=GateFailed]false'
    }

    It 'counts every data-case of a target [Theory] method (same method attribute, different display names)' {
        $file = Join-Path $script:testDir 'TestResults-Theory-Method.xml'

        @'
<assemblies>
  <assembly total="4" passed="3" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Updating Font(initialSize: 10, newSize: 20)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Pass" />
      <test name="Updating Font(initialSize: 12, newSize: 24)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Pass" />
      <test name="Updating Font(initialSize: 14, newSize: 28)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Fail" />
      <test name="Unrelated" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Unrelated" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'UpdatingFont'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 1
    }

    It 'recovers the method from the FQN name when a runner omits the method attribute' {
        $file = Join-Path $script:testDir 'TestResults-Method-NoAttr.xml'

        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.CompletedFiresOnRealEnterKeyPress" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.UnrelatedFlakyTest" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 0
    }

    It 'throws a method-aware error when the class ran but none of the target methods did' {
        $file = Join-Path $script:testDir 'TestResults-Method-Missing.xml'

        # The class IS present (2 tests) but neither is a target method -> distinct from
        # "class absent"; the throw must name the methods, not just the class.
        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Sibling one" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SiblingOne" result="Pass" />
      <test name="Sibling two" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SiblingTwo" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -IncludeMethods 'CompletedFiresOnRealEnterKeyPress' } |
            Should -Throw -ExpectedMessage '*contained the class(es)*but none of the target method(s)*CompletedFiresOnRealEnterKeyPress*did not run*'
    }

    It 'rejects a partial method match instead of passing when one requested method never ran' {
        $file = Join-Path $script:testDir 'TestResults-PartialMethods.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Target A" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Unrelated sibling" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SomeOtherEntryTest" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -IncludeMethods 'CompletedFiresOnRealEnterKeyPress;CompletedDoesNotFireOnIMECandidateEnter' } |
            Should -Throw -ExpectedMessage '*did not contain every requested method*Missing: CompletedDoesNotFireOnIMECandidateEnter*'
    }

    It 'emits exact strict identities, skip state, failure signatures, and source digests' {
        $file = Join-Path $script:testDir 'TestResults-Strict.xml'
        @'
<assemblies>
  <assembly total="3" passed="1" failed="1" skipped="1" errors="0">
    <collection>
      <test name="plain" type="Microsoft.Maui.DeviceTests.LabelTests" method="Plain" result="Pass" />
      <test name="theory(value: &quot;&lt;b&gt;&quot;)" type="Microsoft.Maui.DeviceTests.LabelTests" method="Theory" result="Fail">
        <failure exception-type="Xunit.Sdk.EqualException"><message>Expected: Html
Actual: &lt;b&gt;Html&lt;/b&gt;</message><stack-trace>at Microsoft.Maui.DeviceTests.LabelTests.Theory() in /agent/_work/1/s/LabelTests.cs:line 42</stack-trace></failure>
      </test>
      <test name="known skip" type="Microsoft.Maui.DeviceTests.LabelTests" method="KnownSkip" result="Skip" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -RequireClassIsolation `
            -StrictEvidence `
            -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1))

        $summary.Records | Should -HaveCount 3
        $summary.Records[1].displayName | Should -Be 'theory(value: "<b>")'
        $summary.Records[1].failureSignature | Should -Match '^[0-9a-f]{64}$'
        $summary.Records[2].outcome | Should -Be 'Skip'
        $summary.ResultDigests | Should -HaveCount 1
        $summary.ResultDigests[0].sha256 | Should -Match '^[0-9a-f]{64}$'
    }

    It 'distinguishes identical failure messages from different assertion frames' {
        [xml]$first = @'
<test result="Fail"><failure exception-type="Xunit.Sdk.EqualException"><message>Expected: Html</message><stack-trace>
at Microsoft.Maui.DeviceTests.LabelTests.HtmlTextInitializesCorrectly() in /agent/a/LabelTests.Android.cs:line 42
at Xunit.Assert.Equal() in /agent/a/Assert.cs:line 10
</stack-trace></failure></test>
'@
        [xml]$second = @'
<test result="Fail"><failure exception-type="Xunit.Sdk.EqualException"><message>Expected: Html</message><stack-trace>
at Microsoft.Maui.DeviceTests.LabelTests.OtherAssertion() in /agent/b/LabelTests.Android.cs:line 42
at Xunit.Assert.Equal() in /agent/b/Assert.cs:line 10
</stack-trace></failure></test>
'@

        Get-ReplicationDeviceTestFailureSignature -Test $first.DocumentElement |
            Should -Not -BeExactly (
                Get-ReplicationDeviceTestFailureSignature -Test $second.DocumentElement)
    }

    It 'normalizes volatile failure paths and line numbers without losing frame identity' {
        [xml]$first = @'
<test result="Fail"><failure exception-type="Xunit.Sdk.EqualException"><message>Expected: Html</message><stack-trace>
at Microsoft.Maui.DeviceTests.LabelTests.HtmlTextInitializesCorrectly() in /agent/a/LabelTests.Android.cs:line 42
at Xunit.Assert.Equal() in /agent/a/Assert.cs:line 10
</stack-trace></failure></test>
'@
        [xml]$second = @'
<test result="Fail"><failure exception-type="Xunit.Sdk.EqualException"><message>Expected: Html</message><stack-trace>
at Microsoft.Maui.DeviceTests.LabelTests.HtmlTextInitializesCorrectly() in D:\work\b\LabelTests.Android.cs:line 987
at Xunit.Assert.Equal() in D:\work\b\Assert.cs:line 200
</stack-trace></failure></test>
'@

        Get-ReplicationDeviceTestFailureSignature -Test $first.DocumentElement |
            Should -BeExactly (
                Get-ReplicationDeviceTestFailureSignature -Test $second.DocumentElement)
    }

    It 'fails strict evidence closed for duplicate identities' {
        $file = Join-Path $script:testDir 'TestResults-Duplicate.xml'
        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="same" type="Microsoft.Maui.DeviceTests.LabelTests" method="Theory" result="Pass" />
      <test name="same" type="Microsoft.Maui.DeviceTests.LabelTests" method="Theory" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
                -RequireClassIsolation `
                -StrictEvidence `
                -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1)) } |
            Should -Throw -ExpectedMessage '*duplicate test identity*'
    }

    It 'prohibits DTD processing in strict evidence XML' {
        $file = Join-Path $script:testDir 'TestResults-Dtd.xml'
        @'
<!DOCTYPE assemblies [ <!ENTITY external SYSTEM "file:///etc/passwd"> ]>
<assemblies><assembly total="1" passed="1" failed="0" skipped="0" errors="0">
<collection><test name="&external;" type="Microsoft.Maui.DeviceTests.LabelTests" method="Target" result="Pass" /></collection>
</assembly></assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
                -RequireClassIsolation `
                -StrictEvidence `
                -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1)) } |
            Should -Throw
    }

    It 'rejects stale and incomplete strict evidence XML' {
        $file = Join-Path $script:testDir 'TestResults-Stale.xml'
        '<assemblies><assembly total="2" passed="1" failed="0" skipped="0" errors="0"><collection><test name="one" type="Microsoft.Maui.DeviceTests.LabelTests" method="One" result="Pass" /></collection></assembly></assemblies>' |
            Set-Content $file -Encoding UTF8

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
                -RequireClassIsolation `
                -StrictEvidence `
                -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(1)) } |
            Should -Throw -ExpectedMessage '*fresh bounded regular XML*'

        { Get-DeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
                -RequireClassIsolation `
                -StrictEvidence `
                -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1)) } |
            Should -Throw -ExpectedMessage '*incomplete or inconsistent assembly totals*'
    }

    It 'writes a closed strict document and retains the exact source XML' {
        $sourceDirectory = Join-Path $script:testDir 'strict-source'
        $outputDirectory = Join-Path $script:testDir 'strict-output'
        New-Item -ItemType Directory -Path $sourceDirectory, $outputDirectory -Force |
            Out-Null
        $file = Join-Path $sourceDirectory 'TestResults.xml'
        @'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test name="existing label behavior" type="Microsoft.Maui.DeviceTests.LabelTests" method="ExistingBehavior" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8
        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -RequireClassIsolation `
            -StrictEvidence `
            -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1))
        $script:OutputDirectory = $outputDirectory
        $evidencePath = Join-Path $outputDirectory 'strict-test-evidence.json'

        Write-DeviceTestStrictEvidence `
            -Path $evidencePath `
            -Summary $summary `
            -RunStartedUtc ([datetime]::UtcNow.AddMinutes(-1)) `
            -Project 'Controls' `
            -Platform 'android' `
            -TestFilter 'Category=Label' `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests'

        $evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
        @($evidence.PSObject.Properties.Name | Sort-Object -CaseSensitive) |
            Should -Be @(
                'completed', 'completedUtc', 'errors', 'failed', 'includeClass',
                'passed', 'platform', 'project', 'records', 'resultFiles',
                'runStartedUtc', 'schemaVersion', 'skipped', 'testFilter', 'total'
            )
        $retained = Join-Path $outputDirectory $evidence.resultFiles[0].name
        (Get-FileHash -LiteralPath $retained -Algorithm SHA256).Hash.ToLowerInvariant() |
            Should -Be $evidence.resultFiles[0].sha256
    }

    It 'removes early strict completion evidence when required cleanup fails' {
        $outputDirectory = Join-Path $script:testDir 'cleanup-failure'
        $sourceDirectory = Join-Path $script:testDir 'cleanup-failure-source'
        New-Item -ItemType Directory -Path $outputDirectory, $sourceDirectory -Force |
            Out-Null
        $script:OutputDirectory = $outputDirectory
        $evidencePath = Join-Path $outputDirectory 'strict-test-evidence.json'
        $sourcePath = Join-Path $sourceDirectory 'TestResults.xml'
        Set-Content -LiteralPath $sourcePath -Encoding utf8NoBOM -Value @'
<assemblies><assembly total="1" passed="1" failed="0" skipped="0" errors="0"><collection>
<test name="existing behavior" type="Microsoft.Maui.DeviceTests.LabelTests" method="ExistingBehavior" result="Pass" />
</collection></assembly></assemblies>
'@
        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($sourcePath) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -RequireClassIsolation `
            -StrictEvidence `
            -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1))
        Set-Content -LiteralPath $evidencePath -Value '{"completed":true}' -Encoding utf8NoBOM
        $errors = [Collections.Generic.List[string]]::new()
        $errors.Add('package cleanup failed: access denied')

        {
            Complete-DeviceTestStrictEvidence `
                -Path $evidencePath `
                -Summary $summary `
                -RunStartedUtc ([datetime]::UtcNow.AddMinutes(-1)) `
                -Project 'Controls' `
                -Platform 'windows' `
                -TestFilter 'Category=Label' `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
                -ExecutionCompleted $true `
                -CleanupErrors $errors
        } | Should -Throw -ExpectedMessage '*WINDOWS_DEVICE_TEST_CLEANUP_FAILED:*'
        Test-Path -LiteralPath $evidencePath | Should -BeFalse
    }

    It 'retains strict Windows XML after package LocalState cleanup' {
        $outputDirectory = Join-Path $script:testDir 'durable-output'
        $localStateDirectory = Join-Path $script:testDir 'package/LocalState'
        New-Item -ItemType Directory `
            -Path $outputDirectory, $localStateDirectory -Force |
            Out-Null
        $script:OutputDirectory = $outputDirectory
        $sourcePath = Join-Path $localStateDirectory 'TestResults-Label.xml'
        Set-Content -LiteralPath $sourcePath -Encoding utf8NoBOM -Value @'
<assemblies><assembly total="1" passed="1" failed="0" skipped="0" errors="0"><collection>
<test name="existing label behavior" type="Microsoft.Maui.DeviceTests.LabelTests" method="ExistingBehavior" result="Pass" />
</collection></assembly></assemblies>
'@
        $started = [datetime]::UtcNow.AddMinutes(-1)
        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($sourcePath) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -RequireClassIsolation `
            -StrictEvidence `
            -ResultNotBeforeUtc $started

        $durableFiles = @(
            Copy-DeviceTestStrictResultsToDurableDirectory `
                -Summary $summary `
                -OutputDirectory $outputDirectory `
                -ExpectedClass 'Microsoft.Maui.DeviceTests.LabelTests' `
                -ResultNotBeforeUtc $started
        )
        Remove-Item -LiteralPath (Split-Path -Parent $localStateDirectory) `
            -Recurse -Force

        Test-Path -LiteralPath $sourcePath | Should -BeFalse
        $durableFiles.Count | Should -Be 1
        Test-Path -LiteralPath $durableFiles[0] -PathType Leaf |
            Should -BeTrue
        $summary.ResultDigests[0].sourcePath |
            Should -BeExactly ([IO.Path]::GetFullPath($durableFiles[0]))

        $evidencePath = Join-Path $outputDirectory 'strict-test-evidence.json'
        Complete-DeviceTestStrictEvidence `
            -Path $evidencePath `
            -Summary $summary `
            -RunStartedUtc $started `
            -Project 'Controls' `
            -Platform 'windows' `
            -TestFilter 'Category=Label' `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -ExecutionCompleted $true `
            -CleanupErrors ([Collections.Generic.List[string]]::new())

        $evidence = Get-Content -LiteralPath $evidencePath -Raw |
            ConvertFrom-Json
        $retained = Join-Path $outputDirectory $evidence.resultFiles[0].name
        $reparsed = Read-ReplicationDeviceTestResultXmlStrict `
            -Path $retained `
            -NotBeforeUtc $started `
            -ExpectedClass 'Microsoft.Maui.DeviceTests.LabelTests'
        $reparsed.Total | Should -Be 1
        $reparsed.Records[0].type |
            Should -BeExactly 'Microsoft.Maui.DeviceTests.LabelTests'
    }

    It 'publishes completed strict evidence after an ordinary test failure and clean teardown' {
        $outputDirectory = Join-Path $script:testDir 'logical-failure'
        $sourceDirectory = Join-Path $script:testDir 'logical-failure-source'
        New-Item -ItemType Directory -Path $outputDirectory, $sourceDirectory -Force |
            Out-Null
        $script:OutputDirectory = $outputDirectory
        $sourcePath = Join-Path $sourceDirectory 'TestResults.xml'
        Set-Content -LiteralPath $sourcePath -Encoding utf8NoBOM -Value @'
<assemblies><assembly total="1" passed="0" failed="1" skipped="0" errors="0"><collection>
<test name="existing behavior" type="Microsoft.Maui.DeviceTests.LabelTests" method="ExistingBehavior" result="Fail">
<failure exception-type="Xunit.Sdk.EqualException"><message>Expected one</message><stack-trace>
at Microsoft.Maui.DeviceTests.LabelTests.ExistingBehavior() in /agent/LabelTests.cs:line 42
</stack-trace></failure></test></collection></assembly></assemblies>
'@
        $summary = Get-DeviceTestResultSummary `
            -ResultFiles @($sourcePath) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -RequireClassIsolation `
            -StrictEvidence `
            -ResultNotBeforeUtc ([datetime]::UtcNow.AddMinutes(-1))
        $evidencePath = Join-Path $outputDirectory 'strict-test-evidence.json'
        $errors = [Collections.Generic.List[string]]::new()

        Complete-DeviceTestStrictEvidence `
            -Path $evidencePath `
            -Summary $summary `
            -RunStartedUtc ([datetime]::UtcNow.AddMinutes(-1)) `
            -Project 'Controls' `
            -Platform 'windows' `
            -TestFilter 'Category=Label' `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.LabelTests' `
            -ExecutionCompleted $true `
            -CleanupErrors $errors

        $evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
        $evidence.completed | Should -BeTrue
        $evidence.failed | Should -Be 1
        $evidence.records[0].outcome | Should -BeExactly 'Fail'
    }
}
