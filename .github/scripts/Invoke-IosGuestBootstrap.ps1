#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string] $ExpectedSourceVersion,

    [Parameter(Mandatory)]
    [ValidatePattern('^[1-9][0-9]*$')]
    [string] $BuildId,

    [Parameter(Mandatory)]
    [string] $OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-IosGuestBootstrapScope {
    param(
        [string] $DefinitionId,
        [string] $SourceBranch,
        [string] $Platform,
        [string] $IssueNumber,
        [string] $PRNumber
    )

    if ($DefinitionId -cne '27723' -or
        $SourceBranch -cne 'refs/heads/copilot/replicate-issues-pipeline' -or
        $Platform -cne 'ios' -or $IssueNumber -cne '0' -or $PRNumber -cne '0') {
        throw 'ios-guest-bootstrap requires trusted Azure definition 27723, Platform=ios and both target numbers=0.'
    }
}

function Get-IosGuestBootstrapEnvironment {
    param([string] $HomeDirectory, [string] $TemporaryDirectory)

    @{
        PATH = '/usr/bin:/bin:/usr/sbin:/sbin'
        HOME = $HomeDirectory
        TMPDIR = $TemporaryDirectory
        LANG = 'en_US.UTF-8'
        LC_ALL = 'en_US.UTF-8'
    }
}

