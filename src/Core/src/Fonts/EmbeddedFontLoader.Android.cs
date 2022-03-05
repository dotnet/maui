#nullable enable

using System;
using System.IO;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : FileSystemEmbeddedFontLoader
	{
		public EmbeddedFontLoader(IServiceProvider? serviceProvider = null)
			: base(GetTempPath, serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		static string GetTempPath()
		{
			var ctx = Android.App.Application.Context;
			return ctx.CacheDir?.AbsolutePath ?? Path.GetTempPath();
		}
	}
}