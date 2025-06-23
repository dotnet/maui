using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class SetterBenchmarker
	{
		static readonly VisualStateGroupList _vsg = new VisualStateGroupList()
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState { Name = VisualStateManager.CommonStates.Normal },
					new VisualState
					{
						Name = VisualStateManager.CommonStates.Disabled,
						Setters =
						{
							new Setter { Property = VisualElement.OpacityProperty, Value = 0.5 }
						}
					}
				}
			}
		};

		static readonly Style _labelStyle = new Style(typeof(Label))
		{
			Setters =
			{
				new Setter { Property = Label.TextProperty, Value = "Style" },
				new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = _vsg }
			}
		};

		// Avoids the warning:
		// The minimum observed iteration time is 10.1000 us which is very small. It's recommended to increase it to at least 100.0000 ms using more operations.
		const int Iterations = 100;

		[Benchmark]
		public void SettersFromDifferentSources()
		{
			var label = new Label
			{
				Style = _labelStyle
			};

			for (int i = 0; i < Iterations; i++)
			{
				label.Text = "Direct";
				label.IsEnabled = false;
				label.IsEnabled = true;
				label.ClearValue(Label.TextProperty);
			}
		}
	}
}
