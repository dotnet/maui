#nullable disable
using System;
using Android.Content;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemsViewAdapter<TItemsView, TItemsViewSource> : RecyclerView.Adapter
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected readonly TItemsView ItemsView;
		readonly Func<View, Context, ItemContentView> _createItemContentView;
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
				templatedItemViewHolder.Recycle(ItemsView);
			}

			base.OnViewRecycled(holder);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			switch (holder)
			{
				case TextViewHolder textViewHolder:
					textViewHolder.TextView.Text = ItemsSource.GetItem(position).ToString();
					break;
				case TemplatedItemViewHolder templatedItemViewHolder:
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource.GetItem(position));
					break;
			}
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
