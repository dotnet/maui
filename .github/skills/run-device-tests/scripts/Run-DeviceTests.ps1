<#
.SYNOPSIS
    Builds and runs native MAUI tests using dotnet test and genuine MTP TRX.
.DESCRIPTION
    An explicitly supplied device is borrowed, never deleted. Otherwise the runner
    provisions and cleans up its own emulator/simulator. Windows supports either
    unpackaged execution or an owned signed MSIX activated with its real identity.
.PARAMETER ProjectPath
    Explicit native test project inside RepoRoot, instead of a known Project alias.
.PARAMETER TargetFramework
    Optional explicit platform TFM. Defaults to the checkout's MAUI TFM.
#>
[CmdletBinding(DefaultParameterSetName = 'KnownProject')]
param(
    [Parameter(Mandatory, Position = 0, ParameterSetName = 'KnownProject')]
    [ValidateSet('Controls', 'Core', 'Essentials', 'Graphics', 'BlazorWebView')]
    [string] $Project,
    [Parameter(Mandatory, ParameterSetName = 'ProjectPath')]
    [string] $ProjectPath,
    [ValidatePattern('^net[0-9]+\.[0-9]+-(android|ios|maccatalyst|windows)[0-9.]*$')]
    [string] $TargetFramework,
    [ValidateSet('ios', 'maccatalyst', 'android', 'windows')]
    [string] $Platform,
    [string] $iOSVersion,
    [ValidatePattern('^[A-Za-z0-9_-]+$')]
    [string] $Configuration = 'Release',
    [switch] $Rebuild,
    [string] $TestFilter,
    [string] $IncludeClasses,
    [string] $IncludeMethods,
    [switch] $BuildOnly,
    [string] $OutputDirectory = 'artifacts/log',
    [string] $Timeout = '01:00:00',
    [string] $DeviceUdid,
    [switch] $SkipXcodeVersionCheck,
    [ValidateSet('unpackaged', 'packaged')]
    [string] $WindowsPackaging = 'unpackaged',
    [string] $RepoRoot,
    [string] $DotnetPath
)

$ErrorActionPreference = 'Stop'

function ConvertTo-AzdoSafeConsole {
    param([string] $Text)
    return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
}

function Invoke-WithoutGhTokens {
    param([scriptblock] $Action)
    $saved = @{}
    try {
        foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
            $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
            [Environment]::SetEnvironmentVariable($name, $null, 'Process')
        }
        & $Action
    }
    finally {
        foreach ($name in $saved.Keys) {
            [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process')
        }
    }
}

function ConvertTo-DeviceTestClassFilterValue {
    param([string] $Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return '' }
    if ($Value.Length -gt 32768 -or $Value -match '[\x00-\x1F\x7F]') {
        throw 'Device test class filter is too long or contains a control character.'
    }
    $values = @($Value -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -Unique)
    if ($values.Count -gt 64) { throw 'Device test class filter contains more than 64 classes.' }
    foreach ($name in $values) {
        if ($name.Length -gt 512 -or $name -notmatch '^[A-Za-z_][A-Za-z0-9_]*(?:[.+][A-Za-z_][A-Za-z0-9_]*)+$') {
            throw "Invalid fully qualified device test class '$name'."
        }
    }
    return $values -join ','
}

function ConvertTo-DeviceTestMethodFilterValue {
    param([string] $Value, [string] $Classes)
    if ([string]::IsNullOrWhiteSpace($Value)) { return '' }
    if ($Value.Length -gt 32768 -or $Value -match '[\x00-\x1F\x7F]') {
        throw 'Device test method filter is too long or contains a control character.'
    }
    $values = @($Value -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -Unique)
    if ($values.Count -gt 64) { throw 'Device test method filter contains more than 64 methods.' }
    $methods = [Collections.Generic.List[string]]::new()
    foreach ($name in $values) {
        if ($name.Length -gt 1024 -or $name -notmatch '^[A-Za-z_][A-Za-z0-9_]*(?:[.+][A-Za-z_][A-Za-z0-9_]*)*$') {
            throw "Invalid device test method '$name'."
        }
        if ($name.Contains('.')) {
            $methods.Add($name)
        }
        else {
            if (!$Classes) { throw 'Simple method names require -IncludeClasses.' }
            foreach ($class in $Classes -split ',') { $methods.Add("$class.$name") }
        }
    }
    return @($methods | Select-Object -Unique) -join ','
}

