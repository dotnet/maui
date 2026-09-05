#!/usr/bin/env pwsh
#Requires -Version 7.0

$Script:InternalOfficialBuildDefinitionId = 1095
$Script:InternalOfficialBuildOrg = 'dnceng'
$Script:InternalOfficialBuildProject = 'internal'

function Get-InternalBuildProperty {
    param(
        [AllowNull()]$InputObject,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $InputObject) { return $null }
    if ($InputObject -is [System.Collections.IDictionary]) {
        if ($InputObject.Contains($Name)) { return $InputObject[$Name] }
        return $null
    }
    if ($InputObject.PSObject -and $InputObject.PSObject.Properties[$Name]) {
        return $InputObject.$Name
    }
    return $null
}

function Test-IsGitHubActions {
    param([AllowNull()][string]$Value = $env:GITHUB_ACTIONS)
    return -not [string]::IsNullOrWhiteSpace($Value) -and $Value.Equals('true', [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-InternalOfficialBuildBranches {
    param(
        [Parameter(Mandatory = $true)][int]$MajorVersion,
        [Parameter(Mandatory = $true)][string]$ReleaseBranch,
        [Parameter(Mandatory = $true)][bool]$ReleaseBranchExists
    )

    $refs = [System.Collections.Generic.List[string]]::new()
    [void]$refs.Add("refs/heads/net$MajorVersion.0")
    $releaseRef = "refs/heads/$ReleaseBranch"
    if ($ReleaseBranchExists -and -not $refs.Contains($releaseRef)) {
        [void]$refs.Add($releaseRef)
    }
    return @($refs)
}

function Get-InternalOfficialBuildClassification {
    param(
        [AllowNull()]$Build,
        [Parameter(Mandatory = $true)][string]$ExpectedBranchRef,
        [AllowNull()][string]$BranchHeadSha,
        [AllowNull()][Nullable[bool]]$BuildCoversHead,
        [bool]$BlocksRegardlessOfCurrency = $false
    )

    if ($null -eq $Build) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'no-build' }
    }

    $sourceBranch = [string](Get-InternalBuildProperty $Build 'sourceBranch')
    $sourceSha = [string](Get-InternalBuildProperty $Build 'sourceVersion')
    $status = [string](Get-InternalBuildProperty $Build 'status')
    $result = [string](Get-InternalBuildProperty $Build 'result')

    if ([string]::IsNullOrWhiteSpace($sourceBranch) -or
        -not $sourceBranch.Equals($ExpectedBranchRef, [System.StringComparison]::OrdinalIgnoreCase)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'branch-mismatch' }
    }
    if ($BlocksRegardlessOfCurrency -and
        (Test-InternalOfficialBuildHasBlockingResult $Build)) {
        return [PSCustomObject]@{
            Classification = 'failed-or-stale'
            Reason = "completed-$($result.ToLowerInvariant())-or-stale"
        }
    }
    if ([string]::IsNullOrWhiteSpace($sourceSha)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'missing-source-sha' }
    }
    if ([string]::IsNullOrWhiteSpace($BranchHeadSha)) {
        return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'missing-branch-head' }
    }
    $sourceMatchesHead = $sourceSha.Equals($BranchHeadSha, [System.StringComparison]::OrdinalIgnoreCase)
    if (-not $sourceMatchesHead) {
        if ($null -eq $BuildCoversHead) {
            return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'build-currency-unavailable' }
        }
        if ($BuildCoversHead -ne $true) {
            return [PSCustomObject]@{ Classification = 'stale'; Reason = 'source-sha-trails-head' }
        }
    }
    $triggerCurrentReasonSuffix = if ($sourceMatchesHead) { '' } else { '-after-trigger-excluded-changes' }

    switch ($status.ToLowerInvariant()) {
        { $_ -in @('inprogress', 'notstarted', 'postponed', 'cancelling') } {
            return [PSCustomObject]@{ Classification = 'in-progress'; Reason = 'build-not-complete' }
        }
        'completed' {
            switch ($result.ToLowerInvariant()) {
                'succeeded' {
                    return [PSCustomObject]@{ Classification = 'green'; Reason = "completed-succeeded$triggerCurrentReasonSuffix" }
                }
                'partiallysucceeded' {
                    return [PSCustomObject]@{ Classification = 'partial-success'; Reason = "completed-partiallysucceeded$triggerCurrentReasonSuffix" }
                }
                { $_ -in @('failed', 'canceled', 'cancelled') } {
                    return [PSCustomObject]@{ Classification = 'red'; Reason = "completed-$($_)" }
                }
                default {
                    return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'completed-with-unknown-result' }
                }
            }
        }
        default {
            return [PSCustomObject]@{ Classification = 'unknown'; Reason = 'unknown-build-status' }
        }
    }
}

