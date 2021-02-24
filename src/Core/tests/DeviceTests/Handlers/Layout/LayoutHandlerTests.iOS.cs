using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return (layoutHandler.NativeView as UIView).Subviews.Length;
		}
	}
}
