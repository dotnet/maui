#nullable disable
using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class SimpleViewHolder : RecyclerView.ViewHolder
	{
		global::Android.Views.View _itemView;
		public SimpleViewHolder(global::Android.Views.View itemView, View rootElement) : base(itemView)
		{
			_itemView = itemView;
			View = rootElement;
		}

		public View View { get; }

		public void Recycle(ItemsView itemsView)
		{
			if (_itemView is SizedItemContentView _sizedItemContentView)
			{
				_sizedItemContentView.Recycle();
			}
			itemsView.RemoveLogicalChild(View);
		}

		public static SimpleViewHolder FromText(string text, Context context, Func<double> width = null, Func<double> height = null, ItemsView container = null, bool fill = true, bool isEmptyView = false)
		{
			if (fill)
			{
				// When displaying an EmptyView with Header and Footer, we need to account for the Header and Footer sizes in layout calculations.
				// This prevents the EmptyView from occupying the full remaining space. 
				Label label = new Label() { Text = text, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
				SizedItemContentView itemContentControl = CreateSizedItemContentView(context, width, height, isEmptyView);
				itemContentControl.RealizeContent(label, container);
				return new SimpleViewHolder(itemContentControl, null);
			}

			TextView textView = new TextView(context) { Text = text, Gravity = GravityFlags.Center };
			return new SimpleViewHolder(textView, null);
		}

		public static SimpleViewHolder FromFormsView(View formsView, Context context, Func<double> width, Func<double> height, ItemsView container, bool isEmptyView = false)
		{
			var itemContentControl = CreateSizedItemContentView(context, width, height, isEmptyView);

			// Make sure the Visual property is available during renderer creation
			Internals.PropertyPropagationExtensions.PropagatePropertyChanged(null, formsView, container);
			itemContentControl.RealizeContent(formsView, container);

			return new SimpleViewHolder(itemContentControl, formsView);
		}

		public static SimpleViewHolder FromFormsView(View formsView, Context context, ItemsView container)
		{
			var itemContentControl = new ItemContentView(context);
			itemContentControl.RealizeContent(formsView, container);
			return new SimpleViewHolder(itemContentControl, formsView);
		}

		static SizedItemContentView CreateSizedItemContentView(Context context, Func<double> width, Func<double> height, bool isEmptyView)
		{
			return isEmptyView
				? new EmptyViewContentView(context, width, height)
				: new SizedItemContentView(context, width, height);
		}
	}
}
