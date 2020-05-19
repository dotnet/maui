using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Maui
{
	public interface IStreamImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken));
	}
}