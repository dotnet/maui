#if IOS || MACCATALYST
using NativeView = Microsoft.Maui.Platform.MauiSwipeView;
#elif ANDROID
using NativeView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Controls.SwipeControl;
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
			[nameof(ISwipeView.LeftItems)] = MapLeftItems,
			[nameof(ISwipeView.TopItems)] = MapTopItems,
			[nameof(ISwipeView.RightItems)] = MapRightItems,
			[nameof(ISwipeView.BottomItems)] = MapBottomItems,
#if ANDROID || IOS
			[nameof(IView.IsEnabled)] = MapIsEnabled,
#endif
#if ANDROID
			[nameof(IView.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<ISwipeView, ISwipeViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(ISwipeView.RequestOpen)] = MapRequestOpen,
			[nameof(ISwipeView.RequestClose)] = MapRequestClose,
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