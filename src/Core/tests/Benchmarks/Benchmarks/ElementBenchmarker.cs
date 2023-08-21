// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class ElementBenchmarker
	{
		/// <summary>
		/// Benchmarks setting an Element's property many times.
		/// </summary>
		[Benchmark]
		public bool SetProperty()
		{
			SolidColorBrush brush = new SolidColorBrush(Colors.Red);
			for (int i = 0; i < 10_000; i++)
			{
				brush.Color = Colors.Black;
				brush.Color = Colors.Green;
			}
			return brush.Color == Colors.Green;
		}
	}
}
