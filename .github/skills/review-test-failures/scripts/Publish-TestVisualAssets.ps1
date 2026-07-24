#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes /review tests visual comparisons to a durable GitHub asset branch.

.DESCRIPTION
    Reads visual evidence gathered from public AzDO APIs, downloads only validated PNG
    attachments and the exact snapshot baseline from the tested merge commit, uploads
    them to a dedicated GitHub branch, and records immutable URLs for deterministic
    insertion into the single /review tests analysis comment.

    Visual publishing is supplementary evidence. Failures are recorded in context.json
    and never change the deterministic merge-readiness gate.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [int]$PrNumber,

    [Parameter(Mandatory = $true)]
    [string]$ContextJsonPath,

    [Parameter(Mandatory = $false)]
    [string]$Repository = $env:GITHUB_REPOSITORY,

    [Parameter(Mandatory = $false)]
    [string]$AssetBranch = "review-tests-assets",

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 50)]
    [int]$MaxComparisons = 24,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1024, 52428800)]
    [long]$MaxFileBytes = 10485760,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1024, 524288000)]
    [long]$MaxTotalBytes = 104857600
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Repository)) {
    $Repository = "dotnet/maui"
}
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
    throw "Repository must be an owner/name pair."
}
if ($AssetBranch -notmatch '^[A-Za-z0-9][A-Za-z0-9._/-]{0,100}$' -or
    $AssetBranch.Contains("..") -or
    $AssetBranch.Contains("//") -or
    $AssetBranch.Contains("@{") -or
    $AssetBranch.EndsWith("/") -or
    $AssetBranch.EndsWith(".")) {
    throw "Asset branch name is not safe."
}
if ($AssetBranch -in @('main', 'master', 'HEAD')) {
    # Defense-in-depth: this workflow runs with `contents: write`, so never allow a
    # misconfiguration (or future reuse) to publish generated assets onto a
    # protected/default branch.
    throw "Asset branch must not be a protected or default branch name."
}
if (-not (Test-Path -LiteralPath $ContextJsonPath)) {
    throw "Context JSON was not found: $ContextJsonPath"
}

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) {
    $RepoRoot = (Get-Location).Path
}
$RunDirectory = Split-Path -Parent $ContextJsonPath
$DownloadRoot = if (-not [string]::IsNullOrWhiteSpace($env:RUNNER_TEMP)) {
    Join-Path $env:RUNNER_TEMP "review-test-visual-assets"
}
else {
    Join-Path ([System.IO.Path]::GetTempPath()) "review-test-visual-assets"
}
$DownloadDirectory = Join-Path $DownloadRoot "$PrNumber-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Force -Path $DownloadDirectory | Out-Null

function Get-SafeAssetSlug {
    param([string]$Value)

    $slug = ([string]$Value).ToLowerInvariant()
    $slug = [regex]::Replace($slug, '[^a-z0-9._-]+', '-')
    $slug = $slug.Trim('-', '.', '_')
    if ([string]::IsNullOrWhiteSpace($slug)) {
        $slug = "snapshot"
    }
    if ($slug.Length -gt 72) {
        $slug = $slug.Substring(0, 72).TrimEnd('-', '.', '_')
    }
    return $slug
}

function ConvertTo-UrlPath {
    param([string]$Path)

    return (($Path -replace '\\', '/').Split('/') | ForEach-Object {
        [Uri]::EscapeDataString($_)
    }) -join '/'
}

function Test-PngFile {
    param(
        [string]$Path,
        [long]$MaximumBytes
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $false
    }
    $file = Get-Item -LiteralPath $Path
    if ($file.Length -lt 33 -or $file.Length -gt $MaximumBytes) {
        return $false
    }

    $header = New-Object byte[] 24
    $stream = [System.IO.File]::OpenRead($Path)
    try {
        if ($stream.Read($header, 0, $header.Length) -ne $header.Length) {
            return $false
        }
        $expected = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
        for ($i = 0; $i -lt $expected.Length; $i++) {
            if ($header[$i] -ne $expected[$i]) {
                return $false
            }
        }

        $readUInt32BigEndian = {
            param([int]$Offset)

            return [uint32](
                ([uint64]$header[$Offset] * 16777216) +
                ([uint64]$header[$Offset + 1] * 65536) +
                ([uint64]$header[$Offset + 2] * 256) +
                [uint64]$header[$Offset + 3])
        }
        $ihdrLength = & $readUInt32BigEndian 8
        $ihdrType = [System.Text.Encoding]::ASCII.GetString($header, 12, 4)
        $width = & $readUInt32BigEndian 16
        $height = & $readUInt32BigEndian 20
        if ($ihdrLength -ne 13 -or $ihdrType -ne "IHDR" -or
            $width -eq 0 -or $height -eq 0 -or
            $width -gt 16384 -or $height -gt 16384 -or
            ([uint64]$width * [uint64]$height) -gt 50000000) {
            return $false
        }
        return $true
    }
    finally {
        $stream.Dispose()
    }
}

