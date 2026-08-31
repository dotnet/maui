function Read-ReplicationApplePlist {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Description
    )

    if ([Text.Encoding]::UTF8.GetByteCount($Content) -gt 128KB) {
        throw "$Description is oversized."
    }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Ignore
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 128KB
    $textReader = [IO.StringReader]::new($Content)
    $reader = $null
    try {
        $reader = [Xml.XmlReader]::Create($textReader, $settings)
        $document = [Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        return $document
    } catch {
        throw "$Description is not a safe, well-formed plist: $($_.Exception.Message)"
    } finally {
        if ($reader) { $reader.Dispose() }
        $textReader.Dispose()
    }
}

function Get-ReplicationAppleEntitlementMap {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Xml.XmlDocument]$Document,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $dicts = @($Document.SelectNodes('/plist/dict'))
    if ($dicts.Count -ne 1) {
        throw "$Description must contain exactly one root dictionary."
    }
    $children = @($dicts[0].ChildNodes | Where-Object {
            $_.NodeType -eq [Xml.XmlNodeType]::Element
        })
    if ($children.Count % 2 -ne 0) {
        throw "$Description has a malformed entitlement dictionary."
    }
    $result = [ordered]@{}
    for ($index = 0; $index -lt $children.Count; $index += 2) {
        $keyNode = $children[$index]
        $valueNode = $children[$index + 1]
        if ($keyNode.LocalName -cne 'key' -or
            [string]::IsNullOrWhiteSpace($keyNode.InnerText) -or
            $result.Contains([string]$keyNode.InnerText)) {
            throw "$Description has an invalid or duplicate entitlement key."
        }
        $result[[string]$keyNode.InnerText] = $valueNode
    }
    return $result
}

function Assert-ReplicationMacCatalystEntitlementDocument {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Xml.XmlDocument]$Document,
        [Parameter(Mandatory = $true)][string]$Description,
        [switch]$SourceTemplate
    )

    $entitlements = Get-ReplicationAppleEntitlementMap `
        -Document $Document `
        -Description $Description
    $sandbox = $entitlements['com.apple.security.app-sandbox']
    if ($null -eq $sandbox -or $sandbox.LocalName -cne 'true') {
        throw "$Description must enable com.apple.security.app-sandbox."
    }

    foreach ($name in @($entitlements.Keys)) {
        if (
            $name -match '^com\.apple\.security\.network\.' -or
            $name -match '^com\.apple\.security\.temporary-exception\.' -or
            $name -match '^com\.apple\.security\.exception\.' -or
            $name -in @(
                'com.apple.security.application-groups',
                'com.apple.security.cs.allow-dyld-environment-variables',
                'com.apple.security.cs.disable-library-validation',
                'com.apple.security.cs.disable-executable-page-protection',
                'com.apple.security.cs.allow-unsigned-executable-memory',
                'com.apple.security.automation.apple-events',
                'com.apple.security.files.all',
                'com.apple.security.device.audio-input',
                'com.apple.security.device.camera',
                'com.apple.security.device.usb',
                'keychain-access-groups'
            )
        ) {
            throw "$Description contains forbidden entitlement '$name'."
        }
    }
    if ($SourceTemplate -and $entitlements.Count -ne 1) {
        throw "$Description may declare only com.apple.security.app-sandbox."
    }

    return [pscustomobject]@{
        AppSandbox = $true
        EntitlementCount = $entitlements.Count
    }
}

