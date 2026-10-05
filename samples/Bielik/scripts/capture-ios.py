#!/usr/bin/env python3
"""Exercise the installed Debug app and record only its dedicated iOS simulator.

Uses the pinned DevFlow agent's non-forced mutation lease, never XCTest or desktop
input. Resets the sample conversation and local endpoint on the specified simulator.
"""

import argparse
import datetime
import json
import signal
import subprocess
import time
import uuid
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urlencode
from urllib.request import HTTPRedirectHandler, ProxyHandler, Request, build_opener


class NoRedirects(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        raise RuntimeError("The local automation agent must not redirect requests.")


class App:
    def __init__(self, simulator, output):
        self.simulator = simulator
        self.output = output
        self.base = "http://127.0.0.1:9235/api/v1"
        self.lease = str(uuid.uuid4())
        self.opener = build_opener(ProxyHandler({}), NoRedirects())
        self.steps = []

    def request(self, path, body=None, method=None):
        data = None if body is None else json.dumps(body).encode("utf-8")
        req = Request(
            self.base + path,
            data=data,
            method=method,
            headers={"Content-Type": "application/json", "X-DevFlow-Lease": self.lease},
        )
        for attempt in range(10):
            try:
                with self.opener.open(req, timeout=25) as response:
                    result = json.load(response)
                break
            except HTTPError as exception:
                text = exception.read().decode("utf-8")
                details = json.loads(text)
                if (req.get_method() == "GET" and exception.code == 409
                        and details.get("reason") == "capture-changed-during-read" and attempt < 9):
                    time.sleep(0.15 * (attempt + 1))
                    continue
                raise RuntimeError(f"{path}: HTTP {exception.code}: {text}") from exception
        if isinstance(result, dict) and (result.get("success") is False or result.get("ok") is False):
            raise RuntimeError(f"{path}: {result}")
        return result

    def claim(self):
        result = self.request("/agent/lease", {
            "action": "claim", "leaseId": self.lease, "holderKind": "agent",
            "label": "Bielik iOS capture", "force": False,
        })
        if not result.get("youHold"):
            raise RuntimeError("Another session owns the app. No takeover was attempted.")

    def action(self, name, **body):
        self.claim()
        return self.request(f"/ui/actions/{name}", body)

    def navigate(self, route):
        self.action("navigate", route=f"//{route}")
        time.sleep(0.5)

    def element(self, automation_id):
        result = self.request("/ui/elements?" + urlencode({"automationId": automation_id}))
        if len(result) != 1:
            raise AssertionError(f"Expected one {automation_id!r}, got {len(result)}.")
        return result[0]

    def wait(self, description, predicate, timeout=20):
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            if predicate():
                return
            time.sleep(0.3)
        raise AssertionError(f"Timed out: {description}")

    def tap(self, automation_id):
        self.action("tap", elementId=self.element(automation_id)["id"])

    def fill(self, automation_id, text):
        self.action("fill", elementId=self.element(automation_id)["id"], text=text)
        self.wait("text binding", lambda: self.element(automation_id).get("text") == text)

    def scroll(self, automation_id, delta):
        self.action("scroll", elementId=self.element(automation_id)["id"], deltaY=delta, animated=True)
        time.sleep(0.7)

    def screenshot(self, name):
        time.sleep(0.5)
        path = self.output / f"{name}.png"
        subprocess.run(
            ["xcrun", "simctl", "io", self.simulator, "screenshot", str(path)],
            check=True, capture_output=True, text=True,
        )
        return path.name

    def passed(self, name, **evidence):
        self.steps.append({"name": name, "status": "passed", **evidence})
        print(f"{name}: passed", flush=True)

    def wait_idle(self):
        self.wait("generation completed", lambda: self.element("new-conversation")["isEnabled"], timeout=180)

    def reply(self, sequence):
        return self.element(f"message-text-{sequence}").get("text", "")


def exercise(app, tour_only):
    app.navigate("discover")
    title = app.element("discover-title")
    if title.get("bounds", {}).get("width", 0) <= 0:
        raise AssertionError("The discover page was not rendered.")
    app.passed("native_discover", screenshot=app.screenshot("01-discover"))
    app.scroll("discover-scroll", 390)
    app.passed("inspiration_cards", screenshot=app.screenshot("02-inspiration"))
    app.scroll("discover-scroll", -1000)
    app.tap("start-chat")
    app.wait("chat page visible", lambda: app.element("chat-input").get("bounds", {}).get("width", 0) > 0)
    app.tap("new-conversation")
    app.tap("chat-idea-learn")
    draft = app.element("chat-input").get("text", "")
    if "model językowy" not in draft:
        raise AssertionError(f"Inspiration did not populate the composer: {draft!r}")
    app.fill("chat-input", "")
    app.passed("native_navigation_and_presets", screenshot=app.screenshot("03-chat-empty"))

    app.navigate("model")
    app.passed("pinned_model_screen", screenshot=app.screenshot("04-model"))
    app.scroll("model-scroll", 350)
    app.passed("local_inference_disclosure", screenshot=app.screenshot("05-model-details"))

    app.navigate("settings")
    app.fill("server-address", "https://example.com")
    app.tap("save-connection")
    app.wait("public endpoint rejection", lambda: "Tylko lokalnie" in app.element("connection-error").get("text", ""))
    app.passed("public_endpoint_rejected", error=app.element("connection-error")["text"])
    app.fill("server-address", "http://127.0.0.1:11434")
    app.tap("save-connection")
    app.wait("connection check", lambda: app.element("save-connection")["isEnabled"])
    app.passed("local_settings", screenshot=app.screenshot("06-settings"))

    if tour_only:
        app.navigate("chat")
        app.passed("tour_only_no_inference_claimed")
        return

    if app.element("connection-status").get("text") != "Lokalny Bielik gotowy":
        raise AssertionError("The exact local Bielik must be installed before end-to-end inference.")
    app.passed("actual_model_ready", digest=app.element("model-digest").get("text"))

    app.navigate("chat")
    app.tap("new-conversation")
    app.fill("chat-input", "Wyjaśnij w dwóch krótkich zdaniach, czym jest lokalna sztuczna inteligencja.")
    app.tap("send-message")
    app.wait_idle()
    first_reply = app.reply(2)
    if not first_reply.strip() or app.element("chat-error").get("text"):
        raise AssertionError(f"Real streamed generation failed: {first_reply}")
    status = app.element("message-status-2").get("text", "")
    if "TOK/S" not in status:
        raise AssertionError(f"Missing actual completion metrics: {status}")
    app.passed("real_polish_streamed_reply", reply=first_reply, metrics=status,
               screenshot=app.screenshot("07-real-chat"))
    app.tap("copy-reply")
    copied = subprocess.run(
        ["xcrun", "simctl", "pbpaste", app.simulator], check=True, capture_output=True, text=True,
    ).stdout
    if copied.rstrip("\n") != first_reply.rstrip("\n"):
        raise AssertionError("The system clipboard did not contain the actual model reply.")
    app.passed("native_clipboard")

    app.navigate("settings")
    app.tap("save-connection")
    app.wait("same endpoint check", lambda: app.element("save-connection")["isEnabled"])
    app.navigate("chat")
    if app.reply(2) != first_reply:
        raise AssertionError("Rechecking an unchanged endpoint discarded the conversation.")
    app.passed("same_endpoint_check_keeps_conversation")

    app.tap("new-conversation")
    app.fill("chat-input", 'Hasło tej rozmowy to bursztyn. Zapamiętaj je i odpowiedz tylko "OK".')
    app.tap("send-message")
    app.wait_idle()
    app.fill("chat-input", "Jakie jest hasło tej rozmowy? Odpowiedz wyłącznie hasłem.")
    app.tap("send-message")
    app.wait_idle()
    recalled = app.reply(4).strip().lower().strip(".")
    if recalled != "bursztyn":
        raise AssertionError(f"Conversation memory mismatch: {recalled!r}")
    app.passed("actual_conversation_memory", reply=recalled,
               screenshot=app.screenshot("08-memory"))

    app.tap("new-conversation")
    app.fill("chat-input", "Napisz długie opowiadanie po polsku, co najmniej tysiąc słów, o wyprawie w góry.")
    app.tap("send-message")
    app.wait("visible stop control", lambda: app.element("stop-generation")["isVisible"])
    app.wait("real streaming fragment",
             lambda: bool(app.reply(2).strip()) and app.reply(2) != "Przygotowuję odpowiedź…")
    app.tap("stop-generation")
    app.fill("chat-input", "")
    app.wait("canceled completion", lambda: "PRZERWANO" in app.element("message-status-2").get("text", ""))
    app.passed("real_generation_cancellation", screenshot=app.screenshot("09-cancellation"))
    app.tap("new-conversation")

    app.navigate("settings")
    app.fill("server-address", "http://127.0.0.1:1")
    app.tap("save-connection")
    app.wait("server unavailable error", lambda: "Nie można połączyć" in app.element("connection-error").get("text", ""))
    app.passed("offline_error_without_fallback", error=app.element("connection-error")["text"])
    app.fill("server-address", "http://127.0.0.1:11434")
    app.tap("save-connection")
    app.wait("restored local model", lambda: app.element("connection-status").get("text") == "Lokalny Bielik gotowy")
    app.navigate("chat")
    app.fill("chat-input", "Wyjaśnij w dwóch krótkich zdaniach, dlaczego warto korzystać z Bielika lokalnie.")
    app.tap("send-message")
    app.wait_idle()
    recovered_reply = app.reply(2)
    if not recovered_reply.strip() or app.element("chat-error").get("text"):
        raise AssertionError(f"Chat did not recover after restoring the local endpoint: {recovered_reply}")
    if "TOK/S" not in app.element("message-status-2").get("text", ""):
        raise AssertionError("The recovered response did not complete successfully.")
    app.passed("restored_local_chat", reply=recovered_reply, screenshot=app.screenshot("10-final-chat"))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--simulator", required=True, help="Dedicated, already booted simulator UDID")
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--tour-only", action="store_true", help="UI progress tour without claiming model inference")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    app = App(args.simulator, args.output)
    metadata = app.request("/agent/status")
    if metadata.get("app", {}).get("packageId") != "dev.bielik.companion":
        raise RuntimeError("Port 9235 is not the Bielik app. No actions were performed.")

    movie = args.output / ("ui-progress.mp4" if args.tour_only else "local-bielik-walkthrough.mp4")
    error = None
    with (args.output / "recording.log").open("w") as log:
        recorder = subprocess.Popen(
            ["xcrun", "simctl", "io", args.simulator, "recordVideo", "--codec=h264", "--force", str(movie)],
            stdout=log, stderr=log,
        )
        try:
            time.sleep(1)
            if recorder.poll() is not None:
                raise RuntimeError("Simulator recording failed to start. See recording.log.")
            exercise(app, args.tour_only)
        except (AssertionError, RuntimeError, OSError, ValueError) as exception:
            error = str(exception)
            app.screenshot("failed-step")
        finally:
            if recorder.poll() is None:
                recorder.send_signal(signal.SIGINT)
                recorder.wait(timeout=30)
            app.request("/agent/lease", {"action": "release", "leaseId": app.lease})
    report = {
        "recorded_at": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "simulator": args.simulator, "app": metadata, "xctest_used": False,
        "local_model_exercised": any(step["name"] == "real_polish_streamed_reply" for step in app.steps),
        "steps": app.steps, "error": error,
        "recording": movie.name,
    }
    (args.output / "ui-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8",
    )
    if error:
        raise RuntimeError(error)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
