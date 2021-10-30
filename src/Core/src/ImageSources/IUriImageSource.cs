#nullable enable
using System;

namespace Microsoft.Maui
{
	public interface IUriImageSource : IImageSource
	{
		Uri Uri { get; }

		TimeSpan CacheValidity { get; }

		bool CachingEnabled { get; }
	}
}