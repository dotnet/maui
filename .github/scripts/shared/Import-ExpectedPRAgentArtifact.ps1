function Import-ExpectedPRAgentArtifact {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactRoot,

        [Parameter(Mandatory = $true)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$MaxFileCount = 2048,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$MaxDirectoryCount = 1024,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [long]::MaxValue)]
        [long]$MaxFileBytes = 16MB,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [long]::MaxValue)]
        [long]$MaxTotalBytes = 128MB,

        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [string[]]$TruncateOversizedFileExtensions = @('.log', '.patch', '.diff')
    )

    $rootItem = Get-Item -LiteralPath $ArtifactRoot -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer -or
        ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
        throw "CopilotLogs root must be a regular directory: '$ArtifactRoot'."
    }

    $rootPath = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($rootItem.FullName))
    $expectedPath = [System.IO.Path]::GetFullPath(
        (Join-Path $rootPath "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"))
    $expectedRelativePath = [System.IO.Path]::GetRelativePath($rootPath, $expectedPath)
    $parentPrefix = "..$([System.IO.Path]::DirectorySeparatorChar)"
    $alternateParentPrefix = "..$([System.IO.Path]::AltDirectorySeparatorChar)"
    if ($expectedRelativePath -eq '.' -or
        $expectedRelativePath -eq '..' -or
        [System.IO.Path]::IsPathRooted($expectedRelativePath) -or
        $expectedRelativePath.StartsWith($parentPrefix, [System.StringComparison]::Ordinal) -or
        $expectedRelativePath.StartsWith($alternateParentPrefix, [System.StringComparison]::Ordinal)) {
        throw 'Expected PRAgent path escaped the CopilotLogs root.'
    }

    $pathComparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    } else {
        [System.StringComparison]::Ordinal
    }
    $pendingDirectories = [System.Collections.Generic.Stack[string]]::new()
    $pendingDirectories.Push($rootPath)
    $candidates = [System.Collections.Generic.List[System.IO.DirectoryInfo]]::new()
    $discoveredDirectoryCount = 1

    while ($pendingDirectories.Count -gt 0) {
        $directoryPath = $pendingDirectories.Pop()
        $directoryItem = Get-Item -LiteralPath $directoryPath -Force -ErrorAction Stop
        if (-not $directoryItem.PSIsContainer -or
            ($directoryItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
            throw "CopilotLogs discovery encountered an unsupported reparse point: '$directoryPath'."
        }

        foreach ($entryPath in [System.IO.Directory]::EnumerateDirectories($directoryPath)) {
            $entry = Get-Item -LiteralPath $entryPath -Force -ErrorAction Stop
            if (-not $entry.PSIsContainer) {
                continue
            }

            $entryFullPath = [System.IO.Path]::GetFullPath($entry.FullName)
            $entryRelativePath = [System.IO.Path]::GetRelativePath($rootPath, $entryFullPath)
            if ($entryRelativePath -eq '.' -or
                $entryRelativePath -eq '..' -or
                [System.IO.Path]::IsPathRooted($entryRelativePath) -or
                $entryRelativePath.StartsWith($parentPrefix, [System.StringComparison]::Ordinal) -or
                $entryRelativePath.StartsWith(
                    $alternateParentPrefix,
                    [System.StringComparison]::Ordinal)) {
                throw "CopilotLogs directory escaped the canonical artifact root: '$entryPath'."
            }

            if ($entry.Name.Equals('PRAgent', $pathComparison)) {
                [void]$candidates.Add($entry)
                if ($candidates.Count -gt 1) {
                    throw "CopilotLogs must contain exactly one PRAgent directory; found $($candidates.Count)."
                }
            }

            if ($entry.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                continue
            }

            $discoveredDirectoryCount++
            if ($discoveredDirectoryCount -gt $MaxDirectoryCount) {
                throw "CopilotLogs discovery exceeded the $MaxDirectoryCount-directory limit."
            }

            $pendingDirectories.Push($entryFullPath)
        }
    }

    if ($candidates.Count -ne 1) {
        throw "CopilotLogs must contain exactly one PRAgent directory; found $($candidates.Count)."
    }

    $candidatePath = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($candidates[0].FullName))
    if (-not $candidatePath.Equals($expectedPath, $pathComparison)) {
        throw "PRAgent directory was not at the expected path for PR $PRNumber."
    }
    if ($candidates[0].Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw 'PRAgent directory must not be a reparse point.'
    }

    return Copy-BoundedRegularFileTree `
        -SourceDirectory $candidatePath `
        -DestinationDirectory $DestinationDirectory `
        -MaxFileCount $MaxFileCount `
        -MaxDirectoryCount $MaxDirectoryCount `
        -MaxFileBytes $MaxFileBytes `
        -MaxTotalBytes $MaxTotalBytes `
        -TruncateOversizedFileExtensions $TruncateOversizedFileExtensions
}
