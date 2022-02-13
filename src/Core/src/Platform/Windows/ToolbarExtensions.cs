﻿namespace Microsoft.Maui.Platform
{
	internal static class ToolbarExtensions
	{
		public static void UpdateTitle(this MauiToolbar nativeToolbar, IToolbar toolbar)
		{
			nativeToolbar.Title = toolbar.Title;
		}
	}
}
