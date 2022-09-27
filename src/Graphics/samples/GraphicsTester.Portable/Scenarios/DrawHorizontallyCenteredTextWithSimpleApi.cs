using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawHorizontallyCenteredTextWithSimpleApi : AbstractScenario
	{
		public DrawHorizontallyCenteredTextWithSimpleApi()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			canvas.FillColor = Colors.CornflowerBlue;
			canvas.Font = Font.Default;
			canvas.FontSize = 12f;

			canvas.Translate(0, 10);
			canvas.DrawString("Default - Left", 100, 10, HorizontalAlignment.Left);
			canvas.FillCircle(100, 10, 2);

			canvas.Translate(0, 30);
			canvas.DrawString("Default - Center", 100, 10, HorizontalAlignment.Center);
			canvas.FillCircle(100, 10, 2);

			canvas.Translate(0, 30);
			canvas.DrawString("Default - Right", 100, 10, HorizontalAlignment.Right);
			canvas.FillCircle(100, 10, 2);

			canvas.Font = Font.Default;

			canvas.Translate(0, 30);
			canvas.DrawString("System - Left", 100, 10, HorizontalAlignment.Left);
			canvas.FillCircle(100, 10, 2);

			canvas.Translate(0, 30);
			canvas.DrawString("System - Center", 100, 10, HorizontalAlignment.Center);
			canvas.FillCircle(100, 10, 2);

			canvas.Translate(0, 30);
			canvas.DrawString("System - Right", 100, 10, HorizontalAlignment.Right);
			canvas.FillCircle(100, 10, 2);

			canvas.RestoreState();
		}
	}
}
