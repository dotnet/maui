#!/usr/bin/env bash
# Selects MAUI UI test categories using GitHub Copilot CLI and a custom pipeline agent.
# Exits successfully and emits Azure DevOps variables even when Copilot fails (falls back to ALL).

set -euo pipefail

log() {
  echo "[select-ui-test-categories] $*"
}

emit_variables() {
  local categories_json="$1"
  local reasoning="$2"
  local confidence="$3"
  local fallback="$4"
  # Persist for debugging
  mkdir -p "${BUILD_ARTIFACTSTAGINGDIRECTORY:-$(pwd)/artifacts}/copilot"
  local output_path="${BUILD_ARTIFACTSTAGINGDIRECTORY:-$(pwd)/artifacts}/copilot/selected-categories.json"
  printf '{"categories":%s,"reasoning":%s,"confidence":"%s","fallback":%s}\n' "$categories_json" "$(python - <<'PY'
import json
import os
reasoning = os.environ.get('COPILOT_REASON', '')
print(json.dumps(reasoning))
PY
)" "$confidence" "$fallback" >"$output_path"
  log "Wrote selection payload to $output_path"

  # Azure DevOps output variables (single line values)
  # Remove surrounding brackets when passing into condition checks
  local categories_csv
  categories_csv=$(python - <<'PY'
import json, os
cats = json.loads(os.environ['COPILOT_CATEGORIES_JSON'])
print(','.join(cats))
PY
)

  echo "##vso[task.setvariable variable=SelectedCategoryGroups;isOutput=true]$categories_csv"
  echo "##vso[task.setvariable variable=SelectionReason;isOutput=true]$reasoning"
  echo "##vso[task.setvariable variable=SelectionConfidence;isOutput=true]$confidence"
  echo "##vso[task.setvariable variable=SelectionFallbackTriggered;isOutput=true]$fallback"
}

fallback_all() {
  local message="$1"
  log "Falling back to ALL categories: $message"
  export COPILOT_CATEGORIES_JSON='["ALL"]'
  export COPILOT_REASON="$message"
  emit_variables "$COPILOT_CATEGORIES_JSON" "$COPILOT_REASON" "low" "true"
  exit 0
}

CI_PROVIDER=""
PR_BASE_SHA_OVERRIDE=""
PR_HEAD_SHA_OVERRIDE=""
CATEGORIES_JSON=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --ci-provider)
      CI_PROVIDER="$2"
      shift 2
      ;;
    --pr-base-sha)
      PR_BASE_SHA_OVERRIDE="$2"
      shift 2
      ;;
    --pr-head-sha)
      PR_HEAD_SHA_OVERRIDE="$2"
      shift 2
      ;;
    --use-categories-json)
      CATEGORIES_JSON="$2"
      shift 2
      ;;
    *)
      break
      ;;
  esac
done

DEFAULT_CATEGORIES=${DEFAULT_CATEGORY_GROUPS:-""}
if [[ -z "$DEFAULT_CATEGORIES" ]]; then
  DEFAULT_CATEGORIES="ALL"
fi

log "Default category groups: $DEFAULT_CATEGORIES"
BUILD_REASON_UPPER=$(echo "${BUILD_REASON:-}" | tr '[:lower:]' '[:upper:]')

