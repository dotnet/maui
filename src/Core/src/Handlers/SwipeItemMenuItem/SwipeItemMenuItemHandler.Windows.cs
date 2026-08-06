using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
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

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.UpdateTextColor(view);
			UpdateTextColorIconDependency(handler, view);
		}

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.UpdateBackground(view.Background);
			UpdateBackgroundColorDependencies(handler);
		}

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

		internal static async Task LoadFileIconAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem item)
		{
			if (handler.PlatformView is not WSwipeItem swipeItem || handler.MauiContext is null)
			{
				return;
			}

			int generation = BeginIconLoad(handler);

			var source = item.Source;
			if (source is null)
			{
				swipeItem.IconSource = null;
				return;
			}

			// ImageIconSource renders the image as-is and ignores Foreground, so an explicit tint has to go
			// through BitmapIconSource/FontIconSource instead, which honor Foreground (BitmapIconSource
			// draws the bitmap as a monochrome mask by default).
			var tintColor = (item as ISwipeItemMenuItemIconColor)?.IconColor;

			try
			{
				if (tintColor is not null)
				{
					var tintedIconSource = CreateTintedIconSource(source, handler.MauiContext);

					if (tintedIconSource is not null)
					{
						if (IsIconLoadCurrent(handler, item, swipeItem, generation) &&
							ReferenceEquals(item.Source, source))
						{
							tintedIconSource.Foreground = tintColor.ToPlatform();
							swipeItem.IconSource = tintedIconSource;
						}

						return;
					}
				}

				var imageSourceServiceProvider = handler.MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var scale = handler.MauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				IImageSource loadSource = source;
				if (source is IFontImageSource { Color: null } fontImageSource &&
					item.GetTextColor() is Color fallbackColor)
				{
					loadSource = new TintedFontImageSource(fontImageSource, fallbackColor);
				}

				var service = imageSourceServiceProvider.GetRequiredImageSourceService(loadSource);
				// Do not use ConfigureAwait(false): WinUI DependencyProperty writes require the UI thread.
				var result = await service.GetImageSourceAsync(loadSource, scale);

				// Only apply the result if no newer load has started on THIS handler while this one was
				// in flight. Checking item.Source == source alone is not enough: the source can be
				// identical while IconColor changed, so a stale untinted result would overwrite a tinted one.
				if (!IsIconLoadCurrent(handler, item, swipeItem, generation))
					return;

				if (ReferenceEquals(item.Source, source))
				{
					swipeItem.IconSource = result?.Value is WImageSource platformImage ? new ImageIconSource { ImageSource = platformImage } : null;
				}
			}
			catch (System.Exception ex)
			{
				handler.MauiContext?.CreateLogger<SwipeItemMenuItemHandler>()?.Log(LogLevel.Warning, new EventId(), "Cannot load SwipeItem Icon", ex, static (state, _) => state);
			}
		}

		internal static IconSource? CreateTintedIconSource(IImageSource source, IMauiContext mauiContext)
		{
			if (source is IFileImageSource fileImageSource)
			{
				var filename = fileImageSource.File;
				if (string.IsNullOrEmpty(filename) || Path.IsPathRooted(filename))
					return null;

				return new BitmapIconSource
				{
					UriSource = new Uri("ms-appx:///" + Path.GetFileName(filename))
				};
			}

			return source.ToIconSource(mauiContext);
		}

		sealed class TintedFontImageSource : IFontImageSource
		{
			readonly IFontImageSource _source;

			public TintedFontImageSource(IFontImageSource source, Color color)
			{
				_source = source;
				Color = color;
			}

			public bool IsEmpty => _source.IsEmpty;

			public Color Color { get; }

			public Font Font => _source.Font;

			public string Glyph => _source.Glyph;
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
