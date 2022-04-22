using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class MenuExtensions
	{
		internal static UIImage? GetPlatformMenuImage(this IImageSource? imageSource, IMauiContext mauiContext)
		{
			if (imageSource == null)
				return null;

			if (imageSource is IFileImageSource fileImageSource)
				return fileImageSource.GetPlatformImage();

			if (imageSource is IFontImageSource fontImageSource)
			{
				var fontManager = mauiContext.Services.GetRequiredService<IFontManager>();
				return fontImageSource.GetPlatformImage(fontManager, 1);
			}

			throw new InvalidOperationException("MenuItems on Catalyst currently only support Font and File Images");
		}

		public static void SendClicked(this UICommand uICommand)
		{
			MenuFlyoutItemHandler.Execute(uICommand);
		}

		internal static string GetIdentifier(this UIMenu uIMenu)
		{
			return (NSString)uIMenu.PerformSelector(new Selector("identifier"));
		}

		internal static UIMenu ToPlatformMenu(
			this IList<IMenuElement> menuElements,
			string title,
			IImageSource? imageSource,
			IMauiContext mauiContext,
			IUIMenuBuilder? uIMenuBuilder)
		{
			if (String.IsNullOrWhiteSpace(title))
				throw new ArgumentNullException(nameof(title), $"{menuElements} requires title text.");

			uIMenuBuilder = uIMenuBuilder ??
				MauiUIApplicationDelegate.Current.MenuBuilder!;

			UIMenu? platformMenu = null;
			UIImage? uiImage = imageSource.GetPlatformMenuImage(mauiContext);

			// You can't have two menus at the same level with the same title
			// This locates an existing menu and then will just merge the users
			// menu into it
			if (Enum.TryParse(typeof(UIMenuIdentifier), title, out object? result))
			{
				if (result != null)
				{
					platformMenu =
						uIMenuBuilder.GetMenu(((UIMenuIdentifier)result).GetConstant());
				}
			}

			UIMenuElement[] platformMenuElements = new UIMenuElement[menuElements.Count];

			for (int i = 0; i < menuElements.Count; i++)
			{
				var item = menuElements[i];
				var menuElement = (UIMenuElement)item.ToHandler(mauiContext)!.PlatformView!;
				platformMenuElements[i] = menuElement;
			}

			// This means we are merging into an existing menu
			if (platformMenu != null)
			{
				if (platformMenuElements.Length > 0)
				{
					var menuContainer =
						UIMenu.Create(String.Empty,
							uiImage,
							UIMenuIdentifier.None,
							UIMenuOptions.DisplayInline,
							platformMenuElements);

					uIMenuBuilder.InsertChildMenuAtStart(menuContainer, platformMenu.GetIdentifier());
				}
			}
			else
			{
				// This means we are creating our own new menu/submenu
				platformMenu =
					UIMenu.Create(title, uiImage, UIMenuIdentifier.None,
						UIMenuOptions.SingleSelection, platformMenuElements);
			}

			return platformMenu;
		}
	}
}

