#nullable enable
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class SourceGeneratedBindingBenchmarker
	{
		// Avoids the warning:
		// The minimum observed iteration time is 10.1000 us which is very small. It's recommended to increase it to at least 100.0000 ms using more operations.
		const int Iterations = 10;

		public class MyObject : BindableObject
		{
			public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(MyObject));

			public string Name
			{
				get { return (string)GetValue(NameProperty); }
				set { SetValue(NameProperty, value); }
			}

			public MyObject? Child { get; set; }

			public List<MyObject> Children { get; private set; } = new List<MyObject>();
		}

		readonly MyObject Source = new()
		{
			Name = "A",
			Child = new() { Name = "A.Child" },
			Children =
			{
				new() { Name = "A.Children[0]" },
				new() { Name = "A.Children[1]" },
			}
		};
		readonly MyObject Target = new() { Name = "B" };

		[Benchmark]
		public void SourceGeneratedBindName()
		{
			for (int i = 0; i < Iterations; i++)
			{
				Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Name, source: Source, mode: BindingMode.OneWay);
			}
		}

		[Benchmark]
		public void SourceGeneratedBindChild()
		{
			for (int i = 0; i < Iterations; i++)
			{
				Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Child?.Name, source: Source, mode: BindingMode.OneWay);
			}
		}

		[Benchmark]
		public void SourceGeneratedBindChildIndexer()
		{
			for (int i = 0; i < Iterations; i++)
			{
				Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Children[0].Name, source: Source, mode: BindingMode.OneWay);
			}
		}
	}
}