function Get-InternalOfficialBuildOverallClassification {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][array]$Branches)

    if ($Branches.Count -eq 0) { return 'skipped' }

    $rank = @{
        'skipped'     = 0
        'green'       = 1
        'in-progress' = 2
        'partial-success' = 3
        'unknown'     = 4
        'stale'           = 5
        'failed-or-stale' = 6
        'red'             = 6
    }
    $worst = 'skipped'
    foreach ($branch in $Branches) {
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        if (-not $rank.ContainsKey($classification)) { $classification = 'unknown' }
        if ($rank[$classification] -gt $rank[$worst]) { $worst = $classification }
    }
    return $worst
}

function Test-InternalOfficialBuildHasBlockingResult {
    param([AllowNull()]$Build)

    if ($null -eq $Build) { return $false }
    $status = [string](Get-InternalBuildProperty $Build 'status')
    $result = [string](Get-InternalBuildProperty $Build 'result')
    return $status.Equals('completed', [System.StringComparison]::OrdinalIgnoreCase) -and
        $result.ToLowerInvariant() -in @('failed', 'canceled', 'cancelled')
}

function ConvertTo-SafeInternalBuildNumber {
    param([AllowNull()]$BuildNumber)

    if ($null -eq $BuildNumber) { return $null }

    $value = [string]$BuildNumber
    if ($value -match '\A\d{8}\.\d{1,10}\z') {
        return $value
    }

    return 'invalid-build-number'
}

function Test-InternalOfficialBuildTriggerExcludedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ($Path.StartsWith('.github/', [System.StringComparison]::Ordinal)) {
        return $true
    }
    if ($Path -cmatch '\Adocs/[^/]+\z') {
        return $true
    }
    return $Path -cin @(
        'CODE-OF-CONDUCT.md',
        'CONTRIBUTING.md',
        'LICENSE.TXT',
        'PATENTS.TXT',
        'README.md',
        'THIRD-PARTY-NOTICES.TXT'
    )
}

