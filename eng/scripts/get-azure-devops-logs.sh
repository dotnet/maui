#!/usr/bin/env bash
# Download Azure DevOps build logs to logs/azure-devops/<buildId>/
#
# Usage:
#   eng/scripts/get-azure-devops-logs.sh [--details-url URL | --build-id ID --org ORG --project PROJECT | --check-run-json FILE]
#                                        [--output-dir DIR]
#
# Env:
#   AZURE_DEVOPS_TOKEN | AZDO_PAT   Azure DevOps PAT (required)
#
# Notes:
# - Creates per-log files and a combined.log under the output dir
# - If --check-run-json is used, jq is required to parse details_url/html_url
set -euo pipefail

usage() {
  cat <<EOF
Usage: $0 [--details-url URL | --build-id ID --org ORG --project PROJECT | --check-run-json FILE] [--output-dir DIR]

Download Azure DevOps build logs under logs/azure-devops/<buildId>/.

Options:
  --details-url URL       Azure DevOps build details URL (dev.azure.com or *.visualstudio.com)
  --build-id ID           Azure DevOps build ID (requires --org and --project)
  --org ORG               Azure DevOps organization
  --project PROJECT       Azure DevOps project
  --check-run-json FILE   Check run JSON file containing details_url/html_url (requires jq)
  --output-dir DIR        Output directory base (default: logs)
  -h, --help              Show this help
EOF
}

DETAILS_URL=""
BUILD_ID=""
ORG=""
PROJECT=""
CHECK_RUN_JSON=""
OUTPUT_DIR="logs"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --details-url)
      DETAILS_URL="${2:-}"; shift 2;;
    --build-id)
      BUILD_ID="${2:-}"; shift 2;;
    --org)
      ORG="${2:-}"; shift 2;;
    --project)
      PROJECT="${2:-}"; shift 2;;
    --check-run-json)
      CHECK_RUN_JSON="${2:-}"; shift 2;;
    --output-dir)
      OUTPUT_DIR="${2:-}"; shift 2;;
    -h|--help)
      usage; exit 0;;
    *)
      echo "Unknown argument: $1" >&2; usage; exit 2;;
  esac
done

ado_pat="${AZURE_DEVOPS_TOKEN:-${AZDO_PAT:-}}"
if [[ -z "$ado_pat" ]]; then
  echo "Error: AZURE_DEVOPS_TOKEN/AZDO_PAT not set" >&2
  exit 2
fi

# If JSON provided, parse link with jq
if [[ -n "$CHECK_RUN_JSON" ]]; then
  if ! command -v jq >/dev/null 2>&1; then
    echo "Error: jq is required when using --check-run-json" >&2
    exit 2
  fi
  if [[ ! -f "$CHECK_RUN_JSON" ]]; then
    echo "Error: file not found: $CHECK_RUN_JSON" >&2
    exit 2
  fi
  run_json="$(cat "$CHECK_RUN_JSON")"
  DETAILS_URL=$(jq -r '.details_url // .html_url // empty' <<<"$run_json")
fi

# Derive org/project/buildId from DETAILS_URL if provided
if [[ -n "$DETAILS_URL" ]]; then
  if [[ "$DETAILS_URL" == *"dev.azure.com"* ]]; then
    ORG="${ORG:-$(echo "$DETAILS_URL" | sed -nE 's#https?://dev\.azure\.com/([^/]+)/.*#\1#p')}"
    PROJECT="${PROJECT:-$(echo "$DETAILS_URL" | sed -nE 's#https?://dev\.azure\.com/[^/]+/([^/]+)/.*#\1#p')}"
    BUILD_ID="${BUILD_ID:-$(echo "$DETAILS_URL" | grep -oE 'buildId=[0-9]+' | head -n1 | cut -d= -f2 || true)}"
  elif [[ "$DETAILS_URL" == *"visualstudio.com"* ]]; then
    ORG="${ORG:-$(echo "$DETAILS_URL" | sed -nE 's#https?://([^./]+)\.visualstudio\.com/.*#\1#p')}"
    PROJECT="${PROJECT:-$(echo "$DETAILS_URL" | sed -nE 's#https?://[^./]+\.visualstudio\.com/([^/]+)/.*#\1#p')}"
    BUILD_ID="${BUILD_ID:-$(echo "$DETAILS_URL" | grep -oE 'buildId=[0-9]+' | head -n1 | cut -d= -f2 || true)}"
  fi
fi

if [[ -z "$BUILD_ID" || -z "$ORG" || -z "$PROJECT" ]]; then
  echo "Error: missing org/project/buildId. Provide --details-url or --build-id with --org and --project." >&2
  exit 2
fi

b64=$(printf ':%s' "$ado_pat" | base64 | tr -d '\n')
authHeader=( -H "Authorization: Basic $b64" )
apiBase="https://dev.azure.com/$ORG/$PROJECT/_apis/build/builds/$BUILD_ID"
apiVer="api-version=7.0"

logsDir="$OUTPUT_DIR/azure-devops/$BUILD_ID"
mkdir -p "$logsDir"

echo "Fetching Azure DevOps logs list for $ORG/$PROJECT build $BUILD_ID"
logsList=$(curl -sS "${authHeader[@]}" -H "Accept: application/json" "$apiBase/logs?$apiVer")
echo "$logsList" > "$logsDir/logs-list.json"

ids=""
if command -v jq >/dev/null 2>&1; then
  ids=$(jq -r '.value[].id' <<<"$logsList")
else
  echo "Warning: jq not found; attempting to parse IDs from JSON"
  ids=$(grep -oE '"id"\s*:\s*[0-9]+' "$logsDir/logs-list.json" | grep -oE '[0-9]+')
fi

if [[ -z "$ids" ]]; then
  echo "No logs returned for build $BUILD_ID." >&2
  exit 1
fi

combined="$logsDir/combined.log"
: > "$combined"
for lid in $ids; do
  lf="$logsDir/log-${lid}.txt"
  echo "Downloading ADO log $lid -> $lf"
  curl -sS "${authHeader[@]}" -H "Accept: text/plain" "$apiBase/logs/$lid?$apiVer" -o "$lf"
  {
    echo "================ Log $lid ================"
    cat "$lf"
    echo
  } >> "$combined"
done

echo "Saved Azure DevOps logs to $logsDir and combined log at $combined"
