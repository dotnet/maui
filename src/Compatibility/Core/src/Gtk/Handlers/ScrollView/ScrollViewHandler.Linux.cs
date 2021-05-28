using System;
using Gtk;
using Microsoft.Maui.Controls;
using NativeView = Gtk.Widget;

namespace Microsoft.Maui.Handlers.ScrollView
{

	// https://developer.gnome.org/gtk3/stable/GtkScrolledWindow.html
	public class ScrollViewHandler : ViewHandler<Controls.ScrollView, GtkScrollView>
	{

		public static PropertyMapper<Controls.ScrollView, ScrollViewHandler> ScrollViewMapper = new(ViewHandler.ViewMapper)
		{
			[nameof(Controls.ScrollView.ContentSize)] = MapContentSize,
			[nameof(Controls.ScrollView.ScrollX)] = MapScrollX,
			[nameof(Controls.ScrollView.ScrollY)] = MapScrollY,
			[nameof(Controls.ScrollView.Orientation)] = MapOrientation,
			[nameof(Controls.ScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
			[nameof(Controls.ScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,

		};

		public static void MapContentSize(ScrollViewHandler handler, Controls.ScrollView view)
		{
			;
		}

		public static void MapScrollX(ScrollViewHandler handler, Controls.ScrollView view)
		{
			if (handler?.NativeView is not { } nativeView || !(nativeView.VScrollbar?.Visible ?? true))
				return;

			nativeView.Vadjustment.Value = view.ScrollX;
		}

		public static void MapScrollY(ScrollViewHandler handler, Controls.ScrollView view)
		{
			if (handler?.NativeView is not { } nativeView || !(nativeView.HScrollbar?.Visible ?? true))
				return;

			nativeView.Hadjustment.Value = view.ScrollY;

		}

		public static void MapOrientation(ScrollViewHandler handler, Controls.ScrollView view)
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

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, Controls.ScrollView view)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			nativeView.HscrollbarPolicy = view.HorizontalScrollBarVisibility.ToNative();

		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, Controls.ScrollView view)
		{
			if (handler?.NativeView is not { } nativeView)
				return;

			nativeView.VscrollbarPolicy = view.VerticalScrollBarVisibility.ToNative();

		}

		public ScrollViewHandler() : base(ScrollViewMapper)
		{ }

		public ScrollViewHandler(PropertyMapper mapper = null) : base(mapper) { }

		protected override GtkScrollView CreateNativeView()
		{
			var s = new GtkScrollView();

			s.SizeAllocated += (s, o) =>
			{
				;
			};

			s.ResizeChecked += (s, o) =>
			{
				;
			};

			return s;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (VirtualView.Content != null)
				NativeView.Child = VirtualView.Content.ToNative(MauiContext);
		}

		protected override void ConnectHandler(GtkScrollView nativeView)
		{ }

		protected override void DisconnectHandler(GtkScrollView nativeView)
		{ }

	}

}