#nullable enable
using Microsoft.Maui.Handlers;

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class StubBaseHandler : ViewHandler<StubBase, StubPlatformView>
	{
		public static IPropertyMapper<StubBase, StubBaseHandler> StubMapper =
			new PropertyMapper<StubBase, StubBaseHandler>(ViewMapper)
			{
			};

		public static CommandMapper<StubBase, StubBaseHandler> StubCommandMapper =
			new(ViewCommandMapper)
			{
			};

		public StubBaseHandler()
			: this(null, null)
		{
		}

		public StubBaseHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(
				new PropertyMapper<StubBase, StubBaseHandler>(mapper ?? StubMapper),
				new CommandMapper<StubBase, StubBaseHandler>(commandMapper ?? ViewCommandMapper))
		{
		}

		public CommandMapper<StubBase, StubBaseHandler>? CommandMapper =>
			_commandMapper as CommandMapper<StubBase, StubBaseHandler>;

		public PropertyMapper<StubBase, StubBaseHandler>? PropertyMapper =>
			_mapper as PropertyMapper<StubBase, StubBaseHandler>;

		protected override StubPlatformView CreatePlatformView() =>
			new StubPlatformView(MauiContext!);
	}

	public class StubPlatformView : PlatformView
	{
		public StubPlatformView(IMauiContext mauiContext)
#if __ANDROID__
			: base(mauiContext.Context)
#endif
		{
			MauiContext = mauiContext;
		}

		public IMauiContext MauiContext { get; }
	}
}