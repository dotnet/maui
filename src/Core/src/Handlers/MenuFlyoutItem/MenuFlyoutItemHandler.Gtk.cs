using System;
using PlatformView = Microsoft.Maui.Platform.MauiMenuBarItem;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler
	{
		protected override PlatformView CreatePlatformElement()
		{
			return new();
		}

		public void Add(IMenuElement view)
		{
		}

		public void Remove(IMenuElement view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuElement view)
		{
		}
		
		public static void MapIsEnabled(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
	}
}
