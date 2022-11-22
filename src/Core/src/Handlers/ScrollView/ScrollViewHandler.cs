#nullable enable
using System;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIScrollView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiScrollView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ScrollViewer;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.ScrollView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
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
			[nameof(IScrollView.IsEnabled)] = MapIsEnabled,
#endif
		};

		public static CommandMapper<IScrollView, IScrollViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo
		};

		public ScrollViewHandler() : base(Mapper, CommandMapper)
		{

		}

		public ScrollViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IScrollView IScrollViewHandler.VirtualView => VirtualView;

		PlatformView IScrollViewHandler.PlatformView => PlatformView;
	}
}
