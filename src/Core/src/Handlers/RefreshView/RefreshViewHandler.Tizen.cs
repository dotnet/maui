using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	// TODO : Need to implement
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, NView>
	{
		protected override NView CreatePlatformView() => throw new NotImplementedException();

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
		{
		}

		public static void MapRefreshViewBackground(IRefreshViewHandler handler, IView view)
		{
		}

	}
}
