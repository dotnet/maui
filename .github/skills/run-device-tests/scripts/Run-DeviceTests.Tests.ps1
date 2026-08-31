#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Run-DeviceTests.ps1'
    $script:WindowsDeviceNoResultsMarker = 'WINDOWS_DEVICE_TEST_NO_RESULTS:'
    $script:WindowsDeviceTargetTimeoutMarker = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:'

    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'ConvertTo-AzdoSafeConsole',
        'Get-CategoryFiltersFromTestFilter',
        'ConvertTo-DeviceTestClassFilterValue',
        'New-AndroidDeviceTestClassFilterInjection',
        'Get-XHarnessTestResultSnapshot',
        'Get-FreshXHarnessTestResultFiles',
        'New-XHarnessRunOutputDirectory',
        'Select-WindowsDeviceTestCategories',
        'Test-WindowsDeviceTestCategoryDiscovery',
        'Start-WindowsDeviceTestProcess',
        'Wait-ForPath',
        'ConvertTo-DeviceTestCount',
        'Get-DeviceTestResultSummary',
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
            '(?s)-RequireClassIsolation:\(\s*-not \$RequireAppContainer -and\s*' +
            '-not \[string\]::IsNullOrWhiteSpace\(\$IncludeClasses\)\)')
        $content | Should -Not -Match '\$summaryClassFilter\s*=\s*if\s*\(-not\s+\$useCategoryFiltering\)'
    }

    It 'packages replication tests into the audited Controls AppContainer only' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '\[switch\]\$RequireWindowsAppContainer'
        $content | Should -Match (
            "Windows replication permits only the Controls device-test package")
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
            '& dotnet @windowsGraphBuildArgs',
            [StringComparison]::Ordinal)
        $packageBuild = $content.IndexOf(
            '& dotnet @buildArgs',
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
}
