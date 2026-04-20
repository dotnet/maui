<#
.SYNOPSIS
    Checks instruction files for drift against upstream documentation sources.

.DESCRIPTION
    Reads .sync.yaml manifest files and checks each configured source for changes:
    - GitHub issues: checks open/closed state via gh CLI
    - Web pages: fetches and computes content hash
    - GitHub releases: checks latest release tag

    Outputs a JSON report to stdout describing what changed.

.PARAMETER ManifestPath
    Path to a specific .sync.yaml manifest. If not provided, discovers all manifests.

.PARAMETER RepoRoot
    Repository root directory. Defaults to current directory.

.EXAMPLE
    pwsh Check-Staleness.ps1
    pwsh Check-Staleness.ps1 -ManifestPath .github/instructions/gh-aw-workflows.sync.yaml
#>

param(
    [string]$ManifestPath,
    [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

function Get-ContentHash {
    param([string]$Content)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Content)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return [System.BitConverter]::ToString($hash).Replace('-', '').ToLower().Substring(0, 16)
}

function Test-GitHubIssue {
    param(
        [string]$Repo,
        [int]$Number
    )

    try {
        $json = gh api "repos/$Repo/issues/$Number" --jq '{state: .state, title: .title, updated_at: .updated_at, closed_at: .closed_at}' 2>&1
        if ($LASTEXITCODE -ne 0) {
            return @{
                status  = 'error'
                error   = "Failed to fetch issue: $json"
                number  = $Number
                repo    = $Repo
            }
        }
        $issue = $json | ConvertFrom-Json
        return @{
            status     = 'ok'
            number     = $Number
            repo       = $Repo
            state      = $issue.state
            title      = $issue.title
            updated_at = $issue.updated_at
            closed_at  = $issue.closed_at
        }
    }
    catch {
        return @{
            status = 'error'
            error  = $_.Exception.Message
            number = $Number
            repo   = $Repo
        }
    }
}

function Test-WebPage {
    param([string]$Url)

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
        $hash = Get-ContentHash -Content $response.Content
        return @{
            status       = 'ok'
            url          = $Url
            content_hash = $hash
            status_code  = $response.StatusCode
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        return @{
            status      = 'error'
            url         = $Url
            error       = $_.Exception.Message
            status_code = $statusCode
        }
    }
}

function Get-GitHubLatestRelease {
    param([string]$Repo)

    try {
        $json = gh api "repos/$Repo/releases?per_page=1" --jq '.[0] | {tag_name: .tag_name, name: .name, published_at: .published_at}' 2>&1
        if ($LASTEXITCODE -ne 0) {
            return @{
                status = 'error'
                repo   = $Repo
                error  = "Failed to fetch releases: $json"
            }
        }
        if (-not $json -or $json -eq 'null') {
            return @{
                status = 'ok'
                repo   = $Repo
                latest = $null
            }
        }
        $release = $json | ConvertFrom-Json
        return @{
            status = 'ok'
            repo   = $Repo
            latest = @{
                tag          = $release.tag_name
                name         = $release.name
                published_at = $release.published_at
            }
        }
    }
    catch {
        return @{
            status = 'error'
            repo   = $Repo
            error  = $_.Exception.Message
        }
    }
}

function ConvertFrom-SyncManifest {
    param([string]$Path)

    # Simple YAML parser for our specific manifest format
    $content = Get-Content $Path -Raw
    $manifest = @{
        target     = ''
        sources    = @()
        divergence = @()
        path       = $Path
    }

    $lines = $content -split "`n"
    $currentSection = ''
    $currentItem = $null

    foreach ($line in $lines) {
        $trimmed = $line.Trim()

        # Skip comments and empty lines
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }

        if ($trimmed -match '^target:\s*(.+)$') {
            $manifest.target = $Matches[1].Trim()
        }
        elseif ($trimmed -eq 'sources:') {
            $currentSection = 'sources'
        }
        elseif ($trimmed -eq 'divergence:') {
            $currentSection = 'divergence'
        }
        elseif ($trimmed -eq 'style: |') {
            $currentSection = 'style'
        }
        elseif ($currentSection -eq 'sources') {
            if ($trimmed -match '^-\s*url:\s*(.+)$') {
                $currentItem = @{ type = 'web'; url = $Matches[1].Trim() }
                $manifest.sources += $currentItem
            }
            elseif ($trimmed -match '^-\s*issue:\s*(.+)$') {
                $issueRef = $Matches[1].Trim()
                if ($issueRef -match '^(.+)#(\d+)$') {
                    $currentItem = @{
                        type   = 'issue'
                        repo   = $Matches[1]
                        number = [int]$Matches[2]
                    }
                    $manifest.sources += $currentItem
                }
            }
            elseif ($trimmed -match '^-\s*releases:\s*(.+)$') {
                $currentItem = @{ type = 'releases'; repo = $Matches[1].Trim() }
                $manifest.sources += $currentItem
            }
            elseif ($trimmed -match '^resolution_expected:\s*(.+)$') {
                if ($currentItem) {
                    $currentItem.resolution_expected = $Matches[1].Trim() -eq 'true'
                }
            }
            elseif ($trimmed -match '^sections:') {
                # sections are informational, skip for now
            }
        }
        elseif ($currentSection -eq 'divergence') {
            if ($trimmed -match '^-\s*section:\s*"?(.+?)"?$') {
                $currentItem = @{ section = $Matches[1] }
                $manifest.divergence += $currentItem
            }
            elseif ($trimmed -match '^reason:\s*"?(.+?)"?$') {
                if ($currentItem) {
                    $currentItem.reason = $Matches[1]
                }
            }
        }
    }

    return $manifest
}

