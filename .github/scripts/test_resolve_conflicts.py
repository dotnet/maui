#!/usr/bin/env python3
"""Hermetic regression tests: python3 -m unittest discover -s .github/scripts -p test_resolve_conflicts.py."""

import base64
import copy
from datetime import datetime
from html.parser import HTMLParser
import importlib.util
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


SCRIPT = Path(__file__).with_name("resolve_conflicts.py")
ROOT = SCRIPT.resolve().parents[2]
SPEC = importlib.util.spec_from_file_location("resolve_conflicts_under_test", SCRIPT)
resolver = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(resolver)
DATE = "2026-09-19T12:00:00Z"
ORIGINAL = "one\ntwo\nthree\nbase\nfive\nsix\nseven\n"
PR_TEXT = ORIGINAL.replace("base", "PR")
TARGET_TEXT = ORIGINAL.replace("base", "target")
RESOLVED = ORIGINAL.replace("base", "PR and target")


def context():
    return {
        "repository": "dotnet/maui",
        "head_repository": "contributor/maui",
        "head_ref": "topic",
        "base_ref": "main",
        "head_sha": "1" * 40,
        "base_sha": "2" * 40,
        "date": DATE,
        "pr": 42,
        "actor": "maintainer",
        "author": "contributor",
        "default_branch": "main",
    }


def pull_request(snapshot):
    return {
        "state": "open",
        "merged": False,
        "maintainer_can_modify": True,
        "user": {"login": snapshot["author"]},
        "head": {
            "sha": snapshot["head_sha"],
            "ref": snapshot["head_ref"],
            "repo": {"full_name": snapshot["head_repository"]},
        },
        "base": {
            "sha": snapshot["base_sha"],
            "ref": snapshot["base_ref"],
            "repo": {"full_name": snapshot["repository"]},
        },
    }


def comment_event(body="/resolve conflicts"):
    return {
        "repository": {"default_branch": "main"},
        "action": "created",
        "issue": {"number": 42, "pull_request": {"url": "https://api.github.com/repos/dotnet/maui/pulls/42"}},
        "comment": {
            "body": body,
            "user": {"login": "maintainer", "type": "User"},
            "author_association": "MEMBER",
        },
    }


class FakeGitHub:
    """Only explicitly enumerated API requests are allowed; nothing is sent."""

    def __init__(self, snapshot, permission="write"):
        self.snapshot = snapshot
        self.permission = permission
        self.pr = pull_request(snapshot)
        self.calls = []
        self.reports = []

    def __call__(self, endpoint, payload=None, paginate=False):
        self.calls.append((endpoint, copy.deepcopy(payload), paginate))
        prefix = f"repos/{self.snapshot['repository']}"
        if endpoint.startswith(prefix + "/collaborators/") and endpoint.endswith("/permission"):
            assert payload is None
            return {"permission": self.permission}
        if endpoint == f"{prefix}/pulls/{self.snapshot['pr']}":
            assert payload is None
            return copy.deepcopy(self.pr)
        comments = f"{prefix}/issues/{self.snapshot['pr']}/comments"
        if endpoint == comments + "?per_page=100":
            assert paginate and payload is None
            return [[]]
        if endpoint == comments and payload is not None:
            self.reports.append(payload["body"])
            return {"id": 1}
        raise AssertionError(f"Unexpected GitHub request: {endpoint}")


class HermeticCase(unittest.TestCase):
    def setUp(self):
        # Keep every fixture under the checkout, not the system temporary folder.
        self.temporary = tempfile.TemporaryDirectory(prefix=".resolve-tests-", dir=ROOT)
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.home = self.root / "home"
        self.home.mkdir()
        env = {
            name: value for name, value in os.environ.items()
            if name in {"PATH", "SYSTEMROOT", "SystemRoot", "WINDIR", "COMSPEC", "PATHEXT"}
        }
        env.update(
            HOME=str(self.home), USERPROFILE=str(self.home),
            TMPDIR=str(self.root), TEMP=str(self.root), TMP=str(self.root),
            LC_ALL="C", LANG="C", GIT_CONFIG_NOSYSTEM="1", GIT_CONFIG_GLOBAL=os.devnull,
            GIT_TERMINAL_PROMPT="0", GIT_AUTHOR_DATE=DATE, GIT_COMMITTER_DATE=DATE,
        )
        environment = patch.dict(os.environ, env, clear=True)
        environment.start()
        self.addCleanup(environment.stop)
        self.start_patch(patch.object(tempfile, "tempdir", str(self.root)))
        self.original_run = resolver.run
        self.start_patch(patch.object(resolver, "run", side_effect=self.local_only))
        self.counter = 0

    def start_patch(self, patcher):
        result = patcher.start()
        self.addCleanup(patcher.stop)
        return result

    def local_only(self, args, **kwargs):
        if args[0] != "git" or any(
            str(arg).startswith(("https://", "http://", "ssh://", "git@"))
            for arg in args
        ):
            raise AssertionError(f"Unexpected external command: {args[0]}")
        return self.original_run(args, **kwargs)

    def fresh(self, name):
        self.counter += 1
        return self.root / f"{name}-{self.counter}"

    def github(self, snapshot=None, permission="write"):
        api = FakeGitHub(snapshot or context(), permission)
        self.start_patch(patch.object(resolver, "gh", side_effect=api))
        return api

    def invoke_main(self, phase, root, snapshot, **values):
        env = {
            "CONFLICT_CONTEXT": json.dumps(snapshot),
            "GITHUB_RUN_ID": "123",
            "RESOLVE_STATE": "resolved",
            "RESOLVE_RESULT": "success",
            "BUILD_RESULT": "success",
            "EXPECTED_CANDIDATE": "",
            "PUSH_TOKEN": "fixture-push-credential",
        }
        env.update(values)
        with patch.dict(os.environ, env), patch.object(
            sys, "argv", [str(SCRIPT), phase, "--root", str(root)]
        ):
            resolver.main()


