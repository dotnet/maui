using System;
using UIKit;
using Foundation;
using ObjCRuntime;
using System.Collections.Generic;

namespace Microsoft.Maui.Platform
{
	internal static class MenuExtensions
	{
		public static string GetIdentifier(this UIMenu uIMenu)
		{
			return (NSString)uIMenu.PerformSelector(new Selector("identifier"));
		}

		public static UIMenu ToPlatformMenu(
			this IList<IMenuElement> menuElements,
			string title,
			IMauiContext mauiContext)
		{
			UIMenu? platformMenu = null;
			if (Enum.TryParse(typeof(UIMenuIdentifier), title, out object? result))
			{
				if (result != null)
				{
					platformMenu =
						MauiUIApplicationDelegate.Current.MenuBuilder?.GetMenu(((UIMenuIdentifier)result).GetConstant());
				}
			}

			UIMenuElement[] platformMenuElements = new UIMenuElement[menuElements.Count];

			for (int i = 0; i < menuElements.Count; i++)
			{
				var item = menuElements[i];
				var menuElement = (UIMenuElement)item.ToHandler(mauiContext)!.PlatformView!;
				platformMenuElements[i] = menuElement;
			}

			if (platformMenu != null)
			{
				if (platformMenuElements.Length > 0)
				{
					var menuContainer =
						UIMenu.Create(String.Empty, null,
							UIMenuIdentifier.None,
							UIMenuOptions.DisplayInline,
							platformMenuElements);

					MauiUIApplicationDelegate
									.Current
									.MenuBuilder?
									.InsertChildMenuAtStart(menuContainer, platformMenu.GetIdentifier());
				}
			}
			else
			{
				platformMenu =
					UIMenu.Create(title, null, UIMenuIdentifier.None,
						UIMenuOptions.SingleSelection, platformMenuElements);
			}

			return platformMenu;
		}
	}
}

