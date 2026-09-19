#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $ErrorActionPreference = 'Stop'
    $tokens = $null
    $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile(
        (Join-Path $PSScriptRoot 'Start-DevFlowSandbox.ps1'), [ref]$tokens, [ref]$errors)
    if ($errors.Count) { throw ($errors.Message -join [Environment]::NewLine) }
    foreach ($function in $ast.FindAll({
        $args[0] -is [Management.Automation.Language.FunctionDefinitionAst]
    }, $false)) {
        . ([scriptblock]::Create($function.Extent.Text))
    }
    $version = '0.1.0-preview.10.26274.3'
    $project = [IO.Path]::Combine($TestDrive, 'repo with spaces', 'Sandbox.csproj')
}

Describe 'SDK arguments' {
    BeforeEach {
        $parameters = @{
            ProjectPath = $project
            Platform = 'Android'
            DeviceId = 'emulator-5554'
            TfmVersion = '10.0'
            SessionId = 'df123'
            BinlogPath = [IO.Path]::Combine($TestDrive, 'logs with spaces', 'build.binlog')
        }
    }

    It 'keeps paths and the explicit Android serial in separate intact arguments' {
        $arguments = Get-DevFlowBuildArguments @parameters
        $arguments[1] | Should -BeExactly $project
        $arguments | Should -Contain '-p:AdbTarget=-s emulator-5554'
        $arguments | Should -Contain '-p:MauiSamplePlatforms=net10.0-android'
        $arguments | Should -Contain '-p:UseMaui=false'
        $arguments | Should -Contain '-p:EnableMauiDevFlow=true'
        $arguments | Should -Not -Contain '--no-restore'
    }

    It 'uses the repository TFM and simulator host architecture rather than a fixed SDK' {
        $parameters.Platform = 'iOS'
        $parameters.DeviceId = '20F5E8A1-901C-4C42-B8AA-E41A0641E2C5'
        $parameters.TfmVersion = '11.0'
        $arguments = Get-DevFlowBuildArguments @parameters -HostArchitecture X64
        $arguments | Should -Contain 'net11.0-ios'
        $arguments | Should -Contain 'iossimulator-x64'
        $arguments | Should -Contain '-p:IncludeAndroidTargetFrameworks=False'
    }

    It 'uses the already-restored build for the targeted Android launch' {
        $arguments = Get-DevFlowBuildArguments @parameters -Target Run
        $arguments | Should -Contain '-t:Run'
        $arguments | Should -Contain '--no-restore'
        $arguments | Should -Contain '-p:MauiDevFlowSessionId=df123'
    }

    It 'rejects Release, AVD names, and MSBuild argument injection' {
        { Get-DevFlowBuildArguments @parameters -Configuration Release } | Should -Throw '*Debug*'
        $parameters.DeviceId = 'Copilot_API_35'
        { Get-DevFlowBuildArguments @parameters } | Should -Throw '*serial*'
        $parameters.DeviceId = 'emulator-5554;UseMaui=true'
        { Get-DevFlowBuildArguments @parameters } | Should -Throw '*serial*'
    }

    It 'rejects an unspecified simulator or unsupported platform' {
        $parameters.Platform = 'iOS'
        { Get-DevFlowBuildArguments @parameters } | Should -Throw '*UDID*'
        $parameters.Platform = 'Windows'
        { Get-DevFlowBuildArguments @parameters } | Should -Throw '*Unsupported platform*'
    }
}

