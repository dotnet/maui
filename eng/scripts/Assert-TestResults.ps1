[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$files = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -Recurse -File)
if ($files.Count -eq 0) {
    throw "No real TRX results were produced in '$ResultsDirectory'."
}

foreach ($file in $files) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($file.FullName, $settings)
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
    }
    finally {
        $reader.Dispose()
    }

    $summary = $document.SelectSingleNode("/*[local-name()='TestRun']/*[local-name()='ResultSummary']")
    $counters = $document.SelectSingleNode("/*[local-name()='TestRun']/*[local-name()='ResultSummary']/*[local-name()='Counters']")
    $results = $document.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']")
    if (!$counters -or [int]$counters.executed -le 0 -or $results.Count -eq 0) {
        throw "TRX '$($file.FullName)' contains no executed tests."
    }
    if ([int]$counters.failed -gt 0 -or [int]$counters.error -gt 0 -or
        [int]$counters.aborted -gt 0 -or [int]$counters.timeout -gt 0 -or
        $summary.outcome -notin @('Completed', 'Passed')) {
        throw "TRX '$($file.FullName)' reports failed or incomplete tests."
    }
}
