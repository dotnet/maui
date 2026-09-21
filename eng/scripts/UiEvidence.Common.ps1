Set-StrictMode -Version Latest

function Assert-UiEvidenceSha {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($Value -notmatch '^[0-9a-fA-F]{40}$') {
        throw "$Name must be a 40-character Git commit SHA."
    }
}

function Assert-UiEvidenceId {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($Value -notmatch '^[a-z0-9][a-z0-9-]{0,63}$') {
        throw "$Name '$Value' is invalid."
    }
}

function Normalize-UiEvidenceRepositoryPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($Path -match '[\x00-\x1f:*?"<>|]') {
        throw "Repository path contains an unsafe character."
    }

    $normalized = $Path.Replace('\', '/').Trim()
    if ([string]::IsNullOrWhiteSpace($normalized) -or
        $normalized.StartsWith('/') -or
        $normalized -match '^[a-zA-Z]:' -or
        $normalized -match '(^|/)\.\.?(/|$)') {
        throw "Repository path '$Path' is not a safe relative path."
    }

    return $normalized
}

function Assert-UiEvidenceLocalPath([string]$Path) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith('\\') -or $fullPath.StartsWith('//')) {
        throw "UI evidence paths must be local."
    }
    for ($ancestor = $fullPath; $ancestor; $ancestor = [IO.Path]::GetDirectoryName($ancestor)) {
        if (Test-Path -LiteralPath $ancestor) {
            if (((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "UI evidence paths cannot contain a reparse point: $ancestor"
            }
        }
    }
    return $fullPath
}

function Resolve-UiEvidenceChildPath([string]$Root, [string]$RelativePath) {
    $rootPath = Assert-UiEvidenceLocalPath $Root
    $relative = Normalize-UiEvidenceRepositoryPath $RelativePath
    $fullPath = [IO.Path]::GetFullPath((Join-Path $rootPath $relative))
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    if (-not $fullPath.StartsWith($rootPath.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw "UI evidence path escapes its root: $RelativePath"
    }
    return Assert-UiEvidenceLocalPath $fullPath
}

function Get-UiEvidenceFiles([string]$Root) {
    $rootPath = Assert-UiEvidenceLocalPath $Root
    if (-not (Test-Path -LiteralPath $rootPath -PathType Container)) {
        throw "UI evidence directory does not exist: $rootPath"
    }
    $directories = [Collections.Generic.Stack[string]]::new()
    $directories.Push($rootPath)
    while ($directories.Count -gt 0) {
        foreach ($item in @(Get-ChildItem -LiteralPath $directories.Pop() -Force -ErrorAction Stop)) {
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "UI evidence trees cannot contain a reparse point: $($item.FullName)"
            }
            if ($item.PSIsContainer) {
                $directories.Push($item.FullName)
            }
            else {
                $item
            }
        }
    }
}

function New-UiEvidenceOutputDirectory([string]$Path, [string[]]$InputRoots = @()) {
    $fullPath = Assert-UiEvidenceLocalPath $Path
    if (Test-Path -LiteralPath $fullPath) {
        throw "UI evidence output directory must be fresh: $fullPath"
    }
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    foreach ($inputRoot in $InputRoots) {
        $source = Assert-UiEvidenceLocalPath $inputRoot
        if ($fullPath.Equals($source, $comparison) -or
            $fullPath.StartsWith($source.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar, $comparison)) {
            throw "UI evidence output must be outside its input directories."
        }
    }
    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    return $fullPath
}

function Get-UiEvidenceSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "File does not exist: $Path"
    }

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-UiEvidenceStringSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $bytes = [Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [Security.Cryptography.SHA256]::HashData($bytes)
    return [Convert]::ToHexString($hash).ToLowerInvariant()
}

function Get-UiEvidenceRequestKey {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Repository,

        [Parameter(Mandatory = $true)]
        [int]$PullRequestNumber,

        [Parameter(Mandatory = $true)]
        [string]$BaseCommitSha,

        [Parameter(Mandatory = $true)]
        [string]$HeadCommitSha,

        [Parameter(Mandatory = $true)]
        [string]$HarnessSha,

        [Parameter(Mandatory = $true)]
        [string]$RegistrySha256,

        [Parameter(Mandatory = $true)]
        [string]$ScenarioId,

        [Parameter(Mandatory = $true)]
        [string]$Platform
    )

    $identity = @(
        "schema=1",
        "repository=$Repository",
        "pr=$PullRequestNumber",
        "base=$($BaseCommitSha.ToLowerInvariant())",
        "head=$($HeadCommitSha.ToLowerInvariant())",
        "harness=$($HarnessSha.ToLowerInvariant())",
        "registry=$($RegistrySha256.ToLowerInvariant())",
        "scenario=$ScenarioId",
        "platform=$Platform"
    ) -join "`n"

    return "maui-ui-$((Get-UiEvidenceStringSha256 $identity).Substring(0, 24))"
}

function Read-UiEvidenceJson {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "JSON file does not exist: $Path"
    }

    try {
        return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        throw "JSON file is invalid: $Path. $($_.Exception.Message)"
    }
}

function Write-UiEvidenceJson {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object]$Value,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $directory = Split-Path -Parent $Path
    if ($directory) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    ConvertTo-Json -InputObject $Value -Depth 32 |
        Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-UiEvidenceRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $relative = [IO.Path]::GetRelativePath(
        [IO.Path]::GetFullPath($Root),
        [IO.Path]::GetFullPath($Path))

    return Normalize-UiEvidenceRepositoryPath $relative
}
