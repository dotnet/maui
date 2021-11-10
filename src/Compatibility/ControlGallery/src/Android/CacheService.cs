using System;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class CacheService : ICacheService
	{
		public void ClearImageCache()
		{
			throw new NotImplementedException("TODO: CACHING https://github.com/dotnet/runtime/issues/52332");
		}
	}
}