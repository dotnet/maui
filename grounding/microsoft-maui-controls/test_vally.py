import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from summarize_vally import classify
from vally import generate


class VallyTests(unittest.TestCase):
    def test_generation_separates_agent_and_grader_sources_and_changes_only_skill_availability(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            baseline = root / "source"
            (baseline / "app").mkdir(parents=True)
            (root / "skill").mkdir()
            (root / "skill/SKILL.md").write_text("unit skill fixture")
            for name in ("stage.json", "Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (baseline / name).write_text("unit fixture")
            with patch("vally.stage", return_value=baseline):
                result = generate(root / "study", root / "skill")
            plain = json.loads((result / "baseline/eval.yaml").read_text())
            available = json.loads((result / "skill-available/eval.yaml").read_text())
            self.assertNotIn("scoring", plain)
            self.assertIn("paused", plain["agent_environment"]["commands"][0])
            self.assertNotIn("skills", plain["agent_environment"])
            skill_snapshot = result / "skills/skill"
            self.assertEqual([str(skill_snapshot)], available["agent_environment"]["skills"])
            self.assertEqual((root / "skill/SKILL.md").read_bytes(), (skill_snapshot / "SKILL.md").read_bytes())
            self.assertEqual(plain["stimuli"], available["stimuli"])
            for stimulus in plain["stimuli"]:
                agent_sources = [Path(item["src"]) for item in stimulus["agent_environment"]["files"]]
                for item in stimulus["grading_environment"]["files"]:
                    hidden = Path(item["src"])
                    self.assertTrue(all(hidden != source and not hidden.is_relative_to(source)
                                        for source in agent_sources))
                    if item["dest"].startswith(".vally-grading/instrument/"):
                        self.assertTrue(hidden.is_relative_to(result / "instrument"))
                        self.assertEqual((Path(__file__).parent / hidden.name).read_bytes(), hidden.read_bytes())
                reference = result / "references" / stimulus["name"]
                receipt = json.loads((reference / "trajectory.json").read_text())
                self.assertEqual("reference-replay-no-agent", receipt["agent"]["name"])
                self.assertEqual(stimulus["prompt"], receipt["steps"][0]["message"])
                for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                    self.assertEqual((baseline / name).read_bytes(), (reference / name).read_bytes())

    def test_infrastructure_is_not_a_candidate_failure_and_inconsistent_results_are_rejected(self):
        for status in ("target-pass", "candidate-failed", "infrastructure"):
            result = {"status": status, "kind": "reference-replay", "evidence": "/unit/evidence"}
            record = {
                "status": "success", "trajectory": {"stimulus": {"name": "unit"}},
                "gradeResult": {"passed": status == "target-pass", "details": [
                    {"graderType": "run-command", "metadata": {"stdout": json.dumps(result)}},
                ]},
            }
            with self.subTest(status=status):
                self.assertEqual(status, classify(record)["status"])
                record["gradeResult"]["passed"] = not record["gradeResult"]["passed"]
                with self.assertRaises(ValueError):
                    classify(record)


if __name__ == "__main__":
    unittest.main()