function Assert-ReplicationMacCatalystEntitlements {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = [IO.Path]::GetFullPath($Path)
    $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or $item.Length -gt 128KB) {
        throw "Mac Catalyst replication entitlements must be a bounded regular file: $fullPath"
    }
    $document = Read-ReplicationApplePlist `
        -Content ([IO.File]::ReadAllText($fullPath)) `
        -Description "Mac Catalyst replication entitlements '$fullPath'"
    return Assert-ReplicationMacCatalystEntitlementDocument `
        -Document $document `
        -Description "Mac Catalyst replication entitlements '$fullPath'" `
        -SourceTemplate
}

function Resolve-ReplicationAppleExistingPath {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not [OperatingSystem]::IsMacOS()) {
        return [IO.Path]::GetFullPath($Path)
    }
    $resolved = [string](& /bin/realpath -- ([IO.Path]::GetFullPath($Path)) 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($resolved)) {
        throw "Unable to resolve Apple path '$Path'."
    }
    return $resolved
}

function Assert-ReplicationSignedMacCatalystAppSandbox {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$AppPath)

    if (-not [OperatingSystem]::IsMacOS()) {
        throw 'Mac Catalyst signed-app validation requires macOS.'
    }
    $fullPath = Resolve-ReplicationAppleExistingPath -Path $AppPath
    $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if (-not $item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Extension -cne '.app') {
        throw "Mac Catalyst replication app must be a regular .app bundle: $fullPath"
    }
    & /usr/bin/codesign --verify --deep --strict $fullPath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Mac Catalyst replication app failed strict code-signature validation."
    }
    $entitlementOutput = @(
        & /usr/bin/codesign -d --entitlements :- $fullPath 2>&1
    )
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to read Mac Catalyst replication app entitlements.'
    }
    $combinedOutput = $entitlementOutput -join "`n"
    $plistStart = $combinedOutput.IndexOf('<?xml', [StringComparison]::Ordinal)
    if ($plistStart -lt 0) {
        $plistStart = $combinedOutput.IndexOf('<plist', [StringComparison]::Ordinal)
    }
    $plistEnd = $combinedOutput.LastIndexOf('</plist>', [StringComparison]::Ordinal)
    if ($plistStart -lt 0 -or $plistEnd -lt $plistStart) {
        throw 'Mac Catalyst replication app carries no readable entitlements.'
    }
    $content = $combinedOutput.Substring(
        $plistStart,
        $plistEnd + '</plist>'.Length - $plistStart)
    $document = Read-ReplicationApplePlist `
        -Content $content `
        -Description "Signed entitlements in '$fullPath'"
    return Assert-ReplicationMacCatalystEntitlementDocument `
        -Document $document `
        -Description "Signed entitlements in '$fullPath'"
}

function Initialize-ReplicationAppleSandboxNativeMethods {
    if ('ReplicationAppleSandboxNativeMethods' -as [type]) {
        return
    }
    Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;

public static class ReplicationAppleSandboxNativeMethods
{
    [DllImport("/usr/lib/libsandbox.1.dylib", CharSet = CharSet.Ansi)]
    public static extern int sandbox_check(
        int processId,
        string operation,
        int filterType);
}
'@ -ErrorAction Stop | Out-Null
}

