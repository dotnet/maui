using Foundation;
using System;
using System.Collections.Generic;

namespace System.Maui.Platform.iOS
{
	internal interface IAccessibilityElementsController
	{
		List<NSObject> GetAccessibilityElements();
	}
}