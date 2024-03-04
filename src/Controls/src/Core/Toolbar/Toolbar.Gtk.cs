using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{

	public partial class Toolbar
	{

		MauiToolbar PlatformView => Handler?.PlatformView as MauiToolbar ?? throw new InvalidOperationException("Native View not set");

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarTextColor((IToolbarHandler)arg1, arg2);

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarBackground((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonTitle((IToolbarHandler)arg1, arg2);

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarItems((IToolbarHandler)arg1, arg2);

		public static void MapTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitle((IToolbarHandler)arg1, arg2);

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapIconColor((IToolbarHandler)arg1, arg2);

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleIcon((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonVisible((IToolbarHandler)arg1, arg2);

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapIsVisible((IToolbarHandler)arg1, arg2);

		[MissingMapper]
		public static void MapBarTextColor(IToolbarHandler handler, Toolbar toolbar)
		{ }

		public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateIsVisible(toolbar);
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateBackButtonVisibility(toolbar);
		}

		[MissingMapper]
		public static void MapTitleIcon(IToolbarHandler handler, Toolbar toolbar)
		{ }

		[MissingMapper]
		public static void MapTitle(IToolbarHandler handler, Toolbar toolbar)
		{ }

		[MissingMapper]
		public static void MapIconColor(IToolbarHandler handler, Toolbar toolbar)
		{ }

		[MissingMapper]
		public static void MapToolbarItems(IToolbarHandler handler, Toolbar toolbar)
		{ }

		[MissingMapper]
		public static void MapBackButtonTitle(IToolbarHandler handler, Toolbar toolbar)
		{ }

		[MissingMapper]
		public static void MapBarBackground(IToolbarHandler handler, Toolbar toolbar)
		{ }

	}

}