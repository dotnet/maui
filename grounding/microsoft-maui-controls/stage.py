"""Stage pinned public consumers outside the MAUI checkout; never launch a model."""

import argparse
import hashlib
import io
import json
import os
from pathlib import Path, PurePosixPath
import re
import tarfile
import urllib.request
import xml.etree.ElementTree as ET


HERE = Path(__file__).resolve().parent
IGNORED = {".git", ".github", ".vs", ".vscode", "bin", "obj"}
SUITE = json.loads((HERE / "suite.json").read_text())
SDK = {mode: SUITE[mode]["sdk"] for mode in ("source", "target")}
PACKAGE = {mode: SUITE[mode]["controls"] for mode in ("source", "target")}
TFM = {"source": "net10.0-android", "target": "net11.0-android"}


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def files(root):
    result = {}
    for directory, directories, names in os.walk(root):
        directories[:] = sorted(name for name in directories if name not in IGNORED)
        for name in sorted(names):
            path = Path(directory) / name
            if name not in IGNORED and path.is_file():
                result[path.relative_to(root).as_posix()] = sha256(path.read_bytes())
    return dict(sorted(result.items()))


def contained(root, relative):
    path = PurePosixPath(relative)
    if path.is_absolute() or ".." in path.parts or "\\" in relative:
        raise ValueError(f"Unsafe relative path: {relative}")
    resolved = (root / relative).resolve()
    resolved.relative_to(root.resolve())
    return resolved


def adapt(project, mode):
    """Declare setup adaptations, not an independently scored agent solution."""
    tree = ET.parse(project)
    root = tree.getroot()
    if root.tag != "Project":
        raise ValueError("Only SDK-style projects without an XML namespace are supported.")
    removed = []
    for parent in root.iter():
        for item in list(parent):
            if item.tag in {
                "TargetFramework", "TargetFrameworks", "SupportedOSPlatformVersion",
                "TargetPlatformMinVersion", "MauiVersion",
            }:
                removed.append(ET.tostring(item, encoding="unicode"))
                parent.remove(item)
            elif item.tag == "PackageReference" and item.get("Include") in {
                "Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Compatibility",
                "Microsoft.Extensions.Logging.Debug",
            }:
                removed.append(ET.tostring(item, encoding="unicode"))
                parent.remove(item)
    props = ET.SubElement(root, "PropertyGroup")
    for key, value in {
        "TargetFramework": TFM[mode],
        "MauiVersion": PACKAGE[mode],
        "SupportedOSPlatformVersion": "24.0",
        "EmbedAssembliesIntoApk": "true",
        "AndroidPackageFormats": "apk",
    }.items():
        ET.SubElement(props, key).text = value
    items = ET.SubElement(root, "ItemGroup")
    ET.SubElement(items, "PackageReference", Include="Microsoft.Maui.Controls", Version="$(MauiVersion)")
    ET.SubElement(items, "PackageReference", Include="Microsoft.Extensions.Logging.Debug", Version=SUITE["loggingVersion"])
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    return removed


