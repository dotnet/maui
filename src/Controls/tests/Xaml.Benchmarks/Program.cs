using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Controls.Xaml.Benchmarks;

[MemoryDiagnoser(false)]
public class InflatorBenchmark
{
#pragma warning disable CA1806 // Do not ignore method results
	[Benchmark(Baseline = true)]
	public void XamlC() => new UnitTests.Benchmark(XamlInflator.XamlC);

	[Benchmark] public void SourceGen() => new UnitTests.Benchmark(XamlInflator.SourceGen);

	[Benchmark] public void SourceGenLazy() => new UnitTests.Benchmark2(XamlInflator.SourceGen);

	[Benchmark] public void Runtime() => new UnitTests.Benchmark(XamlInflator.Runtime);
#pragma warning restore CA1806 // Do not ignore method results
}

[MemoryDiagnoser(false)]
public class GeneratorBenchmark
{
	[Benchmark(Baseline = true)]
	public void XamlC() => new UnitTests.Benchmark().MockGenerationXamlC();

	[Benchmark]
	public void SourceGen() => new UnitTests.Benchmark().MockSourceGen();
}

class Program
{
	static void Main(string[] args)
	{
		BenchmarkDotNet.Running.BenchmarkRunner.Run<InflatorBenchmark>();
		//BenchmarkDotNet.Running.BenchmarkRunner.Run<GeneratorBenchmark>();
	}
}