using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class MainThread_Tests
	{
		[Fact]
		public Task IsOnMainThread()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.True(MainThread.IsMainThread);
			});
		}

		[Fact]
		public Task IsNotOnMainThread()
		{
			return Task.Run(() =>
			{
				Assert.False(MainThread.IsMainThread);
			});
		}
	}
}
