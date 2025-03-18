using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class TypedBindingBenchmarker
	{
		// Avoids the warning:
		// The minimum observed iteration time is 10.1000 us which is very small. It's recommended to increase it to at least 100.0000 ms using more operations.
		const int Iterations = 10;

		class MyObject : BindableObject
		{
			public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(MyObject));

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
		public void TypedBindName()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new TypedBinding<MyObject, string>(
					o => (o.Name, true),
					null,
					handlers: new[] {
						Tuple.Create<Func<MyObject, object>, string>(o => o, "Name")
					}
				)
				{ Source = Source };
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}

		[Benchmark]
		public void TypedBindChild()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new TypedBinding<MyObject, string>(
					o => (o.Child.Name, true),
					null,
					handlers: new[]
					{
						Tuple.Create<Func<MyObject, object>, string>(o => o, "Child"),
						Tuple.Create<Func<MyObject, object>, string>(o => o.Child, "Name"),
					}
				)
				{ Source = Source };
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}

		[Benchmark]
		public void TypedBindChildIndexer()
		{
			for (int i = 0; i < Iterations; i++)
			{
				var binding = new TypedBinding<MyObject, string>(
					o => (o.Children[0].Name, true),
					null,
					handlers: new[]
					{
						Tuple.Create<Func<MyObject, object>, string>(o => o, "Children"),
						Tuple.Create<Func<MyObject, object>, string>(o => o.Children, "Item[0]"),
						Tuple.Create<Func<MyObject, object>, string>(o => o.Children[0], "Name"),
					}
				)
				{ Source = Source };
				Target.SetBinding(MyObject.NameProperty, binding);
			}
		}
	}
}
