using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class PlatformVersion
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

	public static class PlatformApis
	{
		public const string RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden = "RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden";
		public const int UIActivityIndicatorViewStyleMedium = 13;
	}
}