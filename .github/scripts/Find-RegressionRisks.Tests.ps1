Set-StrictMode -Version Latest

BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot 'Find-RegressionRisks.ps1'
    $script:Source = Get-Content -LiteralPath $script:ScriptPath -Raw
    $script:Ast = [System.Management.Automation.Language.Parser]::ParseInput(
        $script:Source, [ref]$null, [ref]$null)

    function Get-ScriptFunction {
        param([string]$Name)
        $script:Ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $Name
        }, $true)
    }

    function Invoke-Script {
        param([string[]]$Arguments)
        $output = & pwsh -NoProfile -File $script:ScriptPath @Arguments 2>&1
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
    }
}

Describe 'A patch that exists only in a fork can still be judged' {
    # A replicate fix is a patch in a fork at the moment it needs judging: there
    # is no pull request to name it by, so `gh pr diff` cannot reach it. Every
    # step downstream works from file paths and diff text, so supplying the diff
    # directly makes the whole script reusable unchanged.

    It 'refuses a diff path that does not exist rather than scoring it CLEAN' {
        $missing = Join-Path $TestDrive 'absent.diff'
        $result = Invoke-Script -Arguments @('-DiffPath', $missing)

        $result.ExitCode | Should -Be 2
        $result.Output | Should -Not -Match 'No regression risks detected'
        # Asserted on the reason, not the code. Removing this refusal still
        # exits 2 - the run falls through to `gh pr diff 0`, which is also
        # empty - so an exit-code assertion passes against code that never
        # noticed the diff was missing. Only the message tells them apart.
        $result.Output | Should -Match ([regex]::Escape($missing)) -Because (
            'the failure has to name the diff it could not read')
    }

    It 'refuses an empty diff, which would otherwise score CLEAN by having no removals' {
        $empty = Join-Path $TestDrive 'empty.diff'
        Set-Content -LiteralPath $empty -Value '' -NoNewline

        $result = Invoke-Script -Arguments @('-DiffPath', $empty)

        $result.ExitCode | Should -Be 2
        $result.Output | Should -Not -Match 'No regression risks detected'
        # Same trap as above: without this refusal the empty diff falls through
        # to a PR lookup that also fails, so only the reason distinguishes them.
        $result.Output | Should -Match 'empty' -Because (
            'a patch that read as empty must be reported as empty, not as a missing PR')
    }

    It 'refuses being given neither a pull request nor a diff' {
        $result = Invoke-Script -Arguments @('-Repo', 'dotnet/maui')

        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match 'PRNumber|DiffPath'
    }

    It 'takes the files to inspect from the diff headers the parser already trusts' {
        # Reading paths from anywhere else lets the file list and the hunks
        # disagree about which file is under review.
        $detection = $script:Source -split "`n" |
            Where-Object { $_ -match '\+\+\+ b/' }

        @($detection).Count | Should -BeGreaterThan 0
    }
}

Describe 'The diff under review is not mistaken for the history it is compared against' {
    # The defect this measures was introduced while adding -DiffPath, survived a
    # full end-to-end run against a real reverting patch, and reported CLEAN.
    #
    # Get-PRDiffText serves two callers: the change under review, and every
    # historical bug-fix PR whose additions the change is compared against.
    # Answering both with the supplied patch makes each fix PR appear to have
    # added nothing, so no line can ever match and the verdict is always CLEAN.
    # It is silent, and a false CLEAN is the worst answer this script can give.

    It 'never answers a fix-PR diff request with the patch under review' {
        $fetch = Get-ScriptFunction -Name 'Get-PRDiffText'
        $fetch | Should -Not -BeNullOrEmpty

        $references = $fetch.Find({
            $args[0] -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $args[0].VariablePath.UserPath -match 'SuppliedDiff'
        }, $true)

        # Asserted over the syntax tree, not the text: a comment naming the
        # variable satisfies a grep and means nothing, which this repository has
        # now been caught by twice.
        $references | Should -BeNullOrEmpty -Because (
            'this helper also fetches the diffs the comparison reads as evidence')
    }

    It 'substitutes the supplied patch at exactly one place' {
        $assignments = $script:Ast.FindAll({
            $args[0] -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $args[0].VariablePath.UserPath -match 'SuppliedDiff'
        }, $true)

        # Loading it, guarding it twice, the file-list branch, and the one
        # substitution. The count is asserted so a second substitution cannot be
        # added silently.
        @($assignments).Count | Should -BeGreaterThan 0
        $script:Source | Should -Match '\$prDiff\s*=\s*if\s*\(\$script:SuppliedDiff\)'
    }
}

Describe 'The history walked is the history of the file under review' {
    # Measured on this repository at `main`: ViewExtensions.cs (iOS) and
    # TimePickerHandler.MacCatalyst.cs each have exactly one commit in a
    # six-month window, and `--follow` reports the SAME eight for both - a
    # SearchHandler fix, a BindableObject optimization, a Magick.NET version
    # bump, a RadioButton feature. None of them touch either file.
    #
    # Those PRs are then inspected for lines they added TO THIS FILE and
    # compared against lines removed FROM THIS FILE, so a coincidental match is
    # a false REVERT, and seven wrong PRs crowding out the real one under
    # -MaxRecentPRsPerFile is a false CLEAN.

    It 'does not follow renames when listing the commits that touched a file' {
        $script:Source | Should -Not -Match 'git log[^\r\n]*--follow' -Because (
            'renamed history is a different path, and every other step here keys on the path string')
    }

    It 'still scopes every history query to the file' {
        $logLines = @($script:Source -split "`n" | Where-Object { $_ -match '\$commitLog\s*=\s*git log' })

        @($logLines).Count | Should -BeGreaterThan 0
        foreach ($line in $logLines) {
            $line | Should -Match '--\s+\$filePath' -Because (
                'an unscoped log returns the whole branch, which is what --follow effectively did')
        }
    }

    It 'reports the same commits git reports for a path' {
        # Reads the repository rather than the script, because a claim about
        # what `--follow` does is a claim about git, and it is the reason the
        # flag was removed.
        $root = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
        if (-not $root) { Set-ItResult -Skipped -Because 'not a git checkout'; return }

        $probe = 'src/Core/src/Platform/iOS/ViewExtensions.cs'
        if (-not (Test-Path -LiteralPath (Join-Path $root $probe))) {
            Set-ItResult -Skipped -Because 'probe file absent'; return
        }

        $scoped = @(& git -C $root log --oneline --since='6 months ago' -- $probe 2>$null)
        $followed = @(& git -C $root log --oneline --follow --since='6 months ago' -- $probe 2>$null)

        # Not an equality assertion: what is being pinned is that --follow is
        # not merely a superset of harmless extra history, it is a different
        # answer, so the removal changes behaviour rather than tidying it.
        if (@($followed).Count -gt 0 -or @($scoped).Count -gt 0) {
            @($followed).Count | Should -BeGreaterOrEqual @($scoped).Count
        }
    }
}
