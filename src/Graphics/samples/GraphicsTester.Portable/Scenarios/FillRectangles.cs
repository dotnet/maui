// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace GraphicsTester.Scenarios
{
	public class FillRectangles : AbstractFillScenario
	{
		public FillRectangles()
			: base((canvas, x, y, width, height) => canvas.FillRectangle(x, y, width, height))
		{
		}
	}
}
