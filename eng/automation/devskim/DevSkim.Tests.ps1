#Requires -Modules Pester

Describe 'DevSkim banned-function gate' {
    BeforeAll {
        Import-Module (Join-Path $PSScriptRoot 'DevSkimReport.psm1') -Force

        function New-Result {
            param (
                [string[]] $Tags = @('API.DangerousAPI.BannedFunction'),
                [string] $Level = 'warning',
                [string] $Uri = 'src/native.c',
                [int] $Line = 12
            )

            @{
                ruleId     = 'DS154189'
                level      = $Level
                message    = @{ text = 'Banned C function detected' }
                locations  = @(@{ physicalLocation = @{ artifactLocation = @{ uri = $Uri }; region = @{ startLine = $Line } } })
                properties = @{ tags = $Tags }
            }
        }

        function New-Report {
            param ([object[]] $Results = @())

            @{ version = '2.1.0'; runs = @(@{ tool = @{ driver = @{ name = 'devskim' } }; results = $Results }) }
        }

        function Write-Report {
            param ($Report, [string] $Name = 'report.sarif')

            $path = Join-Path $TestDrive $Name
            ConvertTo-Json -InputObject $Report -Depth 10 | Set-Content -LiteralPath $path
            $path
        }
    }

    Describe 'Get-DevSkimBannedFindings' {
        It 'finds nothing in a report without results' {
            Get-DevSkimBannedFindings -Path (Write-Report (New-Report)) | Should -BeNullOrEmpty
        }

        It 'reports a banned function at severity <Level>' -ForEach @(
            @{ Level = 'error' }, @{ Level = 'warning' }, @{ Level = 'note' }, @{ Level = 'none' }
        ) {
            $findings = @(Get-DevSkimBannedFindings -Path (Write-Report (New-Report @(New-Result -Level $Level))))
            $findings.Count | Should -Be 1
        }

        It 'reports the rule, location and message of a finding' {
            $finding = Get-DevSkimBannedFindings -Path (Write-Report (New-Report @(New-Result -Uri 'src/native.c' -Line 12)))

            $finding.RuleId | Should -Be 'DS154189'
            $finding.Uri | Should -Be 'src/native.c'
            $finding.Line | Should -Be 12
            $finding.Message | Should -Be 'Banned C function detected'
        }

        It 'ignores results of other rules' {
            $results = @(
                New-Result -Tags @('API.DangerousAPI.ProblematicFunction')
                New-Result -Tags @()
            )

            Get-DevSkimBannedFindings -Path (Write-Report (New-Report $results)) | Should -BeNullOrEmpty
        }

        It 'reports the findings of every run' {
            $report = New-Report @(New-Result)
            $report.runs += (New-Report @(New-Result -Uri 'src/other.c')).runs

            @(Get-DevSkimBannedFindings -Path (Write-Report $report)).Uri | Should -Be 'src/native.c', 'src/other.c'
        }

        It 'reports a finding that has an inline suppression' {
            $result = New-Result
            $result.suppressions = @(@{ kind = 'inSource'; status = 'accepted' })

            @(Get-DevSkimBannedFindings -Path (Write-Report (New-Report @($result)))).Count | Should -Be 1
        }

        It 'rejects a report with <Problem>' -ForEach @(
            @{ Problem = 'a wrong SARIF version'; Json = '{"version":"2.0.0","runs":[{}]}' }
            @{ Problem = 'no runs'; Json = '{"version":"2.1.0","runs":[]}' }
            @{ Problem = 'a run from another tool'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"other"}},"results":[]}]}' }
            @{ Problem = 'no results array'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}}}]}' }
            @{ Problem = 'a result without a rule ID'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}},"results":[{"properties":{"tags":[]}}]}]}' }
            @{ Problem = 'a result without tags'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}},"results":[{"ruleId":"DS1"}]}]}' }
            @{ Problem = 'a result with non-string tags'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}},"results":[{"ruleId":"DS1","properties":{"tags":[1]}}]}]}' }
            @{ Problem = 'an unsuccessful execution'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}},"invocations":[{"executionSuccessful":false}],"results":[]}]}' }
            @{ Problem = 'an execution error'; Json = '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"devskim"}},"invocations":[{"toolExecutionNotifications":[{"level":"error"}]}],"results":[]}]}' }
            @{ Problem = 'invalid JSON'; Json = '{not json' }
            @{ Problem = 'no content'; Json = '' }
        ) {
            $path = Join-Path $TestDrive 'invalid.sarif'
            Set-Content -LiteralPath $path -Value $Json

            { Get-DevSkimBannedFindings -Path $path } | Should -Throw
        }

        It 'rejects a missing report' {
            { Get-DevSkimBannedFindings -Path (Join-Path $TestDrive 'missing.sarif') } | Should -Throw
        }
    }

    Describe 'Get-DevSkimAnalyzedFileCount' {
        It 'reads the count from the summary' {
            $log = Join-Path $TestDrive 'scan.log'
            Set-Content -LiteralPath $log -Value '[12:00:00 DBG] file:/src/a.c', '[12:00:01 DBG] Issues found: 3 in 7 files', '[12:00:01 DBG] Files analyzed: 7', '[12:00:01 DBG] Files skipped: 2'

            Get-DevSkimAnalyzedFileCount -LogPath $log | Should -Be 7
        }

        It 'reads a count of zero' {
            $log = Join-Path $TestDrive 'scan.log'
            Set-Content -LiteralPath $log -Value '[12:00:01 DBG] Files analyzed: 0'

            Get-DevSkimAnalyzedFileCount -LogPath $log | Should -Be 0
        }

        It 'rejects a log without the summary' {
            $log = Join-Path $TestDrive 'scan.log'
            Set-Content -LiteralPath $log -Value '[12:00:00 ERR] Failed to parse a.yml'

            { Get-DevSkimAnalyzedFileCount -LogPath $log } | Should -Throw
        }
    }

    Describe 'Invoke-DevSkimScan.ps1' -Skip:$IsWindows {
        BeforeAll {
            $script = Join-Path $PSScriptRoot 'Invoke-DevSkimScan.ps1'
            $repository = Join-Path $TestDrive 'repository'
            $null = New-Item -ItemType Directory -Path (Join-Path $repository 'src'), (Join-Path $repository 'eng')

            # A stand-in for devskim that copies the SARIF reports named in the environment and prints the summary.
            $devskim = Join-Path $TestDrive 'devskim'
            Set-Content -LiteralPath $devskim -Value @'
#!/bin/sh
if [ "$1" = "--version" ]; then echo "devskim ${FAKE_DEVSKIM_VERSION}+fake"; exit 0; fi
while [ $# -gt 0 ]; do
    case "$1" in
        --source-code) source="$2" ;;
        --output-file) output="$2" ;;
    esac
    shift
done
if [ -n "$FAKE_EXIT_CODE" ]; then exit "$FAKE_EXIT_CODE"; fi
case "$source" in
    */canary) cp "$FAKE_CANARY_REPORT" "$output"; echo "[00:00:00 DBG] Files analyzed: 3" ;;
    *) cp "$FAKE_TREE_REPORT" "$output"; echo "[00:00:00 DBG] Files analyzed: $FAKE_FILES_ANALYZED" ;;
