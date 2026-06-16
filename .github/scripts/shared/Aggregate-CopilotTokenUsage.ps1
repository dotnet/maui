#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Aggregates Copilot CLI usage telemetry into publishable artifacts.

.DESCRIPTION
    Reads raw telemetry records emitted by Review-PR.ps1 and writes JSON,
    Markdown, CSV, and JSONL summaries. Missing input is treated as a valid
    no-usage report so the publishing stage can still produce artifacts after
    partial pipeline failures.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputDir,

    [Parameter(Mandatory = $false)]
    [string]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string[]]$ExpectedStages = @(
        'ReviewPR',
        'RunDeepUITests',
        'UpdateAISummaryComment',
        'AnalyzeCopilotTokenUsage'
    )
)

$ErrorActionPreference = 'Stop'

function Get-ObjectMemberValue {
    param(
        [object]$InputObject,
        [string[]]$Names
    )

    if ($null -eq $InputObject) { return $null }

    foreach ($name in $Names) {
        if ($InputObject -is [System.Collections.IDictionary] -and $InputObject.Contains($name)) {
            return $InputObject[$name]
        }

        $property = $InputObject.PSObject.Properties[$name]
        if ($property) {
            return $property.Value
        }
    }

    return $null
}

function Get-NestedValue {
    param(
        [object]$InputObject,
        [string[]]$Path
    )

    $current = $InputObject
    foreach ($segment in $Path) {
        $current = Get-ObjectMemberValue -InputObject $current -Names @($segment)
        if ($null -eq $current) { return $null }
    }

    return $current
}

function Get-NumericOrNull {
    param([object]$Value)

    if ($null -eq $Value) { return $null }
    if ($Value -is [byte] -or
        $Value -is [sbyte] -or
        $Value -is [int16] -or
        $Value -is [uint16] -or
        $Value -is [int] -or
        $Value -is [uint32] -or
        $Value -is [long] -or
        $Value -is [uint64] -or
        $Value -is [float] -or
        $Value -is [double] -or
        $Value -is [decimal]) {
        return [double]$Value
    }

    $parsed = 0.0
    if ([double]::TryParse([string]$Value,
            [System.Globalization.NumberStyles]::Float -bor [System.Globalization.NumberStyles]::AllowThousands,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$parsed)) {
        return $parsed
    }

    return $null
}

function Get-NullableSum {
    param([object[]]$Values)

    $hasValue = $false
    $sum = 0.0
    foreach ($value in @($Values)) {
        $numeric = Get-NumericOrNull -Value $value
        if ($null -ne $numeric) {
            $hasValue = $true
            $sum += $numeric
        }
    }

    if (-not $hasValue) { return $null }
    return [long][Math]::Round($sum)
}

function Get-NullableDecimalSum {
    param([object[]]$Values)

    $hasValue = $false
    $sum = 0.0
    foreach ($value in @($Values)) {
        $numeric = Get-NumericOrNull -Value $value
        if ($null -ne $numeric) {
            $hasValue = $true
            $sum += $numeric
        }
    }

    if (-not $hasValue) { return $null }
    return [Math]::Round($sum, 3)
}

function Get-RecordStageName {
    param([object]$Record)

    $stageName = [string](Get-NestedValue -InputObject $Record -Path @('pipeline', 'stageName'))
    if ([string]::IsNullOrWhiteSpace($stageName)) {
        return 'ReviewPR'
    }

    return $stageName
}

function Get-RecordTokenValue {
    param(
        [object]$Record,
        [string]$Name
    )

    return Get-NestedValue -InputObject $Record -Path @('normalizedTokens', $Name)
}

function Get-RecordAicUsed {
    param([object]$Record)

    return Get-NestedValue -InputObject $Record -Path @('cliUsage', 'aicUsed')
}

function Get-RecordCopilotCost {
    param([object]$Record)

    return Get-NestedValue -InputObject $Record -Path @('cliUsage', 'copilotCost')
}

function Get-RecordPremiumRequests {
    param([object]$Record)

    return Get-NestedValue -InputObject $Record -Path @('cliUsage', 'premiumRequests')
}

