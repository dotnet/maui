using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, Button>
	{
		ImageSourcePartLoader? _imageSourcePartLoader;

		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this);

		protected override Button CreatePlatformElement()
		{
			var platformView = new MauiButton
			{
				BorderThickness = new WThickness(0),
				CornerRadius = new UI.Xaml.CornerRadius(0)
			};

			return platformView;
		}

		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			if (platformView is Button button)
				button.Loaded += OnSwipeItemMenuItemLoaded;
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);

			if (platformView is Button button)
				button.Loaded -= OnSwipeItemMenuItemLoaded;
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

		void IImageSourcePartSetter.SetImageSource(ImageSource? platformImageSource)
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

		void OnSwipeItemMenuItemLoaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			var button = sender as Button;

			if (button is null)
				return;

			if (button.Content is not StackPanel container)
				return;

			var image = button.GetContent<Image>();

			if (image is null)
				return;

			container.Orientation = Orientation.Vertical;

			var iconSize = GetIconSize();
			image.MaxHeight = image.MaxWidth = iconSize;
		}

		double GetIconSize()
		{
			if (VirtualView is not IImageSourcePart imageSourcePart || imageSourcePart.Source is null)
				return 0;

			var mauiSwipeView = PlatformView.Parent.GetParentOfType<MauiSwipeView>();

			if (mauiSwipeView is null || MauiContext is null)
				return 0;

			double contentHeight = mauiSwipeView.ActualHeight;
			double contentWidth = SwipeViewExtensions.SwipeItemWidth;

			return Math.Min(contentHeight, contentWidth) / 2;
		}
	}
}