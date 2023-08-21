// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Graphics.Benchmarks
{
	[MemoryDiagnoser]
	public class ColorBenchmarker
	{
		[Benchmark]
		public Color Parse() => Color.Parse("#979797");

		[Benchmark]
		public Color ParseBlack() => Color.Parse("Black");

		[Benchmark]
		public Color ParseLightGoldenrodYellowWithSpace() => Color.Parse(" LightGoldenrodYellow ");
	}
}
