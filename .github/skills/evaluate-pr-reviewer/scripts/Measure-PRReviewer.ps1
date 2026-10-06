#requires -Version 7.4
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$StatisticsPath,
    [Parameter(Mandatory)][string]$CorpusPath,
    [Parameter(Mandatory)][string]$OutputDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-Property($Object, [string]$Name) {
    if ($null -eq $Object) { return $null }
    if ($Object -is [Collections.IDictionary]) {
        if ($Object.Contains($Name)) { return $Object[$Name] }
        return $null
    }
    $property = $Object.PSObject.Properties[$Name]
    if ($property) { return $property.Value }
    return $null
}

function Get-Number($Value, [string]$Field) {
    if ($null -eq $Value) { return $null }
    $number = 0.0
    if ($Value -is [bool] -or -not [double]::TryParse(
        [string]$Value, [Globalization.NumberStyles]::Float,
        [Globalization.CultureInfo]::InvariantCulture, [ref]$number) -or
        -not [double]::IsFinite($number) -or $number -lt 0) {
        throw "Invalid nonnegative numeric telemetry field: $Field."
    }
    return $number
}

function Get-BuildId($Value) {
    $number = Get-Number $Value 'buildId'
    if ($null -eq $number -or $number -lt 1 -or $number -ne [Math]::Truncate($number) -or $number -gt [int]::MaxValue) {
        throw 'buildId must be a positive integer.'
    }
    return [int]$number
}

function Assert-RegularPath([string]$Path) {
    $current = [IO.Path]::GetFullPath($Path)
    while ($current) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'Inputs and outputs must not traverse symlinks or reparse points.'
            }
        }
        $current = Split-Path -Parent $current
    }
}

function Read-BoundedText([string]$Path, [long]$MaxBytes = 2MB) {
    Assert-RegularPath $Path
    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer -or $item.Length -gt $MaxBytes) {
        throw "Input must be a regular file no larger than $MaxBytes bytes."
    }
    return [IO.File]::ReadAllText($item.FullName)
}

