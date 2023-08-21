// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="IBitmapExportService"/>.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public class W2DBitmapExportService
#else
	public class PlatformBitmapExportService
#endif
		: IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			throw new NotImplementedException();
		}
	}
}
