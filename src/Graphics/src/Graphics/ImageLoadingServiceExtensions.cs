// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.Maui.Graphics
{
	public static class ImageLoadingServiceExtensions
	{
		public static IImage FromBytes(this IImageLoadingService target, byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return target.FromStream(stream);
			}
		}
	}
}
