#requires -Version 5
param(
    [Parameter(Mandatory = $true)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$resultFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter 'TestResults-*.xml' -File)
if ($resultFiles.Count -eq 0) {
    throw "No device-test result files found in '$ResultsDirectory'."
}

$mergedDoc = New-Object System.Xml.XmlDocument
$assembliesNode = $mergedDoc.CreateElement('assemblies')
$mergedDoc.AppendChild($assembliesNode) | Out-Null
$exitCode = 0

foreach ($file in $resultFiles) {
    $reader = $null
    try {
        $settings = New-Object System.Xml.XmlReaderSettings
        $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [System.Xml.XmlReader]::Create($file.FullName, $settings)
        $doc = New-Object System.Xml.XmlDocument
        $doc.XmlResolver = $null
        $doc.Load($reader)
        $nodes = $doc.SelectNodes('/assemblies/assembly')
        if ($nodes.Count -eq 0) {
            throw 'No xUnit assembly results were found.'
        }

        foreach ($node in $nodes) {
            foreach ($attribute in @('total', 'passed', 'failed', 'skipped')) {
                $count = 0
                if (-not [int]::TryParse($node.GetAttribute($attribute), [ref] $count) -or $count -lt 0) {
                    throw "Invalid or missing '$attribute' test count."
                }
            }

            if ([int] $node.total -ne ([int] $node.passed + [int] $node.failed + [int] $node.skipped)) {
                throw 'Test outcome counts do not add up to the total.'
            }

            if ($node.SelectNodes('collection/test').Count -ne [int] $node.total) {
                throw 'The reported total does not match the actual test results.'
            }

            foreach ($outcome in @{ passed = 'Pass'; failed = 'Fail'; skipped = 'Skip' }.GetEnumerator()) {
                if ($node.SelectNodes("collection/test[@result='$($outcome.Value)']").Count -ne
                    [int] $node.GetAttribute($outcome.Key)) {
                    throw "The reported '$($outcome.Key)' count does not match the actual test results."
                }
            }

            $errors = 0
            if ($node.HasAttribute('errors') -and
                (-not [int]::TryParse($node.GetAttribute('errors'), [ref] $errors) -or $errors -lt 0)) {
                throw "Invalid 'errors' count."
            }

            if ([int] $node.failed -gt 0 -or $errors -gt 0 -or $node.SelectNodes('.//failure | errors/error').Count -gt 0) {
                Write-Host "ERROR: Test failures or assembly errors in $($file.Name)."
                $exitCode = 1
            }

            $assembliesNode.AppendChild($mergedDoc.ImportNode($node, $true)) | Out-Null
        }
    }
    catch {
        Write-Host "ERROR: Failed to parse $($file.Name): $_"
        $exitCode = 1
    }
    finally {
        if ($null -ne $reader) {
            $reader.Dispose()
        }
    }
}

$mergedDoc.Save((Join-Path $ResultsDirectory 'testResults.xml'))
Write-Host "Created merged testResults.xml from $($assembliesNode.ChildNodes.Count) assembly result(s)."
exit $exitCode
