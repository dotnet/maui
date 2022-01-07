#if IOS || MACCATALYST
using NativeView = UIKit.UIView;
#elif ANDROID
using NativeView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ISwipeViewHandler
	{
		public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper = new PropertyMapper<ISwipeView, ISwipeViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
			[nameof(ISwipeView.SwipeTransitionMode)] = MapSwipeTransitionMode,
#if ANDROID
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(IView.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<ISwipeView, ISwipeViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};


		public SwipeViewHandler() : base(Mapper, CommandMapper)
		{

		}

		protected SwipeViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		public SwipeViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		ISwipeView ISwipeViewHandler.TypedVirtualView => VirtualView;

		NativeView ISwipeViewHandler.TypedNativeView => NativeView;
	}
}