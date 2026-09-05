#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IStreamImageSource : IImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default);
	}

	internal interface IStreamImageSourceWithCache : IStreamImageSource
	{
	}

	internal interface IImageSourceCacheStream
	{
		bool CanCache { get; }

		long? ExpectedLength { get; }
	}
}