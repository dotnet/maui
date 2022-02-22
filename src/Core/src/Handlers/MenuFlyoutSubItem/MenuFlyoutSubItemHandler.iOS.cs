using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			return VirtualView.ToPlatformMenu(VirtualView.Text, MauiContext!);
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
			UIMenuSystem
				.MainSystem
				.SetNeedsRebuild();
		}
	}
}
