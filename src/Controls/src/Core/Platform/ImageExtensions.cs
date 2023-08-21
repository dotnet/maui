// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Platform;

static partial class ImageExtensions
{
	public static bool IsNullOrEmpty(this ImageSource? imageSource) =>
		imageSource is null || imageSource.IsEmpty;
}
