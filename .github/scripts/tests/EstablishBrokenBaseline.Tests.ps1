#!/usr/bin/env pwsh
<#
    Pester coverage for EstablishBrokenBaseline.ps1's snapshot mode.

    Snapshot mode is what lets try-fix run against an issue replication, where
    there is no author fix to revert. These tests exercise the real script
    against a real git repository, because every guarantee it makes -- no
    revert, restorable scope, byte-identical restore -- is a statement about
    git's behaviour, not about PowerShell's.
#>

BeforeAll {
    $script:ScriptSource = Join-Path $PSScriptRoot '..' 'EstablishBrokenBaseline.ps1'
    $script:ScriptSource = (Resolve-Path -LiteralPath $script:ScriptSource).Path

    # The script refuses to run when the working tree is dirty and reverts real
    # files when it does run, so every test gets a throwaway repository.
    function New-ScratchRepository {
        param([hashtable]$Files = @{ 'src/Handler.cs' = "BROKEN ORIGINAL`n" })

        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("ebb-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $root -Force | Out-Null

        Push-Location $root
        try {
            git init -q . 2>&1 | Out-Null
            git config user.email 'test@example.com' 2>&1 | Out-Null
            git config user.name 'Test' 2>&1 | Out-Null
            git config commit.gpgsign false 2>&1 | Out-Null

            New-Item -ItemType Directory -Path (Join-Path $root '.github/scripts') -Force | Out-Null
            Copy-Item -LiteralPath $script:ScriptSource -Destination (Join-Path $root '.github/scripts') -Force

            foreach ($relative in $Files.Keys) {
                $target = Join-Path $root $relative
                New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
                Set-Content -LiteralPath $target -Value $Files[$relative] -NoNewline
            }

            git add -A 2>&1 | Out-Null
            git commit -qm 'init' 2>&1 | Out-Null
        } finally {
            Pop-Location
        }

        return $root
    }

    # '&' makes $MyInvocation.InvocationName '&', which the script treats as a
    # dot-source and returns early from. try-fix invokes it as a file, so the
    # tests must too or they would exercise nothing at all.
    function Invoke-BaselineScript {
        param(
            [string]$Root,
            [string[]]$ScriptArguments = @(),
            [hashtable]$Environment = @{}
        )

        $previous = @{}
        foreach ($key in $Environment.Keys) {
            $previous[$key] = [Environment]::GetEnvironmentVariable($key)
            [Environment]::SetEnvironmentVariable($key, $Environment[$key])
        }

        Push-Location $Root
        try {
            $arguments = @('-NoProfile', '-File', (Join-Path $Root '.github/scripts/EstablishBrokenBaseline.ps1')) + $ScriptArguments
            $output = & pwsh @arguments 2>&1 | Out-String
            return $output
        } finally {
            Pop-Location
            foreach ($key in $Environment.Keys) {
                [Environment]::SetEnvironmentVariable($key, $previous[$key])
            }
        }
    }

    function New-ScopeFile {
        param([string[]]$Files, [string]$Raw)

        $path = Join-Path ([System.IO.Path]::GetTempPath()) ("scope-" + [Guid]::NewGuid().ToString('N') + '.json')
        if ($PSBoundParameters.ContainsKey('Raw')) {
            Set-Content -LiteralPath $path -Value $Raw
        } else {
            Set-Content -LiteralPath $path -Value (@{ files = $Files } | ConvertTo-Json)
        }
        return $path
    }

    function Get-StatePath {
        param([string]$Root)
        return (Join-Path $Root '.github/.baseline-state.json')
    }
}

Describe 'Snapshot mode records a scope without reverting anything' {
    BeforeEach {
        $script:Repo = New-ScratchRepository
        $script:Target = Join-Path $script:Repo 'src/Handler.cs'
    }

    AfterEach {
        if ($script:Repo -and (Test-Path $script:Repo)) {
            Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'leaves the tree byte-for-byte untouched' {
        $before = Get-FileHash -LiteralPath $script:Target -Algorithm SHA256

        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly') | Out-Null

        $after = Get-FileHash -LiteralPath $script:Target -Algorithm SHA256
        $after.Hash | Should -Be $before.Hash
    }

    It 'writes the scope where try-fix looks for its editable files' {
        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly') | Out-Null

        $state = Get-Content -LiteralPath (Get-StatePath $script:Repo) -Raw | ConvertFrom-Json
        @($state.RevertedFiles) | Should -Be @('src/Handler.cs')
        $state.Mode | Should -Be 'snapshot'
    }

    It 'skips merge-base detection, and so skips its network fetches' {
        $output = Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly')

        $output | Should -Not -Match 'Detecting base branch'
        $output | Should -Not -Match 'scanning remote branches'
    }

    It 'reports that it reverted nothing' {
        $output = Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly')

        $output | Should -Match '0 reverted'
    }

    It 'records nothing when only asked what it would do' {
        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly', '-DryRun') | Out-Null

        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }
}

Describe 'Restoring a snapshot returns the tree the agent was given' {
    BeforeEach {
        $script:Repo = New-ScratchRepository
        $script:Target = Join-Path $script:Repo 'src/Handler.cs'
    }

    AfterEach {
        if ($script:Repo -and (Test-Path $script:Repo)) {
            Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'discards the agent edit byte-for-byte' {
        $before = Get-FileHash -LiteralPath $script:Target -Algorithm SHA256

        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly') | Out-Null
        Set-Content -LiteralPath $script:Target -Value "AGENT FIX`n" -NoNewline
        Invoke-BaselineScript -Root $script:Repo -ScriptArguments @('-Restore') | Out-Null

        $after = Get-FileHash -LiteralPath $script:Target -Algorithm SHA256
        $after.Hash | Should -Be $before.Hash
    }

    It 'clears the state so the next candidate starts clean' {
        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly') | Out-Null
        Invoke-BaselineScript -Root $script:Repo -ScriptArguments @('-Restore') | Out-Null

        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }

    It 'leaves the tree clean enough to scope again' {
        # The panel runs five candidates in a row against one repository, so a
        # restore that leaves the tree dirty would fail every later candidate
        # on the script's own dirty-tree guard.
        Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly') | Out-Null
        Set-Content -LiteralPath $script:Target -Value "AGENT FIX`n" -NoNewline
        Invoke-BaselineScript -Root $script:Repo -ScriptArguments @('-Restore') | Out-Null

        $second = Invoke-BaselineScript -Root $script:Repo `
            -ScriptArguments @('-EditableFiles', 'src/Handler.cs', '-SnapshotOnly')

        $second | Should -Not -Match 'DIRTY WORKING DIRECTORY'
        $second | Should -Match 'Baseline scoped'
    }
}

Describe 'A bare invocation finds the scope try-fix could not pass it' {
    BeforeEach {
        $script:Repo = New-ScratchRepository
        $script:Target = Join-Path $script:Repo 'src/Handler.cs'
    }

    AfterEach {
        if ($script:Repo -and (Test-Path $script:Repo)) {
            Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue
        }
        if ($script:Scope -and (Test-Path $script:Scope)) {
            Remove-Item -LiteralPath $script:Scope -Force -ErrorAction SilentlyContinue
        }
    }

    It 'enters snapshot mode from the environment alone' {
        # This is precisely try-fix Step 2: the script with no arguments.
        $script:Scope = New-ScopeFile -Files @('src/Handler.cs')

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope }

        $output | Should -Match 'Snapshot mode'
        @((Get-Content -LiteralPath (Get-StatePath $script:Repo) -Raw | ConvertFrom-Json).RevertedFiles) |
            Should -Be @('src/Handler.cs')
    }

    It 'still reverts nothing when the scope came from the environment' {
        $script:Scope = New-ScopeFile -Files @('src/Handler.cs')
        $before = Get-FileHash -LiteralPath $script:Target -Algorithm SHA256

        Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope } | Out-Null

        (Get-FileHash -LiteralPath $script:Target -Algorithm SHA256).Hash | Should -Be $before.Hash
    }

    It 'collapses a scope that names the same file twice' {
        # The expert phase can implicate one file for two separate reasons, and
        # a duplicated entry would otherwise be reported to the agent as two
        # editable files.
        $script:Scope = New-ScopeFile -Files @('src/Handler.cs', 'src/Handler.cs')

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope }

        $output | Should -Match 'Editable scope \(1\)'
    }

    It 'does not enter snapshot mode when no scope is set' {
        # Every existing caller reaches this path. If an unset environment
        # variable could reach snapshot mode, the reviewer would silently stop
        # reverting author fixes and try-fix would grade against a tree that
        # already contains the fix it is supposed to be writing.
        $output = Invoke-BaselineScript -Root $script:Repo

        $output | Should -Not -Match 'Snapshot mode'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }
}

Describe 'An unusable scope is refused before an agent touches the tree' {
    BeforeEach {
        $script:Repo = New-ScratchRepository
    }

    AfterEach {
        if ($script:Repo -and (Test-Path $script:Repo)) {
            Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue
        }
        if ($script:Scope -and (Test-Path $script:Scope)) {
            Remove-Item -LiteralPath $script:Scope -Force -ErrorAction SilentlyContinue
        }
    }

    It 'refuses a path HEAD does not track, which could never be restored' {
        $script:Scope = New-ScopeFile -Files @('src/NeverCommitted.cs')

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope }

        $output | Should -Match 'HEAD does not track'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }

    It 'refuses -SnapshotOnly with nothing to scope' {
        $output = Invoke-BaselineScript -Root $script:Repo -ScriptArguments @('-SnapshotOnly')

        $output | Should -Match 'requires a scope'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }

    It 'refuses a scope that names no files' {
        $script:Scope = New-ScopeFile -Files @()

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope }

        $output | Should -Match 'names no files'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }

    It 'refuses a scope that is not JSON' {
        $script:Scope = New-ScopeFile -Raw 'not json{'

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $script:Scope }

        $output | Should -Match 'not valid JSON'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }

    It 'refuses a scope file that is not there' {
        $missing = Join-Path ([System.IO.Path]::GetTempPath()) ("absent-" + [Guid]::NewGuid().ToString('N') + '.json')

        $output = Invoke-BaselineScript -Root $script:Repo `
            -Environment @{ MAUI_BASELINE_SCOPE_FILE = $missing }

        $output | Should -Match 'does not exist'
        Test-Path (Get-StatePath $script:Repo) | Should -BeFalse
    }
}
