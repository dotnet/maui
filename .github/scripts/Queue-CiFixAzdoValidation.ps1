#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$EventPath = $env:GITHUB_EVENT_PATH,
    [string]$Repository = $env:GITHUB_REPOSITORY,
    [string]$EventName = $env:GITHUB_EVENT_NAME,
    [string]$PullRequestsFixturePath,
    [ValidateRange(1, 540)][int]$DispatcherBudgetSeconds = 420,
    [long]$DispatcherDeadlineUnixSeconds = 0,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:AzureDevOpsOrganization = 'dnceng-public'
$script:AzureDevOpsProject = 'public'
$script:TransientHttpStatusCodes = @(408, 429, 500, 502, 503, 504)
$script:MaxHttpAttempts = 4
$script:RetryBaseDelaySeconds = 2
$secondsUntilDeadline = if ($DispatcherDeadlineUnixSeconds -gt 0) {
    $DispatcherDeadlineUnixSeconds - [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
}
else {
    $DispatcherBudgetSeconds
}
$script:EffectiveDispatcherBudgetSeconds = [Math]::Max(
    0,
    [Math]::Min($DispatcherBudgetSeconds, $secondsUntilDeadline))
$script:DispatcherStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$script:DispatcherBudgetPrefix = '[dispatcher-budget-exhausted]'

function Get-DispatcherElapsedSeconds {
    return $script:DispatcherStopwatch.Elapsed.TotalSeconds
}

function Get-DispatcherRemainingSeconds {
    return [Math]::Max(0, $script:EffectiveDispatcherBudgetSeconds - (Get-DispatcherElapsedSeconds))
}

function Test-IsDispatcherBudgetException {
    param([Parameter(Mandatory = $true)][System.Exception]$Exception)

    return $Exception.Message.StartsWith($script:DispatcherBudgetPrefix, [System.StringComparison]::Ordinal)
}

function Get-DispatcherHttpTimeoutSeconds {
    param(
        [Parameter(Mandatory = $true)][string]$OperationName,
        [ValidateRange(1, 300)][int]$MaximumSeconds = 30
    )

    $remainingSeconds = [Math]::Floor((Get-DispatcherRemainingSeconds))
    if ($remainingSeconds -lt 1) {
        throw "$($script:DispatcherBudgetPrefix) No time remains before '$OperationName'."
    }

    return [int][Math]::Min($MaximumSeconds, $remainingSeconds)
}

function Invoke-DispatcherSleep {
    param(
        [Parameter(Mandatory = $true)][string]$OperationName,
        [ValidateRange(1, 300)][int]$RequestedSeconds
    )

    $remainingSeconds = [Math]::Floor((Get-DispatcherRemainingSeconds))
    $sleepSeconds = [Math]::Min($RequestedSeconds, $remainingSeconds - 1)
    if ($sleepSeconds -lt 1) {
        throw "$($script:DispatcherBudgetPrefix) No retry time remains before '$OperationName'."
    }

    Start-Sleep -Seconds $sleepSeconds
}

function Get-ObjectPropertyValue {
    param(
        [AllowNull()][object]$InputObject,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-AzdoQueueFailureMessage {
    param(
        [Parameter(Mandatory = $true)][System.Management.Automation.ErrorRecord]$ErrorRecord,
        [ValidateRange(256, 4096)][int]$MaximumDetailLength = 1024
    )

    $exceptionMessage = $ErrorRecord.Exception.Message
    $responseBody = [string](Get-ObjectPropertyValue -InputObject $ErrorRecord.ErrorDetails -Name 'Message')
    if ([string]::IsNullOrWhiteSpace($responseBody)) {
        return $exceptionMessage
    }

    $detail = $responseBody
    try {
        $errorResponse = $responseBody | ConvertFrom-Json -Depth 10 -ErrorAction Stop
        $azureMessage = [string](Get-ObjectPropertyValue -InputObject $errorResponse -Name 'message')
        $typeKey = [string](Get-ObjectPropertyValue -InputObject $errorResponse -Name 'typeKey')
        if (-not [string]::IsNullOrWhiteSpace($azureMessage)) {
            $detail = if ([string]::IsNullOrWhiteSpace($typeKey)) {
                $azureMessage
            }
            else {
                "$azureMessage (Azure type: $typeKey)"
            }
        }
    }
    catch [System.ArgumentException] {
        # Preserve non-JSON provider diagnostics after applying the same bounds and redaction.
    }

    $detail = $detail -replace '[\r\n\t]+', ' '
    $detail = $detail -replace '(?i)\b(Bearer|Basic)\s+[A-Za-z0-9._~+/=-]+', '$1 [redacted]'
    $detail = $detail -replace '(?i)("(?:client_assertion|access_token|id_token|refresh_token|token)"\s*:\s*)"(?:\\.|[^"\\])*"', '$1"[redacted]"'
    $detail = $detail -replace '(?i)\b(client_assertion|access_token|id_token|refresh_token|token)=([^&\s]+)', '$1=[redacted]'
    $detail = $detail -replace '\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\b', '[redacted-jwt]'
    $detail = $detail.Trim()
    if ($detail.Length -gt $MaximumDetailLength) {
        $truncationSuffix = '... [truncated]'
        $detail = $detail.Substring(0, $MaximumDetailLength - $truncationSuffix.Length) + $truncationSuffix
    }

    if ([string]::IsNullOrWhiteSpace($detail)) {
        return $exceptionMessage
    }

    return "$exceptionMessage Azure response: $detail"
}

function Test-CiFixPrFingerprint {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string]$HeadRepository,
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$BaseRef,
        [Parameter(Mandatory = $true)][string]$HeadRef,
        [Parameter(Mandatory = $true)][string]$AuthorLogin,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Labels
    )

    if ($Repository -cne 'dotnet/maui') { return $false }
    if ($HeadRepository -cne 'dotnet/maui') { return $false }
    if ($AuthorLogin -cne 'github-actions[bot]') { return $false }
    if ($HeadRef -cnotmatch '^ci-fix/[A-Za-z0-9][A-Za-z0-9._/-]*$') { return $false }
    if ($Labels -cnotcontains 'agentic-workflows') { return $false }

    switch ($BaseRef) {
        'main' { return $Title -cmatch '^\[ci-fix\](?:\s|$)' }
        'net11.0' { return $Title -cmatch '^\[ci-fix-net11\](?:\s|$)' }
        default { return $false }
    }
}

function Get-CiFixPipelineDefinitions {
    return @(
        [pscustomobject]@{ Name = 'maui-pr'; DefinitionId = 302 },
        [pscustomobject]@{ Name = 'maui-pr-uitests'; DefinitionId = 313 },
        [pscustomobject]@{ Name = 'maui-pr-devicetests'; DefinitionId = 314 }
    )
}

function Get-CiFixContextFromPullRequest {
    param(
        [Parameter(Mandatory = $true)][object]$PullRequest,
        [Parameter(Mandatory = $true)][string]$Repository,
        [bool]$RequireMergeSha = $true
    )

    $base = Get-ObjectPropertyValue -InputObject $PullRequest -Name 'base'
    $head = Get-ObjectPropertyValue -InputObject $PullRequest -Name 'head'
    $baseRepository = Get-ObjectPropertyValue -InputObject $base -Name 'repo'
    $headRepository = Get-ObjectPropertyValue -InputObject $head -Name 'repo'
    $author = Get-ObjectPropertyValue -InputObject $PullRequest -Name 'user'

    $labels = @(
        @(Get-ObjectPropertyValue -InputObject $PullRequest -Name 'labels') |
            ForEach-Object { [string](Get-ObjectPropertyValue -InputObject $_ -Name 'name') } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    $context = [pscustomobject]@{
        Repository = $Repository
        BaseRepository = [string](Get-ObjectPropertyValue -InputObject $baseRepository -Name 'full_name')
        HeadRepository = [string](Get-ObjectPropertyValue -InputObject $headRepository -Name 'full_name')
        PullRequestNumber = [int](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'number')
        PullRequestId = [long](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'id')
        State = [string](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'state')
        Draft = [bool](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'draft')
        Title = [string](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'title')
        AuthorLogin = [string](Get-ObjectPropertyValue -InputObject $author -Name 'login')
        BaseRef = [string](Get-ObjectPropertyValue -InputObject $base -Name 'ref')
        HeadRef = [string](Get-ObjectPropertyValue -InputObject $head -Name 'ref')
        HeadSha = [string](Get-ObjectPropertyValue -InputObject $head -Name 'sha')
        MergeSha = [string](Get-ObjectPropertyValue -InputObject $PullRequest -Name 'merge_commit_sha')
        Labels = $labels
    }

    if ($context.BaseRepository -cne 'dotnet/maui') {
        throw "Unexpected base repository '$($context.BaseRepository)'."
    }
    if ($context.State -cne 'open') {
        return $null
    }
    if ($context.PullRequestNumber -le 0 -or $context.PullRequestId -le 0) {
        throw 'Pull request number and id must be positive integers.'
    }

    $isEligible = Test-CiFixPrFingerprint `
        -Repository $context.Repository `
        -HeadRepository $context.HeadRepository `
        -Title $context.Title `
        -BaseRef $context.BaseRef `
        -HeadRef $context.HeadRef `
        -AuthorLogin $context.AuthorLogin `
        -Labels $context.Labels

    if (-not $isEligible) {
        return $null
    }

    if ($context.HeadSha -cnotmatch '^[0-9a-fA-F]{40}$') {
        throw 'Eligible CI-fix pull request head SHA is missing or invalid.'
    }
    if ($RequireMergeSha -and $context.MergeSha -cnotmatch '^[0-9a-fA-F]{40}$') {
        Write-Warning "Eligible CI-fix PR #$($context.PullRequestNumber) has no merge commit yet; deferring validation."
        return $null
    }

    return $context
}

function Get-GitHubApiHeaders {
    param([Parameter(Mandatory = $true)][string]$GitHubToken)

    return @{
        Authorization = "Bearer $GitHubToken"
        Accept = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
    }
}

function Get-FixturePullRequestDetail {
    param(
        [Parameter(Mandatory = $true)][object]$FixtureData,
        [Parameter(Mandatory = $true)][int]$PullRequestNumber
    )

    $details = Get-ObjectPropertyValue -InputObject $FixtureData -Name 'pullRequestDetails'
    $detail = Get-ObjectPropertyValue -InputObject $details -Name "$PullRequestNumber"
    if ($null -eq $detail) {
        throw "Pull request fixture has no detail metadata for PR #$PullRequestNumber."
    }

    return $detail
}

function Get-FixtureCommit {
    param(
        [Parameter(Mandatory = $true)][object]$FixtureData,
        [Parameter(Mandatory = $true)][string]$CommitSha
    )

    $commits = Get-ObjectPropertyValue -InputObject $FixtureData -Name 'commits'
    $commit = Get-ObjectPropertyValue -InputObject $commits -Name $CommitSha
    if ($null -eq $commit) {
        throw "Pull request fixture has no commit metadata for '$CommitSha'."
    }

    return $commit
}

function Get-CiFixMergePairDiagnostic {
    param(
        [Parameter(Mandatory = $true)][object]$Context,
        [Parameter(Mandatory = $true)][object]$Commit
    )

    $commitSha = [string](Get-ObjectPropertyValue -InputObject $Commit -Name 'sha')
    if ($commitSha -cne $Context.MergeSha) {
        return "requested merge '$($Context.MergeSha)' returned commit '$commitSha'"
    }

    $parents = @(
        @(Get-ObjectPropertyValue -InputObject $Commit -Name 'parents') |
            ForEach-Object { $_ }
    )
    if ($parents.Count -ne 2) {
        return "merge '$($Context.MergeSha)' has $($parents.Count) parent(s), expected exactly 2"
    }

    $baseParentSha = [string](Get-ObjectPropertyValue -InputObject $parents[0] -Name 'sha')
    if ($baseParentSha -cnotmatch '^[0-9a-fA-F]{40}$') {
        return "merge '$($Context.MergeSha)' has an invalid first parent '$baseParentSha'"
    }

    $sourceParentSha = [string](Get-ObjectPropertyValue -InputObject $parents[1] -Name 'sha')
    if ($sourceParentSha -cne $Context.HeadSha) {
        return "merge '$($Context.MergeSha)' source parent '$sourceParentSha' does not match head '$($Context.HeadSha)'"
    }

    return $null
}

function Resolve-VerifiedCiFixContext {
    param(
        [Parameter(Mandatory = $true)][object]$NominatedContext,
        [Parameter(Mandatory = $true)][string]$Repository,
        [AllowEmptyString()][string]$GitHubToken,
        [AllowNull()][object]$FixtureData
    )

    $lastDiagnostic = 'no test merge metadata was available'
    $lastHeadSha = $NominatedContext.HeadSha
    $lastMergeSha = $NominatedContext.MergeSha

    for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
        $pullRequest = if ($null -ne $FixtureData) {
            Get-FixturePullRequestDetail `
                -FixtureData $FixtureData `
                -PullRequestNumber $NominatedContext.PullRequestNumber
        }
        else {
            Invoke-WithHttpRetry `
                -OperationName "GitHub pull request #$($NominatedContext.PullRequestNumber) detail refresh" `
                -Operation {
                    param($timeoutSeconds)

                    Invoke-RestMethod `
                        -Method Get `
                        -Uri "https://api.github.com/repos/$Repository/pulls/$($NominatedContext.PullRequestNumber)" `
                        -Headers (Get-GitHubApiHeaders -GitHubToken $GitHubToken) `
                        -TimeoutSec $timeoutSeconds
                }
        }

        $context = Get-CiFixContextFromPullRequest `
            -PullRequest $pullRequest `
            -Repository $Repository `
            -RequireMergeSha $false
        if ($null -eq $context) {
            return $null
        }

        $lastHeadSha = $context.HeadSha
        $lastMergeSha = $context.MergeSha
        if ($context.MergeSha -cnotmatch '^[0-9a-fA-F]{40}$') {
            $lastDiagnostic = 'the refreshed pull request has no syntactically valid test merge SHA'
        }
        else {
            try {
                $commit = if ($null -ne $FixtureData) {
                    Get-FixtureCommit -FixtureData $FixtureData -CommitSha $context.MergeSha
                }
                else {
                    Invoke-WithHttpRetry `
                        -OperationName "GitHub test merge commit $($context.MergeSha) for PR #$($context.PullRequestNumber)" `
                        -Operation {
                            param($timeoutSeconds)

                            Invoke-RestMethod `
                                -Method Get `
                                -Uri "https://api.github.com/repos/$Repository/git/commits/$($context.MergeSha)" `
                                -Headers (Get-GitHubApiHeaders -GitHubToken $GitHubToken) `
                                -TimeoutSec $timeoutSeconds
                        }
                }
                $lastDiagnostic = Get-CiFixMergePairDiagnostic -Context $context -Commit $commit
                if ($null -eq $lastDiagnostic) {
                    return $context
                }
            }
            catch {
                if (Test-IsDispatcherBudgetException -Exception $_.Exception) {
                    throw
                }
                $statusCode = Get-HttpStatusCode -Exception $_.Exception
                if ($null -eq $FixtureData -and $statusCode -notin @(404, 409, 422)) {
                    throw
                }
                $lastDiagnostic = "test merge metadata read failed: $($_.Exception.Message)"
            }
        }

        if ($attempt -lt $script:MaxHttpAttempts) {
            Invoke-DispatcherSleep `
                -OperationName "fresh test merge for PR #$($context.PullRequestNumber) attempt $($attempt + 1)" `
                -RequestedSeconds ($script:RetryBaseDelaySeconds * $attempt)
        }
    }

    $verificationError = "Eligible CI-fix PR #$($NominatedContext.PullRequestNumber) head '$lastHeadSha' did not obtain a verified test merge after $($script:MaxHttpAttempts) attempts. Last candidate merge '$lastMergeSha': $lastDiagnostic. No Azure DevOps validation was queued or deduplicated for this PR."
    $context | Add-Member -NotePropertyName VerificationError -NotePropertyValue $verificationError -Force
    return $context
}

function Get-CiFixEventContext {
    param(
        [Parameter(Mandatory = $true)][object]$Event,
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string]$EventName
    )

    if ($EventName -cne 'pull_request_target') {
        throw "Unexpected event '$EventName'."
    }

    $action = [string](Get-ObjectPropertyValue -InputObject $Event -Name 'action')
    if ($action -cnotin @('opened', 'reopened', 'synchronize', 'labeled')) {
        throw "Unexpected pull_request_target action '$action'."
    }

    if ($action -ceq 'labeled') {
        $eventLabel = Get-ObjectPropertyValue -InputObject (Get-ObjectPropertyValue -InputObject $Event -Name 'label') -Name 'name'
        if ([string]$eventLabel -cne 'agentic-workflows') {
            return $null
        }
    }

    $pullRequest = Get-ObjectPropertyValue -InputObject $Event -Name 'pull_request'
    if ($null -eq $pullRequest) {
        throw 'The event does not contain pull_request metadata.'
    }

    return Get-CiFixContextFromPullRequest -PullRequest $pullRequest -Repository $Repository
}

