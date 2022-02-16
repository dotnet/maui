using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override MenuFlyoutSubItem CreateNativeElement()
		{
			return new MenuFlyoutSubItem();
		}

		public static void MapText(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.NativeView.Text = view.Text;
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.NativeView.UpdateIsEnabled(view.IsEnabled);

		public static void MapTextColor(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.NativeView?.UpdateTextColor(view);

		public static void MapCharacterSpacing(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.NativeView?.UpdateCharacterSpacing(view);

		public static void MapFont(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem menuBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(menuBar, fontManager);
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuFlyoutSubItem)view))
			{
				Add(item);
			}
		}

		public void Add(IMenuFlyoutItemBase view)
		{
			NativeView.Items.Add((MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuFlyoutItemBase view)
		{
			if (view.Handler != null)
				NativeView.Items.Remove((MenuFlyoutItemBase)view.ToPlatform());
		}

		public void Clear()
		{
			NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuFlyoutItemBase view)
		{
			NativeView.Items.Insert(index, (MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}
	}
}
