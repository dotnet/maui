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
export EVALUATE_USE_HOST_COPILOT_HOME=1
exec "$TRUSTED_COPILOT_CLI_PATH" "$@" \
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
grep -Fqx -- "--sandbox" "$probe_output"
grep -Fqx -- "--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN" "$probe_output"
if grep -Fqx -- "--experimental" "$probe_output"; then
	echo "Copilot wrapper must use trusted settings instead of rewriting --experimental" >&2
	exit 1
fi

# Vally normally creates an empty per-run Copilot home and passes it as the
# session configDirectory, which takes precedence over COPILOT_HOME. Confirm the
# pinned executor honors the opt-out before any model credential enters the job.
copilot_home_module="$install_root/node_modules/@microsoft/vally/dist/executor/copilot-home.js"
if [ ! -f "$copilot_home_module" ]; then
	echo "Expected Vally Copilot-home resolver at $copilot_home_module" >&2
	exit 1
fi
EVALUATE_USE_HOST_COPILOT_HOME=1 node --input-type=module - "$copilot_home_module" <<'EOF'
import { pathToFileURL } from "node:url";

const modulePath = process.argv[2];
const { createIsolatedCopilotHome } = await import(pathToFileURL(modulePath));
const resolved = createIsolatedCopilotHome(process.env, () => {
	throw new Error("Vally attempted to create an isolated Copilot home");
});
if (resolved !== undefined) {
	throw new Error(`Expected host Copilot home opt-out, received ${resolved}`);
}
EOF

