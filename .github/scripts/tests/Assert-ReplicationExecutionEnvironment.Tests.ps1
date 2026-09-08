#!/usr/bin/env pwsh
#Requires -Modules Pester

# Generated Sandbox code, generated tests, and generated fixes run on the same
# agent as the credentials that publish the result. These tests cover the
# construction of the child environment those processes get, and the scan that
# proves nothing leaked into what the run published.

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationExecutionEnvironment.ps1')

    $script:ScratchRoot = Join-Path $PSScriptRoot 'execution-environment-scratch'
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $script:ScratchRoot -Force | Out-Null

    $script:Canary = (Get-ReplicationSecretCanaryPrefix) + '15121999-1'

    # A realistic agent environment: what a device pool actually carries when
    # the replicate step starts.
    function script:New-AgentEnvironment {
        return @{
            # The real PATH, because one of these tests starts a grandchild
            # process and a fabricated PATH would prove only that pwsh is missing.
            PATH = [Environment]::GetEnvironmentVariable('PATH')
            CI = 'true'
            HOME = [Environment]::GetEnvironmentVariable('HOME')
            TMPDIR = [IO.Path]::GetTempPath()
            DOTNET_ROOT = '/usr/share/dotnet'
            DOTNET_NOLOGO = '1'
            DOTNET_CLI_HOME = '/agent/_temp/dotnet'
            GRADLE_USER_HOME = '/agent/_temp/gradle'
            JAVA_HOME = '/usr/lib/jvm/temurin-17'
            ANDROID_HOME = '/usr/local/lib/android/sdk'
            ANDROID_SDK_ROOT = '/usr/local/lib/android/sdk'
            APPIUM_HOME = '/agent/_temp/.appium'
            DEVICE_UDID = 'emulator-5554'
            XDG_RUNTIME_DIR = '/run/user/1000'
            DBUS_SESSION_BUS_ADDRESS = 'unix:path=/run/user/1000/bus'

            GH_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            GITHUB_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            GH_COMMENT_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            COPILOT_GITHUB_TOKEN = 'cop_pretend_token'
            SYSTEM_ACCESSTOKEN = 'azdo-oauth-token'
            AZURE_STORAGE_KEY = 'base64+storage+key'
            AZURE_CLIENT_SECRET = 'client-secret'
            ENDPOINT_AUTH_SYSTEMVSSCONNECTION = '{"parameters":{"AccessToken":"x"}}'
            VSS_NUGET_EXTERNAL_FEED_ENDPOINTS = '{"endpointCredentials":[]}'
            NUGET_PLUGIN_PATHS = '/agent/credprovider'
            GIT_ASKPASS = '/agent/askpass.sh'
            GIT_CONFIG_PARAMETERS = "'http.extraheader=AUTHORIZATION: basic Zm9v'"
            HTTPS_PROXY = 'http://proxyuser:proxypassword@proxy.internal:8080'
            HTTP_PROXY = 'http://proxyuser:proxypassword@proxy.internal:8080'
            AGENT_PROXYPASSWORD = 'proxypassword'
            MY_PRIVATE_FEED_TOKEN = 'feed-token'
            SOME_SERVICE_PASSWORD = 'hunter2'
            MAUI_REPLICATION_SECRET_CANARY = $script:Canary
            MAUI_REPLICATION_EGRESS_ISOLATED = '1'
        }
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Constructing the environment a generated process sees' {
    BeforeEach {
        $script:Built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
    }

    It 'keeps only the runtime variables a build and a device run need' {
        foreach ($name in @(
            'PATH', 'CI', 'HOME', 'TMPDIR', 'DOTNET_ROOT', 'DOTNET_NOLOGO',
            'DOTNET_CLI_HOME', 'GRADLE_USER_HOME',
            'JAVA_HOME', 'ANDROID_HOME', 'ANDROID_SDK_ROOT', 'APPIUM_HOME',
            'DEVICE_UDID')) {
            $script:Built.Contains($name) | Should -BeTrue -Because "$name is required to build and run"
        }
    }

    It 'drops every GitHub, Copilot, and Azure DevOps credential' {
        foreach ($name in @(
            'GH_TOKEN', 'GITHUB_TOKEN', 'GH_COMMENT_TOKEN',
            'COPILOT_GITHUB_TOKEN', 'SYSTEM_ACCESSTOKEN')) {
            $script:Built.Contains($name) | Should -BeFalse -Because "$name may never reach generated code"
        }
    }

    It 'drops every AZURE_ variable and every service-connection endpoint' {
        foreach ($name in @(
            'AZURE_STORAGE_KEY', 'AZURE_CLIENT_SECRET',
            'ENDPOINT_AUTH_SYSTEMVSSCONNECTION')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops git credential helpers, askpass, and injected git config' {
        foreach ($name in @('GIT_ASKPASS', 'GIT_CONFIG_PARAMETERS')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops user-manager control channels that could escape a cgroup' {
        foreach ($name in @('XDG_RUNTIME_DIR', 'DBUS_SESSION_BUS_ADDRESS')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops proxy variables, which is where proxy credentials live' {
        foreach ($name in @('HTTP_PROXY', 'HTTPS_PROXY', 'AGENT_PROXYPASSWORD')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
        foreach ($value in @($script:Built.Values)) {
            [string]$value | Should -Not -Match 'proxypassword'
        }
    }

    It 'drops NuGet and dotnet feed credentials' {
        foreach ($name in @('VSS_NUGET_EXTERNAL_FEED_ENDPOINTS', 'NUGET_PLUGIN_PATHS')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops an inherited secret nobody thought to list' {
        # The point of an allowlist. Neither of these appears anywhere in the
        # pipeline; both are refused because they were never permitted.
        foreach ($name in @('MY_PRIVATE_FEED_TOKEN', 'SOME_SERVICE_PASSWORD')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops the canary' {
        $script:Built.Contains('MAUI_REPLICATION_SECRET_CANARY') | Should -BeFalse
        foreach ($value in @($script:Built.Values)) {
            [string]$value | Should -Not -Match ([regex]::Escape((Get-ReplicationSecretCanaryPrefix)))
        }
    }

    It 'drops a permitted name that was given the canary as its value' {
        # An allowlisted name is not a licence to carry anything.
        $inherited = script:New-AgentEnvironment
        $inherited['APPIUM_HOME'] = $script:Canary
        $built = Get-ReplicationExecutionEnvironment -Inherited $inherited
        $built.Contains('APPIUM_HOME') | Should -BeFalse
    }

    It 'lets a trusted caller add a required runtime value' {
        $built = Get-ReplicationExecutionEnvironment `
            -Inherited (script:New-AgentEnvironment) `
            -Additional @{ MSBUILDDISABLENODEREUSE = '1' }
        $built['MSBUILDDISABLENODEREUSE'] | Should -Be '1'
    }

    It 'refuses a trusted caller that tries to add a credential back' {
        {
            Get-ReplicationExecutionEnvironment `
                -Inherited @{ PATH = '/usr/bin' } `
                -Additional @{ GH_TOKEN = 'ghp_x' }
        } | Should -Throw '*may not add a forbidden variable*'
    }

    It 'refuses a trusted caller that tries to smuggle the canary in' {
        {
            Get-ReplicationExecutionEnvironment `
                -Inherited @{ PATH = '/usr/bin' } `
                -Additional @{ APPIUM_HOME = $script:Canary }
        } | Should -Throw '*canary-bearing content*'
    }
}

Describe 'Re-checking a constructed environment' {
    It 'accepts the environment the allowlist produced' {
        $built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
        { Assert-ReplicationExecutionEnvironment -Environment $built } | Should -Not -Throw
    }

    It 'refuses a forbidden name that reached the set some other way' {
        # The assertion is deliberately independent of the builder: a mistake in
        # the allowlist has to be caught rather than trusted.
        {
            Assert-ReplicationExecutionEnvironment `
                -Environment @{ PATH = '/usr/bin'; SYSTEM_ACCESSTOKEN = 'x' } `
                -Context 'test'
        } | Should -Throw '*carries variables it may not*SYSTEM_ACCESSTOKEN*'
    }

    It 'refuses a name that is merely not on the allowlist' {
        {
            Assert-ReplicationExecutionEnvironment -Environment @{ PATH = '/usr/bin'; SOMETHING_ELSE = 'x' }
        } | Should -Throw '*SOMETHING_ELSE*'
    }

    It 'refuses an allowed name carrying the canary' {
        {
            Assert-ReplicationExecutionEnvironment -Environment @{ PATH = $script:Canary }
        } | Should -Throw '*PATH*'
    }
}

Describe 'A generated process really does not see the secrets' {
    It 'keeps them from the child and from the grandchild it starts' {
        # The end-to-end claim, made by actually starting the processes: a pwsh
        # child launched with the constructed environment, which itself starts a
        # grandchild and reports what both could see.
        $grandchild = Join-Path $script:ScratchRoot 'grandchild.ps1'
        Set-Content -LiteralPath $grandchild -Encoding utf8NoBOM -Value @'
$names = @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN','SYSTEM_ACCESSTOKEN',
  'AZURE_STORAGE_KEY','AZURE_CLIENT_SECRET','ENDPOINT_AUTH_SYSTEMVSSCONNECTION',
  'VSS_NUGET_EXTERNAL_FEED_ENDPOINTS','GIT_ASKPASS','GIT_CONFIG_PARAMETERS',
  'HTTPS_PROXY','HTTP_PROXY','AGENT_PROXYPASSWORD','MY_PRIVATE_FEED_TOKEN',
  'SOME_SERVICE_PASSWORD','MAUI_REPLICATION_SECRET_CANARY',
  'MAUI_REPLICATION_EGRESS_ISOLATED')
foreach ($n in $names) {
  $v = [Environment]::GetEnvironmentVariable($n)
  if (-not [string]::IsNullOrEmpty($v)) { Write-Output "GRANDCHILD-LEAK:$n" }
}
Write-Output "GRANDCHILD-PATH:$([bool][Environment]::GetEnvironmentVariable('PATH'))"
'@

        $child = Join-Path $script:ScratchRoot 'child.ps1'
        Set-Content -LiteralPath $child -Encoding utf8NoBOM -Value @"
`$names = @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN','SYSTEM_ACCESSTOKEN',
  'AZURE_STORAGE_KEY','AZURE_CLIENT_SECRET','ENDPOINT_AUTH_SYSTEMVSSCONNECTION',
  'VSS_NUGET_EXTERNAL_FEED_ENDPOINTS','GIT_ASKPASS','GIT_CONFIG_PARAMETERS',
  'HTTPS_PROXY','HTTP_PROXY','AGENT_PROXYPASSWORD','MY_PRIVATE_FEED_TOKEN',
  'SOME_SERVICE_PASSWORD','MAUI_REPLICATION_SECRET_CANARY',
  'MAUI_REPLICATION_EGRESS_ISOLATED')
foreach (`$n in `$names) {
  `$v = [Environment]::GetEnvironmentVariable(`$n)
  if (-not [string]::IsNullOrEmpty(`$v)) { Write-Output "CHILD-LEAK:`$n" }
}
& '$((Get-Command pwsh).Source)' -NoLogo -NoProfile -NonInteractive -File '$grandchild'
"@

        $built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
        $startInfo = [Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = (Get-Command pwsh).Source
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in @('-NoLogo', '-NoProfile', '-NonInteractive', '-File', $child)) {
            [void]$startInfo.ArgumentList.Add($argument)
        }
        $startInfo.Environment.Clear()
        foreach ($name in @($built.Keys)) {
            $startInfo.Environment[[string]$name] = [string]$built[$name]
        }

        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        try {
            $process.Start() | Should -BeTrue
            $stdout = $process.StandardOutput.ReadToEnd()
            $stderr = $process.StandardError.ReadToEnd()
            $process.WaitForExit(120000) | Should -BeTrue
        } finally {
            $process.Dispose()
        }

        if ([string]::IsNullOrWhiteSpace($stdout)) {
            throw "The child produced no output. stderr: $stderr"
        }
        $stdout | Should -Not -Match 'CHILD-LEAK'
        $stdout | Should -Not -Match 'GRANDCHILD-LEAK'
        $stdout | Should -Match 'GRANDCHILD-PATH:True'
        # And nothing the child printed carries a marker either.
        Get-ReplicationSecretMarkerMatch -Text ($stdout + $stderr) | Should -BeNullOrEmpty
    }
}

Describe 'Fail-closed outbound network isolation' {
    BeforeEach {
        $script:PriorIsolation = [Environment]::GetEnvironmentVariable(
            'MAUI_REPLICATION_EGRESS_ISOLATED')
        [Environment]::SetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED', '1')
        $script:NetworkRepo = Join-Path $script:ScratchRoot ([guid]::NewGuid().ToString('N'))
        $manifest = Join-Path $script:NetworkRepo (
            'src/Controls/samples/Controls.Sample.Sandbox/Platforms/Android/ReplicationNetworkIsolationManifest.xml')
        New-Item -ItemType Directory -Path (Split-Path -Parent $manifest) -Force |
            Out-Null
        Set-Content -LiteralPath $manifest -Encoding utf8NoBOM -Value @'
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
          xmlns:tools="http://schemas.android.com/tools">
  <uses-permission android:name="android.permission.INTERNET" tools:node="remove" />
</manifest>
'@
    }

    Describe 'Windows AppContainer trusted host command allowlist' {
        BeforeEach {
            $script:WindowsTrustedRoot = Join-Path $script:ScratchRoot (
                'windows-' + [guid]::NewGuid().ToString('N'))
            foreach ($relative in @(
                'scripts/BuildAndRunSandbox.ps1',
                'scripts/shared/Record-Reproduction.ps1',
                'scripts/shared/Invoke-ReplicationTestVerification.ps1',
                'scripts/Other.ps1'
            )) {
                $path = Join-Path $script:WindowsTrustedRoot $relative
                New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force |
                    Out-Null
                Set-Content -LiteralPath $path -Value '# trusted test fixture'
            }
            $script:WindowsEnvironment = @{
                PATH = [Environment]::GetEnvironmentVariable('PATH')
            }
        }

        It 'admits the exact packaged Sandbox runner with the boundary switch' {
            $command = Get-ReplicationWindowsAppContainerCommand `
                -TrustedRoot $script:WindowsTrustedRoot `
                -ScriptPath (Join-Path $script:WindowsTrustedRoot (
                    'scripts/BuildAndRunSandbox.ps1')) `
                -Arguments @(
                    '-Platform', 'windows',
                    '-PrepareOnly',
                    '-EnforceNetworkIsolation'
                ) `
                -Environment $script:WindowsEnvironment `
                -OperatingSystem windows

            $command.Boundary | Should -BeExactly 'windows-appcontainer'
            $command.Arguments | Should -Contain '-EnforceNetworkIsolation'
        }

        It 'rejects a Sandbox run without AppContainer enforcement' {
            {
                Get-ReplicationWindowsAppContainerCommand `
                    -TrustedRoot $script:WindowsTrustedRoot `
                    -ScriptPath (Join-Path $script:WindowsTrustedRoot (
                        'scripts/BuildAndRunSandbox.ps1')) `
                    -Arguments @('-Platform', 'windows', '-PrepareOnly') `
                    -Environment $script:WindowsEnvironment `
                    -OperatingSystem windows
            } | Should -Throw '*requires the AppContainer boundary*'
        }

        It 'round-trips recording replay arguments without flattening the array' {
            $nestedArguments = @(
                '-Platform', 'windows',
                '-Configuration', 'Debug',
                '-EnforceNetworkIsolation'
            )
            $payload = [Convert]::ToBase64String(
                [Text.Encoding]::UTF8.GetBytes(
                    (ConvertTo-Json -InputObject $nestedArguments -Compress)))
            $sandboxPath = Join-Path $script:WindowsTrustedRoot (
                'scripts/BuildAndRunSandbox.ps1')

            $command = Get-ReplicationWindowsAppContainerCommand `
                -TrustedRoot $script:WindowsTrustedRoot `
                -ScriptPath (Join-Path $script:WindowsTrustedRoot (
                    'scripts/shared/Record-Reproduction.ps1')) `
                -Arguments @(
                    '-Platform', 'windows',
                    '-ReproductionScriptPath', $sandboxPath,
                    '-ReproductionArgumentsPayload', $payload
                ) `
                -Environment $script:WindowsEnvironment `
                -OperatingSystem windows

            $command.Boundary | Should -BeExactly 'windows-appcontainer'
        }

        It 'rejects recording replay arguments that drop AppContainer enforcement' {
            $nestedArguments = @(
                '-Platform', 'windows',
                '-Configuration', 'Debug'
            )
            $payload = [Convert]::ToBase64String(
                [Text.Encoding]::UTF8.GetBytes(
                    (ConvertTo-Json -InputObject $nestedArguments -Compress)))
            {
                Get-ReplicationWindowsAppContainerCommand `
                    -TrustedRoot $script:WindowsTrustedRoot `
                    -ScriptPath (Join-Path $script:WindowsTrustedRoot (
                        'scripts/shared/Record-Reproduction.ps1')) `
                    -Arguments @(
                        '-Platform', 'windows',
                        '-ReproductionScriptPath', (Join-Path (
                            $script:WindowsTrustedRoot) (
                            'scripts/BuildAndRunSandbox.ps1')),
                        '-ReproductionArgumentsPayload', $payload
                    ) `
                    -Environment $script:WindowsEnvironment `
                    -OperatingSystem windows
            } | Should -Throw '*must preserve the AppContainer boundary*'
        }

        It 'rejects unlisted trusted scripts and host-executed test tiers' {
            {
                Get-ReplicationWindowsAppContainerCommand `
                    -TrustedRoot $script:WindowsTrustedRoot `
                    -ScriptPath (Join-Path $script:WindowsTrustedRoot 'scripts/Other.ps1') `
                    -Arguments @('-Platform', 'windows') `
                    -Environment $script:WindowsEnvironment `
                    -OperatingSystem windows
            } | Should -Throw '*limited to exact trusted runners*'

            {
                Get-ReplicationWindowsAppContainerCommand `
                    -TrustedRoot $script:WindowsTrustedRoot `
                    -ScriptPath (Join-Path $script:WindowsTrustedRoot (
                        'scripts/shared/Invoke-ReplicationTestVerification.ps1')) `
                    -Arguments @(
                        '-Platform', 'windows',
                        '-TestType', 'UnitTest'
                    ) `
                    -Environment $script:WindowsEnvironment `
                    -OperatingSystem windows
            } | Should -Throw '*only packaged device tests*'
        }
    }

    AfterEach {
        [Environment]::SetEnvironmentVariable(
            'MAUI_REPLICATION_EGRESS_ISOLATED',
            $script:PriorIsolation)
    }

    It 'accepts only a configured boundary whose DNS TCP and HTTP probes are all denied' {
        $result = Assert-ReplicationOutboundNetworkIsolation `
            -Platform android `
            -RepositoryRoot $script:NetworkRepo `
            -DnsProbe { $false } `
            -TcpProbe { $false } `
            -HttpProbe { $false }

        $result.Boundary | Should -BeExactly 'job'
        $result.DnsDenied | Should -BeTrue
        $result.DirectTcpDenied | Should -BeTrue
        $result.HttpDenied | Should -BeTrue
    }

    It 'fails closed when the pool did not attest an installed boundary' {
        [Environment]::SetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED', $null)

        {
            Assert-ReplicationOutboundNetworkIsolation `
                -Platform android `
                -RepositoryRoot $script:NetworkRepo `
                -DnsProbe { $false } `
                -TcpProbe { $false } `
                -HttpProbe { $false }
        } | Should -Throw '*requires a verified job-level*'
    }

    It 'fails closed when any DNS direct TCP or HTTP path escapes' -TestCases @(
        @{ Name = 'DNS'; Dns = $true; Tcp = $false; Http = $false },
        @{ Name = 'direct metadata TCP'; Dns = $false; Tcp = $true; Http = $false },
        @{ Name = 'HTTP'; Dns = $false; Tcp = $false; Http = $true }
    ) {
        param($Name, $Dns, $Tcp, $Http)
        $dnsResult = $Dns
        $tcpResult = $Tcp
        $httpResult = $Http
        $dnsProbe = { $dnsResult }.GetNewClosure()
        $tcpProbe = { $tcpResult }.GetNewClosure()
        $httpProbe = { $httpResult }.GetNewClosure()

        {
            Assert-ReplicationOutboundNetworkIsolation `
                -Platform android `
                -RepositoryRoot $script:NetworkRepo `
                -DnsProbe $dnsProbe `
                -TcpProbe $tcpProbe `
                -HttpProbe $httpProbe
        } | Should -Throw "*allowed $Name egress*"
    }

    It 'requires Android INTERNET permission removal in addition to the host boundary' {
        $manifest = Join-Path $script:NetworkRepo (
            'src/Controls/samples/Controls.Sample.Sandbox/Platforms/Android/ReplicationNetworkIsolationManifest.xml')
        Set-Content -LiteralPath $manifest -Encoding utf8NoBOM -Value @'
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
  <uses-permission android:name="android.permission.INTERNET" />
</manifest>
'@

        {
            Assert-ReplicationOutboundNetworkIsolation `
                -Platform android `
                -RepositoryRoot $script:NetworkRepo `
                -DnsProbe { $false } `
                -TcpProbe { $false } `
                -HttpProbe { $false }
        } | Should -Throw '*must remove INTERNET permission*'
    }
}

