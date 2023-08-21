// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Graphics;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class ColorExtensions
	{
		public static global::Android.Graphics.Color? ToColor(this int[] color)
		{
			if (color == null)
				return null;

			return new global::Android.Graphics.Color(color[0], color[1], color[2], color[3]);
		}
	}
}