function Test-TrustedCiFixWorkflowRun {
    param(
        [Parameter(Mandatory = $true)][object]$Event,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    if ($Repository -cne 'dotnet/maui') {
        return $false
    }
    if ([string](Get-ObjectPropertyValue -InputObject $Event -Name 'action') -cne 'completed') {
        return $false
    }

    $eventRepository = Get-ObjectPropertyValue -InputObject $Event -Name 'repository'
    if ([string](Get-ObjectPropertyValue -InputObject $eventRepository -Name 'full_name') -cne 'dotnet/maui') {
        return $false
    }

    $workflowRun = Get-ObjectPropertyValue -InputObject $Event -Name 'workflow_run'
    $workflowName = [string](Get-ObjectPropertyValue -InputObject $workflowRun -Name 'name')
    $workflowPath = [string](Get-ObjectPropertyValue -InputObject $workflowRun -Name 'path')
    $headBranch = [string](Get-ObjectPropertyValue -InputObject $workflowRun -Name 'head_branch')
    $headRepository = Get-ObjectPropertyValue -InputObject $workflowRun -Name 'head_repository'

    $allowedWorkflows = @{
        'CI Failure Fixer (main)' = '.github/workflows/ci-status-fix.lock.yml'
        'CI Failure Fixer (net11.0)' = '.github/workflows/ci-status-fix-net11.lock.yml'
    }

    return $allowedWorkflows.ContainsKey($workflowName) -and
        $workflowPath -ceq $allowedWorkflows[$workflowName] -and
        $headBranch -ceq 'main' -and
        [string](Get-ObjectPropertyValue -InputObject $headRepository -Name 'full_name') -ceq 'dotnet/maui'
}

function Get-OpenCiFixContexts {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [AllowEmptyString()][string]$GitHubToken,
        [AllowEmptyString()][string]$FixturePath
    )

    if (-not [string]::IsNullOrWhiteSpace($FixturePath)) {
        if (-not (Test-Path -LiteralPath $FixturePath -PathType Leaf)) {
            throw "Pull request fixture '$FixturePath' does not exist."
        }

        $fixtureData = Get-Content -Raw -LiteralPath $FixturePath | ConvertFrom-Json -Depth 100
        $fixturePullRequests = @(
            @(Get-ObjectPropertyValue -InputObject $fixtureData -Name 'pullRequests') |
                ForEach-Object { $_ }
        )
        $fixtureContexts = [System.Collections.Generic.List[object]]::new()
        foreach ($pullRequest in $fixturePullRequests) {
            $nominatedContext = Get-CiFixContextFromPullRequest `
                -PullRequest $pullRequest `
                -Repository $Repository `
                -RequireMergeSha $false
            if ($null -ne $nominatedContext) {
                $verifiedContext = Resolve-VerifiedCiFixContext `
                    -NominatedContext $nominatedContext `
                    -Repository $Repository `
                    -GitHubToken '' `
                    -FixtureData $fixtureData
                if ($null -ne $verifiedContext) {
                    $fixtureContexts.Add($verifiedContext)
                }
            }
        }
        return $fixtureContexts.ToArray()
    }

    if ([string]::IsNullOrWhiteSpace($GitHubToken)) {
        throw 'GITHUB_TOKEN is required for workflow_run reconciliation.'
    }

    $contexts = [System.Collections.Generic.List[object]]::new()
    for ($page = 1; $page -le 10; $page++) {
        $pageResponse = Invoke-WithHttpRetry -OperationName "GitHub open pull request query page $page" -Operation {
            param($timeoutSeconds)

                Invoke-RestMethod `
                    -Method Get `
                    -Uri "https://api.github.com/repos/$Repository/pulls?state=open&per_page=100&page=$page" `
                    -Headers (Get-GitHubApiHeaders -GitHubToken $GitHubToken) `
                    -TimeoutSec $timeoutSeconds
            }
        # Invoke-RestMethod returns a top-level JSON array as one Object[] value.
        # Enumerate it explicitly so pagination and per-PR validation see each PR.
        $pullRequests = @($pageResponse | ForEach-Object { $_ })

        foreach ($pullRequest in $pullRequests) {
            $nominatedContext = Get-CiFixContextFromPullRequest `
                -PullRequest $pullRequest `
                -Repository $Repository `
                -RequireMergeSha $false
            if ($null -ne $nominatedContext) {
                $verifiedContext = Resolve-VerifiedCiFixContext `
                    -NominatedContext $nominatedContext `
                    -Repository $Repository `
                    -GitHubToken $GitHubToken `
                    -FixtureData $null
                if ($null -ne $verifiedContext) {
                    $contexts.Add($verifiedContext)
                }
            }
        }

        if ($pullRequests.Count -lt 100) {
            return $contexts.ToArray()
        }
    }

    throw 'Open pull request reconciliation exceeded the bounded 1,000-PR scan.'
}