function Test-AzDoAttachmentUrl {
    param(
        [string]$Url,
        [int]$RunId,
        [int]$ResultId,
        [int]$AttachmentId
    )

    try {
        $uri = [Uri]$Url
    }
    catch {
        return $false
    }

    if ($uri.Scheme -ne "https" -or $uri.Host -ine "dev.azure.com") {
        return $false
    }
    $expectedPath = "/dnceng-public/public/_apis/test/Runs/$RunId/Results/$ResultId/Attachments/$AttachmentId"
    return $uri.AbsolutePath -ieq $expectedPath
}

function Invoke-DownloadFile {
    param(
        [string]$Url,
        [string]$Path,
        [long]$MaximumBytes,
        [int]$MaxAttempts = 3,
        [datetime]$Deadline = [datetime]::MaxValue
    )

    $perRequestTimeout = [TimeSpan]::FromMinutes(2)

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        # Bound each attempt by the smaller of the per-request ceiling and the time left on the
        # aggregate publish budget. HttpClient.Timeout stops applying once ResponseHeadersRead
        # returns, so a host that sends headers then stalls the body would otherwise hold the job
        # open indefinitely. A CancellationTokenSource covers the whole operation (GetAsync *and*
        # the body reads), giving a hard wall-clock cap and honoring the shared deadline.
        $remainingBudget = $Deadline - (Get-Date)
        if ($remainingBudget -le [TimeSpan]::Zero) {
            throw "Publish budget exhausted before downloading '$Url'."
        }
        $attemptTimeout = if ($remainingBudget -lt $perRequestTimeout) { $remainingBudget } else { $perRequestTimeout }

        $completed = $false
        $caught = $null
        $statusCode = 0
        $handler = $null
        $client = $null
        $response = $null
        $inputStream = $null
        $outputStream = $null
        $cts = $null
        try {
            $cts = [System.Threading.CancellationTokenSource]::new()
            $cts.CancelAfter($attemptTimeout)
            $token = $cts.Token

            $handler = [System.Net.Http.HttpClientHandler]::new()
            $handler.AllowAutoRedirect = $true
            $handler.MaxAutomaticRedirections = 5
            $client = [System.Net.Http.HttpClient]::new($handler)
            # The cancellation token, not HttpClient.Timeout, enforces the wall-clock bound so it
            # also covers the post-headers body read.
            $client.Timeout = [System.Threading.Timeout]::InfiniteTimeSpan
            $response = $client.GetAsync(
                $Url,
                [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead,
                $token
            ).GetAwaiter().GetResult()
            [void]$response.EnsureSuccessStatusCode()
            if ($response.Content.Headers.ContentLength -and
                $response.Content.Headers.ContentLength.Value -gt $MaximumBytes) {
                throw "Download exceeded the $MaximumBytes-byte size limit."
            }

            $inputStream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
            $outputStream = [System.IO.File]::Create($Path)
            $buffer = New-Object byte[] 81920
            $total = 0L
            while (($read = $inputStream.ReadAsync($buffer, 0, $buffer.Length, $token).GetAwaiter().GetResult()) -gt 0) {
                $total += $read
                if ($total -gt $MaximumBytes) {
                    throw "Download exceeded the $MaximumBytes-byte size limit."
                }
                $outputStream.Write($buffer, 0, $read)
            }
            $completed = $true
        }
        catch {
            $caught = $_
            if ($response) {
                $statusCode = [int]$response.StatusCode
            }
        }
        finally {
            if ($outputStream) { $outputStream.Dispose() }
            if ($inputStream) { $inputStream.Dispose() }
            if ($response) { $response.Dispose() }
            if ($client) { $client.Dispose() }
            if ($handler) { $handler.Dispose() }
            if ($cts) { $cts.Dispose() }
            if (-not $completed) {
                Remove-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
            }
        }

        if ($completed) {
            return
        }
        $nonRetryableClientError = $statusCode -ge 400 -and
            $statusCode -lt 500 -and
            $statusCode -notin @(408, 429)
        if ($attempt -ge $MaxAttempts -or $nonRetryableClientError) {
            throw $caught
        }
        Start-Sleep -Seconds $attempt
    }
}

function Get-SnapshotRoot {
    param([string]$Platform)

    switch ($Platform) {
        "android" { return "src/Controls/tests/TestCases.Android.Tests/snapshots" }
        "ios" { return "src/Controls/tests/TestCases.iOS.Tests/snapshots" }
        "macos" { return "src/Controls/tests/TestCases.Mac.Tests/snapshots" }
        "windows" { return "src/Controls/tests/TestCases.WinUI.Tests/snapshots" }
        default { return $null }
    }
}

