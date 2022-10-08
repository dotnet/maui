using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using TColor = Tizen.UIExtensions.Common.Color;
using NColor = Tizen.NUI.Color;
using TSize = Tizen.UIExtensions.Common.Size;
using MaterialIcons = Tizen.UIExtensions.Common.GraphicsView.MaterialIcons;
using MaterialIconButton = Tizen.UIExtensions.NUI.GraphicsView.MaterialIconButton;

namespace Microsoft.Maui.Platform
{
	public class MauiStepper : NView, IMeasurable
	{
		StepperButton _less;
		StepperButton _more;
		double _value;
		double _minimum;
		double _maximum;

		public MauiStepper()
		{
			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Horizontal
			};

			_less = new StepperButton { Icon = MaterialIcons.Remove };
			_more = new StepperButton { Icon = MaterialIcons.Add };

			_less.Clicked += OnLessClicked;
			_more.Clicked += OnMoreClicked;

			Add(_less);
			Add(_more);

			Minimum = 0;
			Maximum = 10;
		}

		public event EventHandler? ValueChanged;

		public double Value
		{
			get => _value;
			set
			{
				_value = value.Clamp(Minimum, Maximum);
				ValueChanged?.Invoke(this, EventArgs.Empty);
				UpdateButtonState();
			}
		}

		public double Minimum
		{
			get => _minimum;
			set
			{
				_minimum = value;
				UpdateButtonState();
			}
		}

		public double Maximum
		{
			get => _maximum;
			set
			{
				_maximum = value;
				UpdateButtonState();
			}
		}

		public double Increment { get; set; } = 1;

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return new TSize(Math.Min(200d.ToScaledPixel(), availableWidth), 60d.ToScaledPixel());
		}

		public void UpdateMinimum(IStepper stepper)
		{
			Minimum = stepper.Minimum;
		}

		public void UpdateMaximum(IStepper stepper)
		{
			Maximum = stepper.Maximum;
		}

		public void UpdateIncrement(IStepper stepper)
		{
			Increment = stepper.Interval;
		}

		public void UpdateValue(IStepper stepper)
		{
			if (Value != stepper.Value)
				Value = stepper.Value;
		}

		protected override void OnEnabled(bool enabled)
		{
			base.OnEnabled(enabled);
			if (!enabled)
			{
				_more.IsEnabled = false;
				_less.IsEnabled = false;
			}
			else
			{
				UpdateButtonState();
			}
		}

		void UpdateButtonState()
		{
			if (!IsEnabled)
				return;
			_more.IsEnabled = Value != Maximum;
			_less.IsEnabled = Value != Minimum;
		}

		void OnMoreClicked(object? sender, EventArgs e)
		{
			Value += Increment;
		}

		void OnLessClicked(object? sender, EventArgs e)
		{
			Value -= Increment;
		}

		class StepperButton : MaterialIconButton
		{
			static TColor s_normalBg = TColor.FromHex("#eeeeee");
			static TColor s_disableBg = TColor.FromHex("#e0e0e0");
			static TColor s_pressedBg = TColor.FromHex("#fefefe");
			static double s_margin = 10d;
			static double s_cornerRadius = 10d;

			public StepperButton()
			{
				BackgroundColor = s_normalBg.ToNative();
				HeightSpecification = LayoutParamPolicies.MatchParent;
				WidthSpecification = LayoutParamPolicies.MatchParent;
				Margin = new Extents((ushort)s_margin.ToScaledPixel(), (ushort)s_margin.ToScaledPixel(), (ushort)s_margin.ToScaledPixel(), (ushort)s_margin.ToScaledPixel());
				BorderlineWidth = 1d.ToScaledPixel();
				BorderlineColor = NColor.Black;
				CornerRadius = s_cornerRadius.ToScaledPixel();

				Pressed += OnPressed;
				Released += OnReleased;
				KeyEvent += OnKeyEvent;
			}

			protected override void OnEnabled(bool enabled)
			{
				base.OnEnabled(enabled);
				BackgroundColor = enabled ? s_normalBg.ToNative() : s_disableBg.ToNative();
				Color = enabled ? TColor.Black : TColor.Gray;
			}

			void OnReleased(object? sender, EventArgs e)
			{
				BackgroundColor = s_normalBg.ToNative();
			}

			void OnPressed(object? sender, EventArgs e)
			{
				BackgroundColor = s_pressedBg.ToNative();
			}

			bool OnKeyEvent(object source, KeyEventArgs e)
			{
				if (e.Key.KeyPressedName.IsEnterKey())
				{
					BackgroundColor = e.Key.State == Key.StateType.Down ? s_pressedBg.ToNative() : s_normalBg.ToNative();
				}
				return false;
			}
		}
	}
}