#if IOS || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif ANDROID
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeControl;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ISwipeViewHandler
	{
		public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper
			= ViewMapper.RemapFor<ISwipeView, ISwipeViewHandler>()
				.Map(nameof(ISwipeView.SwipeTransitionMode), MapSwipeTransitionMode)
				.Map(nameof(ISwipeView.LeftItems), MapLeftItems)
				.Map(nameof(ISwipeView.TopItems), MapTopItems)
				.Map(nameof(ISwipeView.RightItems), MapRightItems)
				.Map(nameof(ISwipeView.BottomItems), MapBottomItems)
				.Map(nameof(ISwipeView.Content), MapContent)
#if ANDROID || IOS || TIZEN
				.Prepend(nameof(IView.IsEnabled), MapIsEnabled)
#endif
#if ANDROID
				.Modify(nameof(IView.Background), MapBackground)
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
#pragma warning restore RS0016 // Add public types and members to the declared API