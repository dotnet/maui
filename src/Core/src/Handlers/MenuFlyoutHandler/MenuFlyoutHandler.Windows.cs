using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, MenuFlyout>, IMenuFlyoutHandler
	{
		protected override MenuFlyout CreatePlatformElement()
		{
			return new MenuFlyout();
		}

		protected override void DisconnectHandler(MenuFlyout platformView)
		{
			if (VirtualView is not null)
			{
				foreach (var item in VirtualView)
				{
					item.Handler?.DisconnectHandler();
				}
			}

			base.DisconnectHandler(platformView);
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in (IMenuFlyout)view)
			{
				Add(item);
			}
		}

		public void Add(IMenuElement view)
		{
			PlatformView.Items.Add((MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuElement view)
		{
			if (view.Handler != null)
			{
				PlatformView.Items.Remove((MenuFlyoutItemBase)view.ToPlatform());
			}
		}

		public void Clear()
		{
			PlatformView.Items.Clear();
		}

		public void Insert(int index, IMenuElement view)
		{
			PlatformView.Items.Insert(index, (MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}
	}
}
