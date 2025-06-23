using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="IStringSizeService"/> which
	/// can measure a given string and return the dimensions.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public class W2DStringSizeService
#else
	public class PlatformStringSizeService
#endif
		: IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float textSize)
			=> GetStringSize(value, font, textSize, HorizontalAlignment.Left, VerticalAlignment.Top);

		public SizeF GetStringSize(string value, IFont font, float textSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			if (string.IsNullOrEmpty(value) || font is null || textSize <= 0)
                return SizeF.Zero;

            var format = font.ToCanvasTextFormat(textSize);
            format.WordWrapping = CanvasWordWrapping.NoWrap;

            var device = CanvasDevice.GetSharedDevice();

            using (var textLayout = new CanvasTextLayout(device, value, format, float.MaxValue, float.MaxValue))
            {
                textLayout.VerticalAlignment = verticalAlignment switch
                {
                    VerticalAlignment.Top => CanvasVerticalAlignment.Top,
                    VerticalAlignment.Center => CanvasVerticalAlignment.Center,
                    VerticalAlignment.Bottom => CanvasVerticalAlignment.Bottom,
                    _ => CanvasVerticalAlignment.Top
                };
                textLayout.HorizontalAlignment = horizontalAlignment switch
                {
                    HorizontalAlignment.Left => CanvasHorizontalAlignment.Left,
                    HorizontalAlignment.Center => CanvasHorizontalAlignment.Center,
                    HorizontalAlignment.Right => CanvasHorizontalAlignment.Right,
                    HorizontalAlignment.Justified => CanvasHorizontalAlignment.Justified,
                    _ => CanvasHorizontalAlignment.Left,
                };

                var bounds = textLayout.LayoutBounds;

                // If LayoutBounds is empty, fallback to DrawBounds
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    bounds = textLayout.DrawBounds;
                }

                return new SizeF((float)bounds.Width, (float)bounds.Height);
            }
        }
	}
}