function Read-CopilotTokenUsageRecords {
    param([string]$Root)

    $records = New-Object System.Collections.ArrayList
    if ([string]::IsNullOrWhiteSpace($Root) -or -not (Test-Path $Root)) {
        return @()
    }

    # Security: the CopilotLogs artifact also bundles PR-worktree content (CustomAgentLogsTmp)
    # where PR-controlled steps can drop forged copilot-token-usage-*.json. Only trust records
    # under the pipeline-written 'copilot-token-usage/raw' subtree, and never those under
    # CustomAgentLogsTmp, so a forged file can't be aggregated and dispatched as official usage.
    $files = Get-ChildItem -Path $Root -Recurse -File -Filter 'copilot-token-usage-*.json' -ErrorAction SilentlyContinue |
        Where-Object {
            $normalized = $_.FullName -replace '\\', '/'
            $normalized -match '/copilot-token-usage/raw/' -and
            $normalized -notmatch '/CustomAgentLogsTmp/' -and
            $normalized -notmatch '/agent-pr-session/'
        } |
        Sort-Object FullName

    foreach ($file in @($files)) {
        try {
            $record = Get-Content -Path $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json -ErrorAction Stop
            $record | Add-Member -NotePropertyName sourceFile -NotePropertyValue $file.FullName -Force
            [void]$records.Add($record)
        } catch {
            Write-Warning "Skipping malformed token usage record '$($file.FullName)': $_"
        }
    }

    return @($records.ToArray())
}

function New-StageSummaryRows {
    param(
        [object[]]$Records,
        [string[]]$ExpectedStages
    )

    $stageNames = New-Object System.Collections.ArrayList
    foreach ($stage in @($ExpectedStages)) {
        if (-not [string]::IsNullOrWhiteSpace($stage) -and -not $stageNames.Contains($stage)) {
            [void]$stageNames.Add($stage)
        }
    }

    foreach ($record in @($Records)) {
        $stage = Get-RecordStageName -Record $record
        if (-not $stageNames.Contains($stage)) {
            [void]$stageNames.Add($stage)
        }
    }

    $rows = New-Object System.Collections.ArrayList
    foreach ($stage in @($stageNames.ToArray())) {
        $stageRecords = @($Records | Where-Object { (Get-RecordStageName -Record $_) -eq $stage })
        $hasRecords = $stageRecords.Count -gt 0
        [void]$rows.Add([pscustomobject][ordered]@{
            stageName         = $stage
            invocationCount   = $stageRecords.Count
            inputTokens       = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'inputTokens' }) } else { 0 }
            outputTokens      = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'outputTokens' }) } else { 0 }
            cachedInputTokens = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'cachedInputTokens' }) } else { 0 }
            reasoningOutputTokens = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'reasoningOutputTokens' }) } else { 0 }
            totalTokens       = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'totalTokens' }) } else { 0 }
            aicUsed           = if ($hasRecords) { Get-NullableDecimalSum -Values @($stageRecords | ForEach-Object { Get-RecordAicUsed -Record $_ }) } else { 0 }
            copilotCost       = if ($hasRecords) { Get-NullableDecimalSum -Values @($stageRecords | ForEach-Object { Get-RecordCopilotCost -Record $_ }) } else { 0 }
            premiumRequests   = if ($hasRecords) { Get-NullableDecimalSum -Values @($stageRecords | ForEach-Object { Get-RecordPremiumRequests -Record $_ }) } else { 0 }
            durationMs        = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { $_.durationMs }) } else { 0 }
            apiDurationMs     = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { $_.apiDurationMs }) } else { 0 }
            turnCount         = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { $_.turnCount }) } else { 0 }
            toolCount         = if ($hasRecords) { Get-NullableSum -Values @($stageRecords | ForEach-Object { $_.toolCount }) } else { 0 }
            note              = if ($hasRecords) { '' } else { 'No Copilot invocation observed in this stage.' }
        })
    }

    return @($rows.ToArray())
}

function New-StepSummaryRows {
    param([object[]]$Records)

    $groups = @{}
    foreach ($record in @($Records)) {
        $stage = Get-RecordStageName -Record $record
        $step = [string]$record.copilotStep
        $model = [string]$record.model
        $key = "$stage|$step|$model"
        if (-not $groups.ContainsKey($key)) {
            $groups[$key] = New-Object System.Collections.ArrayList
        }
        [void]$groups[$key].Add($record)
    }

    $rows = New-Object System.Collections.ArrayList
    foreach ($key in ($groups.Keys | Sort-Object)) {
        $items = @($groups[$key].ToArray())
        $first = $items[0]
        [void]$rows.Add([pscustomobject][ordered]@{
            stageName       = Get-RecordStageName -Record $first
            scriptPhase     = [string]$first.scriptPhase
            copilotStep     = [string]$first.copilotStep
            model           = [string]$first.model
            invocationCount = $items.Count
            inputTokens     = Get-NullableSum -Values @($items | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'inputTokens' })
            outputTokens    = Get-NullableSum -Values @($items | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'outputTokens' })
            cachedInputTokens = Get-NullableSum -Values @($items | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'cachedInputTokens' })
            reasoningOutputTokens = Get-NullableSum -Values @($items | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'reasoningOutputTokens' })
            totalTokens     = Get-NullableSum -Values @($items | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'totalTokens' })
            aicUsed         = Get-NullableDecimalSum -Values @($items | ForEach-Object { Get-RecordAicUsed -Record $_ })
            copilotCost     = Get-NullableDecimalSum -Values @($items | ForEach-Object { Get-RecordCopilotCost -Record $_ })
            premiumRequests = Get-NullableDecimalSum -Values @($items | ForEach-Object { Get-RecordPremiumRequests -Record $_ })
            durationMs      = Get-NullableSum -Values @($items | ForEach-Object { $_.durationMs })
            apiDurationMs   = Get-NullableSum -Values @($items | ForEach-Object { $_.apiDurationMs })
            turnCount       = Get-NullableSum -Values @($items | ForEach-Object { $_.turnCount })
            toolCount       = Get-NullableSum -Values @($items | ForEach-Object { $_.toolCount })
        })
    }

    return @($rows.ToArray())
}

