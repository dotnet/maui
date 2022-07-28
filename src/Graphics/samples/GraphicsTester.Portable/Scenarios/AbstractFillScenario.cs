using System;
using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public abstract class AbstractFillScenario : AbstractScenario
	{
		private readonly Action<ICanvas, float, float, float, float> action;

		protected AbstractFillScenario(Action<ICanvas, float, float, float, float> action)
			: base(720, 1024)
		{
			this.action = action;
		}

		public override void Draw(ICanvas canvas)
		{
			FillRectanglesOfDifferentSizesAndColors(canvas);
			FillRectanglesWithAlpha(canvas);
			FillShadowedRect(canvas);
		}

		private void FillShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.FillColor = Colors.Black;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			action(canvas, 50.5f, 400.5f, 200, 50);
			canvas.RestoreState();

			canvas.SaveState();
			canvas.FillColor = Colors.CornflowerBlue;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			action(canvas, 50.5f, 500.5f, 200, 50);
			canvas.RestoreState();
		}

		private void FillRectanglesOfDifferentSizesAndColors(ICanvas canvas)
		{
			canvas.FillColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				action(canvas, 50, 50 + i * 30, 150, 20);
			}

			canvas.FillColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				action(canvas, 250.5f, 50.5f + i * 30, 150, 20);
			}
		}

		private void FillRectanglesWithAlpha(ICanvas canvas)
		{
			canvas.FillColor = Colors.Black;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				action(canvas, 450f, 200f + i * 30, 150, 20);
			}

			canvas.Alpha = 1;
		}
	}
}
