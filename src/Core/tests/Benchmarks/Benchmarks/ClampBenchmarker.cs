using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Benchmarks;

[MemoryDiagnoser]
public class ClampBenchmarker
{
	private const int Iterations = 1000000;

	[Benchmark]
	public void ClampBenchmarkInt()
	{
		int min = 0, max = 100;
		for (int i = 0; i < Iterations; i++)
		{
			for (int j = min; j <= max; j++)
			{
				j.Clamp(min, max);
			}
		}
	}

	[Benchmark]
	public void ClampBenchmarkFloat()
	{
		float min = 0, max = 100;
		for (float i = 0; i < Iterations; i++)
		{
			for (float j = min; j <= max; j++)
			{
				j.Clamp(min, max);
			}
		}
	}

	[Benchmark]
	public void ClampBenchmarkDouble()
	{
		double min = 0, max = 100;
		for (double i = 0; i < Iterations; i++)
		{
			for (double j = min; j <= max; j++)
			{
				j.Clamp(min, max);
			}
		}
	}
}