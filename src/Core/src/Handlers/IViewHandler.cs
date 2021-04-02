using System;
using Microsoft.Maui;
#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
	public interface IViewHandler
	{
		void SetMauiContext(IMauiContext mauiContext);
		void SetVirtualView(IView view);
		void UpdateValue(string property);
		void DisconnectHandler();
		NativeView? View { get; }
		IView? VirtualView { get; }
		bool HasContainer { get; set; }
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
		void SetFrame(Rectangle frame);
	}
}