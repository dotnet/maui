#requires -Version 5
param(
    [Parameter(Mandatory = $true)][string]$ResultsDirectory
)

$ErrorActionPreference = 'Stop'

function Add-ResultFailure {
    param([xml]$Document, [string]$Name, [string]$Message)

    Write-Host "[FAIL] ${Name}: $Message"
    $failure = [xml]@'
<assembly name="Windows device-test results" total="1" passed="0" failed="1" skipped="0" errors="0" time="0">
  <collection name="Result validation" total="1" passed="0" failed="1" skipped="0" time="0">
    <test name="" type="WindowsDeviceTestRunner" method="ValidateResults" result="Fail" time="0">
      <failure exception-type="System.IO.InvalidDataException"><message /></failure>
    </test>
  </collection>
</assembly>
'@
    $failure.SelectSingleNode('//test').SetAttribute('name', $Name)
    $failure.SelectSingleNode('//message').InnerText = $Message
    $Document.DocumentElement.AppendChild($Document.ImportNode($failure.DocumentElement, $true)) | Out-Null
}

$merged = [xml]'<assemblies />'
$hasFailures = $false
$files = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter 'TestResults-*.xml' -File | Sort-Object Name)
if ($files.Count -eq 0) {
    Add-ResultFailure $merged 'Missing test results' 'No test result files found. All test processes may have crashed.'
    $hasFailures = $true
}

foreach ($file in $files) {
    try {
        $settings = New-Object System.Xml.XmlReaderSettings
        $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [System.Xml.XmlReader]::Create($file.FullName, $settings)
        try {
            $document = New-Object System.Xml.XmlDocument
            $document.XmlResolver = $null
            $document.Load($reader)
        } finally {
            $reader.Dispose()
        }

        $assemblies = $document.SelectNodes('/assemblies/assembly')
        if ($assemblies.Count -eq 0) {
            throw 'The result file contains no xUnit assemblies.'
        }

        foreach ($assembly in $assemblies) {
            $counts = @{}
            foreach ($attribute in @('total', 'passed', 'failed', 'skipped', 'errors')) {
                $count = 0
                if (-not [int]::TryParse($assembly.GetAttribute($attribute), [ref]$count) -or $count -lt 0) {
                    throw "The assembly has an invalid '$attribute' counter."
                }
                $counts[$attribute] = $count
            }
            if ($counts.total -ne $assembly.SelectNodes('collection/test').Count) {
                throw 'The assembly test count does not match its reported test outcomes.'
            }
            foreach ($outcome in @{ Pass = 'passed'; Fail = 'failed'; Skip = 'skipped' }.GetEnumerator()) {
                if ($counts[$outcome.Value] -ne $assembly.SelectNodes("collection/test[@result='$($outcome.Key)']").Count) {
                    throw "The assembly '$($outcome.Value)' counter does not match its reported test outcomes."
                }
            }
            if ($counts.total -ne ($counts.passed + $counts.failed + $counts.skipped)) {
                throw 'The assembly counters do not add up to its total test count.'
            }
        }

        foreach ($assembly in $assemblies) {
            $merged.DocumentElement.AppendChild($merged.ImportNode($assembly, $true)) | Out-Null
        }
        if ($document.SelectNodes('/assemblies/assembly[@failed > 0 or @errors > 0] | //errors/error').Count -gt 0) {
            Write-Host "[FAIL] Test failures or assembly/collection errors in $($file.Name)"
            $hasFailures = $true
        }
    } catch {
        Add-ResultFailure $merged $file.Name "Test run did not finish: invalid test results. $($_.Exception.Message)"
        $hasFailures = $true
    }
}

if (-not $hasFailures -and $merged.SelectNodes('//test').Count -eq 0) {
    Add-ResultFailure $merged 'No tests reported' 'Test run did not finish: every category reported zero tests.'
    $hasFailures = $true
}

foreach ($dump in Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.dmp' -File) {
    Add-ResultFailure $merged $dump.Name 'A crash dump was produced; completed test results do not establish a successful run.'
    $hasFailures = $true
}

$merged.Save((Join-Path $ResultsDirectory 'testResults.xml'))
Write-Host "Created merged testResults.xml from $($files.Count) result file(s)"
exit ([int]$hasFailures)
