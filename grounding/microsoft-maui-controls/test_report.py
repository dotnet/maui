import json
from pathlib import Path
import tempfile
import unittest

from controls import ControlFailure
from report import recheck
from stage import HERE, files, save, sha256


class ReportTests(unittest.TestCase):
    def fixture(self, root):
        case = {
            "id": "example", "commit": "a" * 40, "archiveSha256": "b" * 64,
            "runtime": [{"name": "ready", "assert": [{"selector": {"text": "original"}}]}],
            "expectedControlNative": {"source": "ShellFlyoutRenderer"},
        }
        consumer = root / "consumers/example-source"
        app = consumer / "app"
        apk = app / "bin/Debug/example-Signed.apk"
        apk.parent.mkdir(parents=True)
        apk.write_bytes(b"synthetic unit-test APK, not runtime evidence")
        (app / "prompt.txt").write_text("synthetic unit-test prompt\n")
        save(consumer / "stage.json", {
            "mode": "source", "commit": case["commit"], "archiveSha256": case["archiveSha256"],
        })
        inputs = files(consumer)
        inputs.pop("stage.json")
        output = root / "results/example-source-unit"
        (output / "runtime").mkdir(parents=True)
        save(output / "verdict.json", {
            "status": "control-pass", "agentAttempt": False, "mode": "source", "case": "example",
        })
        save(output / "build.json", {
            "mode": "source", "agentAttempt": False, "apk": str(apk), "applicationId": "com.example.app",
            "apkSha256": sha256(apk.read_bytes()), "inputHashes": inputs,
        })
        commands = [["dotnet", "--version"], ["dotnet", "restore"], ["dotnet", "msbuild"],
                    ["dotnet", "build", "-c", "Debug"], ["dotnet", "restore"],
                    ["dotnet", "build", "-c", "Release"], ["adb", "install", "-r", str(apk)]]
        save(output / "commands.json", [
            {"command": command, "status": "completed", "exitCode": 0} for command in commands
        ])
        for index in range(len(commands)):
            (output / f"{index:02}.log").write_text("synthetic unit-test command output\n")
            (output / f"{index:02}.stderr").write_text("")
        save(output / "harness.json", {
            "suiteSha256": sha256((HERE / "suite.json").read_bytes()),
            "files": {name: sha256((HERE / name).read_bytes()) for name in ("stage.py", "controls.py")},
        })
        save(output / "runtime/verdict.json", {
            "status": "pass", "agentAttempt": False, "applicationId": "com.example.app",
            "checkpoints": ["ready"], "apk": str(apk), "apkSha256": sha256(apk.read_bytes()),
        })
        (output / "runtime/00-ready.xml").write_text(
            '<hierarchy><node package="com.example.app" text="original" /></hierarchy>'
        )
        (output / "runtime/native.txt").write_text(
            "  ACTIVITY com.example.app/.Main\nmResumed=true\nunit.ShellFlyoutRenderer{test}\n"
        )
        (output / "runtime.log").write_text("synthetic unit-test application log\n")
        (output / "launch.txt").write_text("Status: ok\n")
        return case, output, app, apk

    def test_complete_saved_evidence_is_rechecked_without_claiming_a_model_attempt(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            case, _, _, _ = self.fixture(root)
            result = recheck(root, case, "source", "unit")
            self.assertEqual("control-pass", result["status"])
            self.assertFalse(result["agentAttempt"])
            self.assertEqual(1, result["runtimeCheckpoints"])

    def test_pass_verdict_cannot_hide_missing_or_changed_evidence(self):
        for change in ("ui", "apk", "input", "native", "command", "harness", "missing-output"):
            with self.subTest(change=change), tempfile.TemporaryDirectory() as temp:
                root = Path(temp)
                case, output, app, apk = self.fixture(root)
                if change == "ui":
                    (output / "runtime/00-ready.xml").write_text(
                        '<hierarchy><node package="com.example.app" text="wrong" /></hierarchy>'
                    )
                elif change == "apk":
                    apk.write_bytes(b"changed")
                elif change == "input":
                    (app / "prompt.txt").write_text("changed")
                elif change == "native":
                    (output / "runtime/native.txt").write_text("  ACTIVITY another.app/.Main\nmResumed=true\n")
                elif change == "command":
                    rows = json.loads((output / "commands.json").read_text())
                    rows[-1]["exitCode"] = 1
                    save(output / "commands.json", rows)
                elif change == "harness":
                    harness = json.loads((output / "harness.json").read_text())
                    harness["files"]["controls.py"] = "changed"
                    save(output / "harness.json", harness)
                else:
                    (output / "00.log").unlink()
                with self.assertRaises(ControlFailure):
                    recheck(root, case, "source", "unit")


if __name__ == "__main__":
    unittest.main()
