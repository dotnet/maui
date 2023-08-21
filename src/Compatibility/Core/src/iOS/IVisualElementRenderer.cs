//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using ObjCRuntime;
using UIKit;
using NativeView = UIKit.UIView;
using NativeViewController = UIKit.UIViewController;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using NativeView = AppKit.NSView;
using NativeViewController = AppKit.NSViewController;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IVisualElementRenderer : IDisposable, IRegisterable
	{
		VisualElement Element { get; }

		NativeView NativeView { get; }

		NativeViewController ViewController { get; }

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);

		void SetElement(VisualElement element);

		void SetElementSize(Size size);
	}
}