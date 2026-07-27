#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates the net11 CI scanner's complete coverage manifest.

.DESCRIPTION
    This script is the fail-closed boundary between the scanner agent and GitHub
    issue writes. It does not call GitHub or interpret CI logs. It validates the
    single batched safe-output payload and writes a normalized plan for the
    downstream GitHub API step.
#>

$ErrorActionPreference = 'Stop'

$script:ConfiguredPipelines = @(
    [pscustomobject]@{ Name = 'maui-pr'; DefinitionId = 302 },
    [pscustomobject]@{ Name = 'maui-pr-devicetests'; DefinitionId = 314 },
    [pscustomobject]@{ Name = 'maui-pr-uitests'; DefinitionId = 313 }
)
$script:IssueCap = 5
$script:AllowedSkipReasons = @(
    'not-recurring',
    'not-actionable',
    'infrastructure-noise',
    'signature-not-in-fetched-log',
    'cap-reached'
)

function ConvertTo-TrimmedString {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return ([string]$Value).Trim()
}

function ConvertTo-SafeLogValue {
    param(
        [AllowNull()][object]$Value,
        [int]$MaxLength = 180
    )

    $safe = ([string]$Value) -replace '[\r\n]+', ' '
    $safe = $safe -replace '::', ': :'
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaxLength) {
        $safe = $safe.Substring(0, $MaxLength - 3) + '...'
    }

    return $safe
}

