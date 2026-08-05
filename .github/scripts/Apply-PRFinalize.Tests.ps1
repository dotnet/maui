#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in apply-pr-finalize.ps1.

.EXAMPLE
    Invoke-Pester ./Apply-PRFinalize.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'apply-pr-finalize.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    # Execute the script's real module-scope config assignments (referenced by the helpers)
    # rather than duplicating them here, so the tests can't silently drift from the script.
    foreach ($variableName in @('PlatformPrefixes', 'TestingNoteMarker')) {
        $assignment = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $args[0].Left.Extent.Text -eq "`$script:$variableName"
        }, $true)

        if (-not $assignment) {
            throw "Variable '`$script:$variableName' not found"
        }

        . ([scriptblock]::Create($assignment.Extent.Text))
    }

    foreach ($functionName in @(
        'ConvertTo-AzdoSafeConsole',
        'Test-FinalizeIsNoOp',
        'Get-FinalizeRecommendation',
        'Merge-PreservedTitlePrefix',
        'Merge-PreservedBodyPreamble',
        'New-ExclusiveTempFile'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        . ([scriptblock]::Create($function.Extent.Text))
    }

    # Mirrors the real Phase 4 output shape (see Review-PR.ps1 Phase 4 prompt).
    $script:RecommendContent = @'
**Assessment:** ✏️ Recommend updating — the title still has a `[WIP]` prefix.

**Recommended title**
```text
[iOS] ButtonHandler: Preserve custom UIButton background styling
```

**Recommended description**
```text
### Issue details

Something broke.

### Description of Change

Fixed it.
```
'@
}

Describe 'Test-FinalizeIsNoOp' {
    It 'is true for the keep-as-is verdict' {
        Test-FinalizeIsNoOp -Content '✅ Current title and description accurately reflect the change — recommend keeping as-is.' |
            Should -BeTrue
    }

    It 'is true for empty content' {
        Test-FinalizeIsNoOp -Content '' | Should -BeTrue
    }

    It 'is false when an update is recommended' {
        Test-FinalizeIsNoOp -Content $script:RecommendContent | Should -BeFalse
    }
}

Describe 'Get-FinalizeRecommendation' {
    It 'extracts the title and description' {
        $result = Get-FinalizeRecommendation -Content $script:RecommendContent
        $result | Should -Not -BeNullOrEmpty
        $result.Title | Should -Be '[iOS] ButtonHandler: Preserve custom UIButton background styling'
        $result.Body | Should -Match 'Description of Change'
        $result.Body | Should -Not -Match '```'
    }

    It 'returns null for the keep-as-is verdict' {
        Get-FinalizeRecommendation -Content '✅ Current title and description accurately reflect the change — recommend keeping as-is.' |
            Should -BeNullOrEmpty
    }

    It 'returns null when the fenced blocks are missing' {
        Get-FinalizeRecommendation -Content '**Assessment:** ✏️ Recommend updating — but no blocks follow.' |
            Should -BeNullOrEmpty
    }

    It 'returns null when only a title is present' {
        $partial = "**Recommended title**`n``````text`n[iOS] Something`n``````"
        Get-FinalizeRecommendation -Content $partial | Should -BeNullOrEmpty
    }

    It 'preserves fenced code blocks nested inside the description' {
        $nested = @'
**Recommended title**
````text
[Android] Fix it
````

**Recommended description**
````text
### Change

```csharp
var x = 1;
```
````
'@
        $result = Get-FinalizeRecommendation -Content $nested
        $result | Should -Not -BeNullOrEmpty
        $result.Body | Should -Match 'var x = 1;'
    }
}

