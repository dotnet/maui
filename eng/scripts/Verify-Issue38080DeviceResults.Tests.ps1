#Requires -Modules Pester

Describe 'Verify-Issue38080DeviceResults.ps1' {
  BeforeEach {
    $scriptPath = Join-Path $PSScriptRoot 'Verify-Issue38080DeviceResults.ps1'
    $testRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString())
    $resultsRoot = Join-Path $testRoot 'results'
    $runMarker = Join-Path $testRoot 'run-started.txt'
    $runKey = '12345-1'
    $sourceVersion = 'a' * 40
    $runId = [guid]::NewGuid().ToString('N')
    $resultFiles = @{}
    $identityFiles = @{}
    New-Item -ItemType Directory -Path $resultsRoot | Out-Null
    Set-Content -LiteralPath $runMarker -Value 'run started'

    function Invoke-Guard {
      & $scriptPath -ResultsRoot $resultsRoot -RunMarker $runMarker `
        -ExpectedRunKey $runKey -ExpectedSourceVersion $sourceVersion
    }

    function Write-ResultFile {
      param(
        [string] $Directory,
        [string] $Assembly,
        [string] $Category,
        [object[]] $Tests
      )

      $target = Join-Path $resultsRoot $Directory
      New-Item -ItemType Directory -Path $target | Out-Null
      $started = [DateTimeOffset]::UtcNow.ToString('O')
      $invocationId = [guid]::NewGuid().ToString('N')
      $fileName = "testResults-$invocationId.xml"
      $testNodes = foreach ($test in $Tests) {
        $suffix = if ($test.ContainsKey('Arguments')) { $test.Arguments } else { '' }
        @"
      <test name="$($test.Type).$($test.Method)$suffix" type="$($test.Type)" method="$($test.Method)" result="$($test.Result)">
        <traits><trait name="Category" value="$($test.Category)" /></traits>
      </test>
"@
      }
      $xml = @"
<assemblies>
  <assembly name="$Assembly.dll" total="$($Tests.Count)" passed="$(@($Tests | Where-Object Result -eq 'Pass').Count)" failed="$(@($Tests | Where-Object Result -eq 'Fail').Count)" skipped="$(@($Tests | Where-Object Result -eq 'Skip').Count)">
    <collection>
$($testNodes -join [Environment]::NewLine)
    </collection>
  </assembly>
</assemblies>
"@
      $resultFiles[$Directory] = Join-Path $target $fileName
      Set-Content -LiteralPath $resultFiles[$Directory] -Value $xml
      $package = if ($Assembly -eq 'Core.DeviceTests') {
        'com.microsoft.maui.core.devicetests'
      } else {
        'com.microsoft.maui.controls.devicetests'
      }
      $identityFiles[$Directory] = Join-Path $target 'run-identity.txt'
      Set-Content -LiteralPath $identityFiles[$Directory] -Value @(
        "RunKey: $runKey"
        "RunId: $runId"
        "InvocationId: $invocationId"
        "SourceVersion: $sourceVersion"
        "Project: $Assembly"
        "PackageName: $package"
        "Instrumentation: $package.TestInstrumentation"
        "Filter: Category=$Category"
        'DeviceSerial: emulator-5554'
        'DeviceArchitecture: x86_64'
        'ApiLevel: 36'
        "AvdName: Issue38080Adjacency_$($runKey.Replace('-', '_'))_$runId"
        "ResultFileName: $fileName"
        "StartedUtc: $started"
        "CompletedUtc: $([DateTimeOffset]::UtcNow.ToString('O'))"
        'Succeeded: True'
      )
    }

    $webViewType = 'Microsoft.Maui.DeviceTests.WebViewHandlerTests'
    Write-ResultFile -Directory core-webview -Assembly Core.DeviceTests -Category WebView -Tests @(
      @{ Type = $webViewType; Method = 'ClippingContainerSurvivesZeroDimensionThenNormalSize'; Arguments = '(initialWidth: 0, initialHeight: 100)'; Result = 'Pass'; Category = 'WebView' }
      @{ Type = $webViewType; Method = 'ClippingContainerSurvivesZeroDimensionThenNormalSize'; Arguments = '(initialWidth: 100, initialHeight: 0)'; Result = 'Pass'; Category = 'WebView' }
      @{ Type = $webViewType; Method = 'InitialOpacityIsAppliedToClippingContainer'; Result = 'Pass'; Category = 'WebView' }
      @{ Type = $webViewType; Method = 'DisconnectHandlerDestroysNativeWebView'; Result = 'Pass'; Category = 'WebView' }
    )
    Write-ResultFile -Directory core-view -Assembly Core.DeviceTests -Category View -Tests @(
      @{ Type = 'Microsoft.Maui.DeviceTests.ViewHandlerTests'; Method = 'OpacityIsPreservedAcrossNonWebViewContainerTransitions'; Result = 'Pass'; Category = 'View' }
    )
    Write-ResultFile -Directory controls-hybridwebview -Assembly Controls.DeviceTests -Category HybridWebView -Tests @(
      @{ Type = 'Microsoft.Maui.DeviceTests.HybridWebViewTests_Initialization'; Method = 'DisconnectHandlerRemovesClippingContainer'; Result = 'Pass'; Category = 'HybridWebView' }
    )
  }

  It 'accepts the six exact passing cases from three distinct bound invocations' {
    { Invoke-Guard } | Should -Not -Throw
  }

  It 'rejects a missing theory row' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-webview'] -Raw
    [void] $xml.assemblies.assembly.collection.RemoveChild($xml.assemblies.assembly.collection.test[0])
    $xml.Save($resultFiles['core-webview'])
    { Invoke-Guard } | Should -Throw '*expected 2 native case(s)*but found 1*'
  }

  It 'rejects duplicate theory arguments even when there are two passing rows' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-webview'] -Raw
    $xml.assemblies.assembly.collection.test[1].name = $xml.assemblies.assembly.collection.test[0].name
    $xml.Save($resultFiles['core-webview'])
    { Invoke-Guard } | Should -Throw '*distinct (0,100) and (100,0)*'
  }

  It 'rejects a substituted theory argument pair' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-webview'] -Raw
    $xml.assemblies.assembly.collection.test[1].name =
      "$webViewType.ClippingContainerSurvivesZeroDimensionThenNormalSize(initialWidth: 100, initialHeight: 100)"
    $xml.Save($resultFiles['core-webview'])
    { Invoke-Guard } | Should -Throw '*distinct (0,100) and (100,0)*'
  }

  It 'rejects a skipped required case' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-view'] -Raw
    $xml.assemblies.assembly.collection.test.result = 'Skip'
    $xml.Save($resultFiles['core-view'])
    { Invoke-Guard } | Should -Throw "*reported 'Skip' instead of 'Pass'*"
  }

  It 'rejects a result from the wrong assembly' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['controls-hybridwebview'] -Raw
    $xml.assemblies.assembly.name = 'Core.DeviceTests.dll'
    $xml.Save($resultFiles['controls-hybridwebview'])
    { Invoke-Guard } | Should -Throw "*expected assembly 'Controls.DeviceTests'*"
  }

  It 'rejects stale XML even when the identity is current' {
    (Get-Item -LiteralPath $resultFiles['core-webview']).LastWriteTimeUtc =
      (Get-Item -LiteralPath $runMarker).LastWriteTimeUtc.AddMinutes(-1)
    { Invoke-Guard } | Should -Throw '*not a fresh bounded regular file*'
  }

  It 'rejects a missing selected category trait' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-view'] -Raw
    $xml.assemblies.assembly.collection.test.traits.trait.value = 'WebView'
    $xml.Save($resultFiles['core-view'])
    { Invoke-Guard } | Should -Throw '*exactly one Category=View trait*'
  }

  It 'rejects a required identity duplicated in another category result' {
    [xml] $xml = Get-Content -LiteralPath $resultFiles['core-view'] -Raw
    $duplicate = $xml.CreateElement('test')
    $duplicate.SetAttribute('name', "$webViewType.InitialOpacityIsAppliedToClippingContainer")
    $duplicate.SetAttribute('type', $webViewType)
    $duplicate.SetAttribute('method', 'InitialOpacityIsAppliedToClippingContainer')
    $duplicate.SetAttribute('result', 'Pass')
    [void] $xml.assemblies.assembly.collection.AppendChild($duplicate)
    $xml.Save($resultFiles['core-view'])
    { Invoke-Guard } | Should -Throw '*expected 1 total native case(s)*but found 2*'
  }

  It 'rejects a copied old default result filename with a fresh host timestamp' {
    Move-Item -LiteralPath $resultFiles['core-view'] `
      -Destination (Join-Path (Split-Path $resultFiles['core-view']) 'testResults.xml')
    { Invoke-Guard } | Should -Throw '*expected one native result file for invocation*'
  }

  It 'rejects missing run identity' {
    Remove-Item -LiteralPath $identityFiles['core-view']
    { Invoke-Guard } | Should -Throw '*run identity is missing*'
  }

  It 'rejects a duplicate identity field' {
    Add-Content -LiteralPath $identityFiles['core-view'] -Value 'ApiLevel: 36'
    { Invoke-Guard } | Should -Throw '*unknown, duplicate, or malformed field*'
  }

  It 'rejects wrong invocation metadata: <Field>' -TestCases @(
    @{ Field = 'RunKey'; Value = '99999-1' }
    @{ Field = 'SourceVersion'; Value = ('b' * 40) }
    @{ Field = 'PackageName'; Value = 'com.unrelated.app' }
    @{ Field = 'Filter'; Value = 'Category=WebView' }
    @{ Field = 'ApiLevel'; Value = '30' }
    @{ Field = 'AvdName'; Value = 'Emulator_36' }
    @{ Field = 'Succeeded'; Value = 'False' }
  ) {
    param($Field, $Value)
    $content = Get-Content -LiteralPath $identityFiles['core-view'] -Raw
    Set-Content -LiteralPath $identityFiles['core-view'] -NoNewline -Value (
      $content -replace "(?m)^${Field}: [^\r\n]*", "${Field}: $Value")
    { Invoke-Guard } | Should -Throw '*does not match the selected invocation*'
  }

  It 'rejects a different device in the next category' {
    $content = Get-Content -LiteralPath $identityFiles['core-view'] -Raw
    Set-Content -LiteralPath $identityFiles['core-view'] -NoNewline -Value (
      $content.Replace('DeviceSerial: emulator-5554', 'DeviceSerial: emulator-5556'))
    { Invoke-Guard } | Should -Throw '*distinct invocations on one owned device*'
  }

  It 'rejects a reused invocation identifier' {
    $first = Get-Content -LiteralPath $identityFiles['core-webview']
    $invocation = ($first | Where-Object { $_.StartsWith('InvocationId: ') }).Substring(14)
    $content = Get-Content -LiteralPath $identityFiles['core-view'] -Raw
    $content = $content -replace '(?m)^InvocationId: [^\r\n]*', "InvocationId: $invocation"
    $content = $content -replace '(?m)^ResultFileName: [^\r\n]*', "ResultFileName: testResults-$invocation.xml"
    Set-Content -LiteralPath $identityFiles['core-view'] -NoNewline -Value $content
    { Invoke-Guard } | Should -Throw '*distinct invocations on one owned device*'
  }

  It 'rejects a second copy of the expected result' {
    $nested = Join-Path $resultsRoot 'core-view/duplicate'
    New-Item -ItemType Directory -Path $nested | Out-Null
    Copy-Item -LiteralPath $resultFiles['core-view'] -Destination $nested
    { Invoke-Guard } | Should -Throw '*expected one native result file for invocation*'
  }

  It 'rejects stale execution time even when identity and XML were copied recently' {
    $old = [DateTimeOffset]::UtcNow.AddHours(-1).ToString('O')
    $content = Get-Content -LiteralPath $identityFiles['core-view'] -Raw
    Set-Content -LiteralPath $identityFiles['core-view'] -NoNewline -Value (
      $content -replace '(?m)^StartedUtc: [^\r\n]*', "StartedUtc: $old")
    { Invoke-Guard } | Should -Throw '*timestamps are stale or invalid*'
  }

  It 'rejects XML document type declarations' {
    $content = Get-Content -LiteralPath $resultFiles['core-view'] -Raw
    Set-Content -LiteralPath $resultFiles['core-view'] -Value (
      '<!DOCTYPE assemblies [<!ENTITY unwanted "data">]>' + $content)
    { Invoke-Guard } | Should -Throw '*DTD*'
  }
}
