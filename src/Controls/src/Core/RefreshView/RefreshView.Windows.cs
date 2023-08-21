// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		public static void MapRefreshPullDirection(RefreshViewHandler handler, RefreshView refreshView)
			=> MapRefreshPullDirection((IRefreshViewHandler)handler, refreshView);

		public static void MapRefreshPullDirection(IRefreshViewHandler handler, RefreshView refreshView) =>
			Platform.RefreshViewExtensions.UpdateRefreshPullDirection(handler.PlatformView, refreshView);
	}
}