Describe 'Selecting a real process isolation boundary' {
    BeforeAll {
        $script:Wrapper = Join-Path $PSScriptRoot (
            '../shared/Invoke-ReplicationNetworkIsolatedProcess.ps1')
        $script:TrustedRoot = Join-Path $script:ScratchRoot 'trusted-root'
        New-Item -ItemType Directory -Path $script:TrustedRoot -Force | Out-Null
        $script:Target = Join-Path $script:TrustedRoot 'isolated-target.ps1'
        $script:IsolationPlanRepo = Join-Path $script:ScratchRoot 'isolation-plan-repo'
        $script:IsolationPlanArtifactRoot = Join-Path $script:ScratchRoot 'isolation-plan-artifacts'
        New-Item -ItemType Directory `
            -Path $script:IsolationPlanRepo, $script:IsolationPlanArtifactRoot `
            -Force | Out-Null
        foreach ($relative in @('.git/hooks', '.git/objects', '.git/refs', '.git/info')) {
            New-Item -ItemType Directory -Path (Join-Path $script:IsolationPlanRepo $relative) -Force |
                Out-Null
        }
        foreach ($relative in @('.git/config', '.git/HEAD', '.git/packed-refs')) {
            Set-Content -LiteralPath (Join-Path $script:IsolationPlanRepo $relative) `
                -Value 'trusted' -Encoding utf8NoBOM
        }
        $nugetPackages = Join-Path $script:ScratchRoot 'nuget-packages'
        New-Item -ItemType Directory -Path $nugetPackages -Force | Out-Null
        Set-Content -LiteralPath $script:Target -Encoding utf8NoBOM -Value (
            'param([string]$Value) Write-Output $Value')
        $script:MinimalEnvironment = @{
            PATH = [Environment]::GetEnvironmentVariable('PATH')
            HOME = [Environment]::GetEnvironmentVariable('HOME')
            TMPDIR = [IO.Path]::GetTempPath()
            NUGET_PACKAGES = $nugetPackages
        }

        $tokens = $null
        $errors = $null
        $script:OrchestratorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot '../Replicate-Issue.ps1'), [ref]$tokens, [ref]$errors)
        if ($errors) {
            throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
        }
        $runtimeEnvironmentFunction = $script:OrchestratorAst.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'Get-ReplicationRuntimeEnvironment'
        }, $true)
        if ($null -eq $runtimeEnvironmentFunction) {
            throw 'The replication runtime environment factory is missing.'
        }
        . ([scriptblock]::Create($runtimeEnvironmentFunction.Extent.Text))
        $script:Platform = 'android'
        $script:replicationRuntimeRoot = Join-Path $script:ScratchRoot 'runtime-environment'
        $script:replicationHome = Join-Path $script:replicationRuntimeRoot 'home'
        $script:replicationGradleHome = Join-Path $script:replicationRuntimeRoot 'gradle'
        $script:replicationDotnetHome = Join-Path $script:replicationRuntimeRoot 'dotnet'
        $script:replicationNugetPackages = Join-Path $script:replicationRuntimeRoot 'nuget'
        $script:replicationAndroidHome = Join-Path $script:replicationHome '.android'
        $script:replicationCacheHome = Join-Path $script:replicationRuntimeRoot 'cache'
    }

    It 'uses a Linux cgroup firewall with only loopback allowed' {
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @('-Value', 'ok') `
            -Environment $script:MinimalEnvironment `
            -WritableRoots @($script:IsolationPlanRepo) `
            -DeviceUdid 'emulator-5554' `
            -TimeoutSeconds 42 `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        $command.FilePath | Should -BeExactly '/usr/bin/sudo'
        $command.Boundary | Should -BeExactly 'systemd-cgroup-loopback-only'
        $command.Arguments | Should -Contain '--property=IPAddressDeny=any'
        $command.Arguments | Should -Contain '--property=IPAddressAllow=localhost'
        $command.Arguments | Should -Contain '--property=RuntimeMaxSec=42s'
        $command.Arguments | Should -Contain '--property=TimeoutStopSec=15s'
        $command.Arguments | Should -Contain '--property=KillMode=control-group'
        ($command.Arguments -join "`n") | Should -Match '--unit=maui-replication-[0-9]+-[0-9a-f]{32}'
        $command.UnitName | Should -Match '^maui-replication-[0-9]+-[0-9a-f]{32}\.service$'
        ($command.Arguments -join "`n") | Should -Not -Match 'IPAddressAllow=192\.0\.2\.'
        $command.Arguments | Should -Contain '--property=NoNewPrivileges=yes'
        $command.Arguments | Should -Contain '--property=CapabilityBoundingSet='
        $command.Arguments | Should -Contain '--property=RestrictSUIDSGID=yes'
        $command.Arguments | Should -Contain '--property=RestrictNamespaces=yes'
        $command.Arguments | Should -Contain '--property=ProtectSystem=strict'
        $command.Arguments | Should -Contain '--property=ProtectHome=tmpfs'
        $command.Arguments | Should -Not -Contain '--property=ProtectHome=yes'
        $command.Arguments | Should -Contain '--property=ProtectControlGroups=yes'
        $command.Arguments | Should -Contain '--property=ProtectKernelModules=yes'
        $command.Arguments | Should -Contain '--property=ProtectKernelTunables=yes'
        $command.Arguments | Should -Contain '--property=ProtectProc=invisible'
        $command.Arguments | Should -Contain '--property=ProcSubset=pid'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/run/user/1000'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/usr/bin/systemctl'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/usr/bin/busctl'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/run/docker\.sock'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/usr/bin/docker'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/usr/bin/adb'
        $command.Arguments | Should -Contain '--property=PrivateNetwork=yes'
        ($command.Arguments -join "`n") | Should -Match 'InaccessiblePaths=.*?/\.git/hooks'
        ($command.Arguments -join "`n") | Should -Match 'ReadOnlyPaths=.*?/\.git/config'
        $command.Arguments | Should -Contain '--property=SupplementaryGroups='
        $command.Arguments | Should -Not -Contain '--property=RestrictAddressFamilies=AF_UNIX'
        $command.Arguments | Should -Contain '--property=UnsetEnvironment=XDG_RUNTIME_DIR DBUS_SESSION_BUS_ADDRESS'
        ($command.Arguments -join "`n") | Should -Not -Match 'MAUI_REPLICATION_EGRESS_ISOLATED'
        ($command.Arguments -join "`n") | Should -Not -Match '--setenv=XDG_RUNTIME_DIR='
        ($command.Arguments -join "`n") | Should -Match 'BindPaths=.*?/nuget-packages'
        ($command.Arguments -join "`n") |
            Should -Match ([regex]::Escape("BindReadOnlyPaths=$($script:TrustedRoot)"))
    }

    It 'uses a private Android HOME whose .android directory is the adb home' {
        $environment = Get-ReplicationRuntimeEnvironment
        { Assert-ReplicationExecutionEnvironment -Environment $environment } |
            Should -Not -Throw
        $environment['HOME'] | Should -BeExactly $script:replicationHome
        $environment['ANDROID_USER_HOME'] | Should -BeExactly $script:replicationAndroidHome
        $environment['ANDROID_USER_HOME'] |
            Should -BeExactly (Join-Path ([string]$environment['HOME']) '.android')
        Test-Path -LiteralPath $script:replicationHome -PathType Container |
            Should -BeTrue
        Test-Path -LiteralPath $script:replicationAndroidHome -PathType Container |
            Should -BeTrue

        # JDK binding is covered separately and must not depend on this host's provisioning.
        $environment.Remove('JAVA_HOME')
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @() `
            -Environment $environment `
            -WritableRoots @(
                $script:IsolationPlanRepo,
                $script:IsolationPlanArtifactRoot,
                $script:replicationHome) `
            -DeviceUdid 'emulator-5554' `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        $command.Arguments | Should -Contain "--setenv=HOME=$($script:replicationHome)"
        $command.Arguments |
            Should -Contain "--setenv=ANDROID_USER_HOME=$($script:replicationAndroidHome)"
        $command.Arguments |
            Should -Contain "--property=BindPaths=$($script:replicationHome)"
        $command.Arguments | Should -Contain '--property=ProtectHome=tmpfs'

        $hostHomes = @(
            [Environment]::GetEnvironmentVariable('HOME'),
            [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
        ) | Where-Object {
            -not [string]::IsNullOrWhiteSpace($_) -and [IO.Path]::IsPathRooted($_)
        } | ForEach-Object {
            [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($_))
        } | Sort-Object -Unique
        foreach ($hostHome in $hostHomes) {
            if ($hostHome -ceq $script:replicationHome) { continue }
            $command.Arguments | Should -Not -Contain "--property=BindPaths=$hostHome"
            $command.Arguments |
                Should -Not -Contain "--property=BindReadOnlyPaths=$hostHome"
        }
    }

    It 'wires the private home into both production Linux isolated command sites' {
        $networkCalls = @($script:OrchestratorAst.FindAll({
            $args[0] -is [System.Management.Automation.Language.CommandAst] -and
            $args[0].GetCommandName() -eq 'Get-ReplicationNetworkIsolatedCommand'
        }, $true))

        $networkCalls.Count | Should -Be 2
        $inlineCall = @($networkCalls | Where-Object {
                $_.Extent.Text -match '(?s)-Environment\s+\(Get-ReplicationRuntimeEnvironment\)'
            })
        $loggedChildCall = @($networkCalls | Where-Object {
                $_.Extent.Text -match '(?s)-Environment\s+\$childEnvironment\b'
            })
        $inlineCall.Count | Should -Be 1
        $loggedChildCall.Count | Should -Be 1

        $loggedChildFunction = $script:OrchestratorAst.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'Invoke-LoggedChildProcess'
        }, $true)
        $loggedChildFunction | Should -Not -Be $null
        $loggedChildFunction.Extent.Text |
            Should -Match '(?m)^\s*\$childEnvironment\s*=\s*Get-ReplicationRuntimeEnvironment\s*$'

        foreach ($call in @($inlineCall + $loggedChildCall)) {
            $call.Extent.Text |
                Should -Match '(?s)-WritableRoots\s+@\(\$repoRoot,\s*\$ArtifactRoot,\s*\$replicationHome\)'
        }
    }

    It 'preserves inherited HOME outside Android for <Platform>' -TestCases @(
        @{ Platform = 'ios' },
        @{ Platform = 'catalyst' },
        @{ Platform = 'windows' }
    ) {
        param([string]$Platform)

        $previousPlatform = $script:Platform
        try {
            $script:Platform = $Platform
            $inheritedHome = [Environment]::GetEnvironmentVariable('HOME')
            $environment = Get-ReplicationRuntimeEnvironment
            $actualHome = if ($environment.Contains('HOME')) {
                [string]$environment['HOME']
            } else {
                $null
            }

            $actualHome | Should -BeExactly $inheritedHome
            $actualHome | Should -Not -BeExactly $script:replicationHome
        } finally {
            $script:Platform = $previousPlatform
        }
    }

    It 'carries the run-scoped <CacheProperty> from the runtime factory into isolation' -TestCases @(
        @{
            CacheProperty = 'MavenCacheDirectory'
            RelativePath = 'dotnet-android/MavenCacheDirectory'
            TrailingSeparator = $false
        },
        @{
            CacheProperty = 'XamarinBuildDownloadDir'
            RelativePath = 'XamarinBuildDownload'
            TrailingSeparator = $true
        }
    ) {
        param($CacheProperty, $RelativePath, $TrailingSeparator)

        $environment = Get-ReplicationRuntimeEnvironment
        { Assert-ReplicationExecutionEnvironment -Environment $environment } | Should -Not -Throw
        $expectedCache = Join-Path $script:replicationCacheHome $RelativePath
        if ($TrailingSeparator) {
            $expectedCache += [IO.Path]::DirectorySeparatorChar
        }
        $environment[$CacheProperty] | Should -BeExactly $expectedCache
        $environment['XDG_CACHE_HOME'] | Should -BeExactly $script:replicationCacheHome
        Test-Path -LiteralPath $script:replicationCacheHome -PathType Container | Should -BeTrue
        (Get-ReplicationRuntimeEnvironment)[$CacheProperty] |
            Should -BeExactly $expectedCache

        # JDK binding is covered separately and must not depend on this host's provisioning.
        $environment.Remove('JAVA_HOME')
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @() `
            -Environment $environment `
            -WritableRoots @(
                $script:IsolationPlanRepo,
                $script:IsolationPlanArtifactRoot,
                $script:replicationHome) `
            -DeviceUdid 'emulator-5554' `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        $command.Arguments | Should -Contain "--setenv=$CacheProperty=$expectedCache"
        $command.Arguments | Should -Contain "--property=BindPaths=$($script:replicationCacheHome)"
        $command.Arguments | Should -Contain "--property=BindPaths=$($environment['HOME'])"
        $command.Arguments | Should -Contain '--property=ProtectHome=tmpfs'
        $hostHomes = @(
            [Environment]::GetEnvironmentVariable('HOME'),
            [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
        ) | Where-Object {
            -not [string]::IsNullOrWhiteSpace($_) -and [IO.Path]::IsPathRooted($_)
        } | ForEach-Object {
            [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($_))
        } | Sort-Object -Unique
        foreach ($hostHome in $hostHomes) {
            if ($hostHome -ceq $environment['HOME']) { continue }
            $command.Arguments | Should -Not -Contain "--property=BindPaths=$hostHome"
            $command.Arguments |
                Should -Not -Contain "--property=BindReadOnlyPaths=$hostHome"
        }
    }

    It 'binds only the provisioned JDK read-only while keeping the home masked' -Skip:([OperatingSystem]::IsWindows()) {
        $javaHome = Join-Path $script:ScratchRoot 'provisioned-jdk'
        New-Item -ItemType Directory -Path $javaHome -Force | Out-Null
        $environment = $script:MinimalEnvironment.Clone()
        $environment['JAVA_HOME'] = $javaHome + [IO.Path]::DirectorySeparatorChar

        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @() `
            -Environment $environment `
            -WritableRoots @($script:IsolationPlanRepo) `
            -DeviceUdid 'emulator-5554' `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        $homeDirectory = [Environment]::GetFolderPath(
            [Environment+SpecialFolder]::UserProfile)
        $command.Arguments | Should -Contain "--property=BindReadOnlyPaths=$javaHome"
        $command.Arguments | Should -Not -Contain "--property=BindPaths=$javaHome"
        $command.Arguments | Should -Not -Contain "--property=BindReadOnlyPaths=$homeDirectory"
        $command.Arguments | Should -Not -Contain "--property=BindPaths=$homeDirectory"
        $command.Arguments | Should -Contain '--property=ProtectHome=tmpfs'
    }

    It 'rejects an unsafe JDK bind: <Kind>' -TestCases @(
        @{ Kind = 'filesystem root' },
        @{ Kind = 'user home' },
        @{ Kind = 'user home with trailing separator' },
        @{ Kind = 'relative path' },
        @{ Kind = 'missing directory' },
        @{ Kind = 'mount syntax' }
    ) {
        param($Kind)

        $environment = $script:MinimalEnvironment.Clone()
        $environment['JAVA_HOME'] = switch ($Kind) {
            'filesystem root' { [IO.Path]::GetPathRoot($script:ScratchRoot) }
            'user home' {
                [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
            }
            'user home with trailing separator' {
                [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile) +
                    [IO.Path]::DirectorySeparatorChar
            }
            'relative path' { 'relative-jdk' }
            'missing directory' { Join-Path $script:ScratchRoot 'missing-jdk' }
            'mount syntax' { (Join-Path $script:ScratchRoot 'jdk') + ':/tmp' }
        }

        {
            Get-ReplicationNetworkIsolatedCommand `
                -Platform android `
                -RepositoryRoot $script:IsolationPlanRepo `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath $script:Target `
                -Arguments @() `
                -Environment $environment `
                -WritableRoots @($script:IsolationPlanRepo) `
                -DeviceUdid 'emulator-5554' `
                -OperatingSystem linux `
                -UserId 1000 `
                -GroupId 1000
        } | Should -Throw '*dedicated, existing JAVA_HOME directory*'
    }

    It 'withholds lanes that have no enforceable process and app boundary' -TestCases @(
        @{ Platform = 'ios'; OS = 'macos' },
        @{ Platform = 'catalyst'; OS = 'macos' },
        @{ Platform = 'windows'; OS = 'windows' }
    ) {
        param($Platform, $OS)

        {
            Get-ReplicationNetworkIsolatedCommand `
                -Platform $Platform `
                -RepositoryRoot $script:IsolationPlanRepo `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath $script:Target `
                -Arguments @() `
                -Environment $script:MinimalEnvironment `
                -WritableRoots @($script:IsolationPlanRepo) `
                -OperatingSystem $OS
        } | Should -Throw '*withheld*'
    }

    It 'rejects a network-connected Android target instead of widening egress' {
        {
            Get-ReplicationNetworkIsolatedCommand `
                -Platform android `
                -RepositoryRoot $script:IsolationPlanRepo `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath $script:Target `
                -Arguments @() `
                -Environment $script:MinimalEnvironment `
                -WritableRoots @($script:IsolationPlanRepo) `
                -DeviceUdid '192.0.2.10:5555' `
                -OperatingSystem linux `
                -UserId 1000 `
                -GroupId 1000
        } | Should -Throw '*local emulator transport*'
    }

    It 'rejects a generated-execution script outside the immutable trusted root' {
        $outside = Join-Path $script:IsolationPlanRepo 'outside.ps1'
        Set-Content -LiteralPath $outside -Value 'exit 0' -Encoding utf8NoBOM

        {
            Get-ReplicationNetworkIsolatedCommand `
                -Platform android `
                -RepositoryRoot $script:IsolationPlanRepo `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath $outside `
                -Arguments @() `
                -Environment $script:MinimalEnvironment `
                -WritableRoots @($script:IsolationPlanRepo) `
                -DeviceUdid 'emulator-5554' `
                -OperatingSystem linux `
                -UserId 1000 `
                -GroupId 1000
        } | Should -Throw '*inside the trusted root*'
    }

    It 'grants adb only to trusted device-control phases' {
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @('-Value', 'ok') `
            -Environment $script:MinimalEnvironment `
            -WritableRoots @($script:IsolationPlanRepo) `
            -AllowDeviceControl `
            -DeviceUdid 'emulator-5554' `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        ($command.Arguments -join "`n") | Should -Not -Match 'platform-tools/adb'
        $command.Arguments | Should -Not -Contain '--property=PrivateNetwork=yes'
        $command.Arguments | Should -Not -Contain '--property=RestrictAddressFamilies=AF_UNIX'
        $command.Arguments | Should -Contain '-AllowDeviceControl'
        $command.Arguments | Should -Contain 'true'
    }

    It 'protects linked-worktree administrative Git metadata' {
        $worktree = Join-Path $script:ScratchRoot 'linked-worktree'
        $admin = Join-Path $script:ScratchRoot 'git-admin/worktrees/linked'
        $common = Join-Path $script:ScratchRoot 'git-admin'
        New-Item -ItemType Directory -Path $worktree -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $admin 'hooks') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $common 'hooks') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $worktree '.git') `
            -Value "gitdir: $admin" -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $admin 'commondir') `
            -Value '../..' -Encoding utf8NoBOM

        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $worktree `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath $script:Target `
            -Arguments @() `
            -Environment $script:MinimalEnvironment `
            -WritableRoots @($worktree) `
            -DeviceUdid 'emulator-5554' `
            -OperatingSystem linux `
            -UserId 1000 `
            -GroupId 1000

        $arguments = $command.Arguments -join "`n"
        $arguments | Should -Match ([regex]::Escape("ReadOnlyPaths=$worktree/.git"))
        $arguments | Should -Match ([regex]::Escape($admin))
        $arguments | Should -Match ([regex]::Escape($common))
        $arguments | Should -Match ([regex]::Escape("$common/hooks"))
    }

    It 'makes privilege and user-manager escape canaries part of the isolated wrapper' {
        $source = Get-Content -LiteralPath $script:Wrapper -Raw
        $escape = $source.IndexOf(
            'Assert-ReplicationPrivilegeEscapesBlocked',
            [StringComparison]::Ordinal)
        $network = $source.IndexOf(
            'Assert-ReplicationOutboundNetworkIsolation',
            [StringComparison]::Ordinal)
        $launch = $source.LastIndexOf(
            '& (Get-Command pwsh',
            [StringComparison]::Ordinal)

        $source | Should -Match "'/usr/bin/sudo'"
        $source | Should -Match "'/usr/bin/systemd-run'"
        $source | Should -Match "'/usr/bin/docker'"
        $source | Should -Match "'/run/docker.sock'"
        $source | Should -Match "'--user'"
        $escape | Should -BeGreaterOrEqual 0
        $escape | Should -BeLessThan $network
        $network | Should -BeLessThan $launch
    }

    It 'stops and verifies the transient unit after generated execution' {
        $script:unitCleanupCalls = [Collections.Generic.List[string]]::new()
        $script:unitActiveChecks = 0
        $invoker = {
            param([string[]]$Arguments)
            $script:unitCleanupCalls.Add(($Arguments -join ' '))
            if ($Arguments[0] -eq 'is-active') {
                $script:unitActiveChecks++
                return [pscustomobject]@{
                    ExitCode = $(if ($script:unitActiveChecks -eq 1) { 0 } else { 3 })
                    Output = ''
                }
            }
            [pscustomobject]@{ ExitCode = 0; Output = '' }
        }

        {
            Stop-ReplicationNetworkIsolationUnit `
                -UnitName 'maui-replication-1234-0123456789abcdef0123456789abcdef.service' `
                -SystemctlInvoker $invoker
        } | Should -Not -Throw
        $script:unitCleanupCalls | Should -Contain 'stop maui-replication-1234-0123456789abcdef0123456789abcdef.service'
        ($script:unitCleanupCalls -join "`n") | Should -Match 'kill --kill-whom=all --signal=KILL'
        $script:unitActiveChecks | Should -Be 2
    }

    It 'fails closed when the transient unit remains active' {
        $invoker = {
            param([string[]]$Arguments)
            [pscustomobject]@{
                ExitCode = $(if ($Arguments[0] -eq 'is-active') { 0 } else { 0 })
                Output = ''
            }
        }

        {
            Stop-ReplicationNetworkIsolationUnit `
                -UnitName 'maui-replication-1234-0123456789abcdef0123456789abcdef.service' `
                -SystemctlInvoker $invoker
        } | Should -Throw '*remained active*'
    }

}

