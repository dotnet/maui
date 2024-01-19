using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class ConverterBenchmarker
	{
		private const int Iterations = 10000;

		[Benchmark]
		public void CreateLotsOfConverters()
		{
			var converters = new List<TypeConverter>();

			for (int i = 0; i < Iterations; i++)
			{
				converters.Add(new PointCollectionConverter());
				converters.Add(new StrokeShapeTypeConverter());
				converters.Add(new TransformTypeConverter());
				converters.Add(new ColumnDefinitionCollectionTypeConverter());
				converters.Add(new RowDefinitionCollectionTypeConverter());
			}
		}
	}
}