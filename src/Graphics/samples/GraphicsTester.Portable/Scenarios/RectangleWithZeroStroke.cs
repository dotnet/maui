// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class RectangleWithZeroStroke : AbstractScenario
	{
		public RectangleWithZeroStroke() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			canvas.FillColor = Colors.CornflowerBlue;
			canvas.FillRectangle(50, 50, 100, 100);

			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 0;
			canvas.DrawRectangle(50, 50, 100, 100);

			canvas.RestoreState();
		}
	}
}
