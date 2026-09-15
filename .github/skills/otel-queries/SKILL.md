---
name: otel-queries
description: Guidance for querying and interpreting Copilot and agentic workflow OpenTelemetry token and execution metrics.
disable-model-invocation: true
---

# OTEL Queries

Use this skill when investigating Copilot or agentic workflow OpenTelemetry output, token usage, model usage, execution timing, or exported `COPILOT_OTEL_FILE_EXPORTER_PATH` JSONL files.

## Guidance

- Prefer local JSONL inspection when an OTEL export file is available.
- Summarize token counts by model, agent step, and request type.
- Treat missing or malformed OTEL records as diagnostic data, not workflow failure by default.
- Do not require Android/product validation for workflow-only telemetry analysis changes.
