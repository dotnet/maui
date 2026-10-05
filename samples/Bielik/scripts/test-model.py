#!/usr/bin/env python3
"""Small, reproducible probes of the exact local Bielik used by the app."""

import argparse
import datetime
import json
import re
import time
from pathlib import Path
from urllib.request import HTTPRedirectHandler, ProxyHandler, Request, build_opener


ROOT = Path(__file__).resolve().parents[1]
MODEL = re.search(
    r'public const string Id = "([^"]+)"',
    (ROOT / "Bielik.Core" / "ModelInfo.cs").read_text(encoding="utf-8"),
).group(1)
BASE_URL = "http://127.0.0.1:11434"


class NoRedirects(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        raise RuntimeError("Redirects are not allowed in local-only model probes.")


OPENER = build_opener(ProxyHandler({}), NoRedirects())
SYSTEM = {
    "role": "system",
    "content": "Jesteś pomocnym polskojęzycznym asystentem. Przestrzegaj dokładnie wymaganego formatu odpowiedzi.",
}


def request(path, payload=None):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    req = Request(
        BASE_URL + path,
        data=data,
        headers={"Content-Type": "application/json"},
    )
    with OPENER.open(req, timeout=300) as response:
        if response.geturl() != BASE_URL + path:
            raise RuntimeError("Redirects are not allowed in local-only model probes.")
        return json.load(response)


def chat(messages, output_format=None):
    payload = {
        "model": MODEL,
        "messages": [SYSTEM, *messages],
        "stream": False,
        "keep_alive": "15m",
        "options": {"temperature": 0.1, "seed": 42, "num_ctx": 8192, "num_predict": 768, "num_gpu": 0},
    }
    if output_format is not None:
        payload["format"] = output_format
    start = time.monotonic()
    response = request("/api/chat", payload)
    if response.get("model", "").casefold() != MODEL.casefold() or not response.get("done"):
        raise RuntimeError("Invalid local Bielik completion: " + json.dumps(response, ensure_ascii=False))
    duration = response.get("eval_duration", 0)
    return {
        "messages": messages,
        "format": output_format,
        "response": response["message"]["content"],
        "wall_seconds": round(time.monotonic() - start, 3),
        "load_seconds": round(response.get("load_duration", 0) / 1e9, 3),
        "generation_tokens": response.get("eval_count", 0),
        "tokens_per_second": round(response.get("eval_count", 0) * 1e9 / duration, 2) if duration else None,
        "raw_statistics": {
            key: value for key, value in response.items() if key not in ("message", "created_at")
        },
    }


def check_json(text):
    try:
        return json.loads(text) == {"miasto": "Gdańsk", "liczba": 3}
    except json.JSONDecodeError:
        return False


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--code-output", type=Path)
    args = parser.parse_args()
    installed = next(
        (item for item in request("/api/tags")["models"] if item["name"].casefold() == MODEL.casefold()),
        None,
    )
    if installed is None:
        raise RuntimeError(f"Install the official model first: ollama pull {MODEL}")

    tests = []
    report = {
        "recorded_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "endpoint": BASE_URL,
        "ollama": request("/api/version"),
        "model": installed,
        "cloud_fallback": False,
        "execution": "CPU (num_gpu=0), matching the app",
        "temperature": 0.1,
        "seed": 42,
        "scope": "Small functional probes, not a benchmark or a general model-quality guarantee.",
        "tests": tests,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)

    def save_report():
        args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    def record(name, messages, predicate=None, output_format=None):
        result = chat(messages, output_format)
        result["name"] = name
        result["status"] = "manual_review" if predicate is None else "passed" if predicate(result["response"]) else "failed"
        tests.append(result)
        save_report()
        print(f'{name}: {result["status"]}; {result["tokens_per_second"]} tok/s; {result["wall_seconds"]} s', flush=True)
        return result

    record(
        "arithmetic",
        [{"role": "user", "content": "Odpowiedz wyłącznie liczbą. Ile wynosi 17 + 25?"}],
        lambda text: text.strip() == "42",
    )
    json_prompt = (
        'Zwróć wyłącznie JSON z kluczami "miasto" i "liczba". '
        'Dane: miasto Gdańsk, liczba 3. Nie dodawaj Markdown ani żadnego wyjaśnienia.'
    )
    record("json_instruction_only", [{"role": "user", "content": json_prompt}], check_json)
    record(
        "json_schema",
        [{"role": "user", "content": json_prompt}],
        check_json,
        {
            "type": "object",
            "properties": {"miasto": {"type": "string"}, "liczba": {"type": "integer"}},
            "required": ["miasto", "liczba"],
            "additionalProperties": False,
        },
    )
    first = record(
        "conversation_memory_setup",
        [{"role": "user", "content": 'Hasło tej rozmowy to bursztyn. Zapamiętaj je i odpowiedz wyłącznie "OK".'}],
        lambda text: text.strip().strip(".").upper() == "OK",
    )
    record(
        "conversation_memory_recall",
        [
            *first["messages"],
            {"role": "assistant", "content": first["response"]},
            {"role": "user", "content": "Jakie jest hasło tej rozmowy? Odpowiedz wyłącznie hasłem."},
        ],
        lambda text: text.strip().lower().strip(".") == "bursztyn",
    )
    record(
        "polish_explanation",
        [{"role": "user", "content": "Wyjaśnij w dwóch krótkich zdaniach, czym różni się model językowy działający lokalnie od usługi AI w chmurze."}],
    )
    code = record(
        "csharp_generation",
        [{
            "role": "user",
            "content": (
                "Zwróć wyłącznie kompletny kod C# bez Markdown. Użyj `using System;` i publicznej klasy statycznej "
                "`Generated`. Napisz metodę `public static long SumEven(int[] values)`, która sumuje tylko parzyste "
                "liczby, także ujemne, używając long, aby uniknąć przepełnienia int. Dla null rzuć ArgumentNullException. "
                "Bez zależności zewnętrznych i bez kodu wejścia/wyjścia."
            ),
        }],
    )
    if args.code_output is not None:
        args.code_output.parent.mkdir(parents=True, exist_ok=True)
        args.code_output.write_text(code["response"], encoding="utf-8")
    save_report()
    return 1 if any(test["status"] == "failed" for test in tests) else 0


if __name__ == "__main__":
    raise SystemExit(main())
