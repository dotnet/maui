using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutItemHandler
	{
		static Dictionary<int, IMenuElement> menus = new Dictionary<int, IMenuElement>();

		bool IsInContextFlyout()
		{
			IElement? current = VirtualView;
			while (current != null)
			{
				if (current is Microsoft.Maui.IContextFlyout)
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		protected override UIMenuElement CreatePlatformElement()
		{

			// REVIEW: I don't like this code, but couldn't come up with anything better. It feels right to
			// re-use this existing MenuFlyout type, but in the case of context menus it needs to create a
			// different platform element. "Main menu" actions are plugged in through MacCat's global menu
			// system that uses selectors, but "context menu" actions can go through the much simpler, and
			// also unlimited system of context menus (though they _can_ also use the global system, but
			// that seems way more complicated and unnecessary).
			if (IsInContextFlyout())
			{
				// REVIEW: It seems more straightforward to use the non-Selector UIAction because
				// it's simpler to use and avoids the default limit of 50 selectors and dealing
				// with other "global" things like the global menu system (which this doesn't seem
				// to use).
				UIImage? contextUiImage = VirtualView.Source.GetPlatformMenuImage(MauiContext!);

				var uiAction = UIAction.Create(
					title: VirtualView.Text,
					image: contextUiImage,
					identifier: null,
					handler: (_) => VirtualView?.Clicked());

				return uiAction;
			}

			int index = menus.Count;
			UIImage? uiImage = VirtualView.Source.GetPlatformMenuImage(MauiContext!);
			var selector = new Selector($"MenuItem{index}:");

			bool selectorFound =
				MauiUIApplicationDelegate.Current.RespondsToSelector(selector);

			if (!selectorFound)
			{
				throw new InvalidOperationException(
					$"By default we only support 50 MenuItems. You can add more by adding the following code to {MauiUIApplicationDelegate.Current.GetType()}\n\n" +
					$"[Export(\"MenuItem{index}: \")]\n" +
					$"internal void MenuItem{index}(UICommand uICommand)\n" +
					"{\n" +
					"	uICommand.SendClicked();\n" +
					"}");
			}

			var command = UICommand.Create(
				title: VirtualView.Text,
				uiImage,
				selector,
				new NSString($"{index}"));

			menus[index] = VirtualView;
			return command;
		}

		internal static void Execute(UICommand uICommand)
		{
			if (uICommand.PropertyList is NSString nsString &&
				Int32.TryParse(nsString.ToString(), out int index))
			{
				menus[index].Clicked();
			}
		}

		internal static void Reset()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			menus.Clear();
		}
	}
}
