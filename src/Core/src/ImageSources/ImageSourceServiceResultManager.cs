#nullable enable
using System;
using System.Threading;

namespace Microsoft.Maui.Handlers
{
	class ImageSourceServiceResultManager
	{
		CancellationTokenSource? _sourceCancellation;
		IDisposable? _sourceResult;

		public CancellationToken Token =>
			_sourceCancellation?.Token ?? default;

		public bool IsResolutionDependent { get; private set; }

		public CancellationToken BeginLoad()
		{
			_sourceResult?.Dispose();

			_sourceCancellation?.Cancel();
			_sourceCancellation = new CancellationTokenSource();

			return Token;
		}

		public void CompleteLoad<T>(IImageSourceServiceResult<T>? result)
		{
			CompleteLoad((IDisposable?)result);

			IsResolutionDependent = result?.IsResolutionDependent ?? false;
		}

		public void CompleteLoad(IDisposable? result)
		{
			_sourceResult = result;
			_sourceCancellation?.Dispose();
			_sourceCancellation = null;

			IsResolutionDependent = false;
		}

		public void Reset()
		{
			BeginLoad();
			CompleteLoad(null);
		}
	}
}