function Test-IsTransientHttpException {
    param([Parameter(Mandatory = $true)][System.Exception]$Exception)

    $currentException = $Exception
    $statusCode = $null
    $hasTimeout = $false
    $hasNetworkException = $false
    while ($null -ne $currentException) {
        $response = Get-ObjectPropertyValue -InputObject $currentException -Name 'Response'
        $nestedStatusCode = Get-ObjectPropertyValue -InputObject $response -Name 'StatusCode'
        if ($null -eq $nestedStatusCode) {
            $nestedStatusCode = Get-ObjectPropertyValue -InputObject $currentException -Name 'StatusCode'
        }
        if ($null -eq $statusCode -and $null -ne $nestedStatusCode) {
            $statusCode = $nestedStatusCode
        }

        if ($currentException -is [System.TimeoutException]) {
            $hasTimeout = $true
        }
        if ($currentException -is [System.IO.IOException] -or
            $currentException -is [System.Net.Sockets.SocketException] -or
            $currentException -is [System.Net.Http.HttpRequestException]) {
            $hasNetworkException = $true
        }

        $currentException = $currentException.InnerException
    }

    if ($null -ne $statusCode) {
        $numericStatusCode = if ($statusCode.PSObject.Properties['value__']) {
            [int]$statusCode.value__
        }
        else {
            [int]$statusCode
        }
        return $numericStatusCode -in $script:TransientHttpStatusCodes
    }

    # PowerShell's request timeout can be a TaskCanceledException wrapping a
    # TimeoutException, IOException, and SocketException. Plain cancellation
    # remains non-transient so authentication and caller cancellation do not
    # trigger queue retries.
    return $hasTimeout -or $hasNetworkException
}

