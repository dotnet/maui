function Get-ConsoleFailureReason {
    param([string]$Console)

    $out = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $false; isIncomplete = $false }
    if ([string]::IsNullOrWhiteSpace($Console)) { return $out }

    $reasonLines = New-Object System.Collections.Generic.List[string]
    foreach ($ln in ($Console -split "`r?`n")) {
        if ([string]::IsNullOrWhiteSpace($ln)) { continue }
        if ($ln -match '(?i)timeout|timed out') { $out.isTimeout = $true }
        if ($ln -match '(?i)crash|unhandled exception|fatal error|core dumped|\bsegfault\b|\.dmp\b') { $out.isCrash = $true }
        if ($ln -match '(?i)timeout waiting|timed out|did not (complete|finish)|unhandled exception|fatal error|core dumped|\bsegfault\b|segmentation fault|\.dmp\b|test execution completed with exit code:\s*[1-9]|process (was )?killed|\bhang(ing|s)?\b') {
            $t = $ln.Trim()
            if ($t.Length -gt 300) { $t = $t.Substring(0, 300) }
            if (-not $reasonLines.Contains($t)) { $reasonLines.Add($t) }
        }
    }
    if ($reasonLines.Count -gt 0) {
        $out.isIncomplete = $true
        $out.reason = (@($reasonLines) | Select-Object -First 5) -join ' | '
    }
    return $out
}

function New-DeviceWorkItemFailureRecords {
    param(
        [object]$Trx,
        [object]$Console,
        [bool]$HasDump,
        [bool]$AnyResultFile,
        [hashtable]$Context,
        [bool]$ResultReadIncomplete = $false
    )

    $records = New-Object System.Collections.Generic.List[object]
    $failedTests = if ($null -ne $Trx) { @($Trx.failedTests) } else { @() }
    $namedFailures = 0
    foreach ($ft in $failedTests) {
        if ([string]::IsNullOrWhiteSpace([string]$ft.name)) { continue }
        $records.Add([ordered]@{
            testName = [string]$ft.name
            source = 'helix-trx'
            deviceTestRealFailure = $true
        })
        $namedFailures++
    }

    $consoleIncomplete = ($null -ne $Console) -and [bool]$Console.isIncomplete
    $consoleCrash = ($null -ne $Console) -and [bool]$Console.isCrash
    $incomplete = $consoleIncomplete -or $consoleCrash -or $HasDump -or (-not $AnyResultFile) -or $ResultReadIncomplete -or ($namedFailures -eq 0)

    if ($incomplete) {
        $records.Add([ordered]@{
            testName = "device-test work item incomplete ($($Context.helixWorkItem))"
            source = 'helix-workitem-incomplete'
            deviceTestIncomplete = $true
        })
    }

    return $records.ToArray()
}