Describe 'Merge-PreservedTitlePrefix' {
    It 'preserves a triage prefix the recommendation dropped' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle '[inflight regression][iOS] Fix CarouselView2 dropping scrolls' `
            -RecommendedTitle '[iOS] CarouselView2: Stop internal recenter scrolls' |
            Should -Be '[inflight regression][iOS] CarouselView2: Stop internal recenter scrolls'
    }

    It 'preserves [WIP] — un-WIP-ing is the author''s call' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle '[WIP][Android] Fix for CurrentItem' `
            -RecommendedTitle '[Android] CarouselView: Preserve CurrentItem' |
            Should -Be '[WIP][Android] CarouselView: Preserve CurrentItem'
    }

    It 'does not duplicate a platform prefix' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle '[iOS] Old title' `
            -RecommendedTitle '[iOS] New title' |
            Should -Be '[iOS] New title'
    }

    It 'does not duplicate a tag the recommendation already kept' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle '[WIP][iOS] Old' `
            -RecommendedTitle '[WIP][iOS] New' |
            Should -Be '[WIP][iOS] New'
    }

    It 'preserves a branch tag such as [net11.0]' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle '[net11.0][iOS] Respect InputTransparent' `
            -RecommendedTitle '[iOS] UserInteraction: Respect InputTransparent' |
            Should -Be '[net11.0][iOS] UserInteraction: Respect InputTransparent'
    }

    It 'returns the recommendation unchanged when there is no prefix' {
        Merge-PreservedTitlePrefix `
            -CurrentTitle 'Fix grouped CollectionView section removal' `
            -RecommendedTitle '[iOS] CollectionView: Fix grouped section removal' |
            Should -Be '[iOS] CollectionView: Fix grouped section removal'
    }

    It 'handles an empty current title' {
        Merge-PreservedTitlePrefix -CurrentTitle '' -RecommendedTitle '[iOS] New' | Should -Be '[iOS] New'
    }
}

Describe 'Merge-PreservedBodyPreamble' {
    BeforeAll {
        $script:NoteBody = @'
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could test the resulting artifacts. Thank you!

### Issue Details
Old description.
'@
    }

    It 'preserves the required testing note the recommendation omitted' {
        $result = Merge-PreservedBodyPreamble -CurrentBody $script:NoteBody -RecommendedBody "### Issue details`n`nNew description."
        $result | Should -Match 'Are you waiting for the changes in this PR to be merged\?'
        $result | Should -Match 'New description\.'
        $result | Should -Not -Match 'Old description\.'
    }

    It 'keeps the note ahead of the new content' {
        $result = Merge-PreservedBodyPreamble -CurrentBody $script:NoteBody -RecommendedBody '### New'
        $noteIndex = $result.IndexOf('Are you waiting')
        $newIndex = $result.IndexOf('### New')
        $noteIndex | Should -BeLessThan $newIndex
    }

    It 'does not duplicate the note when the recommendation already has it' {
        $withNote = "> [!NOTE]`n> Are you waiting for the changes in this PR to be merged?`n`n### New"
        $result = Merge-PreservedBodyPreamble -CurrentBody $script:NoteBody -RecommendedBody $withNote
        ([regex]::Matches($result, 'Are you waiting')).Count | Should -Be 1
    }

    It 'returns the recommendation unchanged when the current body has no note' {
        Merge-PreservedBodyPreamble -CurrentBody '### Just a description' -RecommendedBody '### New' |
            Should -Be '### New'
    }

    It 'handles an empty current body' {
        Merge-PreservedBodyPreamble -CurrentBody '' -RecommendedBody '### New' | Should -Be '### New'
    }
}

Describe 'ConvertTo-AzdoSafeConsole' {
    # Behaviour is pinned to the canonical implementation in Review-PR.ps1; these mirror the
    # assertions in Review-PR.Tests.ps1 so the duplicated copy can't silently drift.
    It 'defangs the task.setvariable logging command' {
        ConvertTo-AzdoSafeConsole '##vso[task.setvariable variable=x]y' |
            Should -Be '## vso[task.setvariable variable=x]y'
    }

    It 'defangs the bare ## command prefix' {
        ConvertTo-AzdoSafeConsole '##[command]z' | Should -Be '## [command]z'
    }

    It 'collapses a lone CR so it cannot open a column-0 line' {
        ConvertTo-AzdoSafeConsole "safe`r##vso[task.complete]" | Should -Be 'safe ## vso[task.complete]'
    }

    It 'collapses LF' {
        ConvertTo-AzdoSafeConsole "Reviewing`n##vso[task.complete result=Succeeded;]done" |
            Should -Be 'Reviewing ## vso[task.complete result=Succeeded;]done'
    }

    It 'leaves an innocent ## alone' {
        ConvertTo-AzdoSafeConsole 'Reading file src/Foo.cs (## of total)' |
            Should -Be 'Reading file src/Foo.cs (## of total)'
    }
}

