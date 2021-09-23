using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui
{
	internal partial class RvAdapter : RecyclerView.Adapter
	{
		readonly VirtualListViewHandler handler;

		readonly object lockObj = new object();

		readonly PositionalViewSelector positionalViewSelector;

		RvViewHolderClickListener clickListener;

		public Context Context { get; }

		public object BindingContext { get; set; }

		public override int ItemCount
			=> positionalViewSelector?.TotalCount ?? 0;

		internal RvAdapter(Context context, VirtualListViewHandler handler, PositionalViewSelector positionalViewSelector)
		{
			Context = context;
			this.handler = handler;
			this.positionalViewSelector = positionalViewSelector;
		}

		public float DisplayScale =>
			handler?.Context?.Resources.DisplayMetrics.Density ?? 1;

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var info = positionalViewSelector.GetInfo(position);

			if (info == null)
				return;

			// The template selector doesn't infer selected properly
			// so we need to ask the listview which tracks selections about the state
			info.IsSelected = info.Kind == PositionKind.Item
				&& (handler?.VirtualView?.IsItemSelected(info.SectionIndex, info.ItemIndex) ?? false);

			if (holder is RvItemHolder itemHolder)
			{
				var data = info.Kind switch {
					PositionKind.Item =>
						positionalViewSelector?.Adapter?.Item(info.SectionIndex, info.ItemIndex),
					PositionKind.SectionHeader =>
						positionalViewSelector?.Adapter?.Section(info.SectionIndex),
					PositionKind.SectionFooter =>
						positionalViewSelector?.Adapter?.Section(info.SectionIndex),
					_ => null
				};

				if (!itemHolder.HasView)
				{
					var view = positionalViewSelector?.ViewSelector?.CreateView(info.Kind, data, info.SectionIndex, info.ItemIndex);
					itemHolder.SwapView(view);
				}

				itemHolder.Update(info);

				positionalViewSelector?.ViewSelector?.RecycleView(info.Kind, data, itemHolder.ViewContainer.VirtualView, info.SectionIndex, info.ItemIndex);
			}
		}

		List<string> cachedReuseIds = new List<string>();

		public override int GetItemViewType(int position)
		{
			base.GetItemViewType(position);

			var info = positionalViewSelector.GetInfo(position);

			var data = info.Kind switch {
				PositionKind.Item =>
					positionalViewSelector?.Adapter?.Item(info.SectionIndex, info.ItemIndex),
				PositionKind.SectionHeader =>
					positionalViewSelector?.Adapter?.Section(info.SectionIndex),
				PositionKind.SectionFooter =>
					positionalViewSelector?.Adapter?.Section(info.SectionIndex),
				_ => null
			};

			var reuseId = positionalViewSelector.ViewSelector.GetReuseId(info.Kind, data, info.SectionIndex, info.ItemIndex);

			int vt = -1;

			lock (lockObj)
			{
				vt = cachedReuseIds.IndexOf(reuseId) + 1;
				if (vt <= 0)
				{
					cachedReuseIds.Add(reuseId);
					vt = cachedReuseIds.Count;
				}
			}

			return vt;
		}

		public override long GetItemId(int position)
			=> position;

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var viewHolder = new RvItemHolder(handler.MauiContext);

			clickListener = new RvViewHolderClickListener(viewHolder, rvh =>
			{
				if (rvh.PositionInfo == null || rvh.PositionInfo.Kind != PositionKind.Item)
					return;

				var p = new ItemPosition(rvh.PositionInfo.SectionIndex, rvh.PositionInfo.ItemIndex);

				rvh.PositionInfo.IsSelected = !rvh.PositionInfo.IsSelected;

				if (rvh.PositionInfo.IsSelected)
					handler.VirtualView?.SetSelected(p);
				else
					handler.VirtualView?.SetDeselected(p);
			});

			viewHolder.ItemView.SetOnClickListener(clickListener);

			return viewHolder;
		}

		public void Reset()
		{
			lock (lockObj)
				cachedReuseIds.Clear();
		}
	}
}