vally_runner="$install_root/run-vally"
eval_results_root="$install_root/results"
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
	trusted_git_root="$RUNNER_TEMP/${eval_user}-trusted-git"
	trusted_git_config="$trusted_git_root/config"
	trusted_git_hooks="$trusted_git_root/hooks"
	eval_results_root="$RUNNER_TEMP/${eval_user}-results"
	sudo -n useradd --system --user-group --no-create-home \
		--shell /usr/sbin/nologin "$eval_user"
	sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700 \
		"$eval_home" "$eval_home/tmp"
	sudo -n install -d -o "$(id -un)" -g "$eval_user" -m 2770 \
		"$eval_results_root"

	# Vally creates detached worktrees outside the checkout. Grant its no-sudo
	# user write access only to Git's private worktree metadata and the dedicated
	# results root outside the checkout. Keep candidate content and the rest of
	# the common Git directory read-only to the evaluator.
	git_dir="$GITHUB_WORKSPACE/.git"
	if [ ! -d "$git_dir" ]; then
		echo "Expected a standalone Git directory at $git_dir" >&2
		exit 1
	fi
	git_group=$(stat -c '%G' "$git_dir")
	sudo -n chgrp -R "$git_group" "$git_dir"
	sudo -n chmod -R go-w "$git_dir"
	sudo -n install -d -o "$(stat -c '%U' "$git_dir")" -g "$eval_user" -m 2770 \
		"$git_dir/worktrees"
	sudo -n install -d -o root -g root -m 755 "$trusted_git_root"
	sudo -n install -d -o root -g root -m 555 "$trusted_git_hooks"
	sudo -n install -o root -g root -m 600 /dev/null "$trusted_git_config"
	sudo -n git config --file "$trusted_git_config" --add \
		safe.directory "$GITHUB_WORKSPACE"
	sudo -n git config --file "$trusted_git_config" \
		core.hooksPath "$trusted_git_hooks"
	sudo -n chmod 444 "$trusted_git_config"
	for protected_path in \
		"$git_dir/HEAD" \
		"$git_dir/config" \
		"$git_dir/objects" \
		"$git_dir/refs"; do
		if [ -e "$protected_path" ] &&
			sudo -n -u "$eval_user" /usr/bin/test -w "$protected_path"; then
			echo "Isolated Vally user can modify protected Git path $protected_path" >&2
			exit 1
		fi
	done
	for protected_path in \
		"$GITHUB_WORKSPACE" \
		"$GITHUB_WORKSPACE/.github" \
		"$GITHUB_WORKSPACE/.github/scripts" \
		"$GITHUB_WORKSPACE/.github/skills" \
		"$GITHUB_WORKSPACE/.github/workflows"; do
		if [ -e "$protected_path" ] &&
			sudo -n -u "$eval_user" /usr/bin/test -w "$protected_path"; then
			echo "Isolated Vally user can modify protected workspace path $protected_path" >&2
			exit 1
		fi
	done
	worktree_probe="$eval_home/worktree-probe"
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$GITHUB_WORKSPACE" worktree add --detach "$worktree_probe" HEAD
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$GITHUB_WORKSPACE" worktree remove --force "$worktree_probe"
	if [ "$(
		sudo -n -u "$eval_user" env \
			HOME="$eval_home" \
			GIT_CONFIG_GLOBAL="$trusted_git_config" \
			GIT_CONFIG_NOSYSTEM=1 \
			git config --global --get core.hooksPath
	)" != "$trusted_git_hooks" ]; then
		echo "Isolated Vally user is not using the trusted Git configuration" >&2
		exit 1
	fi
	# Keep configuration immutable to the evaluator. Only runtime output that the
	# CLI never reads as policy is writable across its headless sessions.
	sudo -n install -d -o root -g root -m 755 "$trusted_copilot_home"
	sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700 \
		"$trusted_copilot_home/logs" \
		"$trusted_copilot_home/session-state"
	sudo -n install -d -o root -g root -m 555 \
		"$trusted_copilot_home/installed-plugins"
	sudo -n install -o "$eval_user" -g "$eval_user" -m 600 /dev/null \
		"$trusted_copilot_home/session-store.db" \
		"$trusted_copilot_home/session-store.db-shm" \
		"$trusted_copilot_home/session-store.db-wal"
	cat <<EOF | sudo -n tee "$trusted_copilot_home/settings.json" >/dev/null
{
  "disableAllHooks": true,
  "experimental": true,
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
	cat <<'EOF' | sudo -n tee "$trusted_copilot_home/config.json" >/dev/null
// User settings belong in settings.json.
// This file is managed automatically.
{
  "firstLaunchAt": "2000-01-01T00:00:00.000Z"
}
EOF
	sudo -n chmod 444 "$trusted_copilot_home/settings.json"
	sudo -n chmod 444 "$trusted_copilot_home/config.json"
	for protected_path in \
		"$trusted_copilot_home" \
		"$trusted_copilot_home/config.json" \
		"$trusted_copilot_home/installed-plugins" \
		"$trusted_copilot_home/settings.json"; do
		if sudo -n -u "$eval_user" /usr/bin/test -w "$protected_path"; then
			echo "Isolated Vally user can modify protected Copilot path $protected_path" >&2
			exit 1
		fi
	done
	for writable_path in \
		"$trusted_copilot_home/logs" \
		"$trusted_copilot_home/session-state" \
		"$trusted_copilot_home/session-store.db" \
		"$trusted_copilot_home/session-store.db-shm" \
		"$trusted_copilot_home/session-store.db-wal"; do
		if ! sudo -n -u "$eval_user" /usr/bin/test -w "$writable_path"; then
			echo "Isolated Vally user cannot write Copilot runtime state $writable_path" >&2
			exit 1
		fi
	done

	{
		echo '#!/usr/bin/env bash'
		echo 'set -euo pipefail'
		printf 'eval_user=%q\n' "$eval_user"
		printf 'eval_home=%q\n' "$eval_home"
		printf 'trusted_git_config=%q\n' "$trusted_git_config"
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
	"EVALUATE_USE_HOST_COPILOT_HOME=1"
	"GIT_CONFIG_GLOBAL=$trusted_git_config"
	"GIT_CONFIG_NOSYSTEM=1"
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
	test_use_host_copilot_home=$(
		COPILOT_GITHUB_TOKEN=probe \
			COPILOT_CLI_PATH="$copilot_wrapper" \
			TRUSTED_COPILOT_CLI_PATH="${copilot_runtimes[0]}" \
			TRUSTED_COPILOT_HOME="$trusted_copilot_home" \
			"$vally_runner" /usr/bin/printenv EVALUATE_USE_HOST_COPILOT_HOME
	)
	if [ "$test_use_host_copilot_home" != "1" ]; then
		echo "Isolated Vally process is not using the trusted Copilot home" >&2
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
	mkdir -p "$eval_results_root"
	cat > "$trusted_copilot_home/settings.json" <<EOF
{
  "disableAllHooks": true,
  "experimental": true,
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
	cat > "$trusted_copilot_home/config.json" <<'EOF'
// User settings belong in settings.json.
// This file is managed automatically.
{
  "firstLaunchAt": "2000-01-01T00:00:00.000Z"
}
EOF
	mkdir -p \
		"$trusted_copilot_home/installed-plugins" \
		"$trusted_copilot_home/logs" \
		"$trusted_copilot_home/session-state"
	touch \
		"$trusted_copilot_home/session-store.db" \
		"$trusted_copilot_home/session-store.db-shm" \
		"$trusted_copilot_home/session-store.db-wal"
	cat > "$vally_runner" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
export EVALUATE_USE_HOST_COPILOT_HOME=1
exec "$@"
EOF
	chmod 700 "$vally_runner"
fi

# The selected home must contain the complete fail-closed policy that Vally and
# every spawned Copilot session will read.
node - "$trusted_copilot_home/settings.json" "$install_root" <<'EOF'
const fs = require("node:fs");

const settingsPath = process.argv[2];
const installRoot = process.argv[3];
const settings = JSON.parse(fs.readFileSync(settingsPath, "utf8"));
const sandbox = settings.sandbox;
if (settings.disableAllHooks !== true ||
    settings.experimental !== true ||
    !sandbox?.enabled ||
    sandbox.allowBypass !== false ||
    sandbox.gitAuth !== false ||
    sandbox.ghAuth !== false ||
    sandbox.sandboxMcpServers !== true ||
    sandbox.sandboxLspServers !== true ||
    sandbox.allowDevToolAccess !== false ||
    sandbox.userPolicy?.network?.allowLocalNetwork !== false) {
	throw new Error("Trusted Copilot sandbox policy is incomplete");
}
const denied = sandbox.userPolicy?.filesystem?.deniedPaths;
if (!Array.isArray(denied) || !denied.includes("/proc") || !denied.includes(installRoot)) {
	throw new Error("Trusted Copilot filesystem policy is incomplete");
}
EOF

{
	echo "vally_bin=$vally_bin"
	echo "vally_runner=$vally_runner"
	echo "copilot_wrapper=$copilot_wrapper"
	echo "copilot_runtime=${copilot_runtimes[0]}"
	echo "copilot_home=$trusted_copilot_home"
	echo "results_root=$eval_results_root"
} >> "$github_output"
