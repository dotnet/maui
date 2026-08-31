#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Captures and verifies a deterministic attestation of the trusted script tree.

.DESCRIPTION
    The replication pipeline stages `.github/scripts`, `.github/skills`, and
    `eng/scripts` from the immutable pipeline revision into a trusted directory,
    then runs models and generated code on the same agent for hours. Until now
    the only thing standing between that generated code and the gates that judge
    it was `chmod -R a-w`: a mode bit the same user can take off again, on a
    tree whose contents nothing ever re-read.

    So the tree is attested instead. Every file is recorded by normalized
    relative path, mode, size, and SHA-256, and the whole set is reduced to one
    deterministic tree hash bound to the pipeline revision it was captured at.
    Verification recomputes the whole thing and fails closed on any difference:
    a mutated file that kept its name, an added file, a deleted file, a symlink
    that replaced a regular file, or a mode that changed.

    Nothing here trusts a stored number. The attestation document is data; the
    filesystem is the authority; disagreement is an error rather than a repair.
#>

Set-StrictMode -Version Latest

# Written by the trusted staging step and read by everything downstream. It
# lives outside the tree it describes, because a document stored inside its own
# subject changes the hash it claims.
$script:TrustedTreeAttestationSchemaVersion = 1

# A staged trusted tree is a few thousand small text files. These bounds are far
# above that and exist so a tampered or substituted directory cannot turn
# verification into an unbounded read.
$script:TrustedTreeMaxFileCount = 20000
$script:TrustedTreeMaxFileBytes = 8MB
$script:TrustedTreeMaxTotalBytes = 256MB

# The scripts whose identity the certification binding names individually. A
# per-file hash is already in the tree, but a consumer that wants to prove "the
# verifier that produced this evidence is the verifier I am holding" should not
# have to know the tree's internal layout to do it.
$script:TrustedTreeKeyScriptPaths = @(
    'scripts/Replicate-Issue.ps1',
    'scripts/shared/Assert-ReplicationTestGuard.ps1',
    'scripts/shared/Assert-TrustedTreeAttestation.ps1',
    'scripts/shared/Assert-ReplicationCertificationBinding.ps1',
    'scripts/shared/Assert-ReplicationExecutionEnvironment.ps1',
    'scripts/shared/Assert-ReplicationWindowsAppContainer.ps1',
    'scripts/shared/ReplicationWindowsAppContainerManifest.targets',
    'scripts/shared/Invoke-ReplicationWindowsAppx.ps1',
    'scripts/shared/Assert-ReplicationAppleAppSandbox.ps1',
    'scripts/shared/Invoke-ReplicationNetworkIsolatedProcess.ps1',
    'scripts/shared/Get-ReplicationCertification.ps1',
    'scripts/shared/Invoke-ReplicationTestVerification.ps1',
    'scripts/shared/Record-Reproduction.ps1',
    'scripts/shared/Validate-ReplicationCandidate.ps1',
    'scripts/templates/RunReplicationAppiumPlan.cs'
)

function Get-TrustedTreeKeyScriptPaths {
    <#
        .SYNOPSIS
        Returns the trusted-tree relative paths named individually in a binding.
    #>
    return @($script:TrustedTreeKeyScriptPaths)
}

function Get-TrustedTreePathComparison {
    if ([System.OperatingSystem]::IsWindows()) {
        return [System.StringComparison]::OrdinalIgnoreCase
    }

    return [System.StringComparison]::Ordinal
}

function ConvertTo-TrustedTreeRelativePath {
    <#
        .SYNOPSIS
        Normalizes one path under a root into a canonical relative path.

        .DESCRIPTION
        Canonical means forward slashes, no leading separator, no drive, no `.`
        or `..` segment, and no empty segment. Two agents on two operating
        systems must produce byte-identical text for the same file or the tree
        hash is not deterministic, and a path that can express traversal is a
        path that can name a file outside the tree it claims to describe.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $comparison = Get-TrustedTreePathComparison
    $prefix = $fullRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($prefix, $comparison)) {
        throw "Trusted tree path is outside its root: $Path"
    }

    $relative = $fullPath.Substring($prefix.Length).Replace('\', '/')
    if ([string]::IsNullOrWhiteSpace($relative)) {
        throw "Trusted tree path resolved to its own root: $Path"
    }
    foreach ($segment in ($relative -split '/')) {
        if ([string]::IsNullOrEmpty($segment) -or $segment -eq '.' -or $segment -eq '..') {
            throw "Trusted tree path is not canonical: $relative"
        }
    }

    return $relative
}

