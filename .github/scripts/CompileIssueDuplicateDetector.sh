#!/usr/bin/env bash
set -euo pipefail

compiler_commit=86885522c56a777b4734e6b5d81b41ce244a513f
compiler_version=v0.86.2+maui-duplicate-detector
runtime_commit=6aab9e5b5c91c615506061f09bedd81a23babe3c
cache_directory="${XDG_CACHE_HOME:-$HOME/.cache}/maui/gh-aw/$compiler_commit"
source_directory="$cache_directory/source"
compiler_binary="$cache_directory/gh-aw"

run_without_tokens() {
  env -u GH_TOKEN -u GITHUB_TOKEN -u COPILOT_GITHUB_TOKEN -u GH_COMMENT_TOKEN \
    -u GH_AW_GITHUB_TOKEN -u GH_AW_GITHUB_MCP_SERVER_TOKEN \
    -u COPILOT_PAT_0 -u COPILOT_PAT_1 -u COPILOT_PAT_2 -u COPILOT_PAT_3 \
    -u COPILOT_PAT_4 -u COPILOT_PAT_5 -u COPILOT_PAT_6 -u COPILOT_PAT_7 \
    -u COPILOT_PAT_8 -u COPILOT_PAT_9 \
    "$@"
}

if ! command -v go >/dev/null 2>&1; then
  echo "Compiling the duplicate detector requires Go 1.26.5 or later." >&2
  exit 127
fi

cd "$(git rev-parse --show-toplevel)"
mkdir -p "$cache_directory"
if [[ ! -d "$source_directory/.git" ]]; then
  if [[ -e "$source_directory" ]]; then
    echo "Refusing to replace an existing non-repository compiler cache." >&2
    exit 1
  fi
  git init --quiet "$source_directory"
fi
if ! git -C "$source_directory" rev-parse --verify HEAD >/dev/null 2>&1; then
  git -C "$source_directory" fetch --quiet --depth 1 \
    https://github.com/kubaflo/gh-aw.git "$compiler_commit"
  git -C "$source_directory" checkout --quiet --detach FETCH_HEAD
fi
if [[ "$(git -C "$source_directory" rev-parse HEAD)" != "$compiler_commit" ||
      -n "$(git -C "$source_directory" status --porcelain)" ]]; then
  echo "The pinned compiler checkout is stale or modified; refusing to compile." >&2
  exit 1
fi

(
  cd "$source_directory"
  run_without_tokens GOWORK=off go build -p 4 \
    -ldflags "-X main.version=$compiler_version -X main.isRelease=true" \
    -o "$compiler_binary" ./cmd/gh-aw
)

run_without_tokens "$compiler_binary" compile issue-duplicate-detector --strict --validate --no-check-update \
  --action-mode action --action-tag "$runtime_commit" "$@"
