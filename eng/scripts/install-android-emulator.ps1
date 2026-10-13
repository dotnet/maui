param(
    [Parameter(Mandatory)][version] $Version,
    [Parameter(Mandatory)][ValidatePattern('^emulator-[a-z0-9_]+-[0-9]+\.zip$')][string] $ArchiveFileName,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{64}$')][string] $ArchiveSha256
)

$ErrorActionPreference = 'Stop'
$sdkOutput = & dotnet maui android sdk check --ci --json
if ($LASTEXITCODE -ne 0) {
    throw "Could not locate the Android SDK (exit code: $LASTEXITCODE)."
}
$sdk = ($sdkOutput -join [Environment]::NewLine) | ConvertFrom-Json
if ($sdk.status -ne 'Ok' -or [string]::IsNullOrWhiteSpace($sdk.details.path)) {
    throw "Could not locate a usable Android SDK (status: $($sdk.status))."
}

$emulatorPath = Join-Path $sdk.details.path 'emulator'
$packagePath = Join-Path $emulatorPath 'package.xml'
if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Install the emulator SDK package before pinning it: '$packagePath' is missing."
}
$executableName = if ($IsWindows) { 'emulator.exe' } else { 'emulator' }

function Assert-EmulatorVersion([string] $Path) {
    $output = & (Join-Path $Path $executableName) -version 2>&1
    if ($LASTEXITCODE -ne 0 -or ($output -join "`n") -notmatch "Android emulator version $([regex]::Escape($Version.ToString()))(\.0)?(\s|$)") {
        throw "Expected Android emulator $Version at '$Path', but got: $($output -join "`n")"
    }
    Write-Host "Verified Android emulator $Version at '$Path'."
}

$temporaryPath = Join-Path $sdk.details.path ".emulator-provision-$([guid]::NewGuid())"
$backupPath = Join-Path $temporaryPath 'original'
$installationSucceeded = $false
New-Item -ItemType Directory -Path $temporaryPath | Out-Null
try {
    $archivePath = Join-Path $temporaryPath $ArchiveFileName
    Invoke-WebRequest -Uri "https://dl.google.com/android/repository/$ArchiveFileName" -OutFile $archivePath
    if ((Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash -ne $ArchiveSha256) {
        throw "SHA-256 checksum mismatch for '$ArchiveFileName'."
    }

    $extractionPath = Join-Path $temporaryPath 'extracted'
    New-Item -ItemType Directory -Path $extractionPath | Out-Null
    if ($IsWindows) {
        Expand-Archive -LiteralPath $archivePath -DestinationPath $extractionPath
    }
    else {
        # Expand-Archive does not preserve Unix executable permissions.
        & unzip -q $archivePath -d $extractionPath
        if ($LASTEXITCODE -ne 0) {
            throw "Could not extract '$ArchiveFileName' (exit code: $LASTEXITCODE)."
        }
    }
    $stagedEmulatorPath = Join-Path $extractionPath 'emulator'
    Assert-EmulatorVersion $stagedEmulatorPath

    # Archived binaries omit sdkmanager's package.xml; preserve it with the pinned revision.
    [xml] $package = Get-Content -Raw -LiteralPath $packagePath
    $revision = $package.SelectSingleNode("//*[local-name()='localPackage' and @path='emulator']/*[local-name()='revision']")
    if ($null -eq $revision) {
        throw "The emulator package.xml has no emulator revision."
    }
    $revision.major = $Version.Major.ToString()
    $revision.minor = $Version.Minor.ToString()
    $revision.micro = $Version.Build.ToString()
    $package.Save((Join-Path $stagedEmulatorPath 'package.xml'))

    Move-Item -LiteralPath $emulatorPath -Destination $backupPath
    try {
        Move-Item -LiteralPath $stagedEmulatorPath -Destination $emulatorPath
        Assert-EmulatorVersion $emulatorPath
        $installationSucceeded = $true
    }
    catch {
        if (Test-Path -LiteralPath $emulatorPath) {
            Remove-Item -LiteralPath $emulatorPath -Recurse -Force
        }
        Move-Item -LiteralPath $backupPath -Destination $emulatorPath
        throw
    }
}
finally {
    if (-not $installationSucceeded -and (Test-Path -LiteralPath $backupPath)) {
        Write-Warning "Emulator rollback failed. The original installation is preserved at '$backupPath'."
    }
    else {
        Remove-Item -LiteralPath $temporaryPath -Recurse -Force
    }
}
