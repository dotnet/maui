using System;
using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Microsoft.Maui.Controls.Platform.iOS;
using MButton = MaterialComponents.Button;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialStepperRenderer : ViewRenderer<Stepper, MaterialStepper>
	{
		ButtonScheme _buttonScheme;

		protected override void Dispose(bool disposing)
		{
			if (Control is MaterialStepper control)
			{
				control.DecrementButton.TouchUpInside -= OnStep;
				control.IncrementButton.TouchUpInside -= OnStep;
			}

			_buttonScheme?.Dispose();
			_buttonScheme = null;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			_buttonScheme?.Dispose();
			_buttonScheme = CreateButtonScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var stepper = CreateNativeControl();
					stepper.DecrementButton.TouchUpInside += OnStep;
					stepper.IncrementButton.TouchUpInside += OnStep;
					SetNativeControl(stepper);
				}

				UpdateButtons();
				ApplyTheme();
			}
		}

		protected virtual ButtonScheme CreateButtonScheme()
		{
			return new ButtonScheme
			{
				ColorScheme = MaterialColors.Light.CreateColorScheme(),
				ShapeScheme = new ShapeScheme(),
				TypographyScheme = new TypographyScheme(),
			};
		}

		protected virtual void ApplyTheme()
		{
			OutlinedButtonThemer.ApplyScheme(_buttonScheme, Control.DecrementButton);
			OutlinedButtonThemer.ApplyScheme(_buttonScheme, Control.IncrementButton);
		}

		protected override MaterialStepper CreateNativeControl()
		{
			return new MaterialStepper();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(Stepper.MinimumProperty, Stepper.MaximumProperty, Stepper.ValueProperty, VisualElement.IsEnabledProperty))
			{
				UpdateButtons();
			}
		}

		protected override void SetBackgroundColor(Color color)
		{
			// Don't call base
		}

		void UpdateButtons()
		{
			if (Element is Stepper stepper && Control is MaterialStepper control)
			{
				control.DecrementButton.Enabled = stepper.IsEnabled && stepper.Value > stepper.Minimum;
				control.IncrementButton.Enabled = stepper.IsEnabled && stepper.Value < stepper.Maximum;
			}
		}

		void OnStep(object sender, EventArgs e)
		{
			if (Element is Stepper stepper && sender is MButton button)
			{
				var increment = stepper.Increment;
				if (button == Control.DecrementButton)
					increment = -increment;

				stepper.SetValueFromRenderer(Stepper.ValueProperty, stepper.Value + increment);
			}
		}
	}

	public class MaterialStepper : UIView
	{
		const int DefaultButtonSpacing = 4;

		public MaterialStepper()
		{
			DecrementButton = new MButton();
			DecrementButton.SetTitle("－", UIControlState.Normal);

			IncrementButton = new MButton();
			IncrementButton.SetTitle("＋", UIControlState.Normal);

			AddSubviews(DecrementButton, IncrementButton);
		}

		public MButton DecrementButton { get; }

		public MButton IncrementButton { get; }

		public override CGSize SizeThatFits(CGSize size)
		{
			var dec = DecrementButton.SizeThatFits(CGSize.Empty);
			var inc = IncrementButton.SizeThatFits(CGSize.Empty);
			var btn = new CGSize(
				Math.Max(dec.Width, inc.Width),
				Math.Max(dec.Height, inc.Height));
			return new CGSize(btn.Width * 2 + DefaultButtonSpacing, btn.Height);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var btn = new CGSize((Bounds.Width - DefaultButtonSpacing) / 2, Bounds.Height);
			DecrementButton.Frame = new CGRect(0, 0, btn.Width, btn.Height);
			IncrementButton.Frame = new CGRect(btn.Width + DefaultButtonSpacing, 0, btn.Width, btn.Height);
		}
	}
}
