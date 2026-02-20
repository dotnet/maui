# BenchmarkDotNet: How to Run (official guidance summary)

Source: https://benchmarkdotnet.org/articles/guides/how-to-run.html

## Key points
- BenchmarkDotNet works with **console apps**.
- Use `BenchmarkRunner` for quick/single-type runs.
- Use `BenchmarkSwitcher` when you need CLI filtering and flexible selection.
- Pass `args` from `Main(string[] args)` into `.Run(args)` so CLI options work.

## Recommended pattern
```csharp
static void Main(string[] args)
    => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
```

## Useful command examples
```bash
# Filter by benchmark class/method patterns
dotnet run -c Release -- --filter "*BaseChain*"

# Short job across runtimes (example)
dotnet run -c Release -- --job short --runtimes net8.0 --filter "*Label*"
```
