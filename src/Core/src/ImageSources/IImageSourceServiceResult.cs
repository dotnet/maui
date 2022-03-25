#nullable enable
using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceResult<T> : IImageSourceServiceResult
	{
		T Value { get; }
	}

	public interface IImageSourceServiceResult : IDisposable
	{
		bool IsResolutionDependent { get; }

		bool IsDisposed { get; }
	}
}