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
		internal static void UpdateIsEnabled(this UIMenuElement uiMenuElement, IMenuBarItem menuBarItem)
		{
			uiMenuElement.UpdateIsEnabled(menuBarItem.IsEnabled);
		}

		internal static void UpdateIsEnabled(this UIMenuElement uiMenuElement, IMenuFlyoutItem menuFlyoutItem)
		{
			uiMenuElement.UpdateIsEnabled(menuFlyoutItem.IsEnabled);
		}

		internal static void UpdateIsEnabled(this UIMenuElement uiMenuElement, bool isEnabled)
		{
			uiMenuElement.UpdateMenuElementAttributes(isEnabled);
		}

		internal static void UpdateMenuElementAttributes(this UIMenuElement uiMenuElement, bool isEnabled)
		{
			if (uiMenuElement is UIAction action)
				action.Attributes = isEnabled.ToUIMenuElementAttributes();

			if (uiMenuElement is UICommand command)
				command.Attributes = isEnabled.ToUIMenuElementAttributes();
		}

		internal static UIMenuElementAttributes ToUIMenuElementAttributes(this bool isEnabled)
		{
			return isEnabled ? 0 : UIMenuElementAttributes.Disabled;
		}

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

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		public static void SendClicked(this UICommand uICommand)
		{
			MenuFlyoutItemHandler.Execute(uICommand);
		}

		internal static string GetIdentifier(this UIMenu uIMenu)
		{
			return (NSString)uIMenu.PerformSelector(new Selector("identifier"));
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static UIMenu ToPlatformMenu(
			this IList<IMenuElement> menuElements,
			IMauiContext mauiContext)
		{
			UIMenuElement[] platformMenuElements = new UIMenuElement[menuElements.Count];

			for (int i = 0; i < menuElements.Count; i++)
			{
				var item = menuElements[i];
				var menuElement = (UIMenuElement)item.ToHandler(mauiContext)!.PlatformView!;
				platformMenuElements[i] = menuElement;
			}

#pragma warning disable CA1416
			var platformMenu = UIMenu.Create(children: platformMenuElements);
#pragma warning restore CA1416

			return platformMenu;
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
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
						uIMenuBuilder.GetMenu(((UIMenuIdentifier)result).GetConstant() ?? string.Empty);
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
#pragma warning disable CA1416 // TOOO: UIMenuOptions.SingleSelection is only supported on: 'ios' 15.0 and later, 'tvos' 15.0 and later.
				// This means we are creating our own new menu/submenu
				platformMenu =
					UIMenu.Create(title, uiImage, UIMenuIdentifier.None,
						UIMenuOptions.SingleSelection, platformMenuElements);
#pragma warning restore CA1416
			}

			return platformMenu;
		}
	}
}