function Get-HttpStatusCode {
    param([Parameter(Mandatory = $true)][System.Exception]$Exception)

    $response = Get-ObjectPropertyValue -InputObject $Exception -Name 'Response'
    $statusCode = Get-ObjectPropertyValue -InputObject $response -Name 'StatusCode'
    if ($null -eq $statusCode) {
        $statusCode = Get-ObjectPropertyValue -InputObject $Exception -Name 'StatusCode'
    }
    if ($null -eq $statusCode) {
        return $null
    }

    if ($statusCode.PSObject.Properties['value__']) {
        return [int]$statusCode.value__
    }

    return [int]$statusCode
}

function Invoke-WithHttpRetry {
    param(
        [Parameter(Mandatory = $true)][string]$OperationName,
        [Parameter(Mandatory = $true)][scriptblock]$Operation
    )

    for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
        try {
            $timeoutSeconds = Get-DispatcherHttpTimeoutSeconds -OperationName "$OperationName attempt $attempt"
            return & $Operation $timeoutSeconds
        }
        catch {
            if (Test-IsDispatcherBudgetException -Exception $_.Exception) {
                throw
            }

            $isTransient = Test-IsTransientHttpException -Exception $_.Exception
            if (-not $isTransient -or $attempt -eq $script:MaxHttpAttempts) {
                throw
            }

            $delaySeconds = $script:RetryBaseDelaySeconds * $attempt
            Write-Warning "$OperationName failed transiently on attempt $attempt/$($script:MaxHttpAttempts); retrying in $delaySeconds seconds."
            Invoke-DispatcherSleep -OperationName "$OperationName retry $($attempt + 1)" -RequestedSeconds $delaySeconds
        }
    }
}

