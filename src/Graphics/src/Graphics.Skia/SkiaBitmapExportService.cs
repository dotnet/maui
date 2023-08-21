// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Skia
{
	public class PlatformBitmapExportService : IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			return new SkiaBitmapExportContext(width, height, displayScale, 72, false);
		}
	}
}