function New-CopilotTokenUsageSummary {
    param(
        [object[]]$Records,
        [string[]]$ExpectedStages,
        [string]$PRNumber
    )

    $stageRows = @(New-StageSummaryRows -Records $Records -ExpectedStages $ExpectedStages)
    $stepRows = @(New-StepSummaryRows -Records $Records)

    return [ordered]@{
        schemaVersion         = 1
        generatedAtUtc        = ([DateTimeOffset]::UtcNow).ToString('o')
        prNumber              = $PRNumber
        costEstimateAvailable = $false
        costEstimateNote      = 'Dollar cost not calculated; no trusted rate table configured.'
        recordCount           = @($Records).Count
        expectedStages        = @($ExpectedStages)
        totals                = [ordered]@{
            invocationCount   = @($Records).Count
            inputTokens       = Get-NullableSum -Values @($Records | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'inputTokens' })
            outputTokens      = Get-NullableSum -Values @($Records | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'outputTokens' })
            cachedInputTokens = Get-NullableSum -Values @($Records | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'cachedInputTokens' })
            reasoningOutputTokens = Get-NullableSum -Values @($Records | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'reasoningOutputTokens' })
            totalTokens       = Get-NullableSum -Values @($Records | ForEach-Object { Get-RecordTokenValue -Record $_ -Name 'totalTokens' })
            aicUsed           = Get-NullableDecimalSum -Values @($Records | ForEach-Object { Get-RecordAicUsed -Record $_ })
            copilotCost       = Get-NullableDecimalSum -Values @($Records | ForEach-Object { Get-RecordCopilotCost -Record $_ })
            premiumRequests   = Get-NullableDecimalSum -Values @($Records | ForEach-Object { Get-RecordPremiumRequests -Record $_ })
            durationMs        = Get-NullableSum -Values @($Records | ForEach-Object { $_.durationMs })
            apiDurationMs     = Get-NullableSum -Values @($Records | ForEach-Object { $_.apiDurationMs })
            turnCount         = Get-NullableSum -Values @($Records | ForEach-Object { $_.turnCount })
            toolCount         = Get-NullableSum -Values @($Records | ForEach-Object { $_.toolCount })
        }
        stages                = @($stageRows)
        steps                 = @($stepRows)
    }
}

function Format-UsageValue {
    param([object]$Value)

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        return 'n/a'
    }

    return [string]$Value
}