function Get-AzdoToken {
    $tenantId = $env:AZDO_TRIGGER_TENANT_ID
    $clientId = $env:AZDO_TRIGGER_CLIENT_ID
    if ([string]::IsNullOrWhiteSpace($tenantId) -or [string]::IsNullOrWhiteSpace($clientId)) {
        throw 'AZDO_TRIGGER_TENANT_ID and AZDO_TRIGGER_CLIENT_ID must be set.'
    }

    $requestToken = $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN
    $requestUrl = $env:ACTIONS_ID_TOKEN_REQUEST_URL
    if ([string]::IsNullOrWhiteSpace($requestToken) -or [string]::IsNullOrWhiteSpace($requestUrl)) {
        throw 'GitHub OIDC identity token request is unavailable.'
    }

    $oidcResponse = Invoke-WithHttpRetry -OperationName 'GitHub OIDC token request' -Operation {
        param($timeoutSeconds)

        Invoke-RestMethod `
            -Method Get `
            -Uri "$requestUrl&audience=api://AzureADTokenExchange" `
            -Headers @{ Authorization = "Bearer $requestToken" } `
            -TimeoutSec $timeoutSeconds
    }
    $oidcToken = [string](Get-ObjectPropertyValue -InputObject $oidcResponse -Name 'value')
    if ([string]::IsNullOrWhiteSpace($oidcToken)) {
        throw 'GitHub OIDC token request returned no token.'
    }
    Write-Host "::add-mask::$oidcToken"

    $body = @{
        grant_type = 'client_credentials'
        client_id = $clientId
        client_assertion_type = 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'
        client_assertion = $oidcToken
        scope = '499b84ac-1321-427f-aa17-267ca6975798/.default'
    }

    try {
        $tokenResponse = Invoke-WithHttpRetry -OperationName 'Azure AD token exchange' -Operation {
            param($timeoutSeconds)

            Invoke-RestMethod `
                -Method Post `
                -Uri "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" `
                -ContentType 'application/x-www-form-urlencoded' `
                -Body $body `
                -TimeoutSec $timeoutSeconds
        }
    }
    finally {
        $body.client_assertion = $null
        $oidcToken = $null
    }

    $accessToken = [string](Get-ObjectPropertyValue -InputObject $tokenResponse -Name 'access_token')
    if ([string]::IsNullOrWhiteSpace($accessToken)) {
        throw 'Azure AD token exchange returned no Azure DevOps access token.'
    }
    Write-Host "::add-mask::$accessToken"
    return $accessToken
}

function Find-AzdoDuplicateBuild {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Builds,
        [Parameter(Mandatory = $true)][int]$PullRequestNumber,
        [Parameter(Mandatory = $true)][string]$HeadSha,
        [Parameter(Mandatory = $true)][string]$MergeSha
    )

    $expectedBranch = "refs/pull/$PullRequestNumber/merge"
    foreach ($build in $Builds) {
        if ([string](Get-ObjectPropertyValue -InputObject $build -Name 'sourceBranch') -cne $expectedBranch) {
            continue
        }

        $triggerInfo = Get-ObjectPropertyValue -InputObject $build -Name 'triggerInfo'
        $sourceSha = [string](Get-ObjectPropertyValue -InputObject $triggerInfo -Name 'pr.sourceSha')
        $prNumber = [string](Get-ObjectPropertyValue -InputObject $triggerInfo -Name 'pr.number')

        if ($sourceSha -ceq $HeadSha -and
            ($prNumber -ceq '' -or $prNumber -ceq "$PullRequestNumber")) {
            return $build
        }
    }

    return $null
}

function Get-AzdoDuplicateBuild {
    param(
        [Parameter(Mandatory = $true)][int]$DefinitionId,
        [Parameter(Mandatory = $true)][int]$PullRequestNumber,
        [Parameter(Mandatory = $true)][string]$HeadSha,
        [Parameter(Mandatory = $true)][string]$MergeSha,
        [Parameter(Mandatory = $true)][string]$AuthToken
    )

    $branchName = [System.Uri]::EscapeDataString("refs/pull/$PullRequestNumber/merge")
    $url = "https://dev.azure.com/$($script:AzureDevOpsOrganization)/$($script:AzureDevOpsProject)/_apis/build/builds" +
        "?definitions=$DefinitionId&branchName=$branchName&queryOrder=queueTimeDescending&`$top=50&api-version=7.1"

    $response = Invoke-WithHttpRetry -OperationName "Azure DevOps duplicate query for definition $DefinitionId" -Operation {
        param($timeoutSeconds)

        Invoke-RestMethod `
            -Method Get `
            -Uri $url `
            -Headers @{ Authorization = "Bearer $AuthToken" } `
            -TimeoutSec $timeoutSeconds
    }

    return Find-AzdoDuplicateBuild `
        -Builds @(Get-ObjectPropertyValue -InputObject $response -Name 'value') `
        -PullRequestNumber $PullRequestNumber `
        -HeadSha $HeadSha `
        -MergeSha $MergeSha
}

function New-AzdoQueueRequest {
    param(
        [Parameter(Mandatory = $true)][int]$DefinitionId,
        [Parameter(Mandatory = $true)][object]$Context
    )

    return [ordered]@{
        definition = [ordered]@{ id = $DefinitionId }
        reason = 'pullRequest'
        sourceBranch = "refs/pull/$($Context.PullRequestNumber)/merge"
        sourceVersion = $Context.MergeSha
        parameters = ([ordered]@{
            'system.pullRequest.pullRequestId' = "$($Context.PullRequestId)"
            'system.pullRequest.pullRequestNumber' = "$($Context.PullRequestNumber)"
            'system.pullRequest.mergedAt' = ''
            'system.pullRequest.sourceBranch' = $Context.HeadRef
            'system.pullRequest.targetBranch' = $Context.BaseRef
            'system.pullRequest.targetBranchName' = $Context.BaseRef
            'system.pullRequest.sourceRepositoryUri' = 'https://github.com/dotnet/maui'
            'system.pullRequest.sourceCommitId' = $Context.HeadSha
            'system.pullRequest.isFork' = 'False'
        } | ConvertTo-Json -Compress)
        triggerInfo = [ordered]@{
            'pr.sourceBranch' = $Context.HeadRef
            'pr.sourceSha' = $Context.HeadSha
            'pr.targetBranch' = $Context.BaseRef
            'pr.id' = "$($Context.PullRequestId)"
            'pr.title' = $Context.Title
            'pr.number' = "$($Context.PullRequestNumber)"
            'pr.isFork' = 'False'
            'pr.draft' = "$($Context.Draft)"
            'pr.providerId' = 'github'
            'pr.autoCancel' = 'true'
        }
    }
}

function Invoke-AzdoPipelineQueue {
    param(
        [Parameter(Mandatory = $true)][int]$DefinitionId,
        [Parameter(Mandatory = $true)][object]$Context,
        [Parameter(Mandatory = $true)][string]$AuthToken
    )

    $request = New-AzdoQueueRequest -DefinitionId $DefinitionId -Context $Context
    $body = $request | ConvertTo-Json -Depth 10 -Compress
    $url = "https://dev.azure.com/$($script:AzureDevOpsOrganization)/$($script:AzureDevOpsProject)/_apis/build/builds?api-version=7.1"

    $postTimeoutSeconds = Get-DispatcherHttpTimeoutSeconds -OperationName "Azure DevOps queue POST for definition $DefinitionId"
    try {
        $build = Invoke-RestMethod `
            -Method Post `
            -Uri $url `
            -Headers @{ Authorization = "Bearer $AuthToken" } `
            -ContentType 'application/json' `
            -Body $body `
            -TimeoutSec $postTimeoutSeconds
        return [pscustomobject]@{ Build = $build; Reconciled = $false }
    }
    catch {
        if (-not (Test-IsTransientHttpException -Exception $_.Exception)) {
            throw (Get-AzdoQueueFailureMessage -ErrorRecord $_)
        }
        $queueStatusCode = Get-HttpStatusCode -Exception $_.Exception

        # A timed-out or 5xx POST may have been accepted before the response was
        # lost. Never blindly retry an ambiguous queue request. Reconcile the
        # exact definition + PR ref + source head/merge identity first.
        for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
            try {
                Invoke-DispatcherSleep `
                    -OperationName "ambiguous queue reconciliation for definition $DefinitionId attempt $attempt" `
                    -RequestedSeconds ($script:RetryBaseDelaySeconds * $attempt)
                $duplicate = Get-AzdoDuplicateBuild `
                    -DefinitionId $DefinitionId `
                    -PullRequestNumber $Context.PullRequestNumber `
                    -HeadSha $Context.HeadSha `
                    -MergeSha $Context.MergeSha `
                    -AuthToken $AuthToken
            }
            catch {
                if (Test-IsDispatcherBudgetException -Exception $_.Exception) {
                    throw "$($script:DispatcherBudgetPrefix) Azure DevOps queue request for definition $DefinitionId may have been accepted, but the shared dispatcher budget expired before exact reconciliation completed. The POST was issued exactly once and was not retried; acceptance remains uncertain."
                }
                throw "Azure DevOps queue request for definition $DefinitionId may have been accepted after an ambiguous transient failure (HTTP $queueStatusCode), but exact reconciliation failed: $($_.Exception.Message). The POST was issued exactly once and was not retried; acceptance remains uncertain."
            }
            if ($null -ne $duplicate) {
                return [pscustomobject]@{ Build = $duplicate; Reconciled = $true }
            }
        }

        throw "Azure DevOps queue request for definition $DefinitionId may have been accepted after an ambiguous transient failure (HTTP $queueStatusCode), but no exact correlated build appeared after reconciliation. The POST was issued exactly once and was not retried; acceptance remains uncertain."
    }
}

function Write-CiFixJobSummary {
    param(
        [Parameter(Mandatory = $true)][object]$Context,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Results
    )

    if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
        return
    }

    $lines = @(
        '## Automated CI-fix Azure DevOps validation',
        '',
        "- PR: dotnet/maui#$($Context.PullRequestNumber)",
        "- Base: ``$($Context.BaseRef)``",
        "- Head: ``$($Context.HeadSha)``",
        "- PR ref: ``refs/pull/$($Context.PullRequestNumber)/merge``",
        '',
        '| Pipeline | Result | Build | Details |',
        '|---|---|---|---|'
    )

    foreach ($result in $Results) {
        $build = if ($result.BuildId) {
            "[build $($result.BuildId)](https://dev.azure.com/dnceng-public/public/_build/results?buildId=$($result.BuildId))"
        }
        else {
            '-'
        }
        $details = if ($result.PSObject.Properties['Error']) {
            ([string]$result.Error).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
        }
        else {
            '-'
        }
        $lines += "| $($result.Name) | $($result.Outcome) | $build | $details |"
    }

    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ($lines -join [Environment]::NewLine)
}

function New-CiFixFailureResult {
    param(
        [Parameter(Mandatory = $true)][object]$Context,
        [Parameter(Mandatory = $true)][object]$Pipeline,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    return [pscustomobject]@{
        PullRequestNumber = $Context.PullRequestNumber
        Name = $Pipeline.Name
        DefinitionId = $Pipeline.DefinitionId
        Outcome = 'failed'
        BuildId = $null
        Error = $ErrorMessage
    }
}

function Invoke-CiFixQueueWork {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Contexts,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$AuthToken,
        [string]$Repository = 'dotnet/maui',
        [AllowEmptyString()][string]$GitHubToken = '',
        [AllowNull()][object]$FixtureData
    )

    $workResults = [System.Collections.Generic.List[object]]::new()
    $budgetExhaustedAfter = $null
    foreach ($context in $Contexts) {
        $contextResults = [System.Collections.Generic.List[object]]::new()
        $verificationError = [string](Get-ObjectPropertyValue -InputObject $context -Name 'VerificationError')
        if ([string]::IsNullOrWhiteSpace($verificationError) -and
            [string]::IsNullOrWhiteSpace($AuthToken)) {
            throw "Azure DevOps authentication is required before processing verified PR #$($context.PullRequestNumber)."
        }

        $contextStoppedError = $null
        foreach ($pipeline in Get-CiFixPipelineDefinitions) {
            if (-not [string]::IsNullOrWhiteSpace($verificationError)) {
                $result = New-CiFixFailureResult `
                    -Context $context `
                    -Pipeline $pipeline `
                    -ErrorMessage $verificationError
                $workResults.Add($result)
                $contextResults.Add($result)
                continue
            }

            if ($null -ne $contextStoppedError) {
                $result = New-CiFixFailureResult `
                    -Context $context `
                    -Pipeline $pipeline `
                    -ErrorMessage $contextStoppedError
                $workResults.Add($result)
                $contextResults.Add($result)
                continue
            }

            if ($null -ne $budgetExhaustedAfter) {
                $skippedError = "$($script:DispatcherBudgetPrefix) PR #$($context.PullRequestNumber) pipeline '$($pipeline.Name)' was not processed because the shared dispatcher budget was exhausted while processing $budgetExhaustedAfter. No queue POST was attempted for this work item."
                $result = New-CiFixFailureResult -Context $context -Pipeline $pipeline -ErrorMessage $skippedError
                $workResults.Add($result)
                $contextResults.Add($result)
                continue
            }

            try {
                $currentContext = Resolve-VerifiedCiFixContext `
                    -NominatedContext $context `
                    -Repository $Repository `
                    -GitHubToken $GitHubToken `
                    -FixtureData $FixtureData
                $currentVerificationError = if ($null -eq $currentContext) {
                    "PR #$($context.PullRequestNumber) is no longer an eligible open automated CI-fix PR. No queue POST was attempted for pipeline '$($pipeline.Name)' or any remaining pipeline for this PR."
                }
                else {
                    [string](Get-ObjectPropertyValue -InputObject $currentContext -Name 'VerificationError')
                }
                if ([string]::IsNullOrWhiteSpace($currentVerificationError) -and
                    ($currentContext.HeadSha -cne $context.HeadSha -or
                        $currentContext.MergeSha -cne $context.MergeSha)) {
                    $currentVerificationError = "PR #$($context.PullRequestNumber) changed after discovery: expected head '$($context.HeadSha)' with merge '$($context.MergeSha)', but live verification found head '$($currentContext.HeadSha)' with merge '$($currentContext.MergeSha)'. No queue POST was attempted for pipeline '$($pipeline.Name)' or any remaining pipeline for this PR."
                }
                if (-not [string]::IsNullOrWhiteSpace($currentVerificationError)) {
                    $contextStoppedError = $currentVerificationError
                    $result = New-CiFixFailureResult `
                        -Context $context `
                        -Pipeline $pipeline `
                        -ErrorMessage $contextStoppedError
                    $workResults.Add($result)
                    $contextResults.Add($result)
                    continue
                }

                $duplicate = Get-AzdoDuplicateBuild `
                    -DefinitionId $pipeline.DefinitionId `
                    -PullRequestNumber $context.PullRequestNumber `
                    -HeadSha $context.HeadSha `
                    -MergeSha $context.MergeSha `
                    -AuthToken $AuthToken

                if ($null -ne $duplicate) {
                    $result = [pscustomobject]@{
                        PullRequestNumber = $context.PullRequestNumber
                        Name = $pipeline.Name
                        DefinitionId = $pipeline.DefinitionId
                        Outcome = 'deduplicated'
                        BuildId = [int](Get-ObjectPropertyValue -InputObject $duplicate -Name 'id')
                    }
                    $workResults.Add($result)
                    $contextResults.Add($result)
                    continue
                }

                $queueResult = Invoke-AzdoPipelineQueue `
                    -DefinitionId $pipeline.DefinitionId `
                    -Context $context `
                    -AuthToken $AuthToken
                $buildId = [int](Get-ObjectPropertyValue -InputObject $queueResult.Build -Name 'id')
                if ($buildId -le 0) {
                    throw "Azure DevOps returned an invalid build id for definition $($pipeline.DefinitionId)."
                }

                $result = [pscustomobject]@{
                    PullRequestNumber = $context.PullRequestNumber
                    Name = $pipeline.Name
                    DefinitionId = $pipeline.DefinitionId
                    Outcome = if ($queueResult.Reconciled) { 'reconciled-after-ambiguous-post' } else { 'queued' }
                    BuildId = $buildId
                }
                $workResults.Add($result)
                $contextResults.Add($result)
            }
            catch {
                Write-Error -ErrorAction Continue "PR #$($context.PullRequestNumber) pipeline '$($pipeline.Name)' failed: $($_.Exception.Message)"
                $result = New-CiFixFailureResult -Context $context -Pipeline $pipeline -ErrorMessage $_.Exception.Message
                $workResults.Add($result)
                $contextResults.Add($result)
                if (Test-IsDispatcherBudgetException -Exception $_.Exception) {
                    $budgetExhaustedAfter = "PR #$($context.PullRequestNumber) pipeline '$($pipeline.Name)'"
                }
            }
        }
        Write-CiFixJobSummary -Context $context -Results $contextResults
    }

    return $workResults.ToArray()
}

