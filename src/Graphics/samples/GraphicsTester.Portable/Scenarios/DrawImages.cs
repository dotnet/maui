using Microsoft.Maui.Graphics;
using System.Reflection;

namespace GraphicsTester.Scenarios
{
	public class DrawImages : AbstractScenario
	{
		public DrawImages()
			: base(1400, 800)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			IImage image;
			var assembly = GetType().GetTypeInfo().Assembly;
			using (var stream = assembly.GetManifestResourceStream("GraphicsTester.Resources.royals.png"))
			{
				image = GraphicsPlatform.CurrentService.LoadImageFromStream(stream);
			}

			if (image != null)
			{
				var expectedWidth = 512f;
				var expectedHeight = 384f;

				var actualWidth = image.Width;
				var actualHeight = image.Height;

				canvas.DrawImage(image, 600, 50, actualWidth, actualHeight);

				canvas.DrawImage(image, 50, 50, expectedWidth, expectedHeight);
				canvas.StrokeColor = Colors.Blue;
				canvas.DrawRectangle(50, 50, expectedWidth, expectedHeight);

				canvas.DrawImage(image, 50, 500, expectedWidth / 2, expectedHeight / 2);
				canvas.DrawRectangle(50, 500, expectedWidth / 2, expectedHeight / 2);

				canvas.Alpha = .5f;
				canvas.DrawImage(image, 350, 500, expectedWidth / 2, expectedHeight / 2);
				canvas.Alpha = 1;

				canvas.SaveState();
				canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
				canvas.DrawImage(image, 650, 500, expectedWidth / 2, expectedHeight / 2);
				canvas.RestoreState();
			}
		}
	}
}
