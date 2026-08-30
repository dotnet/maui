#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and enforces the explicit environment allowlist for generated code.

.DESCRIPTION
    Generated Sandbox sources, generated tests, generated fixes, and everything
    they start (MSBuild, the test runner, Appium, adb, simctl, node) run on the
    same agent as the credentials that publish the result. Clearing a named list
    of tokens is not enough: it is a denylist, and a denylist is a promise to
    have thought of everything, which is how `VSS_NUGET_EXTERNAL_FEED_ENDPOINTS`
    and `GIT_ASKPASS` reach a grandchild process nobody was watching.

    So the child environment is constructed rather than filtered. Only names on
    the allowlist survive -- the ones a build and a device run genuinely need --
    and everything else is simply not passed on. The assertion that follows is
    a second, independent check on the constructed set, so a mistake in the
    allowlist is caught rather than trusted.

    Nothing here has a broad catch or a silent fallback. A name that cannot be
    classified is not carried "just in case"; it is dropped, and a forbidden
    name that survives is an error.
#>

Set-StrictMode -Version Latest

# The tracer. It is deliberately not a secret: its value is derived from public
# build coordinates so any job can recompute it, and its only job is to be
# something that must never appear in a child process, an artifact, a log, a
# patch, or a JSON document. A denylist can only prove the absence of what it
# lists; the canary proves the mechanism itself is working.
$script:ReplicationSecretCanaryName = 'MAUI_REPLICATION_SECRET_CANARY'
$script:ReplicationSecretCanaryPrefix = 'maui-replication-secret-canary-'

# Exact names that must never reach generated execution.
$script:ReplicationForbiddenEnvironmentNames = @(
    'GH_TOKEN',
    'GITHUB_TOKEN',
    'GH_COMMENT_TOKEN',
    'GH_ENTERPRISE_TOKEN',
    'GITHUB_ENTERPRISE_TOKEN',
    'COPILOT_GITHUB_TOKEN',
    'COPILOT_TOKEN',
    'SYSTEM_ACCESSTOKEN',
    'ACCESSTOKEN',
    'MAUI_REPLICATION_SECRET_CANARY',
    'MAUI_REPLICATION_EGRESS_ISOLATED',
    'GIT_ASKPASS',
    'SSH_ASKPASS',
    'GIT_CONFIG',
    'GIT_CONFIG_GLOBAL',
    'GIT_CONFIG_SYSTEM',
    'GIT_CONFIG_COUNT',
    'GIT_CONFIG_PARAMETERS',
    'GIT_CREDENTIAL_HELPER',
    'GIT_HTTP_EXTRAHEADER',
    'GIT_PROXY_COMMAND',
    'GIT_SSH_COMMAND',
    'HTTP_PROXY',
    'HTTPS_PROXY',
    'ALL_PROXY',
    'FTP_PROXY',
    'NO_PROXY',
    'http_proxy',
    'https_proxy',
    'all_proxy',
    'ftp_proxy',
    'no_proxy',
    'VSS_NUGET_EXTERNAL_FEED_ENDPOINTS',
    'VSS_NUGET_URI_PREFIXES',
    'VSS_NUGET_ACCESSTOKEN',
    'NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED',
    'NUGET_PLUGIN_PATHS',
    'ARTIFACTS_CREDPROVIDER_TOKEN',
    'DOTNET_TOKEN',
    'AGENT_PROXYURL',
    'AGENT_PROXYUSERNAME',
    'AGENT_PROXYPASSWORD'
)

# Name shapes that must never reach generated execution. Anything matching is
# refused whatever else it is, so a new Azure or feed secret is covered the day
# it is introduced rather than the day somebody remembers to list it.
$script:ReplicationForbiddenEnvironmentPatterns = @(
    '^AZURE_',
    '^AZUREAD_',
    '^MSI_',
    '^IDENTITY_',
    '^ENDPOINT_AUTH',
    '^SYSTEM_OIDCREQUESTURI',
    '(?:TOKEN|SECRET|PASSWORD|PASSWD|CREDENTIAL|CREDENTIALS|APIKEY|PRIVATEKEY|CLIENTSECRET|SASTOKEN)$',
    '_(?:TOKEN|SECRET|PASSWORD|PASSWD|CREDENTIAL|CREDENTIALS|APIKEY|API_KEY|KEY|PRIVATE_KEY|CLIENT_SECRET|SAS|PAT)$'
)

