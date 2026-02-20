# BenchmarkDotNet: Choosing Run Strategy (official guidance summary)

Source: https://benchmarkdotnet.org/articles/guides/choosing-run-strategy.html

## Throughput (default)
- Best for microbenchmarks with steady-state behavior.
- BDN auto-tunes operation count and iteration plan.

## ColdStart
- Useful for first-call/startup behavior.
- Skips pilot/warmup behavior typical in Throughput.

## Monitoring
- Useful for long-running or unstable/non-steady workloads.
- Collects per-iteration measurements; good for noisy distributions.

## Practical mapping for this repo
- B0-B3: Throughput by default.
- B4-B6 startup-like checks: evaluate ColdStart/Monitoring where appropriate.
