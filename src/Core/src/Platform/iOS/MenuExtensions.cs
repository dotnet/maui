using System;
using UIKit;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	internal static class MenuExtensions
	{
		public static string GetIdentifier(this UIMenu uIMenu)
		{
			return (NSString)uIMenu.PerformSelector(new Selector("identifier"));
		}
	}
}

