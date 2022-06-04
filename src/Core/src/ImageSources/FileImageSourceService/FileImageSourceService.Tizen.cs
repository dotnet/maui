#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.ElmSharp;
using AppFW = Tizen.Applications;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<Image>?> GetImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<Image>?> GetImageAsync(IFileImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;
			try
			{
				if (!string.IsNullOrEmpty(filename))
				{
					var isLoadComplated = await image.LoadAsync(GetPath(filename), cancellationToken);

					if (!isLoadComplated)
					{
						//If it fails, call the Load function to remove the previous image.
						image.Load(string.Empty);
						throw new InvalidOperationException("Unable to load image file.");
					}

					var result = new ImageSourceServiceResult(image);
					return result;
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

		static string GetPath(string res)
		{
			if (Path.IsPathRooted(res))
			{
				return res;
			}

			foreach (AppFW.ResourceManager.Category category in Enum.GetValues(typeof(AppFW.ResourceManager.Category)))
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
