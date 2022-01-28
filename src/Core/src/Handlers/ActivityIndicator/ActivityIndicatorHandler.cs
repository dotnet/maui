#if __IOS__ || MACCATALYST
using NativeView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif MONOANDROID
using NativeView = Android.Widget.ProgressBar;
#elif WINDOWS
using NativeView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
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
		};

		public static CommandMapper<IActivityIndicator, IActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

		public ActivityIndicatorHandler() : base(Mapper, CommandMapper)
		{

		}

		public ActivityIndicatorHandler(IPropertyMapper mapper) : base(mapper ?? Mapper, CommandMapper)
		{

		}

		IActivityIndicator IActivityIndicatorHandler.VirtualView => VirtualView;

		NativeView IActivityIndicatorHandler.NativeView => NativeView;
	}
}