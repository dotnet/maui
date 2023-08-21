// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ScrollViewExtensions
	{
		public static void UpdateShouldDelayContentTouches(this UIScrollView platformView, ScrollView scrollView)
		{
			platformView.DelaysContentTouches = scrollView.OnThisPlatform().ShouldDelayContentTouches();
		}
	}
}