function Assert-ReplicationMacCatalystProcessNetworkDenied {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][string]$ExpectedExecutablePath
    )

    if (-not [OperatingSystem]::IsMacOS()) {
        throw 'Mac Catalyst runtime sandbox validation requires macOS.'
    }

    $expectedPath = Resolve-ReplicationAppleExistingPath `
        -Path $ExpectedExecutablePath
    $Process.Refresh()
    if ($Process.HasExited -or
        (Resolve-ReplicationAppleExistingPath -Path $Process.Path) -cne
            $expectedPath) {
        throw 'Mac Catalyst replication process does not match the audited app executable.'
    }
    Initialize-ReplicationAppleSandboxNativeMethods
    $networkCheck = -1
    for ($attempt = 1; $attempt -le 20; $attempt++) {
        $Process.Refresh()
        if ($Process.HasExited) {
            throw ("Mac Catalyst replication process $($Process.Id) exited " +
                'before its sandbox boundary could be proven.')
        }
        $networkCheck = [ReplicationAppleSandboxNativeMethods]::sandbox_check(
            $Process.Id,
            'network-outbound',
            0)
        if ($networkCheck -gt 0) {
            $Process.Refresh()
            if ($Process.HasExited -or
                (Resolve-ReplicationAppleExistingPath -Path $Process.Path) -cne
                    $expectedPath) {
                throw 'Mac Catalyst replication process exited or changed identity during sandbox validation.'
            }
            return $true
        }
        if ($attempt -lt 20) {
            Start-Sleep -Milliseconds 250
        }
    }
    if ($networkCheck -lt 0) {
        throw "Mac Catalyst replication could not query outbound-network policy for process $($Process.Id)."
    }
    throw "Mac Catalyst replication process $($Process.Id) is permitted outbound networking."
}

function Start-ReplicationMacCatalystAppSandbox {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$AppPath,
        [ValidateRange(1, 120)][int]$TimeoutSeconds = 30
    )

    $fullPath = Resolve-ReplicationAppleExistingPath -Path $AppPath
    $null = Assert-ReplicationSignedMacCatalystAppSandbox -AppPath $fullPath
    $infoPlist = Join-Path $fullPath 'Contents/Info.plist'
    $executableName = [string](
        & /usr/libexec/PlistBuddy -c 'Print :CFBundleExecutable' $infoPlist 2>$null)
    if ($LASTEXITCODE -ne 0 -or
        $executableName -cnotmatch '^[A-Za-z0-9._-]{1,200}$') {
        throw 'Mac Catalyst replication app has no valid bundle executable.'
    }
    $executablePath = Resolve-ReplicationAppleExistingPath `
        -Path (Join-Path $fullPath "Contents/MacOS/$executableName")
    if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf) -or
        (Get-Item -LiteralPath $executablePath -Force).Attributes -band
            [IO.FileAttributes]::ReparsePoint) {
        throw 'Mac Catalyst replication executable is missing or linked.'
    }
    $before = [Collections.Generic.HashSet[int]]::new()
    foreach ($process in @(Get-Process -Name $executableName `
                -ErrorAction SilentlyContinue)) {
        try {
            if (-not $process.HasExited -and
                (Resolve-ReplicationAppleExistingPath -Path $process.Path) -ceq
                    $executablePath) {
                [void]$before.Add($process.Id)
            }
        } catch {
        } finally {
            $process.Dispose()
        }
    }

    & /usr/bin/open -n $fullPath
    if ($LASTEXITCODE -ne 0) {
        throw 'LaunchServices failed to start the Mac Catalyst replication app.'
    }
    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        foreach ($process in @(Get-Process -Name $executableName `
                    -ErrorAction SilentlyContinue)) {
            try {
                if (-not $process.HasExited -and
                    -not $before.Contains($process.Id) -and
                    (Resolve-ReplicationAppleExistingPath -Path $process.Path) -ceq
                        $executablePath) {
                    $null = Assert-ReplicationMacCatalystProcessNetworkDenied `
                        -Process $process `
                        -ExpectedExecutablePath $executablePath
                    return $process
                }
            } catch {
                if (-not $process.HasExited -and
                    -not $before.Contains($process.Id)) {
                    $process.Kill($true)
                    [void]$process.WaitForExit(10000)
                }
                $process.Dispose()
                throw
            }
            $process.Dispose()
        }
        Start-Sleep -Milliseconds 250
    }
    throw 'Mac Catalyst replication app did not start inside its audited bundle.'
}

function Get-ReplicationMacCatalystAppIdentity {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$AppPath)

    $fullPath = Resolve-ReplicationAppleExistingPath -Path $AppPath
    $null = Assert-ReplicationSignedMacCatalystAppSandbox -AppPath $fullPath
    $infoPlist = Join-Path $fullPath 'Contents/Info.plist'
    if (-not (Test-Path -LiteralPath $infoPlist -PathType Leaf) -or
        (Get-Item -LiteralPath $infoPlist -Force).Attributes -band
            [IO.FileAttributes]::ReparsePoint) {
        throw 'Mac Catalyst replication app Info.plist is missing or linked.'
    }
    $executableName = [string](
        & /usr/libexec/PlistBuddy -c 'Print :CFBundleExecutable' $infoPlist 2>$null)
    $bundleIdentifier = [string](
        & /usr/libexec/PlistBuddy -c 'Print :CFBundleIdentifier' $infoPlist 2>$null)
    if ($LASTEXITCODE -ne 0 -or
        $executableName -cnotmatch '^[A-Za-z0-9._-]{1,200}$' -or
        $bundleIdentifier -cnotmatch '^[A-Za-z0-9](?:[A-Za-z0-9.-]{0,198}[A-Za-z0-9])?$') {
        throw 'Mac Catalyst replication app has invalid bundle identity.'
    }
    $executablePath = Resolve-ReplicationAppleExistingPath `
        -Path (Join-Path $fullPath "Contents/MacOS/$executableName")
    $executable = Get-Item -LiteralPath $executablePath -Force -ErrorAction Stop
    if ($executable.PSIsContainer -or
        $executable.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Mac Catalyst replication executable is missing or linked.'
    }
    return [pscustomobject]@{
        AppPath = $fullPath
        BundleIdentifier = $bundleIdentifier
        ExecutablePath = $executablePath
    }
}

function Stop-ReplicationMacCatalystAppSandbox {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$AppPath)

    $identity = Get-ReplicationMacCatalystAppIdentity -AppPath $AppPath
    $processName = [IO.Path]::GetFileName($identity.ExecutablePath)
    $stopped = 0
    foreach ($process in @(Get-Process -Name $processName `
                -ErrorAction SilentlyContinue)) {
        try {
            if (-not $process.HasExited -and
                (Resolve-ReplicationAppleExistingPath -Path $process.Path) -ceq
                    $identity.ExecutablePath) {
                $process.Kill($true)
                if (-not $process.WaitForExit(10000)) {
                    throw "Mac Catalyst replication process $($process.Id) did not exit during cleanup."
                }
                $stopped++
            }
        } catch {
            throw
        } finally {
            $process.Dispose()
        }
    }
    return $stopped
}

function Assert-ReplicationAppleRegularResultFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Description,
        [ValidateRange(1, 67108864)][long]$MaximumLength = 16777216
    )

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or $item.Length -gt $MaximumLength) {
        throw "$Description must be a bounded regular file."
    }
    return $item
}

function Invoke-ReplicationMacCatalystDeviceTests {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$AppPath,
        [Parameter(Mandatory = $true)][string]$IncludeClasses,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [ValidateRange(1, 7200)][int]$TimeoutSeconds
    )

    if (-not [OperatingSystem]::IsMacOS()) {
        throw 'Mac Catalyst replication device tests require macOS.'
    }
    $classSelectors = @($IncludeClasses -split ',')
    if ([Text.Encoding]::UTF8.GetByteCount($IncludeClasses) -gt 32768 -or
        $classSelectors.Count -gt 64 -or
        @($classSelectors | Where-Object {
            $_ -cnotmatch '^[A-Za-z_][A-Za-z0-9_.+`]*$'
        }).Count -gt 0) {
        throw 'Mac Catalyst replication received an invalid class selector.'
    }

    $identity = Get-ReplicationMacCatalystAppIdentity -AppPath $AppPath
    $userProfile = [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::UserProfile)
    $containerDirectory = Join-Path $userProfile (
        "Library/Containers/$($identity.BundleIdentifier)")
    $containerRoot = Join-Path $containerDirectory 'Data'
    $resultPath = Join-Path $containerRoot (
        'Documents/.config/TestResults.xUnit.xml')
    foreach ($directory in @(
        $containerDirectory,
        $containerRoot,
        (Join-Path $containerRoot 'Documents'),
        (Join-Path $containerRoot 'Documents/.config')
    )) {
        if (Test-Path -LiteralPath $directory) {
            $item = Get-Item -LiteralPath $directory -Force -ErrorAction Stop
            if (-not $item.PSIsContainer -or
                $item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "Mac Catalyst result path contains a linked component: $directory"
            }
        }
    }
    if (Test-Path -LiteralPath $resultPath) {
        $priorResult = Get-Item -LiteralPath $resultPath -Force `
            -ErrorAction Stop
        if ($priorResult.PSIsContainer -or
            $priorResult.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Prior Mac Catalyst device-test result is not a regular file.'
        }
        Remove-Item -LiteralPath $resultPath -Force -ErrorAction Stop
    }

    $runId = [guid]::NewGuid().ToString('N')
    $fullOutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
    New-Item -ItemType Directory -Path $fullOutputDirectory -Force |
        Out-Null
    $logPath = Join-Path $fullOutputDirectory (
        "maccatalyst-device-$runId.log")
    $copiedResultPath = Join-Path $fullOutputDirectory (
        "xunit-test-$runId.xml")

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $identity.ExecutablePath
    $startInfo.WorkingDirectory = Split-Path -Parent $identity.ExecutablePath
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment.Clear()
    $startInfo.Environment['NUNIT_AUTOSTART'] = 'true'
    $startInfo.Environment['NUNIT_AUTOEXIT'] = 'true'
    $startInfo.Environment['NUNIT_ENABLE_NETWORK'] = 'false'
    $startInfo.Environment['NUNIT_ENABLE_XML_OUTPUT'] = 'false'
    $startInfo.Environment['NUNIT_XML_VERSION'] = 'xUnit'
    $startInfo.Environment['NUNIT_SKIPPED_CLASSES'] = $IncludeClasses
    $startInfo.Environment['DISABLE_SYSTEM_PERMISSION_TESTS'] = 'true'

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $standardOutputTask = $null
    $standardErrorTask = $null
    $standardOutput = ''
    $standardError = ''
    $started = $false
    $processExitCode = $null
    $runStartedUtc = [DateTime]::UtcNow
    try {
        if (-not $process.Start()) {
            throw 'Failed to start the Mac Catalyst replication device-test app.'
        }
        $started = $true
        $standardOutputTask = $process.StandardOutput.ReadToEndAsync()
        $standardErrorTask = $process.StandardError.ReadToEndAsync()
        $null = Assert-ReplicationMacCatalystProcessNetworkDenied `
            -Process $process `
            -ExpectedExecutablePath $identity.ExecutablePath
        $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
        while (-not $process.HasExited -and
            [DateTimeOffset]::UtcNow -lt $deadline) {
            Start-Sleep -Milliseconds 250
        }
        if (-not $process.HasExited) {
            throw "Mac Catalyst replication device tests exceeded $TimeoutSeconds seconds."
        }
        $processExitCode = $process.ExitCode
    } finally {
        if ($started -and -not $process.HasExited) {
            $process.Kill($true)
            [void]$process.WaitForExit(10000)
        }
        if ($standardOutputTask) {
            $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
        }
        if ($standardErrorTask) {
            $standardError = $standardErrorTask.GetAwaiter().GetResult()
        }
        $process.Dispose()
        $combinedLog = $standardOutput
        if (-not [string]::IsNullOrWhiteSpace($standardError)) {
            $combinedLog += [Environment]::NewLine + $standardError
        }
        [IO.File]::WriteAllText(
            $logPath,
            $combinedLog,
            [Text.UTF8Encoding]::new($false))
    }

    try {
        $result = Assert-ReplicationAppleRegularResultFile `
            -Path $resultPath `
            -Description 'Mac Catalyst device-test result'
    } catch {
        if ($null -ne $processExitCode -and $processExitCode -ne 0) {
            throw ("Mac Catalyst replication device-test app exited with code " +
                "$processExitCode without a valid fresh result. $($_.Exception.Message)")
        }
        throw
    }
    if ($result.LastWriteTimeUtc -lt $runStartedUtc.AddSeconds(-2)) {
        throw 'Mac Catalyst device-test result predates this run.'
    }
    Copy-Item -LiteralPath $resultPath -Destination $copiedResultPath `
        -ErrorAction Stop
    $null = Assert-ReplicationAppleRegularResultFile `
        -Path $copiedResultPath `
        -Description 'Copied Mac Catalyst device-test result'
    if ($null -ne $processExitCode -and $processExitCode -ne 0) {
        Write-Warning (
            "Mac Catalyst device-test app exited with code $processExitCode after " +
            'writing a valid fresh result; using the verified result file.')
    }
    return [pscustomobject]@{
        ResultFile = $copiedResultPath
        LogFile = $logPath
        BundleIdentifier = $identity.BundleIdentifier
        Boundary = 'mac-catalyst-app-sandbox'
    }
}

