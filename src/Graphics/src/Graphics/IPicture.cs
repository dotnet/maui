// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public interface IPicture
	{
		void Draw(ICanvas canvas);

		float X { get; }

		float Y { get; }

		float Width { get; }

		float Height { get; }
	}
}