# Exact names a build and a device run genuinely need. Everything not here, and
# not matched by a prefix below, is dropped.
$script:ReplicationAllowedEnvironmentNames = @(
    'PATH',
    'Path',
    'HOME',
    'USERPROFILE',
    'HOMEDRIVE',
    'HOMEPATH',
    'TMPDIR',
    'TEMP',
    'TMP',
    'LANG',
    'LC_ALL',
    'LC_CTYPE',
    'SHELL',
    'TERM',
    'USER',
    'LOGNAME',
    'PWD',
    'PATHEXT',
    'COMSPEC',
    'SYSTEMROOT',
    'SYSTEMDRIVE',
    'WINDIR',
    'PROCESSOR_ARCHITECTURE',
    'NUMBER_OF_PROCESSORS',
    'LOCALAPPDATA',
    'APPDATA',
    'PROGRAMFILES',
    'PROGRAMFILES(X86)',
    'PROGRAMDATA',
    'MSBUILDDISABLENODEREUSE',
    'DOTNET_CLI_HOME',
    'NUGET_PACKAGES',
    'GRADLE_USER_HOME',
    'JAVA_HOME',
    'ANDROID_HOME',
    'ANDROID_SDK_ROOT',
    'ANDROID_AVD_HOME',
    'ANDROID_USER_HOME',
    'ANDROID_EMULATOR_HOME',
    'ADB_TRACE',
    'DEVELOPER_DIR',
    'APPIUM_HOME',
    'NODE_PATH',
    'DISPLAY',
    'DEVICE_UDID',
    'MAUI_REPLICATION_DEVICE_UDID'
)

# Prefixes whose whole family is required. Each is still filtered through the
# forbidden patterns above, so `DOTNET_SOMETHING_TOKEN` does not slip in behind
# `DOTNET_`.
$script:ReplicationAllowedEnvironmentPrefixes = @(
    'DOTNET_',
    'MSBUILD',
    'XHARNESS_',
    'JAVA_TOOL_OPTIONS',
    'ANDROID_SDK_',
    'ANDROID_NDK'
)

function Get-ReplicationSecretCanaryName {
    return $script:ReplicationSecretCanaryName
}

function Get-ReplicationSecretCanaryPrefix {
    return $script:ReplicationSecretCanaryPrefix
}

function Get-ReplicationForbiddenEnvironmentNames {
    return @($script:ReplicationForbiddenEnvironmentNames)
}

function Get-ReplicationForbiddenEnvironmentPatterns {
    return @($script:ReplicationForbiddenEnvironmentPatterns)
}

function Test-ReplicationForbiddenEnvironmentName {
    <#
        .SYNOPSIS
        Returns true when a variable name may never reach generated execution.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Name)) { return $true }
    $upper = $Name.ToUpperInvariant()
    foreach ($forbidden in $script:ReplicationForbiddenEnvironmentNames) {
        if ($upper -eq $forbidden.ToUpperInvariant()) { return $true }
    }
    foreach ($pattern in $script:ReplicationForbiddenEnvironmentPatterns) {
        if ($upper -match $pattern) { return $true }
    }

    return $false
}

function Test-ReplicationAllowedEnvironmentName {
    <#
        .SYNOPSIS
        Returns true when a variable is on the explicit runtime allowlist.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Name
    )

    if (Test-ReplicationForbiddenEnvironmentName -Name $Name) { return $false }
    $upper = $Name.ToUpperInvariant()
    foreach ($allowed in $script:ReplicationAllowedEnvironmentNames) {
        if ($upper -eq $allowed.ToUpperInvariant()) { return $true }
    }
    foreach ($prefix in $script:ReplicationAllowedEnvironmentPrefixes) {
        if ($upper.StartsWith($prefix.ToUpperInvariant(), [System.StringComparison]::Ordinal)) { return $true }
    }

    return $false
}

