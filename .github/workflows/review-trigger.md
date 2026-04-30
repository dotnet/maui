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
  group: "review-trigger-${{ inputs.pr_number || github.run_id }}"
  cancel-in-progress: false

timeout-minutes: 150

steps:
  - name: Trigger AzDO pipeline and wait for results
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
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
            \"prNumber\": \"${PR_NUMBER}\"
          },
          \"resources\": {
            \"repositories\": {
              \"self\": {
                \"refName\": \"refs/heads/main\"
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

      # --- Download review artifact ---
      # The pipeline produces a JSON artifact with review comments.
      # TODO: Update ARTIFACT_NAME to match the actual artifact name from the pipeline
      ARTIFACT_NAME="review-results"

      ARTIFACTS_JSON=$(curl -sf \
        "https://dev.azure.com/${AZDO_ORG}/${AZDO_PROJECT}/_apis/build/builds/${RUN_ID}/artifacts?api-version=7.1" \
        -H "Authorization: Bearer ${AZDO_TOKEN}")

      ARTIFACT_URL=$(echo "${ARTIFACTS_JSON}" | jq -r \
        --arg name "$ARTIFACT_NAME" \
        '.value[] | select(.name == $name) | .resource.downloadUrl // empty')

      if [ -n "$ARTIFACT_URL" ]; then
        echo "📦 Downloading artifact: ${ARTIFACT_NAME}"
        curl -sf -o /tmp/pipeline-results/review-artifact.zip \
          -H "Authorization: Bearer ${AZDO_TOKEN}" \
          "${ARTIFACT_URL}"
        # Unzip to get the JSON
        cd /tmp/pipeline-results
        unzip -o review-artifact.zip -d artifact/ 2>/dev/null || true
        echo "✅ Artifact downloaded and extracted"
        ls -la artifact/ 2>/dev/null || true
      else
        echo "⚠️ Artifact '${ARTIFACT_NAME}' not found. Available artifacts:"
        echo "${ARTIFACTS_JSON}" | jq -r '.value[].name' 2>/dev/null || echo "(none)"
        # Still continue — the agent can report what's available
        echo "${ARTIFACTS_JSON}" > /tmp/pipeline-results/available-artifacts.json
      fi

      echo "📋 Pipeline results ready for agent analysis"
---

# DevDiv Review Trigger

You are a review results processor. A DevDiv AzDO pipeline has been triggered for
PR #${{ inputs.pr_number }} and the results are ready for you to analyze and post.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ inputs.pr_number }}
- **Triggered by**: ${{ github.actor }}

## Your Task

1. **Read the pipeline results** from `/tmp/pipeline-results/`:
   - `run-id.txt` — the AzDO build run ID
   - `pipeline-url.txt` — link to the AzDO build
   - `result.txt` — pipeline result (succeeded/failed/etc.)
   - `artifact/` — extracted review artifact (JSON with inline comments)
   - `available-artifacts.json` — list of available artifacts (if expected artifact wasn't found)

2. **Check the pipeline result**:
   - If `result.txt` says "succeeded", process the review artifact
   - If it says "failed", report the failure with the pipeline URL

3. **Process the review artifact** (when available):
   - Read the JSON file from `artifact/`
   - Parse the inline comments and review notes
   - Post a PR review using `submit-pull-request-review` with the inline comments
   - Post a summary comment using `add-comment` with an overview

4. **If no artifact was found**:
   - Check `available-artifacts.json` for what artifacts exist
   - Post a comment explaining the pipeline completed but no review artifact was found
   - Include the pipeline URL so maintainers can check manually

5. **If the pipeline failed**:
   - Post a comment with the failure status and pipeline URL

## Output Format

When posting the review summary via `add-comment`, use this format:

```markdown
## 🔍 DevDiv Pipeline Review — PR #<number>

**Pipeline Result:** ✅ Succeeded / ❌ Failed
**Pipeline Run:** [View in AzDO](<pipeline-url>)
**Triggered by:** @<actor>

### Review Summary
<summary of findings from the JSON artifact>

### Inline Comments
<count> inline comments posted as a PR review.

> 🔍 Review generated from DevDiv pipeline run.
```

## When No Action Is Needed

If there is truly nothing to report (e.g., pipeline result file is missing), call the `noop` tool:

```json
{"noop": {"message": "No pipeline results found to process"}}
```