function Get-TrustedTreeFileMode {
    <#
        .SYNOPSIS
        Returns the comparable mode of one trusted file.

        .DESCRIPTION
        On Unix this is the octal permission triplet, because the executable bit
        is the difference between a data file and something the agent can run.
        Windows has no such bit, so it reports the read-only attribute instead
        and a run captured on one platform is only ever compared with itself.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][System.IO.FileInfo]$File
    )

    if ([System.OperatingSystem]::IsWindows()) {
        return $(if ($File.IsReadOnly) { 'ro' } else { 'rw' })
    }

    $mode = [string]$File.UnixMode
    if ([string]::IsNullOrWhiteSpace($mode)) {
        # UnixMode is populated by the provider and has been empty on
        # non-filesystem providers. Refusing is correct: an unreadable mode is
        # not the same as an unchanged one.
        throw "Unable to read the file mode of $($File.FullName)."
    }

    return $mode
}

function Get-TrustedTreeMapEntries {
    <#
        .SYNOPSIS
        Enumerates a name/hash map whichever shape it arrived in.

        .DESCRIPTION
        An attestation read from disk is a PSCustomObject; one just captured in
        memory is an ordered dictionary. Asking a dictionary for
        `.PSObject.Properties` returns Count, Keys and Values rather than the
        entries, which reads as a map with no key scripts in it. One accessor,
        so a caller cannot get that wrong.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][object]$Map
    )

    if ($null -eq $Map) { return @() }
    if ($Map -is [System.Collections.IDictionary]) {
        return @($Map.Keys | ForEach-Object {
            [pscustomobject]@{ Name = [string]$_; Value = [string]$Map[$_] }
        })
    }

    return @($Map.PSObject.Properties | ForEach-Object {
        [pscustomobject]@{ Name = [string]$_.Name; Value = [string]$_.Value }
    })
}

function Get-TrustedTreeFileHash {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($Path)
        try {
            return [System.BitConverter]::ToString($sha256.ComputeHash($stream)).Replace('-', '').ToLowerInvariant()
        } finally {
            $stream.Dispose()
        }
    } finally {
        $sha256.Dispose()
    }
}

function Get-TrustedTreeTextHash {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return [System.BitConverter]::ToString($sha256.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    } finally {
        $sha256.Dispose()
    }
}

