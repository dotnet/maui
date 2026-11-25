#!/usr/bin/env bash
set -euo pipefail

compiler_commit=ff62cdbec36230acbae869ddb28806e8eca01ea1
compiler_version=v0.87.10+maui-duplicate-detector
runtime_commit=bc8c008a419c5b7a29df6f5641edd35fd1c6ea85
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
  echo "Compiling the duplicate detector requires Go 1.26.7 or later." >&2
  exit 127
fi

cd "$(git rev-parse --show-toplevel)"
compiler_patch="$PWD/.github/scripts/IssueDuplicateCompiler.patch"
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
    https://github.com/github/gh-aw.git "$compiler_commit"
  git -C "$source_directory" checkout --quiet --detach FETCH_HEAD
fi
if [[ "$(git -C "$source_directory" rev-parse HEAD)" != "$compiler_commit" ||
      -n "$(git -C "$source_directory" status --porcelain)" ]]; then
  echo "The pinned compiler checkout is stale or modified; refusing to compile." >&2
  exit 1
fi

build_directory="$(mktemp -d "$cache_directory/build.XXXXXXXX")"
trap 'rm -rf -- "$build_directory"' EXIT
git -C "$source_directory" archive HEAD | tar -x -C "$build_directory"
git --git-dir="$source_directory/.git" --work-tree="$build_directory" \
  -C "$build_directory" apply --unidiff-zero --check "$compiler_patch"
git --git-dir="$source_directory/.git" --work-tree="$build_directory" \
  -C "$build_directory" apply --unidiff-zero "$compiler_patch"
git --git-dir="$source_directory/.git" --work-tree="$build_directory" \
  -C "$build_directory" apply --unidiff-zero --reverse --check "$compiler_patch"

(
  cd "$build_directory"
  run_without_tokens GOWORK=off go build -buildvcs=false -p 4 \
    -ldflags "-X main.version=$compiler_version -X main.isRelease=true" \
    -o "$compiler_binary" ./cmd/gh-aw
)

run_without_tokens "$compiler_binary" compile issue-duplicate-detector --strict --validate --no-check-update \
  --action-mode action --action-tag "$runtime_commit" "$@"
