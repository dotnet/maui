"""Recheck saved control evidence without model inference or an LLM judge."""

import argparse
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

from controls import ControlFailure, check_anr, check_ui, validate_runtime_contract
from stage import HERE, SUITE, files, sha256, save


def recheck(root, case, mode, run_id):
    output = root / "results" / f"{case['id']}-{mode}-{run_id}"
    verdict = json.loads((output / "verdict.json").read_text())
    if verdict != {"status": "control-pass", "agentAttempt": False, "mode": mode, "case": case["id"]}:
        raise ControlFailure(f"Not a completed control: {output}")
    rows = json.loads((output / "commands.json").read_text())
    if len(rows) < 7 or not all(row.get("status") == "completed" and row.get("exitCode") == 0 for row in rows):
        raise ControlFailure(f"Incomplete command evidence: {output}")
    harness = json.loads((output / "harness.json").read_text())
    if (harness["suiteSha256"] != sha256((HERE / "suite.json").read_bytes())
            or any(harness["files"][name] != sha256((HERE / name).read_bytes())
                   for name in ("stage.py", "controls.py"))):
        raise ControlFailure("Control was executed with a different suite or harness; rerun it.")
    for index in range(len(rows)):
        for extension in ("log", "stderr"):
            if not (output / f"{index:02}.{extension}").is_file():
                raise ControlFailure("A command's raw output is missing.")
    build = json.loads((output / "build.json").read_text())
    if build["mode"] != mode or build["agentAttempt"] is not False:
        raise ControlFailure("Build is not the requested reference control.")
    apk = Path(build["apk"]).resolve()
    consumer = root / "consumers" / f"{case['id']}-{mode}"
    apk.relative_to((consumer / "app").resolve())
    if sha256(apk.read_bytes()) != build["apkSha256"]:
        raise ControlFailure("Saved APK no longer matches the evaluated build.")
    current = files(consumer)
    current.pop("stage.json")
    if current != build["inputHashes"]:
        raise ControlFailure("Saved control input hashes no longer match.")
    runtime = json.loads((output / "runtime/verdict.json").read_text())
    names = [step["name"] for step in case["runtime"]]
    if (runtime["status"] != "pass" or runtime["agentAttempt"] is not False
            or runtime["checkpoints"] != names or runtime["apk"] != str(apk)
            or runtime["apkSha256"] != build["apkSha256"]
            or runtime["applicationId"] != build["applicationId"]):
        raise ControlFailure("Missing or inconsistent runtime evidence.")
    validate_runtime_contract(case)
    for index, step in enumerate(case["runtime"]):
        tree = ET.parse(output / "runtime" / f"{index:02}-{step['name']}.xml")
        check_anr(tree)
        app_tree = ET.Element("hierarchy")
        for node in tree.iter("node"):
            if node.get("package") == build["applicationId"]:
                app_tree.append(ET.Element("node", node.attrib))
        check_ui(app_tree, step["assert"])
    native = (output / "runtime/native.txt").read_text()
    active = [section for section in re.split(r"(?=^  ACTIVITY )", native, flags=re.MULTILINE)
              if section.startswith(f"  ACTIVITY {build['applicationId']}/") and "mResumed=true" in section]
    expected = case.get("expectedControlNative", {}).get(mode)
    if len(active) != 1 or expected and not re.search(r"\." + re.escape(expected) + r"\{", active[0]):
        raise ControlFailure("Saved native reference-control evidence does not satisfy the contract.")
    if not (output / "runtime.log").read_text().strip() or "Status: ok" not in (output / "launch.txt").read_text():
        raise ControlFailure("Application launch/log evidence is missing.")
    for configuration in ("Debug", "Release"):
        if not any(row["command"][:2] == ["dotnet", "build"]
                   and configuration in row["command"] for row in rows):
            raise ControlFailure(f"Missing {configuration} build command.")
    install = [row for row in rows if "install" in row["command"]]
    if len(install) != 1 or install[0]["command"][-1] != str(apk):
        raise ControlFailure("Installed APK differs from the recorded build.")
    stage = json.loads((consumer / "stage.json").read_text())
    if (stage["mode"] != mode or stage["commit"] != case["commit"]
            or stage["archiveSha256"] != case["archiveSha256"]):
        raise ControlFailure("Stage provenance differs from the pinned case.")
    return {
        "case": case["id"], "mode": mode, "status": "control-pass",
        "runtimeCheckpoints": len(names), "completedCommands": len(rows),
        "apkSha256": build["apkSha256"], "sourceCommit": stage["commit"],
        "archiveSha256": stage["archiveSha256"],
        "promptSha256": sha256((consumer / "app/prompt.txt").read_bytes()),
        "agentAttempt": False,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--run-id", required=True)
    args = parser.parse_args()
    if not re.fullmatch(r"[a-zA-Z0-9-]+", args.run_id):
        parser.error("--run-id must contain only letters, digits or hyphens.")
    root = args.root.resolve()
    rows = [recheck(root, case, mode, args.run_id)
            for case in SUITE["cases"] for mode in ("source", "target")]
    result = {
        "status": "controls-verified", "kind": "readiness-pilot-not-model-evaluation",
        "suiteSha256": sha256((HERE / "suite.json").read_bytes()),
        "controls": rows, "modelAttempts": 0,
        "skillEffectiveness": "not-measured", "crossPlatformCoverage": "Android arm64 only",
        "limits": [
            "Reference controls are not source-only model trials.",
            "Source/target staging was adapted and recorded; do not call it untouched upstream.",
            "Fresh paired trials require new matched stages and a verified app-only tool boundary.",
            "No causal efficiency, guaranteed activation or ship-readiness claim.",
        ],
    }
    destination = root / "results" / f"readiness-{args.run_id}.json"
    if destination.exists():
        raise FileExistsError(f"Refusing to overwrite a report: {destination}")
    save(destination, result)
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
