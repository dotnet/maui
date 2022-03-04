using System;
using Android.Content;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemContentView : ViewGroup
	{
		protected IPlatformViewHandler Content;
		internal IView View => Content?.VirtualView;
		Size? _size;
		Action<Size> _reportMeasure;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void ClickOn() => CallOnClick();

		internal void RealizeContent(View view, ItemsView itemsView)
		{
			Content = CreateHandler(view, itemsView);
			AddView(Content.PlatformView);

			//TODO: RUI IS THIS THE BEST WAY TO CAST? 
			(View as VisualElement).MeasureInvalidated += ElementMeasureInvalidated;
		}

		internal void Recycle()
		{
			if (View != null)
			{
				(View as VisualElement).MeasureInvalidated -= ElementMeasureInvalidated;
			}

			if (Content?.PlatformView != null)
			{
				RemoveView(Content.PlatformView);
			}

			Content = null;
			_size = null;
		}

		internal void HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size)
		{
			_reportMeasure = reportMeasure;
			_size = size;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Content == null)
			{
				return;
			}

			var size = Context.FromPixels(r - l, b - t);

			//TODO: RUI Is this the best way?
			//View.Arrange(new Rectangle(Point.Zero, size));
			//Arrange doesn't seem to work as expected

			var mauiControlsView = View as View;
			if (mauiControlsView == null)
				return;

			mauiControlsView.Layout(new Rect(Point.Zero, size));

			UpdateContentLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Content == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}

			if (_size != null)
			{
				// If we're using ItemSizingStrategy.MeasureFirstItem and now we have a set size, use that
				SetMeasuredDimension((int)_size.Value.Width, (int)_size.Value.Height);
				return;
			}

			int pixelWidth = MeasureSpec.GetSize(widthMeasureSpec);
			int pixelHeight = MeasureSpec.GetSize(heightMeasureSpec);

			var width = MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: Context.FromPixels(pixelWidth);

			var height = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: Context.FromPixels(pixelHeight);

			SizeRequest measure = (View as VisualElement).Measure(width, height, MeasureFlags.IncludeMargins);

			if (pixelWidth == 0)
			{
				pixelWidth = (int)Context.ToPixels(measure.Request.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)Context.ToPixels(measure.Request.Height);
			}

			_reportMeasure?.Invoke(new Size(pixelWidth, pixelHeight));
			_reportMeasure = null; // Make sure we only report back the measure once

			SetMeasuredDimension(pixelWidth, pixelHeight);
		}

		void ElementMeasureInvalidated(object sender, System.EventArgs e)
		{
			if (this.IsAlive())
			{
				RequestLayout();
			}
			else if (sender is VisualElement ve)
			{
				ve.MeasureInvalidated -= ElementMeasureInvalidated;
			}
		}

		void UpdateContentLayout()
		{
			VisualElement mauiControlsView = (View as VisualElement);
			AView aview = Content.PlatformView;

			if (mauiControlsView == null || aview == null)
				return;

			var x = (int)Context.ToPixels(mauiControlsView.X);
			var y = (int)Context.ToPixels(mauiControlsView.Y);
			var width = Math.Max(0, (int)Context.ToPixels(mauiControlsView.Width));
			var height = Math.Max(0, (int)Context.ToPixels(mauiControlsView.Height));

			Content.PlatformView.Layout(x, y, width, height);

			if ((aview is LayoutViewGroup || aview is ContentViewGroup || aview is CoordinatorLayout || aview is FragmentContainerView) && width == 0 && height == 0)
			{
				// Nothing to do here; just chill.
			}
			else
			{
				aview.Measure(MeasureSpecMode.Exactly.MakeMeasureSpec(width), MeasureSpecMode.Exactly.MakeMeasureSpec(height));
				aview.Layout(x, y, x + width, y + height);
			}
		}

		static IPlatformViewHandler CreateHandler(View view, ItemsView itemsView) =>
			TemplateHelpers.GetHandler(view, itemsView.FindMauiContext());
	}
}
