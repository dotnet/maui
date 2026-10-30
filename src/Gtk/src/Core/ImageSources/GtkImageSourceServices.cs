using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.ImageSources
{
	/// <summary>
	/// An <see cref="IImageSourceService"/> that produces <see cref="Gdk.Texture"/> images for GTK widgets.
	/// </summary>
	public interface IGtkImageSourceService : IImageSourceService
	{
		/// <summary>Loads <paramref name="imageSource"/> as a texture; <see langword="null"/> when it cannot be loaded.</summary>
		/// <param name="imageSource">The MAUI image source.</param>
		/// <param name="cancellationToken">Cancels the load.</param>
		Task<IImageSourceServiceResult<Gdk.Texture>?> GetTextureAsync(IImageSource imageSource, CancellationToken cancellationToken = default);
	}

	/// <summary>Shared plumbing of the GTK image source services.</summary>
	public abstract class GtkImageSourceService<T> : ImageSourceService, IImageSourceService<T>, IGtkImageSourceService
		where T : IImageSource
	{
		/// <summary>Creates the service.</summary>
		/// <param name="logger">Optional logger.</param>
		protected GtkImageSourceService(ILogger? logger = null)
			: base(logger)
		{
		}

		/// <inheritdoc/>
		public async Task<IImageSourceServiceResult<Gdk.Texture>?> GetTextureAsync(IImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource is not T source || imageSource.IsEmpty)
				return null;

			try
			{
				var texture = await LoadTextureAsync(source, cancellationToken).ConfigureAwait(false);
				return texture is null ? null : new ImageSourceServiceResultTexture(texture);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load the image source {ImageSource} as a GTK texture.", imageSource);
				return null;
			}
		}

		/// <summary>Loads the texture for <paramref name="source"/>.</summary>
		/// <param name="source">The typed image source.</param>
		/// <param name="cancellationToken">Cancels the load.</param>
		protected abstract Task<Gdk.Texture?> LoadTextureAsync(T source, CancellationToken cancellationToken);

		/// <summary>Creates a texture from encoded image bytes (PNG, JPEG, SVG, ... whatever gdk-pixbuf can decode).</summary>
		/// <param name="data">The encoded image.</param>
		protected static Gdk.Texture? TextureFromBytes(byte[] data) =>
			data.Length == 0 ? null : new Gdk.Texture(new GLib.Bytes(data));
	}

	sealed class ImageSourceServiceResultTexture : IImageSourceServiceResult<Gdk.Texture>
	{
		public ImageSourceServiceResultTexture(Gdk.Texture value) => Value = value;

		public Gdk.Texture Value { get; }

		public bool IsResolutionDependent => false;

		public bool IsDisposed { get; private set; }

		public void Dispose() => IsDisposed = true;
	}

	/// <summary>Loads <see cref="IFileImageSource"/> files, looking next to the application, in <c>Resources/Images</c> and in the working directory.</summary>
	public sealed class GtkFileImageSourceService : GtkImageSourceService<IFileImageSource>
	{
		static readonly string[] s_extensions = { "", ".png", ".jpg", ".jpeg", ".svg", ".webp", ".gif", ".bmp" };

		/// <summary>Creates the service.</summary>
		/// <param name="logger">Optional logger.</param>
		public GtkFileImageSourceService(ILogger<GtkFileImageSourceService>? logger = null)
			: base(logger)
		{
		}

		/// <inheritdoc/>
		protected override Task<Gdk.Texture?> LoadTextureAsync(IFileImageSource source, CancellationToken cancellationToken)
		{
			var path = Resolve(source.File);
			return Task.FromResult<Gdk.Texture?>(path is null ? null : new Gdk.Texture(path));
		}

		internal static string? Resolve(string? file)
		{
			if (string.IsNullOrEmpty(file))
				return null;

			if (Path.IsPathRooted(file))
				return File.Exists(file) ? file : null;

			var baseDirectory = AppContext.BaseDirectory;
			string[] roots =
			{
				baseDirectory,
				Path.Combine(baseDirectory, "Resources", "Images"),
				Path.Combine(baseDirectory, "Resources"),
				Directory.GetCurrentDirectory(),
			};

			foreach (var root in roots)
			{
				foreach (var extension in s_extensions)
				{
					var candidate = Path.Combine(root, file + extension);
					if (File.Exists(candidate))
						return candidate;
				}
			}

			return null;
		}
	}

	/// <summary>Loads <see cref="IStreamImageSource"/> streams.</summary>
	public sealed class GtkStreamImageSourceService : GtkImageSourceService<IStreamImageSource>
	{
		/// <summary>Creates the service.</summary>
		/// <param name="logger">Optional logger.</param>
		public GtkStreamImageSourceService(ILogger<GtkStreamImageSourceService>? logger = null)
			: base(logger)
		{
		}

		/// <inheritdoc/>
		protected override async Task<Gdk.Texture?> LoadTextureAsync(IStreamImageSource source, CancellationToken cancellationToken)
		{
			using var stream = await source.GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				return null;

			using var memory = new MemoryStream();
			await stream.CopyToAsync(memory, 81920, cancellationToken).ConfigureAwait(false);
			return TextureFromBytes(memory.ToArray());
		}
	}

	/// <summary>Downloads <see cref="IUriImageSource"/> images over HTTP(S).</summary>
	public sealed class GtkUriImageSourceService : GtkImageSourceService<IUriImageSource>
	{
		static readonly Lazy<HttpClient> s_client = new(() => new HttpClient());

		/// <summary>Creates the service.</summary>
		/// <param name="logger">Optional logger.</param>
		public GtkUriImageSourceService(ILogger<GtkUriImageSourceService>? logger = null)
			: base(logger)
		{
		}

		/// <inheritdoc/>
		protected override async Task<Gdk.Texture?> LoadTextureAsync(IUriImageSource source, CancellationToken cancellationToken)
		{
			if (source.Uri is null)
				return null;

			var data = await s_client.Value.GetByteArrayAsync(source.Uri, cancellationToken).ConfigureAwait(false);
			return TextureFromBytes(data);
		}
	}

	/// <summary>Renders <see cref="IFontImageSource"/> glyphs to a texture with Pango and the Cairo renderer.</summary>
	public sealed class GtkFontImageSourceService : GtkImageSourceService<IFontImageSource>
	{
		readonly IFontManager? _fontManager;

		/// <summary>Creates the service.</summary>
		/// <param name="fontManager">Resolves the glyph font.</param>
		/// <param name="logger">Optional logger.</param>
		public GtkFontImageSourceService(IFontManager? fontManager = null, ILogger<GtkFontImageSourceService>? logger = null)
			: base(logger)
		{
			_fontManager = fontManager;
		}

		/// <inheritdoc/>
		protected override Task<Gdk.Texture?> LoadTextureAsync(IFontImageSource source, CancellationToken cancellationToken)
		{
			// Pango contexts and renderers belong to the GTK thread.
			var completion = new TaskCompletionSource<Gdk.Texture?>();
			GtkRuntime.Post(() =>
			{
				try
				{
					completion.SetResult(Render(source));
				}
				catch (Exception ex)
				{
					completion.SetException(ex);
				}
			});
			return completion.Task;
		}

		Gdk.Texture? Render(IFontImageSource source)
		{
			if (string.IsNullOrEmpty(source.Glyph))
				return null;

			var anchor = new global::Gtk.Label();
			using var layout = anchor.CreatePangoLayout(source.Glyph);

			if (_fontManager is Fonts.GtkFontManager manager)
				layout.FontDescription = manager.GetFontDescription(source.Font);

			layout.GetPixelSize(out int width, out int height);
			if (width <= 0 || height <= 0)
				return null;

			var snapshot = new global::Gtk.Snapshot();
			var color = (source.Color ?? Graphics.Colors.Black).ToGdkRgba();
			snapshot.AppendLayout(layout, color);
			var node = snapshot.ToNode();
			if (node is null)
				return null;

			var renderer = new Gsk.CairoRenderer();
			try
			{
				renderer.RealizeForDisplay(Gdk.Display.Default);
				return renderer.RenderTexture(node, GtkInterop.ToGrapheneRect(0, 0, width, height));
			}
			finally
			{
				renderer.Unrealize();
			}
		}
	}
}
