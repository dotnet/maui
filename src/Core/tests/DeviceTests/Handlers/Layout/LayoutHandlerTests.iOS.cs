using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return GetNativeChildCount((layoutHandler as IElementHandler).NativeView as UIView);
		}

		double GetNativeChildCount(object nativeView)
		{
			return (nativeView as UIView).Subviews.Length;
		}

		IReadOnlyList<UIView> GetNativeChildren(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView.Subviews;
		}
	}
}
