#nullable enable
using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIScrollView;
#elif MONOANDROID
using NativeView = Microsoft.Maui.Platform.MauiScrollView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Controls.ScrollViewer;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : IScrollViewHandler
	{
		public static IPropertyMapper<IScrollView, IScrollViewHandler> Mapper = new PropertyMapper<IScrollView, IScrollViewHandler>(ViewMapper)
		{
			[nameof(IScrollView.Content)] = MapContent,
			[nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
			[nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
			[nameof(IScrollView.Orientation)] = MapOrientation,
#if __IOS__
			[nameof(IScrollView.ContentSize)] = MapContentSize
#endif
		};

		public static CommandMapper<IScrollView, IScrollViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo
		};

		public ScrollViewHandler() : base(Mapper, CommandMapper)
		{

		}

		public ScrollViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		IScrollView IScrollViewHandler.VirtualView => VirtualView;

		NativeView IScrollViewHandler.NativeView => NativeView;
	}
}
