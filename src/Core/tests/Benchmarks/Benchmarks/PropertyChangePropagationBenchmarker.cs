using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Benchmarks
{
	/// <summary>
	/// Benchmarks that measure the time and allocations caused by property-change
	/// propagation on the three most commonly used controls: Label, Button and Entry.
	/// All benchmarks attach a PropertyChanged subscriber to simulate real-world scenarios
	/// where the UI or bindings listen to property changes.
	///
	/// Properties tested:
	///   - HeightRequest (double)
	///   - Background    (Brush – exercises color-to-brush conversion and caching)
	///   - IsEnabled     (bool – coerced through the visual tree, uses boxed value caching)
	///   - Opacity       (double – coerced to [0,1])
	///   - FontSize      (double – Button only)
	///
	/// The Background property is particularly interesting as it exercises implicit Color-to-Brush
	/// conversion, which can benefit from brush instance caching. IsEnabled tests boxed bool reuse.
	/// </summary>
	[MemoryDiagnoser]
	public class PropertyChangePropagationBenchmarker
	{
		// Enough iterations to keep BenchmarkDotNet happy (>100ms per benchmark).
		const int Iterations = 1_000;

		Label _label;
		Button _button;
		Entry _entry;

		// Two alternating values per property type keep the BindableObject from
		// short-circuiting the change via its value-equality check.
		static readonly Color _colorA = Colors.Red;
		static readonly Color _colorB = Colors.Blue;

		[GlobalSetup]
		public void Setup()
		{
			_label = new Label();
			_button = new Button();
			_entry = new Entry();

			// Attach a lightweight subscriber to simulate real-world usage where
			// the UI (or a binding) listens to property changes.
			static void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				
			}

			_label.PropertyChanged += OnPropertyChanged;
			_button.PropertyChanged += OnPropertyChanged;
			_entry.PropertyChanged += OnPropertyChanged;
		}

		// -------------------------------------------------------------------------
		// HeightRequest property (double)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_HeightRequest()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.HeightRequest = 444;
				_label.HeightRequest = 445;
			}
		}

		[Benchmark]
		public void Button_HeightRequest()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.HeightRequest = 444;
				_button.HeightRequest = 445;
			}
		}

		[Benchmark]
		public void Entry_HeightRequest()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.HeightRequest = 444;
				_entry.HeightRequest = 445;
			}
		}

		// -------------------------------------------------------------------------
		// Background property (Color)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetBackground()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.Background = _colorA;
				_label.Background = _colorB;
			}
		}

		[Benchmark]
		public void Button_SetBackground()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.Background = _colorA;
				_button.Background = _colorB;
			}
		}

		[Benchmark]
		public void Entry_SetBackground()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.Background = _colorA;
				_entry.Background = _colorB;
			}
		}

		[Benchmark]
		public void Label_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.HeightRequest = 444;
				_label.Background = _colorA;
				_label.IsEnabled = false;
				_label.Opacity = 0.5;

				_label.HeightRequest = 445;
				_label.Background = _colorB;
				_label.IsEnabled = true;
				_label.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void Button_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.HeightRequest = 444;
				_button.Background = _colorA;
				_button.FontSize = 14;
				_button.Opacity = 0.5;

				_button.HeightRequest = 445;
				_button.Background = _colorB;
				_button.IsEnabled = true;
				_button.Opacity = 1.0;
				
				#if DEBUG
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				#endif
			}
		}

		[Benchmark]
		public void Entry_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.HeightRequest = 444;
				_entry.Background = _colorA;
				_entry.IsEnabled = false;
				_entry.Opacity = 0.5;

				_entry.HeightRequest = 445;
				_entry.Background = _colorB;
				_entry.IsEnabled = true;
				_entry.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void SetSameValueOnDifferentControls()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.Background = _colorA;
				_button.Background = _colorA;
				_label.Background = _colorA;


				_entry.Background = null;
				_button.Background =  null;
				_label.Background = null;
			}
		}
	}
}
