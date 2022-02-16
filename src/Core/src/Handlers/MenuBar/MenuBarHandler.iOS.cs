using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarHandler : ElementHandler<IMenuBar, IUIMenuBuilder>, IMenuBarHandler
	{
		protected override IUIMenuBuilder CreateNativeElement()
		{
			return MauiUIApplicationDelegate.Current.MenuBuilder
				?? throw new InvalidOperationException("Menu has not been initialized yet on the Application");
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
			var handler = view.ToHandler(MauiContext!);
			var menuItem = (UIMenu)handler!.NativeView!;

			NativeView.InsertSiblingMenuAfter(menuItem, UIKit.UIMenuIdentifier.File.ToString());
			//NativeView.Items.Add((MenuBarItem)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuBarItem view)
		{
			/*if (view.Handler != null)
				NativeView.Items.Remove((MenuBarItem)view.ToPlatform());*/
		}

		public void Clear()
		{
			//NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuBarItem view)
		{
			//NativeView.Items.Insert(index, (MenuBarItem)view.ToPlatform(MauiContext!));
		}
	}
}