function ConvertTo-DeviceTestCategoryFilter {
    param([string] $Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return '' }
    if ($Value -match '^Category=([A-Za-z0-9_.+-]+)$' -or
        $Value -match '^SkipCategories=([A-Za-z0-9_.+-]+)(,[A-Za-z0-9_.+-]+)*$') {
        return $Value
    }
    throw 'TestFilter must be Category=X or SkipCategories=X,Y,Z.'
}

function ConvertTo-MSBuildPropertyValue {
    param([string] $Value)
    return $Value.Replace('%', '%25').Replace(';', '%3B').Replace(',', '%2C')
}

function Invoke-DeviceDotnet {
    param(
        [string] $FilePath,
        [string[]] $Arguments,
        [string] $WorkingDirectory,
        [string] $LogPath,
        [int] $TimeoutSeconds
    )
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $FilePath
    $start.WorkingDirectory = $WorkingDirectory
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    $started = $false
    try {
        $null = Invoke-WithoutGhTokens {
            foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
                $null = $start.Environment.Remove($name)
            }
            if ([IO.Path]::GetFileName($FilePath) -in @('dotnet', 'dotnet.exe')) {
                $start.Environment['DOTNET_ROOT'] = Split-Path $FilePath -Parent
                $start.Environment['DOTNET_HOST_PATH'] = $FilePath
            }
            $process.Start()
        }
        $started = $true
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $timedOut = !$process.WaitForExit($TimeoutSeconds * 1000)
        if ($timedOut) {
            $process.Kill($true)
            if (!$process.WaitForExit(30000)) { throw 'Could not stop the timed-out dotnet test process tree.' }
        }
        if (!$stdout.Wait(30000) -or !$stderr.Wait(30000)) {
            throw 'The test process exited but its diagnostic streams did not close.'
        }
        $output = $stdout.GetAwaiter().GetResult() + [Environment]::NewLine + $stderr.GetAwaiter().GetResult()
        [IO.File]::WriteAllText($LogPath, $output)
        foreach ($line in $output -split '\r?\n') {
            if ($line) { Write-Host "[dotnet] $(ConvertTo-AzdoSafeConsole $line)" }
        }
        return [pscustomobject]@{ ExitCode = $process.ExitCode; TimedOut = $timedOut }
    }
    finally {
        if ($started -and !$process.HasExited) {
            $process.Kill($true)
            $null = $process.WaitForExit(30000)
        }
        $process.Dispose()
    }
}

