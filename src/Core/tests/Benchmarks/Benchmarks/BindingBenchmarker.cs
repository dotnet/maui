using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class BindingBenchmarker
	{
		// Avoids the warning:
		// The minimum observed iteration time is 10.1000 us which is very small. It's recommended to increase it to at least 100.0000 ms using more operations.
		const int Iterations = 10;

		class MyObject : BindableObject
		{
			public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(MyObject), default(string));

			public string Name
			{
				get { return (string)GetValue(NameProperty); }
				set { SetValue(NameProperty, value); }
			}

			public MyObject Child { get; set; }

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
		public void BindName()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new Binding("Name", source: Source);
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}

		[Benchmark]
		public void BindChild()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new Binding("Child.Name", source: Source);
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}

		[Benchmark]
		public void BindChildIndexer()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new Binding("Children[0].Name", source: Source);
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}
	}
}
