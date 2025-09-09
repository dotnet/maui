#nullable disable
using System;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemContentView : ViewGroup
	{
		Size? _pixelSize;
		WeakReference _reportMeasure;
		WeakReference _retrieveStaticSize;
		int _previousPixelWidth = -1;
		int _previousPixelHeight = -1;

		Action<Size> ReportMeasure
		{
			get => _reportMeasure?.Target as Action<Size>;
		}

		protected IPlatformViewHandler Content;
		internal IView View => Content?.VirtualView;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void ClickOn() => CallOnClick();

		AView PlatformView => Content?.ContainerView ?? Content?.PlatformView;

		internal Func<Size?> RetrieveStaticSize
		{
			get => _retrieveStaticSize?.Target as Func<Size?>;
			set => _retrieveStaticSize = new WeakReference(value);
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (!IsHeaderOrFooterContent())
			{
				return base.DispatchTouchEvent(e);
			}

			bool result = base.DispatchTouchEvent(e);

			if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
			{
				Parent?.RequestDisallowInterceptTouchEvent(false);
			}

			return result;
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (!IsHeaderOrFooterContent())
			{
				return base.OnInterceptTouchEvent(ev);
			}

			if (ev.Action == MotionEventActions.Down)
			{
				Parent?.RequestDisallowInterceptTouchEvent(true);
				return false;
			}

			return base.OnInterceptTouchEvent(ev);
		}

		/// <summary>
		/// Determines if this ItemContentView is being used for header or footer content
		/// by checking if the contained View is the same as the ItemsView's Header or Footer
		/// </summary>
		private bool IsHeaderOrFooterContent()
		{
			if (View == null)
				return false;

			// Find the parent ItemsView by traversing up the view hierarchy
			var itemsView = FindParentItemsView();
			if (itemsView is StructuredItemsView structuredItemsView)
			{
				// Check if our View is the same object reference as the header or footer
				return ReferenceEquals(View, structuredItemsView.Header) ||
					   ReferenceEquals(View, structuredItemsView.Footer);
			}

			return false;
		}

		/// <summary>
		/// Finds the parent StructuredItemsView by traversing the logical parent chain
		/// </summary>
		private StructuredItemsView FindParentItemsView()
		{
			var current = View?.Parent;
			while (current != null)
			{
				if (current is StructuredItemsView itemsView)
					return itemsView;
				current = current.Parent;
			}
			return null;
		}

			return platformView.CanScrollVertically(1) || platformView.CanScrollVertically(-1) ||
				   platformView.CanScrollHorizontally(1) || platformView.CanScrollHorizontally(-1);
		}

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
			_pixelSize = null;
			_reportMeasure = null;
			_previousPixelWidth = -1;
			_previousPixelHeight = -1;
		}

		internal void HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size)
		{
			_reportMeasure = new WeakReference(reportMeasure);
			_pixelSize = size;
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

			int pixelWidth = MeasureSpec.GetSize(widthMeasureSpec);
			int pixelHeight = MeasureSpec.GetSize(heightMeasureSpec);
			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			// This checks to see if a static size has been set already on the adapter
			// The problem is that HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size) 
			// is called during "Bind" and then measure is called on all visible cells after that
			// the result of this is that every single cell does an individual measure opposed to just using
			// the first cell to set the size.

			var possibleNewSize = RetrieveStaticSize?.Invoke();

			// This means a different cell has already set our new size
			// so let's just use that instead of perform our own speculative measure
			if (possibleNewSize is not null &&
				_pixelSize is not null &&
				!_pixelSize.Equals(possibleNewSize))
			{
				_pixelSize = possibleNewSize;
			}
			else
			{
				_pixelSize = _pixelSize ?? possibleNewSize;

				// If the measure changes significantly, we need to invalidate the pixel size
				// This will happen if the user rotates the device or even just changes the height/width
				// on the CollectionView itself. 
				// The Abs comparison I think is currently just a workaround for this
				// https://github.com/dotnet/maui/issues/22271
				// Once we fix 22271 I don't think we need this
				// I'm also curious if we would still need this https://github.com/dotnet/maui/pull/21140
				if (pixelWidth != 0 && Math.Abs(_previousPixelWidth - pixelWidth) > 1)
				{
					// We only need to worry about clearing pixel size if we've
					// already made a first pass
					if (_previousPixelWidth != -1)
					{
						_pixelSize = null;
					}
				}


				if (pixelHeight != 0 && Math.Abs(_previousPixelHeight - pixelHeight) > 1)
				{
					// We only need to worry about clearing pixel size if we've
					// already made a first pass
					if (_previousPixelHeight != -1)
					{
						_pixelSize = null;
					}

					_pixelSize = null;
				}
			}

			if (pixelWidth != 0)
			{
				_previousPixelWidth = pixelWidth;
			}

			if (pixelHeight != 0)
			{
				_previousPixelHeight = pixelHeight;
			}

			// If we're using ItemSizingStrategy.MeasureFirstItem and now we have a set size, use that
			// Even though we already know the size we still need to pass the measure through to the children.
			if (_pixelSize is not null)
			{
				// If the platform childs measure has been invalidated, it's going to still want to
				// participate in the measure lifecycle in order to update its internal
				// book keeping.
				_ = (View.Handler as IPlatformViewHandler)?.MeasureVirtualView(
						MeasureSpec.MakeMeasureSpec((int)_pixelSize.Value.Width, MeasureSpecMode.Exactly),
						MeasureSpec.MakeMeasureSpec((int)_pixelSize.Value.Height, MeasureSpecMode.Exactly)
					);

				SetMeasuredDimension((int)_pixelSize.Value.Width, (int)_pixelSize.Value.Height);
				return;
			}

			var width = widthMode == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: this.FromPixels(pixelWidth);

			var height = heightMode == MeasureSpecMode.Unspecified
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

			ReportMeasure?.Invoke(new Size(pixelWidth, pixelHeight));

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
