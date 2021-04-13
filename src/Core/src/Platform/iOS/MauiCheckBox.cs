using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiCheckBox : UIButton
	{
		// All these values were chosen to just match the android drawables that are used
		const float DefaultSize = 18.0f;
		const float LineWidth = 2.0f;

		static UIImage? Checked;
		static UIImage? Unchecked;

		Color? _tintColor;
		bool _isChecked;
		bool _isEnabled;
		float _minimumViewSize;
		bool _disposed;

		public EventHandler? CheckedChanged;

		internal float MinimumViewSize
		{
			get { return _minimumViewSize; }
			set
			{
				_minimumViewSize = value;
				var xOffset = (value - DefaultSize + LineWidth) / 4;
				ContentEdgeInsets = new UIEdgeInsets(0, xOffset, 0, 0);
			}
		}

		public MauiCheckBox()
		{
			ContentMode = UIViewContentMode.Center;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
			VerticalAlignment = UIControlContentVerticalAlignment.Center;
			AdjustsImageWhenDisabled = false;
			AdjustsImageWhenHighlighted = false;

			TouchUpInside += OnTouchUpInside;
		}

		void OnTouchUpInside(object? sender, EventArgs e)
		{
			IsChecked = !IsChecked;
			CheckedChanged?.Invoke(this, EventArgs.Empty);
		}

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

				_tintColor = value;
				CheckBoxTintUIColor = CheckBoxTintColor?.ToNative();
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

		protected virtual UIImage GetCheckBoximage()
		{
			// Ideally I would use the static images here but when disabled it always tints them grey
			// and I don't know how to make it not tint them gray
			if (!Enabled && CheckBoxTintColor != null)
			{
				if (IsChecked)
					return CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				return CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
			}

			if (Checked == null)
				Checked = CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			if (Unchecked == null)
				Unchecked = CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			return IsChecked ? Checked : Unchecked;
		}

		internal virtual UIBezierPath CreateBoxPath(CGRect backgroundRect) => UIBezierPath.FromOval(backgroundRect);
		internal virtual UIBezierPath CreateCheckPath() => new UIBezierPath
		{
			LineWidth = (nfloat)0.077,
			LineCapStyle = CGLineCap.Round,
			LineJoinStyle = CGLineJoin.Round
		};

		internal virtual void DrawCheckMark(UIBezierPath path)
		{
			path.MoveTo(new CGPoint(0.72f, 0.22f));
			path.AddLineTo(new CGPoint(0.33f, 0.6f));
			path.AddLineTo(new CGPoint(0.15f, 0.42f));
		}

		internal virtual UIImage CreateCheckBox(UIImage? check)
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

		internal UIImage CreateCheckMark()
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
			SetImage(GetCheckBoximage(), UIControlState.Normal);
			SetNeedsDisplay();
		}
	}
}