using System;
using Android.Content;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using Android.Widget;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;

namespace Xamarin.Forms.Platform.Android
{
	public class ItemsViewAdapter<TItemsView, TItemsViewSource> : RecyclerView.Adapter 
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected readonly TItemsView ItemsView;
		readonly Func<View, Context, ItemContentView> _createItemContentView;
		internal TItemsViewSource ItemsSource;

		bool _disposed;
		bool _usingItemTemplate = false;

		internal ItemsViewAdapter(TItemsView itemsView, Func<View, Context, ItemContentView> createItemContentView = null)
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
			if (property.Is(Xamarin.Forms.ItemsView.ItemsSourceProperty))
			{
				UpdateItemsSource();
			}
			else if (property.Is(Xamarin.Forms.ItemsView.ItemTemplateProperty))
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

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == ItemViewType.TextItem)
			{
				var view = new TextView(context);
				return new TextViewHolder(view);
			}

			var itemContentView = _createItemContentView.Invoke(ItemsView, context);

			return new TemplatedItemViewHolder(itemContentView, ItemsView.ItemTemplate);
		}

		public override int ItemCount => ItemsSource.Count;

		public override int GetItemViewType(int position)
		{
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
			return ItemsSource.GetPosition(item);
		}

		protected virtual void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			templatedItemViewHolder.Bind(context, ItemsView);
		}

		void UpdateItemsSource()
		{
			ItemsSource?.Dispose();

			ItemsSource = CreateItemsSource();
		}

		void UpdateUsingItemTemplate()
		{
			_usingItemTemplate = ItemsView.ItemTemplate != null;
		}
	}
}