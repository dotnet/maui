using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IStreamImageSource : IImageSource
	{
		Func<CancellationToken, Task<Stream>> Stream { get; }
	}
}