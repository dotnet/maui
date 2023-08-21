// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class SizeExtensions
	{
		public static global::Windows.Foundation.Size ToPlatform(this Size size) => new global::Windows.Foundation.Size(size.Width, size.Height);
	}
}