def stage(suite, case_id, root, mode):
    root = root.resolve()
    case = next(case for case in suite["cases"] if case["id"] == case_id)
    destination = root / "consumers" / f"{case_id}-{mode}"
    if destination.exists():
        raise FileExistsError(f"Refusing to overwrite a stage: {destination}")
    commit = case["commit"]
    if not re.fullmatch(r"[a-f0-9]{40}", commit):
        raise ValueError("An exact upstream commit is required.")
    cache = root / "sources"
    cache.mkdir(parents=True, exist_ok=True)
    archive = cache / f"{case['repo'].replace('/', '-')}-{commit}.tar.gz"
    if not archive.exists():
        url = f"https://codeload.github.com/{case['repo']}/tar.gz/{commit}"
        with urllib.request.urlopen(url, timeout=120) as response:
            data = response.read()
        with tarfile.open(fileobj=io.BytesIO(data), mode="r:gz") as check:
            if not check.getmembers():
                raise ValueError("Empty source archive.")
        archive.write_bytes(data)
    if sha256(archive.read_bytes()) != case["archiveSha256"]:
        raise ValueError("Downloaded or cached source archive does not match the pinned checksum.")
    source_prefix = PurePosixPath(case["directory"])
    original = {}
    extracted = {}
    with tarfile.open(archive, "r:gz") as source:
        for member in source.getmembers():
            relative = PurePosixPath(*PurePosixPath(member.name).parts[1:])
            if member.isfile() and relative.as_posix() == case["license"]:
                stream = source.extractfile(member)
                extracted["UPSTREAM-LICENSE"] = stream.read()
            if not relative.is_relative_to(source_prefix):
                continue
            name = relative.relative_to(source_prefix).as_posix()
            if any(part in IGNORED for part in relative.parts) or relative.name == "AGENTS.md":
                continue
            if member.issym() or member.islnk():
                raise ValueError(f"Source symlinks are not accepted: {relative}")
            if member.isfile():
                stream = source.extractfile(member)
                data = stream.read()
                original[name] = sha256(data)
                extracted[f"app/{name}"] = data
    if "UPSTREAM-LICENSE" not in extracted or not original:
        raise ValueError("Missing project sources or upstream license.")
    for license_id in case.get("additionalLicenses", []):
        if license_id != "Apache-2.0":
            raise ValueError(f"Unrecognized additional license: {license_id}")
        license_file = cache / "Apache-2.0.txt"
        if not license_file.exists():
            url = "https://www.apache.org/licenses/LICENSE-2.0.txt"
            with urllib.request.urlopen(url, timeout=60) as response:
                license_file.write_bytes(response.read())
        extracted["UPSTREAM-APACHE-2.0"] = license_file.read_bytes()
    for name in extracted:
        contained(destination, name)
    destination.mkdir(parents=True)
    for name, data in extracted.items():
        path = contained(destination, name)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
    app = destination / "app"
    project = contained(app, case["project"])
    removed = adapt(project, mode)
    for relative in case.get("managedProjects", []):
        dependency = contained(app, relative)
        tree = ET.parse(dependency)
        for item in tree.getroot().iter():
            if item.tag in {"TargetFramework", "TargetFrameworks"}:
                item.text = "net10.0" if mode == "source" else "net11.0"
        ET.indent(tree, space="  ")
        tree.write(dependency, encoding="unicode")
    save(app / "global.json", {"sdk": {"version": SDK[mode], "rollForward": "disable"}})
    (destination / "Directory.Build.props").write_text(
        f"<Project><PropertyGroup><MauiVersion>{PACKAGE[mode]}</MauiVersion>"
        "<DotNetVersion>10.0.0</DotNetVersion></PropertyGroup></Project>\n"
    )
    (destination / "Directory.Build.targets").write_text("<Project />\n")
    (destination / "NuGet.config").write_text(
        '<configuration><packageSources><clear />'
        '<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />'
        '</packageSources></configuration>\n'
    )
    (app / "prompt.txt").write_text(case["prompt"] + "\n", encoding="utf-8")
    record = {
        "case": case_id, "mode": mode, "repo": case["repo"], "commit": commit,
        "directory": case["directory"], "license": case["license"], "project": case["project"],
        "archiveSha256": sha256(archive.read_bytes()), "originalFiles": original,
        "setupAdaptations": {
            "removedProjectItems": removed,
            "description": "Android-only, pinned SDK/Controls/min24, standalone APK and explicit logging dependency. Same source and declared setup changes in both control stages.",
        },
        "stagedFiles": files(destination),
        "status": "staged-not-evaluated", "agentAttempt": False,
    }
    save(destination / "stage.json", record)
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--case", required=True)
    parser.add_argument("--mode", choices=SDK, required=True)
    args = parser.parse_args()
    suite = json.loads((HERE / "suite.json").read_text())
    print(stage(suite, args.case, args.root, args.mode))


if __name__ == "__main__":
    main()