# --- Main ---

Write-Host "🔍 Checking instruction file staleness..." -ForegroundColor Cyan

# Discover manifests
$manifests = @()
if ($ManifestPath) {
    $fullPath = Join-Path $RepoRoot $ManifestPath
    if (-not (Test-Path $fullPath)) {
        Write-Error "Manifest not found: $fullPath"
        exit 1
    }
    $manifests += $fullPath
}
else {
    # Use .github/ as primary search path since manifests live next to instruction files.
    # Also search repo root for any other manifests.
    $searchPaths = @(
        (Join-Path $RepoRoot '.github')
    )
    $manifests = @()
    foreach ($sp in $searchPaths) {
        if (Test-Path $sp) {
            $manifests += Get-ChildItem -Path $sp -Filter '*.sync.yaml' -Recurse -Depth 4 -ErrorAction SilentlyContinue |
                Select-Object -ExpandProperty FullName
        }
    }
}

if ($manifests.Count -eq 0) {
    Write-Host "No .sync.yaml manifests found." -ForegroundColor Yellow
    @{ manifests = @(); changes_detected = $false } | ConvertTo-Json -Depth 10
    exit 0
}

Write-Host "Found $($manifests.Count) manifest(s)" -ForegroundColor Green

$results = @()

foreach ($manifestPath in $manifests) {
    Write-Host "`n📋 Processing: $manifestPath" -ForegroundColor Cyan

    $manifest = ConvertFrom-SyncManifest -Path $manifestPath
    Write-Host "   Target: $($manifest.target)"
    Write-Host "   Sources: $($manifest.sources.Count)"

    $sourceResults = @()

    foreach ($source in $manifest.sources) {
        switch ($source.type) {
            'issue' {
                Write-Host "   🔗 Checking issue $($source.repo)#$($source.number)..." -NoNewline
                $result = Test-GitHubIssue -Repo $source.repo -Number $source.number
                $resExpected = if ($source.ContainsKey('resolution_expected')) { $source.resolution_expected } else { $false }
                if ($result.status -eq 'ok') {
                    $stateEmoji = if ($result.state -eq 'closed') { '✅' } else { '🟡' }
                    Write-Host " $stateEmoji $($result.state)" -ForegroundColor $(if ($result.state -eq 'closed') { 'Green' } else { 'Yellow' })
                }
                else {
                    Write-Host " ❌ Error: $($result.error)" -ForegroundColor Red
                }
                $sourceResults += @{
                    type                = 'issue'
                    ref                 = "$($source.repo)#$($source.number)"
                    resolution_expected = $resExpected
                    result              = $result
                }
            }
            'web' {
                Write-Host "   🌐 Checking $($source.url)..." -NoNewline
                $result = Test-WebPage -Url $source.url
                if ($result.status -eq 'ok') {
                    Write-Host " ✅ hash=$($result.content_hash)" -ForegroundColor Green
                }
                else {
                    Write-Host " ❌ Error: $($result.error)" -ForegroundColor Red
                }
                $sourceResults += @{
                    type   = 'web'
                    url    = $source.url
                    result = $result
                }
            }
            'releases' {
                Write-Host "   📦 Checking releases for $($source.repo)..." -NoNewline
                $result = Get-GitHubLatestRelease -Repo $source.repo
                if ($result.status -eq 'ok' -and $result.latest) {
                    Write-Host " ✅ latest=$($result.latest.tag)" -ForegroundColor Green
                }
                elseif ($result.status -eq 'ok') {
                    Write-Host " ℹ️ No releases found" -ForegroundColor Yellow
                }
                else {
                    Write-Host " ❌ Error: $($result.error)" -ForegroundColor Red
                }
                $sourceResults += @{
                    type   = 'releases'
                    repo   = $source.repo
                    result = $result
                }
            }
        }
    }

    $results += @{
        manifest = [System.IO.Path]::GetRelativePath($RepoRoot, $manifestPath)
        target   = $manifest.target
        sources  = $sourceResults
    }
}

# Output JSON report
# changes_detected flags sources that need attention:
#   - fetch errors (source may have moved)
#   - closed issues where resolution_expected is true (instruction may reference outdated workarounds)
#   - open issues where resolution_expected is true (expected resolution hasn't happened yet)
$actionableChanges = $results | ForEach-Object { $_.sources } | Where-Object {
    $_.result.status -eq 'error' -or
    ($_.type -eq 'issue' -and $_.resolution_expected -and $_.result.status -eq 'ok' -and $_.result.state -eq 'closed')
}
$report = @{
    checked_at       = (Get-Date -Format 'o')
    manifests        = $results
    changes_detected = ($actionableChanges | Measure-Object).Count -gt 0
}

Write-Host "`n📊 Report:" -ForegroundColor Cyan
$report | ConvertTo-Json -Depth 10
