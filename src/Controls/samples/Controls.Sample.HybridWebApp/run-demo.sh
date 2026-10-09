#!/usr/bin/env bash
set -euo pipefail

sample_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$sample_root/../../../.." && pwd)"
no_build=false
case "${1:-}" in
  "") ;;
  --no-build) no_build=true ;;
  -h|--help)
    printf 'Usage: %s [--no-build]\nSet DEVELOPER_DIR to a compatible installed Xcode; optional HYBRIDWEBAPP_INSPECT_PORT selects an unused inspection port.\n' "$0"
    exit 0 ;;
  *) printf 'Unknown argument: %s\n' "$1" >&2; exit 2 ;;
esac
if (( $# > 1 )); then
  printf 'Only --no-build is supported.\n' >&2
  exit 2
fi
command -v python3 >/dev/null || { printf 'Python 3 is required for local process supervision.\n' >&2; exit 1; }

# Python's standard library supplies bounded HTTP/JSON probes and process-group supervision.
exec python3 - "$repo_root" "$sample_root" "$no_build" <<'PY'
import datetime
import json
import os
from pathlib import Path
import platform
import re
import secrets
import shutil
import signal
import socket
import subprocess
import sys
import threading
import time
import urllib.error
import urllib.request
import xml.etree.ElementTree as ET

ROOT, SAMPLE = map(Path, sys.argv[1:3])
NO_BUILD = sys.argv[3] == "true"
WRAPPER = ROOT / "eng/common/dotnet.sh"
PROJECT = SAMPLE / "Native/Maui.Controls.Sample.HybridWebApp.csproj"
WEB = SAMPLE / "Web"
os.umask(0o077)
session = SAMPLE / "artifacts" / ("session-" + datetime.datetime.now().strftime("%Y%m%d-%H%M%S") + "-" + secrets.token_hex(4))
session.mkdir(parents=True, mode=0o700)
scratch = session / "tooling"
scratch.mkdir(mode=0o700)
environment = os.environ.copy()
environment.update(TMPDIR=str(scratch), DOTNET_CLI_USE_MSBUILD_SERVER="0")
for name in ("HYBRIDWEBAPP_DEV_URL", "HYBRIDWEBAPP_INSPECT_TOKEN", "HYBRIDWEBAPP_INSPECT_PORT"):
    environment.pop(name, None)
token = ""
port = None
children = []
owned = {}
native = None
launch_time = None
stop_code = None
opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))


def message(text):
    print(text.replace(token, "[redacted]") if token else text, flush=True)


def interrupted(number, _frame):
    global stop_code
    stop_code = 128 + number


signal.signal(signal.SIGINT, interrupted)
signal.signal(signal.SIGTERM, interrupted)


def processes():
    output = subprocess.check_output(["ps", "-axo", "pid=,pgid=,uid=,lstart=,comm="],
                                     text=True, env=dict(environment, LC_ALL="C"))
    result = {}
    pattern = re.compile(r"^\s*(\d+)\s+(\d+)\s+(\d+)\s+(\w{3}\s+\w{3}\s+\d+\s+\d\d:\d\d:\d\d\s+\d{4})\s+(.+)$")
    for line in output.splitlines():
        match = pattern.match(line)
        if match:
            result[int(match[1])] = (int(match[2]), match[4], match[5], int(match[3]))
    return result


def track(snapshot):
    for child in children:
        group = child.pid
        members = {pid: identity for pid, identity in snapshot.items() if identity[0] == group}
        anchored = child.poll() is None or any(owned.get(pid) == identity for pid, identity in members.items())
        if anchored:
            owned.update(members)


def same_process(pid, identity, snapshot=None):
    return (snapshot if snapshot is not None else processes()).get(pid) == identity


def check_interrupt():
    if stop_code is not None:
        raise InterruptedError()


def run_checked(command, timeout=30):
    child = subprocess.Popen(command, cwd=ROOT, env=environment, stdout=subprocess.PIPE,
                             stderr=subprocess.PIPE, text=True, start_new_session=True)
    child.demo_role = "preflight"
    children.append(child)
    deadline = time.monotonic() + timeout
    while True:
        check_interrupt()
        track(processes())
        try:
            stdout, stderr = child.communicate(timeout=0.1)
            break
        except subprocess.TimeoutExpired:
            if time.monotonic() > deadline:
                raise RuntimeError("Preflight command timed out: " + command[0])
    if child.returncode:
        message(stdout + stderr)
        raise RuntimeError("Preflight command failed: " + command[0])
    return stdout.strip()


