using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Widget;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;
using ASize = Android.Util.Size;

namespace Xamarin.Forms.Platform.Android
{
	public class ItemsViewAdapter : RecyclerView.Adapter
	{
		protected readonly ItemsView ItemsView;
		readonly Func<View, Context, ItemContentView> _createItemContentView;
		internal readonly IItemsViewSource ItemsSource;

		bool _disposed;
		ASize _size;

		bool _usingItemTemplate = false;
		int _headerOffset = 0;
		bool _hasFooter;

		internal ItemsViewAdapter(ItemsView itemsView, Func<View, Context, ItemContentView> createItemContentView = null)
		{
			Xamarin.Forms.CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewAdapter));

			ItemsView = itemsView ?? throw new ArgumentNullException(nameof(itemsView));

			UpdateUsingItemTemplate();
			UpdateHeaderOffset();
			UpdateHasFooter();

			ItemsView.PropertyChanged += ItemsViewPropertyChanged;

			_createItemContentView = createItemContentView;
			ItemsSource = ItemsSourceFactory.Create(itemsView.ItemsSource, this);

			if (_createItemContentView == null)
			{
				_createItemContentView = (view, context) => new ItemContentView(context);
			}
		}

		private void ItemsViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs property)
		{
			if (property.Is(ItemsView.HeaderProperty))
			{
				UpdateHeaderOffset();
			}
			else if (property.Is(ItemsView.ItemTemplateProperty))
			{
				UpdateUsingItemTemplate();
			}
			else if (property.Is(ItemsView.ItemTemplateProperty))
			{
				UpdateUsingItemTemplate();
			}
			else if (property.Is(ItemsView.FooterProperty))
			{
				UpdateHasFooter();
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
			if (IsHeader(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsView.Header);
				}

				return;
			}

			if (IsFooter(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsView.Footer);
				}

				return;
			}

			var itemsSourcePosition = position - _headerOffset;

			switch (holder)
			{
				case TextViewHolder textViewHolder:
					textViewHolder.TextView.Text = ItemsSource[itemsSourcePosition].ToString();
					break;
				case TemplatedItemViewHolder templatedItemViewHolder:
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource[itemsSourcePosition]);
					break;
			}
		}

		void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			if (ItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				templatedItemViewHolder.Bind(context, ItemsView, SetStaticSize, _size);
			}
			else
			{
				templatedItemViewHolder.Bind(context, ItemsView);
			}
		}

		void SetStaticSize(ASize size)
		{
			_size = size;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == ItemViewType.Header)
			{
				return CreateHeaderFooterViewHolder(ItemsView.Header, ItemsView.HeaderTemplate, context);
			}

			if (viewType == ItemViewType.Footer)
			{
				return CreateHeaderFooterViewHolder(ItemsView.Footer, ItemsView.FooterTemplate, context);
			}

			if (viewType == ItemViewType.TextItem)
			{
				var view = new TextView(context);
				return new TextViewHolder(view);
			}

			var itemContentView = new ItemContentView(context);
			return new TemplatedItemViewHolder(itemContentView, ItemsView.ItemTemplate);
		}

		public override int ItemCount => ItemsSource.Count + _headerOffset + (_hasFooter ? 1 : 0);

		public override int GetItemViewType(int position)
		{
			if (IsHeader(position))
			{
				return ItemViewType.Header;
			}

			if (IsFooter(position))
			{
				return ItemViewType.Footer;
			}

			if (_usingItemTemplate)
			{
				return ItemViewType.TemplatedItem;
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

		public virtual int GetPositionForItem(object item)
		{
			for (int n = 0; n < ItemsSource.Count; n++)
			{
				if (ItemsSource[n] == item)
				{
					return n + _headerOffset;
				}
			}

			return -1;
		}

		void UpdateUsingItemTemplate()
		{
			_usingItemTemplate = ItemsView.ItemTemplate != null;
		}

		void UpdateHeaderOffset()
		{
			_headerOffset = ItemsView.Header == null ? 0 : 1;
		}

		void UpdateHasFooter()
		{
			_hasFooter = ItemsView.Footer != null;
		}

		bool IsHeader(int position)
		{
			return _headerOffset > 0 && position == 0;
		}

		bool IsFooter(int position)
		{
			return _hasFooter && position > ItemsSource.Count;
		}

		RecyclerView.ViewHolder CreateHeaderFooterViewHolder(object content, DataTemplate template, Context context)
		{
			if (template != null)
			{
				var footerContentView = new ItemContentView(context);
				return new TemplatedItemViewHolder(footerContentView, template);
			}

			if (content is View formsView)
			{
				return SimpleViewHolder.FromFormsView(formsView, context);
			}

			// No template, Footer is not a Forms View, so just display Footer.ToString
			return SimpleViewHolder.FromText(content?.ToString(), context, fill: false);
		}
	}
}