function Test-InternalOfficialBuildChangedPathsCoverHead {
    param([AllowNull()][string[]]$Paths)

    $changedPaths = @($Paths | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($changedPaths.Count -eq 0) { return $true }
    foreach ($path in $changedPaths) {
        if (-not (Test-InternalOfficialBuildTriggerExcludedPath $path)) {
            return $false
        }
    }
    return $true
}

function Get-OrderedInternalOfficialBuilds {
    param([AllowNull()][object[]]$Builds)

    return @($Builds) |
        Where-Object { $null -ne $_ } |
        Sort-Object -Property @(
            @{ Expression = {
                $queueTime = Get-InternalBuildProperty $_ 'queueTime'
                if ($queueTime) {
                    try { return [DateTimeOffset]::Parse([string]$queueTime) } catch { }
                }
                return [DateTimeOffset]::MinValue
            }; Descending = $true },
            @{ Expression = {
                $id = Get-InternalBuildProperty $_ 'id'
                $parsedId = 0L
                if ($null -ne $id -and [long]::TryParse([string]$id, [ref]$parsedId)) {
                    return $parsedId
                }
                return 0L
            }; Descending = $true }
        )
}

function Select-LatestInternalOfficialBuild {
    param([AllowNull()][object[]]$Builds)

    return @(Get-OrderedInternalOfficialBuilds -Builds $Builds) |
        Select-Object -First 1
}

function Select-InternalOfficialBuildForHead {
    param(
        [AllowNull()][object[]]$Builds,
        [AllowNull()][string]$BranchHeadSha,
        [Parameter(Mandatory = $true)][string]$BranchRef,
        [AllowNull()][scriptblock]$BuildCurrencyFetcher
    )

    $orderedBuilds = @(Get-OrderedInternalOfficialBuilds -Builds $Builds)
    if ($orderedBuilds.Count -eq 0) {
        return [PSCustomObject]@{ Build = $null; CoversHead = $null; BlocksRegardlessOfCurrency = $false }
    }

    if (-not [string]::IsNullOrWhiteSpace($BranchHeadSha)) {
        $exactBuild = $orderedBuilds |
            Where-Object {
                $sourceBranch = [string](Get-InternalBuildProperty $_ 'sourceBranch')
                $sourceSha = [string](Get-InternalBuildProperty $_ 'sourceVersion')
                $sourceBranch -eq $BranchRef -and
                    -not [string]::IsNullOrWhiteSpace($sourceSha) -and
                    $sourceSha.Equals($BranchHeadSha, [System.StringComparison]::OrdinalIgnoreCase)
            } |
            Select-Object -First 1
        if ($null -ne $exactBuild) {
            return [PSCustomObject]@{ Build = $exactBuild; CoversHead = $true; BlocksRegardlessOfCurrency = $false }
        }
    }

    $latestBuild = $orderedBuilds[0]
    $latestCoverage = $null
    $firstIndeterminateBuild = $null
    $allIndeterminateBuildsBlock = $true
    if ($BuildCurrencyFetcher -and -not [string]::IsNullOrWhiteSpace($BranchHeadSha)) {
        foreach ($candidate in $orderedBuilds) {
            $sourceBranch = [string](Get-InternalBuildProperty $candidate 'sourceBranch')
            $sourceSha = [string](Get-InternalBuildProperty $candidate 'sourceVersion')
            if ($sourceBranch -ne $BranchRef) {
                if ($null -eq $firstIndeterminateBuild) {
                    $firstIndeterminateBuild = $candidate
                }
                $allIndeterminateBuildsBlock = $false
                continue
            }
            if ([string]::IsNullOrWhiteSpace($sourceSha)) {
                if ($null -eq $firstIndeterminateBuild) {
                    $firstIndeterminateBuild = $candidate
                }
                $allIndeterminateBuildsBlock = $allIndeterminateBuildsBlock -and
                    (Test-InternalOfficialBuildHasBlockingResult $candidate)
                continue
            }

            $coverage = try {
                $currencyEvidence = & $BuildCurrencyFetcher $BranchRef $sourceSha $BranchHeadSha
                if ($null -eq $currencyEvidence) { $null } else { [bool]$currencyEvidence }
            } catch {
                $null
            }
            if ($candidate -eq $latestBuild) {
                $latestCoverage = $coverage
            }
            if ($null -eq $coverage) {
                if ($null -eq $firstIndeterminateBuild) {
                    $firstIndeterminateBuild = $candidate
                }
                $allIndeterminateBuildsBlock = $allIndeterminateBuildsBlock -and
                    (Test-InternalOfficialBuildHasBlockingResult $candidate)
                continue
            }
            if ($coverage -eq $true) {
                if ($null -ne $firstIndeterminateBuild) {
                    if ($allIndeterminateBuildsBlock -and
                        (Test-InternalOfficialBuildHasBlockingResult $candidate)) {
                        return [PSCustomObject]@{ Build = $candidate; CoversHead = $true; BlocksRegardlessOfCurrency = $false }
                    }
                    return [PSCustomObject]@{ Build = $firstIndeterminateBuild; CoversHead = $null; BlocksRegardlessOfCurrency = $false }
                }
                return [PSCustomObject]@{ Build = $candidate; CoversHead = $true; BlocksRegardlessOfCurrency = $false }
            }
        }
    }

    if ($null -ne $firstIndeterminateBuild) {
        return [PSCustomObject]@{
            Build = $firstIndeterminateBuild
            CoversHead = $null
            BlocksRegardlessOfCurrency = $allIndeterminateBuildsBlock
        }
    }
    return [PSCustomObject]@{ Build = $latestBuild; CoversHead = $latestCoverage; BlocksRegardlessOfCurrency = $false }
}

function Invoke-InternalOfficialBuildProcess {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [ValidateRange(1, 600)][int]$TimeoutSeconds = 30,
        [AllowNull()][string]$WorkingDirectory
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    if (-not [string]::IsNullOrWhiteSpace($WorkingDirectory)) {
        if (-not [System.IO.Directory]::Exists($WorkingDirectory)) {
            return [PSCustomObject]@{
                Started = $false
                TimedOut = $false
                ExitCode = -1
                Stdout = ''
                Stderr = ''
            }
        }
        $startInfo.WorkingDirectory = $WorkingDirectory
    }
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardInput = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $timeoutMilliseconds = $TimeoutSeconds * 1000
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        if (-not $process.Start()) {
            return [PSCustomObject]@{
                Started = $false
                TimedOut = $false
                ExitCode = -1
                Stdout = ''
                Stderr = ''
            }
        }

        # Prevent extension-install or credential prompts from waiting on stdin.
        $process.StandardInput.Close()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($timeoutMilliseconds)) {
            try { $process.Kill($true) } catch { }
            [void]$process.WaitForExit(1000)
            return [PSCustomObject]@{
                Started = $true
                TimedOut = $true
                ExitCode = -1
                Stdout = if ($stdoutTask.IsCompletedSuccessfully) { $stdoutTask.GetAwaiter().GetResult() } else { '' }
                Stderr = if ($stderrTask.IsCompletedSuccessfully) { $stderrTask.GetAwaiter().GetResult() } else { '' }
            }
        }

        $remainingMilliseconds = [Math]::Max(
            0,
            $timeoutMilliseconds - [int][Math]::Ceiling($stopwatch.Elapsed.TotalMilliseconds))
        $outputTasks = [System.Threading.Tasks.Task[]]@($stdoutTask, $stderrTask)
        $outputCompleted = $stdoutTask.IsCompleted -and $stderrTask.IsCompleted
        if (-not $outputCompleted -and $remainingMilliseconds -gt 0) {
            $outputCompleted = [System.Threading.Tasks.Task]::WaitAll(
                $outputTasks,
                $remainingMilliseconds)
        }
        if (-not $outputCompleted) {
            return [PSCustomObject]@{
                Started = $true
                TimedOut = $true
                ExitCode = -1
                Stdout = if ($stdoutTask.IsCompletedSuccessfully) { $stdoutTask.GetAwaiter().GetResult() } else { '' }
                Stderr = if ($stderrTask.IsCompletedSuccessfully) { $stderrTask.GetAwaiter().GetResult() } else { '' }
            }
        }

        return [PSCustomObject]@{
            Started = $true
            TimedOut = $false
            ExitCode = $process.ExitCode
            Stdout = $stdoutTask.GetAwaiter().GetResult()
            Stderr = $stderrTask.GetAwaiter().GetResult()
        }
    } catch {
        return [PSCustomObject]@{
            Started = $false
            TimedOut = $false
            ExitCode = -1
            Stdout = ''
            Stderr = ''
        }
    } finally {
        $process.Dispose()
    }
}

