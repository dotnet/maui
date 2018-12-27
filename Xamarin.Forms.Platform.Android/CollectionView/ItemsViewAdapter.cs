using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Widget;
using AView = Android.Views.View;
using Object = Java.Lang.Object;
using ViewGroup = Android.Views.ViewGroup;

namespace Xamarin.Forms.Platform.Android
{
	// TODO hartez 2018/07/25 14:43:04 Experiment with an ItemSource property change as _adapter.notifyDataSetChanged	

	public class ItemsViewAdapter : RecyclerView.Adapter
	{
		protected readonly ItemsView ItemsView;
		readonly Func<IVisualElementRenderer, Context, AView> _createView;
		internal readonly IItemsViewSource ItemsSource;

		internal ItemsViewAdapter(ItemsView itemsView, Func<IVisualElementRenderer, Context, AView> createView = null)
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewAdapter));

			ItemsView = itemsView;
			_createView = createView;
			ItemsSource = ItemsSourceFactory.Create(itemsView.ItemsSource, this);

			if (_createView == null)
			{
				_createView = (renderer, context) => new ItemContentView(renderer, context);
			}
		}

		public override void OnViewRecycled(Object holder)
		{
			if (holder is TemplatedItemViewHolder templatedItemViewHolder)
			{
				templatedItemViewHolder.View.Parent = null;
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
				case TemplatedItemViewHolder templateViewHolder:
					templateViewHolder.View.Parent = ItemsView;
					BindableObject.SetInheritedBindingContext(templateViewHolder.View, ItemsSource[position]);
					break;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			// Does the ItemsView have a DataTemplate?
			var template = ItemsView.ItemTemplate;
			if (template == null)
			{
				// No template, just use the ToString view
				var view = new TextView(context);
				return new TextViewHolder(view);
			}

			// Realize the content, create a renderer out of it, and use that
			var templateElement = (View)template.CreateContent();
			var itemContentControl = _createView(CreateRenderer(templateElement, context), context);

			return new TemplatedItemViewHolder(itemContentControl, templateElement);
		}

		static IVisualElementRenderer CreateRenderer(View view, Context context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var renderer = Platform.CreateRenderer(view, context);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}

		public override int ItemCount => ItemsSource.Count;

		public override int GetItemViewType(int position)
		{
			// TODO hartez We might be able to turn this to our own purposes
			// We might be able to have the CollectionView signal the adapter if the ItemTemplate property changes
			// And as long as it's null, we return a value to that effect here
			// Then we don't have to check _itemsView.ItemTemplate == null in OnCreateViewHolder, we can just use
			// the viewType parameter.
			return 42;
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