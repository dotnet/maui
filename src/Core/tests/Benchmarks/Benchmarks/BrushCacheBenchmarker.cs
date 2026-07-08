using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers.Benchmarks;

[MemoryDiagnoser]
public class FortyColorBrushCacheBenchmarks
{
	private const int ColorCount = 40;
	private static readonly Color[] Colors = BrushCacheBenchmarkData.CreateColors(ColorCount);

	[Benchmark(Baseline = true, OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object LruCache()
		=> BrushCacheBenchmarkData.Run(new LRUBrushCache(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object SwitchCache()
		=> BrushCacheBenchmarkData.Run(new CacheWithSwitch(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object NullCache()
		=> BrushCacheBenchmarkData.Run(new NullCache(), Colors);
}

[MemoryDiagnoser]
public class SixtyColorBrushCacheBenchmarks
{
	private const int ColorCount = 60;
	private static readonly Color[] Colors = BrushCacheBenchmarkData.CreateColors(ColorCount);

	[Benchmark(Baseline = true, OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object LruCache()
		=> BrushCacheBenchmarkData.Run(new LRUBrushCache(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object SwitchCache()
		=> BrushCacheBenchmarkData.Run(new CacheWithSwitch(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = ColorCount * BrushCacheBenchmarkData.LoopCount)]
	public object NullCache()
		=> BrushCacheBenchmarkData.Run(new NullCache(), Colors);
}

[MemoryDiagnoser]
public class WeightedColorBrushCacheBenchmarks
{
	private const int CommonColorCount = 20;
	private const int ColdColorCount = 40;
	private const int CommonColorWeight = 10;
	private const int AccessCount = CommonColorCount * CommonColorWeight + ColdColorCount;

	private static readonly Color[] Colors = BrushCacheBenchmarkData.CreateWeightedColors(
		CommonColorCount, ColdColorCount, CommonColorWeight);

	[Benchmark(Baseline = true, OperationsPerInvoke = AccessCount * BrushCacheBenchmarkData.LoopCount)]
	public object LruCache()
		=> BrushCacheBenchmarkData.Run(new LRUBrushCache(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = AccessCount * BrushCacheBenchmarkData.LoopCount)]
	public object SwitchCache()
		=> BrushCacheBenchmarkData.Run(new CacheWithSwitch(BrushCacheBenchmarkData.CacheCapacity), Colors);

	[Benchmark(OperationsPerInvoke = AccessCount * BrushCacheBenchmarkData.LoopCount)]
	public object NullCache()
		=> BrushCacheBenchmarkData.Run(new NullCache(), Colors);
}

internal static class BrushCacheBenchmarkData
{
	public const int CacheCapacity = 50;
	public const int LoopCount = 10;

	public static ImmutableBrush Run(ICache<Color, ImmutableBrush> cache, Color[] colors)
	{
		ImmutableBrush lastBrush = null;
		for (var iteration = 0; iteration < LoopCount; iteration++)
			foreach (var color in colors)
				lastBrush = cache.Get(color);
		return lastBrush;
	}

	public static Color[] CreateColors(int count)
	{
		var colors = new Color[count];
		for (var i = 0; i < colors.Length; i++)
			colors[i] = new Color(
				red: ((i * 67) % 256) / 255f,
				green: ((i * 97) % 256) / 255f,
				blue: ((i * 131) % 256) / 255f,
				alpha: 1.0f);
		return colors;
	}

	public static Color[] CreateWeightedColors(int commonColorCount, int coldColorCount, int commonColorWeight)
	{
		var uniqueColors = CreateColors(commonColorCount + coldColorCount);
		var accesses = new Color[commonColorCount * commonColorWeight + coldColorCount];
		var index = 0;

		for (var colorIndex = 0; colorIndex < commonColorCount; colorIndex++)
			for (var repeat = 0; repeat < commonColorWeight; repeat++)
				accesses[index++] = uniqueColors[colorIndex];

		for (var colorIndex = commonColorCount; colorIndex < uniqueColors.Length; colorIndex++)
			accesses[index++] = uniqueColors[colorIndex];

		Shuffle(accesses, seed: 42);
		return accesses;
	}

	static void Shuffle(Color[] colors, int seed)
	{
		var random = new Random(seed);
		for (var i = colors.Length - 1; i > 0; i--)
		{
			var j = random.Next(i + 1);
			(colors[i], colors[j]) = (colors[j], colors[i]);
		}
	}
}

internal sealed class NullCache : ICache<Color, ImmutableBrush>
{
	public ImmutableBrush Get(Color key) => new ImmutableBrush(key);
}
