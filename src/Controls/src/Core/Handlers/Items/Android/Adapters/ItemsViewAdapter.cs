#nullable disable
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal interface IItemsViewAdapter
	{
		void UnbindAllItems();
		void UnbindItemRange(int startIndex, int count);
	}

	public class ItemsViewAdapter<TItemsView, TItemsViewSource> : RecyclerView.Adapter, IItemsViewAdapter
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected readonly TItemsView ItemsView;
		readonly Func<View, Context, ItemContentView> _createItemContentView;
		readonly Dictionary<TemplatedItemViewHolder, int> _boundViewHolders = new();
		protected internal TItemsViewSource ItemsSource;

		bool _disposed;
		bool _usingItemTemplate = false;
		DataTemplateSelector _itemTemplateSelector = null;

		protected internal ItemsViewAdapter(TItemsView itemsView, Func<View, Context, ItemContentView> createItemContentView = null)
		{
			ItemsView = itemsView ?? throw new ArgumentNullException(nameof(itemsView));

			UpdateUsingItemTemplate();

			ItemsView.PropertyChanged += ItemsViewPropertyChanged;

			_createItemContentView = createItemContentView;
			ItemsSource = CreateItemsSource();

			if (_createItemContentView == null)
			{
				_createItemContentView = (view, context) => new ItemContentView(context);
			}
		}

		protected virtual TItemsViewSource CreateItemsSource()
		{
			return (TItemsViewSource)ItemsSourceFactory.Create(ItemsView, this);
		}

		protected virtual void ItemsViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs property)
		{
			if (property.Is(Microsoft.Maui.Controls.ItemsView.ItemTemplateProperty))
			{
				UpdateUsingItemTemplate();
			}
		}

		public override void OnViewRecycled(Object holder)
		{
			if (holder is TemplatedItemViewHolder templatedItemViewHolder)
			{
				_boundViewHolders.Remove(templatedItemViewHolder);
				templatedItemViewHolder.Recycle(ItemsView);
			}

			base.OnViewRecycled(holder);
		}

		void IItemsViewAdapter.UnbindItemRange(int startIndex, int count)
		{
			var endIndex = startIndex + count;
			var boundViewHolders = new KeyValuePair<TemplatedItemViewHolder, int>[_boundViewHolders.Count];
			((ICollection<KeyValuePair<TemplatedItemViewHolder, int>>)_boundViewHolders).CopyTo(boundViewHolders, 0);

			for (int n = 0; n < boundViewHolders.Length; n++)
			{
				var viewHolder = boundViewHolders[n].Key;
				var position = viewHolder.BindingAdapterPosition;
				if (position == RecyclerView.NoPosition)
				{
					position = boundViewHolders[n].Value;
				}

				if (position >= startIndex && position < endIndex)
				{
					viewHolder.Unbind(ItemsView);
					_boundViewHolders.Remove(viewHolder);
				}
			}
		}

		void IItemsViewAdapter.UnbindAllItems()
		{
			foreach (var viewHolder in _boundViewHolders.Keys)
			{
				viewHolder.Unbind(ItemsView);
			}

			_boundViewHolders.Clear();
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			switch (holder)
			{
				case TextViewHolder textViewHolder:
					textViewHolder.TextView.Text = ItemsSource.GetItem(position).ToString();
					break;
				case TemplatedItemViewHolder templatedItemViewHolder:
					TrackBoundViewHolder(templatedItemViewHolder, position);
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource.GetItem(position));
					break;
			}
		}

		private protected void TrackBoundViewHolder(TemplatedItemViewHolder viewHolder, int position)
		{
			_boundViewHolders[viewHolder] = position;
		}

		protected virtual bool IsSelectionEnabled(ViewGroup parent, int viewType) => true;

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == ItemViewType.TextItem)
			{
				var view = new TextView(context);
				return new TextViewHolder(view, IsSelectionEnabled(parent, viewType));
			}

			var itemContentView = _createItemContentView.Invoke(ItemsView, context);

			// See if our cached templates have a match
			if (_viewTypeDataTemplates.TryGetValue(viewType, out var dataTemplate))
			{
				return new TemplatedItemViewHolder(itemContentView, dataTemplate, IsSelectionEnabled(parent, viewType));
			}

			return new TemplatedItemViewHolder(itemContentView, ItemsView.ItemTemplate, IsSelectionEnabled(parent, viewType));
		}

		public override int ItemCount => ItemsSource.Count;

		System.Collections.Generic.Dictionary<int, DataTemplate> _viewTypeDataTemplates = new();

		public override int GetItemViewType(int position)
		{
			if (_usingItemTemplate)
			{
				if (_itemTemplateSelector is null)
					return ItemViewType.TemplatedItem;

				var item = ItemsSource?.GetItem(position);

				var template = _itemTemplateSelector?.SelectTemplate(item, ItemsView);
				var id = template?.Id ?? ItemViewType.TemplatedItem;

				// Cache the data template for future use
				_viewTypeDataTemplates.TryAdd(id, template);
				return id;
			}

			// No template, just use the Text view
			return ItemViewType.TextItem;
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					ItemsSource?.Dispose();
					ItemsView.PropertyChanged -= ItemsViewPropertyChanged;
					_boundViewHolders.Clear();
				}

				_disposed = true;

				base.Dispose(disposing);
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public virtual int GetPositionForItem(object item)
		{
			return ItemsSource.GetPosition(item);
		}

		protected virtual void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			templatedItemViewHolder.Bind(context, ItemsView);
		}

		void UpdateUsingItemTemplate()
		{
			_usingItemTemplate = ItemsView.ItemTemplate != null;
			_itemTemplateSelector = ItemsView.ItemTemplate as DataTemplateSelector;
		}
	}
}
