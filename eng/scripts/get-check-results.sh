#!/usr/bin/env bash
# Fetch GitHub Check Run or Check Suite results and save them to logs/
#
# Usage:
#   eng/scripts/get-check-results.sh [--check-run-id ID | --check-suite-id ID]
#                                    [--owner OWNER] [--repo REPO]
#                                    [--output-dir DIR]
#
# Env:
#   GITHUB_TOKEN or GH_TOKEN   GitHub token with repo access (required for higher rate limits)
#   GITHUB_REPOSITORY          Fallback for owner/repo (format: owner/repo)
#   GITHUB_API_URL             Override API base URL (default: https://api.github.com)
#
# Notes:
# - For a single check run, saves:
#     logs/check-run-<id>.json (raw JSON)
#     logs/check-run-<id>.log  (parsed text/summary when jq is available)
# - For a check suite, saves:
#     logs/check-suite-<id>.json (list of runs)
#     logs/check-run-<id>.{json,log} for each run (when jq is available)
set -euo pipefail

usage() {
  cat <<EOF
Usage: $0 [--check-run-id ID | --check-suite-id ID] [--owner OWNER] [--repo REPO] [--output-dir DIR]

Fetch GitHub Check Run or Check Suite results and save them under logs/.

Options:
  --check-run-id ID       Fetch a single check run by ID
  --check-suite-id ID     Fetch all check runs in a check suite by ID
  --owner OWNER           Repository owner (defaults to GITHUB_REPOSITORY env)
  --repo REPO             Repository name (defaults to GITHUB_REPOSITORY env)
  --output-dir DIR        Output directory (default: logs)
  -h, --help              Show this help
EOF
}

OWNER_FLAG=""
REPO_FLAG=""
OUTPUT_DIR="logs"
CHECK_RUN_ID=""
CHECK_SUITE_ID=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --check-run-id)
      CHECK_RUN_ID="${2:-}"; shift 2;;
    --check-suite-id)
      CHECK_SUITE_ID="${2:-}"; shift 2;;
    --owner)
      OWNER_FLAG="${2:-}"; shift 2;;
    --repo)
      REPO_FLAG="${2:-}"; shift 2;;
    --output-dir)
      OUTPUT_DIR="${2:-}"; shift 2;;
    -h|--help)
      usage; exit 0;;
    *)
      echo "Unknown argument: $1" >&2; usage; exit 2;;
  esac
done

if [[ -z "$CHECK_RUN_ID" && -z "$CHECK_SUITE_ID" ]]; then
  echo "Error: one of --check-run-id or --check-suite-id is required" >&2
  usage; exit 2
fi
if [[ -n "$CHECK_RUN_ID" && -n "$CHECK_SUITE_ID" ]]; then
  echo "Error: specify only one of --check-run-id or --check-suite-id" >&2
  usage; exit 2
fi

# Resolve owner/repo
OWNER="$OWNER_FLAG"
REPO="$REPO_FLAG"
if [[ -z "$OWNER" || -z "$REPO" ]]; then
  if [[ -n "${GITHUB_REPOSITORY:-}" ]]; then
    OWNER="${OWNER:-${GITHUB_REPOSITORY%%/*}}"
    REPO="${REPO:-${GITHUB_REPOSITORY##*/}}"
  else
    echo "Error: --owner/--repo not provided and GITHUB_REPOSITORY not set" >&2
    exit 2
  fi
fi

TOKEN="${GITHUB_TOKEN:-${GH_TOKEN:-}}"
if [[ -z "$TOKEN" ]]; then
  echo "Warning: GITHUB_TOKEN/GH_TOKEN not set; proceeding unauthenticated (rate limit may be low)" >&2
fi

API_URL="${GITHUB_API_URL:-https://api.github.com}"
HEADERS=(
  -H "Accept: application/vnd.github+json"
  -H "X-GitHub-Api-Version: 2022-11-28"
)
if [[ -n "$TOKEN" ]]; then
  HEADERS+=( -H "Authorization: Bearer $TOKEN" )
fi

mkdir -p "$OUTPUT_DIR"

have_jq=0
if command -v jq >/dev/null 2>&1; then
  have_jq=1
fi

api_get() {
  local url="$1"
  curl -sS "${HEADERS[@]}" "$url"
}

save_check_run() {
  local id="$1"
  local run_json
  run_json="$(api_get "$API_URL/repos/$OWNER/$REPO/check-runs/$id")"

  local json_path="$OUTPUT_DIR/check-run-$id.json"
  echo "$run_json" > "$json_path"

  if [[ $have_jq -eq 1 ]]; then
    local name status conclusion summary text
    name=$(jq -r '.name' <<<"$run_json")
    status=$(jq -r '.status' <<<"$run_json")
    conclusion=$(jq -r '.conclusion // "null"' <<<"$run_json")
    summary=$(jq -r '.output.summary // empty' <<<"$run_json")
    text=$(jq -r '.output.text // empty' <<<"$run_json")

    local log_path="$OUTPUT_DIR/check-run-$id.log"
    {
      echo "Check Run: $name ($id)"
      echo "Status: $status    Conclusion: $conclusion"
      echo
      if [[ -n "$summary" ]]; then
        echo "Summary:"
        echo "$summary"
        echo
      fi
      if [[ -n "$text" ]]; then
        echo "Output:"
        echo "$text"
      fi
    } > "$log_path"
    echo "Saved check run output to $log_path"
  else
    echo "jq not found; saved raw JSON to $json_path"
  fi
}

if [[ -n "$CHECK_RUN_ID" ]]; then
  save_check_run "$CHECK_RUN_ID"
  exit 0
fi

# Handle check suite: list runs then save each
suite_list_json="$(api_get "$API_URL/repos/$OWNER/$REPO/check-suites/$CHECK_SUITE_ID/check-runs")"
list_path="$OUTPUT_DIR/check-suite-$CHECK_SUITE_ID.json"
echo "$suite_list_json" > "$list_path"

if [[ $have_jq -eq 1 ]]; then
  mapfile -t run_ids < <(jq -r '.check_runs[].id' <<<"$suite_list_json")
  echo "Found ${#run_ids[@]} check runs in suite $CHECK_SUITE_ID"

  for rid in "${run_ids[@]}"; do
    save_check_run "$rid"
  done
else
  echo "jq not found; saved suite run list to $list_path. Install jq to expand runs."
fi
