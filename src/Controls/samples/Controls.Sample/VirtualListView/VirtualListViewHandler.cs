using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public partial class VirtualListViewHandler
	{
		public static PropertyMapper<IVirtualListView, VirtualListViewHandler> VirtualListViewMapper = new PropertyMapper<IVirtualListView, VirtualListViewHandler>(VirtualListViewHandler.ViewMapper)
		{
			[nameof(IVirtualListView.Adapter)] = MapAdapter,
			[nameof(IVirtualListView.Header)] = MapHeader,
			[nameof(IVirtualListView.Footer)] = MapFooter,
			[nameof(IVirtualListView.ViewSelector)] = MapViewSelector,
			[nameof(IVirtualListView.SelectionMode)] = MapSelectionMode,
			[nameof(IVirtualListView.Orientation)] = MapOrientation,
		};

		public static CommandMapper<IVirtualListView, VirtualListViewHandler> VirtualListViewCommandMapper = new(VirtualListViewHandler.ViewCommandMapper)
		{
			[nameof(IVirtualListView.InvalidateData)] = MapInvalidateData,
			[nameof(IVirtualListView.SetSelected)] = MapSetSelected,
			[nameof(IVirtualListView.SetDeselected)] = MapSetDeselected
		};

		public VirtualListViewHandler() : base(VirtualListViewMapper, VirtualListViewCommandMapper)
		{

		}

		public VirtualListViewHandler(PropertyMapper mapper = null, CommandMapper commandMapper = null) : base(mapper ?? VirtualListViewMapper, commandMapper ?? VirtualListViewCommandMapper)
		{

		}
	}
}