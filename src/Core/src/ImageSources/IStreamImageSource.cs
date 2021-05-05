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
}