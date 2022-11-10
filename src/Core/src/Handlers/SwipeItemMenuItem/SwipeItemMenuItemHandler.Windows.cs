using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, Button>
	{
		ImageSourcePartLoader? _imageSourcePartLoader;

		internal ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

		protected override Button CreatePlatformElement()
		{
			var platformView = new MauiButton
			{
				BorderThickness = new WThickness(0),
				CornerRadius = new UI.Xaml.CornerRadius(0)
			};

			return platformView;
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

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
			=> handler.PlatformView.UpdateBackground(view.Background);

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var swipeView = handler.PlatformView.Parent.GetParentOfType<MauiSwipeView>();	
			swipeView?.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.UpdateVisibility(view.Visibility);
		}

		// TODO: NET8 make this public
		internal static void MapIsEnabled(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
			=> handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			if (handler is SwipeItemMenuItemHandler swipeItemMenuItemHandler)
				swipeItemMenuItemHandler.ImageSourceLoader.UpdateImageSourceAsync().FireAndForget(handler);
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

		void OnSetImageSource(ImageSource? platformImageSource)
		{
			PlatformView.UpdateImageSource(platformImageSource);
		}

		void OnSwipeItemInvoked(object sender, UI.Xaml.RoutedEventArgs e)
		{
			if (VirtualView.IsEnabled)
			{
				VirtualView.OnInvoked();
			}
		}
	}
}