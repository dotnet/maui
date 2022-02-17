using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler)
		{
			var platformView = ((UIView)viewHandler.PlatformView);
			return platformView.IsAccessibilityElement;
		}

		protected bool GetExcludedWithChildren(IViewHandler viewHandler)
		{
			var platformView = ((UIView)viewHandler.PlatformView);
			return platformView.AccessibilityElementsHidden;
		}
	}
}
