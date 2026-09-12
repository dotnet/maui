#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs selected MAUI BenchmarkDotNet suites against a PR's merge-base and head.

.DESCRIPTION
    This trusted script owns the empirical phase:

      * resolves and fetches the PR commits before executing PR code;
      * removes credentials from Git configuration;
      * keeps base and head in separate source/build directories;
      * optionally runs each side as a separate unprivileged Linux user with an
        explicit environment allowlist;
      * runs base/head in ABBA order;
      * verifies every requested filter matched in every run;
      * records every build, run, and report in a manifest.

    If benchmark harness inputs changed, that suite is not measured. Mixing different
    benchmark workloads would invalidate the comparison.
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PrNumber,

    [Parameter(Mandatory = $true)]
    [string]$SuitesPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputRoot,

    [Parameter(Mandatory = $false)]
    [ValidateRange(2, 10)]
    [int]$RunsPerSide = 2,

    [Parameter(Mandatory = $false)]
    [ValidateSet("short", "medium", "long")]
    [string]$Job = "short",

    [Parameter(Mandatory = $false)]
    [ValidateSet("None", "LinuxUsers")]
    [string]$IsolationMode = "None",

    [Parameter(Mandatory = $false)]
    [string]$PrMetadataPath
)

$ErrorActionPreference = "Stop"

$BuildFlags = @(
    "-p:IncludeIosTargetFrameworks=false",
    "-p:IncludeMacOSTargetFrameworks=false",
    "-p:IncludeAndroidTargetFrameworks=false",
    "-p:IncludeMacCatalystTargetFrameworks=false",
    "-p:IncludeWindowsTargetFrameworks=false",
    "-p:IncludeTizenTargetFrameworks=false",
    "-p:TreatWarningsAsErrors=false"
)
$BuildEnvironment = [ordered]@{
    IncludeIosTargetFrameworks = "false"
    IncludeMacOSTargetFrameworks = "false"
    IncludeAndroidTargetFrameworks = "false"
    IncludeMacCatalystTargetFrameworks = "false"
    IncludeWindowsTargetFrameworks = "false"
    IncludeTizenTargetFrameworks = "false"
    TreatWarningsAsErrors = "false"
}

$SecretNamePattern = '(?i)(TOKEN|SECRET|PASSWORD|CREDENTIAL|PRIVATE_KEY|ACCESS_KEY|CLIENT_SECRET|(^|_)PAT($|_))'
$manifestPath = Join-Path $OutputRoot "run-manifest.json"
$prPath = Join-Path $OutputRoot "pr.json"
$worktreeRoot = Join-Path ([IO.Path]::GetTempPath()) "mpw-$PID-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$trustedBenchmarkRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\benchmark-overlays"))
$headWorktree = Join-Path $worktreeRoot "head"
$baseWorktree = Join-Path $worktreeRoot "base"
$suiteResults = New-Object System.Collections.Generic.List[object]
$fatalError = $null
$headSha = $null
$mergeBase = $null
$baseRef = $null
$credentialsSanitized = $false
$originalOriginUrl = $null
$restoreOriginalOrigin = $false
$isolationRoot = $null
$isolationUsers = @()
$executionContexts = @{}
$sdkExecutables = @{}
$sdkRoot = Join-Path ([IO.Path]::GetTempPath()) "maui-perf-sdks-$PID-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$buildTaskResults = @{}
$runnerGid = $null

function Write-Info([string]$Message) {
    [Console]::Error.WriteLine($Message)
}

