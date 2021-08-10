using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, UIView>
	{
		protected override UIView CreateNativeView()
		{
			return new UIView();
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