if ([Math]::Floor((Get-DispatcherRemainingSeconds)) -lt 1) {
    $preparationFailure = "$($script:DispatcherBudgetPrefix) Dispatcher deadline exhausted during preparation (trusted checkout / PowerShell startup), before PR discovery or authentication. No queue POST was attempted."
    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
        $lines = @(
            '## Automated CI-fix Azure DevOps validation',
            '',
            '- Stage: preparation / dispatcher deadline',
            '- Result: failed',
            "- Details: $preparationFailure"
        )
        Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ($lines -join [Environment]::NewLine)
    }
    throw $preparationFailure
}

if ([string]::IsNullOrWhiteSpace($EventPath) -or -not (Test-Path -LiteralPath $EventPath -PathType Leaf)) {
    throw 'GITHUB_EVENT_PATH must identify a supported GitHub event payload file.'
}
if ([string]::IsNullOrWhiteSpace($Repository)) {
    throw 'GITHUB_REPOSITORY is required.'
}
if (-not [string]::IsNullOrWhiteSpace($PullRequestsFixturePath) -and -not $DryRun) {
    throw 'PullRequestsFixturePath is permitted only with -DryRun.'
}

$event = Get-Content -Raw -LiteralPath $EventPath | ConvertFrom-Json -Depth 100
$contexts = if ($EventName -ceq 'pull_request_target') {
    [void](Get-CiFixEventContext -Event $event -Repository $Repository -EventName $EventName)
    # GitHub concurrency preserves only one pending run. Every configured
    # PR-target event therefore reconciles every live eligible head, even when
    # its own PR is unrelated. The live scan is authoritative so a delayed
    # webhook snapshot cannot restore revoked eligibility or queue an old SHA.
    @(
        Get-OpenCiFixContexts `
            -Repository $Repository `
            -GitHubToken $env:GITHUB_TOKEN `
            -FixturePath $PullRequestsFixturePath
    )
}
elseif ($EventName -ceq 'workflow_run') {
    if (-not (Test-TrustedCiFixWorkflowRun -Event $event -Repository $Repository)) {
        throw 'workflow_run did not originate from a trusted default-branch CI-fixer workflow.'
    }
    @(
        Get-OpenCiFixContexts `
            -Repository $Repository `
            -GitHubToken $env:GITHUB_TOKEN `
            -FixturePath $PullRequestsFixturePath
    )
}
else {
    throw "Unexpected event '$EventName'."
}

