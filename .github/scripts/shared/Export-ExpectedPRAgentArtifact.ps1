function Export-ExpectedPRAgentArtifact {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [string]$DestinationRoot,

        [Parameter(Mandatory = $false)]
        [string]$DiagnosticsRoot,

        [Parameter(Mandatory = $false)]
        [string]$TokenUsageRoot,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$MaxFileCount = 128,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxFileBytes = 16MB,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxTotalBytes = 128MB
    )

    if (-not (Get-Command Copy-BoundedDiagnosticFile -CommandType Function -ErrorAction SilentlyContinue)) {
        throw 'Copy-BoundedDiagnosticFile must be loaded before exporting the PRAgent artifact.'
    }

    $pathComparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    } else {
        [System.StringComparison]::Ordinal
    }
    $pathComparer = if ($IsWindows) {
        [System.StringComparer]::OrdinalIgnoreCase
    } else {
        [System.StringComparer]::Ordinal
    }
    $parentPrefix = "..$([System.IO.Path]::DirectorySeparatorChar)"
    $alternateParentPrefix = "..$([System.IO.Path]::AltDirectorySeparatorChar)"

    function Get-RegularDirectoryPath {
        param(
            [Parameter(Mandatory = $true)]
            [string]$Path,

            [Parameter(Mandatory = $true)]
            [string]$Description
        )

        $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
        if (-not $item.PSIsContainer -or
            ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
            throw "$Description must be a regular directory: '$Path'."
        }

        return [System.IO.Path]::TrimEndingDirectorySeparator(
            [System.IO.Path]::GetFullPath($item.FullName))
    }

    function Test-PathIsContained {
        param(
            [Parameter(Mandatory = $true)]
            [string]$Root,

            [Parameter(Mandatory = $true)]
            [string]$Path
        )

        $relativePath = [System.IO.Path]::GetRelativePath($Root, $Path)
        return (
            $relativePath -ne '.' -and
            $relativePath -ne '..' -and
            -not [System.IO.Path]::IsPathRooted($relativePath) -and
            -not $relativePath.StartsWith($parentPrefix, [System.StringComparison]::Ordinal) -and
            -not $relativePath.StartsWith(
                $alternateParentPrefix,
                [System.StringComparison]::Ordinal)
        )
    }

    function Get-ContainedRegularFile {
        param(
            [Parameter(Mandatory = $true)]
            [string]$BoundaryRoot,

            [Parameter(Mandatory = $true)]
            [string]$Path
        )

        $fullPath = [System.IO.Path]::GetFullPath($Path)
        if (-not (Test-PathIsContained -Root $BoundaryRoot -Path $fullPath)) {
            throw "Artifact source escaped its canonical root: '$Path'."
        }

        $relativePath = [System.IO.Path]::GetRelativePath($BoundaryRoot, $fullPath)
        $currentPath = $BoundaryRoot
        foreach ($segment in $relativePath.Split(
            [char[]]@(
                [System.IO.Path]::DirectorySeparatorChar,
                [System.IO.Path]::AltDirectorySeparatorChar),
            [System.StringSplitOptions]::RemoveEmptyEntries)) {
            $currentPath = Join-Path $currentPath $segment
            $item = Get-Item -LiteralPath $currentPath -Force -ErrorAction Stop
            if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                throw "Artifact source contains an unsupported reparse point: '$currentPath'."
            }
        }

        if ($item.PSIsContainer -or $item -isnot [System.IO.FileInfo]) {
            throw "Artifact source must be a regular file: '$fullPath'."
        }

        return $item
    }

    function Get-ContainedRegularDirectory {
        param(
            [Parameter(Mandatory = $true)]
            [string]$BoundaryRoot,

            [Parameter(Mandatory = $true)]
            [string]$Path
        )

        $fullPath = [System.IO.Path]::GetFullPath($Path)
        if (-not (Test-PathIsContained -Root $BoundaryRoot -Path $fullPath)) {
            throw "Artifact source escaped its canonical root: '$Path'."
        }

        $relativePath = [System.IO.Path]::GetRelativePath($BoundaryRoot, $fullPath)
        $currentPath = $BoundaryRoot
        foreach ($segment in $relativePath.Split(
            [char[]]@(
                [System.IO.Path]::DirectorySeparatorChar,
                [System.IO.Path]::AltDirectorySeparatorChar),
            [System.StringSplitOptions]::RemoveEmptyEntries)) {
            $currentPath = Join-Path $currentPath $segment
            $item = Get-Item -LiteralPath $currentPath -Force -ErrorAction Stop
            if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                throw "Artifact source contains an unsupported reparse point: '$currentPath'."
            }
        }

        if (-not $item.PSIsContainer) {
            throw "Artifact source must be a regular directory: '$fullPath'."
        }

        return [System.IO.Path]::TrimEndingDirectorySeparator(
            [System.IO.Path]::GetFullPath($item.FullName))
    }

    function Assert-DestinationParentIsRegular {
        param([Parameter(Mandatory = $true)][string]$Path)

        $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
        if (-not $item.PSIsContainer -or
            ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
            throw "Artifact destination parent must be a regular directory: '$($item.FullName)'."
        }
    }

    $repositoryPath = Get-RegularDirectoryPath `
        -Path $RepositoryRoot `
        -Description 'Repository root'
    $destinationPath = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($DestinationRoot))
    if (Test-Path -LiteralPath $destinationPath) {
        throw "Artifact destination must not already exist: '$destinationPath'."
    }

    $destinationParent = Split-Path -Parent $destinationPath
    Assert-DestinationParentIsRegular -Path $destinationParent
    if (Test-PathIsContained -Root $repositoryPath -Path $destinationPath) {
        throw 'Artifact destination must not be contained by the repository root.'
    }

    $sourcePRAgentPath = [System.IO.Path]::GetFullPath(
        (Join-Path $repositoryPath "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"))
    $sourcePRAgentExists = Test-Path -LiteralPath $sourcePRAgentPath -PathType Container
    if ($sourcePRAgentExists) {
        $sourcePRAgentPath = Get-ContainedRegularDirectory `
            -BoundaryRoot $repositoryPath `
            -Path $sourcePRAgentPath
    }

    $diagnosticsPath = $null
    if (-not [string]::IsNullOrWhiteSpace($DiagnosticsRoot) -and
        (Test-Path -LiteralPath $DiagnosticsRoot -PathType Container)) {
        $diagnosticsPath = Get-RegularDirectoryPath `
            -Path $DiagnosticsRoot `
            -Description 'Diagnostics source'
        if (Test-PathIsContained -Root $diagnosticsPath -Path $destinationPath) {
            throw 'Artifact destination must not be contained by the diagnostics source.'
        }
    }

    $tokenUsagePath = $null
    if (-not [string]::IsNullOrWhiteSpace($TokenUsageRoot) -and
        (Test-Path -LiteralPath $TokenUsageRoot -PathType Container)) {
        $tokenUsagePath = Get-RegularDirectoryPath `
            -Path $TokenUsageRoot `
            -Description 'Token-usage source'
        if (Test-PathIsContained -Root $tokenUsagePath -Path $destinationPath) {
            throw 'Artifact destination must not be contained by the token-usage source.'
        }
    }

    $files = [System.Collections.Generic.List[object]]::new()
    $destinationFiles = [System.Collections.Generic.HashSet[string]]::new($pathComparer)
    $state = [pscustomobject]@{ TotalBytes = 0L }

    function Add-ArtifactFile {
        param(
            [Parameter(Mandatory = $true)]
            [string]$BoundaryRoot,

            [Parameter(Mandatory = $true)]
            [string]$SourcePath,

            [Parameter(Mandatory = $true)]
            [string]$DestinationRelativePath,

            [Parameter(Mandatory = $false)]
            [switch]$Truncatable
        )

        $destinationRelative = $DestinationRelativePath.Replace(
            [System.IO.Path]::AltDirectorySeparatorChar,
            [System.IO.Path]::DirectorySeparatorChar)
        $destinationFullPath = [System.IO.Path]::GetFullPath(
            (Join-Path $destinationPath $destinationRelative))
        if (-not (Test-PathIsContained -Root $destinationPath -Path $destinationFullPath)) {
            throw "Artifact destination escaped its canonical root: '$DestinationRelativePath'."
        }
        if (-not $destinationFiles.Add($destinationRelative)) {
            return
        }

        $sourceItem = Get-ContainedRegularFile `
            -BoundaryRoot $BoundaryRoot `
            -Path $SourcePath
        $copyLength = [long]$sourceItem.Length
        $truncate = $false
        if ($copyLength -gt $MaxFileBytes) {
            if (-not $Truncatable) {
                throw "Artifact file '$DestinationRelativePath' exceeded the $MaxFileBytes-byte per-file limit."
            }

            $copyLength = $MaxFileBytes
            $truncate = $true
        }
        if ($files.Count -ge $MaxFileCount) {
            throw "Artifact export exceeded the $MaxFileCount-file limit."
        }
        if ($copyLength -gt ($MaxTotalBytes - $state.TotalBytes)) {
            throw "Artifact export exceeded the $MaxTotalBytes-byte aggregate limit."
        }

        $state.TotalBytes += $copyLength
        [void]$files.Add([pscustomobject]@{
            BoundaryRoot            = $BoundaryRoot
            SourcePath              = [System.IO.Path]::GetFullPath($sourceItem.FullName)
            SourceLength            = [long]$sourceItem.Length
            DestinationRelativePath = $destinationRelative
            CopyLength              = $copyLength
            Truncated               = $truncate
        })
    }

    function Add-ExactPRAgentFile {
        param(
            [Parameter(Mandatory = $true)]
            [string]$RelativePath,

            [Parameter(Mandatory = $false)]
            [switch]$Truncatable
        )

        if (-not $sourcePRAgentExists) {
            return
        }

        $source = Join-Path $sourcePRAgentPath $RelativePath
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Add-ArtifactFile `
                -BoundaryRoot $repositoryPath `
                -SourcePath $source `
                -DestinationRelativePath "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/$RelativePath" `
                -Truncatable:$Truncatable
        }
    }

    function Add-DirectPatternFiles {
        param(
            [Parameter(Mandatory = $true)]
            [string]$BoundaryRoot,

            [Parameter(Mandatory = $true)]
            [string]$SourceDirectory,

            [Parameter(Mandatory = $true)]
            [string]$Pattern,

            [Parameter(Mandatory = $true)]
            [string]$DestinationDirectory,

            [Parameter(Mandatory = $false)]
            [switch]$Truncatable
        )

        if (-not (Test-Path -LiteralPath $SourceDirectory -PathType Container)) {
            return
        }

        $regularDirectory = Get-RegularDirectoryPath `
            -Path $SourceDirectory `
            -Description 'Pattern source'
        if (-not $regularDirectory.Equals($BoundaryRoot, $pathComparison) -and
            -not (Test-PathIsContained -Root $BoundaryRoot -Path $regularDirectory)) {
            throw "Pattern source escaped its canonical root: '$SourceDirectory'."
        }

        foreach ($source in [System.IO.Directory]::EnumerateFiles(
            $regularDirectory,
            $Pattern,
            [System.IO.SearchOption]::TopDirectoryOnly)) {
            $name = [System.IO.Path]::GetFileName($source)
            if ($name -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$') {
                throw 'Artifact source contained an unsupported file name in an allowlisted directory.'
            }
            Add-ArtifactFile `
                -BoundaryRoot $BoundaryRoot `
                -SourcePath $source `
                -DestinationRelativePath (Join-Path $DestinationDirectory $name) `
                -Truncatable:$Truncatable
        }
    }

    foreach ($relativePath in @(
        'gate/content.md'
        'gate/gate-result.txt'
        'gate/verify-tests-fail/verification-report.md'
        'gate/verify-tests-fail/verification-log.txt'
        'pre-flight/content.md'
        'pre-flight/code-review.md'
        'expert-pr-eval/content.md'
        'try-fix/content.md'
        'try-fix-1/content.md'
        'try-fix-2/content.md'
        'try-fix-3/content.md'
        'try-fix-4/content.md'
        'pr-finalize/content.md'
        'pr-finalize/pr-finalize-summary.md'
        'report/content.md'
        'regression-check/content.md'
        'regression-check/result.txt'
        'regression-check/risks.json'
        'regression-check/inline-findings.json'
        'uitests/content.md'
        'uitests/ai-categories.md'
        'inline-findings.json'
        'review-summary.md'
        'winner.json'
        'pr-plus-reviewer/content.md'
    )) {
        Add-ExactPRAgentFile -RelativePath $relativePath
    }

    foreach ($relativePath in @(
        'pr-plus-reviewer/candidate.patch'
        'pr-plus-reviewer/reviewer.patch'
    )) {
        Add-ExactPRAgentFile -RelativePath $relativePath -Truncatable
    }

    if ($sourcePRAgentExists) {
        Add-DirectPatternFiles `
            -BoundaryRoot $repositoryPath `
            -SourceDirectory (Join-Path $sourcePRAgentPath 'pr-plus-reviewer') `
            -Pattern '*.log' `
            -DestinationDirectory "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-plus-reviewer" `
            -Truncatable
        Add-DirectPatternFiles `
            -BoundaryRoot $repositoryPath `
            -SourceDirectory (Join-Path $sourcePRAgentPath 'gate/verify-tests-fail') `
            -Pattern 'test-without-fix*.log' `
            -DestinationDirectory "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/verify-tests-fail" `
            -Truncatable
        Add-DirectPatternFiles `
            -BoundaryRoot $repositoryPath `
            -SourceDirectory (Join-Path $sourcePRAgentPath 'gate/verify-tests-fail') `
            -Pattern 'test-with-fix*.log' `
            -DestinationDirectory "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/verify-tests-fail" `
            -Truncatable
    }

    if ($null -ne $diagnosticsPath) {
        Add-DirectPatternFiles `
            -BoundaryRoot $diagnosticsPath `
            -SourceDirectory $diagnosticsPath `
            -Pattern 'copilot_review_output*.md' `
            -DestinationDirectory '.'
    }

    if ($null -ne $tokenUsagePath) {
        Add-DirectPatternFiles `
            -BoundaryRoot $tokenUsagePath `
            -SourceDirectory $tokenUsagePath `
            -Pattern 'copilot-token-usage-*.json' `
            -DestinationDirectory 'copilot-token-usage/raw'
    }

    $destinationPRAgent = Join-Path `
        $destinationPath `
        "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    $truncatedFiles = 0
    try {
        New-Item -ItemType Directory -Path $destinationPRAgent -Force -ErrorAction Stop |
            Out-Null
        foreach ($file in @($files | Sort-Object DestinationRelativePath)) {
            $current = Get-ContainedRegularFile `
                -BoundaryRoot $file.BoundaryRoot `
                -Path $file.SourcePath
            if ($current.Length -ne $file.SourceLength) {
                throw "Artifact source changed during export: '$($file.SourcePath)'."
            }

            $destination = [System.IO.Path]::GetFullPath(
                (Join-Path $destinationPath $file.DestinationRelativePath))
            if (-not (Test-PathIsContained -Root $destinationPath -Path $destination)) {
                throw "Artifact destination escaped its canonical root: '$destination'."
            }

            $destinationParentPath = Split-Path -Parent $destination
            if (-not (Test-Path -LiteralPath $destinationParentPath)) {
                New-Item `
                    -ItemType Directory `
                    -Path $destinationParentPath `
                    -Force `
                    -ErrorAction Stop |
                    Out-Null
            }

            if ($file.Truncated) {
                $copyResult = Copy-BoundedDiagnosticFile `
                    -Source $current.FullName `
                    -Destination $destination `
                    -MaxBytes $file.CopyLength
                if ($copyResult.SourceBytes -ne $file.SourceLength -or
                    $copyResult.CopiedBytes -ne $file.CopyLength -or
                    -not $copyResult.Truncated) {
                    throw "Artifact source changed during truncated export: '$($file.SourcePath)'."
                }
                $truncatedFiles++
            } else {
                Copy-Item `
                    -LiteralPath $current.FullName `
                    -Destination $destination `
                    -Force `
                    -ErrorAction Stop
                $copiedItem = Get-Item -LiteralPath $destination -Force -ErrorAction Stop
                if ($copiedItem.Length -ne $file.SourceLength) {
                    throw "Artifact copy length changed during export: '$($file.SourcePath)'."
                }
            }
        }
    } catch {
        Remove-Item `
            -LiteralPath $destinationPath `
            -Recurse `
            -Force `
            -ErrorAction SilentlyContinue
        throw
    }

    return [pscustomobject]@{
        CopiedFiles    = $files.Count
        CopiedBytes    = $state.TotalBytes
        TruncatedFiles = $truncatedFiles
        PRAgentPath    = $destinationPRAgent
    }
}
