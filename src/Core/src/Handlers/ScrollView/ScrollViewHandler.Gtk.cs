using System;
using Gdk;
using Gtk;
using Microsoft.Maui.Platform;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.ScrolledWindow.html

	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>
	{

		protected override ScrollView CreatePlatformView()
		{
			var s = new ScrollView();

			return s;
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

		protected override void ConnectHandler(ScrollView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Vadjustment.ValueChanged += OnNativeViewValueChanged;
			nativeView.Hadjustment.ValueChanged += OnNativeViewValueChanged;
			ConnectButtonEvents(nativeView);
			ConnectButtonEvents(nativeView.VScrollbar);
			ConnectButtonEvents(nativeView.HScrollbar);

		}

		protected virtual void ConnectButtonEvents(Widget? widget)
		{
			if (widget == null) return;

			widget.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.ScrollMask | EventMask.SmoothScrollMask;
			widget.ButtonPressEvent += OnNativeViewButtonPressEvent;
			widget.ButtonReleaseEvent += OnNativeViewButtonReleaseEvent;
			widget.ScrollEvent += OnNativeViewScrollEvent;
			widget.MotionNotifyEvent += OnNativeViewotionNotifyEvent;
		}

		protected virtual void DisconnectButtonEvents(Widget? widget)
		{
			if (widget == null) return;

			widget.ButtonPressEvent -= OnNativeViewButtonPressEvent;
			widget.ButtonReleaseEvent -= OnNativeViewButtonReleaseEvent;
			widget.ScrollEvent -= OnNativeViewScrollEvent;

		}

		protected override void DisconnectHandler(ScrollView nativeView)
		{
			base.OnDisconnectHandler(nativeView);

			nativeView.Vadjustment.ValueChanged -= OnNativeViewValueChanged;
			nativeView.Hadjustment.ValueChanged -= OnNativeViewValueChanged;
			DisconnectButtonEvents(nativeView);
			DisconnectButtonEvents(nativeView.VScrollbar);
			DisconnectButtonEvents(nativeView.HScrollbar);
		}

		bool _scrolling = false;
		Point _lastscroll = Point.Zero;
		Point _lastMotion = Point.Zero;
		double _lastDelta = 0d;

		bool _valueChanged = false;
		bool _intermediate = true;

		void EndScrolling()
		{
			_scrolling = false;
			_lastscroll = Point.Zero;
			_lastDelta = 0d;
		}

		void OnNativeViewotionNotifyEvent(object o, MotionNotifyEventArgs args)
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
			if (handler?.PlatformView is not { } nativeView)
				return;

			switch (view.Orientation)
			{
				case ScrollOrientation.Both:
					nativeView.PropagateNaturalWidth = true;
					nativeView.PropagateNaturalHeight = true;
					nativeView.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
					nativeView.HScrollbar.Visible = true;
					nativeView.VScrollbar.Visible = true;

					break;
				case ScrollOrientation.Horizontal:
					nativeView.PropagateNaturalWidth = true;
					nativeView.PropagateNaturalHeight = false;
					nativeView.SetPolicy(PolicyType.Automatic, PolicyType.Never);
					nativeView.HScrollbar.Visible = true;
					nativeView.VScrollbar.Visible = false;

					break;
				case ScrollOrientation.Vertical:
					nativeView.PropagateNaturalHeight = true;
					nativeView.PropagateNaturalWidth = false;
					nativeView.SetPolicy(PolicyType.Never, PolicyType.Automatic);
					nativeView.HScrollbar.Visible = false;
					nativeView.VScrollbar.Visible = true;

					break;

				case ScrollOrientation.Neither:
					nativeView.PropagateNaturalWidth = false;
					nativeView.PropagateNaturalHeight = false;
					nativeView.SetPolicy(PolicyType.Never, PolicyType.Never);
					nativeView.HScrollbar.Visible = false;
					nativeView.VScrollbar.Visible = false;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			nativeView.ScrollOrientation = view.Orientation;
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