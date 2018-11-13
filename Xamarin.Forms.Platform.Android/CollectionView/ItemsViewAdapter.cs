using System;
using System.Diagnostics;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using ViewGroup = Android.Views.ViewGroup;

namespace Xamarin.Forms.Platform.Android
{
	// TODO hartez 2018/07/25 14:39:29 Split up CollectionViewAdapter into one for templates, one for text	
	// TODO hartez 2018/07/25 14:43:04 Experiment with an ItemSource property change as _adapter.notifyDataSetChanged	
	// TODO hartez 2018/07/25 14:44:15 Template property changed should do a whole new adapter; and that way we can cache the template

	public class ItemsViewAdapter : RecyclerView.Adapter
	{
		protected readonly ItemsView ItemsView;
		readonly Context _context;
		readonly Func<IVisualElementRenderer, Context, AView> _createView;
		readonly IItemsViewSource _itemsSource;

		internal ItemsViewAdapter(ItemsView itemsView, Context context, 
			Func<IVisualElementRenderer, Context, AView> createView = null)
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewAdapter));

			ItemsView = itemsView;
			_context = context;
			_createView = createView;
			_itemsSource = ItemsSourceFactory.Create(itemsView.ItemsSource, this);

			if (_createView == null)
			{
				_createView = (renderer, context1) => new ItemContentControl(renderer, context1);
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			switch (holder)
			{
				case TextViewHolder textViewHolder:
					textViewHolder.TextView.Text = _itemsSource[position].ToString();
					break;
				case TemplatedItemViewHolder templateViewHolder:
					BindableObject.SetInheritedBindingContext(templateViewHolder.View, _itemsSource[position]);
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
			var templateElement = template.CreateContent() as View;
			var itemContentControl = _createView(CreateRenderer(templateElement, context), context);

			return new TemplatedItemViewHolder(itemContentControl, templateElement);
		}

		IVisualElementRenderer CreateRenderer(View view, Context context)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			var renderer = Platform.CreateRenderer(view, context);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}

		public override int ItemCount => _itemsSource.Count;

		public override int GetItemViewType(int position)
		{
			// TODO hartez We might be able to turn this to our own purposes
			// We might be able to have the CollectionView signal the adapter if the ItemTemplate property changes
			// And as long as it's null, we return a value to that effect here
			// Then we don't have to check _itemsView.ItemTemplate == null in OnCreateViewHolder, we can just use
			// the viewType parameter.
			return 42;
		}

		internal class TextViewHolder : RecyclerView.ViewHolder
		{
			public TextView TextView { get; }

			public TextViewHolder(TextView itemView) : base(itemView)
			{
				TextView = itemView;
			}
		}

		internal class TemplatedItemViewHolder : RecyclerView.ViewHolder
		{
			public View View { get; }

			public TemplatedItemViewHolder(AView itemView, View rootElement) : base(itemView)
			{
				View = rootElement;
			}
		}

		public virtual int GetPositionForItem(object item)
		{
			for (int n = 0; n < _itemsSource.Count; n++)
			{
				if (_itemsSource[n] == item)
				{
					return n;
				}
			}

			return -1;
		}
	}
	
}