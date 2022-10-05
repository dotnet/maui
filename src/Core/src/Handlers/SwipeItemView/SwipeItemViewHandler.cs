using System;
#if IOS || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemViewHandler : ViewHandler<ISwipeItemView, PlatformView>, ISwipeItemViewHandler
	{
		public static IPropertyMapper<ISwipeItemView, ISwipeItemViewHandler> Mapper = new PropertyMapper<ISwipeItemView, ISwipeItemViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemView.Content)] = MapContent,
			[nameof(ISwipeItemView.Visibility)] = MapVisibility
		};

		public static CommandMapper<ISwipeItemView, ISwipeItemViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public SwipeItemViewHandler() : base(Mapper, CommandMapper)
		{
		}

		protected SwipeItemViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		protected SwipeItemViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ISwipeItemView ISwipeItemViewHandler.VirtualView => VirtualView;

		PlatformView ISwipeItemViewHandler.PlatformView => PlatformView;
	}
}
