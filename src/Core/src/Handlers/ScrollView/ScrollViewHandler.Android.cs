using Android.Views;

namespace Microsoft.Maui.Handlers
{

	public partial class ScrollViewHandler : ViewHandler<IScrollView, MauiScrollView>
	{
		public new class Factory : ViewHandler.Factory
		{
			public virtual MauiScrollView CreateNativeView(ScrollViewHandler scrollViewHandler, IScrollView scrollView)
			{
				return new MauiScrollView(
				   new Android.Views.ContextThemeWrapper(scrollViewHandler.MauiContext!.Context, Resource.Style.scrollViewTheme), null!,
					   Resource.Attribute.scrollViewStyle);
			}
		}

		public static Factory ScrollViewFactory { get; set; } = new Factory();

		public static FactoryMapper<IScrollView, ScrollViewHandler> FactoryMapper = new()
		{
			[nameof(ScrollViewHandler.CreateNativeView)] = ScrollViewFactory.CreateNativeView
		};

		protected override MauiScrollView CreateNativeView()
		{
			if (FactoryMapper.Invoke(this, null, nameof(ScrollViewHandler.CreateNativeView)) is MauiScrollView msv)
				return msv;

			throw new System.Exception("NOOOOOOO");
		}

		protected override void ConnectHandler(MauiScrollView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.ScrollChange += ScrollChange;
		}

		protected override void DisconnectHandler(MauiScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.ScrollChange -= ScrollChange;
		}

		void ScrollChange(object? sender, AndroidX.Core.Widget.NestedScrollView.ScrollChangeEventArgs e)
		{
			var context = (sender as View)?.Context;

			if (context == null)
			{
				return;
			}

			VirtualView.VerticalOffset = Context.FromPixels(e.ScrollY);
			VirtualView.HorizontalOffset = Context.FromPixels(e.ScrollX);
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.NativeView == null || handler.MauiContext == null || scrollView.Content == null)
				return;

			handler.NativeView.SetContent(scrollView.Content.ToNative(handler.MauiContext));
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetVerticalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.SetOrientation(scrollView.Orientation);
		}

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is not ScrollToRequest request)
			{
				return;
			}

			var context = handler.NativeView.Context;

			if (context == null)
			{
				return;
			}

			var horizontalOffsetDevice = (int)context.ToPixels(request.HoriztonalOffset);
			var verticalOffsetDevice = (int)context.ToPixels(request.VerticalOffset);

			handler.NativeView.ScrollTo(horizontalOffsetDevice, verticalOffsetDevice,
				request.Instant, () => handler.VirtualView.ScrollFinished());
		}
	}
}
