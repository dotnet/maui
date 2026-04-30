---
description: >
  Triggers the DevDiv maui-copilot AzDO pipeline for a PR, waits for completion,
  downloads the review JSON artifact, and posts inline review comments.
  Uses OIDC federated credentials (no PAT).

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

      # --- Diagnostic: check secrets are available ---
      if [ -z "${AZDO_CLIENT_ID:-}" ]; then
        echo "::error::AZDO_TRIGGER_CLIENT_ID secret is empty — not configured for this repo/branch"
        exit 1
      fi
      if [ -z "${AZDO_TENANT_ID:-}" ]; then
        echo "::error::AZDO_TRIGGER_TENANT_ID secret is empty — not configured for this repo/branch"
        exit 1
      fi
      echo "✅ Secrets available (client_id length: ${#AZDO_CLIENT_ID}, tenant_id length: ${#AZDO_TENANT_ID})"

      # --- Helper: get a fresh AzDO bearer token via OIDC ---
      # All tokens stay in local variables; nothing written to GITHUB_OUTPUT
      get_azdo_token() {
        local oidc_token
        oidc_token=$(curl -sf \
          -H "Authorization: bearer ${ACTIONS_ID_TOKEN_REQUEST_TOKEN}" \
          "${ACTIONS_ID_TOKEN_REQUEST_URL}&audience=api://AzureADTokenExchange" \
          | jq -r '.value')
        if [ -z "$oidc_token" ] || [ "$oidc_token" = "null" ]; then
          echo "::error::Failed to get OIDC token from GitHub" >&2
          return 1
        fi
        echo "::add-mask::${oidc_token}"
        echo "✅ OIDC token obtained (length: ${#oidc_token})" >&2

        # Exchange OIDC token for AzDO bearer — capture full response for diagnostics
        local azure_response
        azure_response=$(curl -s -X POST \
          "https://login.microsoftonline.com/${AZDO_TENANT_ID}/oauth2/v2.0/token" \
          -d "grant_type=client_credentials" \
          -d "client_id=${AZDO_CLIENT_ID}" \
          -d "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer" \
          -d "client_assertion=${oidc_token}" \
          -d "scope=499b84ac-1321-427f-aa17-267ca6975798/.default")

        local bearer
        bearer=$(echo "$azure_response" | jq -r '.access_token // empty')
        if [ -z "$bearer" ]; then
          echo "::error::Azure AD token exchange failed" >&2
          # Log error details (strip any token values for safety)
          echo "Azure AD response (tokens redacted):" >&2
          echo "$azure_response" | jq 'del(.access_token, .token, .client_assertion) | .' 2>/dev/null >&2 || echo "$azure_response" >&2
          return 1
        fi
        echo "::add-mask::${bearer}"
        echo "✅ AzDO bearer obtained (length: ${#bearer})" >&2
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
      TOKEN_REFRESH_INTERVAL=2700
      RESULT="unknown"

      while [ $ELAPSED -lt $MAX_SECONDS ]; do
        sleep "${POLL_INTERVAL}"
        ELAPSED=$((ELAPSED + POLL_INTERVAL))
        TOKEN_AGE=$((TOKEN_AGE + POLL_INTERVAL))

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
      ARTIFACT_NAME="CopilotLogs"
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
| `inline-findings.json` | Structured inline review comments — array of `{path, line, body}` |
| `winner.json` | Final recommendation — `{schemaVersion, winner, isPRFix, summary, candidateDiff}` |
| `gate/content.md` | Test verification results |
| `pre-flight/content.md` | Context gathering |
| `pre-flight/code-review.md` | Deep code analysis |
| `expert-pr-eval/content.md` | Expert PR evaluation |
| `try-fix/content.md` | Fix exploration summary |
| `report/content.md` | Final recommendation report |

## Step 3: Post inline review comments

If `inline-findings.json` exists and contains entries, use `submit-pull-request-review` to
post them as inline PR review comments as a **COMMENT** review.

## Step 4: Post summary comment

Use `add-comment` to post ONE summary comment on the PR with phase content **verbatim**.
Only include sections for phases that have content files. Keep under 65000 characters.

## Step 5: Handle failures

If the pipeline failed, post a failure comment with the pipeline URL.
If no artifact was found, post a comment explaining pipeline completed but produced no review.

## When No Action Is Needed

If `/tmp/pipeline-results/` is empty or doesn't exist, call noop.
