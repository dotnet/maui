using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Benchmarks;

[MemoryDiagnoser]
public class StyleBenchmarker
{
	Style buttonStyle;
	Style labelStyle;

	[GlobalSetup]
	public void Setup()
	{
		buttonStyle = new Style(typeof(Button))
		{
			Setters = {
				new Setter { Property = Button.TextColorProperty, Value = Colors.Pink },
			},
			Class = "pink",
			ApplyToDerivedTypes = true,
		};
		labelStyle = new Style(typeof(Label))
		{
			Setters = {
				new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Pink },
			},
			Class = "pink",
			ApplyToDerivedTypes = false,
		};
	}

	[Benchmark]
	public void MergedStyle()
	{	
		var button = new Button
		{
			StyleClass = new[] { "pink" },
		};
		var label = new Label
		{
			StyleClass = new[] { "pink" },
		};

		var cv = new ContentView
		{
			Resources = new ResourceDictionary { buttonStyle },
			Content = new StackLayout
			{
				Resources = new ResourceDictionary { labelStyle },
					Children = {
					button,
					label,
				}
			}
		};
	}
}
