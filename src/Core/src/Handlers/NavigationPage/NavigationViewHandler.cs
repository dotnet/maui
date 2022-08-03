#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Frame;
#elif TIZEN
using PlatformView = ElmSharp.Naviframe;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif


namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler : INavigationViewHandler
	{
		public static IPropertyMapper<IStackNavigationView, INavigationViewHandler> Mapper = new PropertyMapper<IStackNavigationView, INavigationViewHandler>(ViewMapper)
		{
		};

		public static CommandMapper<IStackNavigationView, INavigationViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
		};

		public NavigationViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public NavigationViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper, CommandMapper)
		{
		}

		IStackNavigationView INavigationViewHandler.VirtualView => VirtualView;

		PlatformView INavigationViewHandler.PlatformView => PlatformView;
	}
}