function Get-RequiredProperty {
    param(
        [Parameter(Mandatory = $true)][object]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $property = $Object.PSObject.Properties[$Name]
    if (-not $property -or $null -eq $property.Value) {
        throw "$Context is missing required property '$Name'."
    }

    return $property.Value
}

function ConvertTo-PositiveInteger {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $text = ConvertTo-TrimmedString $Value
    if ($text -notmatch '^[1-9]\d*$') {
        throw "$Context must be a positive integer."
    }

    return [Int64]$text
}

function Get-ScannerManifestFromAgentOutput {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Agent output '$Path' does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    $items = @($payload.items | Where-Object { $null -ne $_ })
    if ($items.Count -ne 1 -or $items[0].type -ne 'submit_ci_scan') {
        throw "Agent output must contain exactly one item of type submit_ci_scan and no alternate outputs."
    }

    $rawManifest = Get-RequiredProperty -Object $items[0] -Name 'manifest' -Context 'submit_ci_scan item'
    if ($rawManifest -is [string]) {
        if ([string]::IsNullOrWhiteSpace($rawManifest)) {
            throw 'submit_ci_scan manifest is empty.'
        }
        if ($rawManifest.Length -gt 500000) {
            throw 'submit_ci_scan manifest exceeds the 500000 character limit.'
        }
        return $rawManifest | ConvertFrom-Json
    }

    return $rawManifest
}

function Assert-ValidFingerprint {
    param(
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][string]$PipelineName
    )

    if ($Fingerprint.Length -gt 512) {
        throw 'Fingerprint exceeds 512 characters.'
    }
    if ($Fingerprint -cnotmatch '^[a-z0-9][a-z0-9 ._:/+()\-|]*$') {
        throw "Fingerprint contains non-normalized or unsafe characters."
    }

    $parts = @($Fingerprint.Split('|'))
    if ($parts.Count -ne 6 -or @($parts | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
        throw 'Fingerprint must contain exactly six non-empty pipe-delimited fields.'
    }
    if ($parts[0] -ne 'ci-scan-net11' -or $parts[1] -ne 'net11.0' -or $parts[2] -ne $PipelineName) {
        throw "Fingerprint does not match the net11 scanner and pipeline '$PipelineName'."
    }
}

function Assert-ValidIssuePayload {
    param(
        [Parameter(Mandatory = $true)][object]$Signature,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId,
        [Parameter(Mandatory = $true)][string]$Fingerprint
    )

    $title = ConvertTo-TrimmedString (Get-RequiredProperty -Object $Signature -Name 'title' -Context "filed signature '$Fingerprint'")
    if ($title.Length -lt 10 -or $title.Length -gt 180) {
        throw "Title for '$Fingerprint' must be 10-180 characters."
    }
    if ($title -cnotmatch '^[\x20-\x7E]+$' -or $title -match '[\r\n]') {
        throw "Title for '$Fingerprint' must contain printable single-line ASCII only."
    }
    if ($title -match '(?i)\[Content truncated due to length\]') {
        throw "Title for '$Fingerprint' contains the forbidden truncation placeholder."
    }
    if ($title.StartsWith('[ci-scan-net11] ', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Title for '$Fingerprint' must omit the prefix added by the publisher."
    }

    $body = [string](Get-RequiredProperty -Object $Signature -Name 'body' -Context "filed signature '$Fingerprint'")
    if ($body.Length -lt 20 -or $body.Length -gt 60000) {
        throw "Body for '$Fingerprint' must be 20-60000 characters."
    }
    if ($body -match '(?i)\[Content truncated due to length\]') {
        throw "Body for '$Fingerprint' contains the forbidden truncation placeholder."
    }

    $fingerprintPrefixCount = [regex]::Matches($body, '<!-- ci-scan-fingerprint:').Count
    $canonicalFingerprint = "<!-- ci-scan-fingerprint: $Fingerprint -->"
    $canonicalFingerprintCount = [regex]::Matches(
        $body,
        "(?m)^$([regex]::Escape($canonicalFingerprint))\r?$"
    ).Count
    if ($fingerprintPrefixCount -ne 1 -or $canonicalFingerprintCount -ne 1) {
        throw "Body for '$Fingerprint' must contain exactly one canonical fingerprint marker."
    }

    $matchPrefixCount = [regex]::Matches($body, '<!-- ci-scan-match-count:').Count
    $matchMarkers = [regex]::Matches(
        $body,
        '(?m)^<!-- ci-scan-match-count: ([1-9]\d*) hits in failure\.log -->\r?$'
    )
    if ($matchPrefixCount -ne 1 -or $matchMarkers.Count -ne 1) {
        throw "Body for '$Fingerprint' must contain exactly one canonical positive match-count marker."
    }

    $pipelineLine = "- **Pipeline**: $PipelineName"
    if ([regex]::Matches($body, "(?m)^$([regex]::Escape($pipelineLine))\r?$").Count -ne 1) {
        throw "Body for '$Fingerprint' must contain exactly one pipeline line for '$PipelineName'."
    }

    $buildMatches = [regex]::Matches($body, '(?m)^- \*\*Build ID\*\*: ([1-9]\d*)\r?$')
    if ($buildMatches.Count -ne 1 -or [Int64]$buildMatches[0].Groups[1].Value -ne $BuildId) {
        throw "Body for '$Fingerprint' must contain exactly one Build ID line matching $BuildId."
    }

    return [pscustomobject]@{
        Title      = $title
        FinalTitle = "[ci-scan-net11] $title"
        Body       = $body
        MatchCount = [Int64]$matchMarkers[0].Groups[1].Value
    }
}

function Test-CiScanManifest {
    param([Parameter(Mandatory = $true)][object]$Manifest)

    $pipelines = @(Get-RequiredProperty -Object $Manifest -Name 'pipelines' -Context 'manifest')
    if ($pipelines.Count -ne $script:ConfiguredPipelines.Count) {
        throw "Manifest must contain exactly $($script:ConfiguredPipelines.Count) pipelines."
    }

    $normalizedPipelines = [System.Collections.Generic.List[object]]::new()
    $issues = [System.Collections.Generic.List[object]]::new()
    $fingerprints = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $filedCount = 0
    $hasCapSkip = $false
    $signatureCount = 0

    for ($pipelineIndex = 0; $pipelineIndex -lt $script:ConfiguredPipelines.Count; $pipelineIndex++) {
        $expected = $script:ConfiguredPipelines[$pipelineIndex]
        $pipeline = $pipelines[$pipelineIndex]
        $context = "pipeline[$pipelineIndex]"
        $name = ConvertTo-TrimmedString (Get-RequiredProperty -Object $pipeline -Name 'name' -Context $context)
        $definitionId = ConvertTo-PositiveInteger `
            -Value (Get-RequiredProperty -Object $pipeline -Name 'definition_id' -Context $context) `
            -Context "$context definition_id"
        $status = (ConvertTo-TrimmedString (Get-RequiredProperty -Object $pipeline -Name 'status' -Context $context)).ToLowerInvariant()
        $signatures = @(Get-RequiredProperty -Object $pipeline -Name 'signatures' -Context $context)

        if ($name -ne $expected.Name -or $definitionId -ne $expected.DefinitionId) {
            throw "$context must be $($expected.Name) definition $($expected.DefinitionId) in configured order."
        }
        if ($status -notin @('scanned', 'skipped-no-recent-build', 'skipped-cap-reached')) {
            throw "$context has invalid status '$status'."
        }
        if ($filedCount -eq $script:IssueCap -and $status -ne 'skipped-cap-reached') {
            throw "$context must be skipped-cap-reached because the issue cap was already reached."
        }

        $buildId = $null
        if ($status -eq 'scanned') {
            $buildId = ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $pipeline -Name 'build_id' -Context $context) `
                -Context "$context build_id"
        } else {
            if ($signatures.Count -ne 0) {
                throw "$context with status '$status' must have an empty signatures array."
            }
            if ($status -eq 'skipped-cap-reached') {
                if ($filedCount -ne $script:IssueCap) {
                    throw "$context cannot be skipped-cap-reached before exactly $($script:IssueCap) issues are filed."
                }
                $hasCapSkip = $true
            }
        }

        $normalizedSignatures = [System.Collections.Generic.List[object]]::new()
        for ($signatureIndex = 0; $signatureIndex -lt $signatures.Count; $signatureIndex++) {
            $signatureCount++
            if ($signatureCount -gt 200) {
                throw 'Manifest exceeds the 200 signature safety limit.'
            }

            $signature = $signatures[$signatureIndex]
            $signatureContext = "$context signature[$signatureIndex]"
            $fingerprint = ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $signature -Name 'fingerprint' -Context $signatureContext
            )
            Assert-ValidFingerprint -Fingerprint $fingerprint -PipelineName $name
            if (-not $fingerprints.Add($fingerprint)) {
                throw "Duplicate fingerprint '$fingerprint' in manifest."
            }

            $disposition = (ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $signature -Name 'disposition' -Context $signatureContext
            )).ToLowerInvariant()
            if ($filedCount -eq $script:IssueCap -and $disposition -ne 'skipped') {
                throw "$signatureContext must be skipped with cap-reached because the issue cap was already reached."
            }

            $normalized = [ordered]@{
                fingerprint = $fingerprint
                disposition = $disposition
            }

            switch ($disposition) {
                'filed' {
                    $payload = Assert-ValidIssuePayload `
                        -Signature $signature `
                        -PipelineName $name `
                        -BuildId $buildId `
                        -Fingerprint $fingerprint
                    $filedCount++
                    $normalized.title = $payload.Title
                    $normalized.final_title = $payload.FinalTitle
                    $normalized.body = $payload.Body
                    $normalized.match_count = $payload.MatchCount
                    $issues.Add([pscustomobject]@{
                            Pipeline    = $name
                            BuildId     = $buildId
                            Fingerprint = $fingerprint
                            Title       = $payload.FinalTitle
                            Body        = $payload.Body
                            MatchCount  = $payload.MatchCount
                        })
                }
                'existing' {
                    $issueNumber = ConvertTo-PositiveInteger `
                        -Value (Get-RequiredProperty -Object $signature -Name 'issue_number' -Context $signatureContext) `
                        -Context "$signatureContext issue_number"
                    $normalized.issue_number = $issueNumber
                }
                'skipped' {
                    $skipReason = (ConvertTo-TrimmedString (
                        Get-RequiredProperty -Object $signature -Name 'skip_reason' -Context $signatureContext
                    )).ToLowerInvariant()
                    if ($skipReason -notin $script:AllowedSkipReasons) {
                        throw "$signatureContext has invalid skip_reason '$skipReason'."
                    }
                    if ($skipReason -eq 'cap-reached') {
                        if ($filedCount -ne $script:IssueCap) {
                            throw "$signatureContext cannot use cap-reached before exactly $($script:IssueCap) issues are filed."
                        }
                        $hasCapSkip = $true
                    } elseif ($filedCount -eq $script:IssueCap) {
                        throw "$signatureContext must use cap-reached because the issue cap was already reached."
                    }
                    $normalized.skip_reason = $skipReason
                }
                default {
                    throw "$signatureContext has invalid disposition '$disposition'."
                }
            }

            $normalizedSignatures.Add([pscustomobject]$normalized)
        }

        $normalizedPipelines.Add([pscustomobject]@{
                name          = $name
                definition_id = $definitionId
                status        = $status
                build_id      = $buildId
                signatures    = $normalizedSignatures.ToArray()
            })
    }

    if ($filedCount -gt $script:IssueCap) {
        throw "Manifest files $filedCount issues, exceeding the cap of $($script:IssueCap)."
    }
    if ($hasCapSkip -and $filedCount -ne $script:IssueCap) {
        throw "cap-reached may only be used when exactly $($script:IssueCap) issues are filed."
    }

    return [pscustomobject]@{
        schema_version = 1
        issue_cap      = $script:IssueCap
        filed_count    = $filedCount
        has_cap_skip   = $hasCapSkip
        pipelines      = $normalizedPipelines.ToArray()
        issues         = $issues.ToArray()
    }
}

function Write-CiScanPlan {
    param(
        [Parameter(Mandatory = $true)][object]$Plan,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $parent = Split-Path -Parent $Path
    if ($parent) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    ConvertTo-Json -InputObject $Plan -Depth 20 |
        Set-Content -LiteralPath $Path -Encoding utf8
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if (-not $env:GH_AW_AGENT_OUTPUT) {
    throw 'GH_AW_AGENT_OUTPUT is required.'
}
if (-not $env:CI_SCAN_PLAN_PATH) {
    throw 'CI_SCAN_PLAN_PATH is required.'
}

try {
    $manifest = Get-ScannerManifestFromAgentOutput -Path $env:GH_AW_AGENT_OUTPUT
    $plan = Test-CiScanManifest -Manifest $manifest
    Write-CiScanPlan -Plan $plan -Path $env:CI_SCAN_PLAN_PATH
    Write-Host "Validated complete coverage for $($plan.pipelines.Count) pipelines and $($plan.filed_count) issue payload(s)."
} catch {
    Write-Host "::error::CI scanner manifest rejected: $(ConvertTo-SafeLogValue $_)"
    throw
}
