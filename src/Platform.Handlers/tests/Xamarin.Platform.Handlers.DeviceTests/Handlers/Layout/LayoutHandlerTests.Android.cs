using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform.Handlers.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return (layoutHandler.NativeView as LayoutViewGroup).ChildCount;
		}
	}
}
