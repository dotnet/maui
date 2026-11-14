"""Fail selected Helix work items that did not execute any tests."""

import pathlib
import sys
import xml.etree.ElementTree as ET


def executed_tests(root):
    if root.tag == "assemblies":
        return sum(
            int(assembly.get("passed", "0")) + int(assembly.get("failed", "0"))
            for assembly in root.findall("assembly")
        )
    if root.tag == "test-run":
        return int(root.get("passed", "0")) + int(root.get("failed", "0"))
    if root.tag == "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}TestRun":
        counters = root.find(
            "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}ResultSummary/"
            "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters"
        )
        if counters is not None:
            return int(counters.get("executed", "0"))
    return 0


def main():
    output = pathlib.Path(sys.argv[1])
    for path in sorted(output.rglob("*")):
        if path.suffix.lower() not in (".xml", ".trx") or not path.is_file():
            continue
        try:
            count = executed_tests(ET.parse(path).getroot())
        except (ET.ParseError, OSError, ValueError) as error:
            print(f"Cannot read test results {path}: {error}", file=sys.stderr)
            continue
        if count > 0:
            print(f"Selected device coverage executed {count} tests ({path.name}).")
            return 0
    print(
        "ERROR: Selected device coverage has no readable results with executed tests; "
        "zero or missing coverage is not a passing selection.",
        file=sys.stderr,
    )
    return 1


if __name__ == "__main__":
    sys.exit(main())
