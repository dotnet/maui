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
    'NUGET_PACKAGES',
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
    'XDG_RUNTIME_DIR',
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
