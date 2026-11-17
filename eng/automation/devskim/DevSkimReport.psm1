Set-StrictMode -Version Latest

# DevSkim 1.0.100 writes the rule tags on each result (properties.tags), not on the driver rules.
$script:BannedFunctionTag = 'API.DangerousAPI.BannedFunction'

function Get-JsonValue {
    # Follows a path of keys and indexes through parsed JSON. Emits nothing where the path ends early or the value is null.
    param ($Object, [object[]] $Path)

    foreach ($key in $Path) {
        if ($Object -is [System.Collections.IDictionary] -and $Object.Contains($key)) {
            $Object = $Object[$key]
        }
        elseif ($Object -is [System.Collections.IList] -and $key -is [int] -and $key -lt $Object.Count) {
            $Object = $Object[$key]
        }
        else {
            return
        }
    }

    if ($null -ne $Object) {
        # The comma keeps arrays intact instead of unrolling them into the pipeline.
        , $Object
    }
}

function Get-DevSkimBannedFindings {
    <#
    .SYNOPSIS
    Returns the results tagged API.DangerousAPI.BannedFunction in a DevSkim SARIF report.

    .DESCRIPTION
    Every severity counts, because the general banned-function rule is only Moderate, and so does a result
    that carries an inline suppression. Throws if the file isn't a complete DevSkim report, so that a failed
    scan can't be mistaken for a clean one.
    #>
    [CmdletBinding()]
    param ([Parameter(Mandatory)] [string] $Path)

    function Assert-Report ([bool] $Condition, [string] $Problem) {
        if (-not $Condition) {
            throw "Invalid DevSkim report '$Path': $Problem"
        }
    }

    try {
        $report = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    }
    catch {
        throw "Cannot read the DevSkim report '$Path': $($_.Exception.Message)"
    }

    Assert-Report ((Get-JsonValue $report 'version') -ceq '2.1.0') 'expected a SARIF 2.1.0 document.'
    $runs = Get-JsonValue $report 'runs'
    Assert-Report ($runs -is [System.Collections.IList] -and $runs.Count -gt 0) 'expected at least one run.'

    foreach ($run in $runs) {
        Assert-Report ((Get-JsonValue $run 'tool', 'driver', 'name') -ceq 'devskim') 'expected a DevSkim run.'

        $invocations = Get-JsonValue $run 'invocations'
        foreach ($invocation in $invocations) {
            Assert-Report ((Get-JsonValue $invocation 'executionSuccessful') -ne $false) 'DevSkim reported an unsuccessful execution.'
            $notifications = Get-JsonValue $invocation 'toolExecutionNotifications'
            foreach ($notification in $notifications) {
                Assert-Report ((Get-JsonValue $notification 'level') -cne 'error') 'DevSkim reported an execution error.'
            }
        }

        $results = Get-JsonValue $run 'results'
        Assert-Report ($results -is [System.Collections.IList]) 'expected a results array.'

        foreach ($result in $results) {
            $ruleId = Get-JsonValue $result 'ruleId'
            Assert-Report ($ruleId -is [string]) 'expected every result to have a ruleId.'
            $tags = Get-JsonValue $result 'properties', 'tags'
            Assert-Report ($tags -is [System.Collections.IList] -and @($tags | Where-Object { $_ -isnot [string] }).Count -eq 0) "expected the tags of $ruleId on the result."

            if ($tags -ccontains $script:BannedFunctionTag) {
                $location = Get-JsonValue $result 'locations', 0, 'physicalLocation'
                [pscustomobject]@{
                    RuleId  = $ruleId
                    Uri     = (Get-JsonValue $location 'artifactLocation', 'uri') ?? $Path
                    Line    = (Get-JsonValue $location 'region', 'startLine') ?? 1
                    Message = (Get-JsonValue $result 'message', 'text') ?? 'Banned API detected'
                }
            }
        }
    }
}

function Get-DevSkimAnalyzedFileCount {
    <#
    .SYNOPSIS
    Returns the number of files DevSkim analyzed, from the summary at the end of its Debug console log.
    #>
    [CmdletBinding()]
    param ([Parameter(Mandatory)] [string] $LogPath)

    $summary = Select-String -LiteralPath $LogPath -Pattern 'Files analyzed: (\d+)' | Select-Object -Last 1
    if (-not $summary) {
        throw "The DevSkim log '$LogPath' has no 'Files analyzed' summary. DevSkim must run with --console-verbosity Debug."
    }

    [int] $summary.Matches[0].Groups[1].Value
}

Export-ModuleMember -Function Get-DevSkimBannedFindings, Get-DevSkimAnalyzedFileCount
