// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Platform
{
	internal static class ToolbarExtensions
	{
		public static void UpdateTitle(this MauiToolbar nativeToolbar, IToolbar toolbar)
		{
			nativeToolbar.Title = toolbar.Title;
		}
	}
}
