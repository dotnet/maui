using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawTextRotatedAtPoint : AbstractScenario
	{
		public DrawTextRotatedAtPoint()
			: base(1024, 720)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.Rotate(90, 360, 640);

			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeSize = 1f;
			canvas.FontColor = Colors.Red;
			canvas.FontSize = 12f;

			canvas.DrawLine(50, 50, 250, 50);
			canvas.DrawString("Red - Align Left", 50, 50, HorizontalAlignment.Left);

			canvas.DrawLine(50, 100, 250, 100);
			canvas.DrawString("Red - Align Center", 150, 100, HorizontalAlignment.Center);

			canvas.DrawLine(50, 150, 250, 150);
			canvas.DrawString("Red - Align Right", 250, 150, HorizontalAlignment.Right);

			canvas.SaveState();
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.DrawString("Red - Shadowed", 50, 200, HorizontalAlignment.Left);
			canvas.RestoreState();

			var blurrableCanvas = canvas as IBlurrableCanvas;
			if (blurrableCanvas != null)
			{
				canvas.SaveState();
				blurrableCanvas.SetBlur(CanvasDefaults.DefaultShadowBlur);
				canvas.DrawString("Red - Shadowed", 50, 250, HorizontalAlignment.Left);
				canvas.RestoreState();
			}

			canvas.SaveState();
			canvas.Font = Font.DefaultBold;
			canvas.DrawString("Bold System Font", 50, 350, HorizontalAlignment.Left);
			canvas.Font = Font.Default;
			canvas.DrawString("System Font", 50, 400, HorizontalAlignment.Left);
			canvas.RestoreState();

			canvas.RestoreState();
		}
	}
}
