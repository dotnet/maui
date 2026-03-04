using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[PortHandler("NativeCheckBox")]
	public class FormsCheckBox : UIButton
	{
		static UIImage _checked;
		static UIImage _unchecked;

		// all these values were chosen to just match the android drawables that are used
		const float _defaultSize = 18.0f;
		const float _lineWidth = 2.0f;
		Color _tintColor;
		bool _isChecked;
		bool _isEnabled;
		float _minimumViewSize;
		public EventHandler CheckedChanged;
		bool _disposed;

#pragma warning disable CA1416, CA1422 // TODO: ContentEdgeInsets, AdjustsImageWhenDisabled, AdjustsImageWhenHighlighted unsupported from version 15.0
		internal float MinimumViewSize
		{
			get { return _minimumViewSize; }
			set
			{
				_minimumViewSize = value;
				var xOffset = (value - _defaultSize + _lineWidth) / 4;
				ContentEdgeInsets = new UIEdgeInsets(0, xOffset, 0, 0);
			}
		}

		public FormsCheckBox()
		{
			TouchUpInside += OnTouchUpInside;
			ContentMode = UIViewContentMode.Center;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
			VerticalAlignment = UIControlContentVerticalAlignment.Center;
			AdjustsImageWhenDisabled = false;
			AdjustsImageWhenHighlighted = false;
		}
#pragma warning restore CA1416, CA1422

		void OnTouchUpInside(object sender, EventArgs e)
		{
			IsChecked = !IsChecked;
			CheckedChanged?.Invoke(this, null);
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

		public Color CheckBoxTintColor
		{
			get => _tintColor;
			set
			{
				if (_tintColor == value)
					return;

				_tintColor = value;
				CheckBoxTintUIColor = CheckBoxTintColor?.ToPlatform();
			}
		}

		UIColor _checkBoxTintUIColor;
		UIColor CheckBoxTintUIColor
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

			if (_checked == null)
				_checked = CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			if (_unchecked == null)
				_unchecked = CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			return IsChecked ? _checked : _unchecked;
		}

		internal void UpdateDisplay()
		{
			SetImage(GetCheckBoximage(), UIControlState.Normal);
			SetNeedsDisplay();
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

		internal virtual UIImage CreateCheckBox(UIImage check)
		{
			var renderer = new UIGraphicsImageRenderer(new CGSize(_defaultSize, _defaultSize), new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = 0,
			});

			return renderer.CreateImage((context) =>
			{
				context.CGContext.SaveState();

				var checkedColor = CheckBoxTintUIColor;
				checkedColor.SetFill();
				checkedColor.SetStroke();

				var vPadding = _lineWidth / 2;
				var hPadding = _lineWidth / 2;
				var diameter = _defaultSize - _lineWidth;

				var backgroundRect = new CGRect(hPadding, vPadding, diameter, diameter);
				var boxPath = CreateBoxPath(backgroundRect);

				boxPath.LineWidth = _lineWidth;
				boxPath.Stroke();

				if (check is not null)
				{
					boxPath.Fill();
					check.Draw(new CGPoint(0, 0), CGBlendMode.DestinationOut, 1);
				}

				context.CGContext.RestoreState();
			});
		}


		internal UIImage CreateCheckMark()
		{
			var renderer = new UIGraphicsImageRenderer(new CGSize(_defaultSize, _defaultSize), new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = 0,
			});

			return renderer.CreateImage((context) =>
			{
				context.CGContext.SaveState();

				var vPadding = _lineWidth / 2;
				var hPadding = _lineWidth / 2;
				var diameter = _defaultSize - _lineWidth;

				var checkPath = CreateCheckPath();
				context.CGContext.TranslateCTM(hPadding + (nfloat)(0.05 * diameter), vPadding + (nfloat)(0.1 * diameter));
				context.CGContext.ScaleCTM(diameter, diameter);
				DrawCheckMark(checkPath);
				UIColor.White.SetStroke();
				checkPath.Stroke();
				context.CGContext.RestoreState();
			});
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
	}
}