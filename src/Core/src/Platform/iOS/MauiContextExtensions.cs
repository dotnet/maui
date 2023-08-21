// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static UIWindow GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UIWindow>();

		public static IServiceProvider GetApplicationServices(this IMauiContext mauiContext)
		{
			return MauiUIApplicationDelegate.Current.Services ??
				throw new InvalidOperationException("Unable to find Application Services");
		}
	}
}