function Get-ReplicationExecutionEnvironment {
    <#
        .SYNOPSIS
        Builds the exact environment a generated-code child process may see.

        .DESCRIPTION
        `Inherited` is the environment to start from -- the current process by
        default. `Additional` carries values the trusted caller has decided the
        child needs and that are not inheritable, and it is filtered by the same
        rules, so a caller cannot reintroduce a forbidden name by passing it
        explicitly.
    #>
    [CmdletBinding()]
    param(
        [System.Collections.IDictionary]$Inherited,
        [System.Collections.IDictionary]$Additional
    )

    if ($null -eq $Inherited) {
        $Inherited = [System.Environment]::GetEnvironmentVariables()
    }

    $result = [ordered]@{}
    foreach ($key in @($Inherited.Keys)) {
        $name = [string]$key
        if (-not (Test-ReplicationAllowedEnvironmentName -Name $name)) { continue }
        $value = [string]$Inherited[$key]
        if ($null -eq $value) { continue }
        # A permitted name carrying the tracer is still the tracer. Refusing
        # here is what proves the allowlist is the only path in.
        if ($value.Contains($script:ReplicationSecretCanaryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }
        $result[$name] = $value
    }

    if ($null -ne $Additional) {
        foreach ($key in @($Additional.Keys)) {
            $name = [string]$key
            if (Test-ReplicationForbiddenEnvironmentName -Name $name) {
                throw "A trusted caller may not add a forbidden variable to generated execution: $name"
            }
            $value = [string]$Additional[$key]
            if ($null -eq $value) { continue }
            if ($value.Contains($script:ReplicationSecretCanaryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "A trusted caller may not add canary-bearing content to generated execution: $name"
            }
            $result[$name] = $value
        }
    }

    return $result
}

function Assert-ReplicationExecutionEnvironment {
    <#
        .SYNOPSIS
        Independently re-checks a constructed child environment, fail-closed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][System.Collections.IDictionary]$Environment,
        [string]$Context = 'generated execution'
    )

    $violations = [System.Collections.Generic.List[string]]::new()
    foreach ($key in @($Environment.Keys)) {
        $name = [string]$key
        if (Test-ReplicationForbiddenEnvironmentName -Name $name) {
            $violations.Add($name)
            continue
        }
        if (-not (Test-ReplicationAllowedEnvironmentName -Name $name)) {
            $violations.Add($name)
            continue
        }
        $value = [string]$Environment[$key]
        if ($value -and $value.Contains($script:ReplicationSecretCanaryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            $violations.Add($name)
        }
    }

    if ($violations.Count -gt 0) {
        $detail = (@($violations | Sort-Object -CaseSensitive -Unique | Select-Object -First 12) -join ', ')
        throw "Generated execution environment ($Context) carries variables it may not: $detail"
    }

    return $true
}

function Assert-ReplicationOutboundNetworkIsolation {
    <#
        .SYNOPSIS
        Proves a job-level egress boundary is active before generated code runs.

        .DESCRIPTION
        The replication pools must install an OS or hypervisor boundary that
        denies outbound traffic while preserving only loopback. A pool
        capability alone is not evidence, so this
        assertion also performs independent DNS, direct TCP, and HTTP probes.
        Any successful probe, missing capability, or probe error fails closed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$TrustedRoot = '',
        [scriptblock]$DnsProbe,
        [scriptblock]$TcpProbe,
        [scriptblock]$HttpProbe
    )

    if ([Environment]::GetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED') -cne '1') {
        throw "Generated execution for '$Platform' requires a verified job-level outbound-network deny boundary."
    }

    if ($Platform -ceq 'android') {
        $manifestPath = if (-not [string]::IsNullOrWhiteSpace($TrustedRoot)) {
            Join-Path $TrustedRoot 'source-overrides/ReplicationNetworkIsolationManifest.xml'
        } else {
            Join-Path $RepositoryRoot (
                'src/Controls/samples/Controls.Sample.Sandbox/Platforms/Android/ReplicationNetworkIsolationManifest.xml')
        }
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
            throw 'Android replication network isolation manifest is missing.'
        }
        $manifest = [IO.File]::ReadAllText($manifestPath)
        if ($manifest -notmatch '(?is)<uses-permission\s+android:name="android\.permission\.INTERNET"\s+tools:node="remove"\s*/>') {
            throw 'Android replication must remove INTERNET permission at the package boundary.'
        }
    }

    if ($null -eq $DnsProbe) {
        $DnsProbe = {
            $udp = $null
            try {
                $udp = [Net.Sockets.UdpClient]::new()
                $udp.Client.ReceiveTimeout = 1500
                $udp.Connect('1.1.1.1', 53)
                $id = [BitConverter]::GetBytes([uint16](Get-Random -Minimum 1 -Maximum 65535))
                if ([BitConverter]::IsLittleEndian) { [Array]::Reverse($id) }
                $query = [Collections.Generic.List[byte]]::new()
                $query.AddRange($id)
                $query.AddRange([byte[]](0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00))
                foreach ($label in @('maui-egress-probe', 'invalid')) {
                    $bytes = [Text.Encoding]::ASCII.GetBytes($label)
                    $query.Add([byte]$bytes.Length)
                    $query.AddRange($bytes)
                }
                $query.AddRange([byte[]](0x00, 0x00, 0x01, 0x00, 0x01))
                [void]$udp.Send($query.ToArray(), $query.Count)
                $remote = [Net.IPEndPoint]::new([Net.IPAddress]::Any, 0)
                $response = $udp.Receive([ref]$remote)
                $null -ne $response -and $response.Length -gt 0
            } catch {
                $false
            } finally {
                if ($null -ne $udp) { $udp.Dispose() }
            }
        }
    }
    if ($null -eq $TcpProbe) {
        $TcpProbe = {
            $client = $null
            try {
                $client = [Net.Sockets.TcpClient]::new()
                $task = $client.ConnectAsync('169.254.169.254', 80)
                $task.Wait(1500) -and $client.Connected
            } catch {
                $false
            } finally {
                if ($null -ne $client) { $client.Dispose() }
            }
        }
    }
    if ($null -eq $HttpProbe) {
        $HttpProbe = {
            $handler = [Net.Http.HttpClientHandler]::new()
            $handler.UseProxy = $false
            $client = [Net.Http.HttpClient]::new($handler)
            $client.Timeout = [TimeSpan]::FromSeconds(2)
            try {
                $response = $client.GetAsync('http://1.1.1.1/').GetAwaiter().GetResult()
                $response.Dispose()
                $true
            } catch {
                $false
            } finally {
                $client.Dispose()
                $handler.Dispose()
            }
        }
    }

    foreach ($probe in @(
        @{ Name = 'DNS'; Run = $DnsProbe },
        @{ Name = 'direct metadata TCP'; Run = $TcpProbe },
        @{ Name = 'HTTP'; Run = $HttpProbe }
    )) {
        try {
            $escaped = & $probe.Run
        } catch {
            throw "Generated execution network-isolation $($probe.Name) probe failed closed."
        }
        if ($escaped -ne $false) {
            throw "Generated execution network isolation allowed $($probe.Name) egress."
        }
    }

    return [pscustomobject]@{
        Platform = $Platform
        Boundary = 'job'
        DnsDenied = $true
        DirectTcpDenied = $true
        HttpDenied = $true
    }
}

