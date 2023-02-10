using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarHandler : ElementHandler<IMenuBar, MenuBar>, IMenuBarHandler
	{
		public static void MapIsEnabled(IMenuBarHandler handler, IMenuBar view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		protected override MenuBar CreatePlatformElement()
		{
			return new MenuBar();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuBar)view))
			{
				Add(item);
			}
		}

		public void Add(IMenuBarItem view)
		{
			PlatformView.Items.Add((MenuBarItem)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuBarItem view)
		{
			if (view.Handler != null)
				PlatformView.Items.Remove((MenuBarItem)view.ToPlatform());
		}

		public void Clear()
		{
			PlatformView.Items.Clear();
		}

		public void Insert(int index, IMenuBarItem view)
		{
			PlatformView.Items.Insert(index, (MenuBarItem)view.ToPlatform(MauiContext!));
		}
	}
}
