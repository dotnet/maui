function Get-XunitMtpFilterArguments {
    param([string]$Filter)

    if ($Filter -match '[\r\n\x00]') { throw 'Test filters cannot contain control characters.' }
    if (-not ('MtpTestArguments' -as [type])) {
        Add-Type -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'MtpTestArguments.cs'))
    }
    return [MtpTestArguments]::DesktopFilter($Filter, $false)
}

function Read-XunitMtpResults {
    param([Parameter(Mandatory)][string]$Path)

    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
    }
    finally { $reader.Dispose() }

    $rows = @($document.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']"))
    if ($rows.Count -eq 0) { throw "No tests were recorded in '$Path'." }
    $summary = [ordered]@{ Total = $rows.Count; Passed = 0; Failed = 0; Skipped = 0; Errors = 0 }
    foreach ($row in $rows) {
        switch ($row.GetAttribute('outcome')) {
            'Passed' { $summary.Passed++ }
            'Failed' { $summary.Failed++ }
            'NotExecuted' { $summary.Skipped++ }
            default { $summary.Errors++ }
        }
    }
    return [pscustomobject]$summary
}

function Invoke-XunitMtpProcess {
    param([string[]]$TestArguments)

    $saved = @{}
    try {
        foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
            $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        & dotnet @TestArguments 2>&1
        $script:XunitMtpExitCode = $LASTEXITCODE
    }
    finally {
        foreach ($name in $saved.Keys) {
            [Environment]::SetEnvironmentVariable($name, $saved[$name])
        }
    }
}