function Get-TrustedTreeEntries {
    <#
        .SYNOPSIS
        Enumerates the trusted tree as canonical, ordered file entries.

        .DESCRIPTION
        Directories are walked explicitly rather than through a recursive
        wildcard so a reparse point can be rejected on the way down instead of
        being followed out of the tree first. Every leaf must be a regular file:
        a symlink, a device node, or a junction is a way to make the same
        relative path mean different content later.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Root
    )

    $fullRoot = [System.IO.Path]::GetFullPath($Root)
    if (-not (Test-Path -LiteralPath $fullRoot -PathType Container)) {
        throw "Trusted tree root does not exist as a directory: $Root"
    }
    $rootItem = Get-Item -LiteralPath $fullRoot -Force
    if ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Trusted tree root must be a regular directory: $Root"
    }

    $entries = [System.Collections.Generic.List[object]]::new()
    $totalBytes = 0L
    $pending = [System.Collections.Generic.Queue[string]]::new()
    $pending.Enqueue($fullRoot)

    while ($pending.Count -gt 0) {
        $directory = $pending.Dequeue()
        foreach ($child in @(Get-ChildItem -LiteralPath $directory -Force -ErrorAction Stop)) {
            if ($child.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                $relative = ConvertTo-TrustedTreeRelativePath -Root $fullRoot -Path $child.FullName
                throw "Trusted tree contains a link: $relative"
            }
            if ($child.PSIsContainer) {
                $pending.Enqueue($child.FullName)
                continue
            }
            if ($child -isnot [System.IO.FileInfo]) {
                throw "Trusted tree contains a non-regular entry: $($child.FullName)"
            }
            if ($entries.Count -ge $script:TrustedTreeMaxFileCount) {
                throw "Trusted tree exceeds $($script:TrustedTreeMaxFileCount) files."
            }
            if ($child.Length -gt $script:TrustedTreeMaxFileBytes) {
                $relative = ConvertTo-TrustedTreeRelativePath -Root $fullRoot -Path $child.FullName
                throw "Trusted tree file exceeds the size bound: $relative"
            }
            $totalBytes += [long]$child.Length
            if ($totalBytes -gt $script:TrustedTreeMaxTotalBytes) {
                throw 'Trusted tree exceeds the total size bound.'
            }

            $entries.Add([ordered]@{
                path = ConvertTo-TrustedTreeRelativePath -Root $fullRoot -Path $child.FullName
                mode = Get-TrustedTreeFileMode -File $child
                size = [long]$child.Length
                sha256 = Get-TrustedTreeFileHash -Path $child.FullName
            })
        }
    }

    if ($entries.Count -eq 0) {
        throw "Trusted tree root contains no files: $Root"
    }

    # Ordinal sort, not culture-aware: a tree hash that depends on the agent's
    # locale is not a tree hash.
    $ordered = [System.Collections.Generic.List[object]]::new()
    foreach ($entry in $entries) {
        $ordered.Add($entry)
    }
    $ordered.Sort([System.Comparison[object]]{
        param($left, $right)
        return [System.StringComparer]::Ordinal.Compare(
            [string]$left.path,
            [string]$right.path)
    })
    return $ordered.ToArray()
}

function Get-TrustedTreeHash {
    <#
        .SYNOPSIS
        Reduces ordered trusted-tree entries to one deterministic hash.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Entries
    )

    $builder = [System.Text.StringBuilder]::new()
    foreach ($entry in $Entries) {
        [void]$builder.Append([string]$entry.path)
        [void]$builder.Append("`0")
        [void]$builder.Append([string]$entry.mode)
        [void]$builder.Append("`0")
        [void]$builder.Append([string][long]$entry.size)
        [void]$builder.Append("`0")
        [void]$builder.Append([string]$entry.sha256)
        [void]$builder.Append("`n")
    }

    return Get-TrustedTreeTextHash -Text $builder.ToString()
}

function Get-TrustedTreeContentHash {
    <#
        .SYNOPSIS
        Reduces the same entries to a hash of path and content only.

        .DESCRIPTION
        The mode-bearing tree hash is the strong one and is the only thing the
        same-agent before/after checks use: a trusted file that became
        executable between two model invocations is exactly what they exist to
        catch.

        It cannot be compared across agents, though. A macOS device pool and an
        Ubuntu validation pool disagree about the mode of an identical file for
        reasons that have nothing to do with tampering, and a cross-agent check
        that fails for umask would be switched off within a week. So the
        cross-agent comparison uses this: same paths, same bytes, computed
        independently from the same pinned commit.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Entries
    )

    $builder = [System.Text.StringBuilder]::new()
    foreach ($entry in $Entries) {
        [void]$builder.Append([string]$entry.path)
        [void]$builder.Append("`0")
        [void]$builder.Append([string][long]$entry.size)
        [void]$builder.Append("`0")
        [void]$builder.Append([string]$entry.sha256)
        [void]$builder.Append("`n")
    }

    return Get-TrustedTreeTextHash -Text $builder.ToString()
}

