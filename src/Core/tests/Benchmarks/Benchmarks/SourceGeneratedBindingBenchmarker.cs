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
			Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Name, source: Source, mode: BindingMode.OneWay);
		}

		[Benchmark]
		public void SourceGeneratedBindChild()
		{
			Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Child?.Name, source: Source, mode: BindingMode.OneWay);
		}

		[Benchmark]
		public void SourceGeneratedBindChildIndexer()
		{
			Target.SetBinding(MyObject.NameProperty, static (MyObject o) => o.Children[0].Name, source: Source, mode: BindingMode.OneWay);
		}
	}
}
