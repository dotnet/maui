#Requires -Modules Pester

Describe 'Direct native CI contracts' {
    BeforeAll {
        $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
        $stage = Get-Content (Join-Path $repoRoot 'eng/pipelines/arcade/stage-device-tests.yml') -Raw
        $steps = Get-Content (Join-Path $repoRoot 'eng/pipelines/common/device-tests-steps.yml') -Raw
        $deviceScripts = Join-Path $repoRoot '.github/skills/run-device-tests/scripts'
        $start = Get-Content (Join-Path $deviceScripts 'Start-TestDevice.ps1') -Raw
        $stop = Get-Content (Join-Path $deviceScripts 'Stop-TestDevice.ps1') -Raw
    }

    It 'retains all five executed native suites' {
        @([regex]::Matches($stage, '(?m)^  - name: (controls|core|graphics|essentials|blazorwebview)$')).Count |
            Should -Be 5
    }

    It 'retains both mobile runtimes and CoreCLR-only Catalyst' {
        $stage | Should -Match '(?s)name: coreclr\s+useCoreClr: true'
        $stage | Should -Match '(?s)name: mono\s+useCoreClr: false'
        $stage | Should -Match '(?s)platform: catalyst.*?useCoreClr: true'
    }

    It 'retains mutually exclusive iOS Controls shards and Windows packaging modes' {
        $stage | Should -Match 'CollectionView;Shell;HybridWebView;General'
        $stage | Should -Match 'SkipCategories=CollectionView,Shell,HybridWebView'
        $stage | Should -Match "split\('packaged;unpackaged'"
        $steps | Should -Match '_MauiDeviceTestUnpackaged=true'
        $steps | Should -Match 'Prepare unique MSIX identity'
        $steps | Should -Match 'Install signed MSIX'
        $steps | Should -Match 'MauiTestPackaged=true'
        $steps | Should -Not -Match 'BLOCKED|Invoke-CommandInDesktopPackage'
        $steps | Should -Match "eq\(parameters.device, 'unpackaged'\)"
    }

    It 'collects real iOS container results before removing its owned simulator' {
        $steps | Should -Match 'simctl get_app_container \$state.Device'
        $steps | Should -Match 'Documents/TestResults'
        $steps | Should -Match 'testResults.trx'
        $steps.IndexOf('simctl get_app_container') | Should -BeLessThan $steps.IndexOf('Stop-TestDevice.ps1')
        $iosCommand = [regex]::Match($steps, '(?s)displayName: Run iOS tests directly.*?useExitCodeForErrors: true').Value
        $iosCommand | Should -Not -Match '--report-trx|--results-directory|MauiTestResultsDirectory'
    }

    It 'does not swallow failing subprocess exits or absent results' {
        @([regex]::Matches($steps, 'continueOnError: false')).Count | Should -Be 6
        $steps | Should -Match 'Assert-TestResults.ps1'
        $steps | Should -Match 'failTaskOnMissingResultsFile: true'
        $steps | Should -Match 'failTaskOnFailedTests: true'
    }

    It 'creates a job-owned simulator and uses only its recorded UDID in cleanup' {
        $start | Should -Match 'simctl create'
        $start | Should -Match 'simctl bootstatus \$state.Device -b'
        $stop | Should -Match 'simctl delete \$state.Device'
        $stop | Should -Not -Match 'delete unavailable|shutdown all|booted'
    }

    It 'selects the exact owned Android serial and never stops processes by name' {
        $start | Should -Match 'emulator-\$port'
        $start | Should -Match 'sys.boot_completed'
        $steps | Should -Match '--device "\$\(TestDevice\)"'
        $stop | Should -Match '-s \$state.Device emu kill'
        $stop | Should -Not -Match 'killall|pkill|Stop-Process -Name'
    }

    It 'has no submission or monitor path' {
        $stage + $steps | Should -Not -Match 'send-to-helix|helix_xharness|HelixAccessToken|HelixJobMonitor|xharness'
    }

    It 'allows explicit Windows 11 client validation without mislabeling the configured server pool' {
        $android = [regex]::Match($stage, '(?s)platform: android.*?expectedArchitecture: X64').Value
        $android | Should -Not -Match 'requireWindows11'
        $stage | Should -Match 'requireWindows11: false'
        $stage | Should -Match ([regex]::Escape('requireWindows11: ${{ parameters.requireWindows11 }}'))
        $stage | Should -Not -Match 'displayName: Windows 11'
        $start | Should -Match ([regex]::Escape('if (![Environment]::UserInteractive)'))
        $start | Should -Match ([regex]::Escape('if ($RequireWindows11)'))
        $start | Should -Match ([regex]::Escape('$os.ProductType -ne 1'))
    }

    It 'parses owned PowerShell files without errors' {
        foreach ($file in @('Start-TestDevice.ps1', 'Stop-TestDevice.ps1')) {
            $errors = $null
            [System.Management.Automation.Language.Parser]::ParseFile(
                (Join-Path $deviceScripts $file), [ref]$null, [ref]$errors) | Out-Null
            $errors | Should -BeNullOrEmpty
        }
    }
}
