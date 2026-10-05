#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$EventPath = $env:GITHUB_EVENT_PATH,
    [string]$Repository = $env:GITHUB_REPOSITORY,
    [string]$EventName = $env:GITHUB_EVENT_NAME,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:AzureDevOpsOrganization = 'dnceng-public'
$script:AzureDevOpsProject = 'public'
$script:TransientHttpStatusCodes = @(408, 429, 500, 502, 503, 504)
$script:MaxHttpAttempts = 4
$script:RetryBaseDelaySeconds = 2

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
        [Parameter(Mandatory = $true)][string]$Repository
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
    if ($context.MergeSha -cnotmatch '^[0-9a-fA-F]{40}$') {
        Write-Warning "Eligible CI-fix PR #$($context.PullRequestNumber) has no merge commit yet; deferring validation."
        return $null
    }

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
        [Parameter(Mandatory = $true)][string]$GitHubToken
    )

    if ([string]::IsNullOrWhiteSpace($GitHubToken)) {
        throw 'GITHUB_TOKEN is required for workflow_run reconciliation.'
    }

    $contexts = [System.Collections.Generic.List[object]]::new()
    for ($page = 1; $page -le 10; $page++) {
        $pullRequests = @(
            Invoke-WithHttpRetry -OperationName "GitHub open pull request query page $page" -Operation {
                Invoke-RestMethod `
                    -Method Get `
                    -Uri "https://api.github.com/repos/$Repository/pulls?state=open&per_page=100&page=$page" `
                    -Headers @{
                        Authorization = "Bearer $GitHubToken"
                        Accept = 'application/vnd.github+json'
                        'X-GitHub-Api-Version' = '2022-11-28'
                    } `
                    -TimeoutSec 30
            }
        )

        foreach ($pullRequest in $pullRequests) {
            $context = Get-CiFixContextFromPullRequest -PullRequest $pullRequest -Repository $Repository
            if ($null -ne $context) {
                $contexts.Add($context)
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

    if ($Exception -is [System.TimeoutException] -or $Exception -is [System.Net.Http.HttpRequestException]) {
        $statusCode = Get-ObjectPropertyValue -InputObject $Exception -Name 'StatusCode'
        if ($null -eq $statusCode) {
            return $true
        }
    }

    $response = Get-ObjectPropertyValue -InputObject $Exception -Name 'Response'
    $statusCode = Get-ObjectPropertyValue -InputObject $response -Name 'StatusCode'
    if ($null -eq $statusCode) {
        $statusCode = Get-ObjectPropertyValue -InputObject $Exception -Name 'StatusCode'
    }
    if ($null -eq $statusCode) {
        return $false
    }

    $numericStatusCode = if ($statusCode.PSObject.Properties['value__']) {
        [int]$statusCode.value__
    }
    else {
        [int]$statusCode
    }

    return $numericStatusCode -in $script:TransientHttpStatusCodes
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
            return & $Operation
        }
        catch {
            $isTransient = Test-IsTransientHttpException -Exception $_.Exception
            if (-not $isTransient -or $attempt -eq $script:MaxHttpAttempts) {
                throw
            }

            $delaySeconds = $script:RetryBaseDelaySeconds * $attempt
            Write-Warning "$OperationName failed transiently on attempt $attempt/$($script:MaxHttpAttempts); retrying in $delaySeconds seconds."
            Start-Sleep -Seconds $delaySeconds
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
        Invoke-RestMethod `
            -Method Get `
            -Uri "$requestUrl&audience=api://AzureADTokenExchange" `
            -Headers @{ Authorization = "Bearer $requestToken" } `
            -TimeoutSec 30
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
            Invoke-RestMethod `
                -Method Post `
                -Uri "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" `
                -ContentType 'application/x-www-form-urlencoded' `
                -Body $body `
                -TimeoutSec 30
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
        $sourceVersion = [string](Get-ObjectPropertyValue -InputObject $build -Name 'sourceVersion')

        if (($sourceSha -ceq $HeadSha -and ($prNumber -ceq '' -or $prNumber -ceq "$PullRequestNumber")) -or
            $sourceVersion -ceq $HeadSha -or
            $sourceVersion -ceq $MergeSha) {
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
        Invoke-RestMethod `
            -Method Get `
            -Uri $url `
            -Headers @{ Authorization = "Bearer $AuthToken" } `
            -TimeoutSec 30
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
        triggerInfo = [ordered]@{
            'pr.sourceBranch' = $Context.HeadRef
            'pr.sourceSha' = $Context.HeadSha
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

    try {
        $build = Invoke-RestMethod `
            -Method Post `
            -Uri $url `
            -Headers @{ Authorization = "Bearer $AuthToken" } `
            -ContentType 'application/json' `
            -Body $body `
            -TimeoutSec 30
        return [pscustomobject]@{ Build = $build; Reconciled = $false }
    }
    catch {
        if (-not (Test-IsTransientHttpException -Exception $_.Exception)) {
            throw
        }

        # A timed-out or 5xx POST may have been accepted before the response was
        # lost. Never blindly retry an ambiguous queue request. Reconcile the
        # exact definition + PR ref + source head/merge identity first.
        for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
            Start-Sleep -Seconds ($script:RetryBaseDelaySeconds * $attempt)
            $duplicate = Get-AzdoDuplicateBuild `
                -DefinitionId $DefinitionId `
                -PullRequestNumber $Context.PullRequestNumber `
                -HeadSha $Context.HeadSha `
                -MergeSha $Context.MergeSha `
                -AuthToken $AuthToken
            if ($null -ne $duplicate) {
                return [pscustomobject]@{ Build = $duplicate; Reconciled = $true }
            }
        }

        $statusCode = Get-HttpStatusCode -Exception $_.Exception
        throw "Azure DevOps queue request for definition $DefinitionId had an ambiguous transient failure (HTTP $statusCode) and no exact correlated build appeared after reconciliation. The POST was not retried to avoid duplicate builds."
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
        '| Pipeline | Result | Build |',
        '|---|---|---|'
    )

    foreach ($result in $Results) {
        $build = if ($result.BuildId) {
            "[build $($result.BuildId)](https://dev.azure.com/dnceng-public/public/_build/results?buildId=$($result.BuildId))"
        }
        else {
            '-'
        }
        $lines += "| $($result.Name) | $($result.Outcome) | $build |"
    }

    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ($lines -join [Environment]::NewLine)
}