class AuthorizationTests(HermeticCase):
    def test_exact_command_accepts_each_current_write_role_and_pins_pr(self):
        api = self.github()
        for role in ("write", "maintain", "admin"):
            for association in ("OWNER", "MEMBER", "COLLABORATOR"):
                with self.subTest(role=role, association=association):
                    api.permission = role
                    api.calls.clear()
                    event = comment_event()
                    event["comment"]["author_association"] = association
                    result = resolver.prepare(event, "dotnet/maui", "not-the-commenter", "refs/heads/main")
                    self.assertEqual(result["actor"], "maintainer")
                    for key in ("pr", "repository", "head_repository", "head_ref", "head_sha", "base_ref", "base_sha"):
                        self.assertEqual(result[key], context()[key])
                    self.assertNotIn("blocker", result)
                    self.assertEqual(len(api.calls), 2)
                    self.assertIn("/collaborators/maintainer/permission", api.calls[0][0])

    def test_read_triage_and_unknown_permissions_are_not_authorized(self):
        api = self.github()
        for role in ("read", "triage", "none", "", "WRITE"):
            with self.subTest(role=role):
                api.permission = role
                api.calls.clear()
                self.assertIsNone(resolver.prepare(comment_event(), "dotnet/maui", "maintainer", "refs/heads/main"))
                self.assertEqual(len(api.calls), 1)

    def test_noncommands_bots_non_prs_comment_edits_and_untrusted_associations_are_ignored(self):
        api = self.github()
        events = []
        for body in (
            "", "/resolve", "/resolve conflict", "/resolve conflicts please",
            "please /resolve conflicts", " /resolve conflicts", "/Resolve conflicts",
            "`/resolve conflicts`", "/resolve conflicts\nrun this", "/resolve conflicts\n/resolve conflicts",
        ):
            events.append((repr(body), comment_event(body)))
        for label, mutate in (
            ("bot", lambda e: e["comment"]["user"].update(type="Bot", login="robot[bot]")),
            ("issue", lambda e: e["issue"].pop("pull_request")),
            ("edit", lambda e: e.update(action="edited")),
            ("delete", lambda e: e.update(action="deleted")),
            ("outsider", lambda e: e["comment"].update(author_association="NONE")),
        ):
            event = comment_event()
            mutate(event)
            events.append((label, event))
        for label, event in events:
            with self.subTest(event=label):
                api.calls.clear()
                self.assertIsNone(resolver.prepare(event, "dotnet/maui", "maintainer", "refs/heads/main"))
                self.assertEqual(api.calls, [])

    def test_exact_command_rejects_embedded_newlines(self):
        api = self.github()
        self.assertIsNone(resolver.prepare(
            comment_event("/resolve\nconflicts"), "dotnet/maui", "maintainer", "refs/heads/main"
        ))
        self.assertEqual(api.calls, [])

    def test_manual_dispatch_requires_default_branch_and_current_write_permission(self):
        api = self.github()
        event = {"repository": {"default_branch": "main"}, "inputs": {"pr_number": "42"}}
        for ref in ("refs/heads/topic", "refs/tags/main", "main"):
            with self.subTest(ref=ref):
                with self.assertRaisesRegex(ValueError, "default branch"):
                    resolver.prepare(event, "dotnet/maui", "maintainer", ref)
                self.assertEqual(api.calls, [])
        result = resolver.prepare(event, "dotnet/maui", "maintainer", "refs/heads/main")
        self.assertEqual(result["pr"], 42)
        api.permission = "read"
        self.assertIsNone(resolver.prepare(event, "dotnet/maui", "maintainer", "refs/heads/main"))

    def test_wrong_repository_is_rejected_before_api_calls(self):
        api = self.github()
        with self.assertRaisesRegex(ValueError, "dotnet/maui"):
            resolver.prepare(comment_event(), "contributor/maui", "maintainer", "refs/heads/main")
        self.assertEqual(api.calls, [])

    def test_closed_merged_and_deleted_fork_prs_are_rejected(self):
        api = self.github()
        for field in ("closed", "merged", "deleted"):
            with self.subTest(field=field):
                api.pr = pull_request(context())
                if field == "closed":
                    api.pr["state"] = "closed"
                elif field == "merged":
                    api.pr["merged"] = True
                else:
                    api.pr["head"]["repo"] = None
                with self.assertRaisesRegex(ValueError, "open PR"):
                    resolver.prepare(comment_event(), "dotnet/maui", "maintainer", "refs/heads/main")

    def test_uneditable_fork_produces_authorized_but_blocked_context(self):
        api = self.github()
        api.pr["maintainer_can_modify"] = False
        result = resolver.prepare(comment_event(), "dotnet/maui", "maintainer", "refs/heads/main")
        self.assertEqual(result["actor"], "maintainer")
        self.assertIn("maintainer edits", result["blocker"])

    def test_prepare_outputs_keep_authorized_blocked_requests_eligible_for_reporting(self):
        api = self.github()
        event = self.root / "event.json"
        event.write_text(json.dumps(comment_event()))
        for blocker in (None, "fork-edits", "default-head", "target-head"):
            with self.subTest(blocker=blocker):
                api.pr = pull_request(context())
                if blocker == "fork-edits":
                    api.pr["maintainer_can_modify"] = False
                elif blocker in {"default-head", "target-head"}:
                    api.pr["head"]["repo"]["full_name"] = "dotnet/maui"
                    api.pr["base"]["ref"] = "release"
                    api.pr["head"]["ref"] = "main" if blocker == "default-head" else "release"
                outputs = self.fresh("outputs")
                self.invoke_main(
                    "prepare", self.fresh("prepare"), context(),
                    GITHUB_EVENT_PATH=str(event), GITHUB_OUTPUT=str(outputs),
                    GITHUB_REPOSITORY="dotnet/maui", GITHUB_ACTOR="maintainer",
                    GITHUB_REF="refs/heads/main",
                )
                values = dict(line.split("=", 1) for line in outputs.read_text().splitlines())
                self.assertEqual(values["authorized"], "true")
                self.assertEqual(values["eligible"], "false" if blocker else "true")
                snapshot = json.loads(values["context"])
                if blocker:
                    self.assertTrue(snapshot["blocker"])
                    with self.assertRaisesRegex(ValueError, re.escape(snapshot["blocker"])):
                        self.invoke_main("publish", self.fresh("publish"), snapshot,
                                         RESOLVE_STATE="", RESOLVE_RESULT="skipped", BUILD_RESULT="skipped")
                    self.assertIn("Nothing was pushed.", api.reports[-1])
                    self.assertIn(snapshot["blocker"], api.reports[-1])
                else:
                    self.assertNotIn("blocker", snapshot)

    def test_same_repository_default_and_target_branches_are_blocked(self):
        api = self.github()
        api.pr["head"]["repo"]["full_name"] = "dotnet/maui"
        api.pr["base"]["ref"] = "release"
        for head in ("main", "release"):
            with self.subTest(head=head):
                api.pr["head"]["ref"] = head
                result = resolver.prepare(comment_event(), "dotnet/maui", "maintainer", "refs/heads/main")
                self.assertIn("default or target branch", result["blocker"])

    def test_context_rejects_invalid_repository_sha_ref_date_and_pr(self):
        changes = (
            ("repository", "../outside"), ("head_repository", "owner/repo/extra"),
            ("head_sha", "1" * 39), ("base_sha", "A" * 40),
            ("head_ref", "x:main"), ("base_ref", "../main"),
            ("date", "not-a-date"), ("pr", 0), ("pr", "42"),
        )
        for key, value in changes:
            with self.subTest(key=key, value=value):
                snapshot = context()
                snapshot[key] = value
                with self.assertRaises(ValueError):
                    resolver.validate_context(snapshot)


class GitFixture:
    """Small branches which really conflict; no manufactured unmerged index."""

    def __init__(self, directory, kind="text", path="s.txt"):
        self.directory = directory
        self.path = path
        directory.mkdir()
        resolver.git(directory, "init", "--quiet", "--initial-branch=main")
        common = {
            "p.txt": "old PR file\n", "t.txt": "old target file\n", "u.txt": "untouched\n",
            "AGENTS.md": "UNTRUSTED INSTRUCTIONS\n",
            ".copilot/instructions.md": "UNTRUSTED INSTRUCTIONS\n",
            ".github/skills/resolve-pr-conflicts/SKILL.md": "UNTRUSTED SKILL\n",
            "nested/CLAUDE.md": "UNTRUSTED INSTRUCTIONS\n",
        }
        for name, content in common.items():
            self.write(name, content)
        if kind != "add-add":
            self.version(kind, "base", ORIGINAL)
        self.commit("ancestor")
        self.ancestor = resolver.text(resolver.git(directory, "rev-parse", "HEAD"))
        resolver.git(directory, "checkout", "--quiet", "-b", "topic")
        if kind == "delete-pr":
            (directory / path).unlink()
        elif kind == "rename":
            resolver.git(directory, "mv", path, "renamed.txt")
            self.write("renamed.txt", PR_TEXT)
        elif kind != "clean":
            self.version(kind, "PR", PR_TEXT)
        self.write("p.txt", "nonconflicting PR change\n")
        self.commit("PR change")
        head = resolver.text(resolver.git(directory, "rev-parse", "HEAD"))
        resolver.git(directory, "checkout", "--quiet", "main")
        if kind == "delete-target":
            (directory / path).unlink()
        elif kind != "clean":
            self.version(kind, "target", TARGET_TEXT)
        self.write("t.txt", "nonconflicting target change\n")
        self.commit("target change")
        target = resolver.text(resolver.git(directory, "rev-parse", "HEAD"))
        self.context = dict(context(), head_sha=head, base_sha=target)

    def write(self, path, content):
        file = self.directory / path
        file.parent.mkdir(parents=True, exist_ok=True)
        file.write_text(content, encoding="utf-8", newline="")

    def version(self, kind, label, content):
        file = self.directory / self.path
        file.parent.mkdir(parents=True, exist_ok=True)
        if kind == "symlink":
            if file.is_symlink():
                file.unlink()
            file.symlink_to(label + ".txt")
        elif kind == "binary":
            file.write_bytes(b"\0" + content.encode())
        elif kind == "oversized":
            file.write_text(label + "\n" + "a" * resolver.MAX_FILE, encoding="utf-8")
        elif kind == "non-utf8":
            file.write_bytes(b"\xff" + content.encode())
        else:
            self.write(self.path, content)
            if kind == "executable":
                file.chmod(0o755)

    def commit(self, message):
        resolver.git(self.directory, "add", "-A")
        resolver.git(self.directory, "commit", "--quiet", "-m", message)

    def new_merge(self, snapshot, directory):
        directory = Path(directory)
        directory.mkdir()
        resolver.git(directory, "init", "--quiet")
        # Only Git objects cross the boundary, not refs, config, hooks or index.
        shutil.copytree(self.directory / ".git/objects", directory / ".git/objects", dirs_exist_ok=True)
        resolver.git(directory, "checkout", "--quiet", "--detach", snapshot["head_sha"])
        return resolver.merge(directory, snapshot)


