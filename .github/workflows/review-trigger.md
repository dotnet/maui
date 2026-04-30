---
description: >
  Triggers the DevDiv maui-copilot AzDO pipeline for a PR, waits for completion,
  downloads the review JSON artifact, and posts inline review comments.
  Uses OIDC federated credentials (no PAT). See .github/docs/trigger-azdo-pipeline-setup.md.

on:
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to review'
        required: true
        type: number
      platform:
        description: 'Target platform for review'
        required: false
        type: choice
        options:
          - android
          - ios
          - catalyst
          - windows
        default: android
  # TODO: Add slash_command trigger once workflow_dispatch is validated
  # slash_command:
  #   name: review
  #   events: [pull_request_comment]
  roles: [admin, maintain, write]

permissions:
  id-token: write
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-sonnet-4.6

safe-outputs:
  submit-pull-request-review:
    allowed-events: [COMMENT]
    max: 1
  add-comment:
    max: 1
    target: "*"
    hide-older-comments: true
  noop:
    report-as-issue: false
  messages:
    footer: "> 🔍 *DevDiv review by [{workflow_name}]({run_url})*"
    run-started: "🚀 Triggering DevDiv review pipeline for this PR… [{workflow_name}]({run_url})"
    run-success: "✅ DevDiv review complete! [{workflow_name}]({run_url})"
    run-failure: "❌ DevDiv review failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "review-trigger-${{ inputs.pr_number || github.run_id }}-${{ inputs.platform || 'android' }}"
  cancel-in-progress: false

timeout-minutes: 150

