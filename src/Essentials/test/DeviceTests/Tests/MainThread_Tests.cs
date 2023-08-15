using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("MainThread")]
	public class MainThread_Tests
	{
		[UIFact]
		public void IsOnMainThread()
		{
				Assert.True(MainThread.IsMainThread);
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