if [[ -n "$CATEGORIES_JSON" ]]; then
  if [[ ! -f "$CATEGORIES_JSON" ]]; then
    fallback_all "Categories file $CATEGORIES_JSON not found"
  fi

  mapfile -t _parsed < <(CATEGORIES_JSON_PATH="$CATEGORIES_JSON" python - <<'PY'
import json
import os
import sys

path = os.environ['CATEGORIES_JSON_PATH']
with open(path, 'r', encoding='utf-8') as f:
    data = json.load(f)

cats = data.get('categories') or []
if not cats:
    cats = ['ALL']
    data['reasoning'] = data.get('reasoning') or 'Categories JSON missing categories'
    data['confidence'] = data.get('confidence') or 'low'
    data['fallback'] = True

print(json.dumps(cats))
print(data.get('reasoning', ''))
print(data.get('confidence', 'low'))
print('true' if data.get('fallback', False) else 'false')
PY
  )

  if [[ ${#_parsed[@]} -lt 4 ]]; then
    fallback_all "Categories JSON parse failure"
  fi

  export COPILOT_CATEGORIES_JSON="${_parsed[0]}"
  export COPILOT_REASON="${_parsed[1]}"
  confidence_value="${_parsed[2]}"
  fallback_value="${_parsed[3]}"

  emit_variables "$COPILOT_CATEGORIES_JSON" "$COPILOT_REASON" "$confidence_value" "$fallback_value"
  exit 0
fi

# Determine changed files for PR builds
changed_files=""

if [[ "${CI_PROVIDER,,}" == "github-actions" || "${GITHUB_ACTIONS:-}" == "true" ]]; then
  PR_BASE_SHA=${PR_BASE_SHA_OVERRIDE:-${PR_BASE_SHA:-${GITHUB_EVENT_PULL_REQUEST_BASE_SHA:-${GITHUB_BASE_SHA:-}}}}
  PR_HEAD_SHA=${PR_HEAD_SHA_OVERRIDE:-${PR_HEAD_SHA:-${GITHUB_EVENT_PULL_REQUEST_HEAD_SHA:-${GITHUB_HEAD_SHA:-HEAD}}}}
  PR_BASE_REF=${GITHUB_BASE_REF:-}

  if [[ -n "$PR_BASE_SHA" && -n "$PR_HEAD_SHA" ]]; then
    if ! git rev-parse "$PR_BASE_SHA" >/dev/null 2>&1; then
      if [[ -n "$PR_BASE_REF" ]]; then
        git fetch --depth=0 origin "$PR_BASE_REF" >/dev/null 2>&1 || git fetch origin "$PR_BASE_REF" || true
      fi
    fi
    changed_files=$(git diff --name-only "$PR_BASE_SHA" "$PR_HEAD_SHA" 2>/dev/null || true)
  fi

  if [[ -z "$changed_files" && -n "$PR_BASE_REF" ]]; then
    git fetch --depth=0 origin "$PR_BASE_REF" >/dev/null 2>&1 || git fetch origin "$PR_BASE_REF" || true
    changed_files=$(git diff --name-only "origin/${PR_BASE_REF#refs/heads/}" HEAD 2>/dev/null || true)
  fi

elif [[ "$BUILD_REASON_UPPER" == "PULLREQUEST" || -n "${SYSTEM_PULLREQUEST_PULLREQUESTID:-}" ]]; then
  BASE_REF=${SYSTEM_PULLREQUEST_TARGETBRANCH:-}
  if [[ -z "$BASE_REF" ]]; then
    fallback_all "Missing System.PullRequest.TargetBranch"
  fi
  # Normalize ref for git diff
  if [[ "$BASE_REF" != refs/* ]]; then
    BASE_REF="refs/heads/${BASE_REF#refs/heads/}"
  fi
  log "Fetching base ref $BASE_REF"
  git fetch --depth=0 origin "$BASE_REF" >/dev/null 2>&1 || git fetch origin "$BASE_REF"
  changed_files=$(git diff --name-only "origin/${BASE_REF#refs/heads/}" HEAD)
else
  log "Non-PR build. Defaulting to ALL categories."
  fallback_all "Non-PR build"
fi

if [[ -z "$changed_files" ]]; then
  fallback_all "No changed files detected"
fi

log "Changed files:\n$changed_files"

if ! command -v copilot >/dev/null 2>&1; then
  fallback_all "Copilot CLI not installed"
fi

TOKEN_VALUE="${GH_TOKEN:-${GITHUB_TOKEN:-}}"
if [[ -z "$TOKEN_VALUE" ]]; then
  fallback_all "GitHub token not supplied"
fi

# Detect unresolved Azure DevOps variables like "$(GitHubCopilotToken)"
if [[ "$TOKEN_VALUE" == '\$('* ]]; then
  fallback_all "GitHub token placeholder detected"
fi

CONFIG_ROOT=$(mktemp -d)
export XDG_CONFIG_HOME="$CONFIG_ROOT"
mkdir -p "$CONFIG_ROOT/copilot"
cat <<EOF >"$CONFIG_ROOT/copilot/config.json"
{ "trusted_folders": ["$(pwd)"], "telemetry": { "enabled": false } }
EOF
export COPILOT_DISABLE_TELEMETRY=1

PROMPT=$(cat <<'EOF'
You are the MAUI pipeline expert agent. You receive three inputs: (1) newline-separated changed file paths from the current pull request, (2) the canonical list of UI test category groups, and (3) optional hints. Decide which UI test category groups must run. Always respond with JSON in the schema described in your agent profile. Do not include Markdown or commentary.
EOF
)

PROMPT+=$'\n\nChanged files (one per line):\n'
PROMPT+="$changed_files"
PROMPT+=$'\n\nAvailable category groups (comma-separated):\n'
PROMPT+="$DEFAULT_CATEGORIES"

if [[ -n "${COPILOT_MINIMUM_CATEGORIES:-}" ]]; then
  PROMPT+=$'\n\nMinimum categories (must be included even if unrelated):\n'
  PROMPT+="${COPILOT_MINIMUM_CATEGORIES}"
fi

log "Invoking GitHub Copilot CLI"
set +e
COPILOT_RAW_OUTPUT=$(copilot --agent pipeline-expert --prompt "$PROMPT" 2>/tmp/copilot-cli-stderr.log)
status=$?
set -e

if [[ $status -ne 0 || -z "$COPILOT_RAW_OUTPUT" ]]; then
  log "Copilot CLI failed with status $status"
  if [[ -s /tmp/copilot-cli-stderr.log ]]; then
    log "Copilot stderr:" && cat /tmp/copilot-cli-stderr.log
  fi
  fallback_all "Copilot CLI invocation failed"
fi

log "Copilot raw output: $COPILOT_RAW_OUTPUT"

parse_script=$(cat <<'PY'
import json
import os
import sys
raw = os.environ.get('COPILOT_RAW', '').strip()
try:
    data = json.loads(raw)
    cats = data.get('categories')
    if not isinstance(cats, list) or not cats:
        raise ValueError('categories missing')
    cleaned = []
    for cat in cats:
        if isinstance(cat, str) and cat.strip():
            cleaned.append(cat.strip())
    if not cleaned:
        cleaned = ["ALL"]
        data["fallback"] = True
        data["confidence"] = data.get("confidence") or "low"
        data["reasoning"] = (data.get("reasoning") or "Copilot returned empty categories")
    data["categories"] = cleaned
except Exception as exc:
    sys.stderr.write(f"Failed to parse Copilot output: {exc}\n")
    sys.stderr.write(raw + "\n")
    print(json.dumps({
        "categories": ["ALL"],
        "reasoning": f"Parser failure: {exc}",
        "confidence": "low",
        "fallback": True
    }))
    sys.exit(1)
else:
    print(json.dumps(data))
    PARSED=$(RAW_OUTPUT="$COPILOT_RAW_OUTPUT" python - <<'PY'
    import json
    import os
    import sys

    raw = os.environ.get('RAW_OUTPUT', '').strip()
    try:
      data = json.loads(raw)
    except Exception as exc:
      result = {
        "categories": ["ALL"],
        "reasoning": f"JSON decode failure: {exc}",
        "confidence": "low",
        "fallback": True,
        "raw": raw,
      }
      print(json.dumps(result))
      sys.exit(1)

    cats = data.get("categories")
    if not isinstance(cats, list) or not cats:
      data["categories"] = ["ALL"]
      data["fallback"] = True
      data["confidence"] = data.get("confidence") or "low"
      data["reasoning"] = data.get("reasoning") or "Copilot returned empty categories"
    else:
      cleaned = [str(cat).strip() for cat in cats if isinstance(cat, str) and cat.strip()]
      if cleaned:
        data["categories"] = cleaned
      else:
        data["categories"] = ["ALL"]
        data["fallback"] = True
        data["confidence"] = data.get("confidence") or "low"
        data["reasoning"] = data.get("reasoning") or "Copilot returned empty categories"

    print(json.dumps(data))
    sys.exit(0)
    PY
    )
    PARSE_STATUS=$?

    if [[ $PARSE_STATUS -ne 0 || -z "$PARSED" ]]; then
      fallback_all "JSON parse failure"
    fi

    log "Copilot parsed output: $PARSED"

    export PARSED_JSON="$PARSED"
    export COPILOT_CATEGORIES_JSON=$(python - <<'PY'
    import json, os
    parsed = json.loads(os.environ['PARSED_JSON'])
    print(json.dumps(parsed['categories']))
    PY
    )

    export COPILOT_REASON=$(python - <<'PY'
    import json, os
    parsed = json.loads(os.environ['PARSED_JSON'])
    print(parsed.get('reasoning', ''))
    PY
    )

    COPILOT_CONFIDENCE=$(python - <<'PY'
    import json, os
    parsed = json.loads(os.environ['PARSED_JSON'])
    print(parsed.get('confidence', 'low'))
    PY
    )

    COPILOT_FALLBACK=$(python - <<'PY'
    import json, os
    parsed = json.loads(os.environ['PARSED_JSON'])
    print('true' if parsed.get('fallback', False) else 'false')
    PY
    )