steps:
  - name: Trigger AzDO pipeline and wait for results
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
      PLATFORM: ${{ inputs.platform || 'android' }}
      AZDO_CLIENT_ID: ${{ secrets.AZDO_TRIGGER_CLIENT_ID }}
      AZDO_TENANT_ID: ${{ secrets.AZDO_TRIGGER_TENANT_ID }}
      AZDO_ORG: DevDiv
      AZDO_PROJECT: DevDiv
      AZDO_PIPELINE_ID: "27723"
      POLL_INTERVAL: "60"
      MAX_POLL_MINUTES: "100"
    run: |
      set -euo pipefail

      # --- Validate PR ---
      PR_STATE=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json state --jq .state 2>&1) || {
        echo "::error::Failed to fetch PR #$PR_NUMBER: $PR_STATE"
        exit 1
      }
      if [ "$PR_STATE" != "OPEN" ]; then
        echo "::error::PR #$PR_NUMBER is $PR_STATE — must be open"
        exit 1
      fi
      echo "✅ PR #$PR_NUMBER is open"

      # --- Helper: get a fresh AzDO bearer token via OIDC ---
      # All tokens stay in local variables; nothing written to GITHUB_OUTPUT
      get_azdo_token() {
        local oidc_token
        oidc_token=$(curl -sf \
          -H "Authorization: bearer ${ACTIONS_ID_TOKEN_REQUEST_TOKEN}" \
          "${ACTIONS_ID_TOKEN_REQUEST_URL}&audience=api://AzureADTokenExchange" \
          | jq -r '.value')
        if [ -z "$oidc_token" ] || [ "$oidc_token" = "null" ]; then
          echo "::error::Failed to get OIDC token" >&2
          return 1
        fi
        echo "::add-mask::${oidc_token}"

        local bearer
        bearer=$(curl -sf -X POST \
          "https://login.microsoftonline.com/${AZDO_TENANT_ID}/oauth2/v2.0/token" \
          -d "grant_type=client_credentials" \
          -d "client_id=${AZDO_CLIENT_ID}" \
          -d "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer" \
          -d "client_assertion=${oidc_token}" \
          -d "scope=499b84ac-1321-427f-aa17-267ca6975798/.default" \
          | jq -r '.access_token')
        if [ -z "$bearer" ] || [ "$bearer" = "null" ]; then
          echo "::error::Failed to exchange OIDC token for AzDO bearer" >&2
          return 1
        fi
        echo "::add-mask::${bearer}"
        echo "$bearer"
      }

      # --- Trigger pipeline ---
      AZDO_TOKEN=$(get_azdo_token)
      echo "🔑 OIDC exchange successful"

      TRIGGER_RESPONSE=$(curl -sf -w "\n%{http_code}" \
        -X POST "https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_apis/pipelines/${AZDO_PIPELINE_ID}/runs?api-version=7.1" \
        -H "Authorization: Bearer ${AZDO_TOKEN}" \
        -H "Content-Type: application/json" \
        -d "{
          \"templateParameters\": {
            \"PRNumber\": \"${PR_NUMBER}\",
            \"Platform\": \"${PLATFORM}\"
          },
          \"resources\": {
            \"repositories\": {
              \"self\": {
                \"refName\": \"refs/heads/feature/expert-reviewer-extraction\"
              }
            }
          }
        }")

      HTTP_CODE=$(echo "${TRIGGER_RESPONSE}" | tail -1)
      TRIGGER_BODY=$(echo "${TRIGGER_RESPONSE}" | head -n -1)

      if [ "${HTTP_CODE}" -lt 200 ] || [ "${HTTP_CODE}" -ge 300 ]; then
        echo "::error::Failed to trigger pipeline (HTTP ${HTTP_CODE})"
        # Redact any token-like values from error output
        echo "${TRIGGER_BODY}" | jq 'del(.access_token, .token)' 2>/dev/null || true
        exit 1
      fi

      RUN_ID=$(echo "${TRIGGER_BODY}" | jq -r '.id')
      PIPELINE_URL="https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_build/results?buildId=${RUN_ID}"
      echo "🚀 Pipeline triggered — Run ID: ${RUN_ID}"
      echo "   ${PIPELINE_URL}"

      # Write metadata for the agent (no tokens!)
      mkdir -p /tmp/pipeline-results
      echo "${RUN_ID}" > /tmp/pipeline-results/run-id.txt
      echo "${PIPELINE_URL}" > /tmp/pipeline-results/pipeline-url.txt
      echo "${PR_NUMBER}" > /tmp/pipeline-results/pr-number.txt

      # --- Poll until pipeline completes ---
      MAX_SECONDS=$((MAX_POLL_MINUTES * 60))
      ELAPSED=0
      TOKEN_AGE=0
      TOKEN_REFRESH_INTERVAL=2700  # refresh AzDO token every 45 min
      RESULT="unknown"

      while [ $ELAPSED -lt $MAX_SECONDS ]; do
        sleep "${POLL_INTERVAL}"
        ELAPSED=$((ELAPSED + POLL_INTERVAL))
        TOKEN_AGE=$((TOKEN_AGE + POLL_INTERVAL))

        # Refresh AzDO token before it expires
        if [ $TOKEN_AGE -ge $TOKEN_REFRESH_INTERVAL ]; then
          echo "🔄 Refreshing AzDO token…"
          AZDO_TOKEN=$(get_azdo_token)
          TOKEN_AGE=0
        fi

        RUN_STATUS=$(curl -sf \
          "https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_apis/pipelines/${AZDO_PIPELINE_ID}/runs/${RUN_ID}?api-version=7.1" \
          -H "Authorization: Bearer ${AZDO_TOKEN}" \
          | jq -r '.state')

        echo "⏳ [$((ELAPSED/60))m] Pipeline state: ${RUN_STATUS}"

        if [ "$RUN_STATUS" = "completed" ]; then
          RESULT=$(curl -sf \
            "https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_apis/pipelines/${AZDO_PIPELINE_ID}/runs/${RUN_ID}?api-version=7.1" \
            -H "Authorization: Bearer ${AZDO_TOKEN}" \
            | jq -r '.result')
          echo "✅ Pipeline completed with result: ${RESULT}"
          break
        fi

        if [ "$RUN_STATUS" = "canceling" ] || [ "$RUN_STATUS" = "canceled" ]; then
          echo "::error::Pipeline was canceled"
          echo "canceled" > /tmp/pipeline-results/result.txt
          exit 1
        fi
      done

      if [ "$RUN_STATUS" != "completed" ]; then
        echo "::error::Pipeline timed out after ${MAX_POLL_MINUTES} minutes (state: ${RUN_STATUS})"
        exit 1
      fi

      echo "${RESULT}" > /tmp/pipeline-results/result.txt
      echo "${PLATFORM}" > /tmp/pipeline-results/platform.txt

      # --- Download review artifacts ---
      # The pipeline publishes a 'CopilotLogs' artifact containing:
      #   - copilot_review_output.md (main review output)
      #   - Review_Feedback_*.md files
      #   - agent-pr-session/ directory with session data
      #   - copilot-session-state/ directory
      ARTIFACT_NAME="CopilotLogs"

      # AzDO build artifacts API uses the build ID (same as run ID for pipelines)
      ARTIFACTS_JSON=$(curl -sf \
        "https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_apis/build/builds/${RUN_ID}/artifacts?api-version=7.1" \
        -H "Authorization: Bearer ${AZDO_TOKEN}")

      ARTIFACT_URL=$(echo "${ARTIFACTS_JSON}" | jq -r \
        --arg name "$ARTIFACT_NAME" \
        '.value[] | select(.name == $name) | .resource.downloadUrl // empty')

      if [ -n "$ARTIFACT_URL" ]; then
        echo "📦 Downloading artifact: ${ARTIFACT_NAME}"
        curl -sf -o /tmp/pipeline-results/copilot-logs.zip \
          -H "Authorization: Bearer ${AZDO_TOKEN}" \
          "${ARTIFACT_URL}"
        cd /tmp/pipeline-results
        unzip -o copilot-logs.zip -d copilot-logs/ 2>/dev/null || true
        echo "✅ Artifact downloaded and extracted"
        # The artifact extracts flat (no CopilotLogs/ prefix)
        # Phase files at: copilot-logs/CustomAgentLogsTmp/PRState/<PR>/PRAgent/
        find copilot-logs/ -type f \( -name "*.json" -o -name "content.md" -o -name "code-review.md" -o -name "*.diff" \) | head -30
      else
        echo "⚠️ Artifact '${ARTIFACT_NAME}' not found. Available artifacts:"
        echo "${ARTIFACTS_JSON}" | jq -r '.value[].name' 2>/dev/null || echo "(none)"
        echo "${ARTIFACTS_JSON}" > /tmp/pipeline-results/available-artifacts.json
      fi

      echo "📋 Pipeline results ready for agent analysis"