function Assert-ReplicationAndroidGuestNetworkIsolation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$DeviceUdid,
        [scriptblock]$AdbInvoker,
        [switch]$VerifyOnly
    )

    if ([string]::IsNullOrWhiteSpace($DeviceUdid) -or
        $DeviceUdid -notmatch '^[A-Za-z0-9._:-]{1,128}$') {
        throw 'Android guest network isolation requires a validated device identifier.'
    }
    if ($null -eq $AdbInvoker) {
        $AdbInvoker = {
            param([string[]]$Arguments)
            $output = @(& adb @Arguments 2>&1)
            [pscustomobject]@{
                ExitCode = $LASTEXITCODE
                Output = ($output | ForEach-Object { [string]$_ }) -join "`n"
            }
        }
    }

    $invoke = {
        param(
            [string[]]$Arguments,
            [string]$Description,
            [switch]$AllowFailure
        )
        $result = & $AdbInvoker $Arguments
        if ($null -eq $result) {
            throw "Android guest network isolation could not $Description."
        }
        if ([int]$result.ExitCode -ne 0 -and -not $AllowFailure) {
            throw "Android guest network isolation could not $Description."
        }
        return $result
    }

    if (-not $VerifyOnly) {
        & $invoke @('-s', $DeviceUdid, 'root') 'restart adb as root' | Out-Null
        & $invoke @('-s', $DeviceUdid, 'wait-for-device') 'wait for the rooted emulator' | Out-Null
        & $invoke @('-s', $DeviceUdid, 'shell', 'cmd', 'connectivity', 'airplane-mode', 'enable') `
            'enable airplane mode' | Out-Null
        & $invoke @('-s', $DeviceUdid, 'shell', 'svc', 'wifi', 'disable') `
            'disable guest Wi-Fi' | Out-Null
        & $invoke @('-s', $DeviceUdid, 'shell', 'svc', 'data', 'disable') `
            'disable guest mobile data' | Out-Null
    }

    $chain = 'MAUI_REPLICATION'
    foreach ($tool in @('iptables', 'ip6tables')) {
        $jump = & $invoke @(
            '-s', $DeviceUdid, 'shell', $tool, '-C', 'OUTPUT', '-j', $chain
        ) "inspect the $tool OUTPUT chain" -AllowFailure
        if ([int]$jump.ExitCode -ne 0) {
            if ($VerifyOnly) {
                throw "Android guest network isolation lost the $tool OUTPUT chain."
            }
            & $invoke @(
                '-s', $DeviceUdid, 'shell', $tool, '-N', $chain
            ) "create the $tool isolation chain" | Out-Null
            & $invoke @(
                '-s', $DeviceUdid, 'shell', $tool, '-A', $chain,
                '-o', 'lo', '-j', 'RETURN'
            ) "allow $tool loopback" | Out-Null
            & $invoke @(
                '-s', $DeviceUdid, 'shell', $tool, '-A', $chain, '-j', 'REJECT'
            ) "deny new $tool guest egress" | Out-Null
            & $invoke @(
                '-s', $DeviceUdid, 'shell', $tool, '-I', 'OUTPUT', '1', '-j', $chain
            ) "install the $tool isolation chain" | Out-Null
        }
        foreach ($rule in @(
            @('-C', 'OUTPUT', '-j', $chain),
            @('-C', $chain, '-o', 'lo', '-j', 'RETURN'),
            @('-C', $chain, '-j', 'REJECT')
        )) {
            & $invoke (@('-s', $DeviceUdid, 'shell', $tool) + $rule) `
                "verify the $tool isolation rules" | Out-Null
        }
    }

    foreach ($family in @('-4', '-6')) {
        for ($attempt = 0; $attempt -lt $(if ($VerifyOnly) { 1 } else { 4 }); $attempt++) {
            $routes = (& $invoke @(
                '-s', $DeviceUdid, 'shell', 'ip', $family, 'route', 'show', 'default'
            ) 'inspect guest routes').Output
            if ([string]::IsNullOrWhiteSpace([string]$routes)) { break }
            if ($VerifyOnly) { break }
            & $invoke @(
                '-s', $DeviceUdid, 'shell', 'ip', $family, 'route', 'del', 'default'
            ) 'remove a guest default route' | Out-Null
        }
        $remaining = (& $invoke @(
            '-s', $DeviceUdid, 'shell', 'ip', $family, 'route', 'show', 'default'
        ) 'verify guest routes').Output
        if (-not [string]::IsNullOrWhiteSpace([string]$remaining)) {
            throw "Android guest network isolation left an IPv$($family.TrimStart('-')) default route."
        }
    }

    $airplane = ([string](& $invoke @(
        '-s', $DeviceUdid, 'shell', 'settings', 'get', 'global', 'airplane_mode_on'
    ) 'verify airplane mode').Output).Trim()
    if ($airplane -cne '1') {
        throw 'Android guest network isolation could not prove airplane mode.'
    }

    return [pscustomobject]@{
        Platform = 'android'
        AirplaneMode = $true
        DefaultRoutesRemoved = $true
        NewConnectionsDenied = $true
    }
}

