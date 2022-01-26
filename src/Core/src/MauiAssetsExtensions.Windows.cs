using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Microsoft.Maui
{
	public static partial class MauiAssetsExtensions
	{
		public static async Task<Stream?> OpenMauiAsset(this IMauiContext mauiContext!!, string assetPath!!)
		{
			var winUIAssetPath = Path.Combine("Assets", assetPath.Replace('/', '\\'));

			var winUIItem = await Package.Current.InstalledLocation.TryGetItemAsync(winUIAssetPath);
			if (winUIItem == null)
			{
				return null;
			}

			var winUIFile = await Package.Current.InstalledLocation.GetFileAsync(winUIAssetPath);

			return await winUIFile.OpenStreamForReadAsync();
		}
	}
}
