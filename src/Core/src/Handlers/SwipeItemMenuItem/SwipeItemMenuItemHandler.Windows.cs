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

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

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

		// Per-handler monotonic counter; captured before an async load and re-checked on completion so
		// a stale untinted load cannot overwrite a newer tinted icon (or vice versa) on THIS SwipeItem.
		int _iconLoadGeneration;

		internal static async Task LoadFileIconAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem item)
		{
			if (handler.PlatformView is not WSwipeItem swipeItem || handler.MauiContext is null)
			{
				return;
			}

			// Use the concrete handler for per-instance state; the Windows mapper is always paired
			// with the concrete SwipeItemMenuItemHandler, so this cast is safe.
			var concreteHandler = handler as SwipeItemMenuItemHandler;
			int generation = concreteHandler is not null
				? System.Threading.Interlocked.Increment(ref concreteHandler._iconLoadGeneration)
				: 0;

			if (item.Source is null)
			{
				swipeItem.IconSource = null;
				return;
			}

			// ImageIconSource renders the image as-is and ignores Foreground, so an explicit tint has to go
			// through BitmapIconSource/FontIconSource instead, which honor Foreground (BitmapIconSource
			// draws the bitmap as a monochrome mask by default).
			var tintColor = item.GetIconTintColor();

			if (tintColor is not null)
			{
				var tintedIconSource = item.Source.ToIconSource(handler.MauiContext);

				if (tintedIconSource is not null)
				{
					tintedIconSource.Foreground = tintColor.ToPlatform();
					swipeItem.IconSource = tintedIconSource;
					return;
				}
			}

			var imageSourceServiceProvider = handler.MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
			var scale = handler.MauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
			var source = item.Source;
			try
			{
				var service = imageSourceServiceProvider.GetRequiredImageSourceService(source);
				// Do not use ConfigureAwait(false): WinUI DependencyProperty writes require the UI thread.
				var result = await service.GetImageSourceAsync(source, scale);

				// Only apply the result if no newer load has started on THIS handler while this one was
				// in flight. Checking item.Source == source alone is not enough: the source can be
				// identical while IconColor changed, so a stale untinted result would overwrite a tinted one.
				if (concreteHandler is not null &&
					generation != System.Threading.Volatile.Read(ref concreteHandler._iconLoadGeneration))
					return;

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
