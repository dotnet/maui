using Microsoft.Maui.TestUtils.DeviceTests.Runners;

#if IOS || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif ANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowHandlerProxyStub : ElementHandler<IWindow, PlatformView>, IWindowHandler
	{
		public WindowHandlerProxyStub() : this(new PropertyMapper<IWindow, IWindowHandler>(), new CommandMapper<IWindow, IWindowHandler>())
		{

		}

		public WindowHandlerProxyStub(IPropertyMapper<IWindow, IWindowHandler> mapper = null, CommandMapper<IWindow, IWindowHandler> commandMapper = null)
			: base(mapper ?? new PropertyMapper<IWindow, IWindowHandler>(), commandMapper ?? new CommandMapper<IWindow, IWindowHandler>())
		{
		}

		protected override PlatformView CreatePlatformElement() => TestWindow.PlatformWindow;
	}
}