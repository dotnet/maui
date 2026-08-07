#!/usr/bin/env bash

set -euo pipefail

if [ "$#" -ne 3 ]; then
	echo "Usage: $0 <install-root> <vally-version> <github-output>" >&2
	exit 2
fi

install_root=$1
vally_version=$2
github_output=$3

mkdir -p "$install_root"
(
	cd "$install_root"
	npm install \
		--no-audit \
		--no-fund \
		--no-save \
		--loglevel=error \
		"@microsoft/vally-cli@${vally_version}"
)

vally_bin="$install_root/node_modules/.bin/vally"
if [ ! -x "$vally_bin" ]; then
	echo "Vally executable was not installed at $vally_bin" >&2
	exit 1
fi
installed_version=$("$vally_bin" --version)
if [ "$installed_version" != "$vally_version" ]; then
	echo "Expected Vally $vally_version, found $installed_version" >&2
	exit 1
fi

mapfile -t copilot_runtimes < <(
	find "$install_root/node_modules/@github" \
		-mindepth 2 \
		-maxdepth 2 \
		-type f -name copilot -perm -u+x \
		-print
)
if [ "${#copilot_runtimes[@]}" -ne 1 ]; then
	echo "Expected exactly one native Copilot runtime, found ${#copilot_runtimes[@]}" >&2
	exit 1
fi

copilot_wrapper="$install_root/copilot-runtime-with-secret-isolation"
cat > "$copilot_wrapper" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
: "${TRUSTED_COPILOT_CLI_PATH:?TRUSTED_COPILOT_CLI_PATH is required}"
exec "$TRUSTED_COPILOT_CLI_PATH" "$@" \
	--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN
EOF
chmod 700 "$copilot_wrapper"

# Verify the wrapper forwards SDK arguments and adds Copilot's subprocess
# credential isolation before any model credential enters the job.
probe_runtime="$install_root/copilot-runtime-argument-probe"
probe_output="$install_root/copilot-runtime-arguments.txt"
cat > "$probe_runtime" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
printf '%s\n' "$@" > "$RUNTIME_ARGUMENT_PROBE"
EOF
chmod 700 "$probe_runtime"
RUNTIME_ARGUMENT_PROBE="$probe_output" \
	TRUSTED_COPILOT_CLI_PATH="$probe_runtime" \
	"$copilot_wrapper" --headless --stdio
grep -Fqx -- "--headless" "$probe_output"
grep -Fqx -- "--stdio" "$probe_output"
grep -Fqx -- "--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN" "$probe_output"

{
	echo "vally_bin=$vally_bin"
	echo "copilot_wrapper=$copilot_wrapper"
	echo "copilot_runtime=${copilot_runtimes[0]}"
} >> "$github_output"
