#!/usr/bin/env pwsh
#Requires -Modules Pester

# The trusted tree used to be protected by `chmod -R a-w` and nothing else.
# These tests cover the mechanism that replaced it: an attestation that is
# recomputed from the filesystem every time it is consulted, and that treats
# every possible difference as a refusal rather than a repair.

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-TrustedTreeAttestation.ps1')

    $script:ScratchRoot = Join-Path $PSScriptRoot 'trusted-tree-scratch'
    if (Test-Path -LiteralPath $script:ScratchRoot) {
        & chmod -R u+w $script:ScratchRoot 2>$null
        Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Path $script:ScratchRoot -Force | Out-Null

    $script:SourceVersion = ('a' * 40)

    function script:New-TrustedTreeFixture {
        param([string]$Name = ([guid]::NewGuid().ToString('N')))

        $root = Join-Path $script:ScratchRoot $Name
        $tree = Join-Path $root 'trusted'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        foreach ($relative in (Get-TrustedTreeKeyScriptPaths)) {
            $full = Join-Path $tree $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $full) -Force | Out-Null
            Set-Content -LiteralPath $full -Value "# $relative" -Encoding utf8NoBOM
        }
        Set-Content -LiteralPath (Join-Path $tree 'skills/replicate-issue/SKILL.md') `
            -Value 'skill' -Encoding utf8NoBOM -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Path (Join-Path $tree 'skills/replicate-issue') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $tree 'skills/replicate-issue/SKILL.md') `
            -Value 'skill' -Encoding utf8NoBOM

        $pipeline = Join-Path $root 'ci-copilot.yml'
        Set-Content -LiteralPath $pipeline -Value 'stages: []' -Encoding utf8NoBOM

        $attestationPath = Join-Path $root 'trusted-tree.json'
        $attestation = New-TrustedTreeAttestation `
            -TrustedRoot $tree `
            -SourceVersion $script:SourceVersion `
            -PipelineDefinitionPath $pipeline `
            -OutputPath $attestationPath

        return [pscustomobject]@{
            Root = $root
            Tree = $tree
            PipelinePath = $pipeline
            AttestationPath = $attestationPath
            Attestation = $attestation
        }
    }
}