function Get-ReplicationNetworkIsolatedCommand {
    <#
        .SYNOPSIS
        Builds the fail-closed process command for generated execution.

        .DESCRIPTION
        Android on Linux runs the complete host process tree in a systemd
        egress-denied cgroup and removes the emulator's guest default routes.
        The other lanes do not currently provide a trustworthy process-tree and
        app boundary, so they are explicitly withheld instead of running
        generated code with a best-effort proxy or process-name rule.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$ScriptPath,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Arguments,
        [Parameter(Mandatory = $true)][System.Collections.IDictionary]$Environment,
        [Parameter(Mandatory = $true)][ValidateCount(1, 4)][string[]]$WritableRoots,
        [switch]$AllowDeviceControl,
        [string]$DeviceUdid = '',
        [ValidateRange(1, 10800)][int]$TimeoutSeconds = 1800,
        [ValidateSet('linux', 'macos', 'windows')]
        [string]$OperatingSystem = $(if ([OperatingSystem]::IsLinux()) {
            'linux'
        } elseif ([OperatingSystem]::IsMacOS()) {
            'macos'
        } elseif ([OperatingSystem]::IsWindows()) {
            'windows'
        } else {
            throw 'Generated execution is unsupported on this operating system.'
        }),
        [string]$UserId = '',
        [string]$GroupId = ''
    )

    $validateHostTools = -not $PSBoundParameters.ContainsKey('OperatingSystem')
    if ($Platform -ne 'android') {
        throw "$Platform replication is withheld because this pool has no enforceable process-tree and app outbound-network deny boundary."
    }
    if ($Platform -ceq 'android' -and $OperatingSystem -cne 'linux') {
        throw 'Android replication requires the Linux network-isolated runner.'
    }
    if ($DeviceUdid -notmatch '^emulator-[0-9]{4,6}$') {
        throw 'Android replication requires a local emulator transport; network-connected devices are not permitted.'
    }
    $wrapperPath = Join-Path $PSScriptRoot 'Invoke-ReplicationNetworkIsolatedProcess.ps1'
    if (-not (Test-Path -LiteralPath $wrapperPath -PathType Leaf)) {
        throw 'Trusted replication network-isolation wrapper is missing.'
    }
    $trustedRootPath = [IO.Path]::GetFullPath($TrustedRoot)
    $trustedRootItem = Get-Item -LiteralPath $trustedRootPath -Force -ErrorAction Stop
    if (-not $trustedRootItem.PSIsContainer -or
        $trustedRootItem.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $trustedRootPath -ceq [IO.Path]::GetPathRoot($trustedRootPath) -or
        $trustedRootPath -ceq [Environment]::GetFolderPath(
            [Environment+SpecialFolder]::UserProfile)) {
        throw 'Generated execution requires a regular trusted root directory.'
    }
    $trustedRootPrefix = $trustedRootPath.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar
    ) + [IO.Path]::DirectorySeparatorChar
    $trustedScriptPath = [IO.Path]::GetFullPath($ScriptPath)
    if (-not $trustedScriptPath.StartsWith(
            $trustedRootPrefix,
            [StringComparison]::Ordinal)) {
        throw 'Generated execution script must remain inside the trusted root.'
    }
    $wrapperDirectory = [IO.Path]::GetFullPath($PSScriptRoot)
    $wrapperInsideTrustedRoot = $wrapperDirectory.StartsWith(
        $trustedRootPrefix,
        [StringComparison]::Ordinal)
    if ($validateHostTools -and -not $wrapperInsideTrustedRoot) {
        throw 'The network-isolation wrapper must execute from the trusted root.'
    }
    $argumentJson = ConvertTo-Json -InputObject @($Arguments) -Compress -Depth 4
    $argumentPayload = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($argumentJson))
    $wrapperArguments = @(
        '-NoLogo', '-NoProfile', '-NonInteractive',
        '-File', $wrapperPath,
        '-Platform', $Platform,
        '-RepositoryRoot', $RepositoryRoot,
        '-ScriptPath', $ScriptPath,
        '-ArgumentPayload', $argumentPayload,
        '-DeviceUdid', $DeviceUdid,
        '-AllowDeviceControl', $AllowDeviceControl.IsPresent.ToString().ToLowerInvariant()
    )

    if ($validateHostTools) {
        foreach ($required in @('/usr/bin/sudo', '/usr/bin/systemd-run')) {
            if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
                throw "Linux replication network isolation requires $required."
            }
        }
    }
    if ([string]::IsNullOrWhiteSpace($UserId)) {
        $UserId = ([string](& /usr/bin/id -u)).Trim()
    }
    if ([string]::IsNullOrWhiteSpace($GroupId)) {
        $GroupId = ([string](& /usr/bin/id -g)).Trim()
    }
    if ($UserId -notmatch '^\d+$' -or $GroupId -notmatch '^\d+$') {
        throw 'Linux replication network isolation could not resolve the agent uid/gid.'
    }
    $unitBaseName = "maui-replication-$PID-$([guid]::NewGuid().ToString('N'))"
    $unitName = "$unitBaseName.service"

    $inaccessiblePaths = @(
        "-/run/user/$UserId",
        '-/usr/bin/sudo',
        '-/bin/sudo',
        '-/usr/bin/systemd-run',
        '-/bin/systemd-run',
        '-/usr/bin/systemctl',
        '-/bin/systemctl',
        '-/usr/bin/busctl',
        '-/usr/bin/dbus-send',
        '-/run/docker.sock',
        '-/var/run/docker.sock',
        '-/run/containerd/containerd.sock',
        '-/run/podman/podman.sock',
        '-/run/buildkit/buildkitd.sock',
        '-/usr/bin/docker',
        '-/usr/bin/podman',
        '-/usr/bin/ctr',
        '-/usr/bin/nerdctl',
        '-/usr/bin/buildctl'
    )
    if (-not $AllowDeviceControl) {
        $inaccessiblePaths += @(
            '-/usr/bin/adb',
            '-/usr/local/bin/adb',
            '-/bin/adb'
        )
        foreach ($androidHomeName in @('ANDROID_HOME', 'ANDROID_SDK_ROOT')) {
            $androidHome = [string]$Environment[$androidHomeName]
            if (-not [string]::IsNullOrWhiteSpace($androidHome) -and
                [IO.Path]::IsPathRooted($androidHome)) {
                $inaccessiblePaths += "-$(Join-Path $androidHome 'platform-tools/adb')"
            }
        }
    }
    $gitMarker = Join-Path $RepositoryRoot '.git'
    $gitDirectory = $gitMarker
    if (Test-Path -LiteralPath $gitMarker -PathType Leaf) {
        $markerText = [IO.File]::ReadAllText($gitMarker).Trim()
        if ($markerText -notmatch '^gitdir:\s*(?<path>.+)$') {
            throw 'Generated execution found a malformed linked-worktree .git file.'
        }
        $gitDirectoryValue = $Matches['path']
        $gitDirectory = if ([IO.Path]::IsPathRooted($gitDirectoryValue)) {
            [IO.Path]::GetFullPath($gitDirectoryValue)
        } else {
            [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $gitDirectoryValue))
        }
        $readOnlyPaths = [Collections.Generic.List[string]]::new()
        $readOnlyPaths.Add($gitMarker)
    } else {
        $readOnlyPaths = [Collections.Generic.List[string]]::new()
    }
    if (Test-Path -LiteralPath $gitDirectory -PathType Container) {
        $readOnlyPaths.Add($gitDirectory)
        $inaccessiblePaths += "-$(Join-Path $gitDirectory 'hooks')"
        $commonDirMarker = Join-Path $gitDirectory 'commondir'
        if (Test-Path -LiteralPath $commonDirMarker -PathType Leaf) {
            $commonRelative = [IO.File]::ReadAllText($commonDirMarker).Trim()
            $commonDirectory = if ([IO.Path]::IsPathRooted($commonRelative)) {
                [IO.Path]::GetFullPath($commonRelative)
            } else {
                [IO.Path]::GetFullPath((Join-Path $gitDirectory $commonRelative))
            }
            if (Test-Path -LiteralPath $commonDirectory -PathType Container) {
                $readOnlyPaths.Add($commonDirectory)
                $inaccessiblePaths += "-$(Join-Path $commonDirectory 'hooks')"
            }
        }
        foreach ($relative in @(
            'config',
            'config.worktree',
            'HEAD',
            'objects',
            'refs',
            'packed-refs',
            'info'
        )) {
            $candidate = Join-Path $gitDirectory $relative
            if (Test-Path -LiteralPath $candidate) {
                $readOnlyPaths.Add($candidate)
            }
        }
    }

    $systemdArguments = @(
        '-n',
        '/usr/bin/systemd-run',
        '--quiet',
        '--wait',
        '--pipe',
        '--collect',
        "--unit=$unitBaseName",
        "--uid=$UserId",
        "--gid=$GroupId",
        "--working-directory=$RepositoryRoot",
        "--property=RuntimeMaxSec=$($TimeoutSeconds)s",
        '--property=TimeoutStopSec=15s',
        '--property=KillMode=control-group',
        '--property=IPAddressDeny=any',
        '--property=IPAddressAllow=localhost',
        '--property=NoNewPrivileges=yes',
        '--property=CapabilityBoundingSet=',
        '--property=RestrictSUIDSGID=yes',
        '--property=RestrictNamespaces=yes',
        '--property=PrivateTmp=yes',
        '--property=ProtectSystem=strict',
        '--property=ProtectHome=tmpfs',
        '--property=ProtectControlGroups=yes',
        '--property=ProtectKernelModules=yes',
        '--property=ProtectKernelTunables=yes',
        '--property=ProtectProc=invisible',
        '--property=ProcSubset=pid',
        '--property=LockPersonality=yes',
        '--property=RestrictRealtime=yes',
        '--property=RemoveIPC=yes',
        '--property=SupplementaryGroups=',
        "--property=InaccessiblePaths=$($inaccessiblePaths -join ' ')",
        '--property=UnsetEnvironment=XDG_RUNTIME_DIR DBUS_SESSION_BUS_ADDRESS'
    )
    if (-not $AllowDeviceControl) {
        # Host-only unit/XAML phases need no Appium or adb connection. A private
        # network namespace prevents raw loopback protocol access to the host's
        # adb server even if generated code bypasses every API scan.
        $systemdArguments += '--property=PrivateNetwork=yes'
    }
    $systemdArguments += "--property=BindReadOnlyPaths=$trustedRootPath"
    if (-not $wrapperInsideTrustedRoot) {
        # Unit tests load this helper outside their fake trusted root. Production
        # stages the helper under TrustedRoot, so this extra bind is normally
        # unnecessary but keeps command construction directly testable.
        $systemdArguments += "--property=BindReadOnlyPaths=$wrapperDirectory"
    }
    if ($readOnlyPaths.Count -gt 0) {
        $systemdArguments += "--property=ReadOnlyPaths=$($readOnlyPaths -join ' ')"
    }
    foreach ($writableRoot in @($WritableRoots | Sort-Object -Unique)) {
        $fullWritableRoot = [IO.Path]::GetFullPath($writableRoot)
        if (-not (Test-Path -LiteralPath $fullWritableRoot -PathType Container) -or
            $fullWritableRoot -ceq [IO.Path]::GetPathRoot($fullWritableRoot) -or
            $fullWritableRoot -ceq [Environment]::GetFolderPath(
                [Environment+SpecialFolder]::UserProfile)) {
            throw "Generated execution received an unsafe writable root: $writableRoot"
        }
        $systemdArguments += "--property=BindPaths=$fullWritableRoot"
    }
    foreach ($candidate in @(
        $Environment['APPIUM_HOME'],
        $Environment['ANDROID_AVD_HOME'],
        $Environment['ANDROID_USER_HOME'],
        $Environment['GRADLE_USER_HOME'],
        $Environment['DOTNET_CLI_HOME'],
        $Environment['NUGET_PACKAGES']
    )) {
        if (-not [string]::IsNullOrWhiteSpace([string]$candidate) -and
            [IO.Path]::IsPathRooted([string]$candidate) -and
            (Test-Path -LiteralPath ([string]$candidate) -PathType Container)) {
            $systemdArguments += "--property=BindPaths=$([IO.Path]::GetFullPath([string]$candidate))"
        }
    }
    foreach ($name in @($Environment.Keys | Sort-Object)) {
        $systemdArguments += "--setenv=$name=$([string]$Environment[$name])"
    }
    $systemdArguments += @('--', (Get-Command pwsh -ErrorAction Stop).Source)
    $systemdArguments += $wrapperArguments

    return [pscustomobject]@{
        FilePath = '/usr/bin/sudo'
        Arguments = $systemdArguments
        Environment = $Environment
        Boundary = 'systemd-cgroup-loopback-only'
        UnitName = $unitName
    }
}

