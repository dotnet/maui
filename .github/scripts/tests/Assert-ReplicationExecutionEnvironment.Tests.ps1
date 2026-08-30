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
            'PATH', 'HOME', 'TMPDIR', 'DOTNET_ROOT', 'DOTNET_NOLOGO',
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
        $script:Target = Join-Path $script:ScratchRoot 'isolated-target.ps1'
        $script:IsolationPlanRepo = Join-Path $script:ScratchRoot 'isolation-plan-repo'
        New-Item -ItemType Directory -Path $script:IsolationPlanRepo -Force | Out-Null
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
    }

    It 'uses a Linux cgroup firewall with only loopback allowed' {
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
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
        $command.Arguments | Should -Contain '--property=ProtectHome=yes'
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
        $command.Arguments | Should -Contain '--property=RestrictAddressFamilies=AF_UNIX'
        $command.Arguments | Should -Contain '--property=UnsetEnvironment=XDG_RUNTIME_DIR DBUS_SESSION_BUS_ADDRESS'
        ($command.Arguments -join "`n") | Should -Not -Match 'MAUI_REPLICATION_EGRESS_ISOLATED'
        ($command.Arguments -join "`n") | Should -Not -Match '--setenv=XDG_RUNTIME_DIR='
        ($command.Arguments -join "`n") | Should -Match 'BindPaths=.*?/nuget-packages'
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

    It 'grants adb only to trusted device-control phases' {
        $command = Get-ReplicationNetworkIsolatedCommand `
            -Platform android `
            -RepositoryRoot $script:IsolationPlanRepo `
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
    It 'installs and verifies IPv4 and IPv6 output filters and removes default routes' {
        $calls = [Collections.Generic.List[string]]::new()
        $installed = @{}
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            $calls.Add($text)
            if ($text -match 'settings get global airplane_mode_on$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '1' }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -I OUTPUT ') {
                $installed[$Matches['tool']] = $true
            }
            if ($text -match ' (?<tool>ip6tables|iptables) -C OUTPUT ' -and
                -not $installed.ContainsKey($Matches['tool'])) {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '' }
        }.GetNewClosure()

        $result = Assert-ReplicationAndroidGuestNetworkIsolation `
            -DeviceUdid emulator-5554 `
            -AdbInvoker $invoker

        $result.NewConnectionsDenied | Should -BeTrue
        $joined = $calls -join "`n"
        $joined | Should -Match 'iptables -A MAUI_REPLICATION -j REJECT'
        $joined | Should -Match 'ip6tables -A MAUI_REPLICATION -j REJECT'
        $joined | Should -Match 'iptables -A MAUI_REPLICATION -o lo -j RETURN'
        $joined | Should -Match 'ip -4 route show default'
        $joined | Should -Match 'ip -6 route show default'
        $joined | Should -Match 'connectivity airplane-mode enable'
        $joined | Should -Match 'svc wifi disable'
        $joined | Should -Match 'svc data disable'
    }

    It 'fails closed if a guest default route remains' {
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            if ($text -match 'settings get global airplane_mode_on$') {
                return [pscustomobject]@{ ExitCode = 0; Output = '1' }
            }
            if ($text -match ' route show default$') {
                return [pscustomobject]@{ ExitCode = 0; Output = 'default via 10.0.2.2 dev eth0' }
            }
            if ($text -match ' (?:ip6tables|iptables) -C OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 0; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '' }
        }

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker
        } | Should -Throw '*left an IPv4 default route*'
    }

    It 'fails closed after execution if the guest firewall was removed' {
        $invoker = {
            param([string[]]$Arguments)
            $text = $Arguments -join ' '
            if ($text -match ' (?:ip6tables|iptables) -C OUTPUT ') {
                return [pscustomobject]@{ ExitCode = 1; Output = '' }
            }
            return [pscustomobject]@{ ExitCode = 0; Output = '1' }
        }

        {
            Assert-ReplicationAndroidGuestNetworkIsolation `
                -DeviceUdid emulator-5554 `
                -AdbInvoker $invoker `
                -VerifyOnly
        } | Should -Throw '*lost the iptables OUTPUT chain*'
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
