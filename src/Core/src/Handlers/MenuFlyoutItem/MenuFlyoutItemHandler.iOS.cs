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

		protected override UIMenuElement CreatePlatformElement()
		{
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
