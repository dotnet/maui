using System;

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class PlatformVersion
	{
		public static bool IsAtLeast(int version)
		{
			return OperatingSystem.IsIOSVersionAtLeast(version) || OperatingSystem.IsTvOSVersionAtLeast(version);
		}

		private static bool? SetNeedsUpdateOfHomeIndicatorAutoHidden;

		public static bool Supports(string capability)
		{
			switch (capability)
			{
				case PlatformApis.RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden:
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

	internal static class PlatformApis
	{
		public const string RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden = "RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden";
		public const int UIActivityIndicatorViewStyleMedium = 13;
	}
}