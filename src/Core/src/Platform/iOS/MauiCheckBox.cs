using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiCheckBox : UIButton
	{
		// All these values were chosen to just match the android drawables that are used
		const float DefaultSize = 18.0f;
		const float LineWidth = 2.0f;

		static UIImage? Checked;
		static UIImage? Unchecked;
		UIImage? CheckedDisabledAndTinted;
		UIImage? UncheckedDisabledAndTinted;

		UIAccessibilityTrait _accessibilityTraits;

		Color? _tintColor;
		bool _isChecked;
		bool _isEnabled;
		bool _disposed;

		public EventHandler? CheckedChanged;

		public MauiCheckBox()
		{
			ContentMode = UIViewContentMode.Center;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			VerticalAlignment = UIControlContentVerticalAlignment.Center;
#pragma warning disable CA1416 // TODO: both has [UnsupportedOSPlatform("ios15.0")]
#pragma warning disable CA1422 // Validate platform compatibility
			AdjustsImageWhenDisabled = false;
			AdjustsImageWhenHighlighted = false;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
			TouchUpInside += OnTouchUpInside;
		}

		void OnTouchUpInside(object? sender, EventArgs e)
		{
			IsChecked = !IsChecked;
			CheckedChanged?.Invoke(this, EventArgs.Empty);
		}

		internal float MinimumViewSize { get; set; }

		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				if (value == _isChecked)
					return;

				_isChecked = value;
				UpdateDisplay();
			}
		}

		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				if (value == _isEnabled)
					return;

				_isEnabled = value;
				UserInteractionEnabled = IsEnabled;
				UpdateDisplay();
			}
		}

		public Color? CheckBoxTintColor
		{
			get => _tintColor;
			set
			{
				if (_tintColor == value)
					return;

				CheckedDisabledAndTinted = null;
				UncheckedDisabledAndTinted = null;
				_tintColor = value;
				CheckBoxTintUIColor = CheckBoxTintColor?.ToPlatform();
			}
		}

		UIColor? _checkBoxTintUIColor;
		UIColor? CheckBoxTintUIColor
		{
			get
			{
				return _checkBoxTintUIColor ?? UIColor.White;
			}
			set
			{
				if (value == _checkBoxTintUIColor)
					return;

				_checkBoxTintUIColor = value;
				ImageView.TintColor = value;
				TintColor = value;

				if (Enabled)
					SetNeedsDisplay();
				else
					UpdateDisplay();
			}
		}

		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}

			set
			{
				bool changed = base.Enabled != value;
				base.Enabled = value;

				if (changed)
					UpdateDisplay();
			}
		}

		protected virtual UIImage GetCheckBoxImage()
		{
			// Ideally I would use the static images here but when disabled it always tints them grey
			// and I don't know how to make it not tint them gray
			if (!Enabled && CheckBoxTintColor != null)
			{
				if (IsChecked)
				{
					return CheckedDisabledAndTinted ??=
						CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
				}

				return UncheckedDisabledAndTinted ??=
					CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
			}

			Checked ??= CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
			Unchecked ??= CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			return IsChecked ? Checked : Unchecked;
		}

		UIBezierPath CreateBoxPath(CGRect backgroundRect) => UIBezierPath.FromOval(backgroundRect);
		UIBezierPath CreateCheckPath() => new UIBezierPath
		{
			LineWidth = (nfloat)0.077,
			LineCapStyle = CGLineCap.Round,
			LineJoinStyle = CGLineJoin.Round
		};

		void DrawCheckMark(UIBezierPath path)
		{
			path.MoveTo(new CGPoint(0.72f, 0.22f));
			path.AddLineTo(new CGPoint(0.33f, 0.6f));
			path.AddLineTo(new CGPoint(0.15f, 0.42f));
		}

		UIImage CreateCheckBox(UIImage? check)
		{
			UIGraphics.BeginImageContextWithOptions(new CGSize(DefaultSize, DefaultSize), false, 0);
			var context = UIGraphics.GetCurrentContext();
			context.SaveState();

			var checkedColor = CheckBoxTintUIColor;

			if (checkedColor != null)
			{
				checkedColor.SetFill();
				checkedColor.SetStroke();
			}

			var vPadding = LineWidth / 2;
			var hPadding = LineWidth / 2;
			var diameter = DefaultSize - LineWidth;

			var backgroundRect = new CGRect(hPadding, vPadding, diameter, diameter);
			var boxPath = CreateBoxPath(backgroundRect);
			boxPath.LineWidth = LineWidth;
			boxPath.Stroke();

			if (check != null)
			{
				boxPath.Fill();
				check.Draw(new CGPoint(0, 0), CGBlendMode.DestinationOut, 1);
			}

			context.RestoreState();
			var img = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return img;
		}

		UIImage CreateCheckMark()
		{
			UIGraphics.BeginImageContextWithOptions(new CGSize(DefaultSize, DefaultSize), false, 0);
			var context = UIGraphics.GetCurrentContext();
			context.SaveState();

			var vPadding = LineWidth / 2;
			var hPadding = LineWidth / 2;
			var diameter = DefaultSize - LineWidth;

			var checkPath = CreateCheckPath();

			context.TranslateCTM(hPadding + (nfloat)(0.05 * diameter), vPadding + (nfloat)(0.1 * diameter));
			context.ScaleCTM(diameter, diameter);
			DrawCheckMark(checkPath);
			UIColor.White.SetStroke();
			checkPath.Stroke();

			context.RestoreState();
			var img = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return img;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var result = base.SizeThatFits(size);
			var height = Math.Max(MinimumViewSize, result.Height);
			var width = Math.Max(MinimumViewSize, result.Width);
			var final = Math.Min(width, height);

			return new CGSize(final, final);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			UpdateDisplay();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
				TouchUpInside -= OnTouchUpInside;

			base.Dispose(disposing);
		}

		void UpdateDisplay()
		{
			SetImage(GetCheckBoxImage(), UIControlState.Normal);
			SetNeedsDisplay();
		}

		static UIKit.UIAccessibilityTrait? s_switchAccessibilityTraits;
		UIKit.UIAccessibilityTrait SwitchAccessibilityTraits
		{
			get
			{
				// Accessibility Traits are none if VO is off
				// So we return None until we detect that it's been turned on
				if (base.AccessibilityTraits == UIAccessibilityTrait.None)
					return UIAccessibilityTrait.None;

				if (s_switchAccessibilityTraits == null ||
					s_switchAccessibilityTraits == UIKit.UIAccessibilityTrait.None)
				{
					s_switchAccessibilityTraits = new UIKit.UISwitch().AccessibilityTraits;
				}

				return s_switchAccessibilityTraits ?? UIKit.UIAccessibilityTrait.None;
			}
		}

		public override UIAccessibilityTrait AccessibilityTraits
		{
			get => _accessibilityTraits |= SwitchAccessibilityTraits;
			set => _accessibilityTraits = value | SwitchAccessibilityTraits;
		}

		public override string? AccessibilityValue
		{
			get => (IsChecked) ? "1" : "0";
			set { }
		}
	}
}