Describe 'Security regressions — AzDO logging-command injection' {
    # Regression coverage for the review finding: the Post phase runs with GH_COMMENT_TOKEN
    # and sets GateFailed/CopilotFailed, so PR-controlled text reaching stdout unsanitized
    # could set those variables and mask a failing gate.

    It 'does not carry an "##vso[" payload out of an author-controlled title tag' {
        $evil = '[##vso[task.setvariable variable=GateFailed] x] fix'
        $result = Merge-PreservedTitlePrefix -CurrentTitle $evil -RecommendedTitle '[iOS] Clean title'
        $result | Should -Not -Match '##'
        $result | Should -Be '[iOS] Clean title'
    }

    It 'rejects a recommended title containing a lone CR' {
        $CR = [char]13
        $fence = '```'
        $content = @(
            '**Recommended title**'
            "${fence}text"
            "harmless${CR}##vso[task.setvariable variable=GateFailed]x"
            $fence
            ''
            '**Recommended description**'
            "${fence}text"
            'body'
            $fence
        ) -join "`n"

        Get-FinalizeRecommendation -Content $content | Should -BeNullOrEmpty
    }

    It 'rejects a recommended title containing an LF' {
        $fence = '```'
        $content = @(
            '**Recommended title**'
            "${fence}text"
            'line one'
            'line two'
            $fence
            ''
            '**Recommended description**'
            "${fence}text"
            'body'
            $fence
        ) -join "`n"

        Get-FinalizeRecommendation -Content $content | Should -BeNullOrEmpty
    }

    It 'still preserves legitimate triage tags after the tag hardening' {
        Merge-PreservedTitlePrefix -CurrentTitle '[WIP][iOS] old' -RecommendedTitle '[iOS] new' |
            Should -Be '[WIP][iOS] new'
        Merge-PreservedTitlePrefix -CurrentTitle '[inflight regression][iOS] old' -RecommendedTitle '[iOS] new' |
            Should -Be '[inflight regression][iOS] new'
        Merge-PreservedTitlePrefix -CurrentTitle '[net11.0][iOS] old' -RecommendedTitle '[iOS] new' |
            Should -Be '[net11.0][iOS] new'
        Merge-PreservedTitlePrefix -CurrentTitle '[release/10.0.1xx][iOS] old' -RecommendedTitle '[iOS] new' |
            Should -Be '[release/10.0.1xx][iOS] new'
    }

    It 'drops a tag carrying a control character' {
        $tag = "[wi{0}p][iOS] old" -f [char]13
        Merge-PreservedTitlePrefix -CurrentTitle $tag -RecommendedTitle '[iOS] new' |
            Should -Be '[iOS] new'
    }
}

