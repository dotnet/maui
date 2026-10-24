"""Build and exercise evaluator-owned controls, not model comparisons."""

import argparse
import json
import os
from pathlib import Path
import re
import signal
import subprocess
import time
import xml.etree.ElementTree as ET

from stage import HERE, PACKAGE, SDK, contained, files, save, sha256


class ControlFailure(RuntimeError):
    pass


class InfrastructureFailure(ControlFailure):
    pass


def validate_runtime_contract(case):
    steps = case["runtime"]
    if not steps or not all(step.get("assert") for step in steps):
        raise ControlFailure("Every runtime checkpoint must have acceptance checks.")
    names = [step["name"] for step in steps]
    if len(set(names)) != len(names) or not all(re.fullmatch(r"[a-z0-9-]+", name) for name in names):
        raise ControlFailure("Runtime checkpoint names must be unique, safe path components.")
    if not all(any(assertion.get("count", 1) > 0 for assertion in step["assert"]) for step in steps):
        raise ControlFailure("Every checkpoint needs a positive app observation.")


def run(command, cwd, env, output, rows, timeout=600):
    name = f"{len(rows):02}"
    row = {"command": [str(arg) for arg in command], "cwd": str(cwd), "status": "running"}
    rows.append(row)
    save(output / "commands.json", rows)
    started = time.monotonic()
    with (output / f"{name}.log").open("w") as log, (output / f"{name}.stderr").open("w") as stderr:
        try:
            process = subprocess.Popen(row["command"], cwd=cwd, env=env, stdout=log,
                                       stderr=stderr, start_new_session=os.name == "posix")
            exit_code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired as error:
            if os.name == "posix":
                os.killpg(process.pid, signal.SIGKILL)
            else:
                process.kill()
            process.wait()
            row.update(status="timeout", elapsedSeconds=time.monotonic() - started)
            save(output / "commands.json", rows)
            raise InfrastructureFailure(f"Timed out: {command}") from error
    row.update(status="completed", exitCode=exit_code,
               elapsedSeconds=time.monotonic() - started)
    save(output / "commands.json", rows)
    text = (output / f"{name}.log").read_text(errors="replace")
    if exit_code:
        raise ControlFailure(f"Command failed ({exit_code}): {command}; see {name}.log and {name}.stderr")
    return text


