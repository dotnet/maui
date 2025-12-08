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
			var format = font.ToCanvasTextFormat(textSize);
			format.WordWrapping = CanvasWordWrapping.NoWrap;

			var device = CanvasDevice.GetSharedDevice();
			var textLayout = new CanvasTextLayout(device, value, format, 0.0f, 0.0f);
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

			return new SizeF((float)textLayout.DrawBounds.Width, (float)textLayout.DrawBounds.Height);
		}
	}
}