Describe 'New-ExclusiveTempFile' {
    BeforeAll {
        $script:SandboxDir = Join-Path ([System.IO.Path]::GetTempPath()) "apply-prfinalize-tests-$([System.IO.Path]::GetRandomFileName())"
        New-Item -ItemType Directory -Path $script:SandboxDir -Force | Out-Null
        $script:RealNewItem = Get-Command New-Item -CommandType Cmdlet
        $script:OriginalAgentTemp = $env:AGENT_TEMPDIRECTORY
        $env:AGENT_TEMPDIRECTORY = $script:SandboxDir
    }

    AfterAll {
        $env:AGENT_TEMPDIRECTORY = $script:OriginalAgentTemp
        Remove-Item -LiteralPath $script:SandboxDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'creates the file inside AGENT_TEMPDIRECTORY when it is set' {
        $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
        try {
            Test-Path -LiteralPath $path | Should -BeTrue
            (Split-Path -Parent $path) | Should -Be $script:SandboxDir
        } finally {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
        }
    }

    It 'produces a unique path on each call, so the name is not predictable' {
        $a = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
        $b = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
        try {
            $a | Should -Not -Be $b
        } finally {
            Remove-Item -LiteralPath $a -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath $b -Force -ErrorAction SilentlyContinue
        }
    }

    # These pin the actual finding-#2 behaviour. The earlier version of this test was
    # vacuous: it never planted a symlink at a path the helper would try, so a
    # predictable-name Set-Content write-through implementation still passed it. The
    # -NameGenerator seam forces known candidates so a symlink can be planted precisely.
    It 'refuses a pre-planted symlink and leaves its target untouched' {
        $secret = Join-Path $script:SandboxDir 'secret-existing.txt'
        Set-Content -LiteralPath $secret -Value 'ORIGINAL' -Encoding UTF8

        $planted = Join-Path $script:SandboxDir 'pr-finalize-body-123-forced0.md'
        New-Item -ItemType SymbolicLink -Path $planted -Target $secret | Out-Null

        # $script: scope is required — a plain $i++ inside the scriptblock would mutate a
        # local copy, so every attempt would re-request the planted name.
        $script:ForcedIndex = 0
        $script:AttemptedPaths = @()
        Mock New-Item {
            $script:AttemptedPaths += $Path
            & $script:RealNewItem -ItemType $ItemType -Path $Path -ErrorAction Stop
        } -ParameterFilter { $ItemType -eq 'File' }

        $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123' -NameGenerator {
            $n = "forced$($script:ForcedIndex)"; $script:ForcedIndex++; $n
        }
        try {
            # The spy proves New-Item actually attempted the planted path before moving on.
            # Generator consumption alone is insufficient: an implementation could generate
            # forced0, skip it without calling New-Item, then successfully create forced1.
            $script:AttemptedPaths[0] | Should -Be $planted
            $script:AttemptedPaths[1] | Should -Be (Join-Path $script:SandboxDir 'pr-finalize-body-123-forced1.md')
            $script:ForcedIndex | Should -BeGreaterThan 1
            $path | Should -Be (Join-Path $script:SandboxDir 'pr-finalize-body-123-forced1.md')

            $path | Should -Not -Be $planted
            'REPLACEMENT BODY' | Set-Content -LiteralPath $path -Encoding UTF8
            # ...so the symlink target is untouched, and the link is still a link.
            (Get-Content -Raw -LiteralPath $secret).Trim() | Should -Be 'ORIGINAL'
            (Get-Item -LiteralPath $planted).LinkType | Should -Be 'SymbolicLink'
            (Get-Content -Raw -LiteralPath $path).Trim() | Should -Be 'REPLACEMENT BODY'
        } finally {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath $planted -Force -ErrorAction SilentlyContinue
        }
    }

    It 'refuses a dangling pre-planted symlink rather than creating its target' {
        $missingTarget = Join-Path $script:SandboxDir 'never-created.txt'
        $planted = Join-Path $script:SandboxDir 'pr-finalize-body-123-dangle0.md'
        New-Item -ItemType SymbolicLink -Path $planted -Target $missingTarget | Out-Null

        $script:DangleIndex = 0
        $script:AttemptedPaths = @()
        Mock New-Item {
            $script:AttemptedPaths += $Path
            & $script:RealNewItem -ItemType $ItemType -Path $Path -ErrorAction Stop
        } -ParameterFilter { $ItemType -eq 'File' }

        $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123' -NameGenerator {
            $n = "dangle$($script:DangleIndex)"; $script:DangleIndex++; $n
        }
        try {
            # As above: pins that the planted path was attempted and skipped, not bypassed.
            $script:AttemptedPaths[0] | Should -Be $planted
            $script:AttemptedPaths[1] | Should -Be (Join-Path $script:SandboxDir 'pr-finalize-body-123-dangle1.md')
            $script:DangleIndex | Should -BeGreaterThan 1
            $path | Should -Be (Join-Path $script:SandboxDir 'pr-finalize-body-123-dangle1.md')

            $path | Should -Not -Be $planted
            'REPLACEMENT BODY' | Set-Content -LiteralPath $path -Encoding UTF8
            # Writing through a dangling link would have created the target.
            Test-Path -LiteralPath $missingTarget | Should -BeFalse
        } finally {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath $planted -Force -ErrorAction SilentlyContinue
        }
    }

    It 'throws instead of falling back to a predictable path when every candidate is occupied' {
        $secret = Join-Path $script:SandboxDir 'secret-exhaust.txt'
        Set-Content -LiteralPath $secret -Value 'ORIGINAL' -Encoding UTF8

        $planted = 0..4 | ForEach-Object {
            $link = Join-Path $script:SandboxDir "pr-finalize-body-123-exhaust$_.md"
            New-Item -ItemType SymbolicLink -Path $link -Target $secret | Out-Null
            $link
        }

        try {
            $script:ExhaustIndex = 0
            { New-ExclusiveTempFile -Prefix 'pr-finalize-body-123' -NameGenerator {
                $n = "exhaust$($script:ExhaustIndex)"; $script:ExhaustIndex++; $n
            } } | Should -Throw -ExpectedMessage '*after 5 attempts*'
            # No fallback path was written, so the symlink target is still intact.
            (Get-Content -Raw -LiteralPath $secret).Trim() | Should -Be 'ORIGINAL'
        } finally {
            $planted | ForEach-Object { Remove-Item -LiteralPath $_ -Force -ErrorAction SilentlyContinue }
        }
    }

    It 'tries exactly MaxAttempts candidates before giving up' {
        $secret = Join-Path $script:SandboxDir 'secret-count.txt'
        Set-Content -LiteralPath $secret -Value 'ORIGINAL' -Encoding UTF8
        $planted = 0..4 | ForEach-Object {
            $link = Join-Path $script:SandboxDir "pr-finalize-body-123-count$_.md"
            New-Item -ItemType SymbolicLink -Path $link -Target $secret | Out-Null
            $link
        }

        try {
            $script:Calls = 0
            { New-ExclusiveTempFile -Prefix 'pr-finalize-body-123' -NameGenerator { $n = "count$($script:Calls)"; $script:Calls++; $n } } |
                Should -Throw
            $script:Calls | Should -Be 5
        } finally {
            $planted | ForEach-Object { Remove-Item -LiteralPath $_ -Force -ErrorAction SilentlyContinue }
        }
    }

    It 'surfaces a non-collision failure immediately instead of retrying it away' {
        # A missing base directory can never be resolved by picking another name, so it must
        # propagate rather than be masked by the generic "after N attempts" message.
        $saved = $env:AGENT_TEMPDIRECTORY
        $env:AGENT_TEMPDIRECTORY = $script:SandboxDir
        try {
            $script:Calls = 0
            # Assert on the exception *type*, not the message: .NET message strings are
            # localized, so matching "Could not find a part of the path" would fail on a
            # non-en-US agent. The type is culture-invariant.
            $thrown = $null
            try {
                New-ExclusiveTempFile -Prefix 'missing-dir/nope/body' -NameGenerator { $script:Calls++; 'x' }
            } catch {
                $thrown = $_.Exception
            }

            $thrown | Should -Not -BeNullOrEmpty
            $thrown | Should -BeOfType ([System.IO.DirectoryNotFoundException])
            $script:Calls | Should -Be 1
        } finally {
            $env:AGENT_TEMPDIRECTORY = $saved
        }
    }

    It 'ignores AGENT_TEMPDIRECTORY when it points at a file rather than a directory' {
        $saved = $env:AGENT_TEMPDIRECTORY
        $asFile = Join-Path $script:SandboxDir 'not-a-directory.txt'
        Set-Content -LiteralPath $asFile -Value 'x' -Encoding UTF8
        $env:AGENT_TEMPDIRECTORY = $asFile
        try {
            $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
            try {
                Test-Path -LiteralPath $path | Should -BeTrue
                (Split-Path -Parent $path) | Should -Not -Be $asFile
            } finally {
                Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            }
        } finally {
            $env:AGENT_TEMPDIRECTORY = $saved
        }
    }

    It 'falls back to the system temp directory when AGENT_TEMPDIRECTORY is unset' {
        $saved = $env:AGENT_TEMPDIRECTORY
        $env:AGENT_TEMPDIRECTORY = $null
        try {
            $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
            try {
                Test-Path -LiteralPath $path | Should -BeTrue
            } finally {
                Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            }
        } finally {
            $env:AGENT_TEMPDIRECTORY = $saved
        }
    }

    It 'ignores AGENT_TEMPDIRECTORY when it points at a missing directory' {
        $saved = $env:AGENT_TEMPDIRECTORY
        $env:AGENT_TEMPDIRECTORY = Join-Path $script:SandboxDir 'does-not-exist'
        try {
            $path = New-ExclusiveTempFile -Prefix 'pr-finalize-body-123'
            try {
                Test-Path -LiteralPath $path | Should -BeTrue
            } finally {
                Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            }
        } finally {
            $env:AGENT_TEMPDIRECTORY = $saved
        }
    }
}
