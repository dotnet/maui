using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class IdleSynchronizer : IDisposable
	{
#if !WINDOWS
		private static IdleSynchronizer? idleSynchronizer;
		public static IdleSynchronizer GetForCurrentProcess()
		{
			if (idleSynchronizer == null)
			{
				idleSynchronizer = new IdleSynchronizer();
			}
			return idleSynchronizer;
		}

		public void Dispose()
		{
			idleSynchronizer = null;
		}

		public async Task<string> WaitAsync()
		{
			await Task.Delay(1);
			return string.Empty;
		}
#endif
	}
}
