using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class MacIdiomExtensions
	{
		/// <summary>
		/// Gets whether the app is a Mac Catalyst app that uses the Mac idiom (<c>UIDeviceFamily</c> 6, "Optimize Interface for Mac").
		/// </summary>
		internal static bool IsMacIdiom =>
			OperatingSystem.IsMacCatalyst() && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac;

		/// <summary>
		/// In the Mac idiom, UIKit throws <c>_UICatalystUnsupportedMacIdiomBehavior</c> when some views (such as
		/// <see cref="UIStepper"/> and <see cref="UIRefreshControl"/>) are added to a view or window. They check their own
		/// trait collection, so overriding the idiom to iPad for just that view lets it run with its iPad behavior.
		/// Trait overrides need Mac Catalyst 17 or later.
		/// </summary>
		internal static void UsePadIdiomInMacIdiom(this UIView view)
		{
			if (OperatingSystem.IsMacCatalystVersionAtLeast(17) && IsMacIdiom)
			{
				view.TraitOverrides.UserInterfaceIdiom = UIUserInterfaceIdiom.Pad;
				// Trait overrides apply lazily; a view that isn't in a window yet would still report the Mac idiom when it
				// is first added to a superview.
				view.UpdateTraitsIfNeeded();
			}
		}
	}
}