Describe 'Device and Agent selection' {
    It 'requires exactly one running virtual device with the requested identifier' {
        $device = @{ identifier = 'emulator-5554'; platform = 'android'; is_emulator = $true; is_running = $true }
        (Select-DevFlowDevice @($device) Android emulator-5554).identifier | Should -Be 'emulator-5554'
        { Select-DevFlowDevice @($device, $device) Android emulator-5554 } | Should -Throw '*exactly one*'
        $device.is_running = $false
        { Select-DevFlowDevice @($device) Android emulator-5554 } | Should -Throw '*already-running*'
        $device.is_running = $true
        $device.is_emulator = $false
        { Select-DevFlowDevice @($device) Android emulator-5554 } | Should -Throw '*emulator/simulator*'
    }

    BeforeEach {
        $agent = @{
            id = 'agent1'; project = $project; platform = 'Android'; sessionId = 'df123'
            tfm = 'net10.0-android'; port = 10224; version = "$version+commit"
        }
        $selection = @{
            ProjectPath = $project; Platform = 'Android'; SessionId = 'df123'
            TargetFramework = 'net10.0-android'; Version = $version
        }
    }

    It 'accepts the reported port only for this fresh build identity' {
        (Select-DevFlowAgent -Agents @($agent) @selection).port | Should -Be 10224
        $agent.sessionId = 'old'
        Select-DevFlowAgent -Agents @($agent) @selection | Should -BeNullOrEmpty
    }

    It 'rejects ambiguous connections rather than choosing the first' {
        { Select-DevFlowAgent -Agents @($agent, $agent) @selection } | Should -Throw '*Multiple Agents*'
    }

    It 'rejects invalid ports and incompatible Agent versions' {
        $agent.port = 0
        { Select-DevFlowAgent -Agents @($agent) @selection } | Should -Throw '*valid port*'
        $agent.port = 10224
        $agent.version = '0.1.0-preview.12'
        { Select-DevFlowAgent -Agents @($agent) @selection } | Should -Throw '*version*'
    }
}

Describe 'Structured output' {
    It 'handles the pinned SDK Info prefix only for device enumeration' {
        $path = Join-Path $TestDrive 'devices.json'
        "Info: Executing simulator discovery`nInfo: Found devices.`n[]" | Set-Content $path
        $value = Read-DevFlowJson -Path $path -DeviceOutput
        ($value -is [array]) | Should -BeTrue
        $value.Count | Should -Be 0
        { Read-DevFlowJson -Path $path } | Should -Throw '*Expected*JSON*'
    }

    It 'does not interpret preview10 timeout exit zero as readiness' {
        $path = Join-Path $TestDrive 'wait.log'
        'Timeout: no matching agent connected within 1s' | Set-Content $path
        { Read-DevFlowJson -Path $path } | Should -Throw '*timed out*'
    }

    It 'rejects malformed, empty, and unexpected human output' {
        $path = Join-Path $TestDrive 'invalid.json'
        foreach ($text in @('{"broken":', 'null', 'Everything worked!')) {
            $text | Set-Content $path
            { Read-DevFlowJson -Path $path } | Should -Throw
        }
    }
}

