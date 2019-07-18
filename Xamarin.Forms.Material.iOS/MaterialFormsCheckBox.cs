using System;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.Material.iOS
{
	public class MaterialFormsCheckBox : FormsCheckBox
	{
		static UIImage _checked;
		static UIImage _unchecked;

		internal override UIBezierPath CreateBoxPath(CGRect backgroundRect) => UIBezierPath.FromRoundedRect(backgroundRect, 1);
		internal override UIBezierPath CreateCheckPath() => new UIBezierPath
		{
			LineWidth = (nfloat).12,
			LineCapStyle = CGLineCap.Round,
			LineJoinStyle = CGLineJoin.Round
		};

		internal override void DrawCheckMark(UIBezierPath path)
		{
			path.MoveTo(new CGPoint(0.80f, 0.14f));
			path.AddLineTo(new CGPoint(0.33f, 0.6f));
			path.AddLineTo(new CGPoint(0.10f, 0.37f));
		}

		protected override UIImage GetCheckBoximage()
		{
			// Ideally I would use the static images here but when disabled it always tints them grey
			// and I don't know how to make it not tint them gray
			if (!Enabled && CheckBoxTintColor != Color.Default)
			{
				if (IsChecked)
					return CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				return CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
			}

			if (_checked == null)
				_checked = CreateCheckBox(CreateCheckMark()).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			if (_unchecked == null)
				_unchecked = CreateCheckBox(null).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

			if (IsChecked)
				return _checked;

			return _unchecked;
		}
	}
}
