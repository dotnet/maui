using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class EmptyViewAdapter : RecyclerView.Adapter
	{
		int _itemViewType;
		object _emptyView;
		DataTemplate _emptyViewTemplate;

		public object EmptyView
		{
			get => _emptyView;
			set
			{
				_emptyView = value;

				// Change _itemViewType to force OnCreateViewHolder to run again and use this new EmptyView
				_itemViewType += 1;
			}
		}
		
		public DataTemplate EmptyViewTemplate
		{
			get => _emptyViewTemplate;
			set
			{
				_emptyViewTemplate = value;
				
				// Change _itemViewType to force OnCreateViewHolder to run again and use this new template
				_itemViewType += 1;
			}
		}

		protected readonly ItemsView ItemsView;
		public override int ItemCount => 1;

		public EmptyViewAdapter(ItemsView itemsView)
		{
			ItemsView = itemsView;
		}

		public override void OnViewRecycled(Object holder)
		{
			if (holder is TemplatedItemViewHolder templatedItemViewHolder)
			{
				templatedItemViewHolder.Recycle(ItemsView);
			}
			else if (holder is SimpleViewHolder emptyViewHolder)
			{
				emptyViewHolder.Recycle(ItemsView);
			}

			base.OnViewRecycled(holder);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (EmptyView == null)
			{
				return;
			}

			if (holder is SimpleViewHolder emptyViewHolder && emptyViewHolder.View != null)
			{
				// For templated empty views, this will happen on bind. But if we just have a plain-old View,
				// we need to add it as a "child" of the ItemsView here so that stuff like Visual and FlowDirection
				// propagate to the controls in the EmptyView
				ItemsView.AddLogicalChild(emptyViewHolder.View);
			}
			else if (holder is TemplatedItemViewHolder templatedItemViewHolder && EmptyViewTemplate != null)
			{
				// Use EmptyView as the binding context for the template
				templatedItemViewHolder.Bind(EmptyView, ItemsView);
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			var template = EmptyViewTemplate;

			if (template == null)
			{
				if (!(EmptyView is View formsView))
				{
					// No template, EmptyView is not a Forms View, so just display EmptyView.ToString
					return SimpleViewHolder.FromText(EmptyView?.ToString(), context);
				}

				// EmptyView is a Forms View; display that
				return SimpleViewHolder.FromFormsView(formsView, context, () => parent.Width, () => parent.Height);
			}

			var itemContentView = new SizedItemContentView(parent.Context, () => parent.Width, () => parent.Height);
			return new TemplatedItemViewHolder(itemContentView, template, isSelectionEnabled: false);
		}

		public override int GetItemViewType(int position)
		{
			return _itemViewType;
		}
	}
}