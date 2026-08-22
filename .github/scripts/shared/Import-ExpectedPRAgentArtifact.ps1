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
        [long]$MaxTotalBytes = 128MB
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

    $candidates = @(
        Get-ChildItem `
            -LiteralPath $rootPath `
            -Recurse `
            -Directory `
            -Filter 'PRAgent' `
            -Force `
            -ErrorAction Stop
    )
    if ($candidates.Count -ne 1) {
        throw "CopilotLogs must contain exactly one PRAgent directory; found $($candidates.Count)."
    }

    $candidatePath = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($candidates[0].FullName))
    $pathComparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    } else {
        [System.StringComparison]::Ordinal
    }
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
        -MaxTotalBytes $MaxTotalBytes
}
