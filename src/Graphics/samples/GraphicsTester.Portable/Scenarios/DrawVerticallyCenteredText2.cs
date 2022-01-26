using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawVerticallyCenteredText2 : AbstractScenario
	{
		public DrawVerticallyCenteredText2()
			: base(2600, 2600)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.StrokeSize = 1;
			canvas.StrokeColor = Colors.Blue;
			canvas.Font = Font.Default;
			canvas.FontSize = 24f;

			canvas.SaveState();

			var rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();

			canvas.SaveState();
			canvas.Translate(300, 0);

			rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();

			canvas.SaveState();
			canvas.Translate(600, 0);

			rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Really Short 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();

			canvas.SaveState();
			canvas.Translate(0, 300);
			canvas.Font = Font.Default;

			rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();

			canvas.SaveState();
			canvas.Translate(300, 300);
			canvas.Font = Font.Default;

			rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();

			canvas.SaveState();
			canvas.Translate(600, 300);
			canvas.Font = Font.Default;

			rectHeight = 32;
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 1", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 2", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 3", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			rectHeight -= 2;
			canvas.Translate(0, 45);
			canvas.DrawRectangle(10, 0, 200, rectHeight);
			canvas.DrawString("Sys Font R 4", 10, 0, 200, rectHeight, HorizontalAlignment.Center, VerticalAlignment.Bottom);
			canvas.DrawLine(10, rectHeight / 2, 200, rectHeight / 2);

			canvas.RestoreState();
		}
	}
}
