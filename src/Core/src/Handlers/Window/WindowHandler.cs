using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIWindow;
#elif MONOANDROID
using NativeView = Android.App.Activity;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : IWindowHandler
	{
		public static PropertyMapper<IWindow, WindowHandler> WindowMapper = new(ElementHandler.ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
		};

		public WindowHandler()
			: base(WindowMapper)
		{
		}

		public WindowHandler(PropertyMapper? mapper = null)
			: base(mapper ?? WindowMapper)
		{
		}

#if !NETSTANDARD
		NativeView? _window;

		protected override NativeView CreateNativeElement() =>
			_window ?? throw new InvalidOperationException($"Native window must be set via {nameof(SetWindow)}.");

		public void SetWindow(NativeView window) =>
			_window = window;
#endif
	}
}