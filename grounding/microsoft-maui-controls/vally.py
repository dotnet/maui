"""Generate real Vally specs. This command never starts a model."""

import argparse
from pathlib import Path
import shlex
import shutil
import sys

from candidate import CONFIGS, copy_app
from stage import HERE, SUITE, save, stage


def generate(root, skill):
    root = root.resolve()
    destination = root / "vally"
    destination.mkdir(parents=True, exist_ok=False)
    for variant in ("baseline", "skill-available"):
        (destination / variant).mkdir()
    instrument = destination / "instrument"
    instrument.mkdir()
    for name in ("candidate.py", "controls.py", "stage.py", "suite.json", "ShellObservation.cs"):
        shutil.copyfile(HERE / name, instrument / name)
    skill_snapshot = destination / "skills" / skill.name
    copy_app(skill.resolve(), skill_snapshot)
    stimuli = []
    for case in SUITE["cases"]:
        baseline = stage(SUITE, case["id"], root / "vally-inputs", "source")
        target = stage(SUITE, case["id"], root / "vally-inputs", "target")
        reference = destination / "references" / case["id"]
        copy_app(target / "app", reference / "app")
        for name in CONFIGS:
            shutil.copyfile(baseline / name, reference / name)
        save(reference / "trajectory.json", {
            "schema_version": "ATIF-v1.6", "session_id": f"reference-{case['id']}",
            "agent": {"name": "reference-replay-no-agent", "version": "1"},
            "steps": [
                {"step_id": 1, "source": "user", "message": case["prompt"]},
                {"step_id": 2, "source": "agent",
                 "message": "Evaluator-owned reference delivery; no model executed."},
            ],
        })
        protected = destination / "protected" / case["id"]
        protected.mkdir(parents=True)
        for name in ("stage.json", "Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
            shutil.copyfile(baseline / name, protected / name)
        grading_files = [
            {"src": str(instrument / name), "dest": f".vally-grading/instrument/{name}"}
            for name in ("candidate.py", "controls.py", "stage.py", "suite.json", "ShellObservation.cs")
        ] + [
            {"src": str(protected / name), "dest": f".vally-grading/source/{name}"}
            for name in ("stage.json", "Directory.Build.props", "Directory.Build.targets", "NuGet.config")
        ]
        stimuli.append({
            "name": case["id"], "prompt": case["prompt"],
            "agent_environment": {
                "files": [{"src": str(baseline / "app"), "dest": "app"}] + [
                    {"src": str(baseline / name), "dest": name}
                    for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config")
                ],
            },
            "grading_environment": {"files": grading_files},
            "graders": [{
                "type": "run-command",
                "config": {
                    "command": sys.executable,
                    "args": [
                        ".vally-grading/instrument/candidate.py",
                        "--candidate-app", "app", "--source-stage", ".vally-grading/source",
                        "--case", case["id"], "--cache-root", str(root),
                        "--output-root", str(root / "candidate-grades"),
                        "--serial", "${SERIAL}", "--kind", "reference-replay",
                    ],
                    "timeout": "40m", "expected_exit_code": 0,
                    "stdout_contains": '"status": "target-pass"',
                },
            }],
        })
    specification = {
        "name": "maui-controls-11-consumers", "version": "1", "type": "capability",
        "description": "Three Android upgrade tasks; reference replay verifies the deterministic grader, not model efficacy.",
        "defaults": {"runs": 1, "timeout": "40m"},
        "agent_environment": {
            "commands": [
                f'''{shlex.quote(sys.executable)} -c "raise SystemExit('Agent execution is paused: approve a model budget and verify an app-only sandbox before enabling trials.')"'''
            ],
        },
        "stimuli": stimuli,
    }
    save(destination / "baseline/eval.yaml", specification)
    specification["name"] = "maui-controls-11-consumers-skill-available"
    specification["agent_environment"]["skills"] = [str(skill_snapshot)]
    save(destination / "skill-available/eval.yaml", specification)
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--skill", type=Path, required=True)
    args = parser.parse_args()
    if not (args.skill / "SKILL.md").is_file():
        parser.error("--skill must identify the shipping skill directory.")
    print(generate(args.root, args.skill))


if __name__ == "__main__":
    main()