def build(consumer, output, root, rows, probe_targets=None):
    stage = json.loads((consumer / "stage.json").read_text())
    mode = stage["mode"]
    before = files(consumer)
    before.pop("stage.json", None)
    if before != stage["stagedFiles"]:
        raise ControlFailure("Staged control inputs drifted before evaluation.")
    app = consumer / "app"
    env = os.environ.copy()
    for key, path in {
        "NUGET_PACKAGES": root / "cache/packages",
        "NUGET_HTTP_CACHE_PATH": root / "cache/http",
        "NUGET_PLUGINS_CACHE_PATH": root / "cache/plugins",
        "DOTNET_CLI_HOME": root / "cache/dotnet-home",
        "XDG_DATA_HOME": root / "cache/user-data",
        "TMPDIR": root / "tmp",
        "AppSettingsDirectory": root / "cache/android-signing",
    }.items():
        path.mkdir(parents=True, exist_ok=True)
        env[key] = str(path)
    env.update(
        RestoreConfigFile=str(consumer / "NuGet.config"),
        DOTNET_CLI_USE_MSBUILD_SERVER="0", MSBUILDDISABLENODEREUSE="1",
        UseSharedCompilation="false", DOTNET_CLI_TELEMETRY_OPTOUT="1",
        DOTNET_GENERATE_ASPNET_CERTIFICATE="false",
        JAVA_TOOL_OPTIONS=f"-Djava.io.tmpdir={env['TMPDIR']}",
    )
    if probe_targets is not None:
        env["CustomBeforeMicrosoftCommonTargets"] = str(probe_targets)
    project = stage["project"]
    sdk = run(["dotnet", "--version"], app, env, output, rows)
    if sdk.strip() != SDK[mode]:
        raise ControlFailure(f"Wrong SDK: {sdk}")
    properties = None
    for configuration in ("Debug", "Release"):
        run(["dotnet", "restore", project, "-p:RuntimeIdentifier=android-arm64", "--configfile", "../NuGet.config",
             f"-p:Configuration={configuration}"], app, env, output, rows)
        names = ("TargetFramework,TargetFrameworks,TargetFrameworkVersion,"
                 "TargetPlatformIdentifier,MauiVersion,TargetDir,ApplicationId,ProjectAssetsFile")
        text = run(["dotnet", "msbuild", project, "-p:RuntimeIdentifier=android-arm64", f"-p:Configuration={configuration}",
                    f"-getProperty:{names}", "-nologo"], app, env, output, rows)
        start = text.find("{")
        properties = json.loads(text[start:])["Properties"]
        expected_framework = "v10.0" if mode == "source" else "v11.0"
        if (properties["TargetFrameworkVersion"] != expected_framework
                or properties["TargetPlatformIdentifier"].casefold() != "android"
                or properties["TargetFrameworks"]
                or properties["MauiVersion"] != PACKAGE[mode]):
            raise ControlFailure(f"Wrong evaluated target: {properties}")
        assets_path = Path(properties["ProjectAssetsFile"]).resolve()
        assets_path.relative_to(app.resolve())
        assets = json.loads(assets_path.read_text())
        for package in ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Core", "Microsoft.Maui.Core"):
            versions = [key.split("/", 1)[1] for key in assets["libraries"]
                        if key.split("/", 1)[0] == package]
            if versions != [PACKAGE[mode]]:
                raise ControlFailure(f"Wrong restored {package}: {versions}")
        save(output / f"{configuration}-properties.json", properties)
        save(output / f"{configuration}-assets.json", assets)
        run(["dotnet", "build", project, "-p:RuntimeIdentifier=android-arm64", "-c", configuration, "--no-restore",
             "--nologo", "-v", "minimal"], app, env, output, rows)
    debug = json.loads((output / "Debug-properties.json").read_text())
    target = Path(debug["TargetDir"])
    if not target.is_absolute():
        raise ControlFailure("TargetDir must be absolute.")
    target.resolve().relative_to(app.resolve())
    apk = (target / f"{debug['ApplicationId']}-Signed.apk").resolve()
    apk.relative_to(app.resolve())
    if not apk.is_file():
        raise ControlFailure(f"Signed APK is missing: {apk}")
    after = files(consumer)
    after.pop("stage.json", None)
    if after != before:
        raise ControlFailure("Build changed a control input outside bin/obj.")
    save(output / "build.json", {
        "status": "build-pass-runtime-pending", "mode": mode, "apk": str(apk),
        "applicationId": debug["ApplicationId"], "inputHashes": before,
        "apkSha256": sha256(apk.read_bytes()),
        "agentAttempt": False,
    })
    return apk, debug["ApplicationId"], env


def matching_nodes(tree, selector):
    return [node for node in tree.iter("node")
            if all(node.get("text", "").startswith(value) if key == "$textPrefix"
                   else node.get(key) == value for key, value in selector.items())]


def tap_target(tree, step):
    nodes = matching_nodes(tree, step["tap"])
    count = step.get("tapCount", 1)
    index = step.get("tapIndex", 0)
    if len(nodes) != count or not 0 <= index < count:
        raise ControlFailure(f"Tap count/index does not match the declared target: {step['tap']}")
    return nodes[index]


def check_ui(tree, assertions):
    for assertion in assertions:
        matches = matching_nodes(tree, assertion["selector"])
        count = assertion.get("count", 1)
        if len(matches) != count:
            raise ControlFailure(f"Expected {count} matching nodes, got {len(matches)}: {assertion}")
        for node in matches:
            if "number" in assertion:
                number = assertion["number"]
                value = float(node.get("text", "").removeprefix(number["prefix"]))
                if not number["minimum"] <= value <= number["maximum"]:
                    raise ControlFailure(f"Value outside expected range: {value}")
            for name, expected in assertion.get("attributes", {}).items():
                if node.get(name) != expected:
                    raise ControlFailure(f"Wrong {name}: {node.attrib}")


def check_anr(tree):
    messages = [node.get("text", "") for node in tree.iter("node")]
    for message in messages:
        if "isn't responding" in message or "is not responding" in message:
            if message.startswith("System UI "):
                raise InfrastructureFailure(f"System UI ANR: {message}")
            raise ControlFailure(f"Application ANR: {message}")


