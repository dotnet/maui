using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WVisualStateManager = Windows.UI.Xaml.VisualStateManager;
using WVisualStateGroup = Windows.UI.Xaml.VisualStateGroup;
using WVisualState = Windows.UI.Xaml.VisualState;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class StepperControl : Control
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnValueChanged));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnMaxMinChanged));

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(StepperControl), new PropertyMetadata(default(double), OnMaxMinChanged));

		public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(StepperControl),
			new PropertyMetadata(default(double), OnIncrementChanged));

		public static readonly DependencyProperty ButtonBackgroundColorProperty = DependencyProperty.Register(nameof(ButtonBackgroundColor), typeof(Color), typeof(StepperControl), new PropertyMetadata(default(Color), OnButtonBackgroundColorChanged));

		Windows.UI.Xaml.Controls.Button _plus;
		Windows.UI.Xaml.Controls.Button _minus;
		VisualStateCache _plusStateCache;
		VisualStateCache _minusStateCache;

		public StepperControl()
		{
			DefaultStyleKey = typeof(StepperControl);
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

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_plus = GetTemplateChild("Plus") as Windows.UI.Xaml.Controls.Button;
			if (_plus != null)
				_plus.Click += OnPlusClicked;

			_minus = GetTemplateChild("Minus") as Windows.UI.Xaml.Controls.Button;
			if (_minus != null)
				_minus.Click += OnMinusClicked;

			UpdateEnabled(Value);
			UpdateButtonBackgroundColor(ButtonBackgroundColor);
		}

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

		VisualStateCache PseudoDisable(Control control)
		{
			if (VisualTreeHelper.GetChildrenCount(control) == 0)
				control.ApplyTemplate();

			WVisualStateManager.GoToState(control, "Disabled", true);

			var rootElement = (FrameworkElement)VisualTreeHelper.GetChild(control, 0);

			var cache = new VisualStateCache();
			IList<WVisualStateGroup> groups = WVisualStateManager.GetVisualStateGroups(rootElement);

			WVisualStateGroup common = null;
			foreach (var group in groups)
			{
				if (group.Name == "CommonStates")
					common = group;
				else if (group.Name == "FocusStates")
					cache.FocusStates = group;
				else if (cache.FocusStates != null && common != null)
					break;
			}

			if (cache.FocusStates != null)
				groups.Remove(cache.FocusStates);

			if (common != null)
			{
				foreach (WVisualState state in common.States)
				{
					if (state.Name == "Normal")
						cache.Normal = state;
					else if (state.Name == "Pressed")
						cache.Pressed = state;
					else if (state.Name == "PointerOver")
						cache.PointerOver = state;
				}

				if (cache.Normal != null)
					common.States.Remove(cache.Normal);
				if (cache.Pressed != null)
					common.States.Remove(cache.Pressed);
				if (cache.PointerOver != null)
					common.States.Remove(cache.PointerOver);
			}

			return cache;
		}

		/*
		The below serves as a way to disable the button visually, rather than using IsEnabled. It's a hack
		but should remain stable as long as the user doesn't change the WinRT Button template too much.

		The reason we're not using IsEnabled is that the buttons have a click radius that overlap about 40%
		of the next button. This doesn't cause a problem until one button becomes disabled, then if you think
		you're still hitting + (haven't noticed its disabled), you will actually hit -. This hack doesn't
		completely solve the problem, but it drops the overlap to something like 20%. I haven't found the root
		cause, so this will have to suffice for now.
		*/

		void PsuedoEnable(Control control, ref VisualStateCache cache)
		{
			if (cache == null || VisualTreeHelper.GetChildrenCount(control) == 0)
				return;

			var rootElement = (FrameworkElement)VisualTreeHelper.GetChild(control, 0);

			IList<WVisualStateGroup> groups = WVisualStateManager.GetVisualStateGroups(rootElement);

			if (cache.FocusStates != null)
				groups.Add(cache.FocusStates);

			var commonStates = groups.FirstOrDefault(g => g.Name == "CommonStates");
			if (commonStates == null)
				return;

			if (cache.Normal != null)
				commonStates.States.Insert(0, cache.Normal); // defensive
			if (cache.Pressed != null)
				commonStates.States.Add(cache.Pressed);
			if (cache.PointerOver != null)
				commonStates.States.Add(cache.PointerOver);

			WVisualStateManager.GoToState(control, "Normal", true);

			cache = null;
		}

		void UpdateButtonBackgroundColor(Color value)
		{
			Brush brush = value.ToBrush();
			_minus = GetTemplateChild("Minus") as Windows.UI.Xaml.Controls.Button;
			_plus = GetTemplateChild("Plus") as Windows.UI.Xaml.Controls.Button;
			if (_minus != null)
				_minus.Background = brush;
			if (_plus != null)
				_plus.Background = brush;
		}

		void UpdateEnabled(double value)
		{
			double increment = Increment;
			if (_plus != null)
			{
				if (value + increment > Maximum)
					_plusStateCache = PseudoDisable(_plus);
				else
					PsuedoEnable(_plus, ref _plusStateCache);
			}

			if (_minus != null)
			{
				if (value - increment < Minimum)
					_minusStateCache = PseudoDisable(_minus);
				else
					PsuedoEnable(_minus, ref _minusStateCache);
			}
		}

		void UpdateValue(double delta)
		{
			double newValue = Value + delta;
			if (newValue > Maximum || newValue < Minimum)
				return;

			Value = newValue;
		}

		class VisualStateCache
		{
			public WVisualStateGroup FocusStates;
			public WVisualState Normal, PointerOver, Pressed;
		}
	}
}