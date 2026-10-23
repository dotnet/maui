param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z0-9_-]+(?:[.;][A-Za-z0-9_-]+)*$')]
    [string] $Package
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

if ($env:TF_BUILD -ne 'True') {
    & dotnet maui android sdk install $Package
    exit $LASTEXITCODE
}

$maxAttempts = 5
$cacheRoot = Join-Path ([IO.Path]::GetTempPath()) "maui-android-sdk-$([Guid]::NewGuid().ToString('N'))"
$originalEnvironment = @{}
foreach ($name in @('ANDROID_USER_HOME', 'TMPDIR', 'TMP', 'TEMP')) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}

try {
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        # sdkmanager caches downloads in both its user home and temporary directories.
        $attemptRoot = Join-Path $cacheRoot $attempt
        $androidUserHome = Join-Path $attemptRoot '.android'
        $tempDirectory = Join-Path $attemptRoot 'temp'
        New-Item -ItemType Directory -Path $androidUserHome, $tempDirectory -Force | Out-Null
        $env:ANDROID_USER_HOME = $androidUserHome
        $env:TMPDIR = $tempDirectory
        $env:TMP = $tempDirectory
        $env:TEMP = $tempDirectory

        Write-Output "Installing Android SDK package '$Package' (attempt $attempt/$maxAttempts)."
        & dotnet maui android sdk install $Package --ci
        $exitCode = $LASTEXITCODE
        if ($exitCode -eq 0) {
            exit 0
        }
        if ($attempt -eq $maxAttempts) {
            Write-Error "Android SDK package '$Package' failed after $maxAttempts total attempts (exit code $exitCode)."
        }

        Write-Warning "Android SDK package '$Package' failed (attempt $attempt/$maxAttempts, exit code $exitCode). Removing the failed install before retrying."
        $sdkOutput = & dotnet maui android sdk check --ci --json
        if ($LASTEXITCODE -ne 0) {
            throw "Cannot safely clean the failed package '$Package': Android SDK discovery failed with exit code $LASTEXITCODE."
        }
        $sdk = ($sdkOutput -join [Environment]::NewLine) | ConvertFrom-Json
        $sdkPath = $sdk.details.path
        if ([string]::IsNullOrWhiteSpace($sdkPath) -or
            ![IO.Path]::IsPathFullyQualified($sdkPath) -or
            !(Test-Path -LiteralPath $sdkPath -PathType Container)) {
            throw "Cannot safely clean the failed package '$Package': Android SDK discovery returned an invalid path '$sdkPath'."
        }
        $sdkPath = [IO.Path]::GetFullPath($sdkPath)
        if ($sdkPath.TrimEnd([IO.Path]::DirectorySeparatorChar) -eq
            [IO.Path]::GetPathRoot($sdkPath).TrimEnd([IO.Path]::DirectorySeparatorChar)) {
            throw "Refusing to clean an Android package under filesystem root '$sdkPath'."
        }

        $packagePath = Join-Path $sdkPath ($Package.Replace(';', [IO.Path]::DirectorySeparatorChar))
        # Removing package.xml and .installer prevents a partial install being reused.
        foreach ($path in @($packagePath, (Join-Path $sdkPath '.temp'))) {
            if (Test-Path -LiteralPath $path) {
                Write-Output "Removing failed Android SDK install state '$path'."
                Remove-Item -LiteralPath $path -Recurse -Force
            }
        }
        Remove-Item -LiteralPath $attemptRoot -Recurse -Force
        Start-Sleep -Seconds ($attempt * $attempt)
    }
}
finally {
    foreach ($name in $originalEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name])
    }
    if (Test-Path -LiteralPath $cacheRoot) {
        Remove-Item -LiteralPath $cacheRoot -Recurse -Force
    }
}
