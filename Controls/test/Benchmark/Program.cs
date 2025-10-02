// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Benchmark;

class App : Microsoft.Maui.Controls.Application
{
	public App()
	{

	}
}

[MemoryDiagnoser(false)]
public class LayoutBenchmark
{
	[BenchmarkDotNet.Attributes.Benchmark(Baseline = true)]
	public void XamlC()
	{
		var page = new Microsoft.Maui.Controls.Xaml.UnitTests.Benchmark("XamlC");
	}

	[BenchmarkDotNet.Attributes.Benchmark]
	public void SourceGen()
	{
		var page = new Microsoft.Maui.Controls.Xaml.UnitTests.Benchmark("SourceGen");
	}
	[BenchmarkDotNet.Attributes.Benchmark]
	public void Runtime()
	{
		var page = new Microsoft.Maui.Controls.Xaml.UnitTests.Benchmark("Runtime");
	}
}

class Program
{
	static void Main(string[] args)
	{
		var app = new App();
		
		
		BenchmarkDotNet.Running.BenchmarkRunner.Run<LayoutBenchmark>();
	}
}