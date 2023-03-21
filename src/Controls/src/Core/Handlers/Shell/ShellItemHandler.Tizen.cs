using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellItemHandler : ElementHandler<ShellItem, ShellItemView>, IAppearanceObserver, IDisposable
	{
		bool _disposedValue;

		public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
				new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
				{
					[nameof(ShellItem.CurrentItem)] = MapCurrentItem,
					[Shell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible
				};

		public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
				new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);

		public ShellItemHandler() : base(Mapper, CommandMapper)
		{
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			(view.FindParentOfType<Shell>() as IShellController)?.AddAppearanceObserver(this, VirtualView);
		}

		protected override ShellItemView CreatePlatformElement() => new ShellItemView(VirtualView, MauiContext!);

		protected override void ConnectHandler(ShellItemView platformView)
		{
			if (VirtualView.Parent is IShellController controller)
			{
				controller.AddAppearanceObserver(this, VirtualView);
			}
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ShellItemView platformView)
		{
			if (VirtualView.Parent is IShellController controller)
			{
				controller.RemoveAppearanceObserver(this);
			}
			base.DisconnectHandler(platformView);
		}

		public static void MapTabBarIsVisible(ShellItemHandler handler, ShellItem item)
		{
			handler.PlatformView.UpdateTabBar(Shell.GetTabBarIsVisible(item));
		}

		public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
		{
			handler.UpdateCurrentItem();
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					var platformView = PlatformView;
					foreach (var item in VirtualView.Items)
					{
						if (item.Handler is IDisposable thandler)
						{
							thandler.Dispose();
						}
					}

					(VirtualView.FindParentOfType<Shell>() as IShellController)?.RemoveAppearanceObserver(this);

					(this as IElementHandler)?.DisconnectHandler();
					platformView?.Dispose();
				}

				_disposedValue = true;
			}
		}

		void UpdateCurrentItem()
		{
			PlatformView.UpdateCurrentItem(VirtualView.CurrentItem);
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance != null)
			{
				var shellView = VirtualView?.FindParentOfType<Shell>()?.Handler?.PlatformView as ShellView;
				shellView?.UpdateToolbarColors(appearance.ForegroundColor, appearance.BackgroundColor, appearance.TitleColor);
			}

			if (appearance is IShellAppearanceElement shellAppearance)
			{
				var tabBarBackgroudColor = shellAppearance.EffectiveTabBarBackgroundColor;
				var tabBarTitleColor = shellAppearance.EffectiveTabBarTitleColor;
				var tabBarUnselectedColor = shellAppearance.EffectiveTabBarUnselectedColor;

				PlatformView?.UpdateBottomTabBarColors(tabBarBackgroudColor, tabBarTitleColor, tabBarUnselectedColor);
			}
		}
	}
}
