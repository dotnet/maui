using System;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, MauiMenuItem>, IMenuBarItemHandler
	{
		protected override MauiMenuItem CreatePlatformElement()
		{
			return new();
		}

		protected override void ConnectHandler(MauiMenuItem platformView)
		{
			base.ConnectHandler(platformView);
		}

		public static void MapText(IMenuBarItemHandler handler, IMenuBarItem view)
		{
			handler.PlatformView.Label = view.Text;
		}

		public void Add(IMenuElement view)
		{
			;
		}

		public void Remove(IMenuElement view)
		{
			;
		}

		public void Clear()
		{
			;
		}

		public void Insert(int index, IMenuElement view)
		{
			;
		}

		public static void MapIsEnabled(IMenuBarItemHandler handler, IMenuBarItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
	}
}