// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public interface IPattern
	{
		float Width { get; }
		float Height { get; }
		float StepX { get; }
		float StepY { get; }
		void Draw(ICanvas canvas);
	}
}
