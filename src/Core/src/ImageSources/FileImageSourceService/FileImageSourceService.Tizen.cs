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
		public override Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			LoadImageAsync((IFileImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IFileImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			try
			{
				if (!string.IsNullOrEmpty(filename))
				{
					var isLoadComplate = await image.LoadAsync(GetPath(filename), cancellationToken);

					if (!isLoadComplate)
					{
						//If it fails, call the Load function to remove the previous image.
						image.Load(string.Empty);
						throw new InvalidOperationException("Unable to load image file.");
					}

					var result =  new ImageSourceServiceResult(isLoadComplate);

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
				var path = AppFW.ResourceManager.TryGetPath(category, res);

				if (path != null)
				{
					return path;
				}
			}

			AppFW.Application app = AppFW.Application.Current;
			if (app != null)
			{
				string resPath = app.DirectoryInfo.Resource + res;
				if (File.Exists(resPath))
				{
					return resPath;
				}
			}

			return res;
		}
	}
}