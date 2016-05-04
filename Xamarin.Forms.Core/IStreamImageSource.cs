using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public interface IStreamImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken));
	}
}