function Resolve-InternalOfficialBuildCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Arguments,
        [AllowNull()]$CommandInfo,
        [bool]$Windows = $IsWindows,
        [AllowNull()][string]$CommandProcessor = $env:ComSpec
    )

    $resolved = $CommandInfo
    if ($null -eq $resolved) {
        $resolved = Get-Command $Name -ErrorAction SilentlyContinue
    }
    if ($null -eq $resolved) { return $null }

    $commandPath = [string](Get-InternalBuildProperty $resolved 'Source')
    if ([string]::IsNullOrWhiteSpace($commandPath)) {
        $commandPath = [string](Get-InternalBuildProperty $resolved 'Path')
    }
    if ([string]::IsNullOrWhiteSpace($commandPath)) {
        $commandPath = $Name
    }

    $extension = [System.IO.Path]::GetExtension($commandPath)
    if ($Windows -and $extension -in @('.cmd', '.bat')) {
        if ([string]::IsNullOrWhiteSpace($CommandProcessor)) { return $null }
        if (@($Arguments | Where-Object { $_ -match '[&|<>()^%!"\r\n]' }).Count -gt 0) {
            return $null
        }
        return [PSCustomObject]@{
            FileName = $CommandProcessor
            Arguments = @('/d', '/s', '/c', 'call', $commandPath) + @($Arguments)
        }
    }

    return [PSCustomObject]@{
        FileName = $commandPath
        Arguments = @($Arguments)
    }
}

function Get-InternalOfficialBuildAzArguments {
    param(
        [Parameter(Mandatory = $true)][string]$BranchRef,
        [Parameter(Mandatory = $true)][int]$DefinitionId,
        [Parameter(Mandatory = $true)][string]$Organization,
        [Parameter(Mandatory = $true)][string]$Project,
        [AllowNull()][string]$ManualBuildId,
        [AllowNull()][string]$ManualBuildBranchRef
    )

    if (-not [string]::IsNullOrWhiteSpace($ManualBuildId) -and $BranchRef -eq $ManualBuildBranchRef) {
        return @(
            'pipelines', 'runs', 'show',
            '--id', $ManualBuildId,
            '--org', "https://dev.azure.com/$Organization",
            '--project', $Project,
            '-o', 'json'
        )
    }

    return @(
        'pipelines', 'runs', 'list',
        '--pipeline-ids', "$DefinitionId",
        '--branch', $BranchRef,
        '--query-order', 'QueueTimeDesc',
        '--top', '5',
        '--org', "https://dev.azure.com/$Organization",
        '--project', $Project,
        '-o', 'json'
    )
}

function ConvertFrom-InternalOfficialBuildAzOutput {
    param(
        [AllowNull()][string]$Stdout,
        [AllowNull()][string]$Stderr,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][bool]$ManualQuery,
        [Parameter(Mandatory = $true)][int]$ExpectedDefinitionId
    )

    if ($ExitCode -ne 0) {
        $errorText = "$Stdout`n$Stderr"
        $failureKind = if ($errorText -match '(?i)TF400813|VS30063|unauthori[sz]ed|forbidden|401|403|not have permission|az login|sign in|authentication') {
            'access'
        } else {
            'query'
        }
        return [PSCustomObject]@{
            Success = $false
            FailureKind = $failureKind
            Message = "Azure DevOps query failed (exit $ExitCode)."
        }
    }

    try {
        $parsed = $Stdout | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return [PSCustomObject]@{
            Success = $false
            FailureKind = 'malformed'
            Message = 'Azure DevOps returned malformed JSON.'
        }
    }

    if ($ManualQuery) {
        $definition = Get-InternalBuildProperty $parsed 'definition'
        $actualDefinitionId = Get-InternalBuildProperty $definition 'id'
        if ($null -eq $actualDefinitionId -or [string]$actualDefinitionId -ne [string]$ExpectedDefinitionId) {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'definition-mismatch'
                Message = "Azure DevOps build does not belong to definition $ExpectedDefinitionId."
            }
        }
    }

    $builds = @($parsed)
    $build = Select-LatestInternalOfficialBuild -Builds $builds
    return [PSCustomObject]@{
        Success = $true
        Build = $build
        Builds = $builds
    }
}

