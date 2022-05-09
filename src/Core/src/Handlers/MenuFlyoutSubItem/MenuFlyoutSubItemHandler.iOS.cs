using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			var menuBar = VirtualView.FindParentOfType<IMenuBar>();

			IUIMenuBuilder? uIMenuBuilder = null;
			if (menuBar?.Handler?.PlatformView is IUIMenuBuilder builder)
			{
				uIMenuBuilder = builder;
			}

			var menu =
				VirtualView.ToPlatformMenu(
					VirtualView.Text,
					VirtualView.Source,
					MauiContext!,
					uIMenuBuilder);

			return menu;
		}

		public void Add(IMenuElement view)
		{
			Rebuild();
		}

		public void Remove(IMenuElement view)
		{
			Rebuild();
		}

		public void Clear()
		{
			Rebuild();
		}

		public void Insert(int index, IMenuElement view)
		{
			Rebuild();
		}

		void Rebuild()
		{
			MenuBarHandler.Rebuild();
		}
	}
}