Describe 'Native process boundary' {
    BeforeAll {
        $pwsh = (Get-Process -Id $PID).Path
    }

    It 'preserves argument boundaries and strips credentials only from the child' {
        $previous = [Environment]::GetEnvironmentVariable('GH_TOKEN')
        try {
            [Environment]::SetEnvironmentVariable('GH_TOKEN', 'devflow-test-placeholder')
            $path = Invoke-DevFlowProcess -Program $pwsh -WorkingDirectory $TestDrive `
                -LogPrefix (Join-Path $TestDrive 'environment') `
                -Arguments @('-NoProfile', '-NonInteractive', '-Command', '[Console]::WriteLine([string]::IsNullOrEmpty($env:GH_TOKEN)); [Console]::WriteLine("path with spaces")')
            $path | Should -BeOfType [string]
            Get-Content $path | Should -Be @('True', 'path with spaces')
            [Environment]::GetEnvironmentVariable('GH_TOKEN') | Should -Be 'devflow-test-placeholder'
        }
        finally { [Environment]::SetEnvironmentVariable('GH_TOKEN', $previous) }
    }

    It 'retains the failing native exit code' {
        try {
            Invoke-DevFlowProcess -Program $pwsh -WorkingDirectory $TestDrive `
                -LogPrefix (Join-Path $TestDrive 'failure') -Arguments @('-NoProfile', '-Command', 'exit 37')
            throw 'The process unexpectedly succeeded.'
        }
        catch { $_.Exception.Data['NativeExitCode'] | Should -Be 37 }
    }

    It 'fails explicitly when the executable is unavailable' {
        { Invoke-DevFlowProcess -Program 'nonexistent-devflow-test-command' -Arguments @() `
            -WorkingDirectory $TestDrive -LogPrefix (Join-Path $TestDrive 'missing') } | Should -Throw
    }

    It 'bounds a hung child and reports timeout instead of success' {
        try {
            Invoke-DevFlowProcess -Program $pwsh -WorkingDirectory $TestDrive -TimeoutSeconds 1 `
                -LogPrefix (Join-Path $TestDrive 'timeout') -Arguments @('-NoProfile', '-Command', 'Start-Sleep -Seconds 30')
            throw 'The process unexpectedly succeeded.'
        }
        catch { $_.Exception.Data['NativeExitCode'] | Should -Be 124 }
    }
}

Describe 'Launcher orchestration' {
    BeforeEach {
        $script:root = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + ' repo')
        $script:projectPath = Join-Path $root 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'
        foreach ($directory in @((Split-Path $projectPath), "$root/.config", "$root/.buildtasks", "$root/.github/scripts/shared")) {
            $null = New-Item -ItemType Directory -Path $directory -Force
        }
        '<Project />' | Set-Content $projectPath
        '<Project />' | Set-Content "$root/Directory.Build.props"
        @{ tools = @{ 'microsoft.maui.cli' = @{ version = $version } } } | ConvertTo-Json -Depth 5 | Set-Content "$root/.config/dotnet-tools.json"
        '' | Set-Content "$root/.buildtasks/Microsoft.Maui.Controls.Build.Tasks.dll"
        '' | Set-Content "$root/.buildtasks/Microsoft.Maui.Resizetizer.dll"
        'function Get-MauiTfmVersion { param($RepoRoot); return "10.0" }' | Set-Content "$root/.github/scripts/shared/shared-utils.ps1"
        $script:artifacts = Join-Path $root 'results'
        $script:brokerRunning = $true
        $script:healthy = $true
        $script:oldCi = $env:CI
        $script:oldTfBuild = $env:TF_BUILD
        $env:CI = 'false'
        $env:TF_BUILD = 'false'
        Mock Invoke-DevFlowProcess {
            param($Program, $Arguments, $WorkingDirectory, $LogPrefix)
            $script:freshSession = ($Arguments | Where-Object { $_ -like '-p:MauiDevFlowSessionId=*' }) -replace '^-p:MauiDevFlowSessionId=', ''
            $path = "$LogPrefix.stdout.log"
            @{ Properties = @{ ApplicationId = 'com.microsoft.maui.sandbox'; TargetDir = $WorkingDirectory } } |
                ConvertTo-Json | Set-Content $path
            return $path
        }
        Mock Invoke-DevFlowJson {
            param($Arguments)
            if ($Arguments[0] -eq 'device') {
                return ,@(@{ identifier = 'emulator-5554'; platform = 'android'; is_running = $true; is_emulator = $true })
            }
            if ($Arguments[1] -eq 'broker') { return @{ running = $script:brokerRunning } }
            if ($Arguments[1] -eq 'agent') {
                return @{ running = $script:healthy; app = @{ packageId = 'com.microsoft.maui.sandbox' }; device = @{ platform = 'Android'; deviceType = 'Virtual' } }
            }
            if ($Arguments[1] -eq 'wait') { return @{ sessionId = 'old-readiness-hint' } }
            return ,@(@{ id = 'new-agent'; project = $script:projectPath; platform = 'Android'; sessionId = $script:freshSession
                tfm = 'net10.0-android'; version = $version; port = 10225 })
        }
    }

    AfterEach {
        $env:CI = $script:oldCi
        $env:TF_BUILD = $script:oldTfBuild
    }

    It 'persists readiness, not test success, after selecting the fresh Agent' {
        $result = Start-DevFlowSandbox -Platform Android -DeviceId emulator-5554 -RepoRoot $root -ArtifactDirectory $artifacts | ConvertFrom-Json
        $result.status | Should -Be 'ready'
        $result.testsRun | Should -BeFalse
        $result.agent.port | Should -Be 10225
        $result.sessionId | Should -Match '^df[a-f0-9]{32}$'
        $result.connectionArguments | Should -Contain 'emulator-5554'
        Test-Path (Join-Path $artifacts 'ready.json') | Should -BeTrue
        Should -Invoke Invoke-DevFlowProcess -Times 2 -Exactly
    }

    It 'does not build or start a daemon when the broker is missing' {
        $script:brokerRunning = $false
        { Start-DevFlowSandbox -Platform Android -DeviceId emulator-5554 -RepoRoot $root -ArtifactDirectory $artifacts } | Should -Throw '*foreground*'
        Should -Invoke Invoke-DevFlowProcess -Times 0 -Exactly
    }

    It 'does not persist a ready record for an unhealthy app' {
        $script:healthy = $false
        { Start-DevFlowSandbox -Platform Android -DeviceId emulator-5554 -RepoRoot $root -ArtifactDirectory $artifacts } | Should -Throw '*health*'
        Test-Path (Join-Path $artifacts 'ready.json') | Should -BeFalse
    }
}

Describe 'Optional iOS bundle output tracking' {
    BeforeAll {
        $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
        [xml]$sandbox = Get-Content (Join-Path $repoRoot 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj') -Raw
        $tracking = $sandbox.SelectSingleNode("/Project/Target[@Name='TrackMauiDevFlowBundleFiles']")
        $tracking | Should -Not -BeNullOrEmpty
        [xml]$stub = @'
<Project>
  <Target Name="_GenerateBundleName">
    <PropertyGroup><AppBundleDir>generated app</AppBundleDir></PropertyGroup>
  </Target>
  <Target Name="_CleanGetCurrentAndPriorFileWrites" />
  <ItemGroup>
    <ReferenceCopyLocalPaths Include="dependencies/Microsoft.Maui.DevFlow.Agent.dll" />
    <ReferenceCopyLocalPaths Include="dependencies/Microsoft.Maui.DevFlow.Agent.Core.dll" />
    <ReferenceCopyLocalPaths Include="dependencies/Microsoft.Maui.DevFlow.Logging.dll" />
    <ReferenceCopyLocalPaths Include="dependencies/Microsoft.Maui.Controls.dll" />
  </ItemGroup>
</Project>
'@
        $null = $stub.Project.AppendChild($stub.ImportNode($tracking, $true))
        $script:trackingProject = Join-Path $TestDrive 'tracking.proj'
        $stub.Save($trackingProject)
    }

    It 'tracks <Count> optional bundle outputs when enabled=<Enabled>, iOS=<Ios>' -ForEach @(
        @{ Enabled = 'true'; Ios = 'true'; Count = 3 }
        @{ Enabled = 'false'; Ios = 'true'; Count = 0 }
        @{ Enabled = 'true'; Ios = 'false'; Count = 0 }
    ) {
        $path = Invoke-DevFlowProcess -Program 'dotnet' -WorkingDirectory $TestDrive `
            -LogPrefix (Join-Path $TestDrive "tracking-$Enabled-$Ios") `
            -Arguments @('msbuild', $trackingProject, '-nologo', '-t:_CleanGetCurrentAndPriorFileWrites',
                "-p:_MauiDevFlowEnabled=$Enabled", "-p:_MauiTargetPlatformIsiOS=$Ios", '-getItem:FileWrites')
        $items = (Read-DevFlowJson -Path $path).Items.FileWrites
        $items.Count | Should -Be $Count
        if ($Count -gt 0) {
            $items.Identity | Should -Not -Contain 'generated app/Microsoft.Maui.Controls.dll'
            $items.Identity | Should -Contain 'generated app/Microsoft.Maui.DevFlow.Agent.dll'
        }
    }
}
