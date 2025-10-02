// See https://aka.ms/new-console-template for more information

using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Benchmark;

class App : Microsoft.Maui.Controls.Application
{
	public App()
	{

	}
}

public class LayoutBenchmark
{
	[BenchmarkDotNet.Attributes.Benchmark]
	public void BenchmarkMethod()
	{
		var page = new Microsoft.Maui.Controls.Xaml.UnitTests.Benchmark();
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