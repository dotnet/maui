import json
from pathlib import Path
import tempfile
import unittest

from candidate import CONFIGS, check_shell_observation, copy_app, prepare_delivery
from controls import ControlFailure, InfrastructureFailure
from stage import files, save, sha256


class CandidateTests(unittest.TestCase):
    def test_shell_architecture_accepts_subclasses_with_real_framework_ancestry(self):
        log = (
            "I MauiSkillsProbe: run=current;handler=App.CustomHandler,App|"
            "Microsoft.Maui.Controls.Handlers.ShellHandler,Microsoft.Maui.Controls;"
            "platform=App.CustomDrawer,App|Microsoft.Maui.Platform.MauiDrawerLayout,Microsoft.Maui;"
            "assembly=11.0.0.0"
        )
        self.assertEqual("App.CustomHandler,App", check_shell_observation(log, "current")["handler"][0])
        for bad in [
            log.replace("run=current", "run=another"),
            log.replace("ShellHandler,Microsoft.Maui.Controls", "ShellHandler,App"),
            log.replace("ShellHandler,Microsoft.Maui.Controls", "ShellRenderer,Microsoft.Maui.Controls"),
            log.replace("MauiDrawerLayout,Microsoft.Maui", "ShellFlyoutRenderer,Microsoft.Maui.Controls"),
            log.replace("assembly=11", "assembly=10"),
        ]:
            with self.subTest(log=bad), self.assertRaises(ControlFailure):
                check_shell_observation(bad, "current")

    def test_sealing_skips_build_outputs_and_rejects_linked_inputs(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            source = root / "input"
            (source / "bin").mkdir(parents=True)
            (source / "bin/ignored").write_text("build output")
            (source / "Page.cs").write_text("class Page {}")
            copy_app(source, root / "sealed")
            self.assertEqual({"Page.cs"}, set(files(root / "sealed")))
            (source / "linked.cs").symlink_to(root / "sealed/Page.cs")
            with self.assertRaises(ControlFailure):
                copy_app(source, root / "bad")

    def test_grading_uses_protected_configuration_and_distinguishes_its_drift(self):
        for tamper in (None, "delivery", "protected"):
            with self.subTest(tamper=tamper), tempfile.TemporaryDirectory() as temp:
                root = Path(temp)
                source = root / "source"
                source.mkdir()
                delivered = root / "delivery"
                (delivered / "app").mkdir(parents=True)
                (delivered / "app/Page.cs").write_text("class EditedPage {}")
                case = {"id": "test", "project": "App.csproj", "commit": "a" * 40, "archiveSha256": "b" * 64}
                hashes = {}
                for name in CONFIGS:
                    data = b"protected config"
                    (source / name).write_bytes(data)
                    (delivered / name).write_bytes(data)
                    hashes[name] = sha256(data)
                save(source / "stage.json", {
                    **case, "case": "test", "mode": "source", "stagedFiles": hashes,
                })
                if tamper:
                    changed = source if tamper == "protected" else delivered
                    (changed / CONFIGS[0]).write_text("changed")
                    error = InfrastructureFailure if tamper == "protected" else ControlFailure
                    with self.assertRaises(error):
                        prepare_delivery(source, delivered / "app", root / "grading", case)
                else:
                    consumer = prepare_delivery(source, delivered / "app", root / "grading", case)
                    record = json.loads((consumer / "stage.json").read_text())
                    self.assertEqual("target", record["mode"])
                    actual = files(consumer)
                    actual.pop("stage.json")
                    self.assertEqual(record["stagedFiles"], actual)


if __name__ == "__main__":
    unittest.main()
