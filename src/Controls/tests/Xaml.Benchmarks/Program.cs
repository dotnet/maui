﻿using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Controls.Xaml.Benchmarks;

[MemoryDiagnoser(false)]
public class LayoutBenchmark
{
	[Benchmark(Baseline = true)]
	public void XamlC()
	{
		var page = new UnitTests.Benchmark(XamlInflator.XamlC);
	}

	[Benchmark]
	public void SourceGen()
	{
		var page = new UnitTests.Benchmark(XamlInflator.SourceGen);
	}
	[Benchmark]
	public void Runtime()
	{
		var page = new UnitTests.Benchmark(XamlInflator.Runtime);
	}
}

class Program
{
	static void Main(string[] args)
	{		
		BenchmarkDotNet.Running.BenchmarkRunner.Run<LayoutBenchmark>();
	}
}