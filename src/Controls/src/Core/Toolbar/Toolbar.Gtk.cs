using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		MauiToolbar PlatformView => Handler?.PlatformView as MauiToolbar ?? throw new InvalidOperationException("Native View not set");

		[MissingMapper]
		public static void MapBarTextColor(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		public static void MapIsVisible(ToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateIsVisible(toolbar);
		}

		public static void MapBackButtonVisible(ToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateBackButtonVisibility(toolbar);
		}

		[MissingMapper]
		public static void MapTitleIcon(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		[MissingMapper]
		public static void MapTitleView(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		[MissingMapper]
		public static void MapIconColor(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		[MissingMapper]
		public static void MapToolbarItems(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		[MissingMapper]
		public static void MapBackButtonTitle(ToolbarHandler handler, Toolbar toolbar)
		{
		}

		[MissingMapper]
		public static void MapBarBackground(ToolbarHandler handler, Toolbar toolbar)
		{
		}
	}
}
