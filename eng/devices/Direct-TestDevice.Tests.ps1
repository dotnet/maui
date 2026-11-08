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
        $startAst = [Management.Automation.Language.Parser]::ParseInput($start, [ref]$null, [ref]$null)
        $typeSelector = $startAst.Find({
            $args[0] -is [Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq 'Get-CompatibleIosDeviceType'
        }, $true)
        Invoke-Expression $typeSelector.Extent.Text
        $stopAst = [Management.Automation.Language.Parser]::ParseInput($stop, [ref]$null, [ref]$null)
        $ownershipCheck = $stopAst.Find({
            $args[0] -is [Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq 'Assert-OwnedIosSimulator'
        }, $true)
        Invoke-Expression $ownershipCheck.Extent.Text
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
        $run | Should -Match '--crashdump\s+--crashdump-type Mini'
        $runnerProject = Get-Content (Join-Path $repoRoot 'src/TestUtils/src/DeviceTests.Runners/TestUtils.DeviceTests.Runners.csproj') -Raw
        $runnerProject | Should -Match 'Include="Microsoft.Testing.Extensions.CrashDump" Version="\$\(MicrosoftTestingPlatformVersion\)" Condition="[^"]*windows'
        $runner = Get-Content (Join-Path $repoRoot 'src/TestUtils/src/DeviceTests.Runners/HeadlessRunner/HeadlessTestRunner.cs') -Raw
        $runner | Should -Match '(?s)#if WINDOWS\s+builder.AddCrashDumpProvider\(\);'
    }

    It 'logs the original WinUI exception without marking a native crash handled' {
        $application = Get-Content (Join-Path $repoRoot 'src/TestUtils/src/DeviceTests.Runners/MauiTestApplication.cs') -Raw
        $application | Should -Match 'Application.Current.UnhandledException \+='
        $application | Should -Match ([regex]::Escape('Console.OpenStandardError()'))
        $application | Should -Match ([regex]::Escape('output.WriteLine(args.Exception)'))
        $application | Should -Match ([regex]::Escape('File.AppendAllText(fatalLog, args.Exception'))
        $application | Should -Match ([regex]::Escape('AppDomain.CurrentDomain.UnhandledException +='))
        $application | Should -Not -Match 'args.Handled\s*='
    }


    It 'disconnects the Windows Frame layout tree even when its native assertions fail' {
        $fixture = Get-Content (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Elements/Frame/FrameTests.cs') -Raw
        $fixture | Should -Match '(?s)new ContentLayoutPanel\(layout,.*?\}, MauiContext\);\s*\}\s*finally\s*\{\s*layout\.DisconnectHandlers\(\);'
    }

    It 'retains and disposes the Windows attachment scope after disconnecting its handler' {
        $fixture = Get-Content -Raw (
            Join-Path $repoRoot 'src/Controls/tests/DeviceTests/ControlsHandlerTestBase.Windows.cs')
        $fixture.IndexOf('windowContext = window.Handler.MauiContext as MauiContext') |
            Should -BeLessThan $fixture.IndexOf('await runTests.Invoke()')
        $fixture | Should -Match '(?s)window\.Handler\?\.DisconnectHandler\(\).*?finally\s*\{\s*try\s*\{\s*windowContext\?\.DisposeWindowScope\(\);'
        $fixture | Should -Match '(?s)windowContext\?\.DisposeWindowScope\(\).*?finally\s*\{\s*appStub\.Handler\?\.DisconnectHandler\(\);'
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

    It 'selects an iPhone profile compatible with older and current installed runtimes' -TestCases @(
        @{ Runtime = '17.0'; Expected = 'iPhone-15' }
        @{ Runtime = '27.0'; Expected = 'iPhone-16' }
    ) {
        param($Runtime, $Expected)
        $types = @(
            [pscustomobject]@{ productFamily='iPhone'; identifier='com.apple.CoreSimulator.SimDeviceType.iPhone-16'; minRuntimeVersionString='18.0.0'; maxRuntimeVersionString='65535.255.255' },
            [pscustomobject]@{ productFamily='iPhone'; identifier='com.apple.CoreSimulator.SimDeviceType.iPhone-15'; minRuntimeVersionString='17.0.0'; maxRuntimeVersionString='65535.255.255' },
            [pscustomobject]@{ productFamily='iPad'; identifier='ignored'; minRuntimeVersionString='13.0.0'; maxRuntimeVersionString='65535.255.255' }
        )
        Get-CompatibleIosDeviceType -RuntimeVersion $Runtime -DeviceTypes $types |
            Should -Be "com.apple.CoreSimulator.SimDeviceType.$Expected"
    }

    It 'fails explicitly when no installed iPhone profile supports the requested runtime' {
        { Get-CompatibleIosDeviceType -RuntimeVersion '12.0' -DeviceTypes @() } |
            Should -Throw '*No installed iPhone device type supports*'
    }

    It 'rejects aggregate, malformed and foreign simulator state before mutation' -TestCases @(
        @{ Kind = 'aggregate' }
        @{ Kind = 'malformed' }
        @{ Kind = 'foreign' }
        @{ Kind = 'missing-name' }
    ) {
        param($Kind)
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $directory | Out-Null
        $id = [guid]::NewGuid().ToString('D')
        $name = "maui-$([guid]::NewGuid().ToString('N'))"
        $state = @{ Platform='ios'; Device=$id; SimulatorName=$name }
        switch ($Kind) {
            aggregate { $state.Device = 'all' }
            malformed { $state.Device = '../foreign-simulator' }
            missing-name { $state.Remove('SimulatorName') }
        }
        $state | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        $calls = [Collections.Generic.List[string]]::new()
        function xcrun {
            $calls.Add(($args -join ' '))
            $global:LASTEXITCODE = 0
            if (($args -join ' ') -eq 'simctl list devices --json') {
                return (@{ devices=@{ runtime=@(@{ udid=$id; name='pre-existing-user-simulator' }) } } |
                    ConvertTo-Json -Depth 5)
            }
            throw 'Unexpected simulator mutation.'
        }
        { & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory } |
            Should -Throw '*Refusing simulator cleanup*'
        @($calls | Where-Object { $_ -match 'shutdown|delete|spawn' }).Count | Should -Be 0
        $calls.Count | Should -Be $(if ($Kind -eq 'foreign') { 1 } else { 0 })
    }

    It 'requires a successful native ownership lookup even for a valid recorded UUID' -TestCases @(
        @{ LookupExit = 0 }
        @{ LookupExit = 1 }
    ) {
        param($LookupExit)
        $id = [guid]::NewGuid().ToString('D')
        $name = "maui-$([guid]::NewGuid().ToString('N'))"
        function xcrun {
            $global:LASTEXITCODE = $LookupExit
            @{ devices=@{ runtime=@(@{ udid=$id; name=$name }) } } | ConvertTo-Json -Depth 5
        }
        if ($LookupExit -eq 0) {
            { Assert-OwnedIosSimulator @{ Device=$id; SimulatorName=$name } } | Should -Not -Throw
        } else {
            { Assert-OwnedIosSimulator @{ Device=$id; SimulatorName=$name } } |
                Should -Throw '*Could not verify the recorded simulator ownership*'
        }
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

    It 'collects only fresh Windows application errors for the recorded native child PID' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $directory 'diagnostics') -Force | Out-Null
        '{"Platform":"windows"}' | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        "Started test host process. PID: '5656'. HasExited: 'False'." |
            Set-Content -LiteralPath (Join-Path $directory 'diagnostics/controller.diag')
        function Get-WinEvent {
            [CmdletBinding()]
            param([hashtable]$FilterHashtable)
            throw 'The event query must be mocked.'
        }
        function New-TestEvent($ProcessId, $DataProcessId, $Message, $TimeCreated = [DateTime]::UtcNow) {
            $record = [pscustomobject]@{
                Id = 1000; RecordId = 1; ProviderName = 'Application Error'
                ProcessId = $ProcessId; TimeCreated = $TimeCreated; Message = $Message
                Payload = "<Event><EventData><Data Name='ProcessId'>$DataProcessId</Data></EventData></Event>"
            }
            $record | Add-Member -MemberType ScriptMethod -Name ToXml -Value { return $this.Payload }
            return $record
        }
        $records = @(
            (New-TestEvent 5656 '' 'owned runtime exception')
            (New-TestEvent 2222 '0x1618' 'owned native fault')
            (New-TestEvent 3333 '0xD05' 'foreign fault')
            (New-TestEvent 5656 '' 'stale fault' ([DateTime]::UtcNow.AddHours(-1)))
        )
        Mock Get-WinEvent { $records }

        & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory

        $captured = Get-Content -Raw -LiteralPath (Join-Path $directory 'windows-application-events.json') |
            ConvertFrom-Json
        @($captured).Message | Should -Be @('owned runtime exception', 'owned native fault')
        Should -Invoke Get-WinEvent -Times 1 -Exactly -ParameterFilter {
            $FilterHashtable.LogName -eq 'Application' -and
            $FilterHashtable.Id.Count -eq 2 -and
            $FilterHashtable.StartTime -ge [DateTime]::UtcNow.AddMinutes(-1)
        }
    }

    It 'does not query Windows event logs without a recorded test child' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $directory | Out-Null
        '{"Platform":"windows"}' | Set-Content -LiteralPath (Join-Path $directory 'device.json')

        & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory

        (Join-Path $directory 'windows-application-events.json') | Should -Not -Exist
    }

    It 'reports Windows event-log access errors rather than treating them as no matching events' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $directory 'diagnostics') -Force | Out-Null
        '{"Platform":"windows"}' | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        "Started test host process. PID: '5656'." |
            Set-Content -LiteralPath (Join-Path $directory 'diagnostics/controller.diag')
        function Get-WinEvent {
            [CmdletBinding()]
            param([hashtable]$FilterHashtable)
            Write-Error -Message 'Windows application event log access denied.' -ErrorId EventLogAccessDenied
        }

        { & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory } |
            Should -Throw '*access denied*'
        (Join-Path $directory 'windows-application-events.json') | Should -Not -Exist
    }

    It 'records an explicit empty Windows event result when the provider finds no matching events' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $directory 'diagnostics') -Force | Out-Null
        '{"Platform":"windows"}' | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        "Started test host process. PID: '5656'." |
            Set-Content -LiteralPath (Join-Path $directory 'diagnostics/controller.diag')
        function Get-WinEvent {
            [CmdletBinding()]
            param([hashtable]$FilterHashtable)
            Write-Error -Message 'No events were found.' -ErrorId NoMatchingEventsFound
        }

        & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory

        (Get-Content -Raw -LiteralPath (Join-Path $directory 'windows-application-events.json')).Trim() |
            Should -Be '[]'
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
        $deviceDirectory = Join-Path $directory 'device'
        $avdName = "maui-$([guid]::NewGuid().ToString('N'))"
        $deviceRoot = Join-Path $deviceDirectory $avdName
        $avdHome = Join-Path $deviceRoot 'avd'
        $sdkDirectory = Join-Path $directory 'sdk'
        $adbPath = Join-Path $sdkDirectory 'platform-tools/adb'
        New-Item -ItemType Directory -Force -Path $avdHome | Out-Null
        Set-Content -LiteralPath (Join-Path $avdHome 'hardware-qemu.ini.lock') -Value 'owned lock'
        $started = [DateTime]::UtcNow
        @{
            Platform = 'android'
            Device = 'emulator-5554'
            AvdName = $avdName
            DeviceDirectory = $deviceDirectory
            DeviceRoot = $deviceRoot
            AvdHome = $avdHome
            Adb = $adbPath
            ProcessId = 12345
            ProcessStartTime = $started.ToString('O')
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        function Invoke-TestAdb {
            $global:LASTEXITCODE = 0
            if ($args -contains 'getprop') { $avdName }
        }
        Set-Alias -Name $adbPath -Value Invoke-TestAdb
        if ($IsWindows) {
            Mock Get-CimInstance { [pscustomobject]@{ CommandLine="qemu -avd $avdName" } }
        } else {
            function Invoke-TestProcessCommand { $global:LASTEXITCODE = 0; "qemu -avd $avdName" }
            Set-Alias -Name '/bin/ps' -Value Invoke-TestProcessCommand
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

        $originalSdk = $env:ANDROID_SDK_ROOT
        try {
            $env:ANDROID_SDK_ROOT = $sdkDirectory
            & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory
        }
        finally { $env:ANDROID_SDK_ROOT = $originalSdk }

        $avdHome | Should -Not -Exist
        $process.WaitCount | Should -Be $(if ($RequiresTermination) { 2 } else { 1 })
        Should -Invoke Stop-Process -Times $(if ($RequiresTermination) { 1 } else { 0 }) -Exactly
    }

    It 'rejects a matching-timestamp foreign emulator PID with or without a device serial' -TestCases @(
        @{ WithDevice = $false }
        @{ WithDevice = $true }
    ) {
        param($WithDevice)
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $avdName = "maui-$([guid]::NewGuid().ToString('N'))"
        $deviceDirectory = Join-Path $directory 'devices'
        $deviceRoot = Join-Path $deviceDirectory $avdName
        $avdHome = Join-Path $deviceRoot 'avd'
        New-Item -ItemType Directory -Force -Path $avdHome | Out-Null
        $time = [DateTime]::UtcNow
        $sdk = Join-Path $directory 'sdk'
        $adb = Join-Path $sdk 'platform-tools/adb'
        $state = @{
            Platform='android'; DeviceDirectory=$deviceDirectory; DeviceRoot=$deviceRoot
            AvdHome=$avdHome; AvdName=$avdName; ProcessId=12345; ProcessStartTime=$time.ToString('O'); Adb=$adb
        }
        if ($WithDevice) { $state.Device = 'emulator-5554' }
        $state | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        function Invoke-OwnedAdb {
            $global:LASTEXITCODE = 0
            if ($args -contains 'getprop') { $avdName }
        }
        Set-Alias -Name $adb -Value Invoke-OwnedAdb
        $process = [pscustomobject]@{ ProcessName='qemu-system-x86_64'; StartTime=$time }
        $process | Add-Member -MemberType ScriptMethod -Name WaitForExit -Value { throw 'Must not wait on a foreign process.' }
        Mock Get-Process { $process }
        Mock Stop-Process {}
        if ($IsWindows) {
            Mock Get-CimInstance { [pscustomobject]@{ CommandLine='qemu -avd another-user-avd' } }
        } else {
            function Invoke-ForeignProcessCommand { $global:LASTEXITCODE = 0; 'qemu -avd another-user-avd' }
            Set-Alias -Name '/bin/ps' -Value Invoke-ForeignProcessCommand
        }
        $originalSdk = $env:ANDROID_SDK_ROOT
        try {
            $env:ANDROID_SDK_ROOT = $sdk
            { & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory } |
                Should -Throw '*does not run the recorded job-owned AVD*'
        }
        finally { $env:ANDROID_SDK_ROOT = $originalSdk }
        Should -Invoke Stop-Process -Times 0 -Exactly
        $avdHome | Should -Exist
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
            $recorded.DeviceRoot | Should -Be (Join-Path $DeviceDirectory $recorded.AvdName)
            $recorded.AvdHome | Should -Be (Join-Path $recorded.DeviceRoot 'avd')
            $recorded.AvdHome | Should -Exist
            $env:ANDROID_AVD_HOME | Should -Be $recorded.AvdHome
            $recorded.AvdName | Should -Match '^maui-[a-f0-9]{32}$'
            $HOME | Should -Be $originalHome
        }
        finally {
            $env:ANDROID_AVD_HOME = $originalAvdHome
        }
    }

    It 'cleans recorded relative provisioning paths from another working directory without deleting foreign data' {
        $provisioning = Join-Path $TestDrive 'provisioning'
        $cleanup = Join-Path $TestDrive 'cleanup'
        $foreign = Join-Path $cleanup 'devices/avd/sentinel.txt'
        New-Item -ItemType Directory -Force -Path $provisioning, (Split-Path $foreign) | Out-Null
        Set-Content -LiteralPath $foreign -Value 'foreign'
        $normalization = [regex]::Match($start, '(?s)\$baseDirectory =.*?(?=New-Item -ItemType Directory -Force -Path \$ResultsDirectory)')
        $setup = [regex]::Match($start, '(?s)    \$name = "maui-.*?\r?\n    Save-State')
        $normalization.Success | Should -BeTrue
        $setup.Success | Should -BeTrue
        $originalAvdHome = $env:ANDROID_AVD_HOME
        Push-Location $provisioning
        try {
            $ResultsDirectory = 'results'
            $DeviceDirectory = 'devices'
            . ([scriptblock]::Create($normalization.Value))
            New-Item -ItemType Directory -Path $ResultsDirectory | Out-Null
            $statePath = Join-Path $ResultsDirectory 'device.json'
            $state = @{ Platform = 'android'; DeviceDirectory = $DeviceDirectory }
            function Save-State { $state | ConvertTo-Json | Set-Content -LiteralPath $statePath }
            & ([scriptblock]::Create($setup.Value))
            $recorded = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
            [IO.Path]::IsPathRooted($recorded.AvdHome) | Should -BeTrue
            $recorded.DeviceDirectory | Should -Be (Join-Path $provisioning 'devices')
        }
        finally {
            Pop-Location
            $env:ANDROID_AVD_HOME = $originalAvdHome
        }
        Push-Location $cleanup
        try {
            & pwsh -NoLogo -NoProfile -File (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $ResultsDirectory
            $LASTEXITCODE | Should -Be 0
        }
        finally { Pop-Location }
        $recorded.AvdHome | Should -Not -Exist
        Get-Content -LiteralPath $foreign | Should -Be 'foreign'
    }

    It 'rejects ambiguous or tampered AVD ownership paths' -TestCases @(
        @{ Kind = 'relative' }
        @{ Kind = 'outside' }
        @{ Kind = 'shared-root' }
        @{ Kind = 'foreign-name' }
        @{ Kind = 'symbolic-link' }
        @{ Kind = 'missing-home' }
        @{ Kind = 'foreign-adb' }
    ) {
        param($Kind)
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $avdName = "maui-$([guid]::NewGuid().ToString('N'))"
        $deviceDirectory = Join-Path $directory 'devices'
        $deviceRoot = Join-Path $deviceDirectory $avdName
        $avdHome = Join-Path $deviceRoot 'avd'
        $foreign = Join-Path $directory 'foreign/avd'
        New-Item -ItemType Directory -Force -Path $avdHome, $foreign | Out-Null
        $state = @{
            Platform = 'android'; AvdName = $avdName; DeviceDirectory = $deviceDirectory
            DeviceRoot = $deviceRoot; AvdHome = $avdHome
        }
        $marker = Join-Path $directory 'foreign-executable-ran.txt'
        $originalSdk = $env:ANDROID_SDK_ROOT
        switch ($Kind) {
            relative { $state.AvdHome = 'devices/avd' }
            outside { $state.AvdHome = $foreign }
            shared-root { $state.DeviceRoot = $deviceDirectory; $state.AvdHome = Join-Path $deviceDirectory 'avd' }
            foreign-name { $state.AvdName = 'pre-existing-user-device' }
            symbolic-link {
                Mock Get-Item { [pscustomobject]@{ Attributes = [IO.FileAttributes]::ReparsePoint } } `
                    -ParameterFilter { $LiteralPath -eq $avdHome }
            }
            missing-home {
                $state.Remove('AvdHome')
                $state.Device = 'emulator-5554'
                $state.AvdName = 'pre-existing-user-device'
                $state.Adb = 'InvokeForbiddenAdb'
                function InvokeForbiddenAdb { throw 'Unowned emulator commands must never run.' }
            }
            foreign-adb {
                $state.Device = 'emulator-5554'
                $state.Adb = Join-Path $directory 'foreign.ps1'
                "'executed' | Set-Content -LiteralPath '$marker'" | Set-Content -LiteralPath $state.Adb
            }
        }
        $state | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'device.json')
        try {
            $env:ANDROID_SDK_ROOT = Join-Path $directory 'sdk'
            $expectedError = if ($Kind -eq 'foreign-adb') { '*does not match the configured SDK*' } else { '*Refusing AVD cleanup*' }
            { & (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -ResultsDirectory $directory } |
                Should -Throw $expectedError
        }
        finally { $env:ANDROID_SDK_ROOT = $originalSdk }
        $avdHome | Should -Exist
        $foreign | Should -Exist
        $marker | Should -Not -Exist
    }

    It 'keeps snapshot baselines in the test binary directory with either base-directory spelling' {
        $source = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Controls/tests/TestCases.Shared.Tests/UITest.cs') -Raw
        $rootExpression = [regex]::Match($source, 'string projectRootDirectory = ([^;]+);')
        $rootExpression.Success | Should -BeTrue
        $expression = $rootExpression.Groups[1].Value.Replace('AppDomain.CurrentDomain.BaseDirectory', 'baseDirectory')
        Add-Type -TypeDefinition @"
using System.IO;
public static class SnapshotRootRegression
{
    public static string Get(string baseDirectory) => $expression;
}
"@
        $binaryDirectory = Join-Path $TestDrive 'Release/net11.0'
        foreach ($baseDirectory in @($binaryDirectory, "$binaryDirectory$([IO.Path]::DirectorySeparatorChar)")) {
            [SnapshotRootRegression]::Get($baseDirectory).TrimEnd([IO.Path]::DirectorySeparatorChar) |
                Should -Be $binaryDirectory
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

Describe 'HybridWebView startup polling budget' {
    BeforeAll {
        $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
        $source = Get-Content -Raw -LiteralPath (
            Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Elements/HybridWebView/HybridWebViewTestsBase.cs')
        $constants = [regex]::Matches($source,
            'const int (?:MaxWaitTimes|WaitTimeInMS|PageLoadWaitTimes) = \d+;').Value -join "`n"
        $retry = [regex]::Match($source,
            '(?s)private static async Task Retry\(.*?(?=\r?\n\t\tpublic static async Task WaitForHybridWebViewLoaded)').Value
        if (!('HybridReadinessBudgetRegression' -as [type])) {
            Add-Type -TypeDefinition @"
using System;
using System.Threading.Tasks;
public static class HybridReadinessBudgetRegression
{
    $constants
    $retry
    public static int Calls;
    public static Task Run(bool ready, int attempts)
    {
        Calls = 0;
        return Retry(() => { Calls++; return Task.FromResult(ready); },
            timeout => Task.FromResult<Exception>(new InvalidOperationException("timeout=" + timeout)), attempts);
    }
}
"@
        }
    }

    It 'keeps JavaScript-result polling at 25 seconds and cold page startup at 60 seconds' {
        $flags = [Reflection.BindingFlags]'Static,NonPublic'
        $type = [HybridReadinessBudgetRegression]
        $delay = $type.GetField('WaitTimeInMS', $flags).GetRawConstantValue()
        $type.GetMethod('Retry', $flags).GetParameters()[2].DefaultValue * $delay | Should -Be 25000
        $type.GetField('PageLoadWaitTimes', $flags).GetRawConstantValue() * $delay | Should -Be 60000
        $page = [regex]::Match($source,
            '(?s)public static async Task WaitForHybridWebViewLoaded.*?(?=public static async Task WaitForHtmlStatusSet)').Value
        $page | Should -Match 'maxWaitTimes: PageLoadWaitTimes'
        ($source -split 'public static async Task WaitForHtmlStatusSet', 2)[1] |
            Should -Not -Match 'maxWaitTimes:'
    }

    It 'stops polling as soon as the real readiness predicate succeeds' {
        [HybridReadinessBudgetRegression]::Run($true, 240).GetAwaiter().GetResult()
        [HybridReadinessBudgetRegression]::Calls | Should -Be 1
    }

    It 'fails after the selected finite polling budget without accepting an unready page' {
        { [HybridReadinessBudgetRegression]::Run($false, 1).GetAwaiter().GetResult() } |
            Should -Throw '*timeout=250*'
        [HybridReadinessBudgetRegression]::Calls | Should -Be 1
    }
}