$contexts = @($contexts | Where-Object { $null -ne $_ })
if ($contexts.Count -eq 0) {
    Write-Output 'No eligible automated CI-fix pull request heads require reconciliation.'
    exit 0
}

$results = [System.Collections.Generic.List[object]]::new()
$verificationFailureContexts = @(
    $contexts | Where-Object {
        -not [string]::IsNullOrWhiteSpace(
            [string](Get-ObjectPropertyValue -InputObject $_ -Name 'VerificationError'))
    }
)
$queueContexts = @(
    $contexts | Where-Object {
        [string]::IsNullOrWhiteSpace(
            [string](Get-ObjectPropertyValue -InputObject $_ -Name 'VerificationError'))
    }
)
if ($verificationFailureContexts.Count -gt 0) {
    foreach ($result in Invoke-CiFixQueueWork -Contexts $verificationFailureContexts -AuthToken '') {
        $results.Add($result)
    }
}

if ($DryRun) {
    foreach ($context in $queueContexts) {
        $contextResults = [System.Collections.Generic.List[object]]::new()
        foreach ($pipeline in Get-CiFixPipelineDefinitions) {
            $request = New-AzdoQueueRequest -DefinitionId $pipeline.DefinitionId -Context $context
            $result = [pscustomobject]@{
                PullRequestNumber = $context.PullRequestNumber
                Name = $pipeline.Name
                DefinitionId = $pipeline.DefinitionId
                Outcome = 'dry-run'
                BuildId = $null
                Request = $request
            }
            $results.Add($result)
            $contextResults.Add($result)
        }
        Write-CiFixJobSummary -Context $context -Results $contextResults
    }
    $results | ConvertTo-Json -Depth 10
    $dryRunFailures = @($results | Where-Object Outcome -eq 'failed')
    if ($dryRunFailures.Count -gt 0) {
        throw "$($dryRunFailures.Count) of $($results.Count) Azure DevOps validation pipelines failed before dry-run queue payload generation."
    }
    exit 0
}

