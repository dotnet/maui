using System;
using System.Collections.Generic;
using System.Text;

#if IOS || MACCATALYST || WINDOWS || ANDROID
namespace Microsoft.Maui.Controls.Platform.Compatibility
{

	public static class Platform
	{
		public static INativeViewHandler GetRenderer(Element element)
		{
			return (INativeViewHandler)element.Handler;
		}

		public static INativeViewHandler CreateRenderer(Element element)
		{
			_ = element.ToNative();
			return GetRenderer(element);
		}

#if IOS
		internal static UIKit.UIEdgeInsets SafeAreaInsetsForWindow
		{
			get
			{
				UIKit.UIEdgeInsets safeAreaInsets;

				if (!NativeVersion.IsAtLeast(11))
					safeAreaInsets = new UIKit.UIEdgeInsets(UIKit.UIApplication.SharedApplication.StatusBarFrame.Size.Height, 0, 0, 0);
				else if (UIKit.UIApplication.SharedApplication.GetKeyWindow() != null)
					safeAreaInsets = UIKit.UIApplication.SharedApplication.GetKeyWindow().SafeAreaInsets;
				else if (UIKit.UIApplication.SharedApplication.Windows.Length > 0)
					safeAreaInsets = UIKit.UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
				else
					safeAreaInsets = UIKit.UIEdgeInsets.Zero;

				return safeAreaInsets;
			}
		}	
#endif
	}
}
#endif
