using System;

namespace Microsoft.Maui
{
	public interface IUriImageSource : IImageSource
	{
		Uri Uri { get; }
	}
}