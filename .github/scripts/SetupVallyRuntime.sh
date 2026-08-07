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
: "${TRUSTED_COPILOT_HOME:?TRUSTED_COPILOT_HOME is required}"
export COPILOT_HOME="$TRUSTED_COPILOT_HOME"
exec "$TRUSTED_COPILOT_CLI_PATH" "$@" \
	--experimental \
	--sandbox \
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
	TRUSTED_COPILOT_HOME="$install_root/probe-home" \
	"$copilot_wrapper" --headless --stdio
grep -Fqx -- "--headless" "$probe_output"
grep -Fqx -- "--stdio" "$probe_output"
grep -Fqx -- "--experimental" "$probe_output"
grep -Fqx -- "--sandbox" "$probe_output"
grep -Fqx -- "--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN" "$probe_output"

vally_runner="$install_root/run-vally"
if [ "${GITHUB_ACTIONS:-false}" = "true" ]; then
	: "${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required on GitHub Actions}"
	: "${RUNNER_TEMP:?RUNNER_TEMP is required on GitHub Actions}"
	command -v sudo >/dev/null
	command -v useradd >/dev/null
	if ! command -v bwrap >/dev/null; then
		sudo -n apt-get update -qq
		sudo -n apt-get install -y -qq bubblewrap
	fi

	eval_user="vally$(od -An -N6 -tx1 /dev/urandom | tr -d ' \n')"
	eval_home="$RUNNER_TEMP/${eval_user}-home"
	trusted_copilot_home="$RUNNER_TEMP/${eval_user}-copilot-home"
	sudo -n useradd --system --user-group --no-create-home \
		--shell /usr/sbin/nologin "$eval_user"
	sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700 \
		"$eval_home" "$eval_home/tmp"

	# Vally creates synthetic worktrees and results under the checkout. Grant its
	# no-sudo user write access there, while keeping the evaluator/runtime owned
	# by root so agent tool calls cannot replace a later Copilot launch.
	sudo -n chgrp -R "$eval_user" "$GITHUB_WORKSPACE"
	sudo -n chmod -R g+rwX "$GITHUB_WORKSPACE"
	find "$GITHUB_WORKSPACE" -type d -exec sudo -n chmod g+s {} +
	sudo -n -u "$eval_user" env HOME="$eval_home" \
		git config --global --add safe.directory "$GITHUB_WORKSPACE"
	sudo -n install -d -o root -g root -m 1777 "$trusted_copilot_home"
	cat <<EOF | sudo -n tee "$trusted_copilot_home/settings.json" >/dev/null
{
  "sandbox": {
    "enabled": true,
    "allowBypass": false,
    "gitAuth": false,
    "ghAuth": false,
    "sandboxMcpServers": true,
    "sandboxLspServers": true,
    "allowDevToolAccess": false,
    "userPolicy": {
      "filesystem": {
        "deniedPaths": ["/proc", "$install_root"],
        "clearPolicyOnExit": true
      },
      "network": {
        "allowOutbound": true,
        "allowLocalNetwork": false
      }
    }
  }
}
EOF
	sudo -n chmod 444 "$trusted_copilot_home/settings.json"

	{
		echo '#!/usr/bin/env bash'
		echo 'set -euo pipefail'
		printf 'eval_user=%q\n' "$eval_user"
		printf 'eval_home=%q\n' "$eval_home"
		cat <<'EOF'
: "${COPILOT_GITHUB_TOKEN:?COPILOT_GITHUB_TOKEN is required}"
: "${COPILOT_CLI_PATH:?COPILOT_CLI_PATH is required}"
: "${TRUSTED_COPILOT_CLI_PATH:?TRUSTED_COPILOT_CLI_PATH is required}"
: "${TRUSTED_COPILOT_HOME:?TRUSTED_COPILOT_HOME is required}"
umask 0002
child_env=(
	"HOME=$eval_home"
	"TMPDIR=$eval_home/tmp"
	"PATH=$PATH"
	"COPILOT_CLI_PATH=$COPILOT_CLI_PATH"
	"TRUSTED_COPILOT_CLI_PATH=$TRUSTED_COPILOT_CLI_PATH"
	"TRUSTED_COPILOT_HOME=$TRUSTED_COPILOT_HOME"
)
for name in CI GITHUB_ACTIONS GITHUB_WORKSPACE RUNNER_TEMP \
	HTTP_PROXY HTTPS_PROXY NO_PROXY NODE_EXTRA_CA_CERTS SSL_CERT_FILE; do
	if [[ -v "$name" ]]; then
		child_env+=("$name=${!name}")
	fi
done
exec /usr/bin/sudo -n \
	--preserve-env=COPILOT_GITHUB_TOKEN \
	-u "$eval_user" -- /usr/bin/env "${child_env[@]}" "$@"
EOF
	} > "$vally_runner"
	chmod 700 "$vally_runner"

	test_user=$(
		COPILOT_GITHUB_TOKEN=probe \
			COPILOT_CLI_PATH="$copilot_wrapper" \
			TRUSTED_COPILOT_CLI_PATH="${copilot_runtimes[0]}" \
			TRUSTED_COPILOT_HOME="$trusted_copilot_home" \
			"$vally_runner" /usr/bin/id -un
	)
	if [ "$test_user" != "$eval_user" ]; then
		echo "Expected isolated Vally user $eval_user, found $test_user" >&2
		exit 1
	fi
	if [ "$(sudo -n -u "$eval_user" /usr/bin/id -Gn)" != "$eval_user" ]; then
		echo "Isolated Vally user unexpectedly belongs to another group" >&2
		exit 1
	fi
	if sudo -n -u "$eval_user" /usr/bin/test -w "$(dirname "$install_root")"; then
		echo "Isolated Vally user can replace the runtime directory" >&2
		exit 1
	fi

	sudo -n chown -R root:root "$install_root"
	sudo -n chmod -R a+rX,a-w "$install_root"
	if sudo -n -u "$eval_user" /usr/bin/test -w "$copilot_wrapper"; then
		echo "Isolated Vally user can modify the Copilot wrapper" >&2
		exit 1
	fi
else
	trusted_copilot_home="$install_root/copilot-home"
	mkdir -p "$trusted_copilot_home"
	cat > "$trusted_copilot_home/settings.json" <<EOF
{
  "sandbox": {
    "enabled": true,
    "allowBypass": false,
    "gitAuth": false,
    "ghAuth": false,
    "sandboxMcpServers": true,
    "sandboxLspServers": true,
    "allowDevToolAccess": false,
    "userPolicy": {
      "filesystem": {
        "deniedPaths": ["/proc", "$install_root"],
        "clearPolicyOnExit": true
      },
      "network": {
        "allowOutbound": true,
        "allowLocalNetwork": false
      }
    }
  }
}
EOF
	cat > "$vally_runner" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
exec "$@"
EOF
	chmod 700 "$vally_runner"
fi

{
	echo "vally_bin=$vally_bin"
	echo "vally_runner=$vally_runner"
	echo "copilot_wrapper=$copilot_wrapper"
	echo "copilot_runtime=${copilot_runtimes[0]}"
	echo "copilot_home=$trusted_copilot_home"
} >> "$github_output"