function Stop-ReplicationNetworkIsolationUnit {
    <#
    .SYNOPSIS
        Stops and verifies the transient systemd unit that owns generated work.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$UnitName,
        [scriptblock]$SystemctlInvoker
    )

    if ($UnitName -notmatch '^maui-replication-[0-9]+-[0-9a-f]{32}\.service$') {
        throw 'Generated execution cleanup received an invalid systemd unit name.'
    }
    if ($null -eq $SystemctlInvoker) {
        $SystemctlInvoker = {
            param([string[]]$Arguments)
            $output = @(& /usr/bin/sudo -n /usr/bin/systemctl @Arguments 2>&1)
            [pscustomobject]@{
                ExitCode = $LASTEXITCODE
                Output = ($output | ForEach-Object { [string]$_ }) -join "`n"
            }
        }
    }

    $null = & $SystemctlInvoker @('stop', $UnitName)
    $active = & $SystemctlInvoker @('is-active', '--quiet', $UnitName)
    if ([int]$active.ExitCode -eq 0) {
        $null = & $SystemctlInvoker @(
            'kill', '--kill-whom=all', '--signal=KILL', $UnitName)
        $active = & $SystemctlInvoker @('is-active', '--quiet', $UnitName)
    }
    if ([int]$active.ExitCode -eq 0) {
        throw "Generated execution systemd unit remained active after cleanup: $UnitName"
    }
}

