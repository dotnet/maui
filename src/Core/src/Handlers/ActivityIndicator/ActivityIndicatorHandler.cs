#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressRing;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.GraphicsView.ActivityIndicator;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : IActivityIndicatorHandler
	{
		public static IPropertyMapper<IActivityIndicator, IActivityIndicatorHandler> Mapper = new PropertyMapper<IActivityIndicator, IActivityIndicatorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IActivityIndicator.Color)] = MapColor,
			[nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
#if __ANDROID__
			// Android does not have the concept of IsRunning, so we are leveraging the Visibility
			[nameof(IActivityIndicator.Visibility)] = MapIsRunning,
#endif
#if WINDOWS
			[nameof(IActivityIndicator.Width)] = MapWidth,
			[nameof(IActivityIndicator.Height)] = MapHeight,
			[nameof(IActivityIndicator.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<IActivityIndicator, IActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

		public ActivityIndicatorHandler() : base(Mapper, CommandMapper)
		{

		}

		public ActivityIndicatorHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IActivityIndicator IActivityIndicatorHandler.VirtualView => VirtualView;

		PlatformView IActivityIndicatorHandler.PlatformView => PlatformView;
	}
}