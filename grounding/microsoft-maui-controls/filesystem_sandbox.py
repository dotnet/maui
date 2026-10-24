"""macOS filesystem preflight; not an agent, network or credential broker sandbox."""

import argparse
import json
import os
from pathlib import Path
import signal
import subprocess
import sys


def within(path, root):
    def aliases(value):
        data = Path("/System/Volumes/Data") / str(value).lstrip("/")
        if sys.platform == "darwin" and data.exists() and value.exists() and data.samefile(value):
            return value, data
        return (value,)

    return any(child.is_relative_to(parent) for child in aliases(path) for parent in aliases(root))


def policy(app, private, toolchains):
    if app.is_symlink() or private.is_symlink():
        raise ValueError("App and private roots cannot be symlinks.")
    app = app.resolve(strict=True)
    private = private.resolve(strict=True)
    if app.name != "app" or not app.is_dir() or not private.is_dir():
        raise ValueError("An existing staged app/ and private directory are required.")
    if within(app, private) or within(private, app):
        raise ValueError("Private state must be separate from the app and its ancestors.")
    if within(Path.home().resolve(), private):
        raise ValueError("Private state cannot contain the user's home directory.")
    configurations = [app.parent / name for name in (
        "Directory.Build.props", "Directory.Build.targets", "NuGet.config",
    )]
    if any(path.is_symlink() or not path.is_file() for path in configurations):
        raise ValueError("Three real, read-only sibling configuration files are required.")
    readonly = [Path(path).resolve(strict=True) for path in toolchains]
    for root in readonly:
        if (within(app, root) or within(private, root)
                or within(root, private) or within(Path.home().resolve(), root)):
            raise ValueError("Toolchains must be read-only and cannot expose the user's home directory.")
    system = [Path(path) for path in (
        "/System/Library", "/System/Cryptexes",
        "/usr/bin", "/usr/lib", "/usr/libexec", "/usr/share", "/bin", "/sbin", "/dev/fd",
        "/private/var/db/dyld", "/private/preboot/Cryptexes/OS",
        "/System/Volumes/Preboot/Cryptexes/OS",
    )]
    system_files = [Path(path).resolve() for path in (
        "/etc/hosts", "/etc/resolv.conf", "/etc/localtime", "/etc/passwd", "/etc/group",
        "/dev/null", "/dev/random", "/dev/urandom",
        "/Library/Apple/usr/libexec/oah/libRosettaRuntime",
    )]
    readonly = [*system, *readonly]
    if any(within(root, private) for root in system):
        raise ValueError("Private state cannot contain system toolchains.")

    def filters(kind, paths):
        return " ".join(f"({kind} {json.dumps(str(path))})" for path in paths)

    writable = [app, private]
    ancestors = sorted({parent for path in [*writable, *readonly, *configurations, *system_files]
                        for parent in path.parents})
    return "\n".join([
        "(version 1)",
        "(allow default)",
        "(deny file-read*)",
        "(deny file-write*)",
        f"(allow file-read* {filters('subpath', [*writable, *readonly])})",
        f"(allow file-read* {filters('literal', [Path('/'), *configurations, *system_files])})",
        f"(allow file-read-metadata {filters('literal', ancestors)})",
        f"(allow file-read-metadata {filters('literal', [Path('/tmp'), Path('/var'), Path('/etc'), Path('/System/Volumes/Data'), Path.home()])})",
        f"(allow file-write* {filters('subpath', writable)})",
        '(allow file-write* (literal "/dev/null"))',
        '(allow file-read* file-write* (require-all (regex #"^/private/tmp/MSBuild[0-9]+$") '
        '(vnode-type SOCKET)))',
    ]) + "\n"


def launch(text, command):
    if sys.platform != "darwin":
        raise RuntimeError("This filesystem preflight requires macOS.")
    if os.getsid(0) != os.getpid():
        os.setsid()
    session = os.getsid(0)
    directories = []
    try:
        for kind in ("shm", "lockfiles"):
            parent = Path(f"/tmp/.dotnet/{kind}").resolve(strict=True)
            directory = parent / f"session{session}"
            directory.mkdir()
            directories.append(directory)
            text += f"(allow file-read* file-write* (subpath {json.dumps(str(directory))}))\n"
            for ancestor in (parent, parent.parent, parent.parent.parent):
                text += f"(allow file-read* (literal {json.dumps(str(ancestor))}))\n"
        with subprocess.Popen(["/usr/bin/sandbox-exec", "-p", text, *command]) as process:
            code = process.wait()
            if code:
                print(f"Filesystem preflight child PID {process.pid} exited {code}.", file=sys.stderr)
            return code
    finally:
        for directory in directories:
            if directory.exists() and not any(directory.iterdir()):
                directory.rmdir()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--app", type=Path, required=True)
    parser.add_argument("--private", type=Path, required=True)
    parser.add_argument("--read-root", type=Path, action="append", default=[])
    parser.add_argument("command", nargs=argparse.REMAINDER)
    args = parser.parse_args()
    command = args.command
    if command and command[0] == "--":
        command = command[1:]
    if not command:
        parser.error("A command after -- is required.")
    if sys.platform == "darwin" and os.getpgrp() == os.getpid() and os.getsid(0) != os.getpid():
        with subprocess.Popen(
            [sys.executable, str(Path(__file__).resolve()), *sys.argv[1:]],
            start_new_session=True,
        ) as process:
            try:
                code = process.wait()
            except KeyboardInterrupt:
                os.killpg(process.pid, signal.SIGINT)
                process.wait()
                code = 130
        sys.exit(code)
    text = policy(args.app, args.private, args.read_root)
    os.chdir(args.app.resolve(strict=True))
    sys.exit(launch(text, command))


if __name__ == "__main__":
    main()
