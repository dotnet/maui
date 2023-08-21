// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace GraphicsTester.Scenarios
{
	public class FillEllipses : AbstractFillScenario
	{
		public FillEllipses()
			: base((canvas, x, y, width, height) => canvas.FillEllipse(x, y, width, height))
		{
		}
	}
}