Describe 'Isolating the Android guest from confused-deputy egress' {
    It 'installs and verifies both guest filters after <RootDelay> pending root checks' -ForEach @(
        @{ RootDelay = 0 }
        @{ RootDelay = 2 }
    ) {
        $calls = [Collections.Generic.List[string]]::new()
        $sleeps = [Collections.Generic.List[int]]::new()
        $installed = @{}
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            if ($text -match ' shell id -u$') {
                $identityReads = @($calls | Where-Object {
                        $_ -match ' shell id -u$'
                    }).Count
                if ($identityReads -le $RootDelay) {
                    return [pscustomobject]@{
                        ExitCode = $(if ($identityReads -eq 1) { 1 } else { 0 })
                        Output = $(if ($identityReads -eq 1) { 'device offline' } else { '2000' })
                    }
                }
                return [pscustomobject]@{ ExitCode = 0; Output = '0' }
            }
            if ($text -match 'settings get global airplane_mode_on$') {
                $airplaneReads = @($calls | Where-Object {
                        $_ -match 'settings get global airplane_mode_on$'
                    }).Count
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = $(if ($airplaneReads -eq 1) { '0' } else { '1' })
                }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -D OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -F MAUI_REPLICATION$') {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -I OUTPUT ') {
                $installed[$Matches['tool']] = $true
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -S OUTPUT$') {
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = $(if ($installed.ContainsKey($Matches['tool'])) {
                        "-P OUTPUT ACCEPT`n-A OUTPUT -j MAUI_REPLICATION"
                    } else {
                        '-P OUTPUT ACCEPT'
                    })
                }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -C OUTPUT ' -and
                -not $installed.ContainsKey($Matches['tool'])) {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '' }
        }.GetNewClosure()

        $result = Assert-ReplicationAndroidGuestNetworkIsolation `
            -DeviceUdid emulator-5554 `
            -AdbInvoker $invoker `
            -SleepInvoker { param([int]$Seconds) $sleeps.Add($Seconds) }

        $result.NewConnectionsDenied | Should -BeTrue
        @($calls | Where-Object { $_ -match ' shell id -u$' }).Count |
            Should -Be ($RootDelay + 1)
        @($sleeps | Where-Object { $_ -eq 1 }).Count | Should -Be $RootDelay
        @($sleeps | Where-Object { $_ -eq 5 }).Count | Should -Be 1
        $joined = $calls -join "`n"
        $joined | Should -Match 'iptables -A MAUI_REPLICATION -j REJECT'
        $joined | Should -Match 'ip6tables -A MAUI_REPLICATION -j REJECT'
        $joined | Should -Match 'iptables -A MAUI_REPLICATION -o lo -j RETURN'
        $joined | Should -Match 'ip -4 route show default'
        $joined | Should -Match 'ip -6 route show default'
        $joined | Should -Match 'connectivity airplane-mode enable'
        $joined | Should -Match 'svc wifi disable'
        $joined | Should -Match 'svc data disable'
        $joined | Should -Match 'iptables -D OUTPUT -j MAUI_REPLICATION'
        $joined | Should -Match 'iptables -I OUTPUT 1 -j MAUI_REPLICATION'
    }

    It 'rejects successful adb root without a proven root uid: <Case>' -ForEach @(
        @{ Case = 'production image'; IdentityExitCode = 0; IdentityOutput = '2000' }
        @{ Case = 'empty uid'; IdentityExitCode = 0; IdentityOutput = '' }
        @{ Case = 'malformed uid'; IdentityExitCode = 0; IdentityOutput = '0 2000' }
        @{ Case = 'failed identity command'; IdentityExitCode = 1; IdentityOutput = '0' }
    ) {
        $calls = [Collections.Generic.List[string]]::new()
        $sleeps = [Collections.Generic.List[int]]::new()
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            switch ($text) {
                '-s emulator-5554 root' {
                    return [pscustomobject]@{
                        ExitCode = 0
                        Output = 'adbd cannot run as root in production builds'
                    }
                }
                '-s emulator-5554 wait-for-device' {
                    return [pscustomobject]@{ ExitCode = 0; Output = '' }
                }
                '-s emulator-5554 shell id -u' {
                    return [pscustomobject]@{
                        ExitCode = $IdentityExitCode
                        Output = $IdentityOutput
                    }
                }
                default { throw "Guest setup must not proceed before root is proven: $text" }
            }
        }.GetNewClosure()

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker `
                -SleepInvoker { param([int]$Seconds) $sleeps.Add($Seconds) }
        } | Should -Throw '*requires a root-enabled emulator*'

        @($calls | Where-Object { $_ -match ' root$' }).Count | Should -Be 1
        @($calls | Where-Object { $_ -match ' wait-for-device$' }).Count | Should -Be 10
        @($calls | Where-Object { $_ -match ' shell id -u$' }).Count | Should -Be 10
        $sleeps.Count | Should -Be 10
        @($sleeps | Where-Object { $_ -ne 1 }).Count | Should -Be 0
    }

    It 'reasserts each radio boundary when airplane mode is already enabled' {
        $calls = [Collections.Generic.List[string]]::new()
        $installed = @{
            iptables = $true
            ip6tables = $true
        }
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            if ($text -match ' shell id -u$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '0' }
            }
            if ($text -match 'settings get global airplane_mode_on$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '1' }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -D OUTPUT ') {
                if ($installed[$Matches['tool']]) {
                    $installed[$Matches['tool']] = $false
                    return [pscustomobject]@{ ExitCode = 0; Output = '' }
                }
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -I OUTPUT ') {
                $installed[$Matches['tool']] = $true
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -S OUTPUT$') {
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = "-P OUTPUT ACCEPT`n-A OUTPUT -j MAUI_REPLICATION"
                }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '' }
        }.GetNewClosure()

        Assert-ReplicationAndroidGuestNetworkIsolation `
            -DeviceUdid emulator-5554 `
            -AdbInvoker $invoker `
            -SleepInvoker { param([int]$Seconds) } | Out-Null

        $joined = $calls -join "`n"
        $joined | Should -Not -Match 'connectivity airplane-mode enable'
        $joined | Should -Match 'svc wifi disable'
        $joined | Should -Match 'svc data disable'
    }

    It 'fails closed if a guest default route remains' {
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            if ($text -match ' shell id -u$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '0' }
            }
            if ($text -match 'settings get global airplane_mode_on$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '1' }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = 'default via 10.0.2.2 dev eth0' }
            }
            if ($text -match ' (?:ip6tables|iptables) -D OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            if ($text -match ' (?:ip6tables|iptables) -S OUTPUT$') {
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = "-P OUTPUT ACCEPT`n-A OUTPUT -j MAUI_REPLICATION"
                }
            }
            if ($text -match ' (?:ip6tables|iptables) -C OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '' }
        }

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker `
                -SleepInvoker { param([int]$Seconds) }
        } | Should -Throw '*left an IPv4 default route*'
    }

    It 'fails closed after execution if the guest firewall was removed' {
        $calls = [Collections.Generic.List[string]]::new()
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            if ($text -match ' (?:ip6tables|iptables) -C OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '1' }
        }.GetNewClosure()

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker `
                -VerifyOnly
        } | Should -Throw '*lost the iptables OUTPUT chain*'
        $calls.Count | Should -Be 1
        $calls[0] | Should -Be '-s emulator-5554 shell iptables -C OUTPUT -j MAUI_REPLICATION'
    }

    It 'verifies a healthy guest using only read-only commands' {
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            if ($text -match '^-s emulator-5554 shell (?:ip6tables|iptables) -S OUTPUT$') {
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = "-P OUTPUT ACCEPT`n-A OUTPUT -j MAUI_REPLICATION"
                }
            }
            if ($text -match '^-s emulator-5554 shell (?:ip6tables|iptables) -C (?:OUTPUT -j MAUI_REPLICATION|MAUI_REPLICATION -o lo -j RETURN|MAUI_REPLICATION -j REJECT)$' -or
                $text -match '^-s emulator-5554 shell ip -[46] route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            if ($text -ceq '-s emulator-5554 shell settings get global airplane_mode_on') {
                return [pscustomobject]@{ ExitCode = 0; Output = '1' }
            }
            throw "VerifyOnly must not repair or restart the guest: $text"
        }

        $result = Assert-ReplicationAndroidGuestNetworkIsolation `
            -DeviceUdid emulator-5554 `
            -AdbInvoker $invoker `
            -SleepInvoker { throw 'VerifyOnly must not wait for guest setup.' } `
            -VerifyOnly

        $result.NewConnectionsDenied | Should -BeTrue
        $result.DefaultRoutesRemoved | Should -BeTrue
        $result.AirplaneMode | Should -BeTrue
    }

    It 'fails closed when an earlier OUTPUT rule bypasses the isolation jump' {
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            if ($text -match ' (?:ip6tables|iptables) -S OUTPUT$') {
                return [pscustomobject]@{
                    ExitCode = 0
                    Output = (
                        "-P OUTPUT ACCEPT`n" +
                        "-A OUTPUT -d 10.0.2.2/32 -j ACCEPT`n" +
                        "-A OUTPUT -j MAUI_REPLICATION")
                }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '1' }
        }

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker `
                -VerifyOnly
        } | Should -Throw '*unique first rule*'
    }
}

Describe 'Probing actual Unix socket access' -Skip:(-not [OperatingSystem]::IsLinux()) {
    BeforeEach {
        $script:ProbeSocketPath = Join-Path '/tmp' (
            "maui-replication-probe-$([guid]::NewGuid().ToString('N')).sock")
        $script:ProbeListener = $null
    }

    AfterEach {
        if ($null -ne $script:ProbeListener) {
            $script:ProbeListener.Dispose()
        }
        [IO.File]::Delete($script:ProbeSocketPath)
    }

    It 'accepts the managed missing-endpoint error from a real Unix connection' {
        {
            Assert-ReplicationPrivilegedSocketBlocked -SocketPath $script:ProbeSocketPath
        } | Should -Not -Throw
    }

    It 'accepts a refused connection to a socket with no listener' {
        $script:ProbeListener = [Net.Sockets.Socket]::new(
            [Net.Sockets.AddressFamily]::Unix,
            [Net.Sockets.SocketType]::Stream,
            [Net.Sockets.ProtocolType]::Unspecified)
        $script:ProbeListener.Bind(
            [Net.Sockets.UnixDomainSocketEndPoint]::new($script:ProbeSocketPath))

        {
            Assert-ReplicationPrivilegedSocketBlocked -SocketPath $script:ProbeSocketPath
        } | Should -Not -Throw
    }

    It 'rejects a successful connection to a live socket' {
        $script:ProbeListener = [Net.Sockets.Socket]::new(
            [Net.Sockets.AddressFamily]::Unix,
            [Net.Sockets.SocketType]::Stream,
            [Net.Sockets.ProtocolType]::Unspecified)
        $script:ProbeListener.Bind(
            [Net.Sockets.UnixDomainSocketEndPoint]::new($script:ProbeSocketPath))
        $script:ProbeListener.Listen(1)

        {
            Assert-ReplicationPrivilegedSocketBlocked -SocketPath $script:ProbeSocketPath
        } | Should -Throw '*exposed privileged socket*'
    }

    It 'does not turn an invalid endpoint into successful denial evidence' {
        {
            Assert-ReplicationPrivilegedSocketBlocked -SocketPath ('/' + ('x' * 200))
        } | Should -Throw
    }
}

Describe 'Preinstalling Android Appium helpers before isolation' {
    It 'installs the exact trusted helper set and verifies every package' {
        $appiumHome = Join-Path $TestDrive '.appium'
        $modules = Join-Path $appiumHome (
            'node_modules/appium-uiautomator2-driver/node_modules')
        $server = Join-Path $modules 'appium-uiautomator2-server/apks'
        $settings = Join-Path $modules 'io.appium.settings/apks'
        New-Item -ItemType Directory -Path $server, $settings -Force |
            Out-Null
        foreach ($path in @(
            (Join-Path $server 'appium-uiautomator2-server-v7.4.1.apk'),
            (Join-Path $server 'appium-uiautomator2-server-debug-androidTest.apk'),
            (Join-Path $settings 'settings_apk-debug.apk')
        )) {
            Set-Content -LiteralPath $path -Value 'trusted apk'
        }
        $calls = [Collections.Generic.List[string]]::new()
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            [pscustomobject]@{
                ExitCode = 0
                Output = $(if ($text -match ' shell pm path ') {
                    'package:/data/app/trusted/base.apk'
                } else {
                    'Success'
                })
            }
        }.GetNewClosure()

        $result = Install-ReplicationAndroidAppiumHelpers `
            -DeviceUdid emulator-5554 `
            -AppiumHome $appiumHome `
            -AdbInvoker $invoker

        $result.PackageCount | Should -Be 3
        @($calls | Where-Object { $_ -match ' install -r -t ' }).Count |
            Should -Be 3
        @($calls | Where-Object { $_ -match ' shell pm path ' }).Count |
            Should -Be 3
    }

    It 'fails closed when a helper package cannot be verified' {
        $appiumHome = Join-Path $TestDrive '.appium-failure'
        $modules = Join-Path $appiumHome (
            'node_modules/appium-uiautomator2-driver/node_modules')
        $server = Join-Path $modules 'appium-uiautomator2-server/apks'
        $settings = Join-Path $modules 'io.appium.settings/apks'
        New-Item -ItemType Directory -Path $server, $settings -Force |
            Out-Null
        foreach ($path in @(
            (Join-Path $server 'appium-uiautomator2-server-v7.4.1.apk'),
            (Join-Path $server 'appium-uiautomator2-server-debug-androidTest.apk'),
            (Join-Path $settings 'settings_apk-debug.apk')
        )) {
            Set-Content -LiteralPath $path -Value 'trusted apk'
        }
        $invoker = {
            param([string[]]$Arguments)
            [pscustomobject]@{
                ExitCode = $(if (($Arguments -join ' ') -match
                    ' shell pm path io\.appium\.settings$') { 1 } else { 0 })
                Output = ''
            }
        }

        {
            Install-ReplicationAndroidAppiumHelpers `
                -DeviceUdid emulator-5554 `
                -AppiumHome $appiumHome `
                -AdbInvoker $invoker
        } | Should -Throw "*'io.appium.settings' was not installed*"
    }
}

Describe 'Scanning what the run published' {
    BeforeEach {
        $script:ArtifactRoot = Join-Path $script:ScratchRoot ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $script:ArtifactRoot 'evidence') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $script:ArtifactRoot 'verification') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'candidate.json') `
            -Value '{"schemaVersion":1}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'test.patch') `
            -Value "diff --git a/x b/x`n" -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'verification/verification-console.log') `
            -Value 'Passed! - Failed: 1' -Encoding utf8NoBOM
        [IO.File]::WriteAllBytes(
            (Join-Path $script:ArtifactRoot 'evidence/repro.mp4'),
            [byte[]](0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70))
    }

    It 'accepts artifacts with no marker in them' {
        $result = Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot
        $result.ScannedFiles | Should -BeGreaterThan 0
    }

    It 'refuses the run canary in a log' {
        Add-Content -LiteralPath (Join-Path $script:ArtifactRoot 'verification/verification-console.log') `
            -Value "env dump: $script:Canary"
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses the run canary in a patch' {
        Add-Content -LiteralPath (Join-Path $script:ArtifactRoot 'test.patch') `
            -Value "+// $script:Canary"
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses the run canary in a JSON document' {
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'candidate.json') `
            -Value ('{"note":"' + $script:Canary + '"}') -Encoding utf8NoBOM
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses a real credential shape the canary would never have covered' {
        foreach ($case in @(
            @{ Text = 'token ghp_0123456789abcdefghijklmnopqrstuv'; Code = 'github-pat' },
            @{ Text = 'token github_pat_11ABCDEFG0123456789_abcdef'; Code = 'github-fine-grained-pat' },
            @{ Text = 'http.extraheader=AUTHORIZATION: basic eHg6Z2hwX2FiY2RlZmdoaWprbG1ub3A='; Code = 'git-extraheader' },
            @{ Text = 'remote https://x-access-token:ghp_abcdefghijkl@github.com/o/r'; Code = 'url-userinfo-credential' },
            @{ Text = 'AccountKey=abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGH'; Code = 'azure-storage-key' },
            @{ Text = 'https://s.blob.core.windows.net/c?sv=2021&sig=abcdefghijklmnopqrstuvwx'; Code = 'azure-sas' }
        )) {
            (Get-ReplicationSecretMarkerMatch -Text $case.Text).Code |
                Should -Be $case.Code -Because "'$($case.Code)' must be recognised"
        }
    }

    It 'refuses a link planted in the artifact tree' -Skip:([System.OperatingSystem]::IsWindows()) {
        $outside = Join-Path $script:ScratchRoot 'outside.txt'
        Set-Content -LiteralPath $outside -Value 'x' -Encoding utf8NoBOM
        & ln -s $outside (Join-Path $script:ArtifactRoot 'linked.txt')
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw '*found a link*'
    }

    It 'does not report noise from a media file' {
        # Scanning an MP4 for token shapes finds coincidences and proves
        # nothing, so binaries are skipped by extension rather than by guesswork.
        [IO.File]::WriteAllBytes(
            (Join-Path $script:ArtifactRoot 'evidence/preview.gif'),
            [byte[]](1..255))
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } | Should -Not -Throw
    }
}
