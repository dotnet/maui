"""Grade a sealed app delivery; never run an agent or trust its claimed results."""

import argparse
import json
import os
from pathlib import Path
import re
import shutil
import sys
import uuid
import xml.etree.ElementTree as ET

from controls import ControlFailure, InfrastructureFailure, build, runtime
from stage import HERE, IGNORED, SUITE, files, save, sha256


CONFIGS = ("Directory.Build.props", "Directory.Build.targets", "NuGet.config")


def copy_app(source, destination):
    if source.is_symlink() or not source.is_dir():
        raise ControlFailure("Delivery must be a real app directory.")
    destination.mkdir(parents=True, exist_ok=False)
    for directory, directories, names in os.walk(source):
        directories[:] = sorted(name for name in directories if name not in IGNORED)
        for name in [*directories, *names]:
            path = Path(directory) / name
            if name not in IGNORED and (path.is_symlink() or path.is_file() and path.stat().st_nlink > 1):
                raise ControlFailure(f"Linked delivery input is not accepted: {path.relative_to(source)}")
        target = destination / Path(directory).relative_to(source)
        target.mkdir(parents=True, exist_ok=True)
        for name in names:
            if name not in IGNORED:
                shutil.copyfile(Path(directory) / name, target / name)


def prepare_delivery(source_stage, delivery, output, case):
    record = json.loads((source_stage / "stage.json").read_text())
    if (record["case"] != case["id"] or record["mode"] != "source"
            or record["commit"] != case["commit"] or record["archiveSha256"] != case["archiveSha256"]):
        raise InfrastructureFailure("Protected baseline provenance does not match the case.")
    consumer = output / "consumer"
    copy_app(delivery, consumer / "app")
    for name in CONFIGS:
        data = (source_stage / name).read_bytes()
        if sha256(data) != record["stagedFiles"][name]:
            raise InfrastructureFailure(f"Protected baseline configuration drifted: {name}")
        delivered_config = delivery.parent / name
        if (not delivered_config.is_file() or delivered_config.is_symlink()
                or sha256(delivered_config.read_bytes()) != sha256(data)):
            raise ControlFailure(f"Read-only delivery configuration is missing or changed: {name}")
        (consumer / name).write_bytes(data)
    save(consumer / "stage.json", {
        "mode": "target", "project": case["project"],
        "stagedFiles": files(consumer), "agentAttempt": False,
    })
    return consumer


def check_shell_observation(log, token):
    observations = []
    pattern = re.compile(r"run=" + re.escape(token) + r";handler=([^;]+);platform=([^;]+);assembly=([0-9.]+)")
    for line in log.splitlines():
        if "MauiSkillsProbe" not in line:
            continue
        match = pattern.search(line)
        if match:
            observations.append({
                "handler": match[1].split("|"), "platform": match[2].split("|"), "assembly": match[3],
            })
    if not observations:
        raise ControlFailure("No evaluator-owned, current-run Shell observation was captured.")
    expected_handler = "Microsoft.Maui.Controls.Handlers.ShellHandler,Microsoft.Maui.Controls"
    expected_platform = "Microsoft.Maui.Platform.MauiDrawerLayout,Microsoft.Maui"
    for observation in observations:
        if (expected_handler not in observation["handler"]
                or expected_platform not in observation["platform"]
                or observation["assembly"] != "11.0.0.0"):
            raise ControlFailure(f"Observed Shell does not use the target architecture: {observation}")
    return observations[-1]


def grade(args):
    case = next(case for case in SUITE["cases"] if case["id"] == args.case)
    output = (args.output if args.output is not None else args.output_root / uuid.uuid4().hex).resolve()
    delivery = args.candidate_app.absolute()
    for protected in (delivery.resolve(), args.source_stage.resolve()):
        if output == protected or output.is_relative_to(protected):
            raise ControlFailure("Grading output must stay outside candidate and protected baseline inputs.")
    output.mkdir(parents=True, exist_ok=False)
    evidence = output / "evidence"
    evidence.mkdir()
    rows = []
    try:
        consumer = prepare_delivery(args.source_stage.resolve(), delivery, output, case)
        delivered_hashes = files(delivery)
        if files(consumer / "app") != delivered_hashes:
            raise InfrastructureFailure("The delivery changed while it was being sealed.")
        save(output / "delivery.json", {"case": case["id"], "files": delivered_hashes, "kind": args.kind})
        token = uuid.uuid4().hex
        probe_targets = None
        if case.get("expectedControlNative"):
            probe = output / "observer"
            probe.mkdir()
            source = (HERE / "ShellObservation.cs").read_text().replace("__RUN_TOKEN__", token)
            (probe / "ShellObservation.cs").write_text(source)
            project = ET.Element("Project")
            group = ET.SubElement(project, "ItemGroup", Condition=f"'$(MSBuildProjectFile)' == '{Path(case['project']).name}'")
            ET.SubElement(group, "Compile", Include=str(probe / "ShellObservation.cs"))
            probe_targets = probe / "Observe.targets"
            ET.ElementTree(project).write(probe_targets, encoding="unicode")
        save(output / "harness.json", {
            "suiteSha256": sha256((HERE / "suite.json").read_bytes()),
            "files": {name: sha256((HERE / name).read_bytes())
                      for name in ("candidate.py", "controls.py", "stage.py", "ShellObservation.cs")},
        })
        apk, application_id, env = build(consumer, evidence, args.cache_root.resolve(), rows, probe_targets)
        candidate_case = {**case, "expectedControlNative": {}}
        runtime(candidate_case, apk, application_id, args.serial, consumer / "app", env, evidence, rows, "target")
        observation = None
        if probe_targets:
            observation = check_shell_observation((evidence / "runtime.log").read_text(), token)
        if files(delivery) != delivered_hashes:
            raise InfrastructureFailure("The sealed delivery changed during grading.")
        verdict = {
            "status": "target-pass", "kind": args.kind, "case": case["id"],
            "satisfies": True, "targetArchitecture": observation,
            "evidence": str(output),
            "modelEffectiveness": "not-inferred-from-grader-result",
        }
        save(output / "verdict.json", verdict)
        print(json.dumps(verdict))
        return 0
    except (ControlFailure, ValueError, KeyError, OSError) as error:
        infrastructure = isinstance(error, (InfrastructureFailure, OSError))
        verdict = {
            "status": "infrastructure" if infrastructure else "candidate-failed",
            "error": str(error), "kind": args.kind, "evidence": str(output),
        }
        save(output / "verdict.json", verdict)
        print(json.dumps(verdict))
        print(str(error), file=sys.stderr)
        return 2 if infrastructure else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--candidate-app", type=Path, required=True)
    parser.add_argument("--source-stage", type=Path, required=True)
    parser.add_argument("--cache-root", type=Path, required=True)
    outputs = parser.add_mutually_exclusive_group(required=True)
    outputs.add_argument("--output", type=Path)
    outputs.add_argument("--output-root", type=Path)
    parser.add_argument("--case", choices=[case["id"] for case in SUITE["cases"]], required=True)
    parser.add_argument("--serial", required=True)
    parser.add_argument("--kind", choices=("reference-replay", "sealed-delivery"), required=True)
    sys.exit(grade(parser.parse_args()))


if __name__ == "__main__":
    main()
