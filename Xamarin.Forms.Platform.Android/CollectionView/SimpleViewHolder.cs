using System;
using Android.Content;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class SimpleViewHolder : RecyclerView.ViewHolder
	{
		public SimpleViewHolder(global::Android.Views.View itemView, View rootElement) : base(itemView)
		{
			View = rootElement;
		}

		public View View { get; }

		public void Recycle(ItemsView itemsView)
		{
			itemsView.RemoveLogicalChild(View);
		}

		public static SimpleViewHolder FromText(string text, Context context, bool fill = true)
		{
			var textView = new TextView(context) { Text = text };

			if (fill)
			{
				var layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent);
				textView.LayoutParameters = layoutParams;
			}
			
			textView.Gravity = GravityFlags.Center;

			return new SimpleViewHolder(textView, null);
		}

		public static SimpleViewHolder FromFormsView(View formsView, Context context, Func<int> width, Func<int> height)
		{
			var itemContentControl = new SizedItemContentView(context, width, height);
			itemContentControl.RealizeContent(formsView);
			return new SimpleViewHolder(itemContentControl, formsView);
		}

		public static SimpleViewHolder FromFormsView(View formsView, Context context)
		{
			var itemContentControl = new ItemContentView(context);
			itemContentControl.RealizeContent(formsView);
			return new SimpleViewHolder(itemContentControl, formsView);
		}
	}
}