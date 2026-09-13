[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string] $ResultsRoot,

  [Parameter(Mandatory = $true)]
  [string] $RunMarker,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9]+-[0-9]+$')]
  [string] $ExpectedRunKey,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{40}$')]
  [string] $ExpectedSourceVersion
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ResultsRoot -PathType Container)) {
  throw "Issue 38080 results root was not found: '$ResultsRoot'."
}

if (-not (Test-Path -LiteralPath $RunMarker -PathType Leaf)) {
  throw "Issue 38080 run marker was not found: '$RunMarker'."
}

$notBefore = (Get-Item -LiteralPath $RunMarker).LastWriteTimeUtc
$runSpecifications = @(
  [pscustomobject]@{
    Directory = 'core-webview'
    Assembly = 'Core.DeviceTests'
    Package = 'com.microsoft.maui.core.devicetests'
    Category = 'WebView'
    Methods = @(
      [pscustomobject]@{
        Type = 'Microsoft.Maui.DeviceTests.WebViewHandlerTests'
        Method = 'ClippingContainerSurvivesZeroDimensionThenNormalSize'
        Count = 2
        Rows = @(
          '(initialWidth: 0, initialHeight: 100)'
          '(initialWidth: 100, initialHeight: 0)'
        )
      }
      [pscustomobject]@{
        Type = 'Microsoft.Maui.DeviceTests.WebViewHandlerTests'
        Method = 'InitialOpacityIsAppliedToClippingContainer'
        Count = 1
      }
      [pscustomobject]@{
        Type = 'Microsoft.Maui.DeviceTests.WebViewHandlerTests'
        Method = 'DisconnectHandlerDestroysNativeWebView'
        Count = 1
      }
    )
  }
  [pscustomobject]@{
    Directory = 'core-view'
    Assembly = 'Core.DeviceTests'
    Package = 'com.microsoft.maui.core.devicetests'
    Category = 'View'
    Methods = @(
      [pscustomobject]@{
        Type = 'Microsoft.Maui.DeviceTests.ViewHandlerTests'
        Method = 'OpacityIsPreservedAcrossNonWebViewContainerTransitions'
        Count = 1
      }
    )
  }
  [pscustomobject]@{
    Directory = 'controls-hybridwebview'
    Assembly = 'Controls.DeviceTests'
    Package = 'com.microsoft.maui.controls.devicetests'
    Category = 'HybridWebView'
    Methods = @(
      [pscustomobject]@{
        Type = 'Microsoft.Maui.DeviceTests.HybridWebViewTests_Initialization'
        Method = 'DisconnectHandlerRemovesClippingContainer'
        Count = 1
      }
    )
  }
)

$verifiedCases = 0
$allObservedTests = @()
$runId = $null
$deviceSerial = $null
$invocationIds = [System.Collections.Generic.HashSet[string]]::new(
  [System.StringComparer]::Ordinal)
