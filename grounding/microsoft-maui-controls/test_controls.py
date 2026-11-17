import io
import json
import os
from pathlib import Path
import sys
import tarfile
import tempfile
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

from controls import ControlFailure, InfrastructureFailure, check_anr, check_ui, run, runtime, tap_target, validate_runtime_contract
from stage import PACKAGE, SDK, SUITE, adapt, contained, files, sha256, stage


class ControlsTests(unittest.TestCase):
    def test_runtime_rejects_a_changed_application_id_before_device_access(self):
        with patch("controls.run") as command:
            with self.assertRaisesRegex(ControlFailure, "application ID"):
                runtime(SUITE["cases"][0], None, "different.application", "unit-owned",
                        None, {}, None, [], "target")
            command.assert_not_called()

    def test_runtime_replaces_only_the_exact_owned_sample_before_installing(self):
        case = SUITE["cases"][0]
        application_id = case["applicationId"]
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            apk = root / "sample.apk"
            apk.write_bytes(b"unit fixture, not an Android package")
            (root / "build.json").write_text(json.dumps({"apkSha256": sha256(apk.read_bytes())}))
            for installed, should_remove in [
                (f"package:{application_id}\n", True),
                (f"package:{application_id}.unrelated\n", False),
                ("", False),
            ]:
                outputs = [application_id, "24", "device", installed]
                if should_remove:
                    outputs.append("Success")
                outputs.append(InfrastructureFailure("unit stop before install"))
                with self.subTest(installed=installed), patch("controls.run", side_effect=outputs) as command:
                    with self.assertRaisesRegex(InfrastructureFailure, "unit stop before install"):
                        runtime(case, apk, application_id, "unit-owned", root,
                                {"ANDROID_HOME": str(root)}, root, [], "target")
                    removals = [call.args[0][3:] for call in command.call_args_list
                                if call.args[0][3:4] == ["uninstall"]]
                    self.assertEqual([["uninstall", application_id]] if should_remove else [], removals)
                    self.assertEqual(["install", "-r", apk], command.call_args_list[-1].args[0][3:])

    def test_duplicate_source_menu_taps_require_an_explicit_count_and_index(self):
        tree = ET.fromstring('<hierarchy><node text="Enable Buttons" index="0" />'
                             '<node text="Enable Buttons" index="1" /></hierarchy>')
        step = {"tap": {"text": "Enable Buttons"}}
        with self.assertRaises(ControlFailure):
            tap_target(tree, step)
        step.update(tapCount=2, tapIndex=0)
        self.assertEqual("0", tap_target(tree, step).get("index"))
        step["tapIndex"] = 2
        with self.assertRaises(ControlFailure):
            tap_target(tree, step)

    def test_command_stdout_is_not_contaminated_by_java_or_tool_stderr(self):
        with tempfile.TemporaryDirectory() as temp:
            output = Path(temp)
            rows = []
            text = run([sys.executable, "-c", "import sys; print('identity'); print('JAVA options', file=sys.stderr)"],
                       output, os.environ.copy(), output, rows)
            self.assertEqual("identity\n", text)
            self.assertIn("JAVA options", (output / "00.stderr").read_text())
            self.assertEqual(0, rows[0]["exitCode"])

    def test_timeouts_are_incomplete_infrastructure_not_success(self):
        with tempfile.TemporaryDirectory() as temp:
            output = Path(temp)
            rows = []
            with self.assertRaises(InfrastructureFailure):
                run([sys.executable, "-c", "import time; time.sleep(60)"], output, os.environ.copy(),
                    output, rows, timeout=0.1)
            self.assertEqual("timeout", rows[0]["status"])
            self.assertNotIn("exitCode", rows[0])

    def test_system_ui_and_application_anrs_are_not_silently_accepted(self):
        for message, error in [
            ("System UI isn't responding", InfrastructureFailure),
            ("Sample isn't responding", ControlFailure),
        ]:
            with self.subTest(message=message):
                tree = ET.Element("hierarchy")
                ET.SubElement(tree, "node", text=message)
                with self.assertRaises(error):
                    check_anr(tree)
                if error is ControlFailure:
                    try:
                        check_anr(tree)
                    except ControlFailure as result:
                        self.assertNotIsInstance(result, InfrastructureFailure)

    def test_every_checkpoint_needs_a_safe_name_and_a_positive_observation(self):
        for steps in [
            [],
            [{"name": "empty", "assert": []}],
            [{"name": "../escape", "assert": [{"selector": {"text": "ok"}}]}],
            [{"name": "negative", "assert": [{"selector": {"text": "missing"}, "count": 0}]}],
        ]:
            with self.subTest(steps=steps), self.assertRaises(ControlFailure):
                validate_runtime_contract({"runtime": steps})

    def test_all_declared_workflows_have_nontrivial_checkpoints(self):
        for case in SUITE["cases"]:
            with self.subTest(case=case["id"]):
                validate_runtime_contract(case)
                self.assertGreaterEqual(len(case["runtime"]), 5)

    def test_ui_requires_the_requested_count_and_values(self):
        tree = ET.fromstring('<hierarchy><node text="Clicked 1 time" enabled="true" /></hierarchy>')
        check_ui(tree, [{"selector": {"text": "Clicked 1 time"}, "attributes": {"enabled": "true"}}])
        for check in [
            {"selector": {"text": "Clicked 2 times"}},
            {"selector": {"text": "Clicked 1 time"}, "count": 2},
            {"selector": {"text": "Clicked 1 time"}, "attributes": {"enabled": "false"}},
        ]:
            with self.subTest(check=check), self.assertRaises(ControlFailure):
                check_ui(tree, [check])

    def test_path_traversal_and_absolute_paths_are_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            for relative in ("../escape", "/absolute", "app/../../escape", "app\\..\\escape"):
                with self.subTest(relative=relative), self.assertRaises(ValueError):
                    contained(root, relative)

    def test_symlink_escape_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "app").symlink_to(root.parent, target_is_directory=True)
            with self.assertRaises(ValueError):
                contained(root, "app/escape")

    def test_project_adaptation_pins_one_framework_without_disabling_checks(self):
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "App.csproj"
            project.write_text(
                '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
                '<TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>'
                '<SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>'
                '<TreatWarningsAsErrors>true</TreatWarningsAsErrors>'
                '</PropertyGroup><ItemGroup>'
                '<PackageReference Include="Microsoft.Maui.Controls" Version="old" />'
                '</ItemGroup></Project>'
            )
            removed = adapt(project, "target")
            tree = ET.parse(project)
            self.assertEqual("net11.0-android", tree.find(".//TargetFramework").text)
            self.assertIsNone(tree.find(".//TargetFrameworks"))
            self.assertEqual("true", tree.find(".//TreatWarningsAsErrors").text)
            self.assertEqual(PACKAGE["target"], tree.find(".//MauiVersion").text)
            self.assertEqual(3, len(removed))

    def test_pinned_stage_keeps_license_and_separates_source_from_evaluator(self):
        case = {
            "id": "example", "repo": "owner/repo", "commit": "a" * 40,
            "directory": "samples/App", "license": "LICENSE", "project": "App.csproj",
            "prompt": "Upgrade this app.",
        }
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            cache = root / "sources"
            cache.mkdir()
            archive = cache / f"owner-repo-{'a' * 40}.tar.gz"
            with tarfile.open(archive, "w:gz") as target:
                for name, data in {
                    "LICENSE": b"MIT License",
                    "samples/App/App.csproj": b'<Project Sdk="Microsoft.NET.Sdk" />',
                    "samples/App/Page.cs": b"class Page {}",
                    "samples/App/.github/skills/teacher/SKILL.md": b"not an app input",
                    "samples/App/AGENTS.md": b"not an app input",
                    "unrelated/solution.cs": b"not an app input",
                }.items():
                    member = tarfile.TarInfo(f"repo-sha/{name}")
                    member.size = len(data)
                    target.addfile(member, io.BytesIO(data))
            case["archiveSha256"] = sha256(archive.read_bytes())
            with patch("urllib.request.urlopen", side_effect=AssertionError("Unexpected network")):
                consumer = stage({"cases": [case]}, "example", root, "source")
            manifest = json.loads((consumer / "stage.json").read_text())
            self.assertEqual("MIT License", (consumer / "UPSTREAM-LICENSE").read_text())
            self.assertEqual(SDK["source"], json.loads((consumer / "app/global.json").read_text())["sdk"]["version"])
            self.assertFalse((consumer / "app/AGENTS.md").exists())
            self.assertFalse((consumer / "app/.github").exists())
            self.assertFalse((consumer / "unrelated").exists())
            actual = files(consumer)
            actual.pop("stage.json")
            self.assertEqual(manifest["stagedFiles"], actual)
            self.assertFalse(manifest["agentAttempt"])
            with self.assertRaises(FileExistsError):
                stage({"cases": [case]}, "example", root, "source")
            archive.write_bytes(archive.read_bytes() + b"changed")
            with self.assertRaisesRegex(ValueError, "checksum"):
                stage({"cases": [case]}, "example", root, "target")


if __name__ == "__main__":
    unittest.main()
