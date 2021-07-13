#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NativeImage = Gdk.Pixbuf;

namespace Microsoft.Maui
{

	public partial class FileImageSourceService
	{

		public override Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			var filename = imageSource.File;

			NativeImage? TryLoadFile(string file)
			{
				if (File.Exists(file))
					return new NativeImage(filename);

				return null;
			}

			NativeImage? TryLoadResource(string file)
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					var names = assembly.GetManifestResourceNames();
					var res = names.FirstOrDefault(r => r.EndsWith($".{file}"));

					if (res != null)
					{
						return new(assembly, res);
					}
				}

				return default;

			}

			try
			{
				var imageDirectory = Configuration?.GetImageDirectory();
				var image = TryLoadFile(filename);

				if (image == null && imageDirectory != null)
				{
					image = TryLoadFile(Path.Combine(imageDirectory, filename));
				}

				var isResource = false;

				if (image == null)
				{
					image = TryLoadResource(filename);
					isResource = true;
				}

				if (image == null)
					throw new InvalidOperationException("Unable to load image file.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose()) { IsResource = isResource };

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);

				throw;
			}
		}

		static Task<IImageSourceServiceResult<NativeImage>?> FromResult(IImageSourceServiceResult<NativeImage>? result) =>
			Task.FromResult(result);

	}

}