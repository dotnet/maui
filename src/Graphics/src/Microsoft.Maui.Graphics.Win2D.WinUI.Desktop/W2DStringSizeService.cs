using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI.Text;

namespace Microsoft.Maui.Graphics.Win2D
{
	public class W2DStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float textSize)
			=> GetStringSize(value, font, textSize, HorizontalAlignment.Left, VerticalAlignment.Top);

		public SizeF GetStringSize(string value, IFont font, float textSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			var format = new CanvasTextFormat
			{
				FontFamily = font.Name,
				FontSize = textSize,
				FontWeight = new FontWeight { Weight = (ushort)font.Weight },
				FontStyle = font.StyleType.ToFontStyle(),
				WordWrapping = CanvasWordWrapping.NoWrap
			};

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