class GitCase(HermeticCase):
    def setUp(self):
        super().setUp()
        self.fixture = GitFixture(self.fresh("fixture"))
        self.context = self.fixture.context
        self.merge_mock = self.start_patch(patch.object(resolver, "new_merge", side_effect=self.fixture.new_merge))

    def use_fixture(self, kind, path="s.txt"):
        self.fixture = GitFixture(self.fresh("fixture"), kind, path)
        self.context = self.fixture.context
        self.merge_mock.side_effect = self.fixture.new_merge

    def artifact(self, files=None, summary="Preserve both changes.", directory=None):
        directory = directory or self.fresh("artifact")
        directory.mkdir(parents=True)
        path = directory / "resolution.json"
        path.write_text(json.dumps({
            "files": {"s.txt": RESOLVED} if files is None else files,
            "summary": summary,
        }), encoding="utf-8")
        return path

    def candidate(self, artifact=None):
        repo = self.fresh("candidate")
        sha, result = resolver.materialize(self.context, repo, artifact or self.artifact())
        return repo, sha, result


class MergeAndMaterializationTests(GitCase):
    def test_real_conflict_has_all_three_versions(self):
        repo = self.fresh("merge")
        paths = resolver.new_merge(self.context, repo)
        self.assertEqual(paths, ["s.txt"])
        self.assertEqual(resolver.conflict_versions(repo, paths), {
            "s.txt": {"1": ORIGINAL, "2": PR_TEXT, "3": TARGET_TEXT},
        })
        self.assertIn("<<<<<<<", (repo / "s.txt").read_text())
        self.assertEqual(resolver.text(resolver.git(repo, "rev-parse", "MERGE_HEAD")), self.context["base_sha"])

    def test_no_conflicts_skips_agent_and_artifacts(self):
        self.use_fixture("clean")
        output = self.root / "outputs"
        with patch.dict(os.environ, {"GITHUB_OUTPUT": str(output)}):
            resolver.solve(self.context, self.fresh("merge"), self.root / "agent",
                           self.root / "artifact", self.root / "nonexistent-skill")
        self.assertEqual(output.read_text(), "state=no-conflicts\n")
        self.assertFalse((self.root / "agent").exists())
        self.assertFalse((self.root / "artifact").exists())
        self.assertEqual(self.merge_mock.call_count, 1)

    def test_materialization_is_deterministic_two_parent_merge_preserving_both_sides(self):
        artifact = self.artifact()
        repo, first, resolution = self.candidate(artifact)
        other, second, _ = self.candidate(artifact)
        self.assertEqual(first, second)
        self.assertEqual(resolution["files"], {"s.txt": RESOLVED})
        self.assertEqual(resolver.text(resolver.git(repo, "show", "-s", "--format=%P", first)).split(),
                         [self.context["head_sha"], self.context["base_sha"]])
        for directory in (repo, other):
            self.assertEqual((directory / "s.txt").read_text(), RESOLVED)
            self.assertEqual((directory / "p.txt").read_text(), "nonconflicting PR change\n")
            self.assertEqual((directory / "t.txt").read_text(), "nonconflicting target change\n")
            self.assertEqual((directory / "u.txt").read_text(), "untouched\n")
            self.assertEqual(resolver.git(directory, "status", "--porcelain").stdout, b"")
            for parent in ("head_sha", "base_sha"):
                resolver.git(directory, "merge-base", "--is-ancestor", self.context[parent], first)
        message = resolver.text(resolver.git(repo, "show", "-s", "--format=%B", first))
        self.assertIn("Merge main into PR #42", message)
        self.assertIn("Co-authored-by: Copilot App", message)

    def test_commit_identity_and_dates_do_not_depend_on_runner_environment(self):
        artifact = self.artifact()
        _, expected, _ = self.candidate(artifact)
        with patch.dict(os.environ, {
            "GIT_AUTHOR_NAME": "Unexpected Author", "GIT_AUTHOR_EMAIL": "author@example.invalid",
            "GIT_COMMITTER_NAME": "Unexpected Committer", "GIT_COMMITTER_EMAIL": "committer@example.invalid",
            "GIT_AUTHOR_DATE": "2001-01-01T00:00:00Z",
            "GIT_COMMITTER_DATE": "2030-01-01T00:00:00Z",
        }):
            repo, actual, _ = self.candidate(artifact)
        self.assertEqual(actual, expected)
        identity = resolver.text(resolver.git(repo, "show", "-s", "--format=%an%n%ae%n%cn%n%ce%n%at%n%ct", actual))
        timestamp = str(int(datetime.fromisoformat(DATE.replace("Z", "+00:00")).timestamp()))
        self.assertEqual(identity.splitlines(), [
            "github-actions[bot]", "41898282+github-actions[bot]@users.noreply.github.com",
            "github-actions[bot]", "41898282+github-actions[bot]@users.noreply.github.com",
            timestamp, timestamp,
        ])

    def test_deletion_conflicts_expose_missing_stage_and_can_delete_or_keep(self):
        for kind, stages in (("delete-pr", {"1", "3"}), ("delete-target", {"1", "2"})):
            with self.subTest(kind=kind):
                self.use_fixture(kind)
                repo = self.fresh("merge")
                paths = resolver.new_merge(self.context, repo)
                self.assertEqual(paths, ["s.txt"])
                self.assertEqual(set(resolver.conflict_versions(repo, paths)["s.txt"]), stages)
                for value in (None, RESOLVED):
                    with self.subTest(resolution=value):
                        directory, sha, _ = self.candidate(self.artifact({"s.txt": value}))
                        self.assertEqual((directory / "s.txt").exists(), value is not None)
                        self.assertEqual(resolver.text(resolver.git(directory, "show", "-s", "--format=%P", sha)).split(),
                                         [self.context["head_sha"], self.context["base_sha"]])

    def test_rename_conflict_uses_destination_and_preserves_rename(self):
        self.use_fixture("rename")
        repo = self.fresh("merge")
        paths = resolver.new_merge(self.context, repo)
        self.assertEqual(paths, ["renamed.txt"])
        self.assertEqual(resolver.conflict_versions(repo, paths)["renamed.txt"],
                         {"1": ORIGINAL, "2": PR_TEXT, "3": TARGET_TEXT})
        directory, _, _ = self.candidate(self.artifact({"renamed.txt": RESOLVED}))
        self.assertFalse((directory / "s.txt").exists())
        self.assertEqual((directory / "renamed.txt").read_text(), RESOLVED)

    def test_add_add_conflict_has_no_ancestor(self):
        self.use_fixture("add-add")
        repo = self.fresh("merge")
        paths = resolver.new_merge(self.context, repo)
        self.assertEqual(resolver.conflict_versions(repo, paths)["s.txt"], {"2": PR_TEXT, "3": TARGET_TEXT})
        self.candidate()

    def test_executable_file_mode_is_preserved(self):
        self.use_fixture("executable")
        repo, sha, _ = self.candidate()
        self.assertTrue(resolver.text(resolver.git(repo, "ls-tree", sha, "s.txt")).startswith("100755 "))

    def test_resolution_preserves_utf8_and_crlf_bytes(self):
        value = "PR and target: caf\u00e9\r\nnext line\r\n"
        repo, sha, _ = self.candidate(self.artifact({"s.txt": value}))
        self.assertEqual((repo / "s.txt").read_bytes(), value.encode("utf-8"))
        self.assertEqual(resolver.git(repo, "show", sha + ":s.txt").stdout, value.encode("utf-8"))

    def test_conflict_filenames_are_literal_not_git_pathspecs(self):
        paths = ["s[1].txt"]
        if os.name != "nt":
            paths.append(":(exclude)*.txt")
        for path in paths:
            with self.subTest(path=path):
                self.use_fixture("text", path)
                repo = self.fresh("merge")
                conflicts = resolver.new_merge(self.context, repo)
                self.assertEqual(conflicts, [path])
                self.assertEqual(resolver.conflict_versions(repo, conflicts), {
                    path: {"1": ORIGINAL, "2": PR_TEXT, "3": TARGET_TEXT},
                })
                materialized, _, _ = self.candidate(self.artifact({path: RESOLVED}))
                self.assertEqual((materialized / path).read_text(), RESOLVED)
                self.assertEqual((materialized / "u.txt").read_text(), "untouched\n")
                self.assertEqual((materialized / "p.txt").read_text(), "nonconflicting PR change\n")
                self.assertEqual((materialized / "t.txt").read_text(), "nonconflicting target change\n")

    def test_unresolved_markers_invalid_values_and_large_resolutions_are_rejected(self):
        for value in (
            "<<<<<<< HEAD\nours\n", "before\n=======\nafter\n", ">>>>>>> target\n",
            "||||||| ancestor\n", "\0binary", 123, ["text"], {"text": "value"},
            "x" * (resolver.MAX_FILE + 1),
        ):
            with self.subTest(value_type=type(value).__name__, prefix=str(value)[:30]):
                with self.assertRaisesRegex(ValueError, "Unresolved markers|invalid conflict"):
                    self.candidate(self.artifact({"s.txt": value}))

    def test_unexpected_or_missing_resolution_paths_are_rejected(self):
        for files in (
            {}, {"u.txt": "changed"}, {"../escape": "changed"},
            {"s.txt": RESOLVED, "u.txt": "changed"},
            {"s.txt": RESOLVED, ".github/workflows/x.yml": "changed"},
        ):
            with self.subTest(paths=list(files)):
                with self.assertRaisesRegex(ValueError, "paths do not match"):
                    self.candidate(self.artifact(files))
        self.assertFalse((self.root / "escape").exists())

    def test_nonconflicting_artifact_cannot_invent_resolution(self):
        self.use_fixture("clean")
        with self.assertRaisesRegex(ValueError, "paths do not match"):
            self.candidate()

    def test_invalid_summaries_are_rejected(self):
        for summary in (None, 42, ["summary"], "", " \n", "x" * 2001):
            with self.subTest(type=type(summary).__name__):
                with self.assertRaisesRegex(ValueError, "summary"):
                    self.candidate(self.artifact(summary=summary))

    def test_invalid_artifact_schema_is_rejected_before_merging(self):
        payloads = (
            None, [], "not an object", {},
            {"files": {"s.txt": RESOLVED}},
            {"summary": "Missing files"},
            {"files": [], "summary": "Not a mapping"},
            {"files": None, "summary": "Not a mapping"},
            {"files": {"s.txt": RESOLVED}, "summary": "Extra field", "candidate": "1" * 40},
        )
        self.merge_mock.reset_mock()
        for payload in payloads:
            with self.subTest(payload=payload):
                artifact = self.fresh("invalid-schema.json")
                artifact.write_text(json.dumps(payload), encoding="utf-8")
                with self.assertRaisesRegex(ValueError, "artifact schema"):
                    resolver.materialize(self.context, self.fresh("rejected"), artifact)
                self.merge_mock.assert_not_called()

    def test_real_binary_symlink_oversized_and_non_utf8_conflicts_are_rejected(self):
        for kind in ("binary", "symlink", "oversized", "non-utf8"):
            with self.subTest(kind=kind):
                self.use_fixture(kind)
                repo = self.fresh("merge")
                paths = resolver.new_merge(self.context, repo)
                self.assertEqual(paths, ["s.txt"])
                with self.assertRaises(ValueError):
                    resolver.conflict_versions(repo, paths)
                with self.assertRaises(ValueError):
                    self.candidate()

    def test_actual_automation_conflict_is_rejected(self):
        self.use_fixture("text", ".github/workflows/a.yml")
        with self.assertRaisesRegex(ValueError, "automation"):
            resolver.new_merge(self.context, self.fresh("merge"))

    def test_unsafe_paths_are_rejected(self):
        for path in ("", "/absolute", "../outside", "src/../a", ".git/config",
                     "src/.github/a.yml", r"src\a", "src/\nfile", "src/\0file"):
            with self.subTest(path=path):
                with self.assertRaises(ValueError):
                    resolver.validate_path(path)

    def test_oversized_and_nonregular_artifacts_are_rejected_before_merging(self):
        valid = self.artifact()
        symlink = self.root / "linked.json"
        symlink.symlink_to(valid)
        directory = self.root / "directory.json"
        directory.mkdir()
        oversized = self.root / "large.json"
        with oversized.open("wb") as stream:
            stream.truncate(resolver.MAX_ARTIFACT + 1)
        artifacts = [symlink, directory, oversized]
        if hasattr(os, "mkfifo"):
            fifo = self.root / "fifo.json"
            os.mkfifo(fifo)
            artifacts.append(fifo)
        self.merge_mock.reset_mock()
        for artifact in artifacts:
            with self.subTest(artifact=artifact.name):
                with self.assertRaisesRegex(ValueError, "bounded regular"):
                    resolver.materialize(self.context, self.fresh("rejected"), artifact)
                self.merge_mock.assert_not_called()


