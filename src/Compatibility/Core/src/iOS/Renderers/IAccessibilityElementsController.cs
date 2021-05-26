using System;
using System.Collections.Generic;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal interface IAccessibilityElementsController
	{
		List<NSObject> GetAccessibilityElements();
	}
}