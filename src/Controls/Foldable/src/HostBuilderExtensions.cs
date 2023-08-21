// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
		// see Android/HostBuilderExtension.cs for the real implementation
#if !ANDROID
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder) =>
			builder;
#endif
	}
}