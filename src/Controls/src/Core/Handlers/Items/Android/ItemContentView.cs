#nullable disable
using System;
using Android.Content;
using Android.Views;
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

		AView PlatformView => Content?.ContainerView ?? Content?.PlatformView;

		internal void RealizeContent(View view, ItemsView itemsView)
		{
			Content = CreateHandler(view, itemsView);
			var platformView = PlatformView;

			//make sure we don't belong to a previous Holder
			platformView.RemoveFromParent();
			AddView(platformView);

			if (View is VisualElement visualElement)
			{
				visualElement.MeasureInvalidated += ElementMeasureInvalidated;
			}
		}

		internal void Recycle()
		{
			if (View is VisualElement visualElement)
			{
				visualElement.MeasureInvalidated -= ElementMeasureInvalidated;
			}

			var platformView = PlatformView;

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

			if (View?.Handler is IPlatformViewHandler handler)
			{
				handler.LayoutVirtualView(l, t, r, b);
			}
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

		void ElementMeasureInvalidated(object sender, EventArgs e)
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

		static IPlatformViewHandler CreateHandler(View view, ItemsView itemsView) =>
			TemplateHelpers.GetHandler(view, itemsView.FindMauiContext());
	}
}
