#Requires -Modules Pester

Describe 'Direct native CI contracts' {
    BeforeAll {
        $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
        $stage = Get-Content (Join-Path $repoRoot 'eng/pipelines/arcade/stage-device-tests.yml') -Raw
        $steps = Get-Content (Join-Path $repoRoot 'eng/pipelines/common/device-tests-steps.yml') -Raw
        $deviceScripts = Join-Path $repoRoot '.github/skills/run-device-tests/scripts'
        $start = Get-Content (Join-Path $deviceScripts 'Start-TestDevice.ps1') -Raw
        $stop = Get-Content (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -Raw
        $localRunner = Get-Content (Join-Path $deviceScripts 'Run-DeviceTests.ps1') -Raw
    }

    It 'retains all five executed native suites' {
        @([regex]::Matches($stage, '(?m)^  - name: (controls|core|graphics|essentials|blazorwebview)$')).Count |
            Should -Be 5
    }

    It 'retains both mobile runtimes and CoreCLR-only Catalyst' {
        $stage | Should -Match '(?s)name: coreclr\s+useCoreClr: true'
        $stage | Should -Match '(?s)name: mono\s+useCoreClr: false'
        $stage | Should -Match '(?s)platform: catalyst.*?useCoreClr: true'
        @([regex]::Matches($steps, [regex]::Escape('UseMonoRuntime=${{ ne(parameters.useCoreClr, true) }}'))).Count |
            Should -Be 3
        $steps | Should -Not -Match 'not\(parameters.useCoreClr\)'
    }

    It 'retains mutually exclusive iOS Controls shards and Windows packaging modes' {
        $stage | Should -Match 'CollectionView;Shell;HybridWebView;General'
        $stage | Should -Match 'SkipCategories=CollectionView,Shell,HybridWebView'
        $stage | Should -Match "windowsPackaging: both"
        $stage | Should -Match "iif\(eq\(parameters.windowsPackaging, 'both'\), 'packaged;unpackaged'"
        $steps | Should -Match '_MauiDeviceTestUnpackaged=true'
        $steps | Should -Match 'Prepare unique MSIX identity'
        $steps | Should -Match 'Install signed MSIX'
        $steps | Should -Match 'MauiTestPackaged=true'
        $steps | Should -Not -Match 'BLOCKED|Invoke-CommandInDesktopPackage'
        $steps | Should -Match "eq\(parameters.device, 'unpackaged'\)"
    }

    It 'collects real iOS container results before removing its owned simulator' {
        $steps | Should -Match 'simctl get_app_container \$state.Device'
        $steps | Should -Match 'Documents/TestResults'
        $steps | Should -Match 'testResults.trx'
        $steps.IndexOf('simctl get_app_container') | Should -BeLessThan $steps.IndexOf('Stop-TestDevice.ps1')
        $iosCommand = [regex]::Match($steps, '(?s)displayName: Run iOS tests directly.*?useExitCodeForErrors: true').Value
        $iosCommand | Should -Not -Match '--report-trx|--results-directory|MauiTestResultsDirectory'
        $iosCommand | Should -Match '-p:CodesignRequireProvisioningProfile=false'
        $catalystCommand = [regex]::Match($steps, '(?s)displayName: Run Mac Catalyst tests directly.*?useExitCodeForErrors: true').Value
        $catalystCommand | Should -Match '-p:CodesignRequireProvisioningProfile=false'
    }

    It 'does not swallow failing subprocess exits or absent results' {
        @([regex]::Matches($steps, 'continueOnError: false')).Count | Should -Be 6
        $steps | Should -Match 'Assert-TestResults.ps1'
        $steps | Should -Match 'failTaskOnMissingResultsFile: true'
        $steps | Should -Match 'failTaskOnFailedTests: true'
    }

    It 'retains synchronous MTP diagnostics when a Windows test process crashes before TRX finalization' {
        $run = [regex]::Match($steps, '(?s)displayName: Run Windows .*?useExitCodeForErrors: true').Value
        $run | Should -Match '--diagnostic-synchronous-write'
        $run | Should -Match ([regex]::Escape('--diagnostic-output-directory "$(TestResultsDirectory)/diagnostics"'))
    }

    It 'logs the original WinUI exception without marking a native crash handled' {
        $application = Get-Content (Join-Path $repoRoot 'src/TestUtils/src/DeviceTests.Runners/MauiTestApplication.cs') -Raw
        $application | Should -Match 'Application.Current.UnhandledException \+='
        $application | Should -Match ([regex]::Escape('Console.OpenStandardError()'))
        $application | Should -Match ([regex]::Escape('output.WriteLine(args.Exception)'))
        $application | Should -Not -Match 'args.Handled\s*='
    }

    It 'disconnects the Windows Frame layout tree even when its native assertions fail' {
        $fixture = Get-Content (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Elements/Frame/FrameTests.cs') -Raw
        $fixture | Should -Match '(?s)new ContentLayoutPanel\(layout,.*?\}, MauiContext\);\s*\}\s*finally\s*\{\s*layout\.DisconnectHandlers\(\);'
    }

    It 'detaches Apple fixture controllers without prematurely disposing UIKit trait environments' {
        $fixture = Get-Content (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/ControlsHandlerTestBase.iOS.cs') -Raw
        $fixture | Should -Match ([regex]::Escape('await whs.FinishedDisconnecting'))
        $fixture | Should -Match ([regex]::Escape('vc?.RemoveFromParentViewController()'))
        $fixture | Should -Not -Match ([regex]::Escape('window.Content?.DisconnectHandlers()'))
    }

    It 'creates Controls test windows only inside the dispatched native scope' {
        $fixture = Get-Content (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/ControlsHandlerTestBase.cs') -Raw
        [regex]::Matches($fixture, 'CreateWindowForContent\(view\)').Count | Should -Be 1
        $fixture | Should -Match '(?s)return InvokeOnMainThreadAsync\(async \(\) =>\s*\{\s*TestRunnerLogger\.LogDebug\([^;]+;\s*IWindow window = CreateWindowForContent\(view\);'
    }

    It 'awaits both native Frame color assertions before disposing the fixture' {
        $fixture = Get-Content (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Elements/Frame/FrameTests.cs') -Raw
        [regex]::Matches($fixture, 'await platformView\.AssertContainsColor\(expectedColor, \(\(IElementHandler\)handler\)\.MauiContext\)').Count | Should -Be 2
        $fixture | Should -Not -Match '(?m)^\s*platformView\.AssertContainsColor\('
    }

    It 'uses the Blazor project application identity for native iOS report extraction' {
        [xml] $project = Get-Content (Join-Path $repoRoot 'src/BlazorWebView/tests/DeviceTests/MauiBlazorWebView.DeviceTests.csproj') -Raw
        $applicationId = $project.SelectSingleNode('/Project/PropertyGroup/ApplicationId').InnerText
        $stage = Get-Content (Join-Path $repoRoot 'eng/pipelines/arcade/stage-device-tests.yml') -Raw
        $stage | Should -Match ([regex]::Escape("applicationId: $applicationId"))
        $manifest = Get-Content (Join-Path $repoRoot 'src/BlazorWebView/tests/DeviceTests/Platforms/iOS/Info.plist') -Raw
        $manifest | Should -Not -Match '<key>CFBundleIdentifier</key>'
    }

    It 'keeps Windows reference builds RID-specific so PRI paths match the app reference query' {
        $packaged = [regex]::Match($steps, '(?s)displayName: Build and sign owned MSIX.*?useExitCodeForErrors: true').Value
        $run = [regex]::Match($steps, '(?s)displayName: Run Windows .*?useExitCodeForErrors: true').Value
        $packaged | Should -Match '-p:IsRidAgnostic=false'
        $run | Should -Match '-p:IsRidAgnostic=false'
        $localRunner | Should -Match "'-p:IsRidAgnostic=false'"
    }

    It 'does not pass the app-only MSIX generation switch globally into referenced libraries' {
        $steps | Should -Not -Match '-p:GenerateAppxPackageOnBuild=true'
        $localRunner | Should -Not -Match '-p:GenerateAppxPackageOnBuild=true'
    }

    It 'uses addressable Android test assemblies without changing other platforms or Release optimization' -TestCases @(
        @{ Framework = 'net11.0-android'; Expected = 'false' }
        @{ Framework = 'net11.0-ios'; Expected = 'true' }
        @{ Framework = 'net11.0-windows10.0.19041.0'; Expected = 'true' }
    ) {
        param($Framework, $Expected)
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $directory | Out-Null
        $props = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'src/Microsoft.Maui.TestUtils.DeviceTests.Runners.props'))
        @"
<Project>
  <PropertyGroup>
    <EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
    <PublishReadyToRun>true</PublishReadyToRun>
    <Optimize>true</Optimize>
  </PropertyGroup>
  <Import Project="$props" />
  <Target Name="Capture">
    <WriteLinesToFile File="`$(ProbeDirectory)/settings.txt" Lines="`$(EmbedAssembliesIntoApk);`$(PublishReadyToRun);`$(Optimize)" Overwrite="true" />
  </Target>
</Project>
"@ | Set-Content (Join-Path $directory 'app.proj')
        $output = & dotnet msbuild (Join-Path $directory 'app.proj') -nologo -v:quiet -t:Capture `
            "-p:ProbeDirectory=$directory" "-p:TargetFramework=$Framework"
        if ($LASTEXITCODE -ne 0) { throw "Android assembly-location settings probe failed: $output" }
        $settings = @(Get-Content (Join-Path $directory 'settings.txt'))
        $settings[0] | Should -Be $Expected
        $settings[1] | Should -Be $Expected
        $settings[2] | Should -Be 'true'
    }

    It 'enables MSIX generation in the test app without making its reference library a package' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $directory | Out-Null
        $props = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'src/Microsoft.Maui.TestUtils.DeviceTests.Runners.props'))
        @'
<Project>
  <PropertyGroup><GenerateAppxPackageOnBuild>false</GenerateAppxPackageOnBuild></PropertyGroup>
  <Target Name="Capture">
    <WriteLinesToFile File="$(ProbeDirectory)/library.txt" Lines="$(GenerateAppxPackageOnBuild)" Overwrite="true" />
  </Target>
</Project>
'@ | Set-Content (Join-Path $directory 'library.proj')
        @"
<Project>
  <Import Project="$props" />
  <Target Name="Capture">
    <WriteLinesToFile File="`$(ProbeDirectory)/app.txt" Lines="`$(GenerateAppxPackageOnBuild)" Overwrite="true" />
    <MSBuild Projects="library.proj" Targets="Capture" />
  </Target>
</Project>
"@ | Set-Content (Join-Path $directory 'app.proj')
        $output = & dotnet msbuild (Join-Path $directory 'app.proj') -nologo -v:quiet -t:Capture `
            "-p:ProbeDirectory=$directory" -p:MauiTestPackaged=true -p:TargetFramework=net11.0-windows10.0.19041.0
        if ($LASTEXITCODE -ne 0) { throw "Windows app-property probe failed: $output" }
        (Get-Content (Join-Path $directory 'app.txt') -Raw).Trim() | Should -Be 'true'
        (Get-Content (Join-Path $directory 'library.txt') -Raw).Trim() | Should -Be 'false'
    }

    It 'creates a job-owned simulator and uses only its recorded UDID in cleanup' {
        $start | Should -Match 'simctl create'
        $start | Should -Match 'simctl bootstatus \$state.Device -b'
        $stop | Should -Match 'simctl delete \$state.Device'
        $stop | Should -Not -Match 'delete unavailable|shutdown all|booted'
        $stop | Should -Match 'Library/Logs/DiagnosticReports'
        $stop | Should -Match ([regex]::Escape('Select-String -LiteralPath $_.FullName -SimpleMatch $state.Device -Quiet'))
    }

    It 'copies only fresh crash reports containing the owned simulator identity' {
        $collection = [regex]::Match($stop, '(?s)    if \(Test-Path -LiteralPath \$hostReports\).*?(?=\r?\n    & xcrun)')
        $collection.Success | Should -BeTrue
        $ResultsDirectory = Join-Path $TestDrive 'simulator-results'
        $hostReports = Join-Path $TestDrive 'host-reports'
        New-Item -ItemType Directory -Path $ResultsDirectory, $hostReports | Out-Null
        $state = [pscustomobject]@{ Device = [guid]::NewGuid().ToString() }
        Set-Content -LiteralPath (Join-Path $hostReports 'owned.ips') -Value $state.Device
        Set-Content -LiteralPath (Join-Path $hostReports 'foreign.ips') -Value ([guid]::NewGuid().ToString())
        $stale = Join-Path $hostReports 'stale.ips'
        Set-Content -LiteralPath $stale -Value $state.Device
        [IO.File]::SetLastWriteTimeUtc($stale, [DateTime]::UtcNow.AddHours(-1))

        & ([scriptblock]::Create($collection.Value))

        @(Get-ChildItem -LiteralPath (Join-Path $ResultsDirectory 'crashes') -File).Name |
            Should -Be @('owned.ips')
    }

    It 'selects the exact owned Android serial and never stops processes by name' {
        $start | Should -Match 'emulator-\$port'
        $start | Should -Match 'sys.boot_completed'
        $steps | Should -Match '--device "\$\(TestDevice\)"'
        $stop | Should -Match '-s \$state.Device emu kill'
        $stop | Should -Not -Match 'killall|pkill|Stop-Process -Name'
    }

    It 'checks immutable Android boot identity without depending on emulator console output' {
        foreach ($script in @($start, $stop)) {
            $script | Should -Match 'shell getprop ro.boot.qemu.avd_name'
            $script | Should -Match 'shell getprop ro.kernel.qemu.avd_name'
            $script | Should -Not -Match 'emu avd name'
        }
    }

    It 'waits for the owned emulator before deleting its AVD directory' -TestCases @(
        @{ RequiresTermination = $false }
        @{ RequiresTermination = $true }
    ) {
        param($RequiresTermination)
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $avdHome = Join-Path $directory 'device/avd'
        New-Item -ItemType Directory -Force -Path $avdHome | Out-Null
        Set-Content -LiteralPath (Join-Path $avdHome 'hardware-qemu.ini.lock') -Value 'owned lock'
        $started = [DateTime]::UtcNow
        @{
            Platform = 'android'
            Device = 'emulator-5554'
            AvdName = 'owned-test-avd'
            AvdHome = $avdHome
            Adb = 'Invoke-TestAdb'
            ProcessId = 12345
            ProcessStartTime = $started.ToString('O')
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        function Invoke-TestAdb {
            $global:LASTEXITCODE = 0
            if ($args -contains 'getprop') { 'owned-test-avd' }
        }
        $process = [pscustomobject]@{
            ProcessName = 'qemu-system-x86_64'
            StartTime = $started
            WaitCount = 0
            RequiresTermination = $RequiresTermination
            AvdHome = $avdHome
        }
        $process | Add-Member -MemberType ScriptMethod -Name WaitForExit -Value {
            param($milliseconds)
            if (![IO.Directory]::Exists($this.AvdHome)) { throw 'AVD was deleted before the emulator exited.' }
            $this.WaitCount++
            return !$this.RequiresTermination -or $this.WaitCount -gt 1
        }
        Mock Get-Process { $process } -ParameterFilter { $Id -eq 12345 }
        Mock Stop-Process {}

        & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory

        $avdHome | Should -Not -Exist
        $process.WaitCount | Should -Be $(if ($RequiresTermination) { 2 } else { 1 })
        Should -Invoke Stop-Process -Times $(if ($RequiresTermination) { 1 } else { 0 }) -Exactly
    }

    It 'initializes the owned AVD directory without overwriting the read-only PowerShell home' {
        $setup = [regex]::Match($start, '(?s)    \$name = "maui-.*?\r?\n    Save-State')
        $setup.Success | Should -BeTrue
        $DeviceDirectory = Join-Path $TestDrive 'android-device'
        $statePath = Join-Path $TestDrive 'device.json'
        $state = @{}
        function Save-State {
            $state | ConvertTo-Json | Set-Content -LiteralPath $statePath
        }
        $originalHome = $HOME
        $originalAvdHome = $env:ANDROID_AVD_HOME
        try {
            $ErrorActionPreference = 'Stop'
            & ([scriptblock]::Create($setup.Value))
            $recorded = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
            $recorded.AvdHome | Should -Be (Join-Path $DeviceDirectory 'avd')
            $recorded.AvdHome | Should -Exist
            $env:ANDROID_AVD_HOME | Should -Be $recorded.AvdHome
            $recorded.AvdName | Should -Match '^maui-[a-f0-9]{32}$'
            $HOME | Should -Be $originalHome
        }
        finally {
            $env:ANDROID_AVD_HOME = $originalAvdHome
        }
    }

    It 'has no submission or monitor path' {
        $stage + $steps | Should -Not -Match 'send-to-helix|helix_xharness|HelixAccessToken|HelixJobMonitor|xharness'
    }

    It 'allows explicit Windows 11 client validation without mislabeling the configured server pool' {
        $android = [regex]::Match($stage, '(?s)platform: android.*?expectedArchitecture: X64').Value
        $android | Should -Not -Match 'requireWindows11'
        $stage | Should -Match 'requireWindows11: false'
        $stage | Should -Match ([regex]::Escape('requireWindows11: ${{ parameters.requireWindows11 }}'))
        $stage | Should -Not -Match 'displayName: Windows 11'
        $stage | Should -Match ([regex]::Escape('if eq(parameters.runWindowsTests, true)'))
        $start | Should -Match ([regex]::Escape('if (![Environment]::UserInteractive)'))
        $start | Should -Match ([regex]::Escape('if ($RequireWindows11)'))
        $start | Should -Match ([regex]::Escape('$os.ProductType -ne 1'))
    }

    It 'parses owned PowerShell files without errors' {
        foreach ($file in @('Start-TestDevice.ps1', 'Stop-TestDevice.ps1')) {
            $errors = $null
            [System.Management.Automation.Language.Parser]::ParseFile(
                (Join-Path $deviceScripts $file), [ref]$null, [ref]$errors) | Out-Null
            $errors | Should -BeNullOrEmpty
        }
    }
}
