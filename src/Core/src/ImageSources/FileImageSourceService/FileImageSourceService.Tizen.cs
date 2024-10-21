#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AppFW = Tizen.Applications;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, cancellationToken);

		public Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IFileImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			var filename = imageSource.File;
			try
			{
				if (!string.IsNullOrEmpty(filename))
				{
					var image = new MauiImageSource
					{
						ResourceUrl = GetPath(filename)
					};
					var result = new ImageSourceServiceResult(image, () => image.Dispose());
					return FromResult(result);
				}
				else
				{
					throw new InvalidOperationException("Unable to load image file.");
				}
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);
				throw;
			}
		}

		static Task<IImageSourceServiceResult<MauiImageSource>?> FromResult(IImageSourceServiceResult<MauiImageSource>? result) =>
			Task.FromResult(result);

		static string GetPath(string res)
		{
			if (Path.IsPathRooted(res))
			{
				return res;
			}

			foreach (AppFW.ResourceManager.Category category in Enum.GetValues<AppFW.ResourceManager.Category>())
			{
				foreach (var file in new[] { res, res + ".jpg", res + ".png", res + ".gif" })
				{
					var path = AppFW.ResourceManager.TryGetPath(category, file);

					if (path != null)
					{
						return path;
					}
				}
			}

			AppFW.Application app = AppFW.Application.Current;
			if (app != null)
			{
				string resPath = app.DirectoryInfo.Resource + res;

				foreach (var file in new[] { resPath, resPath + ".jpg", resPath + ".png", resPath + ".gif" })
				{
					if (File.Exists(file))
					{
						return file;
					}
				}
			}

			return res;
		}
	}
}
