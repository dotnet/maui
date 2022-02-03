using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;
using PlatformViewController = UIKit.UIViewController;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using PlatformView = AppKit.NSView;
using PlatformViewController = AppKit.NSViewController;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IVisualElementRenderer : IDisposable, IRegisterable
	{
		VisualElement Element { get; }

		PlatformView PlatformView { get; }

		PlatformViewController ViewController { get; }

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);

		void SetElement(VisualElement element);

		void SetElementSize(Size size);
	}
}