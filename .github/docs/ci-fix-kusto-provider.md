# Experimental CI-fix Kusto provider

> [!IMPORTANT]
> **EXPERIMENT - DO NOT MERGE.** This local-only provider exists to define and
> measure the exact-build contract that a future Build Insights MCP tool must
> implement. Production CI continues to use the Azure provider by default.

This prototype targets only the exact-build work in
`Get-CiFixBuildEvidence`: timeline discovery, failed-task identity,
current/previous attempt provenance, log pointers, and truthful completeness.
It does not change build selection, raw Azure log downloads, Helix fallback,
safety limits, or the separate CI-status recurrence/history path. The earlier
[PureWeen/maui#186](https://github.com/PureWeen/maui/pull/186) experiment is
requirements background, not the implementation target here.

## Current Azure operation

For each selected build, the default `azure` provider:

1. Downloads the current Azure DevOps timeline.
2. Finds failed, partially succeeded, and error-bearing task records.
3. Follows each `previousAttempts[]` reference to its exact prior timeline.
4. Resolves each prior failed task through its parent chain to the nearest
   unique referenced record.
5. Downloads raw logs by Azure log ID, subject to the existing 32-log MAUI
   default and the existing byte, build, retry, and producer-time limits.
6. Marks evidence incomplete for missing provenance, missing logs, malformed
   responses, unavailable prior timelines, or any limit.

The Kusto experiment does not replace those guarantees. `-EvidenceProvider
kusto` queries Kusto first and replaces Azure timeline discovery only when a
fail-closed certificate proves a completed, single-attempt public build with a
complete current hierarchy and a bounded exact failure inventory. Any retry,
ambiguous scope, stale or malformed ingestion, structural gap, conflicting
identity, or provider limit falls back to Azure. Raw log downloads continue to
use Azure in either path.

## Local Kusto query

The adapter uses the public Arcade-documented employee data source
`engsrvprod/engineeringdata` ([data](https://github.com/dotnet/arcade/blob/main/Documentation/AzureDevOps/TestReportingData.md),
[queries](https://github.com/dotnet/arcade/blob/main/Documentation/AzureDevOps/TestReportingQueries.md))
and runs through the user's existing Azure CLI login:

```powershell
az login
pwsh -NoProfile -File .github/scripts/Get-CiFixAzdoEvidence.ps1 `
  -EvidenceProvider kusto `
  -CandidatesPath <candidate-snapshot.json> `
  -OutputDirectory <evidence-directory>
```

No token or query payload is written to disk. The script invokes `az rest
--resource https://api.kusto.windows.net` through the user's existing Azure CLI
login. Real evidence should remain outside every checkout.

The bounded query reads the exact build rows from `TimelineBuilds` and all
hierarchy rows for that build ID from `TimelineRecords`. It deterministically
keeps the latest ingestion for each `(TimelineId, RecordId)` and rejects
conflicting variants at the latest ingestion time. The query computes its
certificate before returning at most 257 failed-task rows. Task records are
identified by their four-part hierarchy path, which preserves task rows such
as initialization/finalization records that do not have `TaskName`.

The certificate requires:

- exactly one `dnceng-public/public` build scope with completed failed status;
- one timeline, attempt `1` throughout, and no retry signal;
- a nonempty root/job/task hierarchy with valid parent links, unique paths, and
  no orphan `999.*` sentinel;
- no unfinished task or job records;
- valid record/timeline identities and exact public Azure log URLs;
- build and record ingestion timestamps at or after build completion;
- an exact failure count within the provider row limit, with unique record and
  log identities.

The normalized projection keeps:

- build ID, finish time, and Kusto indexing time;
- record ID, parent ID, task display name, result, and error count;
- attempt, timeline ID, hierarchy path, log ID, and Azure log URL.

The ingester writes each timeline-record batch as one queued in-memory stream.
That contract plus the hierarchy checks allows a completed single-attempt
inventory, including an empty failure inventory, to be certified. Kusto still
does **not** expose `previousAttempts[].recordId` linkage or independent
test/Helix completeness. Multi-attempt builds therefore remain `partial`, and
`ingestion_time()` is used only as one freshness condition rather than as a
standalone completeness claim.

## Measured exact-build results

Measured locally on 2026-10-28 with employee access. Raw query output and logs
were retained under `/tmp`, not committed.

| Public build | Azure truth | Kusto projection | Exact identity comparison | Indexing lag | Result |
|---|---:|---:|---|---:|---|
| `1650505` | 45 current failed tasks | 45 | 45/45 record ID + log ID matches | ~42m 45s | Inventory certified and Azure timeline skipped; overall evidence remains incomplete because the unchanged 32-log cap leaves 13 logs unfetched |
| `1626285` | Retained multi-attempt evidence; live Azure timeline now 404 | 71 current-timeline candidates | Not eligible for substitution | ~49m 5s | `MaxAttempt=2` forces deterministic Azure fallback, which remains incomplete after timeline retention |
| `1650530` | 6 current failed tasks and all 6 Azure logs available | 6 | 6/6 record ID + log ID matches | ~32m 2s | Inventory certified and all six raw logs downloaded; test/Helix completeness remains independent |
| `1626205` | 28 current failed/error-bearing tasks | 28 | 28/28 record ID + log ID matches | ~17m 42s | Inventory certified and Azure timeline skipped |

The 32-log cap is deliberately unchanged. This prototype does not claim log
savings, Helix completeness, or a supported production Kusto dependency.

## Normalized provider contract

The manifest consumer now receives provider metadata independently from raw log
retrieval:

```json
{
  "requestedProvider": "kusto",
  "requestedCoverage": "complete",
  "requestedStatus": "available",
  "usedProvider": "kusto",
  "usedCoverage": "complete | partial",
  "fallbackUsed": false,
  "indexedAt": "2026-10-27T13:21:56.4658131Z",
  "buildFinishTime": "2026-10-27T12:39:11.0000000Z",
  "truncated": false,
  "projectedFailedTaskCount": 45,
  "projectedFailedTasks": [
    { "recordId": "...", "logId": 7028, "attempt": 1 }
  ]
}
```

`projectedFailedTasks` uses the same normalized task shape consumed by raw log
download: record/name/attempt/log identity, source timeline/record/attempt, and
provider-specific result/path fields. A future MCP adapter can populate this
shape without changing manifest assembly, Azure log download, or
evidence-completeness rules.

Completeness must remain independent across:

- `taskInventory`
- `knownIssueAssociations`
- `testAndHelixEvidence`

Only a certified `taskInventory=complete` authorizes replacing Azure timeline
discovery.
Known-issue matches or test/Helix rows cannot imply a complete task inventory.

## Proposed Build Insights MCP tool

```text
get_exact_build_failure_inventory

request:
  organization, project, buildId
  pageSize, continuationToken
  snapshotToken

response:
  build:
    id, definitionId, status, result, finishTime
  freshness:
    indexedAt, snapshotToken
  taskInventory:
    coverage: complete | partial | notIndexed
    reason
    totalCount, returnedCount, continuationToken
  knownIssueAssociations:
    coverage: complete | partial | notIndexed
  testAndHelixEvidence:
    coverage: complete | partial | notIndexed
  failures[]:
    recordId, parentId, sourceTimelineId
    source: current | previous
    attempt
    previousReferenceRecordId, previousReferenceAttempt
    hierarchy[]: { recordId, name, attempt }
    name, taskIdentifier, result, issues[]
    logId, logUrl, stepUrl
    provenanceStatus
    analysisState: analyzed | notAnalyzed
    knownIssueAssociations[]
```

The service must page against a stable snapshot, preserve attempt distinctions,
return unmatched failures, and state truncation explicitly. Raw log bodies stay
as targeted Azure downloads. The current anonymous MCP gateway and known-issue
analysis APIs do not satisfy this contract, and CI workload identity remains a
separate deployment requirement.

## Swap from Kusto to MCP

1. Add an `mcp` adapter that returns the normalized provider inventory.
2. Run the same exact-identity parity cases and synthetic malformed/empty/limit
   cases.
3. Permit substitution only for `taskInventory.coverage=complete` with no
   continuation token remaining.
4. Preserve Azure fallback for `partial`, `notIndexed`, timeout, malformed, and
   authentication failures.
5. Keep Azure raw-log and Helix retrieval unchanged.
6. Remove the experimental Kusto adapter after MCP parity, paging, freshness,
   and CI authentication are proven.
