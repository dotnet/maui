using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	// TODO : Need to implement
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, EvasObject>
	{
		protected override EvasObject CreateNativeView() => throw new NotImplementedException();

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
