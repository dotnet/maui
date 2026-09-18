#!/usr/bin/env bash

set -euo pipefail

if [ "$#" -lt 7 ]; then
	echo "Usage: $0 <github-output> <vally-runner> <copilot-wrapper> <copilot-runtime> <copilot-home> <probe-root> <model>..." >&2
	exit 2
fi

github_output=$1
vally_runner=$2
copilot_wrapper=$3
copilot_runtime=$4
copilot_home=$5
probe_root=$6
shift 6

for executable in "$vally_runner" "$copilot_wrapper" "$copilot_runtime"; do
	if [ ! -x "$executable" ]; then
		echo "Required evaluator executable is missing: $executable" >&2
		exit 1
	fi
done

models=()
declare -A seen_models=()
for model in "$@"; do
	if [[ ! "$model" =~ ^[A-Za-z0-9._-]+$ ]] || [ "$model" = "auto" ]; then
		echo "Invalid explicit model for Copilot PAT probe: $model" >&2
		exit 1
	fi
	if [[ ! -v "seen_models[$model]" ]]; then
		seen_models["$model"]=1
		models+=("$model")
	fi
done
if [ "${#models[@]}" -eq 0 ]; then
	echo "At least one explicit model is required for Copilot PAT selection" >&2
	exit 1
fi

tokens=()
token_slots=()
for i in 0 1 2 3 4 5 6 7 8 9; do
	var="COPILOT_PAT_$i"
	value="${!var-}"
	if [ -n "$value" ]; then
		echo "::add-mask::$value"
		tokens+=("$value")
		token_slots+=("$i")
	fi
done
if [ "${#tokens[@]}" -eq 0 ]; then
	echo "::error::No COPILOT_PAT_* secrets are configured in the copilot-pat-pool environment" >&2
	exit 1
fi

mkdir -p "$probe_root"
chmod 755 "$probe_root"
run_id=${GITHUB_RUN_ID:-0}
run_attempt=${GITHUB_RUN_ATTEMPT:-1}
start_offset=${TOKEN_START_OFFSET:-0}
validate_numeric() {
	local name=$1
	local value=$2
	if [[ ! "$value" =~ ^[0-9]+$ ]]; then
		echo "$name must be numeric" >&2
		exit 1
	fi
}
validate_numeric GITHUB_RUN_ID "$run_id"
validate_numeric GITHUB_RUN_ATTEMPT "$run_attempt"
validate_numeric TOKEN_START_OFFSET "$start_offset"
if [ "$run_attempt" -eq 0 ]; then
	echo "GITHUB_RUN_ATTEMPT must be at least 1" >&2
	exit 1
fi
token_count=${#tokens[@]}
start_index=$(((run_id % token_count + start_offset % token_count + (run_attempt - 1) % token_count) % token_count))

selected_token=""
for ((offset = 0; offset < ${#tokens[@]}; offset++)); do
	index=$(((start_index + offset) % ${#tokens[@]}))
	token="${tokens[$index]}"
	slot="${token_slots[$index]}"
	auth_header_name="Author"
	auth_header_name+="ization"
	auth_scheme="Bear"
	auth_scheme+="er"
	if ! status=$(
		printf '%s: %s %s\n' "$auth_header_name" "$auth_scheme" "$token" |
			curl --silent --show-error --connect-timeout 10 --max-time 20 \
				--header @- \
				--output /dev/null \
				--write-out '%{http_code}' \
				--header "Accept: application/vnd.github+json" \
				--header "X-GitHub-Api-Version: 2022-11-28" \
				https://api.github.com/user
	); then
		status="transport-error"
	fi
	if [ "$status" != "200" ]; then
		if [ "$status" = "transport-error" ]; then
			echo "::warning::Skipping unavailable Copilot PAT slot $slot (GitHub /user transport failed)"
		else
			echo "::warning::Skipping unavailable Copilot PAT slot $slot (GitHub /user returned HTTP $status)"
		fi
		continue
	fi

	supported=true
	for model in "${models[@]}"; do
		probe_output=""
		if ! probe_output=$(
			COPILOT_GITHUB_TOKEN="$token" \
			COPILOT_CLI_PATH="$copilot_wrapper" \
			TRUSTED_COPILOT_CLI_PATH="$copilot_runtime" \
			TRUSTED_COPILOT_HOME="$copilot_home" \
			timeout 90s "$vally_runner" "$copilot_wrapper" \
				-C "$probe_root" \
				--model "$model" \
				--reasoning-effort low \
				--disable-builtin-mcps \
				--no-custom-instructions \
				--no-remote \
				--no-remote-export \
				--allow-all-tools \
				--prompt "Reply with exactly MODEL_OK. Do not use tools." \
				--silent 2>/dev/null
		); then
			echo "::warning::Skipping Copilot PAT slot $slot because model $model is unavailable"
			supported=false
			break
		fi
		if ! grep -Fqx "MODEL_OK" <<< "$probe_output"; then
			echo "::warning::Skipping Copilot PAT slot $slot because model $model returned an unexpected probe response"
			supported=false
			break
		fi
	done
	if [ "$supported" = true ]; then
		selected_token=$token
		echo "Selected PAT slot $slot with all required models: ${models[*]}"
		break
	fi
done

if [ -z "$selected_token" ]; then
	echo "::error::Every configured COPILOT_PAT_* secret is unavailable, rate-limited, or missing a required model" >&2
	exit 1
fi

echo "token=$selected_token" >> "$github_output"