class SolveTests(GitCase):
    def setUp(self):
        super().setUp()
        self.skill = self.root / "trusted-skill"
        self.skill.write_text("TRUSTED SKILL: preserve both sides, use file tools only.\n")
        self.copilot_calls = []
        self.output = self.root / "outputs"
        self.start_patch(patch.dict(os.environ, {
            "GITHUB_OUTPUT": str(self.output), "GH_TOKEN": "not-for-agent",
            "GITHUB_TOKEN": "also-not-for-agent", "COPILOT_GITHUB_TOKEN": "inference-only",
        }))

    def solve(self, edit=None, summary="Preserved the PR and target changes."):
        def command(args, **kwargs):
            if args[0] != "copilot":
                return self.local_only(args, **kwargs)
            self.copilot_calls.append((list(args), kwargs))
            directory = Path(kwargs["cwd"])
            handoff = json.loads((directory / "context.json").read_text())
            self.assertEqual(handoff["head_sha"], self.context["head_sha"])
            self.assertEqual(handoff["base_sha"], self.context["base_sha"])
            self.assertEqual(handoff["conflicts"], [{"path": "s.txt", "versions": "versions/0"}])
            self.assertEqual((directory / "versions/0/ancestor").read_text(), ORIGINAL)
            self.assertEqual((directory / "versions/0/pr").read_text(), PR_TEXT)
            self.assertEqual((directory / "versions/0/target").read_text(), TARGET_TEXT)
            self.assertIn("<<<<<<<", (directory / "source/s.txt").read_text())
            self.assertEqual((directory / "source/p.txt").read_text(), "nonconflicting PR change\n")
            self.assertEqual((directory / "source/t.txt").read_text(), "nonconflicting target change\n")
            for forbidden in (".git", ".github", ".copilot", "AGENTS.md", "nested/CLAUDE.md"):
                self.assertFalse((directory / "source" / forbidden).exists(), forbidden)
            self.assertEqual((directory / ".github/skills/resolve-pr-conflicts/SKILL.md").read_bytes(),
                             self.skill.read_bytes())
            (directory / "source/s.txt").write_text(RESOLVED)
            (directory / "summary.txt").write_text(summary)
            if edit:
                edit(directory)
            return subprocess.CompletedProcess(args, 0, b"", b"")

        merge, agent, artifact = self.fresh("merge"), self.fresh("agent"), self.fresh("artifact")
        with patch.object(resolver, "run", side_effect=command):
            resolver.solve(self.context, merge, agent, artifact, self.skill)
        return merge, agent, artifact

    def test_only_copilot_is_mocked_with_fixed_model_file_tools_and_trusted_skill(self):
        _, agent, artifact = self.solve()
        self.assertEqual(len(self.copilot_calls), 1)
        args, kwargs = self.copilot_calls[0]
        self.assertEqual(args[args.index("--model") + 1], "gpt-6-astra")
        self.assertEqual(args[args.index("--reasoning-effort") + 1], "high")
        self.assertEqual(args[args.index("--available-tools") + 1:args.index("--allow-tool")],
                         ["view", "rg", "glob", "apply_patch", "skill"])
        self.assertEqual([args[i + 1] for i, arg in enumerate(args) if arg == "--allow-tool"],
                         ["read", "write"])
        for flag in ("--no-custom-instructions", "--disable-builtin-mcps", "--no-auto-update", "--disallow-temp-dir",
                     "--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN"):
            self.assertIn(flag, args)
        prompt = args[args.index("-p") + 1]
        self.assertIn(".github/skills/resolve-pr-conflicts/SKILL.md", prompt)
        self.assertIn("Never execute code", prompt)
        self.assertEqual(kwargs["cwd"], agent)
        self.assertNotIn("GH_TOKEN", kwargs["env"])
        self.assertNotIn("GITHUB_TOKEN", kwargs["env"])
        self.assertEqual(kwargs["env"]["COPILOT_GITHUB_TOKEN"], "inference-only")
        self.assertEqual(self.merge_mock.call_count, 2)
        data = json.loads((artifact / "resolution.json").read_text())
        self.assertEqual(data["files"], {"s.txt": RESOLVED})
        candidate = resolver.text(resolver.git(self.root / "verified", "rev-parse", "HEAD"))
        self.assertEqual(self.output.read_text(), f"candidate={candidate}\nstate=resolved\n")

    def test_agent_edit_outside_conflict_allowlist_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "outside the conflict allowlist"):
            self.solve(lambda agent: (agent / "source/u.txt").write_text("unrelated"))
        self.assertFalse(self.output.exists())
        self.assertEqual(self.merge_mock.call_count, 1)

    def test_agent_symlink_is_rejected(self):
        def edit(agent):
            file = agent / "source/s.txt"
            file.unlink()
            file.symlink_to("p.txt")
        with self.assertRaisesRegex(ValueError, "symlink"):
            self.solve(edit)
        self.assertFalse(self.output.exists())

    def test_agent_can_explicitly_delete_conflicted_file(self):
        _, _, artifact = self.solve(lambda agent: (agent / "source/s.txt").unlink())
        self.assertIsNone(json.loads((artifact / "resolution.json").read_text())["files"]["s.txt"])
        self.assertFalse((self.root / "verified/s.txt").exists())
        self.assertIn("state=resolved", self.output.read_text())

    def test_agent_cannot_leave_markers_or_claim_success_without_valid_summary(self):
        for summary in ("", " \n", "x" * 2001, "x" * 8001):
            with self.subTest(summary_length=len(summary)):
                with self.assertRaises(ValueError):
                    self.solve(summary=summary)
                self.assertFalse(self.output.exists())
        with self.assertRaisesRegex(ValueError, "Unresolved markers"):
            self.solve(lambda agent: (agent / "source/s.txt").write_text("<<<<<<< HEAD\n"))
        self.assertFalse(self.output.exists())

    def test_nonregular_summary_is_rejected(self):
        def edit(agent):
            summary = agent / "summary.txt"
            summary.unlink()
            summary.symlink_to("source/s.txt")
        with self.assertRaisesRegex(ValueError, "bounded regular"):
            self.solve(edit)
        self.assertFalse(self.output.exists())


