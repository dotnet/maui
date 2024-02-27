using System;
using Gdk;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Point = Microsoft.Maui.Graphics.Point;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.ScrolledWindow.html

	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>, ICrossPlatformLayout
	{

		protected override ScrollView CreatePlatformView()
		{
			var s = new ScrollView();

			return s;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var scrollView = VirtualView;

			var padding = scrollView.Padding;

			if (scrollView.PresentedContent == null)
			{
				return new Size(padding.HorizontalThickness, padding.VerticalThickness);
			}

			// Exclude the padding while measuring the internal content ...
			var measurementWidth = widthConstraint - padding.HorizontalThickness;
			var measurementHeight = heightConstraint - padding.VerticalThickness;

			var result = (scrollView as ICrossPlatformLayout).CrossPlatformMeasure(measurementWidth, measurementHeight);

			// ... and add the padding back in to the final result
			var fullSize = new Size(result.Width + padding.HorizontalThickness, result.Height + padding.VerticalThickness);

			if (double.IsInfinity(widthConstraint))
			{
				widthConstraint = result.Width;
			}

			if (double.IsInfinity(heightConstraint))
			{
				heightConstraint = result.Height;
			}

			return fullSize.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), scrollView.PresentedContent);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			return (VirtualView as ICrossPlatformLayout).CrossPlatformArrange(bounds);
		}
		
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.Content == null)
			{
				return;
			}

			if (handler?.PlatformView is not { } platformView)
				return;

			if (scrollView.Content is not IView contentView)
				return;

			var platformContent = contentView.ToPlatform(handler.MauiContext);
			var child = platformView.Child;

			// check if nativeContent is set as child of Viewport:
			if (child is Gtk.Viewport vp && vp != platformContent)
			{
				child = vp.Child;
			}

			if (child != platformContent)
			{
				platformView.Child = platformContent;

			}
		}

		protected override void ConnectHandler(ScrollView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
			platformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;

			platformView.Vadjustment.ValueChanged += OnNativeViewValueChanged;
			platformView.Hadjustment.ValueChanged += OnNativeViewValueChanged;
			ConnectButtonEvents(platformView);
			ConnectButtonEvents(platformView.VScrollbar);
			ConnectButtonEvents(platformView.HScrollbar);

		}

		protected virtual void ConnectButtonEvents(Widget? widget)
		{
			if (widget == null) return;

			widget.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.ScrollMask | EventMask.SmoothScrollMask;
			widget.ButtonPressEvent += OnNativeViewButtonPressEvent;
			widget.ButtonReleaseEvent += OnNativeViewButtonReleaseEvent;
			widget.ScrollEvent += OnNativeViewScrollEvent;
			widget.MotionNotifyEvent += OnNativeViewMotionNotifyEvent;
		}

		protected virtual void DisconnectButtonEvents(Widget? widget)
		{
			if (widget == null) return;

			widget.ButtonPressEvent -= OnNativeViewButtonPressEvent;
			widget.ButtonReleaseEvent -= OnNativeViewButtonReleaseEvent;
			widget.ScrollEvent -= OnNativeViewScrollEvent;
			widget.MotionNotifyEvent -= OnNativeViewMotionNotifyEvent;

		}

		protected override void DisconnectHandler(ScrollView platformView)
		{
			base.OnDisconnectHandler(platformView);

			platformView.CrossPlatformArrange = null;
			platformView.CrossPlatformMeasure = null;

			platformView.Vadjustment.ValueChanged -= OnNativeViewValueChanged;
			platformView.Hadjustment.ValueChanged -= OnNativeViewValueChanged;
			DisconnectButtonEvents(platformView);
			DisconnectButtonEvents(platformView.VScrollbar);
			DisconnectButtonEvents(platformView.HScrollbar);
		}

		bool _scrolling;
		Point _lastscroll = Point.Zero;
		Point _lastMotion = Point.Zero;
		double _lastDelta;

		bool _valueChanged;
		bool _intermediate = true;

		void EndScrolling()
		{
			_scrolling = false;
			_lastscroll = Point.Zero;
			_lastDelta = 0d;
		}

		void OnNativeViewMotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			_lastMotion = new Point(args.Event.X, args.Event.Y);

			if (_lastscroll != Point.Zero && _lastMotion != _lastscroll)
			{
				if (_scrolling)
				{
					_intermediate = false;
					OnScrollFinished();
				}

				EndScrolling();

			}
		}

		[GLib.ConnectBefore]
		void OnNativeViewScrollEvent(object o, ScrollEventArgs args)
		{
			var delta = args.Event.DeltaX == 0 ? args.Event.DeltaY : args.Event.DeltaX;

			if (delta != 0)
			{

				if (delta < 0 && _lastDelta > 0 || delta > 0 && _lastDelta < 0)
				{
					_intermediate = false;

				}

				_lastDelta = delta;
				_lastscroll = new Point(args.Event.X, args.Event.Y);
				_scrolling = true;

			}
			else
			{
				_intermediate = true;
				_valueChanged = false;
				EndScrolling();
			}

		}

		[GLib.ConnectBeforeAttribute]
		void OnNativeViewButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			_valueChanged = false;
			_intermediate = true;
		}

		void OnNativeViewButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			_intermediate = false;
			OnScrollFinished();

		}

		protected virtual void OnNativeViewValueChanged(object? sender, EventArgs e)
		{
			if (PlatformView is not { } nativeView || VirtualView is not { } virtualView || sender is not Adjustment adjustment)
				return;

			if (nativeView.Vadjustment == adjustment && adjustment.Value != virtualView.HorizontalOffset)
			{
				virtualView.HorizontalOffset = adjustment.Value;
				_valueChanged = true;

			}

			if (nativeView.Hadjustment == adjustment && adjustment.Value != virtualView.HorizontalOffset)
			{
				virtualView.VerticalOffset = adjustment.Value;
				_valueChanged = true;

			}

			OnScrollFinished();

		}

		[MissingMapper("detect more finishing of scroll")]
		protected virtual void OnScrollFinished()
		{
			if (_intermediate || !_valueChanged)
				return;

			VirtualView?.ScrollFinished();
			EndScrolling();

			_valueChanged = false;
			_intermediate = true;

		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (handler?.PlatformView is not { } nativeView)
				return;

			if (args is ScrollToRequest request)
			{
				if (nativeView.VScrollbar?.Visible ?? false)
					nativeView.Vadjustment.Value = request.VerticalOffset;

				if (nativeView.HScrollbar?.Visible ?? false)
					nativeView.Hadjustment.Value = request.HorizontalOffset;

			}
		}

		public static void MapOrientation(IScrollViewHandler handler, IScrollView view)
		{
			if (handler?.PlatformView is not { } platformView)
				return;

			switch (view.Orientation)
			{
				case ScrollOrientation.Both:
					// platformView.PropagateNaturalWidth = true;
					// platformView.PropagateNaturalHeight = true;
					platformView.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
					platformView.HScrollbar.Visible = true;
					platformView.VScrollbar.Visible = true;

					break;
				case ScrollOrientation.Horizontal:
					// platformView.PropagateNaturalWidth = true;
					// platformView.PropagateNaturalHeight = false;
					platformView.SetPolicy(PolicyType.Automatic, PolicyType.Never);
					platformView.HScrollbar.Visible = true;
					platformView.VScrollbar.Visible = false;

					break;
				case ScrollOrientation.Vertical:
					// platformView.PropagateNaturalHeight = true;
					// platformView.PropagateNaturalWidth = false;
					platformView.SetPolicy(PolicyType.Never, PolicyType.Automatic);
					platformView.HScrollbar.Visible = false;
					platformView.VScrollbar.Visible = true;

					break;

				case ScrollOrientation.Neither:
					// platformView.PropagateNaturalWidth = false;
					// platformView.PropagateNaturalHeight = false;
					platformView.SetPolicy(PolicyType.Never, PolicyType.Never);
					platformView.HScrollbar.Visible = false;
					platformView.VScrollbar.Visible = false;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			platformView.ScrollOrientation = view.Orientation;
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
		{
			if (handler?.PlatformView is not { } nativeView)
				return;

			nativeView.HscrollbarPolicy = view.HorizontalScrollBarVisibility.ToNative();

		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
		{
			if (handler?.PlatformView is not { } nativeView)
				return;

			nativeView.VscrollbarPolicy = view.VerticalScrollBarVisibility.ToNative();

		}

	}

}