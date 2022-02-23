using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, RefreshContainer>
	{
		protected override RefreshContainer CreatePlatformView()
		{
			return new RefreshContainer();
		}

		[MissingMapper]
		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapRefreshViewBackground(IRefreshViewHandler handler, IView view)
		{
		}
	}
}