class BuildTests(GitCase):
    def dotnet(self, returncodes=(0, 0), change=None):
        actual = subprocess.run
        calls = []
        def execute(args, **kwargs):
            if args[0] != "dotnet":
                return actual(args, **kwargs)
            calls.append((list(args), kwargs))
            kwargs["stdout"].write(b"build output\n::set-env name=UNSAFE::ignored\n")
            if change:
                change(Path(kwargs["cwd"]))
            return subprocess.CompletedProcess(args, returncodes[len(calls) - 1])
        self.start_patch(patch.object(subprocess, "run", side_effect=execute))
        return calls

    def test_build_runs_restore_and_release_cake_in_candidate_and_logs_output(self):
        repo, _, _ = self.candidate()
        calls = self.dotnet()
        log = self.root / "build.log"
        resolver.build(repo, log)
        self.assertEqual([args for args, _ in calls], [
            ["dotnet", "tool", "restore"],
            ["dotnet", "cake", "--target=dotnet-build", "--configuration=Release"],
        ])
        self.assertTrue(all(kwargs["cwd"] == repo for _, kwargs in calls))
        self.assertEqual(log.read_bytes().count(b"build output"), 2)
        self.assertTrue(all(kwargs["stderr"] == subprocess.STDOUT for _, kwargs in calls))

    def test_build_strips_tokens_secrets_and_runner_control_environment(self):
        repo, _, _ = self.candidate()
        calls = self.dotnet()
        dangerous = {
            "GH_TOKEN": "gh", "GITHUB_TOKEN": "github", "COPILOT_GITHUB_TOKEN": "copilot",
            "PUSH_TOKEN": "push", "CUSTOM_SECRET": "secret", "some_token": "case-insensitive",
            "ACTIONS_RUNTIME_URL": "https://invalid", "ACTIONS_RUNTIME_TOKEN": "runtime",
            "GITHUB_ENV": "env", "GITHUB_OUTPUT": "output", "GITHUB_PATH": "path",
            "GITHUB_STATE": "state", "BASH_ENV": "startup",
        }
        with patch.dict(os.environ, dict(dangerous, KEEP_BUILD_SETTING="kept")):
            resolver.build(repo, self.root / "build.log")
            for _, kwargs in calls:
                self.assertEqual(set(dangerous) & kwargs["env"].keys(), set())
                self.assertEqual(kwargs["env"]["KEEP_BUILD_SETTING"], "kept")
                self.assertIn("PATH", kwargs["env"])
            for name, value in dangerous.items():
                self.assertEqual(os.environ[name], value)

    def test_build_strips_runner_step_summary_output_path(self):
        repo, _, _ = self.candidate()
        calls = self.dotnet()
        with patch.dict(os.environ, {"GITHUB_STEP_SUMMARY": str(self.root / "runner-summary")}):
            resolver.build(repo, self.root / "build.log")
        for _, kwargs in calls:
            self.assertTrue("GITHUB_STEP_SUMMARY" not in kwargs["env"],
                            "PR-controlled builds must not inherit the runner summary output path")

    def test_build_strips_environment_git_authentication(self):
        repo, _, _ = self.candidate()
        calls = self.dotnet()
        credentials = {
            "GIT_CONFIG_COUNT": "1",
            "GIT_CONFIG_KEY_0": "http.https://github.com/.extraheader",
            "GIT_CONFIG_VALUE_0": "AUTHORIZATION: basic Zml4dHVyZQ==",
        }
        with patch.dict(os.environ, credentials):
            resolver.build(repo, self.root / "build.log")
        for _, kwargs in calls:
            self.assertEqual(set(credentials) & kwargs["env"].keys(), set())

    def test_restore_or_build_failure_blocks_publication(self):
        repo, _, _ = self.candidate()
        api = self.github(self.context)
        calls = self.dotnet(returncodes=(1, 0, 1))
        for phase, count in (("restore", 1), ("cake", 3)):
            with self.subTest(phase=phase):
                with self.assertRaisesRegex(RuntimeError, "failed"):
                    resolver.build(repo, self.fresh("build.log"))
                self.assertEqual(len(calls), count)
                with self.assertRaisesRegex(RuntimeError, "build jobs: failure"):
                    self.invoke_main("publish", self.fresh("publish"), self.context, BUILD_RESULT="failure")
                self.assertIn("Nothing was pushed.", api.reports[-1])
                self.assertNotIn("Pushed merge commit", api.reports[-1])

    def test_build_modifying_tracked_file_is_rejected(self):
        repo, _, _ = self.candidate()
        self.dotnet(change=lambda path: (path / "s.txt").write_text("build mutation\n"))
        with self.assertRaisesRegex(RuntimeError, "changed tracked inputs"):
            resolver.build(repo, self.root / "build.log")

    def test_build_phase_checks_exact_candidate_before_running_dotnet(self):
        root = self.fresh("build-phase")
        self.artifact(directory=root / "artifact")
        calls = self.dotnet()
        with self.assertRaisesRegex(ValueError, "differs from the candidate"):
            self.invoke_main("build", root, self.context, EXPECTED_CANDIDATE="0" * 40)
        self.assertEqual(calls, [])


class LocalTransport:
    """Redirect only ls-remote/push to actual local bare repositories."""

    def __init__(self, test):
        self.original_git = resolver.git
        self.calls = []
        self.remotes = {}
        self.before_push = None
        self.push_refspec = None
        for repository, ref, sha in (
            (test.context["head_repository"], test.context["head_ref"], test.context["head_sha"]),
            (test.context["repository"], test.context["base_ref"], test.context["base_sha"]),
        ):
            bare = test.fresh("remote")
            bare.mkdir()
            resolver.git(bare, "init", "--quiet", "--bare")
            shutil.copytree(test.fixture.directory / ".git/objects", bare / "objects", dirs_exist_ok=True)
            resolver.git(bare, "update-ref", "refs/heads/" + ref, sha)
            self.remotes["https://github.com/" + repository + ".git"] = bare

    def __call__(self, repo, *args, **kwargs):
        index = 0
        while index < len(args):
            if args[index] == "-c":
                index += 2
            elif args[index] == "--literal-pathspecs":
                index += 1
            else:
                break
        if index == len(args) or args[index] not in {"ls-remote", "push"}:
            return self.original_git(repo, *args, **kwargs)
        self.calls.append((args[index:], copy.deepcopy(kwargs)))
        if args[index] == "push" and self.before_push:
            self.before_push()
        local = [str(self.remotes[arg]) if arg in self.remotes else arg for arg in args]
        if args[index] == "push" and self.push_refspec:
            local[-1] = self.push_refspec
        return self.original_git(repo, "-c", "protocol.file.allow=always", *local, **kwargs)


