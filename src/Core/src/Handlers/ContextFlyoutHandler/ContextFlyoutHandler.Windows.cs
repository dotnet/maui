using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ContextFlyoutHandler : ElementHandler<IContextFlyout, MenuFlyout>, IContextFlyoutHandler
	{
		protected override MenuFlyout CreatePlatformElement()
		{
			return new MenuFlyout();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in (IContextFlyout)view)
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
