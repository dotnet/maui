using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class ListUnionBenchmarker
	{
		readonly string[] FirstList = { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot" };
		readonly string[] SecondList = { "Alpha", "Charlie", "Echo", "Golf", "Hotel", "India" };

		[Params(100_000)]
		public int N { get; set; }

		[Benchmark(Baseline = true)]
		public IReadOnlyCollection<string> WithLinqUnion() =>
			FirstList.Union(SecondList).ToList();

		[Benchmark]
		public IReadOnlyCollection<string> WithHashSet()
		{
			var set = new HashSet<string>();
			foreach (var item in FirstList)
				set.Add(item);
			foreach (var item in SecondList)
				set.Add(item);
			return set;
		}
	}
}