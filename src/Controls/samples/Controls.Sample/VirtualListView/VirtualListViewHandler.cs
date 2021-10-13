using System;

namespace Microsoft.Maui
{
#if MACCATALYST
	public partial class VirtualListViewHandler : Handlers.ViewHandler<IVirtualListView, UIKit.UIView>
	{
		protected override UIKit.UIView CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public void InvalidateData() { }
#else
	public partial class VirtualListViewHandler
	{
#endif

		public static PropertyMapper<IVirtualListView, VirtualListViewHandler> VirtualListViewMapper = new PropertyMapper<IVirtualListView, VirtualListViewHandler>(VirtualListViewHandler.ViewMapper)
		{

#if !MACCATALYST && !NET6_0
			[nameof(IVirtualListView.Adapter)] = MapAdapter,
			[nameof(IVirtualListView.Header)] = MapHeader,
			[nameof(IVirtualListView.Footer)] = MapFooter,
			[nameof(IVirtualListView.ViewSelector)] = MapViewSelector,
			[nameof(IVirtualListView.SelectionMode)] = MapSelectionMode,
			[nameof(IVirtualListView.Orientation)] = MapOrientation,
			[nameof(IVirtualListView.InvalidateData)] = MapInvalidateData,
#endif
		};

		public static CommandMapper<IVirtualListView, VirtualListViewHandler> VirtualListViewCommandMapper = new(VirtualListViewHandler.ViewCommandMapper)
		{
#if !MACCATALYST && !NET6_0
			[nameof(IVirtualListView.SetSelected)] = MapSetSelected,
			[nameof(IVirtualListView.SetDeselected)] = MapSetDeselected
#endif
		};

		public VirtualListViewHandler() : base(VirtualListViewMapper, VirtualListViewCommandMapper)
		{

		}

		public VirtualListViewHandler(PropertyMapper mapper = null, CommandMapper commandMapper = null) : base(mapper ?? VirtualListViewMapper, commandMapper ?? VirtualListViewCommandMapper)
		{

		}
	}
}