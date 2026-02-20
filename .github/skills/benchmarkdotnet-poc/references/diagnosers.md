# BenchmarkDotNet: Diagnosers (official guidance summary)

Source: https://benchmarkdotnet.org/articles/configs/diagnosers.html

## Allocation-focused baseline
Use `[MemoryDiagnoser]` for memory/allocation work.

It provides:
- `Allocated` memory per operation
- `Gen 0/1/2` collections per 1000 operations

## Notes
- Diagnosers can trigger separate benchmark runs and increase total runtime.
- Memory diagnoser relies on `GC.GetAllocatedBytesForCurrentThread` and is highly accurate with default/short+ jobs.

## Example
```csharp
[MemoryDiagnoser]
public class MyBenchmarks
{
    [Benchmark]
    public object Create() => new object();
}
```
