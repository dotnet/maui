using System;
using System.Collections.Generic;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Benchmarks
{
	public abstract class BindingBenchmarkBase
	{
		protected class MyObject : BindableObject
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

		protected readonly MyObject Source = new()
		{
			Name = "A",
			Child = new() { Name = "A.Child" },
			Children =
			{
				new() { Name = "A.Children[0]" },
				new() { Name = "A.Children[1]" },
			}
		};

		protected  readonly MyObject Target = new() { Name = "B" };
	}

	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_Name : BindingBenchmarkBase
	{
		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Name", source: Source);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
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

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				o => (o.Name, true),
				null,
				handlersCount: 1,
				handlers: GetHandlers
			)
			{ Source = Source };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}
	}

	
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_Child : BindingBenchmarkBase
	{
		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Child.Name", source: Source);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
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

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				o => (o.Child.Name, true),
				null,
				handlersCount: 2,
				handlers: GetHandlers
			)
			{ Source = Source };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Child");
				var x1 = x0.Child;
				yield return (x1, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}

	}

	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_ChildIndexed : BindingBenchmarkBase
	{

		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Children[0].Name", source: Source);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
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

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				o => (o.Children[0].Name, true),
				null,
				handlersCount: 2,
				handlers: GetHandlers
			)
			{ Source = Source };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Children");
				var x1 = x0.Children;
				// yield return (x1, "Item[0]"); -- not INotifyPropertyChanged
				var x2 = x1[0];
				yield return (x2, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}
	}

	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_NameTwoWay : BindingBenchmarkBase
	{
		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Name", source: Source, mode: BindingMode.TwoWay);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Name, true),
				static (o, v) => o.Name = v,
				handlers: new[] {
					Tuple.Create<Func<MyObject, object>, string>(static o => o, "Name")
				}
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Name, true),
				static (o, v) => o.Name = v,
				handlersCount: 1,
				handlers: GetHandlers
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}
	}

	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_ChildTwoWay : BindingBenchmarkBase
	{
		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Child.Name", source: Source, mode: BindingMode.TwoWay);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Child.Name, true),
				static (o, v) => o.Child.Name = v,
				handlers: new[]
				{
					Tuple.Create<Func<MyObject, object>, string>(static o => o, "Child"),
					Tuple.Create<Func<MyObject, object>, string>(static o => o.Child, "Name"),
				}
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Child.Name, true),
				static (o, v) => o.Child.Name = v,
				handlersCount: 2,
				handlers: GetHandlers
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Child");
				var x1 = x0.Child;
				yield return (x1, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}
	}

	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	public class BindingBenchmark_ChildIndexedTwoWay : BindingBenchmarkBase
	{
		[Benchmark]
		public void Binding()
		{
			var binding = new Binding("Children[0].Name", source: Source, mode: BindingMode.TwoWay);
			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark(Baseline = true)]
		public void TypedBinding()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Children[0].Name, true),
				static (o, v) => o.Children[0].Name = v,
				handlers: new[]
				{
					Tuple.Create<Func<MyObject, object>, string>(static o => o, "Children"),
					Tuple.Create<Func<MyObject, object>, string>(static o => o.Children, "Item[0]"),
					Tuple.Create<Func<MyObject, object>, string>(static o => o.Children[0], "Name"),
				}
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			Target.SetBinding(MyObject.NameProperty, binding);
		}

		[Benchmark]
		public void TypedBinding2()
		{
			var binding = new TypedBinding<MyObject, string>(
				static o => (o.Children[0].Name, true),
				static (o, v) => o.Children[0].Name = v,
				handlersCount: 2,
				handlers: GetHandlers
			)
			{ Source = Source, Mode = BindingMode.TwoWay };

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(MyObject o)
			{
				var x0 = o;
				yield return (x0, "Children");
				var x1 = x0.Children;
				// yield return (x1, "Item[0]"); -- not INotifyPropertyChanged
				var x2 = x1[0];
				yield return (x2, "Name");
			}

			Target.SetBinding(MyObject.NameProperty, binding);
		}
	}
}
