// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		public static void MapShouldDelayContentTouches(ScrollViewHandler handler, ScrollView scrollView)
			=> MapShouldDelayContentTouches((IScrollViewHandler)handler, scrollView);

		public static void MapShouldDelayContentTouches(IScrollViewHandler handler, ScrollView scrollView)
		{
			Platform.ScrollViewExtensions.UpdateShouldDelayContentTouches(handler.PlatformView, scrollView);
		}
	}
}