[CmdletBinding()]
param (
  [Parameter(Mandatory = $true)]
  [String]$App,
  [Parameter(Mandatory = $true)]
  [String]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSEdition -eq "Core") {
  Write-Error "Installing MSIX apps must be run from Windows PowerShell and not PowerShell Core."
  exit 1
}

Write-Host " - Determining app identity..."
$App = Resolve-Path $App
Write-Host "   MSIX installer: '$App'"
try {
  Add-Type -Assembly "System.IO.Compression.FileSystem"
  $msixZipFile = [IO.Compression.ZipFile]::OpenRead($App)
  $manifestEntry = $msixZipFile.Entries | Where-Object { $_.Name -eq "AppxManifest.xml"}
  $stream = $manifestEntry.Open()
  $reader = New-Object IO.StreamReader($stream)
  [xml]$manifestXml = $reader.ReadToEnd()
  $appIdentity = $manifestXml.Package.Identity.Name
} finally {
  if ($reader) {
    $reader.Close()
  }
  if ($stream) {
    $stream.Close()
  }
  if ($msixZipFile) {
    $msixZipFile.Dispose()
  }
}
Write-Host "   App identity found: '$appIdentity'"

Write-Host " - Testing to see if the app is installed..."
$appInstalls = Get-AppxPackage -Name $appIdentity
if ($appInstalls) {
  $packageFullName = $appInstalls.PackageFullName
  Write-Host "   App was installed '$packageFullName', uninstalling..."
  Remove-AppxPackage -Package $packageFullName
  Write-Host "   Uninstall complete..."
} else {
  Write-Host "   App was not installed."
}

# Install the app
Write-Host " - Installing dependencies..."
$arch = $env:PROCESSOR_ARCHITECTURE
if ($arch -eq "AMD64") {
  $arch = "x64"
}
$deps = Get-ChildItem "$App\..\Dependencies\$arch\*.msix"
foreach ($dep in $deps) {
  try {
    Write-Host "   Installing dependency: '$dep'"
    Add-AppxPackage -Path $dep
  } catch {
    Write-Host "   Dependency failed to install, continuing..."
  }
}
Write-Host " - Installing application..."
Add-AppxPackage -Path $App
$appInstalls = Get-AppxPackage -Name $appIdentity
$packageFullName = $appInstalls.PackageFullName
$packageFamilyName = $appInstalls.PackageFamilyName
Write-Host "   Application installed: '$packageFullName'"

Write-Host " - Preparation complete."

# Start the app
Write-Host " - Starting the application..."
New-Item -ItemType Directory $OutputDirectory -Force | Out-Null
$OutputDirectory = Resolve-Path $OutputDirectory
Remove-Item $OutputDirectory -Recurse -Force
Start-Process "shell:AppsFolder\$packageFamilyName!App" -Args "`"$OutputDirectory\TestResults.xml`" --xharness --output-directory=`"$OutputDirectory`""
Write-Host "   Application started."

# Wait for the tests to finish
Write-Host " - Waiting for test results at '$OutputDirectory\TestResults.xml'..."
Write-Host "------------------------------------------------------------"
$lastLine = 0
$timeout = 120
while (!(Test-Path "$OutputDirectory\TestResults.xml")) {
  Start-Sleep 0.6
  $timeout -= 0.6
  if (Test-Path $OutputDirectory\test-output-*.log) {
    $log = Get-ChildItem $OutputDirectory\test-output-*.log
    $lines = [string[]](Get-Content $log | Select-Object -Skip $lastLine)
    foreach ($line in $lines) {
      Write-Host $line
    }
    $lastLine += $lines.Length
  }
  if ($timeout -le 0) {
    break
  } elseif (($timeout % 1) -lt 0.0001) {
    $timeout = [int]$timeout
    Write-Host "... waiting ($timeout seconds remaining) ..."
  }
}
Write-Host "------------------------------------------------------------"
Write-Host " - Checking test results for failures..."
Write-Host "   Results file: '$OutputDirectory\TestResults.xml'"
[xml]$resultsXml = Get-Content "$OutputDirectory\TestResults.xml"
$failed = $resultsXml.assemblies.assembly |
  Where-Object { $_.failed -gt 0 -or $_.error -gt 0 }
if ($failed) {
  Write-Host "   There were test failures."
  $result = 1
} else {
  Write-Host "   There were no test failures."
}
Write-Host " - Tests complete."

# Tests are complete, uninstall the app
Write-Host " - Uninstalling application..."
Remove-AppxPackage -Package $packageFullName
Write-Host "   Application uninstalled."

Write-Host " - Cleanup complete."

exit $result