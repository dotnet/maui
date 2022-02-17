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

		public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapRefreshViewBackground(RefreshViewHandler handler, IView view)
		{
		}
	}
}
