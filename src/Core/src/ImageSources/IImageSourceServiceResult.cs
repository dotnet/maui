using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceResult<T> : IDisposable
	{
		T Value { get; }
	}
}