function New-TrustedTreeAttestation {
    <#
        .SYNOPSIS
        Captures the trusted tree at the immutable pipeline revision.

        .DESCRIPTION
        `SourceVersion` is the commit the trusted tree was copied from, and
        `PipelineDefinitionPath` is the pipeline file itself, hashed separately
        so a later consumer can prove the definition that scheduled the run is
        the definition it is reading. Both are recorded rather than derived,
        because the job that verifies the attestation may not have a git
        worktree at all.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$SourceVersion,
        [string]$PipelineDefinitionPath = '',
        [string]$OutputPath = ''
    )

    if ($SourceVersion -cnotmatch '^[0-9a-f]{40}$') {
        throw 'Trusted tree attestation requires a lowercase 40-character source commit.'
    }

    $fullRoot = [System.IO.Path]::GetFullPath($TrustedRoot)
    $entries = @(Get-TrustedTreeEntries -Root $fullRoot)
    $treeHash = Get-TrustedTreeHash -Entries $entries
    $contentHash = Get-TrustedTreeContentHash -Entries $entries

    $pipelineSha256 = ''
    if (-not [string]::IsNullOrWhiteSpace($PipelineDefinitionPath)) {
        if (-not (Test-Path -LiteralPath $PipelineDefinitionPath -PathType Leaf)) {
            throw "Pipeline definition is missing: $PipelineDefinitionPath"
        }
        $pipelineSha256 = Get-TrustedTreeFileHash -Path $PipelineDefinitionPath
    }

    $comparison = Get-TrustedTreePathComparison
    $keyScripts = [ordered]@{}
    foreach ($relative in (Get-TrustedTreeKeyScriptPaths)) {
        $match = @($entries | Where-Object { ([string]$_.path).Equals($relative, $comparison) })
        if ($match.Count -ne 1) {
            throw "Trusted tree is missing a key script: $relative"
        }
        $keyScripts[$relative] = [string]$match[0].sha256
    }

    $attestation = [ordered]@{
        schemaVersion = $script:TrustedTreeAttestationSchemaVersion
        sourceVersion = $SourceVersion
        pipelineSha256 = $pipelineSha256
        fileCount = $entries.Count
        treeHash = $treeHash
        contentHash = $contentHash
        keyScripts = $keyScripts
        files = @($entries)
    }

    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
        $fullOutput = [System.IO.Path]::GetFullPath($OutputPath)
        $parent = [System.IO.Path]::GetDirectoryName($fullOutput)
        if ([string]::IsNullOrEmpty($parent) -or -not (Test-Path -LiteralPath $parent -PathType Container)) {
            throw 'Trusted tree attestation output parent directory must already exist.'
        }
        # The document may not live inside the tree it describes: writing it
        # there would change the very hash it records.
        $prefix = $fullRoot.TrimEnd(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
        if ($fullOutput.StartsWith($prefix, $comparison)) {
            throw 'Trusted tree attestation must be written outside the tree it attests.'
        }
        $attestation | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $fullOutput -Encoding utf8NoBOM
    }

    return $attestation
}

function Read-TrustedTreeAttestation {
    <#
        .SYNOPSIS
        Reads an attestation document and rejects anything malformed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Trusted tree attestation is missing: $Path"
    }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Trusted tree attestation must be a regular file: $Path"
    }
    if ($item.Length -gt 16MB) {
        throw 'Trusted tree attestation exceeds its size bound.'
    }

    $document = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -Depth 12
    if ($null -eq $document) {
        throw 'Trusted tree attestation is empty.'
    }

    $expected = @('schemaVersion', 'sourceVersion', 'pipelineSha256', 'fileCount', 'treeHash', 'contentHash', 'keyScripts', 'files')
    $actual = @($document.PSObject.Properties.Name | Sort-Object -CaseSensitive)
    if (($actual -join ',') -cne (($expected | Sort-Object -CaseSensitive) -join ',')) {
        throw 'Trusted tree attestation has unexpected fields.'
    }
    if ([int]$document.schemaVersion -ne $script:TrustedTreeAttestationSchemaVersion) {
        throw 'Unsupported trusted tree attestation schema version.'
    }
    if ([string]$document.sourceVersion -cnotmatch '^[0-9a-f]{40}$') {
        throw 'Trusted tree attestation records an invalid source commit.'
    }
    foreach ($hashField in @('treeHash', 'contentHash')) {
        if ([string]$document.$hashField -cnotmatch '^[0-9a-f]{64}$') {
            throw "Trusted tree attestation records an invalid $hashField."
        }
    }

    return $document
}