def port_available(value):
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", value))
        return listener.getsockname()[1]


def probe():
    request = urllib.request.Request(
        "http://127.0.0.1:%d/state" % port,
        headers={"Authorization": "Bearer " + token},
    )
    with opener.open(request, timeout=1) as response:
        if response.status != 200:
            return None
        body = response.read(16385)
        if len(body) > 16384:
            raise RuntimeError("Native identity response exceeds its bound.")
        return json.loads(body)


def identify(state):
    if not isinstance(state, dict) or state.get("mode") != "vite":
        raise RuntimeError("Inspection endpoint does not identify the Vite sample.")
    pid = state.get("pid")
    if not isinstance(pid, int) or pid <= 1 or not re.fullmatch(r"[0-9a-f]{32}", state.get("startupId", "")):
        raise RuntimeError("Invalid native process/startup identity.")
    started = datetime.datetime.fromisoformat(state["startedAt"].replace("Z", "+00:00")).timestamp()
    identity = processes().get(pid)
    if (started < launch_time - 1 or identity is None or identity[3] != os.getuid()
            or Path(identity[2]).resolve() != executable):
        raise RuntimeError("Native identity does not match this SDK launch and expected executable.")
    return pid, identity, state["startupId"]


def discover_for_cleanup():
    global native
    if native is not None or launch_time is None:
        return
    try:
        state = probe()
        if state:
            native = identify(state)
            return
    except (OSError, ValueError, KeyError, RuntimeError, urllib.error.URLError):
        pass
    # Before inspection starts, the per-launch token in the SDK-provided environment proves ownership.
    snapshot = processes()
    for pid, identity in snapshot.items():
        if identity[3] != os.getuid() or Path(identity[2]).resolve() != executable:
            continue
        birth = datetime.datetime.strptime(identity[1], "%a %b %d %H:%M:%S %Y").timestamp()
        if birth < launch_time - 1:
            continue
        output = subprocess.run(["ps", "eww", "-p", str(pid), "-o", "command="], capture_output=True, text=True)
        if ("HYBRIDWEBAPP_INSPECT_TOKEN=" + token) in output.stdout and same_process(pid, identity):
            native = (pid, identity, None)
            return