function New-CopilotTokenUsageMarkdown {
    param([object]$Summary)

    $lines = New-Object System.Collections.ArrayList
    [void]$lines.Add('# Copilot token usage')
    [void]$lines.Add('')
    [void]$lines.Add("- PR: $(if ($Summary.prNumber) { $Summary.prNumber } else { 'n/a' })")
    [void]$lines.Add("- Records: $($Summary.recordCount)")
    [void]$lines.Add("- Cost estimate: not calculated (no trusted rate table configured)")
    [void]$lines.Add('')
    [void]$lines.Add('## Totals')
    [void]$lines.Add('')
    [void]$lines.Add('| Metric | Value |')
    [void]$lines.Add('|---|---:|')
    [void]$lines.Add("| Invocations | $($Summary.totals.invocationCount) |")
    [void]$lines.Add("| Input tokens | $(Format-UsageValue $Summary.totals.inputTokens) |")
    [void]$lines.Add("| Output tokens | $(Format-UsageValue $Summary.totals.outputTokens) |")
    [void]$lines.Add("| Cached input tokens | $(Format-UsageValue $Summary.totals.cachedInputTokens) |")
    [void]$lines.Add("| Reasoning output tokens | $(Format-UsageValue $Summary.totals.reasoningOutputTokens) |")
    [void]$lines.Add("| Total tokens | $(Format-UsageValue $Summary.totals.totalTokens) |")
    [void]$lines.Add("| AIC used | $(Format-UsageValue $Summary.totals.aicUsed) |")
    [void]$lines.Add("| Copilot cost (USD) | $(Format-UsageValue $Summary.totals.copilotCost) |")
    [void]$lines.Add("| Premium requests | $(Format-UsageValue $Summary.totals.premiumRequests) |")
    [void]$lines.Add("| Elapsed ms | $(Format-UsageValue $Summary.totals.durationMs) |")
    [void]$lines.Add("| API duration ms | $(Format-UsageValue $Summary.totals.apiDurationMs) |")
    [void]$lines.Add("| Turns | $(Format-UsageValue $Summary.totals.turnCount) |")
    [void]$lines.Add("| Tools | $(Format-UsageValue $Summary.totals.toolCount) |")
    [void]$lines.Add('')
    [void]$lines.Add('## By stage')
    [void]$lines.Add('')
    [void]$lines.Add('| Stage | Invocations | Input | Output | Cached input | Reasoning | Total | AIC used | Elapsed ms | API ms | Note |')
    [void]$lines.Add('|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|')
    foreach ($stage in @($Summary.stages)) {
        [void]$lines.Add("| $($stage.stageName) | $($stage.invocationCount) | $(Format-UsageValue $stage.inputTokens) | $(Format-UsageValue $stage.outputTokens) | $(Format-UsageValue $stage.cachedInputTokens) | $(Format-UsageValue $stage.reasoningOutputTokens) | $(Format-UsageValue $stage.totalTokens) | $(Format-UsageValue $stage.aicUsed) | $(Format-UsageValue $stage.durationMs) | $(Format-UsageValue $stage.apiDurationMs) | $($stage.note) |")
    }
    [void]$lines.Add('')
    [void]$lines.Add('## By Copilot step')
    [void]$lines.Add('')
    if (@($Summary.steps).Count -eq 0) {
        [void]$lines.Add('No Copilot invocations were recorded.')
    } else {
        [void]$lines.Add('| Stage | Phase | Step | Model | Invocations | Input | Output | Cached input | Reasoning | Total | AIC used | Elapsed ms | API ms |')
        [void]$lines.Add('|---|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|')
        foreach ($step in @($Summary.steps)) {
            [void]$lines.Add("| $($step.stageName) | $($step.scriptPhase) | $($step.copilotStep) | $($step.model) | $($step.invocationCount) | $(Format-UsageValue $step.inputTokens) | $(Format-UsageValue $step.outputTokens) | $(Format-UsageValue $step.cachedInputTokens) | $(Format-UsageValue $step.reasoningOutputTokens) | $(Format-UsageValue $step.totalTokens) | $(Format-UsageValue $step.aicUsed) | $(Format-UsageValue $step.durationMs) | $(Format-UsageValue $step.apiDurationMs) |")
        }
    }

    return ($lines -join [Environment]::NewLine) + [Environment]::NewLine
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$records = @(Read-CopilotTokenUsageRecords -Root $InputRoot)
$summary = New-CopilotTokenUsageSummary -Records $records -ExpectedStages $ExpectedStages -PRNumber $PRNumber

$rawJsonlPath = Join-Path $OutputDir 'token-usage-raw.jsonl'
if ($records.Count -gt 0) {
    $records | ForEach-Object { $_ | ConvertTo-Json -Depth 50 -Compress } |
        Set-Content -Path $rawJsonlPath -Encoding UTF8
} else {
    '' | Set-Content -Path $rawJsonlPath -Encoding UTF8
}

$summary | ConvertTo-Json -Depth 50 | Set-Content -Path (Join-Path $OutputDir 'token-usage-summary.json') -Encoding UTF8
New-CopilotTokenUsageMarkdown -Summary $summary | Set-Content -Path (Join-Path $OutputDir 'token-usage-summary.md') -Encoding UTF8

$csvPath = Join-Path $OutputDir 'token-usage-by-step.csv'
if (@($summary.steps).Count -gt 0) {
    @($summary.steps) | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8
} else {
    'stageName,scriptPhase,copilotStep,model,invocationCount,inputTokens,outputTokens,cachedInputTokens,reasoningOutputTokens,totalTokens,aicUsed,copilotCost,premiumRequests,durationMs,apiDurationMs,turnCount,toolCount' |
        Set-Content -Path $csvPath -Encoding UTF8
}

Write-Host "Copilot token usage records: $($summary.recordCount)"
Write-Host "Copilot token usage artifact directory: $OutputDir"
