using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.WinRT
{
	public sealed partial class StepperControl : UserControl
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnValueChanged));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnMaxMinChanged));

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnMaxMinChanged));

		public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(StepperControl),
			new PropertyMetadata(default(double), OnIncrementChanged));

		public static readonly DependencyProperty ButtonBackgroundColorProperty = DependencyProperty.Register(nameof(ButtonBackgroundColor), typeof(Color), typeof(StepperControl), new PropertyMetadata(default(Color), OnButtonBackgroundColorChanged));

		public StepperControl()
		{
			InitializeComponent();
		}

		public double Increment
		{
			get { return (double)GetValue(IncrementProperty); }
			set { SetValue(IncrementProperty, value); }
		}

		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public Color ButtonBackgroundColor
		{
			get { return (Color)GetValue(ButtonBackgroundColorProperty); }
			set { SetValue(ButtonBackgroundColorProperty, value); }
		}

		public event EventHandler ValueChanged;

		static void OnButtonBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var stepper = (StepperControl)d;
			stepper.UpdateButtonBackgroundColor(stepper.ButtonBackgroundColor);
		}

		static void OnIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var stepper = (StepperControl)d;
			stepper.UpdateEnabled(stepper.Value);
		}

		static void OnMaxMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var stepper = (StepperControl)d;
			stepper.UpdateEnabled(stepper.Value);
		}

		void OnMinusClicked(object sender, RoutedEventArgs e)
		{
			UpdateValue(-Increment);
		}

		void OnPlusClicked(object sender, RoutedEventArgs e)
		{
			UpdateValue(+Increment);
		}

		static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var stepper = (StepperControl)d;
			stepper.UpdateEnabled((double)e.NewValue);

			EventHandler changed = stepper.ValueChanged;
			if (changed != null)
				changed(d, EventArgs.Empty);
		}

		void UpdateButtonBackgroundColor(Color value)
		{
			Windows.UI.Xaml.Media.Brush brush = value.ToBrush();
			Minus.Background = brush;
			Plus.Background = brush;
		}

		void UpdateEnabled(double value)
		{
			double increment = Increment;
			Plus.IsEnabled = value + increment <= Maximum;
			Minus.IsEnabled = value - increment >= Minimum;
		}

		void UpdateValue(double delta)
		{
			double newValue = Value + delta;
			Value = newValue;
		}
	}
}