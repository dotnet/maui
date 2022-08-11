using System;
using System.Runtime.Versioning;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public partial class MauiUIApplicationDelegate
	{
		internal IUIMenuBuilder? MenuBuilder { get; private set; }
		[SupportedOSPlatform("ios13.0")]
		public override void BuildMenu(IUIMenuBuilder builder)
		{
			MenuFlyoutItemHandler.Reset();

			base.BuildMenu(builder);

			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			MenuBuilder = builder;

			var window = Window ?? this.GetWindow() ??
				UIApplication.SharedApplication.GetWindow()?.Handler?.PlatformView as UIWindow;

			window?.GetWindow()?.Handler?.UpdateValue(nameof(IMenuBarElement.MenuBar));

			MenuBuilder = null;
		}

		public override bool CanPerform(Selector action, NSObject? withSender)
		{
			if (action.Name.StartsWith("MenuItem", StringComparison.Ordinal))
				return true;

			return base.CanPerform(action, withSender);
		}
	}
}