function Get-ReplicationSecretMarkerPatterns {
    <#
        .SYNOPSIS
        Returns the patterns that must never appear in a published artifact.

        .DESCRIPTION
        The canary proves the stripping mechanism works. The credential shapes
        beside it catch the case the canary cannot: a real token that reached an
        artifact by some path the canary never travelled.
    #>
    return @(
        [pscustomobject]@{ Code = 'canary'; Pattern = [regex]::Escape($script:ReplicationSecretCanaryPrefix) },
        [pscustomobject]@{ Code = 'github-pat'; Pattern = '\b(?:ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{20,}' },
        [pscustomobject]@{ Code = 'github-fine-grained-pat'; Pattern = '\bgithub_pat_[A-Za-z0-9_]{20,}' },
        [pscustomobject]@{ Code = 'git-extraheader'; Pattern = '(?i)AUTHORIZATION:\s*(?:basic|bearer)\s+\S{16,}' },
        [pscustomobject]@{ Code = 'url-userinfo-credential'; Pattern = '(?i)https?://[^/\s:@]+:[^/\s@]{8,}@' },
        [pscustomobject]@{ Code = 'azure-storage-key'; Pattern = '(?i)AccountKey=[A-Za-z0-9+/]{40,}' },
        [pscustomobject]@{ Code = 'azure-sas'; Pattern = '(?i)[?&]sig=[A-Za-z0-9%+/]{24,}' }
    )
}