$identityKeys = @(
  'RunKey', 'RunId', 'InvocationId', 'SourceVersion', 'Project', 'PackageName',
  'Instrumentation', 'Filter', 'DeviceSerial', 'DeviceArchitecture', 'ApiLevel',
  'AvdName', 'ResultFileName', 'StartedUtc', 'CompletedUtc', 'Succeeded'
)
foreach ($run in $runSpecifications) {
  $runDirectory = Join-Path $ResultsRoot $run.Directory
  if (-not (Test-Path -LiteralPath $runDirectory -PathType Container) -or
      ((Get-Item -LiteralPath $runDirectory).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
    throw "Issue 38080 category results must be a regular directory."
  }
  $identityPath = Join-Path $runDirectory 'run-identity.txt'
  if (-not (Test-Path -LiteralPath $identityPath -PathType Leaf)) {
    throw "Issue 38080 run identity is missing for '$($run.Directory)'."
  }
  $identityItem = Get-Item -LiteralPath $identityPath
  if (($identityItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
      $identityItem.Length -gt 16384 -or
      $identityItem.LastWriteTimeUtc -lt $notBefore) {
    throw "Issue 38080 run identity is not a fresh bounded regular file."
  }
  $identity = [System.Collections.Generic.Dictionary[string, string]]::new(
    [System.StringComparer]::Ordinal)
  foreach ($line in Get-Content -LiteralPath $identityPath -Encoding UTF8) {
    if ($line -cnotmatch '^([A-Za-z]+): (.*)$' -or
        $identityKeys -cnotcontains $Matches[1] -or
        -not $identity.TryAdd($Matches[1], $Matches[2])) {
      throw "Issue 38080 run identity contains an unknown, duplicate, or malformed field."
    }
  }
  if ($identity.Count -ne $identityKeys.Count) {
    throw "Issue 38080 run identity is incomplete."
  }
  if ($identity['RunKey'] -cne $ExpectedRunKey -or
      $identity['SourceVersion'] -cne $ExpectedSourceVersion -or
      $identity['Project'] -cne $run.Assembly -or
      $identity['PackageName'] -cne $run.Package -or
      $identity['Instrumentation'] -cne "$($run.Package).TestInstrumentation" -or
      $identity['Filter'] -cne "Category=$($run.Category)" -or
      $identity['DeviceArchitecture'] -cne 'x86_64' -or
      $identity['ApiLevel'] -cne '36' -or
      $identity['Succeeded'] -cne 'True' -or
      $identity['RunId'] -cnotmatch '^[0-9a-f]{32}$' -or
      $identity['InvocationId'] -cnotmatch '^[0-9a-f]{32}$' -or
      $identity['DeviceSerial'] -cnotmatch '^emulator-[0-9]+$' -or
      $identity['AvdName'] -cne "Issue38080Adjacency_$($ExpectedRunKey.Replace('-', '_'))_$($identity['RunId'])" -or
      $identity['ResultFileName'] -cne "testResults-$($identity['InvocationId']).xml") {
    throw "Issue 38080 run identity does not match the selected invocation."
  }
  if ($null -eq $runId) {
    $runId = $identity['RunId']
    $deviceSerial = $identity['DeviceSerial']
  }
  if ($identity['RunId'] -cne $runId -or
      $identity['DeviceSerial'] -cne $deviceSerial -or
      -not $invocationIds.Add($identity['InvocationId'])) {
    throw "Issue 38080 category runs do not have distinct invocations on one owned device."
  }
  $started = [DateTimeOffset]::MinValue
  $completed = [DateTimeOffset]::MinValue
  if (-not [DateTimeOffset]::TryParseExact($identity['StartedUtc'], 'O',
        [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::None, [ref] $started) -or
      -not [DateTimeOffset]::TryParseExact($identity['CompletedUtc'], 'O',
        [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::None, [ref] $completed) -or
      $started.UtcDateTime -lt $notBefore -or
      $completed -lt $started -or $completed -gt [DateTimeOffset]::UtcNow) {
    throw "Issue 38080 category execution timestamps are stale or invalid."
  }

  $resultFiles = @(Get-ChildItem -LiteralPath $runDirectory -Recurse -File `
    -Filter $identity['ResultFileName'])
  if ($resultFiles.Count -ne 1) {
    throw "Issue 38080 expected one native result file for invocation '$($identity['InvocationId'])'."
  }

  $resultItem = $resultFiles[0]
  $resultFile = $resultItem.FullName
  if (($resultItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
      $resultItem.Length -eq 0 -or $resultItem.Length -gt 16777216 -or
      $resultItem.LastWriteTimeUtc -lt $started.UtcDateTime) {
    throw "Issue 38080 result '$resultFile' is not a fresh bounded regular file."
  }

  $settings = [System.Xml.XmlReaderSettings]::new()
  $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
  $settings.XmlResolver = $null
  $settings.MaxCharactersInDocument = 16777216
  $reader = [System.Xml.XmlReader]::Create($resultFile, $settings)
  try {
    $document = [System.Xml.XmlDocument]::new()
    $document.XmlResolver = $null
    $document.Load($reader)
  } finally {
    $reader.Dispose()
  }
  $assemblies = @($document.assemblies.assembly)
  if ($assemblies.Count -ne 1) {
    throw "Issue 38080 expected exactly one assembly in '$resultFile', but found $($assemblies.Count)."
  }

  $assembly = $assemblies[0]
  $assemblyName = [System.IO.Path]::GetFileName([string] $assembly.name)
  if ($assemblyName.EndsWith('.dll', [System.StringComparison]::OrdinalIgnoreCase)) {
    $assemblyName = $assemblyName.Substring(0, $assemblyName.Length - 4)
  }
  if ($assemblyName -cne $run.Assembly) {
    throw "Issue 38080 expected assembly '$($run.Assembly)' in '$resultFile', but found '$assemblyName'."
  }

  $tests = @(
    $assembly.collection |
      ForEach-Object { @($_.test) } |
      Where-Object { $null -ne $_ }
  )
  if ($tests.Count -eq 0) {
    throw "Issue 38080 result '$resultFile' contains no native tests."
  }

  $allObservedTests += @(
    $tests |
      ForEach-Object {
        [pscustomobject]@{
          Directory = $run.Directory
          Type = [string] $_.type
          Method = [string] $_.method
        }
      }
  )

  foreach ($method in $run.Methods) {
    $matchingTests = @(
      $tests |
        Where-Object {
          ([string] $_.type) -ceq $method.Type -and
          ([string] $_.method) -ceq $method.Method
        }
    )

    $identity = "$($method.Type).$($method.Method)"
    if ($matchingTests.Count -ne $method.Count) {
      throw "Issue 38080 expected $($method.Count) native case(s) for '$identity' in '$resultFile', but found $($matchingTests.Count)."
    }

    if ($method.Count -eq 2) {
      $expectedNames = @($method.Rows | ForEach-Object { "$identity$_" })
      $actualNames = @($matchingTests | ForEach-Object { [string] $_.name })
      if (@(Compare-Object ($expectedNames | Sort-Object) ($actualNames | Sort-Object)).Count -ne 0 -or
          @($actualNames | Select-Object -Unique).Count -ne $method.Count) {
        throw "Issue 38080 theory must contain the distinct (0,100) and (100,0) cases."
      }
    }

    foreach ($test in $matchingTests) {
      if (([string] $test.result) -cne 'Pass') {
        throw "Issue 38080 native case '$([string] $test.name)' in '$resultFile' reported '$([string] $test.result)' instead of 'Pass'."
      }

      $categoryTraits = @(
        $test.traits.trait |
          Where-Object {
            ([string] $_.name) -ceq 'Category' -and
            ([string] $_.value) -ceq $run.Category
          }
      )
      if ($categoryTraits.Count -ne 1) {
        throw "Issue 38080 native case '$([string] $test.name)' did not report exactly one Category=$($run.Category) trait."
      }
    }

    $verifiedCases += $matchingTests.Count
  }
}

if ($verifiedCases -ne 6) {
  throw "Issue 38080 expected six verified native cases, but found $verifiedCases."
}

foreach ($run in $runSpecifications) {
  foreach ($method in $run.Methods) {
    $identity = "$($method.Type).$($method.Method)"
    $allMatches = @(
      $allObservedTests |
        Where-Object {
          $_.Type -ceq $method.Type -and
          $_.Method -ceq $method.Method
        }
    )
    if ($allMatches.Count -ne $method.Count) {
      throw "Issue 38080 expected $($method.Count) total native case(s) for '$identity', but found $($allMatches.Count) across all result files."
    }

    if (@($allMatches | Where-Object Directory -cne $run.Directory).Count -ne 0) {
      throw "Issue 38080 native case '$identity' appeared outside expected run '$($run.Directory)'."
    }
  }
}

Write-Host 'Verified six passing Issue 38080 native adjacency cases across five methods.'
