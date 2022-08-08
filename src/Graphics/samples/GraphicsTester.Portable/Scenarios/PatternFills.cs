using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class PatternFills : AbstractScenario
	{
		public PatternFills()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			IPattern pattern;
			using (var picture = new PictureCanvas(0, 0, 12, 12))
			{
				picture.StrokeColor = Colors.LimeGreen;

				picture.StrokeSize = 1f;
				picture.StrokeDashPattern = null;
				picture.DrawLine(0, 12, 12, 0);

				pattern = AddPictureAsPattern(picture.Picture, 12, 12);
			}

			canvas.SetFillPaint(pattern.AsPaint(), RectF.Zero);
			canvas.FillRectangle(50, 50, 500, 500);

			canvas.RestoreState();
		}

		private IPattern AddPictureAsPattern(IPicture picture, float stepX, float stepY)
		{
			return new PicturePattern(picture, stepX, stepY);
		}
	}
}
