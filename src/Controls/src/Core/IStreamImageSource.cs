using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public interface IStreamImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken));
	}
}