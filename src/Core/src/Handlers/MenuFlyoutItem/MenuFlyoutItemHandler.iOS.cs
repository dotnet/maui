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

			var command = UIAction.Create(
				title: VirtualView.Text,
				uiImage,
				$"{index}",
				OnMenuClicked);
			
			menus[index] = VirtualView;
			return command;
		}

		static void OnMenuClicked(UIAction uIAction)
		{
			Execute(uIAction);
		}

		internal static void Execute(UICommand uICommand)
		{
			if (uICommand.PropertyList is NSString nsString &&
				Int32.TryParse(nsString.ToString(), out int index))
			{
				menus[index].Clicked();
			}
		}

		internal static void Execute(UIAction uICommand)
		{
			if (uICommand.Identifier != null &&
				Int32.TryParse(uICommand.Identifier, out int index))
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
