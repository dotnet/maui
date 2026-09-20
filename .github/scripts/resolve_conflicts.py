#!/usr/bin/env python3
"""Trusted orchestration for /resolve conflicts; no third-party Python packages."""

import argparse
import hashlib
import html
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import stat
import subprocess
import sys
import tempfile
from datetime import datetime, timezone


MODEL = "gpt-6-astra"
MARKER = "<!-- PR Conflict Resolution -->"
ROLES = {"admin", "maintain", "write"}
MAX_FILE = 1024 * 1024
MAX_ARTIFACT = 5 * MAX_FILE
SHA = re.compile(r"[0-9a-f]{40}")
REPOSITORY = re.compile(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+")
MERGE_MARKER = re.compile(r"(?m)^(?:<{7}|={7}|>{7}|\|{7})(?: |$)")


def run(args, cwd=None, env=None, check=True, data=None):
    result = subprocess.run(
        args, cwd=cwd, env=env, input=data, stdout=subprocess.PIPE,
        stderr=subprocess.PIPE, check=False,
    )
    if check and result.returncode:
        # Do not reflect arbitrary subprocess output as runner control commands.
        raise RuntimeError(f"{args[0]} failed ({result.returncode}): "
                           f"{result.stderr.decode(errors='replace')[-4000:]}")
    return result


def git(repo, *args, check=True, env=None, data=None):
    safe_env = os.environ.copy() if env is None else env.copy()
    safe_env.update(GIT_CONFIG_NOSYSTEM="1", GIT_CONFIG_GLOBAL=os.devnull,
                    GIT_TERMINAL_PROMPT="0", GIT_NO_REPLACE_OBJECTS="1")
    return run(
        ["git", "--literal-pathspecs",
         "-c", "core.hooksPath=" + os.devnull, "-c", "core.autocrlf=false",
         "-c", "commit.gpgsign=false", "-c", "protocol.file.allow=never",
         "-c", "user.name=github-actions[bot]",
         "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
         *args], cwd=repo, env=safe_env, check=check, data=data,
    )


def text(result):
    return result.stdout.decode("utf-8").strip()


def gh(endpoint, payload=None, paginate=False):
    args = ["gh", "api", endpoint]
    if paginate:
        args += ["--paginate", "--slurp"]
    if payload is not None:
        args += ["--input", "-"]
    return json.loads(run(args, data=None if payload is None else
                          json.dumps(payload).encode()).stdout)


def output(name, value):
    value = str(value)
    if "\n" in value or "\r" in value:
        raise ValueError("Multiline job output is not allowed")
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as stream:
            stream.write(f"{name}={value}\n")


def read_regular(path, limit):
    path = Path(path)
    if not stat.S_ISREG(path.lstat().st_mode) or path.stat().st_size > limit:
        raise ValueError(f"Expected a bounded regular file: {path.name}")
    return path.read_bytes()


def validate_context(context):
    for key in ("repository", "head_repository"):
        if (not REPOSITORY.fullmatch(context[key]) or
                any(part in {".", ".."} for part in context[key].split("/"))):
            raise ValueError("Invalid repository")
    for key in ("head_sha", "base_sha"):
        if not SHA.fullmatch(context[key]):
            raise ValueError("Invalid commit")
    for key in ("head_ref", "base_ref"):
        if run(["git", "check-ref-format", "refs/heads/" + context[key]],
               check=False).returncode:
            raise ValueError("Invalid branch")
    if not isinstance(context["pr"], int) or context["pr"] < 1:
        raise ValueError("Invalid PR number")
    datetime.strptime(context["date"], "%Y-%m-%dT%H:%M:%SZ")
    return context


def get_permission(repository, actor):
    from urllib.parse import quote
    return gh(f"repos/{repository}/collaborators/{quote(actor, safe='')}/permission")["permission"]


def prepare(event, repository, actor, ref):
    if repository != "dotnet/maui":
        raise ValueError("This workflow only operates on dotnet/maui")
    default_branch = event["repository"]["default_branch"]
    if "comment" in event:
        comment = event["comment"]
        if (event.get("action") != "created" or
                not event.get("issue", {}).get("pull_request") or
                not re.fullmatch(r"/resolve[ \t]+conflicts\s*", comment["body"]) or
                comment["user"]["type"] != "User" or
                comment["author_association"] not in {"OWNER", "MEMBER", "COLLABORATOR"}):
            return None
        actor = comment["user"]["login"]
        number = event["issue"]["number"]
    else:
        if ref != "refs/heads/" + default_branch:
            raise ValueError("Manual dispatch must use the default branch")
        number = int(event["inputs"]["pr_number"])
    if get_permission(repository, actor) not in ROLES:
        return None
    pr = gh(f"repos/{repository}/pulls/{number}")
    if pr["state"] != "open" or pr["merged"] or not pr["head"]["repo"]:
        raise ValueError("An open PR with an existing head repository is required")
    context = validate_context({
        "repository": repository, "pr": number, "actor": actor,
        "author": pr["user"]["login"], "head_repository": pr["head"]["repo"]["full_name"],
        "head_ref": pr["head"]["ref"], "head_sha": pr["head"]["sha"],
        "base_ref": pr["base"]["ref"], "base_sha": pr["base"]["sha"],
        "date": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "default_branch": default_branch,
    })
    try:
        assert_editable(context, pr)
    except ValueError as error:
        context["blocker"] = str(error)
    return context


def assert_editable(context, pr):
    if pr["base"]["repo"]["full_name"] != context["repository"]:
        raise ValueError("Unexpected target repository")
    if context["head_repository"] != context["repository"] and not pr["maintainer_can_modify"]:
        raise ValueError("The fork must allow maintainer edits")
    if (context["head_repository"] == context["repository"] and
            context["head_ref"] in {context["default_branch"], context["base_ref"]}):
        raise ValueError("Refusing to push to the default or target branch")


def new_merge(context, directory):
    directory = Path(directory)
    directory.mkdir()
    git(directory, "init", "--quiet")
    git(directory, "remote", "add", "origin", "https://github.com/" + context["repository"] + ".git")
    for sha in (context["head_sha"], context["base_sha"]):
        git(directory, "fetch", "--quiet", "--no-tags", "origin", sha)
    git(directory, "checkout", "--quiet", "--detach", context["head_sha"])
    return merge(directory, context)


def merge(directory, context):
    result = git(directory, "merge", "--no-commit", "--no-ff", context["base_sha"], check=False)
    conflicts = git(directory, "diff", "--name-only", "--diff-filter=U", "-z").stdout
    paths = [p.decode("utf-8") for p in conflicts.split(b"\0") if p]
    if result.returncode not in (0, 1) or (result.returncode and not paths):
        raise RuntimeError("Git could not construct the automatic merge")
    if len(paths) > 50:
        raise ValueError("More than 50 conflicting paths require manual resolution")
    for path in paths:
        validate_path(path)
    return paths


def validate_path(path):
    parts = PurePosixPath(path).parts
    if (not parts or path.startswith("/") or "\\" in path or
            any(p in {".", "..", ".git", ".github"} for p in parts) or
            any(ord(c) < 32 for c in path)):
        raise ValueError("Unsafe or automation conflict path requires manual resolution")


def conflict_versions(repo, paths):
    versions = {}
    for path in paths:
        entries = git(repo, "ls-files", "-u", "-z", "--", path).stdout.split(b"\0")
        versions[path] = {}
        for entry in filter(None, entries):
            metadata, _ = entry.decode("utf-8").split("\t", 1)
            mode, sha, stage = metadata.split()
            if mode not in {"100644", "100755"}:
                raise ValueError("Symlink/submodule conflicts require manual resolution")
            content = git(repo, "cat-file", "blob", sha).stdout
            if len(content) > MAX_FILE or b"\0" in content:
                raise ValueError("Binary/oversized conflicts require manual resolution")
            versions[path][stage] = content.decode("utf-8")
    return versions


def manifest(directory):
    result = {}
    for root, directories, files in os.walk(directory):
        for name in directories + files:
            path = Path(root, name)
            if path.is_symlink():
                raise ValueError("The agent introduced a symlink")
        for name in files:
            path = Path(root, name)
            result[path.relative_to(directory).as_posix()] = hashlib.sha256(path.read_bytes()).hexdigest()
    return result


def solve(context, repo, agent, artifact, skill):
    paths = new_merge(context, repo)
    if not paths:
        output("state", "no-conflicts")
        return
    versions = conflict_versions(repo, paths)
    automatic_tree = text(git(repo, "rev-parse", "AUTO_MERGE^{tree}"))
    git(repo, "read-tree", "--reset", "-u", automatic_tree)
    agent = Path(agent)
    agent.mkdir()

    def ignore(directory, names):
        return [n for n in names if n in {".git", ".github", ".copilot", "AGENTS.md", "CLAUDE.md"}
                or Path(directory, n).is_symlink()]

    source = agent / "source"
    shutil.copytree(repo, source, ignore=ignore)
    before = manifest(source)
    trusted_skill = agent / ".github/skills/resolve-pr-conflicts/SKILL.md"
    trusted_skill.parent.mkdir(parents=True)
    shutil.copyfile(skill, trusted_skill)
    for index, path in enumerate(paths):
        location = agent / "versions" / str(index)
        location.mkdir(parents=True)
        for stage, name in (("1", "ancestor"), ("2", "pr"), ("3", "target")):
            if stage in versions[path]:
                (location / name).write_text(versions[path][stage], encoding="utf-8", newline="")
    handoff = dict(context, conflicts=[{"path": p, "versions": f"versions/{i}"} for i, p in enumerate(paths)])
    (agent / "context.json").write_text(json.dumps(handoff), encoding="utf-8")
    prompt = ("Read and follow .github/skills/resolve-pr-conflicts/SKILL.md. "
              "Resolve the conflicts in context.json using only file reading/editing tools. "
              "Write summary.txt. Never execute code, delegate, change model, or publish.")
    env = os.environ.copy()
    for key in ("GH_TOKEN", "GITHUB_TOKEN"):
        env.pop(key, None)
    run([
        "copilot", "--model", MODEL, "--reasoning-effort", "high",
        "--no-custom-instructions", "--disable-builtin-mcps", "--no-auto-update",
        "--disallow-temp-dir",
        "--available-tools", "view", "rg", "glob", "apply_patch", "skill",
        "--allow-tool", "read", "--allow-tool", "write",
        "--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN",
        "-p", prompt,
    ], cwd=agent, env=env)
    after = manifest(source)
    changed = {p for p in before.keys() | after.keys() if before.get(p) != after.get(p)}
    if changed - set(paths):
        raise ValueError("The agent edited files outside the conflict allowlist")
    files = {}
    for path in paths:
        candidate = source / path
        files[path] = (read_regular(candidate, MAX_FILE).decode("utf-8")
                       if candidate.exists() else None)
    summary = read_regular(agent / "summary.txt", 8000).decode("utf-8")
    if not summary.strip() or len(summary) > 2000:
        raise ValueError("A concise resolution summary is required")
    artifact = Path(artifact)
    artifact.mkdir()
    data = json.dumps({"files": files, "summary": summary}).encode()
    if len(data) > MAX_ARTIFACT:
        raise ValueError("Resolution artifact is too large")
    (artifact / "resolution.json").write_bytes(data)
    # Reconstruct in another repository; never accept the agent's index/commit/config.
    verify = Path(repo).parent / "verified"
    sha, _ = materialize(context, verify, artifact / "resolution.json")
    output("candidate", sha)
    output("state", "resolved")


def materialize(context, repo, artifact):
    resolution = json.loads(read_regular(artifact, MAX_ARTIFACT))
    if (not isinstance(resolution, dict) or set(resolution) != {"files", "summary"} or
            not isinstance(resolution["files"], dict)):
        raise ValueError("Invalid resolution artifact schema")
    paths = new_merge(context, repo)
    conflict_versions(repo, paths)
    if not paths or set(resolution["files"]) != set(paths):
        raise ValueError("Resolution paths do not match the actual conflicts")
    if (not isinstance(resolution["summary"], str) or not resolution["summary"].strip() or
            len(resolution["summary"]) > 2000):
        raise ValueError("Invalid resolution summary")
    git(repo, "read-tree", "--reset", "-u", "AUTO_MERGE")
    for path in paths:
        value = resolution["files"][path]
        file = Path(repo, path)
        if value is None:
            if file.exists():
                file.unlink()
        else:
            if (not isinstance(value, str) or len(value.encode("utf-8")) > MAX_FILE or
                    "\0" in value or MERGE_MARKER.search(value)):
                raise ValueError("Unresolved markers or invalid conflict contents")
            file.parent.mkdir(parents=True, exist_ok=True)
            file.write_text(value, encoding="utf-8", newline="")
        git(repo, "add", "-A", "--", path)
    tree = text(git(repo, "write-tree"))
    env = os.environ.copy()
    env.update(GIT_AUTHOR_DATE=context["date"], GIT_COMMITTER_DATE=context["date"],
               GIT_AUTHOR_NAME="github-actions[bot]", GIT_COMMITTER_NAME="github-actions[bot]",
               GIT_AUTHOR_EMAIL="41898282+github-actions[bot]@users.noreply.github.com",
               GIT_COMMITTER_EMAIL="41898282+github-actions[bot]@users.noreply.github.com")
    message = (f"Merge {context['base_ref']} into PR #{context['pr']}: resolve conflicts\n\n"
               "Co-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>\n")
    sha = text(git(repo, "commit-tree", tree, "-p", context["head_sha"],
                   "-p", context["base_sha"], env=env, data=message.encode()))
    git(repo, "checkout", "--quiet", "--detach", sha)
    return sha, resolution


def without_tokens():
    return {k: v for k, v in os.environ.items() if not (
        "TOKEN" in k.upper() or "SECRET" in k.upper() or k.startswith("ACTIONS_") or
        k.startswith(("GIT_CONFIG", "COPILOT_PAT_")) or k.endswith("_PAT") or
        k in {"GITHUB_ENV", "GITHUB_OUTPUT", "GITHUB_PATH", "GITHUB_STATE",
              "GITHUB_STEP_SUMMARY", "BASH_ENV", "GIT_ASKPASS", "SSH_ASKPASS",
              "SSH_AUTH_SOCK", "GIT_SSH_COMMAND", "GIT_PROXY_COMMAND"}
    )}


def build(repo, log):
    env = without_tokens()
    with open(log, "wb") as stream:
        for args in (["dotnet", "tool", "restore"],
                     ["dotnet", "cake", "--target=dotnet-build", "--configuration=Release"]):
            process = subprocess.run(args, cwd=repo, env=env, stdout=stream,
                                     stderr=subprocess.STDOUT, check=False)
            if process.returncode:
                raise RuntimeError(f"{' '.join(args)} failed; see the build log")
    if git(repo, "status", "--porcelain", "--untracked-files=no").stdout:
        raise RuntimeError("The build changed tracked inputs; refusing to publish")


def revalidate(context):
    if get_permission(context["repository"], context["actor"]) not in ROLES:
        raise ValueError("The requester no longer has write access")
    pr = gh(f"repos/{context['repository']}/pulls/{context['pr']}")
    if pr["state"] != "open" or pr["merged"] or not pr["head"]["repo"]:
        raise ValueError("The PR is no longer open/editable")
    assert_editable(context, pr)
    for side, repository in (("head", context["head_repository"]), ("base", context["repository"])):
        if (pr[side]["sha"] != context[f"{side}_sha"] or
                pr[side]["ref"] != context[f"{side}_ref"] or
                pr[side]["repo"]["full_name"] != repository):
            raise ValueError("The PR head or target changed; rerun /resolve conflicts")
    return pr


def push(context, repo, candidate, build_result, token):
    if build_result != "success":
        raise ValueError("Both independent builds must succeed before pushing")
    if not SHA.fullmatch(candidate):
        raise ValueError("Invalid candidate SHA")
    revalidate(context)
    if not token:
        raise ValueError("No PR-branch push credential is configured")
    parents = text(git(repo, "show", "-s", "--format=%P", candidate)).split()
    if parents != [context["head_sha"], context["base_sha"]]:
        raise ValueError("Candidate is not the expected non-rewriting merge")
    url = "https://github.com/" + context["head_repository"] + ".git"
    for remote, ref, expected in (
        (url, context["head_ref"], context["head_sha"]),
        ("https://github.com/" + context["repository"] + ".git", context["base_ref"], context["base_sha"]),
    ):
        actual = text(git(repo, "ls-remote", "--exit-code", remote, "refs/heads/" + ref)).split()
        if not actual or actual[0] != expected:
            raise ValueError("A branch moved during validation; rerun /resolve conflicts")
    import base64
    env = without_tokens()
    env.update(GIT_CONFIG_COUNT="2", GIT_CONFIG_KEY_0="http.https://github.com/.extraheader",
               GIT_CONFIG_VALUE_0="AUTHORIZATION: basic " + base64.b64encode(
                   ("x-access-token:" + token).encode()).decode(),
               GIT_CONFIG_KEY_1="credential.helper", GIT_CONFIG_VALUE_1="",
               EXPECTED_HEAD=context["head_sha"], EXPECTED_CANDIDATE=candidate,
               EXPECTED_REF="refs/heads/" + context["head_ref"])
    # Check the server-advertised old OID too: a normal push alone could accept a
    # concurrent rewind to an ancestor. receive-pack then enforces that same OID.
    with tempfile.TemporaryDirectory(prefix="conflict-push-hooks-") as directory:
        hook = Path(directory, "pre-push")
        hook.write_text(
            '#!/bin/sh\n'
            'count=0\n'
            'while read -r local_ref local_oid remote_ref remote_oid; do\n'
            '  count=$((count + 1))\n'
            '  if [ "$local_oid" != "$EXPECTED_CANDIDATE" ] || '
            '[ "$remote_oid" != "$EXPECTED_HEAD" ] || '
            '[ "$remote_ref" != "$EXPECTED_REF" ]; then\n'
            '    echo "PR branch changed before push; rerun /resolve conflicts" >&2\n'
            '    exit 1\n'
            '  fi\n'
            'done\n'
            '[ "$count" -eq 1 ]\n', encoding="utf-8")
        hook.chmod(0o700)
        git(repo, "-c", "core.hooksPath=" + directory, "push", "--porcelain", url,
            candidate + ":refs/heads/" + context["head_ref"], env=env)


def render(context, state, build_result, summary, paths, candidate, run_url):
    escape = lambda s: html.escape(str(s)).replace("@", "&#64;")
    short = context["head_sha"][:7]
    commit_url = f"https://github.com/{context['repository']}/commit/{context['head_sha']}"
    if state == "pushed":
        outcome = f"Pushed merge commit [`{candidate[:7]}`](https://github.com/{context['head_repository']}/commit/{candidate})."
        followup = "The PR remains open for normal review and CI."
    elif state == "no-conflicts":
        outcome = "No merge conflicts found. Nothing was pushed."
        followup = "No conflict-resolution update was needed."
    else:
        outcome = "Nothing was pushed. Conflict resolution or validation did not complete successfully."
        followup = "Inspect the linked run, address the blocker, and retry."
    details = "<br/>".join(escape(summary).splitlines()) or "No agent resolution was produced."
    files = "\n".join(f"- <code>{escape(p)}</code>" for p in paths) or "No resolved files."
    return f"""{MARKER}

## PR Conflict Resolution

> @{escape(context['author'])} &#x2014; conflict resolution for commit [`{short}`]({commit_url}).

<p align="left">
  <img alt="Scope PR conflicts" src="https://img.shields.io/badge/Scope-PR%20conflicts-1f6feb?labelColor=30363d&amp;style=flat-square">
  <img alt="Commit {short}" src="https://img.shields.io/badge/Commit-{short}-1f6feb?labelColor=30363d&amp;style=flat-square">
</p>

---

<details>
<summary><strong>&#x1F527; Conflict Resolution</strong> &#x2014; click to expand</summary>
<br/>

{outcome}

Target: <code>{escape(context['base_ref'])}</code> at [`{context['base_sha'][:7]}`](https://github.com/{context['repository']}/commit/{context['base_sha']}).

<details>
<summary><strong>&#x1F4DD; Changes</strong></summary>
<br/>

{details}

{files}

</details>

---

<details>
<summary><strong>&#x1F9EA; Build validation</strong></summary>
<br/>

Windows and macOS Release builds: **{escape(build_result)}**.
Both must pass before pushing. [Workflow and build logs]({run_url}).

</details>

</details>

---

<details>
<summary><strong>&#x1F4CC; Follow-up</strong> &#x2014; click to expand</summary>
<br/>

{followup}

To retry or refresh, comment `/resolve conflicts`.

</details>
"""


def publish_comment(context, body, run_url):
    endpoint = f"repos/{context['repository']}/issues/{context['pr']}/comments"
    comments = [c for page in gh(endpoint + "?per_page=100", paginate=True) for c in page]
    older = [c for c in comments if c["user"]["login"] == "github-actions[bot]"
             and c["body"].startswith(MARKER)]
    current = next((c for c in older if f"]({run_url})" in c["body"]), None)
    if current:
        # --method is explicit because gh api with an input defaults to POST.
        run(["gh", "api", "--method", "PATCH",
             f"repos/{context['repository']}/issues/comments/{current['id']}", "--input", "-"],
            data=json.dumps({"body": body}).encode())
    else:
        gh(endpoint, {"body": body})
    for comment in older:
        if current and comment["id"] == current["id"]:
            continue
        gh("graphql", {
            "query": "mutation($id:ID!){minimizeComment(input:{subjectId:$id,classifier:OUTDATED}){minimizedComment{isMinimized}}}",
            "variables": {"id": comment["node_id"]},
        })


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("phase", choices=["prepare", "solve", "build", "publish"])
    parser.add_argument("--root", type=Path, required=True)
    args = parser.parse_args()
    args.root.mkdir(parents=True, exist_ok=True)
    if args.phase == "prepare":
        event = json.loads(Path(os.environ["GITHUB_EVENT_PATH"]).read_text())
        context = prepare(event, os.environ["GITHUB_REPOSITORY"], os.environ["GITHUB_ACTOR"],
                          os.environ["GITHUB_REF"])
        output("authorized", "true" if context else "false")
        if context:
            output("eligible", "false" if context.get("blocker") else "true")
            output("context", json.dumps(context, separators=(",", ":")))
        return
    context = validate_context(json.loads(os.environ["CONFLICT_CONTEXT"]))
    artifact = args.root / "artifact/resolution.json"
    if args.phase == "solve":
        solve(context, args.root / "merge", args.root / "agent", artifact.parent,
              Path(__file__).resolve().parents[1] / "skills/resolve-pr-conflicts/SKILL.md")
    elif args.phase == "build":
        candidate, _ = materialize(context, args.root / "build", artifact)
        if candidate != os.environ["EXPECTED_CANDIDATE"]:
            raise ValueError("The reconstructed commit differs from the candidate")
        build(args.root / "build", args.root / "build.log")
    else:
        state = os.environ["RESOLVE_STATE"]
        builds = os.environ["BUILD_RESULT"]
        summary, paths, candidate = "", [], ""
        failure = None
        try:
            if context.get("blocker"):
                raise ValueError(context["blocker"])
            if state == "resolved" and builds == "success":
                candidate, resolution = materialize(context, args.root / "publish", artifact)
                summary, paths = resolution["summary"], list(resolution["files"])
                if candidate != os.environ["EXPECTED_CANDIDATE"]:
                    raise ValueError("The publication candidate differs from the built commit")
                push(context, args.root / "publish", candidate, builds,
                     os.environ.get("PUSH_TOKEN", ""))
                state = "pushed"
            elif state != "no-conflicts":
                raise RuntimeError(f"Resolution job: {os.environ['RESOLVE_RESULT']}; build jobs: {builds}")
        except (RuntimeError, ValueError, KeyError, OSError) as error:
            state, summary, failure = "blocked", str(error), error
        run_url = f"https://github.com/{context['repository']}/actions/runs/{os.environ['GITHUB_RUN_ID']}"
        publish_comment(context, render(context, state, builds, summary, paths, candidate, run_url), run_url)
        if failure:
            raise failure


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, ValueError, KeyError, OSError) as error:
        print("Conflict resolution blocked: " + str(error).replace("\r", "").replace("\n", " "), file=sys.stderr)
        sys.exit(1)
