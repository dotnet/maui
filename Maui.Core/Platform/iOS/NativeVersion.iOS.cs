using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace System.Maui.Platform
{
	public static class NativeVersion
	{
		public static bool IsAtLeast(int version) 
		{
			return UIDevice.CurrentDevice.CheckSystemVersion(version, 0);
		}

		private static bool? SetNeedsUpdateOfHomeIndicatorAutoHidden;

		public static bool Supports(string capability)
		{
			switch (capability)
			{
				case NativeApis.RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden:
					if (!SetNeedsUpdateOfHomeIndicatorAutoHidden.HasValue)
					{
						SetNeedsUpdateOfHomeIndicatorAutoHidden = new UIViewController().RespondsToSelector(new ObjCRuntime.Selector("setNeedsUpdateOfHomeIndicatorAutoHidden"));
					}
					return SetNeedsUpdateOfHomeIndicatorAutoHidden.Value;
			}

			return false;
		}

		public static bool Supports(int capability) 
		{
			return IsAtLeast(capability);
		}
	}

	public static class NativeApis
	{
		public const string RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden = "RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden";
		public const int UIActivityIndicatorViewStyleMedium = 13;
	}
}
