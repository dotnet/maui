using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

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
