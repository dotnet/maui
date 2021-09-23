using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Handlers;
using System;

namespace Microsoft.Maui
{
	public partial class VirtualListViewHandler : ViewHandler<IVirtualListView, RecyclerView>
	{
		RvAdapter adapter;
		RecyclerView recyclerView;
		LinearLayoutManager layoutManager;
		PositionalViewSelector positionalViewSelector;

		protected override RecyclerView CreateNativeView()
			=> recyclerView ??= new RecyclerView(Context);

		protected override void ConnectHandler(RecyclerView nativeView)
		{
			layoutManager = new LinearLayoutManager(Context);
			//layoutManager.Orientation = LinearLayoutManager.Horizontal;

			positionalViewSelector = new PositionalViewSelector(VirtualView);

			adapter = new RvAdapter(Context, this, positionalViewSelector);
			
			recyclerView.AddOnScrollListener(new RvScrollListener((rv, dx, dy) =>
			{
				var x = Context.FromPixels(dx);
				var y = Context.FromPixels(dy);
				// TODO: Proxy up to event

				// VirtualView?.RaiseScrolled((x, y));
			}));

			recyclerView.SetLayoutManager(layoutManager);
			recyclerView.SetAdapter(adapter);
			recyclerView.LayoutParameters = new ViewGroup.LayoutParams(
				ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
		}

		protected override void DisconnectHandler(RecyclerView nativeView)
		{
			recyclerView.ClearOnScrollListeners();
			recyclerView.SetAdapter(null);
			adapter.Dispose();
			adapter = null;
			layoutManager.Dispose();
			layoutManager = null;
		}

		public void InvalidateData()
		{
			positionalViewSelector.Reset();
			adapter?.Reset();
			adapter?.NotifyDataSetChanged();
		}

		public static void MapAdapter(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler.InvalidateData();

		public static void MapHeader(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler.InvalidateData();

		public static void MapFooter(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler.InvalidateData();

		public static void MapViewSelector(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler.InvalidateData();

		public static void MapSelectionMode(VirtualListViewHandler handler, IVirtualListView virtualListView)
		{ }
		public static void MapInvalidateData(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler.InvalidateData();

		public static void MapSetSelected(VirtualListViewHandler handler, IVirtualListView virtualListView, object parameter)
		{
			handler.adapter.NotifyDataSetChanged();
		}

		public static void MapSetDeselected(VirtualListViewHandler handler, IVirtualListView virtualListView, object parameter)
		{
			handler.adapter.NotifyDataSetChanged();
		}

		public static void MapOrientation(VirtualListViewHandler handler, IVirtualListView virtualListView)
		{
			handler.layoutManager.Orientation = virtualListView.Orientation switch
			{
				ListOrientation.Vertical => LinearLayoutManager.Vertical,
				ListOrientation.Horizontal => LinearLayoutManager.Horizontal,
				_ => LinearLayoutManager.Vertical
			};
			handler.adapter.NotifyDataSetChanged();
		}

		class RvScrollListener : RecyclerView.OnScrollListener
		{
			public RvScrollListener(Action<RecyclerView, int, int> scrollHandler)
			{
				ScrollHandler = scrollHandler;
			}

			Action<RecyclerView, int, int> ScrollHandler { get; }

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				base.OnScrolled(recyclerView, dx, dy);

				ScrollHandler?.Invoke(recyclerView, dx, dy);
			}
		}
	}
}