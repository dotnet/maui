# BenchmarkDotNet: Good Practices (official guidance summary)

Source: https://benchmarkdotnet.org/articles/guides/good-practices.html

## Non-negotiables
1. **Release build only** (optimized), no debugger attached.
2. Keep environment stable (machine load, power mode, background apps).
3. Avoid dead-code elimination; return/use benchmark results.
4. Don't extrapolate across runtimes/platforms without measuring there.

## For this PoC
- Benchmark methods must be one operation (no manual loops).
- Use same benchmark filters/profiles for before-vs-after comparison.