if ([string]::IsNullOrWhiteSpace($EventPath) -or -not (Test-Path -LiteralPath $EventPath -PathType Leaf)) {
    throw 'GITHUB_EVENT_PATH must identify the pull_request_target event payload.'
}
if ([string]::IsNullOrWhiteSpace($Repository)) {
    throw 'GITHUB_REPOSITORY is required.'
}

$event = Get-Content -Raw -LiteralPath $EventPath | ConvertFrom-Json -Depth 100
$contexts = if ($EventName -ceq 'pull_request_target') {
    @(Get-CiFixEventContext -Event $event -Repository $Repository -EventName $EventName)
}
elseif ($EventName -ceq 'workflow_run') {
    if (-not (Test-TrustedCiFixWorkflowRun -Event $event -Repository $Repository)) {
        throw 'workflow_run did not originate from a trusted main-branch CI-fixer workflow.'
    }
    @(Get-OpenCiFixContexts -Repository $Repository -GitHubToken $env:GITHUB_TOKEN)
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
if ($DryRun) {
    foreach ($context in $contexts) {
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
    exit 0
}

$authToken = Get-AzdoToken
try {
    foreach ($context in $contexts) {
        $contextResults = [System.Collections.Generic.List[object]]::new()
        foreach ($pipeline in Get-CiFixPipelineDefinitions) {
            try {
                $duplicate = Get-AzdoDuplicateBuild `
                    -DefinitionId $pipeline.DefinitionId `
                    -PullRequestNumber $context.PullRequestNumber `
                    -HeadSha $context.HeadSha `
                    -MergeSha $context.MergeSha `
                    -AuthToken $authToken

                if ($null -ne $duplicate) {
                    $result = [pscustomobject]@{
                        PullRequestNumber = $context.PullRequestNumber
                        Name = $pipeline.Name
                        DefinitionId = $pipeline.DefinitionId
                        Outcome = 'deduplicated'
                        BuildId = [int](Get-ObjectPropertyValue -InputObject $duplicate -Name 'id')
                    }
                    $results.Add($result)
                    $contextResults.Add($result)
                    continue
                }

                $queueResult = Invoke-AzdoPipelineQueue `
                    -DefinitionId $pipeline.DefinitionId `
                    -Context $context `
                    -AuthToken $authToken
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
                $results.Add($result)
                $contextResults.Add($result)
            }
            catch {
                Write-Error -ErrorAction Continue "PR #$($context.PullRequestNumber) pipeline '$($pipeline.Name)' failed: $($_.Exception.Message)"
                $result = [pscustomobject]@{
                    PullRequestNumber = $context.PullRequestNumber
                    Name = $pipeline.Name
                    DefinitionId = $pipeline.DefinitionId
                    Outcome = 'failed'
                    BuildId = $null
                    Error = $_.Exception.Message
                }
                $results.Add($result)
                $contextResults.Add($result)
            }
        }
        Write-CiFixJobSummary -Context $context -Results $contextResults
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
