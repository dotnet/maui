import os
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

from filesystem_sandbox import offline_environment, policy


class FilesystemSandboxTests(unittest.TestCase):
    @unittest.skipUnless(sys.platform == "darwin", "Requires the actual macOS sandbox.")
    def test_offline_cli_sanitizes_the_child_and_propagates_its_failure(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp).resolve()
            app, private, toolchain = root / "app", root / "private", root / "toolchain"
            for path in (app, private, toolchain):
                path.mkdir()
            for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (root / name).write_text("configuration")
            result = subprocess.run(
                [sys.executable, str(Path(__file__).with_name("filesystem_sandbox.py")),
                 "--app", str(app), "--private", str(private), "--offline",
                 "--dotnet-root", str(toolchain), "--android-sdk", str(toolchain),
                 "--java-home", str(toolchain), "--", "/bin/sh", "-c",
                 'test -z "$BOUNDARY_PROVIDER_TOKEN" || exit 10; '
                 'test "$HOME" = "$1/home" || exit 11; '
                 'test "$DOTNET_EnableDiagnostics" = 0 || exit 12; '
                 'printf "sanitized-child\\n"; exit 7', "sh", str(private)],
                env={**os.environ, "BOUNDARY_PROVIDER_TOKEN": "nonsecret-test-value"},
                cwd=app, capture_output=True, text=True, timeout=30,
            )
            self.assertEqual(7, result.returncode, result.stdout + result.stderr)
            self.assertEqual("sanitized-child\n", result.stdout)
            self.assertIn("exited 7", result.stderr)

    def test_offline_environment_drops_inherited_secrets_and_redirects_all_build_state(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            private = root / "private"
            private.mkdir()
            with patch.dict(os.environ, {"GITHUB_TOKEN": "not-a-real-token", "SSH_AUTH_SOCK": "/not-a-socket"}):
                environment = offline_environment(root / "app", private, root, root, root)
            self.assertNotIn("GITHUB_TOKEN", environment)
            self.assertNotIn("SSH_AUTH_SOCK", environment)
            self.assertEqual("0", environment["DOTNET_EnableDiagnostics"])
            for key in ("HOME", "DOTNET_CLI_HOME", "XDG_DATA_HOME", "XDG_CACHE_HOME",
                        "TMPDIR", "NUGET_PACKAGES", "AppSettingsDirectory"):
                self.assertTrue(Path(environment[key]).is_relative_to(private.resolve()))
            (private / "home").rmdir()
            (private / "home").symlink_to(root, target_is_directory=True)
            with self.assertRaises(ValueError):
                offline_environment(root / "app", private, root, root, root)

    @unittest.skipUnless(sys.platform == "darwin", "Requires the actual macOS sandbox.")
    def test_offline_blocks_network_credential_service_and_unrelated_process_access(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp).resolve()
            app, private = root / "app", root / "private"
            app.mkdir()
            private.mkdir()
            for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (root / name).write_text("configuration")
            executable = Path(sys.executable).resolve()
            python_roots = [Path(sys.base_prefix), executable,
                            *[parent for parent in executable.parents if parent.suffix == ".app"]]
            text = policy(app, private, python_roots, offline=True)
            self.assertIn("(deny default)", text)
            self.assertNotIn("MSBuild[0-9]", text)
            with socket.socket() as server:
                server.bind(("127.0.0.1", 0))
                server.listen()
                port = server.getsockname()[1]
                with socket.create_connection(("127.0.0.1", port), timeout=5):
                    pass
                process = subprocess.Popen(
                    [sys.executable, "-c", "import time; print('ready', flush=True); time.sleep(60)",
                     "public-probe-canary"],
                    env={"PATH": "/usr/bin:/bin"},
                    stdout=subprocess.PIPE, text=True,
                )
                try:
                    self.assertEqual("ready\n", process.stdout.readline())
                    script = """
import ctypes, os, socket, sys
pid, port, sandboxed = map(int, sys.argv[1:])
lib = ctypes.CDLL('/usr/lib/libSystem.B.dylib', use_errno=True)
bootstrap = ctypes.c_uint.in_dll(lib, 'bootstrap_port').value
target = ctypes.c_uint()
code = lib.bootstrap_look_up(bootstrap, b'com.apple.SecurityServer', ctypes.byref(target))
assert (code != 0) == bool(sandboxed)
if not sandboxed:
    lib.mach_port_deallocate(lib.mach_task_self(), target)
mib = (ctypes.c_int * 3)(1, 49, pid)
size = ctypes.c_size_t(262144)
buffer = ctypes.create_string_buffer(size.value)
code = lib.sysctl(mib, 3, buffer, ctypes.byref(size), None, 0)
assert (code != 0) == bool(sandboxed)
if not sandboxed:
    assert b'public-probe-canary' in buffer.raw[:size.value]
try:
    os.kill(pid, 0)
except PermissionError:
    assert sandboxed
else:
    assert not sandboxed
if sandboxed:
    with socket.socket() as connection:
        try:
            connection.connect(('127.0.0.1', port))
        except PermissionError:
            pass
        else:
            raise AssertionError('Network connection was allowed')
"""
                    for sandboxed in (False, True):
                        prefix = ["/usr/bin/sandbox-exec", "-p", text] if sandboxed else []
                        result = subprocess.run(
                            [*prefix, sys.executable, "-c", script,
                             str(process.pid), str(port), str(int(sandboxed))],
                            cwd=app, env={"PATH": "/usr/bin:/bin", "PYTHONDONTWRITEBYTECODE": "1"},
                            capture_output=True, text=True, timeout=30,
                        )
                        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
                finally:
                    process.terminate()
                    process.wait()
                    process.stdout.close()

    @unittest.skipUnless(sys.platform == "darwin", "Requires the actual macOS sandbox.")
    def test_cli_can_own_a_session_when_started_as_a_terminal_process_group_leader(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            app, private = root / "app", root / "private"
            app.mkdir()
            private.mkdir()
            for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (root / name).write_text("configuration")
            result = subprocess.run(
                [sys.executable, str(Path(__file__).with_name("filesystem_sandbox.py")),
                 "--app", str(app), "--private", str(private), "--", "/bin/echo", "session-pass"],
                preexec_fn=os.setpgrp, capture_output=True, text=True, timeout=30,
            )
            self.assertEqual(0, result.returncode, result.stdout + result.stderr)
            self.assertEqual("session-pass\n", result.stdout)

    def test_private_state_cannot_contain_the_app_or_its_sibling_configuration(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            app = root / "app"
            app.mkdir()
            with self.assertRaises(ValueError):
                policy(app, root, [])

    def test_toolchains_cannot_grant_home_reads_or_private_writes(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            app, private = root / "app", root / "private"
            app.mkdir()
            private.mkdir()
            for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (root / name).write_text("configuration")
            readonly_roots = [Path.home(), private]
            alias = Path("/System/Volumes/Data") / str(Path.home()).lstrip("/")
            if alias.is_dir() and alias.samefile(Path.home()):
                readonly_roots.extend([alias, alias.parent, Path("/System/Volumes/Data")])
                with self.assertRaises(ValueError):
                    policy(app, Path("/System/Volumes/Data"), [])
            for readonly in readonly_roots:
                with self.subTest(readonly=readonly), self.assertRaises(ValueError):
                    policy(app, private, [readonly])

    @unittest.skipUnless(sys.platform == "darwin", "Requires the actual macOS sandbox.")
    def test_real_reads_writes_descendants_and_symlinks_respect_the_boundary(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp).resolve()
            app, private, teacher, dependency = [root / name for name in (
                "app", "private", "teacher", "public-dependency",
            )]
            for path in (app, private, teacher, dependency):
                path.mkdir()
            for name in ("Directory.Build.props", "Directory.Build.targets", "NuGet.config"):
                (root / name).write_text("read-only configuration")
            canary = teacher / "answer.txt"
            canary.write_text("not an actor input")
            package = dependency / "package.txt"
            package.write_text("public dependency")
            escape = app / "escape"
            escape.symlink_to(teacher, target_is_directory=True)
            executable = Path(sys.executable).resolve()
            python_roots = [Path(sys.base_prefix), executable,
                            *[parent for parent in executable.parents if parent.suffix == ".app"]]
            protected = Path(__file__).resolve()
            alias = Path("/System/Volumes/Data") / str(protected).lstrip("/")
            protected_paths = [protected]
            if alias.is_file():
                self.assertTrue(alias.samefile(protected))
                protected_paths.append(alias)
            script = """
import os, pathlib, socket, subprocess, sys
app, private, canary, package = map(pathlib.Path, sys.argv[1:5])
assert package.read_text() == 'public dependency'
for name in ('Directory.Build.props', 'Directory.Build.targets', 'NuGet.config'):
    assert (app.parent / name).read_text() == 'read-only configuration'
for path in (app / 'source.txt', private / 'cache.txt'):
    path.write_text('permitted')
    assert path.read_text() == 'permitted'
for path in (canary, app / 'escape/answer.txt', *map(pathlib.Path, sys.argv[5:])):
    try:
        path.read_text()
    except PermissionError:
        pass
    else:
        raise AssertionError(f'Protected read succeeded: {path}')
for path in (canary, package, app.parent / 'Directory.Build.props'):
    try:
        path.write_text('forbidden')
    except PermissionError:
        pass
    else:
        raise AssertionError(f'Protected write succeeded: {path}')
child = subprocess.run(['/bin/sh', '-c', 'cat \"$1\"', 'sh', str(canary)],
                       capture_output=True, text=True, start_new_session=True)
assert child.returncode != 0, child
endpoint = pathlib.Path(f'/tmp/MSBuild{os.getpid()}')
with socket.socket(socket.AF_UNIX) as server:
    if os.environ['PREFLIGHT_OFFLINE'] == '1':
        try:
            server.bind(str(endpoint))
        except PermissionError:
            pass
        else:
            raise AssertionError('Offline build accessed a shared MSBuild endpoint')
    else:
        server.bind(str(endpoint))
if endpoint.exists():
    endpoint.unlink()
ordinary = pathlib.Path(f'/tmp/MSBuild{1000000000 + os.getpid()}')
try:
    ordinary.write_text('forbidden')
except PermissionError:
    pass
else:
    raise AssertionError('The SDK socket exception allowed an ordinary file')
"""
            for offline in (False, True):
                with self.subTest(offline=offline):
                    text = policy(app, private, [dependency, *python_roots], offline=offline)
                    result = subprocess.run(
                        ["/usr/bin/sandbox-exec", "-p", text, sys.executable, "-c", script,
                         str(app), str(private), str(canary), str(package), *map(str, protected_paths)],
                        env={**os.environ, "PYTHONDONTWRITEBYTECODE": "1",
                             "PREFLIGHT_OFFLINE": str(int(offline))},
                        cwd=app, capture_output=True, text=True, timeout=30,
                    )
                    self.assertEqual(0, result.returncode, result.stdout + result.stderr)
            self.assertEqual("not an actor input", canary.read_text())
            self.assertEqual("public dependency", package.read_text())


if __name__ == "__main__":
    unittest.main()
