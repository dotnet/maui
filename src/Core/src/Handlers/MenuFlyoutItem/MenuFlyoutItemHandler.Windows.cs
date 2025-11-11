using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler
	{
		protected override MenuFlyoutItem CreatePlatformElement()
		{
			return new MenuFlyoutItem();
		}

		protected override void ConnectHandler(MenuFlyoutItem PlatformView)
		{
			base.ConnectHandler(PlatformView);
			PlatformView.Click += OnClicked;
		}

		protected override void DisconnectHandler(MenuFlyoutItem PlatformView)
		{
			base.DisconnectHandler(PlatformView);
			PlatformView.Click -= OnClicked;
		}

		void OnClicked(object sender, UI.Xaml.RoutedEventArgs e)
		{
			VirtualView.Clicked();
		}

		public static void MapSource(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			var iconSource = view.Source?.ToIconSource(handler.MauiContext!);

			// Ensure MenuFlyoutItem icons preserve original colors instead of showing as monochrome silhouettes
			if (iconSource is BitmapIconSource bitmapIconSource)
			{
				bitmapIconSource.ShowAsMonochrome = false;
			}

			handler.PlatformView.Icon = iconSource?.CreateIconElement();
		}

		public static void MapText(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		/// <summary>
		///	Maps keyboard accelerator to the Windows platform control.
		/// </summary>
		/// <param name="handler">The handler, of type <see cref="IMenuFlyoutItemHandler"/>.</param>
		/// <param name="view">The view, of type <see cref="IMenuFlyoutItem"/>.</param>
		public static void MapKeyboardAccelerators(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView.UpdateKeyboardAccelerators(view);
		}

		public static void MapIsEnabled(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
	}
}
