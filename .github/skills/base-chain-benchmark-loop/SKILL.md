---
name: base-chain-benchmark-loop
description: "Rebuild and run base-chain allocation benchmarks after each DS change, then record results in the evaluation log."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: macOS/Windows with local MAUI SDK provisioned
---

# Base-chain Benchmark Loop Skill

Use this runbook after **every DSxx code change** to keep comparisons consistent and per-operation.

> Primary canonical guidance now lives in `.github/skills/benchmarkdotnet-poc/SKILL.md`.
> Keep this file as a compact shortcut focused on the DS loop commands.

## Rules
- Do not add manual inner loops inside benchmark methods.
- One `[Benchmark]` method = one logical operation (e.g., one `new Label()`).
- Let BenchmarkDotNet control iteration counts and invocation.

## Benchmark project
- Primary project: `src/Core/tests/Benchmarks/Core.Benchmarks.csproj`
- Benchmark classes should include B0-B6 scenarios for base-chain baggage.

## Rebuild + run loop (after each DS change)

```bash
# 1) Build benchmark project (Release)
dotnet build src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release

# 2) Run targeted class/filter for the DS change (fast check)
dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -- --filter "*BaseChain*" --memory true

# 3) Run full B0-B6 matrix for cumulative snapshot
dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -- --filter "*BaseChain*|*Startup*|*Resource*" --memory true
```

> If local repo policy requires local SDK, substitute `dotnet` with `./.dotnet/dotnet`.

## Recording requirements (mandatory)
After each run, append to `poc-evaluation-log.md`:
- DS id
- diff summary + changed files
- command(s) used
- allocated bytes/op, mean, Gen0/Gen1 deltas
- keep/rework/revert decision

## Baseline and final checkpoints
- Baseline checkpoint: before DS01.
- Final checkpoint: after DS34 (or best-known reverted set).

## Overnight execution strategy
- Prefer low-risk DS items first for stable cumulative gains.
- Revert quickly on regression/noise-only outcomes.
- Keep benchmark environment stable (same config, same machine load profile when possible).