esac
'@
            chmod +x $devskim

            $canary = New-Report @(
                New-Result -Uri 'canary.c'
                New-Result -Uri 'canary.cpp'
                New-Result -Uri 'canary.m'
            )
            $script:canaryReport = Write-Report $canary 'canary.sarif'
            $script:cleanReport = Write-Report (New-Report) 'clean.sarif'
            $script:bannedReport = Write-Report (New-Report @(New-Result)) 'banned.sarif'
        }

        BeforeEach {
            $env:FAKE_DEVSKIM_VERSION = '1.0.100'
            $env:FAKE_CANARY_REPORT = $script:canaryReport
            $env:FAKE_TREE_REPORT = $script:cleanReport
            $env:FAKE_FILES_ANALYZED = '25'
            $env:FAKE_EXIT_CODE = ''
            $output = Join-Path $TestDrive "output-$(New-Guid)"
        }

        AfterAll {
            Remove-Item Env:FAKE_DEVSKIM_VERSION, Env:FAKE_CANARY_REPORT, Env:FAKE_TREE_REPORT, Env:FAKE_FILES_ANALYZED, Env:FAKE_EXIT_CODE -ErrorAction Ignore
        }

        It 'passes when the canary is detected and the source trees are clean' {
            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Not -Throw

            Join-Path $output 'src.sarif' | Should -Exist
            Join-Path $output 'eng.log' | Should -Exist
        }

        It 'fails on a banned function in a source tree' {
            $env:FAKE_TREE_REPORT = $script:bannedReport

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Throw '*banned-function finding*'
        }

        It 'fails when DevSkim does not detect the canary' {
            $env:FAKE_CANARY_REPORT = $script:cleanReport

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Throw "*didn't report the banned strcpy call*"
        }

        It 'fails when DevSkim analyzed no files' {
            $env:FAKE_FILES_ANALYZED = '0'

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Throw '*analyzed no files*'
        }

        It 'fails for a DevSkim version it was not validated with' {
            $env:FAKE_DEVSKIM_VERSION = '1.0.101'

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Throw '*was validated with DevSkim*'
        }

        It 'fails when DevSkim fails' {
            $env:FAKE_EXIT_CODE = '134'

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository *> $null } | Should -Throw '*failed with exit code 134*'
        }

        It 'fails when the options file defines no globs' {
            $options = Join-Path $TestDrive 'no-globs.json'
            Set-Content -LiteralPath $options -Value '{ "Globs": [] }'

            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository -OptionsFile $options *> $null } | Should -Throw "*doesn't define the Globs*"
        }

        It 'fails when the options file is missing' {
            { & $script -DevSkim $devskim -OutputDirectory $output -RepositoryRoot $repository -OptionsFile (Join-Path $TestDrive 'missing.json') *> $null } | Should -Throw
        }
    }
}
