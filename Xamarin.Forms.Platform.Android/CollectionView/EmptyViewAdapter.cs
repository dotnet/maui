using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class EmptyViewAdapter : RecyclerView.Adapter
	{
		public object EmptyView { get; set; }
		public DataTemplate EmptyViewTemplate { get; set; }

		public override int ItemCount => 1;

		public EmptyViewAdapter()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(EmptyViewAdapter));
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (EmptyView == null || EmptyViewTemplate == null)
			{
				return;
			}

			if (!(holder is EmptyViewHolder emptyViewHolder))
			{
				return;
			}

			// Use EmptyView as the binding context for the template
			BindableObject.SetInheritedBindingContext(emptyViewHolder.View, EmptyView);
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (EmptyViewTemplate == null)
			{
				if (!(EmptyView is View formsView))
				{
					// No template, EmptyView is not a Forms View, so just display EmptyView.ToString
					return new EmptyViewHolder(CreateTextView(EmptyView?.ToString(), context), null);
				}

				// EmptyView is a Forms View; display that
				var itemContentControl = new SizedItemContentControl(CreateRenderer(formsView, context), context,
					() => parent.Width, () => parent.Height);
				return new EmptyViewHolder(itemContentControl, formsView);
			}

			// We have a template, so create a view from it
			var templateElement = EmptyViewTemplate.CreateContent() as View;
			var templatedItemContentControl = new SizedItemContentControl(CreateRenderer(templateElement, context), context, () => parent.Width, () => parent.Height);
			return new EmptyViewHolder(templatedItemContentControl, templateElement);
		}

		IVisualElementRenderer CreateRenderer(View view, Context context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			var renderer = Platform.CreateRenderer(view, context);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}

		static TextView CreateTextView(string text, Context context)
		{
			var textView = new TextView(context) { Text = text };
			var layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
				ViewGroup.LayoutParams.MatchParent);
			textView.LayoutParameters = layoutParams;
			textView.Gravity = GravityFlags.Center;
			return textView;
		}

		internal class EmptyViewHolder : RecyclerView.ViewHolder
		{
			public EmptyViewHolder(global::Android.Views.View itemView, View rootElement) : base(itemView)
			{
				View = rootElement;
			}

			public View View { get; }
		}
	}
}