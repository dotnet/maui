using Foundation;
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal interface IAccessibilityElementsController
	{
		List<NSObject> GetAccessibilityElements();
	}
}