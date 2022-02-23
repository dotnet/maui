using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler
	{
		static int i = 0;
		static Dictionary<int, IMenuElement> menus = new Dictionary<int, IMenuElement>();

		protected override UIMenuElement CreatePlatformElement()
		{
			UIImage? uiImage = VirtualView.Source.GetPlatformMenuImage(MauiContext!);
			var selector = new Selector($"MenuItem{i++}:");
			var command = UICommand.Create(
				title: VirtualView.Text,
				uiImage,
				selector,
				new NSString($"{i}"));

			menus[i] = VirtualView;
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
			menus.Clear();
		}
	}
}