function Get-ValidatedSnapshotPathHint {
    param(
        [string]$Platform,
        [string]$SnapshotFileName,
        [string]$PathHint
    )

    if ([string]::IsNullOrWhiteSpace($PathHint)) {
        return $null
    }
    $snapshotRoot = Get-SnapshotRoot -Platform $Platform
    $normalized = $PathHint -replace '\\', '/'
    if (-not $snapshotRoot -or
        $normalized.Contains("..") -or
        -not $normalized.StartsWith("$snapshotRoot/", [StringComparison]::Ordinal) -or
        [System.IO.Path]::GetFileName($normalized) -ne $SnapshotFileName) {
        return $null
    }
    return $normalized
}

function Get-SnapshotCandidatePaths {
    param(
        [string]$Platform,
        [string]$SnapshotFileName,
        [string]$EnvironmentName,
        [string]$BaselinePathHint,
        [string]$RepositoryRoot
    )

    if ($SnapshotFileName -notmatch '^[A-Za-z0-9][A-Za-z0-9._ -]*\.png$' -or
        $SnapshotFileName.Contains("..")) {
        return @()
    }

    $snapshotRoot = Get-SnapshotRoot -Platform $Platform
    if (-not $snapshotRoot) {
        return @()
    }

    $paths = New-Object System.Collections.Generic.List[string]
    $seen = @{}
    $addPath = {
        param([string]$Candidate)
        $normalized = $Candidate -replace '\\', '/'
        if ($normalized.Contains("..") -or
            -not $normalized.StartsWith("$snapshotRoot/", [StringComparison]::Ordinal) -or
            [System.IO.Path]::GetFileName($normalized) -ne $SnapshotFileName -or
            $seen.ContainsKey($normalized)) {
            return
        }
        $seen[$normalized] = $true
        $paths.Add($normalized)
    }

    $validatedPathHint = Get-ValidatedSnapshotPathHint `
        -Platform $Platform `
        -SnapshotFileName $SnapshotFileName `
        -PathHint $BaselinePathHint
    if ($validatedPathHint) {
        & $addPath $validatedPathHint
    }
    if (-not [string]::IsNullOrWhiteSpace($EnvironmentName) -and
        $EnvironmentName -match '^[a-z0-9][a-z0-9._-]*$') {
        & $addPath "$snapshotRoot/$EnvironmentName/$SnapshotFileName"
    }

    $localSnapshotRoot = Join-Path $RepositoryRoot ($snapshotRoot -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    if (Test-Path -LiteralPath $localSnapshotRoot -PathType Container) {
        foreach ($directory in @(Get-ChildItem -LiteralPath $localSnapshotRoot -Directory | Sort-Object Name)) {
            & $addPath "$snapshotRoot/$($directory.Name)/$SnapshotFileName"
        }
    }

    return $paths.ToArray()
}

function Select-BaselineCandidate {
    param(
        [object[]]$CandidateFiles,
        [string]$PreferredPath
    )

    $files = @($CandidateFiles | Where-Object { $null -ne $_ })
    if (-not [string]::IsNullOrWhiteSpace($PreferredPath)) {
        $preferred = @($files | Where-Object {
                [string]::Equals(
                    [string]$_.repositoryPath,
                    $PreferredPath,
                    [StringComparison]::Ordinal)
            })
        if ($preferred.Count -eq 1) {
            return [pscustomobject]@{
                localPath = [string]$preferred[0].localPath
                repositoryPath = [string]$preferred[0].repositoryPath
                status = "resolved from the tested runtime environment"
            }
        }
        return [pscustomobject]@{
            localPath = $null
            repositoryPath = $null
            status = "preferred runtime environment snapshot was unavailable at the tested merge commit"
        }
    }

    if ($files.Count -eq 1) {
        return [pscustomobject]@{
            localPath = [string]$files[0].localPath
            repositoryPath = [string]$files[0].repositoryPath
            status = "only matching snapshot at the tested merge commit"
        }
    }
    return [pscustomobject]@{
        localPath = $null
        repositoryPath = $null
        status = $(if ($files.Count -gt 1) {
                "ambiguous across multiple snapshot environments"
            }
            else {
                "not found at the tested merge commit"
            })
    }
}

function Invoke-GhApiJson {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body,
        [datetime]$Deadline = [datetime]::MaxValue
    )

    if ($Deadline -ne [datetime]::MaxValue -and (Get-Date) -ge $Deadline) {
        throw "Publish budget exhausted before gh api $Method $Endpoint."
    }

    $arguments = @("api", "--method", $Method, $Endpoint)
    $payloadPath = $null
    $process = $null
    try {
        if ($null -ne $Body) {
            $payloadPath = [System.IO.Path]::GetTempFileName()
            $Body | ConvertTo-Json -Depth 20 -Compress | Set-Content -LiteralPath $payloadPath -Encoding UTF8
            $arguments += @("--input", $payloadPath)
        }

        $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = "gh"
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in $arguments) {
            [void]$startInfo.ArgumentList.Add([string]$argument)
        }

        $process = [System.Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        if (-not $process.Start()) {
            throw "gh api $Method $Endpoint could not be started."
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()

        if ($Deadline -eq [datetime]::MaxValue) {
            $process.WaitForExit()
        }
        else {
            $remainingMilliseconds = [Math]::Floor(($Deadline - (Get-Date)).TotalMilliseconds)
            if ($remainingMilliseconds -le 0 -or
                -not $process.WaitForExit([int][Math]::Min([int]::MaxValue, $remainingMilliseconds))) {
                try {
                    $process.Kill($true)
                    $process.WaitForExit()
                }
                catch {
                    # The process may have exited between the timeout and termination request.
                }
                throw "Publish budget exhausted while calling gh api $Method $Endpoint."
            }
        }

        $output = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            throw "gh api $Method $Endpoint failed: $stderr $output"
        }
        $text = ([string]$output).Trim()
        if ([string]::IsNullOrWhiteSpace($text)) {
            return $null
        }
        return $text | ConvertFrom-Json
    }
    finally {
        if ($payloadPath) {
            Remove-Item -LiteralPath $payloadPath -Force -ErrorAction SilentlyContinue
        }
        if ($null -ne $process) {
            $process.Dispose()
        }
    }
}

function Get-AssetBranchRef {
    param(
        [string]$Repository,
        [string]$Branch,
        [datetime]$Deadline = [datetime]::MaxValue
    )

    try {
        return Invoke-GhApiJson -Method "GET" -Endpoint "repos/$Repository/git/ref/heads/$Branch" -Deadline $Deadline
    }
    catch {
        if ($_.Exception.Message -match 'HTTP 404|Reference does not exist') {
            return $null
        }
        throw
    }
}

function Initialize-AssetBranch {
    param(
        [string]$Repository,
        [string]$Branch,
        [datetime]$Deadline = [datetime]::MaxValue
    )

    $existing = Get-AssetBranchRef -Repository $Repository -Branch $Branch -Deadline $Deadline
    if ($existing) {
        return $existing
    }

    $repositoryInfo = Invoke-GhApiJson -Method "GET" -Endpoint "repos/$Repository" -Deadline $Deadline
    $defaultBranch = [string]$repositoryInfo.default_branch
    if ([string]::IsNullOrWhiteSpace($defaultBranch)) {
        throw "Repository '$Repository' did not expose a default branch."
    }
    $defaultRef = Invoke-GhApiJson -Method "GET" -Endpoint "repos/$Repository/git/ref/heads/$defaultBranch" -Deadline $Deadline
    try {
        Invoke-GhApiJson -Method "POST" -Endpoint "repos/$Repository/git/refs" -Body @{
            ref = "refs/heads/$Branch"
            sha = $defaultRef.object.sha
        } -Deadline $Deadline | Out-Null
    }
    catch {
        if ($_.Exception.Message -notmatch 'HTTP 422|Reference already exists') {
            throw
        }
    }

    $created = Get-AssetBranchRef -Repository $Repository -Branch $Branch -Deadline $Deadline
    if (-not $created) {
        throw "Asset branch '$Branch' could not be initialized."
    }
    return $created
}

function Publish-GitAssets {
    param(
        [string]$Repository,
        [string]$Branch,
        [object[]]$Assets,
        [string]$CommitMessage,
        [datetime]$Deadline = [datetime]::MaxValue,
        [int]$MaxAttempts = 5
    )

    Initialize-AssetBranch -Repository $Repository -Branch $Branch -Deadline $Deadline | Out-Null

    $blobByHash = @{}
    $entries = New-Object System.Collections.Generic.List[object]
    foreach ($asset in $Assets) {
        $bytes = [System.IO.File]::ReadAllBytes([string]$asset.localPath)
        $hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
        if (-not $blobByHash.ContainsKey($hash)) {
            $blob = Invoke-GhApiJson -Method "POST" -Endpoint "repos/$Repository/git/blobs" -Body @{
                content = [Convert]::ToBase64String($bytes)
                encoding = "base64"
            } -Deadline $Deadline
            $blobByHash[$hash] = $blob.sha
        }
        $entries.Add([ordered]@{
            path = [string]$asset.assetPath
            mode = "100644"
            type = "blob"
            sha = $blobByHash[$hash]
        })
    }

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $branchRef = Get-AssetBranchRef -Repository $Repository -Branch $Branch -Deadline $Deadline
        if (-not $branchRef) {
            Initialize-AssetBranch -Repository $Repository -Branch $Branch -Deadline $Deadline | Out-Null
            $branchRef = Get-AssetBranchRef -Repository $Repository -Branch $Branch -Deadline $Deadline
        }
        $parentSha = [string]$branchRef.object.sha
        $parentCommit = Invoke-GhApiJson -Method "GET" -Endpoint "repos/$Repository/git/commits/$parentSha" -Deadline $Deadline
        $tree = Invoke-GhApiJson -Method "POST" -Endpoint "repos/$Repository/git/trees" -Body @{
            base_tree = $parentCommit.tree.sha
            tree = $entries.ToArray()
        } -Deadline $Deadline
        $commit = Invoke-GhApiJson -Method "POST" -Endpoint "repos/$Repository/git/commits" -Body @{
            message = $CommitMessage
            tree = $tree.sha
            parents = @($parentSha)
        } -Deadline $Deadline

        try {
            Invoke-GhApiJson -Method "PATCH" -Endpoint "repos/$Repository/git/refs/heads/$Branch" -Body @{
                sha = $commit.sha
                force = $false
            } -Deadline $Deadline | Out-Null
            return [string]$commit.sha
        }
        catch {
            if ($attempt -ge $MaxAttempts -or $_.Exception.Message -notmatch 'HTTP 409|HTTP 422|not a fast forward') {
                throw
            }
            if ($Deadline -ne [datetime]::MaxValue -and (Get-Date).AddSeconds($attempt) -ge $Deadline) {
                throw "Publish budget exhausted before retrying the asset branch update."
            }
            Start-Sleep -Seconds $attempt
        }
    }

    throw "Asset branch update exhausted $MaxAttempts attempts."
}

function Get-VisualEvidenceDedupKey {
    param([object]$Evidence)

    # Key on stable *logical* identity only: platform + snapshot + resolved
    # runtime environment (leg). Build/run/result identifiers are intentionally
    # excluded so that retry attempts of the same logical snapshot failure
    # collapse to a single panel instead of consuming the bounded comparison
    # budget with duplicate retry panels. The caller sorts newest-first, so the
    # retained entry is the latest attempt; genuinely different environments or
    # platforms still produce distinct keys.
    # When the environment is unresolved (the gatherer sets environmentName to null whenever a
    # build exposes multiple environment hints for one platform), platform + snapshot alone cannot
    # tell two distinct legs apart: distinct iOS legs failing the same snapshot would both key on
    # "ios|name.png|" and one would be collapsed as if it were a retry of the other. Prefer the AzDO
    # test-run *name* -- the pipeline job/leg display name carried through gathering -- as the leg
    # identity in that case: it is stable across retries of the SAME leg (a retry re-runs the same
    # job and reuses its run name) yet differs between DISTINCT legs, so same-leg retries collapse to
    # one panel (freeing MaxComparisons slots for genuinely distinct failures) while distinct legs
    # stay separate. Only when no run name is available do we fall back to the per-result identifier,
    # which never collapses distinct legs (fails safe); its cost is that a same-leg retry lacking a
    # run name is not collapsed, which the downstream omittedCount caveat still surfaces.
    $environmentName = [string]$Evidence.environmentName
    $runName = [string]$Evidence.runName
    $legDiscriminator = if (-not [string]::IsNullOrWhiteSpace($environmentName)) {
        $environmentName
    }
    elseif (-not [string]::IsNullOrWhiteSpace($runName)) {
        "run:$runName"
    }
    else {
        "leg:$([string]$Evidence.runId):$([string]$Evidence.resultId)"
    }
    $parts = @(
        [string]$Evidence.platform,
        [string]$Evidence.snapshotFileName,
        $legDiscriminator
    )
    return ($parts -join '|').ToLowerInvariant()
}

function Save-Context {
    param(
        [object]$Context,
        [string]$Path
    )

    $Context | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Remove-VisualDownloadDirectory {
    param([string]$Path)

    if (-not [string]::IsNullOrWhiteSpace($Path) -and (Test-Path -LiteralPath $Path)) {
        Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

try {
$context = Get-Content -LiteralPath $ContextJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$context.repository -ne $Repository) {
    throw "Context repository '$($context.repository)' did not match trusted repository '$Repository'."
}
if ([int]$context.pr.number -ne $PrNumber) {
    throw "Context PR '$($context.pr.number)' did not match trusted PR '$PrNumber'."
}

$visualEvidence = @($context.visualEvidence.comparisons | Where-Object { $null -ne $_ })
if ($visualEvidence.Count -eq 0) {
    Write-Host "No visual snapshot failures were detected."
    $context | Add-Member -NotePropertyName visualAssets -NotePropertyValue ([ordered]@{
        published = $false
        comparisonCount = 0
        errors = @()
    }) -Force
    Save-Context -Context $context -Path $ContextJsonPath
    exit 0
}

$deduped = [ordered]@{}
$dedupeDroppedCount = 0
foreach ($evidence in @($visualEvidence | Sort-Object `
        @{ Expression = { if ($_.completedDate) { [datetime]$_.completedDate } else { [datetime]::MinValue } }; Descending = $true }, `
        @{ Expression = { [int]$_.buildId }; Descending = $true }, `
        @{ Expression = { [int]$_.resultId }; Descending = $true })) {
    $key = Get-VisualEvidenceDedupKey -Evidence $evidence
    if (-not $deduped.Contains($key)) {
        $deduped[$key] = $evidence
    }
    else {
        $dedupeDroppedCount++
    }
}

