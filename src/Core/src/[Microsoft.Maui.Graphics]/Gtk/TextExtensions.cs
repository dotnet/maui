namespace Microsoft.Maui.Graphics.Native.Gtk
{

	public static class TextExtensions
	{

		public static double GetLineHeigth(this Pango.Layout layout, bool scaled = true)
		{
			var inkRect = new Pango.Rectangle();
			var logicalRect = new Pango.Rectangle();
			layout.GetLineReadonly(0).GetExtents(ref inkRect, ref logicalRect);
			var lineHeigh = scaled ? logicalRect.Height.ScaledFromPango() : logicalRect.Height;

			return lineHeigh;
		}

		public static (int width, int height) GetPixelSize(this Pango.Layout layout, string text, double desiredSize = -1d, bool heightForWidth = true)
		{

			if (desiredSize > 0)
			{
				if (heightForWidth)
				{
					layout.Width = desiredSize.ScaledToPango();
				}
				else
				{
					layout.Height = desiredSize.ScaledToPango();
				}
			}

			layout.SetText(text);
			layout.GetPixelSize(out var textWidth, out var textHeight);

			return (textWidth, textHeight);
		}

	}

}