function New-AzdoInternalOfficialBuildFetcher {
    param(
        [int]$DefinitionId = $Script:InternalOfficialBuildDefinitionId,
        [string]$Org = $Script:InternalOfficialBuildOrg,
        [string]$Project = $Script:InternalOfficialBuildProject,
        [AllowNull()][string]$ManualBuildId,
        [AllowNull()][string]$ManualBuildBranchRef,
        [ValidateRange(1, 600)][int]$TimeoutSeconds = 30,
        [AllowNull()][scriptblock]$ProcessInvoker
    )

    $definition = $DefinitionId
    $organization = $Org
    $projectName = $Project
    $manualId = $ManualBuildId
    $manualRef = $ManualBuildBranchRef
    $timeout = $TimeoutSeconds
    $checkCommandAvailability = $null -eq $ProcessInvoker
    $invokeProcess = if ($ProcessInvoker) {
        $ProcessInvoker
    } else {
        { param($FileName, $Arguments, $ProcessTimeout, $WorkingDirectory) Invoke-InternalOfficialBuildProcess -FileName $FileName -Arguments $Arguments -TimeoutSeconds $ProcessTimeout -WorkingDirectory $WorkingDirectory }
    }

    return {
        param([string]$BranchRef)

        $azCommandInfo = if ($checkCommandAvailability) {
            Get-Command az -ErrorAction SilentlyContinue
        } else {
            $null
        }
        if ($checkCommandAvailability -and $null -eq $azCommandInfo) {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'access'
                Message = 'Azure CLI is unavailable.'
            }
        }
        if (-not [string]::IsNullOrWhiteSpace($manualId) -and $manualId -notmatch '\A\d+\z') {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'malformed'
                Message = 'Internal build ID must contain only digits.'
            }
        }

        $manualQuery = -not [string]::IsNullOrWhiteSpace($manualId) -and $BranchRef -eq $manualRef
        $azArgs = Get-InternalOfficialBuildAzArguments `
            -BranchRef $BranchRef `
            -DefinitionId $definition `
            -Organization $organization `
            -Project $projectName `
            -ManualBuildId $manualId `
            -ManualBuildBranchRef $manualRef

        if ($checkCommandAvailability) {
            $azCommand = Resolve-InternalOfficialBuildCommand `
                -Name 'az' `
                -Arguments $azArgs `
                -CommandInfo $azCommandInfo
            if ($null -eq $azCommand) {
                return [PSCustomObject]@{
                    Success = $false
                    FailureKind = 'query'
                    Message = 'Azure CLI launcher could not be resolved.'
                }
            }
        } else {
            $azCommand = [PSCustomObject]@{ FileName = 'az'; Arguments = $azArgs }
        }

        $processResult = & $invokeProcess $azCommand.FileName $azCommand.Arguments $timeout $null
        if ([bool](Get-InternalBuildProperty $processResult 'TimedOut')) {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'timeout'
                Message = "Azure DevOps query timed out after $timeout seconds."
            }
        }
        if (-not [bool](Get-InternalBuildProperty $processResult 'Started')) {
            return [PSCustomObject]@{
                Success = $false
                FailureKind = 'query'
                Message = 'Azure CLI could not be started.'
            }
        }

        return ConvertFrom-InternalOfficialBuildAzOutput `
            -Stdout ([string](Get-InternalBuildProperty $processResult 'Stdout')) `
            -Stderr ([string](Get-InternalBuildProperty $processResult 'Stderr')) `
            -ExitCode ([int](Get-InternalBuildProperty $processResult 'ExitCode')) `
            -ManualQuery:$manualQuery `
            -ExpectedDefinitionId $definition
    }.GetNewClosure()
}

function New-GitHubBranchHeadFetcher {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [ValidateRange(1, 600)][int]$TimeoutSeconds = 30,
        [AllowNull()][scriptblock]$ProcessInvoker
    )

    $repo = $Repository
    $timeout = $TimeoutSeconds
    $checkCommandAvailability = $null -eq $ProcessInvoker
    $invokeProcess = if ($ProcessInvoker) {
        $ProcessInvoker
    } else {
        { param($FileName, $Arguments, $ProcessTimeout, $WorkingDirectory) Invoke-InternalOfficialBuildProcess -FileName $FileName -Arguments $Arguments -TimeoutSeconds $ProcessTimeout -WorkingDirectory $WorkingDirectory }
    }
    return {
        param([string]$BranchRef)

        if ($checkCommandAvailability -and -not (Get-Command gh -ErrorAction SilentlyContinue)) { return $null }
        $branchName = $BranchRef -replace '^refs/heads/', ''
        $encodedBranch = [System.Uri]::EscapeDataString($branchName)
        $ghArgs = @('api', "repos/$repo/commits/$encodedBranch", '--jq', '.sha')
        $processResult = & $invokeProcess 'gh' $ghArgs $timeout
        if (-not [bool](Get-InternalBuildProperty $processResult 'Started') -or
            [bool](Get-InternalBuildProperty $processResult 'TimedOut') -or
            [int](Get-InternalBuildProperty $processResult 'ExitCode') -ne 0) {
            return $null
        }
        $sha = ([string](Get-InternalBuildProperty $processResult 'Stdout')).Trim()
        if ([string]::IsNullOrWhiteSpace($sha)) { return $null }
        return $sha
    }.GetNewClosure()
}

