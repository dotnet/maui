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
kusto` queries Kusto first, records its normalized projection, and falls back
to Azure because current Kusto coverage is always `partial` or `notIndexed`.
Only a future provider response with `taskInventory.coverage=complete` after
all pages may replace Azure timeline discovery.

## Local Kusto query

The adapter uses the public Arcade-documented employee data source
`engsrvprod/engineeringdata` ([data](https://github.com/dotnet/arcade/blob/main/Documentation/AzureDevOps/TestReportingData.md),
[queries](https://github.com/dotnet/arcade/blob/main/Documentation/AzureDevOps/TestReportingQueries.md))
and runs through the user's existing Azure CLI login:

```powershell
az login
pwsh -NoProfile -File .github/scripts/Get-CiFixAzdoEvidence.ps1 `
  -EvidenceProvider kusto `
  -CandidatesPath /tmp/gh-aw/agent/prefetch.json `
  -OutputDirectory /tmp/ci-fix-kusto
```

No token is written to disk or passed to the script. The script invokes
`az rest --resource https://api.kusto.windows.net` with a temporary query body
outside the checkout. Real evidence should remain outside every checkout.

The bounded query reads one build row from `TimelineBuilds` and at most 257
candidate task rows from `TimelineRecords`. Candidate rows require a task name,
positive log ID, and either a failed/partially-succeeded result or a positive
timeline error count. The normalized projection keeps:

- build ID, finish time, and Kusto indexing time;
- record ID, parent ID, task display name, result, and error count;
- attempt, timeline ID, hierarchy path, log ID, and Azure log URL.

Kusto currently does **not** provide an explicit complete-build marker, prior
timeline rows or `previousAttempts[].recordId` linkage, a stable
current-versus-previous classification, total/paged inventory counts, or an
independent test/Helix completeness state. `ingestion_time()` measures
freshness, not completeness. `TimelineRecords` also lacks organization and
project columns, so the adapter can validate the `TimelineBuilds` row for
`dnceng-public/public` but cannot prove record-level scoping if build IDs ever
collide across projects. Therefore an indexed empty result remains `partial`,
not complete.

## Measured exact-build results

Measured locally on 2026-10-28 with employee access. Raw query output and logs
were retained under `/tmp`, not committed.

| Public build | Azure truth | Kusto projection | Exact identity comparison | Indexing lag | Result |
|---|---:|---:|---|---:|---|
| `1650505` | 45 current failed tasks | 45 | 45/45 record ID + log ID + name + attempt matches | ~42m 45s | Kusto partial; Azure fallback downloads 32 logs and leaves 13 at the unchanged cap |
| `1626285` | Retained: 58 current + 64 previous-attempt tasks; live Azure timeline now 404 | 71 current-timeline candidates | 58/58 retained current failed-task matches; 13 additional error-bearing rows; 0/64 prior-task log IDs and none of 8 prior timeline IDs | ~49m 5s | Kusto cannot reconstruct prior-attempt provenance; Azure fallback remains incomplete because retention expired |
| `1650530` | 6 current failed tasks and all 6 Azure logs available | 6 | 6/6 exact matches | ~32m 2s | Azure task/log inventory completes, but underlying test/Helix evidence remains independently incomplete |
| `1626205` | Retained baseline: 10 result-failed tasks, no prior attempts | 28 current-timeline candidates | All 10 retained failed-task identities match; 18 additional error-bearing rows. Current Azure consumer semantics also select 28 | ~17m 42s | Kusto partial; Azure fallback remains authoritative |

The 32-log cap is deliberately unchanged. This prototype does not claim log
savings, Helix completeness, or a supported production Kusto dependency.

## Normalized provider contract

The manifest consumer now receives provider metadata independently from raw log
retrieval:

```json
{
  "requestedProvider": "kusto",
  "requestedCoverage": "complete | partial | notIndexed",
  "requestedStatus": "available | malformed | error | notIndexed",
  "usedProvider": "azure",
  "usedCoverage": "complete | partial",
  "fallbackUsed": true,
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

Only `taskInventory=complete` authorizes replacing Azure timeline discovery.
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
