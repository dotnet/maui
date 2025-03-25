namespace Microsoft.Maui.Benchmarks;

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class IndexOfBenchmarker
{
	private List<int> list;
	private int[] array;
	private HashSet<int> hashSet;

	[GlobalSetup]
	public void Setup()
	{
		list = Enumerable.Range(0, 1000000).ToList();
		array = list.ToArray();
		hashSet = list.ToHashSet();
	}

	[Benchmark]
	public void IndexOfList() => EnumerableExtensions.IndexOf(list, 999999);

	[Benchmark]
	public void IndexOfArray() => EnumerableExtensions.IndexOf(array, 999999);

	[Benchmark]
	public void IndexOfHashSet() => EnumerableExtensions.IndexOf(hashSet, 999999);
}