function Get-ReplicationAppleSingleArgumentValue {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $indexes = @(
        for ($index = 0; $index -lt $Arguments.Count; $index++) {
            if ($Arguments[$index] -ceq $Name) { $index }
        }
    )
    if ($indexes.Count -ne 1 -or
        $indexes[0] + 1 -ge $Arguments.Count -or
        $Arguments[$indexes[0] + 1].StartsWith(
            '-',
            [StringComparison]::Ordinal)) {
        throw "Apple replication requires exactly one value for '$Name'."
    }
    return $Arguments[$indexes[0] + 1]
}

function Get-ReplicationAppleIsolatedCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('ios', 'catalyst')][string]$Platform,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$ScriptPath,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][object[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Environment,
        [ValidateSet('macos')][string]$OperatingSystem = $(if (
            [OperatingSystem]::IsMacOS()) {
            'macos'
        } else {
            throw 'Apple replication requires a macOS host.'
        })
    )

    if ($Platform -eq 'ios' -and
        [Environment]::GetEnvironmentVariable(
            'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED') -cne '1') {
        throw ('Unsupported replication scenario: iOS Simulator replication requires ' +
            'an Aces host/hypervisor egress boundary; app permissions and host pf are insufficient.')
    }

    $trustedRootPath = [IO.Path]::GetFullPath($TrustedRoot)
    $trustedRootItem = Get-Item -LiteralPath $trustedRootPath -Force `
        -ErrorAction Stop
    if (-not $trustedRootItem.PSIsContainer -or
        $trustedRootItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Apple replication trusted root must be a regular directory.'
    }
    $trustedScriptPath = [IO.Path]::GetFullPath($ScriptPath)
    $allowedScripts = [ordered]@{
        (Join-Path $trustedRootPath 'scripts/BuildAndRunSandbox.ps1') = 'sandbox'
        (Join-Path $trustedRootPath 'scripts/shared/Record-Reproduction.ps1') = 'record'
        (Join-Path $trustedRootPath 'scripts/shared/Invoke-ReplicationTestVerification.ps1') = 'verify'
    }
    $kind = ''
    foreach ($candidate in $allowedScripts.Keys) {
        if ($trustedScriptPath -ceq [IO.Path]::GetFullPath($candidate)) {
            $kind = [string]$allowedScripts[$candidate]
            break
        }
    }
    if ([string]::IsNullOrWhiteSpace($kind) -or
        -not (Test-Path -LiteralPath $trustedScriptPath -PathType Leaf) -or
        (Get-Item -LiteralPath $trustedScriptPath -Force).Attributes -band
            [IO.FileAttributes]::ReparsePoint) {
        throw 'Apple replication host execution is limited to exact trusted runners.'
    }
    if (@($Arguments | Where-Object { $null -eq $_ }).Count -gt 0) {
        throw 'Apple replication command contains a null argument.'
    }
    $values = @($Arguments | ForEach-Object { [string]$_ })
    if ($values.Count -gt 64 -or
        @($values | Where-Object {
            $_.Length -gt 4096 -or $_.IndexOf([char]0) -ge 0 -or
            $_ -match '[\r\n]'
        }).Count -gt 0) {
        throw 'Apple replication command contains unsafe arguments.'
    }
    $argumentPlatform = Get-ReplicationAppleSingleArgumentValue `
        -Arguments $values `
        -Name '-Platform'
    if ($argumentPlatform -cne $Platform) {
        throw 'Apple replication trusted runner received a different platform.'
    }
    switch ($kind) {
        'sandbox' {
            if (@($values | Where-Object {
                    $_ -ceq '-EnforceNetworkIsolation'
                }).Count -ne 1) {
                throw 'Apple Sandbox execution requires its bounded app boundary.'
            }
        }
        'record' {
            $reproductionScriptPath =
                Get-ReplicationAppleSingleArgumentValue `
                    -Arguments $values `
                    -Name '-ReproductionScriptPath'
            $payload = Get-ReplicationAppleSingleArgumentValue `
                -Arguments $values `
                -Name '-ReproductionArgumentsPayload'
            if ([IO.Path]::GetFullPath($reproductionScriptPath) -cne
                    [IO.Path]::GetFullPath(
                        (Join-Path $trustedRootPath 'scripts/BuildAndRunSandbox.ps1'))) {
                throw 'Apple recording may replay only the trusted Sandbox runner.'
            }
            try {
                $payloadJson = [Text.UTF8Encoding]::new($false, $true).GetString(
                    [Convert]::FromBase64String($payload))
                $decodedArguments = ConvertFrom-Json `
                    -InputObject $payloadJson `
                    -NoEnumerate
                $nested = @(
                    $decodedArguments |
                        ForEach-Object { [string]$_ })
            } catch {
                throw 'Apple recording replay arguments are malformed.'
            }
            $nestedPlatform = Get-ReplicationAppleSingleArgumentValue `
                -Arguments $nested `
                -Name '-Platform'
            if (@($nested | Where-Object {
                    $_ -ceq '-EnforceNetworkIsolation'
                }).Count -ne 1 -or $nestedPlatform -cne $Platform) {
                throw 'Apple recording replay dropped its bounded app boundary.'
            }
        }
        'verify' {
            $testType = Get-ReplicationAppleSingleArgumentValue `
                -Arguments $values `
                -Name '-TestType'
            if ($testType -cne 'DeviceTest') {
                throw 'Apple replication permits only sandboxed device tests.'
            }
        }
    }

    return [pscustomobject]@{
        FilePath = (Get-Command pwsh -ErrorAction Stop).Source
        Arguments = @(
            '-NoLogo',
            '-NoProfile',
            '-NonInteractive',
            '-File', $trustedScriptPath
        ) + $values
        Environment = $Environment
        UnitName = ''
        Boundary = if ($Platform -eq 'catalyst') {
            'mac-catalyst-app-sandbox'
        } else {
            'aces-hypervisor'
        }
    }
}