function New-GitBuildCurrencyFetcher {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryPath,
        [ValidateRange(1, 600)][int]$TimeoutSeconds = 30,
        [AllowNull()][scriptblock]$ProcessInvoker
    )

    $repoPath = $RepositoryPath
    $timeout = $TimeoutSeconds
    $checkCommandAvailability = $null -eq $ProcessInvoker
    $invokeProcess = if ($ProcessInvoker) {
        $ProcessInvoker
    } else {
        { param($FileName, $Arguments, $ProcessTimeout, $WorkingDirectory) Invoke-InternalOfficialBuildProcess -FileName $FileName -Arguments $Arguments -TimeoutSeconds $ProcessTimeout -WorkingDirectory $WorkingDirectory }
    }
    return {
        param([string]$BranchRef, [string]$BuildSourceSha, [string]$BranchHeadSha)

        if ([string]::IsNullOrWhiteSpace($BuildSourceSha) -or
            [string]::IsNullOrWhiteSpace($BranchHeadSha)) {
            return $null
        }
        if ($BuildSourceSha.Equals($BranchHeadSha, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
        if ($checkCommandAvailability -and -not (Get-Command git -ErrorAction SilentlyContinue)) {
            return $null
        }

        $objectsAvailable = $true
        foreach ($sha in @($BuildSourceSha, $BranchHeadSha)) {
            $objectResult = & $invokeProcess 'git' @('cat-file', '-e', "$sha`^{commit}") $timeout $repoPath
            if (-not [bool](Get-InternalBuildProperty $objectResult 'Started') -or
                [bool](Get-InternalBuildProperty $objectResult 'TimedOut') -or
                [int](Get-InternalBuildProperty $objectResult 'ExitCode') -ne 0) {
                $objectsAvailable = $false
                break
            }
        }
        if (-not $objectsAvailable) {
            $fetchResult = & $invokeProcess 'git' @(
                'fetch',
                '--no-tags',
                '--quiet',
                'origin',
                $BranchRef
            ) $timeout $repoPath
            if (-not [bool](Get-InternalBuildProperty $fetchResult 'Started') -or
                [bool](Get-InternalBuildProperty $fetchResult 'TimedOut') -or
                [int](Get-InternalBuildProperty $fetchResult 'ExitCode') -ne 0) {
                return $null
            }
            foreach ($sha in @($BuildSourceSha, $BranchHeadSha)) {
                $objectResult = & $invokeProcess 'git' @('cat-file', '-e', "$sha`^{commit}") $timeout $repoPath
                if (-not [bool](Get-InternalBuildProperty $objectResult 'Started') -or
                    [bool](Get-InternalBuildProperty $objectResult 'TimedOut') -or
                    [int](Get-InternalBuildProperty $objectResult 'ExitCode') -ne 0) {
                    return $null
                }
            }
        }

        $ancestryResult = & $invokeProcess 'git' @('merge-base', '--is-ancestor', $BuildSourceSha, $BranchHeadSha) $timeout $repoPath
        if (-not [bool](Get-InternalBuildProperty $ancestryResult 'Started') -or
            [bool](Get-InternalBuildProperty $ancestryResult 'TimedOut')) {
            return $null
        }
        $ancestryExitCode = [int](Get-InternalBuildProperty $ancestryResult 'ExitCode')
        if ($ancestryExitCode -eq 1) { return $false }
        if ($ancestryExitCode -ne 0) { return $null }

        # Aggregate compare diffs can hide a trigger-eligible change that a
        # later commit reverted, so inspect the paths touched by every commit.
        $logResult = & $invokeProcess 'git' @(
            'log',
            '--format=',
            '--name-only',
            '--no-renames',
            '--first-parent',
            '--diff-merges=first-parent',
            "$BuildSourceSha..$BranchHeadSha"
        ) $timeout $repoPath
        if (-not [bool](Get-InternalBuildProperty $logResult 'Started') -or
            [bool](Get-InternalBuildProperty $logResult 'TimedOut') -or
            [int](Get-InternalBuildProperty $logResult 'ExitCode') -ne 0) {
            return $null
        }

        $paths = @(([string](Get-InternalBuildProperty $logResult 'Stdout')) -split '\r?\n')
        return Test-InternalOfficialBuildChangedPathsCoverHead $paths
    }.GetNewClosure()
}

function Get-InternalOfficialBuildHealth {
    param(
        [Parameter(Mandatory = $true)][int]$MajorVersion,
        [Parameter(Mandatory = $true)][string]$ReleaseBranch,
        [Parameter(Mandatory = $true)][bool]$ReleaseBranchExists,
        [Parameter(Mandatory = $true)][scriptblock]$BuildFetcher,
        [Parameter(Mandatory = $true)][scriptblock]$HeadFetcher,
        [AllowNull()][scriptblock]$BuildCurrencyFetcher,
        [bool]$GitHubActions = (Test-IsGitHubActions)
    )

    $branchRefs = @(Get-InternalOfficialBuildBranches `
        -MajorVersion $MajorVersion `
        -ReleaseBranch $ReleaseBranch `
        -ReleaseBranchExists $ReleaseBranchExists)

    if ($MajorVersion -ne 11) {
        return [PSCustomObject]@{
            overall = 'skipped'
            skipReason = 'unsupported-major'
            branches = @()
        }
    }
    if ($GitHubActions) {
        return [PSCustomObject]@{
            overall = 'skipped'
            skipReason = 'github-actions'
            branches = @()
        }
    }

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($branchRef in $branchRefs) {
        try {
            $fetchResult = & $BuildFetcher $branchRef
        } catch {
            $fetchResult = [PSCustomObject]@{
                Success = $false
                FailureKind = 'query'
                Message = 'Internal build fetcher threw an exception.'
            }
        }

        $success = [bool](Get-InternalBuildProperty $fetchResult 'Success')
        $failureKind = [string](Get-InternalBuildProperty $fetchResult 'FailureKind')
        if (-not $success -and $failureKind -eq 'access') {
            $branchName = $branchRef -replace '^refs/heads/', ''
            [void]$results.Add([PSCustomObject]@{
                branch = $branchName
                branchRef = $branchRef
                classification = 'unknown'
                reason = 'internal-auth-unavailable'
                headSha = $null
                build = $null
            })
            continue
        }

        $branchName = $branchRef -replace '^refs/heads/', ''
        if (-not $success) {
            [void]$results.Add([PSCustomObject]@{
                branch = $branchName
                branchRef = $branchRef
                classification = 'unknown'
                reason = if ($failureKind) { $failureKind } else { 'query' }
                headSha = $null
                build = $null
            })
            continue
        }

        $headSha = try { [string](& $HeadFetcher $branchRef) } catch { $null }
        $builds = @(Get-InternalBuildProperty $fetchResult 'Builds' | Where-Object { $null -ne $_ })
        if ($builds.Count -eq 0) {
            $fallbackBuild = Get-InternalBuildProperty $fetchResult 'Build'
            if ($null -ne $fallbackBuild) {
                $builds = @($fallbackBuild)
            }
        }
        $selection = Select-InternalOfficialBuildForHead `
            -Builds $builds `
            -BranchHeadSha $headSha `
            -BranchRef $branchRef `
            -BuildCurrencyFetcher $BuildCurrencyFetcher
        $build = $selection.Build
        $buildCoversHead = $selection.CoversHead
        $classification = Get-InternalOfficialBuildClassification `
            -Build $build `
            -ExpectedBranchRef $branchRef `
            -BranchHeadSha $headSha `
            -BuildCoversHead $buildCoversHead `
            -BlocksRegardlessOfCurrency ([bool](Get-InternalBuildProperty $selection 'BlocksRegardlessOfCurrency'))

        $buildId = Get-InternalBuildProperty $build 'id'
        $buildUrl = if ($buildId) {
            "https://dev.azure.com/$($Script:InternalOfficialBuildOrg)/$($Script:InternalOfficialBuildProject)/_build/results?buildId=$buildId"
        } else {
            [string](Get-InternalBuildProperty $build 'url')
        }

        [void]$results.Add([PSCustomObject]@{
            branch = $branchName
            branchRef = $branchRef
            classification = $classification.Classification
            reason = $classification.Reason
            headSha = $headSha
            build = if ($null -eq $build) {
                $null
            } else {
                [PSCustomObject]@{
                    id = $buildId
                    buildNumber = ConvertTo-SafeInternalBuildNumber (Get-InternalBuildProperty $build 'buildNumber')
                    status = Get-InternalBuildProperty $build 'status'
                    result = Get-InternalBuildProperty $build 'result'
                    sourceSha = Get-InternalBuildProperty $build 'sourceVersion'
                    url = $buildUrl
                }
            }
        })
    }

    $branchResults = @($results)
    $nonAuthResults = @($branchResults | Where-Object {
        [string](Get-InternalBuildProperty $_ 'reason') -ne 'internal-auth-unavailable'
    })
    if ($branchResults.Count -gt 0 -and $nonAuthResults.Count -eq 0) {
        return [PSCustomObject]@{
            overall = 'skipped'
            skipReason = 'internal-auth-unavailable'
            branches = @()
        }
    }

    return [PSCustomObject]@{
        overall = Get-InternalOfficialBuildOverallClassification -Branches $branchResults
        skipReason = $null
        branches = $branchResults
    }
}

function Convert-InternalOfficialBuildHealthToChecks {
    param(
        [Parameter(Mandatory = $true)]$Health,
        [Parameter(Mandatory = $true)][bool]$PublicSafe
    )

    $overall = [string](Get-InternalBuildProperty $Health 'overall')
    if ($PublicSafe) {
        $status = switch ($overall) {
            'green' { 'READY' }
            { $_ -in @('red', 'stale', 'failed-or-stale') } { 'BLOCKED' }
            { $_ -in @('in-progress', 'partial-success') } { 'WATCH' }
            default { 'UNKNOWN' }
        }
        $wasSkipped = $overall -eq 'skipped'
        return ,([PSCustomObject]@{
            Area = 'Internal release pipelines'
            Status = $status
            Details = if ($wasSkipped) {
                'Internal official-build status was not queried in this public-safe run.'
            } else {
                "Internal release pipeline status is $status."
            }
            NextAction = if ($wasSkipped) {
                'Run locally with authorized internal access for official-build evidence.'
            } elseif ($status -eq 'READY') {
                'No action needed.'
            } else {
                'Release owner should inspect the authorized internal release pipeline.'
            }
        })
    }

    if ($overall -eq 'skipped') { return @() }

    $checks = [System.Collections.Generic.List[object]]::new()
    foreach ($branch in @((Get-InternalBuildProperty $Health 'branches'))) {
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        $build = Get-InternalBuildProperty $branch 'build'
        $status = switch ($classification) {
            'green' { 'READY' }
            { $_ -in @('red', 'stale', 'failed-or-stale') } { 'BLOCKED' }
            { $_ -in @('in-progress', 'partial-success') } { 'WATCH' }
            default { 'UNKNOWN' }
        }
        $branchName = [string](Get-InternalBuildProperty $branch 'branch')
        $summary = switch ($classification) {
            'green' { 'The latest official build succeeded at current branch HEAD.' }
            'red' { 'The latest official build did not succeed at current branch HEAD.' }
            'stale' { 'The latest official build does not match current branch HEAD.' }
            'failed-or-stale' { 'The observed official build either failed/canceled while current or is stale; build currency could not be determined.' }
            'in-progress' { 'The latest official build has not completed.' }
            'partial-success' { 'The latest official build completed with issues and needs manual review.' }
            default { 'Official-build evidence could not be determined for this branch.' }
        }

        [void]$checks.Add([PSCustomObject]@{
            Area = "Internal official build ($branchName)"
            Status = $status
            Details = "$($classification.ToUpperInvariant()): $summary"
            NextAction = switch ($classification) {
                'green' { 'No action needed.' }
                'red' { 'Investigate and repair the failed official build before release.' }
                'stale' { 'Run the official pipeline at current branch HEAD before judging readiness.' }
                'failed-or-stale' { 'Restore build-currency evidence, then repair the failed build or run the official pipeline at current branch HEAD as indicated.' }
                'in-progress' { 'Wait for the current official build to complete.' }
                'partial-success' { 'Review the partially-succeeded official build legs before release.' }
                default { 'Verify internal access and confirm the latest official build manually.' }
            }
        })
    }
    return @($checks)
}

function Format-InternalOfficialBuildTable {
    param(
        [Parameter(Mandatory = $true)]$Health,
        [Parameter(Mandatory = $true)][bool]$PublicSafe
    )

    if ($PublicSafe) { return '' }

    $overall = [string](Get-InternalBuildProperty $Health 'overall')
    if ($overall -eq 'skipped') {
        $reason = [string](Get-InternalBuildProperty $Health 'skipReason')
        return "_Internal official-build query skipped: $reason._"
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    [void]$lines.Add('| Branch | Health | Build ID | Build number | Pipeline status/result | Source SHA | Build |')
    [void]$lines.Add('|--------|--------|----------|--------------|------------------------|------------|-------|')
    foreach ($branch in @((Get-InternalBuildProperty $Health 'branches'))) {
        $build = Get-InternalBuildProperty $branch 'build'
        $branchName = [string](Get-InternalBuildProperty $branch 'branch')
        $classification = [string](Get-InternalBuildProperty $branch 'classification')
        $id = Get-InternalBuildProperty $build 'id'
        $number = Get-InternalBuildProperty $build 'buildNumber'
        $status = Get-InternalBuildProperty $build 'status'
        $result = Get-InternalBuildProperty $build 'result'
        $sha = Get-InternalBuildProperty $build 'sourceSha'
        $url = Get-InternalBuildProperty $build 'url'
        $statusResult = if ($status -or $result) { "$status/$result" } else { '—' }
        $buildLink = if ($url) { "[open]($url)" } else { '—' }
        $branchCell = '`' + $branchName + '`'
        $shaCell = if ($sha) { '`' + $sha + '`' } else { '—' }
        [void]$lines.Add("| $branchCell | **$classification** | $(if ($id) { $id } else { '—' }) | $(if ($number) { $number } else { '—' }) | $statusResult | $shaCell | $buildLink |")
    }
    return $lines -join "`n"
}
