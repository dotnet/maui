function Get-NUnitMtpArguments {
    param(
        [string]$Project,
        [string]$Configuration,
        [string]$Filter,
        [string]$ResultsDirectory,
        [string]$TrxFileName,
        [string]$NUnitDirectory,
        [switch]$NoBuild
    )

    [void][System.IO.Directory]::CreateDirectory($NUnitDirectory)
    $settings = Join-Path $NUnitDirectory 'nunit.runsettings'
    $document = [System.Xml.Linq.XDocument]::Parse('<RunSettings><NUnit /></RunSettings>')
    $document.Root.Element('NUnit').SetElementValue('TestOutputXml', [System.IO.Path]::GetFullPath($NUnitDirectory))
    $document.Save($settings)
    $arguments = @('--project', $Project, '--configuration', $Configuration,
        '--report-trx', '--report-trx-filename', $TrxFileName,
        '--results-directory', $ResultsDirectory, '--settings', $settings)
    if ($Filter) { $arguments += @('--filter', $Filter) }
    if ($NoBuild) { $arguments += '--no-build' }
    return $arguments
}

function Complete-NUnitMtpAttempt {
    param(
        [string]$RepoRoot,
        [string]$TrxPath,
        [string]$NUnitDirectory,
        [int]$ExitCode
    )

    if (-not ('UITestRetry' -as [type])) {
        # Use the helper from this script's trusted snapshot, never the PR checkout.
        Add-Type -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'UITestRetry.cs'))
    }
    $trx = [System.Xml.Linq.XDocument]::Load($TrxPath)
    $rawDirectory = Join-Path $NUnitDirectory 'raw'
    [void][System.IO.Directory]::CreateDirectory($rawDirectory)
    [UITestRetry]::PreserveAttachments($trx, (Split-Path $TrxPath), (Join-Path $rawDirectory 'attachments'))
    $trx.Save($TrxPath)
    Copy-Item -LiteralPath $TrxPath -Destination $rawDirectory
    $files = @(Get-ChildItem -LiteralPath $NUnitDirectory -Filter '*.xml' -File)
    if ($files.Count -eq 0) { throw 'No NUnit XML was produced; fixture failures cannot be reconciled.' }
    [System.Xml.Linq.XDocument[]]$reports = @($files | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $rawDirectory
        [System.Xml.Linq.XDocument]::Load($_.FullName)
    })
    $nunitFailed = [UITestRetry]::ApplyNUnitFailures($trx, $reports)
    $trx.Save($TrxPath)
    return [pscustomobject]@{
        ExitCode = if ($ExitCode -ne 0) { $ExitCode } elseif ($nunitFailed) { 1 } else { 0 }
        Reports = $reports
    }
}

function Invoke-NUnitMtpProcess {
    param([string[]]$TestArguments)

    $saved = @{}
    try {
        foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
            $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        & dotnet test @TestArguments 2>&1
        $script:NUnitMtpExitCode = $LASTEXITCODE
    }
    finally {
        foreach ($name in $saved.Keys) {
            [Environment]::SetEnvironmentVariable($name, $saved[$name])
        }
    }
}
