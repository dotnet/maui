using Foundation;
using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.iOS
{
	internal interface IAccessibilityElementsController
	{
		void ResetAccessibilityElements();
		List<NSObject> GetAccessibilityElements();
	}
}