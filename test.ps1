$failedRecords = @(
    [ordered]@{ id = "1"; type = "Task"; result = "canceled"; name = "Task1" },
    [ordered]@{ id = "2"; type = "Phase"; result = "canceled"; name = "Phase1" }
)
$failedParentIds = @{}
foreach ($fr in $failedRecords) {
    if ([string]$fr.result -eq 'failed' -and $fr.parentId) { $failedParentIds[[string]$fr.parentId] = $true }
}
$allUnexplainedLegs = New-Object System.Collections.Generic.List[object]
foreach ($fr in $failedRecords) {
    if ([string]$fr.result -ne 'failed') { continue }
    if ([string]$fr.type -ne 'Task') { continue }
    $allUnexplainedLegs.Add($fr)
}
foreach ($fr in $failedRecords) {
    if ([string]$fr.result -ne 'failed') { continue }
    if ([string]$fr.type -eq 'Task') { continue }
    if ($failedParentIds.ContainsKey([string]$fr.id)) { continue }
    $allUnexplainedLegs.Add($fr)
}
$allUnexplainedLegs.Count
