// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	class ImageSourcePaint : Paint
	{
		public ImageSourcePaint()
		{
		}

		public ImageSourcePaint(IImageSource imageSource)
		{
			ImageSource = imageSource;
		}

		public IImageSource? ImageSource { get; set; }
	}
}