using System;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// An <see cref="IStringSizeService"/> that measures text with Pango (through a throw-away Cairo context),
	/// so the result matches what <see cref="CairoCanvas"/> draws.
	/// </summary>
	public class PangoStringSizeService : IStringSizeService
	{
		private static readonly object s_lock = new object();
		private static CairoImageSurface s_surface;
		private static CairoContext s_context;

		/// <inheritdoc/>
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			return Measure(value, font, fontSize);
		}

		/// <inheritdoc/>
		/// <remarks>The text is measured on a single, unwrapped layout: the alignments do not change its size.</remarks>
		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) =>
			GetStringSize(value, font, fontSize);

		private static SizeF Measure(string value, IFont font, float fontSize)
		{
			lock (s_lock)
			{
				if (s_context == null)
				{
					s_surface = new CairoImageSurface(CairoFormat.Argb32, 1, 1);
					s_context = new CairoContext(s_surface);
				}

				using (var layout = PangoText.CreateLayout(s_context, value, font, fontSize))
				{
					PangoText.Measure(layout, out var width, out var height, out _);
					return new SizeF((float)width, (float)height);
				}
			}
		}
	}
}
