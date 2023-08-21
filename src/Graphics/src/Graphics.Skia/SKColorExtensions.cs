// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public static class SKColorExtensions
	{
		public static SKColor ToColor(this Color target, float alpha = 1)
		{
			var r = (byte)(target.Red * 255f);
			var g = (byte)(target.Green * 255f);
			var b = (byte)(target.Blue * 255f);
			var a = (byte)(target.Alpha * 255f * alpha);

			return new SKColor(r, g, b, a);
		}
	}
}
