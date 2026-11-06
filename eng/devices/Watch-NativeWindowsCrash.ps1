[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ToolPath,
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [Parameter(Mandatory)][string]$WorkspaceRoot,
    [Parameter(Mandatory)][string]$ExecutableName,
    [Parameter(Mandatory)][int]$TaskProcessId
)

$ErrorActionPreference = 'Stop'

function Get-OwnedNativeTestProcess {
    param([int]$NativeProcessId, [int]$OwnerProcessId, [string]$Root, [string]$Name, [DateTime]$Started)

    $native = Get-CimInstance Win32_Process -Filter "ProcessId = $NativeProcessId"
    if (!$native -or !$native.ExecutablePath) { return }
    $rootPath = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar
    if (![IO.Path]::GetFullPath($native.ExecutablePath).StartsWith($rootPath, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($native.ExecutablePath) -ne $Name -or
        $native.CreationDate.ToUniversalTime() -lt $Started) { return }

    $ancestor = $native
    $seen = [Collections.Generic.HashSet[int]]::new()
    for ($depth = 0; $depth -lt 16 -and $ancestor; $depth++) {
        if ($ancestor.ProcessId -eq $OwnerProcessId) { return $native }
        if (!$seen.Add([int]$ancestor.ProcessId)) { return }
        $ancestor = Get-CimInstance Win32_Process -Filter "ProcessId = $($ancestor.ParentProcessId)"
    }
}

function Get-MtpNativeChildIds {
    param([string]$DiagnosticDirectory)

    foreach ($file in Get-ChildItem -LiteralPath $DiagnosticDirectory -Filter '*.diag' -File) {
        $reader = [IO.StreamReader]::new([IO.File]::Open(
            $file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite))
        try { $text = $reader.ReadToEnd() }
        finally { $reader.Dispose() }
        foreach ($match in [regex]::Matches($text, "Started test host process\. PID: '([0-9]+)'")) {
            [int]$match.Groups[1].Value
        }
    }
}

$stopFile = Join-Path $ResultsDirectory 'native-crash-monitor.stop'
$diagnostics = Join-Path $ResultsDirectory 'diagnostics'
$started = (Get-Item -LiteralPath $ResultsDirectory).CreationTimeUtc
$taskProcess = Get-Process -Id $TaskProcessId
$monitor = $null
try {
    while (!$taskProcess.HasExited -and !(Test-Path -LiteralPath $stopFile)) {
        if (Test-Path -LiteralPath $diagnostics) {
            foreach ($nativeProcessId in @(Get-MtpNativeChildIds -DiagnosticDirectory $diagnostics)) {
                $native = Get-OwnedNativeTestProcess -NativeProcessId $nativeProcessId `
                    -OwnerProcessId $TaskProcessId -Root $WorkspaceRoot -Name $ExecutableName -Started $started
                if (!$native) { continue }
                $dumpDirectory = Join-Path $ResultsDirectory "native-dumps/$($native.ProcessId)"
                New-Item -ItemType Directory -Force -Path $dumpDirectory | Out-Null
                $start = [Diagnostics.ProcessStartInfo]::new($ToolPath)
                $start.UseShellExecute = $false
                $start.CreateNoWindow = $true
                $start.RedirectStandardOutput = $true
                $start.RedirectStandardError = $true
                $start.Arguments = "-accepteula -mm -e -t $($native.ProcessId) `"$dumpDirectory`""
                $monitor = [Diagnostics.Process]::Start($start)
                if (!$monitor) { throw 'ProcDump did not start.' }
                $output = $monitor.StandardOutput.ReadToEndAsync()
                $errors = $monitor.StandardError.ReadToEndAsync()
                Write-Host "ProcDump PID $($monitor.Id) targets owned test child PID $($native.ProcessId)."
                $drain = $null
                while (!$monitor.WaitForExit(200)) {
                    if ($taskProcess.HasExited -or (Test-Path -LiteralPath $stopFile)) {
                        if (!$drain) { $drain = [Diagnostics.Stopwatch]::StartNew() }
                        if ($drain.Elapsed.TotalSeconds -ge 60) { throw 'ProcDump did not finish after the test task ended.' }
                    }
                }
                $output.GetAwaiter().GetResult() | Set-Content -LiteralPath (Join-Path $dumpDirectory 'procdump.stdout.log')
                $errors.GetAwaiter().GetResult() | Set-Content -LiteralPath (Join-Path $dumpDirectory 'procdump.stderr.log')
                if ($monitor.ExitCode -ne 0) { throw "ProcDump exited with code $($monitor.ExitCode)." }
                if (@(Get-ChildItem -LiteralPath $dumpDirectory -Filter '*.dmp' -File).Count -eq 0) {
                    throw 'ProcDump exited without producing a native dump.'
                }
                return
            }
        }
        Start-Sleep -Milliseconds 200
    }
    Write-Host 'No owned native test child was available for debugger capture.'
}
finally {
    if ($monitor) {
        if (!$monitor.HasExited) { $monitor.Kill(); $null = $monitor.WaitForExit(10000) }
        $monitor.Dispose()
    }
    $taskProcess.Dispose()
}