$allUnique = @($deduped.Values)
$selectedEvidence = @($allUnique | Select-Object -First $MaxComparisons)
$omittedCount = [Math]::Max(0, $allUnique.Count - $selectedEvidence.Count) + $dedupeDroppedCount
$assets = New-Object System.Collections.Generic.List[object]
$prepared = New-Object System.Collections.Generic.List[object]
$errors = New-Object System.Collections.Generic.List[string]
$preparationFailureCount = 0
$totalBytes = 0L
$index = 0

# Aggregate wall-clock budget shared by preparation and Git publication. Reserve four minutes below
# the workflow's 16-minute hard stop so a budget failure can still update context.json and let the
# ordinary-report fallback run. Every download and gh API call observes the same deadline.
$publishBudgetSeconds = 720
$parsedPublishBudget = 0
if (-not [string]::IsNullOrWhiteSpace($env:REVIEW_TESTS_PUBLISH_BUDGET_SECONDS) -and
    [int]::TryParse($env:REVIEW_TESTS_PUBLISH_BUDGET_SECONDS, [ref]$parsedPublishBudget) -and
    $parsedPublishBudget -gt 0) {
    $publishBudgetSeconds = $parsedPublishBudget
}
$publishDeadline = (Get-Date).AddSeconds($publishBudgetSeconds)

