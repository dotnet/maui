using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public class ResourceDictionaryBenchmarker
	{
		const int Size = 100;
		readonly ResourceDictionary _resourceDictionary;

		public ResourceDictionaryBenchmarker()
		{
			_resourceDictionary = new ResourceDictionary();
			for (var i = 0; i < Size; i++)
			{
				_resourceDictionary.Add($"key{i}", i);
			}
			
			for (var j = 0; j < Size; j++)
			{
				var merged = new ResourceDictionary();
				for (var i = 0; i < Size; i++)
				{
					merged.Add($"merged{i},{j}", i);
				}
				_resourceDictionary.MergedDictionaries.Add(merged);
			}
		}

		[Benchmark]
		[Arguments("key0")]
		[Arguments("merged50,50")]
		[Arguments("merged99,99")]
		public void TryGetValue(string key) => _resourceDictionary.TryGetValue(key, out _);

		[Benchmark]
		[Arguments("key0")]
		[Arguments("merged50,50")]
		[Arguments("merged99,99")]
		public void Indexer(string key) => _ = _resourceDictionary[key];
	}
}