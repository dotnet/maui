#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Fixed Android native harness probe' {
    BeforeAll {
        $tokens = $null
        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Replicate-Issue.ps1'),
            [ref]$tokens, [ref]$parseErrors)
        if ($parseErrors.Count -ne 0) {
            throw ($parseErrors.Message -join '; ')
        }
        $definition = $ast.Find({
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq 'Invoke-ReplicationAndroidNativeHarnessProbe'
        }, $false)
        if ($null -eq $definition) {
            throw 'The native harness probe helper is missing.'
        }
        . ([scriptblock]::Create($definition.Extent.Text))

        function Assert-ReplicationTrustedTree { param($Context) }
        function Assert-InitialReplicationWorktree { }
        function Invoke-ReplicationTrustedRestore {
            param($Target, $Verb = 'restore', $AdditionalArguments)
        }
        function Restore-TrackedVerificationSideEffects {
            param([AllowEmptyCollection()][string[]]$PreservedFiles)
        }
        function Invoke-LoggedChildProcess {
            param($ScriptPath, $Arguments, $LogPath, $Description,
                [switch]$AllowDeviceControl, $TimeoutSeconds)
        }
    }

    BeforeEach {
        $script:repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $script:trustedScripts = $PSScriptRoot
        $script:trustedSkills = Join-Path $PSScriptRoot '../skills'
        $script:sandboxArtifactDir = Join-Path $repoRoot 'diagnostics'
        $script:DeviceUdid = 'emulator-5554'
        $script:fixtureTarget = Join-Path $repoRoot (
            'src/Controls/tests/DeviceTests/Elements/Button/ReplicationAndroidButtonHarnessProbe.Android.cs')
        $null = New-Item -ItemType Directory -Path (
            Split-Path -Parent $fixtureTarget) -Force
        $script:probeEvents = [Collections.Generic.List[string]]::new()
        Mock Assert-ReplicationTrustedTree { }
        Mock Assert-InitialReplicationWorktree {
            Test-Path -LiteralPath $script:fixtureTarget | Should -BeFalse
        }
        Mock Invoke-ReplicationTrustedRestore {
            Test-Path -LiteralPath $script:fixtureTarget | Should -BeFalse
            $Target | Should -BeLike '*/src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
            $script:probeEvents.Add($Verb)
        }
        Mock Restore-TrackedVerificationSideEffects {
            Test-Path -LiteralPath $script:fixtureTarget | Should -BeFalse
            $PreservedFiles | Should -BeNullOrEmpty
            $script:probeEvents.Add('cleanup')
        }
        Mock Invoke-LoggedChildProcess {
            $script:probeEvents.Add('run')
            Test-Path -LiteralPath $script:fixtureTarget | Should -BeTrue
            $ScriptPath | Should -BeLike '*/run-device-tests/scripts/Run-DeviceTests.ps1'
            $AllowDeviceControl | Should -BeTrue
            $TimeoutSeconds | Should -Be 1800
            $Arguments | Should -Contain '-NoRestore'
            $Arguments | Should -Contain 'Microsoft.Maui.DeviceTests.ReplicationAndroidButtonHarnessProbe'
            $Arguments | Should -Contain 'RegisteredButtonAttachesToWindow'
            $Arguments | Should -Not -Contain '-BuildOnly'
            $Arguments | Should -Not -Contain '-PreflightXHarnessOnly'
        }
    }

    It 'prewarms the clean baseline then runs the fixed fixture through the isolated selected-test runner' {
        Invoke-ReplicationAndroidNativeHarnessProbe
        ($script:probeEvents -join ',') | Should -BeExactly 'restore,build,cleanup,run'
        Test-Path -LiteralPath $fixtureTarget | Should -BeFalse
        Should -Invoke Assert-InitialReplicationWorktree -Times 2 -Exactly
        Should -Invoke Invoke-ReplicationTrustedRestore -Times 1 -Exactly -ParameterFilter {
            $Verb -ceq 'build' -and $AdditionalArguments -contains '--no-restore'
        }
        Should -Invoke Invoke-LoggedChildProcess -Times 1 -Exactly
        Should -Invoke Restore-TrackedVerificationSideEffects -Times 1 -Exactly
        Should -Invoke Assert-ReplicationTrustedTree -Times 1 -Exactly -ParameterFilter {
            $Context -ceq 'after native harness probe'
        }
    }

    It 'does not stage or execute the fixture when trusted build cleanup rejects a path' {
        Mock Restore-TrackedVerificationSideEffects {
            throw 'Verification created an unexpected untracked repository path: unexpected.cs'
        }
        { Invoke-ReplicationAndroidNativeHarnessProbe } |
            Should -Throw '*unexpected untracked repository path: unexpected.cs*'
        Test-Path -LiteralPath $fixtureTarget | Should -BeFalse
        Should -Invoke Invoke-LoggedChildProcess -Times 0 -Exactly
        Should -Invoke Assert-InitialReplicationWorktree -Times 1 -Exactly
    }

    It 'retains the native runner failure and removes only its owned fixture' {
        Mock Invoke-LoggedChildProcess { throw 'APP_CRASH exit 80: retained native failure' }
        { Invoke-ReplicationAndroidNativeHarnessProbe } |
            Should -Throw '*APP_CRASH exit 80: retained native failure*'
        Test-Path -LiteralPath $fixtureTarget | Should -BeFalse
        Should -Invoke Assert-ReplicationTrustedTree -Times 1 -Exactly -ParameterFilter {
            $Context -ceq 'after native harness probe'
        }
    }

    It 'refuses to overwrite an existing fixture before restore or execution' {
        Set-Content -LiteralPath $fixtureTarget -Value 'existing source' -NoNewline
        { Invoke-ReplicationAndroidNativeHarnessProbe } |
            Should -Throw '*overwrite an existing source file*'
        Get-Content -LiteralPath $fixtureTarget -Raw | Should -BeExactly 'existing source'
        Should -Invoke Invoke-ReplicationTrustedRestore -Times 0 -Exactly
        Should -Invoke Invoke-LoggedChildProcess -Times 0 -Exactly
    }
}
