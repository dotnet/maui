#nullable disable
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ContainerView : ViewGroup
	{
		int _parentTopPadding;
		View _view;
		ShellViewRenderer _shellContentView;
		readonly IMauiContext _mauiContext;
		internal AView PlatformView => _view?.Handler?.PlatformView as AView;
		internal Graphics.Size DesiredSize { get; private set; }
		internal Graphics.Size MinimumSize { get; private set; }

		public ContainerView(Context context, View view, IMauiContext mauiContext) : base(context)
		{
			_mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
			View = view;
		}

		public bool MatchHeight { get; set; }

		internal bool MeasureHeight { get; set; }

		public bool MatchWidth { get; set; }

		public View View
		{
			get { return _view; }
			set
			{
				if (_view == value)
					return;

				_view = value;
				OnViewSet(value);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_shellContentView?.TearDown();
				_view = null;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_shellContentView == null || View?.Handler == null)
			{
				return;
			}

			((IPlatformViewHandler)View.Handler).LayoutVirtualView(l, t, r, b);
		}

		// This indicates the amount of top padding the parent wants this control to have. This
		// allows us to adjust the height measured so that the parent container maintains a constant height.
		//
		// This is mainly used with ShellHeaderFlyoutBehavior.CollapseOnScroll
		// The container added to the AppBarLayout needs to maintain a constant height otherwise the 
		// OnOffsetChanged value passed in to IOnOffsetChangedListener will just infinitely change as the
		// header size keeps changing. So in order to make the collapse work we just have to resize the xplat view
		// and then shift it down in the container.
		//
		// In theory we could call `IView.Arrange` with the desired position in the parent but then that sets the xplat `Frame`
		// to values that don't really make any sense from an xplat perspective. This just causes implementation details
		// to leak up into the abstraction. It'll just be confusing that the headers position is "0,300" even though it looks
		// like it's "0,0"
		internal void SetParentTopPadding(int v)
		{
			if (_parentTopPadding == v)
				return;

			_parentTopPadding = v;
			this.MaybeRequestLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (View == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}
			if (!View.IsVisible)
			{
				View.Measure(0, 0);
				SetMeasuredDimension(0, 0);
				return;
			}

			var width = widthMeasureSpec.GetSize();
			var height = heightMeasureSpec.GetSize();

			var measureWidth = width > 0 ? MeasureSpecMode.Exactly.MakeMeasureSpec(width) : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			var measureHeight = height > 0 ? MeasureSpecMode.Exactly.MakeMeasureSpec(height) : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);

			double? maxHeight = null;

			if (MeasureHeight)
			{
				measureHeight = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			}
			else if (MatchWidth)
			{
				measureWidth = widthMeasureSpec;
			}
			else if (MatchHeight)
			{
				measureHeight = heightMeasureSpec;
				maxHeight = heightMeasureSpec.GetSize();
			}

			var size = _shellContentView.Measure(measureWidth, measureHeight, null, (int?)maxHeight);

			if (size.Height < this.MinimumHeight)
				size = new Graphics.Size(size.Width, this.MinimumHeight);

			DesiredSize = size;
			var newHeight = size.Height;

			if (_parentTopPadding > 0)
			{
				newHeight -= _parentTopPadding;
			}

			if (newHeight != size.Height)
			{
				// Android treats zero as unspecified so we need to set the height to at least 1.
				// If there's a view that's able to squish/stretch then passing in zero will give it's stretched size
				// but what we want here is the squished size.
				var minHeightMeasureSpec = 1;

				if (MinimumHeight > 0)
					minHeightMeasureSpec = MinimumHeight;

				if (newHeight <= 0)
				{
					newHeight = minHeightMeasureSpec;
				}

				// For collapse on scroll we need to know the minimum size the view can reach so we can set the minimum size
				// on the headers container. This is the only way to get the AppBarLayout collapse behavior to stop producing
				// offset changes once the header has reached the minimum size it can get to
				MinimumSize = _shellContentView.Measure(measureWidth, MeasureSpecMode.Exactly.MakeMeasureSpec(minHeightMeasureSpec), null, (int?)maxHeight);

				// If the offset of the appbar layout hasn't reached the minimum size then 
				// we use the new collapsed size to remeasure so everything is the correct size
				if (newHeight > minHeightMeasureSpec)
				{
					size = _shellContentView.Measure(measureWidth, MeasureSpecMode.Exactly.MakeMeasureSpec((int)newHeight), null, (int?)maxHeight);
				}
				else
				{
					size = MinimumSize;
				}
			}
			else
			{
				this.MinimumSize = new Graphics.Size((int)size.Width, this.MinimumHeight);
			}

			SetMeasuredDimension((int)size.Width, (int)size.Height);
		}

		protected virtual void OnViewSet(View view)
		{
			if (_shellContentView == null)
				_shellContentView = new ShellViewRenderer(this.Context, view, _mauiContext);
			else
				_shellContentView.OnViewSet(view);

			if (_shellContentView.PlatformView != null)
				AddView(_shellContentView.PlatformView);
		}
	}
}
