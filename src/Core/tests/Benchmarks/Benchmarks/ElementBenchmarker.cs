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

		class MockElement : Element
		{
			public static readonly BindableProperty TextProperty = BindableProperty.Create(
				nameof(Text),
				typeof(string),
				typeof(MockElement));

			public string Text
			{
				get => (string)GetValue(TextProperty);
				set => SetValue(TextProperty, value);
			}
		}

		[Benchmark]
		public void ApplyPropertyViaBindingContextInheritance()
		{
			const int iterations = 100;

			for (int i = 0; i < iterations; i++)
			{
				var bindingContext = "a binding context";

				var root = new MockElement();
				var bindableProperty = MockElement.TextProperty;

				var level1 = new MockElement();
				level1.SetBinding(
					bindableProperty,
					new Binding(".", BindingMode.OneWay));

				var level2 = new MockElement();
				level2.SetBinding(
					bindableProperty,
					new Binding(".", BindingMode.OneWay));

				root.AddLogicalChild(level1);
				level1.AddLogicalChild(level2);

				root.BindingContext = bindingContext;
			}
		}

		[Benchmark]
		public void ApplyBindingContextViaBindingContextInheritance()
		{
			const int iterations = 100;

			var bindingContext = new
			{
				Level1 = new
				{
					Level2 = new
					{
						Text = "a binding context"
					}
				}
			};

			for (int i = 0; i < iterations; i++)
			{
				var root = new MockElement();

				var level1 = new MockElement();
				level1.SetBinding(
					BindableObject.BindingContextProperty,
					new Binding("Level1", BindingMode.OneWay));

				var level2 = new MockElement();
				level2.SetBinding(MockElement.TextProperty, new Binding("Text", BindingMode.OneWay));
				level2.SetBinding(
					BindableObject.BindingContextProperty,
					new Binding("Level2", BindingMode.OneWay));

				root.AddLogicalChild(level1);
				level1.AddLogicalChild(level2);

				root.BindingContext = bindingContext;
			}
		}
	}
}
