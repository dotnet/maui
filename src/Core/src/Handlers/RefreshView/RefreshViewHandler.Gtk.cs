using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Native;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, RefreshView>
	{
		protected override RefreshView CreateNativeView()
		{
			return new RefreshView();
		}

		[MissingMapper]
		public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		[MissingMapper]
		public static void MapRefreshViewBackground(RefreshViewHandler handler, IView view)
		{
		}
	}
}
