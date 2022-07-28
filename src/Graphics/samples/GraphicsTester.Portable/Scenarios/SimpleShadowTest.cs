using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class SimpleShadowTest : AbstractScenario
	{
		public SimpleShadowTest() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			canvas.SetShadow(
				CanvasDefaults.DefaultShadowOffset,
				CanvasDefaults.DefaultShadowBlur,
				CanvasDefaults.DefaultShadowColor);
			canvas.FillColor = Colors.Blue;
			var path = new PathF(10, 10);
			path.LineTo(30, 10);
			path.LineTo(20, 30);
			path.Close();
			canvas.FillPath(path);

			canvas.RestoreState();
		}
	}
}