function Get-DeviceTestResultSummary {
    param(
        [Parameter(Mandatory)]
        [string[]] $ResultFiles,
        [string] $IncludeClasses,
        [string] $IncludeMethods,
        [switch] $RequireClassIsolation
    )
    if ($ResultFiles.Count -eq 0) { throw 'Device test result files are missing (the target tests did not run).' }
    $classes = @($IncludeClasses -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $methods = @($IncludeMethods -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $summary = @{ Total = 0; Passed = 0; Failed = 0; Skipped = 0; Errors = 0; FailedTests = @() }
    $seenClasses = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $seenMethods = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($path in $ResultFiles) {
        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [Xml.XmlReader]::Create($path, $settings)
        try {
            $document = [Xml.XmlDocument]::new()
            $document.XmlResolver = $null
            $document.Load($reader)
        }
        finally {
            $reader.Dispose()
        }
        if ($document.DocumentElement.LocalName -ne 'TestRun') { throw "Expected genuine TRX in '$path'." }
        $definitions = @{}
        foreach ($test in $document.SelectNodes("/*[local-name()='TestRun']/*[local-name()='TestDefinitions']/*[local-name()='UnitTest']")) {
            $method = $test.SelectSingleNode("*[local-name()='TestMethod']")
            if ($method) {
                $definitions[$test.id] = @{
                    Class = ($method.className -split ',')[0].Trim()
                    Method = ($method.name -replace '\(.*$', '')
                }
            }
        }
        $fileFailed = 0
        $results = $document.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']")
        if ($results.Count -eq 0) { throw "TRX '$path' contains no tests (the target tests did not run)." }
        foreach ($result in $results) {
            $identity = $definitions[$result.testId]
            if (($classes.Count -or $methods.Count) -and !$identity) {
                throw 'TRX lacks the TestMethod identity required to verify target isolation.'
            }
            if ($classes.Count -and $identity.Class -cnotin $classes) {
                throw "Native class filter was not enforced: unrelated class '$($identity.Class)' ran."
            }
            $fullMethod = if ($identity) { "$($identity.Class).$($identity.Method)" } else { '' }
            if ($methods.Count -and $identity.Method -cnotin $methods -and $fullMethod -cnotin $methods) {
                throw "Native method filter was not enforced: unrelated method '$fullMethod' ran."
            }
            $summary.Total++
            switch ($result.outcome) {
                'Passed' { $summary.Passed++ }
                { $_ -in @('NotExecuted', 'Skipped') } { $summary.Skipped++ }
                { $_ -in @('Failed', 'Error', 'Timeout', 'Aborted') } {
                    $summary.Failed++
                    $fileFailed++
                    $summary.FailedTests += ConvertTo-AzdoSafeConsole $result.testName
                }
                default { throw "Unrecognized TRX outcome '$($result.outcome)'." }
            }
            if ($result.outcome -notin @('NotExecuted', 'Skipped') -and $identity) {
                $null = $seenClasses.Add($identity.Class)
                $null = $seenMethods.Add($identity.Method)
                $null = $seenMethods.Add($fullMethod)
            }
        }
        $run = $document.SelectSingleNode("/*[local-name()='TestRun']/*[local-name()='ResultSummary']")
        $counters = if ($run) { $run.SelectSingleNode("*[local-name()='Counters']") } else { $null }
        if (!$run -or $run.outcome -notin @('Completed', 'Passed', 'Failed') -or
            ($run.outcome -eq 'Failed' -and $fileFailed -eq 0) -or
            ($counters -and ([int]$counters.error -gt 0 -or [int]$counters.aborted -gt 0 -or [int]$counters.timeout -gt 0))) {
            $summary.Errors++
        }
    }
    foreach ($class in $classes) {
        if (!$seenClasses.Contains($class)) { throw "Requested class '$class' did not execute (missing or only skipped tests)." }
    }
    foreach ($method in $methods) {
        if (!$seenMethods.Contains($method)) { throw "Requested method '$method' did not execute (missing or only skipped tests)." }
    }
    if ($summary.Passed + $summary.Failed -eq 0) {
        throw 'Device tests did not execute (empty selection or only skipped tests).'
    }
    return $summary
}

$ownedResults = $null
$reportSummary = $null
$exitCode = 1
$locationPushed = $false
$originalAvdHome = $env:ANDROID_AVD_HOME
$windowsPackageState = $null
try {
    $IncludeClasses = ConvertTo-DeviceTestClassFilterValue $IncludeClasses
    $nativeMethods = ConvertTo-DeviceTestMethodFilterValue -Value $IncludeMethods -Classes $IncludeClasses
    $TestFilter = ConvertTo-DeviceTestCategoryFilter $TestFilter
    $duration = [TimeSpan]::Parse($Timeout)
    if ($duration.TotalSeconds -le 0 -or $duration.TotalSeconds -gt 86400) { throw 'Timeout must be between zero and 24 hours.' }
    if (!$Platform) { $Platform = if ($IsWindows) { 'windows' } else { 'ios' } }
    if ($Platform -in @('ios', 'maccatalyst') -and !$IsMacOS) { throw "$Platform tests require macOS." }
    if ($Platform -eq 'windows' -and !$IsWindows) { throw 'Windows tests require Windows.' }
    if (!$RepoRoot) {
        $RepoRoot = (Get-Location).Path
        while ($RepoRoot -and !(Test-Path (Join-Path $RepoRoot '.git'))) { $RepoRoot = Split-Path $RepoRoot -Parent }
        if (!$RepoRoot) { throw 'Run this script from a MAUI checkout, or supply -RepoRoot.' }
    }
    $RepoRoot = [IO.Path]::GetFullPath($RepoRoot)
    Push-Location $RepoRoot
    $locationPushed = $true
    $OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
    $runDirectory = Join-Path $OutputDirectory "run-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null
    if (!$DotnetPath) {
        $localDotnet = Join-Path $RepoRoot $(if ($IsWindows) { '.dotnet/dotnet.exe' } else { '.dotnet/dotnet' })
        $DotnetPath = if (Test-Path $localDotnet) { $localDotnet } else { (Get-Command dotnet -ErrorAction Stop).Source }
    }
    $DotnetPath = (Get-Command $DotnetPath -ErrorAction Stop).Source
    [xml] $buildProps = Get-Content -LiteralPath (Join-Path $RepoRoot 'Directory.Build.props') -Raw
    $major = $buildProps.SelectSingleNode('/Project/PropertyGroup/_MauiDotNetVersionMajor').InnerText
    $minor = $buildProps.SelectSingleNode('/Project/PropertyGroup/_MauiDotNetVersionMinor').InnerText
    if ($major -notmatch '^\d+$' -or $minor -notmatch '^\d+$') { throw 'Cannot determine the checked-out MAUI target framework.' }
    $tfm = "net$major.$minor"
    $projects = @{
        Controls = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
        Core = 'src/Core/tests/DeviceTests/Core.DeviceTests.csproj'
        Essentials = 'src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj'
        Graphics = 'src/Graphics/tests/DeviceTests/Graphics.DeviceTests.csproj'
        BlazorWebView = 'src/BlazorWebView/tests/DeviceTests/MauiBlazorWebView.DeviceTests.csproj'
    }
    if ($ProjectPath) {
        $projectPath = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($ProjectPath)) {
            $ProjectPath
        } else { Join-Path $RepoRoot $ProjectPath }))
        $rootPrefix = $RepoRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
        if (!$projectPath.StartsWith($rootPrefix, $comparison) -or
            [IO.Path]::GetExtension($projectPath) -ne '.csproj' -or
            !(Test-Path -LiteralPath $projectPath -PathType Leaf)) {
            throw 'ProjectPath must identify an existing native test project inside RepoRoot.'
        }
    } else {
        $projectPath = Join-Path $RepoRoot $projects[$Project]
    }
    $target = if ($TargetFramework) { $TargetFramework } else {
        switch ($Platform) {
            windows { "$tfm-windows10.0.19041.0" }
            default { "$tfm-$Platform" }
        }
    }
    if ($target -notmatch ("^net[0-9]+\.[0-9]+-" + [regex]::Escape($Platform) + '[0-9.]*$')) {
        throw "TargetFramework '$target' does not match platform '$Platform'."
    }
    $architecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
    $rid = switch ($Platform) {
        ios { "iossimulator-$architecture" }
        maccatalyst { "maccatalyst-$architecture" }
        android { "android-$architecture" }
        windows { 'win-x64' }
    }
    if (!$BuildOnly -and $Platform -in @('ios', 'android') -and !$DeviceUdid) {
        $ownedResults = $runDirectory
        $devicePlatform = $Platform
        $api = if ($Platform -eq 'ios') { if ($iOSVersion) { $iOSVersion } else { 'latest' } } else { '33' }
        Invoke-WithoutGhTokens {
            & (Join-Path $PSScriptRoot 'Start-TestDevice.ps1') -Platform $devicePlatform -ApiVersion $api `
                -ResultsDirectory $runDirectory -DeviceDirectory (Join-Path $runDirectory 'device')
        } 6>&1 | ForEach-Object { Write-Host "[device] $(ConvertTo-AzdoSafeConsole "$_")" }
        $state = Get-Content (Join-Path $runDirectory 'device.json') -Raw | ConvertFrom-Json
        $DeviceUdid = $state.Device
    }
    if (!$BuildOnly -and $Platform -in @('ios', 'android') -and !$DeviceUdid) { throw 'An exact device selector is required.' }
    if ($DeviceUdid -and ($DeviceUdid -match '[\x00-\x20\x7F]' -or $DeviceUdid.StartsWith('-'))) { throw 'Invalid device selector.' }
    if (!$BuildOnly -and $Platform -eq 'android') {
        $sdk = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } else { $env:ANDROID_HOME }
        $adb = if ($sdk) { Join-Path $sdk 'platform-tools/adb' } else { (Get-Command adb -ErrorAction Stop).Source }
        $abi = Invoke-WithoutGhTokens { & $adb -s $DeviceUdid shell getprop ro.product.cpu.abi }
        if ($LASTEXITCODE -ne 0) { throw "Cannot query selected Android device '$DeviceUdid'." }
        $rid = switch ("$abi".Trim()) {
            'x86_64' { 'android-x64' }
            'arm64-v8a' { 'android-arm64' }
            'armeabi-v7a' { 'android-arm' }
            'x86' { 'android-x86' }
            default { throw "Unsupported selected Android device ABI '$abi'." }
        }
    }
    $properties = @(
        "-p:MauiTestClasses=$(ConvertTo-MSBuildPropertyValue $IncludeClasses)",
        "-p:MauiTestMethods=$(ConvertTo-MSBuildPropertyValue $nativeMethods)",
        "-p:MauiTestFilter=$(ConvertTo-MSBuildPropertyValue $TestFilter)",
        "-p:RuntimeIdentifier=$rid",
        '-p:TreatWarningsAsErrors=false'
    )
    if ($Platform -eq 'windows') {
        $properties += @('-p:SelfContained=true', '-p:RuntimeIdentifierOverride=win-x64', '-p:UseMonoRuntime=false',
            '-p:IsRidAgnostic=false',
            "-p:MauiTestResultsDirectory=$(ConvertTo-MSBuildPropertyValue $runDirectory)")
        if ($WindowsPackaging -eq 'packaged') {
            $windowsPackageState = Join-Path $runDirectory 'package-state.json'
            $packageTimeout = if ($IncludeClasses) { [Math]::Min(600, [int]$duration.TotalSeconds) } else { [int]$duration.TotalSeconds }
            $session = Invoke-WithoutGhTokens {
                & (Join-Path $PSScriptRoot 'WindowsTestPackage.ps1') -Action Prepare -StatePath $windowsPackageState `
                    -ProjectPath $projectPath -TimeoutSeconds $packageTimeout
            }
            $launcherDirectory = Join-Path $runDirectory 'launcher'
            $launcher = Invoke-DeviceDotnet -FilePath $DotnetPath -WorkingDirectory $RepoRoot -Arguments @(
                'build', (Join-Path $PSScriptRoot 'windows-launcher/WindowsTestLauncher.csproj'),
                '--configuration', 'Release', '--output', $launcherDirectory
            ) -LogPath (Join-Path $runDirectory 'launcher-build.log') -TimeoutSeconds 600
            if ($launcher.TimedOut -or $launcher.ExitCode -ne 0) { throw 'Packaged MTP launcher build failed.' }
            $properties += @('-p:MauiTestPackaged=true', '-p:WindowsPackageType=MSIX', '-p:_MauiDeviceTestUnpackaged=false',
                "-p:ApplicationId=$($session.IdentityName)",
                "-p:MauiTestPackageIdentityName=$($session.IdentityName)",
                "-p:MauiTestPackagePublisher=$($session.Publisher)",
                "-p:PackageCertificateThumbprint=$($session.CertificateThumbprint)",
                '-p:AppxPackageSigningEnabled=true',
                '-p:UapAppxPackageBuildMode=SideloadOnly', '-p:AppxBundle=Never', '-p:ExtraDefineConstants=PACKAGED',
                "-p:AppxPackageDir=$(ConvertTo-MSBuildPropertyValue (Join-Path $runDirectory 'msix/'))",
                "-p:MauiTestLauncherPath=$(ConvertTo-MSBuildPropertyValue (Join-Path $launcherDirectory 'WindowsTestLauncher.dll'))",
                "-p:MauiTestPackageStatePath=$(ConvertTo-MSBuildPropertyValue $windowsPackageState)",
                "-p:MauiTestPackageManifestPath=$(ConvertTo-MSBuildPropertyValue (Join-Path $runDirectory 'Package.appxmanifest'))")
        }
        else { $properties += @('-p:WindowsPackageType=None', '-p:_MauiDeviceTestUnpackaged=true') }
    }
    if ($Platform -in @('ios', 'maccatalyst')) {
        $properties += '-p:CodesignRequireProvisioningProfile=false'
        if ($SkipXcodeVersionCheck) { $properties += '-p:ValidateXcodeVersion=false' }
    }
    if ($Platform -eq 'maccatalyst') { $properties += "-p:MauiTestResultsDirectory=$(ConvertTo-MSBuildPropertyValue $runDirectory)" }
    if ($BuildOnly -or $Rebuild -or $windowsPackageState) {
        $buildArgs = @('build', $projectPath, '--configuration', $Configuration, '--framework', $target) + $properties
        if ($Rebuild) { $buildArgs += '-t:Rebuild' }
        $build = Invoke-DeviceDotnet -FilePath $DotnetPath -Arguments $buildArgs -WorkingDirectory $RepoRoot `
            -LogPath (Join-Path $runDirectory 'build.log') -TimeoutSeconds ([Math]::Max(600, [int]$duration.TotalSeconds))
        if ($build.TimedOut -or $build.ExitCode -ne 0) { throw "Device test build failed (exit $($build.ExitCode), timeout=$($build.TimedOut))." }
        if ($BuildOnly) { Write-Output 'Build completed successfully'; $exitCode = 0 }
    }
    if (!$BuildOnly) {
    if ($windowsPackageState) {
        $null = Invoke-WithoutGhTokens {
            & (Join-Path $PSScriptRoot 'WindowsTestPackage.ps1') -Action Install -StatePath $windowsPackageState `
                -PackageDirectory (Join-Path $runDirectory 'msix')
        }
    }
    $nativePath = $null
    $nativeHash = $null
    if ($Platform -in @('ios', 'android')) {
        [xml] $projectXml = Get-Content -LiteralPath $projectPath -Raw
        $applicationId = $projectXml.SelectSingleNode('/Project/PropertyGroup/ApplicationId').InnerText
        if ($applicationId -notmatch '^[A-Za-z0-9_.-]+$') { throw 'Expected a literal native ApplicationId.' }
    }
    if ($Platform -eq 'android') {
        $androidNativePath = "/sdcard/Android/data/$applicationId/files/TestResults/testResults.trx"
        $checkLog = Join-Path $runDirectory 'android-result-before.log'
        $check = Invoke-DeviceDotnet -FilePath $adb -WorkingDirectory $RepoRoot -Arguments @(
            '-s', $DeviceUdid, 'shell', "if [ -f '$androidNativePath' ]; then echo present; else echo absent; fi"
        ) -LogPath $checkLog -TimeoutSeconds 30
        $presence = (Get-Content -LiteralPath $checkLog -Raw).Trim()
        if ($check.TimedOut -or $check.ExitCode -ne 0 -or $presence -notin @('present', 'absent')) {
            throw 'Cannot establish whether the selected Android device has a previous native result.'
        }
        if ($presence -eq 'present') {
            $previousPath = Join-Path $runDirectory 'android-result-before.trx'
            $pull = Invoke-DeviceDotnet -FilePath $adb -WorkingDirectory $RepoRoot -Arguments @(
                '-s', $DeviceUdid, 'pull', $androidNativePath, $previousPath
            ) -LogPath (Join-Path $runDirectory 'android-pull-before.log') -TimeoutSeconds 30
            if ($pull.TimedOut -or $pull.ExitCode -ne 0 -or !(Test-Path -LiteralPath $previousPath)) {
                throw 'Cannot preserve the previous native Android result for freshness verification.'
            }
            $nativeHash = (Get-FileHash -LiteralPath $previousPath -Algorithm SHA256).Hash
        }
    }
    if ($Platform -eq 'ios') {
        $oldContainer = & xcrun simctl get_app_container $DeviceUdid $applicationId data 2>$null
        if ($LASTEXITCODE -eq 0 -and $oldContainer) {
            $nativePath = Join-Path ("$oldContainer".Trim()) 'Documents/TestResults/testResults.trx'
            if (Test-Path -LiteralPath $nativePath) { $nativeHash = (Get-FileHash -LiteralPath $nativePath -Algorithm SHA256).Hash }
        }
    }
    $testArgs = @('test', '--project', $projectPath, '--configuration', $Configuration, '--framework', $target) + $properties
    if ($Rebuild -or $windowsPackageState) { $testArgs += '--no-build' }
    if ($Platform -in @('android', 'ios')) { $testArgs += @('--device', $DeviceUdid) }
    if ($Platform -in @('android', 'windows')) { $testArgs += @('--report-trx', '--results-directory', $runDirectory) }
    $testArgs += "-bl:$(Join-Path $runDirectory 'test.binlog')"
    $seconds = [int]$duration.TotalSeconds
    if ($Platform -eq 'windows' -and $IncludeClasses) { $seconds = [Math]::Min(600, $seconds) }
    $run = Invoke-DeviceDotnet -FilePath $DotnetPath -Arguments $testArgs -WorkingDirectory $RepoRoot `
        -LogPath (Join-Path $runDirectory 'dotnet-test.log') -TimeoutSeconds $seconds
    if ($run.TimedOut) {
        $marker = if ($Platform -eq 'windows' -and $IncludeClasses) { 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: ' } else { '' }
        throw "${marker}dotnet test timed out running requested targets."
    }
    if ($Platform -eq 'ios') {
        $container = & xcrun simctl get_app_container $DeviceUdid $applicationId data 2>$null
        if ($LASTEXITCODE -ne 0 -or !$container) { throw 'Could not resolve the native app result container.' }
        $nativePath = Join-Path ("$container".Trim()) 'Documents/TestResults/testResults.trx'
        if (!(Test-Path -LiteralPath $nativePath)) { throw 'Native app did not produce genuine TRX.' }
        if ($nativeHash -and (Get-FileHash -LiteralPath $nativePath -Algorithm SHA256).Hash -eq $nativeHash) {
            throw 'The native result is stale: no new TRX was produced in this execution.'
        }
        Copy-Item -LiteralPath $nativePath -Destination (Join-Path $runDirectory 'testResults.trx')
    }
    $files = @(Get-ChildItem -LiteralPath $runDirectory -Filter '*.trx' -Recurse -File | ForEach-Object FullName)
    if ($Platform -eq 'android') {
        $files = @($files | Where-Object { $_ -ne (Join-Path $runDirectory 'android-result-before.trx') })
        if ($files.Count -eq 0) { throw 'dotnet test did not produce genuine host TRX.' }
        # The Android SDK proxy rewrites TestMethod.name to the display name.
        # Keep its report for transport diagnostics, but verify selectors against the app's original TRX.
        $proxySummary = Get-DeviceTestResultSummary -ResultFiles $files
        $nativeDirectory = Join-Path $runDirectory 'android-native'
        New-Item -ItemType Directory -Path $nativeDirectory | Out-Null
        $nativePath = Join-Path $nativeDirectory 'testResults.trx'
        $pull = Invoke-DeviceDotnet -FilePath $adb -WorkingDirectory $RepoRoot -Arguments @(
            '-s', $DeviceUdid, 'pull', $androidNativePath, $nativePath
        ) -LogPath (Join-Path $runDirectory 'android-pull-after.log') -TimeoutSeconds 30
        if ($pull.TimedOut -or $pull.ExitCode -ne 0 -or !(Test-Path -LiteralPath $nativePath)) {
            throw 'Native Android app did not produce readable genuine TRX.'
        }
        if ($nativeHash -and (Get-FileHash -LiteralPath $nativePath -Algorithm SHA256).Hash -eq $nativeHash) {
            throw 'The native result is stale: no new TRX was produced in this execution.'
        }
        $files = @($nativePath)
    }
    if ($files.Count -eq 0) {
        $marker = if ($Platform -eq 'windows') { 'WINDOWS_DEVICE_TEST_NO_RESULTS: ' } else { '' }
        throw "${marker}dotnet test did not produce genuine TRX."
    }
    $summary = Get-DeviceTestResultSummary -ResultFiles $files -IncludeClasses $IncludeClasses `
        -IncludeMethods $IncludeMethods -RequireClassIsolation:([bool]$IncludeClasses)
    if ($Platform -eq 'android') {
        if ($proxySummary.Errors -gt 0) { throw 'The Android SDK proxy reported run-level errors.' }
        foreach ($counter in @('Total', 'Passed', 'Failed', 'Skipped')) {
            if ($summary[$counter] -ne $proxySummary[$counter]) {
                throw "Native Android and SDK proxy TRX disagree on $counter."
            }
        }
    }
    if ($summary.Errors -gt 0 -or ($run.ExitCode -ne 0 -and $summary.Failed -eq 0)) {
        throw "Device test infrastructure failure: dotnet test exited $($run.ExitCode), run-level errors=$($summary.Errors)."
    }
    $reportSummary = $summary
    $exitCode = if ($run.ExitCode -ne 0) { $run.ExitCode } elseif ($summary.Failed -gt 0) { 1 } else { 0 }
    }
}
catch {
    $reportSummary = $null
    Write-Output "Device test infrastructure failure: $(ConvertTo-AzdoSafeConsole $_.Exception.Message)"
    $exitCode = 1
}
finally {
    if ($windowsPackageState) {
        try {
            Invoke-WithoutGhTokens {
                & (Join-Path $PSScriptRoot 'WindowsTestPackage.ps1') -Action Cleanup -StatePath $windowsPackageState
            }
        }
        catch {
            $reportSummary = $null
            Write-Output "Windows package cleanup failed: $(ConvertTo-AzdoSafeConsole $_.Exception.Message)"
            $exitCode = 1
        }
    }
    if ($ownedResults) {
        try {
            Invoke-WithoutGhTokens {
                & (Join-Path $PSScriptRoot 'Stop-TestDevice.ps1') -ResultsDirectory $ownedResults
            } 6>&1 | ForEach-Object { Write-Host "[device] $(ConvertTo-AzdoSafeConsole "$_")" }
        }
        catch {
            $reportSummary = $null
            Write-Output "Device cleanup failed: $(ConvertTo-AzdoSafeConsole $_.Exception.Message)"
            $exitCode = 1
        }
    }
    if ($locationPushed) { Pop-Location }
    $env:ANDROID_AVD_HOME = $originalAvdHome
}
if ($reportSummary) {
    foreach ($name in $reportSummary.FailedTests) { Write-Output "Failed test: $name" }
    Write-Output "  Passed: $($reportSummary.Passed)"
    Write-Output "  Failed: $($reportSummary.Failed)"
    Write-Output "  Skipped: $($reportSummary.Skipped)"
    Write-Output "  Total: $($reportSummary.Total)"
}
exit $exitCode
