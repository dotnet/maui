# BenchmarkDotNet: How It Works (official guidance summary)

Source: https://benchmarkdotnet.org/articles/guides/how-it-works.html

## Core model
- BDN generates/builds a benchmark executable (Release).
- It runs benchmark process(es) per job/runtime (`LaunchCount`).
- It measures operations inside iterations and applies overhead correction.

## Terminology
- **Operation**: one invocation of benchmarked workload.
- **Iteration**: a batch of operations.
- Iteration phases include pilot, warmup, actual workload, and overhead runs.

## Why no manual loops in benchmark methods
BDN already controls invocation count/unrolling and reports per-op metrics. Manual loops inside benchmark methods distort per-operation interpretation and can hide regressions.
