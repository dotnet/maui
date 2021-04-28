using System;
using Gtk;

namespace Microsoft.Maui
{

	public static class WidgetExtensions
	{

		public static void UpdateIsEnabled(this Widget native, bool isEnabled) =>
			native.Sensitive = isEnabled;

		public static SizeRequest GetDesiredSize(
			this Widget? nativeView,
			double widthConstraint,
			double heightConstraint)
		{
			if (nativeView == null)
				return Graphics.Size.Zero;

			nativeView.GetPreferredSize(out var minimumSize, out var req);

			var desiredSize = new Gdk.Size(
				req.Width > 0 ? req.Width : 0,
				req.Height > 0 ? req.Height : 0);

			var widthFits = widthConstraint >= desiredSize.Width;
			var heightFits = heightConstraint >= desiredSize.Height;

			if (widthFits && heightFits) // Enough space with given constraints
			{
				return new SizeRequest(new Graphics.Size(desiredSize.Width, desiredSize.Height));
			}

			if (!widthFits)
			{
				nativeView.SetSize((int)widthConstraint, -1);

				nativeView.GetPreferredSize(out minimumSize, out req);

				desiredSize = new Gdk.Size(
					req.Width > 0 ? req.Width : 0,
					req.Height > 0 ? req.Height : 0);

				heightFits = heightConstraint >= desiredSize.Height;
			}

			var size = new Graphics.Size(desiredSize.Width, heightFits ? desiredSize.Height : (int)heightConstraint);

			return new SizeRequest(size);

		}

		public static void SetSize(this Gtk.Widget self, double width, double height)
		{
			int calcWidth = (int)Math.Round(width);
			int calcHeight = (int)Math.Round(height);

			// Avoid negative values
			if (calcWidth < -1)
			{
				calcWidth = -1;
			}

			if (calcHeight < -1)
			{
				calcHeight = -1;
			}

			if (calcWidth != self.WidthRequest || calcHeight != self.HeightRequest)
			{
				self.SetSizeRequest(calcWidth, calcHeight);
			}
		}

		public static void UpdateFont(this Widget widget, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;

			var fontFamily = fontManager.GetFontFamily(font);
#pragma warning disable 612
			widget.ModifyFont(fontFamily);
#pragma warning restore 612

		}
	}

}