function Ensure-Directory([string]$Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Invoke-Native {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [object[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $FilePath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    return [PSCustomObject]@{
        ExitCode = $exitCode
        Output = @($output | ForEach-Object { $_.ToString() })
    }
}

function Invoke-LoggedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [object[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [string]$LogPath,

        [Parameter(Mandatory = $false)]
        [switch]$ScrubSecrets
    )

    Ensure-Directory (Split-Path -Parent $LogPath)
    $savedEnvironment = @{}

    if ($ScrubSecrets) {
        foreach ($entry in Get-ChildItem Env:) {
            if ($entry.Name -match $SecretNamePattern) {
                $savedEnvironment[$entry.Name] = $entry.Value
                Remove-Item "Env:$($entry.Name)" -ErrorAction SilentlyContinue
            }
        }
    }

    Push-Location $WorkingDirectory
    try {
        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        try {
            & $FilePath @Arguments *> $LogPath
            return $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $previousErrorActionPreference
        }
    }
    finally {
        Pop-Location
        foreach ($name in $savedEnvironment.Keys) {
            Set-Item "Env:$name" $savedEnvironment[$name]
        }
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Arguments
    )

    $result = Invoke-Native -FilePath "git" -Arguments $Arguments
    if ($result.ExitCode -ne 0) {
        throw "git $($Arguments -join ' ') failed: $($result.Output -join [Environment]::NewLine)"
    }

    return @($result.Output)
}

function Invoke-Sudo {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Arguments,

        [Parameter(Mandatory = $false)]
        [switch]$IgnoreFailure
    )

    $result = Invoke-Native -FilePath "sudo" -Arguments (@("-n") + $Arguments)
    if ($result.ExitCode -ne 0 -and -not $IgnoreFailure) {
        throw "sudo $($Arguments -join ' ') failed: $($result.Output -join [Environment]::NewLine)"
    }

    return $result
}

function Set-AnonymousOrigin {
    $script:originalOriginUrl = (& git remote get-url origin 2>$null | Select-Object -First 1)
    $script:restoreOriginalOrigin =
        $script:originalOriginUrl -and
        $script:originalOriginUrl -notmatch '(?i)(x-access-token|oauth2:|https://[^/@]+@)'

    $repository = if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { "dotnet/maui" }
    $server = if ($env:GITHUB_SERVER_URL) { $env:GITHUB_SERVER_URL.TrimEnd('/') } else { "https://github.com" }
    $anonymousUrl = "$server/$repository.git"

    Invoke-Git @("remote", "set-url", "origin", $anonymousUrl) | Out-Null
    & git config --local --unset-all "http.https://github.com/.extraheader" 2>$null
    & git config --global --unset-all "http.https://github.com/.extraheader" 2>$null

    $effectiveUrl = (& git remote get-url origin 2>$null | Select-Object -First 1)
    if (-not $effectiveUrl -or $effectiveUrl -match '(?i)(x-access-token|oauth2:|https://[^/@]+@)') {
        throw "Git credentials were not removed from origin before running PR code."
    }

    Write-Info "Git origin sanitized to an anonymous URL."
}

function Remove-Worktree([string]$Path) {
    if (Test-Path $Path) {
        & git worktree remove --force $Path *> $null
    }
}

function Install-TrustedBenchmarkFiles {
    foreach ($suite in $runnableSuites) {
        $trustedBenchmarkFiles = if ($null -ne $suite.PSObject.Properties["trustedBenchmarkFiles"]) {
            @($suite.trustedBenchmarkFiles | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
        }
        else {
            @()
        }
        foreach ($relativePath in $trustedBenchmarkFiles) {
            if ([string]::IsNullOrWhiteSpace($relativePath) -or
                [IO.Path]::IsPathRooted($relativePath) -or
                $relativePath -match '(^|[\\/])\.\.([\\/]|$)') {
                throw "Invalid trusted benchmark path: $relativePath"
            }

            $source = [IO.Path]::GetFullPath((Join-Path $trustedBenchmarkRoot $relativePath))
            if (-not $source.StartsWith($trustedBenchmarkRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
                -not (Test-Path $source -PathType Leaf)) {
                throw "Trusted benchmark source is missing or outside the trusted overlay root: $relativePath"
            }

            foreach ($context in @($executionContexts.Values)) {
                $work = [IO.Path]::GetFullPath([string]$context.Work)
                $destination = [IO.Path]::GetFullPath((Join-Path $work $relativePath))
                if (-not $destination.StartsWith($work + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                    throw "Trusted benchmark destination is outside the worktree: $relativePath"
                }

                if ($IsolationMode -eq "LinuxUsers") {
                    Invoke-Sudo @("mkdir", "-p", (Split-Path -Parent $destination)) | Out-Null
                    Invoke-Sudo @("cp", "--", $source, $destination) | Out-Null
                    Invoke-Sudo @("chown", "$($context.User)`:$runnerGid", $destination) | Out-Null
                    Invoke-Sudo @("chmod", "0644", $destination) | Out-Null
                }
                else {
                    Ensure-Directory (Split-Path -Parent $destination)
                    Copy-Item -LiteralPath $source -Destination $destination -Force
                }
            }
        }
    }
}

function New-LinuxExecutionContext {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Side,

        [Parameter(Mandatory = $true)]
        [string]$SourceWorktree,

        [Parameter(Mandatory = $true)]
        [string]$UserName
    )

    $sideRoot = Join-Path $isolationRoot $Side
    $home = Join-Path $sideRoot "home"
    $work = Join-Path $sideRoot "work"

    Invoke-Sudo @("mkdir", "-p", $sideRoot) | Out-Null
    Invoke-Sudo @("useradd", "--system", "--create-home", "--home-dir", $home, "--shell", "/bin/bash", $UserName) | Out-Null
    $script:isolationUsers += $UserName

    Invoke-Sudo @("mkdir", "-p", (Join-Path $home "tmp"), (Join-Path $home ".dotnet"), ([IO.Path]::Combine($home, ".nuget", "packages"))) | Out-Null
    # A standalone clone preserves Git metadata required by Arcade/versioning while
    # preventing either side from mutating the main repository or the sibling side.
    Invoke-Sudo @("git", "clone", "--quiet", "--no-hardlinks", $SourceWorktree, $work) | Out-Null
    Invoke-Sudo @("chown", "-R", "$UserName`:$runnerGid", $sideRoot) | Out-Null
    Invoke-Sudo @("chmod", "0711", $isolationRoot) | Out-Null
    Invoke-Sudo @("chmod", "-R", "u+rwX,g+rX,o-rwx", $sideRoot) | Out-Null

    return [PSCustomObject]@{
        Side = $Side
        User = $UserName
        Root = $sideRoot
        Home = $home
        Work = $work
    }
}

function Initialize-ExecutionContexts {
    if ($IsolationMode -eq "None") {
        $script:executionContexts["base"] = [PSCustomObject]@{
            Side = "base"
            User = $null
            Home = $null
            Work = $baseWorktree
        }
        $script:executionContexts["head"] = [PSCustomObject]@{
            Side = "head"
            User = $null
            Home = $null
            Work = $headWorktree
        }
        return
    }

    if (-not $IsLinux) {
        throw "IsolationMode LinuxUsers requires Linux."
    }

    $sudoCheck = Invoke-Native -FilePath "sudo" -Arguments @("-n", "true")
    if ($sudoCheck.ExitCode -ne 0) {
        throw "Passwordless sudo is required for LinuxUsers isolation."
    }
    if (-not (Get-Command "systemd-run" -ErrorAction SilentlyContinue)) {
        throw "systemd-run is required for LinuxUsers process containment."
    }

    $script:runnerGid = ((Invoke-Native -FilePath "id" -Arguments @("-g")).Output | Select-Object -First 1).Trim()
    $suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
    $script:isolationRoot = Join-Path ([IO.Path]::GetTempPath()) "maui-perf-isolation-$suffix"

    Invoke-Sudo @("mkdir", "-p", $isolationRoot) | Out-Null
    Invoke-Sudo @("chmod", "0711", $isolationRoot) | Out-Null

    $script:executionContexts["base"] =
        New-LinuxExecutionContext -Side "base" -SourceWorktree $baseWorktree -UserName "mperf_b_$suffix"
    $script:executionContexts["head"] =
        New-LinuxExecutionContext -Side "head" -SourceWorktree $headWorktree -UserName "mperf_h_$suffix"
}

function Remove-Isolation {
    foreach ($user in $isolationUsers) {
        Invoke-Sudo @("pkill", "-KILL", "-u", $user) -IgnoreFailure | Out-Null
        Invoke-Sudo @("userdel", "--force", $user) -IgnoreFailure | Out-Null
    }

    if ($isolationRoot) {
        Invoke-Sudo @("rm", "-rf", $isolationRoot) -IgnoreFailure | Out-Null
    }
}

function Get-IsolatedEnvironmentArguments($Context) {
    $arguments = @(
        "env",
        "-i",
        "HOME=$($Context.Home)",
        "USER=$($Context.User)",
        "LOGNAME=$($Context.User)",
        "PATH=$($env:PATH)",
        "DOTNET_CLI_HOME=$($Context.Home)/.dotnet",
        "NUGET_PACKAGES=$($Context.Home)/.nuget/packages",
        "TMPDIR=$($Context.Home)/tmp",
        "LANG=C.UTF-8",
        "LC_ALL=C.UTF-8",
        "CI=true",
        "MSBUILDDISABLENODEREUSE=1"
    )

    if ($env:DOTNET_ROOT) {
        $arguments += "DOTNET_ROOT=$($env:DOTNET_ROOT)"
    }
    if ($env:DOTNET_ROOT_X64) {
        $arguments += "DOTNET_ROOT_X64=$($env:DOTNET_ROOT_X64)"
    }
    foreach ($entry in $BuildEnvironment.GetEnumerator()) {
        $arguments += "$($entry.Key)=$($entry.Value)"
    }

    return $arguments
}

function Invoke-Dotnet {
    param(
        [Parameter(Mandatory = $true)]
        $Context,

        [Parameter(Mandatory = $true)]
        [object[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$LogPath
    )

    $dotnet = if ($Context.DotNet) { [string]$Context.DotNet } else { "dotnet" }
    if ($IsolationMode -eq "None") {
        $savedBuildEnvironment = @{}
        foreach ($entry in $BuildEnvironment.GetEnumerator()) {
            $savedBuildEnvironment[$entry.Key] = [Environment]::GetEnvironmentVariable($entry.Key)
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
        }
        try {
            return Invoke-LoggedCommand -FilePath $dotnet -Arguments $Arguments -WorkingDirectory $Context.Work -LogPath $LogPath -ScrubSecrets
        }
        finally {
            foreach ($entry in $savedBuildEnvironment.GetEnumerator()) {
                [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
            }
        }
    }

    $unitName = "maui-perf-$($Context.Side)-$PID-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
    $sudoArguments = @(
        "-n",
        "systemd-run",
        "--quiet",
        "--wait",
        "--collect",
        "--pipe",
        "--unit=$unitName",
        "--uid=$($Context.User)",
        "--working-directory=$($Context.Work)",
        "--property=KillMode=control-group",
        "--property=NoNewPrivileges=yes",
        "--property=PrivateDevices=yes",
        "--property=ProtectHome=yes",
        "--property=ProtectSystem=full",
        "--property=RestrictSUIDSGID=yes"
    ) + @(Get-IsolatedEnvironmentArguments $Context) + @($dotnet) + $Arguments

    try {
        return Invoke-LoggedCommand -FilePath "sudo" -Arguments $sudoArguments -WorkingDirectory $Context.Work -LogPath $LogPath -ScrubSecrets
    }
    finally {
        # Defense in depth for children that deliberately detach or create a new session.
        Invoke-Sudo @("pkill", "-KILL", "-u", $Context.User) -IgnoreFailure | Out-Null
    }
}

function Get-RequiredSdkVersion($Context) {
        $globalJsonPath = Join-Path $Context.Work "global.json"
        if (-not (Test-Path $globalJsonPath)) {
            return $null
        }

        $globalJson = Get-Content $globalJsonPath -Raw | ConvertFrom-Json
        $version = [string]$globalJson.sdk.version
        if ([string]::IsNullOrWhiteSpace($version)) {
            $version = [string]$globalJson.tools.dotnet
        }
        if ([string]::IsNullOrWhiteSpace($version)) {
            return $null
        }
        if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:[-.][0-9A-Za-z]+)*$') {
            throw "global.json contains an invalid SDK version: '$version'."
        }

        return $version
    }

function Ensure-DotNetSdk([string]$Version) {
        if ($sdkExecutables.ContainsKey($Version)) {
            return $sdkExecutables[$Version]
        }

        $systemDotNet = (Get-Command "dotnet" -ErrorAction Stop).Source
        $installedVersions = @(
            & $systemDotNet --list-sdks 2>$null |
                ForEach-Object { ($_ -split '\s+')[0] }
        )
        if ($installedVersions -contains $Version) {
            $sdkExecutables[$Version] = $systemDotNet
            return $systemDotNet
        }

        Ensure-Directory $sdkRoot
        $installDirectory = Join-Path $sdkRoot $Version
        $installer = Join-Path $sdkRoot "dotnet-install.ps1"
        if (-not (Test-Path $installer)) {
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installer -UseBasicParsing
        }

        $installLog = Join-Path $sdkRoot "install-$($Version -replace '[^0-9A-Za-z_.-]', '_').log"
        & $installer -Version $Version -InstallDir $installDirectory -NoPath *> $installLog

        $executable = Join-Path $installDirectory $(if ($IsWindows) { "dotnet.exe" } else { "dotnet" })
        if (-not (Test-Path $executable)) {
            throw "Installed .NET SDK $Version did not contain '$executable'."
        }

        if ($IsLinux -or $IsMacOS) {
            & chmod -R a-w $installDirectory
            & chmod -R a+rX $installDirectory
            & chmod a+rx $executable
        }

        $sdkExecutables[$Version] = $executable
        return $executable
    }

function Initialize-DotNetSdks {
    foreach ($side in @("base", "head")) {
        $context = $executionContexts[$side]
        $version = Get-RequiredSdkVersion $context
        $dotnet = if ($version) { Ensure-DotNetSdk $version } else { (Get-Command "dotnet" -ErrorAction Stop).Source }
        $context | Add-Member -NotePropertyName DotNet -NotePropertyValue $dotnet -Force
        $context | Add-Member -NotePropertyName SdkVersion -NotePropertyValue $version -Force
    }
}

function Write-BenchmarkBuildOverride($Context) {
    $overridePath = Join-Path $Context.Work "Directory.Build.Override.props"
    if (Test-Path $overridePath) {
        throw "Cannot apply trusted benchmark build settings because '$overridePath' already exists."
    }

$content = @'
<Project>
  <PropertyGroup>
    <IncludeIosTargetFrameworks>false</IncludeIosTargetFrameworks>
    <IncludeMacOSTargetFrameworks>false</IncludeMacOSTargetFrameworks>
    <IncludeAndroidTargetFrameworks>false</IncludeAndroidTargetFrameworks>
    <IncludeMacCatalystTargetFrameworks>false</IncludeMacCatalystTargetFrameworks>
    <IncludeWindowsTargetFrameworks>false</IncludeWindowsTargetFrameworks>
    <IncludeTizenTargetFrameworks>false</IncludeTizenTargetFrameworks>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
'@

    if ($IsolationMode -eq "None") {
        $content | Set-Content $overridePath -Encoding UTF8
        return
    }

    $temporaryOverride = Join-Path ([IO.Path]::GetTempPath()) "maui-perf-override-$PID-$([Guid]::NewGuid().ToString('N')).props"
    try {
        $content | Set-Content $temporaryOverride -Encoding UTF8
        Invoke-Sudo @(
            "install",
            "-o", $Context.User,
            "-g", $runnerGid,
            "-m", "0644",
            $temporaryOverride,
            $overridePath
        ) | Out-Null
    }
    finally {
        Remove-Item $temporaryOverride -Force -ErrorAction SilentlyContinue
    }
}

function Initialize-BenchmarkBuildOverrides {
    foreach ($side in @("base", "head")) {
        Write-BenchmarkBuildOverride $executionContexts[$side]
    }
}

function Assert-RealDirectoryPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $rootPath = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar)
    $candidatePath = [IO.Path]::GetFullPath($Path).TrimEnd([IO.Path]::DirectorySeparatorChar)
    $rootPrefix = "$rootPath$([IO.Path]::DirectorySeparatorChar)"
    if (-not $candidatePath.StartsWith($rootPrefix, [StringComparison]::Ordinal)) {
        throw "Isolated artifact path '$candidatePath' is outside '$rootPath'."
    }

    $components = New-Object System.Collections.Generic.List[string]
    $current = $candidatePath
    while ($true) {
        $components.Add($current)
        if ($current -eq $rootPath) {
            break
        }

        $parent = [IO.Path]::GetDirectoryName($current)
        if ([string]::IsNullOrWhiteSpace($parent) -or $parent.Length -lt $rootPath.Length) {
            throw "Isolated artifact path '$candidatePath' does not resolve beneath '$rootPath'."
        }
        $current = $parent
    }

    foreach ($component in @($components | Sort-Object Length)) {
        $directoryCheck = Invoke-Native -FilePath "sudo" -Arguments @("-n", "test", "-d", $component)
        $linkCheck = Invoke-Native -FilePath "sudo" -Arguments @("-n", "test", "!", "-L", $component)
        if ($directoryCheck.ExitCode -ne 0 -or $linkCheck.ExitCode -ne 0) {
            throw "Isolated artifact path contains a missing, non-directory, or symbolic-link component: $component"
        }
    }
}

function Assert-SafeArtifactTree {
    param(
        [Parameter(Mandatory = $true)]
        $Context,

        [Parameter(Mandatory = $true)]
        [string]$Source
    )

    Assert-RealDirectoryPath -Root $Context.Root -Path $Source

    $unsafeEntries = Invoke-Sudo @(
        "find",
        $Source,
        "-xdev",
        "-mindepth", "1",
        "!", "-type", "f",
        "!", "-type", "d",
        "-print"
    )

    if ($unsafeEntries.Output.Count -gt 0) {
        throw "Isolated benchmark artifacts contain links or non-regular files: $($unsafeEntries.Output -join ', ')"
    }
}

function Copy-IsolatedArtifacts {
    param(
        [Parameter(Mandatory = $true)]
        $Context,

        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    if ($IsolationMode -eq "None") {
        return
    }

    if (-not (Test-Path $Destination)) {
        Ensure-Directory $Destination
    }

    Assert-SafeArtifactTree -Context $Context -Source $Source
    Invoke-Sudo @("chgrp", "-R", $runnerGid, $Source) | Out-Null
    Invoke-Sudo @("chmod", "-R", "g+rX,o-rwx", $Source) | Out-Null

    $sourceRoot = [IO.Path]::GetFullPath($Source)
    foreach ($file in @(Get-ChildItem -LiteralPath $Source -File -Recurse -Force)) {
        if ($file.LinkType -or ($file.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Isolated benchmark artifact is a link: $($file.FullName)"
        }
        if ($file.Name -notlike "*-report-full*.json") {
            continue
        }

        $relativePath = [IO.Path]::GetRelativePath($sourceRoot, $file.FullName)
        $destinationPath = Join-Path $Destination $relativePath
        Ensure-Directory (Split-Path -Parent $destinationPath)
        Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
    }
}

function Get-RunEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RunDirectory,

        [Parameter(Mandatory = $true)]
        [string[]]$Filters
    )

    $reports = @(
        Get-ChildItem -Path $RunDirectory -Recurse -Filter '*-report-full*.json' -ErrorAction SilentlyContinue
    )
    $benchmarkNames = New-Object System.Collections.Generic.HashSet[string]
    $parseErrors = New-Object System.Collections.Generic.List[string]

    foreach ($report in $reports) {
        try {
            $document = Get-Content $report.FullName -Raw | ConvertFrom-Json
            foreach ($benchmark in @($document.Benchmarks)) {
                if ($benchmark.FullName) {
                    [void]$benchmarkNames.Add([string]$benchmark.FullName)
                }
            }
        }
        catch {
            $parseErrors.Add($report.FullName)
        }
    }

    $matchedFilters = New-Object System.Collections.Generic.List[string]
    $missingFilters = New-Object System.Collections.Generic.List[string]
    foreach ($filter in $Filters) {
        $matched = @($benchmarkNames | Where-Object { $_ -like $filter }).Count -gt 0
        if ($matched) {
            $matchedFilters.Add($filter)
        }
        else {
            $missingFilters.Add($filter)
        }
    }

    return [PSCustomObject]@{
        ReportCount = $reports.Count
        BenchmarkCount = $benchmarkNames.Count
        BenchmarkNames = @($benchmarkNames | Sort-Object)
        MatchedFilters = @($matchedFilters)
        MissingFilters = @($missingFilters)
        ParseErrors = @($parseErrors)
    }
}

function Write-Manifest([string]$Status) {
    Ensure-Directory $OutputRoot

    $runOrder = @(
        for ($run = 1; $run -le $RunsPerSide; $run++) {
            [PSCustomObject]@{
                run = $run
                order = if (($run % 2) -eq 1) {
                    @("base", "head")
                }
                else {
                    @("head", "base")
                }
            }
        }
    )

    $manifest = [PSCustomObject]@{
        schemaVersion = 2
        status = $Status
        prNumber = $PrNumber
        baseRef = $baseRef
        baseSha = $mergeBase
        headSha = $headSha
        job = $Job
        runsPerSide = $RunsPerSide
        runOrder = $runOrder
        isolationMode = $IsolationMode
        prerequisites = [PSCustomObject]@{
            buildTasks = [PSCustomObject]@{
                base = $buildTaskResults["base"]
                head = $buildTaskResults["head"]
            }
        }
        credentialsSanitized = $credentialsSanitized
        generatedAtUtc = [DateTime]::UtcNow.ToString("o")
        fatalError = $fatalError
        suites = @($suiteResults | ForEach-Object { $_ })
    }

    $manifest | ConvertTo-Json -Depth 14 | Set-Content -Path $manifestPath -Encoding UTF8
    Write-Info "Wrote $manifestPath"
}

Ensure-Directory $OutputRoot
if ($IsolationMode -eq "LinuxUsers") {
    $chmodResult = Invoke-Native -FilePath "chmod" -Arguments @("0700", $OutputRoot)
    if ($chmodResult.ExitCode -ne 0) {
        throw "Could not restrict the performance output directory."
    }
}

try {
    if (-not (Test-Path $SuitesPath)) {
        throw "Suite selection file not found: $SuitesPath"
    }

    $selection = Get-Content $SuitesPath -Raw | ConvertFrom-Json
    $suites = @($selection.suites)

    if ($PrMetadataPath) {
        if (-not (Test-Path $PrMetadataPath)) {
            throw "Pinned PR metadata file not found: $PrMetadataPath"
        }
        $pr = Get-Content $PrMetadataPath -Raw | ConvertFrom-Json
    }
    else {
        $prJson = & gh pr view $PrNumber --json number,title,state,baseRefName,baseRefOid,headRefOid,url 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Could not read PR #$PrNumber metadata: $($prJson -join [Environment]::NewLine)"
        }
        $pr = ($prJson -join [Environment]::NewLine) | ConvertFrom-Json
    }

    if ($pr.state -ne "OPEN") {
        throw "PR #$PrNumber is $($pr.state), not OPEN."
    }

    $pr | ConvertTo-Json -Depth 6 | Set-Content -Path $prPath -Encoding UTF8
    $headSha = $pr.headRefOid
    $baseRef = $pr.baseRefName

    if ($PrMetadataPath) {
        $mergeBase = [string]$pr.mergeBaseOid
        foreach ($sha in @($headSha, $mergeBase)) {
            $probe = Invoke-Native -FilePath "git" -Arguments @("cat-file", "-e", "$sha^{commit}")
            if ($probe.ExitCode -ne 0) {
                throw "Pinned commit is not available locally: $sha"
            }
        }
    }
    else {
        Invoke-Git @("fetch", "--quiet", "--no-tags", "origin", $baseRef, "pull/$PrNumber/head") | Out-Null
        $mergeBase = (Invoke-Git @("merge-base", "origin/$baseRef", $headSha) | Select-Object -First 1).Trim()
    }
    if (-not $mergeBase) {
        throw "Could not resolve the merge-base for PR #$PrNumber."
    }

    Set-AnonymousOrigin
    $credentialsSanitized = $true

    if ($suites.Count -eq 0) {
        Write-Manifest "no-managed-suites"
        exit 0
    }

    $runnableSuites = New-Object System.Collections.Generic.List[object]
    foreach ($suite in $suites) {
        if ($suite.benchmarkInputsChanged) {
            $suiteResults.Add([PSCustomObject]@{
                project = $suite.project
                csproj = $suite.csproj
                filters = @($suite.filters)
                matchedFiles = @($suite.matchedFiles)
                expectedRunsPerSide = $RunsPerSide
                builds = $null
                runs = @()
                complete = $false
                skipReason = "benchmark inputs changed"
                changedBenchmarkInputFiles = @($suite.changedBenchmarkInputFiles)
            })
        }
        else {
            $activeFilters = if ($null -ne $suite.PSObject.Properties["runnableFilters"]) {
                @($suite.runnableFilters)
            }
            else {
                @($suite.filters)
            }
            $suite | Add-Member -NotePropertyName ActiveFilters -NotePropertyValue $activeFilters -Force
            $runnableSuites.Add($suite)
        }
    }

    if ($runnableSuites.Count -eq 0) {
        Write-Manifest "incomplete"
        exit 0
    }

    Ensure-Directory (Split-Path -Parent $headWorktree)
    Remove-Worktree $headWorktree
    Remove-Worktree $baseWorktree
    & git worktree prune *> $null

    Invoke-Git @("worktree", "add", "--detach", $headWorktree, $headSha) | Out-Null
    Invoke-Git @("worktree", "add", "--detach", $baseWorktree, $mergeBase) | Out-Null
    Initialize-ExecutionContexts
    Install-TrustedBenchmarkFiles
    Initialize-BenchmarkBuildOverrides
    Initialize-DotNetSdks

    $requiresBuildTasks = @($runnableSuites | Where-Object { $_.project -eq "Xaml" }).Count -gt 0
    if ($requiresBuildTasks) {
        foreach ($side in @("base", "head")) {
            $context = $executionContexts[$side]
            $logPath = [IO.Path]::Combine($OutputRoot, "results", $side, "prerequisites", "build-tasks.log")
            $arguments = @(
                "build",
                "Microsoft.Maui.BuildTasks.slnf",
                "-c", "Release",
                "-m:1",
                "-p:BuildInParallel=false"
            ) + $BuildFlags
            $exitCode = Invoke-Dotnet -Context $context -Arguments $arguments -LogPath $logPath
            $buildTaskResults[$side] = [PSCustomObject]@{
                exitCode = $exitCode
                logPath = $logPath
                succeeded = ($exitCode -eq 0)
            }
            Write-Info "Build tasks $side exit=$exitCode"
        }
    }

    foreach ($suite in $runnableSuites) {
        $suiteKey = $suite.project -replace '[^A-Za-z0-9_.-]', '_'
        $resultsRoot = Join-Path $OutputRoot "results"
        $buildResults = @{}
        $runResults = New-Object System.Collections.Generic.List[object]

        foreach ($side in @("base", "head")) {
            $context = $executionContexts[$side]
            $logPath = [IO.Path]::Combine($resultsRoot, $side, $suiteKey, "build.log")
            if ($suite.project -eq "Xaml" -and -not $buildTaskResults[$side].succeeded) {
                Ensure-Directory (Split-Path -Parent $logPath)
                "Skipped because Microsoft.Maui.BuildTasks.slnf failed to build." |
                    Set-Content $logPath -Encoding UTF8
                $exitCode = [int]$buildTaskResults[$side].exitCode
            }
            else {
                $arguments = @("build", $suite.csproj, "-c", "Release") + $BuildFlags
                $exitCode = Invoke-Dotnet -Context $context -Arguments $arguments -LogPath $logPath
            }

            $buildResults[$side] = [PSCustomObject]@{
                exitCode = $exitCode
                logPath = $logPath
                succeeded = ($exitCode -eq 0)
            }
            Write-Info "Build $($suite.project) $side exit=$exitCode"
        }

        for ($run = 1; $run -le $RunsPerSide; $run++) {
            $order = if (($run % 2) -eq 1) { @("base", "head") } else { @("head", "base") }

            foreach ($side in $order) {
                $context = $executionContexts[$side]
                $runDirectory = [IO.Path]::Combine($resultsRoot, $side, $suiteKey, "run$run")
                $isolatedRunDirectory = if ($IsolationMode -eq "None") {
                    $runDirectory
                }
                else {
                    [IO.Path]::Combine($context.Home, "artifacts", $suiteKey, "run$run")
                }
                $logPath = Join-Path $runDirectory "benchmark.log"
                Ensure-Directory $runDirectory

                if (-not $buildResults[$side].succeeded) {
                    $runResults.Add([PSCustomObject]@{
                        side = $side
                        run = $run
                        exitCode = $null
                        reportCount = 0
                        benchmarkCount = 0
                        matchedFilters = @()
                        missingFilters = @($suite.ActiveFilters)
                        parseErrors = @()
                        logPath = $logPath
                        skipped = $true
                        reason = "build failed"
                    })
                    continue
                }

                if ($IsolationMode -eq "LinuxUsers") {
                    Invoke-Sudo @("mkdir", "-p", $isolatedRunDirectory) | Out-Null
                    Invoke-Sudo @("chown", "-R", "$($context.User)`:$runnerGid", (Split-Path -Parent (Split-Path -Parent $isolatedRunDirectory))) | Out-Null
                    Invoke-Sudo @("chmod", "-R", "u+rwX,g+rX,o-rwx", (Split-Path -Parent (Split-Path -Parent $isolatedRunDirectory))) | Out-Null
                }

                $arguments = @(
                    "run",
                    "-c", "Release",
                    "--no-build",
                    "--project", $suite.csproj
                ) + $BuildFlags + @(
                    "--",
                    "--filter"
                ) + @($suite.ActiveFilters) + @(
                    "--job", $Job,
                    "--memory",
                    "--exporters", "json",
                    "--cli", $context.DotNet,
                    "--envVars"
                ) + @(
                    $BuildEnvironment.GetEnumerator() |
                        ForEach-Object { "$($_.Key):$($_.Value)" }
                ) + @(
                    "--artifacts", $isolatedRunDirectory
                )

                $exitCode = Invoke-Dotnet -Context $context -Arguments $arguments -LogPath $logPath
                Copy-IsolatedArtifacts -Context $context -Source $isolatedRunDirectory -Destination $runDirectory
                $evidence = Get-RunEvidence -RunDirectory $runDirectory -Filters @($suite.ActiveFilters)

                $reason = if ($exitCode -ne 0) {
                    "benchmark process failed"
                }
                elseif ($evidence.ReportCount -eq 0) {
                    "no BenchmarkDotNet report"
                }
                elseif ($evidence.ParseErrors.Count -gt 0) {
                    "invalid BenchmarkDotNet report"
                }
                elseif ($evidence.MissingFilters.Count -gt 0) {
                    "one or more filters matched no benchmarks"
                }
                else {
                    $null
                }

                $runResults.Add([PSCustomObject]@{
                    side = $side
                    run = $run
                    exitCode = $exitCode
                    reportCount = $evidence.ReportCount
                    benchmarkCount = $evidence.BenchmarkCount
                    benchmarkNames = @($evidence.BenchmarkNames)
                    matchedFilters = @($evidence.MatchedFilters)
                    missingFilters = @($evidence.MissingFilters)
                    parseErrors = @($evidence.ParseErrors)
                    logPath = $logPath
                    skipped = $false
                    reason = $reason
                })
                Write-Info "Run $($suite.project) $side/$run exit=$exitCode reports=$($evidence.ReportCount) benchmarks=$($evidence.BenchmarkCount)"
            }
        }

        $notApplicableFilters = @(
            $suite.ActiveFilters | Where-Object {
                $filter = $_
                @($runResults | Where-Object {
                    $_.matchedFilters -contains $filter
                }).Count -eq 0
            }
        )
        $asymmetricMissingFilters = @(
            $suite.ActiveFilters | Where-Object {
                $filter = $_
                $baseMatched = @($runResults | Where-Object {
                    $_.side -eq "base" -and $_.matchedFilters -contains $filter
                }).Count -gt 0
                $headMatched = @($runResults | Where-Object {
                    $_.side -eq "head" -and $_.matchedFilters -contains $filter
                }).Count -gt 0
                $baseMatched -ne $headMatched
            }
        )
        foreach ($runResult in $runResults) {
            $effectiveMissingFilters = @(
                $runResult.missingFilters | Where-Object {
                    $_ -notin $notApplicableFilters
                }
            )
            $runResult | Add-Member `
                -NotePropertyName effectiveMissingFilters `
                -NotePropertyValue $effectiveMissingFilters `
                -Force
            if ($runResult.reason -eq "one or more filters matched no benchmarks" -and
                $effectiveMissingFilters.Count -eq 0) {
                $runResult.reason = $null
            }
        }

        $expectedRunCount = $RunsPerSide * 2
        $suiteComplete =
            $buildResults["base"].succeeded -and
            $buildResults["head"].succeeded -and
            $runResults.Count -eq $expectedRunCount -and
            @(
                $runResults | Where-Object {
                    $_.skipped -or
                    $_.exitCode -ne 0 -or
                    $_.reportCount -eq 0 -or
                    $_.parseErrors.Count -gt 0 -or
                    $_.effectiveMissingFilters.Count -gt 0
                }
            ).Count -eq 0

        $suiteResults.Add([PSCustomObject]@{
            project = $suite.project
            csproj = $suite.csproj
            filters = @($suite.filters)
            trustedBenchmarkFiles = if ($null -ne $suite.PSObject.Properties["trustedBenchmarkFiles"]) {
                @($suite.trustedBenchmarkFiles | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
            } else {
                @()
            }
            runnableFilters = @($suite.ActiveFilters)
            skippedFilters = @($suite.filters | Where-Object { $_ -notin $suite.ActiveFilters })
            notApplicableFilters = $notApplicableFilters
            asymmetricMissingFilters = $asymmetricMissingFilters
            matchedFiles = @($suite.matchedFiles)
            expectedRunsPerSide = $RunsPerSide
            builds = [PSCustomObject]@{
                base = $buildResults["base"]
                head = $buildResults["head"]
            }
            runs = @($runResults | ForEach-Object { $_ })
            complete = $suiteComplete
            skipReason = $null
            changedBenchmarkInputFiles = @($suite.changedBenchmarkInputFiles)
        })
    }

    $unexpectedIncomplete = @(
        $suiteResults | Where-Object {
            -not $_.complete -and [string]$_.skipReason -ne "benchmark inputs changed"
        }
    ).Count -gt 0
    $plannedCoverageGap = @(
        $suiteResults | Where-Object {
            [string]$_.skipReason -eq "benchmark inputs changed" -or
            @($_.skippedFilters).Count -gt 0
        }
    ).Count -gt 0
    $manifestStatus = if ($unexpectedIncomplete) {
        "incomplete"
    }
    elseif ($plannedCoverageGap) {
        "partial"
    }
    else {
        "complete"
    }
    Write-Manifest $manifestStatus
    exit 0
}
catch {
    $fatalError = $_.Exception.Message
    Write-Info "ERROR: $fatalError"
    Write-Manifest "failed"
    exit 1
}
finally {
    foreach ($dotnet in @($sdkExecutables.Values | Sort-Object -Unique)) {
        try {
            & $dotnet build-server shutdown *> $null
        }
        catch {
            Write-Info "WARN: could not shut down build servers for '$dotnet'."
        }
    }
    Remove-Isolation
    Remove-Worktree $headWorktree
    Remove-Worktree $baseWorktree
    & git worktree prune *> $null
    Remove-Item $worktreeRoot -Recurse -Force -ErrorAction SilentlyContinue

    if ($restoreOriginalOrigin -and $originalOriginUrl) {
        try {
            Invoke-Git @("remote", "set-url", "origin", $originalOriginUrl) | Out-Null
        }
        catch {
            Write-Info "WARN: could not restore the original non-credentialed Git remote."
        }
    }

    if (Test-Path $sdkRoot) {
        try {
            if ($IsWindows) {
                Get-ChildItem $sdkRoot -Recurse -Force -File -ErrorAction SilentlyContinue |
                    ForEach-Object { $_.IsReadOnly = $false }
            }
            elseif ($IsLinux -or $IsMacOS) {
                & chmod -R u+w $sdkRoot
            }
            Remove-Item $sdkRoot -Recurse -Force -ErrorAction Stop
        }
        catch {
            Write-Info "WARN: could not remove temporary SDK directory '$sdkRoot'."
        }
    }
}
