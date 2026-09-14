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

    if ($Path.IndexOf([char]0) -ge 0) {
        throw "Repository path contains a NUL character."
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
