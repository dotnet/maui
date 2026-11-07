#Requires -Modules Pester

Describe 'Direct test result validation' {
    BeforeAll {
        $scriptPath = Join-Path $PSScriptRoot 'Assert-TestResults.ps1'
        function Write-TestTrx {
            param(
                [string] $Directory,
                [int] $Executed = 1,
                [int] $Failed = 0,
                [string] $Outcome = 'Completed'
            )
            New-Item -ItemType Directory -Force -Path $Directory | Out-Null
            @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testName="Real reporter result" outcome="Passed" /></Results>
  <ResultSummary outcome="$Outcome"><Counters executed="$Executed" failed="$Failed" error="0" aborted="0" timeout="0" /></ResultSummary>
</TestRun>
"@ | Set-Content -LiteralPath (Join-Path $Directory 'report.trx')
        }
    }

    It 'accepts a real namespaced nonempty successful TRX' {
        $directory = Join-Path $TestDrive 'passing'
        Write-TestTrx $directory
        { & $scriptPath -ResultsDirectory $directory } | Should -Not -Throw
    }

    It 'rejects missing result directories' {
        { & $scriptPath -ResultsDirectory (Join-Path $TestDrive 'absent') } | Should -Throw
    }

    It 'rejects missing TRX even when other logs exist' {
        $directory = Join-Path $TestDrive 'logs-only'
        New-Item -ItemType Directory -Path $directory | Out-Null
        'Passed' | Set-Content (Join-Path $directory 'stdout.log')
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw '*No real TRX*'
    }

    It 'rejects unexpectedly empty discovery or all-skipped execution' {
        $directory = Join-Path $TestDrive 'empty'
        Write-TestTrx $directory -Executed 0
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw '*no executed tests*'
    }

    It 'rejects failed tests independently of launcher success' {
        $directory = Join-Path $TestDrive 'failed'
        Write-TestTrx $directory -Failed 1
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw '*failed or incomplete*'
    }

    It 'rejects a run-level abort even if individual tests passed' {
        $directory = Join-Path $TestDrive 'aborted'
        Write-TestTrx $directory -Outcome 'Aborted'
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw '*failed or incomplete*'
    }

    It 'rejects malformed TRX' {
        $directory = Join-Path $TestDrive 'malformed'
        New-Item -ItemType Directory -Path $directory | Out-Null
        '<TestRun>' | Set-Content (Join-Path $directory 'report.trx')
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw
    }

    It 'rejects external XML entity declarations' {
        $directory = Join-Path $TestDrive 'dtd'
        New-Item -ItemType Directory -Path $directory | Out-Null
        '<!DOCTYPE TestRun [<!ENTITY file SYSTEM "file:///not-readable">]><TestRun>&file;</TestRun>' |
            Set-Content (Join-Path $directory 'report.trx')
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw
    }

    It 'validates every report, not only the first matching result' {
        $directory = Join-Path $TestDrive 'multiple'
        Write-TestTrx $directory
        Write-TestTrx (Join-Path $directory 'second-framework') -Failed 1
        { & $scriptPath -ResultsDirectory $directory } | Should -Throw '*failed or incomplete*'
    }
}
