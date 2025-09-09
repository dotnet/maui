using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Controls.Xaml.Benchmarks;

[MemoryDiagnoser(false)]
public class GeneratorBenchmark
{
	[Benchmark(Baseline = true)]
	public void XamlC() => new UnitTests.Benchmark().MockGenerationXamlC();

	[Benchmark]
	public void SourceGen() => new UnitTests.Benchmark().MockSourceGen();

	[Benchmark]
	public void SourceGenLazy() => new UnitTests.Benchmark().MockSourceGenLazy();
}
