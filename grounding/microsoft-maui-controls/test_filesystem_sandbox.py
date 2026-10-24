import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

from filesystem_sandbox import policy


class FilesystemSandboxTests(unittest.TestCase):
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
            text = policy(app, private, [dependency, *python_roots])
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
    server.bind(str(endpoint))
endpoint.unlink()
ordinary = pathlib.Path(f'/tmp/MSBuild{1000000000 + os.getpid()}')
try:
    ordinary.write_text('forbidden')
except PermissionError:
    pass
else:
    raise AssertionError('The SDK socket exception allowed an ordinary file')
"""
            result = subprocess.run(
                ["/usr/bin/sandbox-exec", "-p", text, sys.executable, "-c", script,
                 str(app), str(private), str(canary), str(package), *map(str, protected_paths)],
                env={**os.environ, "PYTHONDONTWRITEBYTECODE": "1"},
                cwd=app, capture_output=True, text=True, timeout=30,
            )
            self.assertEqual(0, result.returncode, result.stdout + result.stderr)
            self.assertEqual("not an actor input", canary.read_text())
            self.assertEqual("public dependency", package.read_text())


if __name__ == "__main__":
    unittest.main()
