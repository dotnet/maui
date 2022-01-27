using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Foundation;

namespace Microsoft.Maui
{
	public static partial class MauiAssetsExtensions
	{
		public static Task<Stream?> OpenMauiAsset(this IMauiContext mauiContext!!, string assetPath!!)
		{
			var iOSResourcePath = Path.Combine(NSBundle.MainBundle.ResourcePath, assetPath.Replace('\\', '/'));

			try
			{
				return Task.FromResult<Stream?>(File.OpenRead(iOSResourcePath));
			}
			catch
			{
				return Task.FromResult<Stream?>(null);
			}
		}
	}
}
