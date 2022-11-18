#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WVisualState = Microsoft.UI.Xaml.VisualState;
using WVisualStateGroup = Microsoft.UI.Xaml.VisualStateGroup;
using WVisualStateManager = Microsoft.UI.Xaml.VisualStateManager;

namespace Microsoft.Maui.Platform
{
	public class MauiStepper : Control
	{
		Button _plus;
		Button _minus;
		VisualStateCache _plusStateCache;
		VisualStateCache _minusStateCache;
		double _increment;
		double _value;
		double _maximum;
		double _minimum;
		Color _buttonBackgroundColor;
		WBrush _buttonBackground;

		public MauiStepper()
		{
			DefaultStyleKey = typeof(MauiStepper);
		}

		public double Increment
		{
			get => _increment;
			set
			{
				_increment = value;
				UpdateEnabled(Value);
			}
		}

		public double Maximum
		{
			get => _maximum;
			set
			{
				_maximum = value;
				UpdateEnabled(Value);
			}
		}

		public double Minimum
		{
			get => _minimum;
			set
			{
				_minimum = value;
				UpdateEnabled(Value);
			}
		}

		public double Value
		{
			get => _value;
			set
			{
				_value = value;
				UpdateEnabled(value);
				ValueChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public Color ButtonBackgroundColor
		{
			get => _buttonBackgroundColor;
			set
			{
				_buttonBackgroundColor = value;
				UpdateButtonBackgroundColor(value);
			}
		}

		public WBrush ButtonBackground
		{
			get => _buttonBackground;
			set
			{
				_buttonBackground = value;
				UpdateButtonBackground();
			}
		}

		public event EventHandler ValueChanged;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_plus = GetTemplateChild("Plus") as Button;
			if (_plus != null)
				_plus.Click += OnPlusClicked;

			_minus = GetTemplateChild("Minus") as Button;
			if (_minus != null)
				_minus.Click += OnMinusClicked;

			UpdateEnabled(Value);
			UpdateButtonBackground();
		}

		void OnMinusClicked(object sender, RoutedEventArgs e)
		{
			UpdateValue(-Increment);
		}

		void OnPlusClicked(object sender, RoutedEventArgs e)
		{
			UpdateValue(+Increment);
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

		void UpdateButtonBackground()
		{
			if (_minus != null)
				_minus.Background = ButtonBackground;
			if (_plus != null)
				_plus.Background = ButtonBackground;
		}

		void UpdateButtonBackgroundColor(Color value)
		{
			if (value == null)
			{
				return;
			}

			ButtonBackground = value.ToPlatform();
			UpdateButtonBackground();
		}

		void UpdateEnabled(double value)
		{
			double increment = Increment;
			if (_plus != null)
			{
				if (value + increment > Maximum && _plusStateCache is null)
					_plusStateCache = PseudoDisable(_plus);
				else if (value + increment <= Maximum)
					PsuedoEnable(_plus, ref _plusStateCache);
			}

			if (_minus != null)
			{
				if (value - increment < Minimum && _minusStateCache is null)
					_minusStateCache = PseudoDisable(_minus);
				else if (value - increment >= Minimum)
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