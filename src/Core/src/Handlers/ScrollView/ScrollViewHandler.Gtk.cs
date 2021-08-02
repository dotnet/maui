using System;
using Gdk;
using Gtk;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Handlers
{

	// https://developer.gnome.org/gtk3/stable/GtkScrolledWindow.html

	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>
	{

		protected override ScrollView CreateNativeView()
		{
			var s = new ScrollView();

			return s;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.Content == null)
			{
				return;
			}

			if (handler?.NativeView is not { } nativeView)
				return;

			var nativeContent = scrollView.Content.ToNative(handler.MauiContext);
			var child = nativeView.Child;

			// check if nativeContent is set as child of Viewport:
			if (child is Gtk.Viewport vp && vp != nativeContent)
			{
				child = vp.Child;
			}

			if (child != nativeContent)
			{
				nativeView.Child = nativeContent;

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
			if (NativeView is not { } nativeView || VirtualView is not { } virtualView || sender is not Adjustment adjustment)
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

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			if (args is ScrollToRequest request)
			{
				if (nativeView.VScrollbar?.Visible ?? false)
					nativeView.Vadjustment.Value = request.VerticalOffset;

				if (nativeView.HScrollbar?.Visible ?? false)
					nativeView.Hadjustment.Value = request.HoriztonalOffset;

			}
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView view)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			switch (view.Orientation)
			{
				case ScrollOrientation.Both:
					// nativeView.PropagateNaturalWidth = true;
					// nativeView.PropagateNaturalHeight = true;
					nativeView.SetPolicy(PolicyType.Always, PolicyType.Always);
					nativeView.HScrollbar.Visible = true;
					nativeView.VScrollbar.Visible = true;

					break;
				case ScrollOrientation.Horizontal:
					// nativeView.PropagateNaturalWidth = true;
					// nativeView.PropagateNaturalHeight = false;
					nativeView.SetPolicy(PolicyType.Always, PolicyType.Never);
					nativeView.HScrollbar.Visible = true;
					nativeView.VScrollbar.Visible = false;

					break;
				case ScrollOrientation.Vertical:
					// nativeView.PropagateNaturalHeight = true;
					// nativeView.PropagateNaturalWidth = false;
					nativeView.SetPolicy(PolicyType.Never, PolicyType.Always);
					nativeView.HScrollbar.Visible = false;
					nativeView.VScrollbar.Visible = true;

					break;

				case ScrollOrientation.Neither:
					// nativeView.PropagateNaturalWidth = false;
					// nativeView.PropagateNaturalHeight = false;
					nativeView.SetPolicy(PolicyType.Never, PolicyType.Never);
					nativeView.HScrollbar.Visible = false;
					nativeView.VScrollbar.Visible = false;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			nativeView.ScrollOrientation = view.Orientation;
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView view)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			nativeView.HscrollbarPolicy = view.HorizontalScrollBarVisibility.ToNative();

		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView view)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			nativeView.VscrollbarPolicy = view.VerticalScrollBarVisibility.ToNative();

		}

	}

}