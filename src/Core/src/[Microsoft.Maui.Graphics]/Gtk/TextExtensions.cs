using System;

namespace Microsoft.Maui.Graphics.Native.Gtk
{

	public static class TextExtensions
	{

		public static double GetLineHeigth(this Pango.Layout layout, int numLines, bool scaled = true)
		{
			var inkRect = new Pango.Rectangle();
			var logicalRect = new Pango.Rectangle();
			var numLines1 = numLines > 0 ? Math.Min(numLines, layout.LineCount) : layout.LineCount;
			var lineHeigh = 0d;

			var metrics = layout.Context.GetMetrics(layout.FontDescription, Pango.Language.Default);
			var baseline = metrics.Ascent / (double)(metrics.Ascent + metrics.Descent);
			layout.GetLineReadonly(0).GetExtents(ref inkRect, ref logicalRect);
			lineHeigh += (scaled ? logicalRect.Height.ScaledFromPango() : logicalRect.Height);

			return lineHeigh * baseline + (lineHeigh * numLines - 1);
		}

		public static (int width, int height) GetPixelSize(this Pango.Layout layout, string text, double desiredSize = -1d, bool heightForWidth = true)
		{
			desiredSize = double.IsInfinity(desiredSize) ? -1 : desiredSize;
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

		public static double ScaledFromPango(this double it)
			=> Math.Ceiling(it / Pango.Scale.PangoScale);

	}

}