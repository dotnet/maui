<#
.SYNOPSIS
    Checks instruction files for drift against upstream documentation sources.

.DESCRIPTION
    Reads .sync.yaml manifest files and checks each configured source for changes:
    - GitHub issues: checks open/closed state via gh CLI
    - Web pages: fetches and computes content hash
    - GitHub releases: checks latest release tag and release notes
    - Index pages: crawls doc site indexes to discover new/untracked pages
    - Recently closed issues: discovers closed issues not yet in the manifest

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
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash($bytes)
    }
    finally {
        $sha.Dispose()
    }
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
        $json = gh api "repos/$Repo/releases?per_page=1" --jq '.[0] | {tag_name: .tag_name, name: .name, published_at: .published_at, body: .body}' 2>&1
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
        # Truncate release notes to first 2000 chars to keep report manageable
        $body = if ($release.body.Length -gt 2000) { $release.body.Substring(0, 2000) + '...' } else { $release.body }
        return @{
            status = 'ok'
            repo   = $Repo
            latest = @{
                tag          = $release.tag_name
                name         = $release.name
                published_at = $release.published_at
                release_notes = $body
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

function Get-IndexPageLinks {
    <#
    .SYNOPSIS
        Crawls an index page and extracts all documentation links.
    .DESCRIPTION
        Fetches an index/section page from a docs site and extracts relative
        links to child pages. Used to discover new pages not yet in the manifest.
    #>
    param(
        [string]$IndexUrl,
        [string]$BaseUrl
    )

    try {
        $response = Invoke-WebRequest -Uri $IndexUrl -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
        $content = $response.Content

        # Extract href links that are relative paths (not external, not anchors)
        $links = @()
        $hrefPattern = 'href="([^"#]+)"'
        $matches_ = [regex]::Matches($content, $hrefPattern)
        foreach ($m in $matches_) {
            $href = $m.Groups[1].Value
            # Skip external links, assets, anchors, and non-doc paths
            if ($href -match '^(https?://|mailto:|#|/assets/|/images/)') { continue }
            if ($href -match '\.(css|js|png|jpg|svg|ico|xml|json)$') { continue }

            # Resolve relative to index URL using System.Uri for correct path handling
            $resolvedUri = [System.Uri]::new([System.Uri]::new($IndexUrl), $href)
            $resolvedUrl = $resolvedUri.AbsoluteUri

            # Normalize: remove trailing slashes for comparison, then add back
            $resolvedUrl = $resolvedUrl.TrimEnd('/') + '/'
            if ($resolvedUrl -ne $IndexUrl -and $resolvedUrl.StartsWith($BaseUrl)) {
                $links += $resolvedUrl
            }
        }

        return @{
            status = 'ok'
            url    = $IndexUrl
            links  = ($links | Sort-Object -Unique)
        }
    }
    catch {
        return @{
            status = 'error'
            url    = $IndexUrl
            error  = $_.Exception.Message
        }
    }
}

function Find-UntrackedPages {
    <#
    .SYNOPSIS
        Compares discovered index links against tracked URLs in the manifest.
    .DESCRIPTION
        Crawls doc site index pages and identifies pages not yet tracked
        in any manifest source. Returns a list of untracked URLs.
    #>
    param(
        [string[]]$IndexUrls,
        [string]$BaseUrl,
        [string[]]$TrackedUrls
    )

    $allDiscovered = @()
    foreach ($indexUrl in $IndexUrls) {
        Write-Host "   🔎 Crawling index: $indexUrl..." -NoNewline
        $result = Get-IndexPageLinks -IndexUrl $indexUrl -BaseUrl $BaseUrl
        if ($result.status -eq 'ok') {
            Write-Host " found $($result.links.Count) links" -ForegroundColor Green
            $allDiscovered += $result.links
        }
        else {
            Write-Host " ❌ $($result.error)" -ForegroundColor Red
        }
    }

    $allDiscovered = $allDiscovered | Sort-Object -Unique

    # Normalize tracked URLs for comparison
    $normalizedTracked = $TrackedUrls | ForEach-Object { $_.TrimEnd('/') + '/' }

    $untracked = $allDiscovered | Where-Object {
        $normalized = $_.TrimEnd('/') + '/'
        $normalized -notin $normalizedTracked
    }

    return $untracked
}

function Get-RecentClosedIssues {
    <#
    .SYNOPSIS
        Discovers recently closed issues in a repo not yet tracked in the manifest.
    .DESCRIPTION
        Fetches issues closed in the last 90 days and identifies ones
        not already in the manifest's tracked issues list.
    #>
    param(
        [string]$Repo,
        [int[]]$TrackedIssueNumbers,
        [int]$DaysBack = 90
    )

    try {
        $since = (Get-Date).AddDays(-$DaysBack).ToString('yyyy-MM-ddTHH:mm:ssZ')
        # --paginate feeds each page through jq separately, so emit objects
        # (not wrapped in array) to allow clean concatenation across pages.
        # Wrap the entire output in [...] for ConvertFrom-Json.
        $raw = gh api --paginate "repos/$Repo/issues?state=closed&since=$since&per_page=100&sort=updated&direction=desc" --jq '.[] | select(.pull_request == null) | {number: .number, title: .title, closed_at: .closed_at, labels: [.labels[].name]}' 2>&1
        if ($LASTEXITCODE -ne 0) {
            return @{
                status = 'error'
                repo   = $Repo
                error  = "Failed to fetch issues: $raw"
            }
        }
        if (-not $raw -or $raw.Trim() -eq '') {
            $issues = @()
        }
        else {
            # Each line is a JSON object — wrap in array for parsing
            $jsonArray = "[$($raw -replace "`n", ',')]"
            $issues = $jsonArray | ConvertFrom-Json
        }
        $untracked = $issues | Where-Object { $_.number -notin $TrackedIssueNumbers }
        return @{
            status    = 'ok'
            repo      = $Repo
            untracked = @($untracked)
            total_checked = ($issues | Measure-Object).Count
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
        target            = ''
        secondary_targets = @()
        sources           = @()
        divergence        = @()
        path              = $Path
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
        elseif ($trimmed -eq 'secondary_targets:') {
            $currentSection = 'secondary_targets'
        }
        elseif ($currentSection -eq 'secondary_targets') {
            if ($trimmed -match '^-\s*(.+)$') {
                $manifest.secondary_targets += $Matches[1].Trim()
            }
            else {
                # Not a list item — fall through to other section checks below
                $currentSection = ''
                # Re-check this line against section headers
                if ($trimmed -eq 'sources:') { $currentSection = 'sources' }
                elseif ($trimmed -eq 'divergence:') { $currentSection = 'divergence' }
                elseif ($trimmed -eq 'style: |') { $currentSection = 'style' }
            }
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
            elseif ($trimmed -match '^coverage_gaps:') {
                if ($currentItem) {
                    $currentItem.coverage_gaps = @()
                    $currentSection = 'coverage_gaps'
                }
            }
            elseif ($trimmed -match '^sections:') {
                # sections are informational, pass through
            }
        }
        elseif ($currentSection -eq 'coverage_gaps') {
            # Source-entry patterns or section transition — exit coverage_gaps
            if ($trimmed -match '^-\s*(url|issue|releases):' -or
                $trimmed -eq 'divergence:' -or $trimmed -eq 'style: |' -or
                ($trimmed -ne '' -and -not $trimmed.StartsWith('-'))) {
                # Left coverage_gaps block — determine correct section
                if ($trimmed -eq 'divergence:') {
                    $currentSection = 'divergence'
                }
                elseif ($trimmed -eq 'style: |') {
                    $currentSection = 'style'
                }
                else {
                    $currentSection = 'sources'
                    # Re-process this line in sources context
                    if ($trimmed -match '^-\s*url:\s*(.+)$') {
                        $currentItem = @{ type = 'web'; url = $Matches[1].Trim() }
                        $manifest.sources += $currentItem
                    }
                    elseif ($trimmed -match '^-\s*issue:\s*(.+)$') {
                        $issueRef = $Matches[1].Trim()
                        if ($issueRef -match '^(.+)#(\d+)$') {
                            $currentItem = @{ type = 'issue'; repo = $Matches[1]; number = [int]$Matches[2] }
                            $manifest.sources += $currentItem
                        }
                    }
                    elseif ($trimmed -match '^-\s*releases:\s*(.+)$') {
                        $currentItem = @{ type = 'releases'; repo = $Matches[1].Trim() }
                        $manifest.sources += $currentItem
                    }
                }
            }
            elseif ($trimmed -match '^-\s*"?(.+?)"?$') {
                if ($currentItem -and $currentItem.ContainsKey('coverage_gaps')) {
                    $currentItem.coverage_gaps += $Matches[1]
                }
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
                $gaps = if ($source.ContainsKey('coverage_gaps')) { $source.coverage_gaps } else { @() }
                if ($result.status -eq 'ok') {
                    Write-Host " ✅ hash=$($result.content_hash)" -ForegroundColor Green
                }
                else {
                    Write-Host " ❌ Error: $($result.error)" -ForegroundColor Red
                }
                if ($gaps.Count -gt 0) {
                    Write-Host "      ⚠️ $($gaps.Count) known coverage gap(s)" -ForegroundColor Yellow
                }
                $entry = @{
                    type   = 'web'
                    url    = $source.url
                    result = $result
                }
                if ($gaps.Count -gt 0) {
                    $entry.coverage_gaps = $gaps
                }
                $sourceResults += $entry
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

    $resultEntry = @{
        manifest          = [System.IO.Path]::GetRelativePath($RepoRoot, $manifestPath)
        target            = $manifest.target
        sources           = $sourceResults
    }
    if ($manifest.secondary_targets.Count -gt 0) {
        $resultEntry.secondary_targets = $manifest.secondary_targets
    }
    $results += $resultEntry

    # --- Discovery: find untracked pages and recently closed issues ---

    # Collect tracked URLs and issue numbers from this manifest
    $trackedUrls = @($manifest.sources | Where-Object { $_.type -eq 'web' } | ForEach-Object { $_.url })
    $trackedIssueNumbers = @($manifest.sources | Where-Object { $_.type -eq 'issue' } | ForEach-Object { $_.number })
    $releaseRepos = @($manifest.sources | Where-Object { $_.type -eq 'releases' } | ForEach-Object { $_.repo })
    $issueRepos = @($manifest.sources | Where-Object { $_.type -eq 'issue' } | ForEach-Object { $_.repo } | Sort-Object -Unique)
    $allRepos = @($releaseRepos + $issueRepos | Sort-Object -Unique)

    # Discover base URL from tracked URLs and crawl index pages
    $baseUrls = $trackedUrls | ForEach-Object {
        if ($_ -match '^(https://[^/]+/[^/]+/)') { $Matches[1] }
    } | Sort-Object -Unique

    foreach ($baseUrl in $baseUrls) {
        Write-Host "`n🔍 Discovering new pages under $baseUrl" -ForegroundColor Cyan

        # Common doc site section indexes to crawl
        $indexUrls = @(
            "${baseUrl}reference/"
            "${baseUrl}patterns/"
            "${baseUrl}guides/"
        )

        $untrackedPages = Find-UntrackedPages -IndexUrls $indexUrls -BaseUrl $baseUrl -TrackedUrls $trackedUrls
        if ($untrackedPages.Count -gt 0) {
            Write-Host "   ⚠️ Found $($untrackedPages.Count) untracked page(s):" -ForegroundColor Yellow
            foreach ($page in $untrackedPages) {
                Write-Host "      📄 $page" -ForegroundColor Yellow
            }
            # Add to results
            $results[-1].untracked_pages = @($untrackedPages)
        }
        else {
            Write-Host "   ✅ All discovered pages are tracked" -ForegroundColor Green
        }
    }

    # Discover recently closed issues not in the manifest
    foreach ($repo in $allRepos) {
        Write-Host "`n🔍 Checking recently closed issues in $repo" -ForegroundColor Cyan
        # Filter tracked issue numbers to THIS repo to avoid cross-repo collisions
        $repoTrackedNumbers = @($manifest.sources | Where-Object { $_.type -eq 'issue' -and $_.repo -eq $repo } | ForEach-Object { $_.number })
        $closedResult = Get-RecentClosedIssues -Repo $repo -TrackedIssueNumbers $repoTrackedNumbers
        if ($closedResult.status -eq 'ok') {
            $untrackedCount = ($closedResult.untracked | Measure-Object).Count
            if ($untrackedCount -gt 0) {
                Write-Host "   ⚠️ Found $untrackedCount recently closed issue(s) not in manifest:" -ForegroundColor Yellow
                foreach ($issue in $closedResult.untracked) {
                    $labels = if ($issue.labels) { " [$($issue.labels -join ', ')]" } else { '' }
                    Write-Host "      #$($issue.number): $($issue.title)$labels" -ForegroundColor Yellow
                }
                $results[-1].untracked_closed_issues = @($closedResult.untracked)
            }
            else {
                Write-Host "   ✅ No new closed issues (checked $($closedResult.total_checked) in last 90 days)" -ForegroundColor Green
            }
        }
        else {
            Write-Host "   ❌ $($closedResult.error)" -ForegroundColor Red
        }
    }
}

# Output JSON report
# changes_detected flags sources that need attention:
#   - fetch errors (source may have moved)
#   - closed issues where resolution_expected is true (instruction may reference outdated workarounds)
#   - untracked pages discovered via index crawling
#   - untracked recently closed issues
$actionableChanges = $results | ForEach-Object { $_.sources } | Where-Object {
    $_.result.status -eq 'error' -or
    ($_.type -eq 'issue' -and $_.resolution_expected -and $_.result.status -eq 'ok' -and $_.result.state -eq 'closed')
}
$hasUntrackedPages = ($results | Where-Object { $_.untracked_pages.Count -gt 0 } | Measure-Object).Count -gt 0
$hasUntrackedIssues = ($results | Where-Object { $_.untracked_closed_issues.Count -gt 0 } | Measure-Object).Count -gt 0
$report = @{
    checked_at       = (Get-Date -Format 'o')
    manifests        = $results
    changes_detected = (($actionableChanges | Measure-Object).Count -gt 0) -or $hasUntrackedPages -or $hasUntrackedIssues
}

Write-Host "`n📊 Report:" -ForegroundColor Cyan
$report | ConvertTo-Json -Depth 10
