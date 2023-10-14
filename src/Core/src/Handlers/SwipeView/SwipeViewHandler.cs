#if IOS || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif ANDROID
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeControl;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ISwipeViewHandler
	{
		public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper = 
			new PropertyMapper<ISwipeView, ISwipeViewHandler>(ViewHandler.ViewMapper)
				.ReplaceMapping(nameof(ISwipeView.SwipeTransitionMode), MapSwipeTransitionMode)
				.ReplaceMapping(nameof(ISwipeView.LeftItems), MapLeftItems)
				.ReplaceMapping(nameof(ISwipeView.TopItems), MapTopItems)
				.ReplaceMapping(nameof(ISwipeView.RightItems), MapRightItems)
				.ReplaceMapping(nameof(ISwipeView.BottomItems), MapBottomItems)
				.ReplaceMapping(nameof(IContentView.Content), MapContent)
#if ANDROID || IOS || TIZEN
				.ReplaceMapping(nameof(IView.IsEnabled), MapIsEnabled)
#endif
#if ANDROID
				.ModifyMapping(nameof(IView.Background), MapBackground)
#endif
		;


		public static CommandMapper<ISwipeView, ISwipeViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(ISwipeView.RequestOpen)] = MapRequestOpen,
			[nameof(ISwipeView.RequestClose)] = MapRequestClose,
		};

		public SwipeViewHandler() : base(Mapper, CommandMapper)
		{

		}

		public SwipeViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public SwipeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ISwipeView ISwipeViewHandler.VirtualView => VirtualView;

		PlatformView ISwipeViewHandler.PlatformView => PlatformView;
	}
}
