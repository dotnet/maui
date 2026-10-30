using System;
using System.IO;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// A <see cref="BitmapExportContext"/> that draws into an in-memory Cairo image surface.
	/// </summary>
	public class CairoBitmapExportContext : BitmapExportContext
	{
		private CairoImageSurface _surface;
		private CairoContext _context;
		private CairoCanvas _platformCanvas;

		/// <summary>
		/// Creates a context.
		/// </summary>
		/// <param name="width">Width in pixels.</param>
		/// <param name="height">Height in pixels.</param>
		/// <param name="displayScale">The value reported by <see cref="ICanvas.DisplayScale"/> (used to size pattern tiles). It is not applied automatically: call <c>Scale</c> on the canvas.</param>
		/// <param name="dpi">The resolution reported through <see cref="BitmapExportContext.Dpi"/>.</param>
		/// <param name="transparent"><see langword="true"/> for an alpha channel, <see langword="false"/> for an opaque 24-bit surface.</param>
		public CairoBitmapExportContext(int width, int height, float displayScale = 1, int dpi = 72, bool transparent = true)
			: base(width, height, dpi)
		{
			_surface = new CairoImageSurface(transparent ? CairoFormat.Argb32 : CairoFormat.Rgb24, width, height);
			_context = new CairoContext(_surface);
			_platformCanvas = new CairoCanvas(_context) { DisplayScale = displayScale };
		}

		/// <inheritdoc/>
		/// <remarks>
		/// The <see cref="CairoCanvas"/> itself: its <c>Scale</c>/<c>Translate</c> are real transforms, so (unlike the Skia
		/// canvas) it must not be wrapped in a <see cref="ScalingCanvas"/>.
		/// </remarks>
		public override ICanvas Canvas => _platformCanvas;

		/// <summary>Gets the surface that is drawn on.</summary>
		public CairoImageSurface Surface => _surface;

		/// <inheritdoc/>
		/// <remarks>A copy of the pixels at the time of the call, owned by the caller.</remarks>
		public override IImage Image
		{
			get
			{
				_surface.Flush();
				var copy = new CairoImageSurface(_surface.Format, _surface.Width, _surface.Height);
				using (var context = new CairoContext(copy))
				{
					context.Operator = CairoOperator.Source;
					context.SetSource(_surface, 0, 0);
					context.Paint();
				}

				copy.Flush();
				return new CairoImage(copy);
			}
		}

		/// <inheritdoc/>
		public override void WriteToStream(Stream stream)
		{
			using (var image = (CairoImage)Image)
				image.Save(stream, ImageFormat.Png);
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			_platformCanvas?.Dispose();
			_platformCanvas = null;
			_context?.Dispose();
			_context = null;
			_surface?.Dispose();
			_surface = null;
			base.Dispose();
		}
	}
}
