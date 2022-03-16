#nullable enable
using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceResult<T> : IDisposable
	{
		T Value { get; }

		bool IsResolutionDependent { get; }

		bool IsDisposed { get; }
	}
}