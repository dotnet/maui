using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Benchmarks;

[MemoryDiagnoser]
public class ClampBenchmarker
{
	[Params(3000000)]
	public int N;

	[Benchmark]
	public void ClampBenchmark()
	{
		int min = 0, max = 100;
		for (int i = 0; i < N; i++)
		{
			for (int j = min; j <= max; j++)
			{
				CompareExtensions.Clamp(j, min, max);
			}
		}
	}
}