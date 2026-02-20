---
name: benchmarkdotnet-poc
description: "Run BenchmarkDotNet correctly with per-operation metrics, then execute this repo's base-chain B0-B6 PoC loop and log each DS change evaluation."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Console benchmark projects with BenchmarkDotNet; optimized for this repository's Core.Benchmarks workflow.
---

# BenchmarkDotNet PoC Skill

Use this skill when you need reliable performance/allocation benchmarks, especially for the base-chain data-structure PoC (`DS01`-`DS34`).

## When to use
- You need benchmark results for code changes (`Allocated/op`, `Mean`, `Gen0/Gen1`).
- You need to compare baseline vs after-change metrics.
- You are implementing or evaluating any `eval-dsXX` todo.
- You need startup-like benchmark coverage (not only microbenchmarks).

## Mandatory pre-run checklist
Before every benchmark run, explicitly verify:
1. Benchmark methods represent one operation; **no manual inner loops**.
2. Build is `Release`; no attached debugger.
3. Benchmarks avoid dead-code elimination (return/use results).
4. You are using the same profile/filter when comparing before vs after.
5. You will append command + results + decision to `poc-evaluation-log.md`.

## Workflow

### 1) Choose benchmark scope
- **Micro-only**: isolate one low-level operation.
- **Startup-like**: use B0-B6 matrix (see repo addendum reference).
- **Change validation**: run targeted subset first, then full matrix.

### 2) Choose run strategy intentionally
- Default: `Throughput` for steady-state microbenchmarks.
- `ColdStart`: when startup/first-call behavior matters.
- `Monitoring`: long/unstable operations where per-iteration sampling is useful.

### 3) Run benchmarks from CLI
Use a console benchmark app (`BenchmarkSwitcher` preferred for filtering):

```bash
# Build benchmark project first
dotnet build src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release

# Run filtered benchmarks
dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -- --filter "*BaseChain*" --memory true
```

### 4) Collect diagnostics correctly
- Always enable memory diagnostics for allocation work.
- Capture `Allocated` and `Gen 0/1/2` per 1000 ops.
- For startup-like runs, keep same environment to reduce noise.

### 5) Record and decide
For each DS change:
- Record pre/post commands and outputs.
- Record deltas in `poc-evaluation-log.md`.
- Decide: **Keep / Rework / Revert** before moving to next DS item.

## Reference files
- `references/how-to-run.md`
- `references/good-practices.md`
- `references/run-strategy.md`
- `references/how-it-works.md`
- `references/diagnosers.md`
- `references/repo-base-chain-addendum.md`

## Repository-specific addendum
For this PoC, always follow `references/repo-base-chain-addendum.md` for:
- B0-B6 benchmark ladder
- DS01->DS34 cumulative loop
- logging and acceptance criteria