$pipelines = @(Get-CiFixPipelineDefinitions)
if ($queueContexts.Count -eq 0) {
    $results | ConvertTo-Json -Depth 10
    throw "$($results.Count) Azure DevOps validation pipelines failed test-merge verification; no queue requests were attempted."
}

try {
    $authToken = Get-AzdoToken
}
catch {
    $authFailure = "Dispatcher authentication failed before queueing: $($_.Exception.Message)"
    foreach ($context in $queueContexts) {
        $contextResults = @(
            foreach ($pipeline in $pipelines) {
                New-CiFixFailureResult -Context $context -Pipeline $pipeline -ErrorMessage $authFailure
            }
        )
        foreach ($result in $contextResults) {
            $results.Add($result)
        }
        Write-CiFixJobSummary -Context $context -Results $contextResults
    }
    $results | ConvertTo-Json -Depth 10
    throw $authFailure
}

try {
    $queueFixtureData = if ([string]::IsNullOrWhiteSpace($PullRequestsFixturePath)) {
        $null
    }
    else {
        Get-Content -Raw -LiteralPath $PullRequestsFixturePath | ConvertFrom-Json -Depth 100
    }
    foreach ($result in Invoke-CiFixQueueWork `
            -Contexts $queueContexts `
            -AuthToken $authToken `
            -Repository $Repository `
            -GitHubToken $env:GITHUB_TOKEN `
            -FixtureData $queueFixtureData) {
        $results.Add($result)
    }
}
finally {
    $authToken = $null
}

$results | ConvertTo-Json -Depth 10
$failures = @($results | Where-Object Outcome -eq 'failed')
if ($failures.Count -gt 0) {
    throw "$($failures.Count) of $($results.Count) Azure DevOps validation pipelines failed to queue or deduplicate."
}
