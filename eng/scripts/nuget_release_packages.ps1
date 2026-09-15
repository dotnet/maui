param (
  [Parameter(Mandatory)]
  [ValidateSet('FilterExisting', 'Verify', 'ValidateManifests')]
  [string] $Action,

  [Parameter(Mandatory)]
  [string] $PackagesPath,

  [string] $SkipFilters = 'skip',

  [string] $RecoveryAuditPath,

  [ValidateRange(1, 1000)]
  [int] $MaxAttempts = 1,

  [ValidateRange(0, 3600)]
  [int] $DelaySeconds = 20,

  [ValidateRange(1, 10)]
  [int] $StatusQueryAttempts = 3,

  [ValidateRange(0, 300)]
  [int] $StatusQueryDelaySeconds = 5,

  [ValidateRange(1, 240)]
  [int] $MaxDurationMinutes = 30
)

$ErrorActionPreference = 'Stop'
$verificationDeadline = if ($Action -eq 'Verify') {
  [DateTime]::UtcNow.AddMinutes($MaxDurationMinutes)
}
else {
  $null
}

function Get-Filters {
  param (
    [string] $Value
  )

  if ([string]::IsNullOrWhiteSpace($Value) -or $Value -eq 'skip') {
    return @()
  }

  return @($Value -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

function Test-AnyFilter {
  param (
    [string] $Name,
    [string[]] $Filters
  )

  foreach ($filter in $Filters) {
    if ($Name -like $filter) {
      return $true
    }
  }

  return $false
}

function Get-ExpectedPackages {
  $manifestPath = Join-Path $PackagesPath 'expected-packages.json'
  if (!(Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Expected package manifest '$manifestPath' was not found."
  }

  $packages = @(Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json)
  if ($packages.Count -eq 0) {
    throw "Expected package manifest '$manifestPath' contains no packages."
  }

  $expectedFiles = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
  foreach ($package in $packages) {
    foreach ($property in @('id', 'version', 'normalizedVersion', 'fileName')) {
      if ([string]::IsNullOrWhiteSpace($package.$property)) {
        throw "Expected package manifest '$manifestPath' has an entry without '$property'."
      }
    }

    if ($package.fileName -match '[\\/]' -or [System.IO.Path]::GetFileName($package.fileName) -ne $package.fileName) {
      throw "Package file name '$($package.fileName)' must not contain a directory."
    }

    if (!$expectedFiles.Add($package.fileName)) {
      throw "Expected package manifest '$manifestPath' contains duplicate file name '$($package.fileName)'."
    }
  }

  $unexpectedFiles = @(Get-ChildItem -LiteralPath $PackagesPath -Filter '*.nupkg' -File |
    Where-Object { !$expectedFiles.Contains($_.Name) })
  if ($unexpectedFiles.Count -gt 0) {
    throw "Package directory '$PackagesPath' contains files absent from the manifest: $($unexpectedFiles.Name -join ', ')."
  }

  return $packages
}

function Get-NuGetPackageStatus {
  param (
    [Parameter(Mandatory)]
    [object] $Package
  )

  $id = [Uri]::EscapeDataString($Package.id.ToLowerInvariant())
  $version = [Uri]::EscapeDataString($Package.normalizedVersion.ToLowerInvariant())
  $url = "https://api.nuget.org/v3-flatcontainer/$id/$version/$id.$version.nupkg"

  for ($attempt = 1; $attempt -le $StatusQueryAttempts; $attempt++) {
    if ($verificationDeadline -and [DateTime]::UtcNow -ge $verificationDeadline) {
      throw [TimeoutException]::new("The NuGet.org verification deadline was reached.")
    }

    try {
      $response = Invoke-WebRequest -Uri $url -Method Head -SkipHttpErrorCheck -TimeoutSec 30
      $statusCode = [int] $response.StatusCode
      if ($statusCode -notin @(408, 429, 500, 502, 503, 504) -or $attempt -eq $StatusQueryAttempts) {
        return [pscustomobject]@{
          StatusCode = $statusCode
          Url = $url
        }
      }

      Write-Warning "NuGet.org returned HTTP $statusCode for '$url' (attempt $attempt of $StatusQueryAttempts)."
    }
    catch {
      if ($attempt -eq $StatusQueryAttempts) {
        throw
      }

      Write-Warning "Unable to query '$url' (attempt $attempt of $StatusQueryAttempts): $($_.Exception.Message)"
    }

    if ($verificationDeadline -and [DateTime]::UtcNow -ge $verificationDeadline) {
      throw [TimeoutException]::new("The NuGet.org verification deadline was reached.")
    }
    Start-Sleep -Seconds $StatusQueryDelaySeconds
  }
}

$expected = @(Get-ExpectedPackages)

if ($Action -eq 'ValidateManifests') {
  $audit = if ($RecoveryAuditPath) {
    Get-Content -LiteralPath $RecoveryAuditPath -Raw | ConvertFrom-Json
  }
  if ($RecoveryAuditPath -and @($audit.manifests).Count -ne $expected.Count) {
    throw 'The retained manifests do not match the approved recovery audit.'
  }
  foreach ($package in $expected) {
    if ($package.id -notmatch '^Microsoft\.NET\.Sdk\.Maui\.Manifest-[0-9]+\.[0-9]+\.[0-9]+(?:\.Msi\.(?:arm64|x64|x86))?$' -or
        $package.fileName -ine "$($package.id).$($package.normalizedVersion).nupkg") {
      throw "Package '$($package.fileName)' is not an expected MAUI workload manifest."
    }

    $packagePath = Join-Path $PackagesPath $package.fileName
    if ($RecoveryAuditPath) {
      $approved = @($audit.manifests | Where-Object {
        $_.fileName -ieq $package.fileName -and $_.id -ieq $package.id -and $_.version -ceq $package.version
      })
      if ($approved.Count -ne 1 -or
          (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash -ine $approved[0].sha256) {
        throw "Package '$packagePath' does not match the approved recovery audit."
      }
    }
    $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
      $nuspec = @($archive.Entries | Where-Object {
        $_.Name -like '*.nuspec' -and $_.FullName -eq $_.Name
      })
      if ($nuspec.Count -ne 1) {
        throw "Expected one root nuspec in '$packagePath'."
      }

      $settings = [System.Xml.XmlReaderSettings]::new()
      $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
      $settings.XmlResolver = $null
      $stream = $nuspec[0].Open()
      try {
        $reader = [System.Xml.XmlReader]::Create($stream, $settings)
        try {
          $document = [System.Xml.XmlDocument]::new()
          $document.XmlResolver = $null
          $document.Load($reader)
          $ids = $document.SelectNodes("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='id']")
          $versions = $document.SelectNodes("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']")
          if ($ids.Count -ne 1 -or $versions.Count -ne 1 -or $ids[0].InnerText.Trim() -ine $package.id -or
              $versions[0].InnerText.Trim() -cne $package.version) {
            throw "Package '$packagePath' does not match its expected ID and version."
          }
        }
        finally {
          $reader.Dispose()
        }
      }
      finally {
        $stream.Dispose()
      }
    }
    finally {
      $archive.Dispose()
    }
  }
  Write-Host "Validated all $($expected.Count) MAUI workload manifest identities."
  return
}

if ($Action -eq 'FilterExisting') {
  $skipPatterns = @(Get-Filters -Value $SkipFilters)
  $unmatchedSkipPatterns = @($skipPatterns | Where-Object {
    $filter = $_
    @($expected | Where-Object { $_.fileName -like $filter }).Count -eq 0
  })
  if ($unmatchedSkipPatterns.Count -gt 0) {
    throw "Previously attempted package filters matched no expected packages: $($unmatchedSkipPatterns -join ', ')."
  }

  $remaining = 0
  foreach ($package in $expected) {
    $packagePath = Join-Path $PackagesPath $package.fileName
    $packageExists = Test-Path -LiteralPath $packagePath -PathType Leaf

    if ($skipPatterns.Count -gt 0 -and (Test-AnyFilter -Name $package.fileName -Filters $skipPatterns)) {
      Write-Host "Skipping previously attempted package $($package.id) $($package.version)."
      if ($packageExists) {
        Remove-Item -LiteralPath $packagePath
      }
      continue
    }

    $status = Get-NuGetPackageStatus -Package $package
    if ($status.StatusCode -eq 200) {
      Write-Host "Skipping already-published package $($package.id) $($package.version)."
      if ($packageExists) {
        Remove-Item -LiteralPath $packagePath
      }
    }
    elseif ($status.StatusCode -eq 404) {
      if (!$packageExists) {
        throw "Unpublished package '$packagePath' was not found."
      }
      $remaining++
    }
    else {
      throw "NuGet.org returned HTTP $($status.StatusCode) while checking '$($status.Url)'."
    }
  }

  $hasPackages = ($remaining -gt 0).ToString().ToLowerInvariant()
  Write-Host "$remaining of $($expected.Count) packages remain to publish."
  Write-Host "##vso[task.setvariable variable=NuGetPackagesToPublish]$hasPackages"
  return
}

$missing = @()
for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
  $missing = @(foreach ($package in $expected) {
    try {
      if ((Get-NuGetPackageStatus -Package $package).StatusCode -ne 200) {
        $package
      }
    }
    catch {
      Write-Warning "Unable to query NuGet.org for $($package.id) $($package.version): $($_.Exception.Message)"
      $package
    }
  })

  if ($missing.Count -eq 0) {
    Write-Host "Verified all $($expected.Count) packages on NuGet.org."
    return
  }

  if ([DateTime]::UtcNow -ge $verificationDeadline) {
    break
  }

  if ($attempt -lt $MaxAttempts) {
    Write-Host "Waiting for $($missing.Count) packages to become available on NuGet.org (attempt $attempt of $MaxAttempts)."
    $remainingSeconds = [Math]::Max(0, [Math]::Floor(($verificationDeadline - [DateTime]::UtcNow).TotalSeconds))
    Start-Sleep -Seconds ([Math]::Min($DelaySeconds, $remainingSeconds))
  }
}

$details = $missing | ForEach-Object { "$($_.id) $($_.version)" }
throw "The following packages are not available from NuGet.org: $($details -join ', ')"