foreach ($evidence in $selectedEvidence) {
    $index++
    if ((Get-Date) -ge $publishDeadline) {
        # Budget exhausted: items $index..Count (inclusive of the current one) are untouched. Record
        # them as omitted and stop before starting any more downloads.
        $remaining = $selectedEvidence.Count - $index + 1
        if ($remaining -gt 0) {
            $omittedCount += $remaining
            $errors.Add("Publish budget of ${publishBudgetSeconds}s exhausted before preparing $remaining remaining comparison(s); they were omitted.")
        }
        break
    }
    $runId = [int]$evidence.runId
    $resultId = [int]$evidence.resultId
    $buildId = [int]$evidence.buildId
    $snapshotFileName = [string]$evidence.snapshotFileName
    $slug = Get-SafeAssetSlug -Value ([System.IO.Path]::GetFileNameWithoutExtension($snapshotFileName))
    $revision = [string]$evidence.buildSourceVersion
    $revisionPath = if ($revision -match '^[0-9a-fA-F]{12,40}$') { $revision.Substring(0, 12).ToLowerInvariant() } else { "unknown" }
    $assetPrefix = "pr-$PrNumber/$revisionPath/build-$buildId/$('{0:d2}' -f $index)-$slug"

    try {
        $actual = $evidence.actual
        if (-not $actual -or
            -not (Test-AzDoAttachmentUrl -Url ([string]$actual.url) -RunId $runId -ResultId $resultId -AttachmentId ([int]$actual.id))) {
            throw "Actual attachment URL did not match the expected AzDO result."
        }
        if ($actual.size -and [long]$actual.size -gt $MaxFileBytes) {
            throw "Actual attachment exceeds the per-file size limit."
        }

        $actualPath = Join-Path $DownloadDirectory "$assetPrefix-actual.png"
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $actualPath) | Out-Null
        Invoke-DownloadFile -Url ([string]$actual.url) -Path $actualPath -MaximumBytes $MaxFileBytes -Deadline $publishDeadline
        if (-not (Test-PngFile -Path $actualPath -MaximumBytes $MaxFileBytes)) {
            throw "Actual attachment was not a bounded PNG."
        }

        $itemAssets = New-Object System.Collections.Generic.List[object]
        $actualSize = (Get-Item -LiteralPath $actualPath).Length
        $itemAssets.Add([ordered]@{ kind = "actual"; localPath = $actualPath; assetPath = "$assetPrefix-actual.png"; size = $actualSize })

        $diffPath = $null
        if ($evidence.diff) {
            $diff = $evidence.diff
            if (Test-AzDoAttachmentUrl -Url ([string]$diff.url) -RunId $runId -ResultId $resultId -AttachmentId ([int]$diff.id)) {
                if (-not $diff.size -or [long]$diff.size -le $MaxFileBytes) {
                    $diffCandidatePath = Join-Path $DownloadDirectory "$assetPrefix-diff.png"
                    try {
                        Invoke-DownloadFile -Url ([string]$diff.url) -Path $diffCandidatePath -MaximumBytes $MaxFileBytes -Deadline $publishDeadline
                        if (Test-PngFile -Path $diffCandidatePath -MaximumBytes $MaxFileBytes) {
                            $diffPath = $diffCandidatePath
                            $itemAssets.Add([ordered]@{ kind = "diff"; localPath = $diffPath; assetPath = "$assetPrefix-diff.png"; size = (Get-Item -LiteralPath $diffPath).Length })
                        }
                        else {
                            Remove-Item -LiteralPath $diffCandidatePath -Force -ErrorAction SilentlyContinue
                        }
                    }
                    catch {
                        # The diff is optional and the renderer supports a null diff, so a
                        # diff download/validation failure must not bubble to the outer
                        # per-comparison catch and discard the already-validated actual
                        # (and any baseline). Isolate it like the baseline candidate
                        # downloads below and keep publishing the remaining panel.
                        Remove-Item -LiteralPath $diffCandidatePath -Force -ErrorAction SilentlyContinue
                        $diffPath = $null
                    }
                }
            }
        }

        $baselinePath = $null
        $baselineRepositoryPath = $null
        $baselineStatus = "not found at the tested merge commit"
        if ($evidence.kind -ne "missing-baseline" -and $revision -match '^[0-9a-fA-F]{40}$') {
            $candidatePaths = @(Get-SnapshotCandidatePaths `
                -Platform ([string]$evidence.platform) `
                -SnapshotFileName $snapshotFileName `
                -EnvironmentName ([string]$evidence.environmentName) `
                -BaselinePathHint ([string]$evidence.baselinePathHint) `
                -RepositoryRoot $RepoRoot)
            $candidateFiles = New-Object System.Collections.Generic.List[object]
            foreach ($candidatePath in $candidatePaths) {
                $candidateUrl = "https://raw.githubusercontent.com/$Repository/$revision/$(ConvertTo-UrlPath $candidatePath)"
                $candidateLocalPath = Join-Path $DownloadDirectory "$assetPrefix-baseline-$($candidateFiles.Count).png"
                try {
                    Invoke-DownloadFile -Url $candidateUrl -Path $candidateLocalPath -MaximumBytes $MaxFileBytes -Deadline $publishDeadline
                    if (Test-PngFile -Path $candidateLocalPath -MaximumBytes $MaxFileBytes) {
                        $candidateFiles.Add([ordered]@{
                            repositoryPath = $candidatePath
                            localPath = $candidateLocalPath
                        })
                    }
                    else {
                        # The download succeeded but the payload is not a usable PNG (e.g. an HTML
                        # 404 body or an oversized image). Mirror the diff-download and catch-path
                        # cleanup so a rejected candidate never lingers in the temp download dir and
                        # is never mistaken for a validated baseline.
                        Remove-Item -LiteralPath $candidateLocalPath -Force -ErrorAction SilentlyContinue
                    }
                }
                catch {
                    Remove-Item -LiteralPath $candidateLocalPath -Force -ErrorAction SilentlyContinue
                }
            }

            $preferredPath = $null
            if ($evidence.baselinePathHint) {
                $preferredPath = [string]$evidence.baselinePathHint
            }
            elseif ($evidence.environmentName) {
                $root = Get-SnapshotRoot -Platform ([string]$evidence.platform)
                if ($root) {
                    $preferredPath = "$root/$($evidence.environmentName)/$snapshotFileName"
                }
            }

            $baselineSelection = Select-BaselineCandidate `
                -CandidateFiles $candidateFiles.ToArray() `
                -PreferredPath $preferredPath
            $baselinePath = [string]$baselineSelection.localPath
            $baselineRepositoryPath = [string]$baselineSelection.repositoryPath
            $baselineStatus = [string]$baselineSelection.status
        }
        elseif ($evidence.kind -eq "missing-baseline") {
            $baselineRepositoryPath = Get-ValidatedSnapshotPathHint `
                -Platform ([string]$evidence.platform) `
                -SnapshotFileName $snapshotFileName `
                -PathHint ([string]$evidence.baselinePathHint)
            $baselineStatus = "baseline was not present in CI"
        }

        if ($baselinePath) {
            $itemAssets.Add([ordered]@{ kind = "baseline"; localPath = $baselinePath; assetPath = "$assetPrefix-baseline.png"; size = (Get-Item -LiteralPath $baselinePath).Length })
        }

        $itemBytes = (@($itemAssets | ForEach-Object { [long]$_.size }) | Measure-Object -Sum).Sum
        if (($totalBytes + $itemBytes) -gt $MaxTotalBytes) {
            # Items $index..Count (inclusive of the current one, which is being rejected for the byte
            # cap) are untouched. Use the index-based remainder -- matching the publish-budget break
            # above -- so comparisons that already failed preparation (counted in
            # $preparationFailureCount) are not also double-counted here as omitted.
            $omittedCount += [Math]::Max(0, $selectedEvidence.Count - $index + 1)
            break
        }
        $totalBytes += $itemBytes
        foreach ($asset in $itemAssets) {
            $assets.Add($asset)
        }

        $prepared.Add([ordered]@{
            testName = $(if ($evidence.testName) { [string]$evidence.testName } else { [System.IO.Path]::GetFileNameWithoutExtension($snapshotFileName) })
            automatedTestName = [string]$evidence.automatedTestName
            platform = [string]$evidence.platform
            snapshotFileName = $snapshotFileName
            description = $(if ($evidence.description) { [string]$evidence.description } else { [string]$evidence.kind })
            buildId = $buildId
            buildUrl = [string]$evidence.buildUrl
            baselineRepositoryPath = $baselineRepositoryPath
            baselineStatus = $baselineStatus
            baselineAssetPath = $(if ($baselinePath) { "$assetPrefix-baseline.png" } else { $null })
            actualAssetPath = "$assetPrefix-actual.png"
            diffAssetPath = $(if ($diffPath) { "$assetPrefix-diff.png" } else { $null })
        })
    }
    catch {
        $preparationFailureCount++
        $errors.Add("$snapshotFileName (build $buildId, run $runId, result $resultId): $($_.Exception.Message)")
    }
}

