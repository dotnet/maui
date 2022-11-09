using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, Button>
	{
		protected override Button CreatePlatformElement()
		{
			return new Button
			{
				BorderThickness = new UI.Xaml.Thickness(0),
				CornerRadius = new UI.Xaml.CornerRadius()
			};
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateTextColor(view);

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view)
			=> handler.PlatformView?.UpdateCharacterSpacing(view);

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(view, fontManager);
		}

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
			=> handler.PlatformView.UpdateText(view.Text);

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateBackground(view.Background);

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// TODO: Map the IsVisible property
		}
	
		public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// TODO: Map the ImageSource property
		}

		protected override void ConnectHandler(Button platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Click += OnSwipeItemInvoked;
		}

		protected override void DisconnectHandler(Button platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Click -= OnSwipeItemInvoked;
		}

		void OnSwipeItemInvoked(object sender, UI.Xaml.RoutedEventArgs e)
		{
			VirtualView.OnInvoked();
		}
	}
}