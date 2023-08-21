// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CoreGraphics;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class CGColorExtensions
	{
		public static CGColor ToCGColor(this float[] color)
		{
			if (color == null)
				return null;

			return new CGColor(color[0], color[1], color[2], color[3]);
		}
	}
}
