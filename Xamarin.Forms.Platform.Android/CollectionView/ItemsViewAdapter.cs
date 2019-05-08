using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Widget;
using AView = Android.Views.View;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;

namespace Xamarin.Forms.Platform.Android
{
	public class ItemsViewAdapter : RecyclerView.Adapter
	{
		const int TextView = 41;
		const int TemplatedView = 42;

		protected readonly ItemsView ItemsView;
		readonly Func<View, Context, ItemContentView> _createItemContentView;
		internal readonly IItemsViewSource ItemsSource;
		bool _disposed;

		internal ItemsViewAdapter(ItemsView itemsView, Func<View, Context, ItemContentView> createItemContentView = null)
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewAdapter));

			ItemsView = itemsView;
			_createItemContentView = createItemContentView;
			ItemsSource = ItemsSourceFactory.Create(itemsView.ItemsSource, this);

			if (_createItemContentView == null)
			{
				_createItemContentView = (view, context) => new ItemContentView(context);
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
					textViewHolder.TextView.Text = ItemsSource[position].ToString();
					break;
				case TemplatedItemViewHolder templatedItemViewHolder:
					templatedItemViewHolder.Bind(ItemsSource[position], ItemsView);
					break;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if(viewType == TextView)
			{
				var view = new TextView(context);
				return new TextViewHolder(view);
			}

			var itemContentView = new ItemContentView(parent.Context);
			return new TemplatedItemViewHolder(itemContentView, ItemsView.ItemTemplate);
		}

		public override int ItemCount => ItemsSource.Count;

		public override int GetItemViewType(int position)
		{
			// Does the ItemsView have a DataTemplate?
			// TODO ezhart We could probably cache this instead of having to GetValue every time
			if (ItemsView.ItemTemplate == null)
			{
				// No template, just use the Text view
				return TextView;
			}

			return TemplatedView;
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					ItemsSource?.Dispose();
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
					return n;
				}
			}

			return -1;
		}
	}
}