def runtime(case, apk, application_id, serial, app, env, output, rows, mode):
    validate_runtime_contract(case)
    if application_id != case["applicationId"]:
        raise ControlFailure("The delivered application ID differs from the source case.")
    adb = Path(env["ANDROID_HOME"]) / "platform-tools/adb"
    analyzer = Path(env["ANDROID_HOME"]) / "cmdline-tools/latest/bin/apkanalyzer"
    build_record = json.loads((output / "build.json").read_text())
    if sha256(apk.read_bytes()) != build_record["apkSha256"]:
        raise ControlFailure("APK drifted after the recorded build.")
    manifest_id = run([analyzer, "manifest", "application-id", apk], app, env, output, rows)
    if manifest_id.strip() != application_id:
        raise ControlFailure("Packaged application ID does not match evaluation.")
    minimum = run([analyzer, "manifest", "min-sdk", apk], app, env, output, rows)
    if int(minimum.strip()) != 24:
        raise ControlFailure("Packaged Android minimum differs from the staged API 24 contract.")

    def device(*args):
        return run([adb, "-s", serial, *args], app, env, output, rows, timeout=120)

    try:
        state = device("get-state").strip()
    except ControlFailure as error:
        raise InfrastructureFailure("Requested evaluator-owned device is unavailable.") from error
    if state != "device":
        raise InfrastructureFailure("Requested evaluator-owned device is unavailable.")
    installed = device("shell", "pm", "list", "packages", application_id).splitlines()
    if f"package:{application_id}" in installed:
        if device("uninstall", application_id).strip() != "Success":
            raise InfrastructureFailure("Could not remove the previous evaluator-owned sample installation.")
    device("install", "-r", apk)
    device("shell", "am", "force-stop", application_id)
    device("shell", "pm", "clear", application_id)
    component = device("shell", "cmd", "package", "resolve-activity", "--brief",
                       "-a", "android.intent.action.MAIN",
                       "-c", "android.intent.category.LAUNCHER", application_id).strip().splitlines()[-1]
    if not component.startswith(application_id + "/"):
        raise ControlFailure(f"Unexpected launch activity: {component}")

    def launch_activity():
        text = device("shell", "am", "start", "-W", "-n", component)
        if "Status: ok" not in text:
            raise ControlFailure(f"Activity did not start successfully: {text}")
        return text

    launch = launch_activity()
    (output / "launch.txt").write_text(launch)
    ui_root = output / "runtime"
    ui_root.mkdir()

    def snapshot(name):
        # Polling observes readiness; it does not alter the app or dismiss errors.
        device("shell", "uiautomator", "dump", "/sdcard/maui-skills-eval.xml")
        xml = device("shell", "cat", "/sdcard/maui-skills-eval.xml")
        (ui_root / f"{name}.xml").write_text(xml)
        tree = ET.fromstring(xml)
        check_anr(tree)
        app_tree = ET.Element("hierarchy")
        for node in tree.iter("node"):
            if node.get("package") == application_id:
                app_tree.append(ET.Element("node", node.attrib))
        if not len(app_tree):
            raise ControlFailure("The foreground UI does not belong to the evaluated app.")
        return app_tree

    for index, step in enumerate(case["runtime"]):
        if "scrollTo" in step:
            for attempt in range(12):
                tree = snapshot(f"{index:02}-scroll-{attempt:02}")
                if matching_nodes(tree, step["scrollTo"]):
                    break
                scrollers = matching_nodes(tree, {"scrollable": "true"})
                if not scrollers:
                    raise ControlFailure("No accessible scrollable area.")
                coordinates = re.fullmatch(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]",
                                           scrollers[0].get("bounds", ""))
                if not coordinates:
                    raise ControlFailure("Invalid scroll bounds.")
                x1, y1, x2, y2 = map(int, coordinates.groups())
                device("shell", "input", "swipe", str((x1 + x2) // 2),
                       str(y1 + (y2 - y1) * 4 // 5), str((x1 + x2) // 2),
                       str(y1 + (y2 - y1) // 5), "400")
            else:
                raise ControlFailure(f"Scrollable target did not appear: {step['scrollTo']}")
        deadline = time.monotonic() + 45
        while True:
            tree = snapshot(f"{index:02}-{step['name']}")
            try:
                check_ui(tree, step.get("assert", []))
                break
            except ControlFailure:
                if time.monotonic() >= deadline:
                    raise
                time.sleep(1)
        if "tap" in step:
            bounds = tap_target(tree, step).get("bounds")
            coordinates = re.fullmatch(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", bounds or "")
            if not coordinates:
                raise ControlFailure(f"Invalid bounds: {bounds}")
            x1, y1, x2, y2 = map(int, coordinates.groups())
            device("shell", "input", "tap", str((x1 + x2) // 2), str((y1 + y2) // 2))
        if "text" in step:
            device("shell", "input", "text", step["text"])
        if "drag" in step:
            drag = step["drag"]
            nodes = matching_nodes(tree, drag["selector"])
            if len(nodes) != drag["count"]:
                raise ControlFailure(f"Wrong number of drag targets: {drag}")
            node = nodes[drag["index"]]
            coordinates = re.fullmatch(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]",
                                       node.get("bounds", ""))
            if not coordinates:
                raise ControlFailure("Invalid drag bounds.")
            x1, y1, x2, y2 = map(int, coordinates.groups())
            device("shell", "input", "swipe", str(x1 + (x2 - x1) // 10),
                   str((y1 + y2) // 2), str(x1 + int((x2 - x1) * drag["fraction"])),
                   str((y1 + y2) // 2), "400")
        if step.get("restart"):
            device("shell", "am", "force-stop", application_id)
            launch_activity()
        if step.get("back"):
            device("shell", "input", "keyevent", "4")
    pid = device("shell", "pidof", application_id).strip()
    if not pid:
        raise ControlFailure("Application process is not alive after navigation.")
    (output / "runtime.log").write_text(device("logcat", "-d", f"--pid={pid}"))
    native = device("shell", "dumpsys", "activity", "top")
    (ui_root / "native.txt").write_text(native)
    sections = re.split(r"(?=^  ACTIVITY )", native, flags=re.MULTILINE)
    active = [section for section in sections
              if section.startswith(f"  ACTIVITY {application_id}/") and "mResumed=true" in section]
    if len(active) != 1:
        raise ControlFailure("Cannot identify one resumed activity for the app.")
    expected = case.get("expectedControlNative", {}).get(mode)
    if expected and not re.search(r"\." + re.escape(expected) + r"\{", active[0]):
        raise ControlFailure(f"Default reference-control native view not observed: {expected}")
    save(ui_root / "verdict.json", {
        "status": "pass", "serial": serial, "checkpoints": [step["name"] for step in case["runtime"]],
        "applicationId": application_id, "apk": str(apk),
        "apkSha256": build_record["apkSha256"], "agentAttempt": False,
        "nativeControlClass": expected,
    })


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--case", required=True)
    parser.add_argument("--mode", choices=SDK, required=True)
    parser.add_argument("--run-id", default="01", help="A fresh evidence suffix; existing runs are never overwritten.")
    parser.add_argument("--serial", help="An explicitly owned emulator; omission is build-only, never full pass.")
    args = parser.parse_args()
    if not re.fullmatch(r"[a-zA-Z0-9-]+", args.run_id):
        parser.error("--run-id must contain only letters, digits or hyphens.")
    root = args.root.resolve()
    suite = json.loads((HERE / "suite.json").read_text())
    case = next(case for case in suite["cases"] if case["id"] == args.case)
    consumer = contained(root, f"consumers/{args.case}-{args.mode}")
    output = root / "results" / f"{args.case}-{args.mode}-{args.run_id}"
    output.mkdir(parents=True, exist_ok=False)
    save(output / "harness.json", {
        "suiteSha256": sha256((HERE / "suite.json").read_bytes()),
        "files": {name: sha256((HERE / name).read_bytes())
                  for name in ("stage.py", "controls.py")},
        "runtimeIdentifier": "android-arm64",
    })
    rows = []
    try:
        apk, application_id, env = build(consumer, output, root, rows)
        status = "build-pass-runtime-pending"
        if args.serial:
            runtime(case, apk, application_id, args.serial, consumer / "app", env, output, rows, args.mode)
            status = "control-pass"
        save(output / "verdict.json", {
            "status": status, "agentAttempt": False, "mode": args.mode, "case": args.case,
        })
    except (ControlFailure, ValueError, KeyError, OSError) as error:
        save(output / "verdict.json", {
            "status": "control-infrastructure" if isinstance(error, InfrastructureFailure) else "control-failed",
            "agentAttempt": False, "error": str(error),
        })
        raise
    print(json.dumps({"case": args.case, "mode": args.mode, "status": status}))


if __name__ == "__main__":
    main()
