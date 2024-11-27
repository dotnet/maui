#nullable disable
using System;
using System.IO;
using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;
using AppFW = Tizen.Applications;
using NImage = Tizen.NUI.BaseComponents.ImageView;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class ImageExtensions
	{
		public static Task LoadImage(this NImage image, ImageSource source)
		{
			if (source is FileImageSource file)
			{
				return image.LoadImageAsync(file);
			}
			else if (source is UriImageSource uri)
			{
				return image.LoadImageAsync(uri);
			}
			else if (source is StreamImageSource stream)
			{
				return image.LoadImageAsync(stream);
			}

			return Task.CompletedTask;
		}

		public static async Task<bool> LoadImageAsync(this NImage image, FileImageSource imageSource)
		{
			if (image == null)
				return false;
			return await image.LoadAsync(GetPath(imageSource.File));
		}

		public static async Task<bool> LoadImageAsync(this NImage image, UriImageSource imageSource)
		{
			if (image == null)
				return false;
			return await image.LoadAsync(imageSource.Uri.AbsoluteUri);
		}

		public static async Task<bool> LoadImageAsync(this NImage image, StreamImageSource imageSource)
		{
			if (image == null)
				return false;
			if (imageSource.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)imageSource).GetStreamAsync())
				{
					if (streamImage != null)
						return await image.LoadAsync(streamImage);
				}
			}
			return false;
		}

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
						return resPath;
					}
				}
			}
			return res;
		}
	}
}