function Assert-TrustedTreeAttestation {
    <#
        .SYNOPSIS
        Re-verifies the trusted tree against its attestation, fail-closed.

        .DESCRIPTION
        Every difference is reported by name and kind, because "the trusted tree
        changed" is not actionable and a run that mutated one gate is a very
        different incident from one that dropped a file. The `Context` is the
        moment of the check -- before or after a specific model invocation or
        generated execution -- so a log line says which boundary was crossed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][object]$Attestation,
        [string]$ExpectedSourceVersion = '',
        [string]$Context = 'trusted tree'
    )

    if ($Attestation -is [string]) {
        $Attestation = Read-TrustedTreeAttestation -Path $Attestation
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedSourceVersion)) {
        if ([string]$Attestation.sourceVersion -cne $ExpectedSourceVersion.ToLowerInvariant()) {
            throw "Trusted tree attestation ($Context) was captured at a different pipeline revision."
        }
    }

    $entries = @(Get-TrustedTreeEntries -Root $TrustedRoot)
    $comparison = Get-TrustedTreePathComparison
    $observed = [ordered]@{}
    foreach ($entry in $entries) {
        $observed[[string]$entry.path] = $entry
    }

    $differences = [System.Collections.Generic.List[string]]::new()
    $attested = [System.Collections.Generic.HashSet[string]]::new(
        $(if ([System.OperatingSystem]::IsWindows()) { [System.StringComparer]::OrdinalIgnoreCase } else { [System.StringComparer]::Ordinal }))

    foreach ($file in @($Attestation.files)) {
        $path = [string]$file.path
        if (-not $attested.Add($path)) {
            $differences.Add("duplicated: $path")
            continue
        }
        if (-not $observed.Contains($path)) {
            $differences.Add("deleted: $path")
            continue
        }
        $current = $observed[$path]
        if (([string]$current.mode) -cne ([string]$file.mode)) {
            $differences.Add("mode changed: $path")
        }
        if (([long]$current.size) -ne ([long]$file.size)) {
            $differences.Add("size changed: $path")
        }
        if (([string]$current.sha256) -cne ([string]$file.sha256)) {
            $differences.Add("content changed: $path")
        }
    }

    foreach ($path in @($observed.Keys)) {
        if (-not $attested.Contains($path)) {
            $differences.Add("added: $path")
        }
    }

    if ($differences.Count -gt 0) {
        $detail = (@($differences | Sort-Object -CaseSensitive | Select-Object -First 12) -join '; ')
        throw "Trusted tree verification failed ($Context): $detail"
    }

    $treeHash = Get-TrustedTreeHash -Entries $entries
    if ($treeHash -cne ([string]$Attestation.treeHash)) {
        throw "Trusted tree hash does not match its attestation ($Context)."
    }
    $contentHash = Get-TrustedTreeContentHash -Entries $entries
    if ($contentHash -cne ([string]$Attestation.contentHash)) {
        throw "Trusted tree content hash does not match its attestation ($Context)."
    }
    if ([int]$Attestation.fileCount -ne $entries.Count) {
        throw "Trusted tree file count does not match its attestation ($Context)."
    }

    foreach ($property in (Get-TrustedTreeMapEntries -Map $Attestation.keyScripts)) {
        $path = [string]$property.Name
        if (-not $observed.Contains($path)) {
            throw "Trusted tree is missing a key script ($Context): $path"
        }
        if (([string]$observed[$path].sha256) -cne ([string]$property.Value)) {
            throw "Trusted key script does not match its attestation ($Context): $path"
        }
    }

    return [pscustomobject]@{
        TreeHash = $treeHash
        ContentHash = $contentHash
        FileCount = $entries.Count
        SourceVersion = [string]$Attestation.sourceVersion
        PipelineSha256 = [string]$Attestation.pipelineSha256
        Context = $Context
    }
}