if ($assets.Count -eq 0 -or $prepared.Count -eq 0) {
    $message = "Visual comparisons were detected, but no bounded image set could be prepared for publishing."
    if ($errors.Count -gt 0) {
        $message += " " + ($errors.ToArray() -join " ")
    }
    $context | Add-Member -NotePropertyName visualAssets -NotePropertyValue ([ordered]@{
        published = $false
        omittedCount = $omittedCount
        preparationFailureCount = $preparationFailureCount
        errors = $errors.ToArray()
    }) -Force
    Save-Context -Context $context -Path $ContextJsonPath
    Write-Warning $message
    exit 0
}

try {
    $headSha = [string]$context.pr.headRefOid
    $headLabel = if ($headSha.Length -ge 7) { $headSha.Substring(0, 7) } else { "unknown" }
    $assetCommit = Publish-GitAssets `
        -Repository $Repository `
        -Branch $AssetBranch `
        -Assets $assets.ToArray() `
        -CommitMessage "[skip ci] Store /review tests visuals for PR #$PrNumber at $headLabel" `
        -Deadline $publishDeadline

    $published = New-Object System.Collections.Generic.List[object]
    foreach ($comparison in $prepared) {
        $rawPrefix = "https://raw.githubusercontent.com/$Repository/$assetCommit/"
        $published.Add([ordered]@{
            testName = $comparison.testName
            automatedTestName = $comparison.automatedTestName
            platform = $comparison.platform
            snapshotFileName = $comparison.snapshotFileName
            description = $comparison.description
            buildId = $comparison.buildId
            buildUrl = $comparison.buildUrl
            baselineRepositoryPath = $comparison.baselineRepositoryPath
            baselineStatus = $comparison.baselineStatus
            baselineUrl = $(if ($comparison.baselineAssetPath) { "$rawPrefix$($comparison.baselineAssetPath)" } else { $null })
            actualUrl = "$rawPrefix$($comparison.actualAssetPath)"
            diffUrl = $(if ($comparison.diffAssetPath) { "$rawPrefix$($comparison.diffAssetPath)" } else { $null })
        })
    }

    $context | Add-Member -NotePropertyName visualAssets -NotePropertyValue ([ordered]@{
        published = $true
        branch = $AssetBranch
        commit = $assetCommit
        comparisonCount = $published.Count
        omittedCount = $omittedCount
        preparationFailureCount = $preparationFailureCount
        comparisons = $published.ToArray()
        errors = $errors.ToArray()
    }) -Force
    Save-Context -Context $context -Path $ContextJsonPath
    Write-Host "Published $($published.Count) visual comparison(s) at asset commit $assetCommit."
}
catch {
    # A Git/API failure occurred AFTER images were prepared. Preserve the omission and preparation
    # counters already computed above -- discarding them (the prior behavior) rendered real omitted or
    # failed comparisons as zero -- and append this failure to the existing error list. Set an explicit
    # publicationFailed flag so the merger classifies this as a post-preparation publication failure
    # instead of inferring it from a non-empty error list, which also holds pre-publication budget and
    # omission messages emitted before any image was prepared.
    $message = "Visual asset publishing failed without changing the deterministic test verdict: $($_.Exception.Message)"
    $errors.Add($message)
    $context | Add-Member -NotePropertyName visualAssets -NotePropertyValue ([ordered]@{
        published = $false
        publicationFailed = $true
        omittedCount = $omittedCount
        preparationFailureCount = $preparationFailureCount
        errors = $errors.ToArray()
    }) -Force
    Save-Context -Context $context -Path $ContextJsonPath
    Write-Warning $message
}
}
finally {
    Remove-VisualDownloadDirectory -Path $DownloadDirectory
}
