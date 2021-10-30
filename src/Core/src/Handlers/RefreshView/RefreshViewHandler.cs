using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler
	{

		public static PropertyMapper<IRefreshView, RefreshViewHandler> RefreshMapper = new PropertyMapper<IRefreshView, RefreshViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
			[nameof(IRefreshView.Content)] = MapContent,
			[nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
			[nameof(IView.Background)] = MapBackground,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
		};

		public RefreshViewHandler() : base(RefreshMapper)
		{
		}

		public RefreshViewHandler(PropertyMapper? mapper = null) : base(mapper ?? RefreshMapper)
		{

		}
	}
}
