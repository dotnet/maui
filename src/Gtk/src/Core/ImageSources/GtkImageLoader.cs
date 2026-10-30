using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.ImageSources
{
	/// <summary>
	/// Loads the <see cref="IImageSourcePart.Source"/> of a handler's view into a <see cref="Gdk.Texture"/> and hands it to
	/// a setter on the GTK thread. It is the backend's replacement for <c>ImageSourcePartLoader</c>, which does nothing
	/// on the platform-less (<c>net10.0</c>) assemblies.
	/// </summary>
	/// <remarks>
	/// A newer load cancels the one in flight; the previous result is released when it is replaced. Loading flags and
	/// <see cref="IImageSourcePartEvents"/> are reported the same way the other platforms do.
	/// </remarks>
	public sealed class GtkImageLoader
	{
		CancellationTokenSource? _cancellation;
		IDisposable? _result;

		/// <summary>
		/// Finds the GTK service for <paramref name="source"/>. The service registered for the concrete source type (Controls registers
		/// <c>FileImageSource</c> and friends) may be a stock one that cannot produce textures; the GTK services are registered for the
		/// core source interfaces, so those are tried next.
		/// </summary>
		static IGtkImageSourceService? Resolve(IImageSourceServiceProvider provider, IImageSource source)
		{
			if (provider.GetImageSourceService(source.GetType()) is IGtkImageSourceService direct)
				return direct;

			if (source is IFileImageSource && provider.GetImageSourceService(typeof(IFileImageSource)) is IGtkImageSourceService file)
				return file;
			if (source is IStreamImageSource && provider.GetImageSourceService(typeof(IStreamImageSource)) is IGtkImageSourceService stream)
				return stream;
			if (source is IUriImageSource && provider.GetImageSourceService(typeof(IUriImageSource)) is IGtkImageSourceService uri)
				return uri;
			if (source is IFontImageSource && provider.GetImageSourceService(typeof(IFontImageSource)) is IGtkImageSourceService font)
				return font;

			return null;
		}

		/// <summary>Cancels any load in flight and releases the current result.</summary>
		public void Reset()
		{
			_cancellation?.Cancel();
			_cancellation = null;
			_result?.Dispose();
			_result = null;
		}

		/// <summary>Loads the source of <paramref name="part"/> and passes the texture (or <see langword="null"/>) to <paramref name="apply"/>.</summary>
		/// <param name="handler">The handler (for its services).</param>
		/// <param name="part">The image source part of the virtual view.</param>
		/// <param name="apply">Receives the texture on the GTK thread.</param>
		/// <returns>A task that completes when the load finished or was superseded.</returns>
		public async Task UpdateAsync(IElementHandler handler, IImageSourcePart? part, Action<Gdk.Texture?> apply)
		{
			Reset();

			var source = part?.Source;
			if (part is null || source is null || source.IsEmpty)
			{
				apply(null);
				part?.UpdateIsLoading(false);
				return;
			}

			var cancellation = _cancellation = new CancellationTokenSource();
			var token = cancellation.Token;

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var service = Resolve(provider, source);
			var events = part as IImageSourcePartEvents;

			if (service is null)
			{
				events?.LoadingFailed(new InvalidOperationException($"No GTK image source service is registered for {source.GetType()}."));
				return;
			}

			part.UpdateIsLoading(true);
			events?.LoadingStarted();

			IImageSourceServiceResult<Gdk.Texture>? result = null;
			Exception? failure = null;
			try
			{
				result = await service.GetTextureAsync(source, token).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return;
			}
			catch (Exception ex)
			{
				failure = ex;
			}

			GtkRuntime.Post(() =>
			{
				if (token.IsCancellationRequested)
				{
					result?.Dispose();
					return;
				}

				_result = result;
				apply(result?.Value);
				part.UpdateIsLoading(false);

				if (failure is not null)
					events?.LoadingFailed(failure);
				else
					events?.LoadingCompleted(result is not null);
			});
		}
	}
}
