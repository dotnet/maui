using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

#if __MOBILE__
using NativeColor = UIKit.UIColor;
using NativeControl = UIKit.UIControl;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using NativeView = AppKit.NSView;
using NativeColor = CoreGraphics.CGColor;
using NativeControl = AppKit.NSControl;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler ControlChanging;
		event EventHandler ControlChanged;

		NativeView Control { get; }
	}
}