#if IOS || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif ANDROID
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeControl;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
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

		ISwipeView ISwipeViewHandler.VirtualView => VirtualView;

		PlatformView ISwipeViewHandler.PlatformView => PlatformView;
	}
}
