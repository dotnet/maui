using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Benchmarks
{
	/// <summary>
	/// Benchmarks that measure the time and allocations caused by property-change
	/// propagation on the three most commonly used text-based controls: Label, Button
	/// and Entry.  Only properties that exist on all three controls are exercised so
	/// the numbers are directly comparable.
	///
	/// Common properties tested:
	///   - Text          (string)
	///   - TextColor     (Color)
	///   - FontSize      (double – triggers InvalidateMeasure internally)
	///   - FontAttributes (enum)
	///   - IsEnabled     (bool – coerced through the visual tree)
	///   - Opacity       (double – coerced to [0,1])
	///
	/// Each benchmark group is run both without and with a PropertyChanged subscriber
	/// so you can isolate the cost of the notification-dispatch leg.
	/// </summary>
	[MemoryDiagnoser]
	public class PropertyChangePropagationBenchmarker
	{
		// Enough iterations to keep BenchmarkDotNet happy (>100ms per benchmark).
		const int Iterations = 1_000;

		// Pre-allocate controls outside the benchmark methods so construction cost
		// is excluded and only the property-change propagation is measured.
		Label _label;
		Button _button;
		Entry _entry;

		Label _labelWithSubscriber;
		Button _buttonWithSubscriber;
		Entry _entryWithSubscriber;

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

			_labelWithSubscriber = new Label();
			_buttonWithSubscriber = new Button();
			_entryWithSubscriber = new Entry();

			// Attach a lightweight subscriber to simulate real-world usage where
			// the UI (or a binding) listens to property changes.
			static void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { }

			_labelWithSubscriber.PropertyChanged += OnPropertyChanged;
			_buttonWithSubscriber.PropertyChanged += OnPropertyChanged;
			_entryWithSubscriber.PropertyChanged += OnPropertyChanged;
		}

		// -------------------------------------------------------------------------
		// Text property (string)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetText()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.Text = "Hello World A";
				_label.Text = "Hello World B";
			}
		}

		[Benchmark]
		public void Button_SetText()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.Text = "Hello World A";
				_button.Text = "Hello World B";
			}
		}

		[Benchmark]
		public void Entry_SetText()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.Text = "Hello World A";
				_entry.Text = "Hello World B";
			}
		}

		// -------------------------------------------------------------------------
		// TextColor property (Color)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetTextColor()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.TextColor = _colorA;
				_label.TextColor = _colorB;
			}
		}

		[Benchmark]
		public void Button_SetTextColor()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.TextColor = _colorA;
				_button.TextColor = _colorB;
			}
		}

		[Benchmark]
		public void Entry_SetTextColor()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.TextColor = _colorA;
				_entry.TextColor = _colorB;
			}
		}

		// -------------------------------------------------------------------------
		// FontSize property (double – triggers InvalidateMeasure on Button/Label)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetFontSize()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.FontSize = 16;
				_label.FontSize = 18;
			}
		}

		[Benchmark]
		public void Button_SetFontSize()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.FontSize = 16;
				_button.FontSize = 18;
			}
		}

		[Benchmark]
		public void Entry_SetFontSize()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.FontSize = 16;
				_entry.FontSize = 18;
			}
		}

		// -------------------------------------------------------------------------
		// Multiple common properties in one pass (composite benchmark)
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetCommonProperties()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_label.Text = "A";
				_label.TextColor = _colorA;
				_label.FontSize = 14;
				_label.FontAttributes = FontAttributes.Bold;
				_label.IsEnabled = false;
				_label.Opacity = 0.5;

				_label.Text = "B";
				_label.TextColor = _colorB;
				_label.FontSize = 16;
				_label.FontAttributes = FontAttributes.None;
				_label.IsEnabled = true;
				_label.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void Button_SetCommonProperties()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_button.Text = "A";
				_button.TextColor = _colorA;
				_button.FontSize = 14;
				_button.FontAttributes = FontAttributes.Bold;
				_button.IsEnabled = false;
				_button.Opacity = 0.5;

				_button.Text = "B";
				_button.TextColor = _colorB;
				_button.FontSize = 16;
				_button.FontAttributes = FontAttributes.None;
				_button.IsEnabled = true;
				_button.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void Entry_SetCommonProperties()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entry.Text = "A";
				_entry.TextColor = _colorA;
				_entry.FontSize = 14;
				_entry.FontAttributes = FontAttributes.Bold;
				_entry.IsEnabled = false;
				_entry.Opacity = 0.5;

				_entry.Text = "B";
				_entry.TextColor = _colorB;
				_entry.FontSize = 16;
				_entry.FontAttributes = FontAttributes.None;
				_entry.IsEnabled = true;
				_entry.Opacity = 1.0;
			}
		}

		// -------------------------------------------------------------------------
		// Same composite benchmark – with a PropertyChanged subscriber attached.
		// Compares the propagation overhead vs. the no-subscriber variants above.
		// -------------------------------------------------------------------------

		[Benchmark]
		public void Label_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_labelWithSubscriber.Text = "A";
				_labelWithSubscriber.TextColor = _colorA;
				_labelWithSubscriber.FontSize = 14;
				_labelWithSubscriber.FontAttributes = FontAttributes.Bold;
				_labelWithSubscriber.IsEnabled = false;
				_labelWithSubscriber.Opacity = 0.5;

				_labelWithSubscriber.Text = "B";
				_labelWithSubscriber.TextColor = _colorB;
				_labelWithSubscriber.FontSize = 16;
				_labelWithSubscriber.FontAttributes = FontAttributes.None;
				_labelWithSubscriber.IsEnabled = true;
				_labelWithSubscriber.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void Button_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_buttonWithSubscriber.Text = "A";
				_buttonWithSubscriber.TextColor = _colorA;
				_buttonWithSubscriber.FontSize = 14;
				_buttonWithSubscriber.FontAttributes = FontAttributes.Bold;
				_buttonWithSubscriber.IsEnabled = false;
				_buttonWithSubscriber.Opacity = 0.5;

				_buttonWithSubscriber.Text = "B";
				_buttonWithSubscriber.TextColor = _colorB;
				_buttonWithSubscriber.FontSize = 16;
				_buttonWithSubscriber.FontAttributes = FontAttributes.None;
				_buttonWithSubscriber.IsEnabled = true;
				_buttonWithSubscriber.Opacity = 1.0;
			}
		}

		[Benchmark]
		public void Entry_SetCommonProperties_WithSubscriber()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_entryWithSubscriber.Text = "A";
				_entryWithSubscriber.TextColor = _colorA;
				_entryWithSubscriber.FontSize = 14;
				_entryWithSubscriber.FontAttributes = FontAttributes.Bold;
				_entryWithSubscriber.IsEnabled = false;
				_entryWithSubscriber.Opacity = 0.5;

				_entryWithSubscriber.Text = "B";
				_entryWithSubscriber.TextColor = _colorB;
				_entryWithSubscriber.FontSize = 16;
				_entryWithSubscriber.FontAttributes = FontAttributes.None;
				_entryWithSubscriber.IsEnabled = true;
				_entryWithSubscriber.Opacity = 1.0;
			}
		}
	}
}
