"""Classify deterministic grader results; do not treat infrastructure as model failure."""

import argparse
import json
from pathlib import Path

from stage import save


def classify(record):
    if record["status"] == "error":
        return {"status": "infrastructure", "reason": "Vally did not complete grading."}
    if record["status"] != "success":
        raise ValueError(f"Unsupported Vally record status: {record['status']}")
    grade = record["gradeResult"]
    outputs = [
        json.loads(detail["metadata"]["stdout"])
        for detail in grade["details"]
        if detail.get("graderType") == "run-command"
    ]
    if len(outputs) != 1 or outputs[0]["status"] not in {
        "target-pass", "candidate-failed", "infrastructure",
    }:
        raise ValueError("Expected exactly one structured MAUI candidate grader result.")
    result = outputs[0]
    if (result["status"] == "target-pass") != grade["passed"]:
        raise ValueError("Vally and the candidate grader disagree.")
    return {
        "case": record["trajectory"]["stimulus"]["name"],
        "status": result["status"], "kind": result["kind"], "evidence": result["evidence"],
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("inputs", type=Path, nargs="+")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists():
        raise FileExistsError(f"Refusing to overwrite a summary: {args.output}")
    results = [
        classify(json.loads(line))
        for path in args.inputs for line in path.read_text().splitlines() if line.strip()
    ]
    if not results:
        raise ValueError("No grading records were supplied.")
    summary = {
        "kind": "candidate-grader-results-not-an-effectiveness-study",
        "results": results,
        "infrastructureExcluded": sum(row["status"] == "infrastructure" for row in results),
        "skillEffectiveness": "not-measured",
    }
    save(args.output, summary)
    print(json.dumps(summary, indent=2))


if __name__ == "__main__":
    main()
