using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using System.Reflection;

namespace GraphicsTester.Scenarios
{
	public class ImageFills : AbstractScenario
	{
		public ImageFills()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			IImage image;
			var assembly = GetType().GetTypeInfo().Assembly;
			using (var stream = assembly.GetManifestResourceStream("GraphicsTester.Resources.swirl_pattern.png"))
			{
				image = PlatformImage.FromStream(stream);
			}

			if (image != null)
			{
				canvas.SetFillPaint(image.AsPaint(), RectF.Zero);
				canvas.FillRectangle(50, 50, 500, 500);
			}
		}
	}
}