---

# DevDiv Review Trigger

You are a review results processor. The DevDiv `maui-copilot` AzDO pipeline was triggered
for PR #${{ inputs.pr_number }} on platform **${{ inputs.platform || 'android' }}** and
results are in `/tmp/pipeline-results/`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ inputs.pr_number }}
- **Platform**: ${{ inputs.platform || 'android' }}
- **Triggered by**: ${{ github.actor }}

## Step 1: Read pipeline metadata

Read these files from `/tmp/pipeline-results/`:
- `result.txt` — pipeline result (succeeded/failed/canceled)
- `pipeline-url.txt` — link to the AzDO build
- `platform.txt` — target platform

If `result.txt` says "failed" or is missing, skip to **Step 5 (failure)**.

## Step 2: Find the review artifacts

The `CopilotLogs` artifact is extracted to `/tmp/pipeline-results/copilot-logs/`.
The artifact extracts **flat** (no `CopilotLogs/` prefix wrapper).

Phase content files are at:
```
copilot-logs/CustomAgentLogsTmp/PRState/${{ inputs.pr_number }}/PRAgent/
```

Use `find /tmp/pipeline-results/copilot-logs/ -type f \( -name "*.json" -o -name "content.md" -o -name "code-review.md" \)` to discover what's available.

### Key files

| File | Purpose |
|------|---------|
| `inline-findings.json` | **Structured inline review comments** — array of `{path, line, body}` objects for posting as inline PR review comments |
| `winner.json` | **Final recommendation** — `{schemaVersion, winner, isPRFix, summary, candidateDiff}` |
| `gate/content.md` | Test verification (gate) results |
| `pre-flight/content.md` | Context gathering & validation |
| `pre-flight/code-review.md` | Deep code analysis |
| `expert-pr-eval/content.md` | Expert PR evaluation |
| `try-fix/content.md` | Fix exploration summary |
| `try-fix/attempt-N/approach.md` | Individual fix attempt approaches |
| `try-fix/attempt-N/fix.diff` | Individual fix attempt diffs |
| `try-fix/attempt-N/analysis.md` | Individual fix attempt analysis |
| `report/content.md` | Final recommendation report |

**Not all files will exist** — phases may be skipped depending on the PR.

## Step 3: Post inline review comments

If `inline-findings.json` exists and contains entries, use `submit-pull-request-review` to
post them as inline PR review comments. The JSON format is:

```json
[
  {"path": "src/File.cs", "line": 42, "body": "**[severity] Category** — Description..."},
  ...
]
```

Post these as a **COMMENT** review (not approve or request changes).

## Step 4: Post summary comment

Use `add-comment` to post ONE summary comment on the PR. Build it from the phase files:

```markdown
## 🤖 AI Review — PR #<number> (<platform>)

**Pipeline:** [View run](<pipeline-url>) | **Platform:** <platform> | **Triggered by:** @<actor>

---

### 📋 Recommendation

<Read winner.json — include the `summary` field verbatim>
<If `isPRFix` is false and `candidateDiff` exists, include the diff in a collapsed details block>

### 🛡️ Gate — Test Verification
<content from gate/content.md — verbatim>

### 🔍 Pre-Flight
<content from pre-flight/content.md — verbatim>

### 🔬 Code Review
<content from pre-flight/code-review.md — verbatim>

### 🧪 Expert Evaluation
<content from expert-pr-eval/content.md — verbatim>

### 🔧 Fix Analysis
<content from try-fix/content.md — verbatim>

### 📋 Final Report
<content from report/content.md — verbatim>

---
> 🤖 *Review generated by DevDiv `maui-copilot` pipeline.*
```

**Rules:**
- Only include sections for phases that have content files
- Include the phase content **verbatim** — do NOT summarize or rewrite it
- If a phase file is empty or missing, skip that section
- Keep the comment under 65000 characters (GitHub limit). If too long,
  truncate try-fix attempt details first, then the transcript log

## Step 5: Handle failures

If the pipeline **failed**, post a comment:

```markdown
## ❌ AI Review Failed — PR #<number> (<platform>)

The DevDiv `maui-copilot` pipeline failed for this PR.

**Pipeline:** [View run](<pipeline-url>)
**Platform:** <platform>
**Triggered by:** @<actor>

Check the pipeline logs for details.
```

If **no artifact** was found (`available-artifacts.json` exists):
- Post a comment explaining the pipeline completed but produced no review artifact
- Include the pipeline URL

## When No Action Is Needed

If `/tmp/pipeline-results/` is empty or doesn't exist, call noop:

```json
{"noop": {"message": "No pipeline results found to process"}}
```