function Resolve-ArtifactDirectory([string]$Root, [string]$RelativePath) {
    $relative = $RelativePath.Replace('\', [IO.Path]::DirectorySeparatorChar)
    if ([IO.Path]::IsPathRooted($relative)) { throw 'Artifact directory must be relative to the corpus.' }
    $path = [IO.Path]::GetFullPath([IO.Path]::Combine($Root, $relative))
    $prefix = [IO.Path]::TrimEndingDirectorySeparator($Root) + [IO.Path]::DirectorySeparatorChar
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    if (-not $path.StartsWith($prefix, $comparison)) { throw 'Artifact directory escapes the corpus directory.' }
    Assert-RegularPath $path
    if (-not (Test-Path -LiteralPath $path -PathType Container)) { throw 'Declared artifact directory does not exist.' }
    return $path
}

function Get-PhaseStatus([string]$Directory, [string[]]$Files, [string]$Coverage, [string]$Phase) {
    if (-not $Directory) { return 'unavailable' }
    foreach ($relative in $Files) {
        $path = Join-Path $Directory $relative.Replace('\', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $path)) { continue }
        $text = Read-BoundedText $path
        if ([string]::IsNullOrWhiteSpace($text)) { return 'empty' }
        if ($Phase -eq 'expert-review' -and $text -match '(?m)^#{1,3}\s+Code Review:\s*SKIPPED\s*$') {
            return 'skipped'
        }
        if ($Phase -eq 'try-fix' -and $text -match '(?m)^<!-- TRY-FIX-STATUS: not-requested -->\r?$') {
            return 'not-requested'
        }
        if ($Phase -eq 'report') {
            $first = @($text -split '\r?\n' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })[0]
            if ($first -notin @('## ✅ Final Recommendation: APPROVE', '## ⚠️ Final Recommendation: REQUEST CHANGES')) {
                return 'invalid'
            }
        }
        return 'present'
    }
    if ($Coverage -eq 'complete') { return 'missing' }
    return 'unavailable'
}

function Assert-Evidence($Evidence, [string]$HeadSha) {
    foreach ($item in $Evidence) {
        if ($item.purpose -in @('correctness', 'novelty', 'gold') -and $item.sourceSha -ine $HeadSha) {
            throw 'Correctness, novelty, and gold evidence must match the reviewed head; later revisions are uptake evidence only.'
        }
    }
}

function Test-IndependentEvidence($Evidence, [string]$Purpose, [string[]]$Kinds = @('code', 'test')) {
    return @($Evidence | Where-Object {
        $_.purpose -eq $Purpose -and $_.origin -eq 'independent' -and $_.kind -in $Kinds
    }).Count -gt 0
}

function Assert-Uptake($Item, $Sequencing) {
    if ($Item.uptake -eq 'unknown') { return }
    if (-not $Sequencing.uptakeJudgedAfterFindings) {
        throw 'Uptake must be judged after finding/candidate validity, not used to establish correctness.'
    }
    $kinds = if ($Item.uptake -eq 'code-verified') { @('code') } else { @('human') }
    if (-not (Test-IndependentEvidence $Item.evidence 'uptake' $kinds)) {
        throw 'Uptake requires separate independent evidence for its claimed purpose and kind.'
    }
}

function Measure-Quality($Assessment, $HeadSha, $Provenance) {
    if ($null -eq $Assessment) {
        return [ordered]@{ status = 'unassessed'; precision = $null; knownDefectRecall = $null; usefulDefects = $null; missedMajorDefects = $null }
    }
    if (-not $HeadSha -or $Assessment.reviewedHeadSha -ine $HeadSha) {
        throw 'Assessment must match the immutable reviewed PR head SHA, not the pipeline source revision.'
    }
    if (-not $Provenance) { throw 'An assessment requires run provenance.' }
    $sequence = $Assessment.sequencing
    $blinded = $sequence.mode -eq 'blinded'
    if ($blinded -and ($sequence.goldExposure -ne 'before-review-output' -or -not (Get-Property $sequence 'goldRecord') -or
        $Provenance.partition -eq 'retrospective')) {
        throw 'Blinded assessments require a recorded source-only gold pass before review output and a non-retrospective partition.'
    }
    if (-not $blinded -and $Provenance.partition -eq 'held-out') {
        throw 'Retrospective assessments cannot be held-out evidence.'
    }
    $ids = @{}
    foreach ($finding in $Assessment.findings) {
        if ($ids.ContainsKey($finding.id)) { throw 'Duplicate finding ID in an assessment.' }
        $ids[$finding.id] = $true
        Assert-Evidence $finding.evidence $HeadSha
        if ($finding.correctness -ne 'unknown' -and -not (Test-IndependentEvidence $finding.evidence 'correctness')) {
            throw 'Confirmed/refuted findings require independent reviewed-head code or test evidence; human agreement alone is insufficient.'
        }
        if ($finding.novelty -ne 'unknown' -and -not (Test-IndependentEvidence $finding.evidence 'novelty' @('code', 'test', 'prior-review'))) {
            throw 'Novelty requires a separate independent pre-stage evidence baseline.'
        }
        Assert-Uptake $finding $sequence
    }
    $knownIds = @{}
    foreach ($defect in $Assessment.knownDefects) {
        if ($knownIds.ContainsKey($defect.id)) { throw 'Duplicate known defect ID in an assessment.' }
        $knownIds[$defect.id] = $true
        Assert-Evidence $defect.evidence $HeadSha
        if (-not (Test-IndependentEvidence $defect.evidence 'gold')) {
            throw 'Known defects require independent code or test evidence.'
        }
    }
    $candidateIds = @{}
    foreach ($candidate in $Assessment.candidates) {
        if ($candidateIds.ContainsKey($candidate.id)) { throw 'Duplicate candidate ID in an assessment.' }
        $candidateIds[$candidate.id] = $true
        Assert-Evidence $candidate.evidence $HeadSha
        if ($candidate.implementation -ne 'present' -and $candidate.validity -ne 'unknown') {
            throw 'Independent candidate validity requires an actual retained implementation, not an empty or unknown proposal.'
        }
        if ($candidate.validity -ne 'unknown' -and -not (Test-IndependentEvidence $candidate.evidence 'correctness')) {
            throw 'Candidate validity requires independent reviewed-head code or test evidence, not a winner or self-reported test result.'
        }
        if ($candidate.reportedValidation -ne 'unknown' -and @($candidate.evidence | Where-Object purpose -eq 'reported-validation').Count -eq 0) {
            throw 'Reported candidate validation requires its retained log or output reference.'
        }
        Assert-Uptake $candidate $sequence
    }
    $confirmed = @($Assessment.findings | Where-Object correctness -eq 'confirmed')
    $refuted = @($Assessment.findings | Where-Object correctness -eq 'refuted')
    $useful = @($confirmed | Where-Object { $_.novelty -eq 'new' -and $_.actionability -eq 'actionable' })
    foreach ($defect in @($useful | Group-Object defectId)) {
        if (@($defect.Group | Select-Object -ExpandProperty stage -Unique).Count -gt 1) {
            throw 'A new useful defect cannot be credited to multiple stages; mark repeated findings as duplicate.'
        }
    }
    $detectedIds = @($confirmed | Select-Object -ExpandProperty defectId -Unique)
    $missed = @($Assessment.knownDefects | Where-Object { $_.id -notin $detectedIds })
    $assessedCount = $confirmed.Count + $refuted.Count
    $stages = [ordered]@{}
    foreach ($stage in @('gate', 'regression', 'expert-review', 'try-fix', 'report', 'metadata')) {
        $stages[$stage] = @($useful | Where-Object stage -eq $stage | Select-Object -ExpandProperty defectId -Unique).Count
    }
    return [ordered]@{
        status = if ($Assessment.complete) { 'assessed' } else { 'partial' }
        mode = $sequence.mode
        annotatedFindings = @($Assessment.findings).Count
        confirmedFindings = $confirmed.Count
        refutedFindings = $refuted.Count
        unknownFindings = @($Assessment.findings | Where-Object correctness -eq 'unknown').Count
        precision = if ($assessedCount) { $confirmed.Count / $assessedCount } else { $null }
        usefulDefects = @($useful | Select-Object -ExpandProperty defectId -Unique).Count
        unresolvedUsefulFindings = @($Assessment.findings | Where-Object {
            $_.correctness -ne 'refuted' -and $_.novelty -ne 'duplicate' -and $_.actionability -ne 'non-actionable' -and
            ($_.correctness -eq 'unknown' -or $_.novelty -eq 'unknown' -or $_.actionability -eq 'unknown')
        }).Count
        usefulDefectsByStage = $stages
        acknowledgedFindings = @($Assessment.findings | Where-Object uptake -eq 'acknowledged').Count
        codeVerifiedUptake = @($Assessment.findings | Where-Object uptake -eq 'code-verified').Count
        knownDefectCount = @($Assessment.knownDefects).Count
        knownDefectRecall = if ($blinded -and $Assessment.complete -and @($Assessment.knownDefects).Count) {
            1 - $missed.Count / @($Assessment.knownDefects).Count
        } else { $null }
        missedMajorDefects = if ($blinded -and $Assessment.complete -and @($Assessment.knownDefects).Count) {
            @($missed | Where-Object { $_.severity -in @('critical', 'major') }).Count
        } else { $null }
        candidates = [ordered]@{
            annotated = @($Assessment.candidates).Count
            supported = @($Assessment.candidates | Where-Object validity -eq 'supported').Count
            regressive = @($Assessment.candidates | Where-Object validity -eq 'regressive').Count
            invalid = @($Assessment.candidates | Where-Object validity -eq 'invalid').Count
            unknown = @($Assessment.candidates | Where-Object validity -eq 'unknown').Count
            codeVerifiedUptake = @($Assessment.candidates | Where-Object uptake -eq 'code-verified').Count
            acknowledged = @($Assessment.candidates | Where-Object uptake -eq 'acknowledged').Count
            rejected = @($Assessment.candidates | Where-Object uptake -eq 'rejected').Count
            unknownUptake = @($Assessment.candidates | Where-Object uptake -eq 'unknown').Count
            byStage = @(
                foreach ($stage in @('try-fix', 'expert-review')) {
                    $items = @($Assessment.candidates | Where-Object stage -eq $stage)
                    [ordered]@{
                        stage = $stage; annotated = $items.Count
                        supported = @($items | Where-Object validity -eq 'supported').Count
                        unknown = @($items | Where-Object validity -eq 'unknown').Count
                        implemented = @($items | Where-Object implementation -eq 'present').Count
                        empty = @($items | Where-Object implementation -eq 'empty').Count
                        reportedPass = @($items | Where-Object reportedValidation -eq 'pass').Count
                        reportedFail = @($items | Where-Object reportedValidation -eq 'fail').Count
                        reportedBlocked = @($items | Where-Object reportedValidation -eq 'blocked').Count
                    }
                }
            )
        }
    }
}

function Measure-StepMetric([object[]]$Steps, [string]$Name, [bool]$SingleInvocationRows = $false) {
    if ($null -eq $Steps) { $Steps = [object[]]::new(0) }
    $values = @($Steps | ForEach-Object { Get-Number (Get-Property $_ $Name) $Name })
    $known = @($values | Where-Object { $null -ne $_ })
    return [ordered]@{
        observedRecords = $known.Count
        expectedRecords = $Steps.Count
        observedSubtotal = if ($known.Count) { ($known | Measure-Object -Sum).Sum } else { $null }
        total = if ($SingleInvocationRows -and $Steps.Count -gt 0 -and $known.Count -eq $Steps.Count) { ($known | Measure-Object -Sum).Sum } else { $null }
    }
}

function Get-CompleteRunMetric($Run, [object[]]$Steps, $RecordCount, [string]$Name, [bool]$SingleInvocationRows) {
    if ($null -eq $Steps) { $Steps = [object[]]::new(0) }
    $reported = Get-Number (Get-Property $Run $Name) $Name
    $coverage = Measure-StepMetric $Steps $Name $SingleInvocationRows
    if (-not $SingleInvocationRows -or $null -eq $RecordCount -or $RecordCount -eq 0 -or
        $RecordCount -ne $Steps.Count -or $coverage.observedRecords -ne $RecordCount) {
        return $null
    }
    if ($null -eq $reported) { return $null }
    if ([Math]::Abs($reported - $coverage.total) -gt 0.01) {
        throw "Run and complete step telemetry disagree for $Name."
    }
    return $reported
}

$corpusText = Read-BoundedText $CorpusPath 16MB
$schema = Join-Path $PSScriptRoot '..\references\corpus.schema.json'
if (-not (Test-Json -Json $corpusText -SchemaFile $schema)) { throw 'Invalid corpus manifest.' }
$corpus = $corpusText | ConvertFrom-Json
$statistics = (Read-BoundedText $StatisticsPath 128MB) | ConvertFrom-Json
$runProperty = $statistics.PSObject.Properties['runs']
$stepProperty = $statistics.PSObject.Properties['steps']
if (-not $runProperty -or -not $stepProperty -or
    $runProperty.Value -isnot [array] -or $stepProperty.Value -isnot [array]) {
    throw 'Statistics must contain runs and steps arrays.'
}
$runsById = @{}
foreach ($run in $statistics.runs) {
    $id = Get-BuildId (Get-Property $run 'buildId')
    if ($runsById.ContainsKey($id)) { throw 'Duplicate build ID in statistics.' }
    $runsById[$id] = $run
}
$stepsById = @{}
foreach ($step in $statistics.steps) {
    $id = Get-BuildId (Get-Property $step 'buildId')
    if (-not $stepsById.ContainsKey($id)) { $stepsById[$id] = [Collections.Generic.List[object]]::new() }
    $stepsById[$id].Add($step)
}
$corpusRoot = Split-Path -Parent ([IO.Path]::GetFullPath($CorpusPath))
$seen = @{}
$results = @(foreach ($entry in $corpus.runs) {
    $id = Get-BuildId $entry.buildId
    if ($seen.ContainsKey($id)) { throw 'Duplicate build ID in corpus.' }
    $seen[$id] = $true
    if (-not $runsById.ContainsKey($id)) { throw "Corpus build $id is absent from statistics." }
    $run = $runsById[$id]
    $prValue = Get-Property $run 'pr'
    $pr = if ($null -ne $prValue) { Get-BuildId $prValue } else { $null }
    $platform = Get-Property $run 'platform'
    if ($null -ne $platform) {
        if ($platform -isnot [string] -or $platform -notin @('android', 'ios', 'catalyst', 'maccatalyst', 'windows')) {
            throw 'Unsupported platform in selected build metadata.'
        }
        $platform = $platform.ToLowerInvariant()
    }
    $pipelineSourceSha = Get-Property $run 'sourceVersion'
    if ($null -ne $pipelineSourceSha -and
        ($pipelineSourceSha -isnot [string] -or $pipelineSourceSha -notmatch '^[a-fA-F0-9]{40}$')) {
        throw 'Selected pipeline source revision must be a full SHA or null.'
    }
    $artifact = Get-Property $entry 'artifacts'
    $directory = if ($artifact) { Resolve-ArtifactDirectory $corpusRoot $artifact.directory } else { '' }
    $coverage = if ($artifact) { $artifact.coverage } else { 'unavailable' }
    $phaseFiles = [ordered]@{
        'pre-flight' = @('pre-flight\content.md')
        'expert-review' = @('expert-pr-eval\content.md', 'pre-flight\code-review.md')
        'try-fix' = @('try-fix\content.md')
        'report' = @('report\content.md')
    }
    $requiredPhases = @(Get-Property $entry 'requiredPhases' | Where-Object { $null -ne $_ })
    if ($requiredPhases.Count -eq 0) { $requiredPhases = @($phaseFiles.Keys) }
    $phases = [ordered]@{}
    foreach ($phase in $phaseFiles.Keys) {
        $phases[$phase] = if ($phase -in $requiredPhases) {
            Get-PhaseStatus $directory $phaseFiles[$phase] $coverage $phase
        } else { 'not-required' }
    }
    $delivery = if (@($phases.Values | Where-Object { $_ -notin @('present', 'not-required') }).Count -eq 0) { 'complete' }
        elseif (@($phases.Values | Where-Object { $_ -in @('missing', 'empty', 'invalid', 'skipped', 'not-requested') }).Count) { 'incomplete' }
        else { 'unknown' }
    $recordCount = Get-Number (Get-Property $run 'recordCount') 'recordCount'
    if ($null -ne $recordCount -and $recordCount -ne [Math]::Truncate($recordCount)) {
        throw 'recordCount must be an integer.'
    }
    $steps = @(if ($stepsById.ContainsKey($id)) { $stepsById[$id].ToArray() })
    $singleInvocationRows = $recordCount -gt 0 -and $recordCount -eq $steps.Count -and
        @($steps | Where-Object { (Get-Property $_ 'invocationCount') -gt 1 }).Count -eq 0
    $costByStep = @(
        foreach ($group in @($steps | Group-Object {
            if ($_.step -eq 'STEP 5a: PREFLIGHT CONTEXT') { 'pre-flight' }
            elseif ($_.step -match '^STEP 5a:') { 'try-fix' }
            elseif ($_.step -match '^STEP 5b:') { 'expert-review-and-comparison' }
            else { 'other' }
        })) {
            [ordered]@{
                stage = $group.Name
                totalTokens = Measure-StepMetric @($group.Group) 'totalTokens' $singleInvocationRows
                aicUsed = Measure-StepMetric @($group.Group) 'aicUsed' $singleInvocationRows
                invocationDurationMs = Measure-StepMetric @($group.Group) 'durationMs' $singleInvocationRows
            }
        }
    )
    [ordered]@{
        buildId = $id
        variant = $entry.variant
        pr = $pr
        platform = $platform
        reviewedHeadSha = $entry.reviewedHeadSha
        pipelineSourceSha = $pipelineSourceSha
        provenance = if (Get-Property $entry 'provenance') {
            [ordered]@{
                reviewerRevisionSha = $entry.provenance.reviewerRevisionSha
                models = $entry.provenance.models
                partition = $entry.provenance.partition
                reviewBaseSha = Get-Property $entry.provenance 'reviewBaseSha'
                assessor = Get-Property $entry.provenance 'assessor'
            }
        } else { $null }
        artifactCoverage = $coverage
        requiredPhases = $requiredPhases
        phases = $phases
        delivery = $delivery
        quality = Measure-Quality (Get-Property $entry 'assessment') $entry.reviewedHeadSha (Get-Property $entry 'provenance')
        cost = [ordered]@{
            recordCount = $recordCount
            totalTokens = Get-CompleteRunMetric $run $steps $recordCount 'totalTokens' $singleInvocationRows
            aicUsed = Get-CompleteRunMetric $run $steps $recordCount 'aicUsed' $singleInvocationRows
            pipelineDurationSeconds = Get-Number (Get-Property $run 'durationSeconds') 'durationSeconds'
            byStep = $costByStep
        }
    }
})

$output = [ordered]@{
    schemaVersion = 1
    runCount = $results.Count
    delivery = [ordered]@{
        complete = @($results | Where-Object delivery -eq 'complete').Count
        incomplete = @($results | Where-Object delivery -eq 'incomplete').Count
        unknown = @($results | Where-Object delivery -eq 'unknown').Count
    }
    assessedRuns = @($results | Where-Object { $_.quality.status -eq 'assessed' }).Count
    tokenCoverage = @($results | Where-Object { $null -ne $_.cost.totalTokens }).Count
    creditCoverage = @($results | Where-Object { $null -ne $_.cost.aicUsed }).Count
    runs = $results
}
$lines = [Collections.Generic.List[string]]::new()
$lines.Add('# PR reviewer grades')
$lines.Add('')
$lines.Add('Offline observations, not a verdict that any reviewer stage is safe to remove.')
$lines.Add("Runs: $($results.Count). Fully assessed: $($output.assessedRuns). Token coverage: $($output.tokenCoverage)/$($results.Count). Credit coverage: $($output.creditCoverage)/$($results.Count).")
$lines.Add('')
$lines.Add('| Build | Variant | Delivery | Quality | Confirmed useful defects | Precision (annotated) | Known-defect recall | Major misses | Tokens | AI credits |')
$lines.Add('|---|---|---|---|---|---|---|---|---|---|')
function Format-Metric($Value) {
    if ($null -eq $Value) { return 'unknown' }
    return ([Math]::Round([double]$Value, 4)).ToString([Globalization.CultureInfo]::InvariantCulture)
}
foreach ($row in $results) {
    $lines.Add("| $($row.buildId) | $($row.variant) | $($row.delivery) | $($row.quality.status) | $(Format-Metric $row.quality.usefulDefects) | $(Format-Metric $row.quality.precision) | $(Format-Metric $row.quality.knownDefectRecall) | $(Format-Metric $row.quality.missedMajorDefects) | $(Format-Metric $row.cost.totalTokens) | $(Format-Metric $row.cost.aicUsed) |")
}
$lines.Add('')
$lines.Add('Unknown telemetry is not zero cost. Unavailable downloads are not failed review phases. Step durations are summed invocation time, not pipeline wall time. Expert-step cost includes comparison/refinement/metadata, not just finding bugs.')
$lines.Add('Judgments are curator-supplied, not automatically proven by this script. Precision covers annotated findings only. Retrospective recall and major misses remain unknown. Candidate validity/uptake are separate from defect findings and reported validation; a winner or reported pass is not independent validity.')
$lines.Add('Confirmed useful defects is a lower bound when correctness/novelty/actionability remain unresolved; zero is not evidence of no useful review value. See unresolvedUsefulFindings in JSON.')
$lines.Add('')
$lines.Add('| Build | Annotated candidates | Supported | Unknown validity | Code-matched uptake | Unknown uptake |')
$lines.Add('|---|---|---|---|---|---|')
foreach ($row in $results) {
    $candidates = Get-Property $row.quality 'candidates'
    if ($null -ne $candidates) {
        $lines.Add("| $($row.buildId) | $($candidates.annotated) | $($candidates.supported) | $($candidates.unknown) | $($candidates.codeVerifiedUptake) | $($candidates.unknownUptake) |")
    }
}

$outputPath = [IO.Path]::GetFullPath($OutputDir)
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
$comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
if ($outputPath.Equals($repositoryRoot, $comparison) -or
    $outputPath.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, $comparison)) {
    throw 'Store private evaluation output outside the MAUI repository.'
}
Assert-RegularPath $outputPath
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
foreach ($name in @('reviewer-grades.json', 'reviewer-grades.md')) {
    $destination = Join-Path $outputPath $name
    if ($destination.Equals([IO.Path]::GetFullPath($CorpusPath), $comparison) -or
        $destination.Equals([IO.Path]::GetFullPath($StatisticsPath), $comparison)) {
        throw 'Output must not overwrite an input file.'
    }
    Assert-RegularPath $destination
}
$output | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $outputPath 'reviewer-grades.json') -Encoding utf8
$lines -join "`n" | Set-Content -LiteralPath (Join-Path $outputPath 'reviewer-grades.md') -Encoding utf8
Write-Host "Graded $($results.Count) builds; quality assessed for $($output.assessedRuns). Reports contain metrics only, not transcripts or evidence text."
