// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public static class PictureExtensions
	{
		public static RectF GetBounds(this IPicture target)
		{
			if (target == null)
				return default;
			return new RectF(target.X, target.Y, target.Width, target.Height);
		}
	}
}
