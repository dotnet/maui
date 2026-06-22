using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, WSwipeItem>
	{
		protected override WSwipeItem CreatePlatformElement()
		{
			return new WSwipeItem();
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateTextColor(view);

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateBackground(view.Background);

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// WinUI SwipeItem does not support a Visibility property, so we need to
			// rebuild the parent SwipeView's swipe items to reflect the visibility change.
			var swipeView = GetParentSwipeView(view);
			if (swipeView?.Handler is ISwipeViewHandler swipeViewHandler)
			{
				if (swipeView.LeftItems?.Contains(view) == true)
				{
					swipeViewHandler.UpdateValue(nameof(ISwipeView.LeftItems));
				}
				else if (swipeView.RightItems?.Contains(view) == true)
				{
					swipeViewHandler.UpdateValue(nameof(ISwipeView.RightItems));
				}
				else if (swipeView.TopItems?.Contains(view) == true)
				{
					swipeViewHandler.UpdateValue(nameof(ISwipeView.TopItems));
				}
				else if (swipeView.BottomItems?.Contains(view) == true)
				{
					swipeViewHandler.UpdateValue(nameof(ISwipeView.BottomItems));
				}
			}
		}

		// Walk up the virtual view parent chain to find the owning ISwipeView.
		// This is more robust than assuming a fixed depth (Parent?.Parent) which
		// can silently fail if parenting is in transition.
		static ISwipeView? GetParentSwipeView(IElement? element)
		{
			var parent = element?.Parent;
			while (parent is not null)
			{
				if (parent is ISwipeView swipeView)
				{
					return swipeView;
				}
				parent = parent.Parent;
			}
			return null;
		}

		protected override void ConnectHandler(WSwipeItem platformView)
		{
			base.ConnectHandler(platformView);
			PlatformView.Invoked += OnSwipeItemInvoked;
		}

		protected override void DisconnectHandler(WSwipeItem platformView)
		{
			base.DisconnectHandler(platformView);
			PlatformView.Invoked -= OnSwipeItemInvoked;
		}

		void OnSwipeItemInvoked(WSwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
		{
			VirtualView.OnInvoked();
		}

		internal static async Task LoadFileIconAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem item)
		{
			if (handler.PlatformView is not WSwipeItem swipeItem || handler.MauiContext is null)
			{
				return;
			}

			if (item.Source is null)
			{
				swipeItem.IconSource = null;
				return;
			}

			var imageSourceServiceProvider = handler.MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
			var scale = handler.MauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
			var source = item.Source;
			try
			{
				var service = imageSourceServiceProvider.GetRequiredImageSourceService(source);
				// Do not use ConfigureAwait(false): WinUI DependencyProperty writes require the UI thread.
				var result = await service.GetImageSourceAsync(source, scale);
				if (item.Source == source)
				{
					swipeItem.IconSource = result?.Value is WImageSource platformImage ? new ImageIconSource { ImageSource = platformImage } : null;
				}
			}
			catch (System.Exception ex)
			{
				handler.MauiContext?.CreateLogger<SwipeItemMenuItemHandler>()?.Log(LogLevel.Warning, new EventId(), "Cannot load SwipeItem Icon", ex, static (state, _) => state);
			}
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not WSwipeItem button)
					return;

				button.IconSource = new ImageIconSource { ImageSource = platformImage };
			}
		}
	}
}