function Assert-TrustedTreeMatchesReference {
    <#
        .SYNOPSIS
        Independently re-derives the trusted tree from a clean checkout.

        .DESCRIPTION
        This is the cross-agent half. The device agent attested the tree it
        staged; a separate credentialless job stages the same subset from its
        own clean checkout of the same pinned commit and asks whether the two
        describe the same bytes. It is the check that makes the attestation an
        independent statement about the pipeline revision rather than a
        self-report by the agent that ran the models.

        Modes are deliberately not compared here -- see
        Get-TrustedTreeContentHash -- but the pipeline definition and the key
        script identities are, because those cross platforms unchanged.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][object]$Attestation,
        [Parameter(Mandatory = $true)][string]$ReferenceRoot,
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [string]$ReferencePipelineDefinitionPath = '',
        [string]$Context = 'clean checkout'
    )

    if ($Attestation -is [string]) {
        $Attestation = Read-TrustedTreeAttestation -Path $Attestation
    }
    if ([string]$Attestation.sourceVersion -cne $ExpectedSourceVersion.ToLowerInvariant()) {
        throw "Trusted tree attestation ($Context) was captured at a different pipeline revision."
    }

    $entries = @(Get-TrustedTreeEntries -Root $ReferenceRoot)
    $contentHash = Get-TrustedTreeContentHash -Entries $entries
    if ($contentHash -cne ([string]$Attestation.contentHash)) {
        throw ("The trusted tree the run used does not match the pinned pipeline revision ($Context): " +
            "expected content hash $contentHash.")
    }
    if ([int]$Attestation.fileCount -ne $entries.Count) {
        throw "Trusted tree file count does not match the pinned pipeline revision ($Context)."
    }

    if (-not [string]::IsNullOrWhiteSpace($ReferencePipelineDefinitionPath)) {
        if (-not (Test-Path -LiteralPath $ReferencePipelineDefinitionPath -PathType Leaf)) {
            throw "Reference pipeline definition is missing ($Context): $ReferencePipelineDefinitionPath"
        }
        $pipelineSha256 = Get-TrustedTreeFileHash -Path $ReferencePipelineDefinitionPath
        if ($pipelineSha256 -cne ([string]$Attestation.pipelineSha256)) {
            throw "The pipeline definition the run used does not match the pinned revision ($Context)."
        }
    }

    $comparison = Get-TrustedTreePathComparison
    foreach ($property in (Get-TrustedTreeMapEntries -Map $Attestation.keyScripts)) {
        $path = [string]$property.Name
        $match = @($entries | Where-Object { ([string]$_.path).Equals($path, $comparison) })
        if ($match.Count -ne 1) {
            throw "The pinned pipeline revision has no key script named ($Context): $path"
        }
        if (([string]$match[0].sha256) -cne ([string]$property.Value)) {
            throw "A key script the run used differs from the pinned pipeline revision ($Context): $path"
        }
    }

    return [pscustomobject]@{
        ContentHash = $contentHash
        FileCount = $entries.Count
        SourceVersion = [string]$Attestation.sourceVersion
        Context = $Context
    }
}

function Assert-PathOutsideTrustedRoots {
    <#
        .SYNOPSIS
        Refuses a write target that lands in, or is, a trusted root.

        .DESCRIPTION
        Exact-file model write approvals are the one place where an agent is
        told "you may write here". A trusted root inside that set would let the
        agent replace the script that later judges its work, so the check is a
        hard refusal rather than a filter -- a silently dropped approval is a
        phase that fails much later for a reason nobody can trace.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$TrustedRoots
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $comparison = Get-TrustedTreePathComparison
    foreach ($root in $TrustedRoots) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $fullRoot = [System.IO.Path]::GetFullPath($root).TrimEnd(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar)
        if ($fullPath.Equals($fullRoot, $comparison)) {
            throw "Write approval targets a trusted root: $Path"
        }
        if ($fullPath.StartsWith($fullRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison)) {
            throw "Write approval targets a file inside a trusted root: $Path"
        }
    }
}
