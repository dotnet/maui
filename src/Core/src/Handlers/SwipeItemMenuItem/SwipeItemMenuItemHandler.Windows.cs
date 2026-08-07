using System;
using System.IO;
using System.Runtime.CompilerServices;
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
		static readonly ConditionalWeakTable<ISwipeItemMenuItemHandler, HandlerState> s_externalHandlerStates = new();
		int _iconLoadGeneration;

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

			// ImageIconSource renders the image as-is and ignores Foreground. Built-in packaged
			// files can use BitmapIconSource's monochrome mask; font icons are rendered by their
			// registered service with the resolved tint. Other source services keep their own colors.
			var tintColor = (item as ISwipeItemMenuItemIconColor)?.IconColor;

			try
			{
				var imageSourceServiceProvider = handler.MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();

				if (tintColor is not null)
				{
					var sourceService = imageSourceServiceProvider.GetRequiredImageSourceService(source);
					var tintedIconSource = CreateTintedIconSource(source, sourceService);

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

				var scale = handler.MauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				IImageSource loadSource = source;
				if (source is IFontImageSource fontImageSource)
				{
					var resolvedFontColor = tintColor ??
						(fontImageSource.Color is null ? item.GetTextColor() : null);

					if (resolvedFontColor is Color fontColor)
						loadSource = new TintedFontImageSource(fontImageSource, fontColor);
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

		internal static IconSource? CreateTintedIconSource(
			IImageSource source,
			IImageSourceService imageSourceService)
		{
			if (!CanCreateTintedIconSource(source, imageSourceService) ||
				source is not IFileImageSource fileImageSource)
			{
				return null;
			}

			return new BitmapIconSource
			{
				UriSource = new Uri("ms-appx:///" + Path.GetFileName(fileImageSource.File))
			};
		}

		internal static bool CanCreateTintedIconSource(
			IImageSource source,
			IImageSourceService imageSourceService) =>
			source is IFileImageSource { File: string filename } &&
			imageSourceService.GetType() == typeof(FileImageSourceService) &&
			!string.IsNullOrEmpty(filename) &&
			!Path.IsPathRooted(filename);

		static partial void UpdateIconColorPlatform(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem view,
			ref bool handled)
		{
			var source = view.Source;
			if (source is null)
			{
				handled = true;
				return;
			}

			if (source is IFontImageSource)
				return;

			if (source is not IFileImageSource || handler.MauiContext is null)
			{
				handled = true;
				return;
			}

			try
			{
				var provider = handler.MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = provider.GetRequiredImageSourceService(source);
				handled = !CanCreateTintedIconSource(source, service);
			}
			catch (InvalidOperationException)
			{
				// Let the normal source update preserve existing error logging.
			}
		}

		internal static int BeginIconLoad(ISwipeItemMenuItemHandler handler)
		{
			if (handler is SwipeItemMenuItemHandler platformHandler)
				return System.Threading.Interlocked.Increment(ref platformHandler._iconLoadGeneration);

			var state = s_externalHandlerStates.GetValue(handler, static _ => new HandlerState());
			return System.Threading.Interlocked.Increment(ref state.IconLoadGeneration);
		}

		internal static bool IsIconLoadCurrent(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem item,
			object platformView,
			int generation)
		{
			int currentGeneration = handler is SwipeItemMenuItemHandler platformHandler
				? System.Threading.Volatile.Read(ref platformHandler._iconLoadGeneration)
				: System.Threading.Volatile.Read(
					ref s_externalHandlerStates.GetValue(handler, static _ => new HandlerState()).IconLoadGeneration);

			return generation == currentGeneration &&
				ReferenceEquals(handler.VirtualView, item) &&
				ReferenceEquals(handler.PlatformView, platformView);
		}

		sealed class HandlerState
		{
			public int IconLoadGeneration;
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