AfterAll {
    if (Test-Path -LiteralPath $script:ScratchRoot) {
        & chmod -R u+w $script:ScratchRoot 2>$null
        Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Describe 'Capturing a trusted tree' {
    It 'records every file with a normalized path, mode, size and digest' {
        $fixture = script:New-TrustedTreeFixture
        $fixture.Attestation.schemaVersion | Should -Be 1
        $fixture.Attestation.sourceVersion | Should -Be $script:SourceVersion
        $fixture.Attestation.treeHash | Should -Match '^[0-9a-f]{64}$'
        $fixture.Attestation.contentHash | Should -Match '^[0-9a-f]{64}$'
        $fixture.Attestation.pipelineSha256 | Should -Match '^[0-9a-f]{64}$'
        @($fixture.Attestation.files).Count | Should -Be $fixture.Attestation.fileCount

        foreach ($entry in @($fixture.Attestation.files)) {
            # Forward slashes only, and no way to express a parent.
            $entry.path | Should -Not -Match '\\'
            $entry.path | Should -Not -Match '(^|/)\.\.?(/|$)'
            $entry.path | Should -Not -Match '^/'
            $entry.sha256 | Should -Match '^[0-9a-f]{64}$'
            [string]$entry.mode | Should -Not -BeNullOrEmpty
            $entry.size | Should -BeGreaterThan 0
        }
    }

    It 'names every key script individually' {
        $fixture = script:New-TrustedTreeFixture
        foreach ($relative in (Get-TrustedTreeKeyScriptPaths)) {
            [string]$fixture.Attestation.keyScripts.$relative | Should -Match '^[0-9a-f]{64}$'
        }
    }

    It 'refuses to attest a tree that is missing a key script' {
        $fixture = script:New-TrustedTreeFixture
        Remove-Item -LiteralPath (Join-Path $fixture.Tree 'scripts/Replicate-Issue.ps1') -Force
        { New-TrustedTreeAttestation -TrustedRoot $fixture.Tree -SourceVersion $script:SourceVersion } |
            Should -Throw '*missing a key script*'
    }

    It 'refuses to store the attestation inside the tree it attests' {
        $fixture = script:New-TrustedTreeFixture
        {
            New-TrustedTreeAttestation `
                -TrustedRoot $fixture.Tree `
                -SourceVersion $script:SourceVersion `
                -OutputPath (Join-Path $fixture.Tree 'trusted-tree.json')
        } | Should -Throw '*outside the tree it attests*'
    }

    It 'requires an immutable lowercase source commit' {
        $fixture = script:New-TrustedTreeFixture
        { New-TrustedTreeAttestation -TrustedRoot $fixture.Tree -SourceVersion 'HEAD' } |
            Should -Throw '*lowercase 40-character source commit*'
        { New-TrustedTreeAttestation -TrustedRoot $fixture.Tree -SourceVersion ('A' * 40) } |
            Should -Throw '*lowercase 40-character source commit*'
    }

    It 'reads its key-script map whichever shape it arrived in' {
        # An attestation read from disk is a PSCustomObject; one just captured
        # is an ordered dictionary. Asking a dictionary for .PSObject.Properties
        # returns Count, Keys and Values, which reads as a map with no key
        # scripts in it -- and a verification that finds no key scripts checks
        # nothing while reporting success.
        $fixture = script:New-TrustedTreeFixture
        $fromMemory = @(Get-TrustedTreeMapEntries -Map $fixture.Attestation.keyScripts)
        $fromDisk = @(Get-TrustedTreeMapEntries -Map (
                Read-TrustedTreeAttestation -Path $fixture.AttestationPath).keyScripts)

        $expected = @(Get-TrustedTreeKeyScriptPaths).Count
        $fromMemory.Count | Should -Be $expected
        $fromDisk.Count | Should -Be $expected
        @($fromMemory | ForEach-Object { $_.Name } | Sort-Object -CaseSensitive) |
            Should -Be @($fromDisk | ForEach-Object { $_.Name } | Sort-Object -CaseSensitive)
        foreach ($entry in $fromMemory) { $entry.Value | Should -Match '^[0-9a-f]{64}$' }

        # And both shapes drive a real verification.
        {
            Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree `
                -Attestation $fixture.Attestation -Context 'in memory'
        } | Should -Not -Throw
    }

    It 'refuses a key script swapped when the attestation came from memory' {
        $fixture = script:New-TrustedTreeFixture
        Set-Content -LiteralPath (Join-Path $fixture.Tree 'scripts/shared/Get-ReplicationCertification.ps1') `
            -Value 'exit 0' -Encoding utf8NoBOM
        {
            Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree `
                -Attestation $fixture.Attestation -Context 'in memory'
        } | Should -Throw '*content changed*'
    }

    It 'is deterministic across captures of identical content' {
        $first = script:New-TrustedTreeFixture
        $second = script:New-TrustedTreeFixture
        $first.Attestation.contentHash | Should -Be $second.Attestation.contentHash
        $first.Attestation.treeHash | Should -Be $second.Attestation.treeHash
    }

    It 'orders paths ordinally regardless of process culture' {
        $fixture = script:New-TrustedTreeFixture
        Set-Content -LiteralPath (Join-Path $fixture.Tree 'I-file.txt') -Value 'I' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $fixture.Tree 'i-file.txt') -Value 'i' -Encoding utf8NoBOM
        $originalCulture = [Globalization.CultureInfo]::CurrentCulture
        try {
            [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo('en-US')
            $first = Get-TrustedTreeEntries -Root $fixture.Tree
            [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo('tr-TR')
            $second = Get-TrustedTreeEntries -Root $fixture.Tree
        } finally {
            [Globalization.CultureInfo]::CurrentCulture = $originalCulture
        }

        (Get-TrustedTreeContentHash -Entries $first) |
            Should -Be (Get-TrustedTreeContentHash -Entries $second)
        @($first.path) | Should -Be @($second.path)
    }
}

Describe 'Verifying a trusted tree' {
    It 'accepts an unchanged tree' {
        $fixture = script:New-TrustedTreeFixture
        $result = Assert-TrustedTreeAttestation `
            -TrustedRoot $fixture.Tree `
            -Attestation $fixture.AttestationPath `
            -ExpectedSourceVersion $script:SourceVersion `
            -Context 'unchanged'
        $result.TreeHash | Should -Be $fixture.Attestation.treeHash
        $result.FileCount | Should -Be $fixture.Attestation.fileCount
    }

    It 'refuses a trusted file that was mutated under the same name' {
        # The attack this exists for: replace the gate, keep its name, and the
        # mode bits never change.
        $fixture = script:New-TrustedTreeFixture
        Set-Content `
            -LiteralPath (Join-Path $fixture.Tree 'scripts/shared/Validate-ReplicationCandidate.ps1') `
            -Value 'exit 0' -Encoding utf8NoBOM
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*content changed: scripts/shared/Validate-ReplicationCandidate.ps1*'
    }

    It 'refuses a trusted file that was deleted' {
        $fixture = script:New-TrustedTreeFixture
        Remove-Item -LiteralPath (Join-Path $fixture.Tree 'scripts/shared/Get-ReplicationCertification.ps1') -Force
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*deleted: scripts/shared/Get-ReplicationCertification.ps1*'
    }

    It 'refuses a file that was added to the tree' {
        $fixture = script:New-TrustedTreeFixture
        Set-Content -LiteralPath (Join-Path $fixture.Tree 'scripts/Extra.ps1') `
            -Value 'extra' -Encoding utf8NoBOM
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*added: scripts/Extra.ps1*'
    }

    It 'refuses a trusted file whose mode changed' -Skip:([System.OperatingSystem]::IsWindows()) {
        $fixture = script:New-TrustedTreeFixture
        & chmod +x (Join-Path $fixture.Tree 'scripts/Replicate-Issue.ps1')
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*mode changed: scripts/Replicate-Issue.ps1*'
    }

    It 'refuses a regular file replaced by a symlink' -Skip:([System.OperatingSystem]::IsWindows()) {
        $fixture = script:New-TrustedTreeFixture
        $target = Join-Path $fixture.Root 'impostor.ps1'
        Set-Content -LiteralPath $target -Value 'exit 0' -Encoding utf8NoBOM
        $victim = Join-Path $fixture.Tree 'scripts/shared/Assert-ReplicationTestGuard.ps1'
        Remove-Item -LiteralPath $victim -Force
        & ln -s $target $victim
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*contains a link*'
    }

    It 'refuses a directory replaced by a symlink' -Skip:([System.OperatingSystem]::IsWindows()) {
        $fixture = script:New-TrustedTreeFixture
        $decoy = Join-Path $fixture.Root 'decoy'
        New-Item -ItemType Directory -Path $decoy -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $decoy 'RunReplicationAppiumPlan.cs') -Value '//' -Encoding utf8NoBOM
        $victim = Join-Path $fixture.Tree 'scripts/templates'
        Remove-Item -LiteralPath $victim -Recurse -Force
        & ln -s $decoy $victim
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*contains a link*'
    }

    It 'refuses an attestation captured at another revision' {
        $fixture = script:New-TrustedTreeFixture
        {
            Assert-TrustedTreeAttestation `
                -TrustedRoot $fixture.Tree `
                -Attestation $fixture.AttestationPath `
                -ExpectedSourceVersion ('b' * 40)
        } | Should -Throw '*different pipeline revision*'
    }

    It 'refuses an attestation whose stored tree hash was edited' {
        $fixture = script:New-TrustedTreeFixture
        $document = Get-Content -LiteralPath $fixture.AttestationPath -Raw | ConvertFrom-Json -Depth 12
        $document.treeHash = ('f' * 64)
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.AttestationPath -Encoding utf8NoBOM
        { Assert-TrustedTreeAttestation -TrustedRoot $fixture.Tree -Attestation $fixture.AttestationPath } |
            Should -Throw '*does not match its attestation*'
    }

    It 'refuses an attestation with fields nobody expects' {
        $fixture = script:New-TrustedTreeFixture
        $document = Get-Content -LiteralPath $fixture.AttestationPath -Raw | ConvertFrom-Json -Depth 12
        $document | Add-Member -NotePropertyName 'trustMe' -NotePropertyValue $true
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.AttestationPath -Encoding utf8NoBOM
        { Read-TrustedTreeAttestation -Path $fixture.AttestationPath } |
            Should -Throw '*unexpected fields*'
    }

    It 'refuses an attestation that is simply missing' {
        { Read-TrustedTreeAttestation -Path (Join-Path $script:ScratchRoot 'absent.json') } |
            Should -Throw '*missing*'
    }
}

Describe 'Re-deriving a trusted tree on another agent' {
    It 'accepts a clean checkout with identical content' {
        $recorded = script:New-TrustedTreeFixture
        $reference = script:New-TrustedTreeFixture
        $result = Assert-TrustedTreeMatchesReference `
            -Attestation $recorded.AttestationPath `
            -ReferenceRoot $reference.Tree `
            -ExpectedSourceVersion $script:SourceVersion `
            -ReferencePipelineDefinitionPath $reference.PipelinePath
        $result.ContentHash | Should -Be $recorded.Attestation.contentHash
    }

    It 'ignores a mode difference the two agents disagree about' -Skip:([System.OperatingSystem]::IsWindows()) {
        # Cross-agent, an executable bit is umask, not tampering. Same-agent it
        # is caught by the mode-bearing hash; here it must not be.
        $recorded = script:New-TrustedTreeFixture
        $reference = script:New-TrustedTreeFixture
        & chmod 444 (Join-Path $reference.Tree 'scripts/Replicate-Issue.ps1')
        {
            Assert-TrustedTreeMatchesReference `
                -Attestation $recorded.AttestationPath `
                -ReferenceRoot $reference.Tree `
                -ExpectedSourceVersion $script:SourceVersion
        } | Should -Not -Throw
    }

    It 'refuses a run whose trusted tree differs from the pinned revision' {
        $recorded = script:New-TrustedTreeFixture
        $reference = script:New-TrustedTreeFixture
        Set-Content -LiteralPath (Join-Path $reference.Tree 'scripts/Replicate-Issue.ps1') `
            -Value '# a different commit' -Encoding utf8NoBOM
        {
            Assert-TrustedTreeMatchesReference `
                -Attestation $recorded.AttestationPath `
                -ReferenceRoot $reference.Tree `
                -ExpectedSourceVersion $script:SourceVersion
        } | Should -Throw '*does not match the pinned pipeline revision*'
    }

    It 'refuses a run whose pipeline definition differs from the pinned revision' {
        $recorded = script:New-TrustedTreeFixture
        $reference = script:New-TrustedTreeFixture
        Set-Content -LiteralPath $reference.PipelinePath -Value 'stages: [tampered]' -Encoding utf8NoBOM
        {
            Assert-TrustedTreeMatchesReference `
                -Attestation $recorded.AttestationPath `
                -ReferenceRoot $reference.Tree `
                -ExpectedSourceVersion $script:SourceVersion `
                -ReferencePipelineDefinitionPath $reference.PipelinePath
        } | Should -Throw '*pipeline definition the run used does not match*'
    }

    It 'refuses a run captured at a revision this job did not check out' {
        $recorded = script:New-TrustedTreeFixture
        $reference = script:New-TrustedTreeFixture
        {
            Assert-TrustedTreeMatchesReference `
                -Attestation $recorded.AttestationPath `
                -ReferenceRoot $reference.Tree `
                -ExpectedSourceVersion ('c' * 40)
        } | Should -Throw '*different pipeline revision*'
    }
}

Describe 'Write approvals may never name a trusted root' {
    It 'refuses the trusted root itself' {
        $fixture = script:New-TrustedTreeFixture
        { Assert-PathOutsideTrustedRoots -Path $fixture.Tree -TrustedRoots @($fixture.Tree) } |
            Should -Throw '*targets a trusted root*'
    }

    It 'refuses any file inside a trusted root' {
        $fixture = script:New-TrustedTreeFixture
        {
            Assert-PathOutsideTrustedRoots `
                -Path (Join-Path $fixture.Tree 'scripts/shared/Validate-ReplicationCandidate.ps1') `
                -TrustedRoots @($fixture.Tree)
        } | Should -Throw '*inside a trusted root*'
    }

    It 'refuses a traversal that lands back inside a trusted root' {
        $fixture = script:New-TrustedTreeFixture
        $traversal = Join-Path $fixture.Root ('other/../trusted/scripts/Replicate-Issue.ps1')
        { Assert-PathOutsideTrustedRoots -Path $traversal -TrustedRoots @($fixture.Tree) } |
            Should -Throw '*trusted root*'
    }

    It 'accepts a path outside every trusted root' {
        $fixture = script:New-TrustedTreeFixture
        {
            Assert-PathOutsideTrustedRoots `
                -Path (Join-Path $fixture.Root 'artifacts/candidate.json') `
                -TrustedRoots @($fixture.Tree)
        } | Should -Not -Throw
    }

    It 'is not fooled by a sibling directory that shares a prefix' {
        $fixture = script:New-TrustedTreeFixture
        {
            Assert-PathOutsideTrustedRoots `
                -Path ($fixture.Tree + '-copy/scripts/Replicate-Issue.ps1') `
                -TrustedRoots @($fixture.Tree)
        } | Should -Not -Throw
    }
}
