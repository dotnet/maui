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
        'Merge-PreservedBodyPreamble'
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
