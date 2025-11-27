using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Xaml.Benchmarks;

/// <summary>
/// Benchmarks for Issue #9648 fix - ApplyToDerivedTypes with multiple implicit styles
/// Measures performance impact of style merging
/// </summary>
[MemoryDiagnoser]
public class ImplicitStylesBenchmark
{
	private MockApplication _app = null!;

	[GlobalSetup]
	public void Setup()
	{
		_app = new MockApplication();
		Application.Current = _app;
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		Application.Current = null;
	}

	/// <summary>
	/// Baseline: No implicit styles - should have zero overhead
	/// </summary>
	[Benchmark(Baseline = true)]
	public void NoImplicitStyles()
	{
		var page = new ContentPage
		{
			Content = new Entry { Text = "Test" }
		};
	}

	/// <summary>
	/// Single implicit style - should have minimal overhead (early return optimization)
	/// </summary>
	[Benchmark]
	public void SingleImplicitStyle()
	{
		var page = new ContentPage
		{
			Resources = new ResourceDictionary
			{
				new Style(typeof(Entry))
				{
					Setters = { new Setter { Property = Entry.TextColorProperty, Value = Colors.Red } },
					ApplyToDerivedTypes = true
				}
			},
			Content = new Entry { Text = "Test" }
		};
		
	}

	/// <summary>
	/// Two implicit styles in hierarchy - requires merging
	/// Typical case: InputView + Entry
	/// </summary>
	[Benchmark]
	public void TwoImplicitStyles()
	{
		var page = new ContentPage
		{
			Resources = new ResourceDictionary
			{
				new Style(typeof(InputView))
				{
					Setters = { new Setter { Property = InputView.TextColorProperty, Value = Colors.Red } },
					ApplyToDerivedTypes = true
				},
				new Style(typeof(Entry))
				{
					Setters = { new Setter { Property = Entry.FontSizeProperty, Value = 48.0 } },
					ApplyToDerivedTypes = true
				}
			},
			Content = new Entry { Text = "Test" }
		};
		
	}

	/// <summary>
	/// Three implicit styles in hierarchy - maximum typical nesting
	/// InputView + Entry + CustomEntry
	/// </summary>
	[Benchmark]
	public void ThreeImplicitStyles()
	{
		var page = new ContentPage
		{
			Resources = new ResourceDictionary
			{
				new Style(typeof(InputView))
				{
					Setters = { new Setter { Property = InputView.TextColorProperty, Value = Colors.Red } },
					ApplyToDerivedTypes = true
				},
				new Style(typeof(Entry))
				{
					Setters = { new Setter { Property = Entry.FontSizeProperty, Value = 48.0 } },
					ApplyToDerivedTypes = true
				},
				new Style(typeof(CustomEntry))
				{
					Setters = { new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.LightGreen } },
					ApplyToDerivedTypes = true
				}
			},
			Content = new CustomEntry { Text = "Test" }
		};
		
	}

	/// <summary>
	/// Multiple elements with two implicit styles - realistic ListView scenario
	/// </summary>
	[Benchmark]
	public void MultipleElementsWithTwoImplicitStyles()
	{
		var page = new ContentPage
		{
			Resources = new ResourceDictionary
			{
				new Style(typeof(InputView))
				{
					Setters = { new Setter { Property = InputView.TextColorProperty, Value = Colors.Red } },
					ApplyToDerivedTypes = true
				},
				new Style(typeof(Entry))
				{
					Setters = { new Setter { Property = Entry.FontSizeProperty, Value = 48.0 } },
					ApplyToDerivedTypes = true
				}
			},
			Content = new StackLayout
			{
				Children =
				{
					new Entry { Text = "Entry 1" },
					new Entry { Text = "Entry 2" },
					new Entry { Text = "Entry 3" },
					new Entry { Text = "Entry 4" },
					new Entry { Text = "Entry 5" },
					new Entry { Text = "Entry 6" },
					new Entry { Text = "Entry 7" },
					new Entry { Text = "Entry 8" },
					new Entry { Text = "Entry 9" },
					new Entry { Text = "Entry 10" }
				}
			}
		};
		
	}

	/// <summary>
	/// Implicit style with Behaviors and Triggers - tests complete style copying
	/// </summary>
	[Benchmark]
	public void ImplicitStyleWithBehaviorsAndTriggers()
	{
		var page = new ContentPage
		{
			Resources = new ResourceDictionary
			{
				new Style(typeof(InputView))
				{
					Setters = { new Setter { Property = InputView.TextColorProperty, Value = Colors.Red } },
					ApplyToDerivedTypes = true
				},
				new Style(typeof(Entry))
				{
					Setters = 
					{ 
						new Setter { Property = Entry.FontSizeProperty, Value = 48.0 },
						new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.Yellow }
					},
					ApplyToDerivedTypes = true,
					Triggers =
					{
						new Trigger(typeof(Entry))
						{
							Property = Entry.IsFocusedProperty,
							Value = true,
							Setters = { new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.LightBlue } }
						}
					}
				}
			},
			Content = new Entry { Text = "Test" }
		};
		
	}

	/// <summary>
	/// Style with explicit Style property set (no implicit style merging)
	/// </summary>
	[Benchmark]
	public void ExplicitStyleNoImplicit()
	{
		var explicitStyle = new Style(typeof(Entry))
		{
			Setters = { new Setter { Property = Entry.TextColorProperty, Value = Colors.Blue } }
		};

		var page = new ContentPage
		{
			Content = new Entry { Text = "Test", Style = explicitStyle }
		};
		
	}
}

// Helper class for CustomEntry benchmark
public class CustomEntry : Entry
{
}
