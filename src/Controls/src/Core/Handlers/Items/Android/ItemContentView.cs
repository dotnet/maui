using System;
using Android.Content;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemContentView : ViewGroup
	{
		Size? _size;
		Action<Size> _reportMeasure;

		protected IPlatformViewHandler Content;
		internal IView View => Content?.VirtualView;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void ClickOn() => CallOnClick();

		internal void RealizeContent(View view, ItemsView itemsView)
		{
			Content = CreateHandler(view, itemsView);
			var platformView = Content.ContainerView ?? Content.PlatformView;
			//make sure we don't belong to a previous Holder
			platformView.RemoveFromParent();
			AddView(platformView);

			//TODO: RUI IS THIS THE BEST WAY TO CAST? 
			(View as VisualElement).MeasureInvalidated += ElementMeasureInvalidated;
		}

		internal void Recycle()
		{
			if (View != null)
			{
				(View as VisualElement).MeasureInvalidated -= ElementMeasureInvalidated;
			}

			var platformView = Content?.ContainerView ?? Content?.PlatformView;

			if (platformView != null)
			{
				RemoveView(platformView);
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

			var size = this.FromPixels(r - l, b - t);

			//TODO: RUI Is this the best way?
			//View.Arrange(new Rectangle(Point.Zero, size));
			//Arrange doesn't seem to work as expected

			if (View?.Handler is not IPlatformViewHandler handler)
				return;

			handler.LayoutVirtualView(l, t, r, b);

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
				: this.FromPixels(pixelWidth);

			var height = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: this.FromPixels(pixelHeight);


			var measure = View.Measure(width, height);

			if (pixelWidth == 0)
			{
				pixelWidth = (int)this.ToPixels(measure.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)this.ToPixels(measure.Height);
			}

			_reportMeasure?.Invoke(new Size(pixelWidth, pixelHeight));
			_reportMeasure = null; // Make sure we only report back the measure once

			SetMeasuredDimension(pixelWidth, pixelHeight);
		}

		void ElementMeasureInvalidated(object sender, System.EventArgs e)
		{
			if (this.IsAlive())
			{
				PlatformInterop.RequestLayoutIfNeeded(this);
			}
			else if (sender is VisualElement ve)
			{
				ve.MeasureInvalidated -= ElementMeasureInvalidated;
			}
		}

		void UpdateContentLayout()
		{
			VisualElement mauiControlsView = View as VisualElement;
			AView aview = Content.ToPlatform();

			if (mauiControlsView == null || aview == null)
				return;

			var x = (int)this.ToPixels(mauiControlsView.X);
			var y = (int)this.ToPixels(mauiControlsView.Y);
			var width = Math.Max(0, (int)this.ToPixels(mauiControlsView.Width));
			var height = Math.Max(0, (int)this.ToPixels(mauiControlsView.Height));

			aview.Layout(x, y, width, height);

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
