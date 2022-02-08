using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

#if __MOBILE__
using NativeColor = UIKit.UIColor;
using PlatformControl = UIKit.UIControl;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using PlatformView = AppKit.NSView;
using NativeColor = CoreGraphics.CGColor;
using PlatformControl = AppKit.NSControl;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IVisualPlatformElementRenderer : IVisualElementRenderer
	{
		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler ControlChanging;
		event EventHandler ControlChanged;

		PlatformView Control { get; }
	}
}