function Get-ReplicationSecretMarkerMatch {
    <#
        .SYNOPSIS
        Returns the first secret marker found in text, or $null.
    #>
    [CmdletBinding()]
    param(
        [AllowEmptyString()][AllowNull()][string]$Text
    )

    if ([string]::IsNullOrEmpty($Text)) { return $null }
    foreach ($marker in (Get-ReplicationSecretMarkerPatterns)) {
        if ($Text -match $marker.Pattern) {
            return [pscustomobject]@{ Code = [string]$marker.Code }
        }
    }

    return $null
}

function Assert-ReplicationNoSecretMarkers {
    <#
        .SYNOPSIS
        Fails when any file under a root carries a secret or canary marker.

        .DESCRIPTION
        Artifacts, logs, patches, and JSON are all read as text with a size
        bound; media and other binaries are skipped by extension rather than by
        guesswork, because scanning an MP4 for token shapes reports noise and
        proves nothing. The scan is bounded and its refusal names the file and
        the marker class, never the matched value.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [string]$Context = 'replication artifacts',
        [int]$MaximumFileCount = 4000,
        [long]$MaximumFileBytes = 8MB
    )

    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        throw "Secret marker scan root does not exist: $Root"
    }

    $binaryExtensions = @(
        '.mp4', '.gif', '.png', '.jpg', '.jpeg', '.webp', '.mov', '.zip',
        '.gz', '.tgz', '.dll', '.exe', '.so', '.dylib', '.pdb', '.nupkg',
        '.apk', '.aab', '.ipa', '.app', '.bin', '.ttf', '.otf', '.ico'
    )

    $scanned = 0
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Force -ErrorAction Stop)) {
        if ($file.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
            throw "Secret marker scan found a link ($Context): $($file.FullName)"
        }
        if ($binaryExtensions -contains $file.Extension.ToLowerInvariant()) { continue }
        if ($file.Length -gt $MaximumFileBytes) { continue }
        $scanned++
        if ($scanned -gt $MaximumFileCount) {
            throw "Secret marker scan exceeded $MaximumFileCount files ($Context)."
        }

        $text = [System.IO.File]::ReadAllText($file.FullName)
        $match = Get-ReplicationSecretMarkerMatch -Text $text
        if ($match) {
            throw ("Secret marker '$($match.Code)' found in a replication artifact ($Context): " +
                (Split-Path -Leaf $file.FullName))
        }
    }

    return [pscustomobject]@{ ScannedFiles = $scanned; Context = $Context }
}