class PublicationTests(GitCase):
    def setUp(self):
        super().setUp()
        self.api = self.github(self.context)
        self.transport = LocalTransport(self)
        self.start_patch(patch.object(resolver, "git", side_effect=self.transport))

    def assert_no_push(self):
        self.assertFalse([args for args, _ in self.transport.calls if args[0] == "push"])

    def test_successful_push_is_normal_fast_forward_to_original_fork_branch(self):
        repo, sha, _ = self.candidate()
        resolver.push(self.context, repo, sha, "success", "fixture-push-credential")
        calls = self.transport.calls
        self.assertEqual([args[0] for args, _ in calls], ["ls-remote", "ls-remote", "push"])
        fork = "https://github.com/contributor/maui.git"
        target = "https://github.com/dotnet/maui.git"
        self.assertEqual(calls[0][0], ("ls-remote", "--exit-code", fork, "refs/heads/topic"))
        self.assertEqual(calls[1][0], ("ls-remote", "--exit-code", target, "refs/heads/main"))
        self.assertEqual(calls[2][0], ("push", "--porcelain", fork, sha + ":refs/heads/topic"))
        self.assertNotIn("fixture-push-credential", " ".join(calls[2][0]))
        env = calls[2][1]["env"]
        self.assertEqual(env["GIT_CONFIG_KEY_0"], "http.https://github.com/.extraheader")
        encoded = env["GIT_CONFIG_VALUE_0"].removeprefix("AUTHORIZATION: basic ")
        self.assertEqual(base64.b64decode(encoded), b"x-access-token:fixture-push-credential")
        self.assertEqual(env["GIT_CONFIG_KEY_1"], "credential.helper")
        self.assertEqual(env["GIT_CONFIG_VALUE_1"], "")
        remote = self.transport.remotes[fork]
        self.assertEqual(resolver.text(resolver.git(remote, "rev-parse", "refs/heads/topic")), sha)
        resolver.git(remote, "merge-base", "--is-ancestor", self.context["head_sha"], sha)
        resolver.git(remote, "merge-base", "--is-ancestor", self.context["base_sha"], sha)
        self.assertEqual(resolver.text(resolver.git(self.transport.remotes[target], "rev-parse", "refs/heads/main")),
                         self.context["base_sha"])

    def test_push_environment_has_only_scoped_credential_and_pinned_hook_expectations(self):
        repo, sha, _ = self.candidate()
        inherited = {
            "GH_TOKEN": "comment-only", "GITHUB_TOKEN": "runner-only",
            "COPILOT_GITHUB_TOKEN": "inference-only", "PUSH_TOKEN": "inherited-not-selected",
            "CUSTOM_SECRET": "other-secret", "ACTIONS_RUNTIME_TOKEN": "artifact-only",
            "GITHUB_ENV": str(self.root / "runner-env"),
            "GITHUB_OUTPUT": str(self.root / "runner-output"),
        }
        with patch.dict(os.environ, inherited):
            resolver.push(self.context, repo, sha, "success", "explicit-push-credential")
        args, kwargs = self.transport.calls[-1]
        self.assertEqual(args[0], "push")
        env = kwargs["env"]
        self.assertEqual(set(inherited) & env.keys(), set())
        self.assertEqual(env["EXPECTED_HEAD"], self.context["head_sha"])
        self.assertEqual(env["EXPECTED_CANDIDATE"], sha)
        self.assertEqual(env["EXPECTED_REF"], "refs/heads/topic")
        self.assertEqual(env["GIT_CONFIG_COUNT"], "2")
        encoded = env["GIT_CONFIG_VALUE_0"].removeprefix("AUTHORIZATION: basic ")
        self.assertEqual(base64.b64decode(encoded), b"x-access-token:explicit-push-credential")

    def test_failed_skipped_cancelled_or_missing_build_never_passes_push_gate(self):
        for result in ("failure", "skipped", "cancelled", "", "neutral", "timed_out"):
            with self.subTest(result=result):
                self.api.calls.clear()
                with self.assertRaisesRegex(ValueError, "Both independent builds"):
                    resolver.push(self.context, self.root / "nonexistent", "1" * 40, result, "token")
                self.assertEqual(self.api.calls, [])
                self.assertEqual(self.transport.calls, [])

    def test_stale_pr_head_target_refs_and_repositories_prevent_push(self):
        repo, sha, _ = self.candidate()
        for side in ("head", "base"):
            for field, value in (("sha", "3" * 40), ("ref", "another-branch"), ("repo", {"full_name": "other/repo"})):
                with self.subTest(side=side, field=field):
                    self.api.pr = pull_request(self.context)
                    self.api.pr[side][field] = value
                    with self.assertRaises(ValueError):
                        resolver.push(self.context, repo, sha, "success", "token")
                    self.assertEqual(self.transport.calls, [])

    def test_revoked_permissions_maintainer_edits_and_pr_state_prevent_push(self):
        repo, sha, _ = self.candidate()
        for failure in ("read", "triage", "none", "maintainer", "closed", "merged", "deleted-fork"):
            with self.subTest(failure=failure):
                self.api.permission = "write"
                self.api.pr = pull_request(self.context)
                if failure in {"read", "triage", "none"}:
                    self.api.permission = failure
                elif failure == "maintainer":
                    self.api.pr["maintainer_can_modify"] = False
                elif failure == "closed":
                    self.api.pr["state"] = "closed"
                elif failure == "merged":
                    self.api.pr["merged"] = True
                else:
                    self.api.pr["head"]["repo"] = None
                with self.assertRaises(ValueError):
                    resolver.push(self.context, repo, sha, "success", "token")
                self.assertEqual(self.transport.calls, [])

    def test_branch_race_after_metadata_validation_prevents_push(self):
        repo, sha, _ = self.candidate()
        for side, repository in (("head", "contributor/maui"), ("base", "dotnet/maui")):
            with self.subTest(side=side):
                remote = self.transport.remotes["https://github.com/" + repository + ".git"]
                parent = self.context[side + "_sha"]
                tree = resolver.text(resolver.git(remote, "rev-parse", parent + "^{tree}"))
                moved = resolver.text(resolver.git(remote, "commit-tree", tree, "-p", parent, data=b"branch advanced\n"))
                ref = "refs/heads/" + self.context[side + "_ref"]
                resolver.git(remote, "update-ref", ref, moved)
                with self.assertRaisesRegex(ValueError, "branch moved"):
                    resolver.push(self.context, repo, sha, "success", "token")
                self.assert_no_push()
                resolver.git(remote, "update-ref", ref, parent)

    def test_head_rewind_after_ls_remote_is_rejected_by_actual_pre_push_hook(self):
        repo, sha, _ = self.candidate()
        remote = self.transport.remotes["https://github.com/contributor/maui.git"]
        self.transport.before_push = lambda: resolver.git(
            remote, "update-ref", "refs/heads/topic", self.fixture.ancestor
        )
        with self.assertRaisesRegex(RuntimeError, "PR branch changed before push"):
            resolver.push(self.context, repo, sha, "success", "fixture-push-credential")
        self.assertEqual(resolver.text(resolver.git(remote, "rev-parse", "refs/heads/topic")),
                         self.fixture.ancestor)

    def test_pre_push_hook_rejects_changed_candidate_or_destination_even_when_fast_forward(self):
        repo, sha, _ = self.candidate()
        tree = resolver.text(resolver.git(repo, "rev-parse", sha + "^{tree}"))
        other_candidate = resolver.text(resolver.git(
            repo, "commit-tree", tree, "-p", self.context["head_sha"], "-p", self.context["base_sha"],
            data=b"different but still fast-forward merge\n",
        ))
        resolver.git(repo, "merge-base", "--is-ancestor", self.context["head_sha"], other_candidate)
        remote = self.transport.remotes["https://github.com/contributor/maui.git"]
        resolver.git(remote, "update-ref", "refs/heads/other", self.context["head_sha"])
        for label, refspec in (
            ("candidate", other_candidate + ":refs/heads/topic"),
            ("destination", sha + ":refs/heads/other"),
        ):
            with self.subTest(changed=label):
                self.transport.push_refspec = refspec
                with self.assertRaisesRegex(RuntimeError, "PR branch changed before push"):
                    resolver.push(self.context, repo, sha, "success", "fixture-push-credential")
                for ref in ("topic", "other"):
                    self.assertEqual(resolver.text(resolver.git(remote, "rev-parse", "refs/heads/" + ref)),
                                     self.context["head_sha"])

    def test_single_parent_reversed_parents_invalid_sha_and_missing_credential_are_rejected(self):
        repo, sha, _ = self.candidate()
        tree = resolver.text(resolver.git(repo, "rev-parse", sha + "^{tree}"))
        reversed_parents = resolver.text(resolver.git(repo, "commit-tree", tree,
                                                    "-p", self.context["base_sha"],
                                                    "-p", self.context["head_sha"], data=b"wrong order\n"))
        for candidate, token, error in (
            (self.context["head_sha"], "token", "non-rewriting merge"),
            (reversed_parents, "token", "non-rewriting merge"),
            ("invalid", "token", "Invalid candidate"),
            (sha, "", "credential"),
        ):
            with self.subTest(candidate=candidate, credential=bool(token)):
                with self.assertRaisesRegex(ValueError, error):
                    resolver.push(self.context, repo, candidate, "success", token)
                self.assertEqual(self.transport.calls, [])

    def test_publish_reconstructs_exact_candidate_pushes_and_reports(self):
        root = self.fresh("publish-phase")
        artifact = self.artifact(directory=root / "artifact")
        _, expected, _ = self.candidate(artifact)
        self.invoke_main("publish", root, self.context, EXPECTED_CANDIDATE=expected)
        self.assertEqual(len(self.api.reports), 1)
        report = self.api.reports[0]
        self.assertIn("Pushed merge commit", report)
        self.assertIn(f"https://github.com/contributor/maui/commit/{expected}", report)
        self.assertIn("**success**", report)
        self.assertIn("<code>s.txt</code>", report)

    def test_publication_candidate_mismatch_reports_no_push(self):
        root = self.fresh("publish-phase")
        self.artifact(directory=root / "artifact")
        with self.assertRaisesRegex(ValueError, "differs from the built commit"):
            self.invoke_main("publish", root, self.context, EXPECTED_CANDIDATE="0" * 40)
        self.assert_no_push()
        self.assertIn("Nothing was pushed.", self.api.reports[-1])
        self.assertNotIn("Pushed merge commit", self.api.reports[-1])

    def test_failed_or_incomplete_jobs_report_no_push_without_reading_artifacts(self):
        self.merge_mock.reset_mock()
        for state, resolve_result, build_result in (
            ("resolved", "success", "failure"),
            ("resolved", "success", "skipped"),
            ("resolved", "success", "cancelled"),
            ("", "failure", "skipped"),
            ("", "cancelled", "skipped"),
        ):
            with self.subTest(state=state, build=build_result):
                with self.assertRaises(RuntimeError):
                    self.invoke_main("publish", self.fresh("publish"), self.context,
                                     RESOLVE_STATE=state, RESOLVE_RESULT=resolve_result, BUILD_RESULT=build_result)
                self.merge_mock.assert_not_called()
                self.assertEqual(self.transport.calls, [])
                self.assertIn("Nothing was pushed.", self.api.reports[-1])
                self.assertNotIn("Pushed merge commit", self.api.reports[-1])
                self.assertIn(f"**{build_result}**", self.api.reports[-1])

    def test_no_conflicts_report_does_not_require_artifact_or_successful_builds(self):
        self.merge_mock.reset_mock()
        self.invoke_main("publish", self.fresh("publish"), self.context,
                         RESOLVE_STATE="no-conflicts", BUILD_RESULT="skipped")
        self.merge_mock.assert_not_called()
        self.assertEqual(self.transport.calls, [])
        self.assertIn("No merge conflicts found. Nothing was pushed.", self.api.reports[-1])
        self.assertNotIn("Pushed merge commit", self.api.reports[-1])


class ReportParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.details = []
        self.stack = []
        self.images = []
        self.in_summary = False
        self.errors = []

    def handle_starttag(self, tag, attrs):
        if tag == "details":
            node = {"parent": self.stack[-1] if self.stack else None,
                    "attrs": dict(attrs), "summary": ""}
            self.details.append(node)
            self.stack.append(node)
        elif tag == "summary":
            self.in_summary = True
        elif tag == "img":
            self.images.append(dict(attrs))

    def handle_endtag(self, tag):
        if tag == "details":
            if self.stack:
                self.stack.pop()
            else:
                self.errors.append("unmatched details close")
        elif tag == "summary":
            self.in_summary = False

    def handle_data(self, data):
        if self.in_summary and self.stack:
            self.stack[-1]["summary"] += data


class ReportTests(HermeticCase):
    def report(self, state="blocked", build="failure", summary="Summary", paths=None):
        return resolver.render(context(), state, build, summary, paths or ["s.txt"],
                               "3" * 40, "https://github.com/dotnet/maui/actions/runs/123")

    def test_closed_balanced_details_have_sibling_followup_and_nested_changes_builds(self):
        parser = ReportParser()
        parser.feed(self.report())
        self.assertEqual(parser.stack, [])
        self.assertEqual(parser.errors, [])
        self.assertEqual(len(parser.details), 4)
        self.assertTrue(all("open" not in node["attrs"] for node in parser.details))
        top = [node for node in parser.details if node["parent"] is None]
        self.assertEqual(len(top), 2)
        self.assertIn("Conflict Resolution", top[0]["summary"])
        self.assertIn("Follow-up", top[1]["summary"])
        children = [node for node in parser.details if node["parent"] is top[0]]
        self.assertEqual(len(children), 2)
        self.assertIn("Changes", children[0]["summary"])
        self.assertIn("Build validation", children[1]["summary"])

    def test_report_has_exact_scope_commit_badges_author_and_run_links(self):
        report = self.report()
        parser = ReportParser()
        parser.feed(report)
        self.assertTrue(report.startswith("<!-- PR Conflict Resolution -->"))
        self.assertIn("@contributor", report)
        self.assertEqual([image["alt"] for image in parser.images], ["Scope PR conflicts", "Commit 1111111"])
        self.assertTrue(all("style=flat-square" in image["src"] and "1f6feb" in image["src"]
                            for image in parser.images))
        self.assertIn("https://github.com/dotnet/maui/commit/" + "1" * 40, report)
        self.assertIn("https://github.com/dotnet/maui/actions/runs/123", report)
        self.assertIn("comment `/resolve conflicts`", report)

    def test_untrusted_summaries_paths_and_build_status_are_html_escaped(self):
        report = self.report(summary='<script>alert("x")</script>\n@someone & <details open>',
                             paths=['src/<img onerror="x">@someone&.cs'], build="<success>@someone")
        self.assertNotIn("<script>", report)
        self.assertNotIn("<details open>", report)
        self.assertNotIn("<img onerror", report)
        self.assertNotIn("@someone", report)
        self.assertIn("&lt;script&gt;alert(&quot;x&quot;)&lt;/script&gt;<br/>", report)
        self.assertIn("&#64;someone &amp; &lt;details open&gt;", report)
        self.assertIn("<code>src/&lt;img onerror=&quot;x&quot;&gt;&#64;someone&amp;.cs</code>", report)
        self.assertIn("**&lt;success&gt;&#64;someone**", report)

    def test_only_pushed_state_claims_a_push_and_never_claims_pr_merged(self):
        for state, build in (("blocked", "failure"), ("resolved", "success"),
                             ("no-conflicts", "skipped"), ("", "cancelled")):
            with self.subTest(state=state):
                report = self.report(state=state, build=build)
                self.assertIn("Nothing was pushed.", report)
                self.assertNotIn("Pushed merge commit", report)
                self.assertNotIn("contributor/maui/commit/" + "3" * 40, report)
        report = self.report(state="pushed", build="success")
        self.assertIn("Pushed merge commit", report)
        self.assertIn("The PR remains open for normal review and CI.", report)
        self.assertNotIn("Nothing was pushed.", report)

    def test_empty_summary_and_paths_have_explicit_fallbacks(self):
        report = resolver.render(context(), "blocked", "failure", "", [], "",
                                 "https://github.com/dotnet/maui/actions/runs/123")
        self.assertIn("No agent resolution was produced.", report)
        self.assertIn("No resolved files.", report)


class WorkflowContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.workflow = (ROOT / ".github/workflows/resolve-conflicts.yml").read_text(encoding="utf-8")
        cls.module = SCRIPT.read_text(encoding="utf-8")
        jobs_text = cls.workflow.split("\njobs:\n", 1)[1]
        starts = list(re.finditer(r"(?m)^  ([a-z][a-z0-9_-]*):[ \t]*$", jobs_text))
        cls.jobs = {
            match.group(1): jobs_text[match.start():starts[index + 1].start() if index + 1 < len(starts) else None]
            for index, match in enumerate(starts)
        }

    def scalar(self, block, name, indent=4):
        match = re.search(rf"(?m)^{' ' * indent}{re.escape(name)}:[ \t]+([^\n]+)$", block)
        self.assertIsNotNone(match, f"Missing {name} at indentation {indent}")
        return match.group(1).strip()

    def steps(self, job):
        return re.split(r"(?m)^      - ", self.jobs[job].split("    steps:\n", 1)[1])[1:]

    def step(self, job, text):
        matches = [step for step in self.steps(job) if text in step]
        self.assertEqual(len(matches), 1, f"Expected one {job} step containing {text!r}")
        return matches[0]

    def permissions(self, job):
        match = re.search(r"(?ms)^    permissions:\n(.*?)(?=^    \S|\Z)", self.jobs[job])
        self.assertIsNotNone(match, f"Missing {job} permissions")
        return dict(re.findall(r"(?m)^      ([a-z-]+): ([a-z]+)$", match.group(1)))

    def function(self, name):
        match = re.search(rf"(?ms)^def {re.escape(name)}\(.*?(?=^def |\Z)", self.module)
        self.assertIsNotNone(match, f"Missing {name} function")
        return match.group()

    def test_inference_build_and_publication_are_separate_hosted_jobs(self):
        self.assertEqual(set(self.jobs), {"authorize", "resolve", "build", "publish"})
        for name in ("authorize", "resolve", "publish"):
            with self.subTest(job=name):
                self.assertEqual(self.scalar(self.jobs[name], "runs-on"), "ubuntu-latest")
        self.assertEqual(self.scalar(self.jobs["build"], "runs-on"), "${{ matrix.os }}")
        self.assertNotIn("self-hosted", self.workflow)
        for job, phase in (("authorize", "prepare"), ("resolve", "solve"), ("build", "build"), ("publish", "publish")):
            with self.subTest(job=job):
                phases = re.findall(r"\.github/scripts/resolve_conflicts\.py (prepare|solve|build|publish)\b",
                                    self.jobs[job])
                self.assertEqual(phases, [phase])

    def test_created_comment_and_explicit_manual_dispatch_are_the_only_triggers(self):
        triggers = self.workflow.split("\non:\n", 1)[1].split("\npermissions:", 1)[0]
        self.assertRegex(triggers, r"(?m)^  issue_comment:\n    types: \[created\]$")
        self.assertRegex(triggers, r"(?m)^  workflow_dispatch:$")
        self.assertEqual(re.findall(r"(?m)^  ([a-z_]+):", triggers), ["issue_comment", "workflow_dispatch"])
        self.assertRegex(triggers, r"(?m)^      pr_number:\n")
        self.assertIn("github.repository == 'dotnet/maui'", self.jobs["authorize"])

    def test_build_matrix_requires_both_windows_and_macos_without_allowing_failures(self):
        build = self.jobs["build"]
        self.assertRegex(build, r"(?m)^        os: \[windows-latest, macos-latest\]$")
        self.assertRegex(build, r"(?m)^      fail-fast: false$")
        self.assertNotIn("continue-on-error", build)
        self.assertEqual(self.scalar(build, "needs"), "[authorize, resolve]")
        self.assertEqual(self.scalar(build, "if"), "needs.resolve.outputs.state == 'resolved'")
        self.assertIn("if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }", build)

    def test_default_permissions_are_empty_and_builds_have_no_write_permissions_or_credentials(self):
        self.assertRegex(self.workflow, r"(?m)^permissions: \{\}$")
        self.assertNotRegex(self.workflow.split("\njobs:\n", 1)[0], r"(?m)^env:")
        self.assertEqual(self.permissions("build"), {"contents": "read"})
        self.assertEqual(self.permissions("resolve"), {"contents": "read"})
        self.assertEqual(self.permissions("authorize"), {"contents": "read", "pull-requests": "write"})
        self.assertEqual(self.permissions("publish"), {
            "contents": "write", "pull-requests": "write", "issues": "write",
        })
        self.assertNotRegex(self.jobs["build"], re.compile(
            r"\bsecrets\.|\bgithub\.token\b|^\s+(?:[A-Z_]*(?:TOKEN|SECRET)[A-Z_]*|token):", re.MULTILINE
        ))

    def test_every_job_checks_out_pinned_trusted_sha_without_persisted_credentials(self):
        for name in self.jobs:
            with self.subTest(job=name):
                checkout = self.step(name, "uses: actions/checkout@")
                self.assertRegex(checkout, r"uses: actions/checkout@[0-9a-f]{40}(?:\s|$)")
                self.assertEqual(self.scalar(checkout, "ref", indent=10), "${{ github.sha }}")
                self.assertEqual(self.scalar(checkout, "persist-credentials", indent=10), "false")
                self.assertNotRegex(checkout, r"(?m)^          (?:token|repository|submodules):")
                self.assertNotIn("pull_request.head", checkout)

    def test_agent_has_only_step_scoped_inference_credential_and_fixed_gpt_astra(self):
        resolve = self.jobs["resolve"]
        self.assertNotRegex(resolve, r"(?m)^    env:")
        self.assertEqual(self.scalar(resolve, "needs"), "authorize")
        self.assertEqual(self.scalar(resolve, "if"), "needs.authorize.outputs.eligible == 'true'")
        agent = self.step("resolve", "resolve_conflicts.py solve")
        self.assertIn("COPILOT_GITHUB_TOKEN: ${{ secrets.COPILOT_PAT_", agent)
        self.assertNotRegex(resolve, r"(?m)^\s+(?:GH_TOKEN|GITHUB_TOKEN|PUSH_TOKEN):")
        for step in self.steps("resolve"):
            if "resolve_conflicts.py solve" not in step:
                self.assertNotIn("COPILOT_GITHUB_TOKEN:", step)
        self.assertRegex(self.module, r'(?m)^MODEL = "gpt-6-astra"$')
        self.assertIn('"copilot", "--model", MODEL, "--reasoning-effort", "high"', self.function("solve"))
        self.assertRegex(self.step("resolve", "npm install"),
                         r"npm install --global @github/copilot@\d+\.\d+\.\d+(?:\s|$)")

    def test_only_resolution_data_crosses_into_fresh_build_and_publication_runners(self):
        upload = self.step("resolve", "uses: actions/upload-artifact@")
        self.assertEqual(self.scalar(upload, "name", indent=10), "conflict-resolution")
        self.assertEqual(self.scalar(upload, "path", indent=10),
                         "${{ runner.temp }}/conflicts/artifact/resolution.json")
        for job in ("build", "publish"):
            with self.subTest(job=job):
                download = self.step(job, "uses: actions/download-artifact@")
                self.assertEqual(self.scalar(download, "name", indent=10), "conflict-resolution")
                self.assertEqual(self.scalar(download, "path", indent=10), "${{ runner.temp }}/conflicts/artifact")
                self.assertNotIn("github.workspace", download)
                execute = self.step(job, f"resolve_conflicts.py {job}")
                self.assertEqual(self.scalar(execute, "EXPECTED_CANDIDATE", indent=10),
                                 "${{ needs.resolve.outputs.candidate }}")

    def test_publisher_runs_for_authorized_blockers_and_failures_not_only_success(self):
        publish = self.jobs["publish"]
        self.assertEqual(self.scalar(publish, "needs"), "[authorize, resolve, build]")
        self.assertEqual(self.scalar(publish, "if"),
                         "always() && needs.authorize.outputs.authorized == 'true'")
        report = self.step("publish", "resolve_conflicts.py publish")
        self.assertEqual(self.scalar(report, "if", indent=8), "always()")
        for name, value in (
            ("BUILD_RESULT", "${{ needs.build.result }}"),
            ("RESOLVE_RESULT", "${{ needs.resolve.result }}"),
            ("RESOLVE_STATE", "${{ needs.resolve.outputs.state }}"),
            ("CONFLICT_CONTEXT", "${{ needs.authorize.outputs.context }}"),
        ):
            self.assertEqual(self.scalar(report, name, indent=10), value)
        self.assertNotIn("eligible", self.scalar(publish, "if"))
        download = self.step("publish", "uses: actions/download-artifact@")
        self.assertEqual(self.scalar(download, "if", indent=8),
                         "needs.resolve.outputs.state == 'resolved' && needs.build.result == 'success'")

    def test_python_push_is_inside_success_gate_but_report_is_outside(self):
        main = self.function("main")
        guard = re.search(
            r'(?m)^            if state == "resolved" and builds == "success":\n'
            r"(?P<body>(?: {16}[^\n]*\n|\n)+)", main,
        )
        self.assertIsNotNone(guard)
        self.assertIn("push(context,", guard.group("body"))
        self.assertEqual(len(re.findall(r"\bpush\(context,", main)), 1)
        self.assertGreater(main.index("publish_comment(context, render("), guard.end())
        push = self.function("push")
        self.assertIn('if build_result != "success":', push)
        self.assertLess(push.index('if build_result != "success":'), push.index("revalidate(context)"))

    def test_publisher_uses_dedicated_push_credential_and_never_runs_agent_or_build(self):
        publish = self.jobs["publish"]
        report = self.step("publish", "resolve_conflicts.py publish")
        self.assertEqual(self.scalar(report, "PUSH_TOKEN", indent=10),
                         "${{ secrets.GH_AW_GITHUB_TOKEN || github.token }}")
        self.assertEqual(self.scalar(report, "GH_TOKEN", indent=10), "${{ github.token }}")
        self.assertNotIn("COPILOT_PAT", publish)
        self.assertNotIn("COPILOT_GITHUB_TOKEN", publish)
        self.assertNotRegex(publish, r"\bdotnet (?:build|test|run|cake|restore)\b|\bcopilot --|\bnpm install\b")

    def test_no_force_flags_or_workflow_shell_push_bypass(self):
        self.assertNotRegex(self.workflow + self.module, r"--force(?:-with-lease)?(?:[=,\s\"']|$)")
        self.assertNotRegex(self.workflow, r"\bgit\s+(?:push|rebase)\b")
        self.assertNotRegex(self.function("push"), r"""["']-f["']""")


if __name__ == "__main__":
    unittest.main()
