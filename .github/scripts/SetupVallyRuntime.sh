#!/usr/bin/env bash

set -euo pipefail

if [ "$#" -ne 3 ]; then
	echo "Usage: $0 <install-root> <vally-version> <github-output>" >&2
	exit 2
fi

install_root=$1
vally_version=$2
github_output=$3
copilot_sdk_version=1.0.7

mkdir -p "$install_root"
(
	cd "$install_root"
	npm install \
		--no-audit \
		--no-fund \
		--no-save \
		--loglevel=error \
		"@microsoft/vally-cli@${vally_version}" \
		"@github/copilot-sdk@${copilot_sdk_version}"
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

installed_copilot_sdk_version=$(
	node -e 'console.log(require(process.argv[1]).version)' \
		"$install_root/node_modules/@github/copilot-sdk/package.json"
)
if [ "$installed_copilot_sdk_version" != "$copilot_sdk_version" ]; then
	echo "Expected Copilot SDK $copilot_sdk_version, found $installed_copilot_sdk_version" >&2
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
	missing_packages=()
	if ! command -v bwrap >/dev/null; then
		missing_packages+=(bubblewrap)
	fi
	if ! command -v setfacl >/dev/null; then
		missing_packages+=(acl)
	fi
	if [ "${#missing_packages[@]}" -gt 0 ]; then
		sudo -n apt-get update -qq
		sudo -n apt-get install -y -qq "${missing_packages[@]}"
	fi

	eval_user="vally$(od -An -N6 -tx1 /dev/urandom | tr -d ' \n')"
	workspace_owner=$(id -un)
	eval_home="$RUNNER_TEMP/${eval_user}-home"
	workspace_root="$RUNNER_TEMP/${eval_user}-workspace"
	trusted_copilot_home="$RUNNER_TEMP/${eval_user}-copilot-home"
	trusted_git_root="$RUNNER_TEMP/${eval_user}-trusted-git"
	trusted_git_config="$trusted_git_root/config"
	trusted_git_hooks="$trusted_git_root/hooks"
	eval_results_root="$RUNNER_TEMP/${eval_user}-results"
	original_workspace_stat=$(stat -c '%u:%g:%a' "$GITHUB_WORKSPACE")
	original_git_stat=$(stat -c '%u:%g:%a' "$GITHUB_WORKSPACE/.git")
	sudo -n useradd --system --user-group --no-create-home \
		--shell /usr/sbin/nologin "$eval_user"
	sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700 \
		"$eval_home" "$eval_home/tmp"
	sudo -n install -d -o "$(id -un)" -g "$eval_user" -m 2770 \
		"$eval_results_root"

	# Preserve the prepared working tree and complete object database without
	# changing the Actions checkout. This retains patched specs and synthetic
	# fixture commits that a fresh clone would omit.
	git_dir="$workspace_root/.git"
	sudo -n install -d -o "$workspace_owner" -g "$eval_user" -m 3770 \
		"$workspace_root"
	cp -a "$GITHUB_WORKSPACE/." "$workspace_root/"
	if [ ! -d "$git_dir" ]; then
		echo "Expected a standalone Git directory at $git_dir" >&2
		exit 1
	fi
	workspace_symlink=$(
		find "$workspace_root" -path "$git_dir" -prune -o \
			-type l -print -quit
	)
	if [ -n "$workspace_symlink" ]; then
		echo "Evaluator workspace contains unsupported symlink: ${workspace_symlink#"$workspace_root"/}" >&2
		exit 1
	fi
	sudo -n chown -R "$workspace_owner:$eval_user" "$workspace_root"
	sudo -n chmod -R g+rX,g-w,o-rwx "$workspace_root"

	# The prepared working tree is evaluator-readable but remains writable only
	# by the runner for baseline restoration and trusted fixture preparation.
	# Detached worktrees need only their dedicated common-Git metadata plus the
	# root lock used when packed refs are refreshed.
	sudo -n chmod 2750 "$workspace_root"
	sudo -n chmod -R g+rX,g-w,o-rwx "$git_dir"
	sudo -n chmod 3770 "$git_dir"
	for mutable_git_path in objects worktrees refs logs; do
		sudo -n install -d -o "$workspace_owner" -g "$eval_user" -m 2770 \
			"$git_dir/$mutable_git_path"
		sudo -n chgrp -R "$eval_user" "$git_dir/$mutable_git_path"
		sudo -n chmod -R g+rwX,o-rwx "$git_dir/$mutable_git_path"
		sudo -n find "$git_dir/$mutable_git_path" -type d \
			-exec chmod g+rws,o-rwx {} +
	done
	if [ -e "$git_dir/packed-refs" ]; then
		sudo -n chown "$eval_user:$eval_user" "$git_dir/packed-refs"
		sudo -n chmod 660 "$git_dir/packed-refs"
	fi
	workspace_parent=$(dirname "$workspace_root")
	while [ "$workspace_parent" != "/" ]; do
		sudo -n setfacl -m "u:$eval_user:--x" "$workspace_parent"
		workspace_parent=$(dirname "$workspace_parent")
	done

	sudo -n install -d -o root -g root -m 755 "$trusted_git_root"
	sudo -n install -d -o root -g root -m 555 "$trusted_git_hooks"
	sudo -n install -o root -g root -m 600 /dev/null "$trusted_git_config"
	sudo -n git config --file "$trusted_git_config" --add \
		safe.directory "$workspace_root"
	sudo -n git config --file "$trusted_git_config" \
		core.hooksPath "$trusted_git_hooks"
	sudo -n chmod 444 "$trusted_git_config"

	# Rebuild the copied repository configuration from a small functional
	# allowlist, then protect it with the sticky Git directory. The evaluator
	# can update refs, objects, and worktree metadata without replacing config.
	copied_git_config="$git_dir/config"
	sanitized_git_config="$RUNNER_TEMP/${eval_user}-git-config"
	: > "$sanitized_git_config"
	for key in \
		core.repositoryformatversion \
		core.filemode \
		core.bare \
		core.logallrefupdates \
		core.ignorecase \
		core.precomposeunicode \
		core.sparsecheckout \
		core.sparsecheckoutcone \
		extensions.objectformat; do
		while IFS= read -r value; do
			git config --file "$sanitized_git_config" --add "$key" "$value"
		done < <(git config --file "$copied_git_config" --get-all "$key" || true)
	done
	git config --file "$sanitized_git_config" core.hooksPath "$trusted_git_hooks"
	if [ -n "${TRUSTED_UPSTREAM_URL:-}" ]; then
		if [[ ! "$TRUSTED_UPSTREAM_URL" =~ ^https://github\.com/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+\.git$ ]]; then
			echo "TRUSTED_UPSTREAM_URL must be a plain GitHub repository URL" >&2
			exit 1
		fi
		git config --file "$sanitized_git_config" \
			remote.upstream.url "$TRUSTED_UPSTREAM_URL"
		git config --file "$sanitized_git_config" \
			remote.upstream.fetch "+refs/heads/*:refs/remotes/upstream/*"
	fi
	sudo -n install -o root -g root -m 444 \
		"$sanitized_git_config" "$copied_git_config"
	rm -f "$sanitized_git_config"
	sudo -n rm -f "$git_dir/config.worktree"

	if ! sudo -n -u "$eval_user" /usr/bin/test -r "$workspace_root/.git/HEAD"; then
		echo "Isolated Vally user cannot read the evaluator workspace" >&2
		exit 1
	fi
	repository_controls=(
		"file:.mcp.json"
		"dir:.github/hooks"
		"file:.github/mcp.json"
		"file:.github/copilot/settings.json"
		"file:.github/copilot/settings.local.json"
		"file:.claude/settings.json"
		"file:.claude/settings.local.json"
	)
	control_probe_number=0
	for control_entry in "${repository_controls[@]}"; do
		control_kind=${control_entry%%:*}
		control_relative_path=${control_entry#*:}
		control_path="$workspace_root/$control_relative_path"
		control_probe_number=$((control_probe_number + 1))
		replacement_path="$eval_home/repository-control-replacement-$control_probe_number"
		renamed_path="$control_path.runtime-probe"

		if [ -e "$control_path" ]; then
			if sudo -n -u "$eval_user" /usr/bin/test -w "$control_path"; then
				echo "Isolated Vally user can modify repository control $control_path" >&2
				exit 1
			fi
			if [ "$control_kind" = "file" ] &&
				printf 'runtime probe\n' |
					sudo -n -u "$eval_user" /usr/bin/tee "$control_path" >/dev/null 2>&1; then
				echo "Isolated Vally user can overwrite repository control $control_path" >&2
				exit 1
			fi
			if sudo -n -u "$eval_user" /bin/mv \
				"$control_path" "$renamed_path" 2>/dev/null; then
				echo "Isolated Vally user can rename repository control $control_path" >&2
				exit 1
			fi
			if [ "$control_kind" = "dir" ]; then
				sudo -n -u "$eval_user" /bin/mkdir "$replacement_path"
				remove_command=(/bin/rm -rf -- "$control_path")
			else
				sudo -n -u "$eval_user" /usr/bin/touch "$replacement_path"
				remove_command=(/bin/rm -- "$control_path")
			fi
			if sudo -n -u "$eval_user" "${remove_command[@]}" 2>/dev/null; then
				echo "Isolated Vally user can remove repository control $control_path" >&2
				exit 1
			fi
			if sudo -n -u "$eval_user" /bin/mv -T -f \
				"$replacement_path" "$control_path" 2>/dev/null; then
				echo "Isolated Vally user can replace repository control $control_path" >&2
				exit 1
			fi
			sudo -n -u "$eval_user" /bin/rm -rf -- "$replacement_path"
		elif [ "$control_kind" = "dir" ]; then
			if sudo -n -u "$eval_user" /bin/mkdir "$control_path" 2>/dev/null; then
				echo "Isolated Vally user can create repository control $control_path" >&2
				exit 1
			fi
		else
			control_parent=$(dirname "$control_path")
			if [ ! -d "$control_parent" ] &&
				sudo -n -u "$eval_user" /bin/mkdir -p "$control_parent" 2>/dev/null; then
				echo "Isolated Vally user can create repository control parent $control_parent" >&2
				exit 1
			fi
			if sudo -n -u "$eval_user" /usr/bin/touch "$control_path" 2>/dev/null; then
				echo "Isolated Vally user can create repository control $control_path" >&2
				exit 1
			fi
		fi
	done
	for probe_number in 1 2; do
		worktree_probe="$eval_home/worktree-probe-$probe_number"
		sudo -n -u "$eval_user" env \
			HOME="$eval_home" \
			GIT_CONFIG_GLOBAL="$trusted_git_config" \
			GIT_CONFIG_NOSYSTEM=1 \
			git -C "$workspace_root" worktree add --detach "$worktree_probe" HEAD
		sudo -n -u "$eval_user" env \
			HOME="$eval_home" \
			GIT_CONFIG_GLOBAL="$trusted_git_config" \
			GIT_CONFIG_NOSYSTEM=1 \
			git -C "$workspace_root" worktree remove --force "$worktree_probe"
	done
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
	if [ "$(
		sudo -n -u "$eval_user" env \
			HOME="$eval_home" \
			GIT_CONFIG_GLOBAL="$trusted_git_config" \
			GIT_CONFIG_NOSYSTEM=1 \
			git -C "$workspace_root" config --get core.hooksPath
	)" != "$trusted_git_hooks" ]; then
		echo "Evaluator workspace is not using the trusted Git hooks path" >&2
		exit 1
	fi
	if sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$workspace_root" config --local alias.runtime-probe status 2>/dev/null; then
		echo "Isolated Vally user can replace the evaluator Git configuration" >&2
		exit 1
	fi
	if [ -n "${TRUSTED_UPSTREAM_URL:-}" ]; then
		mapfile -t evaluator_remotes < <(
			sudo -n -u "$eval_user" env \
				HOME="$eval_home" \
				GIT_CONFIG_GLOBAL="$trusted_git_config" \
				GIT_CONFIG_NOSYSTEM=1 \
				git -C "$workspace_root" remote
		)
		if [ "${#evaluator_remotes[@]}" -ne 1 ] ||
			[ "${evaluator_remotes[0]}" != "upstream" ]; then
			echo "Evaluator workspace contains an unexpected Git remote" >&2
			exit 1
		fi
		evaluator_upstream=$(
			sudo -n -u "$eval_user" env \
				HOME="$eval_home" \
				GIT_CONFIG_GLOBAL="$trusted_git_config" \
				GIT_CONFIG_NOSYSTEM=1 \
				git -C "$workspace_root" remote get-url upstream
		)
		if [ "$evaluator_upstream" != "$TRUSTED_UPSTREAM_URL" ]; then
			echo "Evaluator workspace is not using the trusted upstream repository" >&2
			exit 1
		fi
	else
		if sudo -n -u "$eval_user" env \
			HOME="$eval_home" \
			GIT_CONFIG_GLOBAL="$trusted_git_config" \
			GIT_CONFIG_NOSYSTEM=1 \
			git -C "$workspace_root" remote | grep -q .; then
			echo "Evaluator workspace retained a checkout Git remote" >&2
			exit 1
		fi
	fi
	probe_ref="refs/heads/vally-runtime-probe"
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$workspace_root" update-ref "$probe_ref" HEAD
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$workspace_root" pack-refs --all --prune
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$workspace_root" update-ref -d "$probe_ref"
	sudo -n -u "$eval_user" env \
		HOME="$eval_home" \
		GIT_CONFIG_GLOBAL="$trusted_git_config" \
		GIT_CONFIG_NOSYSTEM=1 \
		git -C "$workspace_root" pack-refs --all --prune
	if [ "$(stat -c '%u:%g:%a' "$GITHUB_WORKSPACE")" != "$original_workspace_stat" ] ||
		[ "$(stat -c '%u:%g:%a' "$GITHUB_WORKSPACE/.git")" != "$original_git_stat" ]; then
		echo "Runtime setup changed the original Actions checkout permissions" >&2
		exit 1
	fi

	# Keep configuration immutable to the evaluator. Only runtime output that the
	# CLI never reads as policy is writable across its headless sessions.
	sudo -n install -d -o root -g "$eval_user" -m 1770 "$trusted_copilot_home"
	sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700 \
		"$trusted_copilot_home/logs" \
		"$trusted_copilot_home/session-state"
	sudo -n install -d -o root -g root -m 555 \
		"$trusted_copilot_home/installed-plugins"
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
		"$trusted_copilot_home/config.json" \
		"$trusted_copilot_home/installed-plugins" \
		"$trusted_copilot_home/settings.json"; do
		if sudo -n -u "$eval_user" /usr/bin/test -w "$protected_path"; then
			echo "Isolated Vally user can modify protected Copilot path $protected_path" >&2
			exit 1
		fi
	done
	for protected_path in \
		"$trusted_copilot_home/config.json" \
		"$trusted_copilot_home/installed-plugins" \
		"$trusted_copilot_home/settings.json"; do
		remove_command=(/bin/rm "$protected_path")
		if [ -d "$protected_path" ]; then
			remove_command=(/usr/bin/rmdir "$protected_path")
		fi
		if sudo -n -u "$eval_user" "${remove_command[@]}" 2>/dev/null; then
			echo "Isolated Vally user can remove protected Copilot path $protected_path" >&2
			exit 1
		fi
		if sudo -n -u "$eval_user" /bin/mv \
			"$protected_path" "$protected_path.runtime-probe" 2>/dev/null; then
			echo "Isolated Vally user can rename protected Copilot path $protected_path" >&2
			exit 1
		fi
		replacement_path="$trusted_copilot_home/runtime-policy-replacement"
		sudo -n -u "$eval_user" /usr/bin/touch "$replacement_path"
		if sudo -n -u "$eval_user" /bin/mv -f \
			"$replacement_path" "$protected_path" 2>/dev/null; then
			echo "Isolated Vally user can replace protected Copilot path $protected_path" >&2
			exit 1
		fi
		sudo -n -u "$eval_user" /bin/rm -f "$replacement_path"
	done
	for writable_path in \
		"$trusted_copilot_home/logs" \
		"$trusted_copilot_home/session-state"; do
		if ! sudo -n -u "$eval_user" /usr/bin/test -w "$writable_path"; then
			echo "Isolated Vally user cannot write Copilot runtime state $writable_path" >&2
			exit 1
		fi
	done
	for state_name in \
		session-store.db \
		session-store.db-shm \
		session-store.db-wal; do
		state_path="$trusted_copilot_home/$state_name"
		sudo -n -u "$eval_user" /usr/bin/touch "$state_path"
		sudo -n -u "$eval_user" /bin/rm "$state_path"
		sudo -n -u "$eval_user" /usr/bin/touch "$state_path"
		sudo -n -u "$eval_user" /bin/rm "$state_path"
	done

	{
		echo '#!/usr/bin/env bash'
		echo 'set -euo pipefail'
		printf 'eval_user=%q\n' "$eval_user"
		printf 'eval_home=%q\n' "$eval_home"
		printf 'workspace_root=%q\n' "$workspace_root"
		printf 'trusted_git_config=%q\n' "$trusted_git_config"
		cat <<'EOF'
: "${COPILOT_GITHUB_TOKEN:?COPILOT_GITHUB_TOKEN is required}"
: "${COPILOT_CLI_PATH:?COPILOT_CLI_PATH is required}"
: "${TRUSTED_COPILOT_CLI_PATH:?TRUSTED_COPILOT_CLI_PATH is required}"
: "${TRUSTED_COPILOT_HOME:?TRUSTED_COPILOT_HOME is required}"
umask 0002
cd "$workspace_root"
child_env=(
	"HOME=$eval_home"
	"TMPDIR=$eval_home/tmp"
	"PATH=$PATH"
	"GITHUB_WORKSPACE=$workspace_root"
	"COPILOT_CLI_PATH=$COPILOT_CLI_PATH"
	"TRUSTED_COPILOT_CLI_PATH=$TRUSTED_COPILOT_CLI_PATH"
	"TRUSTED_COPILOT_HOME=$TRUSTED_COPILOT_HOME"
	"EVALUATE_USE_HOST_COPILOT_HOME=1"
	"GIT_CONFIG_GLOBAL=$trusted_git_config"
	"GIT_CONFIG_NOSYSTEM=1"
)
for name in CI GITHUB_ACTIONS RUNNER_TEMP \
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
	test_workspace=$(
		COPILOT_GITHUB_TOKEN=probe \
			COPILOT_CLI_PATH="$copilot_wrapper" \
			TRUSTED_COPILOT_CLI_PATH="${copilot_runtimes[0]}" \
			TRUSTED_COPILOT_HOME="$trusted_copilot_home" \
			"$vally_runner" /usr/bin/printenv GITHUB_WORKSPACE
	)
	test_working_directory=$(
		COPILOT_GITHUB_TOKEN=probe \
			COPILOT_CLI_PATH="$copilot_wrapper" \
			TRUSTED_COPILOT_CLI_PATH="${copilot_runtimes[0]}" \
			TRUSTED_COPILOT_HOME="$trusted_copilot_home" \
			"$vally_runner" /bin/pwd
	)
	if [ "$test_workspace" != "$workspace_root" ] ||
		[ "$test_working_directory" != "$workspace_root" ]; then
		echo "Isolated Vally process is not using the evaluator workspace" >&2
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
	workspace_root=${GITHUB_WORKSPACE:-$(pwd)}
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
	{
		echo '#!/usr/bin/env bash'
		echo 'set -euo pipefail'
		printf 'workspace_root=%q\n' "$workspace_root"
		cat <<'EOF'
cd "$workspace_root"
export GITHUB_WORKSPACE="$workspace_root"
export EVALUATE_USE_HOST_COPILOT_HOME=1
exec "$@"
EOF
	} > "$vally_runner"
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
	echo "workspace_root=$workspace_root"
} >> "$github_output"