def terminate_owned(roles):
    snapshot = processes()
    track(snapshot)
    selected = {child.pid for child in children if child.demo_role in roles}
    groups = {identity[0] for pid, identity in owned.items() if identity[0] in selected and snapshot.get(pid) == identity}
    for group in groups:
        try:
            os.killpg(group, signal.SIGTERM)
        except ProcessLookupError:
            pass
    deadline = time.monotonic() + 6
    while time.monotonic() < deadline:
        snapshot = processes()
        track(snapshot)
        remaining = {pid: identity for pid, identity in owned.items() if identity[0] in selected and snapshot.get(pid) == identity}
        if not remaining:
            break
        time.sleep(0.1)
    for pid, identity in owned.items():
        if identity[0] in selected and same_process(pid, identity):
            try:
                os.kill(pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
    for child in children:
        if child.demo_role not in roles:
            continue
        try:
            child.wait(timeout=2)
        except subprocess.TimeoutExpired:
            pass


def cleanup():
    try:
        # Stop SDK work first so it cannot launch another app during shutdown.
        terminate_owned({"preflight", "native-sdk"})
        deadline = time.monotonic() + 3
        while native is None and launch_time is not None and time.monotonic() < deadline:
            discover_for_cleanup()
            if native is None:
                time.sleep(0.1)
        if native is not None and same_process(native[0], native[1]):
            try:
                os.kill(native[0], signal.SIGTERM)
            except ProcessLookupError:
                pass
            deadline = time.monotonic() + 5
            while same_process(native[0], native[1]) and time.monotonic() < deadline:
                time.sleep(0.1)
            if same_process(native[0], native[1]):
                try:
                    os.kill(native[0], signal.SIGKILL)
                except ProcessLookupError:
                    pass
    finally:
        terminate_owned({"vite"})
        (session / "token").unlink(missing_ok=True)
        shutil.rmtree(scratch, ignore_errors=True)
        message("Owned processes stopped; token removed. Logs: " + str(session))


def start(command, name):
    log = (session / (name + ".log")).open("w")
    child = subprocess.Popen(command, cwd=ROOT, env=environment, stdout=subprocess.PIPE,
                             stderr=subprocess.STDOUT, text=True, start_new_session=True, bufsize=1)
    child.demo_role = name
    children.append(child)
    track(processes())

    def output():
        try:
            for line in child.stdout:
                safe = line.replace(token, "[redacted]")
                log.write(safe)
                log.flush()
                print(safe, end="", flush=True)
        finally:
            log.close()

    threading.Thread(target=output, daemon=True).start()
    return child


status = 1
try:
    if platform.system() != "Darwin":
        raise RuntimeError("This DEV-1 launcher supports Mac Catalyst on macOS only.")
    for command in ("bash", "node", "npm", "xcrun", "ps", "lsof"):
        if shutil.which(command) is None:
            raise RuntimeError("Missing prerequisite: " + command)
    try:
        port_available(5173)
        requested = os.environ.get("HYBRIDWEBAPP_INSPECT_PORT")
        port = int(requested) if requested else port_available(0)
        if not 1024 <= port <= 65535 or port == 5173:
            raise ValueError()
        port_available(port)
    except (OSError, ValueError):
        raise RuntimeError("Vite port 5173 or the requested inspection port is unavailable/invalid; no listener was killed.")
    node = tuple(map(int, run_checked(["node", "--version"]).lstrip("v").split(".")))
    if not ((node[0] == 20 and node[1] >= 19) or (node[0] == 22 and node[1] >= 12) or node[0] > 22):
        raise RuntimeError("Vite requires Node 20.19+ within 20.x or Node 22.12+.")
    if not (WEB / "node_modules/vite/package.json").is_file():
        raise RuntimeError("Frontend dependencies are missing. Run npm --prefix " + str(WEB) + " ci once.")
    pinned = json.loads((ROOT / "global.json").read_text())["tools"]["dotnet"]
    sdk = run_checked(["bash", str(WRAPPER), "--version"], timeout=600).splitlines()[-1]
    if sdk != pinned:
        raise RuntimeError("Repository SDK mismatch: expected " + pinned + ", got " + sdk)
    api = ET.parse(ROOT / "Directory.Build.props").findtext(".//MacCatalystTargetFrameworkVersion")
    xcode = re.search(r"Xcode (\d+(?:\.\d+)*)", run_checked(["xcrun", "xcodebuild", "-version"]))
    if not xcode:
        raise RuntimeError("Selected Xcode could not be identified.")
    sdk_pack = ROOT / ".dotnet/packs" / ("Microsoft.MacCatalyst.Sdk.net11.0_" + api)
    if not sdk_pack.is_dir():
        raise RuntimeError("Repository-local Mac Catalyst packs are missing; follow the sample README workload setup.")
    for name in ("Microsoft.Maui.Core.props", "Microsoft.Maui.Controls.Build.Tasks.dll", "Microsoft.Maui.Resizetizer.dll"):
        if not (ROOT / ".buildtasks" / name).is_file():
            raise RuntimeError("Build repository tasks first: bash eng/common/dotnet.sh build Microsoft.Maui.BuildTasks.slnf")
    architecture = {"arm64": "arm64", "x86_64": "x64"}.get(platform.machine())
    if architecture is None:
        raise RuntimeError("Unsupported Mac architecture.")
    executable = ROOT / ("artifacts/bin/Maui.Controls.Sample.HybridWebApp/Debug/net11.0-maccatalyst/maccatalyst-" +
                         architecture + "/Maui.Controls.Sample.HybridWebApp.app/Contents/MacOS/Maui.Controls.Sample.HybridWebApp")
    if NO_BUILD and not executable.is_file():
        raise RuntimeError("--no-build requires an existing in-tree Debug build.")
    if any(Path(identity[2]).resolve() == executable for identity in processes().values()):
        raise RuntimeError("The sample is already running; close it before launching a new owned session.")
    token = secrets.token_urlsafe(32)
    (session / "token").write_text(token)
    message("Repository SDK " + sdk + "; Xcode " + xcode[1] + ". Session: " + str(session))
    vite = start(["npm", "--prefix", str(WEB), "run", "dev"], "vite")
    deadline = time.monotonic() + 30
    while True:
        check_interrupt()
        track(processes())
        if vite.poll() is not None:
            raise RuntimeError("Owned Vite process exited during startup.")
        try:
            with opener.open("http://127.0.0.1:5173/", timeout=1) as response:
                page = response.read(65537)
                if response.status == 200 and b"/@vite/client" in page and b'id="html-message"' in page:
                    listeners = subprocess.run(["lsof", "-nP", "-iTCP:5173", "-sTCP:LISTEN", "-t"],
                                               capture_output=True, text=True, timeout=3)
                    snapshot = processes()
                    if any(snapshot.get(int(pid), (None,))[0] == vite.pid for pid in listeners.stdout.split()):
                        break
        except (OSError, urllib.error.URLError):
            pass
        if time.monotonic() > deadline:
            raise RuntimeError("Expected Vite page did not become ready within 30 seconds.")
        time.sleep(0.1)
    command = ["bash", str(WRAPPER), "run", "--project", str(PROJECT), "-c", "Debug", "-f", "net11.0-maccatalyst",
               "-p:IncludeAndroidTargetFrameworks=false", "-p:IncludeIosTargetFrameworks=false",
               "-p:IncludeMacOSTargetFrameworks=false", "-p:UseWorkload=false", "--no-launch-profile",
               "--disable-build-servers", "-v", "minimal",
               "-e", "HYBRIDWEBAPP_DEV_URL=http://127.0.0.1:5173/",
               "-e", "HYBRIDWEBAPP_INSPECT_PORT=" + str(port),
               "-e", "HYBRIDWEBAPP_INSPECT_TOKEN=" + token]
    if NO_BUILD:
        command.append("--no-build")
    launch_time = time.time()
    runner = start(command, "native-sdk")
    deadline = time.monotonic() + 600
    runner_finished = False
    while native is None:
        check_interrupt()
        track(processes())
        if vite.poll() is not None:
            raise RuntimeError("Owned Vite server exited before native startup.")
        if runner.poll() not in (None, 0):
            status = runner.returncode if 0 < runner.returncode < 126 else 1
            raise RuntimeError("Native SDK build/launch failed.")
        if runner.poll() == 0 and not runner_finished:
            runner_finished = True
            deadline = min(deadline, time.monotonic() + 30)
        try:
            state = probe()
            if state:
                candidate = identify(state)
                if state.get("uri") == "app://0.0.0.1/":
                    native = candidate
        except (OSError, ValueError, KeyError, urllib.error.URLError):
            pass
        if time.monotonic() > deadline:
            raise RuntimeError("Authenticated native startup timed out (ten-minute build bound, thirty seconds after SDK return).")
        time.sleep(0.1)
    state.update(inspectionPort=port, executable=str(executable), launcherPid=os.getpid(),
                 vitePid=vite.pid, sdkPid=runner.pid)
    (session / "state.json").write_text(json.dumps(state, indent=2))
    message("Ready: native PID %d, startup %s, inspection http://127.0.0.1:%d. Ctrl+C stops owned processes." %
            (native[0], native[2], port))
    next_vite_check = 0
    vite_failures = 0
    while same_process(native[0], native[1]):
        check_interrupt()
        track(processes())
        if vite.poll() is not None:
            raise RuntimeError("Owned Vite server exited while the native app was running.")
        if runner.poll() not in (None, 0):
            raise RuntimeError("Native SDK runner failed after startup.")
        if time.monotonic() >= next_vite_check:
            next_vite_check = time.monotonic() + 2
            try:
                with opener.open("http://127.0.0.1:5173/", timeout=1) as response:
                    if response.status != 200 or b"/@vite/client" not in response.read(65537):
                        raise OSError("Unexpected Vite response.")
                vite_failures = 0
            except (OSError, urllib.error.URLError):
                vite_failures += 1
                if vite_failures == 3:
                    raise RuntimeError("Owned Vite listener stopped serving the expected page.")
        time.sleep(0.2)
    status = 0
    message("Native app exited normally.")
except InterruptedError:
    status = stop_code
    message("Stopping after signal.")
except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
    message("ERROR: " + str(error))
finally:
    cleanup()
sys.exit(status)
PY
