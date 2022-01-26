using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public static partial class MauiAssetsExtensions
	{
		public static Task<Stream?> OpenMauiAsset(this IMauiContext mauiContext!!, string assetPath!!)
		{
			if (mauiContext.Context?.Assets == null)
			{
				throw new ArgumentException($"The specified {nameof(IMauiContext)} must have its {nameof(IMauiContext.Context)} property set to a valid Android.Content.Context, which then has a valid Android.Content.Res.AssetManager.", nameof(mauiContext));
			}
			assetPath = assetPath.Replace('\\', '/');

			try
			{
				return Task.FromResult<Stream?>(mauiContext.Context.Assets.Open(assetPath));
			}
			catch
			{
				return Task.FromResult<Stream?>(null);
			}
		}
	}
}