function Invoke-IosGuestBootstrapCommand {
    param(
        [Parameter(Mandatory)][string] $FilePath,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $ArgumentList,
        [Parameter(Mandatory)][Collections.IDictionary] $Environment,
        [Parameter(Mandatory)][ValidateRange(1, 4680)][int] $TimeoutSeconds
    )

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $FilePath
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.Environment.Clear()
    foreach ($name in $Environment.Keys) {
        $start.Environment[$name] = $Environment[$name]
    }
    foreach ($argument in $ArgumentList) {
        $start.ArgumentList.Add($argument)
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    $started = $false
    try {
        if (-not $process.Start()) {
            throw 'The fixed guest-bootstrap command did not start.'
        }
        $started = $true
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
        if ($timedOut) {
            $process.Kill($true)
            if (-not $process.WaitForExit(10000)) {
                throw 'The exact timed-out guest-bootstrap process did not terminate.'
            }
        }
        $output = $stdout.GetAwaiter().GetResult()
        $errorOutput = $stderr.GetAwaiter().GetResult()
        [pscustomobject]@{
            ExitCode = $process.ExitCode
            TimedOut = $timedOut
            Output = $output.Substring(0, [Math]::Min(1048576, $output.Length))
            ErrorOutput = $errorOutput.Substring(0, [Math]::Min(1048576, $errorOutput.Length))
            Diagnostic = $errorOutput.Substring(0, [Math]::Min(4096, $errorOutput.Length))
        }
    } finally {
        try {
            if ($started -and -not $process.HasExited) {
                $process.Kill($true)
                if (-not $process.WaitForExit(10000)) {
                    throw 'Guest-bootstrap cleanup could not terminate its exact process.'
                }
            }
        } finally {
            $process.Dispose()
        }
    }
}

function Assert-IosGuestBootstrapResult {
    param(
        [Parameter(Mandatory)][string] $Root,
        [Parameter(Mandatory)][string] $ExpectedSourceVersion,
        [Parameter(Mandatory)][string] $BuildId
    )

    $file = Get-Item -LiteralPath (Join-Path $Root 'result.json')
    if ($file.PSIsContainer -or $file.LinkTarget -or $file.Length -gt 64KB) {
        throw 'Guest-bootstrap result must be a bounded regular file.'
    }
    $document = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json -AsHashtable
    $fields = @(
        'schemaVersion', 'mode', 'pipelineCommit', 'buildId',
        'guestInstalled', 'guestStarted', 'guestStopped', 'outcome', 'reasonCode',
        'exitCode', 'exitDiagnostic', 'successScope', 'screenshots',
        'networkDeviceCount', 'directoryShareCount', 'socketDeviceCount',
        'restoreImageURL', 'restoreImageVersion', 'restoreImageBuild', 'restoreImageSHA256',
        'certifiesIssue', 'enforcesEgress', 'allowsGeneratedExecution'
    )
    if ($document -isnot [Collections.IDictionary] -or $document.Count -ne $fields.Count) {
        throw 'Guest-bootstrap result has an unexpected schema.'
    }
    foreach ($field in $fields) {
        if (-not $document.Contains($field)) {
            throw "Guest-bootstrap result is missing $field."
        }
    }
    foreach ($field in @('schemaVersion', 'buildId', 'exitCode',
            'networkDeviceCount', 'directoryShareCount', 'socketDeviceCount')) {
        if ($document[$field] -isnot [int] -and $document[$field] -isnot [long]) {
            throw "Guest-bootstrap result has a non-integer $field."
        }
    }
    if ($document.schemaVersion -ne 1 -or $document.mode -cne 'ios-guest-bootstrap' -or
        $document.pipelineCommit -cne $ExpectedSourceVersion -or
        [string]$document.buildId -cne $BuildId -or $document.exitCode -ne 0 -or
        $document.outcome -cne 'succeeded' -or
        $document.reasonCode -cne 'guest-restored-booted-visual-evidence' -or
        $document.successScope -cne 'restore-and-boot-with-owned-window-screenshots-only') {
        throw 'Guest-bootstrap result does not describe successful execution of this exact build and source.'
    }
    foreach ($field in @('guestInstalled', 'guestStarted', 'guestStopped')) {
        if ($document[$field] -isnot [bool] -or -not $document[$field]) {
            throw "Guest-bootstrap result does not confirm $field."
        }
    }
    foreach ($field in @('certifiesIssue', 'enforcesEgress', 'allowsGeneratedExecution')) {
        if ($document[$field] -isnot [bool] -or $document[$field]) {
            throw "Guest-bootstrap result cannot grant $field."
        }
    }
    foreach ($field in @('networkDeviceCount', 'directoryShareCount', 'socketDeviceCount')) {
        if ($document[$field] -ne 0) {
            throw "Guest-bootstrap result reports an unexpected $field."
        }
    }
    $imageUri = $null
    if ($document.restoreImageURL -isnot [string] -or
        -not [Uri]::TryCreate($document.restoreImageURL, [UriKind]::Absolute, [ref]$imageUri) -or
        $imageUri.Scheme -cne 'https' -or $imageUri.Port -ne 443 -or $imageUri.UserInfo -or
        -not $imageUri.DnsSafeHost.EndsWith('.cdn-apple.com', [StringComparison]::OrdinalIgnoreCase) -or
        $document.restoreImageSHA256 -isnot [string] -or
        $document.restoreImageSHA256 -cnotmatch '^[0-9a-f]{64}$' -or
        $document.restoreImageVersion -isnot [string] -or
        $document.restoreImageVersion -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+$' -or
        $document.restoreImageBuild -isnot [string] -or
        $document.restoreImageBuild -cnotmatch '^[A-Za-z0-9]{1,32}$' -or
        $document.exitDiagnostic -isnot [string] -or $document.exitDiagnostic.Length -gt 2048) {
        throw 'Guest-bootstrap result has invalid restore provenance or diagnostics.'
    }
    $expectedSnapshots = @('preflight-window.png',
        'guest-window-01.png', 'guest-window-02.png', 'guest-window-03.png')
    if ($document.screenshots -isnot [array] -or
        $document.screenshots.Count -ne $expectedSnapshots.Count) {
        throw 'Guest-bootstrap result does not include the four fixed snapshots.'
    }
    $snapshotHashes = [ordered]@{}
    for ($index = 0; $index -lt $expectedSnapshots.Count; $index++) {
        $name = $expectedSnapshots[$index]
        if ($document.screenshots[$index] -cne $name) {
            throw 'Guest-bootstrap result has an unexpected or duplicate snapshot.'
        }
        $snapshot = Get-Item -LiteralPath (Join-Path $Root $name)
        if ($snapshot.PSIsContainer -or $snapshot.LinkTarget -or
            $snapshot.Length -lt 33 -or $snapshot.Length -gt 16MB) {
            throw 'Guest-bootstrap snapshot must be a bounded regular PNG.'
        }
        $bytes = [IO.File]::ReadAllBytes($snapshot.FullName)
        if ([Convert]::ToHexString([byte[]]$bytes[0..23]) -cne
            '89504E470D0A1A0A0000000D494844520000050000000320') {
            throw 'Guest-bootstrap snapshot is not a 1280x800 PNG.'
        }
        $snapshotHashes[$name] = (Get-FileHash -LiteralPath $snapshot.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    [ordered]@{
        resultSha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        screenshots = $snapshotHashes
    }
}

Assert-IosGuestBootstrapScope -DefinitionId $env:SYSTEM_DEFINITIONID `
    -SourceBranch $env:BUILD_SOURCEBRANCH -Platform $env:PARAM_PLATFORM `
    -IssueNumber $env:PARAM_ISSUE_NUMBER -PRNumber $env:PARAM_PR_NUMBER
if (-not [OperatingSystem]::IsMacOS() -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() -cne 'Arm64') {
    throw 'The trusted macOS guest bootstrap requires an Apple Silicon macOS host.'
}
if ([string]::IsNullOrWhiteSpace($env:PIPELINE_WORKSPACE) -or $env:BUILD_BUILDID -cne $BuildId) {
    throw 'The guest bootstrap requires the current Azure workspace and build identity.'
}
$root = [IO.Path]::GetFullPath($OutputDirectory)
if ($root -cne [IO.Path]::GetFullPath((Join-Path $env:PIPELINE_WORKSPACE 'IosGuestBootstrap'))) {
    throw 'The guest bootstrap output must be its fixed pipeline workspace directory.'
}
if (Test-Path -LiteralPath $root) {
    throw 'The guest bootstrap refuses a pre-existing output directory.'
}

$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$tracked = @(
    '.github/scripts/Invoke-IosGuestBootstrap.ps1'
    '.github/scripts/fixtures/IosGuestBootstrap.swift'
    '.github/scripts/fixtures/IosGuestBootstrap.entitlements'
    'eng/pipelines/ci-copilot.yml'
)
New-Item -ItemType Directory -Path $root | Out-Null
$launchWork = Join-Path $root 'launch-work'
$guestWork = Join-Path $root 'work'
$diagnostics = Join-Path $root 'diagnostics'
New-Item -ItemType Directory -Path $launchWork, $diagnostics | Out-Null
$privateHome = Join-Path $launchWork 'home'
$temporary = Join-Path $launchWork 'tmp'
New-Item -ItemType Directory -Path $privateHome, $temporary | Out-Null
$environment = Get-IosGuestBootstrapEnvironment -HomeDirectory $privateHome -TemporaryDirectory $temporary
$result = [ordered]@{
    schemaVersion = 1
    mode = 'ios-guest-bootstrap'
    pipelineCommit = $ExpectedSourceVersion
    buildId = $BuildId
    certifiesIssue = $false
    enforcesEgress = $false
    allowsGeneratedExecution = $false
    trustedSource = [ordered]@{}
    commands = [ordered]@{}
    guestEvidence = $null
    outcome = 'infrastructure-error'
    failureMessage = $null
    cleanupFailed = $false
}
$failure = $null
$guestExitConfirmed = $true
try {
    $head = Invoke-IosGuestBootstrapCommand -FilePath '/usr/bin/git' `
        -ArgumentList @('-C', $repo, 'rev-parse', 'HEAD') -Environment $environment -TimeoutSeconds 30
    if ($head.ExitCode -ne 0 -or $head.TimedOut -or $head.Output.Trim() -cne $ExpectedSourceVersion) {
        throw 'The guest bootstrap requires the exact trusted pipeline checkout.'
    }
    foreach ($path in $tracked) {
        $file = Get-Item -LiteralPath (Join-Path $repo $path)
        if ($file.PSIsContainer -or $file.LinkTarget) {
            throw 'A guest-bootstrap input is not a regular checked-in file.'
        }
        $expected = Invoke-IosGuestBootstrapCommand -FilePath '/usr/bin/git' `
            -ArgumentList @('-C', $repo, 'rev-parse', "$ExpectedSourceVersion`:$path") `
            -Environment $environment -TimeoutSeconds 30
        $actual = Invoke-IosGuestBootstrapCommand -FilePath '/usr/bin/git' `
            -ArgumentList @('-C', $repo, 'hash-object', "--path=$path", '--', $file.FullName) `
            -Environment $environment -TimeoutSeconds 30
        if ($expected.ExitCode -ne 0 -or $actual.ExitCode -ne 0 -or
            $expected.TimedOut -or $actual.TimedOut -or
            $expected.Output.Trim() -cnotmatch '^[0-9a-f]{40}$' -or
            $actual.Output.Trim() -cne $expected.Output.Trim()) {
            throw 'A guest-bootstrap input differs from its immutable Git blob.'
        }
        $result.trustedSource[$path] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $root 'scope.json') -Encoding utf8
    $binary = Join-Path $launchWork 'ios-guest-bootstrap'
    $commands = @(
        @{
            Name = 'compile'; File = '/usr/bin/xcrun'; Timeout = 180
            Arguments = @('--sdk', 'macosx', 'swiftc', '-parse-as-library', '-swift-version', '5',
                '-target', 'arm64-apple-macosx15.0',
                '-module-cache-path', (Join-Path $launchWork 'module-cache'),
                (Join-Path $PSScriptRoot 'fixtures/IosGuestBootstrap.swift'),
                '-framework', 'Virtualization', '-framework', 'AppKit',
                '-framework', 'ScreenCaptureKit', '-o', $binary)
        }
        @{
            Name = 'sign'; File = '/usr/bin/codesign'; Timeout = 30
            Arguments = @('--force', '--sign', '-', '--entitlements',
                (Join-Path $PSScriptRoot 'fixtures/IosGuestBootstrap.entitlements'), $binary)
        }
        @{
            Name = 'verify'; File = '/usr/bin/codesign'; Timeout = 30
            Arguments = @('--verify', '--strict', $binary)
        }
        @{
            Name = 'bootstrap'; File = $binary; Timeout = 4680
            Arguments = @('--output-directory', $root, '--source-version', $ExpectedSourceVersion,
                '--build-id', $BuildId)
        }
    )
    foreach ($command in $commands) {
        Write-Host "Starting fixed guest-bootstrap command: $($command.Name)"
        if ($command.Name -ceq 'bootstrap') {
            $guestExitConfirmed = $false
        }
        $execution = Invoke-IosGuestBootstrapCommand -FilePath $command.File `
            -ArgumentList $command.Arguments -Environment $environment -TimeoutSeconds $command.Timeout
        if ($command.Name -ceq 'bootstrap') {
            $guestExitConfirmed = $true
        }
        $result.commands[$command.Name] = @{
            exitCode = $execution.ExitCode
            timedOut = $execution.TimedOut
        }
        $execution.Output | Set-Content -LiteralPath (Join-Path $root "$($command.Name)-stdout.log") -Encoding utf8
        $execution.ErrorOutput | Set-Content -LiteralPath (Join-Path $root "$($command.Name)-stderr.log") -Encoding utf8
        if ($execution.TimedOut -or $execution.ExitCode -ne 0) {
            throw "Guest-bootstrap $($command.Name) failed (exit=$($execution.ExitCode), timeout=$($execution.TimedOut)): $($execution.Diagnostic)"
        }
    }
    $result.guestEvidence = Assert-IosGuestBootstrapResult -Root $root `
        -ExpectedSourceVersion $ExpectedSourceVersion -BuildId $BuildId
    $result.outcome = 'guest-bootstrap-finished-not-certified'
} catch {
    $failure = $_
    $result.failureMessage = $_.Exception.Message.Substring(0, [Math]::Min(4096, $_.Exception.Message.Length))
} finally {
    try {
        if (-not $guestExitConfirmed) {
            throw 'Retaining owned VM files because guest process termination was not confirmed.'
        }
        foreach ($ownedDirectory in @($guestWork, $launchWork)) {
            if (Test-Path -LiteralPath $ownedDirectory) {
                Remove-Item -LiteralPath $ownedDirectory -Recurse -Force
            }
        }
    } catch {
        $result.cleanupFailed = $true
        $result.outcome = 'infrastructure-error'
        if ($null -eq $failure) {
            $failure = $_
            $result.failureMessage = $_.Exception.Message.Substring(0, [Math]::Min(4096, $_.Exception.Message.Length))
        }
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $root 'launcher-result.json') -Encoding utf8
    $totalBytes = 0L
    foreach ($file in @(Get-ChildItem -LiteralPath $root -File)) {
        if ($file.Name -cnotmatch '^[a-z0-9-]+\.(json|png|log)$' -or
            $file.LinkTarget -or $file.Length -gt 16MB) {
            throw 'Guest-bootstrap diagnostics contain an unexpected or oversized file.'
        }
        $totalBytes += $file.Length
        if ($totalBytes -gt 64MB) {
            throw 'Guest-bootstrap diagnostics exceeded the fixed artifact size limit.'
        }
        Copy-Item -LiteralPath $file.FullName -Destination $diagnostics
    }
}
if ($null -ne $failure) {
    throw $failure
}
